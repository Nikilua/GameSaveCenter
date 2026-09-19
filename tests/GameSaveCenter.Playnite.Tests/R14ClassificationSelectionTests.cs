using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R14ClassificationSelectionTests
{
    [Fact]
    public void ClassificationPreviewUsesStableSelectionIdsAndExplicitTargetOverrides()
    {
        var root = TestRepositoryContext.Root;
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.Media.cs"));
        var dashboardViewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var contracts = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Contracts", "MediaClassificationDtos.cs"));
        var worker = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Services", "MediaSyncService.cs"));

        Assert.Contains("IsIncluded", view, StringComparison.Ordinal);
        Assert.Contains("TargetPlayniteId", view, StringComparison.Ordinal);
        Assert.Contains("SelectedHighConfidenceCount", viewModel, StringComparison.Ordinal);
        Assert.Contains("SelectedHighConfidenceCount > 0", dashboardViewModel, StringComparison.Ordinal);
        Assert.Contains("TargetOverrides", viewModel, StringComparison.Ordinal);
        Assert.Contains("MediaClassificationTargetOverrideDto", contracts, StringComparison.Ordinal);
        Assert.Contains("UpdateMediaClassificationBatchItemTargetAsync", worker, StringComparison.Ordinal);
        Assert.Contains("invalidTargetOverrides", worker, StringComparison.Ordinal);
        Assert.Contains("MediaInboxBatchFailures", dashboardViewModel, StringComparison.Ordinal);
        Assert.Contains("RetryFailedMediaInboxBatchCommand", dashboardViewModel, StringComparison.Ordinal);
        Assert.Contains("仅重试失败项", view, StringComparison.Ordinal);
        Assert.Contains("成功项不会再次执行", viewModel, StringComparison.Ordinal);
    }
}
