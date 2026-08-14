using System;
using System.IO;
using System.Threading.Tasks;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Services;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class MetadataRestoreCoordinatorTests
{
    [Fact]
    public async Task PluginSaveFailureRestoresOldSettingsAndRollsBackMetadata()
    {
        var settings = new GameSaveCenterSettings();
        settings.GamePickerStatusFilter = "old";
        var oldJson = settings.ExportPortableJson();
        var newJson = BuildPortableSettingsJson("new");
        var saveCalls = 0;
        var rollbackCalled = false;

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            MetadataRestoreCoordinator.ApplyPluginSettingsAsync(
                settings,
                () =>
                {
                    saveCalls++;
                    if (saveCalls == 1) throw new IOException("plugin save failed");
                },
                () => { },
                () => Task.CompletedTask,
                newJson,
                oldJson,
                () =>
                {
                    rollbackCalled = true;
                    return Task.FromResult(new MetadataRestoreRollbackResultDto
                    {
                        RolledBack = true,
                        Summary = "worker rollback ok"
                    });
                }));

        Assert.True(rollbackCalled);
        Assert.Equal(2, saveCalls);
        Assert.Equal("old", settings.GamePickerStatusFilter);
        Assert.Contains("整体回滚", ex.Message);
    }

    [Fact]
    public async Task RollbackFailureReportsManualIntervention()
    {
        var settings = new GameSaveCenterSettings();
        settings.GamePickerStatusFilter = "old";
        var oldJson = settings.ExportPortableJson();
        var newJson = BuildPortableSettingsJson("new");
        var saveCalls = 0;

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            MetadataRestoreCoordinator.ApplyPluginSettingsAsync(
                settings,
                () =>
                {
                    saveCalls++;
                    if (saveCalls == 1) throw new IOException("plugin save failed");
                },
                () => { },
                () => Task.CompletedTask,
                newJson,
                oldJson,
                () => throw new InvalidOperationException("worker rollback failed")));

        Assert.Contains("需要人工介入", ex.Message);
        Assert.Equal("old", settings.GamePickerStatusFilter);
    }

    [Fact]
    public async Task SuccessfulApplyDoesNotCallRollback()
    {
        var settings = new GameSaveCenterSettings();
        settings.GamePickerStatusFilter = "old";
        var oldJson = settings.ExportPortableJson();
        var newJson = BuildPortableSettingsJson("new");
        var rollbackCalled = false;

        var report = await MetadataRestoreCoordinator.ApplyPluginSettingsAsync(
            settings,
            () => { },
            () => { },
            () => Task.CompletedTask,
            newJson,
            oldJson,
            () =>
            {
                rollbackCalled = true;
                return Task.FromResult(new MetadataRestoreRollbackResultDto());
            });

        Assert.False(rollbackCalled);
        Assert.Equal("new", settings.GamePickerStatusFilter);
        Assert.NotNull(report);
    }

    private static string BuildPortableSettingsJson(string filter)
        => $@"{{
  ""SchemaVersion"": 1,
  ""ExportedUtc"": ""2026-08-14T00:00:00Z"",
  ""Settings"": {{
    ""DefaultBackupIntervalMinutes"": 30,
    ""ProcessPollingSeconds"": 5,
    ""DashboardRefreshSeconds"": 10,
    ""RecentProtectionWindowDays"": 30,
    ""GlassEffectStrength"": 78,
    ""FullBackupLimit"": 3,
    ""DifferentialBackupLimit"": 5,
    ""CompressionLevel"": 3,
    ""ThemeMode"": 0,
    ""BackupFormat"": 0,
    ""NotificationLevel"": 1,
    ""GamePickerStatusFilter"": ""{filter}""
  }}
}}";
}
