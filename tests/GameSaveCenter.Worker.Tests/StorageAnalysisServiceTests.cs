using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class StorageAnalysisServiceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly WorkerOptions options;
    private readonly SqliteStateStore store;

    public StorageAnalysisServiceTests()
    {
        options = new WorkerOptions
        {
            DataDirectory = Path.Combine(root, "Data"),
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            MediaArchiveDirectory = Path.Combine(root, "Media")
        };
        Directory.CreateDirectory(options.DataDirectory);
        Directory.CreateDirectory(options.LudusaviBackupDirectory);
        Directory.CreateDirectory(options.MediaArchiveDirectory);
        store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task AnalyzeReportsVolumeIndexedBytesAndTopGames()
    {
        var archiveDir = Path.Combine(options.LudusaviBackupDirectory, "game");
        Directory.CreateDirectory(archiveDir);
        await File.WriteAllBytesAsync(Path.Combine(archiveDir, "b1.zip"), new byte[2048]);
        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            PlayniteId = "g1",
            BackupId = "b1",
            LudusaviName = "Game One",
            CreatedUtc = DateTime.UtcNow.AddDays(-2),
            TotalBytes = 1024,
            FileCount = 1,
            ArchivePath = Path.Combine(archiveDir, "b1.zip")
        }, "{}", CancellationToken.None);
        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            PlayniteId = "g1",
            BackupId = "b2",
            LudusaviName = "Game One",
            CreatedUtc = DateTime.UtcNow.AddDays(-20),
            TotalBytes = 4096,
            FileCount = 2,
            ArchivePath = Path.Combine(archiveDir, "b2.zip")
        }, "{}", CancellationToken.None);

        var service = new StorageAnalysisService(options, store, NullLogger<StorageAnalysisService>.Instance);
        var result = await service.AnalyzeAsync(CancellationToken.None);

        Assert.True(result.BackupDirectoryAvailable);
        Assert.Equal(2, result.BackupVersionCount);
        Assert.Equal(5120, result.IndexedBackupBytes);
        Assert.True(result.RepositoryBytes >= 2048);
        Assert.Equal(1, result.MissingIndexedPathCount);
        Assert.Equal(4096, result.MissingIndexedBytes);
        Assert.Contains("失联", result.MissingIndexedPathSummary);
        Assert.Equal(3, result.Trends.Count);
        Assert.Equal(1, result.Trends[0].AddedVersionCount);
        Assert.Equal(2, result.Trends[1].AddedVersionCount);
        var top = Assert.Single(result.TopGames);
        Assert.Equal("Game One", top.GameName);
        Assert.Equal(5120, top.BackupBytes);
        Assert.Equal(2, top.BackupCount);
        Assert.Equal("b1", top.LatestBackupId);
        Assert.Contains("历史统计", result.Summary);
    }

    [Fact]
    public async Task AnalyzeBreaksDownArchiveMirrorStagingAndMediaWithoutSummingCopies()
    {
        options.EnableLocalMirror = true;
        options.LocalMirrorPath = Path.Combine(root, "Mirror");
        Directory.CreateDirectory(options.LocalMirrorPath);
        Directory.CreateDirectory(options.RemoteBackupStagingDirectory);

        var archivePath = Path.Combine(options.LudusaviBackupDirectory, "measured.zip");
        var mirrorPath = Path.Combine(options.LocalMirrorPath, "copy.zip");
        var mediaPath = Path.Combine(options.MediaArchiveDirectory, "shot.png");
        await File.WriteAllBytesAsync(archivePath, new byte[3]);
        await File.WriteAllBytesAsync(mirrorPath, new byte[5]);
        await File.WriteAllBytesAsync(Path.Combine(options.RemoteBackupStagingDirectory, "stage.zip"), new byte[7]);
        await File.WriteAllBytesAsync(mediaPath, new byte[11]);
        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            PlayniteId = "g1",
            BackupId = "indexed",
            LudusaviName = "Indexed Game",
            CreatedUtc = DateTime.UtcNow,
            TotalBytes = 101,
            FileCount = 1,
            ArchivePath = archivePath
        }, "{}", CancellationToken.None);
        await store.AddMediaAsync(new MediaItemDto
        {
            MediaId = "assigned-media",
            PlayniteId = "g1",
            Kind = MediaKind.Screenshot,
            Source = MediaSourceKind.Steam,
            ArchivePath = mediaPath,
            OriginalPath = "shot.png",
            CapturedUtc = DateTime.UtcNow,
            SizeBytes = 211,
            Sha256 = "hash-assigned"
        }, CancellationToken.None);
        await store.AddMediaAsync(new MediaItemDto
        {
            MediaId = "inbox-media",
            Kind = MediaKind.VideoClip,
            Source = MediaSourceKind.Custom,
            ArchivePath = Path.Combine(root, "not-present.mp4"),
            OriginalPath = "not-present.mp4",
            CapturedUtc = DateTime.UtcNow,
            SizeBytes = 307,
            Sha256 = "hash-inbox",
            ClassificationState = "Inbox"
        }, CancellationToken.None);

        var service = new StorageAnalysisService(options, store, NullLogger<StorageAnalysisService>.Instance);
        var result = await service.AnalyzeAsync(CancellationToken.None);

        var archive = Assert.Single(result.BudgetBreakdown, entry => entry.Name == "本地存档");
        Assert.Equal<long?>(3L, archive.FileLengthBytes);
        Assert.Equal<long?>(101L, archive.LogicalBytes);
        Assert.Equal("已完成目录测量", archive.StatusDisplay);

        var mirror = Assert.Single(result.BudgetBreakdown, entry => entry.Name == "第二本地镜像");
        Assert.Equal<long?>(5L, mirror.FileLengthBytes);
        Assert.Null(mirror.LogicalBytes);
        Assert.Contains("重复", mirror.BoundaryDisplay);

        var staging = Assert.Single(result.BudgetBreakdown, entry => entry.Name == "远端恢复暂存");
        Assert.Equal<long?>(7L, staging.FileLengthBytes);
        Assert.Null(staging.LogicalBytes);

        var media = Assert.Single(result.BudgetBreakdown, entry => entry.Name == "媒体归档");
        Assert.Equal<long?>(11L, media.FileLengthBytes);
        Assert.Equal<long?>(518L, media.LogicalBytes);
        Assert.Equal(2, media.LogicalItemCount);
        Assert.Contains("待分类", media.BoundaryDisplay);
        Assert.Contains("不生成合计", result.BudgetBreakdownSummary);
    }

    [Fact]
    public async Task AnalyzeDistinguishesEmptyDirectoryFromMissingOrDisabledScopes()
    {
        Directory.CreateDirectory(options.RemoteBackupStagingDirectory);
        var result = await new StorageAnalysisService(options, store, NullLogger<StorageAnalysisService>.Instance)
            .AnalyzeAsync(CancellationToken.None);

        var staging = Assert.Single(result.BudgetBreakdown, entry => entry.Name == "远端恢复暂存");
        Assert.True(staging.MeasurementComplete);
        Assert.Equal<long?>(0L, staging.FileLengthBytes);
        Assert.Equal("目录为空", staging.StatusDisplay);

        var mirror = Assert.Single(result.BudgetBreakdown, entry => entry.Name == "第二本地镜像");
        Assert.False(mirror.IsConfigured);
        Assert.Null(mirror.FileLengthBytes);
        Assert.Equal("未启用", mirror.StatusDisplay);

        options.MediaArchiveDirectory = Path.Combine(root, "MissingMedia");
        var missingResult = await new StorageAnalysisService(options, store, NullLogger<StorageAnalysisService>.Instance)
            .AnalyzeAsync(CancellationToken.None);
        var media = Assert.Single(missingResult.BudgetBreakdown, entry => entry.Name == "媒体归档");
        Assert.True(media.IsConfigured);
        Assert.False(media.DirectoryAvailable);
        Assert.False(media.MeasurementComplete);
        Assert.Null(media.FileLengthBytes);
        Assert.Equal("目录不存在或不可访问", media.StatusDisplay);
    }

    [Fact]
    public async Task AnalyzeWarnsWhenConfiguredBudgetScopesContainTheSameFiles()
    {
        options.EnableLocalMirror = true;
        options.LocalMirrorPath = options.LudusaviBackupDirectory;
        var service = new StorageAnalysisService(options, store, NullLogger<StorageAnalysisService>.Instance);

        var result = await service.AnalyzeAsync(CancellationToken.None);

        var archive = Assert.Single(result.BudgetBreakdown, entry => entry.Name == "本地存档");
        var mirror = Assert.Single(result.BudgetBreakdown, entry => entry.Name == "第二本地镜像");
        Assert.Contains("与“第二本地镜像”相同或互相包含", archive.BoundaryDisplay);
        Assert.Contains("与“本地存档”相同或互相包含", mirror.BoundaryDisplay);
        Assert.Contains("请勿相加", archive.BoundaryDisplay);
        Assert.Contains("请勿相加", mirror.BoundaryDisplay);
    }

    [Fact]
    public async Task AnalyzeUnavailableDirectoryIsNotAnError()
    {
        options.LudusaviBackupDirectory = Path.Combine(root, "MissingSaves");
        var service = new StorageAnalysisService(options, store, NullLogger<StorageAnalysisService>.Instance);

        var result = await service.AnalyzeAsync(CancellationToken.None);

        Assert.False(result.BackupDirectoryAvailable);
        Assert.Empty(result.TopGames);
        Assert.Contains("不可用", result.Summary);
    }

    [Fact]
    public async Task AnalyzeDoesNotProduceExpiryFromInsufficientHistory()
    {
        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            PlayniteId = "g1",
            BackupId = "old",
            LudusaviName = "Old Game",
            CreatedUtc = DateTime.UtcNow.AddDays(-80),
            TotalBytes = 1000,
            FileCount = 1
        }, "{}", CancellationToken.None);
        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            PlayniteId = "g2",
            BackupId = "recent",
            LudusaviName = "Recent Game",
            CreatedUtc = DateTime.UtcNow.AddDays(-5),
            TotalBytes = 2000,
            FileCount = 1
        }, "{}", CancellationToken.None);

        var service = new StorageAnalysisService(options, store, NullLogger<StorageAnalysisService>.Instance);
        var result = await service.AnalyzeAsync(CancellationToken.None);

        Assert.True(result.Trends[0].AddedBytes >= 2000);
        Assert.Equal(1, result.Trends[0].AddedVersionCount);
        Assert.True(result.Trends[1].AddedBytes >= 2000);
        Assert.True(result.Trends[2].AddedBytes >= 3000);
        Assert.False(result.HasHistoricalGrowthRange);
        Assert.Null(result.HistoricalGrowthMinimumBytesPer30Days);
        Assert.Contains("样本不足", result.HistoricalGrowthRangeDisplay);
        Assert.Contains("不输出耗尽日期", result.PredictionSummary);
        Assert.Contains("远端可用容量未知", result.RemoteCapacitySummary);
    }

    [Fact]
    public async Task AnalyzeReportsObservedGrowthRangeWithoutClaimingConfidenceOrExpiry()
    {
        var now = DateTime.UtcNow;
        foreach (var backup in new[]
        {
            (Id: "recent", Age: 5, Bytes: 70L),
            (Id: "middle", Age: 20, Bytes: 20L),
            (Id: "old", Age: 50, Bytes: 90L)
        })
        {
            await store.AddBackupVersionAsync(new BackupVersionDto
            {
                PlayniteId = "g1",
                BackupId = backup.Id,
                LudusaviName = "Game One",
                CreatedUtc = now.AddDays(-backup.Age),
                TotalBytes = backup.Bytes,
                FileCount = 1
            }, "{}", CancellationToken.None);
        }

        var service = new StorageAnalysisService(options, store, NullLogger<StorageAnalysisService>.Instance);
        var result = await service.AnalyzeAsync(CancellationToken.None);

        Assert.True(result.HasHistoricalGrowthRange);
        Assert.Equal(3, result.GrowthObservationDayCount);
        Assert.True(result.GrowthObservationSpanDays >= 30);
        Assert.Equal(90, result.HistoricalGrowthMinimumBytesPer30Days);
        Assert.Equal(300, result.HistoricalGrowthMaximumBytesPer30Days);
        Assert.Contains("90 B–300 B", result.HistoricalGrowthRangeDisplay);
        Assert.Contains("不是置信区间", result.HistoricalGrowthRangeDisplay);
        Assert.DoesNotContain("个月后", result.PredictionSummary);
        Assert.Contains("不据此推算磁盘耗尽日期", result.PredictionSummary);
    }

    [Fact]
    public async Task AnalyzeHonorsCancellation()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var service = new StorageAnalysisService(options, store, NullLogger<StorageAnalysisService>.Instance);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.AnalyzeAsync(cts.Token));
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
    }
}
