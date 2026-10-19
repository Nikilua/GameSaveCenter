using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class InteractivePointerFactAttribute : FactAttribute
{
    public InteractivePointerFactAttribute()
    {
        if (IsElevatedProcess())
        {
            Skip = "当前测试进程以管理员权限运行；Tooltip 鼠标交互用例只在普通桌面权限下运行，以免 Windows 前台输入隔离造成误报。";
            return;
        }

        if (!CanReadAndSetPointer())
        {
            Skip = "当前 Windows 会话不允许访问系统鼠标指针；该 Tooltip 交互用例只在可交互桌面运行。";
        }
    }

    private static bool IsElevatedProcess()
    {
        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch (PlatformNotSupportedException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (System.Security.SecurityException)
        {
            return false;
        }
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
