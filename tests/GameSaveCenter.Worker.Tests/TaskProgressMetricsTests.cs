using GameSaveCenter.Worker.Services;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class TaskProgressMetricsTests
{
    [Fact]
    public async Task WorkSamplerNeedsAdvancingSamplesAndClearsAtUnknownStage()
    {
        TaskProgressMetrics? observed = null;
        var reported = new List<int>();
        var progress = new TaskProgress(
            (percent, _) =>
            {
                reported.Add(percent);
                return Task.CompletedTask;
            },
            setMetrics: metrics => observed = metrics);

        await progress.ReportWorkAsync(0, 100, "文件", "扫描中");
        await Task.Delay(300);
        await progress.ReportWorkAsync(20, 100, "文件", "扫描中");

        Assert.NotNull(observed);
        Assert.Equal(20, observed!.CompletedUnits);
        Assert.Equal(100, observed.TotalUnits);
        Assert.Equal("文件", observed.Unit);
        Assert.True(observed.RatePerSecond > 0);
        Assert.NotNull(observed.EtaSeconds);
        Assert.Equal(20, reported[^1]);

        await progress.ReportAsync(55, "正在校验");

        Assert.Null(observed);
        Assert.Equal(55, reported[^1]);
    }
}
