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

    [Fact]
    public void DuplicateInspectionIsReadOnlyAndSeparatesConfidenceGroups()
    {
        var root = TestRepositoryContext.Root;
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.Media.cs"));
        var dashboardViewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var contracts = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Contracts", "MediaDuplicateDtos.cs"));
        var messages = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Contracts", "MessageTypes.cs"));
        var dispatcher = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Ipc", "IpcRequestDispatcher.cs"));
        var worker = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Services", "MediaSyncService.cs"));

        var start = view.IndexOf("<TabItem Header=\"重复识别\"", StringComparison.Ordinal);
        var end = view.IndexOf("<TabItem Header=\"来源规则\"", StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start);
        var duplicateView = view.Substring(start, end - start);

        Assert.Contains("MessageTypes.ListMediaDuplicateGroups", viewModel, StringComparison.Ordinal);
        Assert.Contains("ReloadMediaDuplicateGroupsCommand", dashboardViewModel, StringComparison.Ordinal);
        Assert.Contains("SelectedMediaDuplicateGroup", duplicateView, StringComparison.Ordinal);
        Assert.Contains("VirtualizationMode=\"Recycling\"", duplicateView, StringComparison.Ordinal);
        Assert.DoesNotContain("DeleteMedia", duplicateView, StringComparison.Ordinal);
        Assert.DoesNotContain("ReassignMedia", duplicateView, StringComparison.Ordinal);
        Assert.Contains("Certain", contracts, StringComparison.Ordinal);
        Assert.Contains("Suspected", contracts, StringComparison.Ordinal);
        Assert.Contains("ListMediaDuplicateGroups", messages, StringComparison.Ordinal);
        Assert.Contains("GetDuplicateGroupsAsync", dispatcher, StringComparison.Ordinal);
        Assert.Contains("SHA-256 完全一致", worker, StringComparison.Ordinal);
        Assert.Contains("同类型、文件名和大小一致", worker, StringComparison.Ordinal);
    }

    [Fact]
    public void ClassificationTargetsShowVisualAndStableIdentityWithoutIndexSelection()
    {
        var root = TestRepositoryContext.Root;
        var media = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
        var picker = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml"));
        var contracts = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Contracts", "GameDtos.cs"));
        var adapter = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Infrastructure", "PlayniteGameAdapter.cs"));

        Assert.Contains("MediaGameTargetTemplate", media, StringComparison.Ordinal);
        Assert.Contains("{Binding IconPath}", media, StringComparison.Ordinal);
        Assert.Contains("{Binding PlatformDisplay}", media, StringComparison.Ordinal);
        Assert.Contains("{Binding IdentityDisplay}", media, StringComparison.Ordinal);
        Assert.Contains("SelectedItem=\"{Binding InboxTargetGame}\"", media, StringComparison.Ordinal);
        Assert.Contains("SelectedItem=\"{Binding MediaTargetGame}\"", media, StringComparison.Ordinal);
        Assert.DoesNotContain("SelectedIndex=", media, StringComparison.Ordinal);
        Assert.Contains("{Binding IconPath}", picker, StringComparison.Ordinal);
        Assert.Contains("{Binding IdentityDisplay}", picker, StringComparison.Ordinal);
        Assert.Contains("IconPath", contracts, StringComparison.Ordinal);
        Assert.Contains("ResolveIconPath(game.Icon)", adapter, StringComparison.Ordinal);
    }
}
