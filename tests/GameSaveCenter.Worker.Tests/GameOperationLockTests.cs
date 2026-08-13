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
}
