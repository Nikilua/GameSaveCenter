using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Controls;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R08MotionHotChangeBehaviorTests
{
    [Fact]
    public void SettingsAnimationToggleEndsEntranceAndReenableDoesNotReplayIt()
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
                var view = new GameSaveCenterSettingsView { DataContext = settings };
                view.Resources["GscMotionNormal"] = new Duration(TimeSpan.FromSeconds(1));
                view.Resources["GscMotionSlow"] = new Duration(TimeSpan.FromSeconds(1));
                window = CreateWindow(view, 1040, 720);
                window.Show();
                window.UpdateLayout();

                var tabs = (ListBox)view.FindName("SettingsSectionTabs")!;
                tabs.SelectedIndex = 2;
                DrainDispatcher();
                window.UpdateLayout();

                var shell = Assert.IsAssignableFrom<FrameworkElement>(view.FindName("SettingsShell"));
                var translate = GscMotion.GetMutableTranslateTransform(shell);
                GscMotion.AnimateEntrance(shell, 12);
                PumpDispatcher(TimeSpan.FromMilliseconds(55));
                Assert.True(DependencyPropertyHelper.GetValueSource(shell, UIElement.OpacityProperty).IsAnimated);
                Assert.True(DependencyPropertyHelper.GetValueSource(translate, TranslateTransform.YProperty).IsAnimated);

                var toggle = FindToggle(view, "启用界面动画");
                toggle.IsChecked = false;
                DrainDispatcher();
                window.UpdateLayout();

                Assert.False(settings.EnableUiAnimations);
                Assert.False(DependencyPropertyHelper.GetValueSource(shell, UIElement.OpacityProperty).IsAnimated);
                Assert.False(DependencyPropertyHelper.GetValueSource(translate, TranslateTransform.YProperty).IsAnimated);
                Assert.Equal(1, shell.Opacity);
                Assert.Equal(0, translate.Y);

                toggle.IsChecked = true;
                DrainDispatcher();
                window.UpdateLayout();
                PumpDispatcher(TimeSpan.FromMilliseconds(1100));

                Assert.True(settings.EnableUiAnimations);
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

    private static ToggleSwitch FindToggle(FrameworkElement root, string automationName)
        => FindVisualChildren<ToggleSwitch>(root)
            .Single(toggle => string.Equals(
                AutomationProperties.GetName(toggle), automationName, StringComparison.Ordinal));

    private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
                yield return match;

            foreach (var descendant in FindVisualChildren<T>(child))
                yield return descendant;
        }
    }

    private static Window CreateWindow(FrameworkElement content, double width = 180, double height = 120)
        => new Window
        {
            Content = content,
            Width = width,
            Height = height,
            ShowInTaskbar = false,
            ShowActivated = false,
            WindowStyle = WindowStyle.None,
            Opacity = 0.01
        };

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
