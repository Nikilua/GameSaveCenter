using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class IpcRequestLedgerTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly SqliteStateStore store;

    public IpcRequestLedgerTests()
    {
        var options = new WorkerOptions
        {
            DataDirectory = Path.Combine(root, "Data"),
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            MediaArchiveDirectory = Path.Combine(root, "Media")
        };
        Directory.CreateDirectory(options.DataDirectory);
        store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task SameRequestIdIsClaimedOnceAndCompletedResponseCanBeReplayed()
    {
        var first = await store.ClaimIpcRequestAsync("request-a", MessageTypes.BackupGame, CancellationToken.None);
        var duplicate = await store.ClaimIpcRequestAsync("request-a", MessageTypes.BackupGame, CancellationToken.None);

        Assert.True(first.IsOwner);
        Assert.False(duplicate.IsOwner);
        Assert.Equal(IpcRequestState.InProgress, duplicate.State);

        await store.CompleteIpcRequestAsync("request-a", "{\"RequestId\":\"request-a\",\"Success\":true}", CancellationToken.None);
        var completed = await store.ClaimIpcRequestAsync("request-a", MessageTypes.BackupGame, CancellationToken.None);

        Assert.False(completed.IsOwner);
        Assert.Equal(IpcRequestState.Completed, completed.State);
        Assert.Contains("request-a", completed.ResponseJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WorkerRestartMarksInFlightWriteAsInterruptedInsteadOfReplayingIt()
    {
        var first = await store.ClaimIpcRequestAsync("request-restart", MessageTypes.RestoreExecute, CancellationToken.None);
        Assert.True(first.IsOwner);

        await store.RecoverIpcRequestLedgerAsync(CancellationToken.None);
        var recovered = await store.ClaimIpcRequestAsync("request-restart", MessageTypes.RestoreExecute, CancellationToken.None);

        Assert.False(recovered.IsOwner);
        Assert.Equal(IpcRequestState.Interrupted, recovered.State);
    }

    [Fact]
    public async Task TaskQueryCanFindTheTaskSubmittedByAnIpcRequest()
    {
        await store.AddOrUpdateTaskAsync(new TaskStatusDto
        {
            TaskId = "task-request-a",
            RequestId = "request-task-a",
            TaskType = "Backup",
            GameId = "game-a",
            GameName = "测试游戏",
            State = TaskState.Succeeded,
            ProgressPercent = 100,
            Message = "已完成",
            CreatedUtc = DateTime.UtcNow,
            FinishedUtc = DateTime.UtcNow
        }, CancellationToken.None);

        var page = await store.GetTaskPageAsync(new TaskQueryDto
        {
            RequestId = "request-task-a",
            Limit = 10
        }, CancellationToken.None);

        var task = Assert.Single(page.Items);
        Assert.Equal("task-request-a", task.TaskId);
        Assert.Equal("request-task-a", task.RequestId);
        Assert.Equal(1, page.TotalCount);
    }

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
    }
}
