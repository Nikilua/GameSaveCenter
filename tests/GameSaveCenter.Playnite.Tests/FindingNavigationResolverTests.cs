using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using System.Collections.Generic;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class FindingNavigationResolverTests
{
    [Fact]
    public void GameFindingRoutesToSaveWorkspace()
    {
        var route = FindingNavigationResolver.Resolve(new ValidationFindingDto
        {
            PlayniteId = "game-1",
            Code = "SAVE_PATH_MISSING",
            Title = "存档路径不存在"
        });

        Assert.Equal(FindingNavigationKind.Save, route.Kind);
        Assert.Equal("进入存档路径确认", route.Text);
    }

    [Fact]
    public void CloudFindingRoutesToCloudQueueBeforeGameFallback()
    {
        var route = FindingNavigationResolver.Resolve(new ValidationFindingDto
        {
            PlayniteId = "game-1",
            Code = "RCLONE_CHECK_FAILED",
            Title = "远端校验失败"
        });

        Assert.Equal(FindingNavigationKind.CloudQueue, route.Kind);
        Assert.Equal("打开云队列", route.Text);
    }

    [Fact]
    public void TaskHintRoutesToFailedTasks()
    {
        var route = FindingNavigationResolver.Resolve(new ValidationFindingDto
        {
            PlayniteId = "game-1",
            Code = "OPERATION_WARNING",
            SuggestedAction = "在任务中心查看详情并重试。"
        });

        Assert.Equal(FindingNavigationKind.FailedTasks, route.Kind);
        Assert.Equal("查看失败任务", route.Text);
    }

    [Fact]
    public void FindingWithoutIdentityHasNoNavigation()
    {
        var route = FindingNavigationResolver.Resolve(new ValidationFindingDto
        {
            Code = "INFO_ONLY",
            Title = "状态提示"
        });

        Assert.Equal(FindingNavigationKind.None, route.Kind);
        Assert.False(route.IsAvailable);
    }

    [Fact]
    public void MissingSaveTargetDoesNotFallBackToCurrentGame()
    {
        var target = FindingNavigationTargetResolver.ResolveExactGame(
            new ValidationFindingDto { PlayniteId = "removed-game", Code = "SAVE_PATH_MISSING" },
            new[] { new GameStatusDto { PlayniteId = "current-game", Name = "当前游戏" } });

        Assert.False(target.IsAvailable);
        Assert.False(target.IsExact);
        Assert.Contains("找不到诊断对应的游戏", target.Message);
    }

    [Fact]
    public void TaskTargetUsesExactGameIdentityWhenSnapshotContainsIt()
    {
        var target = FindingNavigationTargetResolver.ResolveTaskGame(
            new ValidationFindingDto { PlayniteId = "game-2", GameName = "旧名称", Code = "TASK_FAILED" },
            new[] { new GameStatusDto { PlayniteId = "game-2", Name = "当前名称" } });

        Assert.True(target.IsAvailable);
        Assert.True(target.IsExact);
        Assert.Equal("game-2", target.PlayniteId);
        Assert.Equal("当前名称", target.GameName);
    }

    [Fact]
    public void TaskTargetFallsBackToDiagnosticNameWithoutChangingCurrentGame()
    {
        var target = FindingNavigationTargetResolver.ResolveTaskGame(
            new ValidationFindingDto { PlayniteId = "removed-game", GameName = "已移除游戏", Code = "TASK_FAILED" },
            new List<GameStatusDto>());

        Assert.True(target.IsAvailable);
        Assert.False(target.IsExact);
        Assert.Equal("已移除游戏", target.GameName);
        Assert.Contains("未修改当前游戏选择", target.Message);
    }

    [Fact]
    public void TaskTargetWithoutAnyIdentityIsReportedAsUnavailable()
    {
        var target = FindingNavigationTargetResolver.ResolveTaskGame(
            new ValidationFindingDto { Code = "TASK_FAILED" },
            new List<GameStatusDto>());

        Assert.False(target.IsAvailable);
        Assert.True(string.IsNullOrEmpty(target.Message));
    }
}
