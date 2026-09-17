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

namespace GameSaveCenter.Playnite.Tests;

public sealed class R07ScrollOwnershipBehaviorTests
{
    [Fact]
    public void NestedSurfaceTransfersWheelOnlyAtItsVerticalBoundary()
    {
        Exception? exception = null;
        var downOuterOffset = 0d;
        var downInnerOffset = 0d;
        var upOuterOffset = 0d;
        var upInnerOffset = 0d;
        var downHandled = false;
        var upHandled = false;

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var innerContent = new StackPanel();
                for (var index = 0; index < 18; index++)
                    innerContent.Children.Add(new Border { Height = 24, Width = 240 });

                var inner = new ScrollViewer
                {
                    Height = 80,
                    Width = 260,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    Content = innerContent
                };
                var outerContent = new StackPanel();
                outerContent.Children.Add(inner);
                outerContent.Children.Add(new Border { Height = 260, Width = 280 });
                var outer = new ScrollViewer
                {
                    Height = 160,
                    Width = 300,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    Content = outerContent
                };

                ScrollBoundaryRoutingBehavior.SetEnabled(outer, true);
                ScrollBoundaryRoutingBehavior.SetEnabled(inner, true);
                window = new Window
                {
                    Content = outer,
                    Width = 320,
                    Height = 180,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    WindowStyle = WindowStyle.None,
                    Opacity = 0.01
                };
                window.Show();
                window.UpdateLayout();

                inner.ScrollToVerticalOffset(inner.ScrollableHeight);
                outer.ScrollToVerticalOffset(Math.Min(24, outer.ScrollableHeight));
                window.UpdateLayout();
                var initialOuterDown = outer.VerticalOffset;
                var initialInnerDown = inner.VerticalOffset;
                var downArgs = RaiseWheel(inner, -120);
                window.UpdateLayout();
                downOuterOffset = outer.VerticalOffset;
                downInnerOffset = inner.VerticalOffset;
                downHandled = downArgs.Handled;

                inner.ScrollToVerticalOffset(0);
                outer.ScrollToVerticalOffset(Math.Min(32, outer.ScrollableHeight));
                window.UpdateLayout();
                var initialOuterUp = outer.VerticalOffset;
                var initialInnerUp = inner.VerticalOffset;
                var upArgs = RaiseWheel(inner, 120);
                window.UpdateLayout();
                upOuterOffset = outer.VerticalOffset;
                upInnerOffset = inner.VerticalOffset;
                upHandled = upArgs.Handled;

                Assert.True(initialOuterDown > 0);
                Assert.True(initialInnerDown > 0);
                Assert.True(downHandled);
                Assert.True(downOuterOffset > initialOuterDown, $"outer did not receive down transfer: {downOuterOffset} <= {initialOuterDown}");
                Assert.Equal(initialInnerDown, downInnerOffset, 3);

                Assert.True(initialOuterUp > 0);
                Assert.Equal(0, initialInnerUp, 3);
                Assert.True(upHandled);
                Assert.True(upOuterOffset < initialOuterUp, $"outer did not receive up transfer: {upOuterOffset} >= {initialOuterUp}");
                Assert.Equal(0, upInnerOffset, 3);
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
    public void DataGridTransfersWheelAtBoundaryToItsPageSurface()
    {
        Exception? exception = null;
        var initialOuterOffset = 0d;
        var finalOuterOffset = 0d;
        var initialGridOffset = 0d;
        var finalGridOffset = 0d;
        var handled = false;
        var visibleRows = 0;

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var grid = new DataGrid
                {
                    Height = 90,
                    Width = 280,
                    AutoGenerateColumns = true,
                    HeadersVisibility = DataGridHeadersVisibility.Column,
                    CanUserAddRows = false,
                    EnableRowVirtualization = true,
                    ItemsSource = Enumerable.Range(0, 40).Select(index => new GridRow { Index = index }).ToList()
                };
                ScrollViewer.SetCanContentScroll(grid, true);
                ScrollViewer.SetVerticalScrollBarVisibility(grid, ScrollBarVisibility.Auto);
                ScrollViewer.SetHorizontalScrollBarVisibility(grid, ScrollBarVisibility.Disabled);

                var outerContent = new StackPanel();
                outerContent.Children.Add(grid);
                outerContent.Children.Add(new Border { Height = 240, Width = 300 });
                var outer = new ScrollViewer
                {
                    Height = 170,
                    Width = 320,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    Content = outerContent
                };
                ScrollBoundaryRoutingBehavior.SetEnabled(outer, true);
                ScrollBoundaryRoutingBehavior.SetEnabled(grid, true);

                window = new Window
                {
                    Content = outer,
                    Width = 340,
                    Height = 190,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    WindowStyle = WindowStyle.None,
                    Opacity = 0.01
                };
                window.Show();
                window.UpdateLayout();

                var gridScroller = FindVisualChildren<ScrollViewer>(grid).FirstOrDefault();
                if (gridScroller == null || gridScroller.ScrollableHeight <= 0.5)
                    throw new InvalidOperationException("DataGrid did not expose a scrollable internal viewport.");
                gridScroller.ScrollToVerticalOffset(gridScroller.ScrollableHeight);
                outer.ScrollToVerticalOffset(Math.Min(24, outer.ScrollableHeight));
                window.UpdateLayout();

                var sourceRow = FindVisualChildren<DataGridRow>(grid).LastOrDefault();
                if (sourceRow == null) throw new InvalidOperationException("DataGrid did not realize a source row.");
                initialOuterOffset = outer.VerticalOffset;
                initialGridOffset = gridScroller.VerticalOffset;
                var args = RaiseWheel(sourceRow, -120);
                window.UpdateLayout();
                finalOuterOffset = outer.VerticalOffset;
                finalGridOffset = gridScroller.VerticalOffset;
                handled = args.Handled;
                visibleRows = FindVisualChildren<DataGridRow>(grid).Count(row => row.Visibility == Visibility.Visible);
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
        Assert.True(initialOuterOffset > 0);
        Assert.True(initialGridOffset > 0);
        Assert.True(handled);
        Assert.True(finalOuterOffset > initialOuterOffset, $"page surface did not receive DataGrid boundary transfer: {finalOuterOffset} <= {initialOuterOffset}");
        Assert.Equal(initialGridOffset, finalGridOffset, 3);
        Assert.True(visibleRows > 0);
    }

    private static MouseWheelEventArgs RaiseWheel(UIElement source, int delta)
    {
        var args = new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, delta)
        {
            RoutedEvent = UIElement.PreviewMouseWheelEvent
        };
        source.RaiseEvent(args);
        return args;
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
