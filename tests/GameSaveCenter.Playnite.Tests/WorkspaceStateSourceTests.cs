using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class WorkspaceStateSourceTests
{
    [Fact]
    public void SharedWorkspaceStatePresenterExistsAndIsUsedAcrossPages()
    {
        var root = FindRepositoryRoot();
        var control = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Controls", "WorkspaceStatePresenter.cs"));
        var redesign = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "Redesign.xaml"));
        var overview = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml"));
        var task = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));
        var save = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
        var trainer = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TrainerCenterView.xaml"));
        var media = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
        var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));

        Assert.Contains("class WorkspaceStatePresenter", control);
        Assert.Contains("StateProperty", control);
        Assert.Contains("TitleProperty", control);
        Assert.Contains("MessageProperty", control);
        Assert.Contains("RetryCommandProperty", control);
        Assert.Contains("x:Key=\"GscWorkspaceStatePresenter\"", redesign);
        Assert.Contains("<ui:Button x:Name=\"RetryButton\"", redesign);
        Assert.DoesNotContain("<Button x:Name=\"RetryButton\"", redesign);
        Assert.Contains("ui:WorkspaceStatePresenter", overview);
        Assert.Contains("ui:WorkspaceStatePresenter", task);
        Assert.Contains("ui:WorkspaceStatePresenter", save);
        Assert.Contains("ui:WorkspaceStatePresenter", trainer);
        Assert.Contains("ui:WorkspaceStatePresenter", media);
        Assert.Contains("ui:WorkspaceStatePresenter", maintenance);
        Assert.Contains("State=\"Loading\"", save);
        Assert.Contains("State=\"Empty\"", trainer);
        Assert.Contains("State=\"Loading\"", trainer);
        Assert.Contains("State=\"Offline\"", media);
        Assert.Contains("State=\"Degraded\"", maintenance);
    }

    [Fact]
    public void StatePresenterSupportsSixUnifiedStates()
    {
        var root = FindRepositoryRoot();
        var control = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Controls", "WorkspaceStatePresenter.cs"));

        Assert.Contains("\"Loading\"", control);
        Assert.Contains("\"Empty\"", control);
        Assert.Contains("\"Error\"", control);
        Assert.Contains("\"Degraded\"", control);
        Assert.Contains("\"Offline\"", control);
        Assert.Contains("\"Disabled\"", control);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
