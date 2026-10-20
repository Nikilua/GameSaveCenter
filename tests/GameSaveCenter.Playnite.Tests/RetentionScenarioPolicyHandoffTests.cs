using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class RetentionScenarioPolicyHandoffTests
{
    [Fact]
    public void HandoffMapsOnlyRetentionWindowsIntoAnIndependentPolicyDraft()
    {
        var basePolicy = new BackupPolicyDto
        {
            Enabled = false,
            BackupOnGameStop = false,
            BackupDuringPlay = false,
            DuringPlayIntervalMinutes = 77,
            UploadAfterBackup = true,
            SyncMediaDuringPlay = false,
            SyncMediaOnGameStop = false,
            AllowAutomaticRestore = false,
            AnomalyProtectionLevel = BackupAnomalyProtectionLevel.Strict,
            KeepRecentAllHours = 1,
            KeepDailyDays = 2,
            KeepWeeklyWeeks = 3,
            KeepMonthlyMonths = 4
        };
        var scenario = new RetentionScenarioDefinition
        {
            Id = "11111111111111111111111111111111",
            Name = "长期情景",
            KeepAllForHours = 168,
            KeepDailyDays = 45,
            KeepWeeklyWeeks = 20,
            KeepMonthlyMonths = 18,
            GrowthMinimumMiBPer30Days = 250,
            GrowthMaximumMiBPer30Days = 900
        };

        var draft = RetentionScenarioPolicyHandoff.CreatePolicyDraft(scenario, basePolicy);

        Assert.NotSame(basePolicy, draft);
        Assert.Equal(168, draft.KeepRecentAllHours);
        Assert.Equal(45, draft.KeepDailyDays);
        Assert.Equal(20, draft.KeepWeeklyWeeks);
        Assert.Equal(18, draft.KeepMonthlyMonths);
        Assert.Equal(basePolicy.Enabled, draft.Enabled);
        Assert.Equal(basePolicy.BackupOnGameStop, draft.BackupOnGameStop);
        Assert.Equal(basePolicy.BackupDuringPlay, draft.BackupDuringPlay);
        Assert.Equal(basePolicy.DuringPlayIntervalMinutes, draft.DuringPlayIntervalMinutes);
        Assert.Equal(basePolicy.UploadAfterBackup, draft.UploadAfterBackup);
        Assert.Equal(basePolicy.SyncMediaDuringPlay, draft.SyncMediaDuringPlay);
        Assert.Equal(basePolicy.SyncMediaOnGameStop, draft.SyncMediaOnGameStop);
        Assert.Equal(basePolicy.AllowAutomaticRestore, draft.AllowAutomaticRestore);
        Assert.Equal(basePolicy.AnomalyProtectionLevel, draft.AnomalyProtectionLevel);
        Assert.Equal(1, basePolicy.KeepRecentAllHours);
        Assert.Equal(2, basePolicy.KeepDailyDays);
        Assert.Equal(3, basePolicy.KeepWeeklyWeeks);
        Assert.Equal(4, basePolicy.KeepMonthlyMonths);

        draft.KeepDailyDays = 99;
        Assert.Equal(45, scenario.KeepDailyDays);
        Assert.Equal(2, basePolicy.KeepDailyDays);
    }

    [Fact]
    public void HandoffRejectsInvalidSavedScenario()
    {
        var invalidScenario = new RetentionScenarioDefinition
        {
            Name = "无效方案",
            KeepDailyDays = -1,
            GrowthMinimumMiBPer30Days = 0,
            GrowthMaximumMiBPer30Days = 0
        };

        Assert.Throws<System.ArgumentException>(() =>
            RetentionScenarioPolicyHandoff.CreatePolicyDraft(invalidScenario, new BackupPolicyDto()));
    }
}
