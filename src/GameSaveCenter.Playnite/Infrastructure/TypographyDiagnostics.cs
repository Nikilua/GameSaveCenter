using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Shared, deterministic typography checks used by the development fixture and
    /// tests. Candidate coverage and measured layout are reported separately because
    /// WPF's final per-glyph fallback is a host-rendering concern.
    /// </summary>
    internal static class TypographyDiagnostics
    {
        public static readonly string[] UiFontChain =
        {
            "Inter", "Segoe UI Variable Text", "Segoe UI", "Noto Sans SC",
            "Noto Sans CJK SC", "Microsoft YaHei UI", "Microsoft YaHei"
        };

        public static readonly string[] DisplayFontChain =
        {
            "Inter", "Segoe UI Variable Display", "Segoe UI", "Noto Sans SC",
            "Noto Sans CJK SC", "Microsoft YaHei UI", "Microsoft YaHei"
        };

        public sealed class GlyphCandidate
        {
            public string Family { get; set; } = string.Empty;
            public int CodePoint { get; set; }
            public bool HasGlyph { get; set; }
            public FontWeight RequestedWeight { get; set; }
            public FontWeight ActualWeight { get; set; }
        }

        public sealed class TextMetric
        {
            public string Text { get; set; } = string.Empty;
            public double Width { get; set; }
            public double Height { get; set; }
            public double Baseline { get; set; }
            public bool HasUnpairedSurrogate { get; set; }
        }

        public static GlyphCandidate FindCandidate(
            IEnumerable<string> familyChain,
            int codePoint,
            FontWeight requestedWeight)
        {
            if (familyChain == null) throw new ArgumentNullException(nameof(familyChain));
            foreach (var family in familyChain)
            {
                if (TryGetGlyph(family, codePoint, requestedWeight, out var actualWeight))
                {
                    return new GlyphCandidate
                    {
                        Family = family,
                        CodePoint = codePoint,
                        HasGlyph = true,
                        RequestedWeight = requestedWeight,
                        ActualWeight = actualWeight
                    };
                }
            }

            return new GlyphCandidate
            {
                CodePoint = codePoint,
                RequestedWeight = requestedWeight,
                ActualWeight = requestedWeight,
                HasGlyph = false
            };
        }

        public static bool TryGetGlyph(
            string familyName,
            int codePoint,
            FontWeight requestedWeight,
            out FontWeight actualWeight)
        {
            actualWeight = requestedWeight;
            try
            {
                var typeface = new Typeface(
                    new FontFamily(familyName),
                    FontStyles.Normal,
                    requestedWeight,
                    FontStretches.Normal);
                if (!typeface.TryGetGlyphTypeface(out var glyphTypeface)
                    || !glyphTypeface.CharacterToGlyphMap.ContainsKey(codePoint))
                    return false;

                actualWeight = glyphTypeface.Weight;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static TextMetric Measure(string text, string familyName, double fontSize, FontWeight weight)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            var typeface = new Typeface(
                new FontFamily(familyName),
                FontStyles.Normal,
                weight,
                FontStretches.Normal);
            var formatted = new FormattedText(
                text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                Brushes.Black,
                1.0);
            return new TextMetric
            {
                Text = text,
                Width = formatted.WidthIncludingTrailingWhitespace,
                Height = formatted.Height,
                Baseline = formatted.Baseline,
                HasUnpairedSurrogate = ContainsUnpairedSurrogate(text)
            };
        }

        public static bool ContainsUnpairedSurrogate(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            for (var index = 0; index < value.Length; index++)
            {
                var current = value[index];
                if (!char.IsSurrogate(current))
                    continue;

                if (char.IsHighSurrogate(current)
                    && index + 1 < value.Length
                    && char.IsLowSurrogate(value[index + 1]))
                {
                    index++;
                    continue;
                }

                return true;
            }

            return false;
        }

        public static string CodePointText(int codePoint)
        {
            if (codePoint < 0 || codePoint > 0x10FFFF)
                throw new ArgumentOutOfRangeException(nameof(codePoint));
            return char.ConvertFromUtf32(codePoint);
        }
    }
}
