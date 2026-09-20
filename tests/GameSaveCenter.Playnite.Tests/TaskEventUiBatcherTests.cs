using System;
using System.Collections.Generic;
using System.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class TaskEventUiBatcherTests
{
    [Fact]
    public void ProgressUpdatesAreCoalescedAndBounded()
    {
        var scheduled = new List<ScheduledAction>();
        var batches = new List<IReadOnlyList<TaskChangeEventDto>>();
        using var batcher = new TaskEventUiBatcher(
            (action, immediate) => scheduled.Add(new ScheduledAction(action, immediate)),
            changes => batches.Add(changes));

        for (var i = 0; i < 200; i++)
            batcher.Enqueue(Change(i + 1, $"progress-{i}", TaskState.Running));

        Assert.Equal(TaskEventUiBatcher.MaxPendingChanges, batcher.PendingCount);
        Assert.Single(scheduled);
        Drain(scheduled);

        var applied = batches.SelectMany(batch => batch).ToList();
        Assert.Equal(128, applied.Count);
        Assert.Equal(73, applied[0].Sequence);
        Assert.Equal(200, applied[applied.Count - 1].Sequence);
        Assert.All(batches, batch => Assert.InRange(batch.Count, 1, TaskEventUiBatcher.MaxChangesPerBatch));
        Assert.Equal(0, batcher.PendingCount);
    }

    [Fact]
    public void TerminalOutcomeBypassesStaleProgressBatch()
    {
        var scheduled = new List<ScheduledAction>();
        var applied = new List<TaskChangeEventDto>();
        using var batcher = new TaskEventUiBatcher(
            (action, immediate) => scheduled.Add(new ScheduledAction(action, immediate)),
            changes => applied.AddRange(changes));

        batcher.Enqueue(Change(1, "task", TaskState.Running));
        batcher.Enqueue(Change(2, "task", TaskState.Failed));

        Assert.Equal(2, scheduled.Count);
        Assert.False(scheduled[0].Immediate);
        Assert.True(scheduled[1].Immediate);
        scheduled[1].Action();
        scheduled[0].Action();

        var result = Assert.Single(applied);
        Assert.Equal(TaskState.Failed, result.Task.State);
        Assert.Equal(0, batcher.PendingCount);
    }

    [Fact]
    public void DisposeCancelsQueuedUiRefresh()
    {
        var scheduled = new List<ScheduledAction>();
        var applyCount = 0;
        var batcher = new TaskEventUiBatcher(
            (action, immediate) => scheduled.Add(new ScheduledAction(action, immediate)),
            changes => applyCount++);

        batcher.Enqueue(Change(1, "task", TaskState.Running));
        batcher.Dispose();
        Assert.Equal(0, batcher.PendingCount);

        scheduled[0].Action();
        Assert.Equal(0, applyCount);
    }

    private static void Drain(List<ScheduledAction> scheduled)
    {
        while (scheduled.Count > 0)
        {
            var next = scheduled[0];
            scheduled.RemoveAt(0);
            next.Action();
        }
    }

    private static TaskChangeEventDto Change(long sequence, string taskId, TaskState state)
        => new TaskChangeEventDto
        {
            Sequence = sequence,
            Task = new TaskStatusDto { TaskId = taskId, State = state, ProgressPercent = (int)(sequence % 100) }
        };

    private sealed class ScheduledAction
    {
        public ScheduledAction(Action action, bool immediate)
        {
            Action = action;
            Immediate = immediate;
        }

        public Action Action { get; }
        public bool Immediate { get; }
    }
}
