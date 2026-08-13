using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class SessionNotificationAccumulatorTests
{
    [Fact]
    public void CompletesOnlyWhenExpectedTaskCountReached()
    {
        var accumulator = new SessionNotificationAccumulator("Demo");
        accumulator.SetExpectedTaskCount(2);

        Assert.False(accumulator.IsComplete);
        accumulator.Add(Completed("t1"));
        Assert.False(accumulator.IsComplete);
        accumulator.Add(Completed("t2"));
        Assert.True(accumulator.IsComplete);
    }

    [Fact]
    public void SameSessionEmitsFinalSummaryOnlyOnce()
    {
        var accumulator = new SessionNotificationAccumulator("Demo");
        accumulator.SetExpectedTaskCount(1);
        accumulator.Add(Completed("t1"));

        Assert.True(accumulator.TryMarkEmitted());
        Assert.False(accumulator.TryMarkEmitted());
    }

    [Fact]
    public void DuplicateTaskDeliveryReplacesSnapshotWithoutDuplicatingTasks()
    {
        var accumulator = new SessionNotificationAccumulator("Demo");
        accumulator.SetExpectedTaskCount(1);
        accumulator.Add(new TaskStatusDto { TaskId = "t1", State = TaskState.Running });
        accumulator.Add(Completed("t1"));

        Assert.Single(accumulator.Tasks);
        Assert.Equal(TaskState.Succeeded, accumulator.Tasks.Single().State);
        Assert.True(accumulator.IsComplete);
    }

    [Fact]
    public void MissingExpectedCountNeverCompletes()
    {
        var accumulator = new SessionNotificationAccumulator("Demo");
        accumulator.Add(Completed("t1"));

        Assert.False(accumulator.HasExpectedTaskCount);
        Assert.False(accumulator.IsComplete);
    }

    private static TaskStatusDto Completed(string taskId) => new()
    {
        TaskId = taskId,
        TaskType = "Backup",
        State = TaskState.Succeeded,
        ProgressPercent = 100
    };
}
