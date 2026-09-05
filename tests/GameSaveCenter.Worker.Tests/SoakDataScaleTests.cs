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
        var profile = Environment.GetEnvironmentVariable("GSC_SOAK_DATA_SCALE") ?? "0";
        using var harness = new SoakDataScaleHarness();

        await harness.RunAsync(profile, CancellationToken.None);

        Assert.Empty(harness.Errors);
        Assert.True(harness.BoundedGrowth, harness.GrowthSummary);
        Assert.Equal(0, harness.SubscriberResidue);
        Assert.Equal(0, harness.TempResidue);
        output.WriteLine($"[PERF] WorkerSoak profile={harness.DataScaleProfile} games={harness.GameCount} backups={harness.BackupCount} tasks={harness.TaskCount} media={harness.MediaCount} tools={harness.ToolCount} seedMs={harness.SeedDurationMilliseconds} queryMs={harness.FirstGamesQueryMilliseconds}/{harness.WarmGamesQueryMilliseconds} taskPageMs={harness.FirstTaskPageQueryMilliseconds}/{harness.WarmTaskPageQueryMilliseconds}/{harness.NextTaskPageQueryMilliseconds} taskSearchMs={harness.TaskSearchQueryMilliseconds} mediaPageMs={harness.FirstMediaPageQueryMilliseconds}/{harness.WarmMediaPageQueryMilliseconds}/{harness.NextMediaPageQueryMilliseconds} mediaSearchMs={harness.MediaSearchQueryMilliseconds} simulationMs={harness.SimulationDurationMilliseconds} queryAllocatedBytes={harness.QueryAllocatedBytes} simulationAllocatedBytes={harness.SimulationAllocatedBytes} " + harness.GrowthSummary);
        var artifactRoot = Environment.GetEnvironmentVariable("GSC_TEST_ARTIFACT_ROOT");
        if (!string.IsNullOrWhiteSpace(artifactRoot))
            harness.WriteReport(artifactRoot);
    }
}
