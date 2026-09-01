using System;
using System.Collections.Generic;
using System.IO;

namespace GameSaveCenter.Contracts
{
    /// <summary>Dashboard snapshot returned in a single request.</summary>
    public sealed class DashboardSnapshotDto
    {
        public DateTime GeneratedUtc { get; set; } = DateTime.UtcNow;
        public DateTime GeneratedLocal => GeneratedUtc.ToLocalTime();
        public bool WorkerHealthy { get; set; }
        public bool SafeModeEnabled { get; set; }
        public string WorkerVersion { get; set; } = string.Empty;
        public bool LudusaviAvailable { get; set; }
        public bool RcloneAvailable { get; set; }
        public string LudusaviVersion { get; set; } = string.Empty;
        public string LudusaviExecutable { get; set; } = string.Empty;
        public string LudusaviBackupDirectory { get; set; } = string.Empty;
        public BackupStorageFormat BackupFormat { get; set; } = BackupStorageFormat.Zip;
        public int ManagedGames { get; set; }
        public int MatchedGames { get; set; }
        public int RunningGames { get; set; }
        public int WarningGames { get; set; }
        public int HealthyGames { get; set; }
        public int AttentionGames { get; set; }
        public int RiskGames { get; set; }
        public int UnknownGames { get; set; }
        public int PendingCloudTasks { get; set; }
        public int UnassignedMediaCount { get; set; }
        public List<GameStatusDto> Games { get; set; } = new List<GameStatusDto>();
        public List<TaskStatusDto> RecentTasks { get; set; } = new List<TaskStatusDto>();
        public List<ValidationFindingDto> Findings { get; set; } = new List<ValidationFindingDto>();
        public List<AuditLogEntryDto> RecentAudit { get; set; } = new List<AuditLogEntryDto>();
        public List<ActivityEntryDto> RecentActivities { get; set; } = new List<ActivityEntryDto>();
    }

    /// <summary>One curated business activity shown in the Overview timeline.</summary>
    public sealed class ActivityEntryDto
    {
        public string Kind { get; set; } = "Maintenance";
        public string Result { get; set; } = "Info";
        public string GameName { get; set; } = "全局";
        public string Summary { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; }

        public string KindDisplay => Kind switch
        {
            "Backup" => "备份",
            "Restore" => "恢复",
            "Cloud" => "云端",
            "Media" => "媒体",
            "GameTool" => "游戏工具",
            "Health" => "健康",
            "Conflict" => "冲突",
            "Integrity" => "完整性",
            "RepositoryRepair" => "仓库修复",
            _ => "维护"
        };

        public string ResultDisplay => Result switch
        {
            "Succeeded" => "成功",
            "Failed" => "失败",
            "Warning" => "需关注",
            _ => "信息"
        };

        public string CreatedDisplay => CreatedUtc.ToLocalTime().ToString("MM-dd HH:mm");

        public string Glyph => Kind switch
        {
            "Backup" => "\uE8B7",
            "Restore" => "\uE777",
            "Cloud" => "\uE753",
            "Media" => "\uEB9F",
            "GameTool" => "\uE8F1",
            "Health" => "\uE946",
            "Conflict" => "\uEA39",
            "Integrity" => "\uE9D9",
            "RepositoryRepair" => "\uE74D",
            _ => "\uE713"
        };
    }

    /// <summary>Backup metadata presented in the timeline and restore wizard.</summary>
    public sealed class BackupVersionDto
    {
        public string BackupId { get; set; } = string.Empty;
        public string ParentBackupId { get; set; } = string.Empty;
        public string PlayniteId { get; set; } = string.Empty;
        public string LudusaviName { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; }
        public long TotalBytes { get; set; }
        public int FileCount { get; set; }
        public bool IsLocked { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string SourceDevice { get; set; } = string.Empty;
        public string OperatingSystem { get; set; } = string.Empty;
        public bool IsPreRestore { get; set; }
        public bool IsHealthProtected => RestoreReadiness?.Status == RestoreReadinessStatus.Ready;
        /// <summary>Resolved Ludusavi game backup directory plus this version's file name.</summary>
        public string ArchivePath { get; set; } = string.Empty;
        public RestoreReadinessDto? RestoreReadiness { get; set; }
        public DateTime CreatedLocal => CreatedUtc.ToLocalTime();
        public string SizeDisplay => FormatBytes(TotalBytes);
        public string BackupTypeDisplay => IsPreRestore ? "恢复前快照" : "普通备份";
        public string LockStateDisplay => IsLocked ? "已锁定" : "未锁定";
        public string RestoreReadinessStatusDisplay => RestoreReadiness?.StatusDisplay ?? "未验证";
        public string RestoreReadinessSummaryDisplay => RestoreReadiness?.Summary ?? "尚未验证该版本的可恢复性。";
        public string RestoreReadinessMetricsDisplay => RestoreReadiness == null
            ? string.Empty
            : $"文件 {RestoreReadiness.ActualFileCount}/{RestoreReadiness.ExpectedFileCount} · 大小 {FormatBytes(RestoreReadiness.ActualTotalSize)}/{FormatBytes(RestoreReadiness.ExpectedTotalSize)}";

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024L * 1024) return $"{bytes / 1024d:0.##} KiB";
            if (bytes < 1024L * 1024 * 1024) return $"{bytes / 1024d / 1024d:0.##} MiB";
            return $"{bytes / 1024d / 1024d / 1024d:0.##} GiB";
        }
    }

    /// <summary>Persisted evidence from a non-destructive restore-readiness check.</summary>
    public sealed class RestoreReadinessDto
    {
        public RestoreReadinessStatus Status { get; set; } = RestoreReadinessStatus.Unknown;
        public DateTime? CheckedUtc { get; set; }
        public string BackupVersionId { get; set; } = string.Empty;
        public bool ArchiveReadable { get; set; }
        public bool ExtractSucceeded { get; set; }
        public int ExpectedFileCount { get; set; }
        public int ActualFileCount { get; set; }
        public long ExpectedTotalSize { get; set; }
        public long ActualTotalSize { get; set; }
        public string HashValidation { get; set; } = "NotAvailable";
        public int WarningCount { get; set; }
        public int ErrorCount { get; set; }
        public string Summary { get; set; } = string.Empty;

        public string StatusDisplay => Status switch
        {
            RestoreReadinessStatus.Unknown => "未验证",
            RestoreReadinessStatus.Checking => "检查中",
            RestoreReadinessStatus.Ready => "可恢复",
            RestoreReadinessStatus.Warning => "有警告",
            RestoreReadinessStatus.Corrupted => "疑似损坏",
            RestoreReadinessStatus.Unsupported => "格式不支持",
            RestoreReadinessStatus.Failed => "检查失败",
            _ => Status.ToString()
        };
    }

    /// <summary>Human-readable manifest difference between two backups.</summary>
    public sealed class BackupDiffDto
    {
        public string LeftBackupId { get; set; } = string.Empty;
        public string RightBackupId { get; set; } = string.Empty;
        public List<string> Added { get; set; } = new List<string>();
        public List<string> Removed { get; set; } = new List<string>();
        public List<string> Modified { get; set; } = new List<string>();
        public int UnchangedCount { get; set; }
        public long TotalBytesDelta { get; set; }
        public string ComparisonQuality { get; set; } = "Estimated";
        public string ComparisonQualityDisplay => string.Equals(ComparisonQuality, "Exact", StringComparison.OrdinalIgnoreCase) ? "精确比较" :
            string.Equals(ComparisonQuality, "InvalidManifest", StringComparison.OrdinalIgnoreCase) ? "Manifest 无效" : "估算比较（缺少完整 Hash）";
        public string Summary { get; set; } = string.Empty;
    }

    /// <summary>Retention recommendation. Deletion is never implied by this DTO.</summary>
    public sealed class RetentionPreviewDto
    {
        public List<string> KeepBackupIds { get; set; } = new List<string>();
        public List<string> ProtectedHealthBackupIds { get; set; } = new List<string>();
        public List<string> DeleteCandidateIds { get; set; } = new List<string>();
        public string Summary { get; set; } = string.Empty;
    }

    /// <summary>Persisted audit message exposed to the Playnite log page.</summary>
    public sealed class AuditLogEntryDto
    {
        public string Category { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string DetailJson { get; set; } = "{}";
        public DateTime CreatedUtc { get; set; }
        public DateTime CreatedLocal => CreatedUtc.ToLocalTime();
    }

    /// <summary>Detected save path that still requires a user decision.</summary>
    public sealed class SavePathCandidateDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public double Score { get; set; }
        public List<string> Reasons { get; set; } = new List<string>();
        public string Status { get; set; } = "Pending";
        public string ReasonsDisplay => string.Join("；", Reasons ?? new List<string>());
        public string StatusDisplay => Status == "Accepted" ? "已接受" : Status == "Rejected" ? "已忽略" : "待确认";
    }

    /// <summary>Media item indexed by the Worker.</summary>
    public sealed class MediaItemDto
    {
        public string MediaId { get; set; } = string.Empty;
        public string PlayniteId { get; set; } = string.Empty;
        public MediaKind Kind { get; set; }
        public MediaSourceKind Source { get; set; }
        public string ArchivePath { get; set; } = string.Empty;
        public string OriginalPath { get; set; } = string.Empty;
        public DateTime CapturedUtc { get; set; }
        public long SizeBytes { get; set; }
        public string Sha256 { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string CloudState { get; set; } = "Pending";
        public string ClassificationState { get; set; } = "Assigned";
        public string ClassificationReason { get; set; } = string.Empty;
        public DateTime CapturedLocal => CapturedUtc.ToLocalTime();
        public string FileName => Path.GetFileName(string.IsNullOrWhiteSpace(OriginalPath) ? ArchivePath ?? string.Empty : OriginalPath);
        public string SizeDisplay => FormatBytes(SizeBytes);
        public string KindDisplay => Kind == MediaKind.VideoClip ? "录像" : Kind == MediaKind.Screenshot ? "截图" : "未知媒体";
        public string SourceDisplay => Source switch
        {
            MediaSourceKind.Steam => "Steam",
            MediaSourceKind.XboxGameBar => "Xbox Game Bar",
            MediaSourceKind.WindowsScreenshot => "Windows 截图",
            MediaSourceKind.Epic => "Epic",
            MediaSourceKind.Ubisoft => "Ubisoft",
            MediaSourceKind.Ea => "EA",
            MediaSourceKind.Gog => "GOG",
            MediaSourceKind.ReShade => "ReShade",
            MediaSourceKind.Nvidia => "NVIDIA",
            MediaSourceKind.Amd => "AMD",
            MediaSourceKind.GameNative => "游戏内截图",
            MediaSourceKind.Custom => "自定义来源",
            _ => "其他来源"
        };
        public string CloudStateDisplay => CloudState switch
        {
            "Synced" => "已同步",
            "Uploaded" => "已上传",
            "Failed" => "失败",
            "Pending" => "待上传",
            "RetryScheduled" => "等待重试",
            "NotApplicable" => "不适用",
            _ => string.IsNullOrWhiteSpace(CloudState) ? "未启用" : CloudState
        };
        public string ClassificationStateDisplay => string.Equals(ClassificationState, "Inbox", StringComparison.OrdinalIgnoreCase)
            ? "待归类"
            : string.Equals(ClassificationState, "Ignored", StringComparison.OrdinalIgnoreCase)
                ? "已忽略"
                : "已归类";

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024L * 1024) return $"{bytes / 1024d:0.##} KiB";
            if (bytes < 1024L * 1024 * 1024) return $"{bytes / 1024d / 1024d:0.##} MiB";
            return $"{bytes / 1024d / 1024d / 1024d:0.##} GiB";
        }
    }

    /// <summary>Indexed storage totals for the selected game's assigned media.</summary>
    public sealed class MediaStorageSummaryDto
    {
        public int TotalCount { get; set; }
        public int ScreenshotCount { get; set; }
        public int VideoCount { get; set; }
        public int FavoriteCount { get; set; }
        public long TotalBytes { get; set; }
        public string TotalSizeDisplay => FormatBytes(TotalBytes);

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024L * 1024) return $"{bytes / 1024d:0.##} KiB";
            if (bytes < 1024L * 1024 * 1024) return $"{bytes / 1024d / 1024d:0.##} MiB";
            return $"{bytes / 1024d / 1024d / 1024d:0.##} GiB";
        }
    }
}
