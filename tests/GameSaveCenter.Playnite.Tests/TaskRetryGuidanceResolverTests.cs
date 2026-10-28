using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class TaskRetryGuidanceResolverTests
{
    [Theory]
    [InlineData("RCLONE_PERMISSION_DENIED", "权限不足", "上次远端结果")]
    [InlineData("RCLONE_NETWORK_FAILED", "网络或远端暂不可达", "不要盲目重试")]
    [InlineData("RCLONE_TRANSFER_INCOMPLETE", "传输未完成", "核对远端目标对象")]
    public void KnownCloudFailuresHaveSeparateNextSteps(string errorCode, string category, string nextStep)
    {
        var guidance = TaskRetryGuidanceResolver.Resolve(new TaskStatusDto
        {
            TaskType = "CloudUpload",
            State = TaskState.Failed,
            ErrorCode = errorCode
        }, canRetry: true);

        Assert.Contains(category, guidance);
        Assert.Contains(nextStep, guidance);
    }

    [Fact]
    public void UnknownCloudFailureDoesNotEncourageBlindResubmission()
    {
        var guidance = TaskRetryGuidanceResolver.Resolve(new TaskStatusDto
        {
            TaskType = "CloudUpload",
            State = TaskState.Failed,
            ErrorCode = "RCLONE_PROVIDER_NEW_FAILURE"
        }, canRetry: true);

        Assert.Contains("尚未分类", guidance);
        Assert.Contains("核对远端是否已收到本次提交", guidance);
        Assert.Contains("不要盲目重试", guidance);
    }

    [Fact]
    public void UnsupportedRetryAndNonTerminalStatesAreExplainedWithoutEnablingCommands()
    {
        var failed = new TaskStatusDto { TaskType = "Restore", State = TaskState.Failed };
        var successful = new TaskStatusDto { TaskType = "Backup", State = TaskState.Succeeded };

        Assert.Contains("当前不支持安全重试", TaskRetryGuidanceResolver.Resolve(failed, canRetry: false));
        Assert.Empty(TaskRetryGuidanceResolver.Resolve(successful, canRetry: true));
    }
}
