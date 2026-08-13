using GameSaveCenter.Worker.Tests.Infrastructure;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class FaultInjectionTests
{
    [Fact]
    public async Task InjectedFaultsRecoverWithoutResidueOrUnexpectedErrors()
    {
        using var harness = new FaultInjectionHarness();

        await harness.RunAsync(CancellationToken.None);

        Assert.Empty(harness.Errors);
        Assert.Equal(harness.Attempted, harness.Recovered);
        Assert.True(harness.Attempted >= 15, $"Expected at least 15 injected faults, got {harness.Attempted}");
    }
}
