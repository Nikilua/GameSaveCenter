using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Worker.Persistence;

public sealed partial class SqliteStateStore
{
    private const string HealthInspectionStateId = "default";

    public async Task<HealthInspectionStateDto> GetHealthInspectionStateAsync(CancellationToken token)
    {
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = @"SELECT enabled,interval_minutes,stale_after_days,max_duration_seconds,next_due_utc,
last_started_utc,last_completed_utc,last_successful_utc,cursor_playnite_id,cursor_backup_id,last_playnite_id,
last_backup_id,last_status,last_summary,deferred_count,failure_count
FROM health_inspection_state WHERE state_id=$id;";
        command.Parameters.AddWithValue("$id", HealthInspectionStateId);
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        if (!await reader.ReadAsync(token).ConfigureAwait(false))
            return DefaultHealthInspectionState();

        return new HealthInspectionStateDto
        {
            Enabled = reader.GetInt32(0) == 1,
            IntervalMinutes = reader.GetInt32(1),
            StaleAfterDays = reader.GetInt32(2),
            MaxDurationSeconds = reader.GetInt32(3),
            NextDueUtc = ReadNullableUtc(reader, 4),
            LastStartedUtc = ReadNullableUtc(reader, 5),
            LastCompletedUtc = ReadNullableUtc(reader, 6),
            LastSuccessfulUtc = ReadNullableUtc(reader, 7),
            CursorPlayniteId = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
            CursorBackupId = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
            LastPlayniteId = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
            LastBackupId = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
            LastStatus = reader.IsDBNull(12) ? "NeverRun" : reader.GetString(12),
            LastSummary = reader.IsDBNull(13) ? string.Empty : reader.GetString(13),
            DeferredCount = reader.GetInt32(14),
            FailureCount = reader.GetInt32(15)
        };
    }

    public Task SaveHealthInspectionStateAsync(HealthInspectionStateDto state, CancellationToken token)
        => ExecuteAsync(@"INSERT INTO health_inspection_state(state_id,enabled,interval_minutes,stale_after_days,max_duration_seconds,
next_due_utc,last_started_utc,last_completed_utc,last_successful_utc,cursor_playnite_id,cursor_backup_id,last_playnite_id,
last_backup_id,last_status,last_summary,deferred_count,failure_count,updated_utc)
VALUES($id,$enabled,$interval,$stale,$duration,$next,$started,$completed,$successful,$cursor_game,$cursor_backup,$last_game,
$last_backup,$status,$summary,$deferred,$failures,$updated)
ON CONFLICT(state_id) DO UPDATE SET enabled=excluded.enabled,interval_minutes=excluded.interval_minutes,
stale_after_days=excluded.stale_after_days,max_duration_seconds=excluded.max_duration_seconds,next_due_utc=excluded.next_due_utc,
last_started_utc=excluded.last_started_utc,last_completed_utc=excluded.last_completed_utc,last_successful_utc=excluded.last_successful_utc,
cursor_playnite_id=excluded.cursor_playnite_id,cursor_backup_id=excluded.cursor_backup_id,last_playnite_id=excluded.last_playnite_id,
last_backup_id=excluded.last_backup_id,last_status=excluded.last_status,last_summary=excluded.last_summary,
deferred_count=excluded.deferred_count,failure_count=excluded.failure_count,updated_utc=excluded.updated_utc;",
            new Dictionary<string, object?>
            {
                ["$id"] = HealthInspectionStateId,
                ["$enabled"] = state.Enabled ? 1 : 0,
                ["$interval"] = state.IntervalMinutes,
                ["$stale"] = state.StaleAfterDays,
                ["$duration"] = state.MaxDurationSeconds,
                ["$next"] = ToUtcText(state.NextDueUtc),
                ["$started"] = ToUtcText(state.LastStartedUtc),
                ["$completed"] = ToUtcText(state.LastCompletedUtc),
                ["$successful"] = ToUtcText(state.LastSuccessfulUtc),
                ["$cursor_game"] = state.CursorPlayniteId,
                ["$cursor_backup"] = state.CursorBackupId,
                ["$last_game"] = state.LastPlayniteId,
                ["$last_backup"] = state.LastBackupId,
                ["$status"] = state.LastStatus,
                ["$summary"] = state.LastSummary,
                ["$deferred"] = state.DeferredCount,
                ["$failures"] = state.FailureCount,
                ["$updated"] = DateTime.UtcNow.ToString("O")
            }, token);

    public async Task<List<BackupVersionDto>> GetAllBackupVersionsForInspectionAsync(CancellationToken token)
    {
        var result = new List<BackupVersionDto>();
        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = @"SELECT backup_id,playnite_id,ludusavi_name,created_utc,total_bytes,file_count,is_locked,
comment,source_device,operating_system,is_pre_restore,archive_path,restore_readiness_json
FROM backup_versions ORDER BY playnite_id COLLATE NOCASE,created_utc ASC,backup_id COLLATE NOCASE ASC LIMIT 100000;";
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false))
        {
            RestoreReadinessDto? readiness = null;
            if (!reader.IsDBNull(12))
            {
                try { readiness = JsonSerializer.Deserialize<RestoreReadinessDto>(reader.GetString(12), _json); }
                catch (JsonException) { }
            }

            result.Add(new BackupVersionDto
            {
                BackupId = reader.GetString(0),
                PlayniteId = reader.GetString(1),
                LudusaviName = reader.GetString(2),
                CreatedUtc = DateTime.Parse(reader.GetString(3)).ToUniversalTime(),
                TotalBytes = reader.GetInt64(4),
                FileCount = reader.GetInt32(5),
                IsLocked = reader.GetInt32(6) == 1,
                Comment = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
                SourceDevice = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                OperatingSystem = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                IsPreRestore = reader.GetInt32(10) == 1,
                ArchivePath = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                RestoreReadiness = readiness
            });
        }
        return result;
    }

    public Task UpsertHealthInspectionFindingAsync(string playniteId, string backupId, RestoreReadinessDto readiness, CancellationToken token)
    {
        var findingId = HealthFindingId(playniteId, backupId);
        var severity = readiness.Status is RestoreReadinessStatus.Corrupted or RestoreReadinessStatus.Failed
            ? (int)FindingSeverity.Error
            : (int)FindingSeverity.Warning;
        return ExecuteAsync(@"INSERT INTO findings(finding_id,playnite_id,severity,code,title,detail,suggested_action,created_utc,resolved)
VALUES($id,$game,$severity,'HEALTH_INSPECTION_FAILED',$title,$detail,$action,$utc,0)
ON CONFLICT(finding_id) DO UPDATE SET severity=excluded.severity,title=excluded.title,detail=excluded.detail,
suggested_action=excluded.suggested_action,created_utc=excluded.created_utc,resolved=0;",
            new Dictionary<string, object?>
            {
                ["$id"] = findingId,
                ["$game"] = playniteId,
                ["$severity"] = severity,
                ["$title"] = $"备份恢复校验需关注：{backupId}",
                ["$detail"] = readiness.Summary,
                ["$action"] = "请检查归档、Manifest 和存储空间；预检未通过时不要执行真实恢复。",
                ["$utc"] = DateTime.UtcNow.ToString("O")
            }, token);
    }

    public Task ResolveHealthInspectionFindingAsync(string playniteId, string backupId, CancellationToken token)
        => ExecuteAsync("UPDATE findings SET resolved=1 WHERE finding_id=$id;",
            new Dictionary<string, object?> { ["$id"] = HealthFindingId(playniteId, backupId) }, token);

    private HealthInspectionStateDto DefaultHealthInspectionState() => new()
    {
        Enabled = _options.HealthInspectionEnabled,
        IntervalMinutes = _options.HealthInspectionIntervalMinutes,
        StaleAfterDays = _options.HealthInspectionStaleAfterDays,
        MaxDurationSeconds = _options.HealthInspectionMaxDurationSeconds,
        NextDueUtc = DateTime.UtcNow.AddMinutes(_options.HealthInspectionIntervalMinutes),
        LastSummary = "尚未运行恢复可用性巡检。"
    };

    private static DateTime? ReadNullableUtc(Microsoft.Data.Sqlite.SqliteDataReader reader, int ordinal)
        => reader.IsDBNull(ordinal) ? null : DateTime.Parse(reader.GetString(ordinal)).ToUniversalTime();

    private static string? ToUtcText(DateTime? value) => value?.ToUniversalTime().ToString("O");

    private static string HealthFindingId(string playniteId, string backupId)
        => "health-readiness-" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(playniteId + "\0" + backupId)));
}
