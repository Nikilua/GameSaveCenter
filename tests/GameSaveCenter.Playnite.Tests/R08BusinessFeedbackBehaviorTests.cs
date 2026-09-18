using System;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Controls;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R08BusinessFeedbackBehaviorTests
{
    [Fact]
    public void TaskToastAutomationPeerCarriesTheFinalStateText()
    {
        Exception? exception = null;
        string? name = null;
        string? help = null;
        AutomationControlType controlType = AutomationControlType.Custom;

        RunSta(() =>
        {
            try
            {
                var toast = new FeedbackToast();
                DashboardView.ApplyToastAutomation(toast, "后台任务已取消", "赛博朋克 2077 · 存档备份 已取消");
                name = AutomationProperties.GetName(toast);
                help = AutomationProperties.GetHelpText(toast);
                controlType = UIElementAutomationPeer.CreatePeerForElement(toast)!.GetAutomationControlType();
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });

        Assert.Null(exception);
        Assert.Equal("后台任务已取消：赛博朋克 2077 · 存档备份 已取消", name);
        Assert.Contains("最终状态以任务中心记录为准", help);
        Assert.Equal(AutomationControlType.Custom, controlType);
    }

    [Theory]
    [InlineData(TaskState.Succeeded, "成功", "完成")]
    [InlineData(TaskState.Failed, "失败", "失败")]
    [InlineData(TaskState.Cancelled, "已取消", "已取消")]
    public void TerminalTaskDtoKeepsReaderStateAndDetailAligned(TaskState state, string stateDisplay, string expectedDetail)
    {
        var task = new TaskStatusDto
        {
            TaskId = "synthetic-r08-04",
            TaskType = "Backup",
            GameName = "合成游戏",
            State = state,
            Message = state == TaskState.Succeeded ? "完成" : expectedDetail
        };

        Assert.Equal(stateDisplay, task.StateDisplay);
        Assert.Contains(expectedDetail, task.DetailMessage);
        Assert.False(task.CanCancel);
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
