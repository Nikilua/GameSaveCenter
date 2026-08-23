using GameSaveCenter.Worker.Services;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class FlingTrainerCatalogSourceTests
{
    [Fact]
    public void ArchiveDirectoryListing_ProducesSearchableZipEntry()
    {
        const string html="""
            <html><body>
              <a href="Outlast%202%20v1.0-Update%202%20Plus%204%20Trainer.zip">Outlast 2 v1.0-Update 2 Plus 4 Trainer.zip</a>
              <a href="./readme.txt">readme.txt</a>
              <a href="subfolder/">subfolder/</a>
            </body></html>
            """;

        var item=Assert.Single(FlingTrainerCatalogSource.ParseArchiveCatalog(html,DateTime.UtcNow));

        Assert.Contains("Outlast 2",item.Title,StringComparison.OrdinalIgnoreCase);
        Assert.Equal("FLiNG 归档",item.SourceDisplay);
        Assert.EndsWith("Outlast 2 v1.0-Update 2 Plus 4 Trainer.zip",Uri.UnescapeDataString(item.PageUrl),StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ArchiveDirectoryListing_ExposesNestedDirectoriesAndKeepsExternalLinksOut()
    {
        const string html="""
            <html><body>
              <a href="2018/">2018/</a>
              <a href="2019/">2019/</a>
              <a href="https://example.com/other.zip">other.zip</a>
              <a href="readme.txt">readme.txt</a>
            </body></html>
            """;

        var listing=FlingTrainerCatalogSource.ParseArchiveDirectoryListing(
            html,new Uri("https://archive.flingtrainer.com/"),DateTime.UtcNow);

        Assert.Empty(listing.Files);
        Assert.Equal(2,listing.Directories.Count);
        Assert.All(listing.Directories,directory => Assert.Equal("archive.flingtrainer.com",directory.Host));
    }

    [Fact]
    public void ArchiveDirectoryListing_ResolvesFilesRelativeToNestedDirectory()
    {
        const string html="""
            <html><body>
              <a href="Outlast%202%20v1.02%20Trainer.zip">Outlast 2 v1.02 Trainer.zip</a>
            </body></html>
            """;

        var listing=FlingTrainerCatalogSource.ParseArchiveDirectoryListing(
            html,new Uri("https://archive.flingtrainer.com/2018/"),DateTime.UtcNow);

        var item=Assert.Single(listing.Files);
        Assert.EndsWith("/2018/Outlast 2 v1.02 Trainer.zip",Uri.UnescapeDataString(item.PageUrl),StringComparison.OrdinalIgnoreCase);
    }
}
