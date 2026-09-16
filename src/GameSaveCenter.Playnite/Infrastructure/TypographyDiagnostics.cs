using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

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

        public sealed class MixedBaselineEvidence
        {
            public string Text { get; set; } = string.Empty;
            public double LineBaseline { get; set; }
            public double MinimumGlyphBaseline { get; set; }
            public double MaximumGlyphBaseline { get; set; }
            public double BaselineSpread { get; set; }
            public int GlyphRunCount { get; set; }
            public int GlyphCount { get; set; }
            public bool HasUnpairedSurrogate { get; set; }

            public bool IsStable
                => GlyphRunCount > 0
                    && GlyphCount > 0
                    && !HasUnpairedSurrogate
                    && BaselineSpread <= 0.5;
        }

        public sealed class GlyphRunEvidence
        {
            public int CodePoint { get; set; }
            public bool CandidateHasGlyph { get; set; }
            public string CandidateFamily { get; set; } = string.Empty;
            public string FinalFamily { get; set; } = string.Empty;
            public bool HasGlyphRun { get; set; }
            public bool HasNotdefGlyph { get; set; }
            public bool FinalTypefaceHasCodePoint { get; set; }
            public int GlyphRunCount { get; set; }
            public int GlyphCount { get; set; }
            public string EvidenceLevel { get; set; } = "Unknown";
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

        public static MixedBaselineEvidence CaptureMixedBaseline(
            string text,
            IEnumerable<string> familyChain,
            double fontSize,
            FontWeight weight)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (familyChain == null) throw new ArgumentNullException(nameof(familyChain));
            if (fontSize <= 0) throw new ArgumentOutOfRangeException(nameof(fontSize));

            var chain = familyChain.ToArray();
            var typeface = new Typeface(
                new FontFamily(string.Join(", ", chain)),
                FontStyles.Normal,
                weight,
                FontStretches.Normal);
            var runProperties = new ProbeTextRunProperties(typeface, fontSize);
            var source = new ProbeTextSource(text, runProperties);
            var paragraph = new ProbeTextParagraphProperties(runProperties);
            var result = new MixedBaselineEvidence
            {
                Text = text,
                HasUnpairedSurrogate = ContainsUnpairedSurrogate(text)
            };

            using (var formatter = TextFormatter.Create())
            using (var line = formatter.FormatLine(source, 0, 4096, paragraph, null))
            {
                var runs = line.GetIndexedGlyphRuns()
                    .Where(run => run.GlyphRun != null)
                    .Select(run => run.GlyphRun)
                    .ToArray();
                var baselines = runs
                    .Select(run => run.BaselineOrigin.Y)
                    .ToArray();
                result.LineBaseline = line.Baseline;
                result.GlyphRunCount = runs.Length;
                result.GlyphCount = runs.Sum(run => run.GlyphIndices?.Count ?? 0);
                if (baselines.Length > 0)
                {
                    result.MinimumGlyphBaseline = baselines.Min();
                    result.MaximumGlyphBaseline = baselines.Max();
                    result.BaselineSpread = result.MaximumGlyphBaseline - result.MinimumGlyphBaseline;
                }
            }

            return result;
        }

        public static GlyphRunEvidence CaptureGlyphRun(
            string text,
            int codePoint,
            IEnumerable<string> familyChain,
            double fontSize,
            FontWeight weight)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (familyChain == null) throw new ArgumentNullException(nameof(familyChain));
            if (fontSize <= 0) throw new ArgumentOutOfRangeException(nameof(fontSize));

            var chain = familyChain.ToArray();
            var candidate = FindCandidate(chain, codePoint, weight);
            var evidence = new GlyphRunEvidence
            {
                CodePoint = codePoint,
                CandidateHasGlyph = candidate.HasGlyph,
                CandidateFamily = candidate.HasGlyph ? candidate.Family : string.Empty
            };

            var targetIndex = FindCodePointIndex(text, codePoint);
            if (targetIndex < 0)
                return evidence;

            try
            {
                var typeface = new Typeface(
                    new FontFamily(string.Join(", ", chain)),
                    FontStyles.Normal,
                    weight,
                    FontStretches.Normal);
                var runProperties = new ProbeTextRunProperties(typeface, fontSize);
                var source = new ProbeTextSource(text, runProperties);
                var paragraph = new ProbeTextParagraphProperties(runProperties);
                using (var formatter = TextFormatter.Create())
                using (var line = formatter.FormatLine(source, 0, 4096, paragraph, null))
                {
                    var runs = line.GetIndexedGlyphRuns()
                        .Where(run => run.GlyphRun != null
                            && Intersects(
                                run.TextSourceCharacterIndex,
                                run.TextSourceLength,
                                targetIndex,
                                CodePointLength(codePoint)))
                        .Select(run => run.GlyphRun)
                        .ToArray();
                    evidence.HasGlyphRun = runs.Length > 0;
                    evidence.GlyphRunCount = runs.Length;
                    evidence.GlyphCount = runs.Sum(run => run.GlyphIndices?.Count ?? 0);
                    evidence.HasNotdefGlyph = runs.Any(run => run.GlyphIndices != null && run.GlyphIndices.Any(index => index == 0));
                    evidence.FinalFamily = string.Join(
                        " + ",
                        runs.Select(GetFamilyName)
                            .Where(name => !string.IsNullOrWhiteSpace(name))
                            .Distinct(StringComparer.OrdinalIgnoreCase));
                    evidence.FinalTypefaceHasCodePoint = runs.Any(run =>
                        run.GlyphTypeface != null
                        && run.GlyphTypeface.CharacterToGlyphMap.TryGetValue(codePoint, out var glyphIndex)
                        && glyphIndex != 0);
                }
            }
            catch
            {
                return evidence;
            }

            if (!evidence.HasGlyphRun)
                evidence.EvidenceLevel = candidate.HasGlyph ? "CandidateOnly" : "Unknown";
            else if (evidence.HasNotdefGlyph || !evidence.FinalTypefaceHasCodePoint)
                evidence.EvidenceLevel = "GlyphRunNotdefOrUnresolved";
            else
                evidence.EvidenceLevel = "GlyphRunCaptured";
            return evidence;
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

        private static int FindCodePointIndex(string text, int codePoint)
        {
            for (var index = 0; index < text.Length; index++)
            {
                var startIndex = index;
                var current = (int)text[index];
                if (char.IsHighSurrogate(text[index])
                    && index + 1 < text.Length
                    && char.IsLowSurrogate(text[index + 1]))
                {
                    current = char.ConvertToUtf32(text, index);
                    index++;
                }
                if (current == codePoint)
                    return startIndex;
            }

            return -1;
        }

        private static int CodePointLength(int codePoint)
            => codePoint > 0xFFFF ? 2 : 1;

        private static bool Intersects(int firstIndex, int length, int targetIndex, int targetLength)
            => firstIndex < targetIndex + targetLength && targetIndex < firstIndex + length;

        private static string GetFamilyName(GlyphRun run)
        {
            if (run?.GlyphTypeface == null)
                return string.Empty;
            return run.GlyphTypeface.Win32FamilyNames.Values.FirstOrDefault()
                ?? run.GlyphTypeface.FamilyNames.Values.FirstOrDefault()
                ?? string.Empty;
        }

        private sealed class ProbeTextSource : TextSource
        {
            private readonly string _text;
            private readonly TextRunProperties _runProperties;

            public ProbeTextSource(string text, TextRunProperties runProperties)
            {
                _text = text;
                _runProperties = runProperties;
            }

            public override TextRun GetTextRun(int textSourceCharacterIndex)
            {
                if (textSourceCharacterIndex >= _text.Length)
                    return new TextEndOfParagraph(1);
                return new TextCharacters(_text, textSourceCharacterIndex, _text.Length - textSourceCharacterIndex, _runProperties);
            }

            public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int textSourceCharacterIndexLimit)
            {
                var length = Math.Max(0, Math.Min(textSourceCharacterIndexLimit, _text.Length));
                var range = new CharacterBufferRange(_text, 0, length);
                return new TextSpan<CultureSpecificCharacterBufferRange>(
                    length,
                    new CultureSpecificCharacterBufferRange(CultureInfo.InvariantCulture, range));
            }

            public override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int textSourceCharacterIndex)
                => textSourceCharacterIndex;
        }

        private sealed class ProbeTextRunProperties : TextRunProperties
        {
            private readonly Typeface _typeface;
            private readonly double _fontSize;

            public ProbeTextRunProperties(Typeface typeface, double fontSize)
            {
                _typeface = typeface;
                _fontSize = fontSize;
            }

            public override Typeface Typeface => _typeface;
            public override double FontRenderingEmSize => _fontSize;
            public override double FontHintingEmSize => _fontSize;
            public override TextDecorationCollection TextDecorations => null!;
            public override Brush ForegroundBrush => Brushes.Black;
            public override Brush BackgroundBrush => null!;
            public override CultureInfo CultureInfo => CultureInfo.InvariantCulture;
            public override TextEffectCollection TextEffects => null!;
        }

        private sealed class ProbeTextParagraphProperties : TextParagraphProperties
        {
            private readonly TextRunProperties _runProperties;

            public ProbeTextParagraphProperties(TextRunProperties runProperties)
            {
                _runProperties = runProperties;
            }

            public override FlowDirection FlowDirection => FlowDirection.LeftToRight;
            public override TextAlignment TextAlignment => TextAlignment.Left;
            public override double LineHeight => _runProperties.FontRenderingEmSize;
            public override bool FirstLineInParagraph => true;
            public override TextRunProperties DefaultTextRunProperties => _runProperties;
            public override TextWrapping TextWrapping => TextWrapping.NoWrap;
            public override TextMarkerProperties TextMarkerProperties => null!;
            public override double Indent => 0;
        }
    }
}
