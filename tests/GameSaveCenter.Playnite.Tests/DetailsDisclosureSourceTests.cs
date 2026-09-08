using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class DetailsDisclosureSourceTests
{
    [Fact]
    public void SelectionContextsCloseTheirPreviousCompactDetails()
    {
        var root = FindRepositoryRoot();
        var media = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml.cs"));
        var task = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml.cs"));
        var save = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml.cs"));
        var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml.cs"));

        Assert.Contains("mediaInboxInspectorOpen = false;", media);
        Assert.Contains("mediaInboxHistoryOpen = false;", media);
        Assert.Contains("taskInspectorOpen = false;", task);
        Assert.Contains("historyInspectorOpen = false;", save);
        Assert.Contains("candidateInspectorOpen = false;", save);
        Assert.Contains("diagnosticsInspectorOpen = false;", maintenance);
        Assert.Contains("processInspectorOpen = false;", maintenance);
        Assert.Contains("deviceInspectorOpen = false;", maintenance);
        Assert.Contains("cloudTransferInspectorOpen = false;", maintenance);
        Assert.DoesNotContain("CloudTransferGrid.SelectionChanged += OnCloudTransferSelectionChanged", maintenance);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
