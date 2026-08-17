using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class OvernightClosureV6Tests
    {
        [Fact]
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

        [Fact]
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
            Assert.Contains("GscTableHeaderBrush", production);
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

        [Fact]
        public void ChipAndCellSpacingUseSharedTokens()
        {
            var redesign = Read("Themes", "Redesign.xaml");
            var production = Read("Themes", "WpfUiProduction.xaml");
            var overview = Read("Views", "OverviewView.xaml");
            Assert.Contains("<Setter Property=\"CornerRadius\" Value=\"7\"/>", redesign);
            Assert.Contains("<Setter Property=\"MinHeight\" Value=\"26\"/>", redesign);
            Assert.Contains("Property=\"Padding\" Value=\"12,0\"", production);
            Assert.Contains("<UniformGrid x:Name=\"OverviewStatStrip\" Grid.Row=\"1\" Columns=\"6\"", overview);
        }

        [Fact]
        public void SaveCandidateScoreUsesVisualProgressBar()
        {
            var save = Read("Views", "SaveCenterView.xaml");
            Assert.Contains("Header=\"可信度\" Width=\"130\"", save);
            Assert.Contains("<ProgressBar Height=\"8\"", save);
            Assert.Contains("Value=\"{Binding Score, Mode=OneWay}\"", save);
            Assert.Contains("StringFormat=P0", save);
        }

        private static string Read(string folder, string file)
        {
            var root = FindRepositoryRoot();
            return File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", folder, file));
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
