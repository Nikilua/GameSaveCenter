using GameSaveCenter.Contracts;
using Xunit;

namespace GameSaveCenter.Core.Tests;

public sealed class UiDisplayMappingTests
{
    [Theory]
    [InlineData(1, 0, "校验失败")]
    [InlineData(0, 1, "上传失败")]
    [InlineData(2, 3, "校验/上传失败")]
    public void CloudSummarySeparatesCheckAndUploadFailures(int checkFailed, int failed, string expected)
    {
        var summary = new CloudTransferSummaryDto
        {
            TotalCount = checkFailed + failed,
            CheckFailedCount = checkFailed,
            FailedCount = failed
        };

        Assert.Equal(expected, summary.PrimaryStatusDisplay);
    }

    [Fact]
    public void UnknownCloudStateDoesNotLeakInternalValue()
    {
        var transfer = new CloudTransferStatusDto { State = "FutureProviderState" };

        Assert.Equal("未知状态", transfer.StateDisplay);
    }

    [Theory]
    [InlineData("Applied", "已应用")]
    [InlineData("FutureBatchState", "未知状态")]
    [InlineData("", "未知状态")]
    public void MediaClassificationHistoryUsesStableChineseState(string state, string expected)
    {
        var batch = new MediaClassificationBatchSummaryDto { State = state };
        var item = new MediaClassificationBatchItemResultDto { State = state };

        Assert.Equal(expected, batch.StateDisplay);
        Assert.Equal(expected, item.StateDisplay);
    }

    [Fact]
    public void BackupVersionDoesNotInventOriginOrReadinessFacts()
    {
        var backup = new BackupVersionDto
        {
            SourceDevice = string.Empty,
            OperatingSystem = string.Empty
        };

        Assert.Equal("未知设备", backup.SourceDisplay);
        Assert.Equal("未知系统", backup.OperatingSystemDisplay);
        Assert.Equal("未验证", backup.RestoreReadinessStatusDisplay);
        Assert.Equal("尚未检查", backup.RestoreReadinessCheckedDisplay);
    }

    [Theory]
    [InlineData(2_400_000, "+2.29 MiB")]
    [InlineData(-2_400_000, "-2.29 MiB")]
    [InlineData(0, "0 B")]
    public void BackupDiffShowsSignedSizeDelta(long delta, string expected)
    {
        Assert.Equal(expected, new BackupDiffDto { TotalBytesDelta = delta }.TotalBytesDeltaDisplay);
    }
}
