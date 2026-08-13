using System.Collections.Concurrent;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>Combines Playnite events and process detection into one deduplicated game session.</summary>
public sealed class GameSessionCoordinator : BackgroundService, IRestoreSessionState
{
    private readonly SqliteStateStore _store;
    private readonly BackupOrchestrator _backup;
    private readonly MediaSyncService _media;
    private readonly SavePathDetectionService _detection;
    private readonly GameToolService _gameTools;
    private readonly WorkerOptions _options;
    private readonly ILogger<GameSessionCoordinator> _logger;
    private readonly ConcurrentDictionary<string,ActiveSession> _active=new(StringComparer.OrdinalIgnoreCase);

    public GameSessionCoordinator(SqliteStateStore store,BackupOrchestrator backup,MediaSyncService media,SavePathDetectionService detection,GameToolService gameTools,WorkerOptions options,ILogger<GameSessionCoordinator> logger)
    { _store=store;_backup=backup;_media=media;_detection=detection;_gameTools=gameTools;_options=options;_logger=logger; }

    public IReadOnlyCollection<GameSessionEventDto> ActiveSessions=>_active.Values.Select(x=>x.Event).ToList();

    public async Task<GameSessionEventDto> StartAsync(GameSessionEventDto incoming,CancellationToken token)
    {
        if(string.IsNullOrWhiteSpace(incoming.PlayniteId))throw new ArgumentException("PlayniteId is required.");
        if(_active.TryGetValue(incoming.PlayniteId,out var existing))
        {
            // Prefer the precise Playnite event, but preserve one logical session.
            if(incoming.Source==SessionSourceKind.Playnite) existing.Event.Source=SessionSourceKind.Playnite;
            if(incoming.ProcessId.HasValue)existing.Event.ProcessId=incoming.ProcessId;
            if(!string.IsNullOrWhiteSpace(incoming.ProcessName))existing.Event.ProcessName=incoming.ProcessName;
            await _store.AddSessionAsync(existing.Event,token).ConfigureAwait(false);return existing.Event;
        }
        incoming.SessionId=string.IsNullOrWhiteSpace(incoming.SessionId)?Guid.NewGuid().ToString("N"):incoming.SessionId;
        incoming.StartedUtc=incoming.StartedUtc==default?DateTime.UtcNow:incoming.StartedUtc.ToUniversalTime();
        var policy=await _store.GetPolicyAsync(incoming.PlayniteId,token).ConfigureAwait(false);
        var intervalMinutes = Math.Max(1, policy.DuringPlayIntervalMinutes);
        var timedAutomationEnabled=!_options.SafeModeEnabled&&policy.Enabled&&(policy.BackupDuringPlay||policy.SyncMediaDuringPlay);
        var active=new ActiveSession(incoming,DateTime.UtcNow.AddMinutes(intervalMinutes),intervalMinutes,timedAutomationEnabled);
        _active[incoming.PlayniteId]=active;await _store.AddSessionAsync(incoming,token).ConfigureAwait(false);
        if(!_options.SafeModeEnabled)_detection.BeginSessionCapture(incoming);
        _=RunSafeAsync(()=>_gameTools.StartAutomaticAsync(incoming,CancellationToken.None),"automatic game tools",incoming.GameName);
        _logger.LogInformation("Session started for {Game} from {Source}; timed backup is scheduled every {IntervalMinutes} minute(s) (safe mode: {SafeMode})",incoming.GameName,incoming.Source,intervalMinutes,_options.SafeModeEnabled);return incoming;
    }

    public async Task<GameSessionStopResultDto> StopAsync(GameSessionEventDto incoming,CancellationToken token)
    {
        if(!_active.TryRemove(incoming.PlayniteId,out var active))
            return new GameSessionStopResultDto { Stopped=false, SessionId=incoming.SessionId, GameName=incoming.GameName };
        active.Event.StoppedUtc=incoming.StoppedUtc??DateTime.UtcNow;
        active.Event.ElapsedSeconds=incoming.ElapsedSeconds>0?incoming.ElapsedSeconds:(long)(active.Event.StoppedUtc.Value-active.Event.StartedUtc).TotalSeconds;
        await _store.AddSessionAsync(active.Event,token).ConfigureAwait(false);
        await _gameTools.StopAutomaticAsync(active.Event.SessionId,token).ConfigureAwait(false);
        var policy=await _store.GetPolicyAsync(active.Event.PlayniteId,token).ConfigureAwait(false);
        var expectedTaskCount=0;
        if(!_options.SafeModeEnabled&&policy.Enabled&&policy.BackupOnGameStop)
        {
            expectedTaskCount++;
            _=RunSafeAsync(()=>_backup.BackupAsync(new BackupRequestDto{PlayniteIds=new(){active.Event.PlayniteId},Force=true,Reason="GameStopped",SessionId=active.Event.SessionId,NotificationSessionId=active.Event.SessionId},CancellationToken.None),"exit backup",active.Event.GameName);
        }
        if(!_options.SafeModeEnabled&&policy.Enabled&&policy.SyncMediaOnGameStop)
        {
            if(_options.EnableMediaSync) expectedTaskCount += 2; // game media task + shared inbox task (the request keeps its existing default)
            _=RunSafeAsync(()=>_media.SyncAsync(new MediaSyncRequestDto{PlayniteIds=new(){active.Event.PlayniteId},SessionId=active.Event.SessionId,NotificationSessionId=active.Event.SessionId,UploadAfterSync=policy.UploadAfterBackup},CancellationToken.None),"exit media sync",active.Event.GameName);
        }
        var detectedCandidates=0;
        if(!_options.SafeModeEnabled)
        {
            try
            {
                detectedCandidates=await _detection.AnalyzeSessionStopAsync(active.Event,token).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex,"Session save-path analysis failed for {Game}; protection prompt will remain deferred",active.Event.GameName);
            }
        }
        var prompt=_options.SafeModeEnabled
            ? null
            : await BuildProtectionPromptAsync(active.Event,detectedCandidates,token).ConfigureAwait(false);
        _logger.LogInformation("Session stopped for {Game} (safe mode: {SafeMode})",active.Event.GameName,_options.SafeModeEnabled);
        return new GameSessionStopResultDto
        {
            Stopped=true, SessionId=active.Event.SessionId, GameName=active.Event.GameName,
            ExpectedTaskCount=expectedTaskCount, ProtectionPrompt=prompt
        };
    }

    private async Task<ProtectionPromptDto?> BuildProtectionPromptAsync(GameSessionEventDto session,int detectedCandidates,CancellationToken token)
    {
        var existing=await _store.GetSaveCandidatesAsync(session.PlayniteId,token).ConfigureAwait(false);
        var matches=await _store.GetGameMatchesAsync(token).ConfigureAwait(false);
        var recognized=detectedCandidates>0
            ||existing.Any(x=>string.Equals(x.Status,"Accepted",StringComparison.OrdinalIgnoreCase))
            ||(matches.TryGetValue(session.PlayniteId,out var match)&&!string.IsNullOrWhiteSpace(match.Name));
        var state=await _store.GetProtectionPromptRecordAsync(session.PlayniteId,token).ConfigureAwait(false);
        if(state.State==ProtectionPromptState.Enabled||state.State==ProtectionPromptState.Dismissed)
        {
            await _store.RecordProtectionPromptObservationAsync(session.PlayniteId,recognized,false,token).ConfigureAwait(false);
            return null;
        }

        // Keep a deferred decision quiet until a later session observes a newly recognized save.
        // Never issue a prompt for an unchanged "no save found" observation.
        var shouldPrompt=recognized&&(
            !state.LastSaveRecognized
            ||state.State==ProtectionPromptState.NeverShown
            ||(state.State==ProtectionPromptState.Deferred && (!state.LastPromptUtc.HasValue || DateTime.UtcNow-state.LastPromptUtc.Value>=TimeSpan.FromDays(7))));
        if(!recognized)
        {
            await _store.AppendAuditAsync("Protection","暂未发现存档",System.Text.Json.JsonSerializer.Serialize(new{session.PlayniteId,session.SessionId}),token).ConfigureAwait(false);
        }
        await _store.RecordProtectionPromptObservationAsync(session.PlayniteId,recognized,shouldPrompt,token).ConfigureAwait(false);
        return new ProtectionPromptDto
        {
            ShouldPrompt=shouldPrompt,SaveRecognized=recognized,PlayniteId=session.PlayniteId,GameName=session.GameName,
            State=state.State,
            Message=recognized
                ? $"已发现“{session.GameName}”的存档路径，是否启用自动保护？"
                : $"本次退出暂未发现“{session.GameName}”的存档路径；如果之后出现存档，可以在存档中心再次确认。"
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            foreach(var pair in _active.ToArray())
            {
                var policy=await _store.GetPolicyAsync(pair.Key,stoppingToken).ConfigureAwait(false);
                var intervalMinutes = Math.Max(1, policy.DuringPlayIntervalMinutes);
                var timedBackupEnabled=!_options.SafeModeEnabled&&policy.Enabled&&policy.BackupDuringPlay;
                var timedMediaEnabled=!_options.SafeModeEnabled&&policy.Enabled&&policy.SyncMediaDuringPlay;
                var timedAutomationEnabled=timedBackupEnabled||timedMediaEnabled;
                if(pair.Value.IntervalMinutes!=intervalMinutes)
                {
                    pair.Value.IntervalMinutes=intervalMinutes;
                    pair.Value.NextBackupUtc=DateTime.UtcNow.AddMinutes(intervalMinutes);
                    _logger.LogInformation("Timed backup schedule changed for {Game}; next run is in {IntervalMinutes} minute(s)",pair.Value.Event.GameName,intervalMinutes);
                }
                if(pair.Value.TimedAutomationEnabled!=timedAutomationEnabled)
                {
                    pair.Value.TimedAutomationEnabled=timedAutomationEnabled;
                    if(timedAutomationEnabled)pair.Value.NextBackupUtc=DateTime.UtcNow.AddMinutes(intervalMinutes);
                    _logger.LogInformation("Timed automation schedule {State} for {Game}; next UTC {NextUtc}",timedAutomationEnabled?"enabled":"disabled",pair.Value.Event.GameName,timedAutomationEnabled?pair.Value.NextBackupUtc:null);
                }
                if(!timedAutomationEnabled||DateTime.UtcNow<pair.Value.NextBackupUtc)continue;
                var scheduledUtc=pair.Value.NextBackupUtc;
                do pair.Value.NextBackupUtc=pair.Value.NextBackupUtc.AddMinutes(intervalMinutes);
                while(pair.Value.NextBackupUtc<=DateTime.UtcNow);
                _logger.LogInformation("Timed automation cadence reached for {Game}; scheduled UTC {ScheduledUtc:o}, next UTC {NextUtc:o}",pair.Value.Event.GameName,scheduledUtc,pair.Value.NextBackupUtc);
                if(timedBackupEnabled&&Interlocked.CompareExchange(ref pair.Value.BackupPending,1,0)==0)
                {
                    _logger.LogInformation("Starting timed backup for {Game}",pair.Value.Event.GameName);
                    _=RunTimedBackupAsync(pair.Key,pair.Value);
                }
                else if(timedBackupEnabled)
                    _logger.LogWarning("Skipped overlapping timed backup for {Game}; the previous backup is still pending",pair.Value.Event.GameName);
                if(timedMediaEnabled)
                    _=RunSafeAsync(()=>_media.SyncAsync(new MediaSyncRequestDto{PlayniteIds=new(){pair.Key},SessionId=pair.Value.Event.SessionId,UploadAfterSync=false},CancellationToken.None),"timed media sync",pair.Value.Event.GameName);
            }
            // Keep a one-minute policy reasonably precise without running any file work on this loop.
            await Task.Delay(TimeSpan.FromSeconds(5),stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task RunSafeAsync(Func<Task> operation,string label,string game)
    {
        try{await operation().ConfigureAwait(false);}catch(Exception ex){_logger.LogError(ex,"{Label} failed for {Game}",label,game);}
    }

    private async Task RunTimedBackupAsync(string playniteId,ActiveSession active)
    {
        try
        {
            await RunSafeAsync(()=>_backup.BackupAsync(new BackupRequestDto
            {
                PlayniteIds=new(){playniteId},Force=true,Reason="DuringPlay",SessionId=active.Event.SessionId
            },CancellationToken.None),"timed backup",active.Event.GameName).ConfigureAwait(false);
        }
        finally
        {
            Interlocked.Exchange(ref active.BackupPending,0);
        }
    }

    private sealed class ActiveSession
    {
        public ActiveSession(GameSessionEventDto @event,DateTime nextBackupUtc,int intervalMinutes,bool timedAutomationEnabled){Event=@event;NextBackupUtc=nextBackupUtc;IntervalMinutes=intervalMinutes;TimedAutomationEnabled=timedAutomationEnabled;}
        public GameSessionEventDto Event{get;}
        public DateTime NextBackupUtc{get;set;}
        public int IntervalMinutes{get;set;}
        public bool TimedAutomationEnabled{get;set;}
        public int BackupPending;
    }
}
