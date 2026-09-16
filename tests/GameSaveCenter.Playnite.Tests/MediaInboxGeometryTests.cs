using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class MediaInboxGeometryTests
{
    [Fact]
    public void ReadableFloorUsesTableChromeAndFrameChromeIndependently()
    {
        var gridHeight = MediaInboxGeometry.CalculateReadableGridHeight(42, 52, 16);
        var frameHeight = MediaInboxGeometry.CalculateReadableFrameHeight(
            gridHeight,
            new Thickness(14, 12, 14, 12),
            new Thickness(1));

        Assert.Equal(266, gridHeight);
        Assert.Equal(292, frameHeight);
    }

    [Fact]
    public void ProductionInboxKeepsFourRowsOrExposesThePageFallback()
    {
        Exception? exception = null;
        var measuredRows = 0;
        var gridMinimum = 0d;
        var frameMinimum = 0d;
        var requiredGridMinimum = 0d;
        var pageScrollable = false;

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var view = new MediaCenterView();
                var viewType = typeof(MediaCenterView);
                var grid = (DataGrid)viewType.GetField("MediaInboxGrid", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var frame = (Border)viewType.GetField("MediaInboxTableFrame", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var pageScroller = (ScrollViewer)viewType.GetField("MediaInboxPageScrollViewer", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                grid.ItemsSource = Enumerable.Range(0, 20).Select(_ => new object()).ToArray();

                window = new Window
                {
                    Content = view,
                    Width = 760,
                    Height = 600,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    WindowStyle = WindowStyle.None,
                    Opacity = 0.01
                };
                view.ApplyResponsiveLayout(760, 600);
                window.Show();
                window.UpdateLayout();
                view.ApplyResponsiveLayout(760, 600);
                window.UpdateLayout();

                var row = FindVisualChildren<DataGridRow>(grid)
                    .FirstOrDefault(candidate => candidate.Visibility == Visibility.Visible && candidate.ActualHeight > 0);
                Assert.NotNull(row);
                var rowHeight = row!.ActualHeight;
                var horizontalBarHeight = FindVisualChildren<ScrollBar>(grid)
                    .Where(candidate => candidate.Orientation == Orientation.Horizontal
                        && candidate.Visibility == Visibility.Visible
                        && candidate.ActualHeight > 0)
                    .Select(candidate => candidate.ActualHeight)
                    .FirstOrDefault();
                requiredGridMinimum = MediaInboxGeometry.CalculateReadableGridHeight(
                    grid.ColumnHeaderHeight,
                    rowHeight,
                    horizontalBarHeight);
                gridMinimum = grid.MinHeight;
                frameMinimum = frame.MinHeight;
                measuredRows = FindVisualChildren<DataGridRow>(grid)
                    .Count(candidate => IsFullyInside(candidate, grid));
                pageScrollable = pageScroller.ScrollableHeight > 0.5;
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
        Assert.True(requiredGridMinimum >= 250, $"required grid floor was {requiredGridMinimum:0.##} DIP");
        Assert.True(gridMinimum + 0.5 >= requiredGridMinimum, $"grid minimum {gridMinimum:0.##} < required {requiredGridMinimum:0.##}");
        Assert.True(frameMinimum + 0.5 >= gridMinimum + 26, $"frame minimum {frameMinimum:0.##} does not include 24 DIP padding and 2 DIP border");
        Assert.True(measuredRows >= 4 || pageScrollable, $"only {measuredRows} complete rows and no page fallback");
    }

    private static bool IsFullyInside(FrameworkElement element, FrameworkElement boundary)
    {
        var bounds = element.TransformToAncestor(boundary).TransformBounds(
            new Rect(0, 0, element.ActualWidth, element.ActualHeight));
        return bounds.Height > 0
            && bounds.Top >= -0.5
            && bounds.Bottom <= boundary.ActualHeight + 0.5;
    }

    private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
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
}
