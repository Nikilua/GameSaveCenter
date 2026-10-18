using System.Globalization;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Persistence;

public sealed partial class SqliteStateStore
{
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
            cleanup.CommandText = "DELETE FROM notification_records WHERE created_utc < $cutoff;";
            cleanup.Parameters.AddWithValue("$cutoff", now.AddDays(-NotificationRecordPolicy.RetentionDays).ToString("O", CultureInfo.InvariantCulture));
            await cleanup.ExecuteNonQueryAsync(token).ConfigureAwait(false);

            using var insert = connection.CreateCommand();
            insert.Transaction = transaction;
            insert.CommandText = @"
INSERT OR IGNORE INTO notification_records(
    notification_id,deduplication_key,task_id,game_id,severity,title,summary,created_utc,is_read)
VALUES($id,$deduplicationKey,$taskId,$gameId,$severity,$title,$summary,$created,$isRead);";
            insert.Parameters.AddWithValue("$id", normalized.NotificationId);
            insert.Parameters.AddWithValue("$deduplicationKey", normalized.DeduplicationKey);
            insert.Parameters.AddWithValue("$taskId", normalized.TaskId);
            insert.Parameters.AddWithValue("$gameId", normalized.GameId);
            insert.Parameters.AddWithValue("$severity", (int)normalized.Severity);
            insert.Parameters.AddWithValue("$title", normalized.Title);
            insert.Parameters.AddWithValue("$summary", normalized.Summary);
            insert.Parameters.AddWithValue("$created", normalized.CreatedUtc.ToString("O", CultureInfo.InvariantCulture));
            insert.Parameters.AddWithValue("$isRead", normalized.IsRead ? 1 : 0);
            var inserted = await insert.ExecuteNonQueryAsync(token).ConfigureAwait(false) == 1;

            if (inserted)
            {
                using var trim = connection.CreateCommand();
                trim.Transaction = transaction;
                trim.CommandText = @"
DELETE FROM notification_records
WHERE notification_id IN (
    SELECT notification_id
    FROM notification_records
    ORDER BY created_utc DESC, notification_id DESC
    LIMIT -1 OFFSET $maximum);";
                trim.Parameters.AddWithValue("$maximum", NotificationRecordPolicy.MaximumRecords);
                await trim.ExecuteNonQueryAsync(token).ConfigureAwait(false);
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

    public async Task<List<NotificationRecordDto>> GetRecentNotificationRecordsAsync(int limit, CancellationToken token)
    {
        var result = new List<NotificationRecordDto>();
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT notification_id,deduplication_key,task_id,game_id,severity,title,summary,created_utc,is_read
FROM notification_records
WHERE created_utc >= $cutoff
ORDER BY created_utc DESC, notification_id DESC
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
                TaskId = reader.GetString(2),
                GameId = reader.GetString(3),
                Severity = Enum.IsDefined(typeof(NotificationSeverity), reader.GetInt32(4))
                    ? (NotificationSeverity)reader.GetInt32(4)
                    : NotificationSeverity.Information,
                Title = reader.GetString(5),
                Summary = reader.GetString(6),
                CreatedUtc = DateTime.Parse(reader.GetString(7), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToUniversalTime(),
                IsRead = reader.GetInt32(8) != 0
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
