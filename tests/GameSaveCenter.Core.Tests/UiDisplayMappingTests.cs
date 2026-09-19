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
    public void CloudSummaryKeepsUploadedAndRemoteVerifiedCountersSeparate()
    {
        var summary = new CloudTransferSummaryDto
        {
            UploadedCount = 2,
            VerifiedCount = 3
        };

        Assert.Equal("已上传 2 · 已校验 3", summary.GuaranteeDisplay);
    }

    [Theory]
    [InlineData("Pending", "", "等待队列")]
    [InlineData("RetryScheduled", "RCLONE_NETWORK_FAILED", "等待网络")]
    [InlineData("RetryScheduled", "RCLONE_AUTH_FAILED", "等待重试")]
    [InlineData("Transferring", "", "上传中")]
    [InlineData("Verifying", "", "验证中")]
    [InlineData("RemoteVerified", "", "已验证")]
    [InlineData("Uploaded", "", "等待验证")]
    public void CloudTransferExposesTheActualQueuePhase(string state, string errorCode, string expected)
    {
        var transfer = new CloudTransferStatusDto { State = state, LastErrorCode = errorCode };

        Assert.Equal(expected, transfer.QueuePhaseDisplay);
    }

    [Fact]
    public void CloudSummarySeparatesWaitingWindowFromRunningQueue()
    {
        var paused = new CloudTransferSummaryDto { QueuePaused = true, TotalCount = 1 };
        var outsideWindow = new CloudTransferSummaryDto { OutsideAllowedWindow = true };
        var idle = new CloudTransferSummaryDto();
        var running = new CloudTransferSummaryDto { TotalCount = 1 };

        Assert.Equal("自动队列已暂停", paused.QueueControlDisplay);
        Assert.Equal("当前不在允许时段", outsideWindow.QueueControlDisplay);
        Assert.Equal("队列空闲", idle.QueueControlDisplay);
        Assert.Equal("自动队列运行中", running.QueueControlDisplay);
    }

    [Fact]
    public void CloudRetryTimingClampsExpiredAndKeepsAbsoluteTime()
    {
        var future = new CloudTransferStatusDto { NextAttemptUtc = DateTime.UtcNow.AddMinutes(5) };
        var expired = new CloudTransferStatusDto { NextAttemptUtc = DateTime.UtcNow.AddMinutes(-5) };

        Assert.Contains("·", future.RetryTimingDisplay);
        Assert.Contains("后", future.RetryTimingDisplay);
        Assert.Contains("可立即重试", expired.RetryTimingDisplay);
        Assert.DoesNotContain("负", expired.RetryTimingDisplay);
    }

    [Fact]
    public void CloudRetryTimingKeepsNoRetryDistinctFromAnExpiredRetry()
    {
        var noRetry = new CloudTransferStatusDto();

        Assert.Equal("无自动重试", noRetry.RetryTimingDisplay);
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
        Assert.Equal("未提供哈希（不等于校验成功）", backup.RestoreReadinessHashValidationDisplay);
        Assert.Equal("哈希覆盖：未提供", backup.RestoreReadinessHashCoverageDisplay);
        Assert.Equal("尚未检查", backup.RestoreReadinessCheckedDisplay);
    }

    [Fact]
    public void RestoreReadinessDisplaySeparatesPartialHashesAndOldResults()
    {
        var backup = new BackupVersionDto
        {
            RestoreReadiness = new RestoreReadinessDto
            {
                HashValidation = "Partial",
                HashCoveredFileCount = 1,
                HashEligibleFileCount = 2,
                CheckedUtc = DateTime.UtcNow.AddDays(-2)
            }
        };

        Assert.Equal("哈希部分覆盖", backup.RestoreReadinessHashValidationDisplay);
        Assert.Equal("哈希覆盖：1/2 个文件", backup.RestoreReadinessHashCoverageDisplay);
        Assert.Contains("结果较旧", backup.RestoreReadinessCheckedDisplay);
    }

    [Theory]
    [InlineData(2_400_000, "+2.29 MiB")]
    [InlineData(-2_400_000, "-2.29 MiB")]
    [InlineData(0, "0 B")]
    public void BackupDiffShowsSignedSizeDelta(long delta, string expected)
    {
        Assert.Equal(expected, new BackupDiffDto { TotalBytesDelta = delta }.TotalBytesDeltaDisplay);
    }

    [Theory]
    [InlineData(0, "未知大小")]
    [InlineData(-1, "未知大小")]
    [InlineData(1024, "1 KiB")]
    public void TrainerReleaseKeepsUnknownSizeDistinctFromMeasuredSize(long sizeBytes, string expected)
    {
        Assert.Equal(expected, new TrainerReleaseDto { SizeBytes = sizeBytes }.SizeDisplay);
    }

    [Fact]
    public void BackupZeroAndReadinessZeroRemainVisibleAsRealValues()
    {
        var backup = new BackupVersionDto
        {
            TotalBytes = 0,
            RestoreReadiness = new RestoreReadinessDto
            {
                ActualFileCount = 0,
                ExpectedFileCount = 0,
                ActualTotalSize = 0,
                ExpectedTotalSize = 0
            }
        };

        Assert.Equal("0 B", backup.SizeDisplay);
        Assert.Equal("文件 0/0 · 大小 0 B/0 B", backup.RestoreReadinessMetricsDisplay);
    }

    [Fact]
    public void MixedPathAndProductTermsKeepOriginalPathAndSemanticSeparator()
    {
        var candidate = new GameToolEntryCandidateDto
        {
            RelativePath = "工具/FLiNG Trainer v1.2/启动器.exe",
            SizeBytes = 1024
        };

        Assert.Equal("启动器.exe", candidate.FileName);
        Assert.Equal("工具/FLiNG Trainer v1.2/启动器.exe · 1 KiB", candidate.Display);
    }

    [Fact]
    public void MixedLanguageDisplaySurfacesKeepSemanticSpacingAndProductTerms()
    {
        var cloud = new CloudTransferSummaryDto
        {
            TotalCount = 2,
            FailedCount = 2
        };
        var suggestion = new MediaClassificationSuggestionDto
        {
            Confidence = "High",
            SuggestedGameName = "Cyberpunk 2077",
            Reason = "来源含 FLiNG Trainer"
        };
        var batch = new MediaClassificationBatchSummaryDto
        {
            State = "AppliedWithConflicts"
        };
        var backup = new BackupVersionDto
        {
            RestoreReadiness = new RestoreReadinessDto
            {
                ActualFileCount = 2,
                ExpectedFileCount = 3,
                ActualTotalSize = 1024,
                ExpectedTotalSize = 2048
            }
        };
        var protection = new GameSaveCenter.Core.Services.RecentProtectionSummary(
            windowDays: 7,
            recentlyPlayedGames: 3,
            protectedGames: 1,
            attentionGames: 2,
            unrecognizedSaveGames: 0,
            items: System.Array.Empty<GameSaveCenter.Core.Services.RecentProtectionItem>());
        var media = new MediaItemDto { Source = MediaSourceKind.XboxGameBar };
        var trainer = new TrainerReleaseDto { DisplayName = "FLiNG Trainer v1.2 Plus 30" };

        var separated = new[]
        {
            cloud.SummaryDisplay,
            suggestion.SummaryDisplay,
            batch.StateDisplay,
            backup.RestoreReadinessMetricsDisplay,
            protection.SummaryDisplay
        };

        Assert.Equal("上传失败 · 2 项", cloud.SummaryDisplay);
        Assert.Equal("高置信 · Cyberpunk 2077 · 来源含 FLiNG Trainer", suggestion.SummaryDisplay);
        Assert.Equal("已应用 · 有冲突", batch.StateDisplay);
        Assert.Equal("文件 2/3 · 大小 1 KiB/2 KiB", backup.RestoreReadinessMetricsDisplay);
        Assert.Equal("共 3 个 · 已保护 1 个 · 需处理 2 个", protection.SummaryDisplay);
        Assert.Equal("Xbox Game Bar", media.SourceDisplay);
        Assert.Equal("+30 项", trainer.OptionCountDisplay);
        Assert.All(separated, value =>
        {
            Assert.DoesNotContain("··", value);
            Assert.DoesNotContain("  ·", value);
            Assert.DoesNotContain("·  ", value);
        });
    }
}
