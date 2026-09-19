using System;
using System.Collections.Generic;

namespace GameSaveCenter.Contracts
{
    public enum BackupAnomalyProtectionLevel
    {
        Off,
        Normal,
        Strict
    }

    /// <summary>Per-game backup and synchronization policy.</summary>
    public sealed class BackupPolicyDto
    {
        public bool Enabled { get; set; } = true;
        public bool BackupOnGameStop { get; set; } = true;
        public bool BackupDuringPlay { get; set; } = true;
        public int DuringPlayIntervalMinutes { get; set; } = 30;
        public bool UploadAfterBackup { get; set; }
        public bool SyncMediaDuringPlay { get; set; } = true;
        public bool SyncMediaOnGameStop { get; set; } = true;
        public bool AllowAutomaticRestore { get; set; }
        public BackupAnomalyProtectionLevel AnomalyProtectionLevel { get; set; } = BackupAnomalyProtectionLevel.Normal;
        public int KeepRecentAllHours { get; set; } = 24;
        public int KeepDailyDays { get; set; } = 30;
        public int KeepWeeklyWeeks { get; set; } = 12;
        public int KeepMonthlyMonths { get; set; } = 24;
    }

    /// <summary>Request to back up one game or all games.</summary>
    public sealed class BackupRequestDto : IIpcRequestWithId
    {
        public string RequestId { get; set; } = string.Empty;
        public List<string> PlayniteIds { get; set; } = new List<string>();
        public bool Force { get; set; }
        public string Reason { get; set; } = "Manual";
        public string SessionId { get; set; } = string.Empty;
        public string NotificationSessionId { get; set; } = string.Empty;
    }

    /// <summary>Read-only summary from Ludusavi's backup preview; it never represents an archive.</summary>
    public sealed class BackupPreviewDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public string State { get; set; } = "Empty";
        public DateTime GeneratedUtc { get; set; }
        public int PathCount { get; set; }
        public long TotalBytes { get; set; }
        public List<BackupPreviewPathDto> Paths { get; set; } = new List<BackupPreviewPathDto>();
        public string Summary { get; set; } = "点击“预览备份”后，这里会显示本次扫描范围。";
        public string Detail { get; set; } = string.Empty;
        public bool HasData => string.Equals(State, "Ready", StringComparison.OrdinalIgnoreCase) && PathCount > 0;
        public string GeneratedDisplay => GeneratedUtc == default(DateTime) ? "尚未生成" : $"扫描于 {GeneratedUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss}";
        public string PathSummaryDisplay => PathCount <= 0
            ? "尚未识别到可纳入备份的路径。"
            : $"已识别 {PathCount} 个路径 · {FormatBytes(TotalBytes)}{(Paths.Count < PathCount ? $" · 展示前 {Paths.Count} 个" : string.Empty)}";
        public string IdentifiedPathsDisplay => Paths.Count == 0
            ? "路径：尚未识别"
            : "路径：" + string.Join("；", Paths.ConvertAll(x => x.Path));
        public string StateDisplay => State switch
        {
            "Ready" => "已生成预览",
            "Loading" => "扫描中",
            "NoData" => "没有可纳入路径",
            "Unavailable" => "暂不可预览",
            "Error" => "预览失败",
            _ => "尚未预览"
        };

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024L * 1024) return $"{bytes / 1024d:0.##} KiB";
            if (bytes < 1024L * 1024 * 1024) return $"{bytes / 1024d / 1024d:0.##} MiB";
            return $"{bytes / 1024d / 1024d / 1024d:0.##} GiB";
        }
    }

    public sealed class BackupPreviewPathDto
    {
        public string Path { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
    }

    /// <summary>
    /// Layered outcome for a backup task. A cloud copy is a follow-up to the local
    /// version, so its failure must not erase or hide an already indexed local backup.
    /// </summary>
    public sealed class BackupResultDto
    {
        public string LocalState { get; set; } = "Unknown";
        public string CloudState { get; set; } = "Disabled";
        public string Summary { get; set; } = string.Empty;
        public string Remediation { get; set; } = string.Empty;

        public bool HasResult => !string.Equals(LocalState, "Unknown", StringComparison.OrdinalIgnoreCase);
        public bool LocalBackupSucceeded => string.Equals(LocalState, "Succeeded", StringComparison.OrdinalIgnoreCase);
        public bool IsPartialSuccess => LocalBackupSucceeded
            && !string.Equals(CloudState, "Disabled", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(CloudState, "Uploaded", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(CloudState, "RemoteVerified", StringComparison.OrdinalIgnoreCase);
        public bool CanRetryCloudUpload => LocalBackupSucceeded
            && (string.Equals(CloudState, "RetryScheduled", StringComparison.OrdinalIgnoreCase)
                || string.Equals(CloudState, "Failed", StringComparison.OrdinalIgnoreCase)
                || string.Equals(CloudState, "AuthenticationRequired", StringComparison.OrdinalIgnoreCase));
        public bool CanVerifyRemote => LocalBackupSucceeded
            && string.Equals(CloudState, "Uploaded", StringComparison.OrdinalIgnoreCase);

        public string StateDisplay
        {
            get
            {
                if (!LocalBackupSucceeded) return "本地备份未完成";
                return CloudState switch
                {
                    "Disabled" => "本地备份已成功",
                    "RetryScheduled" => "本地备份已成功 · 云端上传排队",
                    "Failed" => "本地备份已成功 · 云端镜像失败",
                    "AuthenticationRequired" => "本地备份已成功 · 云端认证需处理",
                    "Transferring" => "本地备份已成功 · 云端传输中",
                    "Uploaded" => "本地备份已成功 · 云端已上传，待远端校验",
                    "RemoteVerified" => "本地备份已成功 · 远端已校验",
                    _ => $"本地备份已成功 · 云端状态：{CloudState}"
                };
            }
        }

        public string RemediationDisplay => string.IsNullOrWhiteSpace(Remediation)
            ? CanRetryCloudUpload
                ? "可单独重试云端上传；不会重新创建本地备份。"
                : CanVerifyRemote
                    ? "可发起远端校验；不会修改本地副本。"
                    : string.Empty
            : Remediation;
    }

    /// <summary>Request to synchronize screenshot and video sources.</summary>
    public sealed class MediaSyncRequestDto
    {
        public List<string> PlayniteIds { get; set; } = new List<string>();
        public string SessionId { get; set; } = string.Empty;
        public string NotificationSessionId { get; set; } = string.Empty;
        public bool IncludeUnassignedInbox { get; set; } = true;
        public bool SharedOnly { get; set; }
        public bool UploadAfterSync { get; set; }
    }

    /// <summary>Safe restore request. Automatic restore is deliberately absent.</summary>
    public sealed class RestoreRequestDto : IIpcRequestWithId
    {
        public string RequestId { get; set; } = string.Empty;
        public string PlayniteId { get; set; } = string.Empty;
        public string BackupId { get; set; } = string.Empty;
        public bool ConfirmedCurrentSnapshot { get; set; }
        public bool ConfirmedGameClosed { get; set; }
        public string UserComment { get; set; } = string.Empty;
    }

    /// <summary>Durable, credential-free summary of one restore execution.</summary>
    public sealed class RestoreReportDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public string BackupId { get; set; } = string.Empty;
        public int FileCount { get; set; }
        public long TotalBytes { get; set; }
        public string PreRestoreBackupId { get; set; } = string.Empty;
        public bool PreRestoreCreated { get; set; }
        public string Stage { get; set; } = string.Empty;
        public string OutcomeKind { get; set; } = "Running";
        public string FailureCode { get; set; } = string.Empty;
        public bool WasRolledBack { get; set; }
        public bool RequiresManualIntervention { get; set; }
        public string TaskId { get; set; } = string.Empty;

        public string FileScopeDisplay => FileCount > 0 || TotalBytes > 0
            ? $"目标清单：{FileCount} 个文件 · {FormatBytes(TotalBytes)}"
            : "目标文件范围：由执行前预览确认，未返回可计数清单";

        public string ProtectionDisplay => PreRestoreCreated
            ? $"保护备份：已创建并锁定 {PreRestoreBackupId}"
            : "保护备份：尚未确认创建";

        public string OutcomeDisplay => OutcomeKind switch
        {
            "Completed" => "全部完成",
            "RolledBack" => "未完成，已回滚到保护快照",
            "ManualIntervention" => "未完成，需人工检查",
            "Cancelled" => "已取消",
            "Failed" => "未完成",
            _ => "执行中"
        };

        public string FailureDisplay => string.IsNullOrWhiteSpace(FailureCode)
            ? string.IsNullOrWhiteSpace(Stage) ? "无失败阶段" : $"阶段：{Stage}"
            : $"阶段：{Stage} · 错误码：{FailureCode}";

        public RestoreReportDto Clone() => new RestoreReportDto
        {
            PlayniteId = PlayniteId,
            GameName = GameName,
            BackupId = BackupId,
            FileCount = FileCount,
            TotalBytes = TotalBytes,
            PreRestoreBackupId = PreRestoreBackupId,
            PreRestoreCreated = PreRestoreCreated,
            Stage = Stage,
            OutcomeKind = OutcomeKind,
            FailureCode = FailureCode,
            WasRolledBack = WasRolledBack,
            RequiresManualIntervention = RequiresManualIntervention,
            TaskId = TaskId
        };

        public string ToRedactedText()
        {
            var lines = new List<string>
            {
                $"恢复结果：{OutcomeDisplay}",
                $"游戏：{GameName}",
                $"游戏 ID：{PlayniteId}",
                $"目标版本：{BackupId}",
                FileScopeDisplay,
                ProtectionDisplay,
                $"失败阶段：{FailureDisplay}",
                $"任务 ID：{TaskId}"
            };
            if (WasRolledBack) lines.Add("回滚：已执行");
            if (RequiresManualIntervention) lines.Add("人工介入：需要检查当前存档目录");
            return string.Join("\r\n", lines);
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024L * 1024) return $"{bytes / 1024d:0.##} KiB";
            if (bytes < 1024L * 1024 * 1024) return $"{bytes / 1024d / 1024d:0.##} MiB";
            return $"{bytes / 1024d / 1024d / 1024d:0.##} GiB";
        }
    }

    /// <summary>Request to validate one indexed backup without touching live save files.</summary>
    public sealed class RestoreReadinessRequestDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public string BackupId { get; set; } = string.Empty;
    }

    /// <summary>Request for save path candidate analysis.</summary>
    public sealed class DetectionRequestDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public bool IncludeXboxWgs { get; set; } = true;
        public List<string> AdditionalRoots { get; set; } = new List<string>();
    }

    /// <summary>Background task status used by progress UI and audit history.</summary>
    public sealed class TaskStatusDto
    {
        public string TaskId { get; set; } = string.Empty;
        /// <summary>IPC request that submitted this task; empty for legacy/non-IPC tasks.</summary>
        public string RequestId { get; set; } = string.Empty;
        /// <summary>Groups tasks launched by one game session for a single exit summary.</summary>
        public string SessionId { get; set; } = string.Empty;
        /// <summary>Identifies the Worker process/lifecycle that owned this task.</summary>
        public string WorkerSessionId { get; set; } = string.Empty;
        public string TaskType { get; set; } = string.Empty;
        public string GameId { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public TaskState State { get; set; }
        public int ProgressPercent { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; }
        public DateTime? StartedUtc { get; set; }
        public DateTime? FinishedUtc { get; set; }
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public BackupResultDto? BackupResult { get; set; }
        public RestoreReportDto? RestoreReport { get; set; }
        public bool HasRestoreReport => RestoreReport != null;
        public DateTime CreatedLocal => CreatedUtc.ToLocalTime();
        public int ProgressValue => Math.Max(0, Math.Min(100, ProgressPercent));
        public string ProgressDisplay => ProgressPercent < 0 || (State == TaskState.Queued && ProgressPercent == 0)
            ? "—"
            : $"{ProgressValue}%";
        public string StateDisplay => State switch
        {
            TaskState.Queued => "等待中",
            TaskState.Running => "执行中",
            TaskState.Succeeded => "成功",
            TaskState.Failed => "失败",
            TaskState.Cancelled => "已取消",
            TaskState.WaitingForUser => "等待确认",
            _ => State.ToString()
        };
        public string TaskTypeDisplay => TaskType switch
        {
            "Backup" => "存档备份",
            "Restore" => "存档恢复",
            "MediaSync" => "媒体同步",
            "MediaInbox" => "媒体归类",
            "BackupAll" => "整库备份",
            "TrainerDownload" => "修改器下载",
            "RemoteStage" => "远端备份下载",
            "CloudUpload" => "云端上传",
            "Validation" => "存档校验",
            _ => string.IsNullOrWhiteSpace(TaskType) ? "后台任务" : TaskType
        };
        public string DetailMessage => State == TaskState.Failed && !string.IsNullOrWhiteSpace(ErrorMessage)
            ? FormatFailureDetail(ErrorCode, ErrorMessage)
            : Message;
        public bool HasPartialSuccess => BackupResult?.IsPartialSuccess == true;

        private static string FormatFailureDetail(string errorCode, string errorMessage)
            => string.IsNullOrWhiteSpace(errorCode)
                ? errorMessage
                : $"错误码：{errorCode}；{errorMessage}";
        public bool CanCancel => State == TaskState.Queued || State == TaskState.Running;
        public DateTime? StartedLocal => StartedUtc?.ToLocalTime();
        public DateTime? FinishedLocal => FinishedUtc?.ToLocalTime();
        public string DurationDisplay
        {
            get
            {
                var start = StartedUtc ?? CreatedUtc;
                var end = FinishedUtc ?? DateTime.UtcNow;
                var duration = end - start;
                if (duration.TotalSeconds < 1) return "< 1 秒";
                if (duration.TotalMinutes < 1) return $"{duration.TotalSeconds:0} 秒";
                if (duration.TotalHours < 1) return $"{duration.TotalMinutes:0.#} 分钟";
                return $"{duration.TotalHours:0.#} 小时";
            }
        }
    }

    /// <summary>Incremental task state feed. It is deliberately a pull-based reliable fallback for short-lived IPC.</summary>
    public sealed class TaskChangeRequestDto
    {
        public long AfterSequence { get; set; }
        public int Limit { get; set; } = 100;
        /// <summary>
        /// Optional long-poll duration. Zero keeps the original immediate snapshot behavior.
        /// The Worker clamps this value so one client cannot hold a pipe indefinitely.
        /// </summary>
        public int WaitSeconds { get; set; }
    }

    public sealed class TaskChangeEventDto
    {
        public long Sequence { get; set; }
        public TaskStatusDto Task { get; set; } = new TaskStatusDto();
    }

    public sealed class TaskChangeFeedDto
    {
        public long LatestSequence { get; set; }
        public bool ResetRequired { get; set; }
        public List<TaskChangeEventDto> Changes { get; set; } = new List<TaskChangeEventDto>();
    }

    /// <summary>One validation result displayed to the user.</summary>
    public sealed class ValidationFindingDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        /// <summary>Resolved game title when this finding is sent in a dashboard snapshot.</summary>
        public string GameName { get; set; } = string.Empty;
        public FindingSeverity Severity { get; set; }
        public string SeverityDisplay => Severity switch
        {
            FindingSeverity.Info => "提示",
            FindingSeverity.Warning => "警告",
            FindingSeverity.Error => "错误",
            FindingSeverity.Critical => "严重",
            _ => Severity.ToString()
        };
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public string SuggestedAction { get; set; } = string.Empty;
    }


    /// <summary>Updates one game's independent automation policy.</summary>
    public sealed class GamePolicyUpdateDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public BackupPolicyDto Policy { get; set; } = new BackupPolicyDto();
    }

    public sealed class ApplyRecommendedProtectionDto
    {
        public List<string> PlayniteIds { get; set; } = new List<string>();
    }

    /// <summary>
    /// A reusable policy snapshot. Applying a template copies its current values to a
    /// game; it never creates a live inheritance relationship.
    /// </summary>
    public sealed class BackupPolicyTemplateDto
    {
        public string TemplateId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsBuiltIn { get; set; }
        public BackupPolicyDto Policy { get; set; } = new BackupPolicyDto();
    }

    /// <summary>Creates or updates one user-owned policy template.</summary>
    public sealed class PolicyTemplateSaveDto
    {
        public BackupPolicyTemplateDto Template { get; set; } = new BackupPolicyTemplateDto();
    }

    /// <summary>Deletes one user-owned policy template.</summary>
    public sealed class PolicyTemplateDeleteDto
    {
        public string TemplateId { get; set; } = string.Empty;
    }

    /// <summary>Copies a template snapshot to one game's independent policy.</summary>
    public sealed class ApplyPolicyTemplateDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public string TemplateId { get; set; } = string.Empty;
    }

    /// <summary>Compares two indexed backup manifests.</summary>
    public sealed class BackupCompareRequestDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public string LeftBackupId { get; set; } = string.Empty;
        public string RightBackupId { get; set; } = string.Empty;
    }

    /// <summary>User-defined screenshot/video source for a game or shared inbox.</summary>
    public sealed class MediaSourceRuleDto
    {
        public string SourceId { get; set; } = string.Empty;
        public string PlayniteId { get; set; } = string.Empty;
        public MediaSourceKind SourceKind { get; set; } = MediaSourceKind.Custom;
        public string RootPath { get; set; } = string.Empty;
        public string IncludePattern { get; set; } = "*";
        public bool Enabled { get; set; } = true;
        public bool SharedDirectory { get; set; }
        public string SourceKindDisplay => SourceKind switch
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
    }

    /// <summary>Updates a Ludusavi backup comment and lock state.</summary>
    public sealed class BackupMetadataUpdateDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public string BackupId { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public bool? Locked { get; set; }
    }

    /// <summary>Updates user-owned metadata without moving or deleting the media file.</summary>
    public sealed class MediaMetadataUpdateDto
    {
        public string MediaId { get; set; } = string.Empty;
        public bool IsFavorite { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    /// <summary>Atomically updates non-destructive metadata for an explicit media selection.</summary>
    public sealed class MediaMetadataBatchUpdateDto
    {
        public List<string> MediaIds { get; set; } = new List<string>();
        public bool? IsFavorite { get; set; }
        public bool UpdateComment { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    /// <summary>Simple per-game query used by list and undo operations.</summary>
    public sealed class GameQueryDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public int Limit { get; set; } = 200;
        /// <summary>Optional zero-based offset for bounded list responses.</summary>
        public int Offset { get; set; }
        public bool ForceRefresh { get; set; }
    }

    /// <summary>Request to cancel a background task.</summary>
    public sealed class CancelTaskRequestDto
    {
        public string TaskId { get; set; } = string.Empty;
    }

    /// <summary>Result returned after requesting task cancellation.</summary>
    public sealed class CancelTaskResultDto
    {
        public bool Cancelled { get; set; }
    }

    /// <summary>Effective non-secret Worker settings used by diagnostics UI.</summary>
    public sealed class WorkerSettingsSnapshotDto
    {
        public string DataDirectory { get; set; } = string.Empty;
        public bool SafeModeEnabled { get; set; }
        public bool SafeModeRequested { get; set; }
        public string LudusaviExecutable { get; set; } = string.Empty;
        public string LudusaviBackupDirectory { get; set; } = string.Empty;
        public string RcloneExecutable { get; set; } = string.Empty;
        public bool RcloneDestinationConfigured { get; set; }
        public string MediaArchiveDirectory { get; set; } = string.Empty;
        public bool EnableLocalMirror { get; set; }
        public string LocalMirrorPath { get; set; } = string.Empty;
        public int ProcessPollingSeconds { get; set; } = 5;
        public int DefaultBackupIntervalMinutes { get; set; } = 30;
        public bool EnableProcessDetection { get; set; } = true;
        public bool EnableSessionSavePathDetection { get; set; } = true;
        public bool EnableMediaSync { get; set; } = true;
        public bool EnableSteamMedia { get; set; } = true;
        public bool EnableXboxGameBarMedia { get; set; } = true;
        public bool EnableWindowsScreenshotMedia { get; set; } = true;
        public bool EnablePlatformAdjacentMedia { get; set; } = true;
        public bool EnableCustomMedia { get; set; } = true;
        public bool EnableCloudUpload { get; set; }
        public bool CloudUploadQueuePaused { get; set; }
        public int CloudUploadAllowedStartMinute { get; set; }
        public int CloudUploadAllowedEndMinute { get; set; } = 1440;
        public BackupStorageFormat BackupFormat { get; set; } = BackupStorageFormat.Zip;
        public string Compression { get; set; } = "zstd";
        public int CompressionLevel { get; set; } = 3;
        public int FullBackupLimit { get; set; } = 3;
        public int DifferentialBackupLimit { get; set; } = 5;
        public bool HealthInspectionEnabled { get; set; } = true;
        public int HealthInspectionIntervalMinutes { get; set; } = 1440;
        public int HealthInspectionStaleAfterDays { get; set; } = 30;
    }

    /// <summary>Moves an indexed media item to another game without touching the original capture.</summary>
    public sealed class ReassignMediaRequestDto
    {
        public string MediaId { get; set; } = string.Empty;
        public string TargetPlayniteId { get; set; } = string.Empty;
    }

    /// <summary>Removes an unassigned media item from the inbox without deleting its archive copy.</summary>
    public sealed class IgnoreMediaRequestDto
    {
        public string MediaId { get; set; } = string.Empty;
    }

    /// <summary>Batch request for inbox classification actions.</summary>
    public sealed class MediaInboxBatchRequestDto
    {
        public List<string> MediaIds { get; set; } = new List<string>();
        public string TargetPlayniteId { get; set; } = string.Empty;
    }

    /// <summary>One item that could not be processed during a best-effort inbox batch.</summary>
    public sealed class MediaInboxBatchFailureDto
    {
        public string MediaId { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>Best-effort result for a media inbox batch operation.</summary>
    public sealed class MediaInboxBatchResultDto
    {
        public List<MediaItemDto> UpdatedItems { get; set; } = new List<MediaItemDto>();
        public List<MediaInboxBatchFailureDto> Failures { get; set; } = new List<MediaInboxBatchFailureDto>();
    }

    /// <summary>Accepts a detected save directory and creates a custom Ludusavi rule draft.</summary>
    public sealed class AcceptSavePathRequestDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public bool IncludeSubdirectories { get; set; } = true;
    }

    /// <summary>Requests an immediate validity check for one game's latest backup.</summary>
    public sealed class ValidateGameRequestDto
    {
        public string PlayniteId { get; set; } = string.Empty;
    }

    /// <summary>Non-secret runtime settings supplied by the Playnite plugin.</summary>
    public sealed class WorkerSettingsDto
    {
        /// <summary>Stable opaque identity for this GameSaveCenter installation. The machine name is display-only.</summary>
        public string DeviceId { get; set; } = string.Empty;
        public bool SafeModeEnabled { get; set; }
        public bool SafeModeRequested { get; set; }
        public string LudusaviExecutable { get; set; } = string.Empty;
        public string LudusaviBackupDirectory { get; set; } = string.Empty;
        public string RcloneExecutable { get; set; } = string.Empty;
        public string RcloneDestination { get; set; } = string.Empty;
        public string MediaArchiveDirectory { get; set; } = string.Empty;
        public bool EnableLocalMirror { get; set; }
        public string LocalMirrorPath { get; set; } = string.Empty;
        public int ProcessPollingSeconds { get; set; } = 5;
        public int DefaultBackupIntervalMinutes { get; set; } = 30;
        public bool EnableProcessDetection { get; set; } = true;
        public bool EnableSessionSavePathDetection { get; set; } = true;
        public bool EnableMediaSync { get; set; } = true;
        public bool EnableSteamMedia { get; set; } = true;
        public bool EnableXboxGameBarMedia { get; set; } = true;
        public bool EnableWindowsScreenshotMedia { get; set; } = true;
        public bool EnablePlatformAdjacentMedia { get; set; } = true;
        public bool EnableCustomMedia { get; set; } = true;
        public bool EnableCloudUpload { get; set; }
        public bool CloudUploadQueuePaused { get; set; }
        public int CloudUploadAllowedStartMinute { get; set; }
        public int CloudUploadAllowedEndMinute { get; set; } = 1440;
        public BackupStorageFormat BackupFormat { get; set; } = BackupStorageFormat.Zip;
        public string Compression { get; set; } = "zstd";
        public int CompressionLevel { get; set; } = 3;
        public int FullBackupLimit { get; set; } = 3;
        public int DifferentialBackupLimit { get; set; } = 5;
        public bool HealthInspectionEnabled { get; set; } = true;
        public int HealthInspectionIntervalMinutes { get; set; } = 1440;
        public int HealthInspectionStaleAfterDays { get; set; } = 30;
    }

}
