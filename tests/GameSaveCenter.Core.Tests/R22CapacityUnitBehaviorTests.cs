using GameSaveCenter.Contracts;
using Xunit;

namespace GameSaveCenter.Core.Tests;

public sealed class R22CapacityUnitBehaviorTests
{
    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(1, "1 B")]
    [InlineData(1023, "1023 B")]
    [InlineData(1024, "1 KiB")]
    [InlineData(1536, "1.5 KiB")]
    [InlineData(1024 * 1024, "1 MiB")]
    [InlineData(1024L * 1024 * 1024, "1 GiB")]
    public void CanonicalFormatterKeepsSmallValuesAndUsesBinaryUnits(long bytes, string expected)
    {
        Assert.Equal(expected, ByteSizeFormatter.Format(bytes));
    }

    [Theory]
    [InlineData(1, "+1 B")]
    [InlineData(-1, "-1 B")]
    [InlineData(0, "0 B")]
    public void SignedDeltaKeepsZeroAndSmallChangesUnambiguous(long bytes, string expected)
    {
        Assert.Equal(expected, ByteSizeFormatter.FormatDelta(bytes));
    }

    [Fact]
    public void ComparisonSummaryKeepsItsExistingPositiveZeroMarker()
    {
        Assert.Equal("+0 B", ByteSizeFormatter.FormatSignedDelta(0));
    }

    [Fact]
    public void BackupOverviewListDetailAndMediaSummaryShareTheSameCapacityPolicy()
    {
        var backup = new BackupVersionDto
        {
            TotalBytes = 1,
            RestoreReadiness = new RestoreReadinessDto
            {
                ActualTotalSize = 1,
                ExpectedTotalSize = 1024
            }
        };
        var media = new MediaStorageSummaryDto { TotalBytes = 1 };

        Assert.Equal("1 B", backup.SizeDisplay);
        Assert.Equal("文件 0/0 · 大小 1 B/1 KiB", backup.RestoreReadinessMetricsDisplay);
        Assert.Equal("1 B", media.TotalSizeDisplay);
    }

    [Fact]
    public void PreviewDiagnosticsAndTrainerSurfacesUseTheSameCapacityPolicy()
    {
        var preview = new MediaSourcePreviewItemDto { SizeBytes = 1 };
        var candidate = new GameToolEntryCandidateDto { RelativePath = "tool.exe", SizeBytes = 1 };
        var release = new TrainerReleaseDto { SizeBytes = 1 };
        var diagnostics = new DiagnosticsPackageResultDto { PackageBytes = 1, Summary = "已生成" };
        var metadata = new MetadataBackupResultDto { PackageBytes = 1 };

        Assert.Equal("1 B", preview.SizeDisplay);
        Assert.Equal("tool.exe · 1 B", candidate.Display);
        Assert.Equal("1 B", release.SizeDisplay);
        Assert.Contains("大小：1 B", diagnostics.ResultDisplay);
        Assert.DoesNotContain("0 KiB", diagnostics.ResultDisplay);
        Assert.Equal("1 B", metadata.SizeDisplay);
    }
}
