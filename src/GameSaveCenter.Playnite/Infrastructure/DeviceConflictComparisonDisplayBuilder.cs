using System;
using System.Collections.Generic;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.Infrastructure;

/// <summary>Builds a read-only, evidence-led explanation for the two sides of a device conflict.</summary>
public static class DeviceConflictComparisonDisplayBuilder
{
    public static IReadOnlyList<string> Build(DeviceConflictStatusDto? comparison, DateTime? nowUtc = null)
    {
        if (comparison == null)
            return new[] { "选择设备对比记录后显示两侧版本与校验依据。" };

        var referenceUtc = nowUtc ?? DateTime.UtcNow;
        var lines = new List<string>
        {
            $"检测原因：{comparison.ReasonDisplay} · 检测把握 {FormatConfidence(comparison.Confidence)}（摘要启发式，不等同于恢复校验）。",
            FormatVersion("本机", comparison.LocalDevice, comparison.LocalBackupId, comparison.LocalCreatedUtc, comparison.LocalTotalBytes, comparison.LocalFileCount, referenceUtc),
            FormatVersion("远端", comparison.RemoteDevice, comparison.RemoteBackupId, comparison.RemoteCreatedUtc, comparison.RemoteTotalBytes, comparison.RemoteFileCount, referenceUtc),
            FormatParentEvidence(comparison.LocalParentBackupId, comparison.RemoteParentBackupId),
            FormatFingerprintEvidence(comparison),
            FormatReadiness("本机", comparison.LocalReadinessStatus, comparison.LocalReadinessCheckedUtc, referenceUtc),
            FormatReadiness("远端", comparison.RemoteReadinessStatus, comparison.RemoteReadinessCheckedUtc, referenceUtc),
            $"本机设备摘要发布于：{FormatTimestamp(comparison.LocalSummaryGeneratedUtc, referenceUtc)}。",
            $"远端设备摘要发布于：{FormatTimestamp(comparison.RemoteSummaryGeneratedUtc, referenceUtc)}。",
            comparison.HasConflict
                ? "分支冲突需要人工决定；系统不会因为创建时间较新就推荐覆盖另一侧。"
                : "此检测未发现需要人工决定的分叉；仍请按两侧证据确认后再恢复。"
        };
        return lines;
    }

    private static string FormatVersion(string side, string device, string backupId, DateTime createdUtc, long totalBytes, int fileCount, DateTime nowUtc)
    {
        var identity = string.IsNullOrWhiteSpace(backupId) ? "版本未知" : backupId;
        var source = string.IsNullOrWhiteSpace(device) ? "设备未知" : device;
        return $"{side}版本：{identity} · {source} · {FormatTimestamp(createdUtc, nowUtc)} · {ByteSizeFormatter.Format(totalBytes)} · {Math.Max(0, fileCount)} 个文件。";
    }

    private static string FormatParentEvidence(string localParent, string remoteParent)
    {
        if (string.IsNullOrWhiteSpace(localParent) || string.IsNullOrWhiteSpace(remoteParent))
            return "父版本关系：未知（至少一侧没有提供父版本 ID）。";
        return string.Equals(localParent, remoteParent, StringComparison.OrdinalIgnoreCase)
            ? $"共同父版本：{localParent}。"
            : $"父版本不同：本机 {localParent}；远端 {remoteParent}。未确认共同基线。";
    }

    private static string FormatFingerprintEvidence(DeviceConflictStatusDto comparison)
    {
        if (!comparison.LocalContentFingerprintAvailable || !comparison.RemoteContentFingerprintAvailable || !comparison.ContentFingerprintsMatch.HasValue)
        {
            var missing = !comparison.LocalContentFingerprintAvailable && !comparison.RemoteContentFingerprintAvailable
                ? "两侧都没有可比较的内容指纹"
                : !comparison.LocalContentFingerprintAvailable
                    ? "本机没有可比较的内容指纹"
                    : !comparison.RemoteContentFingerprintAvailable
                        ? "远端没有可比较的内容指纹"
                        : "指纹比较结果未提供";
            return $"内容指纹：未知（{missing}）。";
        }

        return comparison.ContentFingerprintsMatch.Value
            ? "内容指纹：两侧相同，摘要内容等价。"
            : "内容指纹：两侧不同，备份内容不等价。";
    }

    private static string FormatReadiness(string side, string status, DateTime? checkedUtc, DateTime nowUtc)
    {
        var displayStatus = ReadinessStatusDisplay(status);
        if (!checkedUtc.HasValue || checkedUtc.Value == default)
            return $"{side}恢复可用性校验：{displayStatus}；校验时间未知（旧设备摘要可能未包含此字段）。";

        return $"{side}恢复可用性校验：{displayStatus} · 最近 {FormatAge(checkedUtc.Value, nowUtc)}（{TimeDisplayFormatter.Full(checkedUtc.Value)}；结果仅反映该次检查）。";
    }

    private static string ReadinessStatusDisplay(string status) => status switch
    {
        "Ready" => "可恢复",
        "Warning" => "有警告",
        "Corrupted" => "疑似损坏",
        "Unsupported" => "格式不支持",
        "Failed" => "检查失败",
        "Checking" => "检查中",
        "Unknown" => "未知",
        _ => "未提供"
    };

    private static string FormatTimestamp(DateTime? value, DateTime nowUtc)
        => !value.HasValue || value.Value == default
            ? "时间未知"
            : $"{TimeDisplayFormatter.Relative(value.Value, nowUtc)}（{TimeDisplayFormatter.Full(value.Value)}）";

    private static string FormatAge(DateTime valueUtc, DateTime nowUtc)
    {
        var delta = nowUtc - valueUtc;
        if (delta.TotalDays >= 7)
            return $"{(int)delta.TotalDays} 天前";
        if (delta.TotalDays <= -7)
            return $"{(int)Math.Ceiling(-delta.TotalDays)} 天后";
        return TimeDisplayFormatter.Relative(valueUtc, nowUtc);
    }

    private static string FormatConfidence(double confidence)
    {
        var bounded = Math.Min(1d, Math.Max(0d, confidence));
        return bounded.ToString("P0", System.Globalization.CultureInfo.CurrentCulture);
    }
}
