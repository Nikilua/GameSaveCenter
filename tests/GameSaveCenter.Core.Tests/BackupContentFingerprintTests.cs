using GameSaveCenter.Core.Models;
using GameSaveCenter.Core.Services;
using Xunit;

namespace GameSaveCenter.Core.Tests;

public sealed class BackupContentFingerprintTests
{
    [Fact]
    public void CompleteHashedManifestProducesOrderIndependentFingerprint()
    {
        var first = BackupContentFingerprint.Compute(new[]
        {
            new FileManifestEntry { RelativePath = "Save/A.dat", SizeBytes = 4, Sha256 = "aa" },
            new FileManifestEntry { RelativePath = "Save/B.dat", SizeBytes = 8, Sha256 = "bb" }
        });
        var second = BackupContentFingerprint.Compute(new[]
        {
            new FileManifestEntry { RelativePath = "save\\b.dat", SizeBytes = 8, Sha256 = "BB", LastWriteUtc = DateTime.UtcNow.AddDays(1) },
            new FileManifestEntry { RelativePath = "save\\a.dat", SizeBytes = 4, Sha256 = "AA", LastWriteUtc = DateTime.UtcNow.AddDays(2) }
        });

        Assert.False(string.IsNullOrWhiteSpace(first));
        Assert.Equal(first, second);
    }

    [Fact]
    public void MissingHashDoesNotCreateStrongEvidence()
    {
        var fingerprint = BackupContentFingerprint.Compute(new[]
        {
            new FileManifestEntry { RelativePath = "save.dat", SizeBytes = 4 }
        });

        Assert.Equal(string.Empty, fingerprint);
    }

    [Fact]
    public void InvalidManifestDoesNotCreateStrongEvidence()
    {
        var fingerprint = BackupContentFingerprint.Compute(new[]
        {
            new FileManifestEntry { RelativePath = "save.dat", SizeBytes = 4, Sha256 = "aa" },
            new FileManifestEntry { RelativePath = "save.dat/", SizeBytes = 4, Sha256 = "aa" }
        });

        Assert.Equal(string.Empty, fingerprint);
    }
}
