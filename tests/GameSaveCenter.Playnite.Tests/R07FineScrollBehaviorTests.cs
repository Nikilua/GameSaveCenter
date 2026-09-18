using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R07FineScrollBehaviorTests
{
    private readonly ITestOutputHelper output;

    public R07FineScrollBehaviorTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void FineReverseAndTerminalWheelSequenceKeepsViewportAndLayoutBounded()
    {
        Exception? exception = null;
        var smallOffsets = new List<double>();
        var reverseOffsets = new List<double>();
        var visibleCounts = new List<int>();
        var layoutCount = 0;
        var wheelCount = 0;
        var terminalOffset = 0d;
        var terminalLayoutDelta = 0;

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var grid = CreateGrid(160);
                window = CreateWindow(grid, 340, 230);
                window.Show();
                window.UpdateLayout();

                var gridScroller = FindVisualChildren<ScrollViewer>(grid).FirstOrDefault();
                if (gridScroller == null || gridScroller.ScrollableHeight <= 0.5)
                    throw new InvalidOperationException("DataGrid did not expose a scrollable internal viewport.");

                grid.LayoutUpdated += (_, _) => layoutCount++;
                gridScroller.ScrollToVerticalOffset(0);
                window.UpdateLayout();

                smallOffsets.Add(gridScroller.VerticalOffset);
                for (var index = 0; index < 12; index++)
                {
                    RaiseWheel(FindWheelSource(grid), -30);
                    wheelCount++;
                    window.UpdateLayout();
                    smallOffsets.Add(gridScroller.VerticalOffset);
                    visibleCounts.Add(CountVisibleRows(grid));
                }

                var beforeReverse = gridScroller.VerticalOffset;
                for (var index = 0; index < 3; index++)
                {
                    RaiseWheel(FindWheelSource(grid), 120);
                    wheelCount++;
                    window.UpdateLayout();
                    reverseOffsets.Add(gridScroller.VerticalOffset);
                    visibleCounts.Add(CountVisibleRows(grid));
                }

                gridScroller.ScrollToVerticalOffset(gridScroller.ScrollableHeight);
                window.UpdateLayout();
                var layoutBeforeTerminal = layoutCount;
                terminalOffset = gridScroller.VerticalOffset;
                for (var index = 0; index < 5; index++)
                {
                    RaiseWheel(FindWheelSource(grid), -30);
                    wheelCount++;
                    window.UpdateLayout();
                    visibleCounts.Add(CountVisibleRows(grid));
                }

                terminalLayoutDelta = layoutCount - layoutBeforeTerminal;

                output.WriteLine($"smallOffsets={string.Join(",", smallOffsets.Select(value => value.ToString("0.###")))}");
                output.WriteLine($"reverseOffsets={string.Join(",", reverseOffsets.Select(value => value.ToString("0.###")))}");
                output.WriteLine($"terminalOffset={terminalOffset:0.###}; scrollableHeight={gridScroller.ScrollableHeight:0.###}; finalOffset={gridScroller.VerticalOffset:0.###}");
                output.WriteLine($"layoutCount={layoutCount}; wheelCount={wheelCount}; terminalLayoutDelta={terminalLayoutDelta}");
                output.WriteLine($"visibleRowCounts={string.Join(",", visibleCounts)}; min={visibleCounts.Min()}; max={visibleCounts.Max()}");

                Assert.True(smallOffsets[smallOffsets.Count - 1] > smallOffsets[0],
                    $"small wheel increments did not move the viewport: {smallOffsets[0]} -> {smallOffsets[smallOffsets.Count - 1]}");
                Assert.True(IsNonDecreasing(smallOffsets),
                    "small wheel increments reversed the viewport before the explicit reverse sequence");
                Assert.True(reverseOffsets.Count == 3 && reverseOffsets[2] < beforeReverse,
                    $"reverse sequence did not move upward: start={beforeReverse}, values={string.Join(",", reverseOffsets)}");
                Assert.Equal(gridScroller.ScrollableHeight, terminalOffset, 3);
                Assert.Equal(terminalOffset, gridScroller.VerticalOffset, 3);
                Assert.True(visibleCounts.All(count => count > 0), "a scroll sample had no realized visible row container");
                Assert.True(layoutCount <= (wheelCount * 4) + 12,
                    $"layout refresh count exceeded the bounded wheel budget: {layoutCount} for {wheelCount} wheel events");
                Assert.True(terminalLayoutDelta <= 1,
                    $"terminal no-op wheel events caused repeated layout refreshes: {terminalLayoutDelta}");
            }
            catch (Exception caught)
            {
                exception = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
    }

    [Fact]
    public void AcceleratedBoundaryWheelTransfersMultipleLinesWithoutOvershootingOuterEnd()
    {
        Exception? exception = null;
        var outerBefore = 0d;
        var outerAfter = 0d;
        var outerMaximum = 0d;
        var innerBefore = 0d;
        var innerAfter = 0d;
        var handled = false;
        var layoutCount = 0;
        var visibleCounts = new List<int>();

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var innerContent = new StackPanel();
                for (var index = 0; index < 30; index++)
                    innerContent.Children.Add(new Border { Height = 24, Width = 240 });

                var inner = new ScrollViewer
                {
                    Height = 92,
                    Width = 260,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    Content = innerContent
                };
                var outerContent = new StackPanel();
                outerContent.Children.Add(inner);
                outerContent.Children.Add(new Border { Height = 520, Width = 280 });
                var outer = new ScrollViewer
                {
                    Height = 180,
                    Width = 300,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    Content = outerContent
                };
                ScrollBoundaryRoutingBehavior.SetEnabled(outer, true);
                ScrollBoundaryRoutingBehavior.SetEnabled(inner, true);

                window = CreateWindow(outer, 320, 200);
                window.Show();
                window.UpdateLayout();
                inner.ScrollToVerticalOffset(inner.ScrollableHeight);
                outer.ScrollToVerticalOffset(Math.Min(32, outer.ScrollableHeight));
                window.UpdateLayout();

                outer.LayoutUpdated += (_, _) => layoutCount++;
                outerBefore = outer.VerticalOffset;
                outerMaximum = outer.ScrollableHeight;
                innerBefore = inner.VerticalOffset;
                var args = RaiseWheel(inner, -360);
                window.UpdateLayout();
                outerAfter = outer.VerticalOffset;
                innerAfter = inner.VerticalOffset;
                handled = args.Handled;
                visibleCounts.Add(CountVisibleBorders(outerContent));

                Assert.True(handled);
                Assert.True(outerAfter > outerBefore, $"accelerated boundary transfer did not move outer surface: {outerAfter} <= {outerBefore}");
                Assert.True(outerAfter <= outerMaximum + 0.5, $"outer surface overshot its end: {outerAfter} > {outerMaximum}");
                Assert.Equal(innerBefore, innerAfter, 3);
                Assert.True(layoutCount <= 4, $"one accelerated boundary wheel caused too many layout refreshes: {layoutCount}");
                Assert.True(visibleCounts[0] > 0);

                output.WriteLine($"outerOffset={outerBefore:0.###}->{outerAfter:0.###}/{outerMaximum:0.###}; innerOffset={innerBefore:0.###}->{innerAfter:0.###}");
                output.WriteLine($"layoutCount={layoutCount}; visibleBorderCount={visibleCounts[0]}");
            }
            catch (Exception caught)
            {
                exception = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
    }

    private static DataGrid CreateGrid(int count)
    {
        var grid = new DataGrid
        {
            Height = 150,
            Width = 300,
            AutoGenerateColumns = true,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            CanUserAddRows = false,
            EnableRowVirtualization = true,
            ItemsSource = Enumerable.Range(0, count).Select(index => new GridRow { Index = index }).ToList()
        };
        ScrollViewer.SetCanContentScroll(grid, true);
        ScrollViewer.SetVerticalScrollBarVisibility(grid, ScrollBarVisibility.Auto);
        ScrollViewer.SetHorizontalScrollBarVisibility(grid, ScrollBarVisibility.Disabled);
        ScrollBoundaryRoutingBehavior.SetEnabled(grid, true);
        return grid;
    }

    private static Window CreateWindow(UIElement content, double width, double height)
        => new Window
        {
            Content = content,
            Width = width,
            Height = height,
            ShowInTaskbar = false,
            ShowActivated = false,
            WindowStyle = WindowStyle.None,
            Opacity = 0.01
        };

    private static MouseWheelEventArgs RaiseWheel(UIElement source, int delta)
    {
        var args = new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, delta)
        {
            RoutedEvent = UIElement.PreviewMouseWheelEvent
        };
        source.RaiseEvent(args);
        if (args.Handled)
            return args;

        var bubblingArgs = new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, delta)
        {
            RoutedEvent = UIElement.MouseWheelEvent
        };
        source.RaiseEvent(bubblingArgs);
        return bubblingArgs;
    }

    private static int CountVisibleRows(DataGrid grid)
        => FindVisualChildren<DataGridRow>(grid).Count(row => row.Visibility == Visibility.Visible);

    private static UIElement FindWheelSource(DataGrid grid)
    {
        var row = FindVisualChildren<DataGridRow>(grid).FirstOrDefault(candidate => candidate.Visibility == Visibility.Visible);
        return row ?? (UIElement)grid;
    }

    private static int CountVisibleBorders(Panel panel)
        => panel.Children.OfType<UIElement>().Count(child => child.Visibility == Visibility.Visible);

    private static bool IsNonDecreasing(IReadOnlyList<double> values)
    {
        for (var index = 1; index < values.Count; index++)
        {
            if (values[index] + 0.01 < values[index - 1]) return false;
        }

        return true;
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

    private sealed class GridRow
    {
        public int Index { get; set; }
    }
}
