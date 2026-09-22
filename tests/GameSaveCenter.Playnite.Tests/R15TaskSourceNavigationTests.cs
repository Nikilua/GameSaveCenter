using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R15TaskSourceNavigationTests
{
    [Fact]
    public void TaskSourceGameUsesStableIdAndDoesNotFallBackToSameName()
    {
        var source = new TaskSourceReferenceDto
        {
            Kind = TaskSourceReferenceKind.Game,
            StableId = "removed-game",
            PlayniteId = "removed-game",
            DisplayName = "同名游戏"
        };

        var target = TaskSourceNavigationResolver.ResolveExactGame(source, new[]
        {
            new GameStatusDto { PlayniteId = "current-game", Name = "同名游戏" }
        });

        Assert.Null(target);
    }

    [Fact]
    public void TaskSourceBackupUsesExactVersionIdAndDoesNotSelectNeighbor()
    {
        var target = TaskSourceNavigationResolver.ResolveExactBackupVersion(
            "missing-version",
            new[]
            {
                new BackupVersionDto { BackupId = "older-version" },
                new BackupVersionDto { BackupId = "newer-version" }
            });

        Assert.Null(target);
    }

    [Fact]
    public void TaskSourceCloneKeepsStableIdentityAndDiagnosticDetail()
    {
        var source = new TaskSourceReferenceDto
        {
            Kind = TaskSourceReferenceKind.MediaBatch,
            StableId = "batch-42",
            PlayniteId = "game-1",
            DisplayName = "媒体批次",
            Detail = "对象已删除时仍保留此诊断"
        };

        var clone = source.Clone();

        Assert.NotSame(source, clone);
        Assert.Equal(source.StableId, clone.StableId);
        Assert.Equal(source.PlayniteId, clone.PlayniteId);
        Assert.Equal(source.Detail, clone.Detail);
    }
}
