using System.Collections.Concurrent;
using System.Threading.Channels;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Worker.Ipc;

/// <summary>
/// In-memory fan-out for best-effort task notifications.
///
/// The task database and the request/response change feed remain the durable source
/// of truth. A slow or disconnected UI must never block a backup, restore, or media
/// operation, so each subscriber has a bounded queue that evicts progress before
/// terminal outcomes.
/// </summary>
public sealed class TaskEventBroadcaster
{
    private const int PerSubscriberCapacity = 128;
    private readonly ConcurrentDictionary<Guid, TaskEventSubscriber> subscribers = new();

    /// <summary>Current live subscriber count, useful for stability probes and diagnostics.</summary>
    public int SubscriberCount => subscribers.Count;

    public TaskEventSubscription Subscribe()
    {
        var id = Guid.NewGuid();
        var subscriber = new TaskEventSubscriber(PerSubscriberCapacity);
        if (!subscribers.TryAdd(id, subscriber)) throw new InvalidOperationException("Could not register task event subscriber.");
        return new TaskEventSubscription(id, subscriber.Reader, Unsubscribe);
    }

    public void Publish(TaskChangeEventDto change)
    {
        if (change?.Task == null) return;
        foreach (var subscriber in subscribers.Values)
            subscriber.Publish(Clone(change));
    }

    private void Unsubscribe(Guid id)
    {
        if (subscribers.TryRemove(id, out var subscriber)) subscriber.Dispose();
    }

    private static TaskChangeEventDto Clone(TaskChangeEventDto change) => new()
    {
        Sequence = change.Sequence,
        OccurredUtc = change.OccurredUtc,
        Task = new TaskStatusDto
        {
            TaskId = change.Task.TaskId,
            RequestId = change.Task.RequestId,
            SessionId = change.Task.SessionId,
            WorkerSessionId = change.Task.WorkerSessionId,
            TaskType = change.Task.TaskType,
            GameId = change.Task.GameId,
            GameName = change.Task.GameName,
            State = change.Task.State,
            ProgressPercent = change.Task.ProgressPercent,
            Message = change.Task.Message,
            StageMessage = change.Task.StageMessage,
            CancellationState = change.Task.CancellationState,
            CreatedUtc = change.Task.CreatedUtc,
            StartedUtc = change.Task.StartedUtc,
            FinishedUtc = change.Task.FinishedUtc,
            ErrorCode = change.Task.ErrorCode,
            ErrorMessage = change.Task.ErrorMessage,
            ProgressCompletedUnits = change.Task.ProgressCompletedUnits,
            ProgressTotalUnits = change.Task.ProgressTotalUnits,
            ProgressUnit = change.Task.ProgressUnit,
            ProgressRatePerSecond = change.Task.ProgressRatePerSecond,
            ProgressEtaSeconds = change.Task.ProgressEtaSeconds,
            ProgressUpdatedUtc = change.Task.ProgressUpdatedUtc,
            SourceReferences = change.Task.SourceReferences?.Select(reference => reference.Clone()).ToList() ?? new List<TaskSourceReferenceDto>(),
            BackupResult = change.Task.BackupResult == null ? null : new BackupResultDto
            {
                LocalState = change.Task.BackupResult.LocalState,
                CloudState = change.Task.BackupResult.CloudState,
                Summary = change.Task.BackupResult.Summary,
                Remediation = change.Task.BackupResult.Remediation
            }
        }
    };
}

/// <summary>
/// A bounded subscriber queue that evicts progress before terminal outcomes.
/// The durable TaskChangeFeed remains the recovery path if a subscriber is gone.
/// </summary>
internal sealed class TaskEventSubscriber : IDisposable
{
    private readonly object gate = new object();
    private readonly Channel<TaskChangeEventDto> channel;

    public TaskEventSubscriber(int capacity)
    {
        channel = Channel.CreateBounded<TaskChangeEventDto>(new BoundedChannelOptions(capacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        });
    }

    public ChannelReader<TaskChangeEventDto> Reader => channel.Reader;

    public void Publish(TaskChangeEventDto change)
    {
        lock (gate)
        {
            if (channel.Writer.TryWrite(change)) return;

            var queued = new List<TaskChangeEventDto>();
            while (channel.Reader.TryRead(out var existing))
                queued.Add(existing);

            var dropIndex = queued.FindIndex(existing => !IsTerminal(existing.Task.State));
            if (dropIndex < 0) dropIndex = queued.Count == 0 ? -1 : 0;
            if (dropIndex >= 0 && dropIndex < queued.Count)
                queued.RemoveAt(dropIndex);

            foreach (var existing in queued)
                channel.Writer.TryWrite(existing);
            channel.Writer.TryWrite(change);
        }
    }

    public void Dispose()
    {
        lock (gate) channel.Writer.TryComplete();
    }

    private static bool IsTerminal(TaskState state)
        => state == TaskState.Succeeded || state == TaskState.Failed || state == TaskState.Cancelled;
}

/// <summary>Owns one transient task-event subscription.</summary>
public sealed class TaskEventSubscription : IDisposable
{
    private readonly Guid id;
    private readonly Action<Guid> unsubscribe;
    private int disposed;

    internal TaskEventSubscription(Guid id, ChannelReader<TaskChangeEventDto> reader, Action<Guid> unsubscribe)
    {
        this.id = id;
        Reader = reader;
        this.unsubscribe = unsubscribe;
    }

    public ChannelReader<TaskChangeEventDto> Reader { get; }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) == 0) unsubscribe(id);
    }
}
