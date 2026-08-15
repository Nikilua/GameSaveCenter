using System;
using System.Collections.Generic;
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

        public static List<Violation> Validate(AdaptiveThemePalette palette, Color background)
        {
            var violations = new List<Violation>();
            var surface = Composite(palette.SurfaceTop, background);
            var controlFill = Composite(palette.ControlFill, surface);
            var secondary = Composite(palette.SecondaryText, background);

            AddContrast(violations, "PrimaryText vs Background", palette.PrimaryText, background, 4.5);
            AddContrast(violations, "SecondaryText vs Background", secondary, background, 3.0);
            AddContrast(violations, "ControlStroke vs Surface", palette.ControlStroke, surface, 1.15);
            AddLuminance(violations, "Surface vs Background", surface, background, 0.016);
            AddLuminance(violations, "ControlFill vs Surface", controlFill, surface, 0.005);
            return violations;
        }

        private static void AddContrast(List<Violation> violations, string name, Color first, Color second, double minimum)
        {
            var actual = ContrastRatio(first, second);
            if (actual + 0.001 < minimum)
                violations.Add(new Violation { Check = name, Actual = Math.Round(actual, 3), Minimum = minimum });
        }

        private static void AddLuminance(List<Violation> violations, string name, Color first, Color second, double minimum)
        {
            var actual = Math.Abs(RelativeLuminance(first) - RelativeLuminance(second));
            if (actual + 0.0005 < minimum)
                violations.Add(new Violation { Check = name, Actual = Math.Round(actual, 4), Minimum = minimum });
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
