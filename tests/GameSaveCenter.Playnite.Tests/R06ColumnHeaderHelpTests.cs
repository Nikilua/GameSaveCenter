using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R06ColumnHeaderHelpTests
{
    [Fact]
    public void GeneratedHeaderGetsDescriptionWithoutReplacingSortingHeader()
    {
        RunSta(() =>
        {
            var grid = new DataGrid
            {
                AutoGenerateColumns = false,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                CanUserSortColumns = true,
                CanUserResizeColumns = true,
                ItemsSource = new[] { new HeaderRow { Value = "512 MiB" } }
            };
            var column = new DataGridTextColumn { Header = "大小", Binding = new System.Windows.Data.Binding("Value") };
            var description = "总大小；按 1024 进制显示 B、KiB、MiB 或 GiB。";
            DataGridColumnHeaderHelpBehavior.SetDescription(column, description);
            grid.Columns.Add(column);

            var headerStyle = new Style(typeof(DataGridColumnHeader));
            headerStyle.Setters.Add(new Setter(DataGridColumnHeaderHelpBehavior.EnabledProperty, true));
            grid.ColumnHeaderStyle = headerStyle;

            var host = CreateHost(grid);
            try
            {
                host.Show();
                host.UpdateLayout();
                grid.UpdateLayout();

                var header = FindVisualChildren<DataGridColumnHeader>(grid)
                    .Single(candidate => ReferenceEquals(candidate.Column, column));
                Assert.Equal("大小", header.Column.Header as string);
                Assert.Equal(description, header.ToolTip as string);
                Assert.Equal(description, AutomationProperties.GetHelpText(header));
                Assert.True(header.Column.CanUserSort);
                Assert.True(header.Column.CanUserReorder);
                Assert.Empty(FindVisualChildren<ButtonBase>(header));
            }
            finally
            {
                host.Close();
            }
        });
    }

    [Fact]
    public void ProductionTablesUseSharedHeaderHelpAndKeepLongHeaderStrategy()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var production = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "WpfUiProduction.xaml"));
        var save = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
        var task = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));
        var media = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
        var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));

        Assert.Contains("DataGridColumnHeaderHelpBehavior.Enabled", production, StringComparison.Ordinal);
        Assert.Contains("TextWrapping=\"Wrap\"", production, StringComparison.Ordinal);
        Assert.Contains("TextTrimming=\"None\"", production, StringComparison.Ordinal);
        Assert.Contains("按 1024 进制显示 B、KiB、MiB 或 GiB", save, StringComparison.Ordinal);
        Assert.Contains("候选匹配置信度", save, StringComparison.Ordinal);
        Assert.Contains("任务生命周期状态", task, StringComparison.Ordinal);
        Assert.Contains("完成比例；有效值为 0–100%", task, StringComparison.Ordinal);
        Assert.Contains("媒体类型", media, StringComparison.Ordinal);
        Assert.Contains("诊断严重级别", maintenance, StringComparison.Ordinal);
        Assert.DoesNotContain("Header=\"{Binding", save, StringComparison.Ordinal);
        Assert.DoesNotContain("Header=\"{Binding", task, StringComparison.Ordinal);
        Assert.DoesNotContain("Header=\"{Binding", media, StringComparison.Ordinal);
    }

    private static Window CreateHost(FrameworkElement content)
        => new Window
        {
            Width = 500,
            Height = 180,
            ShowInTaskbar = false,
            WindowStyle = WindowStyle.None,
            Opacity = 0.01,
            Content = content
        };

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
        where T : DependencyObject
    {
        if (root == null) yield break;
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match) yield return match;
            foreach (var descendant in FindVisualChildren<T>(child)) yield return descendant;
        }
    }

    private static void RunSta(Action action)
    {
        Exception error = null!;
        var thread = new System.Threading.Thread(() =>
        {
            try
            {
                new System.Windows.Threading.DispatcherSynchronizationContext();
                action();
            }
            catch (Exception caught)
            {
                error = caught;
            }
            finally
            {
                if (System.Windows.Threading.Dispatcher.CurrentDispatcher.HasShutdownStarted == false)
                    System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        });
        thread.SetApartmentState(System.Threading.ApartmentState.STA);
        thread.Start();
        thread.Join(TimeSpan.FromSeconds(20));
        if (thread.IsAlive) throw new TimeoutException("STA WPF header fixture timed out.");
        if (error != null) throw new AggregateException(error);
    }

    private sealed class HeaderRow
    {
        public string Value { get; set; } = string.Empty;
    }
}
