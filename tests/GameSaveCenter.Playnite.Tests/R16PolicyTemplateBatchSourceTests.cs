using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R16PolicyTemplateBatchSourceTests
{
    [Fact]
    public void BatchTemplateApplyUsesExplicitTargetsAndKeepsRetryRoute()
    {
        var root = TestRepositoryContext.Root;
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
        var contracts = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Contracts", "OperationDtos.cs"));
        var dispatcher = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Ipc", "IpcRequestDispatcher.cs"));

        Assert.Contains("PolicyTemplateBatchTargets", viewModel);
        Assert.Contains("PolicyTemplateBatchTargetsView", view);
        Assert.Contains("PolicyTemplateBatchSelectedCount", view);
        Assert.Contains("PolicyTemplateBatchExcludedCount", view);
        Assert.Contains("PolicyTemplateBatchChangeCount", view);
        Assert.Contains("ApplyPolicyTemplateBatchCommand", view);
        Assert.Contains("RetryPolicyTemplateBatchItemCommand", view);
        Assert.Contains("PolicyTemplateBatchPreview.Select(PolicyTemplateBatchTargets)", viewModel);
        Assert.Contains("if (selected.Count == 0)", viewModel);
        Assert.Contains("new ApplyPolicyTemplateBatchDto", viewModel);
        Assert.Contains("PolicyTemplateBatchApplyItemDto", contracts);
        Assert.Contains("ApplyPolicyTemplateBatchAsync", dispatcher);
        Assert.Contains("catch (Exception ex)", dispatcher);
        Assert.Contains("FailedCount", dispatcher);

        var applyStart = viewModel.IndexOf("private async Task ApplyPolicyTemplateBatchAsync", StringComparison.Ordinal);
        var applyEnd = viewModel.IndexOf("private async Task RetryPolicyTemplateBatchItemAsync", applyStart, StringComparison.Ordinal);
        Assert.True(applyStart >= 0);
        Assert.True(applyEnd > applyStart);
        var applyBody = viewModel.Substring(applyStart, applyEnd - applyStart);
        Assert.Contains("PolicyTemplateBatchTargets", applyBody);
        Assert.DoesNotContain("Games.Select", applyBody, StringComparison.Ordinal);
    }
}
