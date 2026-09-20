using System.Collections.Generic;
using System.IO;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
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
        Assert.Equal("后台服务需要处理", state.Title);
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

    [Theory]
    [InlineData(WorkspaceKind.Overview, null, null, "今日工作台 · 正在读取概览状态")]
    [InlineData(WorkspaceKind.Overview, null, "后台服务需要处理", "今日工作台 · 后台服务需要处理")]
    [InlineData(WorkspaceKind.Saves, "合成游戏", "不应显示", "合成游戏 · 路径与恢复点状态")]
    public void ShellSubtitleFollowsTheCurrentStateInsteadOfClaimingEverythingIsHealthy(
        WorkspaceKind workspace,
        string? selectedGameName,
        string? overviewPriorityTitle,
        string expected)
    {
        var subtitle = AcrylicProductionShellView.GetPageSubtitle(workspace, selectedGameName, overviewPriorityTitle);

        Assert.Equal(expected, subtitle);
        Assert.DoesNotContain("一切运行正常", subtitle);
    }

    [Fact]
    public void PriorityCopyAlwaysExplainsTheStateAndTheNextAction()
    {
        var states = new[]
        {
            OverviewPriorityResolver.Resolve(new DashboardSnapshotDto { WorkerHealthy = false }, false),
            OverviewPriorityResolver.Resolve(new DashboardSnapshotDto { WorkerHealthy = true }, true),
            OverviewPriorityResolver.Resolve(new DashboardSnapshotDto
            {
                WorkerHealthy = true,
                ManagedGames = 4,
                CloudTransfers = new CloudTransferSummaryDto { FailedCount = 1 }
            }, false),
            OverviewPriorityResolver.Resolve(new DashboardSnapshotDto
            {
                WorkerHealthy = true,
                ManagedGames = 4,
                TaskSummary = new TaskSummaryDto { FailedCount = 1 }
            }, false),
            OverviewPriorityResolver.Resolve(new DashboardSnapshotDto { WorkerHealthy = true, ManagedGames = 0 }, false),
            OverviewPriorityResolver.Resolve(new DashboardSnapshotDto { WorkerHealthy = true, ManagedGames = 4, UnassignedMediaCount = 1 }, false),
            OverviewPriorityResolver.Resolve(new DashboardSnapshotDto { WorkerHealthy = true, ManagedGames = 4, WarningGames = 1 }, false),
            OverviewPriorityResolver.Resolve(new DashboardSnapshotDto { WorkerHealthy = true, ManagedGames = 4 }, false)
        };

        foreach (var state in states)
        {
            Assert.False(string.IsNullOrWhiteSpace(state.Title));
            Assert.False(string.IsNullOrWhiteSpace(state.Description));
            Assert.False(string.IsNullOrWhiteSpace(state.ActionText));
            Assert.DoesNotContain("Worker", state.Title);
            Assert.DoesNotContain("Rclone", state.Description);
            Assert.DoesNotContain("你", state.Description);
        }
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
    public void UnmatchedGamesUseTheExistingPickerRouteBeforeBackupableGames()
    {
        var state = OverviewPriorityResolver.Resolve(new DashboardSnapshotDto
        {
            WorkerHealthy = true,
            ManagedGames = 2,
            MatchedGames = 1,
            LudusaviAvailable = true,
            Games = new List<GameStatusDto>
            {
                new GameStatusDto { LudusaviMatched = false },
                new GameStatusDto { LudusaviMatched = true }
            }
        }, false);

        Assert.Equal("Unmatched", state.Kind);
        Assert.Equal("GamePicker", state.ActionKind);
        Assert.Equal("查看未匹配游戏", state.ActionText);
    }

    [Fact]
    public void MatchedGameWithoutBackupUsesPickerInsteadOfLaunchingBulkWrite()
    {
        var state = OverviewPriorityResolver.Resolve(new DashboardSnapshotDto
        {
            WorkerHealthy = true,
            ManagedGames = 1,
            MatchedGames = 1,
            LudusaviAvailable = true,
            Games = new List<GameStatusDto>
            {
                new GameStatusDto { LudusaviMatched = true }
            }
        }, false);

        Assert.Equal("Backupable", state.Kind);
        Assert.Equal("GamePicker", state.ActionKind);
        Assert.Equal("查看可备份游戏", state.ActionText);
    }

    [Fact]
    public void FailedTasksUseTheTaskCenterRoute()
    {
        var state = OverviewPriorityResolver.Resolve(new DashboardSnapshotDto
        {
            WorkerHealthy = true,
            ManagedGames = 4,
            TaskSummary = new TaskSummaryDto { FailedCount = 2 }
        }, false);

        Assert.Equal("Tasks", state.Kind);
        Assert.Equal("Tasks", state.ActionKind);
        Assert.Equal("查看失败任务", state.ActionText);
    }

    [Fact]
    public void UnrelatedSnapshotChangesDoNotMakeTheSamePriorityJump()
    {
        var first = new DashboardSnapshotDto
        {
            WorkerHealthy = true,
            ManagedGames = 1,
            LudusaviAvailable = true,
            Games = new List<GameStatusDto> { new GameStatusDto { LudusaviMatched = true } }
        };
        var second = new DashboardSnapshotDto
        {
            WorkerHealthy = true,
            ManagedGames = 1,
            WarningGames = 9,
            LudusaviAvailable = true,
            Games = new List<GameStatusDto> { new GameStatusDto { LudusaviMatched = true } }
        };

        var firstState = OverviewPriorityResolver.Resolve(first, false);
        var secondState = OverviewPriorityResolver.Resolve(second, false);

        Assert.Equal(firstState.Kind, secondState.Kind);
        Assert.Equal(firstState.ActionKind, secondState.ActionKind);
        Assert.Equal(firstState.ActionText, secondState.ActionText);
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
        => TestRepositoryContext.Root;
}
