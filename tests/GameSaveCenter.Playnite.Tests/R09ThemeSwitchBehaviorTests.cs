using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R09ThemeSwitchBehaviorTests
{
    [Fact]
    public void ThemeSwitchUpdatesPopupIconAndPlaceholderBeforeTheNextRenderWithoutHostPollution()
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                var root = new Grid();
                var popupSurface = new Border();
                popupSurface.SetResourceReference(Border.BackgroundProperty, "GscPopupBrush");
                var icon = new Path
                {
                    Data = Geometry.Parse("M0,0 L8,8")
                };
                icon.SetResourceReference(Shape.StrokeProperty, "GscAccentBrush");
                var placeholderSurface = new Border();
                placeholderSurface.SetResourceReference(Border.BackgroundProperty, "GscControlFillBrush");
                var placeholderText = new TextBlock();
                placeholderText.SetResourceReference(TextBlock.ForegroundProperty, "GscSecondaryTextBrush");
                root.Children.Add(new StackPanel
                {
                    Children = { popupSurface, icon, placeholderSurface, placeholderText }
                });

                var hostSentinel = new SolidColorBrush(Colors.Magenta);
                var host = new Border();
                host.Resources["GscPopupBrush"] = hostSentinel;

                var light = AdaptiveThemePaletteFactory.Create(
                    root, glassEnabled: true, strengthPercent: 78, GameSaveCenterThemeMode.Light);
                AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(
                    root.Resources, light, glassEnabled: true, motionEnabled: true);
                root.UpdateLayout();

                var lightPopup = Assert.IsType<SolidColorBrush>(popupSurface.Background).Color;
                var lightIcon = Assert.IsType<SolidColorBrush>(icon.Stroke).Color;
                var lightPlaceholder = Assert.IsType<SolidColorBrush>(placeholderSurface.Background).Color;
                Assert.NotEqual(Colors.Magenta, lightPopup);
                Assert.NotEqual(Colors.Transparent, lightIcon);
                Assert.NotEqual(Colors.Transparent, lightPlaceholder);

                var dark = AdaptiveThemePaletteFactory.Create(
                    root, glassEnabled: true, strengthPercent: 78, GameSaveCenterThemeMode.Dark);
                var frame = new DispatcherFrame();
                var renderObserved = false;
                root.Dispatcher.BeginInvoke(new Action(() =>
                {
                    AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(
                        root.Resources, dark, glassEnabled: true, motionEnabled: true);
                    root.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        root.UpdateLayout();
                        var darkPopup = Assert.IsType<SolidColorBrush>(popupSurface.Background).Color;
                        var darkIcon = Assert.IsType<SolidColorBrush>(icon.Stroke).Color;
                        var darkPlaceholder = Assert.IsType<SolidColorBrush>(placeholderSurface.Background).Color;

                        Assert.NotEqual(lightPopup, darkPopup);
                        Assert.NotEqual(lightIcon, darkIcon);
                        Assert.NotEqual(lightPlaceholder, darkPlaceholder);
                        Assert.NotEqual(Colors.Transparent, darkPopup);
                        Assert.NotEqual(Colors.Transparent, darkIcon);
                        Assert.NotEqual(Colors.Transparent, darkPlaceholder);
                        Assert.Equal(hostSentinel, host.Resources["GscPopupBrush"]);
                        renderObserved = true;
                        frame.Continue = false;
                    }), DispatcherPriority.Render);
                }), DispatcherPriority.Background);
                Dispatcher.PushFrame(frame);
                Assert.True(renderObserved);
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
    }
}
