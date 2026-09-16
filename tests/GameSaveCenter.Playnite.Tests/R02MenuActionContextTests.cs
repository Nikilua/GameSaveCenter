using System;
using System.Linq;
using GameSaveCenter.Playnite.Infrastructure;
using Playnite.SDK.Models;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R02MenuActionContextTests
{
    [Fact]
    public void RefreshResolvesTheCapturedIdsInTheirOriginalOrder()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var first = new Game { Id = firstId, Name = "First before refresh" };
        var second = new Game { Id = secondId, Name = "Second before refresh" };
        var context = GameMenuActionContext.Capture(new[] { first, second });
        var refreshedFirst = new Game { Id = firstId, Name = "First after refresh" };
        var refreshedSecond = new Game { Id = secondId, Name = "Second after refresh" };
        var unrelated = new Game { Id = Guid.NewGuid(), Name = "Unrelated selection" };

        var resolved = context.TryResolve(
            new[] { unrelated, refreshedSecond, refreshedFirst },
            out var currentGames,
            out var missingGameId);

        Assert.True(resolved);
        Assert.Equal(Guid.Empty, missingGameId);
        Assert.Equal(new[] { firstId, secondId }, currentGames.Select(game => game.Id));
        Assert.Same(refreshedFirst, currentGames[0]);
        Assert.Same(refreshedSecond, currentGames[1]);
        Assert.NotSame(first, currentGames[0]);
        Assert.NotSame(second, currentGames[1]);
    }

    [Fact]
    public void RemovedTargetAbortsResolutionWithTheMissingIdentity()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var context = GameMenuActionContext.Capture(new[]
        {
            new Game { Id = firstId, Name = "Kept game" },
            new Game { Id = secondId, Name = "Removed game" }
        });

        var resolved = context.TryResolve(
            new[] { new Game { Id = firstId, Name = "Kept game after refresh" } },
            out var currentGames,
            out var missingGameId);

        Assert.False(resolved);
        Assert.Equal(secondId, missingGameId);
        Assert.Empty(currentGames);
    }
}
