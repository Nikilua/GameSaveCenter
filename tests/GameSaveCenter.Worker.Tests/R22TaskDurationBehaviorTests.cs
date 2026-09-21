using System;
using System.Threading;
using System.Threading.Tasks;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Ipc;
using GameSaveCenter.Worker.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class R22TaskDurationBehaviorTests
{
    [Fact]
    public void MonotonicClockUsesCounterDeltaInsteadOfWallTime()
    {
        Assert.Equal(2.5, MonotonicTaskClock.SecondsBetween(100, 350, 100), 3);
        Assert.Equal(0, MonotonicTaskClock.SecondsBetween(350, 100, 100));
        Assert.Equal(0, MonotonicTaskClock.SecondsBetween(100, 350, 0));
    }

    [Fact]
    public async Task TaskCoordinatorPublishesMonotonicDurationForTerminalTask()
    {
        var store = new RecordingTaskStatusStore();
        var coordinator = new TaskCoordinator(store, new TaskEventBroadcaster(), NullLogger<TaskCoordinator>.Instance);
        var result = await coordinator.RunAsync(
            "Backup",
            "duration-game",
            "Synthetic Game",
            async (_, token) => await Task.Delay(120, token),
            CancellationToken.None,
            createdUtc: DateTime.UtcNow.AddDays(-30));

        Assert.Equal(TaskState.Succeeded, result.State);
        Assert.True(result.ElapsedSeconds.HasValue);
        Assert.InRange(result.ElapsedSeconds!.Value, 0, 10);
        Assert.DoesNotContain("小时", result.DurationDisplay, StringComparison.Ordinal);
        Assert.Equal(result.ElapsedSeconds, store.TerminalSnapshot?.ElapsedSeconds);
        Assert.Equal(0, result.MonotonicStartedTimestamp);
        Assert.Equal(0, result.MonotonicFrequency);
    }

    private sealed class RecordingTaskStatusStore : ITaskStatusStore
    {
        public TaskStatusDto? TerminalSnapshot { get; private set; }

        public Task AddOrUpdateTaskAsync(TaskStatusDto task, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (task.State is TaskState.Succeeded or TaskState.Failed or TaskState.Cancelled)
            {
                TerminalSnapshot = new TaskStatusDto
                {
                    State = task.State,
                    ElapsedSeconds = task.ElapsedSeconds,
                    MonotonicStartedTimestamp = task.MonotonicStartedTimestamp,
                    MonotonicFrequency = task.MonotonicFrequency
                };
            }
            return Task.CompletedTask;
        }
    }
}
