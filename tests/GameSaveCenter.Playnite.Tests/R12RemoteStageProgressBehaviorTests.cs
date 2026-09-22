using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R12RemoteStageProgressBehaviorTests
{
    [Fact]
    public void DownloadVerificationAndReadyStatesRemainDistinct()
    {
        var downloading=RemoteBackupStageProjection.FromTask(new TaskStatusDto
        {
            TaskType="RemoteStage",State=TaskState.Running,ProgressPercent=24,Message="正在下载到隔离区"
        });
        var verifying=RemoteBackupStageProjection.FromTask(new TaskStatusDto
        {
            TaskType="RemoteStage",State=TaskState.Running,ProgressPercent=70,Message="一致性校验通过，正在确认目标 Ludusavi 版本"
        });
        var ready=RemoteBackupStageProjection.FromTask(new TaskStatusDto
        {
            TaskType="RemoteStage",State=TaskState.Succeeded,ProgressPercent=100
        });

        Assert.True(downloading.IsActive);
        Assert.True(verifying.IsActive);
        Assert.False(ready.IsActive);
        Assert.Equal(24,downloading.Progress);
        Assert.Equal(70,verifying.Progress);
        Assert.Equal(100,ready.Progress);
        Assert.NotEqual(downloading.StatusText,verifying.StatusText);
        Assert.Contains("等待恢复确认",ready.StatusText);
        Assert.DoesNotContain("恢复完成",ready.StatusText);
    }

    [Fact]
    public void CancellationAndFailureAreTerminalAndDoNotAdvertiseRestore()
    {
        var cancelled=RemoteBackupStageProjection.FromTask(new TaskStatusDto
        {
            TaskType="RemoteStage",State=TaskState.Cancelled,ProgressPercent=48
        });
        var failed=RemoteBackupStageProjection.FromTask(new TaskStatusDto
        {
            TaskType="RemoteStage",State=TaskState.Failed,ProgressPercent=60,ErrorMessage="隔离版本不存在"
        });

        Assert.False(cancelled.IsActive);
        Assert.False(failed.IsActive);
        Assert.Contains("隔离区",cancelled.StatusText);
        Assert.Contains("隔离版本不存在",failed.StatusText);
        Assert.DoesNotContain("恢复完成",cancelled.StatusText+failed.StatusText);
    }

    [Fact]
    public void OtherTaskTypesDoNotChangeRemoteStageProjection()
    {
        var projection=RemoteBackupStageProjection.FromTask(new TaskStatusDto
        {
            TaskType="Restore",State=TaskState.Running,ProgressPercent=60,Message="正在恢复"
        });

        Assert.False(projection.IsRelevant);
        Assert.False(projection.IsActive);
        Assert.Equal(0,projection.Progress);
        Assert.Equal(string.Empty,projection.StatusText);
    }
}
