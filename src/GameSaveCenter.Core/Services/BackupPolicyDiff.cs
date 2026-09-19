using System;
using System.Collections.Generic;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Core.Services;

public sealed class BackupPolicyDiffEntry
{
    public BackupPolicyDiffEntry(string fieldKey, string fieldName, string inheritedValue, string explicitValue)
    {
        FieldKey = fieldKey;
        FieldName = fieldName;
        InheritedValue = inheritedValue;
        ExplicitValue = explicitValue;
    }

    public string FieldKey { get; }
    public string FieldName { get; }
    public string InheritedValue { get; }
    public string ExplicitValue { get; }
    public string ChangeDisplay => $"{FieldName}：{InheritedValue} → {ExplicitValue}";
}

public static class BackupPolicyDiff
{
    public static IReadOnlyList<BackupPolicyDiffEntry> Compare(BackupPolicyDto? inheritedPolicy, BackupPolicyDto? explicitPolicy)
    {
        var inherited = BackupPolicyTemplateCatalog.ClonePolicy(inheritedPolicy);
        var explicitValue = BackupPolicyTemplateCatalog.ClonePolicy(explicitPolicy);
        var result = new List<BackupPolicyDiffEntry>();
        Add(result, "Enabled", "备份策略", inherited.Enabled, explicitValue.Enabled, FormatBool);
        Add(result, "BackupOnGameStop", "退出后自动备份", inherited.BackupOnGameStop, explicitValue.BackupOnGameStop, FormatBool);
        Add(result, "BackupDuringPlay", "游玩中定期备份", inherited.BackupDuringPlay, explicitValue.BackupDuringPlay, FormatBool);
        Add(result, "DuringPlayIntervalMinutes", "游玩中间隔", inherited.DuringPlayIntervalMinutes, explicitValue.DuringPlayIntervalMinutes, value => $"{value} 分钟");
        Add(result, "UploadAfterBackup", "备份后云端上传", inherited.UploadAfterBackup, explicitValue.UploadAfterBackup, FormatBool);
        Add(result, "SyncMediaDuringPlay", "游玩中媒体同步", inherited.SyncMediaDuringPlay, explicitValue.SyncMediaDuringPlay, FormatBool);
        Add(result, "SyncMediaOnGameStop", "退出后媒体同步", inherited.SyncMediaOnGameStop, explicitValue.SyncMediaOnGameStop, FormatBool);
        Add(result, "AllowAutomaticRestore", "自动恢复", inherited.AllowAutomaticRestore, explicitValue.AllowAutomaticRestore, FormatBool);
        Add(result, "AnomalyProtectionLevel", "异常保护等级", inherited.AnomalyProtectionLevel, explicitValue.AnomalyProtectionLevel, FormatProtection);
        Add(result, "KeepRecentAllHours", "近时保留", inherited.KeepRecentAllHours, explicitValue.KeepRecentAllHours, value => $"{value} 小时");
        Add(result, "KeepDailyDays", "每日保留", inherited.KeepDailyDays, explicitValue.KeepDailyDays, value => $"{value} 天");
        Add(result, "KeepWeeklyWeeks", "每周保留", inherited.KeepWeeklyWeeks, explicitValue.KeepWeeklyWeeks, value => $"{value} 周");
        Add(result, "KeepMonthlyMonths", "每月保留", inherited.KeepMonthlyMonths, explicitValue.KeepMonthlyMonths, value => $"{value} 个月");
        return result;
    }

    public static void CopyTo(BackupPolicyDto source, BackupPolicyDto target)
    {
        var normalized = BackupPolicyTemplateCatalog.ClonePolicy(source);
        target.Enabled = normalized.Enabled;
        target.BackupOnGameStop = normalized.BackupOnGameStop;
        target.BackupDuringPlay = normalized.BackupDuringPlay;
        target.DuringPlayIntervalMinutes = normalized.DuringPlayIntervalMinutes;
        target.UploadAfterBackup = normalized.UploadAfterBackup;
        target.SyncMediaDuringPlay = normalized.SyncMediaDuringPlay;
        target.SyncMediaOnGameStop = normalized.SyncMediaOnGameStop;
        target.AllowAutomaticRestore = normalized.AllowAutomaticRestore;
        target.AnomalyProtectionLevel = normalized.AnomalyProtectionLevel;
        target.KeepRecentAllHours = normalized.KeepRecentAllHours;
        target.KeepDailyDays = normalized.KeepDailyDays;
        target.KeepWeeklyWeeks = normalized.KeepWeeklyWeeks;
        target.KeepMonthlyMonths = normalized.KeepMonthlyMonths;
    }

    private static void Add<T>(ICollection<BackupPolicyDiffEntry> result, string key, string name, T inherited, T explicitValue, Func<T, string> format)
    {
        if (EqualityComparer<T>.Default.Equals(inherited, explicitValue)) return;
        result.Add(new BackupPolicyDiffEntry(key, name, format(inherited), format(explicitValue)));
    }

    private static string FormatBool(bool value) => value ? "开启" : "关闭";

    private static string FormatProtection(BackupAnomalyProtectionLevel value)
        => value switch
        {
            BackupAnomalyProtectionLevel.Strict => "严格",
            BackupAnomalyProtectionLevel.Off => "关闭",
            _ => "普通"
        };
}
