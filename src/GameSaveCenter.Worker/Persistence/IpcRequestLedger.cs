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
    public bool IsConflict { get; init; }
}

public sealed partial class SqliteStateStore
{
    private const int IpcRequestCompletedRetentionDays = 7;
    private const int IpcRequestInterruptedRetentionDays = 30;
    private const int IpcRequestCleanupBatchSize = 256;

    /// <summary>
    /// Claims one destructive IPC request. A retry with the same request ID can return
    /// the original envelope only when type, protocol version and canonical payload all match.
    /// </summary>
    internal async Task<IpcRequestClaim> ClaimIpcRequestAsync(string requestId, string type, int protocolVersion,
        string payloadJson, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(requestId)) throw new ArgumentException("IPC request id is required.", nameof(requestId));

        await _writeGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await using var connection = Open();
            await connection.OpenAsync(token).ConfigureAwait(false);
            await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(token).ConfigureAwait(false);

            var payloadHash = IpcPayloadFingerprint.Compute(payloadJson);
            var insert = connection.CreateCommand();
            insert.Transaction = transaction;
            insert.CommandText = @"
INSERT OR IGNORE INTO ipc_request_ledger(request_id,type,protocol_version,payload_hash,state,response_json,created_utc,updated_utc)
VALUES($id,$type,$protocol,$payload,$state,NULL,$utc,$utc);";
            insert.Parameters.AddWithValue("$id", requestId);
            insert.Parameters.AddWithValue("$type", type ?? string.Empty);
            insert.Parameters.AddWithValue("$protocol", protocolVersion);
            insert.Parameters.AddWithValue("$payload", payloadHash);
            insert.Parameters.AddWithValue("$state", (int)IpcRequestState.InProgress);
            insert.Parameters.AddWithValue("$utc", DateTime.UtcNow.ToString("O"));
            var inserted = await insert.ExecuteNonQueryAsync(token).ConfigureAwait(false);

            var select = connection.CreateCommand();
            select.Transaction = transaction;
            select.CommandText = "SELECT type,protocol_version,payload_hash,state,response_json FROM ipc_request_ledger WHERE request_id=$id;";
            select.Parameters.AddWithValue("$id", requestId);
            IpcRequestState state;
            string responseJson;
            string existingType;
            int existingProtocolVersion;
            string existingPayloadHash;
            await using (var reader = await select.ExecuteReaderAsync(token).ConfigureAwait(false))
            {
                if (!await reader.ReadAsync(token).ConfigureAwait(false))
                    throw new InvalidOperationException("IPC request ledger claim disappeared before it could be read.");
                existingType = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                existingProtocolVersion = reader.GetInt32(1);
                existingPayloadHash = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                state = (IpcRequestState)reader.GetInt32(3);
                responseJson = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
            }

            var claim = new IpcRequestClaim
            {
                State = state,
                ResponseJson = responseJson,
                IsOwner = inserted > 0,
                IsConflict = inserted == 0 && (string.IsNullOrWhiteSpace(existingPayloadHash)
                    || !string.Equals(existingType, type ?? string.Empty, StringComparison.Ordinal)
                    || existingProtocolVersion != protocolVersion
                    || !string.Equals(existingPayloadHash, payloadHash, StringComparison.OrdinalIgnoreCase))
            };
            await transaction.CommitAsync(token).ConfigureAwait(false);
            return claim;
        }
        finally
        {
            _writeGate.Release();
        }
    }

    /// <summary>
    /// Performs bounded retention maintenance without changing in-flight requests owned by
    /// the current Worker. Startup recovery is the only path that marks old in-flight rows
    /// as interrupted.
    /// </summary>
    internal async Task MaintainIpcRequestLedgerAsync(CancellationToken token)
    {
        await _writeGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await using var connection = Open();
            await connection.OpenAsync(token).ConfigureAwait(false);
            await CleanupIpcRequestLedgerAsync(connection, token).ConfigureAwait(false);
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
";
            command.Parameters.AddWithValue("$interrupted", (int)IpcRequestState.Interrupted);
            command.Parameters.AddWithValue("$inProgress", (int)IpcRequestState.InProgress);
            command.Parameters.AddWithValue("$utc", DateTime.UtcNow.ToString("O"));
            await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
            await CleanupIpcRequestLedgerAsync(connection, token).ConfigureAwait(false);
        }
        finally
        {
            _writeGate.Release();
        }
    }

    private static async Task CleanupIpcRequestLedgerAsync(SqliteConnection connection, CancellationToken token)
    {
        var command = connection.CreateCommand();
        command.CommandText = @"
DELETE FROM ipc_request_ledger
WHERE request_id IN (
    SELECT request_id FROM ipc_request_ledger
    WHERE (state=$completed AND updated_utc < $completedCutoff)
       OR (state=$interrupted AND updated_utc < $interruptedCutoff)
    ORDER BY updated_utc ASC
    LIMIT $limit
);";
        command.Parameters.AddWithValue("$completed", (int)IpcRequestState.Completed);
        command.Parameters.AddWithValue("$interrupted", (int)IpcRequestState.Interrupted);
        command.Parameters.AddWithValue("$completedCutoff", DateTime.UtcNow.AddDays(-IpcRequestCompletedRetentionDays).ToString("O"));
        command.Parameters.AddWithValue("$interruptedCutoff", DateTime.UtcNow.AddDays(-IpcRequestInterruptedRetentionDays).ToString("O"));
        command.Parameters.AddWithValue("$limit", IpcRequestCleanupBatchSize);
        await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
    }
}
