using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class BackupCoverTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.BackupCover.Tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task KeepsStableCoverAcrossReloadAndReportsOfflineAndReclassifiedScreenshots()
    {
        var store = await OpenAsync();
        await SeedBackupAsync(store, "game-a", "same-id", locked: true);
        await SeedBackupAsync(store, "game-b", "same-id", locked: false);
        var photoA = await SeedMediaAsync(store, "game-a", "photo-a");
        var photoB = await SeedMediaAsync(store, "game-b", "photo-b");
        var replacement = await SeedMediaAsync(store, "game-a", "replacement");
        await Assert.ThrowsAsync<InvalidOperationException>(() => SetCoverAsync(store, "game-a", "same-id", photoB.MediaId));
        await SetCoverAsync(store, "game-a", "same-id", photoA.MediaId);
        await SetCoverAsync(store, "game-b", "same-id", photoB.MediaId);
        await SetCoverAsync(store, "game-a", "same-id", replacement.MediaId);

        store = await OpenAsync();
        var cover = await store.GetBackupCoverAsync("game-a", "same-id", CancellationToken.None);
        Assert.Equal(replacement.MediaId, cover.MediaId);
        Assert.Equal(replacement.ArchivePath, cover.ArchivePath);
        Assert.Equal(BackupCoverState.Available, cover.State);
        Assert.Equal(photoB.MediaId, (await store.GetBackupCoverAsync("game-b", "same-id", CancellationToken.None)).MediaId);

        File.Delete(replacement.ArchivePath);
        cover = await store.GetBackupCoverAsync("game-a", "same-id", CancellationToken.None);
        Assert.Equal(BackupCoverState.Offline, cover.State);
        Assert.Empty(cover.ArchivePath);

        await store.AssignMediaAsync(replacement.MediaId, "game-b", replacement.ArchivePath, CancellationToken.None);
        cover = await store.GetBackupCoverAsync("game-a", "same-id", CancellationToken.None);
        Assert.Equal(BackupCoverState.Reclassified, cover.State);
        Assert.Empty(cover.ArchivePath);
        Assert.Equal(BackupCoverState.None, (await SetCoverAsync(store, "game-a", "same-id", string.Empty)).State);
        Assert.Equal(BackupCoverState.None, (await store.GetBackupCoverAsync("game-a", "same-id", CancellationToken.None)).State);

        var backups = await store.GetBackupVersionPageAsync("game-a", 2, null, null, CancellationToken.None);
        var backup = Assert.Single(backups.Items);
        Assert.True(backup.IsLocked);
        Assert.Equal("王城门口", backup.Comment);
    }

    [Fact]
    public async Task GalleryPagesOnlyCoveredBackupsWithStableIdentityAtEqualTimestamps()
    {
        var store = await OpenAsync();
        var cover = await SeedMediaAsync(store, "game-a", "cover");
        for (var index = 0; index < 39; index++)
        {
            var id = $"version-{index:D2}";
            await SeedBackupAsync(store, "game-a", id, index == 0);
            if (index != 37)
                await SetCoverAsync(store, "game-a", id, cover.MediaId);
        }
        await SeedBackupAsync(store, "game-b", "version-00", locked: false);
        var otherCover = await SeedMediaAsync(store, "game-b", "other-cover");
        await SetCoverAsync(store, "game-b", "version-00", otherCover.MediaId);

        var query = new BackupCoverGalleryQueryDto { PlayniteId = "game-a" };
        var first = await store.GetBackupCoverGalleryAsync(query, CancellationToken.None);
        Assert.Equal(38, first.TotalCount);
        Assert.Equal(36, first.Items.Count);
        Assert.True(first.HasMore);
        Assert.True(first.Items[0].Backup.IsLocked);
        Assert.All(first.Items, item => Assert.Equal(cover.MediaId, item.Cover.MediaId));
        query.BeforeCreatedUtcKey = first.NextCreatedUtcKey;
        query.BeforeBackupId = first.NextBackupId;
        var next = await store.GetBackupCoverGalleryAsync(query, CancellationToken.None);
        Assert.Equal(new[] { "version-36", "version-38" }, next.Items.Select(item => item.Backup.BackupId));
        Assert.False(next.HasMore);
    }

    [Fact]
    public async Task ReverseLookupReturnsBothBackupsAndResolvesOnlyLiveExactLinks()
    {
        var store = await OpenAsync();
        await SeedBackupAsync(store, "game-a", "backup-1", locked: true);
        await SeedBackupAsync(store, "game-a", "backup-2", locked: false);
        await SeedBackupAsync(store, "game-b", "backup-1", locked: false);
        var photo = await SeedMediaAsync(store, "game-a", "shared-cover");
        await SetCoverAsync(store, "game-a", "backup-1", photo.MediaId);
        await SetCoverAsync(store, "game-a", "backup-2", photo.MediaId);

        var query = new BackupCoverMediaQueryDto { PlayniteId = "game-a", MediaId = photo.MediaId };
        var linked = await store.GetBackupCoversForMediaAsync(query, CancellationToken.None);
        Assert.Equal(new[] { "backup-1", "backup-2" }, linked.Items.Select(item => item.BackupId));
        await store.DeleteBackupVersionAsync("game-a", "backup-1", CancellationToken.None);
        query.BackupId = "backup-1";
        Assert.Empty((await store.GetBackupCoversForMediaAsync(query, CancellationToken.None)).Items);
        query.BackupId = "backup-2";
        Assert.Equal("backup-2", Assert.Single((await store.GetBackupCoversForMediaAsync(query, CancellationToken.None)).Items).BackupId);
        Assert.Equal(BackupCoverState.None, (await store.GetBackupCoverAsync("game-a", "backup-1", CancellationToken.None)).State);
        query.PlayniteId = "game-b";
        query.BackupId = string.Empty;
        Assert.Empty((await store.GetBackupCoversForMediaAsync(query, CancellationToken.None)).Items);
    }

    private async Task<SqliteStateStore> OpenAsync()
    {
        var store = new SqliteStateStore(new WorkerOptions
        {
            DataDirectory = Path.Combine(root, "state"),
            LudusaviBackupDirectory = Path.Combine(root, "saves"),
            MediaArchiveDirectory = Path.Combine(root, "media")
        }, NullLogger<SqliteStateStore>.Instance);
        await store.InitializeAsync(CancellationToken.None);
        return store;
    }

    private static Task SeedBackupAsync(SqliteStateStore store, string gameId, string backupId, bool locked)
        => store.AddBackupVersionAsync(new BackupVersionDto
        {
            PlayniteId = gameId,
            BackupId = backupId,
            CreatedUtc = new DateTime(2026, 11, 24, 8, 0, 0, DateTimeKind.Utc),
            LudusaviName = "Demo",
            Comment = "王城门口",
            IsLocked = locked
        }, "[]", CancellationToken.None);

    private async Task<MediaItemDto> SeedMediaAsync(SqliteStateStore store, string gameId, string mediaId)
    {
        var path = Path.Combine(root, mediaId + ".png");
        await File.WriteAllTextAsync(path, mediaId);
        var item = new MediaItemDto
        {
            PlayniteId = gameId,
            MediaId = mediaId,
            Kind = MediaKind.Screenshot,
            ClassificationState = "Assigned",
            ArchivePath = path,
            OriginalPath = path,
            Sha256 = mediaId,
            CapturedUtc = DateTime.UtcNow
        };
        await store.AddMediaAsync(item, CancellationToken.None);
        return item;
    }

    private static Task<BackupCoverDto> SetCoverAsync(SqliteStateStore store, string gameId, string backupId, string mediaId)
        => store.SetBackupCoverAsync(new BackupCoverUpdateDto { PlayniteId = gameId, BackupId = backupId, MediaId = mediaId }, CancellationToken.None);

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }
}
