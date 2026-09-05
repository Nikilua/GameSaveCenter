using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Models;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>
/// Rebuilds the SQLite backup index from the backup repository itself. It can recover
/// history from a fresh/empty database by deriving game and artifact identity from the
/// ZIP layout, never guesses parent relationships, and remains idempotent.
/// </summary>
public sealed class RepositoryRebuildService
{
    private readonly SqliteStateStore _store;
    private readonly WorkerOptions _options;
    private readonly ILogger<RepositoryRebuildService> _logger;
    private readonly GameOperationLock _gameLock;

    public RepositoryRebuildService(
        SqliteStateStore store,
        WorkerOptions options,
        ILogger<RepositoryRebuildService> logger,
        GameOperationLock? gameLock = null)
    {
        _store = store;
        _options = options;
        _logger = logger;
        _gameLock = gameLock ?? new GameOperationLock();
    }

    public async Task<RepositoryRebuildPreviewDto> PreviewAsync(CancellationToken token)
    {
        var artifacts = await ScanAsync(token).ConfigureAwait(false);
        var matches = await TryGetExistingMatchesAsync(token).ConfigureAwait(false);
        var valid = artifacts.Where(x => !x.IsCorrupt).ToList();
        var unassigned = valid.Count(x => !matches.ContainsKey(x.LudusaviName));
        var partial = valid.Count(x => x.FileCount == 0);
        var corrupt = artifacts.Count - valid.Count;
        var summary = $"扫描到 {artifacts.Count} 个归档；可识别 {valid.Count} 个，其中 {unassigned} 个当前无 Playnite 匹配（重建时按 Ludusavi 名称恢复），空归档 {partial} 个，损坏 {corrupt} 个。";
        return new RepositoryRebuildPreviewDto
        {
            FoundArchives = artifacts.Count,
            ConfirmableArchives = valid.Count,
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

        var artifacts = await ScanAsync(token).ConfigureAwait(false);
        Dictionary<string, string> matches;
        try
        {
            matches = await TryGetExistingMatchesAsync(token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new WorkerOperationException(
                "REPOSITORY_REBUILD_DATABASE_UNAVAILABLE",
                "数据库无法读取，无法执行重建；请先恢复或重新初始化 GameSaveCenter 数据库。",
                ex.Message);
        }

        var groups = artifacts
            .Where(x => !x.IsCorrupt && x.FileCount > 0)
            .GroupBy(x => x.LudusaviName, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var rebuilt = 0;
        var failed = 0;
        var versions = 0;
        var recovered = 0;
        var errors = new List<string>();
        foreach (var group in groups)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                var resolution = await ResolveGameAsync(group.Key, matches, token).ConfigureAwait(false);
                if (resolution.Recovered) recovered++;
                using var lease = await _gameLock.AcquireAsync(
                    resolution.PlayniteId,
                    GameOperationKind.RepositoryRepair,
                    TimeSpan.FromSeconds(10),
                    token).ConfigureAwait(false);
                if (lease == null)
                    throw new WorkerOperationException("GAME_OPERATION_BUSY", "该游戏已有备份、恢复、媒体或保留清理操作正在执行，已跳过索引重建。", resolution.PlayniteId);

                var activeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var artifact in group)
                {
                    var version = ToVersion(artifact, resolution.PlayniteId, group.Key);
                    await _store.AddBackupVersionAsync(version, artifact.ManifestJson, token).ConfigureAwait(false);
                    activeIds.Add(artifact.BackupId);
                }

                var current = await _store.GetBackupVersionsAsync(resolution.PlayniteId, token).ConfigureAwait(false);
                foreach (var row in current)
                {
                    if (!activeIds.Contains(row.BackupId) && (row.IsLocked || row.IsPreRestore))
                        activeIds.Add(row.BackupId);
                }
                await _store.RemoveMissingBackupVersionsAsync(resolution.PlayniteId, activeIds.ToArray(), token).ConfigureAwait(false);
                versions += (await _store.GetBackupVersionsAsync(resolution.PlayniteId, token).ConfigureAwait(false)).Count;
                rebuilt++;
            }
            catch (Exception ex)
            {
                failed++;
                errors.Add($"{group.Key}：{ex.Message}");
                _logger.LogWarning(ex, "Repository rebuild failed for {Game}", group.Key);
            }
        }

        var summary = failed == 0
            ? $"备份索引重建完成：{rebuilt} 个游戏成功，恢复 {recovered} 个无匹配游戏，共索引 {versions} 个版本。"
            : $"备份索引重建完成：{rebuilt} 个游戏成功，{failed} 个失败，恢复 {recovered} 个无匹配游戏，共索引 {versions} 个版本；失败游戏已保留原索引。";
        await _store.AppendAuditAsync("RepositoryRebuild", summary,
            JsonSerializer.Serialize(new { rebuilt, recovered, failed, versions, errors = errors.Take(20).ToList() }),
            token).ConfigureAwait(false);
        return new RepositoryRebuildResultDto
        {
            RebuiltGameCount = rebuilt,
            IndexedVersionCount = versions,
            FailedGameCount = failed,
            RecoveredGameCount = recovered,
            Summary = summary
        };
    }

    private Task<List<ScannedArtifact>> ScanAsync(CancellationToken token)
    {
        var artifacts = new List<ScannedArtifact>();
        if (string.IsNullOrWhiteSpace(_options.LudusaviBackupDirectory) ||
            !Directory.Exists(_options.LudusaviBackupDirectory))
        {
            return Task.FromResult(artifacts);
        }

        var root = Path.GetFullPath(_options.LudusaviBackupDirectory);
        foreach (var file in Directory.EnumerateFiles(root, "*.zip", SearchOption.AllDirectories))
        {
            token.ThrowIfCancellationRequested();
            var full = Path.GetFullPath(file);
            var relative = Path.GetRelativePath(root, full);
            var segments = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var gameName = segments.Length > 1 && !string.IsNullOrWhiteSpace(segments[0])
                ? segments[0]
                : Path.GetFileNameWithoutExtension(full);
            var backupId = Path.GetFileNameWithoutExtension(full);
            var bytes = SafeLength(full);
            var createdUtc = SafeLastWriteUtc(full);
            try
            {
                using var archive = ZipFile.OpenRead(full);
                var entries = archive.Entries
                    .Where(x => !string.IsNullOrWhiteSpace(x.Name) && !x.FullName.EndsWith("/", StringComparison.Ordinal))
                    .ToList();
                var manifest = entries.Select(x => new FileManifestEntry
                {
                    RelativePath = x.FullName,
                    SizeBytes = x.Length,
                    LastWriteUtc = x.LastWriteTime.UtcDateTime
                }).ToList();
                artifacts.Add(new ScannedArtifact(
                    full,
                    gameName,
                    backupId,
                    createdUtc,
                    bytes,
                    manifest.Count,
                    JsonSerializer.Serialize(manifest),
                    false));
            }
            catch
            {
                artifacts.Add(new ScannedArtifact(full, gameName, backupId, createdUtc, bytes, 0, "{}", true));
            }
        }
        return Task.FromResult(artifacts);
    }

    private async Task<Dictionary<string, string>> TryGetExistingMatchesAsync(CancellationToken token)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var cache = await _store.GetGameMatchCacheAsync(token).ConfigureAwait(false);
        foreach (var pair in cache)
        {
            if (string.IsNullOrWhiteSpace(pair.Value.LudusaviName)) continue;
            if (!result.ContainsKey(pair.Value.LudusaviName))
                result[pair.Value.LudusaviName] = pair.Key;
        }
        return result;
    }

    private async Task<(string PlayniteId, bool Recovered)> ResolveGameAsync(
        string ludusaviName,
        Dictionary<string, string> matches,
        CancellationToken token)
    {
        if (matches.TryGetValue(ludusaviName, out var existingId) && !string.IsNullOrWhiteSpace(existingId))
            return (existingId, false);

        var games = await _store.GetGamesAsync(token).ConfigureAwait(false);
        var existing = games.FirstOrDefault(x => string.Equals(x.Name, ludusaviName, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            matches[ludusaviName] = existing.PlayniteId;
            return (existing.PlayniteId, false);
        }

        var recoveredId = "recovered-" + ComputeStableHash(ludusaviName);
        var descriptor = new GameDescriptorDto
        {
            PlayniteId = recoveredId,
            Name = ludusaviName,
            Platform = GamePlatformKind.Other
        };
        await _store.UpsertGamesAsync(new[] { descriptor }, token).ConfigureAwait(false);
        await _store.SetGameMatchAsync(recoveredId, ludusaviName, 0.0, GameMatchInput.CreateHash(descriptor), token).ConfigureAwait(false);
        matches[ludusaviName] = recoveredId;
        return (recoveredId, true);
    }

    private static BackupVersionDto ToVersion(ScannedArtifact artifact, string playniteId, string ludusaviName)
    {
        return new BackupVersionDto
        {
            BackupId = artifact.BackupId,
            ParentBackupId = string.Empty,
            PlayniteId = playniteId,
            LudusaviName = ludusaviName,
            CreatedUtc = artifact.CreatedUtc,
            TotalBytes = artifact.TotalBytes,
            FileCount = artifact.FileCount,
            IsLocked = false,
            Comment = string.Empty,
            SourceDevice = string.Empty,
            OperatingSystem = string.Empty,
            IsPreRestore = artifact.BackupId.Contains("PreRestore", StringComparison.OrdinalIgnoreCase),
            ArchivePath = artifact.Path
        };
    }

    private static string ComputeStableHash(string value)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(value.Trim().ToUpperInvariant()));
        return Convert.ToHexString(hash).Substring(0, 16).ToLowerInvariant();
    }

    private static long SafeLength(string path)
    {
        try { return new FileInfo(path).Length; }
        catch { return 0; }
    }

    private static DateTime SafeLastWriteUtc(string path)
    {
        try { return File.GetLastWriteTimeUtc(path); }
        catch { return DateTime.MinValue; }
    }

    private sealed record ScannedArtifact(
        string Path,
        string LudusaviName,
        string BackupId,
        DateTime CreatedUtc,
        long TotalBytes,
        int FileCount,
        string ManifestJson,
        bool IsCorrupt);
}
