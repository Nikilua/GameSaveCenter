using System;
using System.IO;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Settings;
using Newtonsoft.Json.Linq;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class PortableSettingsTests
    {
        [Fact]
        public void ExportImport_RoundTripsNonSecretSettings()
        {
            var source = CreateSettings();
            var json = source.ExportPortableJson();
            var package = JObject.Parse(json);

            Assert.Equal(1, package["SchemaVersion"]!.Value<int>());
            Assert.NotNull(package["ExportedUtc"]);
            Assert.Null(package.SelectToken("Settings.RclonePassword"));

            var imported = new GameSaveCenterSettings();
            var destinationDeviceId = imported.DeviceId;
            var report = imported.ImportPortableJson(json);

            Assert.Equal(1, report.SchemaVersion);
            Assert.Equal(source.WorkerExecutable, imported.WorkerExecutable);
            Assert.Equal(source.RcloneDestination, imported.RcloneDestination);
            Assert.Equal(source.ThemeMode, imported.ThemeMode);
            Assert.Equal(source.FollowSelectedGameBackground, imported.FollowSelectedGameBackground);
            Assert.Equal(source.BackupFormat, imported.BackupFormat);
            Assert.Equal(source.CompressionLevel, imported.CompressionLevel);
            Assert.Equal(source.DifferentialBackupLimit, imported.DifferentialBackupLimit);
            Assert.Equal(source.HealthInspectionEnabled, imported.HealthInspectionEnabled);
            Assert.Equal(source.HealthInspectionIntervalMinutes, imported.HealthInspectionIntervalMinutes);
            Assert.Equal(source.HealthInspectionStaleAfterDays, imported.HealthInspectionStaleAfterDays);
            Assert.Equal(source.CloudUploadQueuePaused, imported.CloudUploadQueuePaused);
            Assert.Equal(source.CloudUploadAllowedStartMinute, imported.CloudUploadAllowedStartMinute);
            Assert.Equal(source.CloudUploadAllowedEndMinute, imported.CloudUploadAllowedEndMinute);
            Assert.Equal(source.RecentProtectionWindowDays, imported.RecentProtectionWindowDays);
            Assert.Equal(source.EnableXboxGameBarMedia, imported.EnableXboxGameBarMedia);
            Assert.Equal(source.EnableCustomMedia, imported.EnableCustomMedia);
            Assert.Equal(source.EnableLocalMirror, imported.EnableLocalMirror);
            Assert.Equal(source.LocalMirrorPath, imported.LocalMirrorPath);
            Assert.Equal(source.LastWorkspace, imported.LastWorkspace);
            Assert.Equal(source.TaskStatusFilterState, imported.TaskStatusFilterState);
            Assert.Equal(source.TaskGameFilterState, imported.TaskGameFilterState);
            Assert.Equal(source.TaskTypeFilterState, imported.TaskTypeFilterState);
            Assert.Equal(source.TaskSearchTextState, imported.TaskSearchTextState);
            Assert.Equal(source.MediaFilterState, imported.MediaFilterState);
            Assert.Equal(source.MediaSearchTextState, imported.MediaSearchTextState);
            Assert.Equal(source.OnboardingCompleted, imported.OnboardingCompleted);
            Assert.Equal(source.NotificationLevel, imported.NotificationLevel);
            Assert.Equal(source.SafeModeEnabled, imported.SafeModeEnabled);
            Assert.Equal(source.SafeModeRequested, imported.SafeModeRequested);
            Assert.Equal(destinationDeviceId, imported.DeviceId);
            Assert.NotEqual(source.DeviceId, imported.DeviceId);
        }

        [Fact]
        public void Import_LegacyPackageUsesDefaultsForNewFields()
        {
            const string json = @"{
  ""SchemaVersion"": 1,
  ""ExportedUtc"": ""2026-07-01T00:00:00Z"",
  ""Settings"": {
    ""WorkerExecutable"": ""C:\\Portable\\GameSaveCenter.Worker.exe"",
    ""LudusaviBackupDirectory"": ""C:\\Portable\\Saves"",
    ""MediaArchiveDirectory"": ""C:\\Portable\\Media""
  }
}";

            var imported = new GameSaveCenterSettings();
            var destinationDeviceId = imported.DeviceId;
            imported.ImportPortableJson(json);

            Assert.True(imported.EnableUiAnimations);
            Assert.True(imported.EnableGlassEffects);
            Assert.True(imported.FollowSelectedGameBackground);
            Assert.Equal(78, imported.GlassEffectStrength);
            Assert.Equal(10, imported.DashboardRefreshSeconds);
            Assert.Equal(30, imported.RecentProtectionWindowDays);
            Assert.Equal(BackupStorageFormat.Zip, imported.BackupFormat);
            Assert.Equal(NotificationLevel.Summary, imported.NotificationLevel);
            Assert.False(imported.SafeModeEnabled);
            Assert.Equal("zstd", imported.Compression);
            Assert.True(imported.HealthInspectionEnabled);
            Assert.Equal(1440, imported.HealthInspectionIntervalMinutes);
            Assert.Equal(30, imported.HealthInspectionStaleAfterDays);
            Assert.False(imported.CloudUploadQueuePaused);
            Assert.Equal(0, imported.CloudUploadAllowedStartMinute);
            Assert.Equal(1440, imported.CloudUploadAllowedEndMinute);
            Assert.True(imported.EnableSteamMedia);
            Assert.True(imported.EnableXboxGameBarMedia);
            Assert.True(imported.EnableWindowsScreenshotMedia);
            Assert.True(imported.EnablePlatformAdjacentMedia);
            Assert.True(imported.EnableCustomMedia);
            Assert.False(imported.EnableLocalMirror);
            Assert.Equal(string.Empty, imported.LocalMirrorPath);
            Assert.Equal("Overview", imported.LastWorkspace);
            Assert.Equal("全部", imported.TaskStatusFilterState);
            Assert.Equal("全部", imported.TaskGameFilterState);
            Assert.Equal("全部", imported.TaskTypeFilterState);
            Assert.Equal(string.Empty, imported.TaskSearchTextState);
            Assert.Equal("全部", imported.MediaFilterState);
            Assert.Equal(string.Empty, imported.MediaSearchTextState);
            Assert.Equal(destinationDeviceId, imported.DeviceId);
        }

        [Fact]
        public void DeviceIdentityRoundTripsInInstalledSettingsButNotPortableTransfer()
        {
            var source = CreateSettings();
            var installedJson = Newtonsoft.Json.JsonConvert.SerializeObject(source);
            var reloaded = Newtonsoft.Json.JsonConvert.DeserializeObject<GameSaveCenterSettings>(installedJson)!;

            Assert.Equal(source.DeviceId, reloaded.DeviceId);

            var destination = new GameSaveCenterSettings();
            var destinationId = destination.DeviceId;
            destination.ImportPortableJson(source.ExportPortableJson());
            Assert.Equal(destinationId, destination.DeviceId);
        }

        [Fact]
        public void Import_InvalidValuesDoesNotMutateCurrentSettings()
        {
            var settings = CreateSettings();
            var originalWorker = settings.WorkerExecutable;
            var originalInterval = settings.DefaultBackupIntervalMinutes;
            var package = JObject.Parse(settings.ExportPortableJson());
            package.SelectToken("Settings.DefaultBackupIntervalMinutes")!.Replace(0);

            Assert.Throws<InvalidDataException>(() => settings.ImportPortableJson(package.ToString()));
            Assert.Equal(originalWorker, settings.WorkerExecutable);
            Assert.Equal(originalInterval, settings.DefaultBackupIntervalMinutes);
        }

        [Fact]
        public void Import_RejectsUnknownSchemaEnumAndOversizedInput()
        {
            var settings = CreateSettings();
            var package = JObject.Parse(settings.ExportPortableJson());
            package["SchemaVersion"] = 2;
            Assert.Throws<InvalidDataException>(() => settings.ImportPortableJson(package.ToString()));

            package = JObject.Parse(settings.ExportPortableJson());
            package.SelectToken("Settings.ThemeMode")!.Replace(999);
            Assert.Throws<InvalidDataException>(() => settings.ImportPortableJson(package.ToString()));

            Assert.Throws<InvalidDataException>(() => settings.ImportPortableJson(new string('x', 1024 * 1024 + 1)));
        }

        [Fact]
        public void Import_ReportsMissingProgramsAndDirectoriesWithoutCreatingThem()
        {
            var root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Settings.Tests", Guid.NewGuid().ToString("N"));
            var settings = CreateSettings();
            settings.WorkerExecutable = Path.Combine(root, "GameSaveCenter.Worker.exe");
            settings.LudusaviExecutable = Path.Combine(root, "ludusavi.exe");
            settings.RcloneExecutable = Path.Combine(root, "rclone.exe");
            settings.LudusaviBackupDirectory = Path.Combine(root, "Saves");
            settings.MediaArchiveDirectory = Path.Combine(root, "Media");
            settings.EnableLocalMirror = true;
            settings.LocalMirrorPath = Path.Combine(root, "Mirror");

            var imported = new GameSaveCenterSettings();
            var report = imported.ImportPortableJson(settings.ExportPortableJson());

            Assert.Equal(6, report.MissingPaths.Count);
            Assert.False(Directory.Exists(root));
        }

        private static GameSaveCenterSettings CreateSettings() => new GameSaveCenterSettings
        {
            WorkerExecutable = @"C:\Tools\GameSaveCenter.Worker.exe",
            LudusaviExecutable = @"C:\Tools\ludusavi.exe",
            LudusaviBackupDirectory = @"D:\Backups\Saves",
            RcloneExecutable = @"C:\Tools\rclone.exe",
            RcloneDestination = "encrypted-remote:GameSaveCenter",
            MediaArchiveDirectory = @"D:\Backups\Media",
            EnableLocalMirror = true,
            LocalMirrorPath = @"H:\GameSaveCenter-Mirror",
            LastWorkspace = "Saves",
            TaskStatusFilterState = "失败",
            TaskGameFilterState = "Game One",
            TaskTypeFilterState = "存档备份",
            TaskSearchTextState = "ff",
            MediaFilterState = "收藏",
            MediaSearchTextState = "shot",
            AutoStartWorker = false,
            EnableProcessDetection = true,
            EnableSessionSavePathDetection = false,
            EnableMediaSync = true,
            EnableSteamMedia = false,
            EnableXboxGameBarMedia = true,
            EnableWindowsScreenshotMedia = false,
            EnablePlatformAdjacentMedia = true,
            EnableCustomMedia = false,
            EnableCloudUpload = true,
            EnableDashboardAutoRefresh = false,
            EnableTaskNotifications = true,
            NotificationLevel = NotificationLevel.Verbose,
            SafeModeEnabled = true,
            SafeModeRequested = true,
            OnboardingCompleted = true,
            ThemeMode = GameSaveCenterThemeMode.Dark,
            EnableUiAnimations = false,
            EnableGlassEffects = false,
            GlassEffectStrength = 64,
            FollowSelectedGameBackground = false,
            DashboardRefreshSeconds = 30,
            RecentProtectionWindowDays = 90,
            ProcessPollingSeconds = 9,
            DefaultBackupIntervalMinutes = 45,
            BackupFormat = BackupStorageFormat.Zip,
            Compression = "zstd",
            CompressionLevel = 8,
            FullBackupLimit = 7,
            DifferentialBackupLimit = 11,
            HealthInspectionEnabled = false,
            HealthInspectionIntervalMinutes = 720,
            HealthInspectionStaleAfterDays = 14,
            CloudUploadQueuePaused = true,
            CloudUploadAllowedStartMinute = 1320,
            CloudUploadAllowedEndMinute = 120
        };
    }
}
