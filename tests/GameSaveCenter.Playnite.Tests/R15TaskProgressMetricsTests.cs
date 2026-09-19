using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R15TaskProgressMetricsTests
{
    [Fact]
    public void TaskSnapshotComparerTreatsProgressTelemetryAsVisibleState()
    {
        var previous = new TaskStatusDto
        {
            TaskId = "task-1",
            TaskType = "BackupAll",
            State = TaskState.Running,
            ProgressPercent = 20,
            ProgressCompletedUnits = 2,
            ProgressTotalUnits = 10,
            ProgressUnit = "游戏",
            ProgressRatePerSecond = 0.5,
            ProgressUpdatedUtc = DateTime.UtcNow
        };
        var same = new TaskStatusDto
        {
            TaskId = previous.TaskId,
            TaskType = previous.TaskType,
            State = previous.State,
            ProgressPercent = previous.ProgressPercent,
            ProgressCompletedUnits = previous.ProgressCompletedUnits,
            ProgressTotalUnits = previous.ProgressTotalUnits,
            ProgressUnit = previous.ProgressUnit,
            ProgressRatePerSecond = previous.ProgressRatePerSecond,
            ProgressUpdatedUtc = previous.ProgressUpdatedUtc
        };
        var changed = new TaskStatusDto
        {
            TaskId = previous.TaskId,
            TaskType = previous.TaskType,
            State = previous.State,
            ProgressPercent = previous.ProgressPercent,
            ProgressCompletedUnits = previous.ProgressCompletedUnits,
            ProgressTotalUnits = previous.ProgressTotalUnits,
            ProgressUnit = previous.ProgressUnit,
            ProgressRatePerSecond = 1.25,
            ProgressUpdatedUtc = previous.ProgressUpdatedUtc
        };

        Assert.True(SnapshotComparers.Task(previous, same));
        Assert.False(SnapshotComparers.Task(previous, changed));
    }
}
