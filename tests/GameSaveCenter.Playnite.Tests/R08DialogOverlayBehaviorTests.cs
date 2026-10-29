using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R08DialogOverlayBehaviorTests
{
    [Fact]
    public void LifecycleClaimsCompletionOnceAndBlocksReentryDuringExit()
    {
        var lifecycle = new DialogLifecycleStateMachine();

        Assert.True(lifecycle.TryBeginOpening());
        Assert.Equal(DialogLifecycleState.Opening, lifecycle.State);
        Assert.True(lifecycle.TryMarkOpen());
        Assert.True(lifecycle.TryClaimCompletion());
        Assert.True(lifecycle.TryBeginClosing());
        Assert.Equal(DialogLifecycleState.Closing, lifecycle.State);
        Assert.False(lifecycle.TryClaimCompletion());
        Assert.False(lifecycle.TryBeginOpening());
        Assert.True(lifecycle.TryFinishClosing());
        Assert.Equal(DialogLifecycleState.Closed, lifecycle.State);
        Assert.False(lifecycle.TryFinishClosing());
    }

    [Fact]
    public void OverlayStaysVisibleDuringCardExitAndCollapsesAfterTheTransition()
    {
        Exception? failure = null;
        bool opened = false;
        bool closed = false;

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var overlay = new Grid
                {
                    Width = 240,
                    Height = 140,
                    Visibility = Visibility.Collapsed
                };
                var card = new Border
                {
                    Width = 120,
                    Height = 60,
                    RenderTransformOrigin = new Point(0.5, 0.5)
                };
                overlay.Resources["GscMotionNormal"] = new Duration(TimeSpan.FromSeconds(1));
                overlay.Children.Add(card);
                window = CreateWindow(overlay);
                window.Show();
                FlushLayout(window);

                var motion = new DialogOverlayMotion(overlay, card);
                var duration = GscMotion.GetDuration(card, GscMotion.MotionDurationKind.Normal);
                Assert.Equal(TimeSpan.FromSeconds(1), duration);
                var openStarted = DateTime.UtcNow;
                var openElapsed = Stopwatch.StartNew();
                motion.BeginOpen(true, () => opened = true);
                Assert.Equal(Visibility.Visible, overlay.Visibility);
                Assert.True(WaitFor(window, () => opened));
                Assert.True(openElapsed.Elapsed >= TimeSpan.FromMilliseconds(100),
                    $"open completed early; monotonic={openElapsed.Elapsed.TotalMilliseconds:0.###}ms; wall={(DateTime.UtcNow - openStarted).TotalMilliseconds:0.###}ms; duration={duration.TotalMilliseconds:0.###}ms; opacity={card.Opacity}; overlay={overlay.Visibility}");

                var closeStarted = DateTime.UtcNow;
                var closeElapsed = Stopwatch.StartNew();
                motion.BeginClose(true, () => closed = true);
                Assert.Equal(Visibility.Visible, overlay.Visibility);
                Assert.False(closed);
                if (!WaitFor(window, () => closed))
                {
                    throw new InvalidOperationException(
                        $"close callback did not run; opacity={card.Opacity}; overlay={overlay.Visibility}; animated={DependencyPropertyHelper.GetValueSource(card, UIElement.OpacityProperty).IsAnimated}");
                }
                Assert.True(closeElapsed.Elapsed >= TimeSpan.FromMilliseconds(100),
                    $"close completed early; monotonic={closeElapsed.Elapsed.TotalMilliseconds:0.###}ms; wall={(DateTime.UtcNow - closeStarted).TotalMilliseconds:0.###}ms; duration={duration.TotalMilliseconds:0.###}ms; opacity={card.Opacity}; overlay={overlay.Visibility}");
                Assert.Equal(Visibility.Collapsed, overlay.Visibility);
                Assert.Equal(0d, card.Opacity, 3);
                var translate = Assert.IsType<System.Windows.Media.TranslateTransform>(card.RenderTransform);
                Assert.Equal(0d, translate.Y, 3);
            }
            catch (Exception caught)
            {
                failure = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(failure);
        if (!opened || !closed)
        {
            throw new InvalidOperationException(
                $"dialog motion terminal flags were not all observed; opened={opened}; closed={closed}");
        }
    }

    [Fact]
    public void DashboardUsesTheSharedOverlayLifecycleForCompletionAndFocusGuards()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var code = File.ReadAllText(Path.Combine(
            root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml.cs"));
        var xaml = File.ReadAllText(Path.Combine(
            root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml"));

        Assert.Contains("private readonly DialogLifecycleStateMachine dialogLifecycle", code);
        Assert.Contains("dialogMotion.BeginClose(MotionEnabled", code);
        Assert.Contains("dialogLifecycle.TryClaimCompletion()", code);
        Assert.Contains("dialogLifecycle.TryFinishClosing()", code);
        Assert.Contains("if (dialogLifecycle.IsClosing) return;", code);
        Assert.Contains("FocusManager.IsFocusScope=\"True\"", xaml);
        Assert.Contains("KeyboardNavigation.TabNavigation=\"Cycle\"", xaml);
    }

    private static Window CreateWindow(UIElement content)
    {
        var window = new Window
        {
            Content = content,
            Width = 260,
            Height = 160,
            ShowInTaskbar = false,
            ShowActivated = false,
            WindowStyle = WindowStyle.None,
            Opacity = 0.01
        };
        AutomationProperties.SetName(window, "对话框进入退出动画测试宿主");
        return window;
    }

    private static void FlushLayout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
        window.UpdateLayout();
    }

    private static bool WaitFor(Window window, Func<bool> predicate)
    {
        var frame = new DispatcherFrame();
        var elapsed = Stopwatch.StartNew();
        var timer = new DispatcherTimer(DispatcherPriority.Background, window.Dispatcher)
        {
            Interval = TimeSpan.FromMilliseconds(10)
        };
        timer.Tick += (_, __) =>
        {
            if (predicate() || elapsed.Elapsed >= TimeSpan.FromMilliseconds(1500))
            {
                timer.Stop();
                frame.Continue = false;
            }
        };
        timer.Start();
        Dispatcher.PushFrame(frame);

        return predicate();
    }

}
