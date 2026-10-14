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
                view.Resources["GscMotionNormal"] = new Duration(TimeSpan.FromSeconds(1));
                view.Resources["GscMotionSlow"] = new Duration(TimeSpan.FromSeconds(1));
                window = new Window
                {
                    Content = view,
                    Width = 1040,
                    Height = 720,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    WindowStyle = WindowStyle.None,
                    Opacity = 0.01
                };
                window.Show();
                window.UpdateLayout();

                var shell = Assert.IsAssignableFrom<FrameworkElement>(view.FindName("SettingsShell"));
                var translate = GscMotion.GetMutableTranslateTransform(shell);
                GscMotion.AnimateEntrance(shell, 12);
                PumpDispatcher(TimeSpan.FromMilliseconds(55));
                Assert.True(DependencyPropertyHelper.GetValueSource(shell, UIElement.OpacityProperty).IsAnimated);
                Assert.True(DependencyPropertyHelper.GetValueSource(translate, TranslateTransform.YProperty).IsAnimated);

                systemAllowsMotion = false;
                RaiseSystemParametersChanged(view);
                DrainDispatcher();
                window.UpdateLayout();

                Assert.True(settings.EnableUiAnimations, "the app preference must stay enabled while the system policy disables motion.");
                Assert.False(DependencyPropertyHelper.GetValueSource(shell, UIElement.OpacityProperty).IsAnimated);
                Assert.False(DependencyPropertyHelper.GetValueSource(translate, TranslateTransform.YProperty).IsAnimated);
                Assert.Equal(1, shell.Opacity);
                Assert.Equal(0, translate.Y);

                systemAllowsMotion = true;
                RaiseSystemParametersChanged(view);
                DrainDispatcher();
                window.UpdateLayout();
                PumpDispatcher(TimeSpan.FromMilliseconds(1100));

                Assert.False(DependencyPropertyHelper.GetValueSource(shell, UIElement.OpacityProperty).IsAnimated);
                Assert.False(DependencyPropertyHelper.GetValueSource(translate, TranslateTransform.YProperty).IsAnimated);
                Assert.Equal(1, shell.Opacity);
                Assert.Equal(0, translate.Y);
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
