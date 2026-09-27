using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Views;
using Xunit;
using Xunit.Abstractions;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R08MotionReverseBehaviorTests
{
    private readonly ITestOutputHelper output;

    public R08MotionReverseBehaviorTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void TranslateReversalStartsAtRenderedValueAndFinishesAtLatestTarget()
    {
        Exception? exception = null;
        var firstMidpoint = 0d;
        var reversalStart = 0d;
        var finalValue = 0d;
        var midpointSampled = false;
        var translationCompleted = false;

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var host = new Border { Width = 48, Height = 48, Background = Brushes.Transparent };
                host.Resources["GscMotionNormal"] = new Duration(TimeSpan.FromSeconds(2));
                window = CreateWindow(host, 90, 90);
                window.Show();
                window.UpdateLayout();

                var translate = GscMotion.GetMutableTranslateTransform(host);
                GscMotion.AnimateTranslate(host, 12, 0, GscMotion.MotionDurationKind.Normal);
                midpointSampled = PumpDispatcherUntil(
                    () =>
                    {
                        window.UpdateLayout();
                        return translate.X > 0.2 && translate.X < 11.8;
                    },
                    TimeSpan.FromSeconds(1));
                firstMidpoint = translate.X;
                GscMotion.AnimateTranslate(host, -8, 0, GscMotion.MotionDurationKind.Normal);
                reversalStart = translate.X;
                translationCompleted = PumpDispatcherUntil(
                    () =>
                    {
                        window.UpdateLayout();
                        var xIsAnimated = DependencyPropertyHelper.GetValueSource(translate, TranslateTransform.XProperty).IsAnimated;
                        var yIsAnimated = DependencyPropertyHelper.GetValueSource(translate, TranslateTransform.YProperty).IsAnimated;
                        return !xIsAnimated
                            && !yIsAnimated
                            && Math.Abs(translate.X + 8) < 0.001
                            && Math.Abs(translate.Y) < 0.001;
                    },
                    TimeSpan.FromSeconds(3));
                window.UpdateLayout();
                finalValue = translate.X;

                Assert.True(midpointSampled, "translation never exposed an in-flight sample within the one-second sampling window");
                Assert.True(translationCompleted, "translation did not release both animation clocks within 3 seconds");
                Assert.InRange(firstMidpoint, 0.2, 11.8);
                Assert.InRange(reversalStart, firstMidpoint - 0.8, firstMidpoint + 0.8);
                Assert.Equal(-8, finalValue, 3);
                Assert.False(DependencyPropertyHelper.GetValueSource(translate, TranslateTransform.XProperty).IsAnimated);
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

        output.WriteLine($"translate midpoint={firstMidpoint:0.###}; sampled={midpointSampled}; reversalStart={reversalStart:0.###}; final={finalValue:0.###}; completed={translationCompleted}");
        Assert.Null(exception);
    }

    [Fact]
    public void SidebarRapidReversalUsesLatestTargetAndReleasesOldClock()
    {
        Exception? exception = null;
        var collapseMidpoint = 0d;
        var reversalStart = 0d;
        var reverseMidpoint = 0d;
        var finalWidth = 0d;
        var finalWidthRoundingTolerance = 0d;
        var finalOpacity = 0d;
        var sidebarTransitionCompleted = false;
        var sidebarTrace = string.Empty;

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var shell = new AcrylicProductionShellView
                {
                    MotionEnabledProvider = () => true,
                    SidebarCollapsedProvider = () => false
                };
                shell.Resources["GscMotionNormal"] = new Duration(TimeSpan.FromSeconds(1));
                window = CreateWindow(shell, 900, 640);
                window.Show();
                window.UpdateLayout();
                shell.UpdateLayout();
                finalWidthRoundingTolerance = 0.5 / VisualTreeHelper.GetDpi(shell).DpiScaleX + 0.001;

                var sidebar = Assert.IsType<ColumnDefinition>(shell.FindName("SidebarColumn"));
                var layer = Assert.IsAssignableFrom<FrameworkElement>(shell.FindName("SidebarContentLayer"));
                shell.SidebarCollapseButtonForAudit.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                PumpDispatcher(TimeSpan.FromMilliseconds(100));
                window.UpdateLayout();
                shell.UpdateLayout();
                collapseMidpoint = sidebar.ActualWidth;
                sidebarTrace = $"after-collapse collapsed={shell.SidebarCollapsedForAudit}; running={shell.SidebarTransitionRunningForAudit}; base={shell.SidebarWidthForAudit:0.###}; actual={sidebar.ActualWidth:0.###}";

                shell.SidebarCollapseButtonForAudit.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                reversalStart = sidebar.ActualWidth;
                PumpDispatcherUntil(
                    () =>
                    {
                        window.UpdateLayout();
                        shell.UpdateLayout();
                        return sidebar.ActualWidth > reversalStart + 0.1;
                    },
                    TimeSpan.FromMilliseconds(600));
                reverseMidpoint = sidebar.ActualWidth;
                sidebarTrace += $" | after-reverse-progress collapsed={shell.SidebarCollapsedForAudit}; running={shell.SidebarTransitionRunningForAudit}; base={shell.SidebarWidthForAudit:0.###}; actual={sidebar.ActualWidth:0.###}";
                sidebarTransitionCompleted = PumpDispatcherUntil(
                    () =>
                    {
                        window.UpdateLayout();
                        shell.UpdateLayout();
                        return !shell.SidebarTransitionRunningForAudit;
                    },
                    TimeSpan.FromSeconds(2));
                window.UpdateLayout();
                shell.UpdateLayout();
                finalWidth = sidebar.ActualWidth;
                finalOpacity = layer.Opacity;

                Assert.InRange(collapseMidpoint, 72.2, 269.8);
                Assert.InRange(reversalStart, collapseMidpoint - 1.5, collapseMidpoint + 1.5);
                Assert.True(reverseMidpoint > reversalStart, $"sidebar did not reverse toward expanded target: {reverseMidpoint} <= {reversalStart}");
                Assert.True(sidebarTransitionCompleted, "sidebar animation did not deliver its completion callback within 2 seconds");
                Assert.InRange(Math.Abs(270 - finalWidth), 0, finalWidthRoundingTolerance);
                Assert.False(shell.SidebarTransitionRunningForAudit);
                Assert.Equal(1, finalOpacity, 3);
                Assert.False(DependencyPropertyHelper.GetValueSource(layer, UIElement.OpacityProperty).IsAnimated);
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

        output.WriteLine($"sidebar collapseMid={collapseMidpoint:0.###}; reversalStart={reversalStart:0.###}; reverseMid={reverseMidpoint:0.###}; final={finalWidth:0.###}; opacity={finalOpacity:0.###}; completed={sidebarTransitionCompleted}; trace={sidebarTrace}");
        Assert.Null(exception);
    }

    private static Window CreateWindow(UIElement content, double width, double height)
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
        var timeoutWatch = Stopwatch.StartNew();
        while (timeoutWatch.Elapsed < timeout)
        {
            if (condition())
                return true;

            PumpDispatcher(TimeSpan.FromMilliseconds(40));
        }

        return condition();
    }
}
