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
        Assert.Equal(3, result.Trends.Count);
        Assert.Equal(1, result.Trends[0].AddedVersionCount);
        Assert.Equal(2, result.Trends[1].AddedVersionCount);
        var top = Assert.Single(result.TopGames);
        Assert.Equal("Game One", top.GameName);
        Assert.Equal(5120, top.BackupBytes);
        Assert.Equal(2, top.BackupCount);
        Assert.Contains("估算", result.Summary);
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
    public async Task AnalyzeUsesRecentGrowthForPrediction()
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
        Assert.Contains("估算", result.PredictionSummary);
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
