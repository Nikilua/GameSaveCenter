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
            Assert.Contains("Padding=\"0,0,0,4\"", redesign);
            Assert.Contains("CornerRadius=\"14\"", redesign);
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
