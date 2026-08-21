using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class UiLayoutRegressionTests
    {
        [LegacyProductionUiBaselineFact]
        public void OverviewHeroStatusPillsUseAFullWidthSecondRow()
        {
            var root = FindRepositoryRoot();
            var overview = XDocument.Parse(File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml")));
            var xamlName = XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml");
            var hero = overview.Descendants().Single(element => element.Attribute(xamlName)?.Value == "OverviewTodayHeroCard");
            var grid = hero.Elements().Single(element => element.Name.LocalName == "Grid");
            var rows = grid.Descendants().Where(element => element.Name.LocalName == "RowDefinition")
                .ToList();
            var statusRow = grid.Descendants().Single(element => element.Name.LocalName == "WrapPanel" && element.Attribute("Grid.Row")?.Value == "1");

            Assert.Equal(2, rows.Count);
            Assert.Equal("Left", statusRow.Attribute("HorizontalAlignment")?.Value);
            Assert.Null(statusRow.Attribute("Grid.Column"));
            Assert.Null(statusRow.Attribute("Grid.ColumnSpan"));
            Assert.Contains("OverviewTodayHeroCard.Padding", File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml.cs")));
        }

        [Fact]
        public void MaintenanceEnvironmentChecksUsePredictableResponsiveColumns()
        {
            var root = FindRepositoryRoot();
            var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
            var code = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml.cs"));

            Assert.Contains("x:Name=\"EnvironmentCheckItems\"", maintenance);
            Assert.Contains("<UniformGrid Columns=\"3\"/>", maintenance);
            Assert.Contains("width >= 900 ? 3 : width >= 620 ? 2 : 1", code);
            Assert.Contains("FindVisualChild<UniformGrid>(EnvironmentCheckItems)", code);
            Assert.Contains("using System.Windows.Media;", code);
        }

        [Fact]
        public void MaintenanceStorageAnalysisUsesResponsiveMetricsAndRealCommand()
        {
            var root = FindRepositoryRoot();
            var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
            var code = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml.cs"));

            Assert.Contains("x:Name=\"MaintenanceStorageMetrics\"", maintenance);
            Assert.Contains("x:Name=\"MaintenanceStorageTrendPanel\"", maintenance);
            Assert.Contains("Command=\"{Binding RefreshStorageAnalysisCommand}\"", maintenance);
            Assert.Contains("StorageAnalysis.Summary", maintenance);
            Assert.Contains("StorageAnalysis.TopGames", maintenance);
            Assert.Contains("MaintenanceStorageMetrics.Columns = width >= 900 ? 4 : width >= 620 ? 2 : 1", code);
            Assert.Contains("MaintenanceStorageTrendPanel.Columns = width >= 720 ? 3 : 1", code);
        }

        [Fact]
        public void MaintenanceRetentionSimulationUsesResponsiveMetricsAndConfirmedApply()
        {
            var root = FindRepositoryRoot();
            var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
            var code = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml.cs"));

            Assert.Contains("x:Name=\"MaintenanceRetentionSimulationMetrics\"", maintenance);
            Assert.Contains("x:Name=\"MaintenanceRetentionSimulationProtectionMetrics\"", maintenance);
            Assert.Contains("Command=\"{Binding RefreshRetentionSimulationCommand}\"", maintenance);
            Assert.Contains("Command=\"{Binding ApplyRetentionSimulationCommand}\"", maintenance);
            Assert.Contains("RetentionSimulation.Summary", maintenance);
            Assert.Contains("RetentionSimulation.Items", maintenance);
            Assert.Contains("MaintenanceRetentionSimulationMetrics.Columns = width >= 900 ? 4 : width >= 620 ? 2 : 1", code);
            Assert.Contains("MaintenanceRetentionSimulationProtectionMetrics.Columns = width >= 720 ? 3 : 1", code);
        }

        [Fact]
        public void SettingsAndMaintenanceExposeLocalMirrorConfiguration()
        {
            var root = FindRepositoryRoot();
            var settings = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml"));
            var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
            var code = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml.cs"));

            Assert.Contains("IsChecked=\"{Binding EnableLocalMirror}\"", settings);
            Assert.Contains("Text=\"{Binding LocalMirrorPath", settings);
            Assert.Contains("Command=\"{Binding RefreshLocalMirrorStatusCommand}\"", maintenance);
            Assert.Contains("Command=\"{Binding SyncLocalMirrorCommand}\"", maintenance);
            Assert.Contains("x:Name=\"MaintenanceLocalMirrorMetrics\"", maintenance);
            Assert.Contains("MaintenanceLocalMirrorMetrics.Columns = width >= 720 ? 3 : 1", code);
        }

        [Fact]
        public void SharedInputChromeKeepsRoundedBoundsAndCenteredContent()
        {
            var root = FindRepositoryRoot();
            var production = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "WpfUiProduction.xaml"));
            var tokens = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "DesignTokens.xaml"));
            var redesign = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "Redesign.xaml"));

            Assert.Contains("CornerRadius=\"10\"", production);
            Assert.Contains("VerticalAlignment=\"Center\"", production);
            Assert.Contains("<Setter Property=\"Height\" Value=\"{DynamicResource GscButtonHeight}\"/>", production);
            Assert.Contains("VerticalAlignment=\"Center\"", tokens);
            Assert.Contains("<Setter Property=\"Height\" Value=\"{DynamicResource GscButtonHeight}\"/>", tokens);
            Assert.Contains("Padding=\"0\"", redesign);
            Assert.Contains("CornerRadius=\"14\"", redesign);
        }

        [LegacyProductionUiBaselineFact]
        public void OverviewProtectionActionsHaveTheirOwnResponsiveRow()
        {
            var root = FindRepositoryRoot();
            var overview = XDocument.Parse(File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml")));
            var xamlName = XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml");
            var protection = overview.Descendants().Single(element => element.Name.LocalName == "ListBox"
                && element.Attribute(xamlName)?.Value == "OverviewProtectionPreviewItems");
            var actions = overview.Descendants()
                .Single(element => element.Name.LocalName == "WrapPanel"
                    && element.Attribute("Grid.Row")?.Value == "2"
                    && element.Descendants().Any(descendant => descendant.Attribute("Command")?.Value == "{Binding OpenProtectionGamesCommand}")
                    && element.Descendants().Any(descendant => descendant.Attribute("Command")?.Value == "{Binding ApplyRecommendedProtectionCommand}"));

            Assert.Equal("2", actions.Attribute("Grid.Row")?.Value);
            Assert.Equal("Multiple", protection.Attribute("SelectionMode")?.Value);
            Assert.Equal("OnProtectionSelectionChanged", protection.Attribute("SelectionChanged")?.Value);
        }

        [LegacyProductionUiBaselineFact]
        public void MediaCurrentListStartsAtTheTopOfItsVirtualizedViewport()
        {
            var root = FindRepositoryRoot();
            var media = XDocument.Parse(File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml")));
            var xamlName = XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml");
            var list = media.Descendants().Single(element => element.Name.LocalName == "ListBox" && element.Attribute(xamlName)?.Value == "MediaGrid");
            var itemsPanel = list.Descendants().Single(element => element.Name.LocalName == "VirtualizingStackPanel");

            Assert.Equal("Stretch", list.Attribute("HorizontalContentAlignment")?.Value);
            Assert.Equal("Top", list.Attribute("VerticalContentAlignment")?.Value);
            Assert.Equal("Top", itemsPanel.Attribute("VerticalAlignment")?.Value);
        }

        [Fact]
        public void MediaSourceFormIsCollapsibleButKeepsFieldsReachable()
        {
            var root = FindRepositoryRoot();
            var media = XDocument.Parse(File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml")));
            var expander = media.Descendants().Single(element => element.Name.LocalName == "Expander"
                && element.Attribute("Header")?.Value == "添加截图或录像来源");

            Assert.Equal("False", expander.Attribute("IsExpanded")?.Value);
            Assert.NotNull(expander.Descendants().SingleOrDefault(element => element.Attribute(XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml"))?.Value == "MediaSourceFields"));
            Assert.Contains("Command=\"{Binding AddMediaSourceCommand}\"", expander.ToString());
        }

        [LegacyProductionUiBaselineFact]
        public void OverviewGlobalActivityTimelineUsesCuratedBusinessEvents()
        {
            var root = FindRepositoryRoot();
            var overview = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml"));

            Assert.Contains("x:Name=\"OverviewActivityTimelineList\"", overview);
            Assert.Contains("ItemsSource=\"{Binding Activities}\"", overview);
            Assert.Contains("Text=\"全局活动\"", overview);
            Assert.Contains("{Binding KindDisplay, Mode=OneWay}", overview);
            Assert.Contains("{Binding ResultDisplay, Mode=OneWay}", overview);
            Assert.DoesNotContain("MaxHeight=\"240\"", overview);
            Assert.Contains("ScrollViewer.VerticalScrollBarVisibility=\"Disabled\"", overview);
        }

        [LegacyProductionUiBaselineFact]
        public void OverviewStatStripUsesTheDemoContinuousSummaryStructure()
        {
            var root = FindRepositoryRoot();
            var overview = XDocument.Parse(File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml")));
            var xamlName = XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml");
            var strip = overview.Descendants().Single(element => element.Attribute(xamlName)?.Value == "OverviewStatStrip");
            var code = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml.cs"));

            Assert.Equal("Border", strip.Name.LocalName);
            Assert.Contains(strip.Descendants(), element => element.Name.LocalName == "Grid");
            Assert.Equal(6, strip.Descendants().Count(element => element.Name.LocalName == "Border" && element.Attribute("Style")?.Value == "{StaticResource OverviewStatCard}"));
            Assert.Equal(5, strip.Descendants().Count(element => element.Name.LocalName == "Rectangle" && element.Attribute("Fill")?.Value == "{DynamicResource GscTableDividerBrush}"));
            Assert.DoesNotContain("OverviewStatStrip.Columns", code);
        }

        [LegacyProductionUiBaselineFact]
        public void OverviewFlowUsesTheFiniteViewportWhenHorizontalScrollingIsDisabled()
        {
            var root = FindRepositoryRoot();
            var overview = XDocument.Parse(File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml")));
            var xamlName = XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml");
            var scrollSurface = overview.Descendants().Single(element => element.Attribute(xamlName)?.Value == "OverviewStackScrollSurface");
            var layout = overview.Descendants().Single(element => element.Attribute(xamlName)?.Value == "OverviewLayoutGrid");

            // WPF can measure a ScrollViewer child with an unbounded horizontal constraint
            // even when the horizontal bar is disabled. The page must bind its flow to the
            // finite viewport, otherwise star columns can place the real current-game
            // actions outside the host window.
            Assert.Equal("Disabled", scrollSurface.Attribute("HorizontalScrollBarVisibility")?.Value);
            Assert.Equal("{Binding ViewportWidth, ElementName=OverviewStackScrollSurface}", layout.Attribute("Width")?.Value);
            Assert.Equal("Left", layout.Attribute("HorizontalAlignment")?.Value);
        }

        [Fact]
        public void OverviewRecentProtectionCardsMatchDemoWithoutCheckboxOrPerItemAction()
        {
            var root = FindRepositoryRoot();
            var overview = XDocument.Parse(File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml")));
            var xamlName = XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml");
            var cards = overview.Descendants().Single(element => element.Name.LocalName == "ListBox"
                && element.Attribute(xamlName)?.Value == "OverviewProtectionPreviewItems");
            var template = cards.Descendants().Single(element => element.Name.LocalName == "DataTemplate");

            Assert.Equal("{Binding RecentProtection.Items}", cards.Attribute("ItemsSource")?.Value);
            Assert.NotNull(template.Descendants().SingleOrDefault(element => element.Name.LocalName == "TextBlock"
                && element.Attribute("Text")?.Value == "{Binding GameName, Mode=OneWay}"));
            Assert.NotNull(template.Descendants().SingleOrDefault(element => element.Name.LocalName == "TextBlock"
                && element.Attribute("Text")?.Value == "{Binding StatusDisplay, Mode=OneWay}"));
            Assert.DoesNotContain(template.Descendants(), element => element.Name.LocalName == "CheckBox");
            Assert.DoesNotContain(template.Descendants(), element => element.Name.LocalName == "Button");
            Assert.DoesNotContain(overview.Descendants(), element => element.Name.LocalName == "Expander"
                && element.Attribute("Header")?.Value == "展开最近游戏保护明细");
        }

        [LegacyProductionUiBaselineFact]
        public void OverviewGlobalActivityUsesStableFourColumnRow()
        {
            var root = FindRepositoryRoot();
            var overview = XDocument.Parse(File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml")));
            var xamlName = XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml");
            var timeline = overview.Descendants().Single(element => element.Name.LocalName == "ItemsControl"
                && element.Attribute(xamlName)?.Value == "OverviewActivityTimelineList");
            var template = timeline.Descendants().Single(element => element.Name.LocalName == "DataTemplate");
            var grid = template.Descendants().Single(element => element.Name.LocalName == "Grid");
            var widths = grid.Elements().Single(element => element.Name.LocalName == "Grid.ColumnDefinitions")
                .Elements().Select(element => element.Attribute("Width")?.Value).ToArray();

            Assert.Equal(new[] { "64", "*", "Auto", "96" }, widths);
            Assert.Equal("140", grid.Elements().Single(element => element.Name.LocalName == "Grid.ColumnDefinitions")
                .Elements().ElementAt(1).Attribute("MinWidth")?.Value);
            Assert.Equal("Center", template.Descendants().Single(element => element.Name.LocalName == "Border"
                && element.Attribute(xamlName)?.Value == "ActivityKindPill").Attribute("VerticalAlignment")?.Value);
            Assert.DoesNotContain("OverviewActivityHeaderRow", overview.ToString());
            Assert.Contains("Margin=\"8,0,20,0\"", template.Descendants().Single(element =>
                element.Name.LocalName == "TextBlock" && element.Attribute("Text")?.Value == "{Binding CreatedDisplay, Mode=OneWay}").ToString());
            Assert.DoesNotContain("ActivityMetaCompact", overview.ToString());
            var activityTextStack = template.Descendants().Single(element =>
                element.Name.LocalName == "StackPanel" && element.Attribute("Grid.Column")?.Value == "1");
            Assert.Contains(activityTextStack.Descendants(), element => element.Name.LocalName == "TextBlock"
                && element.Attribute("Text")?.Value == "{Binding Summary, Mode=OneWay}");
            Assert.DoesNotContain(template.Descendants(), element => element.Name.LocalName == "Border"
                && (element.Attribute(xamlName)?.Value == "ActivityKindChip"));
            Assert.Contains(template.Descendants(), element => element.Name.LocalName == "TextBlock"
                && element.Attribute("Text")?.Value == "{Binding KindDisplay, Mode=OneWay}");
            Assert.Contains(template.Descendants(), element => element.Name.LocalName == "TextBlock"
                && element.Attribute("Text")?.Value == "{Binding ResultDisplay, Mode=OneWay}");
            Assert.Contains(template.Descendants(), element => element.Name.LocalName == "TextBlock"
                && element.Attribute("Text")?.Value == "{Binding CreatedDisplay, Mode=OneWay}");
            Assert.True(template.Descendants()
                .Where(element => element.Name.LocalName == "TextBlock"
                    && element.Attribute("Text")?.Value == "{Binding KindDisplay, Mode=OneWay}")
                .All(element => element.Attribute("HorizontalAlignment")?.Value == "Center"
                    && element.Attribute("TextAlignment")?.Value == "Center"));
            Assert.True(template.Descendants()
                .Where(element => element.Name.LocalName == "TextBlock"
                    && element.Attribute("Text")?.Value == "{Binding ResultDisplay, Mode=OneWay}")
                .All(element => element.Attribute("HorizontalAlignment")?.Value == "Center"
                    && element.Attribute("TextAlignment")?.Value == "Center"));
        }

        [Fact]
        public void SaveCurrentRuleStatusIsOneLineBadgeWithAlignedActions()
        {
            var root = FindRepositoryRoot();
            var save = XDocument.Parse(File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml")));
            var xamlName = XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml");

            var badge = save.Descendants().Single(element => element.Attribute(xamlName)?.Value == "SaveHealthBadge");
            var badgeText = badge.Descendants().Single(element => element.Name.LocalName == "TextBlock");
            var runs = badgeText.Elements().Where(element => element.Name.LocalName == "Run").ToList();

            Assert.Equal(2, runs.Count);
            Assert.Equal("校验状态：", runs[0].Attribute("Text")?.Value);
            Assert.Contains("SelectedGame.HealthStateDisplay", runs[1].Attribute("Text")?.Value);
            var xamlKey = XName.Get("Key", "http://schemas.microsoft.com/winfx/2006/xaml");
            Assert.Contains("Value=\"Risk\"", save.Descendants()
                .Single(element => element.Name.LocalName == "Style" && element.Attribute(xamlKey)?.Value == "SaveHealthStateBadge")
                .ToString());

            var actions = save.Descendants().Single(element => element.Attribute(xamlName)?.Value == "SaveCurrentRuleActions");
            var buttons = actions.Elements().Where(element => element.Name.LocalName == "Button").ToList();
            Assert.Equal(3, buttons.Count);
            Assert.All(buttons, button =>
            {
                Assert.Contains("GscWpfUiCompactButton", button.Attribute("Style")?.Value);
                Assert.Null(button.Attribute("MinHeight"));
                Assert.Equal("100", button.Attribute("MinWidth")?.Value);
            });
        }

        [Fact]
        public void ViewsUseUnifiedDisclosureCardChrome()
        {
            var root = FindRepositoryRoot();
            var views = new[]
            {
                Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml"),
                Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"),
                Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TrainerCenterView.xaml"),
                Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"),
                Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"),
                Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"),
                Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml")
            };

            foreach (var view in views)
                Assert.DoesNotContain("GscExpander", File.ReadAllText(view));
        }

        [Fact]
        public void LaunchDelayEditorExplainsItsUnitAndUsesCompactHeight()
        {
            var root = FindRepositoryRoot();
            var trainer = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TrainerCenterView.xaml"));

            Assert.Contains("x:Key=\"TrainerCompactNumericTextBox\"", trainer);
            Assert.Contains("Text=\"启动延迟\"", trainer);
            Assert.Contains("Text=\"秒\"", trainer);
            Assert.Contains("Style=\"{StaticResource TrainerCompactNumericTextBox}\"", trainer);
        }

        [Fact]
        public void SettingsCategoryCardsKeepRoundedCornersVisibleAtShortHeights()
        {
            var root = FindRepositoryRoot();
            var settings = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml"));
            var code = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml.cs"));
            var redesign = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "Redesign.xaml"));

            Assert.Contains("SettingsCard", settings);
            Assert.Contains("x:Name=\"SettingsHeaderScroller\"", redesign);
            Assert.Contains("x:Name=\"SettingsHeaderItemsHost\"", redesign);
            Assert.Contains("Width=\"248\"", redesign);
            Assert.Contains("Padding=\"0\"", redesign);
            Assert.Contains("Padding=\"0,0,4,18\"", redesign);
            Assert.Contains("CornerRadius=\"14\"", redesign);
            Assert.Contains("x:Name=\"SettingsHeaderBottomSafetyZone\"", redesign);
            Assert.Contains("x:Name=\"TabItemRoot\"", redesign);
            Assert.Contains("VerticalAlignment=\"Top\"", redesign);
            Assert.Contains("Margin=\"0,0,0,2\"", redesign);
            Assert.Contains("ClipToBounds=\"False\"", redesign);
            Assert.DoesNotContain("CornerRadius=\"14\" ClipToBounds=\"True\"", redesign);
            Assert.Contains("var shortHeight = height > 0 && height < 760;", code);
            Assert.Contains("SettingsSectionTabs.MinHeight = 0;", code);
            Assert.Contains("SettingsSectionTabs.MaxHeight = narrow ? 180 : 200;", code);
            Assert.Contains("SettingsCompactContentRow.Height = new GridLength(1, GridUnitType.Star);", code);
        }

        [LegacyProductionUiBaselineFact]
        public void TypographyUsesCentralizedChineseAwareFontChain()
        {
            var root = FindRepositoryRoot();
            var playniteRoot = Path.Combine(root, "src", "GameSaveCenter.Playnite");
            var xamlFiles = Directory.GetFiles(playniteRoot, "*.xaml", SearchOption.AllDirectories);
            var tokens = File.ReadAllText(Path.Combine(playniteRoot, "Themes", "DesignTokens.xaml"));
            var production = File.ReadAllText(Path.Combine(playniteRoot, "Themes", "WpfUiProduction.xaml"));

            Assert.Contains("x:Key=\"GscUiFontFamily\"", tokens);
            Assert.Contains("x:Key=\"GscDisplayFontFamily\">Segoe UI Variable Display, Segoe UI, Microsoft YaHei UI", tokens);
            Assert.Contains("x:Key=\"GscCodeFontFamily\"", tokens);
            Assert.Contains("Microsoft YaHei UI", tokens);
            foreach (var file in xamlFiles)
            {
                var text = File.ReadAllText(file);
                Assert.DoesNotContain("FontFamily=\"Segoe UI Variable Text, Segoe UI\"", text);
                Assert.DoesNotContain("TextElement.FontFamily=\"Segoe UI Variable Text, Segoe UI\"", text);
                Assert.DoesNotContain("Value=\"Segoe UI Variable Text, Segoe UI\"", text);
            }

            var dashboard = File.ReadAllText(Path.Combine(playniteRoot, "Views", "DashboardView.xaml"));
            var productionShell = File.ReadAllText(Path.Combine(playniteRoot, "Views", "AcrylicProductionShellView.xaml"));
            var redesign = File.ReadAllText(Path.Combine(playniteRoot, "Themes", "Redesign.xaml"));
            var overview = File.ReadAllText(Path.Combine(playniteRoot, "Views", "OverviewView.xaml"));
            var maintenance = File.ReadAllText(Path.Combine(playniteRoot, "Views", "MaintenanceView.xaml"));
            Assert.Contains("FontFamily=\"{DynamicResource GscDisplayFontFamily}\"", dashboard);
            Assert.Contains("FontFamily=\"{DynamicResource GscDisplayFontFamily}\"", productionShell);
            Assert.Contains("x:Key=\"GscRedesignHeroTitle\"", redesign);
            Assert.Contains("x:Key=\"GscPageTitleStyle\"", redesign);
            Assert.Contains("<Setter Property=\"FontFamily\" Value=\"{DynamicResource GscDisplayFontFamily}\"/>", redesign);
            Assert.Contains("FontFamily=\"Segoe MDL2 Assets\"", dashboard);
            Assert.Contains("FontFamily=\"Segoe MDL2 Assets\"", overview);
            Assert.Contains("FontFamily=\"Consolas\"", maintenance);

            Assert.Contains("<Setter Property=\"FontWeight\" Value=\"Medium\"/>", production);
            var primaryStyle = production.IndexOf("x:Key=\"GscWpfUiPrimaryButton\"", StringComparison.Ordinal);
            Assert.True(primaryStyle >= 0);
            Assert.Contains("<Setter Property=\"FontWeight\" Value=\"SemiBold\"/>", production.Substring(primaryStyle));
        }

        [LegacyProductionUiBaselineFact]
        public void CompactDetailsButtonsOwnADedicatedActionRow()
        {
            var root = FindRepositoryRoot();
            var playniteRoot = Path.Combine(root, "src", "GameSaveCenter.Playnite");
            var cases = new[]
            {
                ("Views\\SaveCenterView.xaml", "SaveHistoryCompactDetailsButton"),
                ("Views\\SaveCenterView.xaml", "SaveCandidateCompactDetailsButton"),
                ("Views\\TrainerCenterView.xaml", "TrainerToolsCompactDetailsButton"),
                ("Views\\MediaCenterView.xaml", "MediaCompactDetailsButton"),
                ("Views\\TaskCenterView.xaml", "TaskCompactDetailsButton")
            };

            foreach (var (file, name) in cases)
            {
                var text = File.ReadAllText(Path.Combine(playniteRoot, file));
                var segment = text.Substring(text.IndexOf(name, StringComparison.Ordinal));
                if (segment.Length > 700)
                    segment = segment.Substring(0, 700);
                Assert.Contains("Grid.Row=\"1\"", segment);
                Assert.DoesNotContain("VerticalAlignment=\"Top\"", segment);
            }

            var media = File.ReadAllText(Path.Combine(playniteRoot, "Views", "MediaCenterView.xaml"));
            Assert.Contains("<WrapPanel Grid.Row=\"2\" Margin=\"12,10,12,0\">", media);
        }

        [Fact]
        public void SettingsCompactHeaderKeepsBodyViewportBudget()
        {
            var root = FindRepositoryRoot();
            var settings = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml"));
            var code = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml.cs"));

            Assert.Contains("x:Name=\"SettingsIntroDescription\"", settings);
            Assert.Contains("SettingsIntroDescription.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;", code);
            Assert.Contains("SettingsHeaderSubtitle.Visibility = narrow || shortHeight ? Visibility.Collapsed : Visibility.Visible;", code);
            Assert.Contains("SettingsSaveHint.Visibility = narrow || shortHeight ? Visibility.Collapsed : Visibility.Visible;", code);
            Assert.Contains("SettingsHeader.MinHeight = compactHeaderHeight ? 56 : compact ? 68 : 76;", code);
        }

        [Fact]
        public void MaintenanceSeverityColumnsUseSharedContentFitWidth()
        {
            var root = FindRepositoryRoot();
            var tokens = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "DesignTokens.xaml"));
            var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));

            Assert.Contains("x:Key=\"GscSeverityColumnWidth\"", tokens);
            Assert.Contains("GscSeverityColumnWidth\">92<", tokens);
            Assert.Contains("Width=\"{StaticResource GscSeverityColumnWidth}\"", maintenance);
            Assert.DoesNotContain("Header=\"等级\" Width=\"72\"", maintenance);
            Assert.DoesNotContain("Header=\"等级\" Width=\"92\"", maintenance);
        }

        [Fact]
        public void AuditTextFitDetectionCoversShortLabels()
        {
            var root = FindRepositoryRoot();
            var harnessRoot = Path.Combine(root, "tests", "GameSaveCenter.RenderHarness", "UiAudit");
            var analyzer = File.ReadAllText(Path.Combine(harnessRoot, "UiLayoutAnalyzer.cs"));
            var runner = File.ReadAllText(Path.Combine(harnessRoot, "UiAuditRunner.cs"));
            var inspector = File.ReadAllText(Path.Combine(harnessRoot, "UiVisualTreeInspector.cs"));

            Assert.Contains("ComputeUnconstrainedTextWidth", analyzer);
            Assert.Contains("Severity = isTextFit ? \"MEDIUM\" : \"INFO\",", analyzer);
            Assert.Contains("Code = isTextFit ? \"TEXT_FIT\" : \"POSSIBLE_CLIPPING\",", analyzer);
            Assert.Contains("fidelityCodes.Contains(warning.Code)", runner);
            Assert.Contains("element.Visibility == Visibility.Visible", inspector);
        }

        [Fact]
        public void MaintenanceMiddleHeadersRenderThroughSharedStyle()
        {
            var root = FindRepositoryRoot();
            var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
            var redesign = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "Redesign.xaml"));

            Assert.DoesNotContain("Style TargetType=\"DataGridColumnHeader\" BasedOn=\"{StaticResource {x:Type DataGridColumnHeader}}\"", maintenance);
            Assert.Contains("BasedOn=\"{StaticResource GscRedesignWorkspaceDataGrid}\"", maintenance);
            Assert.Contains("Setter Property=\"ColumnHeaderStyle\" Value=\"{StaticResource GscDataGridColumnHeaderStyle}\"", redesign);
        }

        [LegacyProductionUiBaselineFact]
        public void MediaSearchBoxUsesStretchableColumn()
        {
            var root = FindRepositoryRoot();
            var media = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));

            Assert.Contains("x:Name=\"MediaSearchTextBox\" Grid.Column=\"1\" MinWidth=\"160\"", media);
            Assert.Contains("<ColumnDefinition Width=\"*\" MinWidth=\"160\"/>", media);
            Assert.DoesNotContain("<StackPanel Grid.Column=\"0\" Orientation=\"Horizontal\"", media);
        }

        [Fact]
        public void SettingsSelectedCategoryScrollsIntoView()
        {
            var root = FindRepositoryRoot();
            var code = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml.cs"));

            Assert.Contains("ScrollSelectedCategoryIntoView();", code);
            Assert.Contains("ScheduleScrollSelectedCategoryIntoView();", code);
            Assert.Contains("SettingsSectionTabs?.SelectedItem is not ListBoxItem selected", code);
            Assert.Contains("selected.BringIntoView();", code);
        }

        [LegacyProductionUiBaselineFact]
        public void SaveHistoryStatusStaysInNarrowViewport()
        {
            var root = FindRepositoryRoot();
            var xaml = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
            var code = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml.cs"));

            Assert.Contains("x:Name=\"SaveHistoryNoteColumn\"", xaml);
            Assert.Contains("SaveHistoryNoteColumn.Width = narrowHistory", code);
            Assert.Contains("new DataGridLength(0)", code);
            Assert.Contains("SaveHistoryNoteColumn.MinWidth = narrowHistory ? 0 : 180;", code);
        }

        [Fact]
        public void AuditFidelityDetectionsCoverAudit10BlindSpots()
        {
            var root = FindRepositoryRoot();
            var harnessRoot = Path.Combine(root, "tests", "GameSaveCenter.RenderHarness", "UiAudit");
            var analyzer = File.ReadAllText(Path.Combine(harnessRoot, "UiLayoutAnalyzer.cs"));
            var runner = File.ReadAllText(Path.Combine(harnessRoot, "UiAuditRunner.cs"));

            foreach (var code in new[] { "HEADER_CONTENT_FIDELITY", "ACTIVE_TAB_VISIBILITY", "CONTROL_USABILITY_GEOMETRY", "ESSENTIAL_COLUMN_VISIBILITY" })
            {
                Assert.Contains(code, analyzer);
                Assert.Contains(code, runner);
            }
        }

        [Fact]
        public void SaveHistorySizeUsesNoTrimSemanticStyle()
        {
            var root = FindRepositoryRoot();
            var xaml = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));

            Assert.Contains("x:Key=\"SaveSizeValue\"", xaml);
            Assert.Contains("Property=\"TextTrimming\" Value=\"None\"", xaml);
            Assert.Contains("Property=\"Tag\" Value=\"SaveHistorySize\"", xaml);
            Assert.Contains("Header=\"大小\" Binding=\"{Binding SizeDisplay, Mode=OneWay}\" Width=\"116\"", xaml);
            Assert.Contains("BasedOn=\"{StaticResource SaveSizeValue}\"", xaml);
        }

        [Fact]
        public void AuditFidelityDetectionsCoverAudit11BlindSpots()
        {
            var root = FindRepositoryRoot();
            var harnessRoot = Path.Combine(root, "tests", "GameSaveCenter.RenderHarness", "UiAudit");
            var analyzer = File.ReadAllText(Path.Combine(harnessRoot, "UiLayoutAnalyzer.cs"));
            var runner = File.ReadAllText(Path.Combine(harnessRoot, "UiAuditRunner.cs"));

            Assert.Contains("SHORT_SEMANTIC_VALUE_TRIMMING", analyzer);
            Assert.Contains("INTERACTIVE_INSPECTOR_USABILITY", analyzer);
            Assert.Contains("SHORT_SEMANTIC_VALUE_TRIMMING", runner);
            Assert.Contains("INTERACTIVE_INSPECTOR_USABILITY", runner);
        }

        private static string FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
                directory = directory.Parent;
            return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
        }
    }
}
