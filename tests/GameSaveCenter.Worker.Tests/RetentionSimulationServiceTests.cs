using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Models;
using GameSaveCenter.Core.Services;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class RetentionSimulationServiceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly WorkerOptions options;
    private readonly SqliteStateStore store;

    public RetentionSimulationServiceTests()
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
    public async Task PreviewCountsCandidatesAndProtectedVersions()
    {
        await AddZeroRetentionPolicyAsync("g1");
        var archive = CreateArchive("g1-delete.zip", 100);
        await AddVersionAsync("g1", "delete", archive, 100, DateTime.UtcNow.AddDays(-90));
        await AddVersionAsync("g1", "locked", CreateArchive("g1-locked.zip", 200), 200, DateTime.UtcNow.AddDays(-90), isLocked: true);
        await AddVersionAsync("g1", "pre", CreateArchive("g1-pre.zip", 300), 300, DateTime.UtcNow.AddDays(-90), isPreRestore: true);
        await AddVersionAsync("g1", "healthy", CreateArchive("g1-healthy.zip", 400), 400, DateTime.UtcNow.AddDays(-90), isReady: true);

        var policies = await store.GetAllPoliciesAsync(CancellationToken.None);
        Assert.True(policies.ContainsKey("g1"));
        Assert.Equal(0, policies["g1"].KeepMonthlyMonths);
        var rows = await store.GetStorageAnalysisRowsAsync(CancellationToken.None);
        var directPlan = new RetentionPlanner().CreatePlan(
            rows.Select(x => new BackupSnapshot
            {
                BackupId = x.BackupId,
                CreatedUtc = x.CreatedUtc,
                TotalBytes = x.TotalBytes,
                FileCount = x.FileCount,
                IsLocked = x.IsLocked,
                IsPreRestore = x.IsPreRestore,
                ReadinessStatus = x.RestoreReadiness?.Status
            }),
            new RetentionPolicy { KeepAllFor = TimeSpan.Zero, KeepDailyDays = 0, KeepWeeklyWeeks = 0, KeepMonthlyMonths = 0 },
            DateTime.UtcNow);
        Assert.Single(directPlan.DeleteCandidates);

        var service = new RetentionSimulationService(options, store, NullLogger<RetentionSimulationService>.Instance);
        var preview = await service.PreviewAsync(CancellationToken.None);

        Assert.Equal(4, preview.ExistingVersionCount);
        Assert.Equal(1, preview.DeleteCandidateCount);
        Assert.Equal(1, preview.UserLockedCount);
        Assert.Equal(1, preview.PreRestoreCount);
        Assert.Equal(1, preview.HealthProtectedCount);
        Assert.Equal(100, preview.EstimatedReleaseBytes);
        Assert.Contains("预览只读", preview.Summary);
        var item = Assert.Single(preview.Items);
        Assert.Equal("delete", item.BackupId);
    }

    [Fact]
    public async Task ApplyRequiresSecondConfirmation()
    {
        var service = new RetentionSimulationService(options, store, NullLogger<RetentionSimulationService>.Instance);
        var ex = await Assert.ThrowsAsync<WorkerOperationException>(() =>
            service.ApplyAsync(new RetentionSimulationApplyRequestDto { Confirmed = false }, CancellationToken.None));
        Assert.Equal("RETENTION_APPLY_NOT_CONFIRMED", ex.Code);
    }

    [Fact]
    public async Task ApplyDeletesOnlyUnprotectedZipCandidatesBelowBackupRoot()
    {
        await AddZeroRetentionPolicyAsync("g1");
        var deletePath = CreateArchive("g1-delete.zip", 500);
        var lockedPath = CreateArchive("g1-locked.zip", 500);
        var prePath = CreateArchive("g1-pre.zip", 500);
        var healthyPath = CreateArchive("g1-healthy.zip", 500);
        var missingPath = Path.Combine(options.LudusaviBackupDirectory, "g1-missing.zip");
        var unsupportedPath = Path.Combine(options.LudusaviBackupDirectory, "g1-simple.dat");
        await File.WriteAllBytesAsync(unsupportedPath, new byte[50]);
        var outsidePath = Path.Combine(root, "outside.zip");
        await File.WriteAllBytesAsync(outsidePath, new byte[50]);

        await AddVersionAsync("g1", "delete", deletePath, 500, DateTime.UtcNow.AddDays(-90));
        await AddVersionAsync("g1", "locked", lockedPath, 500, DateTime.UtcNow.AddDays(-90), isLocked: true);
        await AddVersionAsync("g1", "pre", prePath, 500, DateTime.UtcNow.AddDays(-90), isPreRestore: true);
        await AddVersionAsync("g1", "healthy", healthyPath, 500, DateTime.UtcNow.AddDays(-90), isReady: true);
        await AddVersionAsync("g1", "missing", missingPath, 500, DateTime.UtcNow.AddDays(-90));
        await AddVersionAsync("g1", "unsupported", unsupportedPath, 500, DateTime.UtcNow.AddDays(-90));
        await AddVersionAsync("g1", "outside", outsidePath, 500, DateTime.UtcNow.AddDays(-90));

        var service = new RetentionSimulationService(options, store, NullLogger<RetentionSimulationService>.Instance);
        var preview = await service.PreviewAsync(CancellationToken.None);
        var result = await service.ApplyAsync(
            new RetentionSimulationApplyRequestDto
            {
                Confirmed = true,
                PreviewId = preview.PreviewId,
                PreviewGeneratedUtc = preview.GeneratedUtc,
                ExpectedCandidateCount = preview.DeleteCandidateCount,
                ExpectedReleaseBytes = preview.EstimatedReleaseBytes
            },
            CancellationToken.None);

        Assert.Equal(1, result.DeletedCount);
        Assert.Equal(0, result.SkippedProtectedCount);
        Assert.Equal(1, result.SkippedMissingCount);
        Assert.Equal(2, result.SkippedUnsupportedCount);
        Assert.Equal(0, result.FailedCount);
        Assert.True(result.FreedBytes >= 500);
        Assert.False(File.Exists(deletePath));
        Assert.True(File.Exists(lockedPath));
        Assert.True(File.Exists(prePath));
        Assert.True(File.Exists(healthyPath));
        Assert.True(File.Exists(outsidePath));
        Assert.False(Directory.Exists(Path.Combine(options.LudusaviBackupDirectory, ".gsc-retention-quarantine")) &&
                     Directory.EnumerateFiles(Path.Combine(options.LudusaviBackupDirectory, ".gsc-retention-quarantine"), "*", SearchOption.AllDirectories).Any());
        var remaining = await store.GetStorageAnalysisRowsAsync(CancellationToken.None);
        Assert.DoesNotContain(remaining, x => x.BackupId == "delete");
        Assert.Contains(remaining, x => x.BackupId == "locked");
        Assert.Contains(remaining, x => x.BackupId == "outside");
        var quarantineLedger = await store.GetRetentionQuarantineEntriesAsync(CancellationToken.None);
        var deletedLedger = Assert.Single(quarantineLedger);
        Assert.Equal("delete", deletedLedger.BackupId);
        Assert.Equal(RetentionQuarantineState.Deleted, deletedLedger.State);
    }

    [Fact]
    public async Task ApplyRejectsPreviewWhenIndexedStateChanged()
    {
        await AddZeroRetentionPolicyAsync("g1");
        var firstPath = CreateArchive("g1-first.zip", 500);
        await AddVersionAsync("g1", "first", firstPath, 500, DateTime.UtcNow.AddDays(-90));

        var service = new RetentionSimulationService(options, store, NullLogger<RetentionSimulationService>.Instance);
        var preview = await service.PreviewAsync(CancellationToken.None);
        await AddVersionAsync("g1", "new", CreateArchive("g1-new.zip", 700), 700, DateTime.UtcNow.AddDays(-90));

        var ex = await Assert.ThrowsAsync<WorkerOperationException>(() => service.ApplyAsync(
            new RetentionSimulationApplyRequestDto
            {
                Confirmed = true,
                PreviewId = preview.PreviewId,
                PreviewGeneratedUtc = preview.GeneratedUtc,
                ExpectedCandidateCount = preview.DeleteCandidateCount,
                ExpectedReleaseBytes = preview.EstimatedReleaseBytes
            }, CancellationToken.None));

        Assert.Equal("RETENTION_PREVIEW_STALE", ex.Code);
        Assert.True(File.Exists(firstPath));
    }

    [Fact]
    public async Task ApplyRejectsReplacementCandidateWithSameCountAndBytes()
    {
        await AddZeroRetentionPolicyAsync("g1");
        var firstPath = CreateArchive("g1-replaced-first.zip", 500);
        await AddVersionAsync("g1", "first", firstPath, 500, DateTime.UtcNow.AddDays(-90));

        var service = new RetentionSimulationService(options, store, NullLogger<RetentionSimulationService>.Instance);
        var preview = await service.PreviewAsync(CancellationToken.None);
        await store.DeleteBackupVersionAsync("g1", "first", CancellationToken.None);
        var replacementPath = CreateArchive("g1-replaced-second.zip", 500);
        await AddVersionAsync("g1", "second", replacementPath, 500, DateTime.UtcNow.AddDays(-90));

        var ex = await Assert.ThrowsAsync<WorkerOperationException>(() => service.ApplyAsync(
            new RetentionSimulationApplyRequestDto
            {
                Confirmed = true,
                PreviewId = preview.PreviewId,
                PreviewGeneratedUtc = preview.GeneratedUtc,
                ExpectedCandidateCount = preview.DeleteCandidateCount,
                ExpectedReleaseBytes = preview.EstimatedReleaseBytes
            }, CancellationToken.None));

        Assert.Equal("RETENTION_PREVIEW_STALE", ex.Code);
        Assert.True(File.Exists(firstPath));
        Assert.True(File.Exists(replacementPath));
    }

    [Fact]
    public async Task ApplyConsumesPreviewAndRejectsDuplicateSubmission()
    {
        await AddZeroRetentionPolicyAsync("g1");
        var archive = CreateArchive("g1-duplicate.zip", 500);
        await AddVersionAsync("g1", "duplicate", archive, 500, DateTime.UtcNow.AddDays(-90));

        var service = new RetentionSimulationService(options, store, NullLogger<RetentionSimulationService>.Instance);
        var preview = await service.PreviewAsync(CancellationToken.None);
        var request = new RetentionSimulationApplyRequestDto
        {
            Confirmed = true,
            PreviewId = preview.PreviewId,
            PreviewGeneratedUtc = preview.GeneratedUtc,
            ExpectedCandidateCount = preview.DeleteCandidateCount,
            ExpectedReleaseBytes = preview.EstimatedReleaseBytes
        };

        var result = await service.ApplyAsync(request, CancellationToken.None);
        Assert.Equal(1, result.DeletedCount);
        var ex = await Assert.ThrowsAsync<WorkerOperationException>(() => service.ApplyAsync(request, CancellationToken.None));
        Assert.Equal("RETENTION_PREVIEW_STALE", ex.Code);
    }

    [Fact]
    public async Task ApplyRejectsChangedArchiveAtTheSamePath()
    {
        await AddZeroRetentionPolicyAsync("g1");
        var archive = CreateArchive("g1-same-path.zip", 500);
        await AddVersionAsync("g1", "same-path", archive, 500, DateTime.UtcNow.AddDays(-90));

        var service = new RetentionSimulationService(options, store, NullLogger<RetentionSimulationService>.Instance);
        var preview = await service.PreviewAsync(CancellationToken.None);
        await File.WriteAllBytesAsync(archive, new byte[500]);
        File.SetLastWriteTimeUtc(archive, DateTime.UtcNow.AddSeconds(2));

        var ex = await Assert.ThrowsAsync<WorkerOperationException>(() => service.ApplyAsync(
            new RetentionSimulationApplyRequestDto
            {
                Confirmed = true,
                PreviewId = preview.PreviewId,
                PreviewGeneratedUtc = preview.GeneratedUtc
            }, CancellationToken.None));

        Assert.Equal("RETENTION_PREVIEW_STALE", ex.Code);
        Assert.True(File.Exists(archive));
    }

    [Fact]
    public async Task ApplyRejectsChangedRetentionPolicyEvenWhenCandidateSetIsUnchanged()
    {
        await AddZeroRetentionPolicyAsync("g1");
        var archive = CreateArchive("g1-policy-change.zip", 500);
        await AddVersionAsync("g1", "policy-change", archive, 500, DateTime.UtcNow.AddDays(-90));

        var service = new RetentionSimulationService(options, store, NullLogger<RetentionSimulationService>.Instance);
        var preview = await service.PreviewAsync(CancellationToken.None);
        await store.SetPolicyAsync("g1", new BackupPolicyDto
        {
            KeepRecentAllHours = 0,
            KeepDailyDays = 1,
            KeepWeeklyWeeks = 0,
            KeepMonthlyMonths = 0
        }, CancellationToken.None);

        var ex = await Assert.ThrowsAsync<WorkerOperationException>(() => service.ApplyAsync(
            new RetentionSimulationApplyRequestDto
            {
                Confirmed = true,
                PreviewId = preview.PreviewId,
                PreviewGeneratedUtc = preview.GeneratedUtc
            }, CancellationToken.None));

        Assert.Equal("RETENTION_PREVIEW_STALE", ex.Code);
        Assert.True(File.Exists(archive));
    }

    [Fact]
    public async Task ApplySkipsGameWhenSharedOperationLockIsHeld()
    {
        await AddZeroRetentionPolicyAsync("g1");
        var archive = CreateArchive("g1-busy.zip", 500);
        await AddVersionAsync("g1", "busy", archive, 500, DateTime.UtcNow.AddDays(-90));

        var gameLock = new GameOperationLock();
        var service = new RetentionSimulationService(
            options,
            store,
            NullLogger<RetentionSimulationService>.Instance,
            gameLock,
            TimeSpan.FromMilliseconds(25));
        var preview = await service.PreviewAsync(CancellationToken.None);
        using var held = await gameLock.AcquireAsync("g1", GameOperationKind.Backup, TimeSpan.FromSeconds(1), CancellationToken.None);
        Assert.NotNull(held);

        var result = await service.ApplyAsync(new RetentionSimulationApplyRequestDto
        {
            Confirmed = true,
            PreviewId = preview.PreviewId,
            PreviewGeneratedUtc = preview.GeneratedUtc
        }, CancellationToken.None);

        Assert.Equal(0, result.DeletedCount);
        Assert.Equal(1, result.SkippedBusyCount);
        Assert.True(File.Exists(archive));
    }

    [Fact]
    public async Task ApplyRejectsExpiredPreview()
    {
        var service = new RetentionSimulationService(options, store, NullLogger<RetentionSimulationService>.Instance);
        var ex = await Assert.ThrowsAsync<WorkerOperationException>(() => service.ApplyAsync(
            new RetentionSimulationApplyRequestDto
            {
                Confirmed = true,
                PreviewId = "expired-preview",
                PreviewGeneratedUtc = DateTime.UtcNow.AddMinutes(-11),
                ExpectedCandidateCount = 0,
                ExpectedReleaseBytes = 0
            }, CancellationToken.None));

        Assert.Equal("RETENTION_PREVIEW_STALE", ex.Code);
    }

    [Fact]
    public async Task ApplyHonorsCancellation()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var service = new RetentionSimulationService(options, store, NullLogger<RetentionSimulationService>.Instance);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.ApplyAsync(new RetentionSimulationApplyRequestDto { Confirmed = true }, cts.Token));
    }

    private async Task AddZeroRetentionPolicyAsync(string playniteId)
    {
        await store.SetPolicyAsync(playniteId, new BackupPolicyDto
        {
            KeepRecentAllHours = 0,
            KeepDailyDays = 0,
            KeepWeeklyWeeks = 0,
            KeepMonthlyMonths = 0
        }, CancellationToken.None);
    }

    private string CreateArchive(string name, int size)
    {
        var path = Path.Combine(options.LudusaviBackupDirectory, name);
        File.WriteAllBytes(path, new byte[size]);
        return path;
    }

    private async Task AddVersionAsync(
        string game,
        string backupId,
        string archivePath,
        long bytes,
        DateTime createdUtc,
        bool isLocked = false,
        bool isPreRestore = false,
        bool isReady = false)
    {
        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            PlayniteId = game,
            BackupId = backupId,
            LudusaviName = "Game " + game,
            CreatedUtc = createdUtc,
            TotalBytes = bytes,
            FileCount = 1,
            IsLocked = isLocked,
            IsPreRestore = isPreRestore,
            ArchivePath = archivePath,
            RestoreReadiness = isReady
                ? new RestoreReadinessDto { Status = RestoreReadinessStatus.Ready }
                : null
        }, "{}", CancellationToken.None);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
    }
}
