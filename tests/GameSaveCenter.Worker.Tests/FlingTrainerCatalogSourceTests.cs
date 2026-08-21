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
}
