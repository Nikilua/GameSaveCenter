using GameSaveCenter.Worker.Tests.Infrastructure;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class SoakStabilityTests
{
    private static int Iterations
    {
        get
        {
            var raw = Environment.GetEnvironmentVariable("GSC_SOAK_ITERATIONS");
            return int.TryParse(raw, out var value) ? Math.Clamp(value, 20, 5000) : 100;
        }
    }

    [Fact]
    public async Task AcceleratedSoakCyclesLeaveNoResidueOrErrors()
    {
        using var harness = new SoakStabilityHarness();

        await harness.RunAsync(Iterations, CancellationToken.None);

        Assert.Equal(Iterations, harness.CompletedCycles);
        Assert.Empty(harness.Errors);
        Assert.Equal(0, harness.TempFileResidue);
        Assert.Equal(0, harness.SubscriberResidue);
        Assert.InRange(harness.TrackedLockGames, 1, 17);
        Assert.InRange(harness.ChangeFeedCount, 1, 500);
    }
}
