using GameSaveCenter.Contracts;

namespace GameSaveCenter.Worker.Persistence;

/// <summary>Durable state for a full-library backup submission.</summary>
internal sealed class BackupAllJobRecord
{
    public string JobId { get; init; } = string.Empty;
    public string RequestJson { get; init; } = string.Empty;
    public TaskState State { get; set; } = TaskState.Queued;
    public int ProgressPercent { get; set; }
    public string Message { get; set; } = "等待执行";
    public string CompletedGameIdsJson { get; set; } = "[]";
    public string CurrentGameId { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; init; }
    public DateTime? StartedUtc { get; set; }
    public DateTime? FinishedUtc { get; set; }
    public string WorkerSessionId { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;

    public TaskStatusDto ToTaskStatus()
        => new()
        {
            TaskId = JobId,
            WorkerSessionId = WorkerSessionId,
            TaskType = "BackupAll",
            GameName = "全部游戏",
            State = State,
            ProgressPercent = ProgressPercent,
            Message = Message,
            CreatedUtc = CreatedUtc,
            StartedUtc = StartedUtc,
            FinishedUtc = FinishedUtc,
            ErrorCode = ErrorCode,
            ErrorMessage = ErrorMessage
        };
}

public sealed partial class SqliteStateStore
{
    internal Task CreateBackupAllJobAsync(BackupAllJobRecord job, CancellationToken token)
        => ExecuteAsync(@"
INSERT INTO backup_all_jobs(job_id,request_json,state,progress,message,completed_game_ids_json,current_game_id,created_utc,started_utc,finished_utc,worker_session_id,error_code,error_message)
VALUES($id,$request,$state,$progress,$message,$completed,$current,$created,$started,$finished,$worker,$errorCode,$errorMessage);",
            ToParameters(job), token);

    internal async Task<BackupAllJobRecord?> GetActiveBackupAllJobAsync(CancellationToken token)
    {
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = @"
SELECT job_id,request_json,state,progress,message,completed_game_ids_json,current_game_id,created_utc,started_utc,finished_utc,worker_session_id,error_code,error_message
FROM backup_all_jobs
WHERE state IN ($queued,$running)
ORDER BY created_utc DESC
LIMIT 1;";
        command.Parameters.AddWithValue("$queued", (int)TaskState.Queued);
        command.Parameters.AddWithValue("$running", (int)TaskState.Running);
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        return await reader.ReadAsync(token).ConfigureAwait(false) ? ReadBackupAllJob(reader) : null;
    }

    internal Task UpdateBackupAllJobAsync(BackupAllJobRecord job, CancellationToken token)
        => ExecuteAsync(@"
UPDATE backup_all_jobs
SET state=$state,progress=$progress,message=$message,completed_game_ids_json=$completed,current_game_id=$current,
    started_utc=$started,finished_utc=$finished,worker_session_id=$worker,error_code=$errorCode,error_message=$errorMessage
WHERE job_id=$id;",
            ToParameters(job), token);

    private static BackupAllJobRecord ReadBackupAllJob(Microsoft.Data.Sqlite.SqliteDataReader reader)
        => new()
        {
            JobId = reader.GetString(0),
            RequestJson = reader.GetString(1),
            State = (TaskState)reader.GetInt32(2),
            ProgressPercent = reader.GetInt32(3),
            Message = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
            CompletedGameIdsJson = reader.IsDBNull(5) ? "[]" : reader.GetString(5),
            CurrentGameId = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
            CreatedUtc = DateTime.Parse(reader.GetString(7)).ToUniversalTime(),
            StartedUtc = reader.IsDBNull(8) ? null : DateTime.Parse(reader.GetString(8)).ToUniversalTime(),
            FinishedUtc = reader.IsDBNull(9) ? null : DateTime.Parse(reader.GetString(9)).ToUniversalTime(),
            WorkerSessionId = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
            ErrorCode = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
            ErrorMessage = reader.IsDBNull(12) ? string.Empty : reader.GetString(12)
        };

    private static IReadOnlyDictionary<string, object?> ToParameters(BackupAllJobRecord job)
        => new Dictionary<string, object?>
        {
            ["$id"] = job.JobId,
            ["$request"] = job.RequestJson,
            ["$state"] = (int)job.State,
            ["$progress"] = job.ProgressPercent,
            ["$message"] = job.Message,
            ["$completed"] = job.CompletedGameIdsJson,
            ["$current"] = job.CurrentGameId,
            ["$created"] = job.CreatedUtc.ToString("O"),
            ["$started"] = job.StartedUtc?.ToString("O"),
            ["$finished"] = job.FinishedUtc?.ToString("O"),
            ["$worker"] = job.WorkerSessionId,
            ["$errorCode"] = job.ErrorCode,
            ["$errorMessage"] = job.ErrorMessage
        };
}
