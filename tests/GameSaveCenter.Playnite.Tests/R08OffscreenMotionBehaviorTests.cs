using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Controls;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R08OffscreenMotionBehaviorTests
{
    [Fact]
    public void IndeterminateProgressPausesForHiddenTabAndMinimizedWindowThenResumes()
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                _ = Application.Current ?? new Application();
                Application.ResourceAssembly = typeof(IndeterminateProgressBehavior).Assembly;
                var dictionary = new ResourceDictionary
                {
                    Source = new Uri(
                        "/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml",
                        UriKind.Relative)
                };
                var progress = new ProgressBar
                {
                    Width = 80,
                    Height = 4,
                    IsIndeterminate = true
                };
                IndeterminateProgressBehavior.SetPauseWhenUnavailable(progress, true);
                var firstTab = new TabItem { Header = "进行中", Content = progress };
                var tabs = new TabControl
                {
                    Items =
                    {
                        firstTab,
                        new TabItem { Header = "其他", Content = new Border { Width = 80, Height = 24 } }
                    }
                };
                window = new Window
                {
                    Content = tabs,
                    Width = 220,
                    Height = 120,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    WindowStyle = WindowStyle.None,
                    Opacity = 0.01
                };
                window.Resources.MergedDictionaries.Add(dictionary);
                window.Show();
                window.UpdateLayout();

                var indicator = FindVisualChildren<Border>(progress)
                    .Single(border => border.Name == "PART_Indicator");
                var translate = Assert.IsType<TranslateTransform>(indicator.RenderTransform);
                PumpDispatcher(TimeSpan.FromMilliseconds(180));
                var activeStart = translate.X;
                var activeMoved = PumpDispatcherUntil(() => Math.Abs(translate.X - activeStart) > 0.5, TimeSpan.FromMilliseconds(600));
                var activeEnd = translate.X;
                Assert.False(IndeterminateProgressBehavior.GetIsAnimationPaused(progress));
                Assert.True(activeMoved, $"active spinner did not move: {activeStart} -> {activeEnd}");

                tabs.SelectedIndex = 1;
                PumpDispatcher(TimeSpan.FromMilliseconds(100));
                Assert.True(IndeterminateProgressBehavior.GetIsAnimationPaused(progress));
                Assert.True(progress.IsIndeterminate, "hiding the tab must not change the busy state");
                var hiddenStart = translate.X;
                PumpDispatcher(TimeSpan.FromMilliseconds(360));
                Assert.Equal(hiddenStart, translate.X, 3);
                Assert.True(progress.IsIndeterminate, "pausing the hidden animation must preserve the busy state");

                tabs.SelectedIndex = 0;
                PumpDispatcher(TimeSpan.FromMilliseconds(100));
                Assert.False(IndeterminateProgressBehavior.GetIsAnimationPaused(progress));
                Assert.True(progress.IsIndeterminate, "restoring the tab must preserve the busy state");
                var restoredStart = translate.X;
                Assert.True(PumpDispatcherUntil(() => Math.Abs(translate.X - restoredStart) > 0.5, TimeSpan.FromMilliseconds(600)), "spinner did not resume after the tab became visible");

                window.WindowState = WindowState.Minimized;
                PumpDispatcher(TimeSpan.FromMilliseconds(100));
                Assert.True(IndeterminateProgressBehavior.GetIsAnimationPaused(progress));
                Assert.True(progress.IsIndeterminate, "minimizing the window must not change the busy state");
                var minimizedStart = translate.X;
                PumpDispatcher(TimeSpan.FromMilliseconds(360));
                Assert.Equal(minimizedStart, translate.X, 3);
                Assert.True(progress.IsIndeterminate, "pausing the minimized animation must preserve the busy state");

                window.WindowState = WindowState.Normal;
                PumpDispatcher(TimeSpan.FromMilliseconds(100));
                Assert.False(IndeterminateProgressBehavior.GetIsAnimationPaused(progress));
                Assert.True(progress.IsIndeterminate, "restoring the window must preserve the busy state");
                var unminimizedStart = translate.X;
                Assert.True(PumpDispatcherUntil(() => Math.Abs(translate.X - unminimizedStart) > 0.5, TimeSpan.FromMilliseconds(600)), "spinner did not resume after the window was restored");
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

    private static void PumpDispatcher(TimeSpan duration)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer { Interval = duration };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            frame.Continue = false;
        };
        timer.Start();
        Dispatcher.PushFrame(frame);
    }

    private static bool PumpDispatcherUntil(Func<bool> condition, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            if (condition())
                return true;

            PumpDispatcher(TimeSpan.FromMilliseconds(40));
        }

        return condition();
    }
}
