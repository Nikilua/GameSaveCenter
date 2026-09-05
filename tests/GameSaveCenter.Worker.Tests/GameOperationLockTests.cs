using GameSaveCenter.Worker.Services;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class GameOperationLockTests
{
    [Fact]
    public async Task SameGameIsSerializedButDifferentGamesRunInParallel()
    {
        var lockService = new GameOperationLock();
        var first = await lockService.AcquireAsync("game-a", TimeSpan.FromMilliseconds(500), CancellationToken.None);
        Assert.NotNull(first);
        var second = await lockService.AcquireAsync("game-a", TimeSpan.FromMilliseconds(100), CancellationToken.None);
        Assert.Null(second);
        var other = await lockService.AcquireAsync("game-b", TimeSpan.FromMilliseconds(100), CancellationToken.None);
        Assert.NotNull(other);

        first!.Dispose();
        other!.Dispose();
        var afterRelease = await lockService.AcquireAsync("game-a", TimeSpan.FromMilliseconds(500), CancellationToken.None);
        Assert.NotNull(afterRelease);
        afterRelease!.Dispose();
    }

    [Fact]
    public async Task RepeatedDistinctGameAcquisitionsStayBounded()
    {
        var lockService = new GameOperationLock();
        for (var i = 0; i < 1000; i++)
        {
            using var lease = await lockService.AcquireAsync($"game-{i % 16}", TimeSpan.FromMilliseconds(500), CancellationToken.None);
            Assert.NotNull(lease);
        }

        Assert.Equal(16, lockService.TrackedGameCount);
    }

    [Theory]
    [InlineData(GameOperationKind.Backup, GameOperationKind.Media, true)]
    [InlineData(GameOperationKind.CloudUpload, GameOperationKind.Media, true)]
    [InlineData(GameOperationKind.RestoreReadiness, GameOperationKind.Backup, true)]
    [InlineData(GameOperationKind.Backup, GameOperationKind.Backup, false)]
    [InlineData(GameOperationKind.Restore, GameOperationKind.Backup, false)]
    [InlineData(GameOperationKind.Restore, GameOperationKind.Restore, false)]
    [InlineData(GameOperationKind.Restore, GameOperationKind.Retention, false)]
    [InlineData(GameOperationKind.Retention, GameOperationKind.Backup, false)]
    [InlineData(GameOperationKind.Metadata, GameOperationKind.Backup, false)]
    public void OperationCompatibilityMatrixIsExplicit(GameOperationKind left, GameOperationKind right, bool expected)
        => Assert.Equal(expected, GameOperationPolicy.IsCompatible(left, right));

    [Fact]
    public async Task TypedAcquireStillSerializesSameGame()
    {
        var lockService = new GameOperationLock();
        using var first = await lockService.AcquireAsync("game-a", GameOperationKind.Backup, TimeSpan.FromSeconds(1), CancellationToken.None);
        Assert.NotNull(first);
        using var second = await lockService.AcquireAsync("game-a", GameOperationKind.Media, TimeSpan.FromMilliseconds(50), CancellationToken.None);
        Assert.Null(second);
    }
}
