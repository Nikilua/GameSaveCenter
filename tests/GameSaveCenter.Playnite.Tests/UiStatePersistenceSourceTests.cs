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

        Assert.Contains("public string LastWorkspace", settings);
        Assert.Contains("public string TaskStatusFilterState", settings);
        Assert.Contains("public string TaskGameFilterState", settings);
        Assert.Contains("public string TaskTypeFilterState", settings);
        Assert.Contains("public string TaskSearchTextState", settings);
        Assert.Contains("public string MediaFilterState", settings);
        Assert.Contains("public string MediaSearchTextState", settings);
        Assert.Contains("Enum.TryParse(plugin.Settings.LastWorkspace", viewModel);
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
