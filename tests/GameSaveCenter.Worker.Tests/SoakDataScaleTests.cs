using GameSaveCenter.Worker.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace GameSaveCenter.Worker.Tests;

public sealed class SoakDataScaleTests
{
    private readonly ITestOutputHelper output;

    public SoakDataScaleTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public async Task DataScaleSoakRemainsBounded()
    {
        var fullScale = string.Equals(Environment.GetEnvironmentVariable("GSC_SOAK_DATA_SCALE"), "1", StringComparison.OrdinalIgnoreCase);
        using var harness = new SoakDataScaleHarness();

        await harness.RunAsync(fullScale, CancellationToken.None);

        Assert.Empty(harness.Errors);
        Assert.True(harness.BoundedGrowth, harness.GrowthSummary);
        Assert.Equal(0, harness.SubscriberResidue);
        Assert.Equal(0, harness.TempResidue);
        output.WriteLine($"[PERF] WorkerSoak fullScale={fullScale} " + harness.GrowthSummary);
    }
}
