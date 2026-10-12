using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R05TooltipTimingBehaviorTests
{
    [Fact]
    public void QuickMousePassesCancelTooltipAndAStationaryDwellOpensIt()
    {
        RunSta(() =>
        {
            var shell = new AcrylicProductionShellView();
            var controls = new StackPanel { Orientation = Orientation.Horizontal };
            var owners = Enumerable.Range(0, 3)
                .Select(index => new Button
                {
                    Content = "Help " + index,
                    Width = 92,
                    Height = 42,
                    Margin = new Thickness(4)
                })
                .ToArray();
            var tooltips = owners.Select((owner, index) =>
            {
                var tooltip = new ToolTip
                {
                    Content = "Synthetic help " + index,
                    Style = (Style)shell.FindResource(typeof(ToolTip)),
                    PlacementTarget = owner
                };
                owner.ToolTip = tooltip;
                controls.Children.Add(owner);
                return tooltip;
            }).ToList();
            var root = new Grid
            {
                Margin = new Thickness(8),
                Width = 560,
                Height = 90,
                Background = System.Windows.Media.Brushes.Transparent
            };
            root.Children.Add(controls);
            shell.Content = root;

            var window = new Window
            {
                Content = shell,
                Width = 590,
                Height = 140,
                ShowInTaskbar = false,
                ShowActivated = true,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };

            var originalCursor = new NativePoint();
            var cursorCaptured = GetCursorPos(ref originalCursor);
            Assert.True(cursorCaptured, "应能保存隔离 Tooltip 行为测试启动前的系统指针位置。");
            try
            {
                window.Show();
                window.UpdateLayout();
                var source = PresentationSource.FromVisual(window) as HwndSource;
                Assert.NotNull(source);
                Assert.Equal(350, ToolTipService.GetInitialShowDelay(shell));
                Assert.Equal(0, ToolTipService.GetBetweenShowDelay(shell));
                Assert.All(owners, owner =>
                {
                    Assert.Equal(350, ToolTipService.GetInitialShowDelay(owner));
                    Assert.Equal(0, ToolTipService.GetBetweenShowDelay(owner));
                });

                var lateOwner = new Button
                {
                    Content = "Late help",
                    Width = 92,
                    Height = 42,
                    Margin = new Thickness(4)
                };
                var lateTooltip = new ToolTip
                {
                    Content = "Synthetic late help",
                    Style = (Style)shell.FindResource(typeof(ToolTip)),
                    PlacementTarget = lateOwner
                };
                lateOwner.ToolTip = lateTooltip;
                controls.Children.Add(lateOwner);
                tooltips.Add(lateTooltip);

                var overrideOwner = new Button
                {
                    Content = "Custom help",
                    Width = 92,
                    Height = 42,
                    Margin = new Thickness(4)
                };
                ToolTipService.SetInitialShowDelay(overrideOwner, 900);
                ToolTipService.SetBetweenShowDelay(overrideOwner, 250);
                var overrideTooltip = new ToolTip
                {
                    Content = "Synthetic custom help",
                    Style = (Style)shell.FindResource(typeof(ToolTip)),
                    PlacementTarget = overrideOwner
                };
                overrideOwner.ToolTip = overrideTooltip;
                controls.Children.Add(overrideOwner);
                tooltips.Add(overrideTooltip);
                window.UpdateLayout();

                Assert.Equal(350, ToolTipService.GetInitialShowDelay(lateOwner));
                Assert.Equal(0, ToolTipService.GetBetweenShowDelay(lateOwner));
                Assert.Equal(900, ToolTipService.GetInitialShowDelay(overrideOwner));
                Assert.Equal(250, ToolTipService.GetBetweenShowDelay(overrideOwner));

                var openedCount = 0;
                foreach (var tooltip in tooltips)
                    tooltip.Opened += (_, _) => openedCount++;

                foreach (var owner in owners.Append(lateOwner))
                {
                    MoveCursorTo(owner);
                    PumpDispatcherFor(TimeSpan.FromMilliseconds(80));
                }

                MoveCursorTo(root, new Point(root.ActualWidth / 2, root.ActualHeight - 6));
                PumpDispatcherFor(TimeSpan.FromMilliseconds(420));
                Assert.Equal(0, openedCount);
                Assert.All(tooltips, tooltip => Assert.False(tooltip.IsOpen));

                MoveCursorTo(owners[0]);
                PumpDispatcherFor(TimeSpan.FromMilliseconds(420));
                Assert.Equal(1, openedCount);
                Assert.True(tooltips[0].IsOpen);

                MoveCursorTo(owners[1]);
                PumpDispatcherFor(TimeSpan.FromMilliseconds(80));
                MoveCursorTo(owners[2]);
                PumpDispatcherFor(TimeSpan.FromMilliseconds(80));
                MoveCursorTo(root, new Point(root.ActualWidth / 2, root.ActualHeight - 6));
                PumpDispatcherFor(TimeSpan.FromMilliseconds(420));
                Assert.Equal(1, openedCount);
                Assert.All(tooltips, tooltip => Assert.False(tooltip.IsOpen));

            }
            finally
            {
                window.Close();
                if (cursorCaptured)
                    SetCursorPos(originalCursor.X, originalCursor.Y);
            }
        });
    }

    [Fact]
    public void ProductionTooltipWrapsLongPathsAndEscClosesWithoutMovingFocus()
    {
        RunSta(() =>
        {
            var shell = new AcrylicProductionShellView();
            var root = new Grid { Margin = new Thickness(18) };
            var path = @"C:\Synthetic\Profiles\GameSaveCenter\Profiles\LongGameName\SaveData\slot-0001\screenshots\2026\09\17\a-very-long-file-name-that-must-remain-readable-without-changing-the-real-files.png";
            var owner = new TextBox
            {
                Width = 180,
                Text = path,
                IsReadOnly = true,
                Style = (Style)shell.FindResource("GscWpfUiPathDetailTextBox")
            };
            var tooltip = new ToolTip
            {
                Content = path,
                Style = (Style)shell.FindResource(typeof(ToolTip)),
                PlacementTarget = owner
            };
            owner.ToolTip = tooltip;
            root.Children.Add(owner);
            shell.Content = root;

            var window = new Window
            {
                Content = shell,
                Width = 260,
                Height = 150,
                ShowInTaskbar = false,
                ShowActivated = true,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };

            try
            {
                window.Show();
                window.UpdateLayout();
                owner.Focus();
                Keyboard.Focus(owner);
                Assert.Same(owner, Keyboard.FocusedElement);
                Assert.Equal(350, ToolTipService.GetInitialShowDelay(shell));
                Assert.Equal(0, ToolTipService.GetBetweenShowDelay(shell));
                Assert.Equal(350, ToolTipService.GetInitialShowDelay(owner));
                Assert.Equal(0, ToolTipService.GetBetweenShowDelay(owner));

                tooltip.IsOpen = true;
                DrainDispatcher();
                window.UpdateLayout();

                Assert.True(tooltip.IsOpen);
                Assert.False(tooltip.Focusable);
                Assert.Equal(PlacementMode.Mouse, tooltip.Placement);
                Assert.Equal(420, tooltip.MaxWidth);
                Assert.Same(owner, Keyboard.FocusedElement);

                var text = FindVisualChildren<TextBlock>(tooltip).Single(block => block.Text == path);
                Assert.Equal(TextWrapping.Wrap, text.TextWrapping);
                Assert.Equal(TextTrimming.None, text.TextTrimming);
                Assert.True(text.ActualHeight > tooltip.FontSize,
                    $"长路径应在 Tooltip 内换行，实际高度为 {text.ActualHeight}，字体大小为 {tooltip.FontSize}。 ");
                Assert.InRange(tooltip.ActualWidth, 1, tooltip.MaxWidth + 1);
                Assert.InRange(text.ActualWidth, 1, tooltip.MaxWidth + 1);

                var focusedBeforeEscape = Keyboard.FocusedElement;
                var escape = new KeyEventArgs(
                    Keyboard.PrimaryDevice,
                    PresentationSource.FromVisual(window)!,
                    0,
                    Key.Escape)
                {
                    RoutedEvent = Keyboard.PreviewKeyDownEvent
                };
                owner.RaiseEvent(escape);
                DrainDispatcher();

                Assert.True(escape.Handled);
                Assert.False(tooltip.IsOpen);
                Assert.Same(focusedBeforeEscape, Keyboard.FocusedElement);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static T[] FindVisualChildren<T>(DependencyObject root) where T : DependencyObject
    {
        var results = new System.Collections.Generic.List<T>();
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match) results.Add(match);
            results.AddRange(FindVisualChildren<T>(child));
        }

        return results.ToArray();
    }

    private static void DrainDispatcher()
    {
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
    }

    private static void PumpDispatcherFor(TimeSpan duration)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer(DispatcherPriority.Background, Dispatcher.CurrentDispatcher)
        {
            Interval = duration
        };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            frame.Continue = false;
        };
        timer.Start();
        Dispatcher.PushFrame(frame);
    }

    private static void MoveCursorTo(UIElement target)
    {
        MoveCursorTo(target, new Point(target.RenderSize.Width / 2, target.RenderSize.Height / 2));
    }

    private static void MoveCursorTo(UIElement target, Point point)
    {
        var screenPoint = target.PointToScreen(point);
        Assert.True(SetCursorPos((int)Math.Round(screenPoint.X), (int)Math.Round(screenPoint.Y)));
        DrainDispatcher();
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetCursorPos(ref NativePoint point);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetCursorPos(int x, int y);

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure != null)
            throw new Xunit.Sdk.XunitException(failure.ToString());
    }
}
