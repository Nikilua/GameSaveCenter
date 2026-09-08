using System.IO;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class OverviewPriorityResolverTests
{
    [Fact]
    public void WorkerFailureWinsOverOtherPendingWork()
    {
        var snapshot = new DashboardSnapshotDto
        {
            WorkerHealthy = false,
            WarningGames = 8,
            UnassignedMediaCount = 12,
            CloudTransfers = new CloudTransferSummaryDto { FailedCount = 3 }
        };

        var state = OverviewPriorityResolver.Resolve(snapshot, true);

        Assert.Equal("Worker", state.Kind);
        Assert.Equal("Maintenance", state.ActionKind);
        Assert.Equal("Worker 需要处理", state.Title);
        Assert.Equal("打开维护中心", state.ActionText);
    }

    [Fact]
    public void CloudAttentionBecomesTheNextActionAfterOnboarding()
    {
        var snapshot = new DashboardSnapshotDto
        {
            WorkerHealthy = true,
            CloudTransfers = new CloudTransferSummaryDto
            {
                RetryScheduledCount = 2,
                AuthenticationRequiredCount = 1
            },
            UnassignedMediaCount = 9,
            WarningGames = 4
        };

        var state = OverviewPriorityResolver.Resolve(snapshot, false);

        Assert.Equal("Cloud", state.Kind);
        Assert.Equal("CloudQueue", state.ActionKind);
        Assert.Equal("3 项云端任务需要处理", state.Title);
        Assert.Equal("查看云端队列", state.ActionText);
    }

    [Fact]
    public void MediaAndAttentionStatesRemainReachableInOrder()
    {
        var media = OverviewPriorityResolver.Resolve(new DashboardSnapshotDto
        {
            WorkerHealthy = true,
            ManagedGames = 10,
            UnassignedMediaCount = 5,
            WarningGames = 2
        }, false);

        var attention = OverviewPriorityResolver.Resolve(new DashboardSnapshotDto
        {
            WorkerHealthy = true,
            ManagedGames = 10,
            WarningGames = 2
        }, false);

        Assert.Equal("Media", media.Kind);
        Assert.Equal("Media", media.ActionKind);
        Assert.Equal("打开媒体中心", media.ActionText);
        Assert.Equal("Attention", attention.Kind);
        Assert.Equal("Attention", attention.ActionKind);
        Assert.Equal("查看关注项", attention.ActionText);
    }

    [Fact]
    public void HealthyStateUsesRefreshInsteadOfAWarningAction()
    {
        var state = OverviewPriorityResolver.Resolve(new DashboardSnapshotDto
        {
            WorkerHealthy = true,
            ManagedGames = 10
        }, false);

        Assert.Equal("Healthy", state.Kind);
        Assert.Equal("Refresh", state.ActionKind);
        Assert.Equal("整体状态安全", state.Title);
        Assert.Equal("刷新概览", state.ActionText);
    }

    [Fact]
    public void EmptyLibraryIsExplicitInsteadOfBeingReportedAsHealthy()
    {
        var state = OverviewPriorityResolver.Resolve(new DashboardSnapshotDto
        {
            WorkerHealthy = true,
            ManagedGames = 0
        }, false);

        Assert.Equal("Empty", state.Kind);
        Assert.Equal("Refresh", state.ActionKind);
        Assert.Equal("还没有可管理的游戏", state.Title);
        Assert.Equal("刷新游戏库", state.ActionText);
    }

    [Fact]
    public void CloudFailureIsPromotedWithTheSnapshotAttentionCount()
    {
        var state = OverviewPriorityResolver.Resolve(new DashboardSnapshotDto
        {
            WorkerHealthy = true,
            ManagedGames = 24,
            CloudTransfers = new CloudTransferSummaryDto { FailedCount = 2 }
        }, false);

        Assert.Equal("Cloud", state.Kind);
        Assert.Equal("2 项云端任务需要处理", state.Title);
        Assert.Equal("查看云端队列", state.ActionText);
    }

    [Fact]
    public void OverviewBindsHeroToPriorityStateAndLeavesOneContextualAttentionEntry()
    {
        var root = FindRepositoryRoot();
        var overview = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml"));

        Assert.Contains("<Setter Property=\"Text\" Value=\"{Binding OverviewPriorityTitle}\"/>", overview);
        Assert.Contains("Command=\"{Binding OverviewPriorityActionCommand}\"", overview);
        Assert.Contains("Content=\"{Binding OverviewPriorityActionText}\"", overview);
        Assert.DoesNotContain("存在需要处理的项目", overview);
        Assert.DoesNotContain("全局批量命令保留在首页，不与单游戏操作混在一起。", overview);
        Assert.Equal(1, CountOccurrences(overview, "Command=\"{Binding OpenAttentionCenterCommand}\""));
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var start = 0;
        while ((start = text.IndexOf(value, start, System.StringComparison.Ordinal)) >= 0)
        {
            count++;
            start += value.Length;
        }
        return count;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(System.AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new System.InvalidOperationException("Repository root not found.");
    }
}
