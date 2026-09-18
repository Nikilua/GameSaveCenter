using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R09HighContrastBehaviorTests
{
    [Fact]
    public void HighContrastUsesSystemSemanticTokensAndRestoresMaterialAfterRecovery()
    {
        RunSta(() =>
        {
            var root = CreateProbeRoot();
            var highContrast = AdaptiveThemePaletteFactory.Create(
                root,
                glassEnabled: true,
                strengthPercent: 78,
                GameSaveCenterThemeMode.FollowPlaynite,
                highContrastOverride: true);

            AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(
                root.Resources, highContrast, glassEnabled: true, motionEnabled: true);
            root.UpdateLayout();

            Assert.True(highContrast.IsHighContrast);
            Assert.False(highContrast.GlassEnabled);
            Assert.Equal(SystemColors.WindowColor, highContrast.Background);
            Assert.Equal(SystemColors.WindowTextColor, highContrast.PrimaryText);
            Assert.Equal(SystemColors.GrayTextColor, highContrast.DisabledText);
            Assert.Equal(SystemColors.HighlightTextColor, BrushColor(root, "GscSelectionTextBrush"));
            Assert.Equal(SystemColors.ControlDarkColor, BrushColor(root, "GscProgressTrackBrush"));
            Assert.Equal(SystemColors.HighlightColor, BrushColor(root, "GscProgressFillBrush"));
            Assert.Equal(SystemColors.GrayTextColor, BrushColor(root, "GscDisabledTextBrush"));
            Assert.Equal(SystemColors.HighlightColor, BrushColor(root, "GscRowHoverStrongBrush"));
            Assert.Equal(highContrast.Accent, BrushColor(root, "GscAccentIconFillBrush"));
            Assert.All(GradientStops(root, "GscButtonGlassBrush"), stop => Assert.Equal(255, stop.Color.A));
            Assert.All(GradientStops(root, "GscPrimaryButtonBrush"), stop => Assert.Equal(255, stop.Color.A));
            Assert.All(GradientStops(root, "GscAmbientWideWashBrush"), stop => Assert.Equal(0, stop.Color.A));
            Assert.Null(root.Resources["GscSurfaceEffect"]);
            Assert.Null(root.Resources["GscGameBackgroundEffect"]);
            Assert.Equal(false, root.Resources["GscPopupAllowsTransparency"]);
            Assert.Equal(PopupAnimation.None, root.Resources["GscPopupAnimation"]);
            Assert.Equal(0d, root.Resources["GscGameBackgroundOpacity"]);
            var probePanel = Assert.IsType<StackPanel>(root.Children[0]);
            var progress = Assert.IsType<ProgressBar>(probePanel.Children[0]);
            var icon = Assert.IsType<Path>(probePanel.Children[1]);
            var disabledText = Assert.IsType<TextBlock>(probePanel.Children[2]);
            Assert.Equal(SystemColors.HighlightColor, Assert.IsType<SolidColorBrush>(progress.Foreground).Color);
            Assert.Equal(highContrast.Accent, Assert.IsType<SolidColorBrush>(icon.Fill).Color);
            Assert.Equal(SystemColors.GrayTextColor, Assert.IsType<SolidColorBrush>(disabledText.Foreground).Color);

            var normal = AdaptiveThemePaletteFactory.Create(
                root,
                glassEnabled: true,
                strengthPercent: 78,
                GameSaveCenterThemeMode.FollowPlaynite,
                highContrastOverride: false);
            AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(
                root.Resources, normal, glassEnabled: true, motionEnabled: true);
            root.UpdateLayout();

            Assert.False(normal.IsHighContrast);
            Assert.True(normal.GlassEnabled);
            var normalAmbientStops = GradientStops(root, "GscAmbientWideWashBrush");
            Assert.Contains(normalAmbientStops, stop => stop.Color.A > 0);
            var normalButtonStops = GradientStops(root, "GscButtonGlassBrush");
            Assert.Contains(normalButtonStops, stop => stop.Color.A < 255);
            Assert.NotEqual(SystemColors.ControlDarkColor, BrushColor(root, "GscProgressTrackBrush"));
        });
    }

    private static Grid CreateProbeRoot()
    {
        var root = new Grid();
        var progress = new ProgressBar();
        progress.SetResourceReference(ProgressBar.ForegroundProperty, "GscProgressFillBrush");
        progress.SetResourceReference(ProgressBar.BackgroundProperty, "GscProgressTrackBrush");
        var icon = new Path();
        icon.SetResourceReference(Shape.FillProperty, "GscAccentIconFillBrush");
        var disabledText = new TextBlock { Text = "disabled" };
        disabledText.SetResourceReference(TextBlock.ForegroundProperty, "GscDisabledTextBrush");
        root.Children.Add(new StackPanel
        {
            Children = { progress, icon, disabledText }
        });
        return root;
    }

    private static Color BrushColor(FrameworkElement root, string key)
        => Assert.IsType<SolidColorBrush>(root.Resources[key]).Color;

    private static GradientStopCollection GradientStops(FrameworkElement root, string key)
        => Assert.IsType<LinearGradientBrush>(root.Resources[key]).GradientStops;

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
}
