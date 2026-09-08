using System;
using System.IO;
using System.Linq;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class SettingsPathValidationTests
{
    [Fact]
    public void VerifySettingsRejectsDirectorySettingThatPointsToAFile()
    {
        var root = Path.Combine(Path.GetTempPath(), "gsc-settings-path-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var worker = Path.Combine(root, "GameSaveCenter.Worker.exe");
            var backupMarker = Path.Combine(root, "backup-marker.dat");
            File.WriteAllText(worker, "worker");
            File.WriteAllText(backupMarker, "not a directory");
            var settings = CreateValidSettings(root);
            settings.LudusaviBackupDirectory = backupMarker;

            Assert.False(settings.VerifySettings(out var errors));
            var joinedErrors = string.Join("；", errors);
            Assert.Contains("存档目录", joinedErrors);
            Assert.Contains("文件", joinedErrors);
        }
        finally
        {
            try { Directory.Delete(root, true); } catch { }
        }
    }

    [Fact]
    public void VerifySettingsAllowsMissingChildDirectoryWhenItsDriveIsAvailable()
    {
        var root = Path.Combine(Path.GetTempPath(), "gsc-settings-path-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            File.WriteAllText(Path.Combine(root, "GameSaveCenter.Worker.exe"), "worker");
            var settings = CreateValidSettings(root);
            settings.LudusaviBackupDirectory = Path.Combine(root, "Saves", "Unicode-存档");
            settings.MediaArchiveDirectory = Path.Combine(root, "Media", "长文件名-媒体");

            Assert.True(settings.VerifySettings(out var errors), string.Join("；", errors));
            Assert.DoesNotContain("目录", string.Join("；", errors));
            Assert.False(Directory.Exists(settings.LudusaviBackupDirectory));
            Assert.False(Directory.Exists(settings.MediaArchiveDirectory));
        }
        finally
        {
            try { Directory.Delete(root, true); } catch { }
        }
    }

    private static GameSaveCenterSettings CreateValidSettings(string root) => new()
    {
        WorkerExecutable = Path.Combine(root, "GameSaveCenter.Worker.exe"),
        LudusaviBackupDirectory = Path.Combine(root, "Saves"),
        MediaArchiveDirectory = Path.Combine(root, "Media")
    };
}
