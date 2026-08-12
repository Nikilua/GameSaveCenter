using System.IO;

namespace GameSaveCenter.Contracts
{
    /// <summary>Supported platform/source categories.</summary>
    public enum GamePlatformKind
    {
        Unknown = 0,
        Steam = 1,
        Xbox = 2,
        Epic = 3,
        Ubisoft = 4,
        Ea = 5,
        Gog = 6,
        Other = 99
    }

    /// <summary>How a game session was discovered.</summary>
    public enum SessionSourceKind
    {
        Playnite = 0,
        ProcessDetection = 1,
        Manual = 2
    }

    /// <summary>High-level task state used by UI and persistence.</summary>
    public enum TaskState
    {
        Queued = 0,
        Running = 1,
        Succeeded = 2,
        Failed = 3,
        Cancelled = 4,
        WaitingForUser = 5
    }

    /// <summary>Severity of a validation or operational finding.</summary>
    public enum FindingSeverity
    {
        Info = 0,
        Warning = 1,
        Error = 2,
        Critical = 3
    }

    /// <summary>Where a media file was captured.</summary>
    public enum MediaSourceKind
    {
        Unknown = 0,
        Steam = 1,
        XboxGameBar = 2,
        WindowsScreenshot = 3,
        Epic = 4,
        Ubisoft = 5,
        Ea = 6,
        Gog = 7,
        ReShade = 8,
        Nvidia = 9,
        Amd = 10,
        GameNative = 11,
        Custom = 99
    }

    /// <summary>Media classification used by the archive.</summary>
    public enum MediaKind
    {
        Screenshot = 0,
        VideoClip = 1,
        Unknown = 99
    }


    /// <summary>Storage format requested from Ludusavi for new backups.</summary>
    public enum BackupStorageFormat
    {
        Simple = 0,
        Zip = 1
    }

    /// <summary>Result of checking whether one indexed backup can be safely read and staged.</summary>
    public enum RestoreReadinessStatus
    {
        Unknown = 0,
        Checking = 1,
        Ready = 2,
        Warning = 3,
        Corrupted = 4,
        Unsupported = 5,
        Failed = 6
    }

    /// <summary>Small user-facing health vocabulary for one game's backup state.</summary>
    public enum GameHealthState
    {
        Healthy = 0,
        Attention = 1,
        Risk = 2,
        Unknown = 3
    }

    /// <summary>Restore workflow state.</summary>
    public enum RestoreState
    {
        Requested = 0,
        GameClosedVerified = 1,
        PreRestoreBackupCreated = 2,
        CloudJobsPaused = 3,
        RestoreExecuted = 4,
        PostRestoreValidated = 5,
        Completed = 6,
        Failed = 7,
        RollbackAttempted = 8,
        RolledBack = 9,
        ManualInterventionRequired = 10
    }

    public enum GameToolType
    {
        Trainer = 0,
        CheatTable = 1,
        CustomExecutable = 2
    }

    public enum GameToolSourceType
    {
        Manual = 0,
        Fling = 1,
        Other = 99
    }

    public enum GameToolLaunchTiming
    {
        AfterGameStarted = 0,
        Delayed = 1
    }

    /// <summary>How a custom launch item should be started on Windows.</summary>
    public enum GameToolLaunchKind
    {
        Executable = 0,
        Shortcut = 1,
        BatchScript = 2,
        PowerShellScript = 3,
        ShellDocument = 4
    }

    /// <summary>Launch-kind helpers shared by the Worker and the UI.</summary>
    public static class GameToolLaunchKinds
    {
        public static GameToolLaunchKind FromPath(string? path)
        {
            var extension = Path.GetExtension(path ?? string.Empty);
            if (string.Equals(extension, ".exe", StringComparison.OrdinalIgnoreCase))
                return GameToolLaunchKind.Executable;
            if (string.Equals(extension, ".lnk", StringComparison.OrdinalIgnoreCase))
                return GameToolLaunchKind.Shortcut;
            if (string.Equals(extension, ".bat", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".cmd", StringComparison.OrdinalIgnoreCase))
                return GameToolLaunchKind.BatchScript;
            if (string.Equals(extension, ".ps1", StringComparison.OrdinalIgnoreCase))
                return GameToolLaunchKind.PowerShellScript;
            return GameToolLaunchKind.ShellDocument;
        }

        /// <summary>
        /// Only directly started EXEs (including resolved shortcut targets) can be tracked
        /// by PID + start time. Scripts, shell documents and unresolved shortcuts cannot be
        /// safely closed when the game exits.
        /// </summary>
        public static bool CanTrackProcess(string? path) => FromPath(path) == GameToolLaunchKind.Executable;

        public static string DisplayName(string? path) => FromPath(path) switch
        {
            GameToolLaunchKind.Executable => "外部 EXE",
            GameToolLaunchKind.Shortcut => "快捷方式",
            GameToolLaunchKind.BatchScript => "批处理",
            GameToolLaunchKind.PowerShellScript => "PowerShell",
            _ => "系统默认程序"
        };
    }
}
