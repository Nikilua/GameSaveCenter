using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R22RemoteStageTimeBehaviorTests
{
    [Fact]
    public void RemoteStageResultKeepsRelativeFullAndRawExpiryEvidence()
    {
        var result = new RemoteBackupStageResultDto
        {
            StagedUtc = new DateTime(2026, 9, 21, 2, 30, 0, DateTimeKind.Utc),
            ExpiresUtc = new DateTime(2026, 9, 22, 2, 30, 0, DateTimeKind.Utc)
        };

        Assert.NotEqual("时间未知", result.StagedRelativeDisplay);
        Assert.Equal(TimeDisplayFormatter.Full(result.StagedUtc), result.StagedFullDisplay);
        Assert.Equal(TimeDisplayFormatter.RawUtc(result.StagedUtc), result.StagedRawUtcDisplay);
        Assert.NotEqual("时间未知", result.ExpiresRelativeDisplay);
        Assert.Equal(TimeDisplayFormatter.Full(result.ExpiresUtc), result.ExpiresFullDisplay);
        Assert.Equal(TimeDisplayFormatter.RawUtc(result.ExpiresUtc), result.ExpiresRawUtcDisplay);
        Assert.Equal("时间未知", new RemoteBackupStageResultDto().ExpiresFullDisplay);
        Assert.Equal("未记录 UTC 时间", new RemoteBackupStageResultDto().ExpiresRawUtcDisplay);
    }

    [Fact]
    public void RemoteStageStatusUsesRelativeBodyAndFullTooltipContract()
    {
        var result = new RemoteBackupStageResultDto
        {
            GameName = "合成远端游戏",
            RemoteDevice = "设备 B",
            BackupId = "remote-backup-22",
            ExpiresUtc = new DateTime(2026, 9, 22, 2, 30, 0, DateTimeKind.Utc)
        };

        var body = DashboardViewModel.BuildStagedRemoteBackupStatus(result, useFullTime: false);
        var full = DashboardViewModel.BuildStagedRemoteBackupStatus(result, useFullTime: true);

        Assert.Contains("有效期：", body, StringComparison.Ordinal);
        Assert.Contains(result.ExpiresRelativeDisplay, body, StringComparison.Ordinal);
        Assert.Contains("有效期至：", full, StringComparison.Ordinal);
        Assert.Contains(result.ExpiresFullDisplay, full, StringComparison.Ordinal);
        Assert.Equal(
            "尚未下载远端存档。下载只会写入本机隔离区，不会覆盖当前存档。",
            DashboardViewModel.BuildStagedRemoteBackupStatus(null, useFullTime: true));
    }

    [Fact]
    public void MaintenanceDeviceStatusBindsRelativeTextAndFullAutomationEvidence()
    {
        Exception? exception = null;
        var state = new RemoteStageMaintenanceContext();

        var thread = new System.Threading.Thread(() =>
        {
            try
            {
                var view = new MaintenanceView { DataContext = state };
                var window = Show(view, 1100, 720);
                try
                {
                    var tabs = (TabControl)view.FindName("MaintenanceTabControl")!;
                    tabs.SelectedIndex = 2;
                    view.ApplyResponsiveLayout(1100, 720);
                    PumpLayout(window);

                    var status = FindVisualDescendants<TextBlock>(view)
                        .Single(textBlock => textBlock.Text == state.StagedRemoteBackupStatus);

                    Assert.Equal(state.StagedRemoteBackupStatusFullDisplay, status.ToolTip);
                    Assert.Equal(state.StagedRemoteBackupStatusFullDisplay, System.Windows.Automation.AutomationProperties.GetHelpText(status));
                }
                finally
                {
                    window.Close();
                }
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });

        thread.SetApartmentState(System.Threading.ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
    }

    private static Window Show(FrameworkElement content, double width, double height)
    {
        var window = new Window
        {
            Content = content,
            Width = width,
            Height = height,
            ShowInTaskbar = false,
            WindowStyle = WindowStyle.None,
            ResizeMode = ResizeMode.NoResize
        };
        window.Show();
        PumpLayout(window);
        return window;
    }

    private static void PumpLayout(Window window)
    {
        window.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => { }));
        window.UpdateLayout();
    }

    private static List<T> FindVisualDescendants<T>(DependencyObject root)
        where T : DependencyObject
    {
        var matches = new List<T>();
        Visit(root, matches);
        return matches;
    }

    private static void Visit<T>(DependencyObject? current, List<T> matches)
        where T : DependencyObject
    {
        if (current == null) return;
        if (current is T match) matches.Add(match);
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(current); index++)
            Visit(VisualTreeHelper.GetChild(current, index), matches);
    }

    private sealed class RemoteStageMaintenanceContext
    {
        public int MaintenanceTabIndex { get; set; } = 2;
        public string StagedRemoteBackupStatus { get; } = "已校验：合成远端游戏 / 设备 B / remote-backup-22；有效期：明天 10:30。";
        public string StagedRemoteBackupStatusFullDisplay { get; } = "已校验：合成远端游戏 / 设备 B / remote-backup-22；有效期至：2026-09-22 10:30:00 (UTC+08:00) · 2026-09-22T02:30:00.0000000Z。";
        public string RemoteRestoreAvailabilityHint { get; } = "已校验隔离备份，可在确认当前快照和游戏已关闭后恢复。";
        public IReadOnlyList<string> DeviceDecisionOptions { get; } = new[] { "稍后处理", "保留两者" };
        public string DeviceDecision { get; set; } = "稍后处理";
        public string DeviceDecisionComment { get; set; } = string.Empty;
        public ICommand SaveDeviceDecisionCommand { get; } = new TestCommand();
        public ICommand StageRemoteBackupCommand { get; } = new TestCommand();
        public ICommand RestoreStagedRemoteBackupCommand { get; } = new TestCommand();
        public ICommand CancelRemoteBackupStageCommand { get; } = new TestCommand();
        public int RemoteBackupStageProgress => 0;
        public bool IsRemoteBackupStageActive => false;
    }

    private sealed class TestCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) { }
    }
}
