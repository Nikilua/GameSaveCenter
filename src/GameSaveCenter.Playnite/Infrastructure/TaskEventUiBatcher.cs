using System;
using System.Collections.Generic;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.Infrastructure;

/// <summary>
/// Coalesces high-frequency task progress before it reaches the WPF collection.
/// Terminal states bypass the progress queue so a completed/failed task is visible
/// immediately and cannot be evicted by later progress from another task.
/// </summary>
public sealed class TaskEventUiBatcher : IDisposable
{
    public const int MaxPendingChanges = 128;
    public const int MaxChangesPerBatch = 32;

    private const int MaxRememberedTerminalTasks = 256;
    private readonly object gate = new object();
    private readonly Action<Action, bool> scheduleUi;
    private readonly Action<IReadOnlyList<TaskChangeEventDto>> applyBatch;
    private readonly Dictionary<string, TaskChangeEventDto> pendingByTask = new Dictionary<string, TaskChangeEventDto>(StringComparer.OrdinalIgnoreCase);
    private readonly LinkedList<string> pendingOrder = new LinkedList<string>();
    private readonly Dictionary<string, long> terminalSequences = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
    private readonly Queue<string> terminalOrder = new Queue<string>();
    private bool flushScheduled;
    private bool disposed;

    public TaskEventUiBatcher(Action<Action, bool> scheduleUi, Action<IReadOnlyList<TaskChangeEventDto>> applyBatch)
    {
        this.scheduleUi = scheduleUi ?? throw new ArgumentNullException(nameof(scheduleUi));
        this.applyBatch = applyBatch ?? throw new ArgumentNullException(nameof(applyBatch));
    }

    public bool IsDisposed
    {
        get { lock (gate) return disposed; }
    }

    public int PendingCount
    {
        get { lock (gate) return pendingByTask.Count; }
    }

    public void Enqueue(TaskChangeEventDto change)
    {
        if (change == null || change.Task == null || string.IsNullOrWhiteSpace(change.Task.TaskId)) return;

        if (IsTerminal(change.Task.State))
        {
            EnqueueTerminal(change);
            return;
        }

        var shouldSchedule = false;
        lock (gate)
        {
            if (disposed) return;
            if (terminalSequences.TryGetValue(change.Task.TaskId, out var terminalSequence)
                && (change.Sequence <= 0 || terminalSequence <= 0 || change.Sequence <= terminalSequence))
                return;

            if (pendingByTask.TryGetValue(change.Task.TaskId, out var existing))
            {
                if (change.Sequence <= 0 || existing.Sequence <= 0 || change.Sequence >= existing.Sequence)
                    pendingByTask[change.Task.TaskId] = change;
            }
            else
            {
                while (pendingByTask.Count >= MaxPendingChanges && pendingOrder.First != null)
                {
                    pendingByTask.Remove(pendingOrder.First.Value);
                    pendingOrder.RemoveFirst();
                }

                pendingByTask[change.Task.TaskId] = change;
                pendingOrder.AddLast(change.Task.TaskId);
            }

            if (!flushScheduled)
            {
                flushScheduled = true;
                shouldSchedule = true;
            }
        }

        if (shouldSchedule) Schedule(FlushProgress, immediate: false);
    }

    public void Dispose()
    {
        lock (gate)
        {
            if (disposed) return;
            disposed = true;
            pendingByTask.Clear();
            pendingOrder.Clear();
            terminalSequences.Clear();
            terminalOrder.Clear();
            flushScheduled = false;
        }
    }

    private void EnqueueTerminal(TaskChangeEventDto change)
    {
        var shouldSchedule = false;
        lock (gate)
        {
            if (disposed) return;
            if (terminalSequences.TryGetValue(change.Task.TaskId, out var previousSequence)
                && previousSequence > 0
                && change.Sequence > 0
                && change.Sequence <= previousSequence)
                return;

            if (pendingByTask.Remove(change.Task.TaskId))
                RemovePendingId(change.Task.TaskId);

            if (!terminalSequences.ContainsKey(change.Task.TaskId))
                terminalOrder.Enqueue(change.Task.TaskId);
            terminalSequences[change.Task.TaskId] = change.Sequence;
            while (terminalOrder.Count > MaxRememberedTerminalTasks)
            {
                var oldTaskId = terminalOrder.Dequeue();
                terminalSequences.Remove(oldTaskId);
            }
            shouldSchedule = true;
        }

        if (shouldSchedule) Schedule(() => ApplyTerminal(change), immediate: true);
    }

    private void ApplyTerminal(TaskChangeEventDto change)
    {
        lock (gate)
        {
            if (disposed) return;
        }
        applyBatch(new[] { change });
    }

    private void FlushProgress()
    {
        List<TaskChangeEventDto> batch;
        var scheduleNext = false;
        lock (gate)
        {
            if (disposed)
            {
                flushScheduled = false;
                return;
            }

            batch = new List<TaskChangeEventDto>(Math.Min(MaxChangesPerBatch, pendingByTask.Count));
            while (batch.Count < MaxChangesPerBatch && pendingOrder.First != null)
            {
                var taskId = pendingOrder.First.Value;
                pendingOrder.RemoveFirst();
                if (pendingByTask.TryGetValue(taskId, out var change))
                {
                    pendingByTask.Remove(taskId);
                    batch.Add(change);
                }
            }

            if (pendingByTask.Count == 0)
                flushScheduled = false;
            else
                scheduleNext = true;
        }

        if (batch.Count > 0) applyBatch(batch);
        if (scheduleNext) Schedule(FlushProgress, immediate: false);
    }

    private void RemovePendingId(string taskId)
    {
        var node = pendingOrder.First;
        while (node != null)
        {
            if (string.Equals(node.Value, taskId, StringComparison.OrdinalIgnoreCase))
            {
                pendingOrder.Remove(node);
                return;
            }
            node = node.Next;
        }
    }

    private void Schedule(Action action, bool immediate)
    {
        try
        {
            scheduleUi(action, immediate);
        }
        catch
        {
            lock (gate)
            {
                if (immediate || pendingByTask.Count == 0)
                    flushScheduled = false;
            }
        }
    }

    private static bool IsTerminal(TaskState state)
        => state == TaskState.Succeeded || state == TaskState.Failed || state == TaskState.Cancelled;
}
