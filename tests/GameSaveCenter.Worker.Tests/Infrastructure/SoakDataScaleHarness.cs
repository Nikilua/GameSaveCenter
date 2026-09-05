using System.Diagnostics;
using System.Text.Json;
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
/// Data-scale soak harness. The default test run uses a reduced profile so CI stays fast;
/// GSC_SOAK_DATA_SCALE=1 runs the full 2000/20000/10000/5000/500 profile and
/// GSC_SOAK_DATA_SCALE=2 runs the stress 10000/20000/10000/50000/500 profile.
/// </summary>
public sealed class SoakDataScaleHarness : IDisposable
{
    private readonly string root;
    private readonly WorkerOptions options;
    private readonly SqliteStateStore store;
    private readonly TaskEventBroadcaster broadcaster;
    private readonly GameOperationLock operationLock;
    private readonly List<string> errors = new();

    public SoakDataScaleHarness()
    {
        root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.SoakData", Guid.NewGuid().ToString("N"));
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
        broadcaster = new TaskEventBroadcaster();
        operationLock = new GameOperationLock();
    }

    public int SubscriberResidue { get; private set; }
    public int TempResidue { get; private set; }
    public string DataScaleProfile { get; private set; } = "reduced";
    public int GameCount { get; private set; }
    public int BackupCount { get; private set; }
    public int TaskCount { get; private set; }
    public int MediaCount { get; private set; }
    public int ToolCount { get; private set; }
    public long SeedDurationMilliseconds { get; private set; }
    public long SimulationDurationMilliseconds { get; private set; }
    public bool BoundedGrowth { get; private set; }
    public string GrowthSummary { get; private set; } = string.Empty;
    public IReadOnlyList<string> Errors => errors;

    public async Task RunAsync(bool fullScale, CancellationToken token)
        => await RunAsync(fullScale ? "full" : "reduced", token).ConfigureAwait(false);

    public async Task RunAsync(string profile, CancellationToken token)
    {
        // Keep the default developer/install profile bounded even on slow disks. The
        // full and stress profiles are explicit so a normal test run never creates a
        // large fixture unexpectedly.
        var scale = profile.Trim().ToLowerInvariant() switch
        {
            "1" or "full" => (Name: "full", Games: 2000, BackupsPerGame: 10, Tasks: 10000, Media: 5000, Tools: 500),
            "2" or "stress" => (Name: "stress", Games: 10000, BackupsPerGame: 2, Tasks: 10000, Media: 50000, Tools: 500),
            _ => (Name: "reduced", Games: 40, BackupsPerGame: 3, Tasks: 200, Media: 600, Tools: 20)
        };
        DataScaleProfile = scale.Name;
        GameCount = scale.Games;
        BackupCount = scale.Games * scale.BackupsPerGame;
        TaskCount = scale.Tasks;
        MediaCount = scale.Media;
        ToolCount = scale.Tools;

        var process = Process.GetCurrentProcess();
        var beforeManaged = GC.GetTotalMemory(false);
        var beforeHandles = process.HandleCount;
        var beforeThreads = process.Threads.Count;

        var seedTimer = Stopwatch.StartNew();
        await SeedGamesAsync(scale.Games, token).ConfigureAwait(false);
        await SeedBackupsAsync(scale.Games, scale.BackupsPerGame, token).ConfigureAwait(false);
        await SeedTasksAsync(scale.Tasks, token).ConfigureAwait(false);
        await SeedMediaAsync(scale.Media, token).ConfigureAwait(false);
        await SeedToolsAsync(scale.Tools, token).ConfigureAwait(false);
        seedTimer.Stop();
        SeedDurationMilliseconds = seedTimer.ElapsedMilliseconds;

        var simulationTimer = Stopwatch.StartNew();
        for (var cycle = 0; cycle < 20; cycle++)
        {
            await SimulateReadsAsync(cycle, token).ConfigureAwait(false);
            await SimulateEventFanOutAsync(cycle, token).ConfigureAwait(false);
            await SimulateAtomicWriteAsync(cycle, token).ConfigureAwait(false);
            await SimulateOperationLockAsync(cycle, token).ConfigureAwait(false);
        }
        simulationTimer.Stop();
        SimulationDurationMilliseconds = simulationTimer.ElapsedMilliseconds;

        GC.Collect();
        GC.WaitForPendingFinalizers();
        var afterManaged = GC.GetTotalMemory(true);
        var afterHandles = process.HandleCount;
        var afterThreads = process.Threads.Count;
        var managedGrowth = Math.Max(0, afterManaged - beforeManaged);
        var handleGrowth = Math.Max(0, afterHandles - beforeHandles);
        var threadGrowth = Math.Max(0, afterThreads - beforeThreads);
        BoundedGrowth = managedGrowth <= 256L * 1024 * 1024
            && handleGrowth <= 256
            && threadGrowth <= 32;
        GrowthSummary = $"managedGrowth={managedGrowth / 1024d / 1024d:0.#} MiB, handles+{handleGrowth}, threads+{threadGrowth}";
        if (!BoundedGrowth) errors.Add("bounded growth assertion failed: " + GrowthSummary);
    }

    private async Task SeedGamesAsync(int count, CancellationToken token)
    {
        const int chunk = 100;
        for (var start = 0; start < count; start += chunk)
        {
            token.ThrowIfCancellationRequested();
            var games = Enumerable.Range(start, Math.Min(chunk, count - start)).Select(i => new GameDescriptorDto
            {
                PlayniteId = "g-" + i,
                Name = "Game " + i,
                IsInstalled = true
            }).ToList();
            await store.UpsertGamesAsync(games, token).ConfigureAwait(false);
        }
    }

    private async Task SeedBackupsAsync(int games, int perGame, CancellationToken token)
    {
        for (var game = 0; game < games; game++)
        {
            token.ThrowIfCancellationRequested();
            for (var version = 0; version < perGame; version++)
            {
                var playniteId = "g-" + game;
                await store.AddBackupVersionAsync(new BackupVersionDto
                {
                    PlayniteId = playniteId,
                    BackupId = "b-" + game + "-" + version,
                    LudusaviName = "Game " + game,
                    CreatedUtc = DateTime.UtcNow.AddMinutes(-version),
                    TotalBytes = version * 1024 + 1,
                    FileCount = version + 1,
                    ArchivePath = Path.Combine(options.LudusaviBackupDirectory, "Game " + game, version + ".zip")
                }, "[]", token).ConfigureAwait(false);
            }
        }
    }

    private async Task SeedTasksAsync(int count, CancellationToken token)
    {
        for (var i = 0; i < count; i++)
        {
            token.ThrowIfCancellationRequested();
            await store.AddOrUpdateTaskAsync(new TaskStatusDto
            {
                TaskId = "task-" + i,
                TaskType = i % 4 == 0 ? "Backup" : i % 4 == 1 ? "MediaSync" : i % 4 == 2 ? "CloudUpload" : "Validation",
                GameId = "g-" + (i % 200),
                GameName = "Game " + (i % 200),
                State = i % 5 == 0 ? TaskState.Succeeded : TaskState.Failed,
                ProgressPercent = 100,
                Message = "soak",
                CreatedUtc = DateTime.UtcNow.AddMinutes(-i),
                FinishedUtc = DateTime.UtcNow.AddMinutes(-i + 1)
            }, token).ConfigureAwait(false);
        }
    }

    private async Task SeedMediaAsync(int count, CancellationToken token)
    {
        for (var i = 0; i < count; i++)
        {
            token.ThrowIfCancellationRequested();
            await store.AddMediaAsync(new MediaItemDto
            {
                MediaId = "m-" + i,
                PlayniteId = "g-" + (i % 200),
                Kind = i % 3 == 0 ? MediaKind.VideoClip : MediaKind.Screenshot,
                Source = MediaSourceKind.Steam,
                ArchivePath = Path.Combine(options.MediaArchiveDirectory, i + ".png"),
                OriginalPath = Path.Combine(options.MediaArchiveDirectory, i + ".png"),
                CapturedUtc = DateTime.UtcNow.AddMinutes(-i),
                SizeBytes = i * 7,
                Sha256 = i.ToString("x64"),
                IsFavorite = i % 10 == 0,
                CloudState = "Pending"
            }, token).ConfigureAwait(false);
        }
    }

    private async Task SeedToolsAsync(int count, CancellationToken token)
    {
        for (var i = 0; i < count; i++)
        {
            token.ThrowIfCancellationRequested();
            var now = DateTime.UtcNow;
            await store.UpsertGameToolAsync(new GameToolDto
            {
                ToolId = "tool-" + i,
                PlayniteId = "g-" + (i % 200),
                ToolType = GameToolType.CustomExecutable,
                SourceType = GameToolSourceType.Manual,
                DisplayName = "Tool " + i,
                Enabled = true,
                AutoStart = false,
                ActiveVersionId = "v-" + i,
                CreatedUtc = now,
                UpdatedUtc = now
            }, new GameToolVersionDto
            {
                VersionId = "v-" + i,
                ToolId = "tool-" + i,
                VersionName = "1.0",
                EntryPath = Path.Combine(options.GameToolsDirectory, "tool-" + i + ".exe"),
                CreatedUtc = now
            }, token).ConfigureAwait(false);
        }
    }

    private async Task SimulateReadsAsync(int cycle, CancellationToken token)
    {
        _ = await store.GetRecentTasksAsync(200, token).ConfigureAwait(false);
        _ = await store.GetGamesAsync(token).ConfigureAwait(false);
        _ = await store.GetCountsAsync(token).ConfigureAwait(false);
        _ = await store.GetHealthStateCountsAsync(token).ConfigureAwait(false);
        _ = await store.GetBackupVersionsAsync("g-" + (cycle % 200), token).ConfigureAwait(false);
        _ = await store.GetMediaSummaryAsync("g-" + (cycle % 200), token).ConfigureAwait(false);
        _ = await store.GetGameToolsAsync("g-" + (cycle % 200), token).ConfigureAwait(false);
        _ = await store.GetOpenFindingsAsync(20, token).ConfigureAwait(false);
        await store.ProbeReadWriteAsync(token).ConfigureAwait(false);
    }

    private async Task SimulateEventFanOutAsync(int cycle, CancellationToken token)
    {
        using (var subscription = broadcaster.Subscribe())
        {
            for (var i = 0; i < 16; i++)
            {
                broadcaster.Publish(new TaskChangeEventDto
                {
                    Sequence = cycle * 16 + i,
                    Task = new TaskStatusDto { TaskId = $"soak-{cycle}-{i}", TaskType = "Soak" }
                });
            }
        }
        SubscriberResidue += broadcaster.SubscriberCount;
        await Task.Yield();
    }

    private async Task SimulateAtomicWriteAsync(int cycle, CancellationToken token)
    {
        var path = Path.Combine(options.DataDirectory, "soak-" + cycle + ".json");
        await AtomicFileWriter.WriteAllTextAsync(path, $"{{cycle:{cycle}}}", token).ConfigureAwait(false);
        await AtomicFileWriter.WriteAllTextAsync(path, $"{{cycle:{cycle + 1}}}", token).ConfigureAwait(false);
        TempResidue += Directory.EnumerateFiles(options.DataDirectory, "*.tmp").Count()
            + Directory.EnumerateFiles(options.DataDirectory, "*.replace").Count();
    }

    private async Task SimulateOperationLockAsync(int cycle, CancellationToken token)
    {
        using var lease = await operationLock.AcquireAsync("g-" + (cycle % 200), GameOperationKind.Backup, TimeSpan.FromSeconds(2), token).ConfigureAwait(false);
        if (lease == null) errors.Add("operation lock could not be acquired during soak");
        await Task.Yield();
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
    }

    public void WriteReport(string directory)
    {
        Directory.CreateDirectory(directory);
        var report = new
        {
            profile = DataScaleProfile,
            games = GameCount,
            backups = BackupCount,
            tasks = TaskCount,
            media = MediaCount,
            tools = ToolCount,
            seedDurationMilliseconds = SeedDurationMilliseconds,
            simulationDurationMilliseconds = SimulationDurationMilliseconds,
            boundedGrowth = BoundedGrowth,
            subscriberResidue = SubscriberResidue,
            tempResidue = TempResidue,
            growthSummary = GrowthSummary,
            errors
        };
        File.WriteAllText(
            Path.Combine(directory, "worker-scale.json"),
            JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
    }
}
