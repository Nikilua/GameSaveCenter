using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class OvernightV4SharedTests
    {
        [Fact]
        public void ExpandableCardsUseUnifiedDisclosureChromeWithoutInnerScroll()
        {
            var root = FindRepositoryRoot();
            var tokens = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "DesignTokens.xaml"));
            Assert.Contains("VerticalAlignment=\"Center\"", tokens);
            Assert.Contains("VerticalContentAlignment=\"", tokens);
            var views = new[]
            {
                Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml"),
                Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"),
                Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"),
                Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"),
                Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml")
            };

            foreach (var path in views)
            {
                var document = XDocument.Parse(File.ReadAllText(path));
                var expanders = document.Descendants().Where(element => element.Name.LocalName == "Expander").ToList();
                Assert.All(expanders, expander =>
                {
                    Assert.Contains("GscDisclosureCard", expander.Attribute("Style")?.Value ?? string.Empty);
                    Assert.DoesNotContain(">", expander.Attribute("Header")?.Value ?? string.Empty);
                    Assert.DoesNotContain(expander.Descendants(), element => element.Name.LocalName == "ScrollViewer");
                });
            }
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
