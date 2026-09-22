using System;
using System.IO;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class QuickActionSourceTests
{
    [Fact]
    public void PluginExposesGameMenuQuickActions()
    {
        var root = FindRepositoryRoot();
        var plugin = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "GameSaveCenterPlugin.cs"));

        Assert.Contains("GetGameMenuItems(GetGameMenuItemsArgs args)", plugin);
        Assert.Contains("MenuSection = \"GameSaveCenter\"", plugin);
        Assert.Contains("Description = \"立即备份\"", plugin);
        Assert.Contains("Description = \"同步媒体\"", plugin);
        Assert.Contains("SyncMediaFromQuickActionAsync(context)", plugin);
        Assert.Contains("Description = \"查看备份历史\"", plugin);
        Assert.Contains("Description = \"验证最新恢复点\"", plugin);
        Assert.Contains("Description = \"游戏工具\"", plugin);
        Assert.Contains("MessageTypes.BackupGame", plugin);
        Assert.Contains("MessageTypes.SyncMedia", plugin);
        Assert.Contains("UploadAfterSync = Settings.EnableCloudUpload", plugin);
        Assert.Contains("if (!Settings.EnableMediaSync)", plugin);
        Assert.Contains("MessageTypes.ListBackups", plugin);
        Assert.Contains("MessageTypes.ValidateRestoreReadiness", plugin);
        Assert.Contains("MessageTypes.ListGameTools", plugin);
    }

    [Fact]
    public void BackupHistoryQuickActionLineKeepsRelativeAndFullTimeEvidence()
    {
        var timestamp = DateTime.UtcNow.AddDays(-2);
        var backup = new BackupVersionDto
        {
            BackupId = "quick-history-time",
            CreatedUtc = timestamp,
            TotalBytes = 4096
        };

        var line = GameSaveCenterPlugin.FormatBackupHistoryQuickActionLine(backup);

        Assert.Contains(backup.CreatedRelativeDisplay, line, StringComparison.Ordinal);
        Assert.Contains(backup.CreatedFullDisplay, line, StringComparison.Ordinal);
        Assert.Contains(backup.SizeDisplay, line, StringComparison.Ordinal);
        Assert.Contains(backup.RestoreReadinessStatusDisplay, line, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
        => TestRepositoryContext.Root;
}
