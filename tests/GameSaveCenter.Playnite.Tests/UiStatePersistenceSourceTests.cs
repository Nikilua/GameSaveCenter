using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class UiStatePersistenceSourceTests
{
    [Fact]
    public void UiStateFieldsArePersistedAndRestored()
    {
        var root = FindRepositoryRoot();
        var settings = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettings.cs"));
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var plugin = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "GameSaveCenterPlugin.cs"));
        var mediaView = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));

        Assert.Contains("public string LastWorkspace", settings);
        Assert.Contains("public string TaskStatusFilterState", settings);
        Assert.Contains("public string TaskGameFilterState", settings);
        Assert.Contains("public string TaskTypeFilterState", settings);
        Assert.Contains("public string TaskSearchTextState", settings);
        Assert.Contains("public string TaskHistoryScopeState", settings);
        Assert.Contains("public string TaskHistoryRangeState", settings);
        Assert.Contains("public string MediaFilterState", settings);
        Assert.Contains("public string MediaSearchTextState", settings);
        Assert.Contains("public WorkspaceKind? SessionLastWorkspace", plugin);
        Assert.Contains("plugin.SessionLastWorkspace ?? WorkspaceKind.Overview", viewModel);
        Assert.Contains("plugin.SessionLastWorkspace = value", viewModel);
        Assert.Contains("pendingTaskGameFilter", viewModel);
        Assert.Contains("pendingTaskTypeFilter", viewModel);
        Assert.Contains("ClearMediaFiltersCommand", viewModel);
        Assert.Contains("MediaHasActiveFilters", viewModel);
        Assert.Contains("TaskHistoryScopeState", viewModel);
        Assert.Contains("TaskHistoryRangeState", viewModel);
        Assert.Contains("Command=\"{Binding ClearMediaFiltersCommand}\"", mediaView);
        Assert.Contains("MediaHasActiveFilters", mediaView);
        Assert.DoesNotContain("plugin.Settings.LastWorkspace = value.ToString()", viewModel);
        Assert.DoesNotContain("plugin.Settings.LastWorkspace = CurrentWorkspace.ToString()", viewModel);
        Assert.Contains("private void SaveUiStateSettings()", viewModel);
        Assert.Contains("uiStateSave?.Schedule()", viewModel);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
