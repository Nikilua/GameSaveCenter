using System.Text;
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
    public void ArchiveDirectoryListing_ProducesSearchableRarEntry()
    {
        const string html="""
            <html><body>
              <a href="files/Devil%20May%20Cry%204%20Special%20Edition%20v20190328%20Plus%2020%20Trainer.rar">Devil May Cry 4 Special Edition v20190328 Plus 20 Trainer.rar</a>
            </body></html>
            """;

        var listing=FlingTrainerCatalogSource.ParseArchiveDirectoryListing(
            html,new Uri("https://archive.flingtrainer.com/"),DateTime.UtcNow);

        var item=Assert.Single(listing.Files);
        Assert.Contains("Devil May Cry 4 Special Edition",item.Title,StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith(".rar",item.PageUrl,StringComparison.OrdinalIgnoreCase);
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

    [Fact]
    public void OnlineCatalog_BeatsProductionGuardThreshold_AndDeduplicates()
    {
        var html=new StringBuilder();
        for(var i=0;i<120;i++)
        {
            html.Append($"<a href=\"https://flingtrainer.com/trainer/{i:D3}.html\"><strong>Game{i} Trainer v1.0</strong></a>\n");
        }
        // Same trainer referenced twice with different URL casing must collapse to one item.
        html.Append("<a href=\"https://flingtrainer.com/trainer/005.HTML\">Game5 Trainer v1.0 Plus 1</a>");

        var items=FlingTrainerCatalogSource.ParseOnlineCatalog(html.ToString(),DateTime.UtcNow);

        Assert.Equal(120,items.Count);
        Assert.All(items,item =>
        {
            Assert.StartsWith("https://flingtrainer.com/trainer/",item.PageUrl,StringComparison.OrdinalIgnoreCase);
            Assert.False(string.IsNullOrWhiteSpace(item.Title));
            Assert.False(string.IsNullOrWhiteSpace(item.CatalogId));
        });
        // The 005.HTML duplicate collapsed onto 005.html, and the first occurrence's
        // title (without the "Plus 1" suffix) is preserved because First() wins the group.
        var five=Assert.Single(items,item=>string.Equals(item.PageUrl,"https://flingtrainer.com/trainer/005.html",StringComparison.OrdinalIgnoreCase));
        Assert.Equal("Game5 Trainer v1.0",five.Title);
        Assert.Contains(items,item=>item.Title.Contains("Game119",StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void OnlineCatalog_GuardThresholdFails_WhenMarkupChanges()
    {
        // A page that no longer matches the trainer link pattern must parse below the guard.
        var items=FlingTrainerCatalogSource.ParseOnlineCatalog("<html><body><a href=\"/category/foo\">Other content</a></body></html>",DateTime.UtcNow);
        Assert.Empty(items);
    }

    [Fact]
    public void TrainerDetailsPage_ParsesDownloadLinks_WithFallbackNameAndDedup()
    {
        // Single-line escaped string literals, not raw/verbatim: the source gate's delimiter
        // scanner approximates C# lexing and would misread https:// inside raw content as a
        // line comment, which cascades into false delimiter mismatches.
        const string html =
            "<html><body>\n" +
            "<a href=\"https://flingtrainer.com/downloads/Game_Trainer_v1.0.zip\"><b>Game Trainer v1.0</b></a>\n" +
            "<a href=\"https://flingtrainer.com/downloads/Game_Trainer_v2.0.zip\">Game Trainer v2.0</a>\n" +
            "<a href=\"https://flingtrainer.com/downloads/Game_Trainer_v2.0.zip\">Game Trainer v2.0 Duplicate</a>\n" +
            "<a href=\"https://flingtrainer.com/downloads/Game_Trainer_latest.rar\"></a>\n" +
            "<a href=\"https://example.com/offsite.zip\">Offsite ignored</a>\n" +
            "</body></html>";

        var releases=FlingTrainerCatalogSource.ParseReleases(html,"canary-catalog-id");

        Assert.Equal(3,releases.Count);
        Assert.All(releases,release =>
        {
            Assert.Equal("canary-catalog-id",release.CatalogId);
            Assert.Matches("^[0-9a-f]{24}$",release.ReleaseId);
            Assert.StartsWith("https://flingtrainer.com/downloads/",release.DownloadUrl,StringComparison.OrdinalIgnoreCase);
        });
        Assert.Contains(releases,release=>release.DownloadUrl.Contains("v1.0.zip",StringComparison.OrdinalIgnoreCase));
        Assert.Contains(releases,release=>release.DownloadUrl.Contains("v2.0.zip",StringComparison.OrdinalIgnoreCase));
        Assert.Contains(releases,release=>release.DownloadUrl.Contains("latest.rar",StringComparison.OrdinalIgnoreCase));
        // Empty anchor text falls back to the generic label instead of producing a blank row.
        Assert.Single(releases,release=>string.Equals(release.DisplayName,"FLiNG Trainer",StringComparison.Ordinal));
        Assert.DoesNotContain(releases,release=>release.DownloadUrl.Contains("example.com",StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void OnlineCatalog_NormalizesRelativeLinks_AndStripsTrackingQuery()
    {
        const string html =
            "<a class=\"trainer\" href=\"/trainer/relative%20game.html?utm_source=listing#top\"><span>Relative &amp; Game</span></a>\n" +
            "<a href=\"//www.flingtrainer.com/trainer/www-game.html\">WWW Game</a>\n" +
            "<a href=\"https://example.com/trainer/offsite.html\">Offsite</a>\n" +
            "<a href=\"https://flingtrainer.com.evil.example/trainer/fake.html\">Fake FLiNG</a>";

        var items=FlingTrainerCatalogSource.ParseOnlineCatalog(html,DateTime.UtcNow);

        Assert.Equal(2,items.Count);
        var relative=Assert.Single(items,item=>item.Title=="Relative & Game");
        Assert.Equal("https://flingtrainer.com/trainer/relative%20game.html",relative.PageUrl);
        Assert.DoesNotContain("?",relative.PageUrl,StringComparison.Ordinal);
        Assert.DoesNotContain("example.com",string.Join("\n",items.Select(item=>item.PageUrl)),StringComparison.OrdinalIgnoreCase);
        Assert.Contains(items,item=>item.PageUrl.StartsWith("https://www.flingtrainer.com/trainer/",StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void TrainerDetailsPage_ResolvesRelativeDownload_AndKeepsQueryWithoutFragment()
    {
        const string html =
            "<a href=\"../downloads/Game_%26_v1.zip?token=abc&amp;part=1#download\">Game Trainer</a>\n" +
            "<a href=\"https://flingtrainer.com/downloads/Game_%26_v1.zip?token=abc&amp;part=1\">Duplicate</a>\n" +
            "<a href=\"https://example.com/downloads/offsite.zip\">Offsite</a>";

        var releases=FlingTrainerCatalogSource.ParseReleases(
            html,"canary-catalog-id",new Uri("https://flingtrainer.com/trainer/game.html"));

        var release=Assert.Single(releases);
        Assert.StartsWith("https://flingtrainer.com/downloads/Game_%26_v1.zip",release.DownloadUrl,StringComparison.OrdinalIgnoreCase);
        Assert.Contains("token=abc&part=1",release.DownloadUrl,StringComparison.Ordinal);
        Assert.DoesNotContain("#",release.DownloadUrl,StringComparison.Ordinal);
    }
}
