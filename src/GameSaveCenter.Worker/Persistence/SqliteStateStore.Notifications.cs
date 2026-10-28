using System.Globalization;
using System.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Persistence;

public sealed partial class SqliteStateStore
{
    private async Task MigrateLegacyTaskNotificationIssuesAsync(SqliteConnection connection, CancellationToken token)
    {
        var legacy = new List<(string NotificationId, string TaskId, string GameId, string TaskType, string CreatedUtc, string Summary, bool IsRead)>();
        using (var query = connection.CreateCommand())
        {
            query.CommandText = @"
SELECT n.notification_id,n.task_id,n.game_id,t.task_type,n.created_utc,n.summary,n.is_read
FROM notification_records n
INNER JOIN tasks t ON t.task_id=n.task_id
WHERE n.issue_key='' AND n.severity=$error AND n.title='后台任务失败' AND t.state=$failed
ORDER BY n.task_id,n.created_utc,n.notification_id;";
            query.Parameters.AddWithValue("$error", (int)NotificationSeverity.Error);
            query.Parameters.AddWithValue("$failed", (int)TaskState.Failed);
            await using var reader = await query.ExecuteReaderAsync(token).ConfigureAwait(false);
            while (await reader.ReadAsync(token).ConfigureAwait(false))
            {
                legacy.Add((
                    reader.GetString(0),
                    reader.GetString(1),
                    reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    reader.GetString(4),
                    reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    reader.GetInt32(6) != 0));
            }
        }

        if (legacy.Count == 0) return;
        using var transaction = connection.BeginTransaction();
        foreach (var taskEvents in legacy.GroupBy(item => item.TaskId, StringComparer.Ordinal))
        {
            var events = taskEvents.ToList();
            var first = events[0];
            var latest = events[events.Count - 1];
            var issueKey = NotificationRecordPolicy.CreateTaskIssueKey(first.TaskType, first.GameId, first.TaskId);
            using (var update = connection.CreateCommand())
            {
                update.Transaction = transaction;
                update.CommandText = @"
UPDATE notification_records
SET deduplication_key=$issue,issue_key=$issue,task_id=$task,first_task_id=$task,game_id=$game,
    created_utc=$first,occurrence_count=1,first_occurred_utc=$first,last_occurred_utc=$last,
    last_error=$error,summary=$summary,is_resolved=0,is_read=$isRead
WHERE notification_id=$notification;";
                update.Parameters.AddWithValue("$issue", issueKey);
                update.Parameters.AddWithValue("$task", first.TaskId);
                update.Parameters.AddWithValue("$game", first.GameId);
                update.Parameters.AddWithValue("$first", first.CreatedUtc);
                update.Parameters.AddWithValue("$last", latest.CreatedUtc);
                update.Parameters.AddWithValue("$error", latest.Summary);
                update.Parameters.AddWithValue("$summary", latest.Summary);
                update.Parameters.AddWithValue("$isRead", latest.IsRead ? 1 : 0);
                update.Parameters.AddWithValue("$notification", first.NotificationId);
                await update.ExecuteNonQueryAsync(token).ConfigureAwait(false);
            }

            if (events.Count > 1)
            {
                using var removeDuplicates = connection.CreateCommand();
                removeDuplicates.Transaction = transaction;
                var duplicateIds = events.Skip(1).Select((item, index) =>
                {
                    var parameter = "$duplicate" + index.ToString(CultureInfo.InvariantCulture);
                    removeDuplicates.Parameters.AddWithValue(parameter, item.NotificationId);
                    return parameter;
                }).ToList();
                removeDuplicates.CommandText = "DELETE FROM notification_records WHERE notification_id IN (" + string.Join(",", duplicateIds) + ");";
                await removeDuplicates.ExecuteNonQueryAsync(token).ConfigureAwait(false);
            }

            using var claimEvent = connection.CreateCommand();
            claimEvent.Transaction = transaction;
            claimEvent.CommandText = @"
INSERT OR IGNORE INTO notification_issue_events(event_key,issue_key,task_id,occurred_utc)
VALUES($event,$issue,$task,$occurred);";
            claimEvent.Parameters.AddWithValue("$event", NotificationRecordPolicy.CreateTaskOccurrenceKey(first.TaskId, failure: true));
            claimEvent.Parameters.AddWithValue("$issue", issueKey);
            claimEvent.Parameters.AddWithValue("$task", first.TaskId);
            claimEvent.Parameters.AddWithValue("$occurred", latest.CreatedUtc);
            await claimEvent.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        }

        transaction.Commit();
    }

    public async Task<bool> TryAddNotificationAsync(NotificationRecordDto notification, CancellationToken token)
    {
        var normalized = NotificationRecordPolicy.Normalize(notification);
        if (normalized == null || NotificationRecordPolicy.IsExpired(normalized)) return false;

        await _writeGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await using var connection = Open();
            await connection.OpenAsync(token).ConfigureAwait(false);
            using var transaction = connection.BeginTransaction();

            var now = DateTime.UtcNow;
            using var cleanup = connection.CreateCommand();
            cleanup.Transaction = transaction;
            cleanup.CommandText = @"
DELETE FROM notification_records
WHERE COALESCE(NULLIF(last_occurred_utc,''),created_utc) < $cutoff;
DELETE FROM notification_issue_events WHERE occurred_utc < $cutoff;";
            cleanup.Parameters.AddWithValue("$cutoff", now.AddDays(-NotificationRecordPolicy.RetentionDays).ToString("O", CultureInfo.InvariantCulture));
            await cleanup.ExecuteNonQueryAsync(token).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(normalized.IssueKey))
            {
                var eventKey = string.IsNullOrWhiteSpace(normalized.OccurrenceKey)
                    ? normalized.DeduplicationKey
                    : normalized.OccurrenceKey;
                using var claimEvent = connection.CreateCommand();
                claimEvent.Transaction = transaction;
                claimEvent.CommandText = @"
INSERT OR IGNORE INTO notification_issue_events(event_key,issue_key,task_id,occurred_utc)
VALUES($event,$issue,$task,$occurred);";
                claimEvent.Parameters.AddWithValue("$event", eventKey);
                claimEvent.Parameters.AddWithValue("$issue", normalized.IssueKey);
                claimEvent.Parameters.AddWithValue("$task", normalized.TaskId);
                claimEvent.Parameters.AddWithValue("$occurred", normalized.LastOccurredUtc.ToString("O", CultureInfo.InvariantCulture));
                var newEvent = await claimEvent.ExecuteNonQueryAsync(token).ConfigureAwait(false) == 1;
                if (!newEvent)
                {
                    transaction.Commit();
                    return false;
                }

                using var updateIssue = connection.CreateCommand();
                updateIssue.Transaction = transaction;
                if (normalized.IsResolved)
                {
                    updateIssue.CommandText = @"
UPDATE notification_records
SET task_id=$task,source=CASE WHEN $source<>'' THEN $source ELSE source END,severity=$severity,title=$title,summary=$summary,
    last_occurred_utc=$lastOccurred,is_resolved=1,is_read=0
WHERE issue_key=$issue AND last_occurred_utc<=$lastOccurred;";
                }
                else
                {
                    updateIssue.CommandText = @"
UPDATE notification_records
SET first_task_id=CASE WHEN COALESCE(NULLIF(first_occurred_utc,''),created_utc)>$firstOccurred THEN $firstTask ELSE first_task_id END,
    created_utc=CASE WHEN created_utc>$firstOccurred THEN $firstOccurred ELSE created_utc END,
    first_occurred_utc=CASE WHEN COALESCE(NULLIF(first_occurred_utc,''),created_utc)>$firstOccurred THEN $firstOccurred ELSE first_occurred_utc END,
    task_id=CASE WHEN last_occurred_utc<=$lastOccurred THEN $task ELSE task_id END,
    severity=CASE WHEN last_occurred_utc<=$lastOccurred THEN $severity ELSE severity END,
    source=CASE WHEN last_occurred_utc<=$lastOccurred AND $source<>'' THEN $source ELSE source END,
    title=CASE WHEN last_occurred_utc<=$lastOccurred THEN $title ELSE title END,
    summary=CASE WHEN last_occurred_utc<=$lastOccurred THEN $summary ELSE summary END,
    occurrence_count=occurrence_count+1,
    last_occurred_utc=CASE WHEN last_occurred_utc<=$lastOccurred THEN $lastOccurred ELSE last_occurred_utc END,
    last_error=CASE WHEN last_occurred_utc<=$lastOccurred THEN $lastError ELSE last_error END,
    is_resolved=CASE WHEN last_occurred_utc<=$lastOccurred THEN 0 ELSE is_resolved END,
    is_read=CASE WHEN last_occurred_utc<=$lastOccurred THEN 0 ELSE is_read END
WHERE issue_key=$issue;";
                }
                updateIssue.Parameters.AddWithValue("$firstTask", string.IsNullOrWhiteSpace(normalized.FirstTaskId) ? normalized.TaskId : normalized.FirstTaskId);
                updateIssue.Parameters.AddWithValue("$firstOccurred", normalized.FirstOccurredUtc.ToString("O", CultureInfo.InvariantCulture));
                updateIssue.Parameters.AddWithValue("$task", normalized.TaskId);
                updateIssue.Parameters.AddWithValue("$source", normalized.Source);
                updateIssue.Parameters.AddWithValue("$severity", (int)normalized.Severity);
                updateIssue.Parameters.AddWithValue("$title", normalized.Title);
                updateIssue.Parameters.AddWithValue("$summary", normalized.Summary);
                updateIssue.Parameters.AddWithValue("$lastOccurred", normalized.LastOccurredUtc.ToString("O", CultureInfo.InvariantCulture));
                updateIssue.Parameters.AddWithValue("$lastError", normalized.LastError);
                updateIssue.Parameters.AddWithValue("$issue", normalized.IssueKey);
                var updated = await updateIssue.ExecuteNonQueryAsync(token).ConfigureAwait(false) == 1;
                if (!updated && !normalized.IsResolved)
                {
                    using var insertIssue = connection.CreateCommand();
                    insertIssue.Transaction = transaction;
                    insertIssue.CommandText = @"
INSERT OR IGNORE INTO notification_records(
    notification_id,deduplication_key,issue_key,source,task_id,first_task_id,game_id,severity,title,summary,created_utc,
    occurrence_count,first_occurred_utc,last_occurred_utc,last_error,is_resolved,is_read)
VALUES($id,$dedup,$issue,$source,$task,$firstTask,$game,$severity,$title,$summary,$created,
    1,$firstOccurred,$lastOccurred,$lastError,0,$isRead);";
                    insertIssue.Parameters.AddWithValue("$id", normalized.NotificationId);
                    insertIssue.Parameters.AddWithValue("$dedup", normalized.DeduplicationKey);
                    insertIssue.Parameters.AddWithValue("$issue", normalized.IssueKey);
                    insertIssue.Parameters.AddWithValue("$source", normalized.Source);
                    insertIssue.Parameters.AddWithValue("$task", normalized.TaskId);
                    insertIssue.Parameters.AddWithValue("$firstTask", string.IsNullOrWhiteSpace(normalized.FirstTaskId) ? normalized.TaskId : normalized.FirstTaskId);
                    insertIssue.Parameters.AddWithValue("$game", normalized.GameId);
                    insertIssue.Parameters.AddWithValue("$severity", (int)normalized.Severity);
                    insertIssue.Parameters.AddWithValue("$title", normalized.Title);
                    insertIssue.Parameters.AddWithValue("$summary", normalized.Summary);
                    insertIssue.Parameters.AddWithValue("$created", normalized.FirstOccurredUtc.ToString("O", CultureInfo.InvariantCulture));
                    insertIssue.Parameters.AddWithValue("$firstOccurred", normalized.FirstOccurredUtc.ToString("O", CultureInfo.InvariantCulture));
                    insertIssue.Parameters.AddWithValue("$lastOccurred", normalized.LastOccurredUtc.ToString("O", CultureInfo.InvariantCulture));
                    insertIssue.Parameters.AddWithValue("$lastError", normalized.LastError);
                    insertIssue.Parameters.AddWithValue("$isRead", normalized.IsRead ? 1 : 0);
                    await insertIssue.ExecuteNonQueryAsync(token).ConfigureAwait(false);
                }

                if (normalized.IsResolved && !updated)
                {
                    using var issueExists = connection.CreateCommand();
                    issueExists.Transaction = transaction;
                    issueExists.CommandText = "SELECT COUNT(*) FROM notification_records WHERE issue_key=$issue;";
                    issueExists.Parameters.AddWithValue("$issue", normalized.IssueKey);
                    if (Convert.ToInt32(await issueExists.ExecuteScalarAsync(token).ConfigureAwait(false), CultureInfo.InvariantCulture) == 0)
                    {
                        await InsertStandaloneNotificationAsync(connection, transaction, normalized,
                            string.IsNullOrWhiteSpace(normalized.OccurrenceKey) ? normalized.DeduplicationKey : normalized.OccurrenceKey,
                            token).ConfigureAwait(false);
                    }
                }

                await TrimNotificationsAsync(connection, transaction, token).ConfigureAwait(false);
                transaction.Commit();
                return updated || !normalized.IsResolved;
            }

            using var insert = connection.CreateCommand();
            insert.Transaction = transaction;
            insert.CommandText = @"
INSERT OR IGNORE INTO notification_records(
    notification_id,deduplication_key,issue_key,source,task_id,first_task_id,game_id,severity,title,summary,created_utc,
    occurrence_count,first_occurred_utc,last_occurred_utc,last_error,is_resolved,is_read)
VALUES($id,$deduplicationKey,'',$source,$taskId,$taskId,$gameId,$severity,$title,$summary,$created,
    1,$created,$created,'',0,$isRead);";
            insert.Parameters.AddWithValue("$id", normalized.NotificationId);
            insert.Parameters.AddWithValue("$deduplicationKey", normalized.DeduplicationKey);
            insert.Parameters.AddWithValue("$taskId", normalized.TaskId);
            insert.Parameters.AddWithValue("$source", normalized.Source);
            insert.Parameters.AddWithValue("$gameId", normalized.GameId);
            insert.Parameters.AddWithValue("$severity", (int)normalized.Severity);
            insert.Parameters.AddWithValue("$title", normalized.Title);
            insert.Parameters.AddWithValue("$summary", normalized.Summary);
            insert.Parameters.AddWithValue("$created", normalized.CreatedUtc.ToString("O", CultureInfo.InvariantCulture));
            insert.Parameters.AddWithValue("$isRead", normalized.IsRead ? 1 : 0);
            var inserted = await insert.ExecuteNonQueryAsync(token).ConfigureAwait(false) == 1;

            if (inserted)
            {
                await TrimNotificationsAsync(connection, transaction, token).ConfigureAwait(false);
            }

            transaction.Commit();
            return inserted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQLite notification persistence failed");
            throw;
        }
        finally
        {
            _writeGate.Release();
        }
    }

    private static async Task InsertStandaloneNotificationAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        NotificationRecordDto notification,
        string deduplicationKey,
        CancellationToken token)
    {
        using var insert = connection.CreateCommand();
        insert.Transaction = transaction;
        insert.CommandText = @"
INSERT OR IGNORE INTO notification_records(
    notification_id,deduplication_key,issue_key,source,task_id,first_task_id,game_id,severity,title,summary,created_utc,
    occurrence_count,first_occurred_utc,last_occurred_utc,last_error,is_resolved,is_read)
VALUES($id,$dedup,'',$source,$task,$task,$game,$severity,$title,$summary,$created,1,$created,$created,'',0,$isRead);";
        insert.Parameters.AddWithValue("$id", notification.NotificationId);
        insert.Parameters.AddWithValue("$dedup", deduplicationKey);
        insert.Parameters.AddWithValue("$task", notification.TaskId);
        insert.Parameters.AddWithValue("$source", notification.Source);
        insert.Parameters.AddWithValue("$game", notification.GameId);
        insert.Parameters.AddWithValue("$severity", (int)notification.Severity);
        insert.Parameters.AddWithValue("$title", notification.Title);
        insert.Parameters.AddWithValue("$summary", notification.Summary);
        insert.Parameters.AddWithValue("$created", notification.LastOccurredUtc.ToString("O", CultureInfo.InvariantCulture));
        insert.Parameters.AddWithValue("$isRead", notification.IsRead ? 1 : 0);
        await insert.ExecuteNonQueryAsync(token).ConfigureAwait(false);
    }

    private static async Task TrimNotificationsAsync(SqliteConnection connection, SqliteTransaction transaction, CancellationToken token)
    {
        using var trim = connection.CreateCommand();
        trim.Transaction = transaction;
        trim.CommandText = @"
DELETE FROM notification_records
WHERE notification_id IN (
    SELECT notification_id FROM notification_records
    ORDER BY COALESCE(NULLIF(last_occurred_utc,''),created_utc) DESC, notification_id DESC
    LIMIT -1 OFFSET $maximum);
DELETE FROM notification_issue_events
WHERE event_key IN (
    SELECT event_key FROM notification_issue_events
    ORDER BY occurred_utc DESC,event_key DESC
    LIMIT -1 OFFSET $maximumEvents);";
        trim.Parameters.AddWithValue("$maximum", NotificationRecordPolicy.MaximumRecords);
        trim.Parameters.AddWithValue("$maximumEvents", NotificationRecordPolicy.MaximumOccurrenceEvents);
        await trim.ExecuteNonQueryAsync(token).ConfigureAwait(false);
    }

    public async Task<List<NotificationRecordDto>> GetRecentNotificationRecordsAsync(int limit, CancellationToken token)
    {
        var result = new List<NotificationRecordDto>();
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT notification_id,deduplication_key,issue_key,task_id,first_task_id,game_id,severity,title,summary,created_utc,occurrence_count,
       COALESCE(NULLIF(first_occurred_utc,''),created_utc),COALESCE(NULLIF(last_occurred_utc,''),created_utc),last_error,is_resolved,is_read,source
FROM notification_records
WHERE COALESCE(NULLIF(last_occurred_utc,''),created_utc) >= $cutoff
ORDER BY COALESCE(NULLIF(last_occurred_utc,''),created_utc) DESC, notification_id DESC
LIMIT $limit;";
        command.Parameters.AddWithValue("$cutoff", DateTime.UtcNow.AddDays(-NotificationRecordPolicy.RetentionDays).ToString("O", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$limit", Math.Clamp(limit, 1, NotificationRecordPolicy.MaximumPageSize));
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false))
        {
            result.Add(new NotificationRecordDto
            {
                NotificationId = reader.GetString(0),
                DeduplicationKey = reader.GetString(1),
                IssueKey = reader.GetString(2),
                TaskId = reader.GetString(3),
                FirstTaskId = reader.GetString(4),
                GameId = reader.GetString(5),
                Severity = Enum.IsDefined(typeof(NotificationSeverity), reader.GetInt32(6))
                    ? (NotificationSeverity)reader.GetInt32(6)
                    : NotificationSeverity.Information,
                Title = reader.GetString(7),
                Summary = reader.GetString(8),
                CreatedUtc = DateTime.Parse(reader.GetString(9), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToUniversalTime(),
                OccurrenceCount = reader.GetInt32(10),
                FirstOccurredUtc = DateTime.Parse(reader.GetString(11), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToUniversalTime(),
                LastOccurredUtc = DateTime.Parse(reader.GetString(12), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToUniversalTime(),
                LastError = reader.GetString(13),
                IsResolved = reader.GetInt32(14) != 0,
                IsRead = reader.GetInt32(15) != 0,
                Source = reader.GetString(16)
            });
        }

        return result;
    }

    public async Task<int> MarkNotificationRecordsReadAsync(IEnumerable<string?> notificationIds, CancellationToken token)
    {
        var ids = NotificationRecordPolicy.NormalizeNotificationIds(notificationIds);
        if (ids.Count == 0) return 0;

        await _writeGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await using var connection = Open();
            await connection.OpenAsync(token).ConfigureAwait(false);
            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            command.Transaction = transaction;

            var parameters = new List<string>(ids.Count);
            for (var index = 0; index < ids.Count; index++)
            {
                var parameter = "$id" + index.ToString(CultureInfo.InvariantCulture);
                parameters.Add(parameter);
                command.Parameters.AddWithValue(parameter, ids[index]);
            }

            command.CommandText = $@"
UPDATE notification_records
SET is_read = 1
WHERE is_read = 0 AND notification_id IN ({string.Join(",", parameters)});";
            var updated = await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
            transaction.Commit();
            return updated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQLite notification read update failed");
            throw;
        }
        finally
        {
            _writeGate.Release();
        }
    }

    public async Task<int> ClearReadNotificationRecordsAsync(CancellationToken token)
    {
        await _writeGate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await using var connection = Open();
            await connection.OpenAsync(token).ConfigureAwait(false);
            using var transaction = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "DELETE FROM notification_records WHERE is_read = 1;";
            var deleted = await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
            transaction.Commit();
            return deleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQLite read notification cleanup failed");
            throw;
        }
        finally
        {
            _writeGate.Release();
        }
    }
}
