using System.IO.Compression;
using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>
/// Rebuilds the SQLite backup index from Ludusavi's authoritative backup list. It only
/// reads history and reconciles indexed metadata; it never deletes or uploads archives.
/// </summary>
public sealed class RepositoryRebuildService
{
    private readonly IRestoreCatalog _catalog;
    private readonly IBackupHistoryRebuilder _rebuilder;
    private readonly SqliteStateStore _store;
    private readonly WorkerOptions _options;
    private readonly ILogger<RepositoryRebuildService> _logger;

    public RepositoryRebuildService(IRestoreCatalog catalog, IBackupHistoryRebuilder rebuilder, SqliteStateStore store, WorkerOptions options, ILogger<RepositoryRebuildService> logger)
    {
        _catalog = catalog;
        _rebuilder = rebuilder;
        _store = store;
        _options = options;
        _logger = logger;
    }

    public async Task<RepositoryRebuildPreviewDto> PreviewAsync(CancellationToken token)
    {
        var known = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            var probe = await _store.ProbeIntegrityAsync(Array.Empty<string>(), token).ConfigureAwait(false);
            foreach (var path in probe.BackupArchivePaths)
                if (!string.IsNullOrWhiteSpace(path)) known.Add(Path.GetFullPath(path));
        }
        catch
        {
            // A missing database should still allow a read-only archive preview.
        }

        var found = 0;
        var confirmable = 0;
        var partial = 0;
        var corrupt = 0;
        var unassigned = 0;
        if (Directory.Exists(_options.LudusaviBackupDirectory))
        {
            foreach (var file in Directory.EnumerateFiles(_options.LudusaviBackupDirectory, "*.zip", SearchOption.AllDirectories))
            {
                token.ThrowIfCancellationRequested();
                found++;
                var full = Path.GetFullPath(file);
                if (known.Contains(full)) confirmable++;
                else unassigned++;
                try
                {
                    using var archive = ZipFile.OpenRead(file);
                    if (!archive.Entries.Any()) partial++;
                }
                catch
                {
                    corrupt++;
                }
            }
        }

        var summary = $"扫描到 {found} 个归档；已确认归属 {confirmable} 个，未归属 {unassigned} 个，元数据部分缺失 {partial} 个，损坏 {corrupt} 个。";
        return new RepositoryRebuildPreviewDto
        {
            FoundArchives = found,
            ConfirmableArchives = confirmable,
            PartialMetadataArchives = partial,
            CorruptArchives = corrupt,
            UnassignedArchives = unassigned,
            Summary = summary
        };
    }

    public async Task<RepositoryRebuildResultDto> RebuildAsync(RepositoryRebuildRequestDto request, CancellationToken token)
    {
        if (!request.Confirmed)
            throw new WorkerOperationException("REPOSITORY_REBUILD_NOT_CONFIRMED", "请先预览并确认备份索引重建。");

        var matches = await _catalog.GetMatchesAsync(token).ConfigureAwait(false);
        var rebuilt = 0;
        var failed = 0;
        var versions = 0;
        var errors = new List<string>();
        foreach (var pair in matches)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                await _rebuilder.RefreshBackupHistoryAsync(pair.Key, pair.Value.Name, token).ConfigureAwait(false);
                rebuilt++;
                versions += (await _store.GetBackupVersionsAsync(pair.Key, token).ConfigureAwait(false)).Count;
            }
            catch (Exception ex)
            {
                failed++;
                errors.Add($"{pair.Value.Name}：{ex.Message}");
                _logger.LogWarning(ex, "Repository rebuild failed for {Game}", pair.Value.Name);
            }
        }

        var summary = failed == 0
            ? $"备份索引重建完成：{rebuilt} 个游戏成功，共索引 {versions} 个版本。"
            : $"备份索引重建完成：{rebuilt} 个游戏成功，{failed} 个失败，共索引 {versions} 个版本；失败游戏已保留原索引。";
        await _store.AppendAuditAsync("RepositoryRebuild", summary,
            JsonSerializer.Serialize(new { rebuilt, failed, versions, errors = errors.Take(20).ToList() }),
            token).ConfigureAwait(false);
        return new RepositoryRebuildResultDto
        {
            RebuiltGameCount = rebuilt,
            IndexedVersionCount = versions,
            FailedGameCount = failed,
            Summary = summary
        };
    }
}
