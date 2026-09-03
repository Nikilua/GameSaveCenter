using System;
using System.IO;
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
        Assert.Contains("SyncMediaFromQuickActionAsync(games)", plugin);
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

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
