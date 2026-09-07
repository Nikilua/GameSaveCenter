using System;
using System.Collections.Generic;

namespace GameSaveCenter.Contracts;

/// <summary>Requests conservative suggestions for explicit inbox media items.</summary>
public sealed class MediaClassificationPreviewRequestDto
{
    public string RequestId { get; set; } = string.Empty;
    public List<string> MediaIds { get; set; } = new List<string>();
    public string SessionId { get; set; } = string.Empty;
    public int Limit { get; set; } = 200;
}

/// <summary>One explainable game suggestion. Low-confidence items have no target.</summary>
public sealed class MediaClassificationSuggestionDto
{
    public string MediaId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime CapturedUtc { get; set; }
    public string SuggestedPlayniteId { get; set; } = string.Empty;
    public string SuggestedGameName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Confidence { get; set; } = "Low";
    public string State { get; set; } = "Suggested";

    public DateTime CapturedLocal => CapturedUtc.ToLocalTime();
    public bool CanApply => !string.IsNullOrWhiteSpace(SuggestedPlayniteId) && Confidence == "High";
    public string ConfidenceDisplay => Confidence switch
    {
        "High" => "高置信",
        "Medium" => "中置信",
        _ => "低置信"
    };
    public string StateDisplay => State switch
    {
        "Applied" => "已应用",
        "Conflict" => "有冲突",
        "Skipped" => "已跳过",
        _ => CanApply ? "待确认" : "保持未归类"
    };
    public string SummaryDisplay => string.IsNullOrWhiteSpace(SuggestedGameName)
        ? $"{ConfidenceDisplay} · {Reason}"
        : $"{ConfidenceDisplay} · {SuggestedGameName} · {Reason}";
}

/// <summary>Worker-owned, expiring preview that must be explicitly confirmed.</summary>
public sealed class MediaClassificationPreviewDto
{
    public string BatchId { get; set; } = string.Empty;
    public string State { get; set; } = "Preview";
    public DateTime CreatedUtc { get; set; }
    public DateTime ExpiresUtc { get; set; }
    public List<MediaClassificationSuggestionDto> Items { get; set; } = new List<MediaClassificationSuggestionDto>();
    public int HighConfidenceCount { get; set; }
    public int MediumConfidenceCount { get; set; }
    public int LowConfidenceCount { get; set; }
    public string SummaryDisplay =>
        $"建议 {Items.Count} 项：高置信 {HighConfidenceCount}，中置信 {MediumConfidenceCount}，低置信 {LowConfidenceCount}；仅高置信可批量确认。";
}

/// <summary>Confirms selected suggestions from one still-valid preview.</summary>
public sealed class MediaClassificationApplyRequestDto
{
    public string RequestId { get; set; } = string.Empty;
    public string BatchId { get; set; } = string.Empty;
    public List<string> MediaIds { get; set; } = new List<string>();
    public bool HighConfidenceOnly { get; set; } = true;
}

/// <summary>Requests an undo of the applied metadata/moves from one classification batch.</summary>
public sealed class MediaClassificationUndoRequestDto
{
    public string RequestId { get; set; } = string.Empty;
    public string BatchId { get; set; } = string.Empty;
}

/// <summary>Bounded query for durable media classification batches.</summary>
public sealed class MediaClassificationHistoryRequestDto
{
    public int Page { get; set; }
    public int PageSize { get; set; } = 25;
    public string State { get; set; } = string.Empty;
}

/// <summary>Aggregated, restart-safe state for one classification batch.</summary>
public sealed class MediaClassificationBatchSummaryDto
{
    public string BatchId { get; set; } = string.Empty;
    public string State { get; set; } = "Preview";
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
    public DateTime ExpiresUtc { get; set; }
    public string LastError { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public int AppliedCount { get; set; }
    public int UndoneCount { get; set; }
    public int ConflictCount { get; set; }
    public int SkippedCount { get; set; }

    public DateTime CreatedLocal => CreatedUtc.ToLocalTime();
    public DateTime UpdatedLocal => UpdatedUtc.ToLocalTime();
    public DateTime ExpiresLocal => ExpiresUtc.ToLocalTime();
    public bool IsUndoable => (State == "Applied" || State == "AppliedWithConflicts") && AppliedCount > 0;
    public string StateDisplay => State switch
    {
        "Preview" => "待确认",
        "Applied" => "已应用",
        "AppliedWithConflicts" => "已应用 · 有冲突",
        "Undone" => "已撤销",
        "UndoneWithConflicts" => "已撤销 · 有冲突",
        "Conflict" => "应用冲突",
        "Expired" => "已过期",
        _ => string.IsNullOrWhiteSpace(State) ? "未知" : State
    };
    public string CountsDisplay => $"{ItemCount} 项 · 已应用 {AppliedCount} · 冲突 {ConflictCount} · 已撤销 {UndoneCount}";
    public string DetailDisplay
    {
        get
        {
            var error = string.IsNullOrWhiteSpace(LastError) ? string.Empty : $" · {LastError}";
            var expiry = State == "Preview" ? $" · 有效至 {ExpiresLocal:MM-dd HH:mm}" : string.Empty;
            return $"{StateDisplay} · 更新于 {UpdatedLocal:MM-dd HH:mm}{expiry}{error}";
        }
    }
}

/// <summary>Paged durable media classification history.</summary>
public sealed class MediaClassificationHistoryDto
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int LoadedCount { get; set; }
    public bool HasMore { get; set; }
    public string StateFilter { get; set; } = string.Empty;
    public List<MediaClassificationBatchSummaryDto> Items { get; set; } = new List<MediaClassificationBatchSummaryDto>();

    public string LoadedDisplay => HasMore
        ? $"已加载 {LoadedCount}/{TotalCount} 个批次"
        : $"已加载全部 {LoadedCount} 个批次";
}

public sealed class MediaClassificationBatchItemResultDto
{
    public string MediaId { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string StateDisplay => State switch
    {
        "Applied" => "已应用",
        "Undone" => "已撤销",
        "Conflict" => "冲突，未改动",
        "Skipped" => "已跳过",
        _ => State
    };
}

/// <summary>Per-item outcome for apply or undo, preserving partial-failure detail.</summary>
public sealed class MediaClassificationBatchResultDto
{
    public string BatchId { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public int AppliedCount { get; set; }
    public int UndoneCount { get; set; }
    public int ConflictCount { get; set; }
    public int SkippedCount { get; set; }
    public List<MediaClassificationBatchItemResultDto> Items { get; set; } = new List<MediaClassificationBatchItemResultDto>();
    public string SummaryDisplay =>
        $"已应用 {AppliedCount}，已撤销 {UndoneCount}，冲突 {ConflictCount}，跳过 {SkippedCount}";
}
