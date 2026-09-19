using System;
using System.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R12RestoreConflictExplanationBehaviorTests
{
    [Fact]
    public void RunningGameFailureExplainsHowToCloseTargetWithoutBypassingSafety()
    {
        var steps = RestoreWorkflowProgress.Build(
            Version("running", RestoreReadinessStatus.Ready),
            false,
            false,
            false,
            Task(TaskState.Failed, "RESTORE_GAME_RUNNING", "检测到游戏仍在运行"),
            string.Empty,
            string.Empty);

        var target = Step(steps, "target");
        Assert.Contains("退出游戏", target.ResolutionDisplay);
        Assert.Contains("安全检查", target.ResolutionDisplay);
        Assert.DoesNotContain("关闭安全机制", target.ResolutionDisplay);
    }

    [Fact]
    public void OperationLockFailureRequiresTaskCenterCompletionAndNoConcurrentRetry()
    {
        var steps = RestoreWorkflowProgress.Build(
            Version("busy", RestoreReadinessStatus.Ready),
            false,
            false,
            false,
            null,
            string.Empty,
            "已有备份、恢复或媒体操作正在执行，请稍后重试。");

        var execution = Step(steps, "execution");
        Assert.Equal(RestoreWorkflowStepStatus.Failed, execution.Status);
        Assert.Contains("任务中心", execution.ResolutionDisplay);
        Assert.Contains("不要并发重试", execution.ResolutionDisplay);
    }

    [Fact]
    public void DiskSpaceReadinessFailureProvidesCleanupAndRevalidationSteps()
    {
        var backup = Version("disk", RestoreReadinessStatus.Failed);
        backup.RestoreReadiness!.Summary = "恢复校验隔离区所在磁盘空间不足：需要约 2 GiB，当前可用 512 MiB。";

        var readiness = Step(RestoreWorkflowProgress.Build(backup, false, false, false, null, string.Empty, string.Empty), "readiness");
        Assert.Contains("清理或迁移", readiness.ResolutionDisplay);
        Assert.Contains("重新执行可恢复性检查", readiness.ResolutionDisplay);
        Assert.Contains("不要关闭空间检查", readiness.ResolutionDisplay);
    }

    [Fact]
    public void PermissionEvidenceIsSpecificButGenericWriteFailureRemainsUnknown()
    {
        var permission = RestoreWorkflowProgress.Build(
            Version("permission", RestoreReadinessStatus.Ready),
            false,
            false,
            false,
            Task(TaskState.Failed, "RESTORE_WRITE_FAILED", "目标目录拒绝访问 permission denied"),
            string.Empty,
            string.Empty);
        Assert.Contains("运行账户", Step(permission, "execution").ResolutionDisplay);

        var generic = RestoreWorkflowProgress.Build(
            Version("unknown", RestoreReadinessStatus.Ready),
            false,
            false,
            false,
            Task(TaskState.Failed, "RESTORE_WRITE_FAILED", "写入当前存档失败，已回滚"),
            string.Empty,
            string.Empty);
        Assert.Contains("没有明确依据时不要关闭安全机制", Step(generic, "execution").ResolutionDisplay);
    }

    private static RestoreWorkflowStepState Step(
        System.Collections.Generic.IReadOnlyList<RestoreWorkflowStepState> steps,
        string key)
        => steps.Single(step => string.Equals(step.Key, key, StringComparison.Ordinal));

    private static BackupVersionDto Version(string id, RestoreReadinessStatus status)
        => new BackupVersionDto
        {
            BackupId = id,
            CreatedUtc = new DateTime(2026, 9, 19, 1, 0, 0, DateTimeKind.Utc),
            RestoreReadiness = new RestoreReadinessDto
            {
                Status = status,
                Summary = status == RestoreReadinessStatus.Ready ? "隔离校验通过" : "检查失败",
                ExpectedFileCount = 4,
                ActualFileCount = 4,
                ExpectedTotalSize = 4096,
                ActualTotalSize = 4096
            }
        };

    private static TaskStatusDto Task(TaskState state, string errorCode, string message)
        => new TaskStatusDto
        {
            TaskType = "Restore",
            State = state,
            ErrorCode = errorCode,
            ErrorMessage = state == TaskState.Failed ? message : string.Empty,
            Message = message
        };
}
