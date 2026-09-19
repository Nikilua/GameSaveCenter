using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class OvernightClosureV6Tests
    {
        [LegacyProductionUiBaselineFact]
        public void OverviewUsesOnlyTheRootPageScrollSurface()
        {
            var overview = Read("Views", "OverviewView.xaml");
            Assert.Contains("x:Name=\"OverviewStackScrollSurface\"", overview);
            Assert.DoesNotContain("ScrollViewer x:Name=\"OverviewPrimaryScrollSurface\"", overview);
            Assert.DoesNotContain("ScrollViewer x:Name=\"OverviewSecondaryScrollViewer\"", overview);
            Assert.Contains("x:Name=\"OverviewRiskScrollViewer\"", overview);
            Assert.DoesNotContain("ScrollViewer x:Name=\"OverviewRiskScrollViewer\"", overview);
        }

        [Fact]
        public void MaintenanceAndMediaWorkspacesUseFiniteGridsNotParentScrollers()
        {
            var maintenance = Read("Views", "MaintenanceView.xaml");
            var media = Read("Views", "MediaCenterView.xaml");
            Assert.Contains("Grid x:Name=\"MaintenanceDeviceScrollSurface\"", maintenance);
            Assert.Contains("Grid x:Name=\"MaintenanceProcessScrollSurface\"", maintenance);
            Assert.DoesNotContain("ScrollViewer x:Name=\"MaintenanceDeviceScrollSurface\"", maintenance);
            Assert.DoesNotContain("ScrollViewer x:Name=\"MaintenanceProcessScrollSurface\"", maintenance);
            Assert.Contains("Grid x:Name=\"MediaCurrentScrollSurface\"", media);
            Assert.DoesNotContain("ScrollViewer x:Name=\"MediaCurrentScrollSurface\"", media);
        }

        [LegacyProductionUiBaselineFact]
        public void FiltersAndSearchControlsHaveSemanticPrefixes()
        {
            var tasks = Read("Views", "TaskCenterView.xaml");
            var media = Read("Views", "MediaCenterView.xaml");
            Assert.Contains("Text=\"搜索任务…\"", tasks);
            Assert.Contains("Text=\"状态:\"", tasks);
            Assert.Contains("Text=\"游戏:\"", tasks);
            Assert.Contains("Text=\"类型:\"", tasks);
            Assert.Contains("Text=\"搜索当前游戏媒体…\"", media);
            Assert.Contains("Text=\"类型:\"", media);
        }

        [Fact]
        public void MaintenanceHeadersUseSharedThemeResources()
        {
            var production = Read("Themes", "WpfUiProduction.xaml");
            var maintenance = Read("Views", "MaintenanceView.xaml");
            Assert.Contains("x:Key=\"GscDataGridColumnHeaderStyle\"", production);
            Assert.Contains("OverridesDefaultStyle\" Value=\"True\"", production);
            Assert.Contains("TargetType=\"DataGridColumnHeadersPresenter\"", production);
            Assert.Contains("A table frame owns one continuous reading surface", production);
            Assert.Contains("<Setter Property=\"Background\" Value=\"Transparent\"/>", production);
            Assert.Contains("<Setter Property=\"BorderThickness\" Value=\"0\"/>", production);
            Assert.Contains("MaintenanceFirstColumnHeader", maintenance);
            Assert.Contains("MaintenanceLastColumnHeader", maintenance);
        }

        [Fact]
        public void TasksAndMaintenanceKeepGamePickerHidden()
        {
            var dashboard = Read("Views", "DashboardView.xaml.cs");
            Assert.Contains("viewModel.CurrentWorkspace != WorkspaceKind.Tasks", dashboard);
            Assert.Contains("viewModel.CurrentWorkspace != WorkspaceKind.Maintenance", dashboard);
            Assert.Contains("GameSwitcherHost.Visibility = gameScopedWorkspace", dashboard);
        }

        [LegacyProductionUiBaselineFact]
        public void ChipAndCellSpacingUseSharedTokens()
        {
            var redesign = Read("Themes", "Redesign.xaml");
            var production = Read("Themes", "WpfUiProduction.xaml");
            var overview = Read("Views", "OverviewView.xaml");
            Assert.Contains("<Setter Property=\"CornerRadius\" Value=\"7\"/>", redesign);
            Assert.Contains("<Setter Property=\"MinHeight\" Value=\"26\"/>", redesign);
            Assert.Contains("Property=\"Padding\" Value=\"12,8,20,8\"", production);
            Assert.Contains("Margin=\"8,0,20,0\"", overview);
        }

        [Fact]
        public void SaveCandidateScoreUsesVisualProgressBar()
        {
            var save = Read("Views", "SaveCenterView.xaml");
            var document = XDocument.Parse(save);
            var scoreColumn = document.Descendants()
                .Single(element => string.Equals(element.Name.LocalName, "DataGridTemplateColumn", StringComparison.Ordinal)
                    && string.Equals(element.Attribute("Header")?.Value, "可信度", StringComparison.Ordinal));

            Assert.Equal("130", scoreColumn.Attribute("Width")?.Value);
            var progressBar = scoreColumn.Descendants()
                .Single(element => string.Equals(element.Name.LocalName, "ProgressBar", StringComparison.Ordinal));
            Assert.Equal("8", progressBar.Attribute("Height")?.Value);
            Assert.Equal("{Binding Score, Mode=OneWay}", progressBar.Attribute("Value")?.Value);
            Assert.Contains("StringFormat=P0", scoreColumn.ToString(SaveOptions.DisableFormatting));
        }

        private static string Read(string folder, string file)
        {
            var root = FindRepositoryRoot();
            return File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", folder, file));
        }

        private static string FindRepositoryRoot()
            => TestRepositoryContext.Root;
    }
}
