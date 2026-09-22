using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R09BackgroundPressureBehaviorTests
{
    [Fact]
    public void GlassMaterialsKeepSemanticTextReadableAcrossHostBackgroundFamilies()
    {
        RunSta(() =>
        {
            var scenarios = new[]
            {
                new HostScenario("浅色中性", Color.FromRgb(248, 249, 252), false),
                new HostScenario("深色中性", Color.FromRgb(17, 19, 25), true),
                new HostScenario("暖色浅背景", Color.FromRgb(238, 222, 195), false),
                new HostScenario("蓝色深背景", Color.FromRgb(17, 52, 82), true)
            };

            foreach (var scenario in scenarios)
            {
                var host = CreateHost(scenario);
                var palette = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(
                    host,
                    glassEnabled: true,
                    strengthPercent: 78,
                    GameSaveCenterThemeMode.FollowPlaynite,
                    highContrastOverride: false);
                var resources = new ResourceDictionary();
                AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(resources, palette, glassEnabled: true, motionEnabled: true);

                var backdropColors = BrushColors(resources["GscBackdropBrush"]);
                var ambientColors = BrushColors(resources["GscAmbientWideWashBrush"])
                    .Where(color => color.A > 0)
                    .ToArray();
                var surfaceColors = BrushColors(resources["GscGlassFillBrush"])
                    .Concat(BrushColors(resources["GscGlassStrongBrush"]))
                    .ToArray();

                Assert.NotEmpty(backdropColors);
                Assert.NotEmpty(ambientColors);
                Assert.Contains(ambientColors, color => color.A < 255);
                Assert.Contains(surfaceColors, color => color.A > 0 && color.A < 255);

                var samples = new List<AdaptiveThemePaletteContrastGuard.LayeredTextContrastSample>();
                foreach (var backdropColor in backdropColors)
                {
                    foreach (var ambientColor in ambientColors)
                    {
                        foreach (var surfaceColor in surfaceColors)
                        {
                            samples.Add(new AdaptiveThemePaletteContrastGuard.LayeredTextContrastSample
                            {
                                Check = $"{scenario.Name}:{backdropColor}:{ambientColor}:{surfaceColor}",
                                Foreground = palette.PrimaryText,
                                Backdrop = scenario.Background,
                                SurfaceLayers = new[] { backdropColor, ambientColor, surfaceColor },
                                Minimum = 4.5
                            });
                        }
                    }
                }

                var measurements = AdaptiveThemePaletteContrastGuard.MeasureLayeredTextContrast(samples);
                Assert.NotEmpty(measurements);
                Assert.All(measurements, measurement =>
                    Assert.True(
                        measurement.Actual + 0.001 >= measurement.Minimum,
                        $"{measurement.Check} measured {measurement.Actual:0.###}, expected {measurement.Minimum:0.###}."));
            }
        });
    }

    private static IReadOnlyList<Color> BrushColors(object value)
    {
        return value switch
        {
            SolidColorBrush solid => new[] { solid.Color },
            LinearGradientBrush gradient => gradient.GradientStops.Select(stop => stop.Color).ToArray(),
            _ => throw new InvalidOperationException($"Unexpected brush resource type: {value.GetType().FullName}.")
        };
    }

    [Fact]
    public void LayeredContrastProbeRejectsAReadabilityFailure()
    {
        var sample = new AdaptiveThemePaletteContrastGuard.LayeredTextContrastSample
        {
            Check = "deliberately-dark-text-on-dark-material",
            Foreground = Colors.Black,
            Backdrop = Color.FromRgb(8, 8, 12),
            SurfaceLayers = new[] { Color.FromArgb(0x20, 255, 0, 0) },
            Minimum = 4.5
        };

        var violations = AdaptiveThemePaletteContrastGuard.ValidateLayeredTextContrast(new[] { sample });

        Assert.Single(violations);
        Assert.Equal(sample.Check, violations[0].Check);
    }

    private static Border CreateHost(HostScenario scenario)
    {
        var host = new Border { Background = AdaptiveThemePaletteFactory.Brush(scenario.Background) };
        host.Resources["WindowBackgroundBrush"] = AdaptiveThemePaletteFactory.Brush(scenario.Background);
        host.Resources["DarkWindowBackgroundBrush"] = AdaptiveThemePaletteFactory.Brush(scenario.Background);
        host.Resources["ThemeDarkStyle"] = scenario.IsDark;
        host.Resources["TextBrush"] = AdaptiveThemePaletteFactory.Brush(scenario.IsDark ? Colors.White : Colors.Black);
        host.Resources["TextBrushDark"] = AdaptiveThemePaletteFactory.Brush(scenario.IsDark ? Colors.Black : Colors.White);
        host.Resources["HighlightGlyphBrush"] = AdaptiveThemePaletteFactory.Brush(Color.FromRgb(103, 119, 230));
        return host;
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
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

    private sealed class HostScenario
    {
        public HostScenario(string name, Color background, bool isDark)
        {
            Name = name;
            Background = background;
            IsDark = isDark;
        }

        public string Name { get; }
        public Color Background { get; }
        public bool IsDark { get; }
    }
}
