using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Ipc;
using GameSaveCenter.Worker.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class TaskCoordinatorFailureTests
{
    [Fact]
    public async Task TerminalPersistenceFailureAfterSuccessDoesNotStrandGameLockOrPublishSuccess()
    {
        await AssertTerminalPersistenceFailureDoesNotLeakAsync(
            (_, _) => Task.CompletedTask,
            TaskState.Succeeded,
            "Success Game");
    }

    [Fact]
    public async Task TerminalPersistenceFailureAfterBusinessFailureDoesNotStrandGameLock()
    {
        await AssertTerminalPersistenceFailureDoesNotLeakAsync(
            (_, _) => Task.FromException(new WorkerOperationException("INJECTED", "business failure")),
            TaskState.Failed,
            "Failed Game");
    }

    [Fact]
    public async Task TerminalPersistenceFailureAfterCancellationDoesNotStrandGameLock()
    {
        await AssertTerminalPersistenceFailureDoesNotLeakAsync(
            (_, token) => Task.FromException(new OperationCanceledException(token)),
            TaskState.Cancelled,
            "Cancelled Game");
    }

    [Fact]
    public async Task CloudFailureKeepsLocalBackupResultOnTheFailedTask()
    {
        var store = new RecordingTaskStatusStore();
        var broadcaster = new TaskEventBroadcaster();
        using var subscription = broadcaster.Subscribe();
        var coordinator = new TaskCoordinator(store, broadcaster, NullLogger<TaskCoordinator>.Instance);

        var result = await coordinator.RunAsync(
            "Backup",
            "game-under-test",
            "Synthetic Game",
            (progress, _) =>
            {
                progress.SetBackupResult(new BackupResultDto
                {
                    LocalState = "Succeeded",
                    CloudState = "RetryScheduled",
                    Summary = "本地备份已成功；云端上传已排队等待重试。"
                });
                return Task.FromException(new WorkerOperationException("RCLONE_NETWORK_FAILED", "网络不可用"));
            },
            CancellationToken.None);

        Assert.Equal(TaskState.Failed, result.State);
        Assert.True(result.HasPartialSuccess);
        Assert.Equal("Succeeded", result.BackupResult?.LocalState);
        Assert.Equal("RetryScheduled", result.BackupResult?.CloudState);
        Assert.Contains("本地备份已成功", result.BackupResult?.Summary ?? string.Empty);
        Assert.Same(result, store.TerminalTask);

        var events = new List<TaskChangeEventDto>();
        while (subscription.Reader.TryRead(out var change)) events.Add(change);
        var terminalEvent = events.Last(change => change.Task.TaskId == result.TaskId && change.Task.State == TaskState.Failed);
        Assert.Equal("RetryScheduled", terminalEvent.Task.BackupResult?.CloudState);
        Assert.True(terminalEvent.Task.HasPartialSuccess);
    }

    [Fact]
    public async Task TerminalFailureKeepsTheLastReportedStageSeparateFromErrorMessage()
    {
        var store = new RecordingTaskStatusStore();
        var coordinator = new TaskCoordinator(store, new TaskEventBroadcaster(), NullLogger<TaskCoordinator>.Instance);

        var result = await coordinator.RunAsync(
            "Backup",
            "game-under-test",
            "Synthetic Game",
            async (progress, _) =>
            {
                await progress.ReportAsync(10, "正在扫描存档");
                throw new WorkerOperationException("LUDUSAVI_FAILED", "合成失败");
            },
            CancellationToken.None);

        Assert.Equal(TaskState.Failed, result.State);
        Assert.Equal("执行失败", result.Message);
        Assert.Equal("正在扫描存档", result.StageMessage);
        Assert.Equal("扫描中", result.StageDisplay);
        Assert.Contains("合成失败", result.DetailMessage, StringComparison.Ordinal);
        Assert.Same(result, store.TerminalTask);
    }

    private static async Task AssertTerminalPersistenceFailureDoesNotLeakAsync(
        Func<TaskProgress, CancellationToken, Task> operation,
        TaskState expectedState,
        string gameName)
    {
        var store = new FailingTaskStatusStore();
        var broadcaster = new TaskEventBroadcaster();
        var logger = new CapturingLogger<TaskCoordinator>();
        var coordinator = new TaskCoordinator(store, broadcaster, logger);
        using var events = broadcaster.Subscribe();

        var first = await coordinator.RunAsync(
            "InjectedTask",
            "game-under-test",
            gameName,
            operation,
            CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(2));

        Assert.Equal(expectedState, first.State);
        Assert.False(coordinator.Cancel(first.TaskId));
        Assert.Contains(logger.Messages, message =>
            message.Contains(first.TaskId, StringComparison.Ordinal)
            && message.Contains("game-under-test", StringComparison.Ordinal)
            && message.Contains(expectedState.ToString(), StringComparison.Ordinal));

        var published = new List<TaskChangeEventDto>();
        while (events.Reader.TryRead(out var change)) published.Add(change);
        Assert.DoesNotContain(published, change =>
            change.Task.TaskId == first.TaskId && change.Task.State == expectedState);

        // The same game must be able to run again even though its previous terminal
        // update could not be written.  A separate game must remain independent too.
        var sameGame = coordinator.RunAsync(
            "InjectedTask",
            "game-under-test",
            gameName,
            (_, _) => Task.CompletedTask,
            CancellationToken.None);
        var otherGame = coordinator.RunAsync(
            "InjectedTask",
            "other-game",
            "Other Game",
            (_, _) => Task.CompletedTask,
            CancellationToken.None);

        var followUp = await Task.WhenAll(sameGame, otherGame).WaitAsync(TimeSpan.FromSeconds(2));
        Assert.All(followUp, task => Assert.Equal(TaskState.Succeeded, task.State));
        Assert.False(coordinator.Cancel(followUp[0].TaskId));
        Assert.False(coordinator.Cancel(followUp[1].TaskId));
    }

    private sealed class FailingTaskStatusStore : ITaskStatusStore
    {
        private int terminalFailures = 1;

        public Task AddOrUpdateTaskAsync(TaskStatusDto task, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if ((task.State is TaskState.Succeeded or TaskState.Failed or TaskState.Cancelled)
                && Interlocked.Exchange(ref terminalFailures, 0) == 1)
            {
                throw new InvalidOperationException("injected terminal persistence failure");
            }

            return Task.CompletedTask;
        }
    }

    private sealed class RecordingTaskStatusStore : ITaskStatusStore
    {
        public TaskStatusDto? TerminalTask { get; private set; }

        public Task AddOrUpdateTaskAsync(TaskStatusDto task, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (task.State is TaskState.Succeeded or TaskState.Failed or TaskState.Cancelled)
                TerminalTask = task;
            return Task.CompletedTask;
        }
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = new();

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            lock (Messages) Messages.Add(formatter(state, exception));
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}
