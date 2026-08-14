using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class MetadataBackupSourceTests
{
    [Fact]
    public void MetadataBackupIncludesPluginSettingsEndToEnd()
    {
        var root = FindRepositoryRoot();
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var contracts = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Contracts", "MetadataBackupDtos.cs"));
        var service = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Services", "MetadataBackupService.cs"));
        var messages = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Contracts", "MessageTypes.cs"));
        var coordinator = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Services", "MetadataRestoreCoordinator.cs"));

        Assert.Contains("new MetadataBackupCreateRequestDto { PluginSettingsJson = plugin.Settings.ExportPortableJson() }", viewModel);
        Assert.Contains("GameSaveCenterSettings.ValidatePortableJson(preview.PluginSettingsJson)", viewModel);
        Assert.Contains("MetadataRestoreCoordinator.ApplyPluginSettingsAsync", viewModel);
        Assert.Contains("plugin.SavePluginSettings(plugin.Settings)", viewModel);
        Assert.Contains("plugin.NotifyVisualSettingsChanged()", viewModel);
        Assert.Contains("plugin.ApplySettingsAndAwaitAsync()", viewModel);
        Assert.Contains("MessageTypes.RollbackMetadataRestore", viewModel);
        Assert.Contains("MetadataRestoreRollbackRequestDto", viewModel);
        Assert.Contains("PluginSettingsJson", contracts);
        Assert.Contains("PluginSettingsSha256", contracts);
        Assert.Contains("MetadataRestoreRollbackRequestDto", contracts);
        Assert.Contains("MetadataRestoreRollbackResultDto", contracts);
        Assert.Contains("settings/plugin-settings.json", service);
        Assert.Contains("METADATA_PLUGIN_SETTINGS_INVALID", service);
        Assert.Contains("RollbackAsync", service);
        Assert.Contains("METADATA_ROLLBACK_MANUAL_INTERVENTION_REQUIRED", service);
        Assert.Contains("metadata.restore.rollback", messages);
        Assert.Contains("rollbackMetadataRestore", coordinator);
        Assert.Contains("ImportPortableJson(preRestorePluginJson)", coordinator);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
