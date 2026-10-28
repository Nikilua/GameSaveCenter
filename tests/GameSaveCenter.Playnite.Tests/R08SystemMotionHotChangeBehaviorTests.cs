using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("R08ProductionMotionWpf")]
public sealed class R08SystemMotionHotChangeBehaviorTests
{
    [Fact]
    public void SystemAnimationPreferenceChangeEndsEntranceAndReenableDoesNotReplayIt()
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                EnsureApplicationResources();
                var settings = new GameSaveCenterSettings
                {
                    ThemeMode = GameSaveCenterThemeMode.Light,
                    EnableUiAnimations = true
                };
                var systemAllowsMotion = true;
                var view = new GameSaveCenterSettingsView { DataContext = settings };
                view.MotionEnabledOverrideForBehaviorTest = () => settings.EnableUiAnimations && systemAllowsMotion;
                var motionProbe = new Border
                {
                    Width = 40,
                    Height = 40,
                    Background = System.Windows.Media.Brushes.Transparent,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };
                var root = new Grid();
                root.Children.Add(view);
                root.Children.Add(motionProbe);
                window = new Window
                {
                    Content = root,
                    Width = 1040,
                    Height = 720,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    WindowStyle = WindowStyle.None,
                    Opacity = 0.01
                };
                window.Show();
                window.UpdateLayout();
                DrainDispatcher();
                window.UpdateLayout();

                var settingsShell = Assert.IsAssignableFrom<FrameworkElement>(view.FindName("SettingsShell"));
                var shellTranslate = GscMotion.GetMutableTranslateTransform(settingsShell);
                var probeTranslate = GscMotion.GetMutableTranslateTransform(motionProbe);
                // The behavior under test is cancellation after a system-policy change;
                // a neutral probe avoids racing the settings page's own Loaded entrance.
                // Explicit durations keep this independent from live resource timing.
                GscMotion.AnimateEntrance(motionProbe, 12, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
                PumpDispatcher(TimeSpan.FromMilliseconds(55));
                Assert.True(
                    DependencyPropertyHelper.GetValueSource(motionProbe, UIElement.OpacityProperty).IsAnimated,
                    $"system-motion test did not start opacity animation; opacity={motionProbe.Opacity}; loaded={motionProbe.IsLoaded}");
                Assert.True(
                    DependencyPropertyHelper.GetValueSource(probeTranslate, TranslateTransform.YProperty).IsAnimated,
                    $"system-motion test did not start translation animation; y={probeTranslate.Y}; loaded={motionProbe.IsLoaded}");

                systemAllowsMotion = false;
                RaiseSystemParametersChanged(view);
                DrainDispatcher();
                window.UpdateLayout();

                Assert.True(settings.EnableUiAnimations, "the app preference must stay enabled while the system policy disables motion.");
                Assert.False(DependencyPropertyHelper.GetValueSource(motionProbe, UIElement.OpacityProperty).IsAnimated);
                Assert.False(DependencyPropertyHelper.GetValueSource(probeTranslate, TranslateTransform.YProperty).IsAnimated);
                Assert.False(DependencyPropertyHelper.GetValueSource(settingsShell, UIElement.OpacityProperty).IsAnimated);
                Assert.False(DependencyPropertyHelper.GetValueSource(shellTranslate, TranslateTransform.YProperty).IsAnimated);
                Assert.Equal(1, motionProbe.Opacity);
                Assert.Equal(0, probeTranslate.Y);
                Assert.Equal(1, settingsShell.Opacity);
                Assert.Equal(0, shellTranslate.Y);

                systemAllowsMotion = true;
                RaiseSystemParametersChanged(view);
                DrainDispatcher();
                window.UpdateLayout();

                // Re-enabling the system setting updates the theme policy only. It must
                // not replay an entrance animation for content that is already visible.
                Assert.False(DependencyPropertyHelper.GetValueSource(motionProbe, UIElement.OpacityProperty).IsAnimated);
                Assert.False(DependencyPropertyHelper.GetValueSource(probeTranslate, TranslateTransform.YProperty).IsAnimated);
                Assert.False(DependencyPropertyHelper.GetValueSource(settingsShell, UIElement.OpacityProperty).IsAnimated);
                Assert.False(DependencyPropertyHelper.GetValueSource(shellTranslate, TranslateTransform.YProperty).IsAnimated);
                Assert.Equal(1, motionProbe.Opacity);
                Assert.Equal(0, probeTranslate.Y);
                Assert.Equal(1, settingsShell.Opacity);
                Assert.Equal(0, shellTranslate.Y);
            }
            catch (Exception caught)
            {
                exception = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
    }

    private static void RaiseSystemParametersChanged(GameSaveCenterSettingsView view)
    {
        var handler = typeof(GameSaveCenterSettingsView).GetMethod(
            "OnSystemParametersChanged",
            BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("SettingsView system-preference handler was not found.");
        handler.Invoke(view, new object?[]
        {
            null,
            new PropertyChangedEventArgs(nameof(SystemParameters.ClientAreaAnimation))
        });
    }

    private static void EnsureApplicationResources()
    {
        var application = Application.Current ?? new Application();
        application.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        if (!application.Resources.Contains("BaseTextBlockStyle"))
            application.Resources.Add("BaseTextBlockStyle", new Style(typeof(TextBlock)));
    }

    private static void DrainDispatcher()
    {
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
    }

    private static void PumpDispatcher(TimeSpan duration)
    {
        var end = DateTime.UtcNow + duration;
        while (DateTime.UtcNow < end)
        {
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
            Thread.Sleep(5);
        }

        DrainDispatcher();
    }
}
