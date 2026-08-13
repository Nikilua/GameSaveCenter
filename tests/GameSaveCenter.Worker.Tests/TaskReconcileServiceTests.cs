using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class TaskReconcileServiceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly SqliteStateStore store;

    public TaskReconcileServiceTests()
    {
        var options = new WorkerOptions
        {
            DataDirectory = Path.Combine(root, "Data"),
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            MediaArchiveDirectory = Path.Combine(root, "Media")
        };
        store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task ReconcileMarksInterruptedTasksAndIsIdempotent()
    {
        var now = DateTime.UtcNow;
        await store.AddOrUpdateTaskAsync(new TaskStatusDto
        {
            TaskId = "queued",
            TaskType = "Backup",
            State = TaskState.Queued,
            CreatedUtc = now
        }, CancellationToken.None);
        await store.AddOrUpdateTaskAsync(new TaskStatusDto
        {
            TaskId = "running",
            TaskType = "CloudUpload",
            State = TaskState.Running,
            CreatedUtc = now,
            StartedUtc = now
        }, CancellationToken.None);
        await store.AddOrUpdateTaskAsync(new TaskStatusDto
        {
            TaskId = "done",
            TaskType = "Backup",
            State = TaskState.Succeeded,
            CreatedUtc = now,
            FinishedUtc = now
        }, CancellationToken.None);

        var service = new TaskReconcileService(store, NullLogger<TaskReconcileService>.Instance);
        var first = await service.ReconcileAsync(CancellationToken.None);
        var second = await service.ReconcileAsync(CancellationToken.None);

        Assert.Equal(2, first.InterruptedTaskCount);
        Assert.Equal(0, second.InterruptedTaskCount);
        var tasks = await store.GetRecentTasksAsync(10, CancellationToken.None);
        Assert.Equal(TaskState.Failed, tasks.Single(x => x.TaskId == "queued").State);
        Assert.Equal("WORKER_RESTARTED", tasks.Single(x => x.TaskId == "queued").ErrorCode);
        Assert.Equal(TaskState.Failed, tasks.Single(x => x.TaskId == "running").State);
        Assert.Equal("WORKER_RESTARTED", tasks.Single(x => x.TaskId == "running").ErrorCode);
        Assert.Equal(TaskState.Succeeded, tasks.Single(x => x.TaskId == "done").State);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
    }
}
