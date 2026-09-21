using System;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class OverviewSnapshotDisplayTests
{
    [Fact]
    public void UnloadedSnapshotDoesNotPresentDefaultZerosAsFacts()
    {
        Assert.Equal("—", OverviewSnapshotDisplay.Count(false, 0));
        Assert.Equal("全库 · 尚未加载；数字显示 —，不代表 0", OverviewSnapshotDisplay.Scope(false, DateTime.UtcNow));
        Assert.Equal("当前游戏 · 尚未加载；数字显示 —，不代表 0", OverviewSnapshotDisplay.CurrentGameScope(false, DateTime.UtcNow));
        Assert.Equal("更新时间未知", OverviewSnapshotDisplay.Updated(false, DateTime.UtcNow));
    }

    [Fact]
    public void LoadedSnapshotKeepsLegitimateZeroAndGenerationTime()
    {
        var generatedUtc = new DateTime(2026, 9, 20, 8, 30, 0, DateTimeKind.Utc);

        Assert.Equal("0", OverviewSnapshotDisplay.Count(true, 0));
        Assert.Contains(OverviewSnapshotDisplay.UpdatedRelative(true, generatedUtc), OverviewSnapshotDisplay.Scope(true, generatedUtc), StringComparison.Ordinal);
        Assert.Contains(OverviewSnapshotDisplay.UpdatedRelative(true, generatedUtc), OverviewSnapshotDisplay.CurrentGameScope(true, generatedUtc), StringComparison.Ordinal);
        Assert.Contains("更新于 ", OverviewSnapshotDisplay.Updated(true, generatedUtc), StringComparison.Ordinal);
        Assert.Equal($"更新于 {TimeDisplayFormatter.Full(generatedUtc)}", OverviewSnapshotDisplay.UpdatedFull(true, generatedUtc));
        Assert.Equal(TimeDisplayFormatter.RawUtc(generatedUtc), OverviewSnapshotDisplay.UpdatedRawUtc(true, generatedUtc));
    }

    [Fact]
    public void DefaultGenerationTimeRemainsUnknownEvenAfterLoad()
    {
        Assert.Equal("更新时间未知", OverviewSnapshotDisplay.Updated(true, default));
        Assert.Equal("更新时间未知", OverviewSnapshotDisplay.UpdatedRelative(true, default));
        Assert.Equal("未记录 UTC 时间", OverviewSnapshotDisplay.UpdatedRawUtc(true, default));
    }
}
