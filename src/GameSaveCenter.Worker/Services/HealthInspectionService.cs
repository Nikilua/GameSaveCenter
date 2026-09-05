using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>
/// Runs one bounded, non-destructive restore-readiness check at a time. The cursor and
/// terminal state are persisted before and after the check so a Worker restart repeats
/// an interrupted candidate instead of silently advancing past it.
/// </summary>
public sealed class HealthInspectionService : BackgroundService
{
    private readonly SqliteStateStore _store;
    private readonly RestoreReadinessService _readiness;
    private readonly GameOperationLock _gameLock;
    private readonly IRestoreSessionState _sessions;
    private readonly WorkerOptions _options;
    private readonly ILogger<HealthInspectionService> _logger;
    private readonly SemaphoreSlim _runGate = new(1, 1);

    public HealthInspectionService(
        SqliteStateStore store,
        RestoreReadinessService readiness,
        GameOperationLock gameLock,
        IRestoreSessionState sessions,
        WorkerOptions options,
        ILogger<HealthInspectionService> logger)
    {
        _store = store;
        _readiness = readiness;
        _gameLock = gameLock;
        _sessions = sessions;
        _options = options;
        _logger = logger;
    }

    public Task<HealthInspectionStateDto> GetStatusAsync(CancellationToken token)
        => _store.GetHealthInspectionStateAsync(token);

    public async Task<HealthInspectionStateDto> SyncPlanAsync(CancellationToken token)
    {
        var state = await _store.GetHealthInspectionStateAsync(token).ConfigureAwait(false);
        var changed = state.Enabled != _options.HealthInspectionEnabled
            || state.IntervalMinutes != _options.HealthInspectionIntervalMinutes
            || state.StaleAfterDays != _options.HealthInspectionStaleAfterDays
            || state.MaxDurationSeconds != _options.HealthInspectionMaxDurationSeconds;
        if (!changed && state.NextDueUtc.HasValue) return state;

        state.Enabled = _options.HealthInspectionEnabled;
        state.IntervalMinutes = _options.HealthInspectionIntervalMinutes;
        state.StaleAfterDays = _options.HealthInspectionStaleAfterDays;
        state.MaxDurationSeconds = _options.HealthInspectionMaxDurationSeconds;
        if (changed || !state.NextDueUtc.HasValue)
            state.NextDueUtc = DateTime.UtcNow.AddMinutes(state.IntervalMinutes);
        await _store.SaveHealthInspectionStateAsync(state, token).ConfigureAwait(false);
        return state;
    }

    public async Task<HealthInspectionStateDto> RunNowAsync(CancellationToken token)
    {
        var state = await SyncPlanAsync(token).ConfigureAwait(false);
        if (!await _runGate.WaitAsync(0, token).ConfigureAwait(false))
            return await _store.GetHealthInspectionStateAsync(token).ConfigureAwait(false);
        try
        {
            return await RunOneAsync(state, "manual", token).ConfigureAwait(false);
        }
        finally
        {
            _runGate.Release();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await SyncPlanAsync(stoppingToken).ConfigureAwait(false);
        while (!stoppingToken.IsCancellationRequested)
        {
            var state = await _store.GetHealthInspectionStateAsync(stoppingToken).ConfigureAwait(false);
            if (!state.Enabled || !IsDue(state, DateTime.UtcNow))
            {
                await Task.Delay(GetDelay(state), stoppingToken).ConfigureAwait(false);
                continue;
            }

            if (await _runGate.WaitAsync(0, stoppingToken).ConfigureAwait(false))
            {
                try { await RunOneAsync(state, "scheduled", stoppingToken).ConfigureAwait(false); }
                finally { _runGate.Release(); }
            }
        }
    }

    private async Task<HealthInspectionStateDto> RunOneAsync(HealthInspectionStateDto state, string source, CancellationToken token)
    {
        var startedUtc = DateTime.UtcNow;
        state.LastStartedUtc = startedUtc;
        state.LastStatus = "Running";
        state.LastSummary = source == "manual" ? "正在运行手动恢复可用性巡检。" : "正在运行周期恢复可用性巡检。";
        await SaveStateBestEffortAsync(state).ConfigureAwait(false);

        BackupVersionDto? candidate = null;
        RestoreReadinessDto? readiness = null;
        var readinessPersisted = false;
        try
        {
            var versions = await _store.GetAllBackupVersionsForInspectionAsync(token).ConfigureAwait(false);
            candidate = SelectCandidate(versions, state, DateTime.UtcNow);
            if (candidate == null)
                return await CompleteAsync(state, "NoBackups", "当前没有可用于恢复可用性巡检的备份版本。", null, null).ConfigureAwait(false);

            state.LastPlayniteId = candidate.PlayniteId;
            state.LastBackupId = candidate.BackupId;
            state.CursorPlayniteId = candidate.PlayniteId;
            state.CursorBackupId = candidate.BackupId;

            var active = _sessions.ActiveSessions.Any(x => string.Equals(x.PlayniteId, candidate.PlayniteId, StringComparison.OrdinalIgnoreCase));
            if (active)
                return await CompleteAsync(state, "Deferred", "该游戏正在运行，本轮已推迟高成本恢复校验。", candidate, "game-running").ConfigureAwait(false);

            using var lease = await _gameLock.AcquireAsync(candidate.PlayniteId, GameOperationKind.RestoreReadiness, TimeSpan.Zero, token).ConfigureAwait(false);
            if (lease == null)
                return await CompleteAsync(state, "Deferred", "该游戏已有备份、恢复或媒体操作，本轮已推迟恢复校验。", candidate, "game-operation-busy").ConfigureAwait(false);

            using var budget = CancellationTokenSource.CreateLinkedTokenSource(token);
            budget.CancelAfter(TimeSpan.FromSeconds(_options.HealthInspectionMaxDurationSeconds));
            var manifest = await _store.GetBackupManifestAsync(candidate.PlayniteId, candidate.BackupId, budget.Token).ConfigureAwait(false);
            readiness = await _readiness.ValidateAsync(candidate, manifest, _options.RestoreReadinessDirectory, budget.Token).ConfigureAwait(false);
            await _store.SaveRestoreReadinessAsync(candidate.PlayniteId, candidate.BackupId, readiness, CancellationToken.None).ConfigureAwait(false);
            readinessPersisted = true;

            if (readiness.Status == RestoreReadinessStatus.Ready)
                await _store.ResolveHealthInspectionFindingAsync(candidate.PlayniteId, candidate.BackupId, CancellationToken.None).ConfigureAwait(false);
            else
                await _store.UpsertHealthInspectionFindingAsync(candidate.PlayniteId, candidate.BackupId, readiness, CancellationToken.None).ConfigureAwait(false);

            var detail = JsonSerializer.Serialize(new
            {
                Source = source,
                candidate.PlayniteId,
                candidate.BackupId,
                Status = readiness.Status.ToString(),
                readiness.CheckedUtc,
                readiness.StagingCleanupStatus
            });
            await _store.AppendAuditAsync("HealthInspection", "恢复可用性巡检完成", detail, CancellationToken.None).ConfigureAwait(false);
            return await CompleteAsync(state, readiness.Status.ToString(), readiness.Summary, candidate, null, readiness).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            return await CompleteAsync(state, "Cancelled", "本轮恢复可用性巡检已取消；下次巡检会从当前游标继续。", candidate, "cancelled").ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        {
            const string summary = "恢复可用性巡检达到单次时间预算，已标记失败；下次巡检会重试。";
            await RecordFailedReadinessBestEffortAsync(candidate, readinessPersisted ? null : summary).ConfigureAwait(false);
            await AppendFailureAuditBestEffortAsync(source, candidate, summary, "TimeBudget").ConfigureAwait(false);
            return await CompleteAsync(state, "Failed", summary, candidate, "time-budget").ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health inspection failed for {PlayniteId}/{BackupId}", candidate?.PlayniteId, candidate?.BackupId);
            var summary = "恢复可用性巡检失败：" + ex.Message;
            await RecordFailedReadinessBestEffortAsync(candidate, readinessPersisted ? null : summary).ConfigureAwait(false);
            await AppendFailureAuditBestEffortAsync(source, candidate, summary, ex.GetType().Name).ConfigureAwait(false);
            return await CompleteAsync(state, "Failed", summary, candidate, "exception").ConfigureAwait(false);
        }
    }

    private async Task RecordFailedReadinessBestEffortAsync(BackupVersionDto? candidate, string? summary)
    {
        if (candidate == null || string.IsNullOrWhiteSpace(summary)) return;
        var readiness = new RestoreReadinessDto
        {
            BackupVersionId = candidate.BackupId,
            CheckedUtc = DateTime.UtcNow,
            Status = RestoreReadinessStatus.Failed,
            ExpectedFileCount = candidate.FileCount,
            ExpectedTotalSize = candidate.TotalBytes,
            ErrorCount = 1,
            StagingCleanupStatus = "NotNeeded",
            Summary = summary
        };
        try
        {
            await _store.SaveRestoreReadinessAsync(candidate.PlayniteId, candidate.BackupId, readiness, CancellationToken.None).ConfigureAwait(false);
            await _store.UpsertHealthInspectionFindingAsync(candidate.PlayniteId, candidate.BackupId, readiness, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not persist failed health inspection readiness for {PlayniteId}/{BackupId}", candidate.PlayniteId, candidate.BackupId);
        }
    }

    private async Task AppendFailureAuditBestEffortAsync(string source, BackupVersionDto? candidate, string summary, string reason)
    {
        try
        {
            await _store.AppendAuditAsync("HealthInspection", summary, JsonSerializer.Serialize(new
            {
                Source = source,
                candidate?.PlayniteId,
                candidate?.BackupId,
                Reason = reason
            }), CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not persist health inspection failure audit");
        }
    }

    private async Task<HealthInspectionStateDto> CompleteAsync(
        HealthInspectionStateDto state,
        string status,
        string summary,
        BackupVersionDto? candidate,
        string? deferredReason,
        RestoreReadinessDto? readiness = null)
    {
        var completedUtc = DateTime.UtcNow;
        state.LastCompletedUtc = completedUtc;
        state.LastStatus = status;
        state.LastSummary = summary;
        state.NextDueUtc = completedUtc.AddMinutes(state.IntervalMinutes);
        if (candidate != null)
        {
            state.LastPlayniteId = candidate.PlayniteId;
            state.LastBackupId = candidate.BackupId;
            state.CursorPlayniteId = candidate.PlayniteId;
            state.CursorBackupId = candidate.BackupId;
        }
        if (status == "Deferred") state.DeferredCount++;
        if (status is "Corrupted" or "Failed" or "Unsupported") state.FailureCount++;
        if (readiness?.Status == RestoreReadinessStatus.Ready && readiness.CheckedUtc.HasValue)
            state.LastSuccessfulUtc = readiness.CheckedUtc;
        await SaveStateBestEffortAsync(state).ConfigureAwait(false);
        if (deferredReason != null && candidate != null)
        {
            try
            {
                await _store.AppendAuditAsync("HealthInspection", "恢复可用性巡检已推迟", JsonSerializer.Serialize(new
                {
                    candidate.PlayniteId,
                    candidate.BackupId,
                    Reason = deferredReason
                }), CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Could not persist deferred health inspection audit"); }
        }
        return state;
    }

    private async Task SaveStateBestEffortAsync(HealthInspectionStateDto state)
    {
        try { await _store.SaveHealthInspectionStateAsync(state, CancellationToken.None).ConfigureAwait(false); }
        catch (Exception ex) { _logger.LogWarning(ex, "Could not persist health inspection state"); }
    }

    private BackupVersionDto? SelectCandidate(IReadOnlyCollection<BackupVersionDto> versions, HealthInspectionStateDto state, DateTime now)
    {
        if (versions.Count == 0) return null;
        var ordered = versions
            .OrderBy(x => x.PlayniteId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.CreatedUtc)
            .ThenBy(x => x.BackupId, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var staleBefore = now.AddDays(-state.StaleAfterDays);
        var priority = ordered
            .Where(x => x.RestoreReadiness == null || !x.RestoreReadiness.CheckedUtc.HasValue || x.RestoreReadiness.CheckedUtc < staleBefore)
            .OrderBy(x => x.RestoreReadiness?.CheckedUtc ?? DateTime.MinValue)
            .ThenByDescending(x => x.CreatedUtc)
            .ThenBy(x => x.PlayniteId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.BackupId, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
        if (priority != null) return priority;

        var cursor = MakeKey(state.CursorPlayniteId, state.CursorBackupId);
        return ordered.FirstOrDefault(x => string.Compare(MakeKey(x.PlayniteId, x.BackupId), cursor, StringComparison.OrdinalIgnoreCase) > 0)
            ?? ordered[0];
    }

    private static bool IsDue(HealthInspectionStateDto state, DateTime now)
        => state.LastStartedUtc.HasValue && (!state.LastCompletedUtc.HasValue || state.LastStartedUtc > state.LastCompletedUtc)
           || !state.NextDueUtc.HasValue || state.NextDueUtc <= now;

    private static TimeSpan GetDelay(HealthInspectionStateDto state)
    {
        if (state.LastStartedUtc.HasValue && (!state.LastCompletedUtc.HasValue || state.LastStartedUtc > state.LastCompletedUtc))
            return TimeSpan.FromSeconds(5);
        if (!state.NextDueUtc.HasValue) return TimeSpan.FromSeconds(30);
        var remaining = state.NextDueUtc.Value - DateTime.UtcNow;
        return remaining <= TimeSpan.Zero ? TimeSpan.FromSeconds(1) : TimeSpan.FromSeconds(Math.Min(30, Math.Max(1, remaining.TotalSeconds)));
    }

    private static string MakeKey(string playniteId, string backupId) => playniteId + "\0" + backupId;
}
