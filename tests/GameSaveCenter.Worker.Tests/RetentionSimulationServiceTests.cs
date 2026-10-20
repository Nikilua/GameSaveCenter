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
        var candidateCreatedUtc = new DateTime(2026, 6, 20, 10, 20, 30, DateTimeKind.Utc);
        var archive = CreateArchive("g1-delete.zip", 100);
        await AddVersionAsync("g1", "delete", archive, 100, candidateCreatedUtc);
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
        Assert.Equal(candidateCreatedUtc, item.CreatedUtc);
        Assert.Equal(item.CreatedUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm"), item.CreatedDisplay);
        Assert.Equal("超出保留窗口或桶位", item.Reason);
        Assert.False(item.IsLocked);
        Assert.False(item.IsPreRestore);
        Assert.False(item.IsHealthProtected);
        Assert.Contains("候选清理 1 个", preview.Summary);
        Assert.Contains("用户锁定 1", preview.Summary);
        Assert.Contains("健康恢复点保护 1", preview.Summary);
        Assert.Contains("PreRestore 1", preview.Summary);
        Assert.DoesNotContain(preview.Items, candidate => candidate.IsLocked || candidate.IsPreRestore || candidate.IsHealthProtected);
    }

    [Fact]
    public async Task ScenarioComparisonUsesOnePlannerCutoffAndDoesNotCreateCleanupPreview()
    {
        await AddZeroRetentionPolicyAsync("g1");
        await AddZeroRetentionPolicyAsync("g2");
        var now = DateTime.UtcNow;
        var archivePaths = new[]
        {
            CreateArchive("scenario-delete-g1.zip", 100),
            CreateArchive("scenario-lock-g1.zip", 200),
            CreateArchive("scenario-pre-g1.zip", 300),
            CreateArchive("scenario-health-g1.zip", 400),
            CreateArchive("scenario-delete-g2.zip", 500)
        };
        File.Delete(archivePaths[4]);
        await AddVersionAsync("g1", "scenario-delete-g1", archivePaths[0], 100, now.AddDays(-100));
        await AddVersionAsync("g1", "scenario-lock-g1", archivePaths[1], 200, now.AddDays(-90), isLocked: true);
        await AddVersionAsync("g1", "scenario-pre-g1", archivePaths[2], 300, now.AddDays(-80), isPreRestore: true);
        await AddVersionAsync("g1", "scenario-health-g1", archivePaths[3], 400, now.AddDays(-70), isReady: true);
        await AddVersionAsync("g2", "scenario-delete-g2", archivePaths[4], 500, now.AddDays(-60));

        var service = new RetentionSimulationService(options, store, NullLogger<RetentionSimulationService>.Instance);
        var comparison = await service.CompareScenarioAsync(new RetentionScenarioComparisonRequestDto
        {
            KeepAllForHours = 0,
            KeepDailyDays = 365,
            KeepWeeklyWeeks = 0,
            KeepMonthlyMonths = 0,
            GrowthMinimumMiBPer30Days = 25,
            GrowthMaximumMiBPer30Days = 75
        }, CancellationToken.None);

        Assert.Equal(DateTimeKind.Utc, comparison.GeneratedUtc.Kind);
        Assert.Equal(5, comparison.BaselineVersionCount);
        Assert.Equal(comparison.BaselineVersionCount, comparison.Current.ExistingVersionCount);
        Assert.Equal(comparison.BaselineVersionCount, comparison.Candidate.ExistingVersionCount);
        Assert.Equal(3, comparison.Current.KeepVersionCount);
        Assert.Equal(2, comparison.Current.DeleteCandidateCount);
        Assert.Equal(3, comparison.Current.ProtectedVersionCount);
        Assert.Equal(1, comparison.Current.UserLockedCount);
        Assert.Equal(1, comparison.Current.PreRestoreCount);
        Assert.Equal(1, comparison.Current.HealthProtectedCount);
        Assert.Equal(600, comparison.Current.CandidateLogicalBytes);
        Assert.Equal(5, comparison.Candidate.KeepVersionCount);
        Assert.Equal(0, comparison.Candidate.DeleteCandidateCount);
        Assert.Equal(3, comparison.Candidate.ProtectedVersionCount);
        Assert.Equal(0, comparison.Candidate.CandidateLogicalBytes);
        Assert.Equal(25, comparison.GrowthMinimumMiBPer30Days);
        Assert.Equal(75, comparison.GrowthMaximumMiBPer30Days);
        Assert.Equal(2, comparison.Games.Count);
        Assert.Equal(2, comparison.ChangedGameCount);
        Assert.Equal(2, comparison.ChangedVersionCount);
        Assert.Equal(comparison.Current.ExistingVersionCount, comparison.Games.Sum(x => x.ExistingVersionCount));
        Assert.Equal(comparison.Current.KeepVersionCount, comparison.Games.Sum(x => x.CurrentKeepVersionCount));
        Assert.Equal(comparison.Current.DeleteCandidateCount, comparison.Games.Sum(x => x.CurrentDeleteCandidateCount));
        Assert.Equal(comparison.Candidate.KeepVersionCount, comparison.Games.Sum(x => x.CandidateKeepVersionCount));
        Assert.Equal(comparison.Candidate.DeleteCandidateCount, comparison.Games.Sum(x => x.CandidateDeleteCandidateCount));
        Assert.All(comparison.Games, game =>
        {
            Assert.Equal(game.ExistingVersionCount, game.CurrentKeepVersionCount + game.CurrentDeleteCandidateCount);
            Assert.Equal(game.ExistingVersionCount, game.CandidateKeepVersionCount + game.CandidateDeleteCandidateCount);
            Assert.True(game.CurrentProtectedVersionCount <= game.CurrentKeepVersionCount);
            Assert.True(game.CandidateProtectedVersionCount <= game.CandidateKeepVersionCount);
        });
        var missingArchiveDetail = Assert.Single(comparison.Games.Single(x => x.PlayniteId == "g2").VersionDetails);
        Assert.Contains("归档缺失", missingArchiveDetail.ArchiveAvailabilityDisplay);
        Assert.Contains("不计入现存归档候选", missingArchiveDetail.ArchiveAvailabilityDisplay);
        Assert.True(missingArchiveDetail.IsCurrentDeleteCandidate);
        Assert.False(missingArchiveDetail.IsCandidateDeleteCandidate);
        var protectedDetails = comparison.Games.SelectMany(x => x.VersionDetails).Where(x => x.IsProtected).ToList();
        Assert.Equal(3, protectedDetails.Count);
        Assert.All(protectedDetails, detail =>
        {
            Assert.False(detail.IsCurrentDeleteCandidate);
            Assert.False(detail.IsCandidateDeleteCandidate);
            Assert.Contains("保护项不进入淘汰候选", detail.ArchiveAvailabilityDisplay);
        });
        Assert.Contains("统一应用到所有游戏", comparison.Assumptions);
        Assert.Contains("不参与保留数量", comparison.Assumptions);
        Assert.Equal(comparison.Current.KeepVersionCount + comparison.Current.DeleteCandidateCount, comparison.Current.ExistingVersionCount);
        Assert.Equal(comparison.Candidate.KeepVersionCount + comparison.Candidate.DeleteCandidateCount, comparison.Candidate.ExistingVersionCount);

        var snapshot = await store.GetRetentionScenarioSnapshotAsync(CancellationToken.None);
        var candidatePolicy = new RetentionPolicy
        {
            KeepAllFor = TimeSpan.Zero,
            KeepDailyDays = 365,
            KeepWeeklyWeeks = 0,
            KeepMonthlyMonths = 0
        };
        var directCurrent = CountWithPlanner(snapshot.Versions, snapshot.Policies, null, comparison.GeneratedUtc);
        var directCandidate = CountWithPlanner(snapshot.Versions, snapshot.Policies, candidatePolicy, comparison.GeneratedUtc);
        Assert.Equal(directCurrent.Keep, comparison.Current.KeepVersionCount);
        Assert.Equal(directCurrent.Delete, comparison.Current.DeleteCandidateCount);
        Assert.Equal(directCandidate.Keep, comparison.Candidate.KeepVersionCount);
        Assert.Equal(directCandidate.Delete, comparison.Candidate.DeleteCandidateCount);

        var staleApply = await Assert.ThrowsAsync<WorkerOperationException>(() => service.ApplyAsync(
            new RetentionSimulationApplyRequestDto
            {
                Confirmed = true,
                PreviewId = Guid.NewGuid().ToString("N"),
                PreviewGeneratedUtc = comparison.GeneratedUtc
            }, CancellationToken.None));
        Assert.Equal("RETENTION_PREVIEW_STALE", staleApply.Code);
        Assert.All(archivePaths.Take(4), path => Assert.True(File.Exists(path)));
        Assert.False(File.Exists(archivePaths[4]));
        Assert.Equal(5, (await store.GetStorageAnalysisRowsAsync(CancellationToken.None)).Count);
    }

    [Fact]
    public async Task ScenarioComparisonRejectsInvalidWorkerSideInputs()
    {
        var service = new RetentionSimulationService(options, store, NullLogger<RetentionSimulationService>.Instance);
        var ex = await Assert.ThrowsAsync<WorkerOperationException>(() => service.CompareScenarioAsync(
            new RetentionScenarioComparisonRequestDto
            {
                KeepDailyDays = 36500,
                KeepWeeklyWeeks = 0,
                KeepMonthlyMonths = 0,
                GrowthMinimumMiBPer30Days = 400,
                GrowthMaximumMiBPer30Days = 200
            }, CancellationToken.None));

        Assert.Equal("RETENTION_SCENARIO_INVALID", ex.Code);
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
    public async Task IndexDeletionFailureRestoresArchiveAndDoesNotCountFreedBytes()
    {
        await AddZeroRetentionPolicyAsync("g1");
        var archive = CreateArchive("g1-delete-failure.zip", 500);
        await AddVersionAsync("g1", "delete-failure", archive, 500, DateTime.UtcNow.AddDays(-90));
        await ExecuteSqlAsync(@"CREATE TRIGGER fail_retention_index_delete
BEFORE DELETE ON backup_versions
WHEN OLD.backup_id = 'delete-failure'
BEGIN SELECT RAISE(ABORT, 'injected retention index deletion failure'); END;");

        try
        {
            var service = new RetentionSimulationService(options, store, NullLogger<RetentionSimulationService>.Instance);
            var preview = await service.PreviewAsync(CancellationToken.None);
            var result = await service.ApplyAsync(new RetentionSimulationApplyRequestDto
            {
                Confirmed = true,
                PreviewId = preview.PreviewId,
                PreviewGeneratedUtc = preview.GeneratedUtc
            }, CancellationToken.None);

            Assert.Equal(0, result.DeletedCount);
            Assert.Equal(0, result.MovedBytes);
            Assert.Equal(0, result.FreedBytes);
            Assert.Equal(1, result.FailedCount);
            Assert.True(result.RecoveryRequiredCount >= 1);
            Assert.True(File.Exists(archive));
            Assert.Contains(await store.GetStorageAnalysisRowsAsync(CancellationToken.None), x => x.BackupId == "delete-failure");
        }
        finally
        {
            await ExecuteSqlAsync("DROP TRIGGER fail_retention_index_delete;");
        }
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

    private async Task ExecuteSqlAsync(string sql)
    {
        await using var connection = new SqliteConnection($"Data Source={options.DatabasePath};Cache=Shared;Foreign Keys=True");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
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

    private static (int Keep, int Delete) CountWithPlanner(
        IReadOnlyList<BackupVersionDto> versions,
        IReadOnlyDictionary<string, BackupPolicyDto> policies,
        RetentionPolicy? overridePolicy,
        DateTime cutoffUtc)
    {
        var planner = new RetentionPlanner();
        var plans = versions.GroupBy(x => x.PlayniteId, StringComparer.OrdinalIgnoreCase).Select(group =>
        {
            var effective = overridePolicy;
            if (effective == null)
            {
                policies.TryGetValue(group.Key, out var stored);
                effective = new RetentionPolicy
                {
                    KeepAllFor = TimeSpan.FromHours(stored?.KeepRecentAllHours ?? 24),
                    KeepDailyDays = stored?.KeepDailyDays ?? 30,
                    KeepWeeklyWeeks = stored?.KeepWeeklyWeeks ?? 12,
                    KeepMonthlyMonths = stored?.KeepMonthlyMonths ?? 24
                };
            }
            return planner.CreatePlan(group.Select(version => new BackupSnapshot
            {
                BackupId = version.BackupId,
                CreatedUtc = version.CreatedUtc,
                TotalBytes = version.TotalBytes,
                FileCount = version.FileCount,
                IsLocked = version.IsLocked,
                IsPreRestore = version.IsPreRestore,
                ReadinessStatus = version.RestoreReadiness?.Status,
                HasSevereAnomaly = version.FileCount == 0 || version.TotalBytes <= 0 ||
                    version.RestoreReadiness?.Status is RestoreReadinessStatus.Corrupted or RestoreReadinessStatus.Failed
            }), effective, cutoffUtc);
        }).ToList();
        return (plans.Sum(x => x.Keep.Count), plans.Sum(x => x.DeleteCandidates.Count));
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
    }
}
