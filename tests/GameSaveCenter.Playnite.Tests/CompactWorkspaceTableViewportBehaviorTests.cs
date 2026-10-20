using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("R09FocusWpf")]
public sealed class CompactWorkspaceTableViewportBehaviorTests
{
    [Fact]
    public void WideWorkspacesAllocateRemainingPageHeightToTablesWithoutPageOverflow()
    {
        RunSta(() =>
        {
            const double width = 1400;
            const double height = 900;

            var saveView = new SaveCenterView();
            var saveType = typeof(SaveCenterView);
            var saveRows = CreateRows();
            var historyGrid = GetField<DataGrid>(saveType, saveView, "SaveHistoryGrid");
            historyGrid.ItemsSource = saveRows;
            historyGrid.SelectedItem = saveRows[0];
            var historyPage = GetField<ScrollViewer>(saveType, saveView, "SaveHistoryPageScrollViewer");
            var saveHost = MeasureInHost(saveView, width, height);
            saveView.ApplyResponsiveLayout(width, height);
            saveHost.UpdateLayout();
            AssertPageFitsViewport(historyPage);
            AssertTableScrollsInsideFrame(historyGrid, GetField<Border>(saveType, saveView, "SaveHistoryTableFrame"));

            var tabs = FindVisualDescendant<TabControl>(saveView);
            var candidateGrid = GetField<DataGrid>(saveType, saveView, "SaveCandidateGrid");
            candidateGrid.ItemsSource = saveRows;
            candidateGrid.SelectedItem = saveRows[0];
            saveType.GetField("candidateInspectorOpen", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(saveView, true);
            tabs.SelectedIndex = 1;
            saveView.ApplyResponsiveLayout(width, height);
            saveHost.UpdateLayout();
            AssertPageFitsViewport(GetField<ScrollViewer>(saveType, saveView, "SaveCandidatePageScrollViewer"));
            AssertTableScrollsInsideFrame(candidateGrid, GetField<Border>(saveType, saveView, "SaveCandidateTableFrame"));

            var taskView = new TaskCenterView();
            var taskType = typeof(TaskCenterView);
            var taskGrid = GetField<DataGrid>(taskType, taskView, "TaskGrid");
            var taskRows = CreateRows();
            taskGrid.ItemsSource = taskRows;
            taskGrid.SelectedItem = taskRows[0];
            var taskPage = GetField<ScrollViewer>(taskType, taskView, "TaskPageScrollViewer");
            var taskHost = MeasureInHost(taskView, width, height);
            taskView.ApplyResponsiveLayout(width, height);
            taskHost.UpdateLayout();
            AssertPageFitsViewport(taskPage);
            AssertTableScrollsInsideFrame(taskGrid, GetField<Border>(taskType, taskView, "TaskTableFrame"));

            var mediaView = new MediaCenterView();
            var mediaType = typeof(MediaCenterView);
            GetField<TabControl>(mediaType, mediaView, "MediaTabControl").SelectedIndex = 1;
            var mediaGrid = GetField<ListBox>(mediaType, mediaView, "MediaGrid");
            var mediaRows = CreateRows();
            mediaGrid.ItemsSource = mediaRows;
            mediaGrid.SelectedItem = mediaRows[0];
            var mediaPage = GetField<ScrollViewer>(mediaType, mediaView, "MediaCurrentPageScrollViewer");
            var mediaHost = MeasureInHost(mediaView, width, height);
            mediaView.ApplyResponsiveLayout(width, height);
            mediaHost.UpdateLayout();
            AssertPageFitsViewport(mediaPage);
            AssertWithin(mediaGrid, GetField<Border>(mediaType, mediaView, "MediaCurrentTableFrame"));
            AssertInnerScrolls(mediaGrid);
        });
    }

    [Fact]
    public void SaveHistoryAndPathTablesKeepFiniteInnerScrollbarsInsideScrollablePages()
    {
        RunSta(() =>
        {
            var view = new SaveCenterView();
            var type = typeof(SaveCenterView);
            var grid = GetField<DataGrid>(type, view, "SaveHistoryGrid");
            var frame = GetField<Border>(type, view, "SaveHistoryTableFrame");
            var page = GetField<ScrollViewer>(type, view, "SaveHistoryPageScrollViewer");
            var inspector = GetField<ScrollViewer>(type, view, "SaveHistoryActionsScrollViewer");
            var rows = CreateRows();
            grid.ItemsSource = rows;
            grid.SelectedItem = rows[0];
            type.GetField("historyInspectorOpen", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(view, true);

            view.ApplyResponsiveLayout(900, 600);
            var host = MeasureInHost(view, 900, 600);
            view.ApplyResponsiveLayout(900, 600);
            host.UpdateLayout();

            Assert.True(grid.MinHeight > 0 && grid.ActualHeight >= grid.MinHeight);
            Assert.Equal(Visibility.Visible, inspector.Visibility);
            Assert.True(inspector.ActualHeight <= inspector.MaxHeight + 1);
            AssertPageScrolls(page, view);
            AssertTableScrollsInsideFrame(grid, frame);

            var tabControl = FindVisualDescendant<TabControl>(view);
            var candidateGrid = GetField<DataGrid>(type, view, "SaveCandidateGrid");
            var candidatePage = GetField<ScrollViewer>(type, view, "SaveCandidatePageScrollViewer");
            var candidateFrame = FindVisualAncestor<Border>(candidateGrid);
            var candidateInspector = GetField<ScrollViewer>(type, view, "SaveCandidateInspectorScrollViewer");
            candidateGrid.ItemsSource = rows;
            candidateGrid.SelectedItem = rows[0];
            type.GetField("candidateInspectorOpen", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(view, true);
            tabControl.SelectedIndex = 1;
            view.ApplyResponsiveLayout(900, 600);
            host.UpdateLayout();
            view.ApplyResponsiveLayout(900, 600);
            host.UpdateLayout();

            Assert.True(candidateGrid.MinHeight > 0 && candidateGrid.ActualHeight >= candidateGrid.MinHeight);
            Assert.Equal(Visibility.Visible, candidateInspector.Visibility);
            Assert.True(candidateInspector.ActualHeight <= candidateInspector.MaxHeight + 1);
            AssertPageScrolls(candidatePage, view);
            AssertTableScrollsInsideFrame(candidateGrid, candidateFrame);
        });
    }

    [Fact]
    public void CurrentMediaListAndInspectorKeepFiniteScrollbarsInsideScrollablePage()
    {
        RunSta(() =>
        {
            var view = new MediaCenterView();
            var type = typeof(MediaCenterView);
            GetField<TabControl>(type, view, "MediaTabControl").SelectedIndex = 1;
            var layout = GetField<Grid>(type, view, "MediaCurrentLayout");
            var grid = GetField<ListBox>(type, view, "MediaGrid");
            var frame = GetField<Border>(type, view, "MediaCurrentTableFrame");
            var page = GetField<ScrollViewer>(type, view, "MediaCurrentPageScrollViewer");
            var rows = CreateRows();
            grid.ItemsSource = rows;
            grid.SelectedItem = rows[0];
            type.GetField("mediaInspectorOpen", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(view, true);

            view.ApplyResponsiveLayout(900, 600);
            var host = MeasureInHost(view, 900, 600);
            view.ApplyResponsiveLayout(900, 600);
            host.UpdateLayout();

            var inspector = view.MediaInspectorScrollViewerElement;
            Assert.True(grid.MinHeight >= 154 && grid.ActualHeight >= grid.MinHeight);
            Assert.Equal(Math.Min(220, Math.Max(0, page.ViewportHeight * 0.30)), inspector.MaxHeight, 1);
            Assert.True(inspector.ActualHeight > 0 && inspector.ActualHeight <= inspector.MaxHeight + 1);
            Assert.Equal(Visibility.Visible, inspector.Visibility);
            Assert.True(layout.RowDefinitions[1].Height.IsAuto);
            AssertPageScrolls(page, view);
            AssertWithin(grid, frame);
            AssertInnerScrolls(grid);
        });
    }

    [Fact]
    public void TaskQueueAndCompactInspectorKeepTheirOwnScrollbarsInsideScrollablePage()
    {
        RunSta(() =>
        {
            var view = new TaskCenterView();
            var type = typeof(TaskCenterView);
            var workspace = GetField<Grid>(type, view, "TaskWorkspaceLayout");
            var queue = GetField<Border>(type, view, "TaskQueuePanel");
            var grid = GetField<DataGrid>(type, view, "TaskGrid");
            var frame = GetField<Border>(type, view, "TaskTableFrame");
            var page = GetField<ScrollViewer>(type, view, "TaskPageScrollViewer");
            var rows = CreateRows();
            grid.ItemsSource = rows;
            grid.SelectedItem = rows[0];
            type.GetField("taskInspectorOpen", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(view, true);

            view.ApplyResponsiveLayout(715, 577);
            var host = MeasureInHost(view, 715, 577);
            view.ApplyResponsiveLayout(715, 577);
            host.UpdateLayout();

            Assert.True(grid.MinHeight > 0 && grid.ActualHeight >= grid.MinHeight);
            Assert.Equal(Visibility.Visible, view.TaskDetailScrollViewerElement.Visibility);
            Assert.True(view.TaskDetailScrollViewerElement.MaxHeight <= 136 + 1);
            Assert.True(view.TaskDetailScrollViewerElement.ActualHeight <= view.TaskDetailScrollViewerElement.MaxHeight + 1);
            Assert.True(workspace.ActualHeight >= page.ViewportHeight);
            Assert.True(queue.ActualHeight > 0);
            AssertPageScrolls(page, view);
            AssertTableScrollsInsideFrame(grid, frame);
        });
    }

    private static T GetField<T>(Type type, object target, string name)
        => (T)type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(target)!;

    private static object[] CreateRows()
        => Enumerable.Range(0, 32).Select(_ => new object()).ToArray();

    private static T FindVisualDescendant<T>(DependencyObject root) where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
                return match;
            try
            {
                var descendant = FindVisualDescendant<T>(child);
                if (descendant != null)
                    return descendant;
            }
            catch (InvalidOperationException)
            {
                // Non-visual content has no VisualTreeHelper children.
            }
        }
        throw new InvalidOperationException($"Could not find a {typeof(T).Name} in the visual tree.");
    }

    private static T FindVisualAncestor<T>(DependencyObject element) where T : DependencyObject
    {
        var current = VisualTreeHelper.GetParent(element);
        while (current != null)
        {
            if (current is T match)
                return match;
            current = VisualTreeHelper.GetParent(current);
        }
        throw new InvalidOperationException($"Could not find a {typeof(T).Name} above {element.GetType().Name}.");
    }

    private static Grid MeasureInHost(UserControl view, double width, double height)
    {
        var host = new Grid { Width = width, Height = height };
        host.Children.Add(view);
        host.Measure(new Size(width, height));
        host.Arrange(new Rect(0, 0, width, height));
        host.UpdateLayout();
        return host;
    }

    private static void AssertWithin(FrameworkElement element, FrameworkElement ancestor)
    {
        var bottom = element.TransformToAncestor(ancestor).Transform(new Point(0, element.ActualHeight)).Y;
        Assert.True(bottom <= ancestor.ActualHeight + 1,
            $"{element.Name} bottom ({bottom:0.0}) must remain inside {ancestor.Name} height ({ancestor.ActualHeight:0.0}).");
    }

    private static void AssertPageScrolls(ScrollViewer page, FrameworkElement host)
    {
        Assert.True(page.ActualHeight > 0 && page.ActualHeight <= host.ActualHeight + 1);
        Assert.True(page.ViewportHeight > 0);
        Assert.True(page.ExtentHeight > page.ViewportHeight + 1,
            $"Expected the page scroll channel to expose overflow: extent={page.ExtentHeight:0.0}, viewport={page.ViewportHeight:0.0}.");
        Assert.True(page.ScrollableHeight > 0);
    }

    private static void AssertPageFitsViewport(ScrollViewer page)
    {
        Assert.True(page.ViewportHeight > 0);
        Assert.True(page.ScrollableHeight <= 1.5,
            $"Expected the page to fit its host viewport: extent={page.ExtentHeight:0.0}, viewport={page.ViewportHeight:0.0}, scrollable={page.ScrollableHeight:0.0}.");
    }

    private static void AssertTableScrollsInsideFrame(FrameworkElement table, Border frame)
    {
        Assert.True(table.ActualHeight > 0 && table.ActualHeight <= frame.ActualHeight + 1);
        AssertWithin(table, frame);
        AssertInnerScrolls(table);
    }

    private static void AssertInnerScrolls(FrameworkElement list)
    {
        var innerScrollViewer = FindVisualDescendant<ScrollViewer>(list);
        Assert.True(innerScrollViewer.ActualHeight > 0);
        Assert.True(innerScrollViewer.ScrollableHeight > 0,
            $"Expected the virtualized list to retain an inner scroll channel; viewport={innerScrollViewer.ViewportHeight:0.0}, extent={innerScrollViewer.ExtentHeight:0.0}.");
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure != null)
            throw new InvalidOperationException("Compact workspace table layout test failed.", failure);
    }
}
