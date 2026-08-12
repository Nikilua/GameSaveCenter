using GameSaveCenter.Core.Models;
using GameSaveCenter.Core.Services;
using Xunit;
namespace GameSaveCenter.Core.Tests;
public sealed class FileManifestDiffTests
{
    [Fact]
    public void FindsAddedRemovedAndModifiedFiles()
    {
        var before=new[]{new FileManifestEntry{RelativePath="a.sav",SizeBytes=1},new FileManifestEntry{RelativePath="old.sav",SizeBytes=1}};
        var after=new[]{new FileManifestEntry{RelativePath="a.sav",SizeBytes=2},new FileManifestEntry{RelativePath="new.sav",SizeBytes=1}};
        var diff=new FileManifestDiffService().Compare(before,after);
        Assert.Single(diff.Modified);Assert.Single(diff.Removed);Assert.Single(diff.Added);
        Assert.False(diff.IsExactComparison);
        Assert.Equal(2, diff.BeforeTotalBytes);
        Assert.Equal(3, diff.AfterTotalBytes);
    }

    [Fact]
    public void HashCompleteManifestsAreMarkedExactAndTrackSizeDelta()
    {
        var diff = new FileManifestDiffService().Compare(
            new[] { new FileManifestEntry { RelativePath = "save.dat", SizeBytes = 10, Sha256 = "a" } },
            new[] { new FileManifestEntry { RelativePath = "save.dat", SizeBytes = 18, Sha256 = "b" } });

        Assert.True(diff.IsExactComparison);
        Assert.Equal(10, diff.BeforeTotalBytes);
        Assert.Equal(18, diff.AfterTotalBytes);
    }
}
