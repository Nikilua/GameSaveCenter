using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R22ConfirmationStructureBehaviorTests
{
    [Fact]
    public void DangerousConfirmationMakesCancelSafeDefaultAndRequiresExplicitConfirmFocus()
    {
        RunSta(() =>
        {
            var confirm = new Button();
            var cancel = new Button();

            DialogConfirmationPolicy.ApplyConfirmationButtons(confirm, cancel, isDangerous: true);

            Assert.False(confirm.IsDefault);
            Assert.False(confirm.IsCancel);
            Assert.False(cancel.IsDefault);
            Assert.True(cancel.IsCancel);
        });
    }

    [Fact]
    public void OrdinaryConfirmationRetainsEnterDefaultButChoiceAndResultPoliciesAreExplicit()
    {
        RunSta(() =>
        {
            var confirm = new Button();
            var cancel = new Button();

            DialogConfirmationPolicy.ApplyConfirmationButtons(confirm, cancel, isDangerous: false);
            Assert.True(confirm.IsDefault);
            Assert.True(cancel.IsCancel);

            DialogConfirmationPolicy.ApplyChoiceButtons(confirm, cancel);
            Assert.True(confirm.IsDefault);
            Assert.True(cancel.IsCancel);

            DialogConfirmationPolicy.ApplyResultButton(confirm, cancel);
            Assert.True(confirm.IsDefault);
            Assert.False(cancel.IsCancel);
        });
    }

    [Fact]
    public void ConfirmationRequestKeepsExplicitActionLabelsAndSafePendingCompletion()
    {
        var request = new UiConfirmationEventArgs(
            "清理候选版本",
            "对象：合成游戏；范围：3 个未锁定候选；后果：删除 ZIP 与索引。",
            "清理候选版本",
            "取消",
            isDangerous: true);

        Assert.Equal("清理候选版本", request.ConfirmText);
        Assert.Equal("取消", request.CancelText);
        Assert.True(request.IsDangerous);
        Assert.False(request.Completion.Task.IsCompleted);
        Assert.Equal(TaskStatus.WaitingForActivation, request.Completion.Task.Status);
    }

    [Fact]
    public void RestoreFlowsOptOutOfTheEnterDefaultBecauseTheyReplaceCurrentState()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var source = File.ReadAllText(Path.Combine(
            TestRepositoryContext.Root,
            "src",
            "GameSaveCenter.Playnite",
            "ViewModels",
            "DashboardViewModel.cs"));

        var localRestore = source.IndexOf("GameSaveCenter 安全恢复", StringComparison.Ordinal);
        var undoRestore = source.IndexOf("撤销恢复", StringComparison.Ordinal);
        var remoteRestore = source.IndexOf("从已校验的远端备份恢复", StringComparison.Ordinal);

        Assert.True(localRestore >= 0);
        Assert.True(undoRestore >= 0);
        Assert.True(remoteRestore >= 0);
        Assert.Contains("isDangerous: true", source.Substring(localRestore, 700), StringComparison.Ordinal);
        Assert.Contains("isDangerous: true", source.Substring(undoRestore, 500), StringComparison.Ordinal);
        Assert.Contains("isDangerous: true", source.Substring(remoteRestore, 650), StringComparison.Ordinal);
    }

    private static void RunSta(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (exception != null)
            throw new Xunit.Sdk.XunitException(exception.ToString());
    }
}
