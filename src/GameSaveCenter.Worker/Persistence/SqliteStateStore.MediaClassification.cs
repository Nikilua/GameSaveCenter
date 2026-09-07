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

    public async Task<MediaClassificationBatchHistoryPage> GetMediaClassificationBatchHistoryAsync(
        int offset, int limit, string? stateFilter, CancellationToken token)
    {
        var result = new MediaClassificationBatchHistoryPage();
        var state = stateFilter?.Trim() ?? string.Empty;
        var safeOffset = Math.Max(0, offset);
        var safeLimit = Math.Clamp(limit, 1, 100);
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);

        var count = connection.CreateCommand();
        count.CommandText = @"SELECT COUNT(*) FROM media_classification_batches
WHERE ($state='' OR state=$state);";
        count.Parameters.AddWithValue("$state", state);
        result.TotalCount = Convert.ToInt32(await count.ExecuteScalarAsync(token).ConfigureAwait(false));

        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT b.batch_id,b.state,b.created_utc,b.updated_utc,b.expires_utc,b.last_error,
       COUNT(i.media_id),
       COALESCE(SUM(CASE WHEN i.item_state='Applied' THEN 1 ELSE 0 END),0),
       COALESCE(SUM(CASE WHEN i.item_state='Undone' THEN 1 ELSE 0 END),0),
       COALESCE(SUM(CASE WHEN i.item_state='Conflict' THEN 1 ELSE 0 END),0),
       COALESCE(SUM(CASE WHEN i.item_state='Skipped' THEN 1 ELSE 0 END),0)
FROM media_classification_batches b
LEFT JOIN media_classification_batch_items i ON i.batch_id=b.batch_id
WHERE ($state='' OR b.state=$state)
GROUP BY b.batch_id,b.state,b.created_utc,b.updated_utc,b.expires_utc,b.last_error
ORDER BY b.updated_utc DESC,b.batch_id DESC
LIMIT $limit OFFSET $offset;";
        command.Parameters.AddWithValue("$state", state);
        command.Parameters.AddWithValue("$limit", safeLimit);
        command.Parameters.AddWithValue("$offset", safeOffset);
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false))
        {
            result.Items.Add(new MediaClassificationBatchRecord
            {
                BatchId = reader.GetString(0),
                State = reader.GetString(1),
                CreatedUtc = DateTime.Parse(reader.GetString(2)).ToUniversalTime(),
                UpdatedUtc = DateTime.Parse(reader.GetString(3)).ToUniversalTime(),
                ExpiresUtc = DateTime.Parse(reader.GetString(4)).ToUniversalTime(),
                LastError = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                ItemCount = Convert.ToInt32(reader.GetInt64(6)),
                AppliedCount = Convert.ToInt32(reader.GetInt64(7)),
                UndoneCount = Convert.ToInt32(reader.GetInt64(8)),
                ConflictCount = Convert.ToInt32(reader.GetInt64(9)),
                SkippedCount = Convert.ToInt32(reader.GetInt64(10))
            });
        }
        return result;
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

    public async Task CreateMediaClassificationOperationAsync(MediaClassificationOperationRecord operation, CancellationToken token)
    {
        await _writeGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await using var connection = Open();
            await connection.OpenAsync(token).ConfigureAwait(false);
            var command = connection.CreateCommand();
            command.CommandText = @"
INSERT INTO media_classification_operations(
 operation_id,batch_id,media_id,operation_kind,state,source_path,destination_path,expected_sha256,
 source_is_original,destination_preexisted,created_utc,updated_utc,last_error)
VALUES($operation,$batch,$media,$kind,'Planned',$source,$destination,$sha,$source_original,$destination_exists,$created,$updated,'');";
            command.Parameters.AddWithValue("$operation", operation.OperationId);
            command.Parameters.AddWithValue("$batch", operation.BatchId);
            command.Parameters.AddWithValue("$media", operation.MediaId);
            command.Parameters.AddWithValue("$kind", operation.OperationKind);
            command.Parameters.AddWithValue("$source", operation.SourcePath);
            command.Parameters.AddWithValue("$destination", operation.DestinationPath);
            command.Parameters.AddWithValue("$sha", operation.ExpectedSha256);
            command.Parameters.AddWithValue("$source_original", operation.SourceIsOriginal ? 1 : 0);
            command.Parameters.AddWithValue("$destination_exists", operation.DestinationPreexisted ? 1 : 0);
            command.Parameters.AddWithValue("$created", operation.CreatedUtc.ToUniversalTime().ToString("O"));
            command.Parameters.AddWithValue("$updated", operation.CreatedUtc.ToUniversalTime().ToString("O"));
            await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        }
        finally
        {
            _writeGate.Release();
        }
    }

    public Task UpdateMediaClassificationOperationAsync(string operationId, string state, string error, CancellationToken token)
        => ExecuteAsync(@"UPDATE media_classification_operations
SET state=$state,updated_utc=$updated,last_error=$error WHERE operation_id=$operation;",
            new Dictionary<string, object?>
            {
                ["$operation"] = operationId, ["$state"] = state,
                ["$updated"] = DateTime.UtcNow.ToString("O"), ["$error"] = error
            }, token);

    public async Task<List<MediaClassificationOperationRecord>> GetPendingMediaClassificationOperationsAsync(CancellationToken token)
    {
        var result = new List<MediaClassificationOperationRecord>();
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT operation_id,batch_id,media_id,operation_kind,state,source_path,destination_path,expected_sha256,
       source_is_original,destination_preexisted,created_utc,updated_utc,last_error
FROM media_classification_operations
WHERE state IN ('Planned','Moved','RecoveryRequired')
ORDER BY created_utc,operation_id;";
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false))
        {
            result.Add(new MediaClassificationOperationRecord
            {
                OperationId = reader.GetString(0), BatchId = reader.GetString(1), MediaId = reader.GetString(2),
                OperationKind = reader.GetString(3), State = reader.GetString(4), SourcePath = reader.GetString(5),
                DestinationPath = reader.GetString(6), ExpectedSha256 = reader.GetString(7), SourceIsOriginal = reader.GetInt32(8) == 1,
                DestinationPreexisted = reader.GetInt32(9) == 1, CreatedUtc = DateTime.Parse(reader.GetString(10)).ToUniversalTime(),
                UpdatedUtc = DateTime.Parse(reader.GetString(11)).ToUniversalTime(), LastError = reader.IsDBNull(12) ? string.Empty : reader.GetString(12)
            });
        }
        return result;
    }

    public Task<bool> TryCommitMediaClassificationApplyAsync(string operationId, MediaClassificationBatchItemRecord item,
        string appliedArchivePath, CancellationToken token)
        => TryCommitMediaClassificationAsync(operationId, item, appliedArchivePath, appliedArchivePath, "Applied", "item_state NOT IN ('Applied','Undone')", @"
UPDATE media SET playnite_id=$target,archive_path=$applied,classification_state='Assigned',
    classification_reason=$target_reason,cloud_state='Pending'
WHERE media_id=$media AND COALESCE(playnite_id,'')=$original_game
  AND COALESCE(classification_state,'')=$original_state
  AND COALESCE(classification_reason,'')=$original_reason
  AND archive_path=$original_archive AND original_path=$original_path
  AND captured_utc=$captured AND size_bytes=$size AND sha256=$sha
  AND is_favorite=$favorite AND COALESCE(comment,'')=$comment
  AND COALESCE(cloud_state,'')=$original_cloud;", token);

    public Task<bool> TryCommitMediaClassificationUndoAsync(string operationId, MediaClassificationBatchItemRecord item,
        CancellationToken token)
        => TryCommitMediaClassificationAsync(operationId, item, item.AppliedArchivePath, string.Empty, "Undone", "item_state='Applied'", @"
UPDATE media SET playnite_id=$original_game,archive_path=$original_archive,
    classification_state=$original_state,classification_reason=$original_reason,cloud_state=$original_cloud
WHERE media_id=$media AND COALESCE(playnite_id,'')=$target
  AND classification_state='Assigned' AND archive_path=$applied
  AND original_path=$original_path AND captured_utc=$captured AND size_bytes=$size AND sha256=$sha
  AND is_favorite=$favorite AND COALESCE(comment,'')=$comment AND cloud_state='Pending';", token);

    private async Task<bool> TryCommitMediaClassificationAsync(string operationId, MediaClassificationBatchItemRecord item,
        string mediaPathValue, string batchAppliedArchivePath, string itemState, string batchItemCondition, string mediaSql, CancellationToken token)
    {
        await _writeGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await using var connection = Open();
            await connection.OpenAsync(token).ConfigureAwait(false);
            await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(CancellationToken.None).ConfigureAwait(false);
            try
            {
                var media = connection.CreateCommand();
                media.Transaction = transaction;
                media.CommandText = mediaSql;
                AddClassificationParameters(media, item, mediaPathValue);
                if (await media.ExecuteNonQueryAsync(CancellationToken.None).ConfigureAwait(false) != 1)
                {
                    await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
                    return false;
                }

                var batchItem = connection.CreateCommand();
                batchItem.Transaction = transaction;
                batchItem.CommandText = $@"UPDATE media_classification_batch_items
SET item_state=$item_state,applied_archive_path=$applied,updated_utc=$updated
WHERE batch_id=$batch AND media_id=$media AND {batchItemCondition};";
                batchItem.Parameters.AddWithValue("$item_state", itemState);
                batchItem.Parameters.AddWithValue("$applied", batchAppliedArchivePath);
                batchItem.Parameters.AddWithValue("$updated", DateTime.UtcNow.ToString("O"));
                batchItem.Parameters.AddWithValue("$batch", item.BatchId);
                batchItem.Parameters.AddWithValue("$media", item.MediaId);
                if (await batchItem.ExecuteNonQueryAsync(CancellationToken.None).ConfigureAwait(false) != 1)
                {
                    await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
                    return false;
                }

                var operation = connection.CreateCommand();
                operation.Transaction = transaction;
                operation.CommandText = @"UPDATE media_classification_operations
SET state='Committed',updated_utc=$updated,last_error=''
WHERE operation_id=$operation AND state='Moved';";
                operation.Parameters.AddWithValue("$updated", DateTime.UtcNow.ToString("O"));
                operation.Parameters.AddWithValue("$operation", operationId);
                if (await operation.ExecuteNonQueryAsync(CancellationToken.None).ConfigureAwait(false) != 1)
                {
                    await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
                    return false;
                }

                await transaction.CommitAsync(CancellationToken.None).ConfigureAwait(false);
                return true;
            }
            catch
            {
                try { await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false); } catch { }
                throw;
            }
        }
        finally
        {
            _writeGate.Release();
        }
    }

    private static void AddClassificationParameters(SqliteCommand command, MediaClassificationBatchItemRecord item, string appliedArchivePath)
    {
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
        command.Parameters.AddWithValue("$applied", appliedArchivePath);
    }

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
    public int ItemCount { get; set; }
    public int AppliedCount { get; set; }
    public int UndoneCount { get; set; }
    public int ConflictCount { get; set; }
    public int SkippedCount { get; set; }
}

public sealed class MediaClassificationBatchHistoryPage
{
    public int TotalCount { get; set; }
    public List<MediaClassificationBatchRecord> Items { get; } = new List<MediaClassificationBatchRecord>();
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

public sealed class MediaClassificationOperationRecord
{
    public string OperationId { get; set; } = string.Empty;
    public string BatchId { get; set; } = string.Empty;
    public string MediaId { get; set; } = string.Empty;
    public string OperationKind { get; set; } = string.Empty;
    public string State { get; set; } = "Planned";
    public string SourcePath { get; set; } = string.Empty;
    public string DestinationPath { get; set; } = string.Empty;
    public string ExpectedSha256 { get; set; } = string.Empty;
    public bool SourceIsOriginal { get; set; }
    public bool DestinationPreexisted { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
    public string LastError { get; set; } = string.Empty;
}
