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
    private static extern uint GetConsoleProcessList([Out] uint[] processList, uint processCount);

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

        if (!ShouldHideForCurrentCaller())
        {
            return true;
        }

        ShowWindow(window, 0);
        return !IsWindowVisible(window);
    }

    public static bool ShouldHideForLaunchContext(string commandParent, string launcher, bool launcherSharesCurrentConsole)
    {
        if (!string.Equals(commandParent, "cmd.exe", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.Equals(launcher, "explorer.exe", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var isPowerShell = string.Equals(launcher, "powershell.exe", StringComparison.OrdinalIgnoreCase)
            || string.Equals(launcher, "pwsh.exe", StringComparison.OrdinalIgnoreCase);
        return isPowerShell && !launcherSharesCurrentConsole;
    }

    private static bool ShouldHideForCurrentCaller()
    {
        var entries = ReadProcessEntries();
        if (entries == null)
        {
            return false;
        }

        ProcessEntry current;
        ProcessEntry parent;
        ProcessEntry launcher;
        if (!entries.TryGetValue(GetCurrentProcessId(), out current)
            || !entries.TryGetValue(current.ParentProcessId, out parent)
            || !entries.TryGetValue(parent.ParentProcessId, out launcher))
        {
            return false;
        }

        bool launcherSharesCurrentConsole = false;
        var isPowerShell = string.Equals(launcher.ExecutableName, "powershell.exe", StringComparison.OrdinalIgnoreCase)
            || string.Equals(launcher.ExecutableName, "pwsh.exe", StringComparison.OrdinalIgnoreCase);
        if (isPowerShell && !TryIsProcessAttachedToCurrentConsole(launcher.ProcessId, out launcherSharesCurrentConsole))
        {
            // Preserve a caller's console when ownership cannot be determined.
            return false;
        }

        return ShouldHideForLaunchContext(parent.ExecutableName, launcher.ExecutableName, launcherSharesCurrentConsole);
    }

    private static bool TryIsProcessAttachedToCurrentConsole(uint processId, out bool attached)
    {
        attached = false;
        if (GetConsoleWindow() == IntPtr.Zero)
        {
            return false;
        }

        var processIds = new uint[32];
        var count = GetConsoleProcessList(processIds, (uint)processIds.Length);
        if (count == 0)
        {
            return false;
        }

        if (count > processIds.Length)
        {
            processIds = new uint[(int)count];
            count = GetConsoleProcessList(processIds, (uint)processIds.Length);
            if (count == 0 || count > processIds.Length)
            {
                return false;
            }
        }

        for (var index = 0; index < count; index++)
        {
            if (processIds[index] == processId)
            {
                attached = true;
                break;
            }
        }

        return true;
    }

    private static Dictionary<uint, ProcessEntry> ReadProcessEntries()
    {
        var entries = new Dictionary<uint, ProcessEntry>();
        var snapshot = CreateToolhelp32Snapshot(SnapshotProcesses, 0);
        if (snapshot == new IntPtr(-1))
        {
            return null;
        }

        try
        {
            var entry = new ProcessEntry { Size = (uint)Marshal.SizeOf(typeof(ProcessEntry)) };
            if (!Process32First(snapshot, ref entry))
            {
                return null;
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

        return entries;
    }
}
'@

Add-Type -TypeDefinition $signature -ErrorAction Stop
if (-not [GameSaveCenterConsoleWindow]::HideCurrentConsole())
{
    exit 1
}
