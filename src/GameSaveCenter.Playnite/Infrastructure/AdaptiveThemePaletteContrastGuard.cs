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

        /// <summary>
        /// Describes a text sample using the colors that are actually effective at the
        /// sampled surface. This is intentionally separate from palette-level semantic
        /// colors: a status foreground may be readable on one tinted pill and fail on
        /// another surface after alpha compositing.
        /// </summary>
        public sealed class TextContrastSample
        {
            public string Check { get; set; } = string.Empty;
            public Color Foreground { get; set; }
            public Color Background { get; set; }
            public double Minimum { get; set; } = 4.5;
        }

        public sealed class TextContrastMeasurement
        {
            public string Check { get; set; } = string.Empty;
            public Color EffectiveForeground { get; set; }
            public Color Background { get; set; }
            public double Actual { get; set; }
            public double Minimum { get; set; }
        }

        public sealed class LayeredTextContrastSample
        {
            public string Check { get; set; } = string.Empty;
            public Color Foreground { get; set; }
            public Color Backdrop { get; set; }
            public IReadOnlyList<Color> SurfaceLayers { get; set; } = Array.Empty<Color>();
            public double Minimum { get; set; } = 4.5;
        }

        /// <summary>
        /// Measures text after applying the foreground alpha over the sampled background.
        /// Callers must provide a realized surface sample rather than a token color.
        /// </summary>
        public static List<TextContrastMeasurement> MeasureTextContrast(IEnumerable<TextContrastSample> samples)
        {
            if (samples == null) throw new ArgumentNullException(nameof(samples));

            return samples.Select(sample =>
            {
                var effectiveForeground = Composite(sample.Foreground, sample.Background);
                return new TextContrastMeasurement
                {
                    Check = sample.Check,
                    EffectiveForeground = effectiveForeground,
                    Background = sample.Background,
                    Actual = ContrastRatio(effectiveForeground, sample.Background),
                    Minimum = sample.Minimum
                };
            }).ToList();
        }

        public static List<Violation> ValidateTextContrast(IEnumerable<TextContrastSample> samples)
        {
            return MeasureTextContrast(samples)
                .Where(measurement => measurement.Actual + 0.001 < measurement.Minimum)
                .Select(measurement => new Violation
                {
                    Check = measurement.Check,
                    Actual = measurement.Actual,
                    Minimum = measurement.Minimum
                })
                .ToList();
        }

        public static List<TextContrastMeasurement> MeasureLayeredTextContrast(
            IEnumerable<LayeredTextContrastSample> samples)
        {
            if (samples == null) throw new ArgumentNullException(nameof(samples));

            return samples.Select(sample =>
            {
                var surface = sample.SurfaceLayers == null
                    ? sample.Backdrop
                    : sample.SurfaceLayers.Aggregate(
                        sample.Backdrop,
                        (background, layer) => Composite(layer, background));
                return new TextContrastMeasurement
                {
                    Check = sample.Check,
                    EffectiveForeground = Composite(sample.Foreground, surface),
                    Background = surface,
                    Actual = ContrastRatio(Composite(sample.Foreground, surface), surface),
                    Minimum = sample.Minimum
                };
            }).ToList();
        }

        public static List<Violation> ValidateLayeredTextContrast(
            IEnumerable<LayeredTextContrastSample> samples)
        {
            return MeasureLayeredTextContrast(samples)
                .Where(measurement => measurement.Actual + 0.001 < measurement.Minimum)
                .Select(measurement => new Violation
                {
                    Check = measurement.Check,
                    Actual = measurement.Actual,
                    Minimum = measurement.Minimum
                })
                .ToList();
        }

        /// <summary>
        /// Samples the realized gradient through the same alpha compositing path used by
        /// the button chrome. The state matrix includes the focus/hover/pressed layer
        /// combinations, and pressed opacity is applied to the complete rendered group,
        /// not just to the foreground token.
        /// </summary>
        public static List<TextContrastMeasurement> MeasureGradientTextContrast(
            string checkPrefix,
            Color foreground,
            Color backdrop,
            IEnumerable<Color> gradientStops,
            Color hoverOverlay,
            Color pressedOverlay,
            double pressedOpacity = 0.96,
            double minimum = 4.5)
        {
            if (checkPrefix == null) throw new ArgumentNullException(nameof(checkPrefix));
            if (gradientStops == null) throw new ArgumentNullException(nameof(gradientStops));

            var colors = gradientStops.ToArray();
            if (colors.Length == 0) throw new ArgumentException("At least one gradient stop is required.", nameof(gradientStops));

            var stops = colors
                .Select((color, index) => new GradientStop(color, colors.Length == 1 ? 0 : index / (double)(colors.Length - 1)))
                .ToArray();
            return MeasureGradientTextContrast(
                checkPrefix,
                foreground,
                backdrop,
                stops,
                hoverOverlay,
                hoverOverlay,
                pressedOverlay,
                pressedOpacity,
                minimum);
        }

        /// <summary>
        /// Samples gradient stops without discarding their offsets. Focus and hover use
        /// separate arguments because a template may layer them together even when the
        /// current production palette intentionally gives both states the same brush.
        /// </summary>
        public static List<TextContrastMeasurement> MeasureGradientTextContrast(
            string checkPrefix,
            Color foreground,
            Color backdrop,
            IEnumerable<GradientStop> gradientStops,
            Color hoverOverlay,
            Color pressedOverlay,
            double pressedOpacity = 0.96,
            double minimum = 4.5)
        {
            return MeasureGradientTextContrast(
                checkPrefix,
                foreground,
                backdrop,
                gradientStops,
                hoverOverlay,
                hoverOverlay,
                pressedOverlay,
                pressedOpacity,
                minimum);
        }

        public static List<TextContrastMeasurement> MeasureGradientTextContrast(
            string checkPrefix,
            Color foreground,
            Color backdrop,
            IEnumerable<GradientStop> gradientStops,
            Color hoverOverlay,
            Color focusOverlay,
            Color pressedOverlay,
            double pressedOpacity = 0.96,
            double minimum = 4.5)
        {
            if (checkPrefix == null) throw new ArgumentNullException(nameof(checkPrefix));
            if (gradientStops == null) throw new ArgumentNullException(nameof(gradientStops));

            var rawStops = gradientStops.ToArray();
            if (rawStops.Length == 0) throw new ArgumentException("At least one gradient stop is required.", nameof(gradientStops));
            if (rawStops.Any(stop => stop == null || double.IsNaN(stop.Offset) || stop.Offset < 0 || stop.Offset > 1))
                throw new ArgumentException("Gradient stop offsets must be finite values between 0 and 1.", nameof(gradientStops));
            var stops = rawStops.OrderBy(stop => stop.Offset).ToArray();

            var samples = new List<TextContrastSample>();
            for (var step = 0; step <= 10; step++)
            {
                var offset = step / 10.0;
                var baseSurface = Composite(SampleGradient(stops, offset), backdrop);
                var focusSurface = Composite(focusOverlay, baseSurface);
                var hoverSurface = Composite(hoverOverlay, baseSurface);
                var hoverFocusSurface = Composite(hoverOverlay, focusSurface);
                var pressedSurface = Composite(pressedOverlay, baseSurface);
                var hoverPressedSurface = Composite(pressedOverlay, hoverSurface);
                var pressedFocusSurface = Composite(pressedOverlay, focusSurface);
                var hoverPressedFocusSurface = Composite(pressedOverlay, hoverFocusSurface);

                AddButtonStateSample(samples, checkPrefix, "normal", offset, foreground, baseSurface, backdrop, 1, minimum);
                AddButtonStateSample(samples, checkPrefix, "hover", offset, foreground, hoverSurface, backdrop, 1, minimum);
                AddButtonStateSample(samples, checkPrefix, "focus", offset, foreground, focusSurface, backdrop, 1, minimum);
                AddButtonStateSample(samples, checkPrefix, "hover+focus", offset, foreground, hoverFocusSurface, backdrop, 1, minimum);
                AddButtonStateSample(samples, checkPrefix, "pressed", offset, foreground, pressedSurface, backdrop, pressedOpacity, minimum);
                AddButtonStateSample(samples, checkPrefix, "hover+pressed", offset, foreground, hoverPressedSurface, backdrop, pressedOpacity, minimum);
                AddButtonStateSample(samples, checkPrefix, "pressed+focus", offset, foreground, pressedFocusSurface, backdrop, pressedOpacity, minimum);
                AddButtonStateSample(samples, checkPrefix, "hover+pressed+focus", offset, foreground, hoverPressedFocusSurface, backdrop, pressedOpacity, minimum);
            }

            return MeasureTextContrast(samples);
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
            measurements.Add(new Measurement { Check = name, Actual = actual, Minimum = minimum });
        }

        private static void AddLuminance(List<Measurement> measurements, string name, Color first, Color second, double minimum)
        {
            var actual = Math.Abs(RelativeLuminance(first) - RelativeLuminance(second));
            measurements.Add(new Measurement { Check = name, Actual = actual, Minimum = minimum });
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

        private static void AddButtonStateSample(
            List<TextContrastSample> samples,
            string prefix,
            string state,
            double offset,
            Color foreground,
            Color surface,
            Color backdrop,
            double opacity,
            double minimum)
        {
            // ButtonChrome.Opacity is a group opacity: render the text over the realized
            // surface first, then blend both pixels with the real parent backdrop. This
            // keeps the black-on-black negative case black while correctly turning a white
            // chrome into gray at 0.5 opacity.
            var effectiveBackground = BlendWithOpacity(surface, backdrop, opacity);
            var effectiveForeground = BlendWithOpacity(Composite(foreground, surface), backdrop, opacity);
            samples.Add(new TextContrastSample
            {
                Check = $"{prefix}.{state}@{offset:0.0}",
                Foreground = effectiveForeground,
                Background = effectiveBackground,
                Minimum = minimum
            });
        }

        private static Color BlendWithOpacity(Color source, Color backdrop, double opacity)
        {
            var amount = Math.Max(0, Math.Min(1, opacity));
            return Color.FromRgb(
                (byte)Math.Round(source.R * amount + backdrop.R * (1 - amount)),
                (byte)Math.Round(source.G * amount + backdrop.G * (1 - amount)),
                (byte)Math.Round(source.B * amount + backdrop.B * (1 - amount)));
        }

        private static Color SampleGradient(IReadOnlyList<GradientStop> stops, double offset)
        {
            if (stops.Count == 1)
                return stops[0].Color;
            if (offset <= stops[0].Offset)
                return stops[0].Color;
            if (offset >= stops[stops.Count - 1].Offset)
                return stops[stops.Count - 1].Color;

            var upper = 1;
            while (upper < stops.Count && stops[upper].Offset < offset)
                upper++;
            var lower = upper - 1;
            var range = stops[upper].Offset - stops[lower].Offset;
            var fraction = range <= 0 ? 1 : (offset - stops[lower].Offset) / range;
            return Color.FromArgb(
                Interpolate(stops[lower].Color.A, stops[upper].Color.A, fraction),
                Interpolate(stops[lower].Color.R, stops[upper].Color.R, fraction),
                Interpolate(stops[lower].Color.G, stops[upper].Color.G, fraction),
                Interpolate(stops[lower].Color.B, stops[upper].Color.B, fraction));
        }

        private static byte Interpolate(byte first, byte second, double fraction)
            => (byte)Math.Round(first + (second - first) * fraction);

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
