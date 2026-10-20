using System;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Services;
using GameSaveCenter.Playnite.Settings;

namespace GameSaveCenter.Playnite.ViewModels;

internal static class RetentionScenarioPolicyHandoff
{
    public static BackupPolicyDto CreatePolicyDraft(RetentionScenarioDefinition scenario, BackupPolicyDto? basePolicy)
    {
        var normalized = RetentionScenarioDefinition.Normalize(scenario)
            ?? throw new ArgumentException("保留情景无效，无法转交策略模板。", nameof(scenario));
        var policy = BackupPolicyTemplateCatalog.ClonePolicy(basePolicy);
        policy.KeepRecentAllHours = normalized.KeepAllForHours;
        policy.KeepDailyDays = normalized.KeepDailyDays;
        policy.KeepWeeklyWeeks = normalized.KeepWeeklyWeeks;
        policy.KeepMonthlyMonths = normalized.KeepMonthlyMonths;
        return policy;
    }
}
