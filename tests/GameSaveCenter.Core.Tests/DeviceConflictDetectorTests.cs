using GameSaveCenter.Core.Models;
using GameSaveCenter.Core.Services;
using Xunit;
namespace GameSaveCenter.Core.Tests;
public sealed class DeviceConflictDetectorTests
{
    [Fact]
    public void MissingLocalSummaryIsNotAutoResolved()
    {
        var remote=new BackupSnapshot{BackupId="handheld",SourceDevice="HANDHELD",CreatedUtc=DateTime.UtcNow,TotalBytes=140};
        var conflict=new DeviceConflictDetector().Detect(null,remote);
        Assert.False(conflict.HasConflict);Assert.Equal("OnlyOneSideAvailable",conflict.Reason);
    }

    [Fact]
    public void DivergentDevicesAreNotAutoResolved()
    {
        var left=new BackupSnapshot{BackupId="desktop",SourceDevice="DESKTOP",CreatedUtc=DateTime.UtcNow,TotalBytes=100};
        var right=new BackupSnapshot{BackupId="handheld",SourceDevice="HANDHELD",CreatedUtc=DateTime.UtcNow.AddMinutes(5),TotalBytes=140};
        var conflict=new DeviceConflictDetector().Detect(left,right);
        Assert.True(conflict.HasConflict);Assert.True(string.IsNullOrEmpty(conflict.PreferredBackupId));
    }

    [Fact]
    public void BranchesFromTheSameBaseAreMarkedConflictWithoutChoosingWinner()
    {
        var left = new BackupSnapshot { BackupId = "A2", ParentBackupId = "V1", SourceDevice = "A", TotalBytes = 100, FileCount = 2 };
        var right = new BackupSnapshot { BackupId = "B2", ParentBackupId = "V1", SourceDevice = "B", TotalBytes = 110, FileCount = 3 };

        var conflict = new DeviceConflictDetector().Detect(left, right);

        Assert.True(conflict.HasConflict);
        Assert.Equal("DivergedFromCommonBase", conflict.Reason);
        Assert.True(string.IsNullOrWhiteSpace(conflict.PreferredBackupId));
    }

    [Fact]
    public void AChildOfTheKnownOtherVersionIsLinearNotConflict()
    {
        var local = new BackupSnapshot { BackupId = "A3", ParentBackupId = "A2", SourceDevice = "A", TotalBytes = 110, FileCount = 3 };
        var remote = new BackupSnapshot { BackupId = "A2", SourceDevice = "B", TotalBytes = 100, FileCount = 2 };

        var conflict = new DeviceConflictDetector().Detect(local, remote);

        Assert.False(conflict.HasConflict);
        Assert.Equal("LinearFromKnownBase", conflict.Reason);
    }

    [Fact]
    public void TimestampOnlyConflictNeverSuggestsAWinner()
    {
        var left = new BackupSnapshot { BackupId = "newer", SourceDevice = "A", CreatedUtc = DateTime.UtcNow, TotalBytes = 10, FileCount = 1 };
        var right = new BackupSnapshot { BackupId = "older", SourceDevice = "B", CreatedUtc = DateTime.UtcNow.AddHours(-1), TotalBytes = 12, FileCount = 2 };

        var conflict = new DeviceConflictDetector().Detect(left, right);

        Assert.True(conflict.HasConflict);
        Assert.True(string.IsNullOrWhiteSpace(conflict.PreferredBackupId));
    }

    [Fact]
    public void ContinuedIndependentA3AndB3RemainDivergedWithoutAutomaticWinner()
    {
        var detector = new DeviceConflictDetector();
        var a3 = new BackupSnapshot { BackupId = "A3", ParentBackupId = "A2", SourceDevice = "device-a", TotalBytes = 130, FileCount = 4 };
        var b3 = new BackupSnapshot { BackupId = "B3", ParentBackupId = "B2", SourceDevice = "device-b", TotalBytes = 150, FileCount = 5 };

        var conflict = detector.Detect(a3, b3);

        Assert.True(conflict.HasConflict);
        Assert.True(string.IsNullOrWhiteSpace(conflict.PreferredBackupId));
    }
}
