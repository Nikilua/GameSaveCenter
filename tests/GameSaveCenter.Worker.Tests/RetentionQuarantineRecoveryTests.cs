using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class RetentionQuarantineRecoveryTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly WorkerOptions options;
    private readonly SqliteStateStore store;

    public RetentionQuarantineRecoveryTests()
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
    public async Task StartupRecoveryDeletesOnlyAnIndexRemovedKnownEntry()
    {
        var batchId = Guid.NewGuid().ToString("N");
        var entry = await AddEntryAsync(batchId, "known", RetentionQuarantineState.IndexRemoved, 17);
        var unknownPath = Path.Combine(options.LudusaviBackupDirectory, ".gsc-retention-quarantine", batchId, "unknown.pending");
        Directory.CreateDirectory(Path.GetDirectoryName(unknownPath)!);
        await File.WriteAllBytesAsync(unknownPath, new byte[23]);

        var service = new RetentionSimulationService(options, store, NullLogger<RetentionSimulationService>.Instance);
        var recovery = await service.RecoverPendingQuarantineAsync(CancellationToken.None);

        var stateAfterRecovery = Assert.Single(await store.GetRetentionQuarantineEntriesAsync(CancellationToken.None));
        Assert.True(recovery.DeletedCount == 1, $"deleted={recovery.DeletedCount}, restored={recovery.RestoredCount}, recovery={recovery.RecoveryRequiredCount}, state={stateAfterRecovery.State}, error={stateAfterRecovery.LastError}");
        Assert.False(File.Exists(entry.QuarantinePath));
        Assert.True(File.Exists(unknownPath));
        var saved = Assert.Single(await store.GetRetentionQuarantineEntriesAsync(CancellationToken.None));
        Assert.Equal(RetentionQuarantineState.Deleted, saved.State);
    }

    [Fact]
    public async Task StartupRecoveryRestoresAnInterruptedMoveAndDoesNotDeleteIt()
    {
        var entry = await AddEntryAsync(Guid.NewGuid().ToString("N"), "restore", RetentionQuarantineState.Moved, 29);

        var service = new RetentionSimulationService(options, store, NullLogger<RetentionSimulationService>.Instance);
        var recovery = await service.RecoverPendingQuarantineAsync(CancellationToken.None);

        var stateAfterRecovery = Assert.Single(await store.GetRetentionQuarantineEntriesAsync(CancellationToken.None));
        Assert.True(recovery.RestoredCount == 1, $"deleted={recovery.DeletedCount}, restored={recovery.RestoredCount}, recovery={recovery.RecoveryRequiredCount}, state={stateAfterRecovery.State}, error={stateAfterRecovery.LastError}");
        Assert.True(File.Exists(entry.OriginalPath));
        Assert.False(File.Exists(entry.QuarantinePath));
        var saved = Assert.Single(await store.GetRetentionQuarantineEntriesAsync(CancellationToken.None));
        Assert.Equal(RetentionQuarantineState.RecoveryRequired, saved.State);
        Assert.Contains("恢复到原路径", saved.LastError);
    }

    [Fact]
    public async Task PreviewReportsDurableQuarantineOccupancy()
    {
        var entry = await AddEntryAsync(Guid.NewGuid().ToString("N"), "pending", RetentionQuarantineState.IndexRemoved, 31);

        var service = new RetentionSimulationService(options, store, NullLogger<RetentionSimulationService>.Instance);
        var preview = await service.PreviewAsync(CancellationToken.None);

        Assert.Equal(1, preview.PendingQuarantineCount);
        Assert.Equal(31, preview.PendingQuarantineBytes);
        Assert.Equal(31, preview.QuarantineOccupancyBytes);
        Assert.Equal(0, preview.RecoveryRequiredCount);
        Assert.Contains("隔离区当前占用", preview.Summary);
        Assert.True(File.Exists(entry.QuarantinePath));
    }

    private async Task<RetentionQuarantineEntryDto> AddEntryAsync(
        string batchId,
        string backupId,
        RetentionQuarantineState state,
        int bytes)
    {
        await store.CreateRetentionQuarantineBatchAsync(batchId, "test-preview", CancellationToken.None);
        var original = Path.Combine(options.LudusaviBackupDirectory, backupId + ".zip");
        var quarantine = Path.Combine(options.LudusaviBackupDirectory, ".gsc-retention-quarantine", batchId, backupId + ".pending");
        Directory.CreateDirectory(Path.GetDirectoryName(quarantine)!);
        await File.WriteAllBytesAsync(quarantine, new byte[bytes]);
        var entry = new RetentionQuarantineEntryDto
        {
            EntryId = Guid.NewGuid().ToString("N"),
            BatchId = batchId,
            PlayniteId = "g1",
            BackupId = backupId,
            OriginalPath = original,
            QuarantinePath = quarantine,
            FileBytes = bytes,
            State = RetentionQuarantineState.Planned,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow
        };
        await store.CreateRetentionQuarantineEntryAsync(entry, CancellationToken.None);
        await store.UpdateRetentionQuarantineEntryAsync(entry.EntryId, state, null, CancellationToken.None);
        entry.State = state;
        return entry;
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
    }
}
