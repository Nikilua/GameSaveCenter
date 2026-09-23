using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using GameSaveCenter.Playnite.Diagnostics;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class UiDiagnosticsExporterTests
{
    [Fact]
    public void ResourceSnapshotExportsResolvedBrushValues()
    {
        Exception? exception = null;
        var keyFound = false;
        var colorFound = false;

        var thread = new Thread(() =>
        {
            try
            {
                var dictionary = new ResourceDictionary();
                var brush = new SolidColorBrush(Color.FromRgb(18, 30, 45));
                brush.Freeze();
                dictionary["GscBackdropBrush"] = brush;
                var records = UiDiagnosticsExporters.BuildResourceSnapshot(dictionary, "TestScope");
                var record = records.Single(item => item.Key == "GscBackdropBrush");
                keyFound = true;
                colorFound = record.BrushSummary == "#FF121E2D";
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
        Assert.True(keyFound);
        Assert.True(colorFound);
    }

    [Fact]
    public void StyleFingerprintExportsRepresentativeControl()
    {
        Exception? exception = null;
        var matched = false;

        var thread = new Thread(() =>
        {
            try
            {
                var root = new Grid { Width = 300, Height = 100 };
                var border = new Border
                {
                    Background = Brushes.SlateGray,
                    BorderBrush = Brushes.DimGray,
                    BorderThickness = new Thickness(2),
                    Padding = new Thickness(6),
                    Margin = new Thickness(4),
                    Effect = new DropShadowEffect { BlurRadius = 8 }
                };
                root.Children.Add(border);
                root.Measure(new Size(300, 100));
                root.Arrange(new Rect(0, 0, 300, 100));
                root.UpdateLayout();

                var fingerprints = UiDiagnosticsExporters.BuildStyleFingerprints(root);
                var fingerprint = fingerprints.First(item => item.Type == "Border" && item.Margin == "4,4,4,4");
                matched = fingerprint != null
                    && fingerprint.BorderThickness.Contains("2")
                    && fingerprint.Padding.Contains("6")
                    && !string.IsNullOrEmpty(fingerprint.BackgroundArgb)
                    && !string.IsNullOrEmpty(fingerprint.BorderBrushArgb)
                    && fingerprint.EffectType == "DropShadowEffect";
                _ = fingerprint;
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
        Assert.True(matched, "Fingerprint mismatch; no Border record with the expected effective values was exported.");
    }

    [Fact]
    public void HighDpiPngUsesExplicitScaleWithoutApplyingHostDpiTwice()
    {
        Exception? exception = null;
        var pixelWidth = 0;
        var redMaxX = -1;
        var path = Path.Combine(Path.GetTempPath(), "gsc-ui-dpi-capture-" + Guid.NewGuid().ToString("N") + ".png");

        var thread = new Thread(() =>
        {
            try
            {
                var root = new Grid { Width = 100, Height = 40, Background = Brushes.Blue };
                root.Children.Add(new Border
                {
                    Width = 20,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Background = Brushes.Red
                });
                root.Measure(new Size(100, 40));
                root.Arrange(new Rect(0, 0, 100, 40));
                root.UpdateLayout();

                UiDiagnosticsExporters.SavePng(root, path, 1.5);
                using var stream = File.OpenRead(path);
                var frame = BitmapDecoder.Create(
                    stream,
                    BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.OnLoad).Frames[0];
                pixelWidth = frame.PixelWidth;
                var pixels = new byte[frame.PixelWidth * frame.PixelHeight * 4];
                frame.CopyPixels(pixels, frame.PixelWidth * 4, 0);
                for (var y = 0; y < frame.PixelHeight; y++)
                {
                    for (var x = 0; x < frame.PixelWidth; x++)
                    {
                        var offset = (y * frame.PixelWidth + x) * 4;
                        if (pixels[offset + 2] > 200 && pixels[offset + 1] < 100 && pixels[offset] < 100)
                            redMaxX = Math.Max(redMaxX, x);
                    }
                }
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        try
        {
            Assert.Null(exception);
            Assert.Equal(150, pixelWidth);
            Assert.InRange(redMaxX, 25, 35);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    [InlineData(GameSaveCenterThemeMode.FollowPlaynite)]
    public void AdaptivePaletteContrastGuardsPass(GameSaveCenterThemeMode mode)
    {
        Exception? exception = null;
        var violations = Array.Empty<AdaptiveThemePaletteContrastGuard.Violation>();

        var thread = new Thread(() =>
        {
            try
            {
                var host = new Grid();
                var palette = AdaptiveThemePaletteFactory.Create(host, false, 100, mode);
                violations = AdaptiveThemePaletteContrastGuard.Validate(palette, palette.Background).ToArray();
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
        Assert.Empty(violations);
    }

    [Fact]
    public void TextContrastGuardCompositesEffectiveAlphaAndRejectsTheFormerDarkFixtureFalsePositive()
    {
        var readable = new AdaptiveThemePaletteContrastGuard.TextContrastSample
        {
            Check = "disabled-light-label",
            Foreground = Color.FromArgb(0xAE, 0x1B, 0x1F, 0x27),
            Background = Color.FromRgb(0xEF, 0xF0, 0xF5),
            Minimum = 4.5
        };
        var invalid = new AdaptiveThemePaletteContrastGuard.TextContrastSample
        {
            Check = "black-on-dark-negative",
            Foreground = Colors.Black,
            Background = Color.FromRgb(0x25, 0x2A, 0x34),
            Minimum = 4.5
        };

        var readableMeasurements = AdaptiveThemePaletteContrastGuard.MeasureTextContrast(new[] { readable });
        var invalidViolations = AdaptiveThemePaletteContrastGuard.ValidateTextContrast(new[] { invalid });

        Assert.Single(readableMeasurements);
        Assert.True(readableMeasurements[0].Actual >= 4.5, $"Expected effective alpha-composited text to pass, got {readableMeasurements[0].Actual:0.###}.");
        Assert.Single(invalidViolations);
        Assert.Equal("black-on-dark-negative", invalidViolations[0].Check);
    }

    [Fact]
    public void GradientContrastModelsWholeChromeOpacityAndNonUniformStops()
    {
        var stops = new[]
        {
            new GradientStop(Colors.White, 0),
            new GradientStop(Colors.White, 0.9),
            new GradientStop(Colors.Black, 1)
        };

        var measurements = AdaptiveThemePaletteContrastGuard.MeasureGradientTextContrast(
            "composite",
            Colors.Black,
            Colors.Black,
            stops,
            Colors.Transparent,
            Colors.Transparent,
            Colors.Transparent,
            0.5,
            4.5);

        var pressed = measurements.Single(measurement => measurement.Check == "composite.pressed@0.0");
        var combined = measurements.Single(measurement => measurement.Check == "composite.hover+pressed+focus@0.0");
        var earlyEnd = measurements.Single(measurement => measurement.Check == "composite.normal@0.9");
        var lateStop = measurements.Single(measurement => measurement.Check == "composite.normal@1.0");

        Assert.Equal(Color.FromRgb(128, 128, 128), pressed.Background);
        Assert.Equal(Colors.Black, pressed.EffectiveForeground);
        Assert.True(pressed.Actual >= 4.5, $"Expected black text on gray chrome to pass, got {pressed.Actual:0.###}.");
        Assert.Equal(Color.FromRgb(128, 128, 128), combined.Background);
        Assert.Equal(Colors.Black, combined.EffectiveForeground);
        Assert.True(combined.Actual >= 4.5, $"Expected black text on gray chrome with hover+pressed+focus to pass, got {combined.Actual:0.###}.");
        Assert.Equal(Colors.White, earlyEnd.Background);
        Assert.Equal(Colors.Black, lateStop.Background);
    }

    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light)]
    [InlineData(GameSaveCenterThemeMode.Dark)]
    public void SemanticStateContrastCoversGradientButtonSelectionInputDangerAndComplexSurfaces(GameSaveCenterThemeMode mode)
    {
        Exception? exception = null;
        var allStatesReadable = false;
        var selectionAndInputReadable = false;

        var thread = new Thread(() =>
        {
            try
            {
                var host = new Grid();
                var palette = AdaptiveThemePaletteFactory.Create(host, true, 50, mode);
                var resources = new ResourceDictionary();
                AdaptiveThemePaletteFactory.ApplyAccentResources(resources, palette);
                var primaryBrush = Assert.IsType<LinearGradientBrush>(resources["GscPrimaryButtonBrush"]);
                var hoverOverlay = Assert.IsType<SolidColorBrush>(resources["GscOnAccentHoverOverlayBrush"]).Color;
                var pressedOverlay = Assert.IsType<SolidColorBrush>(resources["GscOnAccentPressedOverlayBrush"]).Color;
                var buttonMeasurements = AdaptiveThemePaletteContrastGuard.MeasureGradientTextContrast(
                    "primary-button",
                    palette.OnAccentText,
                    palette.Background,
                    primaryBrush.GradientStops,
                    hoverOverlay,
                    hoverOverlay,
                    pressedOverlay);

                Assert.NotEmpty(buttonMeasurements);
                Assert.Equal(88, buttonMeasurements.Count);
                Assert.Contains(buttonMeasurements, measurement => measurement.Check == "primary-button.hover+pressed+focus@0.0");
                Assert.All(buttonMeasurements, measurement =>
                    Assert.True(
                        measurement.Actual + 0.001 >= measurement.Minimum,
                        $"{measurement.Check} measured {measurement.Actual:0.###}, expected {measurement.Minimum:0.###}."));

                var layeredSamples = new[]
                {
                    new AdaptiveThemePaletteContrastGuard.LayeredTextContrastSample
                    {
                        Check = "selection-text",
                        Foreground = palette.PrimaryText,
                        Backdrop = palette.Background,
                        SurfaceLayers = new[] { palette.AccentTint },
                        Minimum = 4.5
                    },
                    new AdaptiveThemePaletteContrastGuard.LayeredTextContrastSample
                    {
                        Check = "input-text",
                        Foreground = palette.PrimaryText,
                        Backdrop = palette.Background,
                        SurfaceLayers = new[] { palette.ControlFill },
                        Minimum = 4.5
                    },
                    new AdaptiveThemePaletteContrastGuard.LayeredTextContrastSample
                    {
                        Check = "input-placeholder",
                        Foreground = palette.MutedText,
                        Backdrop = palette.Background,
                        SurfaceLayers = new[] { palette.ControlFill },
                        Minimum = 3.0
                    },
                    new AdaptiveThemePaletteContrastGuard.LayeredTextContrastSample
                    {
                        Check = "danger-button",
                        Foreground = Assert.IsType<SolidColorBrush>(resources["GscOnDangerTextBrush"]).Color,
                        Backdrop = palette.Background,
                        SurfaceLayers = new[] { palette.Error },
                        Minimum = 4.5
                    }
                };
                var layeredMeasurements = AdaptiveThemePaletteContrastGuard.MeasureLayeredTextContrast(layeredSamples);
                Assert.All(layeredMeasurements, measurement =>
                    Assert.True(
                        measurement.Actual + 0.001 >= measurement.Minimum,
                        $"{measurement.Check} measured {measurement.Actual:0.###}, expected {measurement.Minimum:0.###}."));

                var complexBackgrounds = new[]
                {
                    Color.FromRgb(255, 255, 255),
                    Color.FromRgb(8, 8, 12),
                    Color.FromRgb(214, 35, 70),
                    Color.FromRgb(28, 130, 198)
                };
                var complexSamples = complexBackgrounds.Select((background, index) => new AdaptiveThemePaletteContrastGuard.LayeredTextContrastSample
                {
                    Check = $"complex-surface-{index}",
                    Foreground = palette.PrimaryText,
                    Backdrop = background,
                    SurfaceLayers = new[] { palette.SurfaceTop, palette.ControlFill },
                    Minimum = 4.5
                }).ToArray();
                var complexMeasurements = AdaptiveThemePaletteContrastGuard.MeasureLayeredTextContrast(complexSamples);
                Assert.All(complexMeasurements, measurement =>
                    Assert.True(
                        measurement.Actual + 0.001 >= measurement.Minimum,
                        $"{measurement.Check} measured {measurement.Actual:0.###}, expected {measurement.Minimum:0.###}."));

                allStatesReadable = true;
                selectionAndInputReadable = true;
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
        Assert.True(allStatesReadable);
        Assert.True(selectionAndInputReadable);
    }

    [Fact]
    public void ThemeResourceSwitchReplacesStateBrushesWithoutLeavingStaticFallbacks()
    {
        Exception? exception = null;
        var switched = false;
        var thread = new Thread(() =>
        {
            try
            {
                var host = new Grid();
                var light = AdaptiveThemePaletteFactory.Create(host, true, 50, GameSaveCenterThemeMode.Light);
                var dark = AdaptiveThemePaletteFactory.Create(host, true, 50, GameSaveCenterThemeMode.Dark);
                var lightResources = new ResourceDictionary();
                var darkResources = new ResourceDictionary();
                AdaptiveThemePaletteFactory.ApplyAccentResources(lightResources, light);
                AdaptiveThemePaletteFactory.ApplyAccentResources(darkResources, dark);

                var lightSelectionBrush = Assert.IsType<SolidColorBrush>(lightResources["GscSelectionTextBrush"]);
                var darkSelectionBrush = Assert.IsType<SolidColorBrush>(darkResources["GscSelectionTextBrush"]);
                var lightButton = Assert.IsType<LinearGradientBrush>(lightResources["GscPrimaryButtonBrush"]);
                var darkButton = Assert.IsType<LinearGradientBrush>(darkResources["GscPrimaryButtonBrush"]);
                var lightStops = lightButton.GradientStops.Select(stop => stop.Color).ToArray();
                var darkStops = darkButton.GradientStops.Select(stop => stop.Color).ToArray();
                switched = lightSelectionBrush.Color != darkSelectionBrush.Color
                    && !ReferenceEquals(lightSelectionBrush, darkSelectionBrush)
                    && lightStops.Length == darkStops.Length
                    && !lightStops.SequenceEqual(darkStops)
                    && !ReferenceEquals(lightButton, darkButton)
                    && !darkButton.GradientStops.Select(stop => stop.Color).SequenceEqual(
                        lightButton.GradientStops.Select(stop => stop.Color));
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
        Assert.True(switched);
    }
}
