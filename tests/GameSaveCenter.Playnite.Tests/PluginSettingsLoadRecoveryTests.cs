using System;
using System.IO;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using Newtonsoft.Json;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class PluginSettingsLoadRecoveryTests
{
    [Fact]
    public void CorruptViewPreferencesAreDiscardedWhileOtherSettingsAndOriginalFileArePreserved()
    {
        var directory = CreateTemporaryDirectory();
        try
        {
            var settingsPath = Path.Combine(directory, "config.json");
            const string json = "{\"WorkerExecutable\":\"C:\\\\GameSaveCenter\\\\Worker.exe\",\"EnableCloudUpload\":true,\"DefaultTableDensityMode\":\"Compact\",\"DataGridColumnWidths\":{\"v1/tasks/state\":{\"bad\":true}},\"ViewConfigurations\":[{\"Name\":\"Saved\",\"WorkspaceKey\":\"tasks\"}],\"ActiveViewConfigurationIds\":{\"tasks\":\"11111111111111111111111111111111\"},\"DataGridHiddenColumnKeys\":{\"v1/tasks\":[\"detail\"]}}";
            File.WriteAllText(settingsPath, json);

            var result = PluginSettingsLoadRecovery.Load(
                () => JsonConvert.DeserializeObject<GameSaveCenterSettings>(File.ReadAllText(settingsPath)),
                settingsPath);

            Assert.True(result.WasRecovered);
            Assert.True(result.CanPersistRecoveredSettings);
            Assert.NotNull(result.Exception);
            Assert.Equal("C:\\GameSaveCenter\\Worker.exe", result.Settings!.WorkerExecutable);
            Assert.True(result.Settings.EnableCloudUpload);
            Assert.Equal(TableDensityPolicy.ComfortableMode, result.Settings.DefaultTableDensityMode);
            Assert.Empty(result.Settings.ViewConfigurations);
            Assert.Empty(result.Settings.ActiveViewConfigurationIds);
            Assert.Empty(result.Settings.DataGridColumnWidths);
            Assert.Empty(result.Settings.DataGridHiddenColumnKeys);
            Assert.Contains("DataGridColumnWidths", result.Diagnostic, StringComparison.Ordinal);
            Assert.True(File.Exists(result.BackupPath));
            Assert.Equal(json, File.ReadAllText(result.BackupPath!));

            File.WriteAllText(settingsPath, JsonConvert.SerializeObject(result.Settings));
            var nextLoad = JsonConvert.DeserializeObject<GameSaveCenterSettings>(File.ReadAllText(settingsPath));
            Assert.NotNull(nextLoad);
            Assert.True(nextLoad!.EnableCloudUpload);
            Assert.Empty(nextLoad.ViewConfigurations);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void MalformedSettingsJsonFallsBackAndKeepsRecoverableBackup()
    {
        var directory = CreateTemporaryDirectory();
        try
        {
            var settingsPath = Path.Combine(directory, "config.json");
            const string json = "{\"EnableCloudUpload\":true,\"ViewConfigurations\":[";
            File.WriteAllText(settingsPath, json);

            var result = PluginSettingsLoadRecovery.Load(
                () => JsonConvert.DeserializeObject<GameSaveCenterSettings>(File.ReadAllText(settingsPath)),
                settingsPath);

            Assert.True(result.WasRecovered);
            Assert.True(result.CanPersistRecoveredSettings);
            Assert.False(result.Settings!.EnableCloudUpload);
            Assert.Empty(result.Settings.ViewConfigurations);
            Assert.Contains("malformed", result.Diagnostic, StringComparison.OrdinalIgnoreCase);
            Assert.True(File.Exists(result.BackupPath));
            Assert.Equal(json, File.ReadAllText(result.BackupPath!));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "GameSaveCenterSettingsRecoveryTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
