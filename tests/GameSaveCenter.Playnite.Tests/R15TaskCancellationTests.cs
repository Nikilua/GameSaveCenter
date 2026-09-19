using System;
using System.IO;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R15TaskCancellationTests
{
    [Fact]
    public void CancellationDisplaySeparatesCancelablePendingFinalizingAndTerminalStates()
    {
        var cancelable = Task(TaskState.Running, string.Empty);
        var requested = Task(TaskState.Running, TaskCancellationStates.Requested);
        var finalizing = Task(TaskState.Running, TaskCancellationStates.Finalizing);
        var cancelled = Task(TaskState.Cancelled, TaskCancellationStates.Cancelled);
        var notInterruptible = Task(TaskState.Failed, TaskCancellationStates.NotInterruptible);

        Assert.True(cancelable.CanCancel);
        Assert.Equal("可取消", cancelable.CancellationDisplay);
        Assert.False(requested.CanCancel);
        Assert.True(requested.IsCancellationPending);
        Assert.Equal("正在取消", requested.CancellationDisplay);
        Assert.Equal("无法立即中断 · 正在安全收尾", finalizing.CancellationDisplay);
        Assert.Equal("已取消", cancelled.CancellationDisplay);
        Assert.Equal("无法中断 · 任务已结束", notInterruptible.CancellationDisplay);
    }

    [Fact]
    public void TaskCenterAndWorkerUseDurableCancellationStateAndIdempotentRequestPath()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var taskView = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));
        var coordinator = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Services", "TaskCoordinator.cs"));
        var dispatcher = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Ipc", "IpcRequestDispatcher.cs"));
        var store = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Persistence", "SqliteStateStore.cs"));
        var queries = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Persistence", "SqliteStateStore.TaskQueries.cs"));
        var comparer = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Infrastructure", "SnapshotComparers.cs"));
        var clipboard = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Infrastructure", "DataGridClipboardBehavior.cs"));

        Assert.Contains("TaskCancellationStateCard", taskView, StringComparison.Ordinal);
        Assert.Contains("SelectedTask.CancellationDisplay", taskView, StringComparison.Ordinal);
        Assert.Contains("SelectedTask.IsCancellationPending", taskView, StringComparison.Ordinal);
        Assert.Contains("public async Task<bool> CancelAsync", coordinator, StringComparison.Ordinal);
        Assert.Contains("TaskCancellationStates.Requested", coordinator, StringComparison.Ordinal);
        Assert.Contains("TaskCancellationStates.Finalizing", coordinator, StringComparison.Ordinal);
        Assert.Contains("CancelAsync(Read<CancelTaskRequestDto>", dispatcher, StringComparison.Ordinal);
        Assert.Contains("cancellation_state", store, StringComparison.Ordinal);
        Assert.Contains("cancellation_state", queries, StringComparison.Ordinal);
        Assert.Contains("CancellationState", comparer, StringComparison.Ordinal);
        Assert.Contains("\"阶段\" => task.StageDisplay", clipboard, StringComparison.Ordinal);
    }

    private static TaskStatusDto Task(TaskState state, string cancellationState)
        => new TaskStatusDto
        {
            TaskId = Guid.NewGuid().ToString("N"),
            TaskType = "Backup",
            GameName = "Synthetic Game",
            State = state,
            CancellationState = cancellationState,
            ProgressPercent = state == TaskState.Succeeded ? 100 : 10,
            CreatedUtc = new DateTime(2026, 9, 20, 0, 0, 0, DateTimeKind.Utc)
        };
}
