using System.Collections.Concurrent;
using System.Diagnostics;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Ipc;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>Serializes destructive work per game while allowing unrelated games to progress.</summary>
public sealed class TaskCoordinator
{
    private readonly ITaskStatusStore _store;
    private readonly ILogger<TaskCoordinator> _logger;
    private readonly TaskEventBroadcaster _events;
    private readonly string workerSessionId;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _gameLocks = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, TaskRuntime> _taskRuntimes = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentQueue<TaskChangeEventDto> _changes = new();
    private readonly object _changeSignalGate = new();
    private TaskCompletionSource<bool> _changeSignal = NewChangeSignal();
    private long _changeSequence;
    private const int ChangeRetention = 500;

    private sealed class TaskRuntime
    {
        public TaskRuntime(TaskStatusDto task, CancellationTokenSource token)
        {
            Task = task;
            Token = token;
        }

        public TaskStatusDto Task { get; }
        public CancellationTokenSource Token { get; }
        public SemaphoreSlim Gate { get; } = new SemaphoreSlim(1, 1);
        public bool CancellationRequested { get; set; }
        public bool Terminal { get; set; }
    }

    public TaskCoordinator(ITaskStatusStore store, TaskEventBroadcaster events, ILogger<TaskCoordinator> logger, WorkerOptions? options = null)
    { _store=store; _events=events; _logger=logger; workerSessionId=options?.WorkerSessionId ?? Guid.NewGuid().ToString("N"); }

    public async Task<TaskStatusDto> RunAsync(
        string taskType,
        string gameId,
        string gameName,
        Func<TaskProgress, CancellationToken, Task> operation,
        CancellationToken outerToken,
        string sessionId = "",
        string? taskId = null,
        DateTime? createdUtc = null,
        string requestId = "",
        IReadOnlyCollection<TaskSourceReferenceDto>? sourceReferences = null)
    {
        var taskSources = (sourceReferences ?? Array.Empty<TaskSourceReferenceDto>())
            .Where(reference => reference != null && reference.HasStableIdentity)
            .Select(reference => reference.Clone())
            .ToList();
        if (!string.IsNullOrWhiteSpace(gameId)
            && !taskSources.Any(reference => reference.Kind == TaskSourceReferenceKind.Game
                && string.Equals(reference.StableId, gameId, StringComparison.OrdinalIgnoreCase)))
        {
            taskSources.Insert(0, new TaskSourceReferenceDto
            {
                Kind = TaskSourceReferenceKind.Game,
                StableId = gameId,
                PlayniteId = gameId,
                DisplayName = gameName ?? string.Empty,
                Detail = "任务关联的 Playnite 游戏 ID"
            });
        }
        var task = new TaskStatusDto
        {
            TaskId=string.IsNullOrWhiteSpace(taskId) ? Guid.NewGuid().ToString("N") : taskId,
            RequestId=requestId ?? string.Empty,
            SessionId=sessionId ?? string.Empty, WorkerSessionId=workerSessionId, TaskType=taskType, GameId=gameId, GameName=gameName ?? string.Empty,
            State=TaskState.Queued, ProgressPercent=0, Message="等待执行", StageMessage="等待执行", CancellationState=TaskCancellationStates.None, CreatedUtc=createdUtc ?? DateTime.UtcNow, ElapsedSeconds=0,
            SourceReferences=taskSources
        };
        var gate=_gameLocks.GetOrAdd(string.IsNullOrWhiteSpace(gameId)?"__global__":gameId,_=>new SemaphoreSlim(1,1));
        using var linked=CancellationTokenSource.CreateLinkedTokenSource(outerToken);
        var runtime = new TaskRuntime(task, linked);
        _taskRuntimes[task.TaskId] = runtime;
        try
        {
            await PersistAndPublishAsync(task, outerToken).ConfigureAwait(false);
        }
        catch
        {
            _taskRuntimes.TryRemove(task.TaskId, out _);
            runtime.Gate.Dispose();
            throw;
        }
        var gateEntered=false;
        TaskProgress? progress=null;
        try
        {
            await gate.WaitAsync(linked.Token).ConfigureAwait(false);
            gateEntered=true;
            task.State=TaskState.Running;task.StartedUtc=DateTime.UtcNow;task.MonotonicStartedTimestamp=MonotonicTaskClock.Timestamp;task.MonotonicFrequency=MonotonicTaskClock.Frequency;task.ElapsedSeconds=0;task.Message="正在执行";task.StageMessage="正在执行";task.CancellationState=TaskCancellationStates.None;
            await PersistAndPublishAsync(task,linked.Token).ConfigureAwait(false);
            progress=new TaskProgress(async (percent,message)=>
            {
                task.ProgressPercent=Math.Clamp(percent,0,100);task.Message=message;task.StageMessage=message ?? string.Empty;
                await PersistAndPublishAsync(task,CancellationToken.None).ConfigureAwait(false);
            }, result => task.BackupResult = result, report =>
            {
                report.TaskId = task.TaskId;
                task.RestoreReport = report;
            }, metrics =>
            {
                task.ProgressCompletedUnits = metrics?.CompletedUnits ?? -1;
                task.ProgressTotalUnits = metrics?.TotalUnits ?? -1;
                task.ProgressUnit = metrics?.Unit ?? string.Empty;
                task.ProgressRatePerSecond = metrics?.RatePerSecond ?? 0;
                task.ProgressEtaSeconds = metrics?.EtaSeconds;
                task.ProgressUpdatedUtc = metrics?.UpdatedUtc;
            });
            await operation(progress,linked.Token).ConfigureAwait(false);
            await CompleteSuccessAsync(runtime, progress).ConfigureAwait(false);
        }
        catch(OperationCanceledException)
        {
            await CompleteCancelledAsync(runtime, progress).ConfigureAwait(false);
        }
        catch(WorkerOperationException ex)
        {
            _logger.LogError(ex,"Task {TaskType} failed for {Game}: {Code}",taskType,gameName,ex.Code);
            MarkRestoreFailure(progress, ex.Code);
            await CompleteFailureAsync(runtime, ex.Code, string.IsNullOrWhiteSpace(ex.DiagnosticDetail)?ex.Message:$"{ex.Message} | {ex.DiagnosticDetail}").ConfigureAwait(false);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex,"Task {TaskType} failed for {Game}",taskType,gameName);
            var errorCode = ex.GetType().Name;
            MarkRestoreFailure(progress, errorCode);
            await CompleteFailureAsync(runtime, errorCode, ex.Message).ConfigureAwait(false);
        }
        finally
        {
            try
            {
                await PersistAndPublishAsync(task,CancellationToken.None).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                // The operation result and its durable terminal record are separate
                // outcomes.  Never let a storage outage strand the per-game gate or
                // keep a completed task runtime reachable from Cancel() only until terminal persistence is attempted.
                _logger.LogError(ex,
                    "Task {TaskId} terminal state persistence failed for game {GameId}. Business result: {BusinessState}; task type: {TaskType}.",
                    task.TaskId, task.GameId, task.State, task.TaskType);
            }
            finally
            {
                _taskRuntimes.TryRemove(task.TaskId,out _);
                runtime.Gate.Dispose();
                if(gateEntered)gate.Release();
            }
        }
        return task;
    }

    private static void MarkRestoreFailure(TaskProgress? progress, string errorCode)
    {
        if (progress?.RestoreReport == null) return;
        progress.RestoreReport.FailureCode = errorCode ?? string.Empty;
        if (progress.RestoreReport.OutcomeKind is not ("RolledBack" or "ManualIntervention"))
            progress.RestoreReport.OutcomeKind = "Failed";
    }

    private static async Task CompleteSuccessAsync(TaskRuntime runtime, TaskProgress? progress)
    {
        await runtime.Gate.WaitAsync(CancellationToken.None).ConfigureAwait(false);
        try
        {
            if (runtime.Terminal) return;
            if (runtime.CancellationRequested)
            {
                runtime.Task.State = TaskState.Cancelled;
                runtime.Task.CancellationState = TaskCancellationStates.Cancelled;
                runtime.Task.Message = string.IsNullOrWhiteSpace(progress?.CancellationMessage) ? "已取消" : progress.CancellationMessage;
                if (progress?.RestoreReport != null)
                {
                    progress.RestoreReport.OutcomeKind = "Cancelled";
                    progress.RestoreReport.FailureCode = string.Empty;
                }
            }
            else
            {
                runtime.Task.State = TaskState.Succeeded;
                runtime.Task.CancellationState = TaskCancellationStates.None;
                runtime.Task.ProgressPercent = 100;
                if (string.IsNullOrWhiteSpace(runtime.Task.Message) || string.Equals(runtime.Task.Message,"正在执行",StringComparison.Ordinal))
                {
                    runtime.Task.Message="已完成";
                    runtime.Task.StageMessage="已完成";
                }
            }
            CaptureMonotonicElapsed(runtime.Task);
            runtime.Task.FinishedUtc = DateTime.UtcNow;
            runtime.Terminal = true;
        }
        finally { runtime.Gate.Release(); }
    }

    private static async Task CompleteCancelledAsync(TaskRuntime runtime, TaskProgress? progress)
    {
        await runtime.Gate.WaitAsync(CancellationToken.None).ConfigureAwait(false);
        try
        {
            if (runtime.Terminal) return;
            runtime.Task.State = TaskState.Cancelled;
            runtime.Task.CancellationState = TaskCancellationStates.Cancelled;
            runtime.Task.Message = string.IsNullOrWhiteSpace(progress?.CancellationMessage) ? "已取消" : progress.CancellationMessage;
            if (progress?.RestoreReport != null)
            {
                progress.RestoreReport.OutcomeKind = "Cancelled";
                progress.RestoreReport.FailureCode = string.Empty;
            }
            CaptureMonotonicElapsed(runtime.Task);
            runtime.Task.FinishedUtc = DateTime.UtcNow;
            runtime.Terminal = true;
        }
        finally { runtime.Gate.Release(); }
    }

    private static async Task CompleteFailureAsync(TaskRuntime runtime, string errorCode, string errorMessage)
    {
        await runtime.Gate.WaitAsync(CancellationToken.None).ConfigureAwait(false);
        try
        {
            if (runtime.Terminal) return;
            runtime.Task.State = TaskState.Failed;
            runtime.Task.CancellationState = runtime.CancellationRequested
                ? TaskCancellationStates.NotInterruptible
                : TaskCancellationStates.None;
            runtime.Task.ErrorCode = errorCode;
            runtime.Task.ErrorMessage = errorMessage;
            runtime.Task.Message = "执行失败";
            CaptureMonotonicElapsed(runtime.Task);
            runtime.Task.FinishedUtc = DateTime.UtcNow;
            runtime.Terminal = true;
        }
        finally { runtime.Gate.Release(); }
    }

    public bool Cancel(string taskId)
        => CancelAsync(taskId).GetAwaiter().GetResult();

    public async Task<bool> CancelAsync(string taskId)
    {
        if(!_taskRuntimes.TryGetValue(taskId,out var runtime)) return false;
        await runtime.Gate.WaitAsync(CancellationToken.None).ConfigureAwait(false);
        try
        {
            if (runtime.Terminal || (runtime.Task.State != TaskState.Queued && runtime.Task.State != TaskState.Running)) return false;
            if (runtime.CancellationRequested) return true;
            runtime.CancellationRequested = true;
            runtime.Task.CancellationState = TaskCancellationStates.Requested;
            runtime.Task.Message = "正在取消";
            try
            {
                await PersistAndPublishAsync(runtime.Task, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Task {TaskId} cancellation request persistence failed; cancellation remains accepted.", taskId);
            }
            try { runtime.Token.Cancel(); }
            catch (ObjectDisposedException) { runtime.CancellationRequested = false; runtime.Task.CancellationState = TaskCancellationStates.None; return false; }
            runtime.Task.CancellationState = TaskCancellationStates.Finalizing;
            runtime.Task.Message = "正在安全收尾";
            try
            {
                await PersistAndPublishAsync(runtime.Task, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Task {TaskId} cancellation state persistence failed; cancellation remains accepted.", taskId);
            }
            return true;
        }
        finally { runtime.Gate.Release(); }
    }

    public TaskChangeFeedDto GetChanges(long afterSequence,int limit)
    {
        // Progress callbacks can reach this queue concurrently. Normalize the small bounded
        // retention window so a scheduler interleave cannot make the first enqueued item look
        // newer than the actual oldest sequence.
        var snapshot=_changes.ToArray().OrderBy(x=>x.Sequence).ToArray();
        var oldest=snapshot.Length==0?Interlocked.Read(ref _changeSequence):snapshot[0].Sequence;
        var latest=Interlocked.Read(ref _changeSequence);
        var resetRequired=afterSequence>latest || (snapshot.Length>0 && afterSequence<oldest-1);
        var changes=resetRequired
            ? snapshot.Take(Math.Clamp(limit,1,500))
            : snapshot.Where(x=>x.Sequence>afterSequence).Take(Math.Clamp(limit,1,500));
        return new TaskChangeFeedDto{LatestSequence=latest,ResetRequired=resetRequired,Changes=changes.ToList()};
    }

    public async Task<TaskChangeFeedDto> WaitForChangesAsync(long afterSequence,int limit,int waitSeconds,CancellationToken token)
    {
        var current=GetChanges(afterSequence,limit);
        if(current.ResetRequired||current.Changes.Count>0||waitSeconds<=0)return current;

        Task signalTask;
        lock(_changeSignalGate) signalTask=_changeSignal.Task;

        // Close the small race between the first snapshot and subscribing to the signal.
        current=GetChanges(afterSequence,limit);
        if(current.ResetRequired||current.Changes.Count>0)return current;

        using var timeout=CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(waitSeconds,1,25)));
        try{await signalTask.WaitAsync(timeout.Token).ConfigureAwait(false);}
        catch(OperationCanceledException) when(!token.IsCancellationRequested){/* Normal long-poll timeout. */}
        return GetChanges(afterSequence,limit);
    }

    private async Task PersistAndPublishAsync(TaskStatusDto task,CancellationToken token)
    {
        await _store.AddOrUpdateTaskAsync(task,token).ConfigureAwait(false);
        var sequence=Interlocked.Increment(ref _changeSequence);
        var change = new TaskChangeEventDto { Sequence = sequence, OccurredUtc = DateTime.UtcNow, Task = Clone(task) };
        _changes.Enqueue(change);
        while(_changes.Count>ChangeRetention && _changes.TryDequeue(out _)) { }
        _events.Publish(change);
        TaskCompletionSource<bool> signal;
        lock(_changeSignalGate)
        {
            signal=_changeSignal;
            _changeSignal=NewChangeSignal();
        }
        signal.TrySetResult(true);
    }

    private static TaskCompletionSource<bool> NewChangeSignal()
        =>new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static void CaptureMonotonicElapsed(TaskStatusDto task)
    {
        task.ElapsedSeconds = Math.Max(0, task.ElapsedSeconds ?? 0)
            + MonotonicTaskClock.SecondsSince(task.MonotonicStartedTimestamp, task.MonotonicFrequency);
        task.MonotonicStartedTimestamp = 0;
        task.MonotonicFrequency = 0;
    }

    private static TaskStatusDto Clone(TaskStatusDto task)=>new()
    {
            TaskId=task.TaskId,SessionId=task.SessionId,WorkerSessionId=task.WorkerSessionId,TaskType=task.TaskType,GameId=task.GameId,GameName=task.GameName,State=task.State,
            RequestId=task.RequestId,
            ProgressPercent=task.ProgressPercent,Message=task.Message,StageMessage=task.StageMessage,CancellationState=task.CancellationState,CreatedUtc=task.CreatedUtc,StartedUtc=task.StartedUtc,
            FinishedUtc=task.FinishedUtc,ElapsedSeconds=task.ElapsedSeconds,MonotonicStartedTimestamp=task.MonotonicStartedTimestamp,MonotonicFrequency=task.MonotonicFrequency,ErrorCode=task.ErrorCode,ErrorMessage=task.ErrorMessage,
            ProgressCompletedUnits=task.ProgressCompletedUnits,ProgressTotalUnits=task.ProgressTotalUnits,ProgressUnit=task.ProgressUnit,
            ProgressRatePerSecond=task.ProgressRatePerSecond,ProgressEtaSeconds=task.ProgressEtaSeconds,ProgressUpdatedUtc=task.ProgressUpdatedUtc,
            SourceReferences=task.SourceReferences?.Select(reference => reference.Clone()).ToList() ?? new List<TaskSourceReferenceDto>(),
            BackupResult=task.BackupResult == null ? null : new BackupResultDto
            {
                LocalState = task.BackupResult.LocalState,
                CloudState = task.BackupResult.CloudState,
                Summary = task.BackupResult.Summary,
                Remediation = task.BackupResult.Remediation
            },
            RestoreReport=task.RestoreReport?.Clone()
    };
}

/// <summary>Task progress sink safe for background callers.</summary>
public sealed class TaskProgress
{
    private readonly Func<int,string,Task> _report;
    private readonly Action<BackupResultDto>? _setBackupResult;
    private readonly Action<RestoreReportDto>? _setRestoreReport;
    private readonly Action<TaskProgressMetrics?>? _setMetrics;
    private readonly Queue<WorkSample> _samples = new();
    private long lastCompletedUnits = -1;
    private long lastTotalUnits = -1;
    private string lastUnit = string.Empty;
    private long lastAdvancedTimestamp;
    private DateTime lastAdvancedUtc;
    private static readonly TimeSpan SamplingResetAfter = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan StaleAfter = TimeSpan.FromSeconds(10);

    public TaskProgress(
        Func<int,string,Task> report,
        Action<BackupResultDto>? setBackupResult = null,
        Action<RestoreReportDto>? setRestoreReport = null,
        Action<TaskProgressMetrics?>? setMetrics = null)
    {
        _report=report;
        _setBackupResult=setBackupResult;
        _setRestoreReport=setRestoreReport;
        _setMetrics=setMetrics;
    }
    public Task ReportAsync(int percent,string message)
    {
        _setMetrics?.Invoke(null);
        return _report(percent,message);
    }

    /// <summary>
    /// Reports a known unit total. The sampler deliberately needs two advancing
    /// observations before exposing a rate or ETA, and resets after a long gap so
    /// a pause cannot turn into an absurd estimate.
    /// </summary>
    public Task ReportWorkAsync(long completedUnits,long totalUnits,string unit,string message,int? percent = null)
    {
        var nowUtc=DateTime.UtcNow;
        var nowTimestamp=Stopwatch.GetTimestamp();
        var valid=totalUnits>0 && completedUnits>=0 && completedUnits<=totalUnits && !string.IsNullOrWhiteSpace(unit);
        if(!valid)
        {
            _setMetrics?.Invoke(null);
            return _report(percent ?? 0,message);
        }

        var identityChanged=totalUnits!=lastTotalUnits
            || !string.Equals(unit,lastUnit,StringComparison.OrdinalIgnoreCase)
            || completedUnits<lastCompletedUnits;
        var gapAfterProgress=lastAdvancedTimestamp!=0
            && ElapsedSeconds(lastAdvancedTimestamp,nowTimestamp)>SamplingResetAfter.TotalSeconds;
        if(identityChanged || gapAfterProgress)
        {
            _samples.Clear();
            lastAdvancedTimestamp=0;
        }

        if(completedUnits>lastCompletedUnits)
        {
            _samples.Enqueue(new WorkSample(completedUnits,nowTimestamp));
            while(_samples.Count>5)_samples.Dequeue();
            lastAdvancedTimestamp=nowTimestamp;
            lastAdvancedUtc=nowUtc;
        }

        lastCompletedUnits=completedUnits;
        lastTotalUnits=totalUnits;
        lastUnit=unit;

        double rate=0;
        double? eta=null;
        if(_samples.Count>=2)
        {
            var first=_samples.Peek();
            var latest=_samples.Last();
            var elapsed=ElapsedSeconds(first.Timestamp,latest.Timestamp);
            var delta=latest.CompletedUnits-first.CompletedUnits;
            if(elapsed>=0.25 && delta>0)
            {
                rate=delta/elapsed;
                if(completedUnits<totalUnits)eta=(totalUnits-completedUnits)/rate;
            }
        }
        if(lastAdvancedTimestamp==0 || ElapsedSeconds(lastAdvancedTimestamp,nowTimestamp)>StaleAfter.TotalSeconds)
        {
            rate=0;
            eta=null;
        }

        _setMetrics?.Invoke(new TaskProgressMetrics
        {
            CompletedUnits=completedUnits,
            TotalUnits=totalUnits,
            Unit=unit.Trim(),
            RatePerSecond=rate,
            EtaSeconds=eta,
            UpdatedUtc=lastAdvancedTimestamp==0 ? (DateTime?)null : lastAdvancedUtc
        });
        var resolvedPercent=percent ?? (int)Math.Max(0,Math.Min(100,Math.Round(completedUnits*100d/totalUnits)));
        return _report(resolvedPercent,message);
    }

    private static double ElapsedSeconds(long start,long end)
        => (end-start)/(double)Stopwatch.Frequency;

    private readonly struct WorkSample
    {
        public WorkSample(long completedUnits,long timestamp)
        {
            CompletedUnits=completedUnits;
            Timestamp=timestamp;
        }

        public long CompletedUnits { get; }
        public long Timestamp { get; }
    }
    public void SetBackupResult(BackupResultDto result)=>_setBackupResult?.Invoke(result);
    public RestoreReportDto? RestoreReport { get; private set; }
    public void SetRestoreReport(RestoreReportDto report)
    {
        RestoreReport=report;
        _setRestoreReport?.Invoke(report);
    }
    public string CancellationMessage { get; private set; } = string.Empty;
    public void SetCancellationMessage(string message)=>CancellationMessage=message??string.Empty;
}

public sealed class TaskProgressMetrics
{
    public long CompletedUnits { get; init; }
    public long TotalUnits { get; init; }
    public string Unit { get; init; } = string.Empty;
    public double RatePerSecond { get; init; }
    public double? EtaSeconds { get; init; }
    public DateTime? UpdatedUtc { get; init; }
}
