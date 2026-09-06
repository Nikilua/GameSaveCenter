using System.IO.Compression;
using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Models;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class HealthInspectionServiceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly WorkerOptions options;
    private readonly SqliteStateStore store;

    public HealthInspectionServiceTests()
    {
        Directory.CreateDirectory(root);
        options = new WorkerOptions
        {
            DataDirectory = Path.Combine(root, "Data"),
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            MediaArchiveDirectory = Path.Combine(root, "Media"),
            HealthInspectionIntervalMinutes = 15,
            HealthInspectionStaleAfterDays = 30,
            HealthInspectionMaxDurationSeconds = 30
        };
        store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task StatePersistsInFlightCursorAcrossRestart()
    {
        var started = DateTime.UtcNow.AddMinutes(-2);
        var state = new HealthInspectionStateDto
        {
            Enabled = true,
            IntervalMinutes = 15,
            StaleAfterDays = 30,
            MaxDurationSeconds = 30,
            NextDueUtc = DateTime.UtcNow.AddMinutes(10),
            LastStartedUtc = started,
            LastCompletedUtc = started.AddMinutes(-1),
            CursorPlayniteId = "game-1",
            CursorBackupId = "backup-1",
            LastPlayniteId = "game-1",
            LastBackupId = "backup-1",
            LastStatus = "Running",
            LastSummary = "正在恢复校验。",
            DeferredCount = 2,
            FailureCount = 1
        };
        await store.SaveHealthInspectionStateAsync(state, CancellationToken.None);

        var restarted = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        await restarted.InitializeAsync(CancellationToken.None);
        var loaded = await restarted.GetHealthInspectionStateAsync(CancellationToken.None);

        Assert.True(loaded.IsRunning);
        Assert.Equal("game-1", loaded.CursorPlayniteId);
        Assert.Equal("backup-1", loaded.CursorBackupId);
        Assert.Equal("Running", loaded.LastStatus);
        Assert.Equal(2, loaded.DeferredCount);
        Assert.Equal(1, loaded.FailureCount);
    }

    [Fact]
    public async Task ValidArchive_IsCheckedInIsolation_AndPersistsSuccess()
    {
        var archive = CreateArchive(("profile.dat", "save"));
        await AddBackupAsync("game-1", "backup-1", archive, Manifest("profile.dat", 4));
        var service = CreateService();

        var result = await service.RunNowAsync(CancellationToken.None);

        Assert.Equal("Ready", result.LastStatus);
        Assert.NotNull(result.LastSuccessfulUtc);
        var stored = Assert.Single(await store.GetBackupVersionsAsync("game-1", CancellationToken.None));
        Assert.Equal(RestoreReadinessStatus.Ready, stored.RestoreReadiness?.Status);
        Assert.Equal("Cleaned", stored.RestoreReadiness?.StagingCleanupStatus);
        Assert.Empty(await store.GetOpenFindingsAsync(20, CancellationToken.None));
        Assert.True(!Directory.Exists(options.RestoreReadinessDirectory)
            || !Directory.EnumerateFileSystemEntries(options.RestoreReadinessDirectory).Any());
    }

    [Fact]
    public async Task CorruptArchive_CreatesAttentionFinding()
    {
        var archive = Path.Combine(root, "corrupt.zip");
        await File.WriteAllTextAsync(archive, "not a zip");
        await AddBackupAsync("game-1", "backup-1", archive, "[]");
        var service = CreateService();

        var result = await service.RunNowAsync(CancellationToken.None);

        Assert.Equal("Corrupted", result.LastStatus);
        Assert.Equal(1, result.FailureCount);
        var finding = Assert.Single(await store.GetOpenFindingsAsync(20, CancellationToken.None));
        Assert.Equal("HEALTH_INSPECTION_FAILED", finding.Code);
        Assert.Equal(FindingSeverity.Error, finding.Severity);
    }

    [Fact]
    public async Task RunningGame_DefersWithoutReadingArchive()
    {
        var archive = Path.Combine(root, "missing.zip");
        await AddBackupAsync("game-1", "backup-1", archive, "[]");
        var sessions = new FakeRestoreSessionState();
        sessions.Active.Add(new GameSessionEventDto { PlayniteId = "game-1", GameName = "Example" });
        var service = CreateService(sessions);

        var result = await service.RunNowAsync(CancellationToken.None);

        Assert.Equal("Deferred", result.LastStatus);
        Assert.Equal(1, result.DeferredCount);
        var stored = Assert.Single(await store.GetBackupVersionsAsync("game-1", CancellationToken.None));
        Assert.Null(stored.RestoreReadiness);
        Assert.Empty(await store.GetOpenFindingsAsync(20, CancellationToken.None));
    }

    [Fact]
    public async Task ScheduledLoopBacksOffWhileManualInspectionOwnsRunGate()
    {
        var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var service = CreateService();
        var hookUsed = 0;
        var gateMisses = 0;
        service.InspectionStageHook = stage =>
        {
            if (stage == HealthInspectionStage.RunningStateSaved && Interlocked.Exchange(ref hookUsed, 1) == 0)
            {
                entered.TrySetResult(true);
                return release.Task;
            }
            return Task.CompletedTask;
        };
        service.ScheduledGateMissHook = () =>
        {
            Interlocked.Increment(ref gateMisses);
            return Task.CompletedTask;
        };

        var manual = service.RunNowAsync(CancellationToken.None);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        using var backgroundCancellation = new CancellationTokenSource();
        await service.StartAsync(backgroundCancellation.Token);
        await Task.Delay(TimeSpan.FromMilliseconds(700));

        Assert.InRange(gateMisses, 1, 5);
        backgroundCancellation.Cancel();
        release.TrySetResult(true);
        var result = await manual;
        await service.StopAsync(CancellationToken.None);
        Assert.Equal("NoBackups", result.LastStatus);
    }

    [Fact]
    public async Task CandidateIdentityIsPersistedBeforeArchiveValidation()
    {
        var archive = CreateArchive(("profile.dat", "save"));
        await AddBackupAsync("game-1", "backup-1", archive, Manifest("profile.dat", 4));
        var persisted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var service = CreateService();
        service.InspectionStageHook = async stage =>
        {
            if (stage != HealthInspectionStage.CandidateStateSaved) return;
            var state = await store.GetHealthInspectionStateAsync(CancellationToken.None);
            if (state.CursorPlayniteId == "game-1" && state.CursorBackupId == "backup-1" && state.IsRunning)
                persisted.TrySetResult(true);
        };

        var result = await service.RunNowAsync(CancellationToken.None);

        Assert.Equal("Ready", result.LastStatus);
        Assert.True(await persisted.Task.WaitAsync(TimeSpan.FromSeconds(2)));
    }

    [Fact]
    public async Task CandidateStateWriteFailureDoesNotCreateArchiveFailure()
    {
        var archive = CreateArchive(("profile.dat", "save"));
        await AddBackupAsync("game-1", "backup-1", archive, Manifest("profile.dat", 4));
        await ExecuteSqlAsync(@"CREATE TRIGGER fail_health_candidate_state_update
BEFORE UPDATE OF cursor_playnite_id ON health_inspection_state
WHEN NEW.cursor_playnite_id <> ''
BEGIN SELECT RAISE(ABORT, 'injected candidate state failure'); END;");

        try
        {
            var result = await CreateService().RunNowAsync(CancellationToken.None);

            Assert.Equal("Failed", result.LastStatus);
            Assert.Contains("尚未开始归档校验", result.LastSummary);
            var stored = Assert.Single(await store.GetBackupVersionsAsync("game-1", CancellationToken.None));
            Assert.Null(stored.RestoreReadiness);
            Assert.Empty(await store.GetOpenFindingsAsync(20, CancellationToken.None));
        }
        finally
        {
            await ExecuteSqlAsync("DROP TRIGGER fail_health_candidate_state_update;");
        }
    }

    [Fact]
    public async Task DeferredGameDoesNotStarveAnotherCheckableGame()
    {
        var deferredArchive = Path.Combine(root, "deferred-missing.zip");
        await AddBackupAsync("game-a", "backup-a", deferredArchive, "[]");
        var healthyArchive = CreateArchive(("profile.dat", "save"));
        await AddBackupAsync("game-b", "backup-b", healthyArchive, Manifest("profile.dat", 4));
        var sessions = new FakeRestoreSessionState();
        sessions.Active.Add(new GameSessionEventDto { PlayniteId = "game-a", GameName = "Running Game" });
        var service = CreateService(sessions);

        var first = await service.RunNowAsync(CancellationToken.None);
        var second = await service.RunNowAsync(CancellationToken.None);

        Assert.Equal("Deferred", first.LastStatus);
        Assert.Equal("game-a", first.LastPlayniteId);
        Assert.Equal("Ready", second.LastStatus);
        Assert.Equal("game-b", second.LastPlayniteId);
        var deferred = await store.GetHealthInspectionDeferredCandidatesAsync(CancellationToken.None);
        Assert.Contains(deferred, x => x.PlayniteId == "game-a" && x.BackupId == "backup-a");
    }

    [Fact]
    public async Task FreshBackupSetReturnsUpToDateWithoutReadingArchiveAgain()
    {
        var archive = CreateArchive(("profile.dat", "save"));
        await AddBackupAsync("game-1", "backup-1", archive, Manifest("profile.dat", 4));
        var service = CreateService();

        var first = await service.RunNowAsync(CancellationToken.None);
        File.Delete(archive);
        var second = await service.RunNowAsync(CancellationToken.None);

        Assert.Equal("Ready", first.LastStatus);
        Assert.Equal("UpToDate", second.LastStatus);
    }

    [Fact]
    public async Task PersistedInFlightCursorWinsOverNewerUnvalidatedCandidate()
    {
        var inFlightArchive = CreateArchive(("profile.dat", "save"));
        await AddBackupAsync("game-a", "backup-a", inFlightArchive, Manifest("profile.dat", 4));
        await AddBackupAsync("game-b", "backup-b", Path.Combine(root, "missing.zip"), "[]");
        await store.SaveRestoreReadinessAsync("game-a", "backup-a", new RestoreReadinessDto
        {
            BackupVersionId = "backup-a", CheckedUtc = DateTime.UtcNow, Status = RestoreReadinessStatus.Ready,
            ExpectedFileCount = 1, ExpectedTotalSize = 4, StagingCleanupStatus = "Cleaned", Summary = "ok"
        }, CancellationToken.None);
        var interrupted = new HealthInspectionStateDto
        {
            Enabled = true, IntervalMinutes = 15, StaleAfterDays = 30, MaxDurationSeconds = 30,
            NextDueUtc = DateTime.UtcNow.AddMinutes(10), LastStartedUtc = DateTime.UtcNow.AddMinutes(-1),
            LastCompletedUtc = DateTime.UtcNow.AddMinutes(-2), CursorPlayniteId = "game-a", CursorBackupId = "backup-a",
            LastPlayniteId = "game-a", LastBackupId = "backup-a", LastStatus = "Running", LastSummary = "interrupted"
        };
        await store.SaveHealthInspectionStateAsync(interrupted, CancellationToken.None);

        var result = await CreateService().RunNowAsync(CancellationToken.None);

        Assert.Equal("Ready", result.LastStatus);
        Assert.Equal("game-a", result.LastPlayniteId);
        Assert.Equal("backup-a", result.LastBackupId);
    }

    [Fact]
    public async Task ExecutionStateWriteDoesNotOverwriteAConcurrentPlanChange()
    {
        var initial = new HealthInspectionStateDto
        {
            Enabled = true,
            IntervalMinutes = 15,
            StaleAfterDays = 30,
            MaxDurationSeconds = 30,
            NextDueUtc = DateTime.UtcNow.AddMinutes(15),
            LastStatus = "NeverRun"
        };
        await store.SaveHealthInspectionStateAsync(initial, CancellationToken.None);
        options.HealthInspectionEnabled = false;
        options.HealthInspectionIntervalMinutes = 60;
        options.HealthInspectionStaleAfterDays = 90;
        options.HealthInspectionMaxDurationSeconds = 120;
        var service = CreateService();

        var planned = await service.SyncPlanAsync(CancellationToken.None);
        Assert.False(planned.Enabled);
        Assert.Equal(60, planned.IntervalMinutes);

        initial.LastStatus = "Running";
        initial.LastStartedUtc = DateTime.UtcNow;
        await store.SaveHealthInspectionExecutionStateAsync(initial, CancellationToken.None);

        var loaded = await store.GetHealthInspectionStateAsync(CancellationToken.None);
        Assert.False(loaded.Enabled);
        Assert.Equal(60, loaded.IntervalMinutes);
        Assert.Equal(90, loaded.StaleAfterDays);
        Assert.Equal(120, loaded.MaxDurationSeconds);
        Assert.Equal("Running", loaded.LastStatus);
    }

    private HealthInspectionService CreateService(FakeRestoreSessionState? sessions = null)
        => new(
            store,
            new RestoreReadinessService(NullLogger<RestoreReadinessService>.Instance),
            new GameOperationLock(),
            sessions ?? new FakeRestoreSessionState(),
            options,
            NullLogger<HealthInspectionService>.Instance);

    private async Task AddBackupAsync(string playniteId, string backupId, string archive, string manifest)
    {
        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            PlayniteId = playniteId,
            BackupId = backupId,
            LudusaviName = "Example",
            CreatedUtc = DateTime.UtcNow,
            ArchivePath = archive,
            FileCount = 1,
            TotalBytes = 4
        }, manifest, CancellationToken.None);
    }

    private string CreateArchive(params (string Name, string Content)[] entries)
    {
        var path = Path.Combine(root, Guid.NewGuid().ToString("N") + ".zip");
        using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
        foreach (var (name, content) in entries)
        {
            using var writer = new StreamWriter(archive.CreateEntry(name).Open());
            writer.Write(content);
        }
        return path;
    }

    private static string Manifest(string path, long bytes)
        => JsonSerializer.Serialize(new[] { new FileManifestEntry { RelativePath = path, SizeBytes = bytes } });

    private async Task ExecuteSqlAsync(string sql)
    {
        await using var connection = new SqliteConnection($"Data Source={options.DatabasePath};Cache=Shared;Foreign Keys=True");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); } catch { }
    }

    private sealed class FakeRestoreSessionState : IRestoreSessionState
    {
        public List<GameSessionEventDto> Active { get; } = new();
        public IReadOnlyCollection<GameSessionEventDto> ActiveSessions => Active;
    }
}
