using System;
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

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var host = new Border { Width = 48, Height = 48, Background = Brushes.Transparent };
                host.Resources["GscMotionNormal"] = new Duration(TimeSpan.FromMilliseconds(240));
                window = CreateWindow(host, 90, 90);
                window.Show();
                window.UpdateLayout();

                var translate = GscMotion.GetMutableTranslateTransform(host);
                GscMotion.AnimateTranslate(host, 12, 0, GscMotion.MotionDurationKind.Normal);
                PumpDispatcher(TimeSpan.FromMilliseconds(80));
                window.UpdateLayout();
                firstMidpoint = translate.X;
                GscMotion.AnimateTranslate(host, -8, 0, GscMotion.MotionDurationKind.Normal);
                reversalStart = translate.X;
                PumpDispatcher(TimeSpan.FromMilliseconds(500));
                window.UpdateLayout();
                finalValue = translate.X;

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

        output.WriteLine($"translate midpoint={firstMidpoint:0.###}; reversalStart={reversalStart:0.###}; final={finalValue:0.###}");
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
        var finalOpacity = 0d;
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
                PumpDispatcher(TimeSpan.FromMilliseconds(200));
                window.UpdateLayout();
                shell.UpdateLayout();
                reverseMidpoint = sidebar.ActualWidth;
                sidebarTrace += $" | after-reverse-200 collapsed={shell.SidebarCollapsedForAudit}; running={shell.SidebarTransitionRunningForAudit}; base={shell.SidebarWidthForAudit:0.###}; actual={sidebar.ActualWidth:0.###}";
                PumpDispatcher(TimeSpan.FromMilliseconds(1000));
                window.UpdateLayout();
                shell.UpdateLayout();
                finalWidth = sidebar.ActualWidth;
                finalOpacity = layer.Opacity;

                Assert.InRange(collapseMidpoint, 72.2, 269.8);
                Assert.InRange(reversalStart, collapseMidpoint - 1.5, collapseMidpoint + 1.5);
                Assert.True(reverseMidpoint > reversalStart, $"sidebar did not reverse toward expanded target: {reverseMidpoint} <= {reversalStart}");
                Assert.Equal(270, finalWidth, 1);
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

        output.WriteLine($"sidebar collapseMid={collapseMidpoint:0.###}; reversalStart={reversalStart:0.###}; reverseMid={reverseMidpoint:0.###}; final={finalWidth:0.###}; opacity={finalOpacity:0.###}; trace={sidebarTrace}");
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
}
