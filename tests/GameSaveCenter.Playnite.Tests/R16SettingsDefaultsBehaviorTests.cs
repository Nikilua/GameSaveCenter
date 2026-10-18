using System.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Settings;
using Newtonsoft.Json;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R16SettingsDefaultsBehaviorTests
{
    [Fact]
    public void AllDefaultsPreserveConnectionFieldsAndCancelRestoresTheOriginalDraft()
    {
        var settings = new GameSaveCenterSettings
        {
            WorkerExecutable = "worker.exe",
            LudusaviExecutable = "ludusavi.exe",
            LudusaviBackupDirectory = "D:/saves",
            RcloneExecutable = "rclone.exe",
            RcloneDestination = "remote:profile",
            MediaArchiveDirectory = "D:/media",
            LocalMirrorPath = "E:/mirror",
            EnableCloudUpload = true,
            Compression = "none",
            GamePickerSearchText = "keep?"
        };

        settings.BeginEdit();
        settings.Compression = "bzip2";
        settings.RcloneDestination = "remote:changed";
        SettingsResetCatalog.ResetAll(settings);

        Assert.Equal("worker.exe", settings.WorkerExecutable);
        Assert.Equal("ludusavi.exe", settings.LudusaviExecutable);
        Assert.Equal("D:/saves", settings.LudusaviBackupDirectory);
        Assert.Equal("rclone.exe", settings.RcloneExecutable);
        Assert.Equal("remote:changed", settings.RcloneDestination);
        Assert.Equal("D:/media", settings.MediaArchiveDirectory);
        Assert.Equal("E:/mirror", settings.LocalMirrorPath);
        Assert.Equal("zstd", settings.Compression);
        Assert.False(settings.EnableCloudUpload);
        Assert.Equal(string.Empty, settings.GamePickerSearchText);

        settings.CancelEdit();

        Assert.Equal("none", settings.Compression);
        Assert.Equal("remote:profile", settings.RcloneDestination);
        Assert.True(settings.EnableCloudUpload);
        Assert.Equal("keep?", settings.GamePickerSearchText);
    }

    [Fact]
    public void FieldCatalogExcludesSensitivePathsAndExposesEachResetScope()
    {
        Assert.NotEmpty(SettingsResetCatalog.Fields);
        Assert.Contains(SettingsResetCatalog.Fields, field => field.Category == SettingsResetCategory.General);
        Assert.Contains(SettingsResetCatalog.Fields, field => field.Category == SettingsResetCategory.BackupRestore);
        Assert.Contains(SettingsResetCatalog.Fields, field => field.Category == SettingsResetCategory.Appearance);
        Assert.Contains(SettingsResetCatalog.Fields, field => field.Category == SettingsResetCategory.AutomationMedia);
        Assert.DoesNotContain(SettingsResetCatalog.Fields, field => field.Key == "WorkerExecutable"
            || field.Key == "LudusaviExecutable"
            || field.Key == "LudusaviBackupDirectory"
            || field.Key == "RcloneExecutable"
            || field.Key == "RcloneDestination"
            || field.Key == "MediaArchiveDirectory"
            || field.Key == "LocalMirrorPath");
        Assert.Contains("不会清除", SettingsResetCatalog.BuildAllImpact());
    }

    [Fact]
    public void TableDensityDefaultPersistsNormalizesCopiesAndResetsWithAppearance()
    {
        var settings = new GameSaveCenterSettings();
        Assert.Equal("Comfortable", settings.DefaultTableDensityMode);

        settings.DefaultTableDensityMode = " compact ";
        Assert.Equal("Compact", settings.DefaultTableDensityMode);

        var restored = JsonConvert.DeserializeObject<GameSaveCenterSettings>(JsonConvert.SerializeObject(settings))!;
        Assert.Equal("Compact", restored.DefaultTableDensityMode);

        restored.BeginEdit();
        restored.DefaultTableDensityMode = "Comfortable";
        restored.CancelEdit();
        Assert.Equal("Compact", restored.DefaultTableDensityMode);

        Assert.True(SettingsResetCatalog.ResetField(restored, "DefaultTableDensityMode"));
        Assert.Equal("Comfortable", restored.DefaultTableDensityMode);
        Assert.Contains(SettingsResetCatalog.Fields, field =>
            field.Key == "DefaultTableDensityMode"
            && field.Category == SettingsResetCategory.Appearance
            && field.DefaultDisplay == "舒适");
    }
}
