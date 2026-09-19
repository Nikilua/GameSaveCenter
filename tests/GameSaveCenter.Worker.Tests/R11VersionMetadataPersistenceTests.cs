using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class R11VersionMetadataPersistenceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.R11.Metadata.Tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task CommentAndLockStateRemainReadableAfterStoreRestartWithoutChangingBackupIdOrArchivePath()
    {
        Directory.CreateDirectory(root);
        var options = new WorkerOptions
        {
            DataDirectory = Path.Combine(root, "state"),
            LudusaviBackupDirectory = Path.Combine(root, "saves"),
            MediaArchiveDirectory = Path.Combine(root, "media")
        };

        var firstStore = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        await firstStore.InitializeAsync(CancellationToken.None);
        await firstStore.AddBackupVersionAsync(new BackupVersionDto
        {
            BackupId = "stable-backup-id",
            PlayniteId = "game-1",
            LudusaviName = "Demo Game",
            CreatedUtc = DateTime.UtcNow,
            ArchivePath = Path.Combine(options.LudusaviBackupDirectory, "stable-backup-id"),
            Comment = "原始说明",
            IsLocked = false
        }, "{}", CancellationToken.None);

        // This is the same upsert contract used after Ludusavi edit + history refresh.
        await firstStore.AddBackupVersionAsync(new BackupVersionDto
        {
            BackupId = "stable-backup-id",
            PlayniteId = "game-1",
            LudusaviName = "Demo Game",
            CreatedUtc = DateTime.UtcNow,
            ArchivePath = string.Empty,
            Comment = "重启后仍可读的说明",
            IsLocked = true
        }, "{}", CancellationToken.None);

        var restartedStore = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        await restartedStore.InitializeAsync(CancellationToken.None);
        var loaded = Assert.Single(await restartedStore.GetBackupVersionsAsync("game-1", CancellationToken.None));

        Assert.Equal("stable-backup-id", loaded.BackupId);
        Assert.Equal("重启后仍可读的说明", loaded.Comment);
        Assert.True(loaded.IsLocked);
        Assert.Equal(Path.Combine(options.LudusaviBackupDirectory, "stable-backup-id"), loaded.ArchivePath);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(root))
        {
            try { Directory.Delete(root, recursive: true); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
