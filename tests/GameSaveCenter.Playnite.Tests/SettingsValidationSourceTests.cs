using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class SettingsValidationSourceTests
{
    [Fact]
    public void SettingsPageShowsInlineValidationSummary()
    {
        var root = FindRepositoryRoot();
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml"));
        var code = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml.cs"));

        Assert.Contains("x:Name=\"SettingsValidationSummary\"", view);
        Assert.Contains("AutomationProperties.Name=\"设置验证错误\"", view);
        Assert.Contains("AddHandler(TextBox.TextChangedEvent", code);
        Assert.Contains("AddHandler(ComboBox.SelectionChangedEvent", code);
        Assert.Contains("AddHandler(CheckBox.ClickEvent", code);
        Assert.Contains("RefreshValidationSummary", code);
        Assert.Contains("settings.VerifySettings(out errors)", code);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
