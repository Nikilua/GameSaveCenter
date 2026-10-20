using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
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
public sealed class S05StorageBudgetBreakdownBehaviorTests
{
    [Fact]
    public void MaintenanceViewShowsIndependentStorageScopesAndUnknownValuesInProductionTemplate()
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                foreach (var theme in new[] { GameSaveCenterThemeMode.Light, GameSaveCenterThemeMode.Dark })
                {
                    var archive = Entry("本地存档", "已完成目录测量", 4096, 8192, "SQLite 已索引版本逻辑大小", "失联索引仍单独提示。");
                    var mirror = Entry("第二本地镜像", "目录不存在或不可访问", null, null, string.Empty, "镜像副本可能重复，不作相加。");
                    var staging = Entry("远端恢复暂存", "目录为空", 0, null, string.Empty, "无暂存文件。");
                    var media = Entry("媒体归档", "已完成目录测量", 16384, 32768, "全部登记媒体记录逻辑大小", "目录外登记文件不会进入实测。");
                    var state = new StorageBudgetContext(new StorageAnalysisDto
                    {
                        BudgetBreakdown = new List<StorageBudgetEntryDto> { archive, mirror, staging, media }
                    });

                    var view = new MaintenanceView { DataContext = state };
                    ApplyTheme(view, theme);
                    var window = new Window
                    {
                        Content = view,
                        Width = 1100,
                        Height = 900,
                        ShowInTaskbar = false,
                        WindowStyle = WindowStyle.None,
                        ResizeMode = ResizeMode.NoResize,
                        ShowActivated = false,
                        Opacity = 0.01
                    };
                    try
                    {
                        window.Show();
                        PumpLayout(window);
                        var breakdown = (Border)view.FindName("MaintenanceStorageDemoBudgetBreakdown")!;
                        var items = (ItemsControl)view.FindName("MaintenanceStorageDemoBudgetEntries")!;
                        Assert.Equal(Visibility.Visible, breakdown.Visibility);
                        Assert.Equal(4, items.Items.Count);
                        FindVisualAncestor<ScrollViewer>(breakdown)?.ScrollToVerticalOffset(double.MaxValue);
                        PumpLayout(window);
                        Assert.True(breakdown.IsVisible, $"{theme}: storage budget card was not reachable by the page scroll.");

                        var rendered = FindVisualDescendants<TextBlock>(breakdown)
                            .Select(block => block.Text)
                            .Where(text => !string.IsNullOrWhiteSpace(text))
                            .ToArray();
                        Assert.Contains("本地存档", rendered);
                        Assert.Contains("第二本地镜像", rendered);
                        Assert.Contains("远端恢复暂存", rendered);
                        Assert.Contains("媒体归档", rendered);
                        Assert.Contains("未知", rendered);
                        Assert.Contains("未统计", rendered);
                        Assert.Contains("目录为空", rendered);
                        Assert.Contains(rendered, text => string.Equals(text.Trim(), "目录文件长度", StringComparison.Ordinal));
                        Assert.Contains(rendered, text => text.IndexOf("文件长度合计不等于磁盘簇实占", StringComparison.Ordinal) >= 0);
                    }
                    finally
                    {
                        window.Close();
                    }
                }
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        Assert.Null(exception);
    }

    private static StorageBudgetEntryDto Entry(
        string name,
        string status,
        long? fileLengthBytes,
        long? logicalBytes,
        string logicalMetricLabel,
        string boundary)
        => new()
        {
            Name = name,
            StatusDisplay = status,
            ScopeDisplay = name + "范围说明",
            BoundaryDisplay = boundary,
            FileLengthBytes = fileLengthBytes,
            FileCount = fileLengthBytes.HasValue ? 1 : null,
            HasLogicalMetric = logicalMetricLabel.Length > 0,
            LogicalMetricLabel = logicalMetricLabel,
            LogicalBytes = logicalBytes,
            LogicalItemCount = logicalBytes.HasValue ? 1 : null
        };

    private static void PumpLayout(Window window)
    {
        window.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
        window.UpdateLayout();
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

    private static T? FindVisualAncestor<T>(DependencyObject? current)
        where T : DependencyObject
    {
        for (var node = current; node != null; node = VisualTreeHelper.GetParent(node))
        {
            if (node is T match) return match;
        }
        return null;
    }

    private sealed class StorageBudgetContext
    {
        public StorageBudgetContext(StorageAnalysisDto storageAnalysis) => StorageAnalysis = storageAnalysis;
        public int MaintenanceTabIndex { get; set; } = 3;
        public StorageAnalysisDto StorageAnalysis { get; }
    }
}
