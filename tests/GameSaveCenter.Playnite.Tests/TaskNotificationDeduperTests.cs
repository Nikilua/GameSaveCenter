using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class TaskNotificationDeduperTests
{
    [Fact]
    public void ProgressUpdatesDoNotClaimScreenNotifications()
    {
        var deduper = new TaskNotificationDeduper();

        Assert.False(deduper.TryClaim(new TaskStatusDto
        {
            TaskId = "task-progress",
            State = TaskState.Running,
            ProgressPercent = 12,
            Message = "正在扫描"
        }));
        Assert.False(deduper.TryClaim(new TaskStatusDto
        {
            TaskId = "task-progress",
            State = TaskState.Running,
            ProgressPercent = 88,
            Message = "正在校验"
        }));
    }

    [Fact]
    public void TerminalSnapshotWithoutTaskIdDoesNotClaimAReplayKey()
    {
        var deduper = new TaskNotificationDeduper();

        Assert.False(deduper.TryClaim(new TaskStatusDto { State = TaskState.Failed, ErrorCode = "UNKNOWN" }));
    }

    [Fact]
    public void SameFailureEvidenceIsClaimedOnce()
    {
        var deduper = new TaskNotificationDeduper();
        var first = Failed("task-failure", "LUDUSAVI_NOT_CONFIGURED", "工具未配置");

        Assert.True(deduper.TryClaim(first));
        Assert.False(deduper.TryClaim(Failed("task-failure", "LUDUSAVI_NOT_CONFIGURED", "工具未配置")));
    }

    [Fact]
    public void DistinctFailureEvidenceRemainsVisibleForTheSameTask()
    {
        var deduper = new TaskNotificationDeduper();

        Assert.True(deduper.TryClaim(Failed("task-failure", "RCLONE_AUTH_FAILED", "认证失败")));
        Assert.True(deduper.TryClaim(Failed("task-failure", "RCLONE_NO_SPACE", "远端空间不足")));
    }

    [Fact]
    public void SuccessAndCancellationAreEachClaimedOnceWithoutUsingProgressText()
    {
        var deduper = new TaskNotificationDeduper();

        Assert.True(deduper.TryClaim(new TaskStatusDto { TaskId = "task-terminal", State = TaskState.Succeeded, Message = "完成" }));
        Assert.False(deduper.TryClaim(new TaskStatusDto { TaskId = "task-terminal", State = TaskState.Succeeded, Message = "完成" }));
        Assert.True(deduper.TryClaim(new TaskStatusDto { TaskId = "task-terminal", State = TaskState.Cancelled, Message = "已取消" }));
    }

    private static TaskStatusDto Failed(string taskId, string code, string message) => new()
    {
        TaskId = taskId,
        State = TaskState.Failed,
        ErrorCode = code,
        ErrorMessage = message
    };
}
