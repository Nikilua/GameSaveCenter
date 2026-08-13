using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Ipc;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;

namespace GameSaveCenter.Worker.Tests.Infrastructure;

/// <summary>
/// Accelerated soak harness that repeatedly exercises the Worker surfaces most likely to
/// leak or degrade over long uptime: task coordination, event fan-out, per-game operation
/// locks, atomic file replacement and SQLite read/write probes. It runs only against a
/// temporary data directory and never touches real saves, media or user settings.
/// </summary>
public sealed class SoakStabilityHarness : IDisposable
{
    private const int ChangeFeedRetention = 500;
    private const int LockGameCount = 16;
    private readonly string root;
    private readonly string probeDirectory;
    private readonly WorkerOptions options;
    private readonly SqliteStateStore store;
    private readonly TaskEventBroadcaster broadcaster;
    private readonly TaskCoordinator coordinator;
    private readonly GameOperationLock operationLock;
    private readonly List<string> errors = new();

    public SoakStabilityHarness()
    {
        root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Soak", Guid.NewGuid().ToString("N"));
        options = new WorkerOptions
        {
            DataDirectory = Path.Combine(root, "Data"),
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            MediaArchiveDirectory = Path.Combine(root, "Media")
        };
        Directory.CreateDirectory(options.DataDirectory);
        Directory.CreateDirectory(options.LudusaviBackupDirectory);
        Directory.CreateDirectory(options.MediaArchiveDirectory);
        probeDirectory = Path.Combine(root, "Probe");
        Directory.CreateDirectory(probeDirectory);

        store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
        broadcaster = new TaskEventBroadcaster();
        coordinator = new TaskCoordinator(store, broadcaster, NullLogger<TaskCoordinator>.Instance);
        operationLock = new GameOperationLock();
    }

    public int CompletedCycles { get; private set; }
    public int TempFileResidue { get; private set; }
    public int SubscriberResidue { get; private set; }
    public int ChangeFeedCount { get; private set; }
    public int TrackedLockGames { get; private set; }
    public IReadOnlyList<string> Errors => errors;

    public async Task RunAsync(int cycles, CancellationToken token)
    {
        for (var cycle = 0; cycle < cycles && !token.IsCancellationRequested; cycle++)
        {
            await RunStepAsync("atomic-file-writer", () => RunAtomicWriteCycleAsync(cycle, token)).ConfigureAwait(false);
            if (errors.Count > 0) break;

            await RunStepAsync("game-operation-lock", () => RunOperationLockCycleAsync(cycle, token)).ConfigureAwait(false);
            if (errors.Count > 0) break;

            await RunStepAsync("task-coordinator", () => RunTaskCycleAsync(cycle, token)).ConfigureAwait(false);
            if (errors.Count > 0) break;

            await RunStepAsync("task-event-broadcaster", () => RunBroadcasterCycleAsync(token)).ConfigureAwait(false);
            if (errors.Count > 0) break;

            await RunStepAsync("sqlite-probe", () => RunSqliteProbeCycleAsync(cycle, token)).ConfigureAwait(false);
            if (errors.Count > 0) break;

            CompletedCycles++;
        }
    }

    private async Task RunStepAsync(string name, Func<Task> step)
    {
        try
        {
            await step().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            errors.Add($"{name}: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private async Task RunAtomicWriteCycleAsync(int cycle, CancellationToken token)
    {
        var settingsPath = Path.Combine(probeDirectory, "settings.json");
        var payload = $"{{\"cycle\":{cycle},\"utc\":\"{DateTime.UtcNow:O}\"}}";
        await AtomicFileWriter.WriteAllTextAsync(settingsPath, payload, token).ConfigureAwait(false);

        var archivePath = Path.Combine(probeDirectory, $"archive-{cycle}.json");
        await AtomicFileWriter.CopyAtomicallyAsync(settingsPath, archivePath, token).ConfigureAwait(false);
        var copied = await File.ReadAllTextAsync(archivePath, token).ConfigureAwait(false);
        if (!string.Equals(copied, payload, StringComparison.Ordinal))
            errors.Add($"atomic-file-writer: copied content mismatch at cycle {cycle}");

        var occupied = Path.Combine(probeDirectory, "occupied");
        Directory.CreateDirectory(occupied);
        try
        {
            await AtomicFileWriter.CopyAtomicallyAsync(settingsPath, occupied, token).ConfigureAwait(false);
            errors.Add($"atomic-file-writer: failed copy unexpectedly succeeded at cycle {cycle}");
        }
        catch (IOException)
        {
        }

        var residue = Directory.EnumerateFiles(probeDirectory, "*.tmp").Count()
            + Directory.EnumerateFiles(probeDirectory, "*.partial").Count();
        TempFileResidue += residue;
    }

    private async Task RunOperationLockCycleAsync(int cycle, CancellationToken token)
    {
        for (var game = 0; game < LockGameCount; game++)
        {
            using var lease = await operationLock.AcquireAsync($"soak-game-{game}", TimeSpan.FromSeconds(2), token).ConfigureAwait(false);
            if (lease == null)
                errors.Add($"game-operation-lock: could not acquire game {game} at cycle {cycle}");
            else
                await Task.Yield();
        }

        var held = await operationLock.AcquireAsync("soak-contended", TimeSpan.FromSeconds(2), token).ConfigureAwait(false);
        if (held == null)
        {
            errors.Add($"game-operation-lock: could not hold contended lock at cycle {cycle}");
            return;
        }
        using (held)
        {
            var denied = await operationLock.AcquireAsync("soak-contended", TimeSpan.FromMilliseconds(50), token).ConfigureAwait(false);
            if (denied != null)
            {
                denied.Dispose();
                errors.Add($"game-operation-lock: contended lock was not serialized at cycle {cycle}");
            }
        }
        using var afterRelease = await operationLock.AcquireAsync("soak-contended", TimeSpan.FromSeconds(2), token).ConfigureAwait(false);
        if (afterRelease == null)
            errors.Add($"game-operation-lock: contended lock was not released at cycle {cycle}");

        TrackedLockGames = operationLock.TrackedGameCount;
    }

    private async Task RunTaskCycleAsync(int cycle, CancellationToken token)
    {
        var gameId = $"soak-game-{cycle % LockGameCount}";
        var task = await coordinator.RunAsync(
            "SoakBackup",
            gameId,
            $"Soak Game {cycle % LockGameCount}",
            async (progress, _) =>
            {
                await progress.ReportAsync(cycle % 100, $"soak cycle {cycle}").ConfigureAwait(false);
                await Task.Yield();
            },
            token).ConfigureAwait(false);

        if (task.State != TaskState.Succeeded)
            errors.Add($"task-coordinator: task ended as {task.State} at cycle {cycle}");
        if (coordinator.Cancel(task.TaskId))
            errors.Add($"task-coordinator: completed task was still cancelable at cycle {cycle}");

        var feed = coordinator.GetChanges(long.MinValue, 5000);
        ChangeFeedCount = feed.Changes.Count;
        if (feed.Changes.Count > ChangeFeedRetention)
            errors.Add($"task-coordinator: change feed exceeded {ChangeFeedRetention} at cycle {cycle}");
    }

    private async Task RunBroadcasterCycleAsync(CancellationToken token)
    {
        using (var subscription = broadcaster.Subscribe())
        {
            for (var i = 0; i < 8; i++)
            {
                broadcaster.Publish(new TaskChangeEventDto
                {
                    Sequence = i,
                    Task = new TaskStatusDto
                    {
                        TaskId = $"soak-event-{i}",
                        TaskType = "Soak",
                        State = TaskState.Running,
                        ProgressPercent = i
                    }
                });
            }
            if (broadcaster.SubscriberCount != 1)
                errors.Add("task-event-broadcaster: subscriber count did not match active subscription");
            await Task.Yield();
        }

        SubscriberResidue += broadcaster.SubscriberCount;
    }

    private async Task RunSqliteProbeCycleAsync(int cycle, CancellationToken token)
    {
        await store.ProbeReadWriteAsync(token).ConfigureAwait(false);
        await store.AppendAuditAsync("Soak", $"cycle {cycle}", "{}", token).ConfigureAwait(false);
        var recent = await store.GetRecentTasksAsync(200, token).ConfigureAwait(false);
        if (recent.Count == 0)
            errors.Add($"sqlite-probe: no recent tasks visible at cycle {cycle}");
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
    }
}
