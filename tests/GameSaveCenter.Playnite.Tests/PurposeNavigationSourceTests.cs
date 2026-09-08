using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class PurposeNavigationSourceTests
{
    [Fact]
    public void FindingNavigationUsesStableTargetAndOneExplicitTaskLoad()
    {
        var root = FindRepositoryRoot();
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));

        Assert.Contains("FindingNavigationTargetResolver.ResolveExactGame", viewModel);
        Assert.Contains("SetTaskNavigationTarget(taskTarget)", viewModel);
        Assert.Contains("taskSearchRefresh.Cancel();", viewModel);
        Assert.Contains("taskHistoryQueryRefresh.Cancel();", viewModel);
        Assert.Contains("Run(() => LoadTaskPageAsync(true));", viewModel);
        Assert.Contains("RestoreTaskSelection(selectedTaskId);", viewModel);
        Assert.Contains("taskNavigationGameName", viewModel);
    }

    [Fact]
    public void WorkspacePagesKeepTheirTabContextWhenSidebarChanges()
    {
        var root = FindRepositoryRoot();
        var shell = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml.cs"));
        var media = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
        var save = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
        var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));

        Assert.Contains("pages[WorkspaceKind.Media] = CreatePage(new MediaCenterView())", shell);
        Assert.Contains("pages[WorkspaceKind.Saves] = CreatePage(new SaveCenterView())", shell);
        Assert.Contains("pages[WorkspaceKind.Maintenance] = CreatePage(new MaintenanceView())", shell);
        Assert.Contains("MediaTabIndex, Mode=TwoWay", media);
        Assert.Contains("SaveTabIndex, Mode=TwoWay", save);
        Assert.Contains("MaintenanceTabIndex, Mode=TwoWay", maintenance);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
