using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Keeps the runtime adaptive palette from collapsing into a single low-contrast
    /// surface under any Playnite theme. Thresholds are calibrated against the design
    /// tokens used by the offscreen reference palette.
    /// </summary>
    internal static class AdaptiveThemePaletteContrastGuard
    {
        public sealed class Violation
        {
            public string Check { get; set; } = string.Empty;
            public double Actual { get; set; }
            public double Minimum { get; set; }
        }

        public sealed class Measurement
        {
            public string Check { get; set; } = string.Empty;
            public double Actual { get; set; }
            public double Minimum { get; set; }
        }

        public static List<Violation> Validate(AdaptiveThemePalette palette, Color background)
        {
            return Measure(palette, background)
                .Where(measurement => measurement.Actual + 0.001 < measurement.Minimum)
                .Select(measurement => new Violation
                {
                    Check = measurement.Check,
                    Actual = measurement.Actual,
                    Minimum = measurement.Minimum
                })
                .ToList();
        }

        public static List<Measurement> Measure(AdaptiveThemePalette palette, Color background)
        {
            var measurements = new List<Measurement>();
            var surface = Composite(palette.SurfaceTop, background);
            var controlFill = Composite(palette.ControlFill, surface);
            var secondary = Composite(palette.SecondaryText, background);
            var muted = Composite(palette.MutedText, background);

            AddContrast(measurements, "PrimaryText vs Background", palette.PrimaryText, background, 4.5);
            AddContrast(measurements, "SecondaryText vs Background", secondary, background, 4.5);
            AddContrast(measurements, "MutedText vs Background", muted, background, 4.5);
            AddContrast(measurements, "OnAccentText vs Accent", palette.OnAccentText, palette.Accent, 4.5);
            AddContrast(measurements, "Info vs Background", palette.Info, background, 3.0);
            AddContrast(measurements, "Success vs Background", palette.Success, background, 3.0);
            AddContrast(measurements, "Warning vs Background", palette.Warning, background, 3.0);
            AddContrast(measurements, "Error vs Background", palette.Error, background, 3.0);
            AddContrast(measurements, "ControlStroke vs Surface", palette.ControlStroke, surface, 1.15);
            AddLuminance(measurements, "Surface vs Background", surface, background, 0.016);
            AddLuminance(measurements, "ControlFill vs Surface", controlFill, surface, 0.005);
            return measurements;
        }

        private static void AddContrast(List<Measurement> measurements, string name, Color first, Color second, double minimum)
        {
            var actual = ContrastRatio(first, second);
            measurements.Add(new Measurement { Check = name, Actual = Math.Round(actual, 3), Minimum = minimum });
        }

        private static void AddLuminance(List<Measurement> measurements, string name, Color first, Color second, double minimum)
        {
            var actual = Math.Abs(RelativeLuminance(first) - RelativeLuminance(second));
            measurements.Add(new Measurement { Check = name, Actual = Math.Round(actual, 4), Minimum = minimum });
        }

        private static Color Composite(Color color, Color background)
        {
            var alpha = color.A / 255.0;
            if (alpha >= 1)
                return color;
            return Color.FromRgb(
                (byte)Math.Round(color.R * alpha + background.R * (1 - alpha)),
                (byte)Math.Round(color.G * alpha + background.G * (1 - alpha)),
                (byte)Math.Round(color.B * alpha + background.B * (1 - alpha)));
        }

        private static double ContrastRatio(Color first, Color second)
        {
            var lighter = Math.Max(RelativeLuminance(first), RelativeLuminance(second));
            var darker = Math.Min(RelativeLuminance(first), RelativeLuminance(second));
            return (lighter + 0.05) / (darker + 0.05);
        }

        private static double RelativeLuminance(Color color)
        {
            double Convert(byte channel)
            {
                var value = channel / 255.0;
                return value <= 0.03928 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
            }

            return 0.2126 * Convert(color.R) + 0.7152 * Convert(color.G) + 0.0722 * Convert(color.B);
        }
    }
}
