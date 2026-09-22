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
    public void HealthFindingRoutesToItsExactBackupVersion()
    {
        var finding = new ValidationFindingDto
        {
            PlayniteId = "game-1",
            BackupId = "backup-2",
            Code = "HEALTH_INSPECTION_FAILED",
            Title = "备份恢复校验需关注：backup-2"
        };

        var route = FindingNavigationResolver.Resolve(finding);
        var backupId = FindingNavigationTargetResolver.ResolveBackupId(finding);
        var backup = TaskSourceNavigationResolver.ResolveExactBackupVersion(
            backupId,
            new[]
            {
                new BackupVersionDto { BackupId = "backup-1" },
                new BackupVersionDto { BackupId = "backup-2" }
            });

        Assert.Equal(FindingNavigationKind.BackupVersion, route.Kind);
        Assert.Equal("backup-2", backupId);
        Assert.NotNull(backup);
        Assert.Equal("backup-2", backup!.BackupId);
    }

    [Fact]
    public void LegacyHealthFindingUsesTitleIdentityButMissingVersionDoesNotSelectNeighbor()
    {
        var finding = new ValidationFindingDto
        {
            PlayniteId = "game-1",
            Code = "HEALTH_INSPECTION_FAILED",
            Title = "备份恢复校验需关注：legacy-backup"
        };

        var route = FindingNavigationResolver.Resolve(finding);
        var backup = TaskSourceNavigationResolver.ResolveExactBackupVersion(
            FindingNavigationTargetResolver.ResolveBackupId(finding),
            new[] { new BackupVersionDto { BackupId = "other-backup" } });

        Assert.Equal(FindingNavigationKind.BackupVersion, route.Kind);
        Assert.Null(backup);
    }

    [Fact]
    public void HealthFindingWithoutVersionIdentityKeepsTheDiagnosticWithoutTaskFallback()
    {
        var route = FindingNavigationResolver.Resolve(new ValidationFindingDto
        {
            PlayniteId = "game-1",
            Code = "HEALTH_INSPECTION_FAILED",
            Title = "恢复巡检需要关注"
        });

        Assert.Equal(FindingNavigationKind.None, route.Kind);
        Assert.False(route.IsAvailable);
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
