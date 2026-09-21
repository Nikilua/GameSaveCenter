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
        Assert.Contains("ClipboardRetry.TrySetTextAsync(text, Clipboard.SetText)", viewModel);
        Assert.Contains("CreateMaintenanceReportRequest()", viewModel);
        Assert.Contains("File.WriteAllText(dialog.FileName, report.ReportText)", viewModel);
        Assert.Contains("GetMaintenanceReport = \"maintenance.report.get\"", messages);
    }

    [Fact]
    public void MaintenanceReportKeepsIdentityAndStructuredSectionsAcrossTheIpcBoundary()
    {
        var root = FindRepositoryRoot();
        var service = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Services", "MaintenanceReportService.cs"));
        var dispatcher = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Ipc", "IpcRequestDispatcher.cs"));
        var dto = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Contracts", "MaintenanceReportDtos.cs"));

        Assert.Contains("## 软件身份", service);
        Assert.Contains("AppendSection(builder, \"待处理\", pending)", service);
        Assert.Contains("AppendSection(builder, \"已验证\", verified)", service);
        Assert.Contains("AppendSection(builder, \"未知\", unknown)", service);
        Assert.Contains("GeneratedUtc = generatedUtc", service);
        Assert.Contains("MaintenanceReportRedactor.Redact(builder.ToString())", service);
        Assert.Contains("Read<MaintenanceReportRequestDto>(request)", dispatcher);
        Assert.Contains("class MaintenanceReportRequestDto", dto);
        Assert.Contains("UrlParameters", dto);
        Assert.Contains("WindowsUserPath", dto);
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

    [Fact]
    public void RetentionPreviewShowsDateReasonAndSafetyImpactBeforeApply()
    {
        var root = FindRepositoryRoot();
        var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
        var service = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Services", "RetentionSimulationService.cs"));
        var dto = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Contracts", "RetentionSimulationDtos.cs"));

        Assert.Contains("Text=\"{Binding CreatedRelativeDisplay}\"", maintenance);
        Assert.Contains("CreatedFullDisplay, Mode=OneWay", maintenance);
        Assert.Contains("Text=\"{Binding Reason}\"", maintenance);
        Assert.Contains("Text=\"{Binding RetentionSimulation.Summary}\"", maintenance);
        Assert.Contains("Command=\"{Binding ApplyRetentionSimulationCommand}\"", maintenance);
        Assert.Contains("MaxHeight=\"240\"", maintenance);
        Assert.Contains("PendingQuarantineCount", service);
        Assert.Contains("SkippedBusyCount", service);
        Assert.Contains("RecoveryRequiredCount", service);
        Assert.Contains("CreatedDisplay", dto);
        Assert.Contains("IsHealthProtected", dto);
        Assert.Contains("RETENTION_PREVIEW_STALE", service);
        Assert.Contains("GameOperationKind.Retention", service);
    }

    private static string FindRepositoryRoot()
        => TestRepositoryContext.Root;
}
