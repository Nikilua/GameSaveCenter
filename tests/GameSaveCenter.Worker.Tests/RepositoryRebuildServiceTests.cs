using System.IO.Compression;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class RepositoryRebuildServiceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly WorkerOptions options;
    private readonly SqliteStateStore store;

    public RepositoryRebuildServiceTests()
    {
        options = new WorkerOptions
        {
            DataDirectory = Path.Combine(root, "Data"),
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            MediaArchiveDirectory = Path.Combine(root, "Media")
        };
        store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task RebuildRequiresConfirmation()
    {
        var service = CreateService();
        var ex = await Assert.ThrowsAsync<WorkerOperationException>(() => service.RebuildAsync(
            new RepositoryRebuildRequestDto { Confirmed = false },
            CancellationToken.None));
        Assert.Equal("REPOSITORY_REBUILD_NOT_CONFIRMED", ex.Code);
    }

    [Fact]
    public async Task PreviewScansArchivesWithoutWriting()
    {
        Directory.CreateDirectory(options.LudusaviBackupDirectory);
        CreateZip(Path.Combine(options.LudusaviBackupDirectory, "empty.zip"), new Dictionary<string, byte[]>());
        await File.WriteAllBytesAsync(Path.Combine(options.LudusaviBackupDirectory, "corrupt.zip"), new byte[] { 1, 2, 3 });
        var service = CreateService();

        var preview = await service.PreviewAsync(CancellationToken.None);

        Assert.Equal(2, preview.FoundArchives);
        Assert.Equal(1, preview.ConfirmableArchives);
        Assert.Equal(1, preview.UnassignedArchives);
        Assert.Equal(1, preview.PartialMetadataArchives);
        Assert.Equal(1, preview.CorruptArchives);
    }

    [Fact]
    public async Task RebuildRecoversHistoryFromFreshDatabaseAndRepositoryArtifacts()
    {
        var gameDir = Path.Combine(options.LudusaviBackupDirectory, "Demo One");
        Directory.CreateDirectory(gameDir);
        CreateZip(Path.Combine(gameDir, "2026-08-14 12-00-00.zip"), new Dictionary<string, byte[]>
        {
            ["save.dat"] = new byte[] { 1, 2, 3 },
            ["settings.ini"] = new byte[] { 4, 5 }
        });
        var service = CreateService();

        var preview = await service.PreviewAsync(CancellationToken.None);
        Assert.Equal(1, preview.FoundArchives);
        Assert.Equal(1, preview.ConfirmableArchives);
        Assert.Equal(1, preview.UnassignedArchives);

        var result = await service.RebuildAsync(new RepositoryRebuildRequestDto { Confirmed = true }, CancellationToken.None);

        Assert.Equal(1, result.RebuiltGameCount);
        Assert.Equal(1, result.RecoveredGameCount);
        Assert.Equal(1, result.IndexedVersionCount);
        Assert.Equal(0, result.FailedGameCount);

        var games = await store.GetGamesAsync(CancellationToken.None);
        var game = Assert.Single(games);
        Assert.Equal("Demo One", game.Name);
        Assert.StartsWith("recovered-", game.PlayniteId);

        var versions = await store.GetBackupVersionsAsync(game.PlayniteId, CancellationToken.None);
        var version = Assert.Single(versions);
        Assert.Equal("2026-08-14 12-00-00", version.BackupId);
        Assert.Equal(2, version.FileCount);
        Assert.True(version.TotalBytes > 0);
        Assert.Equal(string.Empty, version.ParentBackupId);
        Assert.EndsWith(".zip", version.ArchivePath);
        var manifest = await store.GetBackupManifestAsync(game.PlayniteId, version.BackupId, CancellationToken.None);
        Assert.Contains("save.dat", manifest);

        var second = await service.RebuildAsync(new RepositoryRebuildRequestDto { Confirmed = true }, CancellationToken.None);
        Assert.Equal(1, second.RebuiltGameCount);
        Assert.Equal(0, second.RecoveredGameCount);
        Assert.Equal(1, second.IndexedVersionCount);
        Assert.Single(await store.GetGamesAsync(CancellationToken.None));
        Assert.Single(await store.GetBackupVersionsAsync(game.PlayniteId, CancellationToken.None));
    }

    [Fact]
    public async Task RebuildReusesExistingMatchWhenAvailable()
    {
        var descriptor = new GameDescriptorDto
        {
            PlayniteId = "g1",
            Name = "Demo One",
            Platform = GamePlatformKind.Other
        };
        await store.UpsertGamesAsync(new[] { descriptor }, CancellationToken.None);
        await store.SetGameMatchAsync("g1", "Demo One", 1.0, GameMatchInput.CreateHash(descriptor), CancellationToken.None);
        var gameDir = Path.Combine(options.LudusaviBackupDirectory, "Demo One");
        Directory.CreateDirectory(gameDir);
        CreateZip(Path.Combine(gameDir, "v1.zip"), new Dictionary<string, byte[]>
        {
            ["save.dat"] = new byte[] { 1 }
        });
        var service = CreateService();

        var result = await service.RebuildAsync(new RepositoryRebuildRequestDto { Confirmed = true }, CancellationToken.None);

        Assert.Equal(1, result.RebuiltGameCount);
        Assert.Equal(0, result.RecoveredGameCount);
        Assert.Equal(1, result.IndexedVersionCount);
        var versions = await store.GetBackupVersionsAsync("g1", CancellationToken.None);
        Assert.Equal("v1", Assert.Single(versions).BackupId);
        Assert.DoesNotContain(await store.GetGamesAsync(CancellationToken.None), x => x.PlayniteId.StartsWith("recovered-", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task RebuildDoesNotGuessParentAndPreservesLockedMissingVersions()
    {
        var descriptor = new GameDescriptorDto
        {
            PlayniteId = "g1",
            Name = "Demo One",
            Platform = GamePlatformKind.Other
        };
        await store.UpsertGamesAsync(new[] { descriptor }, CancellationToken.None);
        await store.SetGameMatchAsync("g1", "Demo One", 1.0, GameMatchInput.CreateHash(descriptor), CancellationToken.None);
        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            PlayniteId = "g1",
            BackupId = "locked-missing",
            LudusaviName = "Demo One",
            CreatedUtc = DateTime.UtcNow.AddDays(-1),
            TotalBytes = 1,
            FileCount = 1,
            IsLocked = true
        }, "{}", CancellationToken.None);
        var gameDir = Path.Combine(options.LudusaviBackupDirectory, "Demo One");
        Directory.CreateDirectory(gameDir);
        CreateZip(Path.Combine(gameDir, "v1.zip"), new Dictionary<string, byte[]> { ["save.dat"] = new byte[] { 1 } });
        CreateZip(Path.Combine(gameDir, "v2.zip"), new Dictionary<string, byte[]> { ["save.dat"] = new byte[] { 2 } });
        var service = CreateService();

        var result = await service.RebuildAsync(new RepositoryRebuildRequestDto { Confirmed = true }, CancellationToken.None);

        Assert.Equal(1, result.RebuiltGameCount);
        Assert.Equal(3, result.IndexedVersionCount);
        var versions = (await store.GetBackupVersionsAsync("g1", CancellationToken.None)).OrderBy(x => x.BackupId).ToList();
        Assert.Contains(versions, x => x.BackupId == "locked-missing" && x.IsLocked);
        Assert.Equal(2, versions.Count(x => x.BackupId != "locked-missing"));
        Assert.All(versions.Where(x => x.BackupId != "locked-missing"), x => Assert.Equal(string.Empty, x.ParentBackupId));
    }

    [Fact]
    public async Task RebuildSkipsCorruptArchivesWithoutCrashing()
    {
        Directory.CreateDirectory(options.LudusaviBackupDirectory);
        await File.WriteAllBytesAsync(Path.Combine(options.LudusaviBackupDirectory, "corrupt.zip"), new byte[] { 1, 2, 3 });
        var service = CreateService();

        var result = await service.RebuildAsync(new RepositoryRebuildRequestDto { Confirmed = true }, CancellationToken.None);

        Assert.Equal(0, result.RebuiltGameCount);
        Assert.Equal(0, result.IndexedVersionCount);
        Assert.Equal(0, result.FailedGameCount);
        var audit = await store.GetAuditAsync(20, CancellationToken.None);
        Assert.Contains(audit, x => x.Category == "RepositoryRebuild");
    }

    private RepositoryRebuildService CreateService()
        => new(store, options, NullLogger<RepositoryRebuildService>.Instance);

    private static void CreateZip(string path, IReadOnlyDictionary<string, byte[]> entries)
    {
        using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
        foreach (var pair in entries)
        {
            var entry = archive.CreateEntry(pair.Key);
            using var stream = entry.Open();
            stream.Write(pair.Value, 0, pair.Value.Length);
        }
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
    }
}
