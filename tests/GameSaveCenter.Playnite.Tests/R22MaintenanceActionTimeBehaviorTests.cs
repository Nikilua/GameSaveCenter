using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R22MaintenanceActionTimeBehaviorTests
{
    [Fact]
    public void MaintenanceActionKeepsRelativeBodyAndFullEvidenceForHealthAndQuarantine()
    {
        var health = new MaintenanceActionItem
        {
            ActionKind = MaintenanceActionKind.HealthInspection,
            LastVerifiedDisplay = "3 分钟前",
            LastVerifiedFullDisplay = "2026-09-21 10:00:00 (UTC+08:00) · 2026-09-21T02:00:00.0000000Z",
            LastAttemptDisplay = "刚刚",
            LastAttemptFullDisplay = "2026-09-21 10:03:00 (UTC+08:00) · 2026-09-21T02:03:00.0000000Z",
            NextAttemptDisplay = "明天 10:00",
            NextAttemptFullDisplay = "2026-09-22 10:00:00 (UTC+08:00) · 2026-09-22T02:00:00.0000000Z"
        };
        var quarantine = new MaintenanceActionItem
        {
            ActionKind = MaintenanceActionKind.RetentionQuarantine,
            LedgerUpdatedDisplay = "昨天 10:00",
            LedgerUpdatedFullDisplay = "2026-09-20 10:00:00 (UTC+08:00) · 2026-09-20T02:00:00.0000000Z",
            NextAttemptDisplay = "需人工确认"
        };

        Assert.Equal("最近完成：刚刚 · 最近成功：3 分钟前 · 下轮计划：明天 10:00", health.TimingDisplay);
        Assert.Contains("2026-09-21T02:00:00.0000000Z", health.TimingFullDisplay, StringComparison.Ordinal);
        Assert.Contains("2026-09-22T02:00:00.0000000Z", health.TimingFullDisplay, StringComparison.Ordinal);
        Assert.Equal("账本更新：昨天 10:00 · 下次尝试：需人工确认", quarantine.TimingDisplay);
        Assert.Contains("2026-09-20T02:00:00.0000000Z", quarantine.TimingFullDisplay, StringComparison.Ordinal);
    }

    [Fact]
    public void MaintenanceViewBindsActionTimingBodyAndFullAutomationEvidence()
    {
        Exception? exception = null;
        var item = new MaintenanceActionItem
        {
            ItemId = "health-time-22",
            CategoryDisplay = "恢复巡检",
            Title = "合成恢复可用性巡检",
            StatusDisplay = "最近验证通过",
            Detail = "合成状态",
            ActionText = "查看巡检状态",
            ActionKind = MaintenanceActionKind.HealthInspection,
            LastVerifiedDisplay = "3 分钟前",
            LastVerifiedFullDisplay = "2026-09-21 10:00:00 (UTC+08:00) · 2026-09-21T02:00:00.0000000Z",
            LastAttemptDisplay = "刚刚",
            LastAttemptFullDisplay = "2026-09-21 10:03:00 (UTC+08:00) · 2026-09-21T02:03:00.0000000Z",
            NextAttemptDisplay = "明天 10:00",
            NextAttemptFullDisplay = "2026-09-22 10:00:00 (UTC+08:00) · 2026-09-22T02:00:00.0000000Z"
        };
        var state = new MaintenanceActionContext(item);

        var thread = new System.Threading.Thread(() =>
        {
            try
            {
                var view = new MaintenanceView { DataContext = state };
                var window = Show(view, 1100, 720);
                try
                {
                    var diagnosticsTabs = (TabControl)view.FindName("MaintenanceDiagnosticsSubTabs")!;
                    diagnosticsTabs.SelectedIndex = 1;
                    view.ApplyResponsiveLayout(1100, 720);
                    PumpLayout(window);

                    var timing = FindVisualDescendants<TextBlock>(view)
                        .Single(textBlock => textBlock.Text == item.TimingDisplay);

                    Assert.Equal(item.TimingFullDisplay, timing.ToolTip);
                    Assert.Equal(item.TimingFullDisplay, System.Windows.Automation.AutomationProperties.GetHelpText(timing));
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

    private sealed class MaintenanceActionContext
    {
        public MaintenanceActionContext(MaintenanceActionItem item)
        {
            MaintenanceActionSections = new[]
            {
                new MaintenanceActionSection(
                    MaintenanceActionGroup.Routine,
                    "例行巡检",
                    "合成状态",
                    new[] { item })
            };
        }

        public int MaintenanceTabIndex { get; set; } = 0;
        public IReadOnlyList<MaintenanceActionSection> MaintenanceActionSections { get; }
        public ICommand RunMaintenanceActionCommand { get; } = new TestCommand();
    }

    private sealed class TestCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) { }
    }
}
