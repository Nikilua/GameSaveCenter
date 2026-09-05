using System.Collections.Concurrent;
using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Models;
using GameSaveCenter.Core.Services;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>
/// Global retention simulation across every game. Preview is read-only; apply requires a
/// second confirmation and only deletes ZIP archives below the configured backup root.
/// Locked, PreRestore and healthy restore points are never deleted.
/// </summary>
public sealed class RetentionSimulationService
{
    private static readonly TimeSpan MaximumPreviewAge = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan MaximumFutureClockSkew = TimeSpan.FromMinutes(1);
    private const string QuarantineDirectoryName = ".gsc-retention-quarantine";

    private readonly WorkerOptions _options;
    private readonly SqliteStateStore _store;
    private readonly GameOperationLock _gameLock;
    private readonly ILogger<RetentionSimulationService> _logger;
    private readonly ConcurrentDictionary<string, RetentionPreviewSnapshot> _previews = new(StringComparer.OrdinalIgnoreCase);
    private const int MaximumStoredPreviews = 8;
    private readonly TimeSpan _gameLockTimeout;

    public RetentionSimulationService(WorkerOptions options, SqliteStateStore store, ILogger<RetentionSimulationService> logger, GameOperationLock? gameLock = null, TimeSpan? gameLockTimeout = null)
    {
        _options = options;
        _store = store;
        _gameLock = gameLock ?? new GameOperationLock();
        _logger = logger;
        _gameLockTimeout = gameLockTimeout ?? TimeSpan.FromSeconds(10);
    }

    public async Task<RetentionSimulationPreviewDto> PreviewAsync(CancellationToken token)
    {
        var (versions, policies) = await LoadAsync(token).ConfigureAwait(false);
        var plans = BuildPlans(versions, policies);
        var generatedUtc = DateTime.UtcNow;
        var previewId = Guid.NewGuid().ToString("N");
        var candidates = plans
            .SelectMany(x => x.Plan.DeleteCandidates.Select(candidate => ToFingerprint(x.PlayniteId, x.Policy, x.Versions, candidate)))
            .ToList();
        StorePreview(new RetentionPreviewSnapshot(previewId, generatedUtc, candidates));
        var preview = new RetentionSimulationPreviewDto
        {
            PreviewId = previewId,
            GeneratedUtc = generatedUtc,
            ExistingVersionCount = versions.Count,
            KeepVersionCount = plans.Sum(x => x.Plan.Keep.Count),
            DeleteCandidateCount = plans.Sum(x => x.Plan.DeleteCandidates.Count),
            HealthProtectedCount = plans.Sum(x => x.Plan.HealthProtected.Count),
            UserLockedCount = versions.Count(x => x.IsLocked),
            PreRestoreCount = versions.Count(x => x.IsPreRestore),
            EstimatedReleaseBytes = plans.Sum(x => x.Plan.DeleteCandidates.Sum(y => Math.Max(0, y.TotalBytes)))
        };
        preview.Items = plans
            .SelectMany(x => x.Plan.DeleteCandidates.Select(candidate => ToItem(x.PlayniteId, x.GameName, x.Versions, candidate)))
            .OrderByDescending(x => x.TotalBytes)
            .Take(200)
            .ToList();
        var itemHint = preview.DeleteCandidateCount > preview.Items.Count
            ? $"明细按体积展示前 {preview.Items.Count} 条"
            : "已展示全部候选明细";
        preview.Summary = $"现有 {preview.ExistingVersionCount} 个版本，建议保留 {preview.KeepVersionCount} 个，候选清理 {preview.DeleteCandidateCount} 个（预计释放 {preview.EstimatedReleaseDisplay}）；用户锁定 {preview.UserLockedCount}，健康恢复点保护 {preview.HealthProtectedCount}，PreRestore {preview.PreRestoreCount}。{itemHint}。预览只读，清理不会自动执行。";
        return preview;
    }

    public async Task<RetentionSimulationResultDto> ApplyAsync(RetentionSimulationApplyRequestDto request, CancellationToken token)
    {
        if (request == null || !request.Confirmed)
            throw new WorkerOperationException("RETENTION_APPLY_NOT_CONFIRMED", "全局清理需要二次确认；预览只读不会删除任何文件。");
        token.ThrowIfCancellationRequested();
        ValidatePreviewRequest(request);

        if (!_previews.TryRemove(request.PreviewId, out var previewSnapshot))
            throw new WorkerOperationException("RETENTION_PREVIEW_STALE", "全局预览不存在、已被使用或已在 Worker 重启后失效，请刷新预览后再清理。");
        if (DateTime.UtcNow - previewSnapshot.GeneratedUtc > MaximumPreviewAge)
            throw new WorkerOperationException("RETENTION_PREVIEW_STALE", "全局预览已过期，请刷新预览后再清理。");
        if (request.PreviewGeneratedUtc.ToUniversalTime() != previewSnapshot.GeneratedUtc)
            throw new WorkerOperationException("RETENTION_PREVIEW_STALE", "确认的预览不是当前 Worker 保存的预览，请刷新后再清理。");

        var (versions, policies) = await LoadAsync(token).ConfigureAwait(false);
        var plans = BuildPlans(versions, policies);
        if (!MatchesSnapshot(plans, previewSnapshot.Candidates))
            throw new WorkerOperationException("RETENTION_PREVIEW_STALE", "备份状态在确认前已变化，请刷新全局预览后再清理。");

        var result = new RetentionSimulationResultDto();
        var failures = new List<string>();
        var backupRoot = string.IsNullOrWhiteSpace(_options.LudusaviBackupDirectory)
            ? string.Empty
            : Path.GetFullPath(_options.LudusaviBackupDirectory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

        foreach (var group in plans)
        {
            token.ThrowIfCancellationRequested();
            using var lease = await _gameLock.AcquireAsync(group.PlayniteId, GameOperationKind.Retention, _gameLockTimeout, token).ConfigureAwait(false);
            if (lease == null)
            {
                result.SkippedBusyCount++;
                failures.Add($"{group.PlayniteId}: 游戏正在执行备份、恢复或媒体操作，已跳过清理候选；请刷新预览后重试。");
                continue;
            }

            // Re-read while holding the same per-game lock used by backup, restore and
            // metadata updates. A preview is a safety handle, not a substitute for this
            // final identity check.
            var liveVersions = await _store.GetBackupVersionsAsync(group.PlayniteId, token).ConfigureAwait(false);
            var livePolicy = await _store.GetPolicyAsync(group.PlayniteId, token).ConfigureAwait(false);
            var liveGroup = BuildPlan(group.PlayniteId, group.GameName, liveVersions, livePolicy);
            var expectedCandidates = previewSnapshot.Candidates
                .Where(x => string.Equals(x.PlayniteId, group.PlayniteId, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (!MatchesGroupSnapshot(liveGroup, expectedCandidates))
            {
                result.SkippedChangedCount += Math.Max(expectedCandidates.Count, liveGroup.Plan.DeleteCandidates.Count);
                failures.Add($"{group.PlayniteId}: 候选版本或保留策略在执行前发生变化，已跳过该游戏；请刷新预览后重试。");
                continue;
            }

            foreach (var candidate in liveGroup.Plan.DeleteCandidates)
            {
                token.ThrowIfCancellationRequested();
                var current = liveGroup.Versions.FirstOrDefault(x => string.Equals(x.BackupId, candidate.BackupId, StringComparison.OrdinalIgnoreCase));
                if (current == null) continue;
                var expectedFingerprint = previewSnapshot.Candidates.FirstOrDefault(x =>
                    string.Equals(x.PlayniteId, group.PlayniteId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(x.BackupId, candidate.BackupId, StringComparison.OrdinalIgnoreCase));
                var snapshot = ToSnapshot(current);
                if (snapshot.IsLocked || snapshot.IsPreRestore || snapshot.IsHealthyRestorePoint)
                {
                    result.SkippedProtectedCount++;
                    continue;
                }
                if (string.IsNullOrWhiteSpace(current.ArchivePath) || !File.Exists(current.ArchivePath))
                {
                    result.SkippedMissingCount++;
                    continue;
                }

                string fullPath;
                try
                {
                    fullPath = Path.GetFullPath(current.ArchivePath);
                }
                catch
                {
                    result.SkippedUnsupportedCount++;
                    continue;
                }
                if (!fullPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ||
                    backupRoot.Length == 0 ||
                    !fullPath.StartsWith(backupRoot, StringComparison.OrdinalIgnoreCase) ||
                    HasReparsePointBoundary(backupRoot, fullPath))
                {
                    result.SkippedUnsupportedCount++;
                    continue;
                }

                if (expectedFingerprint == null || !MatchesArchiveMetadata(expectedFingerprint, fullPath))
                {
                    result.SkippedChangedCount++;
                    failures.Add($"{current.PlayniteId}/{current.BackupId}: 归档文件身份在执行前发生变化，已跳过；请刷新预览后重试。");
                    continue;
                }

                long fileBytes;
                try
                {
                    fileBytes = new FileInfo(fullPath).Length;
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    failures.Add($"{current.PlayniteId}/{current.BackupId}: {ex.Message}");
                    continue;
                }

                try
                {
                    // Recheck immediately before moving. The preview and the initial
                    // identity check are not enough if a path is replaced by a junction
                    // or reparse point while the Worker is processing another game.
                    if (HasReparsePointBoundary(backupRoot, fullPath))
                    {
                        result.SkippedUnsupportedCount++;
                        continue;
                    }
                    if (!MatchesArchiveMetadata(expectedFingerprint, fullPath))
                    {
                        result.SkippedChangedCount++;
                        failures.Add($"{current.PlayniteId}/{current.BackupId}: 归档文件在移动前发生变化，已跳过；请刷新预览后重试。");
                        continue;
                    }

                    // Quarantine first so a database failure can put the archive back. The
                    // quarantine is inside the configured root but has no .zip suffix, so it
                    // cannot be mistaken for a Ludusavi archive during a later scan.
                    var quarantinePath = CreateQuarantinePath(backupRoot, current.BackupId);
                    Directory.CreateDirectory(Path.GetDirectoryName(quarantinePath)!);
                    File.Move(fullPath, quarantinePath);
                    try
                    {
                        await _store.DeleteBackupVersionAsync(current.PlayniteId, current.BackupId, token).ConfigureAwait(false);
                    }
                    catch
                    {
                        TryRestoreFromQuarantine(quarantinePath, fullPath);
                        throw;
                    }

                    try
                    {
                        File.Delete(quarantinePath);
                    }
                    catch (Exception cleanupException)
                    {
                        result.PendingQuarantineCount++;
                        result.PendingQuarantineBytes += fileBytes;
                        failures.Add($"{current.PlayniteId}/{current.BackupId}: 归档已安全移出但隔离文件清理失败 {cleanupException.Message}");
                    }
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    if (!failures.Any(x => x.StartsWith($"{current.PlayniteId}/{current.BackupId}:", StringComparison.Ordinal)))
                        failures.Add($"{current.PlayniteId}/{current.BackupId}: 清理失败 {ex.Message}");
                    continue;
                }
                result.DeletedCount++;
                result.FreedBytes += fileBytes;
            }
        }

        await _store.AppendAuditAsync(
            "RetentionSimulation",
            "全局保留策略清理已应用",
            JsonSerializer.Serialize(new
            {
                result.DeletedCount,
                result.SkippedProtectedCount,
                result.SkippedMissingCount,
                result.SkippedUnsupportedCount,
                result.SkippedBusyCount,
                result.SkippedChangedCount,
                result.FailedCount,
                result.PendingQuarantineCount,
                result.PendingQuarantineBytes,
                result.FreedBytes,
                Failures = failures.Take(20)
            }),
            token).ConfigureAwait(false);

        result.Summary = $"清理完成：删除 {result.DeletedCount} 个版本，释放 {FormatBytes(result.FreedBytes)}；跳过保护 {result.SkippedProtectedCount}、缺失 {result.SkippedMissingCount}、不支持 {result.SkippedUnsupportedCount}、忙碌 {result.SkippedBusyCount}、状态变化 {result.SkippedChangedCount}，失败 {result.FailedCount}。";
        if (result.PendingQuarantineCount > 0)
            result.Summary += $"有 {result.PendingQuarantineCount} 个隔离文件待重试清理（{FormatBytes(result.PendingQuarantineBytes)}）。";
        if (failures.Count > 0)
            result.Summary += " 失败明细已写入审计日志。";
        return result;
    }

    private static void ValidatePreviewRequest(RetentionSimulationApplyRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.PreviewId) || request.PreviewGeneratedUtc == default)
            throw new WorkerOperationException("RETENTION_PREVIEW_REQUIRED", "请先刷新全局预览，再确认清理候选版本。");

        var age = DateTime.UtcNow - request.PreviewGeneratedUtc.ToUniversalTime();
        if (age > MaximumPreviewAge || age < -MaximumFutureClockSkew)
            throw new WorkerOperationException("RETENTION_PREVIEW_STALE", "全局预览已过期，请刷新预览后再清理。");
    }

    private void StorePreview(RetentionPreviewSnapshot snapshot)
    {
        _previews[snapshot.PreviewId] = snapshot;
        var cutoff = DateTime.UtcNow - MaximumPreviewAge;
        foreach (var item in _previews)
        {
            if (item.Value.GeneratedUtc < cutoff || (_previews.Count > MaximumStoredPreviews && item.Key != snapshot.PreviewId))
                _previews.TryRemove(item.Key, out _);
        }
    }

    private static bool MatchesSnapshot(IEnumerable<RetentionPlanGroup> plans, IReadOnlyList<RetentionCandidateFingerprint> expected)
    {
        var current = plans
            .SelectMany(x => x.Plan.DeleteCandidates.Select(candidate => ToFingerprint(x.PlayniteId, x.Policy, x.Versions, candidate)))
            .ToDictionary(x => CandidateKey(x.PlayniteId, x.BackupId), StringComparer.OrdinalIgnoreCase);
        if (current.Count != expected.Count) return false;
        return expected.All(item => current.TryGetValue(CandidateKey(item.PlayniteId, item.BackupId), out var actual)
                                    && actual.Equals(item));
    }

    private static bool MatchesArchiveMetadata(RetentionCandidateFingerprint expected, string path)
    {
        try
        {
            var file = new FileInfo(path);
            return file.Exists
                && file.Length == expected.ArchiveLength
                && file.LastWriteTimeUtc == expected.ArchiveLastWriteUtc;
        }
        catch
        {
            return false;
        }
    }

    private static bool MatchesGroupSnapshot(RetentionPlanGroup group, IReadOnlyList<RetentionCandidateFingerprint> expected)
    {
        var current = group.Plan.DeleteCandidates
            .Select(candidate => ToFingerprint(group.PlayniteId, group.Policy, group.Versions, candidate))
            .ToDictionary(x => CandidateKey(x.PlayniteId, x.BackupId), StringComparer.OrdinalIgnoreCase);
        if (current.Count != expected.Count) return false;
        return expected.All(item => current.TryGetValue(CandidateKey(item.PlayniteId, item.BackupId), out var actual)
                                    && actual.Equals(item));
    }

    private static RetentionCandidateFingerprint ToFingerprint(
        string playniteId,
        BackupPolicyDto policy,
        IReadOnlyList<BackupVersionDto> versions,
        BackupSnapshot candidate)
    {
        var version = versions.FirstOrDefault(x => string.Equals(x.BackupId, candidate.BackupId, StringComparison.OrdinalIgnoreCase));
        var archiveMetadata = ReadArchiveMetadata(version?.ArchivePath ?? string.Empty);
        return new RetentionCandidateFingerprint(
            playniteId,
            candidate.BackupId,
            NormalizeArchivePath(version?.ArchivePath ?? string.Empty),
            candidate.CreatedUtc,
            candidate.TotalBytes,
            candidate.FileCount,
            candidate.IsLocked,
            candidate.IsPreRestore,
            candidate.IsHealthyRestorePoint,
            GetPolicyFingerprint(policy),
            archiveMetadata.Length,
            archiveMetadata.LastWriteUtc);
    }

    private static string CandidateKey(string playniteId, string backupId)
        => (playniteId ?? string.Empty).Trim().ToUpperInvariant() + "\0" + (backupId ?? string.Empty).Trim().ToUpperInvariant();

    private static string NormalizeArchivePath(string path)
    {
        try
        {
            var expanded = Environment.ExpandEnvironmentVariables(path ?? string.Empty);
            return Path.TrimEndingDirectorySeparator(Path.GetFullPath(expanded));
        }
        catch
        {
            return (path ?? string.Empty).Trim();
        }
    }

    private static (long Length, DateTime LastWriteUtc) ReadArchiveMetadata(string path)
    {
        try
        {
            var file = new FileInfo(path);
            return file.Exists ? (file.Length, file.LastWriteTimeUtc) : (-1, DateTime.MinValue);
        }
        catch
        {
            return (-1, DateTime.MinValue);
        }
    }

    private static bool HasReparsePointBoundary(string backupRoot, string fullPath)
    {
        try
        {
            var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(backupRoot));
            var file = new FileInfo(fullPath);
            if ((file.Attributes & FileAttributes.ReparsePoint) != 0) return true;

            var rootInfo = new DirectoryInfo(root);
            if ((rootInfo.Attributes & FileAttributes.ReparsePoint) != 0) return true;

            var current = file.Directory;
            while (current != null)
            {
                if ((current.Attributes & FileAttributes.ReparsePoint) != 0) return true;
                if (string.Equals(Path.TrimEndingDirectorySeparator(current.FullName), root, StringComparison.OrdinalIgnoreCase))
                    return false;
                current = current.Parent;
            }
        }
        catch
        {
            // An unreadable path cannot be proven to stay below the configured root.
            return true;
        }

        return true;
    }

    private static string GetPolicyFingerprint(BackupPolicyDto policy)
        => string.Join("|", "v1",
            policy.Enabled ? 1 : 0,
            policy.BackupOnGameStop ? 1 : 0,
            policy.BackupDuringPlay ? 1 : 0,
            policy.DuringPlayIntervalMinutes,
            policy.UploadAfterBackup ? 1 : 0,
            policy.SyncMediaDuringPlay ? 1 : 0,
            policy.SyncMediaOnGameStop ? 1 : 0,
            policy.AllowAutomaticRestore ? 1 : 0,
            (int)policy.AnomalyProtectionLevel,
            policy.KeepRecentAllHours,
            policy.KeepDailyDays,
            policy.KeepWeeklyWeeks,
            policy.KeepMonthlyMonths);

    private static string CreateQuarantinePath(string backupRoot, string backupId)
    {
        var root = backupRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var batch = Guid.NewGuid().ToString("N");
        var safeId = string.IsNullOrWhiteSpace(backupId) ? "archive" : backupId;
        foreach (var invalid in Path.GetInvalidFileNameChars()) safeId = safeId.Replace(invalid, '_');
        return Path.Combine(root, QuarantineDirectoryName, batch, safeId + ".pending");
    }

    private static void TryRestoreFromQuarantine(string quarantinePath, string originalPath)
    {
        try
        {
            if (File.Exists(quarantinePath) && !File.Exists(originalPath))
                File.Move(quarantinePath, originalPath);
        }
        catch
        {
            // The original exception contains the database failure. The quarantine path is
            // recorded separately by the audit summary if restoration itself is unavailable.
        }
    }

    private async Task<(List<BackupVersionDto> Versions, Dictionary<string, BackupPolicyDto> Policies)> LoadAsync(CancellationToken token)
    {
        var versions = await _store.GetStorageAnalysisRowsAsync(token).ConfigureAwait(false);
        var policies = await _store.GetAllPoliciesAsync(token).ConfigureAwait(false);
        return (versions, policies);
    }

    private static List<RetentionPlanGroup> BuildPlans(
        List<BackupVersionDto> versions,
        Dictionary<string, BackupPolicyDto> policies)
    {
        return versions
            .GroupBy(x => x.PlayniteId, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var policy = policies.TryGetValue(group.Key, out var value) ? value : new BackupPolicyDto();
                return BuildPlan(group.Key, group.First().LudusaviName, group.ToList(), policy);
            })
            .ToList();
    }

    private static RetentionPlanGroup BuildPlan(
        string playniteId,
        string gameName,
        List<BackupVersionDto> versions,
        BackupPolicyDto policy)
    {
        var snapshots = versions.Select(ToSnapshot).ToList();
        var plan = new RetentionPlanner().CreatePlan(
            snapshots,
            new RetentionPolicy
            {
                KeepAllFor = TimeSpan.FromHours(policy.KeepRecentAllHours),
                KeepDailyDays = policy.KeepDailyDays,
                KeepWeeklyWeeks = policy.KeepWeeklyWeeks,
                KeepMonthlyMonths = policy.KeepMonthlyMonths
            },
            DateTime.UtcNow);
        return new RetentionPlanGroup(playniteId, gameName, versions, policy, plan);
    }

    private static BackupSnapshot ToSnapshot(BackupVersionDto version) => new BackupSnapshot
    {
        BackupId = version.BackupId,
        ParentBackupId = version.ParentBackupId,
        CreatedUtc = version.CreatedUtc,
        TotalBytes = version.TotalBytes,
        FileCount = version.FileCount,
        IsLocked = version.IsLocked,
        IsPreRestore = version.IsPreRestore,
        Comment = version.Comment,
        SourceDevice = version.SourceDevice,
        ReadinessStatus = version.RestoreReadiness?.Status,
        HasSevereAnomaly = version.FileCount == 0 || version.TotalBytes <= 0 ||
                           version.RestoreReadiness?.Status is RestoreReadinessStatus.Corrupted or RestoreReadinessStatus.Failed
    };

    private static RetentionSimulationItemDto ToItem(string playniteId, string gameName, List<BackupVersionDto> versions, BackupSnapshot candidate)
    {
        var version = versions.FirstOrDefault(x => string.Equals(x.BackupId, candidate.BackupId, StringComparison.OrdinalIgnoreCase));
        return new RetentionSimulationItemDto
        {
            PlayniteId = playniteId,
            GameName = gameName,
            BackupId = candidate.BackupId,
            CreatedUtc = candidate.CreatedUtc,
            TotalBytes = candidate.TotalBytes,
            ArchivePath = version?.ArchivePath ?? string.Empty,
            Reason = "超出保留窗口或桶位",
            IsLocked = candidate.IsLocked,
            IsPreRestore = candidate.IsPreRestore,
            IsHealthProtected = candidate.IsHealthyRestorePoint
        };
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes:0} B";
        if (bytes < 1024L * 1024) return $"{bytes / 1024d:0.##} KiB";
        if (bytes < 1024L * 1024 * 1024) return $"{bytes / 1024d / 1024d:0.##} MiB";
        return $"{bytes / 1024d / 1024d / 1024d:0.##} GiB";
    }

    private sealed record RetentionPreviewSnapshot(
        string PreviewId,
        DateTime GeneratedUtc,
        IReadOnlyList<RetentionCandidateFingerprint> Candidates);

    private sealed record RetentionCandidateFingerprint(
        string PlayniteId,
        string BackupId,
        string ArchivePath,
        DateTime CreatedUtc,
        long TotalBytes,
        int FileCount,
        bool IsLocked,
        bool IsPreRestore,
        bool IsHealthProtected,
        string PolicyFingerprint,
        long ArchiveLength,
        DateTime ArchiveLastWriteUtc);

    private sealed record RetentionPlanGroup(
        string PlayniteId,
        string GameName,
        List<BackupVersionDto> Versions,
        BackupPolicyDto Policy,
        RetentionPlan Plan);
}
