using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using GameSaveCenter.Playnite.Controls;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R09IconSemanticBehaviorTests
{
    private static readonly IReadOnlyDictionary<string, string> SemanticIcons =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["备份"] = "GscIconActionBackup",
            ["恢复"] = "GscIconActionRestore",
            ["上传"] = "GscIconActionUpload",
            ["校验"] = "GscIconActionVerify",
            ["归类"] = "GscIconActionCategorize",
            ["忽略"] = "GscIconActionIgnore"
        };

    [Fact]
    public void SemanticActionIconsAreNonEmptyVectorsWithOneSharedCompactSize()
    {
        RunSta(() =>
        {
            var resources = LoadIconPack();
            var style = Assert.IsType<Style>(resources["GscActionIcon"]);
            Assert.Equal(16d, SetterValue<double>(style, FrameworkElement.WidthProperty));
            Assert.Equal(16d, SetterValue<double>(style, FrameworkElement.HeightProperty));

            foreach (var pair in SemanticIcons)
            {
                var geometry = Assert.IsAssignableFrom<Geometry>(resources[pair.Value]);
                Assert.False(geometry.IsEmpty(), $"{pair.Key} icon has an empty geometry.");
                Assert.True(geometry.Bounds.Width > 0 && geometry.Bounds.Height > 0,
                    $"{pair.Key} icon has no measurable bounds.");
            }
        });
    }

    [Fact]
    public void ProductionActionUsagesShareTheSemanticMapAndDisabledButtonsKeepTheirVector()
    {
        var root = TestRepositoryContext.Root;
        var usages = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["src/GameSaveCenter.Playnite/Views/DashboardView.xaml"] = new[] { "备份", "校验" },
            ["src/GameSaveCenter.Playnite/Views/SaveCenterView.xaml"] = new[] { "备份", "恢复", "校验" },
            ["src/GameSaveCenter.Playnite/Views/MaintenanceView.xaml"] = new[] { "上传", "校验" },
            ["src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml"] = new[] { "归类", "忽略", "恢复" }
        };

        foreach (var usage in usages)
        {
            var document = XDocument.Load(Path.Combine(root, usage.Key));
            var iconAttributes = document
                .Descendants()
                .Where(element => element.Name.LocalName == "ThemeAwareIcon")
                .Select(element => (string?)element.Attribute("IconData"))
                .Where(value => value != null)
                .ToArray();

            foreach (var semantic in usage.Value)
            {
                Assert.Contains($"{{StaticResource {SemanticIcons[semantic]}}}", iconAttributes);
            }
        }

        RunSta(() =>
        {
            var resources = LoadIconPack();
            foreach (var iconKey in SemanticIcons.Values)
            {
                var geometry = Assert.IsAssignableFrom<Geometry>(resources[iconKey]);
                var icon = new ThemeAwareIcon
                {
                    IconData = geometry,
                    Style = Assert.IsType<Style>(resources["GscActionIcon"])
                };
                var button = new System.Windows.Controls.Button { Content = icon, IsEnabled = false };
                var window = new Window
                {
                    Content = button,
                    Width = 80,
                    Height = 60,
                    ShowInTaskbar = false,
                    WindowStyle = WindowStyle.None,
                    Opacity = 0.01
                };

                try
                {
                    window.Show();
                    window.UpdateLayout();
                    Assert.Same(geometry, icon.IconData);
                    Assert.Equal(Visibility.Visible, icon.Visibility);
                    Assert.Equal(16d, icon.ActualWidth, 0.1d);
                    Assert.Equal(16d, icon.ActualHeight, 0.1d);
                }
                finally
                {
                    window.Close();
                }
            }
        });
    }

    private static ResourceDictionary LoadIconPack()
    {
        var path = Path.Combine(
            TestRepositoryContext.Root,
            "src",
            "GameSaveCenter.Playnite",
            "Themes",
            "GscIconPack.xaml");
        var markup = File.ReadAllText(path).Replace(
            "clr-namespace:GameSaveCenter.Playnite.Controls",
            "clr-namespace:GameSaveCenter.Playnite.Controls;assembly=GameSaveCenter.Playnite");
        return Assert.IsType<ResourceDictionary>(XamlReader.Parse(markup));
    }

    private static T SetterValue<T>(Style style, DependencyProperty property)
    {
        var setter = style.Setters
            .OfType<Setter>()
            .Single(item => item.Property == property);
        return Assert.IsType<T>(setter.Value);
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
                System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        Assert.Null(failure);
    }
}
