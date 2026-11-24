using GameSaveCenter.Contracts;

namespace GameSaveCenter.Worker.Persistence;

public sealed partial class SqliteStateStore
{
    public async Task<List<BackupCalendarDayDto>> GetBackupCalendarAsync(BackupCalendarQueryDto query, CancellationToken token)
    {
        var result = new List<BackupCalendarDayDto>();
        if (string.IsNullOrWhiteSpace(query.PlayniteId)) return result;
        var boundaries = query.UtcDayBoundaries;
        if (boundaries == null || boundaries.Count < 29 || boundaries.Count > 32
            || boundaries.Any(value => value.Kind != DateTimeKind.Utc)
            || boundaries.Zip(boundaries.Skip(1), (start, end) => start > end).Any(reversed => reversed))
            throw new ArgumentException("日历查询需要一个月内依次排列的 UTC 日期边界。");

        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.Parameters.AddWithValue("$game", query.PlayniteId);
        for (var index = 0; index < boundaries.Count; index++)
            command.Parameters.AddWithValue($"$boundary{index}", boundaries[index].ToString("O"));
        var ranges = string.Join(",", Enumerable.Range(0, boundaries.Count - 1)
            .Select(index => $"({index + 1},$boundary{index},$boundary{index + 1})"));
        command.CommandText = $@"
WITH local_days(day_number,start_utc,end_utc) AS (VALUES {ranges})
SELECT d.day_number, COUNT(b.backup_id)
FROM local_days d
JOIN backup_versions b ON b.playnite_id=$game AND b.created_utc>=d.start_utc AND b.created_utc<d.end_utc
GROUP BY d.day_number
ORDER BY d.day_number;";
        await using var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false);
        while (await reader.ReadAsync(token).ConfigureAwait(false))
            result.Add(new BackupCalendarDayDto { Day = reader.GetInt32(0), Count = reader.GetInt32(1) });
        return result;
    }
}
