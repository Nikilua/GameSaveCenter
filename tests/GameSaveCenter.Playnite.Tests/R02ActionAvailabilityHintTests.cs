using System;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R02ActionAvailabilityHintTests
{
    [Fact]
    public void RestoreHintFollowsBlockingPrerequisitesWithoutEnablingDangerousAction()
    {
        Assert.Equal(
            "请先在页面顶部的游戏选择器选中一个游戏；选择后才能读取历史版本。",
            ViewModels.ActionAvailabilityHints.Restore(false, false, true, false));
        Assert.Equal(
            "请先在历史版本列表选中一个版本；选中后可先验证可恢复性。",
            ViewModels.ActionAvailabilityHints.Restore(true, false, true, false));
        Assert.Contains("Ludusavi", ViewModels.ActionAvailabilityHints.Restore(true, true, false, false));
        Assert.Contains("危险恢复命令保持禁用", ViewModels.ActionAvailabilityHints.Restore(true, true, false, false));
        Assert.Contains("创建并锁定当前快照", ViewModels.ActionAvailabilityHints.Restore(true, true, true, false));
        Assert.Contains("其他操作", ViewModels.ActionAvailabilityHints.Restore(true, true, true, true));
        Assert.True(ViewModels.ActionAvailabilityHints.RestoreNeedsMaintenance(true, true, false, false));
        Assert.False(ViewModels.ActionAvailabilityHints.RestoreNeedsMaintenance(true, true, true, false));
    }

    [Fact]
    public void MediaAndCloudHintsCoverSelectionReadinessAndOperationalFailures()
    {
        Assert.Contains("后台服务当前离线", ViewModels.ActionAvailabilityHints.MediaInbox(false, "待归类", false, false, false));
        Assert.DoesNotContain("Worker 当前离线", ViewModels.ActionAvailabilityHints.MediaInbox(false, "待归类", false, false, false));
        Assert.Contains("选中媒体", ViewModels.ActionAvailabilityHints.MediaInbox(true, "待归类", false, false, false));
        Assert.Contains("目标游戏", ViewModels.ActionAvailabilityHints.MediaInbox(true, "待归类", true, false, false));
        Assert.Contains("来源文件和原始副本仍会保留", ViewModels.ActionAvailabilityHints.MediaInbox(true, "待归类", true, true, false));
        Assert.Contains("恢复到待归类", ViewModels.ActionAvailabilityHints.MediaInbox(true, "已忽略", true, false, false));
        Assert.True(ViewModels.ActionAvailabilityHints.MediaInboxNeedsMaintenance(false, false));
        Assert.False(ViewModels.ActionAvailabilityHints.MediaInboxNeedsMaintenance(true, false));

        Assert.Contains("云端上传未启用", ViewModels.ActionAvailabilityHints.CloudTransfer(true, false, false, null, false));
        Assert.DoesNotContain("Rclone", ViewModels.ActionAvailabilityHints.CloudTransfer(true, true, false, null, false));
        Assert.Contains("选中一条记录", ViewModels.ActionAvailabilityHints.CloudTransfer(true, true, true, null, false));
        Assert.Contains("尚未远端校验", ViewModels.ActionAvailabilityHints.CloudTransfer(true, true, true, Transfer("Uploaded"), false));
        Assert.Contains("重试上传", ViewModels.ActionAvailabilityHints.CloudTransfer(true, true, true, Transfer("Failed"), false));
        Assert.Contains("认证", ViewModels.ActionAvailabilityHints.CloudTransfer(true, true, true, Transfer("AuthenticationRequired"), false));
        Assert.Contains("其他操作", ViewModels.ActionAvailabilityHints.CloudTransfer(true, true, true, Transfer("Uploaded"), true));
        Assert.True(ViewModels.ActionAvailabilityHints.CloudTransferNeedsMaintenance(true, false, true, null, false));
        Assert.False(ViewModels.ActionAvailabilityHints.CloudTransferNeedsMaintenance(true, true, true, Transfer("Uploaded"), false));
    }

    [Fact]
    public void RemoteRestoreHintKeepsRestoreClosedUntilIsolatedVerification()
    {
        Assert.Contains("选中一条记录", ViewModels.ActionAvailabilityHints.RemoteRestore(false, false, false, false, false));
        Assert.Contains("没有远端备份标识", ViewModels.ActionAvailabilityHints.RemoteRestore(true, false, false, false, false));
        Assert.Contains("1 · 下载到隔离区并校验", ViewModels.ActionAvailabilityHints.RemoteRestore(true, true, false, false, false));
        Assert.Contains("恢复命令保持禁用", ViewModels.ActionAvailabilityHints.RemoteRestore(true, true, true, false, false));
        Assert.Contains("创建并锁定本机当前快照", ViewModels.ActionAvailabilityHints.RemoteRestore(true, true, true, true, false));
    }

    [Fact]
    public void AvailabilityHintTextIsFocusableAndHasAutomationName()
    {
        Exception? exception = null;
        var focused = false;
        var focusable = false;
        var tabStop = false;
        var hasPeer = false;
        var automationName = string.Empty;

        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var resourceHost = new MediaCenterView();
                var hint = new TextBlock
                {
                    Text = "请先选择目标游戏，归类按钮才会启用。",
                    Style = (Style)resourceHost.Resources["GscActionAvailabilityHintText"]
                };
                AutomationProperties.SetName(hint, hint.Text);
                AutomationProperties.SetHelpText(hint, hint.Text);
                window = CreateWindow(hint);
                window.Show();
                window.UpdateLayout();

                focusable = hint.Focusable;
                tabStop = KeyboardNavigation.GetIsTabStop(hint);
                focused = hint.Focus();
                hasPeer = UIElementAutomationPeer.CreatePeerForElement(hint) != null;
                automationName = AutomationProperties.GetName(hint);
            }
            catch (Exception caught)
            {
                exception = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        Assert.Null(exception);
        Assert.True(focusable);
        Assert.True(tabStop);
        Assert.True(focused);
        Assert.True(hasPeer);
        Assert.Equal("请先选择目标游戏，归类按钮才会启用。", automationName);
    }

    private static CloudTransferStatusDto Transfer(string state)
        => new() { State = state };

    private static Window CreateWindow(UIElement content)
        => new()
        {
            Content = content,
            Width = 420,
            Height = 180,
            ShowInTaskbar = false,
            ShowActivated = false,
            WindowStyle = WindowStyle.None,
            Opacity = 0.01
        };

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
