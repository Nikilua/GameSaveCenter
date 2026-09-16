using System;
using System.Windows;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Shared geometry contract for the media inbox reading surface.
    /// DataGrid rows include their own cell chrome; the surrounding frame adds
    /// only its measured padding and border thickness.
    /// </summary>
    public static class MediaInboxGeometry
    {
        public const int MinimumCompleteRows = 4;
        public const double DefaultHeaderHeight = 42d;
        public const double DefaultRowHeight = 52d;
        public const double DefaultHorizontalScrollBarHeight = 16d;

        public static double CalculateReadableGridHeight(
            double headerHeight,
            double rowHeight,
            double horizontalScrollBarHeight)
        {
            return PositiveOrDefault(headerHeight, DefaultHeaderHeight)
                + PositiveOrDefault(rowHeight, DefaultRowHeight) * MinimumCompleteRows
                + Math.Max(0, horizontalScrollBarHeight);
        }

        public static double CalculateReadableFrameHeight(
            double gridHeight,
            Thickness padding,
            Thickness borderThickness)
        {
            return Math.Max(0, gridHeight)
                + Math.Max(0, padding.Top)
                + Math.Max(0, padding.Bottom)
                + Math.Max(0, borderThickness.Top)
                + Math.Max(0, borderThickness.Bottom);
        }

        private static double PositiveOrDefault(double value, double fallback)
            => double.IsNaN(value) || double.IsInfinity(value) || value <= 0 ? fallback : value;
    }
}
