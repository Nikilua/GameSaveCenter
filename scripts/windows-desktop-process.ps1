# Runs short-lived Windows processes on a private, non-input desktop.
# This keeps WPF test windows off the user's visible desktop while preserving
# the real WinSta0 desktop implementation used by WPF animation tests.
$desktopProcessSource = @'
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

public sealed class GscHiddenDesktopProcessResult
{
    public uint ProcessId { get; set; }
    public int ExitCode { get; set; }
    public bool Exited { get; set; }
    public bool TimedOut { get; set; }
    public string ProcessStartedUtc { get; set; }
    public string WindowStation { get; set; }
    public string Desktop { get; set; }
    public string InputDesktopBefore { get; set; }
    public string InputDesktopAfter { get; set; }
}

public static class GscHiddenDesktopProcess
{
    private const uint CreateNoWindow = 0x08000000;
    private const uint WaitObject0 = 0;
    private const uint WaitTimeout = 0x00000102;
    private const uint DesktopAccess = 0x01FF;
    private const int UserObjectName = 2;
    private const int StartfUseShowWindow = 0x00000001;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct StartupInfo
    {
        public int cb;
        public string reserved;
        public string desktop;
        public string title;
        public int x;
        public int y;
        public int xSize;
        public int ySize;
        public int xChars;
        public int yChars;
        public int fill;
        public int flags;
        public short show;
        public short reservedSize;
        public IntPtr reservedPtr;
        public IntPtr input;
        public IntPtr output;
        public IntPtr error;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessInformation
    {
        public IntPtr process;
        public IntPtr thread;
        public uint processId;
        public uint threadId;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetProcessWindowStation();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetThreadDesktop(uint threadId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr OpenInputDesktop(uint flags, bool inherit, uint access);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateDesktop(string name, IntPtr device, IntPtr devMode, uint flags, uint access, IntPtr securityAttributes);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool CloseDesktop(IntPtr desktop);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool GetUserObjectInformation(IntPtr handle, int index, StringBuilder name, int length, out int needed);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [DllImport("kernel32.dll", EntryPoint = "CreateProcessW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CreateProcess(string application, StringBuilder commandLine, IntPtr processAttributes, IntPtr threadAttributes, bool inheritHandles, uint creationFlags, IntPtr environment, string currentDirectory, ref StartupInfo startupInfo, out ProcessInformation processInformation);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetExitCodeProcess(IntPtr process, out uint exitCode);

    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr handle);

    private static string ReadObjectName(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        var name = new StringBuilder(256);
        int needed;
        if (!GetUserObjectInformation(handle, UserObjectName, name, 512, out needed))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        return name.ToString();
    }

    private static string ReadInputDesktop()
    {
        var desktop = OpenInputDesktop(0, false, 1);
        if (desktop == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not inspect the input desktop.");
        }

        try
        {
            return ReadObjectName(desktop);
        }
        finally
        {
            CloseDesktop(desktop);
        }
    }

    public static string CurrentWindowStation()
    {
        return ReadObjectName(GetProcessWindowStation());
    }

    public static string CurrentDesktop()
    {
        return ReadObjectName(GetThreadDesktop(GetCurrentThreadId()));
    }

    public static string InputDesktop()
    {
        return ReadInputDesktop();
    }

    public static GscHiddenDesktopProcessResult Run(string desktopName, string application, string commandLine, string currentDirectory, uint timeoutMilliseconds)
    {
        var station = CurrentWindowStation();
        if (!String.Equals(station, "WinSta0", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("WPF test isolation requires the interactive WinSta0 window station; current station is " + station + ".");
        }

        var inputBefore = InputDesktop();
        if (String.Equals(inputBefore, desktopName, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Refusing to run WPF tests on the input desktop.");
        }

        var desktop = CreateDesktop(desktopName, IntPtr.Zero, IntPtr.Zero, 0, DesktopAccess, IntPtr.Zero);
        if (desktop == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not create the WPF test desktop.");
        }

        ProcessInformation processInformation = new ProcessInformation();
        var processCreated = false;
        var processStartedUtc = DateTime.UtcNow.ToString("o");
        try
        {
            var startupInfo = new StartupInfo();
            startupInfo.cb = Marshal.SizeOf(typeof(StartupInfo));
            startupInfo.desktop = "WinSta0\\" + desktopName;
            startupInfo.flags = StartfUseShowWindow;
            startupInfo.show = 0;

            if (!CreateProcess(application, new StringBuilder(commandLine), IntPtr.Zero, IntPtr.Zero, false, CreateNoWindow, IntPtr.Zero, currentDirectory, ref startupInfo, out processInformation))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not start the WPF test helper on its private desktop.");
            }

            processCreated = true;
            var waitResult = WaitForSingleObject(processInformation.process, timeoutMilliseconds);
            if (waitResult != WaitObject0 && waitResult != WaitTimeout)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not wait for the WPF test helper.");
            }

            uint nativeExitCode = 124;
            var exited = waitResult == WaitObject0;
            if (exited && !GetExitCodeProcess(processInformation.process, out nativeExitCode))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not read the WPF test helper exit code.");
            }

            var inputAfter = InputDesktop();
            return new GscHiddenDesktopProcessResult
            {
                ProcessId = processInformation.processId,
                ExitCode = unchecked((int)nativeExitCode),
                Exited = exited,
                TimedOut = !exited,
                ProcessStartedUtc = processStartedUtc,
                WindowStation = station,
                Desktop = desktopName,
                InputDesktopBefore = inputBefore,
                InputDesktopAfter = inputAfter
            };
        }
        finally
        {
            if (processCreated)
            {
                CloseHandle(processInformation.thread);
                CloseHandle(processInformation.process);
            }

            CloseDesktop(desktop);
        }
    }
}
'@

if (-not ('GscHiddenDesktopProcess' -as [type])) {
    Add-Type -TypeDefinition $desktopProcessSource -ErrorAction Stop
}

function ConvertTo-GscWindowsArgument {
    param([Parameter(Mandatory = $true)][string]$Value)

    return '"' + [regex]::Replace([regex]::Replace($Value, '(\\*)"', '$1$1\"'), '(\\+)$', '$1$1') + '"'
}

function Invoke-GscHiddenDesktopProcess {
    param(
        [Parameter(Mandatory = $true)][string]$Application,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$WorkingDirectory,
        [int]$TimeoutSeconds = 300,
        [string]$DesktopName = ''
    )

    $desktop = if ([string]::IsNullOrWhiteSpace($DesktopName)) { 'GscWpfTest_' + [Guid]::NewGuid().ToString('N') } else { $DesktopName }
    $quotedArguments = @((ConvertTo-GscWindowsArgument $Application))
    $quotedArguments += @($Arguments | ForEach-Object { ConvertTo-GscWindowsArgument ([string]$_) })
    $commandLine = $quotedArguments -join ' '
    [GscHiddenDesktopProcess]::Run($desktop, $Application, $commandLine, $WorkingDirectory, [uint32]($TimeoutSeconds * 1000))
}
