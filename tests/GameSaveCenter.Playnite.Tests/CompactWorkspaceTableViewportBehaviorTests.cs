using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("R09FocusWpf")]
public sealed class CompactWorkspaceTableViewportBehaviorTests
{
    [Fact]
    public void WideWorkspacesKeepNestedScrollbarsWithinTheirSurfaces()
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
            var saveHost = MeasureInHost(saveView, width, height);
            saveView.ApplyResponsiveLayout(width, height);
            saveHost.UpdateLayout();
            AssertEqualWorkspaceColumns(GetField<Grid>(saveType, saveView, "SaveHistoryLayout"), "存档历史");
            AssertTableScrollsInsideFrame(historyGrid, GetField<Border>(saveType, saveView, "SaveHistoryTableFrame"));

            var tabs = FindVisualDescendant<TabControl>(saveView);
            var candidateGrid = GetField<DataGrid>(saveType, saveView, "SaveCandidateGrid");
            candidateGrid.ItemsSource = saveRows;
            candidateGrid.SelectedItem = saveRows[0];
            saveType.GetField("candidateInspectorOpen", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(saveView, true);
            tabs.SelectedIndex = 1;
            saveView.ApplyResponsiveLayout(width, height);
            saveHost.UpdateLayout();
            AssertEqualWorkspaceColumns(GetField<Grid>(saveType, saveView, "SaveCandidateLayout"), "存档候选");
            AssertTableScrollsInsideFrame(candidateGrid, GetField<Border>(saveType, saveView, "SaveCandidateTableFrame"));

            var taskView = new TaskCenterView();
            var taskType = typeof(TaskCenterView);
            var taskGrid = GetField<DataGrid>(taskType, taskView, "TaskGrid");
            var taskRows = CreateRows();
            taskGrid.ItemsSource = taskRows;
            taskGrid.SelectedItem = taskRows[0];
            var taskHost = MeasureInHost(taskView, width, height);
            taskView.ApplyResponsiveLayout(width, height);
            taskHost.UpdateLayout();
            AssertEqualWorkspaceColumns(GetField<Grid>(taskType, taskView, "TaskWorkspaceLayout"), "任务队列");
            var taskFrame = GetField<Border>(taskType, taskView, "TaskTableFrame");
            AssertTableScrollsInsideFrame(taskGrid, taskFrame);
            var taskBounds = taskGrid.TransformToAncestor(taskFrame)
                .TransformBounds(new Rect(0, 0, taskGrid.ActualWidth, taskGrid.ActualHeight));
            Assert.True(Math.Abs(taskBounds.Top - taskFrame.BorderThickness.Top - taskFrame.Padding.Top) <= 1,
                $"The task table must start at the frame top instead of floating in its middle: table={taskBounds}, frameHeight={taskFrame.ActualHeight}.");
            Assert.True(taskFrame.ActualHeight - taskBounds.Bottom <= taskFrame.BorderThickness.Bottom + taskFrame.Padding.Bottom + 2,
                $"The task table must use the frame height: table={taskBounds}, frameHeight={taskFrame.ActualHeight}.");

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
            AssertWorkspaceColumnRatio(GetField<Grid>(mediaType, mediaView, "MediaCurrentLayout"), 1.0 / 0.75, "当前游戏媒体");
            AssertPageFitsViewport(mediaPage);
            AssertWithin(mediaGrid, GetField<Border>(mediaType, mediaView, "MediaCurrentTableFrame"));
            AssertInnerScrolls(mediaGrid);

            GetField<TabControl>(mediaType, mediaView, "MediaTabControl").SelectedIndex = 0;
            mediaType.GetField("mediaInboxHistoryOpen", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(mediaView, true);
            var inboxLayout = GetField<Grid>(mediaType, mediaView, "MediaInboxLayout");
            mediaView.ApplyResponsiveLayout(width, height);
            mediaHost.UpdateLayout();
            AssertEqualWorkspaceColumns(inboxLayout, "待归类媒体");
        });
    }

    [Fact]
    public void PageSizingTracksCurrentHostHeightAndKeepsDetailsReachableAfterResize()
    {
        RunSta(() =>
        {
            const double fourKWidth = 3840;
            const double fourKHeight = 2160;
            var saveView = new SaveCenterView();
            var saveType = typeof(SaveCenterView);
            var saveRows = CreateRows();
            var historyGrid = GetField<DataGrid>(saveType, saveView, "SaveHistoryGrid");
            historyGrid.ItemsSource = saveRows;
            historyGrid.SelectedItem = saveRows[0];
            saveType.GetField("historyInspectorOpen", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(saveView, true);
            var saveHost = MeasureInHost(saveView, 900, 900);
            saveView.ApplyResponsiveLayout(900, 900);
            saveHost.UpdateLayout();
            var historyHeightAt900 = historyGrid.ActualHeight;
            ResizeHost(saveHost, fourKWidth, fourKHeight, () => saveView.ApplyResponsiveLayout(fourKWidth, fourKHeight));
            // The off-screen host is not loaded, so explicitly run the settled pass
            // that the production PageScrollViewer raises after its viewport changes.
            saveView.ApplyResponsiveLayout(fourKWidth, fourKHeight);
            saveHost.UpdateLayout();
            var historyHeightAt2160 = historyGrid.ActualHeight;
            Assert.True(historyHeightAt2160 > historyHeightAt900 + 300,
                $"The history table should use the taller host viewport: 900x900 DIP={historyHeightAt900:0.0}, 3840x2160 DIP={historyHeightAt2160:0.0}.");
            Assert.True(historyHeightAt2160 > 560,
                $"The history table must grow past the former fixed 560-DIP cap on a tall host: {historyHeightAt2160:0.0}.");
            ResizeHost(saveHost, 900, 600, () => saveView.ApplyResponsiveLayout(900, 600));

            Assert.True(historyGrid.Height >= historyGrid.MinHeight && historyGrid.ActualHeight < historyHeightAt2160 - 300,
                $"The history table should contract with a short host: minimum={historyGrid.MinHeight:0.0}, short={historyGrid.ActualHeight:0.0}, tall={historyHeightAt2160:0.0}.");
            AssertPageContentMeasuredWithinHost(GetField<ScrollViewer>(saveType, saveView, "SaveHistoryPageScrollViewer"), saveHost);
            AssertTableScrollsInsideFrame(historyGrid, GetField<Border>(saveType, saveView, "SaveHistoryTableFrame"));
            AssertContentBoundsFit(GetField<ScrollViewer>(saveType, saveView, "SaveHistoryPageScrollViewer"),
                GetField<Border>(saveType, saveView, "SaveHistoryTableFrame"));
            AssertInnerScrollCanReachEnd(historyGrid);
            AssertPageCanReachEnd(GetField<ScrollViewer>(saveType, saveView, "SaveHistoryPageScrollViewer"));

            var tabs = FindVisualDescendant<TabControl>(saveView);
            var candidateGrid = GetField<DataGrid>(saveType, saveView, "SaveCandidateGrid");
            candidateGrid.ItemsSource = saveRows;
            candidateGrid.SelectedItem = saveRows[0];
            saveType.GetField("candidateInspectorOpen", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(saveView, true);
            var candidateWindow = new Window { Content = saveHost, Width = 900, Height = 600, ShowInTaskbar = false, WindowStyle = WindowStyle.None, Opacity = 0.01 };
            System.Windows.Automation.AutomationProperties.SetName(candidateWindow, "Candidate viewport resize fixture");
            try
            {
                candidateWindow.Show();
                tabs.SelectedIndex = 1;
                saveView.ApplyResponsiveLayout(900, 600);
                saveHost.UpdateLayout();
                Assert.True(GetField<ScrollViewer>(saveType, saveView, "SaveCandidatePageScrollViewer").IsVisible,
                    "The selected candidate viewport must be visible before testing its settled height.");
                Assert.True(candidateGrid.Height >= candidateGrid.MinHeight && candidateGrid.Height <= 560d);
                var candidateHeightAtShortHost = candidateGrid.ActualHeight;
                ResizeHost(saveHost, fourKWidth, fourKHeight, () => saveView.ApplyResponsiveLayout(fourKWidth, fourKHeight));
                saveView.ApplyResponsiveLayout(fourKWidth, fourKHeight);
                saveHost.UpdateLayout();
                Assert.True(candidateGrid.ActualHeight > 560 && candidateGrid.ActualHeight > candidateHeightAtShortHost + 300,
                    $"The candidate table must fill the taller finite viewport: short={candidateHeightAtShortHost:0.0}, tall={candidateGrid.ActualHeight:0.0}, height={candidateGrid.Height:0.0}, candidateViewport={GetField<ScrollViewer>(saveType, saveView, "SaveCandidatePageScrollViewer").ViewportHeight:0.0}, historyViewport={GetField<ScrollViewer>(saveType, saveView, "SaveHistoryPageScrollViewer").ViewportHeight:0.0}, candidateVisible={GetField<ScrollViewer>(saveType, saveView, "SaveCandidatePageScrollViewer").IsVisible}.");
                ResizeHost(saveHost, 900, 600, () => saveView.ApplyResponsiveLayout(900, 600));
                saveView.ApplyResponsiveLayout(900, 600);
                saveHost.UpdateLayout();
                Assert.True(candidateGrid.ActualHeight < candidateHeightAtShortHost + 1 && candidateGrid.ActualHeight >= candidateGrid.MinHeight - 1);
                var candidatePage = GetField<ScrollViewer>(saveType, saveView, "SaveCandidatePageScrollViewer");
                AssertPageContentMeasuredWithinHost(candidatePage, saveHost);
                AssertTableScrollsInsideFrame(candidateGrid, GetField<Border>(saveType, saveView, "SaveCandidateTableFrame"));
                AssertContentBoundsFit(candidatePage, GetField<Border>(saveType, saveView, "SaveCandidateTableFrame"));
                AssertInnerScrollCanReachEnd(candidateGrid);
                AssertPageCanReachEnd(candidatePage);
            }
            finally { candidateWindow.Close(); }

            var taskView = new TaskCenterView();
            var taskType = typeof(TaskCenterView);
            var taskGrid = GetField<DataGrid>(taskType, taskView, "TaskGrid");
            taskGrid.ItemsSource = saveRows;
            taskGrid.SelectedItem = saveRows[0];
            taskType.GetField("taskInspectorOpen", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(taskView, true);
            var taskHost = MeasureInHost(taskView, 715, 900);
            taskView.ApplyResponsiveLayout(715, 900);
            taskHost.UpdateLayout();
            ResizeHost(taskHost, 715, 577, () => taskView.ApplyResponsiveLayout(715, 577));

            Assert.True(taskGrid.ActualHeight >= taskGrid.MinHeight && taskGrid.ActualHeight <= taskHost.ActualHeight);
            var taskPage = GetField<ScrollViewer>(taskType, taskView, "TaskPageScrollViewer");
            AssertPageContentMeasuredWithinHost(taskPage, taskHost);
            AssertTableScrollsInsideFrame(taskGrid, GetField<Border>(taskType, taskView, "TaskTableFrame"));
            AssertContentBoundsFit(taskPage, GetField<Border>(taskType, taskView, "TaskTableFrame"));
            AssertInnerScrollCanReachEnd(taskGrid);
            AssertPageCanReachEnd(taskPage);
        });
    }

    [Fact]
    public void CompactTaskInspectorReversesItsTransitionWithoutMovingTheQueueAnchor()
    {
        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var view = new TaskCenterView
                {
                    UiAnimationsEnabled = true,
                    SystemMotionEnabledProviderForAudit = () => true
                };
                var type = typeof(TaskCenterView);
                var rows = Enumerable.Range(0, 96).Select(_ => new object()).ToArray();
                var grid = GetField<DataGrid>(type, view, "TaskGrid");
                var layout = GetField<Grid>(type, view, "TaskWorkspaceLayout");
                var frame = GetField<Border>(type, view, "TaskDetailInspectorFrame");
                var detailsViewer = view.TaskDetailScrollViewerElement;
                var openButton = GetField<GameSaveCenter.Playnite.Controls.Button>(type, view, "TaskCompactDetailsButton");
                var closeButton = GetField<GameSaveCenter.Playnite.Controls.Button>(type, view, "TaskCompactCloseDetailsButton");
                grid.ItemsSource = rows;
                // This fixture has no DashboardViewModel. Remove its unresolved SelectedTask
                // binding so the control cannot reset the isolated selection during remeasure.
                BindingOperations.ClearBinding(grid, DataGrid.SelectedItemProperty);

                window = new Window
                {
                    Content = view,
                    Width = 960,
                    Height = 640,
                    WindowStyle = WindowStyle.None,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    Opacity = 0.01
                };
                view.ApplyResponsiveLayout(960, 640);
                window.Show();
                FlushWindowLayout(window);
                grid.SelectedItem = rows[4];
                grid.ScrollIntoView(rows[4]);
                FlushWindowLayout(window);

                var gridScroll = FindVisualDescendant<ScrollViewer>(grid);
                gridScroll.ScrollToVerticalOffset(12);
                FlushWindowLayout(window);
                var selectedAnchor = grid.SelectedItem;
                var scrollAnchor = gridScroll.VerticalOffset;
                Assert.True(scrollAnchor > 0);

                InvokeTaskDetailsClick(view, openButton);
                FlushWindowLayout(window);
                Assert.Equal(Visibility.Visible, frame.Visibility);
                Assert.True(frame.IsHitTestVisible && detailsViewer.IsHitTestVisible);
                Assert.True(layout.RowDefinitions[4].Height.IsAuto);
                Assert.True(DependencyPropertyHelper.GetValueSource(frame, UIElement.OpacityProperty).IsAnimated);
                Assert.False(DependencyPropertyHelper.GetValueSource(layout.ColumnDefinitions[2], ColumnDefinition.WidthProperty).IsAnimated);
                Assert.Same(detailsViewer, Keyboard.FocusedElement);
                if (!ReferenceEquals(selectedAnchor, grid.SelectedItem))
                    throw new InvalidOperationException(
                        $"Selection moved during the open layout: selectedIndex={Array.IndexOf(rows, grid.SelectedItem)}; details={detailsViewer.Visibility}; open={GetField<bool>(type, view, "taskInspectorOpen")}.");
                PumpDispatcherFor(TimeSpan.FromMilliseconds(180));
                FlushWindowLayout(window);
                Assert.Equal(1, frame.Opacity);
                Assert.False(DependencyPropertyHelper.GetValueSource(frame, UIElement.OpacityProperty).IsAnimated);
                Assert.True(GetField<bool>(type, view, "taskInspectorOpen"));

                var breakpoint = GetField<object>(type, view, "detailBreakpoint");
                var isCompact = (bool)breakpoint.GetType().GetProperty("IsCompact")!.GetValue(breakpoint)!;
                if (!isCompact || grid.SelectedItem == null)
                    throw new InvalidOperationException(
                        $"Close preconditions changed: compact={isCompact}; selected={grid.SelectedItem != null}; workspaceWidth={layout.ActualWidth:0.##}; surfaceWidth={GetField<FrameworkElement>(type, view, "TaskPageScrollSurface").ActualWidth:0.##}.");
                InvokeTaskDetailsClick(view, closeButton);
                Assert.True(GetField<bool>(type, view, "taskInspectorClosing"));
                FlushWindowLayout(window);
                Assert.Equal(Visibility.Visible, frame.Visibility);
                Assert.False(frame.IsHitTestVisible);
                Assert.True(layout.RowDefinitions[4].Height.IsAuto);
                Assert.Equal(Visibility.Visible, openButton.Visibility);
                Assert.True(DependencyPropertyHelper.GetValueSource(frame, UIElement.OpacityProperty).IsAnimated);
                Assert.Same(openButton, Keyboard.FocusedElement);

                // Reopen during the exit. The newer target must cancel the pending collapse.
                InvokeTaskDetailsClick(view, openButton);
                Assert.Equal(Visibility.Visible, frame.Visibility);
                Assert.True(frame.IsHitTestVisible);
                Assert.Same(detailsViewer, Keyboard.FocusedElement);
                PumpDispatcherFor(TimeSpan.FromMilliseconds(180));
                FlushWindowLayout(window);
                Assert.Equal(Visibility.Visible, frame.Visibility);
                Assert.Equal(1, frame.Opacity);
                Assert.False(DependencyPropertyHelper.GetValueSource(frame, UIElement.OpacityProperty).IsAnimated);

                InvokeTaskDetailsClick(view, closeButton);
                PumpDispatcherFor(TimeSpan.FromMilliseconds(180));
                FlushWindowLayout(window);
                Assert.Equal(Visibility.Collapsed, frame.Visibility);
                Assert.Equal(new GridLength(0), layout.RowDefinitions[4].Height);
                Assert.Same(selectedAnchor, grid.SelectedItem);
                Assert.InRange(Math.Abs(scrollAnchor - gridScroll.VerticalOffset), 0, 0.1);
                Assert.False(DependencyPropertyHelper.GetValueSource(layout.ColumnDefinitions[2], ColumnDefinition.WidthProperty).IsAnimated);
                Assert.Same(openButton, Keyboard.FocusedElement);
            }
            finally
            {
                window?.Close();
            }
        });
    }

    [Fact]
    public void CompactTaskInspectorHonorsReducedMotionAndClosesImmediately()
    {
        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var view = new TaskCenterView
                {
                    UiAnimationsEnabled = false,
                    SystemMotionEnabledProviderForAudit = () => true
                };
                var type = typeof(TaskCenterView);
                var grid = GetField<DataGrid>(type, view, "TaskGrid");
                var layout = GetField<Grid>(type, view, "TaskWorkspaceLayout");
                var frame = GetField<Border>(type, view, "TaskDetailInspectorFrame");
                var openButton = GetField<GameSaveCenter.Playnite.Controls.Button>(type, view, "TaskCompactDetailsButton");
                var closeButton = GetField<GameSaveCenter.Playnite.Controls.Button>(type, view, "TaskCompactCloseDetailsButton");
                var rows = CreateRows();
                grid.ItemsSource = rows;
                grid.SelectedItem = rows[0];

                window = new Window
                {
                    Content = view,
                    Width = 960,
                    Height = 640,
                    WindowStyle = WindowStyle.None,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    Opacity = 0.01
                };
                view.ApplyResponsiveLayout(960, 640);
                window.Show();
                FlushWindowLayout(window);

                InvokeTaskDetailsClick(view, openButton);
                Assert.Equal(Visibility.Visible, frame.Visibility);
                Assert.Equal(1, frame.Opacity);
                Assert.False(DependencyPropertyHelper.GetValueSource(frame, UIElement.OpacityProperty).IsAnimated);
                Assert.True(layout.RowDefinitions[4].Height.IsAuto);

                InvokeTaskDetailsClick(view, closeButton);
                Assert.Equal(Visibility.Collapsed, frame.Visibility);
                Assert.Equal(new GridLength(0), layout.RowDefinitions[4].Height);
                Assert.False(DependencyPropertyHelper.GetValueSource(frame, UIElement.OpacityProperty).IsAnimated);
            }
            finally
            {
                window?.Close();
            }
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
            AssertBelowPageSurface(page, frame, inspector);

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
            AssertBelowPageSurface(candidatePage, candidateFrame, candidateInspector);
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
            AssertBelowPageSurface(page, frame, view.TaskDetailScrollViewerElement);
        });
    }

    [Fact]
    public void ScreenshotSizedCompactWorkspacesKeepFourRowsAndReachLowerDetails()
    {
        RunSta(() =>
        {
            // Matches the production PageHost inside a common 1040x700 DIP shell.
            // Testing the extracted view at the window size would miss clipping by
            // the shell header and workspace tabs above its own page ScrollViewer.
            const double width = 744;
            const double height = 460;
            var rows = CreateRows();

            var saveView = new SaveCenterView();
            var saveType = typeof(SaveCenterView);
            var historyGrid = GetField<DataGrid>(saveType, saveView, "SaveHistoryGrid");
            historyGrid.ItemsSource = rows;
            historyGrid.SelectedItem = rows[0];
            var candidateGrid = GetField<DataGrid>(saveType, saveView, "SaveCandidateGrid");
            candidateGrid.ItemsSource = rows;
            candidateGrid.SelectedItem = rows[0];
            saveType.GetField("candidateInspectorOpen", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(saveView, true);
            var saveHost = MeasureInHost(saveView, width, height);
            saveView.ApplyResponsiveLayout(width, height);
            saveHost.UpdateLayout();
            var historyPage = GetField<ScrollViewer>(saveType, saveView, "SaveHistoryPageScrollViewer");
            var historyFrame = GetField<Border>(saveType, saveView, "SaveHistoryTableFrame");
            AssertTableScrollsInsideFrame(historyGrid, historyFrame);
            AssertContentBoundsFit(historyPage, historyFrame);
            AssertInnerScrollCanReachEnd(historyGrid);
            AssertPageCanReachEnd(historyPage);

            FindVisualDescendant<TabControl>(saveView).SelectedIndex = 1;
            saveView.ApplyResponsiveLayout(width, height);
            saveHost.UpdateLayout();

            var candidatePage = GetField<ScrollViewer>(saveType, saveView, "SaveCandidatePageScrollViewer");
            var candidateFrame = GetField<Border>(saveType, saveView, "SaveCandidateTableFrame");
            var candidateDetailsButton = GetField<Button>(saveType, saveView, "SaveCandidateCompactDetailsButton");
            var candidateViewControls = GetField<Expander>(saveType, saveView, "SaveCandidateViewConfigurationExpander");
            var candidateViewActions = GetField<WrapPanel>(saveType, saveView, "SaveCandidateViewConfigurationActions");
            Assert.True(candidateGrid.ActualHeight + 1 >= candidateGrid.MinHeight,
                $"Candidate grid must retain four readable rows at {width}x{height} DIP: grid={candidateGrid.ActualHeight:0.0}, minimum={candidateGrid.MinHeight:0.0}.");
            Assert.False(candidateViewControls.IsExpanded);
            Assert.Same(candidateViewControls, candidateViewActions.Parent);
            AssertTableScrollsInsideFrame(candidateGrid, candidateFrame);
            AssertHeaderControlBeforeTable(candidatePage, candidateDetailsButton, candidateFrame);
            AssertFrameFitsPageViewportAtTop(candidatePage, candidateFrame);
            AssertInnerScrollCanReachEnd(candidateGrid);
            AssertPageCanReachEnd(candidatePage);

            var taskView = new TaskCenterView();
            var taskType = typeof(TaskCenterView);
            var taskGrid = GetField<DataGrid>(taskType, taskView, "TaskGrid");
            taskGrid.ItemsSource = rows;
            taskGrid.SelectedItem = rows[0];
            var taskHost = MeasureInHost(taskView, width, height);
            taskView.ApplyResponsiveLayout(width, height);
            taskHost.UpdateLayout();

            var taskPage = GetField<ScrollViewer>(taskType, taskView, "TaskPageScrollViewer");
            var taskFrame = GetField<Border>(taskType, taskView, "TaskTableFrame");
            var taskDetailsButton = GetField<Button>(taskType, taskView, "TaskCompactDetailsButton");
            var taskMoreFilters = GetField<Expander>(taskType, taskView, "TaskMoreFiltersExpander");
            var taskMoreFiltersHost = GetField<Grid>(taskType, taskView, "TaskMoreFiltersHost");
            var taskSummary = GetField<Border>(taskType, taskView, "TaskSummaryPanel");
            var taskFilterPresetRow = GetField<WrapPanel>(taskType, taskView, "TaskFilterPresetRow");
            var taskViewConfigurationRow = GetField<WrapPanel>(taskType, taskView, "TaskViewConfigurationRow");
            Assert.True(taskGrid.ActualHeight + 1 >= taskGrid.MinHeight,
                $"Task grid must retain four readable rows at {width}x{height} DIP: grid={taskGrid.ActualHeight:0.0}, minimum={taskGrid.MinHeight:0.0}.");
            Assert.False(taskMoreFilters.IsExpanded);
            Assert.Same(taskMoreFiltersHost, taskSummary.Parent);
            Assert.True(taskMoreFiltersHost.Children.Contains(taskFilterPresetRow));
            Assert.True(taskMoreFiltersHost.Children.Contains(taskViewConfigurationRow));
            AssertTableScrollsInsideFrame(taskGrid, taskFrame);
            AssertHeaderControlBeforeTable(taskPage, taskDetailsButton, taskFrame);
            AssertFrameFitsPageViewportAtTop(taskPage, taskFrame);
            AssertInnerScrollCanReachEnd(taskGrid);
            AssertPageCanReachEnd(taskPage);
        });
    }

    [Fact]
    public void CompactTaskQueueFrameFitsInsideItsInitialPageViewport()
    {
        RunSta(() =>
        {
            var view = new TaskCenterView();
            var type = typeof(TaskCenterView);
            var grid = GetField<DataGrid>(type, view, "TaskGrid");
            grid.ItemsSource = CreateRows();
            var host = MeasureInHost(view, 744, 460);
            view.ApplyResponsiveLayout(744, 460);
            host.UpdateLayout();

            var page = GetField<ScrollViewer>(type, view, "TaskPageScrollViewer");
            var frame = GetField<Border>(type, view, "TaskTableFrame");
            AssertTableScrollsInsideFrame(grid, frame);
            AssertFrameFitsPageViewportAtTop(page, frame);
        });
    }

    private static T GetField<T>(Type type, object target, string name)
        => (T)type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(target)!;

    private static void InvokeTaskDetailsClick(TaskCenterView view, GameSaveCenter.Playnite.Controls.Button button)
        => typeof(TaskCenterView).GetMethod("OnTaskCompactDetailsClick", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(view, new object[] { button, new RoutedEventArgs() });

    private static void FlushWindowLayout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
        window.UpdateLayout();
    }

    private static void PumpDispatcherFor(TimeSpan duration)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = duration
        };
        timer.Tick += (_, __) =>
        {
            timer.Stop();
            frame.Continue = false;
        };
        timer.Start();
        Dispatcher.PushFrame(frame);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LongHistoryDetailsKeepThreeScrollOffsetsIndependent(bool dark)
    {
        RunSta(() =>
        {
            Window? window = null;
            try
            {
            var view = new SaveCenterView();
            var type = typeof(SaveCenterView);
            var rows = Enumerable.Range(0, 32).Select(index => new HistoryScrollDetail
            {
                BackupId = "cross-scroll-" + index,
                RestoreReadinessHashCoverageDisplay = string.Join(" ", Enumerable.Repeat("Long archive validation detail with nested path and coverage explanation.", 100))
            }).ToArray();
            view.DataContext = new HistoryScrollFixture { SelectedBackup = rows[0] };
            var theme = dark ? GameSaveCenter.Playnite.Settings.GameSaveCenterThemeMode.Dark : GameSaveCenter.Playnite.Settings.GameSaveCenterThemeMode.Light;
            var palette = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(view, true, 78, theme, false);
            AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(view.Resources, palette, true, true);
            var grid = GetField<DataGrid>(type, view, "SaveHistoryGrid");
            grid.ItemsSource = rows;
            grid.SelectedIndex = 0;
            type.GetField("historyInspectorOpen", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(view, true);
            var host = MeasureInHost(view, 900, 600);
            window = new Window { Content = host, Width = 900, Height = 600, WindowStyle = WindowStyle.None, ShowActivated = false, ShowInTaskbar = false, Opacity = 0.01 };
            window.Show();
            window.UpdateLayout();
            view.ApplyResponsiveLayout(900, 600);
            host.UpdateLayout();
            var page = GetField<ScrollViewer>(type, view, "SaveHistoryPageScrollViewer");
            var details = GetField<ScrollViewer>(type, view, "SaveHistoryActionsScrollViewer");
            var list = FindVisualDescendant<ScrollViewer>(grid);
            Assert.True(details.ScrollableHeight > 0, $"Long detail range missing: visible={details.Visibility}, height={details.ActualHeight}, max={details.MaxHeight}, extent={details.ExtentHeight}, viewport={details.ViewportHeight}, selected={grid.SelectedItem?.GetType().Name}, text={((HistoryScrollFixture)view.DataContext).SelectedBackup?.RestoreReadinessHashCoverageDisplay.Length}.");
            list.ScrollToEnd(); host.UpdateLayout();
            var listOffset = list.VerticalOffset;
            var detailsOffset = details.VerticalOffset;
            page.ScrollToEnd(); host.UpdateLayout();
            Assert.Equal(listOffset, list.VerticalOffset, 2);
            Assert.Equal(detailsOffset, details.VerticalOffset, 2);
            var pageOffset = page.VerticalOffset;
            details.ScrollToEnd(); host.UpdateLayout();
            Assert.Equal(listOffset, list.VerticalOffset, 2);
            Assert.Equal(pageOffset, page.VerticalOffset, 2);
            Assert.True(details.VerticalOffset >= details.ScrollableHeight - 1);
            var bottomAction = ((StackPanel)details.Content).Children.OfType<Button>().Last(button => button.Visibility == Visibility.Visible);
            bottomAction.BringIntoView(); host.UpdateLayout();
            var actionBounds = bottomAction.TransformToAncestor(details).TransformBounds(new Rect(0, 0, bottomAction.ActualWidth, bottomAction.ActualHeight));
            Assert.True(actionBounds.Top >= -1 && actionBounds.Bottom <= details.ActualHeight + 1, $"Bottom action not reachable: {actionBounds}, viewport={details.ActualHeight}.");
            AssertInnerScrollCanReachEnd(grid);
            AssertPageCanReachEnd(page);
            AssertTableScrollsInsideFrame(grid, GetField<Border>(type, view, "SaveHistoryTableFrame"));
            }
            finally { window?.Close(); }
        });
    }

    public sealed class HistoryScrollFixture
    {
        public int SaveTabIndex { get; set; }
        public HistoryScrollDetail SelectedBackup { get; set; } = null!;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BulkHistorySelectionSurvivesInspectorBreakpointRoundTrips(bool dark)
    {
        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var rows = Enumerable.Range(0, 120).Select(index => new HistoryScrollDetail { BackupId = "bulk-" + index }).ToArray();
                var view = new SaveCenterView { DataContext = new HistoryScrollFixture { SelectedBackup = rows[20] } };
                var type = typeof(SaveCenterView);
                var theme = dark ? GameSaveCenter.Playnite.Settings.GameSaveCenterThemeMode.Dark : GameSaveCenter.Playnite.Settings.GameSaveCenterThemeMode.Light;
                AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(view.Resources, AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(view, true, 78, theme, false), true, true);
                var grid = GetField<DataGrid>(type, view, "SaveHistoryGrid");
                grid.ItemsSource = rows;
                var host = MeasureInHost(view, 1400, 700);
                window = new Window { Content = host, Width = 1400, Height = 700, WindowStyle = WindowStyle.None, ShowActivated = true, ShowInTaskbar = false, Opacity = 0.01 };
                window.Show(); window.UpdateLayout();
                Assert.True(grid.IsVisible, "The production history grid must be visible before its keyboard focus is tested.");
                Assert.True(grid.Focusable && grid.IsEnabled, "The production history grid must accept keyboard focus.");
                Assert.True(window.IsActive, "The focus fixture window must be the active input window.");
                Assert.Same(grid, System.Windows.Input.Keyboard.Focus(grid));
                Assert.Same(grid, System.Windows.Input.Keyboard.FocusedElement);
                view.ApplyResponsiveLayout(1400, 700);
                window.UpdateLayout();
                grid.ScrollIntoView(rows[20]);
                window.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                window.UpdateLayout();
                var selectionScroll = FindVisualDescendant<ScrollViewer>(grid);
                var beforeSelectionOffset = selectionScroll.VerticalOffset;
                Assert.True(beforeSelectionOffset > 0, "Selection anchor must begin below the first viewport.");
                foreach (var row in rows.Skip(20).Take(40)) grid.SelectedItems.Add(row);
                window.UpdateLayout();
                Assert.InRange(Math.Abs(beforeSelectionOffset - selectionScroll.VerticalOffset), 0, 0.1);
                grid.CurrentCell = new DataGridCellInfo(rows[20], grid.Columns[0]);
                type.GetField("historyInspectorOpen", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(view, true);
                var focusedElement = System.Windows.Input.Keyboard.FocusedElement;
                Assert.NotNull(focusedElement);
                Assert.True(grid.IsKeyboardFocusWithin, "Keyboard focus must remain inside the production history grid after selection.");
                var expected = grid.SelectedItems.Cast<HistoryScrollDetail>().Select(row => row.BackupId).OrderBy(id => id).ToArray();
                Assert.Equal(40, expected.Length);
                for (var cycle = 0; cycle < 3; cycle++)
                {
                    foreach (var width in new[] { 900d, 1400d })
                    {
                        window.Width = width;
                        ResizeHost(host, width, 700, () => view.ApplyResponsiveLayout(width, 700));
                        window.UpdateLayout();
                        Assert.Equal(expected, grid.SelectedItems.Cast<HistoryScrollDetail>().Select(row => row.BackupId).OrderBy(id => id).ToArray());
                        Assert.Same(rows[20], grid.CurrentItem);
                        Assert.Same(focusedElement, System.Windows.Input.Keyboard.FocusedElement);
                        Assert.True(grid.IsKeyboardFocusWithin, "Responsive resizing must keep keyboard focus inside the production history grid.");
                        Assert.Equal(Visibility.Visible, GetField<ScrollViewer>(type, view, "SaveHistoryActionsScrollViewer").Visibility);
                        Assert.True(double.IsNaN(view.Height), "Responsive measurement must not freeze the root height.");
                    }
                }
                var escape = new System.Windows.Input.KeyEventArgs(System.Windows.Input.Keyboard.PrimaryDevice,
                    PresentationSource.FromVisual(grid)!, 0, System.Windows.Input.Key.Escape)
                { RoutedEvent = System.Windows.Input.Keyboard.PreviewKeyDownEvent };
                grid.RaiseEvent(escape);
                window.UpdateLayout();
                Assert.Equal(expected, grid.SelectedItems.Cast<HistoryScrollDetail>().Select(row => row.BackupId).OrderBy(id => id).ToArray());
                var beforeClearOffset = selectionScroll.VerticalOffset;
                grid.SelectedItems.Clear();
                window.UpdateLayout();
                Assert.Empty(grid.SelectedItems);
                Assert.InRange(Math.Abs(beforeClearOffset - selectionScroll.VerticalOffset), 0, 0.1);
            }
            finally { window?.Close(); }
        });
    }

    public sealed class HistoryScrollDetail
    {
        public string BackupId { get; set; } = "";
        public string RestoreReadinessHashCoverageDisplay { get; set; } = "";
    }

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

    private static void ResizeHost(Grid host, double width, double height, Action applyResponsiveLayout)
    {
        host.Width = width;
        host.Height = height;
        host.Measure(new Size(width, height));
        host.Arrange(new Rect(0, 0, width, height));
        applyResponsiveLayout();
        host.UpdateLayout();
    }

    private static void AssertWithin(FrameworkElement element, FrameworkElement ancestor)
    {
        var bottom = element.TransformToAncestor(ancestor).Transform(new Point(0, element.ActualHeight)).Y;
        Assert.True(bottom <= ancestor.ActualHeight + 1,
            $"{element.Name} bottom ({bottom:0.0}) must remain inside {ancestor.Name} height ({ancestor.ActualHeight:0.0}).");
    }

    private static void AssertPageScrolls(ScrollViewer page, FrameworkElement host)
    {
        Assert.True(page.ActualHeight > 0 && page.ActualHeight <= host.ActualHeight + 1,
            $"{page.Name} viewport ({page.ActualWidth:0.0}x{page.ActualHeight:0.0}) must stay within host ({host.ActualWidth:0.0}x{host.ActualHeight:0.0}).");
        Assert.True(page.ViewportHeight > 0);
        Assert.True(page.ScrollableHeight > 0);
    }

    private static void AssertPageFitsViewport(ScrollViewer page)
    {
        Assert.True(page.ViewportHeight > 0);
        Assert.True(page.ScrollableHeight <= 1.5,
            $"Expected the page to fit its host viewport: extent={page.ExtentHeight:0.0}, viewport={page.ViewportHeight:0.0}, scrollable={page.ScrollableHeight:0.0}.");
    }

    private static void AssertPageContentMeasuredWithinHost(ScrollViewer page, FrameworkElement host)
    {
        if (page.ActualHeight <= 0 || page.ActualHeight > host.ActualHeight + 1)
            throw new InvalidOperationException(
                $"{page.Name} viewport ({page.ActualWidth:0.0}x{page.ActualHeight:0.0}) must stay within host ({host.ActualWidth:0.0}x{host.ActualHeight:0.0}).");
        Assert.True(page.ViewportHeight > 0);
        Assert.True(page.ExtentHeight + 1 >= page.ViewportHeight);
        if (page.Content is FrameworkElement content)
            Assert.True(content.ActualHeight <= page.ExtentHeight + 1,
                $"{content.Name} content height ({content.ActualHeight:0.0}) must fit within page extent ({page.ExtentHeight:0.0}).");
    }

    private static void AssertPageCanReachEnd(ScrollViewer page)
    {
        Assert.True(page.ScrollableHeight > 0,
            $"Expected the page scroller to include its lower content: extent={page.ExtentHeight:0.0}, viewport={page.ViewportHeight:0.0}.");
        page.ScrollToEnd();
        page.UpdateLayout();
        Assert.True(page.VerticalOffset >= page.ScrollableHeight - 1,
            $"The page scroller should reach the end of its content: offset={page.VerticalOffset:0.0}, scrollable={page.ScrollableHeight:0.0}.");
    }

    private static void AssertTableScrollsInsideFrame(FrameworkElement table, Border frame)
    {
        Assert.True(table.ActualHeight > 0 && table.ActualHeight <= frame.ActualHeight + 1);
        AssertWithin(table, frame);
        AssertInnerScrolls(table);
        var innerScrollViewer = FindVisualDescendant<ScrollViewer>(table);
        Assert.True(innerScrollViewer.ActualHeight <= frame.ActualHeight + 1);
        AssertWithin(innerScrollViewer, frame);
    }

    private static void AssertEqualWorkspaceColumns(Grid layout, string workspace)
    {
        var tableWidth = layout.ColumnDefinitions[0].ActualWidth;
        var inspectorWidth = layout.ColumnDefinitions[2].ActualWidth;
        Assert.True(tableWidth > 0 && inspectorWidth > 0,
            $"{workspace} should allocate space to both the table ({tableWidth:0.##} DIP) and inspector ({inspectorWidth:0.##} DIP).");
        Assert.True(Math.Abs(tableWidth - inspectorWidth) <= 2,
            $"{workspace} wide layout should give the inspector the same width as the table; table={tableWidth:0.##} DIP, inspector={inspectorWidth:0.##} DIP.");
    }

    private static void AssertWorkspaceColumnRatio(Grid layout, double expectedTableToInspectorRatio, string workspace)
    {
        var tableWidth = layout.ColumnDefinitions[0].ActualWidth;
        var inspectorWidth = layout.ColumnDefinitions[2].ActualWidth;
        Assert.True(tableWidth > 0 && inspectorWidth >= 480,
            $"{workspace} should keep a useful table and an inspector at least 480 DIP wide; table={tableWidth:0.##} DIP, inspector={inspectorWidth:0.##} DIP.");
        var actualRatio = tableWidth / inspectorWidth;
        Assert.InRange(actualRatio, expectedTableToInspectorRatio - 0.02, expectedTableToInspectorRatio + 0.02);
    }

    private static void AssertHeaderControlBeforeTable(ScrollViewer page, FrameworkElement element, Border frame)
    {
        Assert.Equal(Visibility.Visible, element.Visibility);
        Assert.True(page.Content is FrameworkElement,
            $"{page.Name} needs a framework-element content surface to measure its header controls.");
        var content = (FrameworkElement)page.Content;
        var frameBounds = frame.TransformToAncestor(content)
            .TransformBounds(new Rect(0, 0, frame.ActualWidth, frame.ActualHeight));
        var elementBounds = element.TransformToAncestor(content)
            .TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));

        Assert.True(elementBounds.Bottom <= frameBounds.Top + 1,
            $"{element.Name} ({elementBounds}) must stay in its toolbar above the table frame {frame.Name} ({frameBounds}).");
    }

    private static void AssertBelowPageSurface(ScrollViewer page, FrameworkElement upper, FrameworkElement lower)
    {
        Assert.True(page.Content is FrameworkElement,
            $"{page.Name} needs a framework-element content surface to verify stacked content.");
        Assert.Equal(Visibility.Visible, lower.Visibility);
        var content = (FrameworkElement)page.Content;
        var upperBounds = upper.TransformToAncestor(content)
            .TransformBounds(new Rect(0, 0, upper.ActualWidth, upper.ActualHeight));
        var lowerBounds = lower.TransformToAncestor(content)
            .TransformBounds(new Rect(0, 0, lower.ActualWidth, lower.ActualHeight));

        Assert.True(lowerBounds.Top + 1 >= upperBounds.Bottom,
            $"{lower.Name} ({lowerBounds}) must begin after {upper.Name} ({upperBounds}) so it cannot cover the table.");
        Assert.True(lowerBounds.Bottom <= content.ActualHeight + 1,
            $"{lower.Name} ({lowerBounds}) must remain inside {content.Name} ({content.ActualWidth:0.0}x{content.ActualHeight:0.0}).");
    }

    private static void AssertFrameFitsPageViewportAtTop(ScrollViewer page, FrameworkElement frame)
    {
        page.ScrollToTop();
        page.UpdateLayout();
        if (!(page.Content is FrameworkElement content))
            throw new InvalidOperationException($"{page.Name} needs a framework-element content surface.");

        var bounds = frame.TransformToAncestor(content)
            .TransformBounds(new Rect(0, 0, frame.ActualWidth, frame.ActualHeight));
        Assert.True(bounds.Bottom <= page.ViewportHeight + 1,
            $"{frame.Name} is clipped by {page.Name} before its table is fully visible: frameBottom={bounds.Bottom:0.0}, viewport={page.ViewportHeight:0.0}, pageExtent={page.ExtentHeight:0.0}.");
    }

    private static void AssertContentBoundsFit(ScrollViewer page, FrameworkElement descendant)
    {
        Assert.True(page.Content is FrameworkElement,
            $"{page.Name} needs a framework-element content surface to measure its scroll extent.");
        var content = (FrameworkElement)page.Content;
        var bounds = descendant.TransformToAncestor(content)
            .TransformBounds(new Rect(0, 0, descendant.ActualWidth, descendant.ActualHeight));
        Assert.True(bounds.Top >= -1 && bounds.Bottom <= content.ActualHeight + 1,
            $"{descendant.Name} bounds ({bounds}) must remain inside scroll content {content.Name} ({content.ActualWidth:0.0}x{content.ActualHeight:0.0}).");
        Assert.True(content.ActualHeight <= page.ExtentHeight + 1,
            $"{page.Name} extent ({page.ExtentHeight:0.0}) must include content height ({content.ActualHeight:0.0}).");
    }

    private static void AssertInnerScrollCanReachEnd(DataGrid grid)
    {
        var viewer = FindVisualDescendant<ScrollViewer>(grid);
        viewer.ScrollToEnd();
        viewer.UpdateLayout();
        Assert.True(viewer.ScrollableHeight > 0,
            $"{grid.Name} needs an inner scroll range for its virtualized rows.");
        Assert.True(viewer.VerticalOffset >= viewer.ScrollableHeight - 1,
            $"{grid.Name} inner scroll should reach its end: offset={viewer.VerticalOffset:0.0}, scrollable={viewer.ScrollableHeight:0.0}.");
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
