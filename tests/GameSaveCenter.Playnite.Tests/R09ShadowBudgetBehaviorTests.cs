using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Effects;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R09ShadowBudgetBehaviorTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MaterialEffectsUseFiniteTiersAndLowCostModeRemovesEffects(bool dark)
    {
        RunSta(() =>
        {
            var host = new Grid();
            var palette = AdaptiveThemePaletteFactory.Create(
                host,
                glassEnabled: true,
                strengthPercent: 78,
                themeMode: dark ? GameSaveCenterThemeMode.Dark : GameSaveCenterThemeMode.Light);
            var resources = new ResourceDictionary();
            AdaptiveThemePaletteFactory.ApplyMaterialResources(resources, palette, glassEnabled: true, motionEnabled: false);

            var surface = Effect(resources, "GscSurfaceEffect");
            var sidebar = Effect(resources, "GscSidebarEffect");
            var popup = Effect(resources, "GscPopupEffect");
            var dialog = Effect(resources, "GscDialogEffect");
            var primary = Effect(resources, "GscPrimaryButtonEffect");
            var slider = Effect(resources, "GscSliderThumbEffect");

            Assert.Equal(14d, surface.BlurRadius);
            Assert.Equal(2d, surface.ShadowDepth);
            Assert.Equal(24d, sidebar.BlurRadius);
            Assert.Equal(3d, sidebar.ShadowDepth);
            Assert.Equal(20d, popup.BlurRadius);
            Assert.Equal(5d, popup.ShadowDepth);
            Assert.Equal(34d, dialog.BlurRadius);
            Assert.Equal(8d, dialog.ShadowDepth);
            Assert.Equal(18d, primary.BlurRadius);
            Assert.Equal(0d, primary.ShadowDepth);
            Assert.Equal(6d, slider.BlurRadius);
            Assert.Equal(1d, slider.ShadowDepth);
            Assert.True(dialog.BlurRadius > popup.BlurRadius && popup.BlurRadius > surface.BlurRadius);
            Assert.True(dialog.ShadowDepth > popup.ShadowDepth && popup.ShadowDepth > surface.ShadowDepth);
            Assert.All(new[] { surface, sidebar, popup, dialog, primary, slider }, effect =>
            {
                Assert.True(effect.Opacity > 0 && effect.Opacity < 1);
                Assert.True(effect.IsFrozen);
            });
            Assert.Equal(PopupAnimation.None, resources["GscPopupAnimation"]);
            Assert.Equal(true, resources["GscPopupAllowsTransparency"]);

            var lowCostResources = new ResourceDictionary();
            AdaptiveThemePaletteFactory.ApplyMaterialResources(lowCostResources, palette, glassEnabled: false, motionEnabled: false);
            Assert.All(new[]
            {
                "GscSurfaceEffect",
                "GscPrimaryButtonEffect",
                "GscSidebarEffect",
                "GscPopupEffect",
                "GscDialogEffect",
                "GscSliderThumbEffect",
                "GscGameBackgroundEffect"
            }, key => Assert.Null(lowCostResources[key]));
            Assert.Equal(false, lowCostResources["GscPopupAllowsTransparency"]);
            var wash = Assert.IsType<LinearGradientBrush>(lowCostResources["GscAmbientWideWashBrush"]);
            Assert.All(wash.GradientStops, stop => Assert.Equal((byte)0, stop.Color.A));
        });
    }

    [Fact]
    public void ElevatedCardEffectsDoNotExpandScrollViewerExtent()
    {
        RunSta(() =>
        {
            var resources = LoadProductionResources();
            var paletteHost = new Grid();
            var palette = AdaptiveThemePaletteFactory.Create(
                paletteHost,
                glassEnabled: true,
                strengthPercent: 78,
                themeMode: GameSaveCenterThemeMode.Dark);
            AdaptiveThemePaletteFactory.ApplyMaterialResources(resources, palette, glassEnabled: true, motionEnabled: true);

            var stack = new StackPanel { Resources = resources };
            var cards = Enumerable.Range(0, 3)
                .Select(_ => new Border
                {
                    Style = Assert.IsType<Style>(resources["GscElevatedSurface"]),
                    Width = 180,
                    Height = 72,
                    Margin = new Thickness(0, 0, 0, 8),
                    Child = new TextBlock { Text = "合成卡片" }
                })
                .ToArray();
            foreach (var card in cards)
                stack.Children.Add(card);

            var scroll = new ScrollViewer
            {
                Width = 220,
                Height = 120,
                Content = stack,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };
            var window = new Window
            {
                Content = scroll,
                Width = 240,
                Height = 140,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = false,
                Opacity = 0.01
            };

            try
            {
                window.Show();
                window.UpdateLayout();
                Assert.All(cards, card => Assert.IsType<DropShadowEffect>(card.Effect));
                Assert.True(scroll.ExtentHeight > scroll.ViewportHeight);
                var withEffects = scroll.ExtentHeight;

                foreach (var card in cards)
                    card.Effect = null;
                scroll.UpdateLayout();

                Assert.Equal(withEffects, scroll.ExtentHeight, 3);
                Assert.Equal(0, cards.Count(card => card.Effect != null));
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static DropShadowEffect Effect(ResourceDictionary resources, string key)
        => Assert.IsType<DropShadowEffect>(resources[key]);

    private static ResourceDictionary LoadProductionResources()
    {
        return Assert.IsType<ResourceDictionary>(XamlReader.Parse(@"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""><ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/Redesign.xaml""/>
</ResourceDictionary.MergedDictionaries></ResourceDictionary>"));
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
