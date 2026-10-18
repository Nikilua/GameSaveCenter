using System;
using System.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class DeviceConflictComparisonDisplayBehaviorTests
{
    [Fact]
    public void SideBySideEvidenceShowsCommonBaseFreshnessAndNoTimestampWinner()
    {
        var now = new DateTime(2026, 10, 19, 8, 0, 0, DateTimeKind.Utc);
        var comparison = new DeviceConflictStatusDto
        {
            LocalDevice = "DESKTOP-01",
            RemoteDevice = "LAPTOP-02",
            LocalBackupId = "local-02",
            RemoteBackupId = "remote-02",
            LocalParentBackupId = "base-01",
            RemoteParentBackupId = "base-01",
            LocalCreatedUtc = now,
            RemoteCreatedUtc = now.AddDays(-3),
            LocalTotalBytes = 4 * 1024,
            RemoteTotalBytes = 5 * 1024,
            LocalFileCount = 12,
            RemoteFileCount = 14,
            LocalContentFingerprintAvailable = true,
            RemoteContentFingerprintAvailable = true,
            ContentFingerprintsMatch = false,
            LocalReadinessStatus = "Ready",
            RemoteReadinessStatus = "Warning",
            LocalReadinessCheckedUtc = now.AddDays(-2),
            RemoteReadinessCheckedUtc = now.AddDays(-41),
            LocalSummaryGeneratedUtc = now.AddMinutes(-10),
            RemoteSummaryGeneratedUtc = now.AddDays(-4),
            HasConflict = true,
            Reason = "DivergedFromCommonBase",
            Confidence = 0.99
        };

        var lines = DeviceConflictComparisonDisplayBuilder.Build(comparison, now);
        var text = string.Join("\n", lines);

        Assert.Contains("两个版本从同一父版本分叉", text);
        Assert.Contains("本机版本：local-02 · DESKTOP-01", text);
        Assert.Contains("远端版本：remote-02 · LAPTOP-02", text);
        Assert.Contains("共同父版本：base-01", text);
        Assert.Contains("内容指纹：两侧不同", text);
        Assert.Contains("本机恢复可用性校验：可恢复", text);
        Assert.Contains("远端恢复可用性校验：有警告", text);
        Assert.Contains("41 天前", text);
        Assert.Contains("系统不会因为创建时间较新就推荐覆盖另一侧", text);
        Assert.DoesNotContain("推荐版本：", text);
    }

    [Fact]
    public void MissingLegacyEvidenceRemainsExplicitlyUnknown()
    {
        var comparison = new DeviceConflictStatusDto
        {
            HasConflict = true,
            Reason = "UnknownDivergence"
        };

        var text = string.Join("\n", DeviceConflictComparisonDisplayBuilder.Build(
            comparison,
            new DateTime(2026, 10, 19, 8, 0, 0, DateTimeKind.Utc)));

        Assert.Contains("父版本关系：未知", text);
        Assert.Contains("内容指纹：未知", text);
        Assert.Contains("本机恢复可用性校验：未提供", text);
        Assert.Contains("校验时间未知（旧设备摘要可能未包含此字段）", text);
        Assert.Contains("远端设备摘要发布于：时间未知", text);
        Assert.Contains("两侧摘要字段相同，但内容指纹证据缺失；冲突原因未知。", text);
    }

    [Fact]
    public void DetectorReasonsHaveLocalizedMeaningForTheComparisonRow()
    {
        Assert.Equal("两个版本从同一父版本分叉", new DeviceConflictStatusDto { Reason = "DivergedFromCommonBase" }.ReasonDisplay);
        Assert.Equal("一侧版本明确基于另一侧版本继续创建", new DeviceConflictStatusDto { Reason = "LinearFromKnownBase" }.ReasonDisplay);
        Assert.Equal("检测器返回了未识别的原因；需要人工查看两侧证据", new DeviceConflictStatusDto { Reason = "FutureReason" }.ReasonDisplay);
        Assert.Equal("两侧摘要字段相同，但内容指纹不同；父版本关系未知。", new DeviceConflictStatusDto
        {
            Reason = "UnknownDivergence",
            LocalContentFingerprintAvailable = true,
            RemoteContentFingerprintAvailable = true,
            ContentFingerprintsMatch = false
        }.ReasonDisplay);
    }
}
