using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class MaintenanceReportSourceTests
{
    [Fact]
    public void MaintenanceReportCommandsAndIpcAreWired()
    {
        var root = FindRepositoryRoot();
        var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var messages = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Contracts", "MessageTypes.cs"));

        Assert.Contains("CopyMaintenanceReportCommand", maintenance);
        Assert.Contains("ExportMaintenanceReportCommand", maintenance);
        Assert.Contains("MessageTypes.GetMaintenanceReport", viewModel);
        Assert.Contains("CopyTextWithRetryAsync(report.ReportText", viewModel);
        Assert.Contains("Clipboard.SetText(text)", viewModel);
        Assert.Contains("File.WriteAllText(dialog.FileName, report.ReportText)", viewModel);
        Assert.Contains("GetMaintenanceReport = \"maintenance.report.get\"", messages);
    }

    [Fact]
    public void MaintenanceOverviewUsesConcreteNextStepRecords()
    {
        var root = FindRepositoryRoot();
        var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
        var actions = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.MaintenanceActions.cs"));
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var messages = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Contracts", "MessageTypes.cs"));
        var dispatcher = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Ipc", "IpcRequestDispatcher.cs"));
        var store = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Persistence", "SqliteStateStore.RetentionQuarantine.cs"));

        Assert.Contains("ItemsSource=\"{Binding MaintenanceActionSections}\"", maintenance);
        Assert.Contains("ItemsSource=\"{Binding PreviewItems}\"", maintenance);
        Assert.Contains("ItemsSource=\"{Binding OverflowItems}\"", maintenance);
        Assert.Contains("RunMaintenanceActionCommand", maintenance);
        Assert.Contains("LastVerifiedDisplay", actions);
        Assert.Contains("NextAttemptDisplay", actions);
        Assert.Contains("MessageTypes.GetRetentionQuarantineEntries", viewModel);
        Assert.Contains("RetentionQuarantinePageRequestDto", viewModel);
        Assert.Contains("RetentionQuarantineLoadedDisplay", maintenance);
        Assert.Contains("LoadMoreRetentionQuarantineCommand", maintenance);
        Assert.Contains("GetRetentionQuarantineEntries", messages);
        Assert.Contains("GetRetentionQuarantinePageAsync", dispatcher);
        Assert.Contains("LIMIT $limit OFFSET $offset", store);
        Assert.Contains("RecoverRetentionQuarantine", dispatcher);
        Assert.Contains("Confirmed = true", actions);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
