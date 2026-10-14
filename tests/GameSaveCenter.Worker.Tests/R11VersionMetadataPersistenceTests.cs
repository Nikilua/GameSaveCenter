using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
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
        foreach (var version in ParseVersions(options, "原始说明", false))
            await firstStore.AddBackupVersionAsync(version, "{}", CancellationToken.None);

        // Model Ludusavi's response after editing metadata and refreshing history.
        // Both backup IDs receive the same comment so display text cannot stand in for identity.
        foreach (var version in ParseVersions(options, "重启后仍可读的同名说明", true))
            await firstStore.AddBackupVersionAsync(version, "{}", CancellationToken.None);

        var restartedStore = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        await restartedStore.InitializeAsync(CancellationToken.None);
        var loaded = await restartedStore.GetBackupVersionsAsync("game-1", CancellationToken.None);

        Assert.Equal(2, loaded.Count);
        var first = Assert.Single(loaded, version => version.BackupId == "backup-a");
        var second = Assert.Single(loaded, version => version.BackupId == "backup-b");
        Assert.Equal("重启后仍可读的同名说明", first.Comment);
        Assert.Equal("重启后仍可读的同名说明", second.Comment);
        Assert.False(first.IsLocked);
        Assert.True(second.IsLocked);
        Assert.Equal(Path.Combine(options.LudusaviBackupDirectory, "backup-a"), first.ArchivePath);
        Assert.Equal(Path.Combine(options.LudusaviBackupDirectory, "backup-b"), second.ArchivePath);
    }

    private static List<BackupVersionDto> ParseVersions(WorkerOptions options, string comment, bool lockSecond)
    {
        var listing = JsonSerializer.Serialize(new
        {
            games = new Dictionary<string, object>
            {
                ["Demo Game"] = new
                {
                    backupPath = options.LudusaviBackupDirectory,
                    backups = new[]
                    {
                        new
                        {
                            name = "backup-a",
                            when = new DateTime(2026, 10, 14, 8, 0, 0, DateTimeKind.Utc),
                            comment,
                            locked = false
                        },
                        new
                        {
                            name = "backup-b",
                            when = new DateTime(2026, 10, 14, 9, 0, 0, DateTimeKind.Utc),
                            comment,
                            locked = lockSecond
                        }
                    }
                }
            }
        });

        using var document = JsonDocument.Parse(listing);
        return LudusaviResultParser.ParseBackupList(document.RootElement, "game-1", "Demo Game");
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
