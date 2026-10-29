using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class RuntimePalettePublicationBehaviorTests
{
    [Theory]
    [InlineData(GameSaveCenterThemeMode.Light, false)]
    [InlineData(GameSaveCenterThemeMode.Dark, false)]
    [InlineData(GameSaveCenterThemeMode.Light, true)]
    [InlineData(GameSaveCenterThemeMode.Dark, true)]
    public void ThemeApplicationPublishesOnlyTheFinalSemanticBrush(
        GameSaveCenterThemeMode theme, bool highContrast)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                var probe = new ResourceProbe();
                var retained = new object();
                var merged = new ResourceDictionary();
                probe.Resources["CallerOwnedResource"] = retained;
                probe.Resources.MergedDictionaries.Add(merged);
                var initial = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(
                    probe, false, 50,
                    theme == GameSaveCenterThemeMode.Light
                        ? GameSaveCenterThemeMode.Dark : GameSaveCenterThemeMode.Light,
                    false);
                AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(probe.Resources, initial, false, true);
                var nullableEffects = new[] { "GscSurfaceEffect", "GscPrimaryButtonEffect", "GscSidebarEffect",
                    "GscPopupEffect", "GscDialogEffect", "GscSliderThumbEffect", "GscGameBackgroundEffect" };
                foreach (var key in nullableEffects)
                    probe.Resources[key] = new DropShadowEffect();
                probe.SetResourceReference(ResourceProbe.ObservedBrushProperty, "GscInfoBrush");
                Assert.IsType<SolidColorBrush>(probe.GetValue(ResourceProbe.ObservedBrushProperty));
                probe.Changes.Clear();

                var palette = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(
                    probe, true, 50, theme, highContrast);
                AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(probe.Resources, palette, true, true);

                var current = Assert.IsType<SolidColorBrush>(probe.GetValue(ResourceProbe.ObservedBrushProperty));
                Assert.Equal(palette.Info, current.Color);
                Assert.Single(probe.Changes);
                Assert.Equal(palette.Info, probe.Changes.Single());
                Assert.Same(retained, probe.Resources["CallerOwnedResource"]);
                Assert.Same(merged, probe.Resources.MergedDictionaries.Single());
                if (highContrast)
                    Assert.All(nullableEffects, key => Assert.Null(probe.Resources[key]));
            }
            catch (Exception caught) { failure = caught; }
            finally
            {
                try { Dispatcher.CurrentDispatcher.InvokeShutdown(); }
                catch (Exception cleanup) { failure = failure == null ? cleanup : new AggregateException(failure, cleanup); }
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure != null) throw new Xunit.Sdk.XunitException(failure.ToString());
    }

    private sealed class ResourceProbe : FrameworkElement
    {
        public List<Color> Changes { get; } = new();
        public static readonly DependencyProperty ObservedBrushProperty = DependencyProperty.Register(
            "ObservedBrush", typeof(Brush), typeof(ResourceProbe),
            new FrameworkPropertyMetadata(null, (sender, args) =>
            {
                if (args.NewValue is SolidColorBrush brush)
                    ((ResourceProbe)sender).Changes.Add(brush.Color);
            }));
    }
}
