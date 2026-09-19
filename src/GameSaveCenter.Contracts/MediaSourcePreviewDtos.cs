using System.Collections.Generic;

namespace GameSaveCenter.Contracts;

/// <summary>Bounded, read-only preview of one media source rule draft.</summary>
public sealed class MediaSourcePreviewRequestDto
{
    public string PlayniteId { get; set; } = string.Empty;
    public string RootPath { get; set; } = string.Empty;
    public string IncludePattern { get; set; } = "*";
    public bool SharedDirectory { get; set; }
    public int MaxItems { get; set; } = 120;
    public int MaxScannedEntries { get; set; } = 2000;
    public int TimeoutMs { get; set; } = 1500;
}

/// <summary>One sample file and the deterministic reason it was included or excluded.</summary>
public sealed class MediaSourcePreviewItemDto
{
    public string Path { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public bool Included { get; set; }
    public string DecisionDisplay => Included ? "命中" : "排除";
    public string Reason { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string SizeDisplay => FormatBytes(SizeBytes);

    private static string FormatBytes(long value)
    {
        if (value < 1024) return $"{value} B";
        if (value < 1024 * 1024) return $"{value / 1024d:0.#} KB";
        if (value < 1024L * 1024 * 1024) return $"{value / (1024d * 1024):0.#} MB";
        return $"{value / (1024d * 1024 * 1024):0.#} GB";
    }
}

/// <summary>Read-only source rule dry-run result. It never persists a rule or media item.</summary>
public sealed class MediaSourcePreviewDto
{
    public string RootPath { get; set; } = string.Empty;
    public string IncludePattern { get; set; } = "*";
    public string State { get; set; } = "NotRun";
    public string ErrorDisplay { get; set; } = string.Empty;
    public int ScannedCount { get; set; }
    public int MatchedCount { get; set; }
    public int ExcludedCount { get; set; }
    public bool ScanTruncated { get; set; }
    public bool TimedOut { get; set; }
    public int MaxItems { get; set; }
    public int MaxScannedEntries { get; set; }
    public int TimeoutMs { get; set; }
    public List<MediaSourcePreviewItemDto> Items { get; set; } = new List<MediaSourcePreviewItemDto>();

    public string SummaryDisplay
    {
        get
        {
            if (State == "NotRun") return "输入目录和文件模式后点击“试运行”；不会保存规则或移动文件。";
            if (State == "Unavailable") return string.IsNullOrWhiteSpace(ErrorDisplay) ? "来源目录不可访问。" : ErrorDisplay;
            var budget = TimedOut ? "已达到时间预算" : ScanTruncated ? "已达到扫描数量预算" : "已扫描完整目录";
            var suffix = string.IsNullOrWhiteSpace(ErrorDisplay) ? string.Empty : $" · {ErrorDisplay}";
            return $"扫描 {ScannedCount} 项：命中 {MatchedCount}，排除 {ExcludedCount}；{budget}{suffix}";
        }
    }
}
