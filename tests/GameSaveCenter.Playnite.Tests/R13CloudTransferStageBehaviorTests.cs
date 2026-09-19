using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R13CloudTransferStageBehaviorTests
{
    [Fact]
    public void MaintenanceQueueBindsStageAndKeepsGuaranteeSeparate()
    {
        var root = TestRepositoryContext.Root;
        var source = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));

        Assert.Contains("QueuePhaseDisplay", source);
        Assert.Contains("GuaranteeLevelDisplay", source);
        Assert.Contains("QueueControlDisplay", source);
    }

    [Fact]
    public void MaintenanceInspectorUsesBoundedRetryTimingDisplay()
    {
        var root = TestRepositoryContext.Root;
        var source = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));

        Assert.Contains("SelectedCloudTransfer.RetryTimingDisplay", source);
        Assert.DoesNotContain("SelectedCloudTransfer.NextAttemptLocal, StringFormat", source);
    }

    [Theory]
    [InlineData("Failed", true)]
    [InlineData("RetryScheduled", true)]
    [InlineData("Transferring", false)]
    [InlineData("Uploaded", false)]
    [InlineData("RemoteVerified", false)]
    public void ManualRetryScopeOnlyAcceptsUnfinishedSelectedTransfers(string state, bool canRetry)
    {
        var transfer = new CloudTransferStatusDto { State = state };

        Assert.Equal(canRetry, transfer.CanManuallyRetry);
        if (canRetry)
        {
            Assert.Contains("仅当前选中项", transfer.ManualRetryScopeDisplay);
            Assert.Contains("不重新执行本地备份", transfer.ManualRetryScopeDisplay);
        }
        else
        {
            Assert.Contains("不会重复提交", transfer.ManualRetryScopeDisplay);
        }
    }

    [Fact]
    public void ManualRetryCommandGateBlocksSecondClickWhileSubmissionIsBusy()
    {
        var transfer = new CloudTransferStatusDto { State = "RetryScheduled" };
        var busy = false;
        var submissions = 0;
        var command = new RelayCommand(
            _ =>
            {
                submissions++;
                busy = true;
            },
            _ => !busy && transfer.CanManuallyRetry);

        Assert.True(command.CanExecute(null));
        command.Execute(null);

        Assert.False(command.CanExecute(null));
        Assert.Equal(1, submissions);
    }

    [Fact]
    public void SettingsExplainPauseRecoveryAndWindowChangeBoundary()
    {
        var root = TestRepositoryContext.Root;
        var source = File.ReadAllText(Path.Combine(
            root,
            "src",
            "GameSaveCenter.Playnite",
            "Settings",
            "GameSaveCenterSettingsView.xaml"));

        Assert.Contains("暂停云端自动重试队列", source);
        Assert.Contains("恢复开关后会继续处理已保存队列", source);
        Assert.Contains("下一轮 Worker 检查时生效", source);
        Assert.Contains("已开始的上传不会被取消", source);
    }
}
