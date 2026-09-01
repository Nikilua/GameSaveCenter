using System;
using System.Collections.Generic;

namespace GameSaveCenter.Contracts
{
    /// <summary>
    /// Playnite-neutral game descriptor sent to the Worker. It contains only the
    /// fields required for matching, process detection and display.
    /// </summary>
    public sealed class GameDescriptorDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public GamePlatformKind Platform { get; set; }
        public string PlatformGameId { get; set; } = string.Empty;
        public string PluginId { get; set; } = string.Empty;
        public string InstallDirectory { get; set; } = string.Empty;
        public bool IsInstalled { get; set; }
        public DateTime? LastPlayedUtc { get; set; }
        public List<GameActionDto> Actions { get; set; } = new List<GameActionDto>();
        public List<string> KnownProcessNames { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();
    }

    /// <summary>Serializable launch action used to learn original and MOD launch paths.</summary>
    public sealed class GameActionDto
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Arguments { get; set; } = string.Empty;
        public string WorkingDirectory { get; set; } = string.Empty;
        public bool IsPlayAction { get; set; }
        public bool IsModLoader { get; set; }
    }

    /// <summary>Event indicating that a game session was started or discovered.</summary>
    public sealed class GameSessionEventDto
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString("N");
        public string PlayniteId { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public SessionSourceKind Source { get; set; }
        public int? ProcessId { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public string LaunchProfile { get; set; } = string.Empty;
        public DateTime StartedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? StoppedUtc { get; set; }
        public long ElapsedSeconds { get; set; }
    }

    public sealed class GameSessionStopResultDto
    {
        public bool Stopped { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public int ExpectedTaskCount { get; set; }
        public ProtectionPromptDto? ProtectionPrompt { get; set; }
    }

    public sealed class GameSessionSummaryDto
    {
        public string GameName { get; set; } = string.Empty;
        public bool IsWarning { get; set; }
        public bool IsFailure { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public sealed class ProtectionPromptDto
    {
        public bool ShouldPrompt { get; set; }
        public bool SaveRecognized { get; set; }
        public string PlayniteId { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public ProtectionPromptState State { get; set; } = ProtectionPromptState.NeverShown;
        public string Message { get; set; } = string.Empty;
    }

    public sealed class ProtectionPromptDecisionDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public ProtectionPromptChoice Choice { get; set; }
    }

    /// <summary>Summarized game state displayed by the dashboard.</summary>
    public sealed class GameStatusDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public GamePlatformKind Platform { get; set; }
        /// <summary>Whether the game is currently installed in Playnite.</summary>
        public bool IsInstalled { get; set; }
        public DateTime? LastPlayedUtc { get; set; }
        public bool IsRunning { get; set; }
        public bool LudusaviMatched { get; set; }
        public string LudusaviName { get; set; } = string.Empty;
        public DateTime? LastBackupUtc { get; set; }
        public int BackupVersionCount { get; set; }
        public DateTime? LastMediaSyncUtc { get; set; }
        public int MediaCount { get; set; }
        public string CloudState { get; set; } = "Disabled";
        public string HealthState { get; set; } = "Unknown";
        public string HealthSummary { get; set; } = string.Empty;
        public List<string> HealthReasons { get; set; } = new List<string>();
        public RestoreReadinessStatus? LatestRestoreReadinessStatus { get; set; }
        public BackupPolicyDto Policy { get; set; } = new BackupPolicyDto();
        public string PlatformDisplay => Platform switch
        {
            GamePlatformKind.Steam => "Steam",
            GamePlatformKind.Xbox => "Xbox",
            GamePlatformKind.Epic => "Epic",
            GamePlatformKind.Ubisoft => "Ubisoft",
            GamePlatformKind.Ea => "EA",
            GamePlatformKind.Gog => "GOG",
            GamePlatformKind.Other => "其他",
            _ => "未知"
        };
        public string InstallStateDisplay => IsInstalled ? "已安装" : "未安装";
        public string MatchStateDisplay => LudusaviMatched ? "已匹配" : "未匹配";
        public string HealthStateDisplay => HealthState switch
        {
            "Healthy" => "健康",
            "Attention" => "注意",
            "Risk" => "风险",
            "Unknown" => "未知",
            "Ready" => "已就绪",
            "Unmatched" => "未匹配",
            "Running" => "运行中",
            "Warning" => "需关注",
            "LudusaviUnavailable" => "Ludusavi 未配置",
            _ => HealthState
        };
        public string HealthReasonDisplay => HealthReasons != null && HealthReasons.Count > 0
            ? string.Join("；", HealthReasons)
            : HealthSummary;
        public string CloudStateDisplay => CloudState switch
        {
            "Uploaded" => "已上传",
            "Failed" => "上传失败",
            "Disabled" => "未启用",
            "Pending" => "待上传",
            "RetryScheduled" => "等待重试",
            _ => string.IsNullOrWhiteSpace(CloudState) ? "未启用" : CloudState
        };
        public DateTime? LastBackupLocal => LastBackupUtc?.ToLocalTime();
        public DateTime? LastMediaSyncLocal => LastMediaSyncUtc?.ToLocalTime();
    }
}
