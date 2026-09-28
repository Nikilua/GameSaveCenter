using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Views;
using Xunit;
using Xunit.Abstractions;

namespace GameSaveCenter.Playnite.Tests;

public sealed class MediaInboxScrollBehaviorTests
{
    private readonly ITestOutputHelper output;

    public MediaInboxScrollBehaviorTests(ITestOutputHelper output) => this.output = output;

    [Theory]
    [InlineData(1.0)]
    [InlineData(1.25)]
    [InlineData(1.5)]
    public void InboxRowsStayAnchoredThroughThumbEndRepeatedScrollAndResize(double outputScale)
    {
        Exception? exception = null;
        var observations = new List<string>();

        RunSta(() =>
        {
            Window? window = null;
            MediaCenterView view = null!;
            BatchObservableCollection<MediaItemDto> items = null!;
            DataGrid grid = null!;
            ScrollViewer pageScroller = null!;
            try
            {
                view = new MediaCenterView();
                var tabControl = (TabControl)typeof(MediaCenterView)
                    .GetField("MediaTabControl", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                    .GetValue(view)!;
                System.Windows.Data.BindingOperations.ClearBinding(tabControl, TabControl.SelectedIndexProperty);
                tabControl.SelectedIndex = 0;
                grid = (DataGrid)typeof(MediaCenterView)
                    .GetField("MediaInboxGrid", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                    .GetValue(view)!;
                pageScroller = (ScrollViewer)typeof(MediaCenterView)
                    .GetField("MediaInboxPageScrollViewer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                    .GetValue(view)!;
                items = new BatchObservableCollection<MediaItemDto>();
                grid.ItemsSource = items;

                var renderSurface = new Grid
                {
                    RenderTransformOrigin = new Point(0, 0),
                    RenderTransform = new ScaleTransform(outputScale, outputScale)
                };
                renderSurface.Children.Add(view);
                window = new Window
                {
                    Content = renderSurface,
                    Width = 1120,
                    Height = 700,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    WindowStyle = WindowStyle.None,
                    ResizeMode = ResizeMode.CanResize,
                    Opacity = 0.01
                };
                window.SizeChanged += (_, _) => view.ApplyResponsiveLayout(view.ActualWidth, view.ActualHeight);
                view.ApplyResponsiveLayout(window.Width, window.Height);
                window.Show();
                view.ApplyResponsiveLayout(view.ActualWidth, view.ActualHeight);
                DrainDispatcher(window);
                Assert.True(window.IsLoaded && view.IsLoaded && grid.IsLoaded,
                    $"the production inbox tab must be selected and connected before binding the scroll fixture: window={window.IsLoaded},view={view.IsLoaded},grid={grid.IsLoaded},tab={tabControl.SelectedIndex}");
                items.ReplaceAll(CreateSyntheticMedia(2000));
                DrainDispatcher(window);

                var internalViewer = FindInternalGridViewer(grid);
                Assert.True(grid.EnableRowVirtualization, "the production inbox must keep row virtualization enabled");
                Assert.True(ScrollViewer.GetCanContentScroll(grid), "the inbox keeps logical item scrolling");
                Assert.Equal(ScrollUnit.Item, VirtualizingPanel.GetScrollUnit(grid));
                Assert.Equal(VirtualizationMode.Standard, VirtualizingPanel.GetVirtualizationMode(grid));
                var realizedCount = FindVisualChildren<DataGridRow>(grid).Count();
                observations.Add($"initialWindowDip={window.ActualWidth:0.##}x{window.ActualHeight:0.##},viewDip={view.ActualWidth:0.##}x{view.ActualHeight:0.##},"
                    + $"gridDip={grid.ActualWidth:0.##}x{grid.ActualHeight:0.##},gridHeight={grid.Height:0.##},gridMaxHeight={grid.MaxHeight:0.##},"
                    + $"innerActual={internalViewer.ActualWidth:0.##}x{internalViewer.ActualHeight:0.##},canScroll={internalViewer.CanContentScroll},"
                    + $"innerViewport={internalViewer.ViewportHeight:0.##},innerExtent={internalViewer.ExtentHeight:0.##},"
                    + $"isVirtualizing={VirtualizingPanel.GetIsVirtualizing(grid)},realized={realizedCount}");
                observations.AddRange(FindVisualChildren<FrameworkElement>(grid)
                    .Where(element => element is ScrollViewer or ScrollContentPresenter or DataGridColumnHeadersPresenter or DataGridRowsPresenter or ItemsPresenter)
                    .Select(element => DescribeLayoutElement(element)));
                observations.Add(DataGridScrollDiagnostics.CaptureNow(grid, "virtualization:initial"));
                Assert.True(realizedCount < 40,
                    $"2,000 synthetic items should remain virtualized after finite layout; realized={realizedCount},grid={grid.ActualHeight:0.##}/{grid.Height:0.##},viewport={internalViewer.ViewportHeight:0.##}");

                CaptureAndAssert("top:initial");
                for (var cycle = 0; cycle < 3; cycle++)
                {
                    internalViewer.ScrollToVerticalOffset(internalViewer.ScrollableHeight * 0.48);
                    DrainDispatcher(window);
                    CaptureAndAssert($"middle:{cycle + 1}");
                    if (cycle == 0)
                        AppendPageAndRestoreAnchor();

                    internalViewer.ScrollToVerticalOffset(internalViewer.ScrollableHeight * 0.78);
                    DrainDispatcher(window);
                    CaptureAndAssert($"three-quarter:{cycle + 1}");

                    ScrollThumbToEnd(internalViewer);
                    DrainDispatcher(window);
                    CaptureAndAssert($"thumb-end:{cycle + 1}");
                    AssertLastLoadedRowComplete(grid, items.Count - 1);

                    internalViewer.ScrollToTop();
                    DrainDispatcher(window);
                    CaptureAndAssert($"top:return:{cycle + 1}");
                }

                window.Width = 920;
                window.Height = 640;
                view.ApplyResponsiveLayout(view.ActualWidth, view.ActualHeight);
                DrainDispatcher(window);
                CaptureAndAssert("resize:compact-height");
                Assert.Equal(ScrollBarVisibility.Auto, pageScroller.VerticalScrollBarVisibility);
                AssertFooterActionReachable(view, pageScroller, window);

                window.Width = 820;
                window.Height = 680;
                view.ApplyResponsiveLayout(view.ActualWidth, view.ActualHeight);
                DrainDispatcher(window);
                internalViewer = FindInternalGridViewer(grid);
                CaptureAndAssert("resize:narrow");
                ScrollThumbToEnd(internalViewer);
                DrainDispatcher(window);
                CaptureAndAssert("resize:narrow-thumb-end");
                AssertLastLoadedRowComplete(grid, items.Count - 1);
                AssertFooterActionReachable(view, pageScroller, window);

                window.Width = 1120;
                window.Height = 860;
                view.ApplyResponsiveLayout(view.ActualWidth, view.ActualHeight);
                DrainDispatcher(window);
                internalViewer = FindInternalGridViewer(grid);
                internalViewer.ScrollToTop();
                DrainDispatcher(window);
                CaptureAndAssert("resize:restore");
                Assert.True(grid.ActualHeight > 0 && grid.ActualHeight <= grid.MaxHeight + 0.5,
                    $"resized inbox remains finite: height={grid.ActualHeight:0.##}, max={grid.MaxHeight:0.##}");
            }
            catch (Exception caught)
            {
                exception = caught;
            }
            finally
            {
                window?.Close();
            }

            void CaptureAndAssert(string checkpoint)
            {
                DrainDispatcher(window!);
                var viewer = FindInternalGridViewer(grid);
                var metrics = ReadGeometry(grid, viewer);
                var realizedRows = FindVisualChildren<DataGridRow>(grid).Count();
                var dpi = VisualTreeHelper.GetDpi(grid);
                var outputPixelScale = outputScale * dpi.DpiScaleY;
                observations.Add($"checkpoint={checkpoint},scale={outputScale:0.##},actualDpi={dpi.DpiScaleX:0.##}x{dpi.DpiScaleY:0.##},"
                    + $"headerBottomGrid={metrics.HeaderBottom:0.##},presenterTopGrid={metrics.PresenterTop:0.##},firstRowTopGrid={metrics.FirstRowTop:0.##},"
                    + $"pageOffset={pageScroller.VerticalOffset:0.##},gridHeight={grid.ActualHeight:0.##},realized={realizedRows}");
                var diagnostic = DataGridScrollDiagnostics.CaptureNow(grid, checkpoint);
                observations.Add(diagnostic);
                Assert.Contains("gridGeometryDip=headerBottom:", diagnostic, StringComparison.Ordinal);
                Assert.Contains("outerPage=", diagnostic, StringComparison.Ordinal);
                Assert.Contains("build=GameSaveCenter.Playnite.dll:", diagnostic, StringComparison.Ordinal);
                Assert.Contains("mvid=", diagnostic, StringComparison.Ordinal);
                Assert.Contains("assemblyPath=", diagnostic, StringComparison.Ordinal);
                Assert.Contains("windowDip=", diagnostic, StringComparison.Ordinal);
                Assert.Contains("dpi=", diagnostic, StringComparison.Ordinal);

                Assert.True(Math.Abs(metrics.PresenterTop - metrics.HeaderBottom) <= 1.5,
                    $"content presenter must start below the column header without an added gap at {checkpoint}: {metrics}");
                Assert.True(realizedRows < 40,
                    $"the 2,000-item inbox remains bounded through {checkpoint}; realized={realizedRows}");
                Assert.True(metrics.HasVisibleRow, $"expected a visible row at {checkpoint}: {metrics}");
                var pixelGap = Math.Abs(metrics.FirstRowTop - metrics.PresenterTop) * outputPixelScale;
                Assert.True(pixelGap <= 1.5,
                    $"first visible row must touch the effective content viewport at {checkpoint}; device-scale gap={pixelGap:0.##}px, {metrics}");
            }

            void AppendPageAndRestoreAnchor()
            {
                var viewType = typeof(MediaCenterView);
                var anchor = viewType.GetMethod("CaptureAnchor", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!
                    .Invoke(null, new object[] { grid });
                Assert.NotNull(anchor);
                var anchorType = anchor!.GetType();
                var anchorId = (string)anchorType.GetProperty("ItemId")!.GetValue(anchor)!;
                var anchorTop = (double)anchorType.GetProperty("RelativeTop")!.GetValue(anchor)!;
                var originalIndex = items.ToList().FindIndex(item => item.MediaId == anchorId);
                Assert.True(originalIndex >= 0, $"captured production anchor remains in the retained prefix: {anchorId}");

                var accumulator = new MediaPageAccumulator(items, capacity: 2000);
                Assert.True(accumulator.AppendPage(CreateSyntheticMedia(50, startIndex: 2000), selectedId: null));
                Assert.Equal(2000, items.Count);
                Assert.Equal("synthetic-media-00050", items[0].MediaId);
                var shiftedIndex = items.ToList().FindIndex(item => item.MediaId == anchorId);
                Assert.Equal(originalIndex - 50, shiftedIndex);

                var generation = (long)viewType.GetField("anchorRestoreGeneration", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                    .GetValue(view)!;
                viewType.GetMethod("QueueRestore", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                    .Invoke(view, new object?[]
                    {
                        grid,
                        anchor,
                        new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                        true,
                        null,
                        generation
                    });
                DrainDispatcher(window!);
                CaptureAndAssert("page-append:anchor-restored");

                var restoredRow = grid.ItemContainerGenerator.ContainerFromIndex(shiftedIndex) as DataGridRow;
                Assert.NotNull(restoredRow);
                var viewer = FindInternalGridViewer(grid);
                var restoredTop = GetRect(restoredRow!, viewer).Top;
                Assert.True(Math.Abs(restoredTop - anchorTop) <= 1.5,
                    $"the production anchor keeps its viewport position after a page append shifts item indices; before={anchorTop:0.##},after={restoredTop:0.##},index={originalIndex}->{shiftedIndex}");
            }
        });

        foreach (var observation in observations)
            output.WriteLine(observation);
        Assert.Null(exception);

        static void AssertFooterActionReachable(MediaCenterView view, ScrollViewer pageScroller, Window window)
        {
            pageScroller.ScrollToVerticalOffset(pageScroller.ScrollableHeight);
            DrainDispatcher(window);
            var footerAction = FindVisualChildren<Button>(view)
                .FirstOrDefault(button => AutomationProperties.GetName(button) == "忽略所选媒体");
            Assert.NotNull(footerAction);
            Assert.True(footerAction!.IsVisible, "the footer ignore action remains visible after page scrolling");

            var pagePresenter = FindVisualChildren<ScrollContentPresenter>(pageScroller).FirstOrDefault();
            Assert.NotNull(pagePresenter);
            var actionRect = GetRect(footerAction, pageScroller);
            var viewportRect = GetRect(pagePresenter!, pageScroller);
            Assert.True(actionRect.Top >= viewportRect.Top - 1 && actionRect.Bottom <= viewportRect.Bottom + 1,
                $"footer action must be fully reachable at page end; action={actionRect},viewport={viewportRect},offset={pageScroller.VerticalOffset:0.##}/{pageScroller.ScrollableHeight:0.##}");
        }
    }

    private static void AssertLastLoadedRowComplete(DataGrid grid, int lastIndex)
    {
        var row = grid.ItemContainerGenerator.ContainerFromIndex(lastIndex) as DataGridRow;
        Assert.NotNull(row);
        var viewer = FindInternalGridViewer(grid);
        var presenter = FindVisualChildren<ScrollContentPresenter>(viewer).FirstOrDefault();
        Assert.NotNull(presenter);
        var rowRect = GetRect(row!, viewer);
        var viewport = GetRect(presenter!, viewer);
        Assert.True(rowRect.Top >= viewport.Top - 1 && rowRect.Bottom <= viewport.Bottom + 1,
            $"the final media item must be a complete row at the internal grid end; row={rowRect},viewport={viewport}");
        Assert.Equal(grid.Items.Count - 1, row!.GetIndex());
    }

    private static GeometrySnapshot ReadGeometry(DataGrid grid, ScrollViewer viewer)
    {
        var headers = FindVisualChildren<DataGridColumnHeadersPresenter>(grid).FirstOrDefault();
        var presenter = FindVisualChildren<ScrollContentPresenter>(viewer).FirstOrDefault();
        Assert.NotNull(headers);
        Assert.NotNull(presenter);
        var firstRow = FindVisualChildren<DataGridRow>(grid)
            .Where(row => row.Visibility == Visibility.Visible && row.ActualHeight > 0)
            .Select(row => (Row: row, Bounds: GetRect(row, presenter)))
            .Where(candidate => candidate.Bounds.Bottom > 0 && candidate.Bounds.Top < presenter.ActualHeight)
            .OrderBy(candidate => candidate.Bounds.Top)
            .FirstOrDefault();
        Assert.NotNull(firstRow.Row);
        var headerRect = GetRect(headers, grid);
        var presenterRect = GetRect(presenter, grid);
        var rowRect = GetRect(firstRow.Row!, grid);
        return new GeometrySnapshot(headerRect.Bottom, presenterRect.Top, rowRect.Top, true);
    }

    private static ScrollViewer FindInternalGridViewer(DataGrid grid)
        => FindVisualChildren<ScrollViewer>(grid)
            .OrderByDescending(candidate => FindVisualChildren<DataGridRowsPresenter>(candidate).Any())
            .First();

    private static string DescribeLayoutElement(FrameworkElement element)
    {
        var parts = new List<string>
        {
            $"visual={element.GetType().Name}",
            $"name={element.Name}",
            $"actual={element.ActualWidth:0.##}x{element.ActualHeight:0.##}",
            $"desired={element.DesiredSize.Width:0.##}x{element.DesiredSize.Height:0.##}",
            $"align={element.VerticalAlignment}",
            $"parent={VisualTreeHelper.GetParent(element)?.GetType().Name}"
        };

        if (element is ScrollViewer viewer)
        {
            var ownerName = viewer.Content is ItemsPresenter itemsPresenter && itemsPresenter.TemplatedParent is FrameworkElement owner
                ? owner.GetType().Name
                : "none";
            var viewerScrollInfo = typeof(ScrollViewer).GetProperty("ScrollInfo", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)?.GetValue(viewer);
            var scrollInfoName = viewerScrollInfo == null ? "none" : viewerScrollInfo.GetType().Name;
            parts.Add($"loaded={viewer.IsLoaded},content={viewer.Content?.GetType().Name},owner={ownerName},scrollInfo={scrollInfoName},viewport={viewer.ViewportHeight:0.##},extent={viewer.ExtentHeight:0.##}");
        }
        if (element is ItemsPresenter presenter)
            parts.Add($"loaded={presenter.IsLoaded},templatedParent={presenter.TemplatedParent?.GetType().Name},children={VisualTreeHelper.GetChildrenCount(presenter)}");
        if (element is DataGridRowsPresenter rows)
        {
            var scrollInfo = (IScrollInfo)rows;
            var scrollOwnerName = scrollInfo.ScrollOwner == null ? "none" : scrollInfo.ScrollOwner.GetType().Name;
            parts.Add($"loaded={rows.IsLoaded},itemsHost={rows.IsItemsHost},virtualizing={VirtualizingPanel.GetIsVirtualizing(rows)},children={VisualTreeHelper.GetChildrenCount(rows)},owner={scrollOwnerName},viewport={scrollInfo.ViewportHeight:0.##},extent={scrollInfo.ExtentHeight:0.##}");
        }

        return string.Join(",", parts);
    }

    private static void ScrollThumbToEnd(ScrollViewer viewer)
    {
        var bar = FindVisualChildren<ScrollBar>(viewer)
            .First(candidate => candidate.Orientation == Orientation.Vertical && candidate.Visibility == Visibility.Visible);
        var args = new ScrollEventArgs(ScrollEventType.ThumbPosition, bar.Maximum)
        {
            RoutedEvent = ScrollBar.ScrollEvent,
            Source = bar
        };
        bar.RaiseEvent(args);
        DrainDispatcher(viewer);
        if (viewer.VerticalOffset < viewer.ScrollableHeight - 0.5)
            viewer.ScrollToEnd();
    }

    private static MediaItemDto[] CreateSyntheticMedia(int count, int startIndex = 0)
        => Enumerable.Range(startIndex, count)
            .Select(index => new MediaItemDto
            {
                MediaId = $"synthetic-media-{index:D5}",
                OriginalPath = $@"C:\SyntheticMedia\screenshot-{index:D5}.png",
                ArchivePath = $@"C:\SyntheticMedia\archive-{index:D5}.zip",
                CapturedUtc = DateTime.UtcNow.AddMinutes(-index),
                Kind = MediaKind.Screenshot,
                Source = MediaSourceKind.WindowsScreenshot,
                ClassificationState = "Inbox",
                ClassificationReason = "synthetic scroll fixture"
            })
            .ToArray();

    private static Rect GetRect(FrameworkElement element, Visual ancestor)
        => element.TransformToAncestor(ancestor).TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root) where T : DependencyObject
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

    private static void DrainDispatcher(DispatcherObject dispatcherObject)
    {
        var dispatcher = dispatcherObject.Dispatcher;
        var frame = new DispatcherFrame();
        dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
        if (dispatcherObject is FrameworkElement element)
            element.UpdateLayout();
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

    private sealed class GeometrySnapshot
    {
        public GeometrySnapshot(double headerBottom, double presenterTop, double firstRowTop, bool hasVisibleRow)
        {
            HeaderBottom = headerBottom;
            PresenterTop = presenterTop;
            FirstRowTop = firstRowTop;
            HasVisibleRow = hasVisibleRow;
        }

        public double HeaderBottom { get; }
        public double PresenterTop { get; }
        public double FirstRowTop { get; }
        public bool HasVisibleRow { get; }

        public override string ToString()
            => $"headerBottom={HeaderBottom:0.##},presenterTop={PresenterTop:0.##},firstRowTop={FirstRowTop:0.##},hasRow={HasVisibleRow}";
    }
}
