using System.Diagnostics;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Core.Services;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>Builds one dashboard snapshot so the Playnite UI can refresh atomically.</summary>
public sealed class DashboardService
{
    private readonly SqliteStateStore _store;
    private readonly GameSessionCoordinator _sessions;
    private readonly LudusaviClient _ludusavi;
    private readonly RcloneClient _rclone;
    private readonly WorkerOptions _options;
    private readonly ILogger<DashboardService> _logger;
    private readonly GameHealthAssessmentService _health = new();
    private readonly SemaphoreSlim _versionGate=new(1,1);
    private string _cachedLudusaviVersion=string.Empty;
    private DateTime _versionCachedUtc=DateTime.MinValue;

    public DashboardService(SqliteStateStore store,GameSessionCoordinator sessions,LudusaviClient ludusavi,RcloneClient rclone,WorkerOptions options,ILogger<DashboardService> logger)
    { _store=store;_sessions=sessions;_ludusavi=ludusavi;_rclone=rclone;_options=options;_logger=logger; }

    public async Task<DashboardSnapshotDto> GetAsync(CancellationToken token)
    {
        var stopwatch=Stopwatch.StartNew();
        var games=await _store.GetDashboardGameRecordsAsync(token).ConfigureAwait(false);
        var active=_sessions.ActiveSessions.Select(x=>x.PlayniteId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var tasks=await _store.GetRecentTasksAsync(50,token).ConfigureAwait(false);
        var taskSummary=await _store.GetTaskSummaryAsync(new TaskQueryDto(),token).ConfigureAwait(false);
        var (localDayStartUtc, localDayEndUtc)=GetCurrentLocalDayUtc();
        var todaySucceededTaskCount=await _store.GetSucceededTaskCountAsync(localDayStartUtc,localDayEndUtc,token).ConfigureAwait(false);
        var findings=await _store.GetOpenFindingsAsync(100,token).ConfigureAwait(false);
        var counts=await _store.GetCountsAsync(token).ConfigureAwait(false);
        var audit=await _store.GetAuditAsync(100,token).ConfigureAwait(false);
        var healthInspection=await _store.GetHealthInspectionStateAsync(token).ConfigureAwait(false);
        // The first dashboard paint must not wait for an external executable.  A cold
        // Ludusavi process can take seconds to start on a large Playnite profile, while
        // the version is informational and is already cached for six hours after the
        // background probe completes.  The next dashboard refresh will expose it.
        var ludusaviVersion = _cachedLudusaviVersion;
        QueueLudusaviVersionRefresh();
        var gameNames=games.ToDictionary(x=>x.Descriptor.PlayniteId,x=>x.Descriptor.Name,StringComparer.OrdinalIgnoreCase);
        var activities = audit.Select(x => ActivityTimelineMapper.Map(x, gameNames)).ToList();
        foreach(var finding in findings)
        {
            if(!string.IsNullOrWhiteSpace(finding.PlayniteId) && gameNames.TryGetValue(finding.PlayniteId,out var gameName))
                finding.GameName=gameName;
            else if(string.IsNullOrWhiteSpace(finding.GameName))
                finding.GameName="全局";
        }
        var snapshot=new DashboardSnapshotDto
        {
            GeneratedUtc=DateTime.UtcNow,WorkerHealthy=true,SafeModeEnabled=_options.SafeModeEnabled,WorkerVersion=typeof(DashboardService).Assembly.GetName().Version?.ToString()??"dev",
            LudusaviAvailable=_ludusavi.IsAvailable,RcloneAvailable=_rclone.IsAvailable,LudusaviVersion=ludusaviVersion,
            LudusaviExecutable=_options.LudusaviExecutable,LudusaviBackupDirectory=_options.LudusaviBackupDirectory,BackupFormat=_options.BackupFormat,
            ManagedGames=counts.Games,MatchedGames=counts.Matched,
            RunningGames=active.Count,PendingCloudTasks=taskSummary.PendingCloudCount,TaskSummary=taskSummary,TodaySucceededTaskCount=todaySucceededTaskCount,
            UnassignedMediaCount=counts.Unassigned,HealthInspection=healthInspection,RecentTasks=tasks,Findings=findings,RecentAudit=audit,RecentActivities=activities
        };
        foreach(var record in games)
        {
            var game=record.Descriptor;
            var matched=!string.IsNullOrWhiteSpace(record.LudusaviName);
            var assessment = _health.Assess(new GameHealthInput
            {
                LudusaviMatched = matched,
                LastPlayedUtc = game.LastPlayedUtc,
                LastBackupUtc = record.LastBackupUtc,
                BackupVersionCount = record.BackupVersionCount,
                LastBackupTaskState = record.LastBackupTaskState,
                RecentBackupFailureCount = record.RecentBackupFailureCount,
                LatestRestoreReadinessStatus = record.LatestRestoreReadiness?.Status,
                OpenFindingWarningCount = record.OpenFindingWarningCount,
                OpenFindingErrorCount = record.OpenFindingErrorCount,
                LatestFindingTitle = record.LatestFindingTitle,
                CloudState = record.CloudState,
                CloudEnabled = _options.EnableCloudUpload && record.Policy.UploadAfterBackup && _rclone.IsConfigured
            }, DateTime.UtcNow);
            snapshot.Games.Add(new GameStatusDto
            {
                PlayniteId=game.PlayniteId,Name=game.Name,Platform=game.Platform,IsInstalled=game.IsInstalled,LastPlayedUtc=game.LastPlayedUtc,IsRunning=active.Contains(game.PlayniteId),LudusaviMatched=matched,
                PlayniteIsInstalled=game.PlayniteIsInstalled,InstallStateSource=game.InstallStateSource,DescriptorSyncedUtc=record.DescriptorSyncedUtc,
                LudusaviName=record.LudusaviName,LastBackupUtc=record.LastBackupUtc,BackupVersionCount=record.BackupVersionCount,
                LastMediaSyncUtc=record.LastMediaUtc,MediaCount=record.MediaCount,CloudState=record.CloudState,
                HealthState=assessment.State.ToString(),
                HealthSummary=assessment.Summary,
                HealthReasons=assessment.Reasons.ToList(),
                LatestRestoreReadinessStatus=record.LatestRestoreReadiness?.Status,
                Policy=record.Policy
            });
        }
        snapshot.HealthyGames = snapshot.Games.Count(x => x.HealthState == GameHealthState.Healthy.ToString());
        snapshot.AttentionGames = snapshot.Games.Count(x => x.HealthState == GameHealthState.Attention.ToString());
        snapshot.RiskGames = snapshot.Games.Count(x => x.HealthState == GameHealthState.Risk.ToString());
        snapshot.UnknownGames = snapshot.Games.Count(x => x.HealthState == GameHealthState.Unknown.ToString());
        snapshot.WarningGames = snapshot.AttentionGames + snapshot.RiskGames;
        stopwatch.Stop();
        _logger.LogDebug("[PERF] DashboardSnapshot fetch={FetchMs}ms games={Games} tasks={Tasks} findings={Findings} audit={Audit}",
            stopwatch.ElapsedMilliseconds,snapshot.Games.Count,snapshot.RecentTasks.Count,snapshot.Findings.Count,snapshot.RecentAudit.Count);
        return snapshot;
    }

    private static (DateTime StartUtc,DateTime EndUtc) GetCurrentLocalDayUtc()
    {
        // Convert the local half-open day before querying SQLite so DST transition days
        // use 23/25 actual hours instead of assuming a fixed 24-hour UTC interval.
        var localStart=DateTime.SpecifyKind(DateTime.Now.Date,DateTimeKind.Local);
        return (localStart.ToUniversalTime(),localStart.AddDays(1).ToUniversalTime());
    }

    private void QueueLudusaviVersionRefresh()
    {
        if(!_ludusavi.IsAvailable || DateTime.UtcNow-_versionCachedUtc<TimeSpan.FromHours(6)) return;

        // WaitAsync(0) coalesces concurrent dashboard snapshots without adding another
        // long-lived task or making any IPC caller wait for the version process.
        _ = RefreshLudusaviVersionAsync();
    }

    private async Task RefreshLudusaviVersionAsync()
    {
        if(!await _versionGate.WaitAsync(0).ConfigureAwait(false)) return;
        var stopwatch=Stopwatch.StartNew();
        try
        {
            if(!_ludusavi.IsAvailable || DateTime.UtcNow-_versionCachedUtc<TimeSpan.FromHours(6)) return;
            _cachedLudusaviVersion=await _ludusavi.GetVersionAsync(CancellationToken.None).ConfigureAwait(false);
            _versionCachedUtc=DateTime.UtcNow;
            stopwatch.Stop();
            _logger.LogDebug("[PERF] Background Ludusavi version probe={ElapsedMs}ms result={Result}", stopwatch.ElapsedMilliseconds, string.IsNullOrWhiteSpace(_cachedLudusaviVersion) ? "empty" : "available");
        }
        catch(Exception ex)
        {
            stopwatch.Stop();
            _logger.LogDebug(ex, "Background Ludusavi version probe failed after {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
        }
        finally
        {
            _versionGate.Release();
        }
    }
}
