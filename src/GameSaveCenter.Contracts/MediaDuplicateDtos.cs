using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSaveCenter.Contracts;

/// <summary>Read-only duplicate inspection query for one selected game's assigned media.</summary>
public sealed class MediaDuplicateQueryDto
{
    public string PlayniteId { get; set; } = string.Empty;
    public int MaxGroups { get; set; } = 100;
    public int ScanLimit { get; set; } = 5000;
}

/// <summary>Bounded result for the selected game's duplicate inspection.</summary>
public sealed class MediaDuplicateInspectionDto
{
    public List<MediaDuplicateGroupDto> Groups { get; set; } = new List<MediaDuplicateGroupDto>();
    public int ScannedItemCount { get; set; }
    public bool ScanTruncated { get; set; }
    public int CertainGroupCount
        => Groups?.Count(x => string.Equals(x.Confidence, "Certain", StringComparison.OrdinalIgnoreCase)) ?? 0;
    public int SuspectedGroupCount
        => Groups?.Count(x => !string.Equals(x.Confidence, "Certain", StringComparison.OrdinalIgnoreCase)) ?? 0;
    public string SummaryDisplay
        => Groups == null || Groups.Count == 0
            ? $"{ScanDisplay} · 未发现重复组"
            : $"{ScanDisplay} · {Groups.Count} 组（确定 {CertainGroupCount} · 疑似 {SuspectedGroupCount}）";
    public string ScanDisplay => ScanTruncated
        ? $"已扫描 {ScannedItemCount} 项（达到扫描上限）"
        : $"已扫描 {ScannedItemCount} 项";
}

/// <summary>A bounded, read-only group of media with duplicate evidence.</summary>
public sealed class MediaDuplicateGroupDto
{
    public string GroupId { get; set; } = string.Empty;
    public string Confidence { get; set; } = "Suspected";
    public string Reason { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public List<MediaItemDto> Items { get; set; } = new List<MediaItemDto>();
    public bool HasHiddenItems => ItemCount > (Items?.Count ?? 0);
    public string ConfidenceDisplay => string.Equals(Confidence, "Certain", StringComparison.OrdinalIgnoreCase)
        ? "确定重复"
        : "疑似重复";
    public string ReasonDisplay => string.IsNullOrWhiteSpace(Reason) ? "暂无可核实依据" : Reason;
    public string SummaryDisplay => $"{ConfidenceDisplay} · {ItemCount} 项";
    public string ItemsDisplay => HasHiddenItems
        ? $"已显示 {Items.Count}/{ItemCount} 项"
        : $"共 {ItemCount} 项";
}
