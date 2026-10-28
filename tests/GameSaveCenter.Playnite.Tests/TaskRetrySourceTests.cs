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
        Assert.Contains("请先确认失败原因和上次提交结果", view);
        Assert.Contains("重试会按所选任务类型和对象重新提交", view);
        Assert.DoesNotContain("TaskQueueFilterSummary", view);
        Assert.Contains("TaskQueueSummaryGrid", view);
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

    [Fact]
    public void ExplicitRetryRequestsCarryRootIssueAndPreserveMediaTransferKind()
    {
        var root = FindRepositoryRoot();
        var taskViewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var plugin = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "GameSaveCenterPlugin.cs"));
        var worker = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Services", "MediaSyncService.cs"));

        Assert.Contains("NotificationIssueId = GetNotificationIssueId(task)", taskViewModel);
        Assert.Contains("reference.StableId?.StartsWith(\"Media:\"", taskViewModel);
        Assert.Contains("NotificationRecordPolicy.CreateTaskIssueKey", plugin);
        Assert.Contains("notificationIssueId: notificationIssueId", worker);
        Assert.Contains("public string NotificationIssueId { get; set; }", File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Contracts", "CloudTransferDtos.cs")));
    }

    private static string FindRepositoryRoot()
        => TestRepositoryContext.Root;
}
