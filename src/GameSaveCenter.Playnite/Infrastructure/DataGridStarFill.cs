using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// WPF DataGrid star columns can stay at their minimum width when the grid is measured
    /// inside a scroll host before the final viewport width is known. This behavior records
    /// the star weights declared in XAML and re-distributes the remaining viewport width to
    /// those columns after the grid is sized. User column drags are preserved because column
    /// resizing does not raise DataGrid.SizeChanged.
    /// </summary>
    public static class DataGridStarFill
    {
        public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
            "Enabled", typeof(bool), typeof(DataGridStarFill), new PropertyMetadata(false, OnEnabledChanged));

        private static readonly DependencyProperty StarIndexesProperty = DependencyProperty.RegisterAttached(
            "StarIndexes", typeof(int[]), typeof(DataGridStarFill), new PropertyMetadata(null));

        private static readonly DependencyProperty StarWeightsProperty = DependencyProperty.RegisterAttached(
            "StarWeights", typeof(double[]), typeof(DataGridStarFill), new PropertyMetadata(null));

        private static readonly DependencyProperty IsRedistributingProperty = DependencyProperty.RegisterAttached(
            "IsRedistributing", typeof(bool), typeof(DataGridStarFill), new PropertyMetadata(false));

        private static readonly DependencyProperty AppliedProperty = DependencyProperty.RegisterAttached(
            "Applied", typeof(bool), typeof(DataGridStarFill), new PropertyMetadata(false));

        public static bool GetEnabled(DependencyObject element) => (bool)element.GetValue(EnabledProperty);
        public static void SetEnabled(DependencyObject element, bool value) => element.SetValue(EnabledProperty, value);
        public static bool GetApplied(DependencyObject element) => (bool)element.GetValue(AppliedProperty);

        private static void OnEnabledChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            if (target is not DataGrid grid)
                return;

            if ((bool)args.NewValue)
            {
                grid.Loaded += OnGridLoaded;
                grid.SizeChanged += OnGridSizeChanged;
                if (grid.IsLoaded)
                    Redistribute(grid);
            }
            else
            {
                grid.Loaded -= OnGridLoaded;
                grid.SizeChanged -= OnGridSizeChanged;
                grid.ClearValue(StarIndexesProperty);
                grid.ClearValue(StarWeightsProperty);
            }
        }

        private static void OnGridLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid grid)
                Redistribute(grid);
        }

        private static void OnGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is DataGrid grid)
                Redistribute(grid);
        }

        public static void Redistribute(DataGrid grid)
        {
            if (grid == null
                || grid.ActualWidth <= 0
                || (bool)grid.GetValue(IsRedistributingProperty))
                return;
            grid.SetValue(IsRedistributingProperty, true);
            try
            {
                if (grid.GetValue(StarIndexesProperty) is not int[] starIndexes
                    || grid.GetValue(StarWeightsProperty) is not double[] starWeights
                    || starIndexes.Length == 0
                    || starIndexes.Length != starWeights.Length)
                {
                    var captured = grid.Columns
                        .Select((column, index) => new { column, index })
                        .Where(item => item.column.Width.IsStar)
                        .Select(item => item.index)
                        .ToArray();
                    var capturedWeights = grid.Columns
                        .Where(column => column.Width.IsStar)
                        .Select(column => column.Width.Value)
                        .ToArray();
                    if (captured.Length == 0 || captured.Length != capturedWeights.Length)
                        return;
                    grid.SetValue(StarIndexesProperty, captured);
                    grid.SetValue(StarWeightsProperty, capturedWeights);
                    starIndexes = captured;
                    starWeights = capturedWeights;
                }

                var starSet = starIndexes.ToHashSet();
                var fixedWidth = 0d;
                for (var index = 0; index < grid.Columns.Count; index++)
                {
                    if (!starSet.Contains(index))
                        fixedWidth += grid.Columns[index].ActualWidth;
                }

                var usableWidth = grid.ActualWidth - fixedWidth;
                var scroller = FindInternalScroller(grid);
                if (scroller != null && scroller.ComputedVerticalScrollBarVisibility == Visibility.Visible)
                    usableWidth -= SystemParameters.VerticalScrollBarWidth;
                if (usableWidth <= 0)
                    return;

                var totalWeight = starWeights.Sum();
                for (var i = 0; i < starIndexes.Length; i++)
                {
                    var index = starIndexes[i];
                    if (index < 0 || index >= grid.Columns.Count)
                        continue;
                    var column = grid.Columns[index];
                    var share = totalWeight > 0 ? starWeights[i] / totalWeight : 1d / starIndexes.Length;
                    column.Width = new DataGridLength(
                        Math.Max(column.MinWidth, usableWidth * share),
                        DataGridLengthUnitType.Pixel);
                }

                grid.SetValue(AppliedProperty, true);
                grid.InvalidateMeasure();
            }
            finally
            {
                grid.SetValue(IsRedistributingProperty, false);
            }
        }

        private static ScrollViewer? FindInternalScroller(DataGrid grid)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(grid); i++)
            {
                var child = VisualTreeHelper.GetChild(grid, i);
                if (child is ScrollViewer scroller)
                    return scroller;
                var nested = FindScroller(child);
                if (nested != null)
                    return nested;
            }
            return null;
        }

        private static ScrollViewer? FindScroller(DependencyObject parent)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is ScrollViewer scroller)
                    return scroller;
                var nested = FindScroller(child);
                if (nested != null)
                    return nested;
            }
            return null;
        }
    }
}
