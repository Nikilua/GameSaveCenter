using System;
using System.Windows;
using System.Windows.Controls;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Reserves enough height for a table and the Auto-sized controls that sit above and
    /// below it. A minimum on the DataGrid alone does not protect those rows when a
    /// compact details pane also occupies the workspace's remaining height.
    /// </summary>
    public static class WorkspaceTableViewportPolicy
    {
        public static double CalculateMinimumSurfaceHeight(
            Grid contentGrid,
            FrameworkElement tableViewport,
            double minimumViewportHeight,
            Border? surface = null,
            double autoRowFallbackHeight = 44d,
            double roundingAllowance = 8d)
        {
            if (contentGrid == null) throw new ArgumentNullException(nameof(contentGrid));
            if (tableViewport == null) throw new ArgumentNullException(nameof(tableViewport));
            if (!IsFiniteNonNegative(minimumViewportHeight))
                throw new ArgumentOutOfRangeException(nameof(minimumViewportHeight));
            if (!IsFiniteNonNegative(autoRowFallbackHeight))
                throw new ArgumentOutOfRangeException(nameof(autoRowFallbackHeight));
            if (!IsFiniteNonNegative(roundingAllowance))
                throw new ArgumentOutOfRangeException(nameof(roundingAllowance));

            var tableRow = Grid.GetRow(tableViewport);
            if (tableRow < 0 || tableRow >= contentGrid.RowDefinitions.Count)
                throw new ArgumentException("The table viewport must be placed in a row of the content grid.", nameof(tableViewport));

            var height = minimumViewportHeight;
            for (var rowIndex = 0; rowIndex < contentGrid.RowDefinitions.Count; rowIndex++)
            {
                if (rowIndex == tableRow)
                    continue;

                var row = contentGrid.RowDefinitions[rowIndex];
                if (row.Height.IsAbsolute)
                {
                    height += row.Height.Value;
                    continue;
                }

                if (!row.Height.IsAuto)
                    continue;

                var measuredHeight = row.ActualHeight;
                var hasVisibleChild = false;
                foreach (UIElement child in contentGrid.Children)
                {
                    if (Grid.GetRow(child) != rowIndex || Grid.GetRowSpan(child) != 1 || child.Visibility != Visibility.Visible)
                        continue;

                    hasVisibleChild = true;
                    if (child is FrameworkElement element)
                    {
                        measuredHeight = Math.Max(measuredHeight, element.ActualHeight);
                        measuredHeight = Math.Max(measuredHeight, element.DesiredSize.Height);
                    }
                }

                if (hasVisibleChild && measuredHeight <= 0)
                    measuredHeight = autoRowFallbackHeight;
                height += measuredHeight;
            }

            if (surface != null)
            {
                height += surface.Padding.Top + surface.Padding.Bottom;
                height += surface.BorderThickness.Top + surface.BorderThickness.Bottom;
            }

            return Math.Ceiling(height + roundingAllowance);
        }

        private static bool IsFiniteNonNegative(double value)
            => !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0;
    }
}
