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

    [Fact]
    public async Task SearchesBeyondLoadedPagesWithLiteralUnicodeAndStableContinuation()
    {
        var options = new WorkerOptions
        {
            DataDirectory = Path.Combine(root, "state"),
            LudusaviBackupDirectory = Path.Combine(root, "saves"),
            MediaArchiveDirectory = Path.Combine(root, "media")
        };
        var store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        var token = CancellationToken.None;
        await store.InitializeAsync(token);
        var timestamp = new DateTime(2026, 10, 19, 8, 0, 0, DateTimeKind.Utc);
        for (var index = 0; index < 903; index++)
        {
            await store.AddBackupVersionAsync(new BackupVersionDto
            {
                PlayniteId = "game-1",
                BackupId = $"version-{index:D4}",
                LudusaviName = "Demo",
                CreatedUtc = index < 900 ? timestamp.AddDays(1) : timestamp,
                Comment = index == 900 ? "王城門口 %_ Ä" : string.Empty,
                SourceDevice = index == 901 ? "王城門口 %_ ä" : string.Empty
            }, "[]", token);
        }
        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            PlayniteId = "game-2",
            BackupId = "other",
            LudusaviName = "Demo",
            CreatedUtc = timestamp,
            Comment = "王城門口 %_ Ä"
        }, "[]", token);

        var unfiltered = await store.GetBackupVersionPageAsync("game-1", 100, null, null, token);
        Assert.DoesNotContain(unfiltered.Items, item => item.BackupId == "version-0900");
        var first = await store.GetBackupVersionPageAsync("game-1", 1, null, null, token, "王城門口 %_ ä");
        Assert.Equal(2, first.TotalCount);
        Assert.Equal("version-0900", Assert.Single(first.Items).BackupId);
        Assert.True(first.HasMore);
        var second = await store.GetBackupVersionPageAsync(
            "game-1", 1, first.NextCreatedUtcKey, first.NextBackupId, token, "王城門口 %_ ä");
        Assert.Equal("version-0901", Assert.Single(second.Items).BackupId);
        Assert.False(second.HasMore);
        var byId = await store.GetBackupVersionPageAsync(
            "game-1", 10, null, null, token, "VERSION-", timestamp, timestamp.AddDays(1));
        Assert.Equal(3, byId.TotalCount);
        Assert.Equal("version-0902", byId.Items.Last().BackupId);
        var exact = await store.GetBackupVersionPageAsync("game-1", 100, null, null, token, exactBackupId: "version-0901");
        Assert.Equal("version-0901", Assert.Single(exact.Items).BackupId);
        Assert.Empty((await store.GetBackupVersionPageAsync(
            "game-2", 100, null, null, token, exactBackupId: "version-0901")).Items);
    }

    [Fact]
    public async Task CalendarUsesLocalDayBoundariesAndCountsOnlyTheRequestedGame()
    {
        Directory.CreateDirectory(root);
        var store = new SqliteStateStore(new WorkerOptions
        {
            DataDirectory = Path.Combine(root, "state"),
            LudusaviBackupDirectory = Path.Combine(root, "saves"),
            MediaArchiveDirectory = Path.Combine(root, "media")
        }, NullLogger<SqliteStateStore>.Instance);
        var token = CancellationToken.None;
        await store.InitializeAsync(token);
        var localMonthStartUtc = new DateTime(2026, 10, 31, 16, 0, 0, DateTimeKind.Utc);
        var dates = new[]
        {
            localMonthStartUtc.AddTicks(-1), localMonthStartUtc,
            localMonthStartUtc.AddDays(1).AddTicks(-1), localMonthStartUtc.AddDays(1),
            localMonthStartUtc.AddDays(30).AddTicks(-1), localMonthStartUtc.AddDays(30)
        };
        for (var index = 0; index < dates.Length; index++)
            await store.AddBackupVersionAsync(new BackupVersionDto
            {
                PlayniteId = "calendar-game",
                BackupId = $"save-{index}",
                LudusaviName = "Demo",
                CreatedUtc = dates[index]
            }, "[]", token);
        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            PlayniteId = "another-game",
            BackupId = "save-1",
            LudusaviName = "Demo",
            CreatedUtc = localMonthStartUtc
        }, "[]", token);

        var counts = await store.GetBackupCalendarAsync(new BackupCalendarQueryDto
        {
            PlayniteId = "calendar-game",
            UtcDayBoundaries = Enumerable.Range(0, 31).Select(day => localMonthStartUtc.AddDays(day)).ToList()
        }, token);
        Assert.Equal(new[] { 1, 2, 30 }, counts.Select(entry => entry.Day));
        Assert.Equal(new[] { 2, 1, 1 }, counts.Select(entry => entry.Count));
        var firstDay = await store.GetBackupVersionPageAsync("calendar-game", 100, null, null, token,
            createdFromUtc: localMonthStartUtc, createdBeforeUtc: localMonthStartUtc.AddDays(1));
        Assert.Equal(new[] { "save-2", "save-1" }, firstDay.Items.Select(item => item.BackupId));
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
