using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GameSaveCenter.Contracts;

/// <summary>Requests conservative suggestions for explicit inbox media items.</summary>
public sealed class MediaClassificationPreviewRequestDto
{
    public string RequestId { get; set; } = string.Empty;
    public List<string> MediaIds { get; set; } = new List<string>();
    public string SessionId { get; set; } = string.Empty;
    public int Limit { get; set; } = 200;
}

/// <summary>One concrete local signal used to explain a classification suggestion.</summary>
public sealed class MediaClassificationEvidenceDto
{
    public string Kind { get; set; } = string.Empty;
    public string CandidatePlayniteId { get; set; } = string.Empty;
    public string CandidateGameName { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;

    public string KindDisplay => Kind switch
    {
        "SourceRule" => "来源规则",
        "GameSession" => "游戏会话",
        "ProcessMapping" => "进程映射",
        "FileName" => "文件名",
        _ => "其他本地依据"
    };

    public string SummaryDisplay
    {
        get
        {
            var detail = string.IsNullOrWhiteSpace(Detail) ? "已命中" : Detail;
            return string.IsNullOrWhiteSpace(CandidateGameName)
                ? $"{KindDisplay} · {detail}"
                : $"{CandidateGameName} · {KindDisplay} · {detail}";
        }
    }
}

/// <summary>One explainable game suggestion. Low-confidence items have no target.</summary>
public sealed class MediaClassificationSuggestionDto : INotifyPropertyChanged
{
    private bool isIncluded = true;
    private string targetPlayniteIdOverride = string.Empty;
    private bool targetOverrideSet;

    public string MediaId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime CapturedUtc { get; set; }
    public string SuggestedPlayniteId { get; set; } = string.Empty;
    public string SuggestedGameName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Confidence { get; set; } = "Low";
    public string State { get; set; } = "Suggested";
    public List<MediaClassificationEvidenceDto> Evidence { get; set; } = new List<MediaClassificationEvidenceDto>();
    public bool IsIncluded
    {
        get => isIncluded;
        set
        {
            if (isIncluded == value) return;
            isIncluded = value;
            OnPropertyChanged(nameof(IsIncluded));
            OnPropertyChanged(nameof(CanApply));
            OnPropertyChanged(nameof(StateDisplay));
        }
    }

    /// <summary>Effective target selected in the still-local preview.</summary>
    public string TargetPlayniteId
    {
        get => targetOverrideSet ? targetPlayniteIdOverride : SuggestedPlayniteId;
        set
        {
            var normalized = value ?? string.Empty;
            var nextOverride = !string.Equals(normalized, SuggestedPlayniteId, StringComparison.OrdinalIgnoreCase);
            if (targetOverrideSet == nextOverride
                && string.Equals(targetPlayniteIdOverride, normalized, StringComparison.OrdinalIgnoreCase)) return;
            targetOverrideSet = nextOverride;
            targetPlayniteIdOverride = normalized;
            OnPropertyChanged(nameof(TargetPlayniteId));
            OnPropertyChanged(nameof(IsTargetOverridden));
            OnPropertyChanged(nameof(TargetSelectionDisplay));
            OnPropertyChanged(nameof(CanApply));
        }
    }

    public bool IsTargetOverridden => targetOverrideSet;
    public bool CanEditTarget => Confidence == "High" && !string.IsNullOrWhiteSpace(SuggestedPlayniteId);

    public DateTime CapturedLocal => CapturedUtc.ToLocalTime();
    public bool CanApply => IsIncluded && !string.IsNullOrWhiteSpace(TargetPlayniteId) && Confidence == "High";
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
    public bool HasEvidence => Evidence != null && Evidence.Count > 0;
    public string EvidenceSummaryDisplay => HasEvidence
        ? $"依据 {Evidence.Count} 条"
        : "待判断 · 尚无可核实依据";
    public string TargetSelectionDisplay => string.IsNullOrWhiteSpace(TargetPlayniteId)
        ? "未选择目标"
        : IsTargetOverridden ? $"已调整目标 · {TargetPlayniteId}" : "使用建议目标";

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
    public int SelectedCount => Items?.Count(x => x.IsIncluded) ?? 0;
    public int ExcludedCount => Items?.Count(x => !x.IsIncluded) ?? 0;
    public int SelectedHighConfidenceCount => Items?.Count(x => x.CanApply) ?? 0;
    public string SummaryDisplay =>
        $"建议 {Items.Count} 项：高置信 {HighConfidenceCount}，中置信 {MediumConfidenceCount}，低置信 {LowConfidenceCount}；仅高置信可批量确认。";
    public string SelectionSummaryDisplay =>
        $"本次纳入 {SelectedCount} 项，可应用高置信 {SelectedHighConfidenceCount} 项，排除 {ExcludedCount} 项。";
}

/// <summary>Explicit per-item target selected in the still-valid preview.</summary>
public sealed class MediaClassificationTargetOverrideDto
{
    public string MediaId { get; set; } = string.Empty;
    public string TargetPlayniteId { get; set; } = string.Empty;
}

/// <summary>Confirms selected suggestions from one still-valid preview.</summary>
public sealed class MediaClassificationApplyRequestDto
{
    public string RequestId { get; set; } = string.Empty;
    public string BatchId { get; set; } = string.Empty;
    public List<string> MediaIds { get; set; } = new List<string>();
    public List<MediaClassificationTargetOverrideDto> TargetOverrides { get; set; } = new List<MediaClassificationTargetOverrideDto>();
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
    /// <summary>Opaque Worker-owned revision returned by the preceding page.</summary>
    public string ConsistencyToken { get; set; } = string.Empty;
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
        _ => "未知状态"
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
    public string ConsistencyToken { get; set; } = string.Empty;
    public bool PageResetRequired { get; set; }
    public string PageResetReason { get; set; } = string.Empty;
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
        _ => "未知状态"
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
