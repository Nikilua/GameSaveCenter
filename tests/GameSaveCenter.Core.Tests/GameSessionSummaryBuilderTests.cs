using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Services;
using Xunit;

namespace GameSaveCenter.Core.Tests;

public sealed class GameSessionSummaryBuilderTests
{
    [Fact]
    public void CloudFailureStillReportsLocalBackupAndRetryAction()
    {
        var summary = GameSessionSummaryBuilder.Build("Demo", new[]
        {
            new TaskStatusDto { TaskType = "Backup", State = TaskState.Failed, ErrorCode = "RCLONE_COPY_FAILED" },
            new TaskStatusDto { TaskType = "MediaSync", State = TaskState.Succeeded }
        });

        Assert.True(summary.IsWarning);
        Assert.False(summary.IsFailure);
        Assert.Contains("本地备份完成", summary.Message);
        Assert.Contains("云端同步失败", summary.Message);
    }

    [Fact]
    public void SessionSummaryUsesTerminalTaskStatesAndOneMessage()
    {
        var summary = GameSessionSummaryBuilder.Build("Demo", new[]
        {
            new TaskStatusDto { TaskType = "Backup", State = TaskState.Succeeded },
            new TaskStatusDto { TaskType = "MediaSync", State = TaskState.Failed, ErrorCode = "MEDIA_FAILED" }
        });

        Assert.True(summary.IsFailure);
        Assert.Contains("本地备份完成", summary.Message);
        Assert.Contains("媒体同步失败", summary.Message);
    }
}
