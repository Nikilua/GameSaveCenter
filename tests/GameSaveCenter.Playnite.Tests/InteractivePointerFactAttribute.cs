using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class InteractivePointerFactAttribute : FactAttribute
{
    public InteractivePointerFactAttribute()
    {
        if (!CanReadAndSetPointer())
        {
            Skip = "当前 Windows 会话不允许访问系统鼠标指针；该 Tooltip 交互用例只在可交互桌面运行。";
            return;
        }

        if (!CanRoutePointerToWpfWindow())
        {
            Skip = "当前 Windows 桌面允许系统指针 API，但不向测试 WPF 窗口分发鼠标事件；Tooltip 鼠标交互用例只在可接收桌面输入时运行。";
        }
    }

    private static bool CanRoutePointerToWpfWindow()
    {
        var canReceive = false;
        var thread = new Thread(() =>
        {
            var original = new NativePoint();
            var cursorCaptured = false;
            Window? window = null;
            try
            {
                cursorCaptured = GetCursorPos(ref original);
                var button = new Button { Width = 84, Height = 42, Content = "Pointer probe" };
                window = new Window
                {
                    Content = button,
                    Width = 120,
                    Height = 80,
                    ShowInTaskbar = false,
                    ShowActivated = true,
                    WindowStyle = WindowStyle.None,
                    Opacity = 0.01,
                    Topmost = true
                };
                window.Show();
                window.Activate();
                window.UpdateLayout();

                var screen = button.PointToScreen(new Point(button.ActualWidth / 2, button.ActualHeight / 2));
                var expectedX = (int)Math.Round(screen.X);
                var expectedY = (int)Math.Round(screen.Y);
                if (!SetCursorPos(expectedX, expectedY))
                    return;

                PumpDispatcherFor(TimeSpan.FromMilliseconds(120));
                var current = new NativePoint();
                canReceive = button.IsMouseOver &&
                    GetCursorPos(ref current) &&
                    Math.Abs(current.X - expectedX) <= 2 &&
                    Math.Abs(current.Y - expectedY) <= 2;
            }
            catch (Exception)
            {
                canReceive = false;
            }
            finally
            {
                try { window?.Close(); }
                catch { }
                if (cursorCaptured)
                {
                    try { SetCursorPos(original.X, original.Y); }
                    catch { }
                }
            }
        })
        {
            IsBackground = true
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return thread.Join(TimeSpan.FromSeconds(3)) && canReceive;
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

    private static bool CanReadAndSetPointer()
    {
        var point = new NativePoint();
        try
        {
            return GetCursorPos(ref point) && SetCursorPos(point.X, point.Y);
        }
        catch (DllNotFoundException)
        {
            return false;
        }
        catch (EntryPointNotFoundException)
        {
            return false;
        }
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
}
