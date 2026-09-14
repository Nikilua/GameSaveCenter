using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class TypographyDiagnosticsTests
    {
        [Fact]
        public void FontChainNamesCoverChineseAndWindowsFallbackFamilies()
        {
            Assert.Contains("Noto Sans SC", TypographyDiagnostics.UiFontChain);
            Assert.Contains("Noto Sans CJK SC", TypographyDiagnostics.UiFontChain);
            Assert.Contains("Microsoft YaHei UI", TypographyDiagnostics.UiFontChain);
            Assert.Contains("Microsoft YaHei", TypographyDiagnostics.UiFontChain);
            Assert.Contains("Segoe UI", TypographyDiagnostics.UiFontChain);
        }

        [Fact]
        public void UnicodeProbeKeepsValidSurrogatePairsAndRejectsDanglingUnits()
        {
            var rareCjk = TypographyDiagnostics.CodePointText(0x20BB7);
            var emoji = TypographyDiagnostics.CodePointText(0x1F9ED);

            Assert.Equal(2, rareCjk.Length);
            Assert.Equal(2, emoji.Length);
            Assert.False(TypographyDiagnostics.ContainsUnpairedSurrogate(rareCjk + "Cafe\u0301"));
            Assert.True(TypographyDiagnostics.ContainsUnpairedSurrogate("dangling\uD842"));
        }

        [Fact]
        public void TypographyMetricsReportBaselineAndNonZeroWidth()
        {
            var candidate = TypographyDiagnostics.FindCandidate(
                TypographyDiagnostics.UiFontChain,
                0x5B58,
                FontWeights.Normal);
            Assert.True(candidate.HasGlyph, "The controlled Windows fixture must resolve a Chinese glyph through the shared chain.");

            var metric = TypographyDiagnostics.Measure(
                "存档中心 Save Center 100 g/j/y",
                candidate.Family,
                14,
                FontWeights.Normal);
            Assert.True(metric.Width > 0);
            Assert.True(metric.Height > 0);
            Assert.True(metric.Baseline > 0);
            Assert.False(metric.HasUnpairedSurrogate);
        }

        [Fact]
        public void SharedTypographyResourcesDeclareReadableLineSpacingAndTabularNumbers()
        {
            var root = FindRepositoryRoot();
            var typography = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "Typography.xaml"));
            var saveCenter = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));

            Assert.Contains("<Setter Property=\"LineHeight\" Value=\"20\"/>", typography);
            Assert.Contains("<Setter Property=\"LineHeight\" Value=\"18\"/>", typography);
            Assert.Contains("<Setter Property=\"Typography.NumeralAlignment\" Value=\"Tabular\"/>", typography);
            Assert.Contains("x:Key=\"SavePathText\"", saveCenter);
            Assert.Contains("Noto Sans CJK SC", typography);
        }

        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
                directory = directory.Parent;
            return directory?.FullName ?? throw new InvalidOperationException("Repository root was not found.");
        }
    }
}
