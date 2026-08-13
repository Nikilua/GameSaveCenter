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
        Assert.Contains("Clipboard.SetText(report.ReportText)", viewModel);
        Assert.Contains("File.WriteAllText(dialog.FileName, report.ReportText)", viewModel);
        Assert.Contains("GetMaintenanceReport = \"maintenance.report.get\"", messages);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
