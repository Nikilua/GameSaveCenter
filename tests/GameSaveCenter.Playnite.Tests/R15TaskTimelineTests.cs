using System;
using System.IO;
using System.Linq;
using GameSaveCenter.Contracts;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R15TaskTimelineTests
{
    [Fact]
    public void TimelineUsesObservedUtcOrderAndDoesNotInventRetryOrMissingMiddleEvents()
    {
        var created = new DateTime(2026, 9, 20, 1, 0, 0, DateTimeKind.Utc);
        var started = created.AddSeconds(2);
        var scanned = started.AddSeconds(3);
        var cancelled = scanned.AddSeconds(2);
        var finished = cancelled.AddSeconds(1);
        var task = new TaskStatusDto
        {
            TaskId = "timeline-task",
            TaskType = "Backup",
            GameName = "Synthetic Game",
            State = TaskState.Cancelled,
            Message = "已取消",
            StageMessage = "正在扫描存档",
            CancellationState = TaskCancellationStates.Cancelled,
            CreatedUtc = created,
            StartedUtc = started,
            FinishedUtc = finished
        };

        var timeline = TaskTimelineBuilder.Build(task, new[]
        {
            Change(1, created, task, TaskState.Queued, "等待执行", TaskCancellationStates.None),
            Change(2, started, task, TaskState.Running, "正在执行", TaskCancellationStates.None),
            Change(3, scanned, task, TaskState.Running, "正在扫描存档", TaskCancellationStates.None),
            Change(4, cancelled, task, TaskState.Running, "正在安全收尾", TaskCancellationStates.Finalizing),
            Change(5, finished, task, TaskState.Cancelled, "正在安全收尾", TaskCancellationStates.Cancelled)
        });

        Assert.Equal(new[] { "已创建", "排队等待", "开始执行", "阶段未知", "扫描中", "阶段未知", "安全收尾", "已取消" }, timeline.Select(x => x.Title).ToArray());
        Assert.Equal("01:00:00", timeline[0].OccurredUtc?.ToString("HH:mm:ss"));
        Assert.Contains("UTC", timeline[1].TimeDisplay);
        Assert.DoesNotContain(timeline, x => x.Title.IndexOf("重试", StringComparison.Ordinal) >= 0);
        Assert.DoesNotContain(timeline, x => x.Detail.IndexOf("重试", StringComparison.Ordinal) >= 0);
    }

    [Fact]
    public void TimelineKeepsUnknownTimeExplicitWhenLegacyEventHasNoTimestamp()
    {
        var task = new TaskStatusDto
        {
            TaskId = "legacy-timeline",
            TaskType = "Restore",
            State = TaskState.Running,
            StageMessage = "正在校验恢复结果",
            CreatedUtc = new DateTime(2026, 9, 20, 2, 0, 0, DateTimeKind.Utc),
            StartedUtc = null
        };

        var timeline = TaskTimelineBuilder.Build(task, new[]
        {
            Change(7, DateTime.MinValue, task, TaskState.Running, "正在校验恢复结果", TaskCancellationStates.None)
        });

        var started = timeline.Single(x => x.Kind == "Started");
        var stage = timeline.Single(x => x.Kind == "Stage");
        Assert.False(started.HasKnownTime);
        Assert.False(stage.HasKnownTime);
        Assert.Equal("时间未知 · 未记录 UTC 时间", stage.TimeDisplay);
    }

    [Fact]
    public void TimelineSortsByObservedUtcThenSequenceAndIgnoresOtherTasks()
    {
        var created = new DateTime(2026, 9, 20, 3, 0, 0, DateTimeKind.Utc);
        var task = new TaskStatusDto
        {
            TaskId = "stable-order-task",
            TaskType = "Backup",
            State = TaskState.Running,
            CreatedUtc = created,
            StartedUtc = created.AddSeconds(1)
        };

        var timeline = TaskTimelineBuilder.Build(task, new[]
        {
            Change(20, created.AddSeconds(10), task, TaskState.Running, "晚到阶段", TaskCancellationStates.None),
            Change(10, created.AddSeconds(2), new TaskStatusDto { TaskId = "other-task" }, TaskState.Running, "不应出现", TaskCancellationStates.None),
            Change(11, created.AddSeconds(2), task, TaskState.Running, "早到阶段", TaskCancellationStates.None),
            Change(12, created.AddSeconds(2), task, TaskState.Running, "同刻第二阶段", TaskCancellationStates.None)
        });

        Assert.Equal(new[] { "早到阶段", "同刻第二阶段", "晚到阶段" }, timeline
            .Where(entry => entry.Kind == "Stage")
            .Select(entry => entry.Detail)
            .ToArray());
        Assert.DoesNotContain(timeline, entry => entry.Detail.IndexOf("不应出现", StringComparison.Ordinal) >= 0);
        Assert.All(timeline.Where(entry => entry.Kind == "Stage"), entry => Assert.Contains("UTC", entry.TimeDisplay));
    }

    [Fact]
    public void TaskCenterUsesBoundedTimelineAndWorkerPublishesEventTimeAndStageFields()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));
        var overview = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml"));
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var coordinator = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Services", "TaskCoordinator.cs"));
        var broadcaster = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Ipc", "TaskEventBroadcaster.cs"));

        Assert.Contains("TaskTimelineCard", view, StringComparison.Ordinal);
        Assert.Contains("SelectedTaskTimeline", view, StringComparison.Ordinal);
        Assert.Contains("MaxHeight=\"220\"", view, StringComparison.Ordinal);
        Assert.Contains("RememberTaskTimelineChange", viewModel, StringComparison.Ordinal);
        Assert.Contains("TaskTimelineBuilder.Build", viewModel, StringComparison.Ordinal);
        Assert.Contains("CreatedRelativeDisplay", view, StringComparison.Ordinal);
        Assert.Contains("CreatedFullDisplay", view, StringComparison.Ordinal);
        Assert.Contains("CreatedRelativeDisplay", overview, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.HelpText=\"{Binding CreatedFullDisplay, Mode=OneWay}\"", overview, StringComparison.Ordinal);
        Assert.Contains("OccurredUtc = DateTime.UtcNow", coordinator, StringComparison.Ordinal);
        Assert.Contains("StageMessage = change.Task.StageMessage", broadcaster, StringComparison.Ordinal);
        Assert.Contains("CancellationState = change.Task.CancellationState", broadcaster, StringComparison.Ordinal);
    }

    private static TaskChangeEventDto Change(long sequence, DateTime occurredUtc, TaskStatusDto task, TaskState state, string stage, string cancellation)
        => new TaskChangeEventDto
        {
            Sequence = sequence,
            OccurredUtc = occurredUtc,
            Task = new TaskStatusDto
            {
                TaskId = task.TaskId,
                TaskType = task.TaskType,
                State = state,
                Message = stage,
                StageMessage = stage,
                CancellationState = cancellation,
                CreatedUtc = task.CreatedUtc,
                StartedUtc = state == TaskState.Queued ? null : task.StartedUtc,
                FinishedUtc = state is TaskState.Cancelled or TaskState.Failed or TaskState.Succeeded ? task.FinishedUtc : null
            }
        };
}
