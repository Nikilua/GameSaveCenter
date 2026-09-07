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

    public DateTime? NextAttemptLocal => NextAttemptUtc?.ToLocalTime();
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

    /// <summary>Explains what has actually been established about the remote copy.</summary>
    public string GuaranteeLevelDisplay => State switch
    {
        "RemoteVerified" => "远端 check 成功",
        "Uploaded" => "上传命令成功，尚未远端 check",
        "CheckFailed" => "远端 check 未通过",
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
    public string StateFilter { get; set; } = string.Empty;
    public CloudTransferKind? KindFilter { get; set; }
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
            : "自动队列运行中";
}
