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
        public string WorkerBuildIdentity { get; set; } = string.Empty;
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
        public CloudTransferSummaryDto CloudTransfers { get; set; } = new CloudTransferSummaryDto();
        public HealthInspectionStateDto HealthInspection { get; set; } = new HealthInspectionStateDto();
        public TaskSummaryDto TaskSummary { get; set; } = new TaskSummaryDto();
        public int TodaySucceededTaskCount { get; set; }
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
        /// <summary>Stable Playnite game id when the event is scoped to a game; empty for global events.</summary>
        public string PlayniteId { get; set; } = string.Empty;
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
        public string CreatedRelativeDisplay => TimeDisplayFormatter.Relative(CreatedUtc, DateTime.UtcNow);
        public string CreatedFullDisplay => TimeDisplayFormatter.Full(CreatedUtc);
        public string CreatedRawUtcDisplay => TimeDisplayFormatter.RawUtc(CreatedUtc);

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
        /// <summary>Matches the retention planner's healthy restore-point safety floor.</summary>
        public bool IsHealthProtected => RestoreReadiness?.Status == RestoreReadinessStatus.Ready
            && FileCount > 0
            && TotalBytes > 0;
        public bool IsRetentionProtected => IsLocked || IsPreRestore || IsHealthProtected;
        /// <summary>Resolved Ludusavi game backup directory plus this version's file name.</summary>
        public string ArchivePath { get; set; } = string.Empty;
        public RestoreReadinessDto? RestoreReadiness { get; set; }
        public DateTime CreatedLocal => CreatedUtc.ToLocalTime();
        public string CreatedRelativeDisplay => TimeDisplayFormatter.Relative(CreatedUtc, DateTime.UtcNow);
        public string CreatedFullDisplay => TimeDisplayFormatter.Full(CreatedUtc);
        public string CreatedRawUtcDisplay => TimeDisplayFormatter.RawUtc(CreatedUtc);
        public string SizeDisplay => FormatBytes(TotalBytes);
        public string BackupTypeDisplay => IsPreRestore ? "恢复前快照" : "普通备份";
        public string ComparisonDisplay => $"{CreatedLocal:yyyy-MM-dd HH:mm} · {BackupTypeDisplay} · {BackupId}";
        public string ComparisonRelativeDisplay => $"{CreatedRelativeDisplay} · {BackupTypeDisplay} · {BackupId}";
        public string ComparisonFullDisplay => $"{CreatedFullDisplay} · {BackupTypeDisplay} · {BackupId}";
        public string ComparisonRawUtcDisplay => CreatedRawUtcDisplay;
        public string LockStateDisplay => IsLocked ? "已锁定" : "未锁定";
        public string SourceDisplay => string.IsNullOrWhiteSpace(SourceDevice) ? "未知设备" : SourceDevice;
        public string OperatingSystemDisplay => string.IsNullOrWhiteSpace(OperatingSystem) ? "未知系统" : OperatingSystem;
        public string RestoreReadinessStatusDisplay => RestoreReadiness?.StatusDisplay ?? "未验证";
        public string ProtectionAndReadinessDisplay => $"{LockStateDisplay} · {RestoreReadinessStatusDisplay}";
        public string RetentionProtectionGlyphDisplay => IsRetentionProtected ? "✓" : "⚠";
        public string RetentionProtectionDisplay => IsLocked
            ? "已锁定保护"
            : IsPreRestore
                ? "PreRestore 保护"
                : IsHealthProtected
                    ? "健康恢复点保护"
                    : "未受保护";
        public string RetentionProtectionExplanationDisplay => IsLocked
            ? "用户锁定：保留预览始终跳过；取消锁定并保存后，下一次预览才会按策略重新评估。"
            : IsPreRestore
                ? "PreRestore 快照：由恢复保护流程保留，保留预览始终跳过。"
                : IsHealthProtected
                    ? "健康恢复点：作为恢复安全底线，保留预览始终跳过。"
                    : "未受保护：保留预览会按当前策略评估；如需长期保留，请锁定并保存。";
        public string RestoreReadinessSummaryDisplay => RestoreReadiness?.Summary ?? "尚未验证该版本的可恢复性。";
        public string RestoreReadinessHashValidationDisplay => RestoreReadiness?.HashValidationDisplay ?? "未提供哈希（不等于校验成功）";
        public string RestoreReadinessHashCoverageDisplay => RestoreReadiness?.HashCoverageDisplay ?? "哈希覆盖：未提供";
        public string RestoreReadinessMetricsDisplay => RestoreReadiness == null
            ? string.Empty
            : $"文件 {RestoreReadiness.ActualFileCount}/{RestoreReadiness.ExpectedFileCount} · 大小 {FormatBytes(RestoreReadiness.ActualTotalSize)}/{FormatBytes(RestoreReadiness.ExpectedTotalSize)}";
        public string RestoreReadinessCheckedDisplay
        {
            get
            {
                if (RestoreReadiness?.CheckedUtc is not DateTime checkedUtc)
                    return "尚未检查";

                var localChecked = checkedUtc.ToLocalTime();
                var age = DateTime.UtcNow - checkedUtc.ToUniversalTime();
                return age >= TimeSpan.FromDays(1)
                    ? $"检查于 {localChecked:yyyy-MM-dd HH:mm:ss}（结果较旧，建议重新验证）"
                    : $"检查于 {localChecked:yyyy-MM-dd HH:mm:ss}";
            }
        }
        public string RestoreReadinessCheckedRelativeDisplay => BuildRestoreReadinessCheckedDisplay(false);
        public string RestoreReadinessCheckedFullDisplay => BuildRestoreReadinessCheckedDisplay(true);

        private string BuildRestoreReadinessCheckedDisplay(bool full)
        {
            if (RestoreReadiness?.CheckedUtc is not DateTime checkedUtc)
                return "尚未检查";

            var age = DateTime.UtcNow - checkedUtc.ToUniversalTime();
            var time = full
                ? TimeDisplayFormatter.Full(checkedUtc)
                : TimeDisplayFormatter.Relative(checkedUtc, DateTime.UtcNow);
            return age >= TimeSpan.FromDays(1)
                ? $"检查于 {time}（结果较旧，建议重新验证）"
                : $"检查于 {time}";
        }

        private static string FormatBytes(long bytes) => ByteSizeFormatter.Format(bytes);
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
        /// <summary>Number of manifest entries that contain a SHA-256 value.</summary>
        public int HashCoveredFileCount { get; set; }
        /// <summary>Total number of manifest entries eligible for hash coverage.</summary>
        public int HashEligibleFileCount { get; set; }
        public int WarningCount { get; set; }
        public int ErrorCount { get; set; }
        /// <summary>Whether the Worker-owned temporary extraction directory was removed.</summary>
        public string StagingCleanupStatus { get; set; } = "NotNeeded";
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

        public string HashValidationDisplay => HashValidation switch
        {
            "Validated" => "哈希已覆盖并通过",
            "Partial" => "哈希部分覆盖",
            "Failed" => "哈希校验失败",
            _ => "未提供哈希（不等于校验成功）"
        };

        public string HashCoverageDisplay => HashEligibleFileCount > 0
            ? $"哈希覆盖：{HashCoveredFileCount}/{HashEligibleFileCount} 个文件"
            : "哈希覆盖：未提供";
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
        public string TotalBytesDeltaDisplay => ByteSizeFormatter.FormatDelta(TotalBytesDelta);
        public string Summary { get; set; } = string.Empty;

        private static string FormatBytes(long bytes) => ByteSizeFormatter.Format(bytes);
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
        public string CreatedRelativeDisplay => TimeDisplayFormatter.Relative(CreatedUtc, DateTime.UtcNow);
        public string CreatedFullDisplay => TimeDisplayFormatter.Full(CreatedUtc);
        public string CreatedRawUtcDisplay => TimeDisplayFormatter.RawUtc(CreatedUtc);
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
        public string CapturedRelativeDisplay => TimeDisplayFormatter.Relative(CapturedUtc, DateTime.UtcNow);
        public string CapturedFullDisplay => TimeDisplayFormatter.Full(CapturedUtc);
        public string CapturedRawUtcDisplay => TimeDisplayFormatter.RawUtc(CapturedUtc);
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
            "AuthenticationRequired" => "认证需处理",
            "Transferring" => "传输中",
            "RemoteVerified" => "已校验",
            "CheckFailed" => "校验失败",
            "Paused" => "已暂停",
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

        private static string FormatBytes(long bytes) => ByteSizeFormatter.Format(bytes);
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

        private static string FormatBytes(long bytes) => ByteSizeFormatter.Format(bytes);
    }
}
