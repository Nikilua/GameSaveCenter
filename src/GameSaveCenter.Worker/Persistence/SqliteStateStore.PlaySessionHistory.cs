using System.Text.Json;
using GameSaveCenter.Contracts;
using Microsoft.Data.Sqlite;

namespace GameSaveCenter.Worker.Persistence;

public sealed partial class SqliteStateStore
{
    private static async Task EnsurePlaySessionHistoryIndexesAsync(SqliteConnection connection, CancellationToken token)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"
CREATE INDEX IF NOT EXISTS ix_sessions_game_stop ON sessions(playnite_id,stopped_utc DESC,session_id DESC);
CREATE INDEX IF NOT EXISTS ix_tasks_session_created ON tasks(session_id,created_utc DESC,task_id DESC);";
        await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
    }

    public async Task<PlaySessionHistoryPageDto> GetPlaySessionHistoryAsync(PlaySessionHistoryQueryDto query, CancellationToken token)
    {
        const int pageSize = 24;
        var page = new PlaySessionHistoryPageDto();
        if (string.IsNullOrWhiteSpace(query.PlayniteId)) return page;
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        await using var sessions = connection.CreateCommand();
        var continuation = string.IsNullOrWhiteSpace(query.BeforeStoppedUtcKey)
            ? string.Empty
            : " AND (stopped_utc<$before OR (stopped_utc=$before AND session_id<$beforeId))";
        sessions.CommandText = $@"
SELECT session_id,started_utc,stopped_utc,elapsed_seconds FROM sessions
WHERE playnite_id=$game AND stopped_utc IS NOT NULL{continuation}
ORDER BY stopped_utc DESC,session_id DESC LIMIT $limit;";
        sessions.Parameters.AddWithValue("$game", query.PlayniteId);
        sessions.Parameters.AddWithValue("$limit", pageSize + 1);
        if (continuation.Length != 0)
        {
            sessions.Parameters.AddWithValue("$before", query.BeforeStoppedUtcKey);
            sessions.Parameters.AddWithValue("$beforeId", query.BeforeSessionId);
        }
        await using (var reader = await sessions.ExecuteReaderAsync(token).ConfigureAwait(false))
        {
            while (await reader.ReadAsync(token).ConfigureAwait(false))
            {
                if (page.Items.Count == pageSize)
                {
                    page.HasMore = true;
                    break;
                }
                page.NextStoppedUtcKey = reader.GetString(2);
                page.NextSessionId = reader.GetString(0);
                page.Items.Add(new PlaySessionHistoryEntryDto
                {
                    SessionId = page.NextSessionId,
                    PlayniteId = query.PlayniteId,
                    StartedUtc = DateTime.Parse(reader.GetString(1)).ToUniversalTime(),
                    StoppedUtc = DateTime.Parse(page.NextStoppedUtcKey).ToUniversalTime(),
                    ElapsedSeconds = reader.IsDBNull(3) ? 0 : reader.GetInt64(3)
                });
            }
        }
        if (page.Items.Count == 0) return page;

        var bySession = page.Items.ToDictionary(item => item.SessionId, StringComparer.Ordinal);
        await using var tasks = connection.CreateCommand();
        var ids = page.Items.Select((session, index) => tasks.Parameters.AddWithValue($"$session{index}", session.SessionId).ParameterName);
        tasks.CommandText = $@"
SELECT task_id,request_id,notification_issue_id,session_id,worker_session_id,task_type,game_id,game_name,state,progress,message,stage_message,cancellation_state,created_utc,started_utc,finished_utc,elapsed_seconds,monotonic_started_timestamp,monotonic_frequency,error_code,error_message,restore_report_json,source_references_json,progress_completed_units,progress_total_units,progress_unit,progress_rate,progress_eta_seconds,progress_updated_utc,backup_result_json
FROM tasks WHERE session_id IN ({string.Join(",", ids)}) AND (game_id=$game OR task_type='MediaInbox')
ORDER BY created_utc DESC,task_id DESC;";
        tasks.Parameters.AddWithValue("$game", query.PlayniteId);
        await using var taskReader = await tasks.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await taskReader.ReadAsync(token).ConfigureAwait(false))
        {
            var task = ReadTask(taskReader);
            var outcome = taskReader.GetString(29);
            if (outcome.Length > 0)
            {
                try { task.BackupResult = JsonSerializer.Deserialize<BackupResultDto>(outcome, _json); }
                catch (JsonException) { }
            }
            bySession[task.SessionId].Tasks.Add(task);
        }
        return page;
    }

    public async Task<MediaItemDto> GetSessionMediaAsync(GameMediaIdentityDto query, CancellationToken token)
    {
        var media = await GetMediaByIdAsync(query.MediaId, token).ConfigureAwait(false);
        return media != null && string.Equals(query.PlayniteId, media.PlayniteId, StringComparison.Ordinal)
            && media.ClassificationState == "Assigned" ? media : new MediaItemDto();
    }
}
