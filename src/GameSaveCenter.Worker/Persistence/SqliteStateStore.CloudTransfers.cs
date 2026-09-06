using GameSaveCenter.Contracts;
using Microsoft.Data.Sqlite;

namespace GameSaveCenter.Worker.Persistence;

/// <summary>Durable state for both backup and media cloud-copy pipelines.</summary>
public sealed partial class SqliteStateStore
{
    public Task UpsertCloudTransferAsync(CloudTransferQueueEntry entry, CancellationToken token)
        => ExecuteAsync(@"
INSERT INTO cloud_transfer_queue(transfer_key,transfer_kind,playnite_id,state,operation_kind,operation_id,prior_state,prior_operation_kind,prior_operation_id,prior_next_attempt_utc,prior_last_attempt_utc,prior_error_code,prior_error,attempt_count,next_attempt_utc,last_attempt_utc,last_error_code,last_error,created_utc,updated_utc)
VALUES($key,$kind,$game,$state,$operation_kind,$operation_id,$prior_state,$prior_operation_kind,$prior_operation_id,$prior_next,$prior_last_attempt,$prior_error_code,$prior_error,$attempts,$next,$last_attempt,$error_code,$error,$created,$updated)
ON CONFLICT(transfer_key) DO UPDATE SET
transfer_kind=excluded.transfer_kind,playnite_id=excluded.playnite_id,state=excluded.state,
operation_kind=excluded.operation_kind,operation_id=excluded.operation_id,prior_state=excluded.prior_state,
prior_operation_kind=excluded.prior_operation_kind,prior_operation_id=excluded.prior_operation_id,
prior_next_attempt_utc=excluded.prior_next_attempt_utc,prior_last_attempt_utc=excluded.prior_last_attempt_utc,
prior_error_code=excluded.prior_error_code,prior_error=excluded.prior_error,
attempt_count=excluded.attempt_count,next_attempt_utc=excluded.next_attempt_utc,last_attempt_utc=excluded.last_attempt_utc,
last_error_code=excluded.last_error_code,last_error=excluded.last_error,updated_utc=excluded.updated_utc;",
            new Dictionary<string, object?>
            {
                ["$key"] = entry.TransferKey,
                ["$kind"] = entry.Kind.ToString(),
                ["$game"] = entry.PlayniteId,
                ["$state"] = entry.State,
                ["$operation_kind"] = entry.OperationKind.ToString(),
                ["$operation_id"] = entry.OperationId,
                ["$prior_state"] = entry.PriorState,
                ["$prior_operation_kind"] = entry.PriorOperationKind.ToString(),
                ["$prior_operation_id"] = entry.PriorOperationId,
                ["$prior_next"] = ToNullableUtc(entry.PriorNextAttemptUtc),
                ["$prior_last_attempt"] = ToNullableUtc(entry.PriorLastAttemptUtc),
                ["$prior_error_code"] = entry.PriorErrorCode,
                ["$prior_error"] = entry.PriorError,
                ["$attempts"] = Math.Max(0, entry.AttemptCount),
                ["$next"] = ToNullableUtc(entry.NextAttemptUtc),
                ["$last_attempt"] = ToNullableUtc(entry.LastAttemptUtc),
                ["$error_code"] = entry.LastErrorCode,
                ["$error"] = entry.LastError,
                ["$created"] = entry.CreatedUtc.ToUniversalTime().ToString("O"),
                ["$updated"] = entry.UpdatedUtc.ToUniversalTime().ToString("O")
            }, token);

    public async Task<bool> TryUpdateCloudTransferAsync(CloudTransferQueueEntry entry, string expectedOperationId, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(expectedOperationId)) return false;
        await _writeGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await using var connection = Open();
            await connection.OpenAsync(token).ConfigureAwait(false);
            var command = connection.CreateCommand();
            command.CommandText = @"
UPDATE cloud_transfer_queue
SET state=$state,operation_kind=$operation_kind,operation_id=$operation_id,prior_state=$prior_state,
    prior_operation_kind=$prior_operation_kind,prior_operation_id=$prior_operation_id,
    prior_next_attempt_utc=$prior_next,prior_last_attempt_utc=$prior_last_attempt,
    prior_error_code=$prior_error_code,prior_error=$prior_error,attempt_count=$attempts,
    next_attempt_utc=$next,last_attempt_utc=$last_attempt,last_error_code=$error_code,last_error=$error,updated_utc=$updated
WHERE transfer_key=$key AND operation_id=$expected_operation_id;";
            command.Parameters.AddWithValue("$state", entry.State);
            command.Parameters.AddWithValue("$operation_kind", entry.OperationKind.ToString());
            command.Parameters.AddWithValue("$operation_id", entry.OperationId);
            command.Parameters.AddWithValue("$prior_state", entry.PriorState);
            command.Parameters.AddWithValue("$prior_operation_kind", entry.PriorOperationKind.ToString());
            command.Parameters.AddWithValue("$prior_operation_id", entry.PriorOperationId);
            command.Parameters.AddWithValue("$prior_next", (object?)ToNullableUtc(entry.PriorNextAttemptUtc) ?? DBNull.Value);
            command.Parameters.AddWithValue("$prior_last_attempt", (object?)ToNullableUtc(entry.PriorLastAttemptUtc) ?? DBNull.Value);
            command.Parameters.AddWithValue("$prior_error_code", entry.PriorErrorCode);
            command.Parameters.AddWithValue("$prior_error", entry.PriorError);
            command.Parameters.AddWithValue("$attempts", Math.Max(0, entry.AttemptCount));
            command.Parameters.AddWithValue("$next", (object?)ToNullableUtc(entry.NextAttemptUtc) ?? DBNull.Value);
            command.Parameters.AddWithValue("$last_attempt", (object?)ToNullableUtc(entry.LastAttemptUtc) ?? DBNull.Value);
            command.Parameters.AddWithValue("$error_code", entry.LastErrorCode);
            command.Parameters.AddWithValue("$error", entry.LastError);
            command.Parameters.AddWithValue("$updated", entry.UpdatedUtc.ToUniversalTime().ToString("O"));
            command.Parameters.AddWithValue("$key", entry.TransferKey);
            command.Parameters.AddWithValue("$expected_operation_id", expectedOperationId);
            return await command.ExecuteNonQueryAsync(token).ConfigureAwait(false) == 1;
        }
        finally
        {
            _writeGate.Release();
        }
    }

    public async Task<CloudTransferQueueEntry?> GetCloudTransferAsync(string transferKey, CancellationToken token)
    {
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = SelectCloudTransfers + " WHERE transfer_key=$key LIMIT 1;";
        command.Parameters.AddWithValue("$key", transferKey);
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        return await reader.ReadAsync(token).ConfigureAwait(false) ? ReadCloudTransfer(reader) : null;
    }

    public async Task<List<CloudTransferQueueEntry>> GetCloudTransfersAsync(int limit, CancellationToken token)
    {
        var result = new List<CloudTransferQueueEntry>();
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = SelectCloudTransfers + " ORDER BY updated_utc DESC LIMIT $limit;";
        command.Parameters.AddWithValue("$limit", Math.Clamp(limit, 1, 1000));
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false)) result.Add(ReadCloudTransfer(reader));
        return result;
    }

    public async Task<CloudTransferSummaryAggregate> GetCloudTransferSummaryAsync(
        string? stateFilter, CloudTransferKind? kindFilter, CancellationToken token)
    {
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = $@"
SELECT COUNT(*),
       COALESCE(SUM(CASE WHEN current.state='Pending' THEN 1 ELSE 0 END),0),
       COALESCE(SUM(CASE WHEN current.state='Transferring' THEN 1 ELSE 0 END),0),
       COALESCE(SUM(CASE WHEN current.state='Verifying' THEN 1 ELSE 0 END),0),
       COALESCE(SUM(CASE WHEN current.state='RetryScheduled' THEN 1 ELSE 0 END),0),
       COALESCE(SUM(CASE WHEN current.state='AuthenticationRequired' THEN 1 ELSE 0 END),0),
       COALESCE(SUM(CASE WHEN current.state='Uploaded' THEN 1 ELSE 0 END),0),
       COALESCE(SUM(CASE WHEN current.state='RemoteVerified' THEN 1 ELSE 0 END),0),
       COALESCE(SUM(CASE WHEN current.state='CheckFailed' THEN 1 ELSE 0 END),0),
       COALESCE(SUM(CASE WHEN current.state='Failed' THEN 1 ELSE 0 END),0),
       COALESCE(SUM(CASE WHEN current.state='Paused' THEN 1 ELSE 0 END),0),
       MIN(CASE WHEN current.state='RetryScheduled' THEN current.next_attempt_utc END)
FROM ({CurrentCloudTransferRows}) AS current
WHERE current.state <> 'NotApplicable'
  AND ($state='' OR current.state=$state)
  AND ($kind='' OR current.transfer_kind=$kind);";
        AddCloudTransferFilters(command, stateFilter, kindFilter);
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        if (!await reader.ReadAsync(token).ConfigureAwait(false)) return new CloudTransferSummaryAggregate();
        return new CloudTransferSummaryAggregate
        {
            TotalCount = Convert.ToInt32(reader.GetInt64(0)),
            PendingCount = Convert.ToInt32(reader.GetInt64(1)),
            TransferringCount = Convert.ToInt32(reader.GetInt64(2)),
            VerifyingCount = Convert.ToInt32(reader.GetInt64(3)),
            RetryScheduledCount = Convert.ToInt32(reader.GetInt64(4)),
            AuthenticationRequiredCount = Convert.ToInt32(reader.GetInt64(5)),
            UploadedCount = Convert.ToInt32(reader.GetInt64(6)),
            VerifiedCount = Convert.ToInt32(reader.GetInt64(7)),
            CheckFailedCount = Convert.ToInt32(reader.GetInt64(8)),
            FailedCount = Convert.ToInt32(reader.GetInt64(9)),
            PausedCount = Convert.ToInt32(reader.GetInt64(10)),
            NextAttemptUtc = ParseNullableUtc(reader, 11)
        };
    }

    public async Task<List<CloudTransferQueueEntry>> GetCloudTransferPageAsync(
        int offset, int limit, string? stateFilter, CloudTransferKind? kindFilter, CancellationToken token)
    {
        var result = new List<CloudTransferQueueEntry>();
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = $@"
SELECT transfer_key,transfer_kind,playnite_id,state,operation_kind,operation_id,prior_state,prior_operation_kind,prior_operation_id,
       prior_next_attempt_utc,prior_last_attempt_utc,prior_error_code,prior_error,attempt_count,next_attempt_utc,last_attempt_utc,
       last_error_code,last_error,created_utc,updated_utc
FROM ({CurrentCloudTransferRows}) AS current
WHERE current.state <> 'NotApplicable'
  AND ($state='' OR current.state=$state)
  AND ($kind='' OR current.transfer_kind=$kind)
ORDER BY CASE current.state
    WHEN 'AuthenticationRequired' THEN 7
    WHEN 'CheckFailed' THEN 6
    WHEN 'Failed' THEN 5
    WHEN 'RetryScheduled' THEN 4
    WHEN 'Transferring' THEN 3
    WHEN 'Verifying' THEN 3
    WHEN 'Pending' THEN 2
    WHEN 'Paused' THEN 1
    ELSE 0 END DESC,
    CASE WHEN current.state='RetryScheduled' THEN COALESCE(current.next_attempt_utc,'9999-12-31T23:59:59.9999999Z') ELSE '9999-12-31T23:59:59.9999999Z' END,
    current.updated_utc DESC,
    current.transfer_key COLLATE NOCASE
LIMIT $limit OFFSET $offset;";
        AddCloudTransferFilters(command, stateFilter, kindFilter);
        command.Parameters.AddWithValue("$limit", Math.Clamp(limit, 1, 100));
        command.Parameters.AddWithValue("$offset", Math.Max(0, offset));
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false)) result.Add(ReadCloudTransfer(reader));
        return result;
    }

    public async Task<List<CloudTransferQueueEntry>> GetDueCloudTransfersAsync(CloudTransferKind kind, DateTime nowUtc, int limit, CancellationToken token)
    {
        var result = new List<CloudTransferQueueEntry>();
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = SelectCloudTransfers + @"
 WHERE transfer_kind=$kind AND state='RetryScheduled' AND next_attempt_utc IS NOT NULL AND next_attempt_utc <= $now
 ORDER BY next_attempt_utc LIMIT $limit;";
        command.Parameters.AddWithValue("$kind", kind.ToString());
        command.Parameters.AddWithValue("$now", nowUtc.ToUniversalTime().ToString("O"));
        command.Parameters.AddWithValue("$limit", Math.Clamp(limit, 1, 100));
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false)) result.Add(ReadCloudTransfer(reader));
        return result;
    }

    public Task RecoverInterruptedCloudTransfersAsync(DateTime nextAttemptUtc, CancellationToken token)
        => ExecuteAsync(@"
UPDATE cloud_transfer_queue
SET state='RetryScheduled',next_attempt_utc=$next,last_error_code='WORKER_RESTARTED_RETRYABLE',
    last_error='Worker 在云端复制过程中退出，已重新排队；本地副本保持不变。',updated_utc=$updated
WHERE state IN ('Pending','Transferring') AND operation_kind='Upload';
UPDATE cloud_transfer_queue
SET state=CASE WHEN prior_state<>'' THEN prior_state ELSE 'CheckFailed' END,
    operation_kind=CASE WHEN prior_state<>'' THEN prior_operation_kind ELSE 'Verify' END,
    operation_id=CASE WHEN prior_state<>'' THEN prior_operation_id ELSE operation_id END,
    next_attempt_utc=CASE WHEN prior_state<>'' THEN prior_next_attempt_utc ELSE NULL END,
    last_attempt_utc=CASE WHEN prior_state<>'' THEN prior_last_attempt_utc ELSE last_attempt_utc END,
    last_error_code=CASE WHEN prior_state<>'' THEN prior_error_code ELSE 'WORKER_RESTARTED_VERIFY' END,
    last_error=CASE WHEN prior_state<>'' THEN prior_error ELSE 'Worker 在远端校验期间退出；未重新发起上传。' END,
    prior_state='',prior_operation_kind='Upload',prior_operation_id='',prior_next_attempt_utc=NULL,
    prior_last_attempt_utc=NULL,prior_error_code='',prior_error='',updated_utc=$updated
WHERE state='Verifying' AND operation_kind='Verify';",
            new Dictionary<string, object?>
            {
                ["$next"] = nextAttemptUtc.ToUniversalTime().ToString("O"),
                ["$updated"] = DateTime.UtcNow.ToString("O")
            }, token);

    public async Task<List<CloudGameStateRecord>> GetCloudGameStatesAsync(CancellationToken token)
    {
        var result = new List<CloudGameStateRecord>();
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT playnite_id,name,cloud_state FROM games WHERE COALESCE(cloud_state,'Disabled') <> 'Disabled';";
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false))
            result.Add(new CloudGameStateRecord { PlayniteId = reader.GetString(0), GameName = reader.GetString(1), State = reader.IsDBNull(2) ? "Disabled" : reader.GetString(2) });
        return result;
    }

    public async Task<List<CloudMediaStateRecord>> GetCloudMediaStatesAsync(CancellationToken token)
    {
        var result = new List<CloudMediaStateRecord>();
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT m.playnite_id,COALESCE(g.name,''),
       CASE WHEN SUM(CASE WHEN m.cloud_state='AuthenticationRequired' THEN 1 ELSE 0 END)>0 THEN 'AuthenticationRequired'
            WHEN SUM(CASE WHEN m.cloud_state='CheckFailed' THEN 1 ELSE 0 END)>0 THEN 'CheckFailed'
            WHEN SUM(CASE WHEN m.cloud_state='Failed' THEN 1 ELSE 0 END)>0 THEN 'Failed'
            WHEN SUM(CASE WHEN m.cloud_state='RetryScheduled' THEN 1 ELSE 0 END)>0 THEN 'RetryScheduled'
            WHEN SUM(CASE WHEN m.cloud_state='Pending' THEN 1 ELSE 0 END)>0 THEN 'Pending'
            WHEN SUM(CASE WHEN m.cloud_state='RemoteVerified' THEN 1 ELSE 0 END)>0 THEN 'RemoteVerified'
            WHEN SUM(CASE WHEN m.cloud_state IN ('Synced','Uploaded') THEN 1 ELSE 0 END)>0 THEN 'Uploaded'
            ELSE 'NotApplicable' END
FROM media m LEFT JOIN games g ON g.playnite_id=m.playnite_id
WHERE COALESCE(m.playnite_id,'')<>'' AND m.classification_state='Assigned'
GROUP BY m.playnite_id,g.name;";
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false))
            result.Add(new CloudMediaStateRecord { PlayniteId = reader.GetString(0), GameName = reader.GetString(1), State = reader.GetString(2) });
        return result;
    }

    public async Task<List<CloudRetryQueueEntry>> GetCloudRetriesAsync(int limit, CancellationToken token)
    {
        var result = new List<CloudRetryQueueEntry>();
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = "SELECT playnite_id,attempt_count,next_attempt_utc,last_error,created_utc,updated_utc FROM cloud_retry_queue ORDER BY updated_utc DESC LIMIT $limit;";
        command.Parameters.AddWithValue("$limit", Math.Clamp(limit, 1, 1000));
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false)) result.Add(ReadCloudRetry(reader));
        return result;
    }

    private static void AddCloudTransferFilters(SqliteCommand command, string? stateFilter, CloudTransferKind? kindFilter)
    {
        command.Parameters.AddWithValue("$state", stateFilter?.Trim() ?? string.Empty);
        command.Parameters.AddWithValue("$kind", kindFilter?.ToString() ?? string.Empty);
    }

    private const string SelectCloudTransfers = @"SELECT transfer_key,transfer_kind,playnite_id,state,operation_kind,operation_id,prior_state,prior_operation_kind,prior_operation_id,prior_next_attempt_utc,prior_last_attempt_utc,prior_error_code,prior_error,attempt_count,next_attempt_utc,last_attempt_utc,last_error_code,last_error,created_utc,updated_utc FROM cloud_transfer_queue";

    private const string CurrentCloudTransferRows = @"
WITH media_base AS (
    SELECT m.playnite_id,COALESCE(g.name,'') AS game_name,
           CASE WHEN SUM(CASE WHEN m.cloud_state='AuthenticationRequired' THEN 1 ELSE 0 END)>0 THEN 'AuthenticationRequired'
                WHEN SUM(CASE WHEN m.cloud_state='CheckFailed' THEN 1 ELSE 0 END)>0 THEN 'CheckFailed'
                WHEN SUM(CASE WHEN m.cloud_state='Failed' THEN 1 ELSE 0 END)>0 THEN 'Failed'
                WHEN SUM(CASE WHEN m.cloud_state='RetryScheduled' THEN 1 ELSE 0 END)>0 THEN 'RetryScheduled'
                WHEN SUM(CASE WHEN m.cloud_state='Pending' THEN 1 ELSE 0 END)>0 THEN 'Pending'
                WHEN SUM(CASE WHEN m.cloud_state='RemoteVerified' THEN 1 ELSE 0 END)>0 THEN 'RemoteVerified'
                WHEN SUM(CASE WHEN m.cloud_state IN ('Synced','Uploaded') THEN 1 ELSE 0 END)>0 THEN 'Uploaded'
                ELSE 'NotApplicable' END AS state
    FROM media m LEFT JOIN games g ON g.playnite_id=m.playnite_id
    WHERE COALESCE(m.playnite_id,'')<>'' AND m.classification_state='Assigned'
    GROUP BY m.playnite_id,g.name
), candidates AS (
    SELECT transfer_key,transfer_kind,playnite_id,state,operation_kind,operation_id,prior_state,prior_operation_kind,prior_operation_id,
           prior_next_attempt_utc,prior_last_attempt_utc,prior_error_code,prior_error,attempt_count,next_attempt_utc,last_attempt_utc,
           last_error_code,last_error,created_utc,updated_utc,0 AS source_priority
    FROM cloud_transfer_queue
    UNION ALL
    SELECT 'Backup:'||r.playnite_id,'Backup',r.playnite_id,
           CASE WHEN lower(COALESCE(r.last_error,'')) LIKE '%authentication%'
                     OR lower(COALESCE(r.last_error,'')) LIKE '%unauthorized%'
                     OR lower(COALESCE(r.last_error,'')) LIKE '%invalid token%'
                     OR lower(COALESCE(r.last_error,'')) LIKE '%expired token%'
                THEN 'AuthenticationRequired' ELSE 'RetryScheduled' END,
           'Upload','', '', 'Upload','', NULL, r.updated_utc, '', '', r.attempt_count, r.next_attempt_utc, r.updated_utc,
           CASE WHEN lower(COALESCE(r.last_error,'')) LIKE '%authentication%'
                     OR lower(COALESCE(r.last_error,'')) LIKE '%unauthorized%'
                     OR lower(COALESCE(r.last_error,'')) LIKE '%invalid token%'
                     OR lower(COALESCE(r.last_error,'')) LIKE '%expired token%'
                THEN 'RCLONE_AUTH_FAILED'
                WHEN lower(COALESCE(r.last_error,'')) LIKE '%permission denied%'
                     OR lower(COALESCE(r.last_error,'')) LIKE '%access denied%'
                     OR lower(COALESCE(r.last_error,'')) LIKE '%forbidden%' THEN 'RCLONE_PERMISSION_DENIED'
                WHEN lower(COALESCE(r.last_error,'')) LIKE '%timeout%'
                     OR lower(COALESCE(r.last_error,'')) LIKE '%timed out%'
                     OR lower(COALESCE(r.last_error,'')) LIKE '%connection%'
                     OR lower(COALESCE(r.last_error,'')) LIKE '%network%'
                     OR lower(COALESCE(r.last_error,'')) LIKE '%temporarily unavailable%'
                     OR lower(COALESCE(r.last_error,'')) LIKE '%429%' THEN 'RCLONE_NETWORK_FAILED'
                WHEN lower(COALESCE(r.last_error,'')) LIKE '%partial%'
                     OR lower(COALESCE(r.last_error,'')) LIKE '%incomplete%'
                     OR (lower(COALESCE(r.last_error,'')) LIKE '%transferred%' AND lower(COALESCE(r.last_error,'')) LIKE '%error%') THEN 'RCLONE_TRANSFER_INCOMPLETE'
                ELSE 'RCLONE_COPY_FAILED' END,
           r.last_error,r.created_utc,r.updated_utc,1
    FROM cloud_retry_queue r
    UNION ALL
    SELECT 'Backup:'||g.playnite_id,'Backup',g.playnite_id,
           CASE WHEN g.cloud_state='Synced' THEN 'Uploaded' ELSE COALESCE(g.cloud_state,'Disabled') END,
           'Upload','', '', 'Upload','', NULL, NULL, '', '', 0, NULL, NULL, '', '',
           strftime('%Y-%m-%dT%H:%M:%fZ','now'),strftime('%Y-%m-%dT%H:%M:%fZ','now'),2
    FROM games g
    WHERE COALESCE(g.cloud_state,'Disabled')<>'Disabled'
    UNION ALL
    SELECT 'Media:'||m.playnite_id,'Media',m.playnite_id,m.state,
           'Upload','', '', 'Upload','', NULL, NULL, '', '', 0, NULL, NULL, '', '',
           strftime('%Y-%m-%dT%H:%M:%fZ','now'),strftime('%Y-%m-%dT%H:%M:%fZ','now'),2
    FROM media_base m
), ranked AS (
    SELECT candidates.*,
           ROW_NUMBER() OVER (PARTITION BY transfer_key COLLATE NOCASE ORDER BY source_priority) AS duplicate_rank
    FROM candidates
)
SELECT transfer_key,transfer_kind,playnite_id,state,operation_kind,operation_id,prior_state,prior_operation_kind,prior_operation_id,
       prior_next_attempt_utc,prior_last_attempt_utc,prior_error_code,prior_error,attempt_count,next_attempt_utc,last_attempt_utc,
       last_error_code,last_error,created_utc,updated_utc
FROM ranked
WHERE duplicate_rank=1";

    private static CloudTransferQueueEntry ReadCloudTransfer(SqliteDataReader reader)
        => new()
        {
            TransferKey = reader.GetString(0),
            Kind = Enum.TryParse<CloudTransferKind>(reader.GetString(1), true, out var kind) ? kind : CloudTransferKind.Backup,
            PlayniteId = reader.GetString(2),
            State = reader.GetString(3),
            OperationKind = Enum.TryParse<CloudTransferOperationKind>(reader.GetString(4), true, out var operationKind) ? operationKind : CloudTransferOperationKind.Upload,
            OperationId = reader.GetString(5),
            PriorState = reader.GetString(6),
            PriorOperationKind = Enum.TryParse<CloudTransferOperationKind>(reader.GetString(7), true, out var priorOperationKind) ? priorOperationKind : CloudTransferOperationKind.Upload,
            PriorOperationId = reader.GetString(8),
            PriorNextAttemptUtc = ParseNullableUtc(reader, 9),
            PriorLastAttemptUtc = ParseNullableUtc(reader, 10),
            PriorErrorCode = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
            PriorError = reader.IsDBNull(12) ? string.Empty : reader.GetString(12),
            AttemptCount = reader.GetInt32(13),
            NextAttemptUtc = ParseNullableUtc(reader, 14),
            LastAttemptUtc = ParseNullableUtc(reader, 15),
            LastErrorCode = reader.IsDBNull(16) ? string.Empty : reader.GetString(16),
            LastError = reader.IsDBNull(17) ? string.Empty : reader.GetString(17),
            CreatedUtc = DateTime.Parse(reader.GetString(18)).ToUniversalTime(),
            UpdatedUtc = DateTime.Parse(reader.GetString(19)).ToUniversalTime()
        };

    private static DateTime? ParseNullableUtc(SqliteDataReader reader, int ordinal)
        => reader.IsDBNull(ordinal) ? null : DateTime.Parse(reader.GetString(ordinal)).ToUniversalTime();

    private static string? ToNullableUtc(DateTime? value)
        => value?.ToUniversalTime().ToString("O");
}

public sealed class CloudTransferQueueEntry
{
    public string TransferKey { get; set; } = string.Empty;
    public CloudTransferKind Kind { get; set; }
    public string PlayniteId { get; set; } = string.Empty;
    public string State { get; set; } = "Pending";
    public CloudTransferOperationKind OperationKind { get; set; } = CloudTransferOperationKind.Upload;
    public string OperationId { get; set; } = string.Empty;
    public string PriorState { get; set; } = string.Empty;
    public CloudTransferOperationKind PriorOperationKind { get; set; } = CloudTransferOperationKind.Upload;
    public string PriorOperationId { get; set; } = string.Empty;
    public DateTime? PriorNextAttemptUtc { get; set; }
    public DateTime? PriorLastAttemptUtc { get; set; }
    public string PriorErrorCode { get; set; } = string.Empty;
    public string PriorError { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
    public DateTime? NextAttemptUtc { get; set; }
    public DateTime? LastAttemptUtc { get; set; }
    public string LastErrorCode { get; set; } = string.Empty;
    public string LastError { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
}

public sealed class CloudTransferSummaryAggregate
{
    public int TotalCount { get; set; }
    public int PendingCount { get; set; }
    public int TransferringCount { get; set; }
    public int VerifyingCount { get; set; }
    public int RetryScheduledCount { get; set; }
    public int AuthenticationRequiredCount { get; set; }
    public int UploadedCount { get; set; }
    public int VerifiedCount { get; set; }
    public int CheckFailedCount { get; set; }
    public int FailedCount { get; set; }
    public int PausedCount { get; set; }
    public DateTime? NextAttemptUtc { get; set; }
}

public class CloudGameStateRecord
{
    public string PlayniteId { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}

public sealed class CloudMediaStateRecord : CloudGameStateRecord
{
}
