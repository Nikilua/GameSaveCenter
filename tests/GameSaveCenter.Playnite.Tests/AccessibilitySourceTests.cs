using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class AccessibilitySourceTests
{
    [Fact]
    public void CtrlFFocusesWorkspaceSearchAndAutomationNamesArePresent()
    {
        var root = FindRepositoryRoot();
        var dashboardCode = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml.cs"));
        var dashboardXaml = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml"));
        var taskXaml = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));
        var mediaXaml = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
        var trainerXaml = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TrainerCenterView.xaml"));

        Assert.Contains("e.Key == Key.F && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control", dashboardCode);
        Assert.Contains("FocusWorkspaceSearch", dashboardCode);
        Assert.Contains("TrainerSearchTextBox", dashboardCode);
        Assert.Contains("TaskSearchTextBox", dashboardCode);
        Assert.Contains("MediaSearchTextBox", dashboardCode);
        Assert.Contains("ProcessMappingExecutableTextBox", dashboardCode);
        Assert.Contains("AutomationProperties.Name=\"搜索游戏\"", dashboardXaml);
        Assert.Contains("x:Name=\"TaskSearchTextBox\"", taskXaml);
        Assert.Contains("AutomationProperties.Name=\"搜索任务\"", taskXaml);
        Assert.Contains("x:Name=\"MediaSearchTextBox\"", mediaXaml);
        Assert.Contains("AutomationProperties.Name=\"搜索当前游戏媒体\"", mediaXaml);
        Assert.Contains("AutomationProperties.Name=\"搜索 FLiNG 目录\"", trainerXaml);
    }

    [Fact]
    public void FocusVisualsAndHighContrastFallbackAreShared()
    {
        var root = FindRepositoryRoot();
        var tokens = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "DesignTokens.xaml"));
        var dashboardCode = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml.cs"));

        Assert.Contains("GscSharedFocusVisual", tokens);
        Assert.Contains("SystemParameters.HighContrast", dashboardCode);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
