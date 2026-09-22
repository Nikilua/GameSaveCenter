using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R14SourceRulePreviewTests
{
    [Fact]
    public void SourceRulePreviewIsReadOnlyBoundedAndExplainable()
    {
        var root = TestRepositoryContext.Root;
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.MediaSources.cs"));
        var dashboardViewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var contracts = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Contracts", "MediaSourcePreviewDtos.cs"));
        var messages = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Contracts", "MessageTypes.cs"));
        var dispatcher = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Ipc", "IpcRequestDispatcher.cs"));
        var worker = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Services", "MediaSyncService.cs"));

        Assert.Contains("PreviewMediaSourceCommand", view, StringComparison.Ordinal);
        Assert.Contains("MediaSourcePreviewSummary", view, StringComparison.Ordinal);
        Assert.Contains("MediaSourcePreview.Items", view, StringComparison.Ordinal);
        Assert.Contains("PreviewMediaSourceAsync", viewModel, StringComparison.Ordinal);
        Assert.Contains("PreviewMediaSourceCommand", dashboardViewModel, StringComparison.Ordinal);
        Assert.Contains("MaxScannedEntries", contracts, StringComparison.Ordinal);
        Assert.Contains("TimeoutMs", contracts, StringComparison.Ordinal);
        Assert.Contains("Reason", contracts, StringComparison.Ordinal);
        Assert.Contains("PreviewMediaSource", messages, StringComparison.Ordinal);
        Assert.Contains("PreviewMediaSourceRuleAsync", dispatcher, StringComparison.Ordinal);
        Assert.Contains("CreateLinkedTokenSource", worker, StringComparison.Ordinal);
        Assert.Contains("ScanTruncated", worker, StringComparison.Ordinal);
        Assert.DoesNotContain("AddMediaSourceAsync", worker.Substring(worker.IndexOf("PreviewMediaSourceRuleAsync", StringComparison.Ordinal)), StringComparison.Ordinal);
    }
}
