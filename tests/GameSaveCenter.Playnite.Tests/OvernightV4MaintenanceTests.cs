using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class OvernightV4MaintenanceTests
    {
        [Fact]
        public void MaintenanceDiagnosticsUseUnifiedDisclosureCardAndFiveReadableColumns()
        {
            var root = FindRepositoryRoot();
            var maintenance = XDocument.Parse(File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml")));
            var xamlName = XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml");

            var findings = maintenance.Descendants().Single(element => element.Attribute(xamlName)?.Value == "FindingsGrid");
            var columns = findings.Elements().Single(element => element.Name.LocalName == "DataGrid.Columns").Elements().ToList();
            Assert.Equal(5, columns.Count);
            Assert.Contains("Width=\"{StaticResource GscSeverityColumnWidth}\"", columns[0].ToString());
            Assert.Contains("Width=\"120\"", columns[1].ToString());
            Assert.Contains("Width=\"160\"", columns[2].ToString());
            Assert.Contains("MinWidth=\"180\"", columns[3].ToString());
            Assert.Contains("Width=\"0.75*\"", columns[4].ToString());
            Assert.Contains("MinWidth=\"140\"", columns[4].ToString());

            var expanders = maintenance.Descendants().Where(element => element.Name.LocalName == "Expander").ToList();
            Assert.NotEmpty(expanders);
            Assert.All(expanders, expander =>
            {
                Assert.Contains("GscDisclosureCard", expander.Attribute("Style")?.Value);
                Assert.DoesNotContain(">", expander.Attribute("Header")?.Value ?? string.Empty);
            });
            Assert.DoesNotContain("EnvironmentCheckDisclosureScroller", maintenance.ToString());
            Assert.DoesNotContain("MaintenanceActionsDisclosureScroller", maintenance.ToString());
            Assert.Contains("x:Name=\"MaintenanceDiagnosticsSubTabs\"", maintenance.ToString());
            Assert.Contains("Content=\"问题列表\"", maintenance.ToString());
            Assert.Contains("Content=\"诊断概览\"", maintenance.ToString());
            Assert.Contains("OnMaintenanceDiagnosticsSubTabChanged", maintenance.ToString());
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
