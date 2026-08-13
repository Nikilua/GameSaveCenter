using System.Text.Json;
using GameSaveCenter.Contracts;
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
    private readonly ILogger<RepositoryRebuildService> _logger;

    public RepositoryRebuildService(IRestoreCatalog catalog, IBackupHistoryRebuilder rebuilder, SqliteStateStore store, ILogger<RepositoryRebuildService> logger)
    {
        _catalog = catalog;
        _rebuilder = rebuilder;
        _store = store;
        _logger = logger;
    }

    public async Task<RepositoryRebuildResultDto> RebuildAsync(CancellationToken token)
    {
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
