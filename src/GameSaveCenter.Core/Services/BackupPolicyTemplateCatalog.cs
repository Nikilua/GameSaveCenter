using System;
using System.Collections.Generic;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Core.Services;

/// <summary>
/// Stable built-in policy recommendations. The catalog deliberately contains only
/// fields already supported by <see cref="BackupPolicyDto"/>.
/// </summary>
public static class BackupPolicyTemplateCatalog
{
    public const string DefaultId = "default";
    public const string ImportantId = "important";
    public const string HighFrequencyId = "high-frequency";
    public const string ExitOnlyId = "exit-only";
    public const string ManualOnlyId = "manual-only";

    public static IReadOnlyList<BackupPolicyTemplateDto> CreateBuiltIns()
        => new List<BackupPolicyTemplateDto>
        {
            Create(DefaultId, "默认", new BackupPolicyDto()),
            Create(ImportantId, "重要游戏", new BackupPolicyDto
            {
                UploadAfterBackup = true,
                AnomalyProtectionLevel = BackupAnomalyProtectionLevel.Strict,
                KeepRecentAllHours = 72,
                KeepDailyDays = 90,
                KeepWeeklyWeeks = 26,
                KeepMonthlyMonths = 36
            }),
            Create(HighFrequencyId, "高频游玩", new BackupPolicyDto
            {
                DuringPlayIntervalMinutes = 15,
                KeepRecentAllHours = 48,
                KeepDailyDays = 60,
                KeepWeeklyWeeks = 16,
                KeepMonthlyMonths = 24
            }),
            Create(ExitOnlyId, "仅退出后", new BackupPolicyDto
            {
                BackupDuringPlay = false,
                SyncMediaDuringPlay = false
            }),
            Create(ManualOnlyId, "仅手动", new BackupPolicyDto
            {
                Enabled = false,
                BackupOnGameStop = false,
                BackupDuringPlay = false,
                UploadAfterBackup = false,
                SyncMediaDuringPlay = false,
                SyncMediaOnGameStop = false
            })
        };

    public static bool IsBuiltInId(string? templateId)
        => string.Equals(templateId, DefaultId, StringComparison.OrdinalIgnoreCase)
           || string.Equals(templateId, ImportantId, StringComparison.OrdinalIgnoreCase)
           || string.Equals(templateId, HighFrequencyId, StringComparison.OrdinalIgnoreCase)
           || string.Equals(templateId, ExitOnlyId, StringComparison.OrdinalIgnoreCase)
           || string.Equals(templateId, ManualOnlyId, StringComparison.OrdinalIgnoreCase);

    public static BackupPolicyTemplateDto Clone(BackupPolicyTemplateDto template)
        => new BackupPolicyTemplateDto
        {
            TemplateId = template?.TemplateId ?? string.Empty,
            Name = template?.Name ?? string.Empty,
            IsBuiltIn = template?.IsBuiltIn ?? false,
            Policy = ClonePolicy(template?.Policy)
        };

    public static BackupPolicyDto ClonePolicy(BackupPolicyDto? policy)
    {
        policy ??= new BackupPolicyDto();
        return new BackupPolicyDto
        {
            Enabled = policy.Enabled,
            BackupOnGameStop = policy.BackupOnGameStop,
            BackupDuringPlay = policy.BackupDuringPlay,
            DuringPlayIntervalMinutes = Clamp(policy.DuringPlayIntervalMinutes, 1, 1440),
            UploadAfterBackup = policy.UploadAfterBackup,
            SyncMediaDuringPlay = policy.SyncMediaDuringPlay,
            SyncMediaOnGameStop = policy.SyncMediaOnGameStop,
            // Automatic restore remains a deliberate safety boundary in every template.
            AllowAutomaticRestore = false,
            AnomalyProtectionLevel = policy.AnomalyProtectionLevel,
            KeepRecentAllHours = Math.Max(0, policy.KeepRecentAllHours),
            KeepDailyDays = Math.Max(0, policy.KeepDailyDays),
            KeepWeeklyWeeks = Math.Max(0, policy.KeepWeeklyWeeks),
            KeepMonthlyMonths = Math.Max(0, policy.KeepMonthlyMonths)
        };
    }

    private static BackupPolicyTemplateDto Create(string id, string name, BackupPolicyDto policy)
        => new BackupPolicyTemplateDto
        {
            TemplateId = id,
            Name = name,
            IsBuiltIn = true,
            Policy = ClonePolicy(policy)
        };

    private static int Clamp(int value, int minimum, int maximum)
        => Math.Min(maximum, Math.Max(minimum, value));
}
