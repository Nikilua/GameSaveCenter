using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Playnite.SDK;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Developer diagnostics for the two large production DataGrids. It observes the
    /// actual visual tree and scroll contract without reading row/file contents. The
    /// behavior is intentionally passive: it never refreshes, rebinds, scrolls, or
    /// changes virtualization settings.
    /// </summary>
    public static class DataGridScrollDiagnostics
    {
        private static readonly ILogger Logger = LogManager.GetLogger();
        private static readonly DependencyProperty StateProperty = DependencyProperty.RegisterAttached(
            "State",
            typeof(DiagnosticState),
            typeof(DataGridScrollDiagnostics),
            new PropertyMetadata(null));

        public static void Attach(DataGrid grid, string stableName, Func<string>? contextProvider = null)
        {
            if (grid == null || string.IsNullOrWhiteSpace(stableName))
                return;

            if (grid.GetValue(StateProperty) is DiagnosticState existing)
            {
                existing.ContextProvider = contextProvider;
                return;
            }

            var state = new DiagnosticState(grid, stableName, contextProvider);
            grid.SetValue(StateProperty, state);
            grid.Loaded += state.OnLoaded;
            grid.Unloaded += state.OnUnloaded;
            grid.SizeChanged += state.OnSizeChanged;
            grid.LayoutUpdated += state.OnLayoutUpdated;
            grid.SelectionChanged += state.OnSelectionChanged;
            grid.AddHandler(UIElement.PreviewMouseWheelEvent, new MouseWheelEventHandler(state.OnPreviewMouseWheel), true);
            grid.AddHandler(UIElement.PreviewMouseDownEvent, new MouseButtonEventHandler(state.OnPreviewMouseDown), true);
            grid.AddHandler(UIElement.PreviewMouseUpEvent, new MouseButtonEventHandler(state.OnPreviewMouseUp), true);
            grid.AddHandler(UIElement.PreviewKeyDownEvent, new KeyEventHandler(state.OnPreviewKeyDown), true);
            grid.AddHandler(FrameworkElement.RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler(state.OnRequestBringIntoView), true);
            if (grid.Items is INotifyCollectionChanged changedItems)
                changedItems.CollectionChanged += state.OnCollectionChanged;
            if (grid.IsLoaded)
                state.AttachInternalScroller();
        }

        /// <summary>
        /// Marks the next geometry snapshot with an application-level trigger such as
        /// “加载更多”. This is only diagnostic context; it does not perform any action.
        /// </summary>
        public static void MarkTrigger(DataGrid grid, string trigger)
        {
            if (grid?.GetValue(StateProperty) is DiagnosticState state)
                state.MarkTrigger(trigger);
        }

        /// <summary>Captures one immediate snapshot for an offline harness or audit.</summary>
        public static string CaptureNow(DataGrid grid, string trigger)
        {
            if (grid?.GetValue(StateProperty) is DiagnosticState state)
                return state.Capture(trigger, force: true);
            return $"[GSC-GRID-DIAGNOSTIC] grid={grid?.Name ?? "unknown"} trigger={trigger} state=not-attached";
        }

        private sealed class DiagnosticState
        {
            private readonly DataGrid grid;
            private readonly string stableName;
            private ScrollViewer? scrollViewer;
            private string? lastSignature;
            private string pendingTrigger = "布局";
            private bool scrollViewerSubscribed;
            private bool isUnloaded;
            private bool logNextLayout;

            internal DiagnosticState(DataGrid grid, string stableName, Func<string>? contextProvider)
            {
                this.grid = grid;
                this.stableName = stableName;
                ContextProvider = contextProvider;
            }

            internal Func<string>? ContextProvider { get; set; }

            internal void OnLoaded(object sender, RoutedEventArgs e)
            {
                isUnloaded = false;
                AttachInternalScroller();
                MarkTrigger("加载");
                Capture("加载", force: true);
            }

            internal void OnUnloaded(object sender, RoutedEventArgs e)
            {
                isUnloaded = true;
                DetachInternalScroller();
                MarkTrigger("卸载");
                Capture("卸载", force: true);
            }

            internal void OnSizeChanged(object sender, SizeChangedEventArgs e)
            {
                MarkTrigger("尺寸变化");
                Capture("尺寸变化", force: true);
            }

            internal void OnLayoutUpdated(object? sender, EventArgs e)
            {
                if (isUnloaded || !grid.IsLoaded)
                    return;

                var snapshot = BuildSnapshot(pendingTrigger);
                if (snapshot.IsAnomaly || logNextLayout || snapshot.Signature != lastSignature)
                {
                    logNextLayout = false;
                    LogSnapshot(snapshot);
                    lastSignature = snapshot.Signature;
                    pendingTrigger = "布局";
                }
            }

            internal void OnScrollViewerChanged(object sender, ScrollChangedEventArgs e)
            {
                var trigger = pendingTrigger == "布局" ? "滚动变更" : pendingTrigger;
                Capture(trigger, force: true);
            }

            internal void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                MarkTrigger("选择变化");
                Capture("选择变化", force: true);
            }

            internal void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            {
                MarkTrigger("集合刷新");
                Capture($"集合刷新:{e.Action}", force: true);
            }

            internal void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
            {
                MarkTrigger("滚轮");
            }

            internal void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
            {
                if (FindAncestor<ScrollBar>(e.OriginalSource as DependencyObject) != null)
                    MarkTrigger("拖动滑块");
            }

            internal void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
            {
                if (FindAncestor<ScrollBar>(e.OriginalSource as DependencyObject) != null)
                {
                    MarkTrigger("拖动滑块完成");
                    logNextLayout = true;
                }
            }

            internal void OnPreviewKeyDown(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.End && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    MarkTrigger("Ctrl+End");
                else if (e.Key == Key.PageDown)
                    MarkTrigger("PageDown");
                else if (e.Key == Key.PageUp)
                    MarkTrigger("PageUp");
                else if (e.Key == Key.Home && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                    MarkTrigger("Ctrl+Home");
            }

            internal void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
            {
                MarkTrigger("定位行");
            }

            internal void MarkTrigger(string trigger)
            {
                if (!string.IsNullOrWhiteSpace(trigger))
                    pendingTrigger = trigger;
            }

            internal void AttachInternalScroller()
            {
                if (scrollViewer != null && !ReferenceEquals(scrollViewer, FindInternalScroller()))
                    DetachInternalScroller();

                scrollViewer ??= FindInternalScroller();
                if (scrollViewer == null || scrollViewerSubscribed)
                    return;

                scrollViewer.ScrollChanged += OnScrollViewerChanged;
                scrollViewerSubscribed = true;
            }

            private void DetachInternalScroller()
            {
                if (scrollViewer != null && scrollViewerSubscribed)
                    scrollViewer.ScrollChanged -= OnScrollViewerChanged;
                scrollViewer = null;
                scrollViewerSubscribed = false;
            }

            internal string Capture(string trigger, bool force)
            {
                if (!string.IsNullOrWhiteSpace(trigger))
                    pendingTrigger = trigger;
                var snapshot = BuildSnapshot(pendingTrigger);
                if (force || snapshot.Signature != lastSignature || snapshot.IsAnomaly)
                {
                    LogSnapshot(snapshot);
                    lastSignature = snapshot.Signature;
                }
                pendingTrigger = "布局";
                return snapshot.ToLogLine();
            }

            private void LogSnapshot(GridSnapshot snapshot)
            {
                Logger.Info(snapshot.ToLogLine());
            }

            private GridSnapshot BuildSnapshot(string trigger)
            {
                AttachInternalScroller();
                var viewer = scrollViewer;
                var presenter = viewer == null ? null : FindDescendant<ScrollContentPresenter>(viewer);
                var rows = FindVisualChildren<DataGridRow>(grid)
                    .Where(row => row.Visibility == Visibility.Visible && row.ActualHeight > 0)
                    .Select(row => DescribeRow(row, presenter, viewer))
                    .OrderBy(row => row.Y)
                    .ToList();
                var visibleRows = rows.Where(row => row.IntersectsPresenter).ToList();
                var horizontalBar = viewer == null ? null : FindScrollBar(viewer, Orientation.Horizontal);
                var verticalBar = viewer == null ? null : FindScrollBar(viewer, Orientation.Vertical);
                var presenterRect = presenter == null || viewer == null ? Rect.Empty : GetRect(presenter, viewer);
                var hBarRect = horizontalBar == null || viewer == null ? Rect.Empty : GetRect(horizontalBar, viewer);
                var selectedRows = rows.Where(row => row.IsSelected).ToList();
                var visibleTextRows = visibleRows.Count(row => row.ContentVisualCount > 0);
                var blank = grid.Items.Count > 0 && visibleRows.Count > 0 && visibleTextRows == 0;
                var selectedContentMissing = selectedRows.Any(row => row.IntersectsPresenter
                    && (row.ContentVisualCount == 0
                        || (row.CellCount > 0 && row.ContentTextCount == 0)));
                var firstGap = visibleRows.Count == 0 ? double.NaN : visibleRows[0].Y - presenterRect.Top;
                var largeGap = visibleRows.Count > 0 && firstGap > Math.Max(32d, visibleRows[0].Height * 1.5d);
                var lastRowBottom = visibleRows.Count == 0 ? double.NaN : visibleRows[visibleRows.Count - 1].Bottom;
                var lastVisibleRow = visibleRows.LastOrDefault();
                var lastVisibleRowComplete = lastVisibleRow != null
                    && !presenterRect.IsEmpty
                    && lastVisibleRow.Bottom <= presenterRect.Bottom + 0.5d;
                var lastLoadedIndex = grid.Items.Count - 1;
                var lastLoadedRow = rows.FirstOrDefault(row => row.Index == lastLoadedIndex);
                var lastLoadedId = grid.Items.Count == 0
                    ? "none"
                    : GetStableId(grid.Items[lastLoadedIndex]);
                var lastLoadedRowComplete = lastLoadedRow != null
                    && !presenterRect.IsEmpty
                    && lastLoadedRow.Y >= presenterRect.Top - 0.5d
                    && lastLoadedRow.Bottom <= presenterRect.Bottom + 0.5d
                    && lastLoadedRow.CellCount > 0
                    && lastLoadedRow.ContentVisualCount == lastLoadedRow.CellCount
                    && lastLoadedRow.ContentTextCount > 0;
                var atVerticalEnd = viewer != null
                    && viewer.VerticalOffset >= viewer.ScrollableHeight - 0.5d;
                var lastLoadedRowIncomplete = atVerticalEnd
                    && grid.Items.Count > 0
                    && lastLoadedRow != null
                    && !lastLoadedRowComplete;
                var horizontalOverlap = horizontalBar != null
                    && horizontalBar.Visibility == Visibility.Visible
                    && !presenterRect.IsEmpty
                    && visibleRows.Any(row => Math.Min(row.Bottom, presenterRect.Bottom) > hBarRect.Top + 0.5d);
                var anomaly = blank || selectedContentMissing || largeGap || horizontalOverlap || lastLoadedRowIncomplete;
                var signature = string.Join("|", new[]
                {
                    grid.Items.Count.ToString(),
                    viewer == null ? "none" : viewer.VerticalOffset.ToString("0.##"),
                    viewer == null ? "none" : viewer.HorizontalOffset.ToString("0.##"),
                    visibleRows.Count.ToString(),
                    visibleRows.Count == 0 ? "none" : visibleRows[0].Index.ToString(),
                    visibleRows.Count == 0 ? "none" : visibleRows[0].Y.ToString("0.##"),
                    visibleTextRows.ToString(),
                    selectedContentMissing.ToString(),
                    horizontalOverlap.ToString()
                });

                var scrollInfoTypes = viewer == null
                    ? Array.Empty<string>()
                    : FindVisualDescendants(viewer)
                        .OfType<IScrollInfo>()
                        .Select(info => info.GetType().Name)
                        .Distinct(StringComparer.Ordinal)
                        .ToArray();

                return new GridSnapshot(
                    stableName,
                    trigger,
                    ContextProvider?.Invoke() ?? "context=unknown",
                    grid.Items.Count,
                    viewer,
                    presenter,
                    horizontalBar,
                    verticalBar,
                    rows,
                    visibleRows,
                    selectedRows,
                    visibleTextRows,
                    firstGap,
                    lastRowBottom,
                    presenterRect,
                    hBarRect,
                    blank,
                    selectedContentMissing,
                    largeGap,
                    horizontalOverlap,
                    lastVisibleRowComplete,
                    lastLoadedRow,
                    lastLoadedId,
                    lastLoadedRowComplete,
                    lastLoadedRowIncomplete,
                    anomaly,
                    signature,
                    VirtualizingPanel.GetScrollUnit(grid),
                    ScrollViewer.GetCanContentScroll(grid),
                    scrollInfoTypes);
            }

            private RowSnapshot DescribeRow(DataGridRow row, ScrollContentPresenter? presenter, ScrollViewer? viewer)
            {
                var viewportRect = presenter == null || viewer == null
                    ? Rect.Empty
                    : GetRect(presenter, viewer);
                var rowRect = viewer == null ? Rect.Empty : GetRect(row, viewer);
                var cellRows = FindVisualChildren<DataGridCell>(row).ToList();
                var contentVisualCount = cellRows.Count(cell => HasVisibleContent(cell));
                var contentTextCount = cellRows.Count(cell => HasVisibleText(cell));
                var clippedCells = cellRows.Count(cell => cell.Clip != null || cell.ClipToBounds);
                return new RowSnapshot(
                    row.GetIndex(),
                    GetStableId(row.Item),
                    rowRect.Top,
                    row.ActualHeight,
                    rowRect.Bottom,
                    row.IsSelected,
                    !viewportRect.IsEmpty && rowRect.Bottom > viewportRect.Top && rowRect.Top < viewportRect.Bottom,
                    contentVisualCount,
                    contentTextCount,
                    cellRows.Count,
                    clippedCells);
            }

            private static bool HasVisibleContent(DataGridCell cell)
            {
                if (cell.Visibility != Visibility.Visible || cell.ActualWidth <= 0 || cell.ActualHeight <= 0)
                    return false;
                return FindVisualChildren<FrameworkElement>(cell)
                    .Any(element => element != cell
                        && element.Visibility == Visibility.Visible
                        && element.ActualWidth > 0
                        && element.ActualHeight > 0);
            }

            private static bool HasVisibleText(DataGridCell cell)
                => FindVisualChildren<TextBlock>(cell).Any(text => text.Visibility == Visibility.Visible && !string.IsNullOrWhiteSpace(text.Text));

            private ScrollViewer? FindInternalScroller()
                => FindVisualChildren<ScrollViewer>(grid)
                    // A host template may expose more than one ScrollViewer. The
                    // responsible one is the viewer whose subtree owns the actual
                    // DataGridRowsPresenter; viewport size alone can select an outer
                    // page viewer and make the diagnostic offsets meaningless.
                    .OrderByDescending(viewer => FindDescendant<DataGridRowsPresenter>(viewer) != null)
                    .ThenByDescending(viewer => viewer.ViewportHeight)
                    .ThenByDescending(viewer => viewer.ViewportWidth)
                    .FirstOrDefault();

            private static ScrollBar? FindScrollBar(ScrollViewer viewer, Orientation orientation)
                => FindVisualChildren<ScrollBar>(viewer).FirstOrDefault(bar => bar.Orientation == orientation);
        }

        private sealed class GridSnapshot
        {
            internal GridSnapshot(
                string name,
                string trigger,
                string context,
                int itemCount,
                ScrollViewer? viewer,
                ScrollContentPresenter? presenter,
                ScrollBar? horizontalBar,
                ScrollBar? verticalBar,
                IReadOnlyList<RowSnapshot> rows,
                IReadOnlyList<RowSnapshot> visibleRows,
                IReadOnlyList<RowSnapshot> selectedRows,
                int visibleTextRows,
                double firstGap,
                double lastRowBottom,
                Rect presenterRect,
                Rect horizontalBarRect,
                bool blank,
                bool selectedContentMissing,
                bool largeGap,
                bool horizontalOverlap,
                bool lastVisibleRowComplete,
                RowSnapshot? lastLoadedRow,
                string lastLoadedId,
                bool lastLoadedRowComplete,
                bool lastLoadedRowIncomplete,
                bool anomaly,
                string signature,
                ScrollUnit scrollUnit,
                bool canContentScroll,
                IReadOnlyList<string> scrollInfoTypes)
            {
                Name = name;
                Trigger = trigger;
                Context = context;
                ItemCount = itemCount;
                Viewer = viewer;
                Presenter = presenter;
                HorizontalBar = horizontalBar;
                VerticalBar = verticalBar;
                Rows = rows;
                VisibleRows = visibleRows;
                SelectedRows = selectedRows;
                VisibleTextRows = visibleTextRows;
                FirstGap = firstGap;
                LastRowBottom = lastRowBottom;
                PresenterRect = presenterRect;
                HorizontalBarRect = horizontalBarRect;
                Blank = blank;
                SelectedContentMissing = selectedContentMissing;
                LargeGap = largeGap;
                HorizontalOverlap = horizontalOverlap;
                LastVisibleRowComplete = lastVisibleRowComplete;
                LastLoadedRow = lastLoadedRow;
                LastLoadedId = lastLoadedId;
                LastLoadedRowComplete = lastLoadedRowComplete;
                LastLoadedRowIncomplete = lastLoadedRowIncomplete;
                IsAnomaly = anomaly;
                Signature = signature;
                ScrollUnit = scrollUnit;
                CanContentScroll = canContentScroll;
                ScrollInfoTypes = scrollInfoTypes;
            }

            internal string Name { get; }
            internal string Trigger { get; }
            internal string Context { get; }
            internal int ItemCount { get; }
            internal ScrollViewer? Viewer { get; }
            internal ScrollContentPresenter? Presenter { get; }
            internal ScrollBar? HorizontalBar { get; }
            internal ScrollBar? VerticalBar { get; }
            internal IReadOnlyList<RowSnapshot> Rows { get; }
            internal IReadOnlyList<RowSnapshot> VisibleRows { get; }
            internal IReadOnlyList<RowSnapshot> SelectedRows { get; }
            internal int VisibleTextRows { get; }
            internal double FirstGap { get; }
            internal double LastRowBottom { get; }
            internal Rect PresenterRect { get; }
            internal Rect HorizontalBarRect { get; }
            internal bool Blank { get; }
            internal bool SelectedContentMissing { get; }
            internal bool LargeGap { get; }
            internal bool HorizontalOverlap { get; }
            internal bool LastVisibleRowComplete { get; }
            internal RowSnapshot? LastLoadedRow { get; }
            internal string LastLoadedId { get; }
            internal bool LastLoadedRowComplete { get; }
            internal bool LastLoadedRowIncomplete { get; }
            internal bool IsAnomaly { get; }
            internal string Signature { get; }
            internal ScrollUnit ScrollUnit { get; }
            internal bool CanContentScroll { get; }
            internal IReadOnlyList<string> ScrollInfoTypes { get; }

            internal string ToLogLine()
            {
                var first = VisibleRows.FirstOrDefault();
                var last = VisibleRows.LastOrDefault();
                var selected = SelectedRows.FirstOrDefault();
                var firstText = first == null ? "none" : $"{first.Index}:{first.Id}@{first.Y:0.##}/{first.Height:0.##}";
                var lastText = last == null ? "none" : $"{last.Index}:{last.Id}@{last.Y:0.##}/{last.Height:0.##}";
                var selectedText = selected == null
                    ? "none"
                    : $"{selected.Index}:{selected.Id},cells={selected.CellCount},visual={selected.ContentVisualCount},text={selected.ContentTextCount},clip={selected.ClippedCellCount}";
                var lastLoadedText = LastLoadedRow == null
                    ? (ItemCount == 0 ? "none" : $"{ItemCount - 1}:{LastLoadedId}:not-realized")
                    : $"{LastLoadedRow.Index}:{LastLoadedRow.Id}@{LastLoadedRow.Y:0.##}/{LastLoadedRow.Height:0.##},cells={LastLoadedRow.CellCount},visual={LastLoadedRow.ContentVisualCount},text={LastLoadedRow.ContentTextCount},clip={LastLoadedRow.ClippedCellCount}";
                var viewerText = Viewer == null
                    ? "none"
                    : $"{Viewer.GetType().Name},off={Viewer.VerticalOffset:0.##}/{Viewer.HorizontalOffset:0.##},viewport={Viewer.ViewportHeight:0.##}x{Viewer.ViewportWidth:0.##},extent={Viewer.ExtentHeight:0.##}x{Viewer.ExtentWidth:0.##},scrollable={Viewer.ScrollableHeight:0.##}x{Viewer.ScrollableWidth:0.##}";
                var presenterText = Presenter == null
                    ? "none"
                    : $"{Presenter.GetType().Name}@{PresenterRect.Left:0.##},{PresenterRect.Top:0.##},{PresenterRect.Width:0.##}x{PresenterRect.Height:0.##}";
                var horizontalText = HorizontalBar == null
                    ? "none"
                    : $"{HorizontalBar.Visibility},rect={HorizontalBarRect.Left:0.##},{HorizontalBarRect.Top:0.##},{HorizontalBarRect.Width:0.##}x{HorizontalBarRect.Height:0.##}";
                var verticalText = VerticalBar == null ? "none" : VerticalBar.Visibility.ToString();
                var scrollInfoType = ScrollInfoTypes.Count == 0 ? "none" : string.Join("|", ScrollInfoTypes);
                var state = IsAnomaly
                    ? $"anomaly=blank:{Blank},selectedMissing:{SelectedContentMissing},gap:{LargeGap},hOverlap:{HorizontalOverlap},lastLoadedIncomplete:{LastLoadedRowIncomplete}"
                    : "state=normal";
                return $"[GSC-GRID-DIAGNOSTIC] grid={Name} trigger={Trigger} items={ItemCount} context={Context} "
                    + $"scroller={viewerText},iscrollinfo={scrollInfoType},canContentScroll={CanContentScroll},scrollUnit={ScrollUnit}, "
                    + $"presenter={presenterText},hbar={horizontalText},vbar={verticalText},rows={VisibleRows.Count}/{Rows.Count},visibleTextRows={VisibleTextRows}, "
                    + $"first={firstText},last={lastText},lastLoaded={lastLoadedText},lastVisibleComplete={LastVisibleRowComplete},lastLoadedComplete={LastLoadedRowComplete},selected={selectedText},firstGap={FirstGap:0.##},lastBottom={LastRowBottom:0.##}, {state}";
            }

        }

        private sealed class RowSnapshot
        {
            internal RowSnapshot(int index, string id, double y, double height, double bottom, bool selected, bool intersectsPresenter, int contentVisualCount, int contentTextCount, int cellCount, int clippedCellCount)
            {
                Index = index;
                Id = id;
                Y = y;
                Height = height;
                Bottom = bottom;
                IsSelected = selected;
                IntersectsPresenter = intersectsPresenter;
                ContentVisualCount = contentVisualCount;
                ContentTextCount = contentTextCount;
                CellCount = cellCount;
                ClippedCellCount = clippedCellCount;
            }

            internal int Index { get; }
            internal string Id { get; }
            internal double Y { get; }
            internal double Height { get; }
            internal double Bottom { get; }
            internal bool IsSelected { get; }
            internal bool IntersectsPresenter { get; }
            internal int ContentVisualCount { get; }
            internal int ContentTextCount { get; }
            internal int CellCount { get; }
            internal int ClippedCellCount { get; }
        }

        private static T? FindDescendant<T>(DependencyObject root) where T : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T match)
                    return match;
                var nested = FindDescendant<T>(child);
                if (nested != null)
                    return nested;
            }
            return null;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root) where T : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T match)
                    yield return match;
                foreach (var nested in FindVisualChildren<T>(child))
                    yield return nested;
            }
        }

        private static IEnumerable<DependencyObject> FindVisualDescendants(DependencyObject root)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                yield return child;
                foreach (var nested in FindVisualDescendants(child))
                    yield return nested;
            }
        }

        private static Rect GetRect(FrameworkElement element, Visual ancestor)
        {
            try
            {
                return element.TransformToAncestor(ancestor).TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
            }
            catch
            {
                return Rect.Empty;
            }
        }

        private static T? FindAncestor<T>(DependencyObject? child) where T : DependencyObject
        {
            var current = child;
            while (current != null)
            {
                if (current is T match)
                    return match;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private static string GetStableId(object? item)
        {
            if (item == null)
                return "none";
            foreach (var propertyName in new[] { "MediaId", "TaskId", "EntryId", "Id" })
            {
                var property = item.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property == null)
                    continue;
                var value = property.GetValue(item, null)?.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                    return value!;
            }
            return "index-only";
        }
    }
}
