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
        Assert.Contains("AutomationProperties.Name=\"批量重试可恢复任务\"", view);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
