using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class PathRemapServiceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly string oldRoot;
    private readonly string newRoot;
    private readonly WorkerOptions options;
    private readonly SqliteStateStore store;

    public PathRemapServiceTests()
    {
        oldRoot = Path.Combine(root, "OldSaves");
        newRoot = Path.Combine(root, "NewSaves");
        options = new WorkerOptions
        {
            DataDirectory = Path.Combine(root, "Data"),
            LudusaviBackupDirectory = oldRoot,
            MediaArchiveDirectory = Path.Combine(root, "Media")
        };
        Directory.CreateDirectory(options.DataDirectory);
        Directory.CreateDirectory(oldRoot);
        Directory.CreateDirectory(options.MediaArchiveDirectory);
        store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task RemapUpdatesDatabaseRowsAndPersistedSettings()
    {
        var archive = Path.Combine(oldRoot, "game", "backup.zip");
        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            PlayniteId = "g1",
            BackupId = "b1",
            LudusaviName = "game",
            CreatedUtc = DateTime.UtcNow,
            TotalBytes = 1,
            FileCount = 1,
            ArchivePath = archive
        }, "{}", CancellationToken.None);

        var service = new PathRemapService(options, store, new MetadataBackupService(options, store, NullLogger<MetadataBackupService>.Instance), NullLogger<PathRemapService>.Instance);
        var result = await service.RemapAsync(new PathRemapRequestDto
        {
            OldRoot = oldRoot,
            NewRoot = newRoot,
            Confirmed = true,
            ApplyMissingTargets = true
        }, CancellationToken.None);

        Assert.True(result.AffectedRows >= 1);
        Assert.Contains("存档目录", result.UpdatedSettings);
        Assert.Equal(newRoot, options.LudusaviBackupDirectory);
        var versions = await store.GetBackupVersionsAsync("g1", CancellationToken.None);
        Assert.Equal(Path.Combine(newRoot, "game", "backup.zip"), versions[0].ArchivePath);
        Assert.True(File.Exists(options.RuntimeSettingsPath));
        var persisted = await File.ReadAllTextAsync(options.RuntimeSettingsPath);
        using var document = JsonDocument.Parse(persisted);
        Assert.Equal(newRoot, document.RootElement.GetProperty("ludusaviBackupDirectory").GetString());
        Assert.NotEqual(oldRoot, document.RootElement.GetProperty("ludusaviBackupDirectory").GetString());
    }

    [Fact]
    public async Task UnconfirmedRemapIsRejected()
    {
        var service = new PathRemapService(options, store, new MetadataBackupService(options, store, NullLogger<MetadataBackupService>.Instance), NullLogger<PathRemapService>.Instance);
        var ex = await Assert.ThrowsAsync<WorkerOperationException>(() => service.RemapAsync(new PathRemapRequestDto
        {
            OldRoot = oldRoot,
            NewRoot = newRoot,
            Confirmed = false
        }, CancellationToken.None));
        Assert.Equal("PATH_REMAP_NOT_CONFIRMED", ex.Code);
    }

    [Fact]
    public async Task PreviewFlagsMissingTargetsAndDefaultPolicySkips()
    {
        var archive = Path.Combine(oldRoot, "game", "missing.zip");
        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            PlayniteId = "g1",
            BackupId = "missing",
            LudusaviName = "game",
            CreatedUtc = DateTime.UtcNow,
            TotalBytes = 1,
            FileCount = 1,
            ArchivePath = archive
        }, "{}", CancellationToken.None);
        var metadata = new MetadataBackupService(options, store, NullLogger<MetadataBackupService>.Instance);
        var service = new PathRemapService(options, store, metadata, NullLogger<PathRemapService>.Instance);

        var preview = await service.PreviewAsync(new PathRemapRequestDto
        {
            OldRoot = oldRoot,
            NewRoot = newRoot
        }, CancellationToken.None);
        Assert.True(preview.AffectedRowCount >= 1);
        Assert.True(preview.MissingTargetCount >= 1);

        var ex = await Assert.ThrowsAsync<WorkerOperationException>(() => service.RemapAsync(
            new PathRemapRequestDto { OldRoot = oldRoot, NewRoot = newRoot, Confirmed = true },
            CancellationToken.None));
        Assert.Equal("PATH_REMAP_TARGET_MISSING", ex.Code);

        var applied = await service.RemapAsync(
            new PathRemapRequestDto { OldRoot = oldRoot, NewRoot = newRoot, Confirmed = true, ApplyMissingTargets = true },
            CancellationToken.None);
        Assert.True(applied.AffectedRows >= 1);
    }

    [Fact]
    public async Task PreviewKeepsFullPathsAndDoesNotMatchSimilarRootsAcrossDrives()
    {
        var differentDriveRoot = @"D:\GameSaveCenter\Synthetic\NewSaves";
        var longSegment = new string('x', 80);
        var archive = Path.Combine(oldRoot, "game", longSegment, "backup.zip");
        var similarArchive = Path.Combine(oldRoot + "-other", "game", "backup.zip");
        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            PlayniteId = "g1",
            BackupId = "long",
            LudusaviName = "game",
            CreatedUtc = DateTime.UtcNow,
            TotalBytes = 1,
            FileCount = 1,
            ArchivePath = archive
        }, "{}", CancellationToken.None);
        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            PlayniteId = "g1",
            BackupId = "similar",
            LudusaviName = "game",
            CreatedUtc = DateTime.UtcNow,
            TotalBytes = 1,
            FileCount = 1,
            ArchivePath = similarArchive
        }, "{}", CancellationToken.None);

        var service = new PathRemapService(options, store,
            new MetadataBackupService(options, store, NullLogger<MetadataBackupService>.Instance),
            NullLogger<PathRemapService>.Instance);
        var preview = await service.PreviewAsync(new PathRemapRequestDto
        {
            OldRoot = oldRoot,
            NewRoot = differentDriveRoot
        }, CancellationToken.None);

        var item = Assert.Single(preview.Items, x => x.OldPath == archive);
        Assert.Equal(archive, item.OldPathDisplay);
        Assert.Equal(Path.Combine(differentDriveRoot, "game", longSegment, "backup.zip"), item.NewPathDisplay);
        Assert.Equal("目标不存在（需明确确认）", item.TargetStateDisplay);
        Assert.DoesNotContain(preview.Items, x => x.OldPath == similarArchive);
        Assert.False(File.Exists(item.NewPath));
        var versions = await store.GetBackupVersionsAsync("g1", CancellationToken.None);
        Assert.Contains(versions, version => version.ArchivePath == archive);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
    }
}
