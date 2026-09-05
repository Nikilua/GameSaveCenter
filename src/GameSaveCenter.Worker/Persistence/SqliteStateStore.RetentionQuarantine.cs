using GameSaveCenter.Contracts;

namespace GameSaveCenter.Worker.Persistence;

public sealed partial class SqliteStateStore
{
    public Task CreateRetentionQuarantineBatchAsync(string batchId, string previewId, CancellationToken token)
        => ExecuteAsync(@"
INSERT INTO retention_quarantine_batches(batch_id,preview_id,created_utc,updated_utc,state,last_error)
VALUES($batch,$preview,$created,$created,'Running',NULL);",
            new Dictionary<string, object?>
            {
                ["$batch"] = batchId,
                ["$preview"] = previewId,
                ["$created"] = DateTime.UtcNow.ToString("O")
            }, token);

    public Task CreateRetentionQuarantineEntryAsync(RetentionQuarantineEntryDto entry, CancellationToken token)
        => ExecuteAsync(@"
INSERT INTO retention_quarantine_entries(entry_id,batch_id,playnite_id,backup_id,original_path,quarantine_path,file_bytes,state,created_utc,updated_utc,last_error)
VALUES($entry,$batch,$game,$backup,$original,$quarantine,$bytes,$state,$created,$updated,$error);",
            new Dictionary<string, object?>
            {
                ["$entry"] = entry.EntryId,
                ["$batch"] = entry.BatchId,
                ["$game"] = entry.PlayniteId,
                ["$backup"] = entry.BackupId,
                ["$original"] = entry.OriginalPath,
                ["$quarantine"] = entry.QuarantinePath,
                ["$bytes"] = entry.FileBytes,
                ["$state"] = entry.State.ToString(),
                ["$created"] = entry.CreatedUtc.ToUniversalTime().ToString("O"),
                ["$updated"] = entry.UpdatedUtc.ToUniversalTime().ToString("O"),
                ["$error"] = string.IsNullOrWhiteSpace(entry.LastError) ? null : entry.LastError
            }, token);

    public Task UpdateRetentionQuarantineEntryAsync(
        string entryId,
        RetentionQuarantineState state,
        string? lastError,
        CancellationToken token)
        => ExecuteAsync(@"
UPDATE retention_quarantine_entries
SET state=$state,updated_utc=$updated,last_error=$error
WHERE entry_id=$entry;",
            new Dictionary<string, object?>
            {
                ["$entry"] = entryId,
                ["$state"] = state.ToString(),
                ["$updated"] = DateTime.UtcNow.ToString("O"),
                ["$error"] = string.IsNullOrWhiteSpace(lastError) ? null : lastError
            }, token);

    public Task UpdateRetentionQuarantineBatchAsync(
        string batchId,
        string state,
        string? lastError,
        CancellationToken token)
        => ExecuteAsync(@"
UPDATE retention_quarantine_batches
SET state=$state,updated_utc=$updated,last_error=$error
WHERE batch_id=$batch;",
            new Dictionary<string, object?>
            {
                ["$batch"] = batchId,
                ["$state"] = state,
                ["$updated"] = DateTime.UtcNow.ToString("O"),
                ["$error"] = string.IsNullOrWhiteSpace(lastError) ? null : lastError
            }, token);

    public async Task<List<RetentionQuarantineEntryDto>> GetRetentionQuarantineEntriesAsync(CancellationToken token)
    {
        var result = new List<RetentionQuarantineEntryDto>();
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT entry_id,batch_id,playnite_id,backup_id,original_path,quarantine_path,file_bytes,state,created_utc,updated_utc,last_error
FROM retention_quarantine_entries
ORDER BY updated_utc DESC;";
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false))
        {
            result.Add(new RetentionQuarantineEntryDto
            {
                EntryId = reader.GetString(0),
                BatchId = reader.GetString(1),
                PlayniteId = reader.GetString(2),
                BackupId = reader.GetString(3),
                OriginalPath = reader.GetString(4),
                QuarantinePath = reader.GetString(5),
                FileBytes = reader.GetInt64(6),
                State = Enum.TryParse<RetentionQuarantineState>(reader.GetString(7), true, out var state)
                    ? state
                    : RetentionQuarantineState.RecoveryRequired,
                CreatedUtc = ParseUtc(reader.GetString(8)),
                UpdatedUtc = ParseUtc(reader.GetString(9)),
                LastError = reader.IsDBNull(10) ? string.Empty : reader.GetString(10)
            });
        }
        return result;
    }

    private static DateTime ParseUtc(string value)
        => DateTime.Parse(value).ToUniversalTime();
}
