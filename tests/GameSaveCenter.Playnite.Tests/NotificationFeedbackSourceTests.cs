using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class NotificationFeedbackSourceTests
{
    [Fact]
    public void NotificationsKeepSummaryDetailAndCancellationSemantics()
    {
        var root = FindRepositoryRoot();
        var plugin = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "GameSaveCenterPlugin.cs"));
        var dashboard = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml.cs"));
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml"));

        Assert.Contains("string? detailMessage = null", plugin);
        Assert.Contains("DetailMessage", plugin);
        Assert.Contains("BuildTaskNotificationDetail", plugin);
        Assert.Contains("UiNotificationKind.Warning", plugin);
        Assert.Contains("public void ShowWarning", plugin);
        Assert.Contains("e.DetailMessage", dashboard);
        Assert.Contains("DialogCopyButton", dashboard);
        Assert.Contains("OnDialogCopyClick", dashboard);
        Assert.Contains("ScrollViewer MaxHeight=\"420\"", view);
        Assert.Contains("VerticalScrollBarVisibility=\"Auto\"", view);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
