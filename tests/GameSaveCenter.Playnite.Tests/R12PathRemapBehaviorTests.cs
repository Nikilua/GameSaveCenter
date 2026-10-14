using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("R12PathRemapWpf")]
public sealed class R12PathRemapBehaviorTests
{
    [Fact]
    public void PreviewItemKeepsSimilarPathsDistinctAndExplainsMissingTarget()
    {
        var oldPath = @"C:\Saves\Profile\long-name\slot-01.sav";
        var newPath = @"D:\Archive\Profile\long-name\slot-01.sav";
        var item = new PathRemapPreviewItemDto
        {
            Category = "存档候选",
            OldPath = oldPath,
            NewPath = newPath,
            TargetExists = false
        };

        Assert.Equal(oldPath, item.OldPathDisplay);
        Assert.Equal(newPath, item.NewPathDisplay);
        Assert.Equal("目标不存在（需明确确认）", item.TargetStateDisplay);
        Assert.NotEqual(oldPath.Replace("C:", "D:"), item.OldPathDisplay);
    }

    [Fact]
    public void MaintenanceViewUsesBoundedVirtualizedPreviewWithCopyableFullPaths()
    {
        var path = Path.Combine(TestRepositoryContext.Root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml");
        var document = XDocument.Load(path);
        var grid = document.Descendants().Single(element =>
            element.Name.LocalName == "DataGrid" && element.Attribute(XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml"))?.Value == "PathRemapPreviewGrid");
        var source = File.ReadAllText(path);

        Assert.Equal("{Binding PathRemapPreview.Items}", grid.Attribute("ItemsSource")?.Value);
        Assert.Equal("True", grid.Attribute("EnableRowVirtualization")?.Value);
        Assert.Equal("260", grid.Attribute("MaxHeight")?.Value);
        Assert.Contains("将写入路径（仅预览）", source);
        Assert.Contains("OldPathDisplay", source);
        Assert.Contains("NewPathDisplay", source);
        Assert.Contains("TargetStateDisplay", source);
        Assert.Contains("IsReadOnly=\"True\"", source);
        Assert.Contains("不会写入用户目录", source);
        Assert.DoesNotContain("File.Move", source);
        Assert.DoesNotContain("Directory.Move", source);
    }

    [Fact]
    public void ProductionPreviewLoadsLongPathsAsSelectableTextInsideBoundedVirtualizedGrid()
    {
        RunSta(() =>
        {
            var longSegment = new string('x', 210);
            var oldPath = $@"C:\Synthetic\Saves\{longSegment}\slot-01.sav";
            var newPath = $@"D:\Archive\Saves\{longSegment}\slot-01.sav";
            var items = Enumerable.Range(0, 80)
                .Select(index => new PathRemapPreviewItemDto
                {
                    Category = "存档候选",
                    OldPath = index == 0 ? oldPath : $@"C:\Synthetic\Saves\game-{index}\slot.sav",
                    NewPath = index == 0 ? newPath : $@"D:\Archive\Saves\game-{index}\slot.sav",
                    TargetExists = false
                })
                .ToList();
            var page = new MaintenanceView
            {
                DataContext = new PathRemapViewContext
                {
                    PathRemapPreview = new PathRemapPreviewDto { Items = items }
                }
            };
            var outerTabs = Assert.IsType<TabControl>(page.FindName("MaintenanceTabControl"));
            BindingOperations.ClearBinding(outerTabs, TabControl.SelectedIndexProperty);
            outerTabs.SelectedIndex = 0;
            var diagnosticTabs = Assert.IsType<TabControl>(page.FindName("MaintenanceDiagnosticsSubTabs"));
            diagnosticTabs.SelectedIndex = 1;
            var actionDisclosure = Assert.IsType<Expander>(page.FindName("MaintenanceActionsDisclosure"));
            actionDisclosure.IsExpanded = true;
            var grid = Assert.IsType<DataGrid>(page.FindName("PathRemapPreviewGrid"));
            var window = new Window
            {
                Content = page,
                Width = 1440,
                Height = 1000,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = false,
                Opacity = 0.01
            };

            try
            {
                window.Show();
                PumpLayout(window);

                Assert.Equal(80, grid.Items.Count);
                Assert.True(grid.IsVisible && grid.ActualWidth > 0,
                    $"preview grid was not realized: {grid.Visibility}/{grid.IsVisible}, {grid.ActualWidth:0.##}x{grid.ActualHeight:0.##}; outer={outerTabs.SelectedIndex}; diagnostics={diagnosticTabs.SelectedIndex}");
                Assert.InRange(grid.ActualHeight, 60, 261);
                Assert.True(VirtualizingPanel.GetIsVirtualizing(grid));
                Assert.Equal(VirtualizationMode.Recycling, VirtualizingPanel.GetVirtualizationMode(grid));

                var firstRow = Assert.IsType<DataGridRow>(grid.ItemContainerGenerator.ContainerFromIndex(0));
                Assert.Null(grid.ItemContainerGenerator.ContainerFromIndex(items.Count - 1));
                var oldPathText = Assert.Single(FindVisualChildren<TextBox>(firstRow), textBox => textBox.Text == oldPath);
                var newPathText = Assert.Single(FindVisualChildren<TextBox>(firstRow), textBox => textBox.Text == newPath);

                Assert.True(oldPathText.IsReadOnly && newPathText.IsReadOnly);
                Assert.Equal(TextWrapping.NoWrap, oldPathText.TextWrapping);
                Assert.Equal(TextWrapping.NoWrap, newPathText.TextWrapping);
                Assert.Same(oldPathText, Keyboard.Focus(oldPathText));
                oldPathText.SelectAll();
                Assert.Equal(oldPath, oldPathText.SelectedText);
                Assert.True(ApplicationCommands.Copy.CanExecute(null, oldPathText));

                Assert.Same(newPathText, Keyboard.Focus(newPathText));
                newPathText.SelectAll();
                Assert.Equal(newPath, newPathText.SelectedText);
                Assert.True(ApplicationCommands.Copy.CanExecute(null, newPathText));
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
                yield return match;
            foreach (var nested in FindVisualChildren<T>(child))
                yield return nested;
        }
    }

    private static void PumpLayout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
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
        Assert.Null(failure);
    }

    private sealed class PathRemapViewContext
    {
        public PathRemapPreviewDto PathRemapPreview { get; set; } = new();
    }
}
