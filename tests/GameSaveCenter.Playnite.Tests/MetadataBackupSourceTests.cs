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

        Assert.Contains("new MetadataBackupCreateRequestDto { PluginSettingsJson = plugin.Settings.ExportPortableJson() }", viewModel);
        Assert.Contains("plugin.Settings.ImportPortableJson(result.PluginSettingsJson)", viewModel);
        Assert.Contains("plugin.SavePluginSettings(plugin.Settings)", viewModel);
        Assert.Contains("plugin.NotifyVisualSettingsChanged()", viewModel);
        Assert.Contains("PluginSettingsJson", contracts);
        Assert.Contains("PluginSettingsSha256", contracts);
        Assert.Contains("settings/plugin-settings.json", service);
        Assert.Contains("METADATA_PLUGIN_SETTINGS_INVALID", service);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
