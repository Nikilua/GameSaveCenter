$ErrorActionPreference = 'Stop'

$signature = @'
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static class GameSaveCenterConsoleWindow
{
    private const uint SnapshotProcesses = 0x00000002;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct ProcessEntry
    {
        public uint Size;
        public uint Usage;
        public uint ProcessId;
        public IntPtr DefaultHeapId;
        public uint ModuleId;
        public uint ThreadCount;
        public uint ParentProcessId;
        public int BasePriority;
        public uint Flags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string ExecutableName;
    }

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentProcessId();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateToolhelp32Snapshot(uint flags, uint processId);

    [DllImport("kernel32.dll", EntryPoint = "Process32FirstW", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
    private static extern bool Process32First(IntPtr snapshot, ref ProcessEntry entry);

    [DllImport("kernel32.dll", EntryPoint = "Process32NextW", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
    private static extern bool Process32Next(IntPtr snapshot, ref ProcessEntry entry);

    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr handle);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr window, int command);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr window);

    public static bool HideCurrentConsole()
    {
        var window = GetConsoleWindow();
        if (window == IntPtr.Zero || !IsWindowVisible(window))
        {
            return true;
        }

        if (!WasLaunchedFromExplorer())
        {
            return true;
        }

        ShowWindow(window, 0);
        return !IsWindowVisible(window);
    }

    private static bool WasLaunchedFromExplorer()
    {
        var entries = new Dictionary<uint, ProcessEntry>();
        var snapshot = CreateToolhelp32Snapshot(SnapshotProcesses, 0);
        if (snapshot == new IntPtr(-1))
        {
            return false;
        }

        try
        {
            var entry = new ProcessEntry { Size = (uint)Marshal.SizeOf(typeof(ProcessEntry)) };
            if (!Process32First(snapshot, ref entry))
            {
                return false;
            }

            do
            {
                entries[entry.ProcessId] = entry;
                entry.Size = (uint)Marshal.SizeOf(typeof(ProcessEntry));
            }
            while (Process32Next(snapshot, ref entry));
        }
        finally
        {
            CloseHandle(snapshot);
        }

        ProcessEntry current;
        ProcessEntry parent;
        ProcessEntry launcher;
        return entries.TryGetValue(GetCurrentProcessId(), out current)
            && entries.TryGetValue(current.ParentProcessId, out parent)
            && string.Equals(parent.ExecutableName, "cmd.exe", StringComparison.OrdinalIgnoreCase)
            && entries.TryGetValue(parent.ParentProcessId, out launcher)
            && string.Equals(launcher.ExecutableName, "explorer.exe", StringComparison.OrdinalIgnoreCase);
    }
}
'@

Add-Type -TypeDefinition $signature -ErrorAction Stop
if (-not [GameSaveCenterConsoleWindow]::HideCurrentConsole())
{
    exit 1
}
