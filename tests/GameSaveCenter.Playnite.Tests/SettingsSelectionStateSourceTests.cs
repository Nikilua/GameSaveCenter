using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class SettingsSelectionStateSourceTests
{
    [Fact]
    public void PersistedSettingSelectionsAreNotOverwrittenByStaticFirstItems()
    {
        var root = FindRepositoryRoot();
        var settings = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml"));

        Assert.DoesNotContain("SelectedIndex=\"0\" SelectedValue=\"{Binding BackupFormat", settings);
        Assert.DoesNotContain("SelectedIndex=\"0\" SelectedValue=\"{Binding Compression", settings);
        Assert.DoesNotContain("SelectedIndex=\"0\" SelectedValue=\"{Binding ThemeMode", settings);
        Assert.Contains("SelectedValue=\"{Binding BackupFormat, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged", settings);
        Assert.Contains("SelectedValue=\"{Binding Compression, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged", settings);
        Assert.Contains("SelectedValue=\"{Binding ThemeMode, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged", settings);
        Assert.Contains("AutomationProperties.Name=\"备份存储格式\"", settings);
        Assert.Contains("AutomationProperties.Name=\"备份压缩方式\"", settings);
        Assert.Contains("AutomationProperties.Name=\"界面主题\"", settings);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
