using System.IO;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R17StorageAnalysisNavigationTests
{
    [Fact]
    public void MissingIndexedPathIsExplainedAndNeverPresentedAsZeroUsage()
    {
        var unavailable = new StorageAnalysisDto { BackupDirectoryAvailable = false };
        var missing = new StorageAnalysisDto
        {
            BackupDirectoryAvailable = true,
            MissingIndexedPathCount = 2,
            MissingIndexedBytes = 4096
        };

        Assert.Contains("路径状态未知", unavailable.MissingIndexedPathSummary);
        Assert.Contains("失联", missing.MissingIndexedPathSummary);
        Assert.Contains("未计入磁盘实测", missing.MissingIndexedPathSummary);
        Assert.Contains("不代表占用为 0", missing.MissingIndexedPathSummary);
    }

    [Fact]
    public void StorageRankResolvesGameAndLatestVersionByStableIdsOnly()
    {
        var rank = new StorageGameRankDto { PlayniteId = "game-1", LatestBackupId = "backup-2" };
        var game = TaskSourceNavigationResolver.ResolveExactGame(
            new TaskSourceReferenceDto { StableId = rank.PlayniteId, PlayniteId = rank.PlayniteId },
            new[] { new GameStatusDto { PlayniteId = "game-1", Name = "同名游戏" } });
        var version = TaskSourceNavigationResolver.ResolveExactBackupVersion(
            rank.LatestBackupId,
            new[] { new BackupVersionDto { BackupId = "backup-2", PlayniteId = "game-1" } });
        var missing = TaskSourceNavigationResolver.ResolveExactBackupVersion(
            "missing-version",
            new[] { new BackupVersionDto { BackupId = "backup-1", PlayniteId = "game-1" } });

        Assert.NotNull(game);
        Assert.Equal("game-1", game!.PlayniteId);
        Assert.NotNull(version);
        Assert.Equal("backup-2", version!.BackupId);
        Assert.Null(missing);
    }

    [Fact]
    public void StorageCardSeparatesLogicalAndPhysicalMetricsAndWiresBothRoutes()
    {
        var root = TestRepositoryContext.Root;
        var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
        var navigation = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.Navigation.cs"));
        var dashboard = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var service = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Services", "StorageAnalysisService.cs"));

        Assert.Contains("索引体积", maintenance);
        Assert.Contains("磁盘实测", maintenance);
        Assert.Contains("OpenStorageGameCommand", maintenance);
        Assert.Contains("OpenStorageBackupCommand", maintenance);
        Assert.Contains("pendingStorageBackupId", navigation);
        Assert.Contains("OpenStorageGameCommand, OpenStorageBackupCommand", dashboard);
        Assert.Contains("MissingIndexedPathCount", service);
    }
}
