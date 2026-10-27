using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("R23ProductionResourcesWpf")]
public sealed class CloudTransferSummaryProductionBehaviorTests
{
    private static readonly GameSaveCenterThemeMode[] Themes =
    {
        GameSaveCenterThemeMode.Light,
        GameSaveCenterThemeMode.Dark
    };

    [Fact]
    public void CloudTransferSummaryUsesThemeAccentAndKeepsExplanationsAccessibleWithoutExtraRowsInBothThemes()
    {
        RunSta(() =>
        {
            var application = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            Window? window = null;

            try
            {
                application.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
                var state = new CloudTransferSummaryContext
                {
                    CloudTransferViewSummary = new CloudTransferSummaryDto
                    {
                        PendingCount = 3,
                        VerifyingCount = 2,
                        RetryScheduledCount = 1
                    }
                };
                var page = new MaintenanceView { DataContext = state };
                var tabs = Assert.IsType<TabControl>(page.FindName("MaintenanceTabControl"));
                var cloudTabIndex = tabs.Items.Cast<TabItem>().ToList().FindIndex(item => Equals(item.Header, "云端队列"));
                Assert.True(cloudTabIndex >= 0, "MaintenanceView is missing its Cloud Queue tab.");
                tabs.SelectedIndex = cloudTabIndex;
                window = CreateWindow(page, 1280, 900);
                window.Show();
                FlushLayout(window);

                var card = Assert.IsType<Border>(page.FindName("CloudTransferSummaryCard"));
                var pendingLabel = Assert.IsType<TextBlock>(page.FindName("CloudTransferPendingLabel"));
                var verifyingLabel = Assert.IsType<TextBlock>(page.FindName("CloudTransferVerifyingLabel"));
                var attentionLabel = Assert.IsType<TextBlock>(page.FindName("CloudTransferAttentionLabel"));
                var pendingCount = Assert.IsType<TextBlock>(page.FindName("CloudTransferPendingCount"));
                var verifyingCount = Assert.IsType<TextBlock>(page.FindName("CloudTransferVerifyingCount"));
                var attentionCount = Assert.IsType<TextBlock>(page.FindName("CloudTransferAttentionCount"));

                foreach (var theme in Themes)
                {
                    ApplyTheme(page, theme);
                    FlushLayout(window);

                    Assert.Equal("3", pendingCount.Text);
                    Assert.Equal("2", verifyingCount.Text);
                    Assert.Equal("1", attentionCount.Text);
                    Assert.Equal("待上传与排队中的本地任务", ToolTipService.GetToolTip(pendingLabel));
                    Assert.Equal(ToolTipService.GetToolTip(pendingLabel), AutomationProperties.GetHelpText(pendingLabel));
                    Assert.Equal("只读远端校验正在执行，不会覆盖本地存档。", ToolTipService.GetToolTip(verifyingLabel));
                    Assert.Equal(ToolTipService.GetToolTip(verifyingLabel), AutomationProperties.GetHelpText(verifyingLabel));
                    Assert.Equal("认证、失败或等待重试", ToolTipService.GetToolTip(attentionLabel));
                    Assert.Equal(ToolTipService.GetToolTip(attentionLabel), AutomationProperties.GetHelpText(attentionLabel));

                    Assert.Equal(BrushColor(page.Resources["GscAccentBrush"]), BrushColor(pendingCount.Foreground));
                    Assert.Equal(BrushColor(page.Resources["GscAccentBrush"]), BrushColor(verifyingCount.Foreground));
                    Assert.NotEqual(BrushColor(page.Resources["GscInfoBrush"]), BrushColor(pendingCount.Foreground));
                    Assert.NotEqual(BrushColor(page.Resources["GscInfoBrush"]), BrushColor(verifyingCount.Foreground));
                    Assert.Equal(BrushColor(page.Resources["GscWarningBrush"]), BrushColor(attentionCount.Foreground));

                    Assert.Equal(2, FindVisualAncestor<StackPanel>(pendingCount)!.Children.OfType<TextBlock>().Count());
                    Assert.Equal(2, FindVisualAncestor<StackPanel>(verifyingCount)!.Children.OfType<TextBlock>().Count());
                    Assert.Equal(2, FindVisualAncestor<StackPanel>(attentionCount)!.Children.OfType<TextBlock>().Count());
                    Assert.InRange(card.ActualHeight, 60, 80);
                }
            }
            finally
            {
                window?.Close();
                application.Shutdown();
            }
        });
    }

    private static void ApplyTheme(FrameworkElement host, GameSaveCenterThemeMode mode)
    {
        var palette = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(
            host,
            glassEnabled: true,
            strengthPercent: 78,
            themeMode: mode,
            highContrastOverride: false);
        AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(host.Resources, palette, glassEnabled: true, motionEnabled: true);
        host.UpdateLayout();
    }

    private static Window CreateWindow(UIElement content, double width, double height)
        => new()
        {
            Content = content,
            Width = width,
            Height = height,
            WindowStyle = WindowStyle.None,
            ShowInTaskbar = false,
            ShowActivated = true,
            Opacity = 0.01
        };

    private static Color BrushColor(object? value)
        => Assert.IsType<SolidColorBrush>(value).Color;

    private static T? FindVisualAncestor<T>(DependencyObject element)
        where T : DependencyObject
    {
        for (DependencyObject? current = element; current != null; current = VisualTreeHelper.GetParent(current))
        {
            if (current is T match)
                return match;
        }

        return null;
    }

    private static void FlushLayout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
        window.UpdateLayout();
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure != null)
            throw new Xunit.Sdk.XunitException(failure.ToString());
    }

    private sealed class CloudTransferSummaryContext
    {
        public CloudTransferSummaryDto CloudTransferViewSummary { get; set; } = new();
    }
}
