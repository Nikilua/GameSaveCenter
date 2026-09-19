using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Services;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class BackupPreviewBehaviorTests
{
    [Fact]
    public void PreviewJsonIsConvertedToBoundedPathSummaryWithoutCreatingBackupMetadata()
    {
        using var document = JsonDocument.Parse("""
        {
          "games": {
            "Demo Game": {
              "files": {
                "profiles/save-01.dat": { "bytes": 128 },
                "settings/user.json": { "bytes": 256 }
              }
            }
          }
        }
        """);

        var snapshot = LudusaviResultParser.ParseOperationSnapshot(
            document.RootElement,
            "Demo Game",
            "preview",
            DateTime.UtcNow);
        var preview = new BackupPreviewDto
        {
            PlayniteId = "game-1",
            GameName = "Demo Game",
            State = snapshot.FileCount > 0 ? "Ready" : "NoData",
            GeneratedUtc = DateTime.UtcNow,
            PathCount = snapshot.FileCount,
            TotalBytes = snapshot.TotalBytes,
            Paths = snapshot.Files.ConvertAll(file => new BackupPreviewPathDto { Path = file.RelativePath, SizeBytes = file.SizeBytes })
        };

        Assert.Equal(2, preview.PathCount);
        Assert.Equal(384, preview.TotalBytes);
        Assert.Equal("已生成预览", preview.StateDisplay);
        Assert.Contains("profiles/save-01.dat", preview.IdentifiedPathsDisplay);
        Assert.Contains("settings/user.json", preview.IdentifiedPathsDisplay);
        Assert.DoesNotContain("BackupId", preview.IdentifiedPathsDisplay);
    }

    [Fact]
    public void PreviewFailureSignalStaysDistinctFromAnEmptyScan()
    {
        using var document = JsonDocument.Parse("""
        { "errors": { "someGamesFailed": true } }
        """);

        Assert.True(LudusaviResultParser.SomeGamesFailed(document.RootElement));
        Assert.Equal("没有可纳入路径", new BackupPreviewDto { State = "NoData" }.StateDisplay);
        Assert.Equal("预览失败", new BackupPreviewDto { State = "Error" }.StateDisplay);
    }
}
