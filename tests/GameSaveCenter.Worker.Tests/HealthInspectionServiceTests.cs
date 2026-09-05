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
