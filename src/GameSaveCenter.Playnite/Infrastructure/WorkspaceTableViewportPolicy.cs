using System;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Keeps workspace tables finite while sharing the page's actual remaining height
    /// between fixed page chrome, the table, and an optional stacked inspector.
    /// </summary>
    internal static class WorkspaceTableViewportPolicy
    {
        public static double MeasureChromeHeight(double surfaceHeight, double tableHeight, double fallbackHeight)
        {
            if (IsFinite(surfaceHeight) && IsFinite(tableHeight) && surfaceHeight > 0 && surfaceHeight >= tableHeight)
                return surfaceHeight - tableHeight;

            return Math.Max(0, fallbackHeight);
        }

        public static double CalculateTableHeight(
            double pageViewportHeight,
            double minimumTableHeight,
            double nonTableChromeHeight,
            double stackedInspectorHeight = 0)
        {
            var remaining = IsFinite(pageViewportHeight)
                ? pageViewportHeight - Math.Max(0, nonTableChromeHeight) - Math.Max(0, stackedInspectorHeight)
                : minimumTableHeight;

            return Math.Max(Math.Max(0, minimumTableHeight), Math.Max(0, remaining));
        }

        public static double CalculatePageContentHeight(
            double pageViewportHeight,
            double nonTableChromeHeight,
            double tableHeight,
            double stackedInspectorHeight = 0)
        {
            var required = Math.Max(0, nonTableChromeHeight)
                + Math.Max(0, tableHeight)
                + Math.Max(0, stackedInspectorHeight);

            return Math.Max(IsFinite(pageViewportHeight) ? Math.Max(0, pageViewportHeight) : 0, required);
        }

        private static bool IsFinite(double value)
            => !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
