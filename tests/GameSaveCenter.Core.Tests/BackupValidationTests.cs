using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Models;
using GameSaveCenter.Core.Services;
using Xunit;
namespace GameSaveCenter.Core.Tests;
public sealed class BackupValidationTests
{
    [Fact]
    public void EmptyBackupIsCritical()
    {
        var findings=new BackupValidationService().Validate(new BackupSnapshot(),null,null,true);
        Assert.Contains(findings,x=>x.Code=="EMPTY_BACKUP");
    }

    [Fact]
    public void LargeSizeDropIsDetected()
    {
        var previous=new BackupSnapshot{FileCount=10,TotalBytes=1000};
        var current=new BackupSnapshot{FileCount=10,TotalBytes=200};
        var findings=new BackupValidationService().Validate(current,previous,null,true);
        Assert.Contains(findings,x=>x.Code=="BACKUP_SIZE_DROP");
    }

    [Fact]
    public void FileRemovalSpikeIsDetectedFromVersionManifest()
    {
        var previous = new BackupSnapshot { FileCount = 10, TotalBytes = 1000 };
        for (var i = 0; i < 10; i++) previous.Files.Add(new FileManifestEntry { RelativePath = $"slot-{i}.sav", SizeBytes = 100 });
        var current = new BackupSnapshot { FileCount = 2, TotalBytes = 200 };
        current.Files.AddRange(previous.Files.Take(2));

        var findings = new BackupValidationService().Validate(current, previous, null, true);

        Assert.Contains(findings, x => x.Code == "BACKUP_FILE_REMOVAL_SPIKE");
    }

    [Fact]
    public void ProtectionLevelControlsComparativeAnomalySensitivity()
    {
        var previous = new BackupSnapshot { FileCount = 10, TotalBytes = 1000 };
        var current = new BackupSnapshot { FileCount = 6, TotalBytes = 500 };
        var validator = new BackupValidationService();

        Assert.DoesNotContain(validator.Validate(current, previous, null, true, BackupAnomalyProtectionLevel.Normal), x => x.Code is "FILE_COUNT_DROP" or "BACKUP_SIZE_DROP");
        Assert.Contains(validator.Validate(current, previous, null, true, BackupAnomalyProtectionLevel.Strict), x => x.Code == "FILE_COUNT_DROP");
        Assert.DoesNotContain(validator.Validate(current, previous, null, true, BackupAnomalyProtectionLevel.Off), x => x.Code is "FILE_COUNT_DROP" or "BACKUP_SIZE_DROP");
    }
}
