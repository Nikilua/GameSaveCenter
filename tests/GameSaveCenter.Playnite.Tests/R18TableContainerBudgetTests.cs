using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
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

[Collection("R18TableWpf")]
public sealed class R18TableContainerBudgetTests
{
    private readonly ITestOutputHelper output;

    public R18TableContainerBudgetTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void ProductionTablesStayViewportBoundedAcrossLargeSyntheticDatasets()
    {
        AssertAssemblyIdentityAndContracts();

        Exception? exception = null;
        var probes = new List<TableProbeResult>();
        var thread = new Thread(() =>
        {
            try
            {
                foreach (var backendCount in new[] { 2_000, 10_000, 20_000 })
                {
                    probes.Add(ProbeTaskGrid(backendCount));
                    probes.Add(ProbeMediaInboxGrid(backendCount));
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
        Assert.Equal(6, probes.Count);

        foreach (var probe in probes)
        {
            output.WriteLine(probe.ToString());
            Assert.True(probe.ScrollableHeight > 0, $"{probe.Kind} did not expose a scrollable viewport");
            Assert.True(probe.MaxVisible > 0, $"{probe.Kind} had no visible rows");
            Assert.True(
                probe.MaxRealized <= probe.ViewportRows + 32,
                $"{probe.Kind} realized {probe.MaxRealized} containers for a {probe.ViewportRows}-row viewport");
            Assert.True(
                probe.MaxRealized < probe.UiItemCount,
                $"{probe.Kind} realized the complete {probe.UiItemCount}-item UI source");
            Assert.True(probe.P95ScrollMs < 2_000, $"{probe.Kind} scroll p95 was {probe.P95ScrollMs:0.###} ms");
            Assert.True(probe.MaxScrollMs < 5_000, $"{probe.Kind} scroll max was {probe.MaxScrollMs:0.###} ms");
        }

        var media = probes.Where(probe => probe.Kind == "Media-Inbox").ToArray();
        Assert.All(media, probe => Assert.Equal(2_000, probe.UiItemCount));
        Assert.All(media, probe => Assert.Equal("Standard", probe.VirtualizationMode));
        Assert.All(media, probe => Assert.Equal("Item", probe.ScrollUnit));
        Assert.All(media, probe => Assert.False(probe.ColumnVirtualization));

        var task = probes.Where(probe => probe.Kind == "Task").ToArray();
        Assert.All(task, probe => Assert.True(probe.ColumnVirtualization));
        Assert.All(task, probe => Assert.Equal("Recycling", probe.VirtualizationMode));
        Assert.All(task, probe => Assert.Equal("Item", probe.ScrollUnit));
    }

    private static void AssertAssemblyIdentityAndContracts()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var media = Read("src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml");
        var mediaCodeBehind = Read("src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml.cs");
        var task = Read("src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml");
        var accumulator = Read("src", "GameSaveCenter.Playnite", "Infrastructure", "MediaPageAccumulator.cs");

        Assert.Contains("EnableRowVirtualization\" Value=\"True\"", media);
        Assert.Contains("VirtualizingPanel.ScrollUnit\" Value=\"Item\"", media);
        Assert.Contains("VirtualizingPanel.VirtualizationMode\" Value=\"Standard\"", media);
        Assert.Contains("EnableColumnVirtualization\" Value=\"False\"", media);
        Assert.Contains("ScrollViewer.CanContentScroll\" Value=\"True\"", media);
        Assert.Contains("TaskGrid", task);
        Assert.Contains("MediaInboxGrid", media);
        Assert.Contains("public const int DefaultCapacity = 2000", accumulator);
        Assert.Contains("var finiteInboxHeight = Math.Max(readableGridHeight, height - inboxNonTableHeightBudget);", mediaCodeBehind);
        Assert.Contains("MediaInboxGrid.Height = finiteInboxHeight", mediaCodeBehind);
        Assert.DoesNotContain("Math.Max(readableGridHeight, Math.Max(1d, height))", mediaCodeBehind);
        Assert.Contains("MediaInboxPageScrollViewer.VerticalScrollBarVisibility = useInboxPageFallbackScroll", mediaCodeBehind);
    }

    private static TableProbeResult ProbeTaskGrid(int backendCount)
    {
        var view = new TaskCenterView();
        var grid = (DataGrid)typeof(TaskCenterView)
            .GetField("TaskGrid", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(view)!;
        grid.ItemsSource = CreateTasks(backendCount);
        return ProbeGrid("Task", backendCount, backendCount, view, grid, 1_100, 640);
    }

    private static TableProbeResult ProbeMediaInboxGrid(int backendCount)
    {
        var view = new MediaCenterView();
        var grid = (DataGrid)typeof(MediaCenterView)
            .GetField("MediaInboxGrid", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(view)!;
        var retained = new BatchObservableCollection<MediaItemDto>();
        var accumulator = new MediaPageAccumulator(retained);
        for (var start = 0; start < backendCount; start += 200)
        {
            var page = Enumerable.Range(start, Math.Min(200, backendCount - start))
                .Select(index => CreateMedia(index))
                .ToArray();
            if (start == 0)
                accumulator.ReplaceFirstPage(page, null);
            else
                accumulator.AppendPage(page, null);
        }

        grid.ItemsSource = retained;
        return ProbeGrid("Media-Inbox", backendCount, retained.Count, view, grid, 1_280, 720);
    }

    private static TableProbeResult ProbeGrid(
        string kind,
        int backendCount,
        int uiItemCount,
        UserControl view,
        DataGrid grid,
        double width,
        double height)
    {
        Window? window = null;
        try
        {
            window = new Window
            {
                Content = view,
                Width = width,
                Height = height,
                ShowInTaskbar = false,
                ShowActivated = false,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                Opacity = 0.01
            };
            if (view is TaskCenterView taskView)
                taskView.ApplyResponsiveLayout(width, height);
            else if (view is MediaCenterView mediaView)
                mediaView.ApplyResponsiveLayout(width, height);
            window.Show();
            FlushLayout(window);

            var scroller = FindVisualChildren<ScrollViewer>(grid)
                .OrderByDescending(candidate => candidate.ViewportHeight)
                .FirstOrDefault();
            if (scroller == null)
                throw new InvalidOperationException(kind + " has no internal ScrollViewer.");

            var initialRows = FindRealizedRows(grid);
            var rowHeight = initialRows.Select(row => row.ActualHeight).FirstOrDefault(value => value > 0);
            if (rowHeight <= 0)
                rowHeight = double.IsNaN(grid.RowHeight) ? 1 : grid.RowHeight;
            var presenterRect = GetPresenterRect(scroller);
            var viewportRows = Math.Max(1, (int)Math.Ceiling(presenterRect.Height / Math.Max(1, rowHeight)));
            var realizedSamples = new List<int>();
            var visibleSamples = new List<int>();
            var scrollMs = new List<double>();
            var fractions = new[] { 0d, 0.25d, 0.5d, 0.75d, 1d, 0d, 1d, 0.5d };
            foreach (var fraction in fractions)
            {
                var timer = Stopwatch.StartNew();
                scroller.ScrollToVerticalOffset(scroller.ScrollableHeight * fraction);
                FlushLayout(window);
                timer.Stop();
                var rows = FindRealizedRows(grid);
                realizedSamples.Add(rows.Length);
                visibleSamples.Add(CountVisibleRows(grid, scroller));
                scrollMs.Add(timer.Elapsed.TotalMilliseconds);
            }

            var mode = VirtualizingPanel.GetVirtualizationMode(grid).ToString();
            var scrollUnit = VirtualizingPanel.GetScrollUnit(grid).ToString();
            var rowsPanel = FindVisualChildren<VirtualizingStackPanel>(grid).FirstOrDefault();
            return new TableProbeResult(
                kind,
                backendCount,
                uiItemCount,
                grid.EnableColumnVirtualization,
                mode,
                scrollUnit,
                scroller.CanContentScroll,
                scroller.ScrollableHeight,
                viewportRows,
                realizedSamples.Max(),
                visibleSamples.Max(),
                scrollMs,
                rowsPanel != null && VirtualizingPanel.GetIsVirtualizing(rowsPanel),
                rowsPanel == null ? 0 : VisualTreeHelper.GetChildrenCount(rowsPanel));
        }
        finally
        {
            window?.Close();
        }
    }

    private static int CountVisibleRows(DataGrid grid, ScrollViewer scroller)
    {
        var presenterRect = GetPresenterRect(scroller);
        var rows = FindRealizedRows(grid)
            .Select(row => row.TransformToAncestor(scroller).TransformBounds(
                new Rect(0, 0, row.ActualWidth, row.ActualHeight)))
            .ToArray();
        return rows.Count(rect => rect.Bottom > presenterRect.Top && rect.Top < presenterRect.Bottom);
    }

    private static DataGridRow[] FindRealizedRows(DataGrid grid)
        => FindVisualChildren<DataGridRow>(grid)
            .Where(row => row.Visibility == Visibility.Visible && row.ActualHeight > 0)
            .ToArray();

    private static Rect GetPresenterRect(ScrollViewer scroller)
    {
        var presenter = FindVisualChildren<ScrollContentPresenter>(scroller).FirstOrDefault();
        return presenter == null
            ? new Rect(0, 0, scroller.ViewportWidth, scroller.ViewportHeight)
            : presenter.TransformToAncestor(scroller).TransformBounds(
                new Rect(0, 0, presenter.ActualWidth, presenter.ActualHeight));
    }

    private static TaskStatusDto[] CreateTasks(int count)
        => Enumerable.Range(0, count)
            .Select(index => new TaskStatusDto
            {
                TaskId = "r18-task-" + index,
                TaskType = "Backup",
                GameId = "game-" + (index % 17),
                GameName = "合成游戏 " + (index % 17),
                State = TaskState.Succeeded,
                ProgressPercent = 100,
                CreatedUtc = DateTime.UtcNow.AddSeconds(-index),
                Message = "R18 表格容器预算"
            })
            .ToArray();

    private static MediaItemDto CreateMedia(int index)
        => new MediaItemDto
        {
            MediaId = "r18-media-" + index,
            Kind = MediaKind.Screenshot,
            Source = MediaSourceKind.WindowsScreenshot,
            ArchivePath = @"D:\R18\archive-" + index + ".png",
            OriginalPath = @"D:\R18\source-" + index + ".png",
            Sha256 = "r18-sha-" + index,
            CapturedUtc = DateTime.UtcNow.AddSeconds(-index),
            ClassificationState = "Inbox",
            ClassificationReason = "合成数据"
        };

    private static void FlushLayout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(window.UpdateLayout));
        window.UpdateLayout();
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

    private static string Read(params string[] parts)
        => File.ReadAllText(Path.Combine(new[] { TestRepositoryContext.Root }.Concat(parts).ToArray()));

    private sealed class TableProbeResult
    {
        public TableProbeResult(
            string kind,
            int backendCount,
            int uiItemCount,
            bool columnVirtualization,
            string virtualizationMode,
            string scrollUnit,
            bool canContentScroll,
            double scrollableHeight,
            int viewportRows,
            int maxRealized,
            int maxVisible,
            IReadOnlyList<double> scrollMs,
            bool rowsPanelVirtualizing,
            int rowsPanelChildren)
        {
            Kind = kind;
            BackendCount = backendCount;
            UiItemCount = uiItemCount;
            ColumnVirtualization = columnVirtualization;
            VirtualizationMode = virtualizationMode;
            ScrollUnit = scrollUnit;
            CanContentScroll = canContentScroll;
            ScrollableHeight = scrollableHeight;
            ViewportRows = viewportRows;
            MaxRealized = maxRealized;
            MaxVisible = maxVisible;
            ScrollSamples = scrollMs;
            RowsPanelVirtualizing = rowsPanelVirtualizing;
            RowsPanelChildren = rowsPanelChildren;
        }

        public string Kind { get; }
        public int BackendCount { get; }
        public int UiItemCount { get; }
        public bool ColumnVirtualization { get; }
        public string VirtualizationMode { get; }
        public string ScrollUnit { get; }
        public bool CanContentScroll { get; }
        public double ScrollableHeight { get; }
        public int ViewportRows { get; }
        public int MaxRealized { get; }
        public int MaxVisible { get; }
        public bool RowsPanelVirtualizing { get; }
        public int RowsPanelChildren { get; }
        public IReadOnlyList<double> ScrollSamples { get; }
        public double P95ScrollMs => Percentile(ScrollSamples, 0.95);
        public double MaxScrollMs => ScrollSamples.Count == 0 ? 0 : ScrollSamples.Max();

        public override string ToString()
            => $"R18-04 {Kind} backend={BackendCount} ui={UiItemCount} mode={VirtualizationMode} "
                + $"unit={ScrollUnit} canContentScroll={CanContentScroll} columnVirtualization={ColumnVirtualization} "
                + $"viewportRows={ViewportRows} maxRealized={MaxRealized} maxVisible={MaxVisible} "
                + $"rowsPanelVirtualizing={RowsPanelVirtualizing} rowsPanelChildren={RowsPanelChildren} "
                + $"scrollable={ScrollableHeight:0.##} p95_ms={P95ScrollMs:0.###} max_ms={MaxScrollMs:0.###} "
                + $"scroll_ms={string.Join(",", ScrollSamples.Select(value => value.ToString("0.###")))}";

        private static double Percentile(IReadOnlyList<double> values, double percentile)
        {
            if (values.Count == 0) return 0;
            var ordered = values.OrderBy(value => value).ToArray();
            var index = Math.Max(0, (int)Math.Ceiling(ordered.Length * percentile) - 1);
            return ordered[index];
        }
    }
}

[CollectionDefinition("R18TableWpf", DisableParallelization = true)]
public sealed class R18TableWpfCollection
{
}
