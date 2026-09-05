using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Ipc;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

/// <summary>
/// Disaster-drill coverage for the restore state machine. All state lives below a unique
/// temp directory and the fake client only mutates an in-memory save marker.
/// </summary>
public sealed class RestoreOrchestratorTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly WorkerOptions options;
    private readonly SqliteStateStore store;
    private readonly FakeRestoreClient client = new();
    private readonly TaskCoordinator tasks;
    private readonly FakeRestoreCatalog catalog;
    private readonly FakeRestoreSessionState sessions = new();
    private readonly CloudTransferCoordinator cloud = new(NullLogger<CloudTransferCoordinator>.Instance);

    public RestoreOrchestratorTests()
    {
        Directory.CreateDirectory(root);
        options = new WorkerOptions
        {
            DataDirectory = root,
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            MediaArchiveDirectory = Path.Combine(root, "Media")
        };
        store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
        catalog = new FakeRestoreCatalog();
        var events = new TaskEventBroadcaster();
        tasks = new TaskCoordinator(store, events, NullLogger<TaskCoordinator>.Instance);
    }

    [Fact]
    public async Task RestoreFailure_RollsBackToPreRestore_AndLeavesNoLiveSaveAccess()
    {
        await SeedGameAsync();
        client.Backups.Add("A", "A");
        client.Backups.Add("B", "B");
        client.FailRestoreFor.Add("B");

        var result = await CreateOrchestrator().ExecuteAsync(Request("B"), CancellationToken.None);

        Assert.Equal(TaskState.Failed, result.State);
        Assert.Equal("A", client.CurrentSave);
        Assert.Contains("B", client.RestoreCalls);
        Assert.Contains(client.RestoreCalls, x => x.StartsWith("pre-", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(client.EditedBackups, x => x.BackupId.StartsWith("pre-", StringComparison.OrdinalIgnoreCase) && x.Locked == true);
        Assert.DoesNotContain(Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories),
            x => string.Equals(Path.GetFileName(x), "profile.dat", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(new[] { RestoreState.Requested, RestoreState.GameClosedVerified, RestoreState.PreRestoreBackupCreated,
            RestoreState.CloudJobsPaused, RestoreState.RollbackAttempted, RestoreState.RolledBack }, await ReadRestoreStatesAsync());
    }

    [Fact]
    public async Task SuccessfulRestoreThenUndo_RestoresThePreRestoreState()
    {
        await SeedGameAsync();
        client.Backups.Add("A", "A");
        client.Backups.Add("B", "B");

        var restore = await CreateOrchestrator().ExecuteAsync(Request("B"), CancellationToken.None);
        Assert.Equal(TaskState.Succeeded, restore.State);
        Assert.Equal("B", client.CurrentSave);

        var undo = await CreateOrchestrator().UndoAsync("game-1", CancellationToken.None);

        Assert.Equal(TaskState.Succeeded, undo.State);
        Assert.Equal("A", client.CurrentSave);
        Assert.Contains(client.RestoreCalls, x => x == "B");
        Assert.True(client.RestoreCalls.Count(x => x.StartsWith("pre-", StringComparison.OrdinalIgnoreCase)) >= 2,
            $"Expected the original and undo PreRestore snapshots, got: {string.Join(",", client.RestoreCalls)}");
    }

    [Fact]
    public async Task RestoreA_FromCurrentB_CompletesAtoBackupBtoRestoreASequence()
    {
        await SeedGameAsync();
        client.SetCurrentSave("B");
        client.Backups.Add("A", "A");

        var result = await CreateOrchestrator().ExecuteAsync(Request("A"), CancellationToken.None);

        Assert.Equal(TaskState.Succeeded, result.State);
        Assert.Equal("A", client.CurrentSave);
        Assert.Equal("A", await File.ReadAllTextAsync(client.LiveSavePath));
    }

    [Fact]
    public async Task PartialWriteFailure_RollsBackFilesToPreRestoreState()
    {
        await SeedGameAsync();
        client.SetCurrentSave("B");
        client.Backups.Add("A", "A");
        client.PartialFailRestoreFor.Add("A");

        var result = await CreateOrchestrator().ExecuteAsync(Request("A"), CancellationToken.None);

        Assert.Equal(TaskState.Failed, result.State);
        Assert.Equal("RESTORE_FAILED_ROLLED_BACK", result.ErrorCode);
        Assert.Equal("B", client.CurrentSave);
        Assert.Equal("B", await File.ReadAllTextAsync(client.LiveSavePath));
    }

    [Fact]
    public async Task WriteExceptionAfterPartialMutation_RollsBackFiles()
    {
        await SeedGameAsync();
        client.SetCurrentSave("B");
        client.Backups.Add("A", "A");
        client.ThrowAfterWriteFor.Add("A");

        var result = await CreateOrchestrator().ExecuteAsync(Request("A"), CancellationToken.None);

        Assert.Equal(TaskState.Failed, result.State);
        Assert.Equal("RESTORE_FAILED_ROLLED_BACK", result.ErrorCode);
        Assert.Equal("B", await File.ReadAllTextAsync(client.LiveSavePath));
    }

    [Fact]
    public async Task PostRestoreValidationFailure_RollsBackFiles()
    {
        await SeedGameAsync();
        client.SetCurrentSave("B");
        client.Backups.Add("A", "A");
        client.FailPostValidationFor.Add("A");

        var result = await CreateOrchestrator().ExecuteAsync(Request("A"), CancellationToken.None);

        Assert.Equal(TaskState.Failed, result.State);
        Assert.Equal("RESTORE_FAILED_ROLLED_BACK", result.ErrorCode);
        Assert.Equal("B", await File.ReadAllTextAsync(client.LiveSavePath));
    }

    [Fact]
    public async Task PermissionDeniedAndReadOnlyFailures_RollBackWithoutLosingCurrentState()
    {
        await SeedGameAsync();
        client.SetCurrentSave("B");
        client.Backups.Add("permission", "A");
        client.Backups.Add("readonly", "A");
        client.ThrowUnauthorizedFor.Add("permission");
        client.ThrowReadOnlyFor.Add("readonly");

        var permission = await CreateOrchestrator().ExecuteAsync(Request("permission"), CancellationToken.None);
        var readOnly = await CreateOrchestrator().ExecuteAsync(Request("readonly"), CancellationToken.None);

        Assert.Equal("RESTORE_FAILED_ROLLED_BACK", permission.ErrorCode);
        Assert.Equal("RESTORE_FAILED_ROLLED_BACK", readOnly.ErrorCode);
        Assert.Equal("B", await File.ReadAllTextAsync(client.LiveSavePath));
    }

    [Fact]
    public async Task MissingLiveSaveDirectory_IsCreatedByRestoreAndRemainsUndoable()
    {
        await SeedGameAsync();
        client.SetCurrentSave("B");
        client.Backups.Add("A", "A");
        Directory.Delete(client.LiveDirectory, true);

        var result = await CreateOrchestrator().ExecuteAsync(Request("A"), CancellationToken.None);
        var undo = await CreateOrchestrator().UndoAsync("game-1", CancellationToken.None);

        Assert.Equal(TaskState.Succeeded, result.State);
        Assert.Equal(TaskState.Succeeded, undo.State);
        Assert.Equal("B", await File.ReadAllTextAsync(client.LiveSavePath));
    }

    [Fact]
    public async Task RollbackFailure_IsMarkedForManualIntervention()
    {
        await SeedGameAsync();
        client.SetCurrentSave("B");
        client.Backups.Add("A", "A");
        client.PartialFailRestoreFor.Add("A");
        client.FailRollback = true;

        var result = await CreateOrchestrator().ExecuteAsync(Request("A"), CancellationToken.None);

        Assert.Equal(TaskState.Failed, result.State);
        Assert.Equal("RESTORE_ROLLBACK_FAILED", result.ErrorCode);
        Assert.Contains(RestoreState.ManualInterventionRequired, await ReadRestoreStatesAsync());
    }

    [Fact]
    public async Task RestoreIsRejectedWhileSessionIsActive()
    {
        await SeedGameAsync();
        sessions.Active.Add(new GameSessionEventDto { PlayniteId = "game-1", GameName = "Test Game" });

        var result = await CreateOrchestrator().ExecuteAsync(Request("B"), CancellationToken.None);

        Assert.Equal(TaskState.Failed, result.State);
        Assert.Equal("RESTORE_GAME_RUNNING", result.ErrorCode);
        Assert.Empty(client.RestoreCalls);
    }

    [Fact]
    public async Task FailedReadinessCheck_BlocksRealRestoreBeforeAnyClientCall()
    {
        await SeedGameAsync();
        client.Backups.Add("B", "B");
        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            PlayniteId = "game-1",
            BackupId = "B",
            LudusaviName = "Test Game",
            CreatedUtc = DateTime.UtcNow,
            FileCount = 1,
            TotalBytes = 4,
            RestoreReadiness = new RestoreReadinessDto
            {
                BackupVersionId = "B",
                Status = RestoreReadinessStatus.Corrupted,
                ErrorCount = 1,
                Summary = "归档损坏。"
            }
        }, "[]", CancellationToken.None);

        var exception = await Assert.ThrowsAsync<WorkerOperationException>(
            () => CreateOrchestrator().ExecuteAsync(Request("B"), CancellationToken.None));

        Assert.Equal("RESTORE_READINESS_FAILED", exception.Code);
        Assert.Empty(client.RestoreCalls);
    }

    private RestoreOrchestrator CreateOrchestrator()
        => new(catalog, store, client, tasks, sessions, cloud, new FakeRemoteStageProvider(), new GameOperationLock());

    private static RestoreRequestDto Request(string backupId) => new()
    {
        PlayniteId = "game-1", BackupId = backupId, ConfirmedCurrentSnapshot = true, ConfirmedGameClosed = true
    };

    private async Task SeedGameAsync()
    {
        await store.UpsertGamesAsync(new[] { new GameDescriptorDto { PlayniteId = "game-1", Name = "Test Game", IsInstalled = true } }, CancellationToken.None);
        await store.SetGameMatchAsync("game-1", "Test Game", 1, GameMatchInput.CreateHash(new GameDescriptorDto { PlayniteId = "game-1", Name = "Test Game", IsInstalled = true }), CancellationToken.None);
        catalog.Games["game-1"] = new GameDescriptorDto { PlayniteId = "game-1", Name = "Test Game", IsInstalled = true };
        catalog.Matches["game-1"] = ("Test Game", 1);
    }

    private async Task<IReadOnlyList<RestoreState>> ReadRestoreStatesAsync()
    {
        var audit = await store.GetAuditAsync(100, CancellationToken.None);
        return audit.Where(x => x.Category == "Restore")
            .Select(x => Enum.TryParse<RestoreState>(x.Message, out var state) ? state : (RestoreState?)null)
            .Where(x => x.HasValue).Select(x => x!.Value).Reverse().ToArray();
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        client.Dispose();
    }

    private sealed class FakeRestoreCatalog : IRestoreCatalog
    {
        public Dictionary<string, GameDescriptorDto> Games { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, (string Name, double Confidence)> Matches { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Task<GameDescriptorDto?> GetGameAsync(string playniteId, CancellationToken token)
            => Task.FromResult(Games.TryGetValue(playniteId, out var game) ? game : null);
        public Task<Dictionary<string, (string Name, double Confidence)>> GetMatchesAsync(CancellationToken token)
            => Task.FromResult(new Dictionary<string, (string Name, double Confidence)>(Matches, StringComparer.OrdinalIgnoreCase));
    }

    private sealed class FakeRestoreSessionState : IRestoreSessionState
    {
        public List<GameSessionEventDto> Active { get; } = new();
        public IReadOnlyCollection<GameSessionEventDto> ActiveSessions => Active;
    }

    private sealed class FakeRemoteStageProvider : IRemoteBackupStageProvider
    {
        public Task<RemoteBackupStage> RevalidateAsync(string stagingId, CancellationToken token)
            => throw new NotSupportedException();
    }

    private sealed class FakeRestoreClient : IRestoreClient, IDisposable
    {
        public Dictionary<string, string> Backups { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> FailRestoreFor { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> PartialFailRestoreFor { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> ThrowAfterWriteFor { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> ThrowUnauthorizedFor { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> ThrowReadOnlyFor { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> FailPostValidationFor { get; } = new(StringComparer.OrdinalIgnoreCase);
        public List<string> RestoreCalls { get; } = new();
        public List<(string BackupId, bool? Locked)> EditedBackups { get; } = new();
        public bool FailRollback { get; set; }
        public string LiveDirectory { get; }
        public string LiveSavePath => Path.Combine(LiveDirectory, "profile.dat");
        public string CurrentSave { get; private set; } = "A";

        public FakeRestoreClient()
        {
            LiveDirectory = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"), "live");
            SetCurrentSave("A");
        }

        public void SetCurrentSave(string value)
        {
            CurrentSave = value;
            Directory.CreateDirectory(LiveDirectory);
            File.WriteAllText(LiveSavePath, value);
        }

        public Task<LudusaviCommandResult> BackupAsync(IEnumerable<string> games, bool force, bool preview, CancellationToken token)
        {
            var id = "pre-" + DateTime.UtcNow.Ticks;
            Backups[id] = CurrentSave;
            return Task.FromResult(Success(BackupJson(id)));
        }

        public Task<LudusaviCommandResult> ListBackupsAsync(IEnumerable<string> games, CancellationToken token)
            => Task.FromResult(Success(BackupJson()));

        public Task<LudusaviCommandResult> RestoreAsync(string game, string backupId, bool preview, CancellationToken token)
        {
            RestoreCalls.Add(backupId);
            if (preview)
            {
                var postValidation = string.Equals(CurrentSave, Backups.TryGetValue(backupId, out var expected) ? expected : backupId, StringComparison.Ordinal);
                return Task.FromResult(postValidation && FailPostValidationFor.Contains(backupId)
                    ? Failure("post validation failed")
                    : Success(BackupJson(backupId)));
            }
            if (backupId.StartsWith("pre-", StringComparison.OrdinalIgnoreCase) && FailRollback)
                return Task.FromResult(Failure("rollback failed"));
            var value = Backups.TryGetValue(backupId, out var saved) ? saved : backupId;
            if (ThrowUnauthorizedFor.Contains(backupId))
                throw new UnauthorizedAccessException(LiveSavePath);
            if (ThrowReadOnlyFor.Contains(backupId))
                throw new IOException("The target file is read-only.");
            if (PartialFailRestoreFor.Contains(backupId))
            {
                SetCurrentSave("PARTIAL:" + value);
                return Task.FromResult(Failure("restore partially failed"));
            }
            if (ThrowAfterWriteFor.Contains(backupId))
            {
                SetCurrentSave("PARTIAL:" + value);
                throw new IOException("write failed after partial mutation");
            }
            if (FailRestoreFor.Contains(backupId)) return Task.FromResult(Failure("restore failed"));
            SetCurrentSave(value);
            return Task.FromResult(Success(BackupJson(backupId)));
        }

        public Task<LudusaviCommandResult> RestoreFromPathAsync(string backupPath, string game, string backupId, bool preview, CancellationToken token)
            => RestoreAsync(game, backupId, preview, token);

        public Task<LudusaviCommandResult> EditBackupAsync(string game, string backupId, string? comment, bool? locked, CancellationToken token)
        {
            EditedBackups.Add((backupId, locked));
            return Task.FromResult(Success(BackupJson(backupId)));
        }

        private string BackupJson(params string[] additional)
        {
            var ids = Backups.Keys.Concat(additional).Distinct(StringComparer.OrdinalIgnoreCase);
            var backups = ids.Select(id => new { name = id, when = DateTime.UtcNow.ToString("O"), locked = false, comment = "" });
            return JsonSerializer.Serialize(new { games = new Dictionary<string, object> { ["Test Game"] = new { backups } } });
        }

        private static LudusaviCommandResult Success(string json)
        {
            using var document = JsonDocument.Parse(json);
            return LudusaviCommandResult.SuccessResult(document.RootElement.Clone(), "", 0, json);
        }

        private static LudusaviCommandResult Failure(string message) => LudusaviCommandResult.Failure("FAKE_FAILURE", message);

        public void Dispose()
        {
            var testRoot = Directory.GetParent(LiveDirectory)?.FullName;
            if (!string.IsNullOrWhiteSpace(testRoot) && Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }
}
