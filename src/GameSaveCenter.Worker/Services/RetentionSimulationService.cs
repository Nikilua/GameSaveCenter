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
    private readonly WorkerOptions _options;
    private readonly SqliteStateStore _store;
    private readonly ILogger<RetentionSimulationService> _logger;

    public RetentionSimulationService(WorkerOptions options, SqliteStateStore store, ILogger<RetentionSimulationService> logger)
    {
        _options = options;
        _store = store;
        _logger = logger;
    }

    public async Task<RetentionSimulationPreviewDto> PreviewAsync(CancellationToken token)
    {
        var (versions, policies) = await LoadAsync(token).ConfigureAwait(false);
        var plans = BuildPlans(versions, policies);
        var preview = new RetentionSimulationPreviewDto
        {
            GeneratedUtc = DateTime.UtcNow,
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
        preview.Summary = $"现有 {preview.ExistingVersionCount} 个版本，建议保留 {preview.KeepVersionCount} 个，候选清理 {preview.DeleteCandidateCount} 个（预计释放 {preview.EstimatedReleaseDisplay}）；用户锁定 {preview.UserLockedCount}，健康恢复点保护 {preview.HealthProtectedCount}，PreRestore {preview.PreRestoreCount}。预览只读，清理不会自动执行。";
        return preview;
    }

    public async Task<RetentionSimulationResultDto> ApplyAsync(RetentionSimulationApplyRequestDto request, CancellationToken token)
    {
        if (request == null || !request.Confirmed)
            throw new WorkerOperationException("RETENTION_APPLY_NOT_CONFIRMED", "全局清理需要二次确认；预览只读不会删除任何文件。");

        var (versions, policies) = await LoadAsync(token).ConfigureAwait(false);
        var plans = BuildPlans(versions, policies);
        var result = new RetentionSimulationResultDto();
        var failures = new List<string>();
        var backupRoot = string.IsNullOrWhiteSpace(_options.LudusaviBackupDirectory)
            ? string.Empty
            : Path.GetFullPath(_options.LudusaviBackupDirectory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;

        foreach (var group in plans)
        {
            foreach (var candidate in group.Plan.DeleteCandidates)
            {
                token.ThrowIfCancellationRequested();
                var current = group.Versions.FirstOrDefault(x => string.Equals(x.BackupId, candidate.BackupId, StringComparison.OrdinalIgnoreCase));
                if (current == null) continue;
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
                    !fullPath.StartsWith(backupRoot, StringComparison.OrdinalIgnoreCase))
                {
                    result.SkippedUnsupportedCount++;
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
                    await _store.DeleteBackupVersionAsync(current.PlayniteId, current.BackupId, token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    failures.Add($"{current.PlayniteId}/{current.BackupId}: 数据库索引删除失败 {ex.Message}");
                    continue;
                }
                try
                {
                    File.Delete(fullPath);
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    failures.Add($"{current.PlayniteId}/{current.BackupId}: 归档删除失败 {ex.Message}");
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
                result.FailedCount,
                result.FreedBytes,
                Failures = failures.Take(20)
            }),
            token).ConfigureAwait(false);

        result.Summary = $"清理完成：删除 {result.DeletedCount} 个版本，释放 {FormatBytes(result.FreedBytes)}；跳过保护 {result.SkippedProtectedCount}、缺失 {result.SkippedMissingCount}、不支持 {result.SkippedUnsupportedCount}，失败 {result.FailedCount}。";
        if (failures.Count > 0)
            result.Summary += " 失败明细已写入审计日志。";
        return result;
    }

    private async Task<(List<BackupVersionDto> Versions, Dictionary<string, BackupPolicyDto> Policies)> LoadAsync(CancellationToken token)
    {
        var versions = await _store.GetStorageAnalysisRowsAsync(token).ConfigureAwait(false);
        var policies = await _store.GetAllPoliciesAsync(token).ConfigureAwait(false);
        return (versions, policies);
    }

    private static List<(string PlayniteId, string GameName, List<BackupVersionDto> Versions, RetentionPlan Plan)> BuildPlans(
        List<BackupVersionDto> versions,
        Dictionary<string, BackupPolicyDto> policies)
    {
        return versions
            .GroupBy(x => x.PlayniteId, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var policy = policies.TryGetValue(group.Key, out var value) ? value : new BackupPolicyDto();
                var snapshots = group.Select(ToSnapshot).ToList();
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
                return (group.Key, group.First().LudusaviName, group.ToList(), plan);
            })
            .ToList();
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
}
