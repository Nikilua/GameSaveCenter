using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
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
        public void MixedChineseLatinDateAndCapacityRunsShareOneBaseline()
        {
            TypographyDiagnostics.MixedBaselineEvidence[] evidence = null!;
            RunSta(() =>
            {
                evidence = new[]
                {
                    TypographyDiagnostics.CaptureMixedBaseline("存档中心 Save Center", TypographyDiagnostics.UiFontChain, 14, FontWeights.Normal),
                    TypographyDiagnostics.CaptureMixedBaseline("日期：2026-09-17 · 时间 03:02", TypographyDiagnostics.UiFontChain, 14, FontWeights.Normal),
                    TypographyDiagnostics.CaptureMixedBaseline("容量：1.71 GiB · 24.6 MiB", TypographyDiagnostics.UiFontChain, 14, FontWeights.Normal),
                    TypographyDiagnostics.CaptureMixedBaseline("中文标点：全角引号“存档”、书名号《中心》……", TypographyDiagnostics.UiFontChain, 14, FontWeights.Normal)
                };
            });

            Assert.NotNull(evidence);
            Assert.Equal(4, evidence.Length);
            Assert.All(evidence, item =>
            {
                Assert.True(item.GlyphRunCount > 0);
                Assert.True(item.GlyphCount > 0);
                Assert.True(item.IsStable, $"Mixed baseline drifted for '{item.Text}': spread={item.BaselineSpread:0.###}.");
            });
        }

        [Fact]
        public void GlyphRunProbeSeparatesCandidateCoverageFromFinalLayoutEvidence()
        {
            TypographyDiagnostics.GlyphRunEvidence[] evidence = null!;
            RunSta(() =>
            {
                evidence = new[]
                {
                    TypographyDiagnostics.CaptureGlyphRun("存", 0x5B58, TypographyDiagnostics.UiFontChain, 14, FontWeights.Normal),
                    TypographyDiagnostics.CaptureGlyphRun("S", 0x0053, TypographyDiagnostics.UiFontChain, 14, FontWeights.Normal),
                    TypographyDiagnostics.CaptureGlyphRun("9", 0x0039, TypographyDiagnostics.UiFontChain, 14, FontWeights.Normal),
                    TypographyDiagnostics.CaptureGlyphRun(TypographyDiagnostics.CodePointText(0x20BB7), 0x20BB7, TypographyDiagnostics.UiFontChain, 14, FontWeights.Normal),
                    TypographyDiagnostics.CaptureGlyphRun("e\u0301", 0x0301, TypographyDiagnostics.UiFontChain, 14, FontWeights.Normal)
                };
            });

            Assert.NotNull(evidence);
            Assert.Equal(5, evidence.Length);
            Assert.All(evidence, item =>
            {
                if (item.EvidenceLevel == "GlyphRunCaptured")
                {
                    Assert.NotEmpty(item.FinalFamily);
                    Assert.True(item.FinalTypefaceHasCodePoint);
                }
                if (item.EvidenceLevel == "CandidateOnly" || item.EvidenceLevel == "Unknown")
                    Assert.False(item.HasGlyphRun);
            });

            Assert.Contains(evidence, item => item.CodePoint == 0x5B58 && item.HasGlyphRun);
            Assert.Contains(evidence, item => item.CodePoint == 0x0053 && item.HasGlyphRun);
            Assert.Contains(evidence, item => item.CodePoint == 0x0039 && item.HasGlyphRun);
            Assert.DoesNotContain(evidence, item => item.EvidenceLevel == "GlyphRunCaptured" && item.HasNotdefGlyph);
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

        [Fact]
        public void ControlledReportsKeepPunctuationAndWeightEvidenceTruthful()
        {
            var root = FindRepositoryRoot();
            var reports = new[]
            {
                File.ReadAllText(Path.Combine(root, "docs", "design", "reviews", "ui-finesse-round2-20260913", "evidence", "q01", "dark", "ui-finesse-fixture-report.txt")),
                File.ReadAllText(Path.Combine(root, "docs", "design", "reviews", "ui-finesse-round2-20260913", "evidence", "q01", "light", "ui-finesse-fixture-report.txt"))
            };

            foreach (var report in reports)
            {
                Assert.Contains("PunctuationSamples: preserved=全角引号“存档” | 《存档中心》 | 路径——待检查 | 稍后重试…… containsUnpairedSurrogate=False", report);
                Assert.Contains("FontWeightCandidate Chinese requested=SemiBold family=Noto Sans SC actual=Bold", report);
                Assert.Contains("FontActualGlyphRun: unknown", report);
            }
        }

        [Fact]
        public void ProductionColumnsKeepNumericAndPathSemantics()
        {
            var root = FindRepositoryRoot();
            var save = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
            var media = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
            var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
            var typography = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "Typography.xaml"));
            var productionControls = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "WpfUiProduction.xaml"));
            var settings = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml"));
            var trainer = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TrainerCenterView.xaml"));

            Assert.Contains("x:Name=\"SaveHistorySizeColumn\"", save);
            Assert.Contains("Width=\"116\"", save);
            Assert.Contains("x:Key=\"SaveCountValue\"", save);
            Assert.Contains("BasedOn=\"{StaticResource SaveSizeValue}\"", save);
            Assert.Contains("BasedOn=\"{StaticResource SaveCountValue}\"", save);
            Assert.Contains("BasedOn=\"{StaticResource SavePathText}\"", save);
            Assert.Contains("ToolTip\" Value=\"{Binding OriginalPath}\"", media);
            Assert.Contains("Header=\"拍摄时间\"", media);
            Assert.Contains("TargetNullValue=—", media);
            Assert.Contains("StringFormat={}{0} 项", maintenance);
            Assert.Contains("x:Key=\"GscPathText\"", typography);
            Assert.Contains("BasedOn=\"{StaticResource GscTypographyCode}\"", typography);
            Assert.Contains("x:Key=\"GscWpfUiPathTextBox\"", productionControls);
            Assert.Contains("FontFamily\" Value=\"{DynamicResource GscCodeFontFamily}\"", productionControls);
            Assert.Equal(7, settings.Split(new[] { "Style=\"{StaticResource GscWpfUiPathTextBox}\"" }, StringSplitOptions.None).Length - 1);
            Assert.Contains("x:Key=\"MediaPathText\"", media);
            Assert.Contains("x:Key=\"MaintenancePathText\"", maintenance);
            Assert.Contains("Style=\"{StaticResource GscPathText}\"", save);
            Assert.Contains("BasedOn=\"{StaticResource GscPathText}\"", trainer);
            Assert.Contains("Style=\"{DynamicResource GscWpfUiPathTextBox}\"", trainer);
        }

        [Fact]
        public void ImportantTrainerDiagnosticsUseReadableSharedStyles()
        {
            var root = FindRepositoryRoot();
            var trainer = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TrainerCenterView.xaml"));

            Assert.Contains("x:Key=\"TrainerDiagnosticLabel\"", trainer);
            Assert.Contains("x:Key=\"TrainerDiagnosticPath\"", trainer);
            Assert.Contains("x:Key=\"TrainerDiagnosticWarning\"", trainer);
            Assert.Contains("Text=\"名称\" Style=\"{StaticResource TrainerDiagnosticLabel}\"", trainer);
            Assert.Contains("SelectedGameToolVersion.EntryPath, TargetNullValue=未选择版本}\" Style=\"{StaticResource TrainerDiagnosticPath}\"", trainer);
            Assert.Contains("SelectedGameTool.AutoStartRiskHint}\" Style=\"{StaticResource TrainerDiagnosticWarning}\"", trainer);
            Assert.DoesNotContain("Text=\"名称\" Foreground=\"{DynamicResource GscSecondaryTextBrush}\" FontSize=\"10\"", trainer);
            Assert.DoesNotContain("Text=\"启动延迟\" Foreground=\"{DynamicResource GscSecondaryTextBrush}\" FontSize=\"10\"", trainer);
        }

        [Fact]
        public void ProductionTenAndElevenPointTextUsesSharedCaptionToken()
        {
            var root = FindRepositoryRoot();
            var productionRoots = new[]
            {
                Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views"),
                Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings")
            };
            var files = productionRoots
                .SelectMany(path => Directory.EnumerateFiles(path, "*.xaml", SearchOption.AllDirectories))
                .Where(path => path.IndexOf(Path.DirectorySeparatorChar + "Development" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) < 0)
                .ToArray();

            Assert.NotEmpty(files);
            foreach (var file in files)
            {
                var source = File.ReadAllText(file);
                Assert.DoesNotContain("FontSize=\"10\"", source);
                Assert.DoesNotContain("FontSize=\"11\"", source);
            }

            var productionSources = files.Select(File.ReadAllText).ToArray();
            Assert.Contains(productionSources, source => source.IndexOf("FontSize=\"{DynamicResource GscCaptionFontSize}\"", StringComparison.Ordinal) >= 0);
        }

        [Fact]
        public void ProductionTypographyUsesSharedHierarchyWithoutMicroSizeDrift()
        {
            var root = FindRepositoryRoot();
            var productionRoots = new[]
            {
                Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views"),
                Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings")
            };
            var files = productionRoots
                .SelectMany(path => Directory.EnumerateFiles(path, "*.xaml", SearchOption.AllDirectories))
                .Where(path => path.IndexOf(Path.DirectorySeparatorChar + "Development" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) < 0)
                .ToArray();

            var microVariants = files
                .SelectMany(file => File.ReadAllLines(file)
                    .Select((line, index) => new { File = file, Line = index + 1, Text = line })
                    .Where(item => item.Text.IndexOf("FontSize=\"9.5\"", StringComparison.Ordinal) >= 0
                                   || item.Text.IndexOf("FontSize=\"10.5\"", StringComparison.Ordinal) >= 0
                                   || item.Text.IndexOf("FontSize=\"12.5\"", StringComparison.Ordinal) >= 0))
                .ToArray();

            Assert.Empty(microVariants);

            var shell = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml"));
            var media = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
            var overview = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml"));
            var save = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));

            Assert.Contains("x:Name=\"SidebarBrandText\" Text=\"GameSaveCenter\" FontSize=\"{DynamicResource GscBodyFontSize}\"", shell);
            Assert.Contains("x:Name=\"SidebarProductionVersionText\" Text=\"v0.6.73\" FontSize=\"{DynamicResource GscCaptionFontSize}\"", shell);
            Assert.Contains("Text=\"{Binding FileName}\" FontSize=\"{DynamicResource GscBodyFontSize}\"", media);
            Assert.Contains("StringFormat={}{0:MM-dd HH:mm}}\" FontSize=\"{DynamicResource GscCaptionFontSize}\"", media);
            Assert.Contains("Text=\"{Binding TaskTypeDisplay, Mode=OneWay}\" Foreground=\"{DynamicResource GscPrimaryTextBrush}\" FontSize=\"{DynamicResource GscBodyFontSize}\"", overview);
            Assert.Contains("Text=\"{Binding DetailMessage, Mode=OneWay}\" FontSize=\"{DynamicResource GscCaptionFontSize}\"", overview);
            Assert.Contains("ComparisonQualityDisplay, TargetNullValue=等待比较, FallbackValue=等待比较}\" Foreground=\"{DynamicResource GscInfoBrush}\" FontSize=\"{DynamicResource GscCaptionFontSize}\"", save);
        }

        private static string FindRepositoryRoot()
            => TestRepositoryContext.Root;

        private static void RunSta(Action action)
        {
            Exception? failure = null;
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                    Dispatcher.CurrentDispatcher.InvokeShutdown();
                }
                catch (Exception exception)
                {
                    failure = exception;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (failure != null)
                throw new Xunit.Sdk.XunitException(failure.ToString());
        }
    }
}
