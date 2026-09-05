using Microsoft.Data.Sqlite;

namespace GameSaveCenter.Worker.Persistence;

internal enum IpcRequestState
{
    InProgress,
    Completed,
    Interrupted
}

internal sealed class IpcRequestClaim
{
    public IpcRequestState State { get; init; }
    public string ResponseJson { get; init; } = string.Empty;
    public bool IsOwner { get; init; }
}

public sealed partial class SqliteStateStore
{
    private const int IpcRequestRetentionDays = 7;

    /// <summary>
    /// Claims one destructive IPC request. A retry with the same request ID can return
    /// the original envelope, but can never execute an in-flight request twice.
    /// </summary>
    internal async Task<IpcRequestClaim> ClaimIpcRequestAsync(string requestId, string type, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(requestId)) throw new ArgumentException("IPC request id is required.", nameof(requestId));

        await _writeGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await using var connection = Open();
            await connection.OpenAsync(token).ConfigureAwait(false);
            await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(token).ConfigureAwait(false);

            var insert = connection.CreateCommand();
            insert.Transaction = transaction;
            insert.CommandText = @"
INSERT OR IGNORE INTO ipc_request_ledger(request_id,type,state,response_json,created_utc,updated_utc)
VALUES($id,$type,$state,NULL,$utc,$utc);";
            insert.Parameters.AddWithValue("$id", requestId);
            insert.Parameters.AddWithValue("$type", type ?? string.Empty);
            insert.Parameters.AddWithValue("$state", (int)IpcRequestState.InProgress);
            insert.Parameters.AddWithValue("$utc", DateTime.UtcNow.ToString("O"));
            var inserted = await insert.ExecuteNonQueryAsync(token).ConfigureAwait(false);

            var select = connection.CreateCommand();
            select.Transaction = transaction;
            select.CommandText = "SELECT state,response_json FROM ipc_request_ledger WHERE request_id=$id;";
            select.Parameters.AddWithValue("$id", requestId);
            IpcRequestState state;
            string responseJson;
            await using (var reader = await select.ExecuteReaderAsync(token).ConfigureAwait(false))
            {
                if (!await reader.ReadAsync(token).ConfigureAwait(false))
                    throw new InvalidOperationException("IPC request ledger claim disappeared before it could be read.");
                state = (IpcRequestState)reader.GetInt32(0);
                responseJson = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            }

            var claim = new IpcRequestClaim
            {
                State = state,
                ResponseJson = responseJson,
                IsOwner = inserted > 0
            };
            await transaction.CommitAsync(token).ConfigureAwait(false);
            return claim;
        }
        finally
        {
            _writeGate.Release();
        }
    }

    internal async Task CompleteIpcRequestAsync(string requestId, string responseJson, CancellationToken token)
    {
        await _writeGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await using var connection = Open();
            await connection.OpenAsync(token).ConfigureAwait(false);
            var command = connection.CreateCommand();
            command.CommandText = @"
UPDATE ipc_request_ledger
SET state=$state,response_json=$response,updated_utc=$utc
WHERE request_id=$id;";
            command.Parameters.AddWithValue("$state", (int)IpcRequestState.Completed);
            command.Parameters.AddWithValue("$response", responseJson ?? string.Empty);
            command.Parameters.AddWithValue("$utc", DateTime.UtcNow.ToString("O"));
            command.Parameters.AddWithValue("$id", requestId);
            await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        }
        finally
        {
            _writeGate.Release();
        }
    }

    /// <summary>Marks requests from a previous Worker process as non-replayable and bounds ledger growth.</summary>
    internal async Task RecoverIpcRequestLedgerAsync(CancellationToken token)
    {
        await _writeGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await using var connection = Open();
            await connection.OpenAsync(token).ConfigureAwait(false);
            var command = connection.CreateCommand();
            command.CommandText = @"
UPDATE ipc_request_ledger
SET state=$interrupted,updated_utc=$utc
WHERE state=$inProgress;
DELETE FROM ipc_request_ledger WHERE updated_utc < $cutoff;";
            command.Parameters.AddWithValue("$interrupted", (int)IpcRequestState.Interrupted);
            command.Parameters.AddWithValue("$inProgress", (int)IpcRequestState.InProgress);
            command.Parameters.AddWithValue("$utc", DateTime.UtcNow.ToString("O"));
            command.Parameters.AddWithValue("$cutoff", DateTime.UtcNow.AddDays(-IpcRequestRetentionDays).ToString("O"));
            await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        }
        finally
        {
            _writeGate.Release();
        }
    }
}
