using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Models;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class BackupHistoryPagingTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.BackupHistoryPaging.Tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task PagesUseStableTimeAndIdCursorAndReturnContentFingerprintWithoutManifestPaths()
    {
        Directory.CreateDirectory(root);
        var options = new WorkerOptions
        {
            DataDirectory = Path.Combine(root, "state"),
            LudusaviBackupDirectory = Path.Combine(root, "saves"),
            MediaArchiveDirectory = Path.Combine(root, "media")
        };
        var store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        await store.InitializeAsync(CancellationToken.None);

        var timestamp = new DateTime(2026, 10, 19, 8, 0, 0, DateTimeKind.Utc);
        foreach (var id in new[] { "b", "a", "c" })
        {
            await store.AddBackupVersionAsync(new BackupVersionDto
            {
                BackupId = id,
                PlayniteId = "game-1",
                LudusaviName = "Demo",
                CreatedUtc = timestamp,
                SourceDevice = id == "c" ? "Laptop" : "Desktop",
                ParentBackupId = id == "c" ? "a" : string.Empty
            }, Manifest("profile.sav", "ABCD"), CancellationToken.None);
        }
        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            BackupId = "older",
            PlayniteId = "game-1",
            LudusaviName = "Demo",
            CreatedUtc = timestamp.AddDays(-1)
        }, "[]", CancellationToken.None);

        var first = await store.GetBackupVersionPageAsync("game-1", 2, null, null, CancellationToken.None);
        Assert.Equal(4, first.TotalCount);
        Assert.True(first.HasMore);
        Assert.Equal(new[] { "a", "b" }, first.Items.Select(item => item.BackupId));
        Assert.NotEmpty(first.Items[0].ContentFingerprint);
        Assert.Equal(first.Items[0].ContentFingerprint, first.Items[1].ContentFingerprint);
        Assert.DoesNotContain("profile.sav", JsonSerializer.Serialize(first));

        // A newly inserted recent backup does not shift the continuation cursor.
        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            BackupId = "newest",
            PlayniteId = "game-1",
            LudusaviName = "Demo",
            CreatedUtc = timestamp.AddMinutes(1)
        }, "[]", CancellationToken.None);

        var next = await store.GetBackupVersionPageAsync(
            "game-1",
            2,
            first.NextCreatedUtcKey,
            first.NextBackupId,
            CancellationToken.None);
        Assert.Equal(5, next.TotalCount);
        Assert.Equal(new[] { "c", "older" }, next.Items.Select(item => item.BackupId));
        Assert.False(next.HasMore);
        Assert.Equal("a", next.Items[0].ParentBackupId);
    }

    private static string Manifest(string path, string hash)
        => JsonSerializer.Serialize(new[] { new FileManifestEntry { RelativePath = path, SizeBytes = 24, Sha256 = hash } });

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
