using System;
using System.Collections.Generic;

namespace GameSaveCenter.Playnite.ViewModels;

/// <summary>Builds truthful, read-only summaries for filter-empty states.</summary>
public static class FilterConditionSummary
{
    public static string GamePicker(string search, string status, string platform)
    {
        var active = new List<string>();
        Add(active, "搜索", search, quote: true);
        Add(active, "状态", status, "全部");
        Add(active, "平台", platform, "全部");
        return Format(active);
    }

    public static string Media(string search, string kind)
    {
        var active = new List<string>();
        Add(active, "搜索", search, quote: true);
        Add(active, "类型", kind, "全部");
        return Format(active);
    }

    public static string Cloud(string state, string kind, string game, string device, string time)
    {
        var active = new List<string>();
        Add(active, "状态", CloudStateDisplay(state), quote: false);
        Add(active, "类型", CloudKindDisplay(kind), quote: false);
        Add(active, "游戏", game, quote: true);
        Add(active, "来源设备", device, quote: true);
        Add(active, "时间", CloudTimeDisplay(time), quote: false);
        return Format(active);
    }

    public static string CloudEmptyState(bool loadFailed, bool hasActiveFilters, string activeSummary)
        => loadFailed
            ? "无法读取云端传输记录。\n请点击“刷新队列”重试；这不是零结果。"
            : hasActiveFilters
                ? $"暂无符合当前筛选的云端传输记录。\n{activeSummary}"
                : "暂无云端传输记录。\n刷新队列后会显示已加载的记录。";

    public static string StaleStateDetail(DateTime? lastSuccessUtc, string errorMessage)
    {
        var lastSuccess = lastSuccessUtc.HasValue
            ? $"上次成功读取：{lastSuccessUtc.Value.ToLocalTime():yyyy-MM-dd HH:mm}。"
            : string.Empty;
        return string.IsNullOrWhiteSpace(errorMessage)
            ? lastSuccess
            : lastSuccess + (lastSuccess.Length == 0 ? string.Empty : " ") + "本次刷新失败：" + errorMessage;
    }

    private static string Format(IReadOnlyList<string> active)
        => active.Count == 0 ? "当前未设置筛选条件" : "当前筛选：" + string.Join(" · ", active);

    private static void Add(List<string> active, string label, string value, string? defaultValue = null, bool quote = false)
    {
        if (string.IsNullOrWhiteSpace(value) || (defaultValue != null && string.Equals(value, defaultValue, StringComparison.Ordinal)))
            return;

        active.Add(quote ? $"{label}“{value.Trim()}”" : $"{label}：{value.Trim()}");
    }

    private static string CloudStateDisplay(string value)
        => value switch
        {
            "Pending" => "待上传",
            "RetryScheduled" => "下次尝试",
            "AuthenticationRequired" => "认证需处理",
            "Uploaded" => "已上传",
            "RemoteVerified" => "已校验",
            "CheckFailed" => "校验失败",
            "Failed" => "上传失败",
            "Paused" => "已暂停",
            _ => value ?? string.Empty
        };

    private static string CloudKindDisplay(string value)
        => value switch
        {
            "Backup" => "备份",
            "Media" => "媒体",
            _ => value ?? string.Empty
        };

    private static string CloudTimeDisplay(string value)
        => value switch
        {
            "24h" => "最近 24 小时",
            "7d" => "最近 7 天",
            "30d" => "最近 30 天",
            _ => value ?? string.Empty
        };
}
