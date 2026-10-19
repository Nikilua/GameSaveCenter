using System;
using System.Runtime.InteropServices;
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
