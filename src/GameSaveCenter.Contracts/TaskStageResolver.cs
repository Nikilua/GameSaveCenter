using System;

namespace GameSaveCenter.Contracts;

/// <summary>Maps the Worker-reported task message to a small, shared stage vocabulary.</summary>
public static class TaskStageResolver
{
    public const string Unknown = "Unknown";
    public const string Queued = "Queued";
    public const string Preparing = "Preparing";
    public const string Scanning = "Scanning";
    public const string Verifying = "Verifying";
    public const string Indexing = "Indexing";
    public const string Uploading = "Uploading";
    public const string Downloading = "Downloading";
    public const string Restoring = "Restoring";
    public const string Coordinating = "Coordinating";
    public const string Protecting = "Protecting";
    public const string Previewing = "Previewing";
    public const string Processing = "Processing";
    public const string Cleaning = "Cleaning";
    public const string Completed = "Completed";

    public static string ResolveKey(string taskType, string? stageMessage)
    {
        var message = stageMessage ?? string.Empty;
        if (string.IsNullOrWhiteSpace(message)) return Unknown;
        if (Has(message, "等待执行") || Has(message, "等待队列")) return Queued;
        if (Has(message, "已完成")
            || Has(message, "完成；")
            || message.EndsWith("完成", StringComparison.Ordinal)) return Completed;
        if (Has(message, "扫描") || Has(message, "查找游戏专属媒体来源") || Has(message, "公共截图与录像目录")) return Scanning;
        if (Has(message, "校验") || Has(message, "一致性")) return Verifying;
        if (Has(message, "索引")) return Indexing;
        if (Has(message, "复制到云端") || Has(message, "复制媒体到云端") || Has(message, "重新复制本地备份到云端")) return Uploading;
        if (Has(message, "确认游戏已关闭") || Has(message, "准备")) return Preparing;
        if (Has(message, "PreRestore") || Has(message, "保护快照")) return Protecting;
        if (Has(message, "预览目标")) return Previewing;
        if (Has(message, "等待现有云端传输")) return Coordinating;
        if (Has(message, "正在恢复") || Has(message, "恢复指定版本")) return Restoring;
        if (Has(message, "下载")) return Downloading;
        if (Has(message, "确认目标 Ludusavi 版本") || Has(message, "写入隔离清单")) return Verifying;
        if (Has(message, "正在处理整库备份") || Has(message, "已记录结果") || Has(message, "已检查")) return Processing;
        if (Has(message, "解压") || Has(message, "清理")) return Cleaning;

        return Unknown;
    }

    private static bool Has(string value, string part)
        => value.IndexOf(part, StringComparison.Ordinal) >= 0;

    public static string GetDisplay(string key)
        => key switch
        {
            Queued => "排队等待",
            Preparing => "准备中",
            Scanning => "扫描中",
            Verifying => "校验中",
            Indexing => "整理历史版本",
            Uploading => "上传中",
            Downloading => "下载中",
            Restoring => "恢复中",
            Coordinating => "协调传输",
            Protecting => "创建保护快照",
            Previewing => "预览目标",
            Processing => "处理中",
            Cleaning => "清理中",
            Completed => "已完成",
            _ => "阶段未知"
        };
}
