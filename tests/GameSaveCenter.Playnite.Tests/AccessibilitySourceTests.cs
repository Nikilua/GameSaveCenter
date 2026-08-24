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
        var shellXaml = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml"));
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
        Assert.Contains("AutomationProperties.Name=\"清除游戏搜索\"", shellXaml);
        Assert.Contains("AutomationProperties.Name=\"清除媒体搜索\"", mediaXaml);
        Assert.Contains("AutomationProperties.Name=\"清除任务搜索\"", taskXaml);
        Assert.Contains("AutomationProperties.Name=\"清除 FLiNG 搜索\"", trainerXaml);
        Assert.Contains("Padding=\"12,3,38,3\"", shellXaml);
        Assert.Contains("Padding=\"12,3,38,3\"", mediaXaml);
        Assert.Contains("Padding=\"30,7,38,7\"", taskXaml);
        Assert.Contains("Padding=\"12,3,38,3\"", trainerXaml);
    }

    [Fact]
    public void SharedTextBoxesUseLeadingCaretAlignment()
    {
        var root = FindRepositoryRoot();
        var production = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "WpfUiProduction.xaml"));
        var tokens = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "DesignTokens.xaml"));

        Assert.Contains("<Setter Property=\"HorizontalContentAlignment\" Value=\"Left\"/>", production);
        Assert.Contains("<Setter Property=\"TextAlignment\" Value=\"Left\"/>", production);
        Assert.Contains("<Setter Property=\"HorizontalContentAlignment\" Value=\"Left\"/>", tokens);
        Assert.Contains("<Setter Property=\"TextAlignment\" Value=\"Left\"/>", tokens);
        Assert.Contains("HorizontalContentAlignment=\"{TemplateBinding HorizontalContentAlignment}\"", production);
        Assert.Contains("HorizontalContentAlignment=\"{TemplateBinding HorizontalContentAlignment}\"", tokens);
        Assert.Contains("TextElement.Foreground=\"{TemplateBinding Foreground}\"", production);
        Assert.Contains("TextElement.Foreground=\"{TemplateBinding Foreground}\"", tokens);
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
