using System;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Settings;

namespace GameSaveCenter.Playnite.ViewModels;

public sealed class WorklistContinuationTarget
{
    public WorklistContinuationTarget(WorklistObjectKind kind, string stableObjectId, string playniteId, string objectId)
    {
        Kind = kind;
        StableObjectId = stableObjectId;
        PlayniteId = playniteId;
        ObjectId = objectId;
    }

    public WorklistObjectKind Kind { get; }
    public string StableObjectId { get; }
    public string PlayniteId { get; }
    public string ObjectId { get; }
}

/// <summary>Builds safe, stable-ID routes from a refreshed worklist entry.</summary>
public static class WorklistContinuationResolver
{
    public static bool TryResolve(
        WorklistBrowserEntry? entry,
        out WorklistContinuationTarget? target,
        out string message)
    {
        target = null;
        if (entry == null)
        {
            message = "请先从待办清单选择一项。";
            return false;
        }

        switch (entry.State)
        {
            case WorklistReferenceState.NotFound:
                message = $"待办引用“{entry.StableObjectId}”已不在当前索引中；原业务对象未被删除，可从清单移除。";
                return false;
            case WorklistReferenceState.InvalidReference:
                message = "该待办编号格式无效，无法续接；可以从清单移除。";
                return false;
            case WorklistReferenceState.Unavailable:
                message = "Worker 暂时无法检查该待办；请刷新引用状态后再试。";
                return false;
            case WorklistReferenceState.NotChecked:
                message = "该待办尚未刷新引用状态；请先刷新状态再续接。";
                return false;
        }

        var stableId = entry.StableObjectId.Trim();
        if (stableId.Length == 0 || stableId.Length > WorklistDefinition.MaximumStableObjectIdLength)
        {
            message = "该待办缺少有效的稳定编号，无法续接；可以从清单移除。";
            return false;
        }

        switch (entry.Kind)
        {
            case WorklistObjectKind.BackupVersion:
                if (!TryParseBackupReference(stableId, out var gameId, out var backupId))
                {
                    message = "该存档版本引用无法解析，未按名称或路径猜测目标；可以从清单移除。";
                    return false;
                }
                if (!string.IsNullOrWhiteSpace(entry.TargetPlayniteId)
                    && !string.Equals(entry.TargetPlayniteId, gameId, StringComparison.OrdinalIgnoreCase))
                {
                    message = "存档版本引用的游戏编号与索引结果不一致，未跳转。";
                    return false;
                }
                target = new WorklistContinuationTarget(entry.Kind, stableId, gameId, backupId);
                message = string.Empty;
                return true;

            case WorklistObjectKind.MediaItem:
                if (string.IsNullOrWhiteSpace(entry.TargetPlayniteId))
                {
                    message = "该媒体仍有索引记录，但没有可验证的所属游戏，当前无法打开游戏媒体页；待办仍可移除。";
                    return false;
                }
                target = new WorklistContinuationTarget(entry.Kind, stableId, entry.TargetPlayniteId, stableId);
                message = string.Empty;
                return true;

            case WorklistObjectKind.BackgroundTask:
                target = new WorklistContinuationTarget(entry.Kind, stableId, entry.TargetPlayniteId, stableId);
                message = string.Empty;
                return true;

            case WorklistObjectKind.ValidationFinding:
                target = new WorklistContinuationTarget(entry.Kind, stableId, entry.TargetPlayniteId, stableId);
                message = string.Empty;
                return true;

            default:
                message = "该待办对象类型不受支持，未执行跳转；可以从清单移除。";
                return false;
        }
    }

    private static bool TryParseBackupReference(string value, out string playniteId, out string backupId)
    {
        playniteId = string.Empty;
        backupId = string.Empty;
        if (!value.StartsWith("v1|", StringComparison.Ordinal)) return false;
        var cursor = 3;
        if (!TryReadLengthPrefixedPart(value, ref cursor, out playniteId)
            || cursor >= value.Length
            || value[cursor++] != '|'
            || !TryReadLengthPrefixedPart(value, ref cursor, out backupId))
            return false;
        return cursor == value.Length && playniteId.Length > 0 && backupId.Length > 0;
    }

    private static bool TryReadLengthPrefixedPart(string value, ref int cursor, out string part)
    {
        part = string.Empty;
        var colon = value.IndexOf(':', cursor);
        if (colon < 0
            || !int.TryParse(value.Substring(cursor, colon - cursor), System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out var length)
            || length <= 0)
            return false;

        cursor = colon + 1;
        if (length > value.Length - cursor) return false;
        part = value.Substring(cursor, length);
        cursor += length;
        return true;
    }
}
