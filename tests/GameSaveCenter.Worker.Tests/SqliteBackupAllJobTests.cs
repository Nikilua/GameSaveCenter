using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class SqliteBackupAllJobTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly SqliteStateStore store;

    public SqliteBackupAllJobTests()
    {
        store = new SqliteStateStore(new WorkerOptions
        {
            DataDirectory = root,
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            MediaArchiveDirectory = Path.Combine(root, "Media")
        }, NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task BackupAllJobRoundTripsQueuedAndUpdatedState()
    {
        var created = DateTime.UtcNow;
        var job = new BackupAllJobRecord
        {
            JobId = "backup-all-1",
            RequestJson = "{\"force\":true}",
            CreatedUtc = created,
            WorkerSessionId = "worker-1",
            CompletedGameIdsJson = "[\"game-1\"]"
        };

        await store.CreateBackupAllJobAsync(job, CancellationToken.None);

        var queued = await store.GetActiveBackupAllJobAsync(CancellationToken.None);
        Assert.NotNull(queued);
        Assert.Equal(job.JobId, queued!.JobId);
        Assert.Equal(TaskState.Queued, queued.State);
        Assert.Equal("整库备份", queued.ToTaskStatus().TaskTypeDisplay);

        queued.State = TaskState.Running;
        queued.ProgressPercent = 42;
        queued.Message = "正在处理整库备份：2/4";
        queued.CurrentGameId = "game-2";
        queued.StartedUtc = created.AddSeconds(1);
        queued.CompletedGameIdsJson = "[\"game-1\",\"game-2\"]";
        await store.UpdateBackupAllJobAsync(queued, CancellationToken.None);

        var running = await store.GetActiveBackupAllJobAsync(CancellationToken.None);
        Assert.NotNull(running);
        Assert.Equal(TaskState.Running, running!.State);
        Assert.Equal(42, running.ProgressPercent);
        Assert.Equal("game-2", running.CurrentGameId);
        Assert.Equal("[\"game-1\",\"game-2\"]", running.CompletedGameIdsJson);
    }

    [Fact]
    public async Task CompletedBackupAllJobIsNotConsideredActive()
    {
        var job = new BackupAllJobRecord
        {
            JobId = "backup-all-completed",
            RequestJson = "{}",
            CreatedUtc = DateTime.UtcNow,
            WorkerSessionId = "worker-1",
            State = TaskState.Succeeded,
            ProgressPercent = 100,
            Message = "整库备份已完成",
            FinishedUtc = DateTime.UtcNow
        };

        await store.CreateBackupAllJobAsync(job, CancellationToken.None);

        Assert.Null(await store.GetActiveBackupAllJobAsync(CancellationToken.None));
    }

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        if (Directory.Exists(root)) Directory.Delete(root, true);
    }
}
