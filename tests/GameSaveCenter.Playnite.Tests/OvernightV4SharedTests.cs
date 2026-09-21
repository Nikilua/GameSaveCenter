using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class OvernightV4SharedTests
    {
        [Fact]
        public void ExpandableCardsUseUnifiedDisclosureChromeAndBoundedRiskScroll()
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
                    var style = expander.Attribute("Style")?.Value
                        ?? expander.Descendants().SingleOrDefault(element => element.Name.LocalName == "Style")?.ToString()
                        ?? string.Empty;
                    Assert.Contains("GscDisclosureCard", style);
                    Assert.DoesNotContain(">", expander.Attribute("Header")?.Value ?? string.Empty);
                    var boundedScrollSurfaces = expander.Descendants()
                        .Where(element => element.Name.LocalName == "ScrollViewer")
                        .ToList();
                    Assert.All(boundedScrollSurfaces, scrollViewer =>
                    {
                        var maxHeightValue = scrollViewer.Attribute("MaxHeight")?.Value;
                        Assert.True(
                            double.TryParse(maxHeightValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var maxHeight),
                            $"Overview disclosure ScrollViewer must declare a numeric MaxHeight, actual: '{maxHeightValue}'.");
                        Assert.InRange(maxHeight, 160d, 520d);
                        Assert.Equal("Auto", scrollViewer.Attribute("VerticalScrollBarVisibility")?.Value);
                        Assert.Equal("Disabled", scrollViewer.Attribute("HorizontalScrollBarVisibility")?.Value);
                    });
                });
            }
        }

        private static string FindRepositoryRoot()
            => TestRepositoryContext.Root;
    }
}
