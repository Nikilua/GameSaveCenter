using System;
using System.Globalization;
using System.Windows;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Applies the user's shared table density to the local GameSaveCenter resource scope.
    /// The host's application resources are never changed.
    /// </summary>
    public static class TableDensityPolicy
    {
        public const string ComfortableMode = "Comfortable";
        public const string CompactMode = "Compact";
        public const int MinimumReadableRows = 4;
        public const double DefaultRowHeight = 52d;
        public const double DefaultHeaderHeight = 42d;
        public const double DefaultHorizontalScrollBarHeight = 16d;
        public const double RoundingAllowance = 8d;

        public static string NormalizeMode(string? value)
            => string.Equals(value?.Trim(), CompactMode, StringComparison.OrdinalIgnoreCase)
                ? CompactMode
                : ComfortableMode;

        public static bool IsCompact(string? value)
            => string.Equals(NormalizeMode(value), CompactMode, StringComparison.Ordinal);

        public static void ApplyResources(FrameworkElement owner, string? densityMode)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));

            var compact = IsCompact(densityMode);
            owner.Resources["GscTableRowHeight"] = owner.FindResource(
                compact ? "GscTableRowHeightCompact" : "GscTableRowHeightComfortable");
            owner.Resources["GscTableHeaderHeight"] = owner.FindResource(
                compact ? "GscTableHeaderHeightCompact" : "GscTableHeaderHeightComfortable");
            owner.Resources["GscTableCellPadding"] = owner.FindResource(
                compact ? "GscTableCellPaddingCompact" : "GscTableCellPaddingComfortable");
        }

        public static double CalculateMinimumGridHeight(
            double rowHeight,
            double headerHeight,
            double horizontalScrollBarHeight = DefaultHorizontalScrollBarHeight,
            int minimumRows = MinimumReadableRows,
            double roundingAllowance = RoundingAllowance)
        {
            var normalizedRowHeight = PositiveOrDefault(rowHeight, DefaultRowHeight);
            var normalizedHeaderHeight = PositiveOrDefault(headerHeight, DefaultHeaderHeight);
            return normalizedHeaderHeight
                + normalizedRowHeight * Math.Max(1, minimumRows)
                + NonNegativeOrZero(horizontalScrollBarHeight)
                + NonNegativeOrZero(roundingAllowance);
        }

        public static double CalculateMinimumGridHeight(FrameworkElement owner)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));

            return CalculateMinimumGridHeight(
                ReadDoubleResource(owner, "GscTableRowHeight", DefaultRowHeight),
                ReadDoubleResource(owner, "GscTableHeaderHeight", DefaultHeaderHeight));
        }

        private static double ReadDoubleResource(FrameworkElement owner, string key, double fallback)
        {
            var value = owner.TryFindResource(key);
            if (value == null) return fallback;
            try
            {
                return Convert.ToDouble(value, CultureInfo.InvariantCulture);
            }
            catch (InvalidCastException)
            {
                return fallback;
            }
            catch (FormatException)
            {
                return fallback;
            }
        }

        private static double PositiveOrDefault(double value, double fallback)
            => double.IsNaN(value) || double.IsInfinity(value) || value <= 0 ? fallback : value;

        private static double NonNegativeOrZero(double value)
            => double.IsNaN(value) || double.IsInfinity(value) || value <= 0 ? 0 : value;
    }
}
