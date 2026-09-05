using System.Text;
using GameSaveCenter.Contracts;
using Microsoft.Data.Sqlite;

namespace GameSaveCenter.Worker.Persistence;

public sealed partial class SqliteStateStore
{
    public async Task<TaskPageDto> GetTaskPageAsync(TaskQueryDto? query, CancellationToken token)
    {
        query ??= new TaskQueryDto();
        var limit = Math.Clamp(query.Limit, 1, 500);
        var summary = await GetTaskSummaryAsync(query, token).ConfigureAwait(false);
        var filter = BuildTaskFilter(query, includeCursor: true);
        var result = new TaskPageDto { TotalCount = summary.TotalCount, Summary = summary };

        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = $@"
SELECT task_id,request_id,session_id,worker_session_id,task_type,game_id,game_name,state,progress,message,created_utc,started_utc,finished_utc,error_code,error_message
FROM tasks
WHERE {filter.Sql}
ORDER BY created_utc DESC,task_id DESC
LIMIT $limit;";
        AddParameters(command, filter.Parameters);
        command.Parameters.AddWithValue("$limit", limit + 1);
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false)) result.Items.Add(ReadTask(reader));

        result.HasMore = result.Items.Count > limit;
        if (result.HasMore) result.Items.RemoveAt(result.Items.Count - 1);
        if (result.HasMore && result.Items.Count > 0)
        {
            var last = result.Items[result.Items.Count - 1];
            result.NextCursor = EncodeTaskCursor(last.CreatedUtc, last.TaskId);
        }
        return result;
    }

    public async Task<TaskSummaryDto> GetTaskSummaryAsync(TaskQueryDto? query, CancellationToken token)
    {
        query ??= new TaskQueryDto();
        var filter = BuildTaskFilter(query, includeCursor: false);
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = $@"
SELECT COUNT(*),
       COALESCE(SUM(CASE WHEN state=$queued THEN 1 ELSE 0 END),0),
       COALESCE(SUM(CASE WHEN state=$running THEN 1 ELSE 0 END),0),
       COALESCE(SUM(CASE WHEN state=$waiting THEN 1 ELSE 0 END),0),
       COALESCE(SUM(CASE WHEN state=$succeeded THEN 1 ELSE 0 END),0),
       COALESCE(SUM(CASE WHEN state=$failed THEN 1 ELSE 0 END),0),
       COALESCE(SUM(CASE WHEN state=$cancelled THEN 1 ELSE 0 END),0),
       COALESCE(SUM(CASE WHEN state IN ($queued,$running) AND task_type LIKE '%Cloud%' THEN 1 ELSE 0 END),0)
FROM tasks
WHERE {filter.Sql};";
        AddParameters(command, filter.Parameters);
        command.Parameters.AddWithValue("$queued", (int)TaskState.Queued);
        command.Parameters.AddWithValue("$running", (int)TaskState.Running);
        command.Parameters.AddWithValue("$waiting", (int)TaskState.WaitingForUser);
        command.Parameters.AddWithValue("$succeeded", (int)TaskState.Succeeded);
        command.Parameters.AddWithValue("$failed", (int)TaskState.Failed);
        command.Parameters.AddWithValue("$cancelled", (int)TaskState.Cancelled);
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        if (!await reader.ReadAsync(token).ConfigureAwait(false)) return new TaskSummaryDto();
        return new TaskSummaryDto
        {
            TotalCount = Convert.ToInt32(reader.GetValue(0)),
            QueuedCount = Convert.ToInt32(reader.GetValue(1)),
            RunningCount = Convert.ToInt32(reader.GetValue(2)),
            WaitingForUserCount = Convert.ToInt32(reader.GetValue(3)),
            SucceededCount = Convert.ToInt32(reader.GetValue(4)),
            FailedCount = Convert.ToInt32(reader.GetValue(5)),
            CancelledCount = Convert.ToInt32(reader.GetValue(6)),
            PendingCloudCount = Convert.ToInt32(reader.GetValue(7))
        };
    }

    /// <summary>Counts successful tasks by their finish instant using a local-day UTC half-open range.</summary>
    public async Task<int> GetSucceededTaskCountAsync(DateTime startUtc, DateTime endUtc, CancellationToken token)
    {
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT COUNT(*) FROM tasks
WHERE state=$state AND finished_utc IS NOT NULL
  AND finished_utc >= $start AND finished_utc < $end;";
        command.Parameters.AddWithValue("$state", (int)TaskState.Succeeded);
        command.Parameters.AddWithValue("$start", startUtc.ToUniversalTime().ToString("O"));
        command.Parameters.AddWithValue("$end", endUtc.ToUniversalTime().ToString("O"));
        return Convert.ToInt32(await command.ExecuteScalarAsync(token).ConfigureAwait(false));
    }

    private static (string Sql, Dictionary<string, object?> Parameters) BuildTaskFilter(TaskQueryDto query, bool includeCursor)
    {
        var predicates = new List<string> { "1=1" };
        var parameters = new Dictionary<string, object?>();
        if (query.State.HasValue)
        {
            predicates.Add("state=$stateFilter");
            parameters["$stateFilter"] = (int)query.State.Value;
        }
        else if (query.States != null && query.States.Count > 0)
        {
            var stateParameters = query.States.Distinct().Select((state, index) =>
            {
                var name = "$stateFilter" + index;
                parameters[name] = (int)state;
                return name;
            });
            predicates.Add("state IN (" + string.Join(",", stateParameters) + ")");
        }
        if (!string.IsNullOrWhiteSpace(query.GameId))
        {
            predicates.Add("game_id=$gameFilter");
            parameters["$gameFilter"] = query.GameId.Trim();
        }
        if (!string.IsNullOrWhiteSpace(query.GameName))
        {
            predicates.Add("game_name=$gameNameFilter");
            parameters["$gameNameFilter"] = query.GameName.Trim();
        }
        if (!string.IsNullOrWhiteSpace(query.TaskType))
        {
            predicates.Add("task_type=$typeFilter");
            parameters["$typeFilter"] = query.TaskType.Trim();
        }
        if (!string.IsNullOrWhiteSpace(query.RequestId))
        {
            predicates.Add("request_id=$requestFilter");
            parameters["$requestFilter"] = query.RequestId.Trim();
        }
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            predicates.Add("(task_id LIKE $search OR task_type LIKE $search OR COALESCE(game_name,'') LIKE $search OR COALESCE(message,'') LIKE $search OR COALESCE(error_message,'') LIKE $search)");
            parameters["$search"] = "%" + query.Search.Trim() + "%";
        }
        if (query.StartUtc.HasValue)
        {
            predicates.Add("created_utc >= $startFilter");
            parameters["$startFilter"] = query.StartUtc.Value.ToUniversalTime().ToString("O");
        }
        if (query.EndUtc.HasValue)
        {
            predicates.Add("created_utc < $endFilter");
            parameters["$endFilter"] = query.EndUtc.Value.ToUniversalTime().ToString("O");
        }
        if (includeCursor && TryDecodeTaskCursor(query.Cursor, out var cursorUtc, out var cursorId))
        {
            predicates.Add("(created_utc < $cursorUtc OR (created_utc = $cursorUtc AND task_id < $cursorId))");
            parameters["$cursorUtc"] = cursorUtc;
            parameters["$cursorId"] = cursorId;
        }
        return (string.Join(" AND ", predicates), parameters);
    }

    private static void AddParameters(SqliteCommand command, IReadOnlyDictionary<string, object?> parameters)
    {
        foreach (var parameter in parameters) command.Parameters.AddWithValue(parameter.Key, parameter.Value ?? DBNull.Value);
    }

    private static TaskStatusDto ReadTask(SqliteDataReader reader)
        => new TaskStatusDto
        {
            TaskId = reader.GetString(0),
            RequestId = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
            SessionId = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
            WorkerSessionId = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
            TaskType = reader.GetString(4),
            GameId = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
            GameName = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
            State = (TaskState)reader.GetInt32(7),
            ProgressPercent = reader.GetInt32(8),
            Message = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
            CreatedUtc = DateTime.Parse(reader.GetString(10)).ToUniversalTime(),
            StartedUtc = reader.IsDBNull(11) ? null : DateTime.Parse(reader.GetString(11)).ToUniversalTime(),
            FinishedUtc = reader.IsDBNull(12) ? null : DateTime.Parse(reader.GetString(12)).ToUniversalTime(),
            ErrorCode = reader.IsDBNull(13) ? string.Empty : reader.GetString(13),
            ErrorMessage = reader.IsDBNull(14) ? string.Empty : reader.GetString(14)
        };

    private static string EncodeTaskCursor(DateTime createdUtc, string taskId)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(createdUtc.ToUniversalTime().ToString("O") + "\n" + (taskId ?? string.Empty)));

    private static bool TryDecodeTaskCursor(string? cursor, out string createdUtc, out string taskId)
    {
        createdUtc = string.Empty;
        taskId = string.Empty;
        if (string.IsNullOrWhiteSpace(cursor)) return false;
        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var separator = decoded.IndexOf('\n');
            if (separator <= 0 || separator == decoded.Length - 1) return false;
            createdUtc = DateTime.Parse(decoded.Substring(0, separator)).ToUniversalTime().ToString("O");
            taskId = decoded.Substring(separator + 1);
            return taskId.Length > 0;
        }
        catch
        {
            return false;
        }
    }
}
