using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

using GscButton = GameSaveCenter.Playnite.Controls.Button;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R03BilingualLengthTests
{
    [Fact]
    public void SharedActionTemplateWrapsCompleteLabelsInsideANarrowSlot()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        Exception? failure = null;
        var measurements = new List<ActionMeasurement>();

        var thread = new Thread(() =>
        {
            try
            {
                var resources = LoadProductionResources();
                var style = Assert.IsType<Style>(resources["GscWpfUiPrimaryActionButton"]);
                var host = new StackPanel { Width = 154 };
                foreach (var label in new[]
                {
                    "Back up the selected game safely now",
                    "打开当前游戏的完整存档详情"
                })
                {
                    var button = new GscButton
                    {
                        Style = style,
                        Content = label,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        Margin = new Thickness(0, 0, 0, 4)
                    };
                    host.Children.Add(button);
                }

                using var window = new WindowHost(new Window
                {
                    Content = new Border { Width = 154, Child = host },
                    Width = 154,
                    Height = 180,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    WindowStyle = WindowStyle.None,
                    Opacity = 0.01
                });
                window.Window.Show();
                window.Window.UpdateLayout();

                foreach (var button in host.Children.OfType<GscButton>())
                {
                    var text = FindVisualChildren<TextBlock>(button).Single(candidate => candidate.Text == button.Content as string);
                    measurements.Add(new ActionMeasurement(
                        text.Text,
                        text.TextWrapping,
                        text.TextTrimming,
                        text.ToolTip as string,
                        button.ActualWidth,
                        button.ActualHeight));
                }
            }
            catch (Exception caught)
            {
                failure = caught;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(failure);
        Assert.Equal(2, measurements.Count);
        Assert.All(measurements, measurement =>
        {
            Assert.Equal(TextWrapping.Wrap, measurement.Wrapping);
            Assert.Equal(TextTrimming.None, measurement.Trimming);
            Assert.Equal(measurement.Label, measurement.ToolTip);
            Assert.InRange(measurement.Width, 153.5, 154.5);
            Assert.True(measurement.Height > 36, Describe(measurement));
        });
    }

    [Fact]
    public void OverviewLengthFixtureKeepsTitlesInspectableAndActionsPresent()
    {
        var root = TestRepositoryContext.Root;
        var view = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml"));
        var harness = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "tests", "GameSaveCenter.RenderHarness", "FakeDashboardData.cs"));
        var runner = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "tests", "GameSaveCenter.RenderHarness", "Program.cs"));

        Assert.Contains("TextTrimming=\"CharacterEllipsis\" ToolTip=\"{Binding SelectedGame.Name, Mode=OneWay}\"", view);
        Assert.Contains("TextTrimming=\"CharacterEllipsis\" ToolTip=\"{Binding DetailMessage}\"", view);
        Assert.Contains("BilingualLengthStress", harness);
        Assert.Contains("BilingualLongEnglishSentence", harness);
        Assert.Contains("windowW == 820", runner);
        Assert.Contains("englishVisible", runner);
        Assert.Contains("currentGameButtons", runner);
    }

    private static string Describe(ActionMeasurement measurement)
        => $"{measurement.Label}: {measurement.Width:0.##}x{measurement.Height:0.##}, wrapping={measurement.Wrapping}, trimming={measurement.Trimming}";

    private static ResourceDictionary LoadProductionResources()
        => (ResourceDictionary)XamlReader.Parse(@"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
                    xmlns:ui=""clr-namespace:GameSaveCenter.Playnite.Controls;assembly=GameSaveCenter.Playnite""><ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/Redesign.xaml""/>
</ResourceDictionary.MergedDictionaries></ResourceDictionary>");

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
                yield return match;
            foreach (var nested in FindVisualChildren<T>(child))
                yield return nested;
        }
    }

    private sealed class ActionMeasurement
    {
        public ActionMeasurement(
            string label,
            TextWrapping wrapping,
            TextTrimming trimming,
            string? toolTip,
            double width,
            double height)
        {
            Label = label;
            Wrapping = wrapping;
            Trimming = trimming;
            ToolTip = toolTip;
            Width = width;
            Height = height;
        }

        public string Label { get; }
        public TextWrapping Wrapping { get; }
        public TextTrimming Trimming { get; }
        public string? ToolTip { get; }
        public double Width { get; }
        public double Height { get; }
    }

    private sealed class WindowHost : IDisposable
    {
        public WindowHost(Window window) => Window = window;

        public Window Window { get; }

        public void Dispose()
        {
            if (Window.IsVisible)
                Window.Close();
        }
    }
}
