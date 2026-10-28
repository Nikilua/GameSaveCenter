using System;
using System.Diagnostics;
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
    public void ScopedTextEditorHelpUsesOwnerPlacementWithoutOverridingExplicitPlacement()
    {
        RunSta(() =>
        {
            var root = new StackPanel();
            var editor = new TextBox();
            var overriddenEditor = new TextBox();
            var button = new Button();
            ToolTipService.SetPlacement(overriddenEditor, PlacementMode.Top);
            root.Children.Add(editor);
            root.Children.Add(overriddenEditor);
            root.Children.Add(button);
            GscToolTipBehavior.SetEnableEscapeClose(root, true);

            Assert.Equal(PlacementMode.Bottom, ToolTipService.GetPlacement(editor));
            Assert.Equal(PlacementMode.Top, ToolTipService.GetPlacement(overriddenEditor));
            Assert.Equal(PlacementMode.Mouse, ToolTipService.GetPlacement(button));
        });
    }

    [Fact]
    public void RefreshIgnoresPresentationRootsOwnedByAnotherDispatcher()
    {
        using var sourceReady = new ManualResetEventSlim();
        Dispatcher? foreignDispatcher = null;
        Exception? foreignSourceFailure = null;
        var sourceThread = new Thread(() =>
        {
            try
            {
                using (var source = new HwndSource(new HwndSourceParameters("GSC synthetic tooltip dispatcher")
                {
                    Width = 80,
                    Height = 60,
                    WindowStyle = 0,
                    ExtendedWindowStyle = 0x80
                }))
                {
                    source.RootVisual = new Grid { Width = 80, Height = 60 };
                    foreignDispatcher = Dispatcher.CurrentDispatcher;
                    sourceReady.Set();
                    Dispatcher.Run();
                }
            }
            catch (Exception exception)
            {
                foreignSourceFailure = exception;
                sourceReady.Set();
            }
        })
        {
            IsBackground = true
        };
        sourceThread.SetApartmentState(ApartmentState.STA);
        sourceThread.Start();

        var sourceThreadStopped = false;
        try
        {
            Assert.True(sourceReady.Wait(TimeSpan.FromSeconds(5)), "synthetic foreign WPF presentation source did not become ready");
            Assert.Null(foreignSourceFailure);
            Assert.NotNull(foreignDispatcher);

            RunSta(() =>
            {
                Assert.NotSame(Dispatcher.CurrentDispatcher, foreignDispatcher);
                GscToolTipBehavior.RefreshOpenTransientSurfaces(new Grid());
            });
        }
        finally
        {
            foreignDispatcher?.BeginInvokeShutdown(DispatcherPriority.Send);
            sourceThreadStopped = sourceThread.Join(TimeSpan.FromSeconds(5));
        }

        Assert.True(sourceThreadStopped, "synthetic foreign WPF dispatcher did not stop after the tooltip refresh check");
        Assert.Null(foreignSourceFailure);
    }

    [InteractivePointerFact]
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
                Opacity = 0.01,
                Topmost = true
            };

            var originalCursor = new NativePoint();
            var cursorCaptured = GetCursorPos(ref originalCursor);
            Assert.True(cursorCaptured, "应能保存隔离 Tooltip 行为测试启动前的系统指针位置。");
            try
            {
                window.Show();
                window.Activate();
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

    [Fact]
    public void ProductionEditorTooltipFlipsAboveOwnerNearWorkAreaBottom()
    {
        RunSta(() =>
        {
            var shell = new AcrylicProductionShellView();
            var owner = new TextBox
            {
                Width = 180,
                Height = 32,
                Text = "编辑器帮助",
                IsReadOnly = true,
                VerticalAlignment = VerticalAlignment.Bottom,
                Style = (Style)shell.FindResource("GscWpfUiPathDetailTextBox")
            };
            var root = new Grid();
            root.Children.Add(owner);
            GscToolTipBehavior.SetEnableEscapeClose(root, true);

            var tooltip = new ToolTip
            {
                Content = "边缘提示内容用于验证可用空间不足时回退到控件上方，并保留完整可读的说明文字。",
                Style = (Style)shell.FindResource(typeof(ToolTip)),
                PlacementTarget = owner
            };
            owner.ToolTip = tooltip;

            var workArea = SystemParameters.WorkArea;
            var window = new Window
            {
                Content = shell,
                Width = 340,
                Height = 150,
                Left = workArea.Left + 20,
                Top = workArea.Bottom - 150,
                ShowInTaskbar = false,
                ShowActivated = false,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01,
                Topmost = true
            };
            shell.Content = root;

            try
            {
                window.Show();
                window.UpdateLayout();
                root.UpdateLayout();

                Assert.Equal(PlacementMode.Bottom, ToolTipService.GetPlacement(owner));
                tooltip.Placement = ToolTipService.GetPlacement(owner);
                tooltip.IsOpen = true;
                DrainDispatcher();
                window.UpdateLayout();

                Assert.True(tooltip.IsOpen);
                Assert.True(tooltip.ActualHeight > 32, "The synthetic tooltip must be taller than the small area left below its editor.");
                var ownerBottom = owner.PointToScreen(new Point(owner.ActualWidth / 2, owner.ActualHeight));
                var tooltipTop = tooltip.PointToScreen(new Point(tooltip.ActualWidth / 2, 0));
                Assert.True(tooltipTop.Y < ownerBottom.Y,
                    $"A bottom-anchored editor tooltip should flip above its owner near the work-area edge; tooltip top={tooltipTop.Y}, owner bottom={ownerBottom.Y}.");
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
        // The pointer driver must leave a short dwell on schedule even when layout
        // and automation work is queued ahead of Background priority.
        var timer = new DispatcherTimer(DispatcherPriority.Send, Dispatcher.CurrentDispatcher)
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
        var expectedX = (int)Math.Round(screenPoint.X);
        var expectedY = (int)Math.Round(screenPoint.Y);
        Assert.True(SetCursorPos(expectedX, expectedY), "应能将隔离 Tooltip 行为测试的系统指针移至目标控件。 ");

        var timeout = Stopwatch.StartNew();
        while (timeout.Elapsed < TimeSpan.FromSeconds(2))
        {
            PumpDispatcherFor(TimeSpan.FromMilliseconds(10));
            if (!target.IsMouseOver || !ReferenceEquals(Mouse.DirectlyOver, target.InputHitTest(point)))
                continue;

            var current = new NativePoint();
            if (GetCursorPos(ref current) &&
                Math.Abs(current.X - expectedX) <= 2 &&
                Math.Abs(current.Y - expectedY) <= 2)
            {
                return;
            }
        }

        var actual = new NativePoint();
        var cursorAvailable = GetCursorPos(ref actual);
        var directTarget = Mouse.DirectlyOver;
        var source = PresentationSource.FromVisual(target) as HwndSource;
        var pointWindow = cursorAvailable ? WindowFromPoint(new NativePoint { X = actual.X, Y = actual.Y }) : IntPtr.Zero;
        var foregroundWindow = GetForegroundWindow();
        var windowRect = source != null && GetWindowRect(source.Handle, out var bounds)
            ? $"{bounds.Left},{bounds.Top}-{bounds.Right},{bounds.Bottom}"
            : "<unavailable>";
        throw new Xunit.Sdk.XunitException(
            $"等待系统指针进入 Tooltip 目标控件超时。目标={target.GetType().Name}; IsMouseOver={target.IsMouseOver}; " +
            $"DirectlyOver={directTarget?.GetType().Name ?? "<null>"}; " +
            $"Cursor={(cursorAvailable ? $"{actual.X},{actual.Y}" : "unavailable")}; 期望={expectedX},{expectedY}; " +
            $"WindowHandle={(source?.Handle.ToInt64().ToString("X") ?? "<none>")}; " +
            $"WindowFromPoint={pointWindow.ToInt64():X}; ForegroundWindow={foregroundWindow.ToInt64():X}; Bounds={windowRect}; " +
            $"Dpi={(source == null ? "<none>" : $"{source.CompositionTarget.TransformToDevice.M11:0.###}x{source.CompositionTarget.TransformToDevice.M22:0.###}")}");
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

    [DllImport("user32.dll")]
    private static extern IntPtr WindowFromPoint(NativePoint point);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetWindowRect(IntPtr windowHandle, out NativeRect rectangle);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

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
