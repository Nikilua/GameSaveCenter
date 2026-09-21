using System;
using System.Collections.Generic;

namespace GameSaveCenter.Contracts;

/// <summary>Kind of local content that can be copied to the configured remote.</summary>
public enum CloudTransferKind
{
    Backup,
    Media
}

/// <summary>Durable operation currently owning a cloud transfer row.</summary>
public enum CloudTransferOperationKind
{
    Upload,
    Verify
}

/// <summary>Outcome of a user-requested media-only cloud upload retry.</summary>
public enum MediaCloudRetryOutcome
{
    Submitted,
    PausedByPolicy,
    CannotSubmit
}

/// <summary>Typed request for retrying an already archived media copy.</summary>
public sealed class MediaCloudRetryRequestDto
{
    public string PlayniteId { get; set; } = string.Empty;
}

/// <summary>
/// Explicitly distinguishes an accepted retry from a policy pause or a preflight failure.
/// The embedded task is included when the Worker reached the task coordinator so the UI can
/// report a terminal failure/cancellation without claiming that an upload was accepted.
/// </summary>
public sealed class MediaCloudRetryResultDto
{
    public MediaCloudRetryOutcome Outcome { get; set; }
    public string PlayniteId { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public TaskStatusDto? Task { get; set; }
}

/// <summary>Request for a read-only remote check of one persisted transfer target.</summary>
public sealed class CloudTransferVerifyRequestDto
{
    public string PlayniteId { get; set; } = string.Empty;
    public CloudTransferKind Kind { get; set; } = CloudTransferKind.Backup;
}

/// <summary>Bounded query for the cloud transfer summary and its independently paged details.</summary>
public sealed class CloudTransferStatusRequestDto
{
    public int Page { get; set; }
    public int PageSize { get; set; } = 100;
    public string State { get; set; } = string.Empty;
    public CloudTransferKind? Kind { get; set; }
    /// <summary>Case-insensitive game-name or Playnite-id fragment.</summary>
    public string GameName { get; set; } = string.Empty;
    /// <summary>Exact current source-device key/name; empty means all devices.</summary>
    public string SourceDevice { get; set; } = string.Empty;
    public DateTime? UpdatedAfterUtc { get; set; }
    public DateTime? UpdatedBeforeUtc { get; set; }
    /// <summary>Opaque Worker-owned revision returned by the preceding page.</summary>
    public string ConsistencyToken { get; set; } = string.Empty;
}

/// <summary>One durable cloud transfer status. A successful copy is not a remote check.</summary>
public sealed class CloudTransferStatusDto
{
    public string TransferKey { get; set; } = string.Empty;
    public CloudTransferKind Kind { get; set; }
    public CloudTransferOperationKind OperationKind { get; set; } = CloudTransferOperationKind.Upload;
    public string PlayniteId { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;
    public string State { get; set; } = "Pending";
    public int AttemptCount { get; set; }
    public DateTime? NextAttemptUtc { get; set; }
    public DateTime? LastAttemptUtc { get; set; }
    public string LastErrorCode { get; set; } = string.Empty;
    public string LastError { get; set; } = string.Empty;
    public DateTime UpdatedUtc { get; set; }
    /// <summary>Display-only full remote object path; credentials are already redacted.</summary>
    public string RemoteObject { get; set; } = string.Empty;
    /// <summary>Stable device key or display name used by the remote object layout.</summary>
    public string SourceDevice { get; set; } = string.Empty;
    /// <summary>
    /// The current durable row can prove this timestamp only while it is RemoteVerified.
    /// Historical verification timestamps are unknown because the queue does not retain them.
    /// </summary>
    public DateTime? LastSuccessfulVerificationUtc { get; set; }

    public DateTime? NextAttemptLocal => NextAttemptUtc?.ToLocalTime();
    public string RemoteObjectDisplay => string.IsNullOrWhiteSpace(RemoteObject)
        ? "未知"
        : CloudRemoteDisplay.Redact(RemoteObject);
    public string SourceDeviceDisplay => string.IsNullOrWhiteSpace(SourceDevice) ? "未知设备" : SourceDevice;
    public string LastAttemptDisplay => LastAttemptUtc.HasValue
        ? LastAttemptUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
        : "未知";
    public string LastAttemptRelativeDisplay => LastAttemptUtc.HasValue
        ? TimeDisplayFormatter.Relative(LastAttemptUtc.Value, DateTime.UtcNow)
        : "未知";
    public string LastAttemptFullDisplay => LastAttemptUtc.HasValue
        ? TimeDisplayFormatter.Full(LastAttemptUtc.Value)
        : "未知";
    public string LastAttemptRawUtcDisplay => TimeDisplayFormatter.RawUtc(LastAttemptUtc ?? DateTime.MinValue);
    public string LastSuccessfulVerificationDisplay => LastSuccessfulVerificationUtc.HasValue
        ? LastSuccessfulVerificationUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
        : "未知";
    public string LastSuccessfulVerificationRelativeDisplay => LastSuccessfulVerificationUtc.HasValue
        ? TimeDisplayFormatter.Relative(LastSuccessfulVerificationUtc.Value, DateTime.UtcNow)
        : "未知";
    public string LastSuccessfulVerificationFullDisplay => LastSuccessfulVerificationUtc.HasValue
        ? TimeDisplayFormatter.Full(LastSuccessfulVerificationUtc.Value)
        : "未知";
    public string LastSuccessfulVerificationRawUtcDisplay => TimeDisplayFormatter.RawUtc(LastSuccessfulVerificationUtc ?? DateTime.MinValue);
    public string RetryTimingDisplay
    {
        get
        {
            if (!NextAttemptUtc.HasValue) return "无自动重试";
            var remaining = NextAttemptUtc.Value - DateTime.UtcNow;
            var relative = remaining <= TimeSpan.Zero
                ? "可立即重试"
                : $"约 {FormatRemaining(remaining)} 后";
            var nextAttemptLocal = NextAttemptUtc.Value.ToLocalTime();
            return $"{nextAttemptLocal:yyyy-MM-dd HH:mm} · {relative}";
        }
    }
    public string KindDisplay => Kind == CloudTransferKind.Backup ? "备份" : "媒体";
    public string StateDisplay => State switch
    {
        "Pending" => "待上传",
        "Transferring" => "传输中",
        "Verifying" => "远端校验中",
        "RetryScheduled" => "下次尝试",
        "AuthenticationRequired" => "认证需处理",
        "Uploaded" => "已上传",
        "RemoteVerified" => "已校验",
        "CheckCancelled" => "校验已取消",
        "CheckFailed" => "校验失败",
        "Failed" => "上传失败",
        "Paused" => "已暂停",
        _ => string.IsNullOrWhiteSpace(State) ? "未启用" : "未知状态"
    };

    /// <summary>Readable queue phase; network backoff stays distinct from a generic retry.</summary>
    public string QueuePhaseDisplay => State switch
    {
        "Pending" => "等待队列",
        "RetryScheduled" when IsNetworkWait => "等待网络",
        "RetryScheduled" => "等待重试",
        "Transferring" => "上传中",
        "Verifying" => "验证中",
        "RemoteVerified" => "已验证",
        "Uploaded" => "等待验证",
        "Paused" => "等待策略时段",
        _ => StateDisplay
    };

    /// <summary>Whether the selected row is an eligible target for a user retry.</summary>
    public bool CanManuallyRetry => string.Equals(State, "Failed", StringComparison.OrdinalIgnoreCase)
        || string.Equals(State, "RetryScheduled", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Explains the selected-row scope. A retry copies the already preserved local source;
    /// it does not recreate a local backup or reprocess a successful transfer.
    /// </summary>
    public string ManualRetryScopeDisplay => CanManuallyRetry
        ? "手动范围：仅当前选中项；只重试云端上传，不重新执行本地备份。"
        : "当前状态不可手动重试；传输中或已完成项不会重复提交。";

    /// <summary>
    /// Explains the bounded offline recovery path without claiming that every queued item
    /// will start at once when connectivity returns.
    /// </summary>
    public string NetworkRecoveryDisplay => State switch
    {
        "RetryScheduled" when IsNetworkFailure => $"等待网络恢复；按退避时间重试（已用 {Math.Max(0, AttemptCount)}/6 次自动重试，本轮最多 10 项）",
        "Transferring" when IsNetworkFailure => "网络已恢复；按批次上传中",
        _ => string.Empty
    };

    public bool HasRecognizedFailure => CloudFailureExplanation.Resolve(LastErrorCode).IsRecognized;
    public string FailureCategoryDisplay => CloudFailureExplanation.Resolve(LastErrorCode).CategoryDisplay;
    public string FailureNextStepDisplay => CloudFailureExplanation.Resolve(LastErrorCode).NextStepDisplay;

    private bool IsNetworkFailure => string.Equals(LastErrorCode, "RCLONE_NETWORK_FAILED", StringComparison.OrdinalIgnoreCase)
        || string.Equals(LastErrorCode, "RCLONE_TRANSFER_INCOMPLETE", StringComparison.OrdinalIgnoreCase)
        || string.Equals(LastErrorCode, "RCLONE_RATE_LIMITED", StringComparison.OrdinalIgnoreCase);

    private bool IsNetworkWait => string.Equals(State, "RetryScheduled", StringComparison.OrdinalIgnoreCase) && IsNetworkFailure;

    private static string FormatRemaining(TimeSpan remaining)
    {
        if (remaining.TotalDays >= 1) return $"{(int)remaining.TotalDays} 天";
        if (remaining.TotalHours >= 1) return $"{(int)remaining.TotalHours} 小时";
        return $"{Math.Max(1, (int)Math.Ceiling(remaining.TotalMinutes))} 分钟";
    }

    /// <summary>Explains what has actually been established about the remote copy.</summary>
    public string GuaranteeLevelDisplay => State switch
    {
        "RemoteVerified" => "远端校验成功",
        "Uploaded" => "上传命令成功，尚未远端校验",
        "CheckFailed" => "远端校验未通过",
        _ => "仅确认本地副本已保留"
    };

    public string DetailDisplay
    {
        get
        {
            var attempt = AttemptCount > 0 ? $"第 {AttemptCount} 次" : "尚未重试";
            var reason = string.IsNullOrWhiteSpace(LastError) ? string.Empty : $" · {LastError}";
            var next = NextAttemptLocal.HasValue ? $" · {NextAttemptLocal.Value:MM-dd HH:mm} 再试" : string.Empty;
            return $"{StateDisplay} · {attempt}{next}{reason}";
        }
    }
}

/// <summary>Bounded aggregate used by dashboard and maintenance views.</summary>
public sealed class CloudTransferSummaryDto
{
    public int TotalCount { get; set; }
    /// <summary>Total queue rows before the current state/kind/game/device/time filters.</summary>
    public int GlobalTotalCount { get; set; }
    public int PendingCount { get; set; }
    public int TransferringCount { get; set; }
    public int VerifyingCount { get; set; }
    public int RetryScheduledCount { get; set; }
    public int AuthenticationRequiredCount { get; set; }
    public int UploadedCount { get; set; }
    public int VerifiedCount { get; set; }
    public int CheckFailedCount { get; set; }
    public int FailedCount { get; set; }
    public int PausedCount { get; set; }
    public bool QueuePaused { get; set; }
    public bool OutsideAllowedWindow { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int LoadedCount { get; set; }
    public bool HasMore { get; set; }
    public string ConsistencyToken { get; set; } = string.Empty;
    public bool PageResetRequired { get; set; }
    public string PageResetReason { get; set; } = string.Empty;
    public string StateFilter { get; set; } = string.Empty;
    public CloudTransferKind? KindFilter { get; set; }
    public string GameNameFilter { get; set; } = string.Empty;
    public string SourceDeviceFilter { get; set; } = string.Empty;
    public DateTime? UpdatedAfterUtc { get; set; }
    public DateTime? UpdatedBeforeUtc { get; set; }
    public DateTime? NextAttemptUtc { get; set; }
    public List<CloudTransferStatusDto> Items { get; set; } = new List<CloudTransferStatusDto>();

    public DateTime? NextAttemptLocal => NextAttemptUtc?.ToLocalTime();
    public int AttentionCount => RetryScheduledCount + AuthenticationRequiredCount + CheckFailedCount + FailedCount;
    public int QueueCount => PendingCount + TransferringCount + VerifyingCount + RetryScheduledCount + AuthenticationRequiredCount
        + CheckFailedCount + FailedCount + PausedCount;
    public string PrimaryStatusDisplay
    {
        get
        {
            if (AuthenticationRequiredCount > 0) return "认证需处理";
            if (CheckFailedCount > 0 && FailedCount > 0) return "校验/上传失败";
            if (CheckFailedCount > 0) return "校验失败";
            if (FailedCount > 0) return "上传失败";
            if (RetryScheduledCount > 0) return "下次尝试";
            if (VerifyingCount > 0) return "远端校验中";
            if (TransferringCount > 0) return "传输中";
            if (PendingCount > 0) return "待上传";
            if (VerifiedCount > 0) return "已校验";
            if (UploadedCount > 0) return "已上传";
            if (PausedCount > 0) return "已暂停";
            return "无云端任务";
        }
    }

    public string SummaryDisplay
    {
        get
        {
            if (TotalCount == 0) return "暂无云端传输记录";
            var next = NextAttemptLocal.HasValue ? $"，下次 {NextAttemptLocal.Value:MM-dd HH:mm}" : string.Empty;
            return $"{PrimaryStatusDisplay} · {TotalCount} 项{next}";
        }
    }

    public string LoadedDisplay => HasMore
        ? $"已加载 {LoadedCount}/{TotalCount} 项"
        : $"已加载全部 {LoadedCount} 项";

    public string QueueControlDisplay => QueuePaused
        ? "自动队列已暂停"
        : OutsideAllowedWindow
            ? "当前不在允许时段"
            : TotalCount <= 0
                ? "队列空闲"
                : "自动队列运行中";

    /// <summary>
    /// Keeps a successful upload distinct from a remote verification. A remote
    /// verification is a stronger guarantee, not an alias for an upload.
    /// </summary>
    public string GuaranteeDisplay => $"已上传 {UploadedCount} · 已校验 {VerifiedCount}";
}
