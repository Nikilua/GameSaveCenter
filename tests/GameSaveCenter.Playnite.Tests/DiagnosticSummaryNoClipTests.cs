using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class DiagnosticSummaryNoClipTests
    {
        [Fact]
        public void DiagnosticSummaryIsNotClippedByOuterBorderAndOwnsItsOwnTextBoxScroll()
        {
            var repositoryRoot = FindRepositoryRoot();
            var xamlPath = Path.Combine(repositoryRoot, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml");
            var xaml = File.ReadAllText(xamlPath);
            var document = XDocument.Parse(xaml);
            var summary = document.Descendants().Single(element =>
                element.Name.LocalName == "Border"
                && element.Attribute(XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml"))?.Value == "MaintenanceDiagnosticSummaryGrid");
            Assert.Null(summary.Attribute("ClipToBounds"));

            var textBox = summary.Descendants().Single(element => element.Name.LocalName == "TextBox");
            Assert.Equal("96", textBox.Attribute("MinHeight")?.Value);
            Assert.Equal("160", textBox.Attribute("MaxHeight")?.Value);
            Assert.Equal("Auto", textBox.Attribute("VerticalScrollBarVisibility")?.Value);

            var code = File.ReadAllText(xamlPath + ".cs");
            Assert.DoesNotContain("MaintenanceDiagnosticSummaryGrid.MinHeight", code);
            Assert.DoesNotContain("MaintenanceDiagnosticSummaryGrid.MaxHeight", code);
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
