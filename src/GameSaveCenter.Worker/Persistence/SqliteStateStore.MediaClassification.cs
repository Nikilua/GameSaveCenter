using GameSaveCenter.Contracts;
using Microsoft.Data.Sqlite;

namespace GameSaveCenter.Worker.Persistence;

/// <summary>Persistent previews and conflict-aware undo records for media classification.</summary>
public sealed partial class SqliteStateStore
{
    public async Task CreateMediaClassificationBatchAsync(string batchId, DateTime createdUtc, DateTime expiresUtc,
        IReadOnlyList<MediaClassificationBatchItemRecord> items, CancellationToken token)
    {
        await _writeGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await using var connection = Open();
            await connection.OpenAsync(token).ConfigureAwait(false);
            await using var transaction = await connection.BeginTransactionAsync(token).ConfigureAwait(false);

            var batch = connection.CreateCommand();
            batch.Transaction = (SqliteTransaction)transaction;
            batch.CommandText = @"
INSERT INTO media_classification_batches(batch_id,state,created_utc,updated_utc,expires_utc,last_error)
VALUES($batch,'Preview',$created,$updated,$expires,'');";
            batch.Parameters.AddWithValue("$batch", batchId);
            batch.Parameters.AddWithValue("$created", createdUtc.ToUniversalTime().ToString("O"));
            batch.Parameters.AddWithValue("$updated", createdUtc.ToUniversalTime().ToString("O"));
            batch.Parameters.AddWithValue("$expires", expiresUtc.ToUniversalTime().ToString("O"));
            await batch.ExecuteNonQueryAsync(token).ConfigureAwait(false);

            foreach (var item in items)
            {
                var command = connection.CreateCommand();
                command.Transaction = (SqliteTransaction)transaction;
                command.CommandText = @"
INSERT INTO media_classification_batch_items(
 batch_id,media_id,original_playnite_id,original_classification_state,original_classification_reason,
 original_archive_path,original_path,original_captured_utc,original_size_bytes,original_sha256,
 original_is_favorite,original_comment,original_cloud_state,target_playnite_id,target_reason,confidence,
 item_state,applied_archive_path,updated_utc)
VALUES($batch,$media,$original_game,$original_state,$original_reason,$original_archive,$original_path,
 $captured,$size,$sha,$favorite,$comment,$original_cloud,$target,$target_reason,$confidence,
 'Pending','',$updated);";
                command.Parameters.AddWithValue("$batch", batchId);
                command.Parameters.AddWithValue("$media", item.MediaId);
                command.Parameters.AddWithValue("$original_game", item.OriginalPlayniteId);
                command.Parameters.AddWithValue("$original_state", item.OriginalClassificationState);
                command.Parameters.AddWithValue("$original_reason", item.OriginalClassificationReason);
                command.Parameters.AddWithValue("$original_archive", item.OriginalArchivePath);
                command.Parameters.AddWithValue("$original_path", item.OriginalPath);
                command.Parameters.AddWithValue("$captured", item.OriginalCapturedUtc.ToUniversalTime().ToString("O"));
                command.Parameters.AddWithValue("$size", item.OriginalSizeBytes);
                command.Parameters.AddWithValue("$sha", item.OriginalSha256);
                command.Parameters.AddWithValue("$favorite", item.OriginalIsFavorite ? 1 : 0);
                command.Parameters.AddWithValue("$comment", item.OriginalComment);
                command.Parameters.AddWithValue("$original_cloud", item.OriginalCloudState);
                command.Parameters.AddWithValue("$target", item.TargetPlayniteId);
                command.Parameters.AddWithValue("$target_reason", item.TargetReason);
                command.Parameters.AddWithValue("$confidence", item.Confidence);
                command.Parameters.AddWithValue("$updated", createdUtc.ToUniversalTime().ToString("O"));
                await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
            }

            await transaction.CommitAsync(token).ConfigureAwait(false);
        }
        finally
        {
            _writeGate.Release();
        }
    }

    public async Task<MediaClassificationBatchRecord?> GetMediaClassificationBatchAsync(string batchId, CancellationToken token)
    {
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = @"SELECT batch_id,state,created_utc,updated_utc,expires_utc,last_error
FROM media_classification_batches WHERE batch_id=$batch LIMIT 1;";
        command.Parameters.AddWithValue("$batch", batchId);
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        if (!await reader.ReadAsync(token).ConfigureAwait(false)) return null;
        return new MediaClassificationBatchRecord
        {
            BatchId = reader.GetString(0), State = reader.GetString(1),
            CreatedUtc = DateTime.Parse(reader.GetString(2)).ToUniversalTime(),
            UpdatedUtc = DateTime.Parse(reader.GetString(3)).ToUniversalTime(),
            ExpiresUtc = DateTime.Parse(reader.GetString(4)).ToUniversalTime(),
            LastError = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
        };
    }

    public async Task<List<MediaClassificationBatchItemRecord>> GetMediaClassificationBatchItemsAsync(string batchId, CancellationToken token)
    {
        var result = new List<MediaClassificationBatchItemRecord>();
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT batch_id,media_id,original_playnite_id,original_classification_state,original_classification_reason,
       original_archive_path,original_path,original_captured_utc,original_size_bytes,original_sha256,
       original_is_favorite,original_comment,original_cloud_state,target_playnite_id,target_reason,confidence,
       item_state,applied_archive_path,updated_utc
FROM media_classification_batch_items WHERE batch_id=$batch ORDER BY media_id;";
        command.Parameters.AddWithValue("$batch", batchId);
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false)) result.Add(ReadMediaClassificationBatchItem(reader));
        return result;
    }

    public Task UpdateMediaClassificationBatchStateAsync(string batchId, string state, string error, CancellationToken token)
        => ExecuteAsync(@"UPDATE media_classification_batches
SET state=$state,updated_utc=$updated,last_error=$error WHERE batch_id=$batch;",
            new Dictionary<string, object?>
            {
                ["$batch"] = batchId, ["$state"] = state, ["$updated"] = DateTime.UtcNow.ToString("O"), ["$error"] = error
            }, token);

    public Task UpdateMediaClassificationBatchItemAsync(string batchId, string mediaId, string state, string appliedArchivePath, CancellationToken token)
        => ExecuteAsync(@"UPDATE media_classification_batch_items
SET item_state=$state,applied_archive_path=$applied,updated_utc=$updated
WHERE batch_id=$batch AND media_id=$media;",
            new Dictionary<string, object?>
            {
                ["$batch"] = batchId, ["$media"] = mediaId, ["$state"] = state,
                ["$applied"] = appliedArchivePath, ["$updated"] = DateTime.UtcNow.ToString("O")
            }, token);

    public async Task<List<MediaSourceRuleDto>> GetEnabledMediaSourcesForClassificationAsync(CancellationToken token)
    {
        var result = new List<MediaSourceRuleDto>();
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = @"SELECT source_id,playnite_id,source_kind,root_path,include_pattern,enabled,shared_directory
FROM media_sources WHERE enabled=1;";
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false)) result.Add(new MediaSourceRuleDto
        {
            SourceId = reader.GetString(0), PlayniteId = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
            SourceKind = (MediaSourceKind)reader.GetInt32(2), RootPath = reader.GetString(3),
            IncludePattern = reader.IsDBNull(4) ? "*" : reader.GetString(4), Enabled = reader.GetInt32(5) == 1,
            SharedDirectory = !reader.IsDBNull(6) && reader.GetInt32(6) == 1
        });
        return result;
    }

    public async Task<List<GameSessionEventDto>> GetSessionsForMediaClassificationAsync(DateTime fromUtc, DateTime toUtc, CancellationToken token)
    {
        var result = new List<GameSessionEventDto>();
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = @"SELECT session_id,playnite_id,source,process_id,process_name,launch_profile,started_utc,stopped_utc,elapsed_seconds
FROM sessions WHERE started_utc <= $to AND (stopped_utc IS NULL OR stopped_utc >= $from)
ORDER BY started_utc DESC LIMIT 2000;";
        command.Parameters.AddWithValue("$from", fromUtc.ToUniversalTime().ToString("O"));
        command.Parameters.AddWithValue("$to", toUtc.ToUniversalTime().ToString("O"));
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false)) result.Add(new GameSessionEventDto
        {
            SessionId = reader.GetString(0), PlayniteId = reader.GetString(1), Source = (SessionSourceKind)reader.GetInt32(2),
            ProcessId = reader.IsDBNull(3) ? null : reader.GetInt32(3), ProcessName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
            LaunchProfile = reader.IsDBNull(5) ? string.Empty : reader.GetString(5), StartedUtc = DateTime.Parse(reader.GetString(6)).ToUniversalTime(),
            StoppedUtc = reader.IsDBNull(7) ? null : DateTime.Parse(reader.GetString(7)).ToUniversalTime(),
            ElapsedSeconds = reader.IsDBNull(8) ? 0 : reader.GetInt64(8)
        });
        return result;
    }

    public async Task<bool> TryApplyMediaClassificationAsync(MediaClassificationBatchItemRecord item, string appliedArchivePath, CancellationToken token)
        => await ExecuteConditionalMediaClassificationAsync(@"
UPDATE media SET playnite_id=$target,archive_path=$applied,classification_state='Assigned',
    classification_reason=$target_reason,cloud_state='Pending'
WHERE media_id=$media AND COALESCE(playnite_id,'')=$original_game
  AND COALESCE(classification_state,'')=$original_state
  AND COALESCE(classification_reason,'')=$original_reason
  AND archive_path=$original_archive AND original_path=$original_path
  AND captured_utc=$captured AND size_bytes=$size AND sha256=$sha
  AND is_favorite=$favorite AND COALESCE(comment,'')=$comment
  AND COALESCE(cloud_state,'')=$original_cloud;", item, appliedArchivePath, token).ConfigureAwait(false);

    public async Task<bool> TryUndoMediaClassificationAsync(MediaClassificationBatchItemRecord item, CancellationToken token)
        => await ExecuteConditionalMediaClassificationAsync(@"
UPDATE media SET playnite_id=$original_game,archive_path=$original_archive,
    classification_state=$original_state,classification_reason=$original_reason,cloud_state=$original_cloud
WHERE media_id=$media AND COALESCE(playnite_id,'')=$target
  AND classification_state='Assigned' AND archive_path=$applied
  AND original_path=$original_path AND captured_utc=$captured AND size_bytes=$size AND sha256=$sha
  AND is_favorite=$favorite AND COALESCE(comment,'')=$comment AND cloud_state='Pending';", item, item.AppliedArchivePath, token).ConfigureAwait(false);

    private async Task<bool> ExecuteConditionalMediaClassificationAsync(string sql, MediaClassificationBatchItemRecord item,
        string pathValue, CancellationToken token)
    {
        await _writeGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await using var connection = Open();
            await connection.OpenAsync(token).ConfigureAwait(false);
            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("$media", item.MediaId);
            command.Parameters.AddWithValue("$original_game", item.OriginalPlayniteId);
            command.Parameters.AddWithValue("$original_state", item.OriginalClassificationState);
            command.Parameters.AddWithValue("$original_reason", item.OriginalClassificationReason);
            command.Parameters.AddWithValue("$original_archive", item.OriginalArchivePath);
            command.Parameters.AddWithValue("$original_path", item.OriginalPath);
            command.Parameters.AddWithValue("$captured", item.OriginalCapturedUtc.ToUniversalTime().ToString("O"));
            command.Parameters.AddWithValue("$size", item.OriginalSizeBytes);
            command.Parameters.AddWithValue("$sha", item.OriginalSha256);
            command.Parameters.AddWithValue("$favorite", item.OriginalIsFavorite ? 1 : 0);
            command.Parameters.AddWithValue("$comment", item.OriginalComment);
            command.Parameters.AddWithValue("$original_cloud", item.OriginalCloudState);
            command.Parameters.AddWithValue("$target", item.TargetPlayniteId);
            command.Parameters.AddWithValue("$target_reason", item.TargetReason);
            command.Parameters.AddWithValue("$applied", pathValue);
            return await command.ExecuteNonQueryAsync(token).ConfigureAwait(false) == 1;
        }
        finally
        {
            _writeGate.Release();
        }
    }

    private static MediaClassificationBatchItemRecord ReadMediaClassificationBatchItem(SqliteDataReader reader)
        => new()
        {
            BatchId = reader.GetString(0), MediaId = reader.GetString(1), OriginalPlayniteId = reader.GetString(2),
            OriginalClassificationState = reader.GetString(3), OriginalClassificationReason = reader.GetString(4),
            OriginalArchivePath = reader.GetString(5), OriginalPath = reader.GetString(6),
            OriginalCapturedUtc = DateTime.Parse(reader.GetString(7)).ToUniversalTime(), OriginalSizeBytes = reader.GetInt64(8),
            OriginalSha256 = reader.GetString(9), OriginalIsFavorite = reader.GetInt32(10) == 1,
            OriginalComment = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
            OriginalCloudState = reader.IsDBNull(12) ? string.Empty : reader.GetString(12), TargetPlayniteId = reader.GetString(13),
            TargetReason = reader.GetString(14), Confidence = reader.GetString(15), ItemState = reader.GetString(16),
            AppliedArchivePath = reader.IsDBNull(17) ? string.Empty : reader.GetString(17),
            UpdatedUtc = DateTime.Parse(reader.GetString(18)).ToUniversalTime()
        };
}

public sealed class MediaClassificationBatchRecord
{
    public string BatchId { get; set; } = string.Empty;
    public string State { get; set; } = "Preview";
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
    public DateTime ExpiresUtc { get; set; }
    public string LastError { get; set; } = string.Empty;
}

public sealed class MediaClassificationBatchItemRecord
{
    public string BatchId { get; set; } = string.Empty;
    public string MediaId { get; set; } = string.Empty;
    public string OriginalPlayniteId { get; set; } = string.Empty;
    public string OriginalClassificationState { get; set; } = "Inbox";
    public string OriginalClassificationReason { get; set; } = string.Empty;
    public string OriginalArchivePath { get; set; } = string.Empty;
    public string OriginalPath { get; set; } = string.Empty;
    public DateTime OriginalCapturedUtc { get; set; }
    public long OriginalSizeBytes { get; set; }
    public string OriginalSha256 { get; set; } = string.Empty;
    public bool OriginalIsFavorite { get; set; }
    public string OriginalComment { get; set; } = string.Empty;
    public string OriginalCloudState { get; set; } = "NotApplicable";
    public string TargetPlayniteId { get; set; } = string.Empty;
    public string TargetReason { get; set; } = string.Empty;
    public string Confidence { get; set; } = "Low";
    public string ItemState { get; set; } = "Pending";
    public string AppliedArchivePath { get; set; } = string.Empty;
    public DateTime UpdatedUtc { get; set; }
}
