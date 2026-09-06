using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class IpcRequestLedgerTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly WorkerOptions options;
    private readonly SqliteStateStore store;

    public IpcRequestLedgerTests()
    {
        options = new WorkerOptions
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
        var first = await store.ClaimIpcRequestAsync("request-a", MessageTypes.BackupGame, ProtocolConstants.ProtocolVersion,
            "{\"gameId\":\"game-a\",\"confirmed\":true}", CancellationToken.None);
        var duplicate = await store.ClaimIpcRequestAsync("request-a", MessageTypes.BackupGame, ProtocolConstants.ProtocolVersion,
            "{\"confirmed\":true,\"gameId\":\"game-a\"}", CancellationToken.None);

        Assert.True(first.IsOwner);
        Assert.False(first.IsConflict);
        Assert.False(duplicate.IsOwner);
        Assert.False(duplicate.IsConflict);
        Assert.Equal(IpcRequestState.InProgress, duplicate.State);

        await store.CompleteIpcRequestAsync("request-a", "{\"RequestId\":\"request-a\",\"Success\":true}", CancellationToken.None);
        var completed = await store.ClaimIpcRequestAsync("request-a", MessageTypes.BackupGame, ProtocolConstants.ProtocolVersion,
            "{\"gameId\":\"game-a\",\"confirmed\":true}", CancellationToken.None);

        Assert.False(completed.IsOwner);
        Assert.Equal(IpcRequestState.Completed, completed.State);
        Assert.Contains("request-a", completed.ResponseJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SameRequestIdWithDifferentTypePayloadOrProtocolIsRejectedAsConflict()
    {
        var first = await store.ClaimIpcRequestAsync("request-conflict", MessageTypes.BackupGame, ProtocolConstants.ProtocolVersion,
            "{\"gameId\":\"game-a\"}", CancellationToken.None);
        Assert.True(first.IsOwner);

        var differentPayload = await store.ClaimIpcRequestAsync("request-conflict", MessageTypes.BackupGame, ProtocolConstants.ProtocolVersion,
            "{\"gameId\":\"game-b\"}", CancellationToken.None);
        var differentType = await store.ClaimIpcRequestAsync("request-conflict", MessageTypes.RestoreExecute, ProtocolConstants.ProtocolVersion,
            "{\"gameId\":\"game-a\"}", CancellationToken.None);
        var differentProtocol = await store.ClaimIpcRequestAsync("request-conflict", MessageTypes.BackupGame, ProtocolConstants.ProtocolVersion + 1,
            "{\"gameId\":\"game-a\"}", CancellationToken.None);

        Assert.All(new[] { differentPayload, differentType, differentProtocol }, claim =>
        {
            Assert.False(claim.IsOwner);
            Assert.True(claim.IsConflict);
            Assert.Equal(IpcRequestState.InProgress, claim.State);
        });
    }

    [Fact]
    public async Task LegacyLedgerEntryWithoutPayloadFingerprintCannotBeReplayed()
    {
        await ExecuteSqlAsync("INSERT INTO ipc_request_ledger(request_id,type,state,response_json,created_utc,updated_utc) VALUES('legacy-request', 'BackupGame', 1, '{\"RequestId\":\"legacy-request\"}', $utc, $utc);", DateTime.UtcNow.ToString("O"));

        var claim = await store.ClaimIpcRequestAsync("legacy-request", MessageTypes.BackupGame, ProtocolConstants.ProtocolVersion,
            "{\"gameId\":\"game-a\"}", CancellationToken.None);

        Assert.False(claim.IsOwner);
        Assert.True(claim.IsConflict);
        Assert.Equal(IpcRequestState.Completed, claim.State);
        Assert.Contains("legacy-request", claim.ResponseJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LedgerMaintenanceExpiresCompletedRowsBeforeInterruptedEvidence()
    {
        var staleUtc = DateTime.UtcNow.AddDays(-8).ToString("O");
        await ExecuteSqlAsync(@"INSERT INTO ipc_request_ledger(request_id,type,state,response_json,created_utc,updated_utc)
VALUES('stale-completed', 'BackupGame', 1, '{}', $utc, $utc);
INSERT INTO ipc_request_ledger(request_id,type,state,response_json,created_utc,updated_utc)
VALUES('recent-interrupted', 'BackupGame', 2, NULL, $utc, $utc);", staleUtc);

        await store.MaintainIpcRequestLedgerAsync(CancellationToken.None);

        Assert.Equal(0, await CountRowsAsync("stale-completed"));
        Assert.Equal(1, await CountRowsAsync("recent-interrupted"));
    }

    [Fact]
    public async Task WorkerRestartMarksInFlightWriteAsInterruptedInsteadOfReplayingIt()
    {
        var first = await store.ClaimIpcRequestAsync("request-restart", MessageTypes.RestoreExecute, ProtocolConstants.ProtocolVersion,
            "{\"gameId\":\"game-a\"}", CancellationToken.None);
        Assert.True(first.IsOwner);

        await store.RecoverIpcRequestLedgerAsync(CancellationToken.None);
        var recovered = await store.ClaimIpcRequestAsync("request-restart", MessageTypes.RestoreExecute, ProtocolConstants.ProtocolVersion,
            "{\"gameId\":\"game-a\"}", CancellationToken.None);

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
        SqliteConnection.ClearAllPools();
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
    }

    private async Task ExecuteSqlAsync(string sql, string utc)
    {
        await using var connection = new SqliteConnection($"Data Source={options.DatabasePath};Cache=Shared;Foreign Keys=True");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$utc", utc);
        await command.ExecuteNonQueryAsync();
    }

    private async Task<long> CountRowsAsync(string requestId)
    {
        await using var connection = new SqliteConnection($"Data Source={options.DatabasePath};Cache=Shared;Foreign Keys=True");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM ipc_request_ledger WHERE request_id=$id;";
        command.Parameters.AddWithValue("$id", requestId);
        return Convert.ToInt64(await command.ExecuteScalarAsync());
    }
}
