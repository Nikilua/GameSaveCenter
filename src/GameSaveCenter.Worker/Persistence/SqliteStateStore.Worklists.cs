using System.Globalization;
using System.Text;
using GameSaveCenter.Contracts;
using Microsoft.Data.Sqlite;

namespace GameSaveCenter.Worker.Persistence;

public sealed partial class SqliteStateStore
{
    private readonly record struct WorklistReferenceLookup(
        WorklistReferenceState State,
        string PlayniteId,
        string CurrentSummary,
        string ParentSummary);

    private async Task<WorklistReferenceLookup> ResolveWorklistReferenceSummaryAsync(
        SqliteConnection connection,
        int kind,
        string stableId,
        CancellationToken token)
    {
        if (stableId.Length == 0 || stableId.Length > 256)
            return EmptyWorklistLookup(WorklistReferenceState.InvalidReference);

        switch (kind)
        {
            case 1 when TryParseBackupReference(stableId, out var playniteId, out var backupId):
                return await ReadBackupWorklistSummaryAsync(connection, playniteId, backupId, token).ConfigureAwait(false);
            case 2:
                return await ReadMediaWorklistSummaryAsync(connection, stableId, token).ConfigureAwait(false);
            case 3:
                return await ReadTaskWorklistSummaryAsync(connection, stableId, token).ConfigureAwait(false);
            case 4:
                return await ReadFindingWorklistSummaryAsync(connection, stableId, token).ConfigureAwait(false);
            default:
                return EmptyWorklistLookup(WorklistReferenceState.InvalidReference);
        }
    }

    private static async Task<WorklistReferenceLookup> ReadBackupWorklistSummaryAsync(
        SqliteConnection connection,
        string playniteId,
        string backupId,
        CancellationToken token)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT bv.created_utc,bv.ludusavi_name,COALESCE(g.name,'')
FROM backup_versions bv LEFT JOIN games g ON g.playnite_id=bv.playnite_id
WHERE bv.playnite_id=$game AND bv.backup_id=$id LIMIT 1;";
        command.Parameters.AddWithValue("$game", playniteId);
        command.Parameters.AddWithValue("$id", backupId);
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        if (!await reader.ReadAsync(token).ConfigureAwait(false))
            return EmptyWorklistLookup(WorklistReferenceState.NotFound);

        var currentGameName = SafeSummaryPart(reader.IsDBNull(2) ? string.Empty : reader.GetString(2));
        var currentName = currentGameName.Length > 0
            ? currentGameName
            : SafeSummaryPart(reader.IsDBNull(1) ? string.Empty : reader.GetString(1));
        var created = FormatWorklistUtc(reader.IsDBNull(0) ? string.Empty : reader.GetString(0));
        return new WorklistReferenceLookup(
            WorklistReferenceState.Indexed,
            playniteId,
            JoinSummary(currentName, "存档版本", created),
            CreateParentSummary(currentGameName, playniteId));
    }

    private static async Task<WorklistReferenceLookup> ReadMediaWorklistSummaryAsync(
        SqliteConnection connection,
        string mediaId,
        CancellationToken token)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT m.playnite_id,m.archive_path,m.kind,m.captured_utc,COALESCE(g.name,'')
FROM media m LEFT JOIN games g ON g.playnite_id=m.playnite_id
WHERE m.media_id=$id LIMIT 1;";
        command.Parameters.AddWithValue("$id", mediaId);
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        if (!await reader.ReadAsync(token).ConfigureAwait(false))
            return EmptyWorklistLookup(WorklistReferenceState.NotFound);

        var playniteId = reader.IsDBNull(0) ? string.Empty : reader.GetString(0).Trim();
        if (playniteId.Length == 0)
            return EmptyWorklistLookup(WorklistReferenceState.NotFound);

        var currentGameName = SafeSummaryPart(reader.IsDBNull(4) ? string.Empty : reader.GetString(4));
        var fileName = GetSafeFileLabel(reader.IsDBNull(1) ? string.Empty : reader.GetString(1));
        var captured = FormatWorklistUtc(reader.IsDBNull(3) ? string.Empty : reader.GetString(3));
        var mediaKind = reader.IsDBNull(2) ? 99 : reader.GetInt32(2);
        return new WorklistReferenceLookup(
            WorklistReferenceState.Indexed,
            playniteId,
            JoinSummary(fileName, GetMediaKindSummary(mediaKind), captured),
            CreateParentSummary(currentGameName, playniteId));
    }

    private static async Task<WorklistReferenceLookup> ReadTaskWorklistSummaryAsync(
        SqliteConnection connection,
        string taskId,
        CancellationToken token)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT t.game_id,t.task_type,t.state,COALESCE(g.name,'')
FROM tasks t LEFT JOIN games g ON g.playnite_id=t.game_id
WHERE t.task_id=$id LIMIT 1;";
        command.Parameters.AddWithValue("$id", taskId);
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        if (!await reader.ReadAsync(token).ConfigureAwait(false))
            return EmptyWorklistLookup(WorklistReferenceState.NotFound);

        var playniteId = reader.IsDBNull(0) ? string.Empty : reader.GetString(0).Trim();
        var taskType = SafeSummaryPart(reader.IsDBNull(1) ? string.Empty : reader.GetString(1));
        var state = reader.IsDBNull(2) ? int.MinValue : reader.GetInt32(2);
        var currentGameName = SafeSummaryPart(reader.IsDBNull(3) ? string.Empty : reader.GetString(3));
        return new WorklistReferenceLookup(
            WorklistReferenceState.Indexed,
            playniteId,
            JoinSummary(taskType.Length == 0 ? "后台任务" : taskType, GetTaskStateSummary(state)),
            CreateParentSummary(currentGameName, playniteId));
    }

    private static async Task<WorklistReferenceLookup> ReadFindingWorklistSummaryAsync(
        SqliteConnection connection,
        string findingId,
        CancellationToken token)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = @"SELECT f.playnite_id,f.title,f.code,f.severity,f.resolved,COALESCE(g.name,'')
FROM findings f LEFT JOIN games g ON g.playnite_id=f.playnite_id
WHERE f.finding_id=$id LIMIT 1;";
        command.Parameters.AddWithValue("$id", findingId);
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        if (!await reader.ReadAsync(token).ConfigureAwait(false))
            return EmptyWorklistLookup(WorklistReferenceState.NotFound);

        var playniteId = reader.IsDBNull(0) ? string.Empty : reader.GetString(0).Trim();
        var title = SafeSummaryPart(reader.IsDBNull(1) ? string.Empty : reader.GetString(1));
        var code = SafeSummaryPart(reader.IsDBNull(2) ? string.Empty : reader.GetString(2));
        var severity = reader.IsDBNull(3) ? int.MinValue : reader.GetInt32(3);
        var resolved = !reader.IsDBNull(4) && Convert.ToInt32(reader.GetValue(4), CultureInfo.InvariantCulture) != 0;
        var currentGameName = SafeSummaryPart(reader.IsDBNull(5) ? string.Empty : reader.GetString(5));
        return new WorklistReferenceLookup(
            resolved ? WorklistReferenceState.Resolved : WorklistReferenceState.Indexed,
            playniteId,
            JoinSummary(GetFindingSeveritySummary(severity), code, title),
            CreateParentSummary(currentGameName, playniteId));
    }

    private static WorklistReferenceLookup EmptyWorklistLookup(WorklistReferenceState state)
        => new(state, string.Empty, string.Empty, string.Empty);

    private static string CreateParentSummary(string gameName, string playniteId)
        => gameName.Length > 0
            ? gameName
            : playniteId.Length > 0 ? "游戏 ID " + SafeSummaryPart(playniteId) : string.Empty;

    private static string JoinSummary(params string[] parts)
        => SafeSummaryPart(string.Join(" · ", parts.Where(part => !string.IsNullOrWhiteSpace(part))));

    private static string GetSafeFileLabel(string path)
    {
        var normalized = (path ?? string.Empty).Replace('\\', '/');
        var separator = normalized.LastIndexOf('/');
        var fileName = separator >= 0 ? normalized.Substring(separator + 1) : normalized;
        return SafeSummaryPart(fileName);
    }

    private static string SafeSummaryPart(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        var builder = new StringBuilder(Math.Min(value.Length, 240));
        var needsSpace = false;
        foreach (var character in value)
        {
            if (char.IsControl(character) || char.IsWhiteSpace(character))
            {
                needsSpace = builder.Length > 0;
                continue;
            }

            if (needsSpace && builder.Length < 240) builder.Append(' ');
            needsSpace = false;
            if (builder.Length >= 240) break;
            builder.Append(character);
        }
        return builder.ToString().Trim();
    }

    private static string FormatWorklistUtc(string value)
        => DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var timestamp)
            ? timestamp.ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture)
            : string.Empty;

    private static string GetMediaKindSummary(int kind)
        => kind switch
        {
            (int)MediaKind.Screenshot => "截图",
            (int)MediaKind.VideoClip => "视频片段",
            _ => "媒体项"
        };

    private static string GetTaskStateSummary(int state)
        => (TaskState)state switch
        {
            TaskState.Queued => "排队中",
            TaskState.Running => "运行中",
            TaskState.Succeeded => "已完成",
            TaskState.Failed => "失败",
            TaskState.Cancelled => "已取消",
            TaskState.WaitingForUser => "等待处理",
            _ => "状态未知"
        };

    private static string GetFindingSeveritySummary(int severity)
        => (FindingSeverity)severity switch
        {
            FindingSeverity.Info => "信息",
            FindingSeverity.Warning => "警告",
            FindingSeverity.Error => "错误",
            FindingSeverity.Critical => "严重",
            _ => "校验问题"
        };
}
