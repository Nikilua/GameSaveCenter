using GameSaveCenter.Contracts;
using Microsoft.Data.Sqlite;

namespace GameSaveCenter.Worker.Persistence;

/// <summary>Durable state for both backup and media cloud-copy pipelines.</summary>
public sealed partial class SqliteStateStore
{
    public Task UpsertCloudTransferAsync(CloudTransferQueueEntry entry, CancellationToken token)
        => ExecuteAsync(@"
INSERT INTO cloud_transfer_queue(transfer_key,transfer_kind,playnite_id,state,attempt_count,next_attempt_utc,last_attempt_utc,last_error_code,last_error,created_utc,updated_utc)
VALUES($key,$kind,$game,$state,$attempts,$next,$last_attempt,$error_code,$error,$created,$updated)
ON CONFLICT(transfer_key) DO UPDATE SET
transfer_kind=excluded.transfer_kind,playnite_id=excluded.playnite_id,state=excluded.state,
attempt_count=excluded.attempt_count,next_attempt_utc=excluded.next_attempt_utc,last_attempt_utc=excluded.last_attempt_utc,
last_error_code=excluded.last_error_code,last_error=excluded.last_error,updated_utc=excluded.updated_utc;",
            new Dictionary<string, object?>
            {
                ["$key"] = entry.TransferKey,
                ["$kind"] = entry.Kind.ToString(),
                ["$game"] = entry.PlayniteId,
                ["$state"] = entry.State,
                ["$attempts"] = Math.Max(0, entry.AttemptCount),
                ["$next"] = ToNullableUtc(entry.NextAttemptUtc),
                ["$last_attempt"] = ToNullableUtc(entry.LastAttemptUtc),
                ["$error_code"] = entry.LastErrorCode,
                ["$error"] = entry.LastError,
                ["$created"] = entry.CreatedUtc.ToUniversalTime().ToString("O"),
                ["$updated"] = entry.UpdatedUtc.ToUniversalTime().ToString("O")
            }, token);

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
WHERE state IN ('Pending','Transferring');",
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

    private const string SelectCloudTransfers = @"SELECT transfer_key,transfer_kind,playnite_id,state,attempt_count,next_attempt_utc,last_attempt_utc,last_error_code,last_error,created_utc,updated_utc FROM cloud_transfer_queue";

    private static CloudTransferQueueEntry ReadCloudTransfer(SqliteDataReader reader)
        => new()
        {
            TransferKey = reader.GetString(0),
            Kind = Enum.TryParse<CloudTransferKind>(reader.GetString(1), true, out var kind) ? kind : CloudTransferKind.Backup,
            PlayniteId = reader.GetString(2),
            State = reader.GetString(3),
            AttemptCount = reader.GetInt32(4),
            NextAttemptUtc = ParseNullableUtc(reader, 5),
            LastAttemptUtc = ParseNullableUtc(reader, 6),
            LastErrorCode = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
            LastError = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
            CreatedUtc = DateTime.Parse(reader.GetString(9)).ToUniversalTime(),
            UpdatedUtc = DateTime.Parse(reader.GetString(10)).ToUniversalTime()
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
    public int AttemptCount { get; set; }
    public DateTime? NextAttemptUtc { get; set; }
    public DateTime? LastAttemptUtc { get; set; }
    public string LastErrorCode { get; set; } = string.Empty;
    public string LastError { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
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
