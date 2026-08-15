using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
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
}
