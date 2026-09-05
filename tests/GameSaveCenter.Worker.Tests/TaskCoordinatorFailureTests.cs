using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Ipc;
using GameSaveCenter.Worker.Services;
using Microsoft.Extensions.Logging;
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
