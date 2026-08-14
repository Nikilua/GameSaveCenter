using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class UiLayoutRegressionTests
    {
        [Fact]
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
            Assert.Contains("<Setter Property=\"Height\" Value=\"42\"/>", production);
            Assert.Contains("VerticalAlignment=\"Center\"", tokens);
            Assert.Contains("<Setter Property=\"Height\" Value=\"42\"/>", tokens);
            Assert.Contains("Padding=\"0\"", redesign);
            Assert.Contains("CornerRadius=\"14\"", redesign);
        }

        [Fact]
        public void OverviewProtectionActionsHaveTheirOwnResponsiveRow()
        {
            var root = FindRepositoryRoot();
            var overview = XDocument.Parse(File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml")));
            var protection = overview.Descendants().Single(element => element.Name.LocalName == "ItemsControl" && element.Attribute("ItemsSource")?.Value == "{Binding RecentProtection.Items}");
            var actions = overview.Descendants()
                .Single(element => element.Name.LocalName == "WrapPanel"
                    && element.Attribute("Grid.Row")?.Value == "1"
                    && element.Descendants().Any(descendant => descendant.Attribute("Command")?.Value == "{Binding OpenProtectionGamesCommand}")
                    && element.Descendants().Any(descendant => descendant.Attribute("Command")?.Value == "{Binding ApplyRecommendedProtectionCommand}"));

            Assert.Equal("1", actions.Attribute("Grid.Row")?.Value);
        }

        [Fact]
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

        [Fact]
        public void OverviewGlobalActivityTimelineUsesCuratedBusinessEvents()
        {
            var root = FindRepositoryRoot();
            var overview = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml"));

            Assert.Contains("x:Name=\"OverviewActivityTimelineList\"", overview);
            Assert.Contains("ItemsSource=\"{Binding Activities}\"", overview);
            Assert.Contains("Text=\"全局活动\"", overview);
            Assert.Contains("{Binding KindDisplay, Mode=OneWay}", overview);
            Assert.Contains("{Binding ResultDisplay, Mode=OneWay}", overview);
            Assert.Contains("MaxHeight=\"240\"", overview);
        }

        [Fact]
        public void OverviewStatStripUsesResponsiveCompactSummaryColumns()
        {
            var root = FindRepositoryRoot();
            var overview = XDocument.Parse(File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml")));
            var xamlName = XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml");
            var strip = overview.Descendants().Single(element => element.Attribute(xamlName)?.Value == "OverviewStatStrip");
            var code = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml.cs"));

            Assert.Equal("6", strip.Attribute("Columns")?.Value);
            Assert.Equal(6, strip.Elements().Count(element => element.Name.LocalName == "Border"));
            Assert.Contains("OverviewStatStrip.Columns = primaryWidth >= 1100 ? 6 : primaryWidth >= 620 ? 3 : 2", code);
        }

        [Fact]
        public void OverviewRecentProtectionDetailsAreCollapsibleWithoutLosingItems()
        {
            var root = FindRepositoryRoot();
            var overview = XDocument.Parse(File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml")));
            var expander = overview.Descendants().Single(element => element.Name.LocalName == "Expander"
                && element.Attribute("Header")?.Value == "展开最近游戏保护明细");

            Assert.NotNull(expander.Descendants().SingleOrDefault(element => element.Name.LocalName == "ItemsControl"
                && element.Attribute("ItemsSource")?.Value == "{Binding RecentProtection.Items}"));
            Assert.NotNull(expander.Descendants().SingleOrDefault(element => element.Name.LocalName == "TextBlock"
                && (element.Attribute("Text")?.Value ?? "").Contains("选择游戏不会自动执行备份或恢复")));
        }

        [Fact]
        public void OverviewGlobalActivityUsesStableFourColumnRow()
        {
            var root = FindRepositoryRoot();
            var overview = XDocument.Parse(File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml")));
            var xamlName = XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml");
            var timeline = overview.Descendants().Single(element => element.Name.LocalName == "ItemsControl"
                && element.Attribute(xamlName)?.Value == "OverviewActivityTimelineList");
            var template = timeline.Descendants().Single(element => element.Name.LocalName == "DataTemplate");
            var grid = template.Elements().Single(element => element.Name.LocalName == "Grid");
            var widths = grid.Elements().Single(element => element.Name.LocalName == "Grid.ColumnDefinitions")
                .Elements().Select(element => element.Attribute("Width")?.Value).ToArray();

            Assert.Equal(new[] { "38", "*", "110", "92" }, widths);
            Assert.Equal("220", grid.Elements().Single(element => element.Name.LocalName == "Grid.ColumnDefinitions")
                .Elements().ElementAt(1).Attribute("MinWidth")?.Value);
            Assert.Contains(template.Descendants(), element => element.Name.LocalName == "TextBlock"
                && element.Attribute("Text")?.Value == "{Binding KindDisplay, Mode=OneWay}");
            Assert.Contains(template.Descendants(), element => element.Name.LocalName == "TextBlock"
                && element.Attribute("Text")?.Value == "{Binding ResultDisplay, Mode=OneWay}");
            Assert.Contains(template.Descendants(), element => element.Name.LocalName == "TextBlock"
                && element.Attribute("Text")?.Value == "{Binding CreatedDisplay, Mode=OneWay}");
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
            Assert.Contains("tab.MinHeight = compact ? 50 : shortHeight ? 60 : 72;", code);
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
