using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class MediaQueryPersistenceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly WorkerOptions options;
    private readonly SqliteStateStore store;

    public MediaQueryPersistenceTests()
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
    public async Task PageUsesStableCursorWhenMediaShareCaptureTime()
    {
        var captured = DateTime.UtcNow.AddMinutes(-5);
        for (var i = 0; i < 5; i++)
            await AddMediaAsync($"same-time-{i:00}", "game-1", captured, originalPath: $"shot-{i:00}.png");

        var first = await store.GetMediaPageAsync(new MediaQueryDto { PlayniteId = "game-1", Limit = 2 }, CancellationToken.None);
        var second = await store.GetMediaPageAsync(new MediaQueryDto { PlayniteId = "game-1", Limit = 2, Cursor = first.NextCursor }, CancellationToken.None);

        Assert.Equal(5, first.TotalCount);
        Assert.Equal(new[] { "same-time-04", "same-time-03" }, first.Items.Select(x => x.MediaId));
        Assert.True(first.HasMore);
        Assert.Equal(new[] { "same-time-02", "same-time-01" }, second.Items.Select(x => x.MediaId));
        Assert.Equal(5, second.TotalCount);
        Assert.True(second.HasMore);
        Assert.DoesNotContain(second.Items, x => first.Items.Any(y => y.MediaId == x.MediaId));
    }

    [Fact]
    public async Task PageAppliesSearchKindFavoriteAndInboxStateTotals()
    {
        await AddMediaAsync("favorite-shot", "game-1", DateTime.UtcNow, MediaKind.Screenshot, true, "old needle");
        await AddMediaAsync("normal-video", "game-1", DateTime.UtcNow.AddMinutes(-1), MediaKind.VideoClip);
        await AddMediaAsync("other-shot", "game-1", DateTime.UtcNow.AddMinutes(-2), MediaKind.Screenshot);
        await AddMediaAsync("inbox-1", string.Empty, DateTime.UtcNow.AddMinutes(-3), classificationState: "Inbox");
        await AddMediaAsync("inbox-2", string.Empty, DateTime.UtcNow.AddMinutes(-4), classificationState: "Inbox");
        await AddMediaAsync("ignored-1", string.Empty, DateTime.UtcNow.AddMinutes(-5), classificationState: "Ignored");

        var filtered = await store.GetMediaPageAsync(new MediaQueryDto
        {
            PlayniteId = "game-1",
            Kind = MediaKind.Screenshot,
            FavoriteOnly = true,
            Search = "needle"
        }, CancellationToken.None);
        var inbox = await store.GetUnassignedMediaPageAsync(new MediaQueryDto { Limit = 1 }, CancellationToken.None);
        var ignored = await store.GetIgnoredMediaPageAsync(new MediaQueryDto(), CancellationToken.None);

        var favorite = Assert.Single(filtered.Items);
        Assert.Equal("favorite-shot", favorite.MediaId);
        Assert.Equal(1, filtered.TotalCount);
        Assert.Single(inbox.Items);
        Assert.Equal(2, inbox.TotalCount);
        Assert.True(inbox.HasMore);
        Assert.Single(ignored.Items);
        Assert.Equal(1, ignored.TotalCount);
    }

    [Fact]
    public async Task SearchFindsMatchingMediaOutsideTheFirstPage()
    {
        var captured = DateTime.UtcNow.AddDays(-30);
        for (var i = 0; i < 210; i++)
            await AddMediaAsync("history-" + i.ToString("000"), "game-1", captured.AddSeconds(i), originalPath: i == 17 ? "needle-old-shot.png" : $"shot-{i:000}.png");

        var page = await store.GetMediaPageAsync(new MediaQueryDto { PlayniteId = "game-1", Search = "needle-old-shot" }, CancellationToken.None);

        var match = Assert.Single(page.Items);
        Assert.Equal("history-017", match.MediaId);
        Assert.Equal(1, page.TotalCount);
    }

    [Fact]
    public async Task MediaPageQueriesUseStableCaptureIndexes()
    {
        for (var i = 0; i < 1000; i++)
            await AddMediaAsync("indexed-" + i.ToString("0000"), "game-1", DateTime.UtcNow.AddSeconds(-i));

        await using var connection = new SqliteConnection($"Data Source={options.DatabasePath};Mode=ReadOnly");
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = @"
EXPLAIN QUERY PLAN
SELECT media_id FROM media
WHERE playnite_id=$game AND classification_state='Assigned'
ORDER BY captured_utc DESC,media_id DESC LIMIT 201;";
        command.Parameters.AddWithValue("$game", "game-1");
        var plan = new List<string>();
        await using (var reader = await command.ExecuteReaderAsync())
            while (await reader.ReadAsync()) plan.Add(reader.GetString(3));

        Assert.Contains(plan, detail => detail.Contains("ix_media_game_state_capture", StringComparison.Ordinal));

        command = connection.CreateCommand();
        command.CommandText = @"
EXPLAIN QUERY PLAN
SELECT media_id FROM media
WHERE classification_state='Inbox'
ORDER BY captured_utc DESC,media_id DESC LIMIT 201;";
        plan.Clear();
        await using (var reader = await command.ExecuteReaderAsync())
            while (await reader.ReadAsync()) plan.Add(reader.GetString(3));

        Assert.Contains(plan, detail => detail.Contains("ix_media_state_capture", StringComparison.Ordinal));
    }

    private async Task AddMediaAsync(
        string mediaId,
        string playniteId,
        DateTime capturedUtc,
        MediaKind kind = MediaKind.Screenshot,
        bool favorite = false,
        string comment = "",
        string originalPath = "shot.png",
        string classificationState = "Assigned")
    {
        await store.AddMediaAsync(new MediaItemDto
        {
            MediaId = mediaId,
            PlayniteId = playniteId,
            Kind = kind,
            Source = MediaSourceKind.Steam,
            ArchivePath = Path.Combine(root, mediaId + ".png"),
            OriginalPath = originalPath,
            CapturedUtc = capturedUtc,
            SizeBytes = 10,
            Sha256 = "hash-" + mediaId,
            IsFavorite = favorite,
            Comment = comment,
            ClassificationState = classificationState
        }, CancellationToken.None);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
    }
}
