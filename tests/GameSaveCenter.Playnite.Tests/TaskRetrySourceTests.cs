using System;
using System.IO;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class TaskRetrySourceTests
{
    [Fact]
    public void TaskCenterExposesSafeBulkRetryWithoutChangingWorkerProtocol()
    {
        var root = FindRepositoryRoot();
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var view = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));

        Assert.Contains("RetryAllTasksCommand = new RelayCommand", viewModel);
        Assert.Contains("private async Task RetryAllTasksAsync()", viewModel);
        Assert.Contains("按游戏和任务类型各重试一次", viewModel);
        Assert.Contains("GroupBy(GetRetryGroupKey", viewModel);
        Assert.Contains("Command=\"{Binding RetryAllTasksCommand}\"", view);
        Assert.Contains("Content=\"重试可恢复\"", view);
        Assert.Contains("当前已加载且符合筛选", view);
        Assert.Contains("AutomationProperties.Name=\"批量重试当前结果中的可恢复任务\"", view);
        Assert.Contains("TaskQueueFilterSummary", view);
        Assert.Contains("TaskActiveFiltersSummary", view);
        Assert.Contains("TaskWaitingSummary", view);
        Assert.Contains("TaskRetrySummary", view);
    }

    [Fact]
    public void BulkActionsCaptureStableIdsAndExplainPartialResults()
    {
        var root = FindRepositoryRoot();
        var taskViewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var mediaViewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.Media.cs"));

        Assert.Contains("RetryBatchCandidate", taskViewModel);
        Assert.Contains("retryableWithStableId", taskViewModel);
        Assert.Contains("ToTaskSnapshot()", taskViewModel);
        Assert.Contains("已捕获 {candidates.Count} 个稳定任务 ID", taskViewModel);
        Assert.Contains("MediaInboxBatchSelection", mediaViewModel);
        Assert.Contains("CaptureInboxMediaSelection", mediaViewModel);
        Assert.Contains("selection.SelectionSummary", mediaViewModel);
        Assert.Contains("selection.MediaIds", mediaViewModel);
        Assert.Contains("ReportMediaClassificationResult", mediaViewModel);
        Assert.Contains("跳过/未返回", mediaViewModel);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
