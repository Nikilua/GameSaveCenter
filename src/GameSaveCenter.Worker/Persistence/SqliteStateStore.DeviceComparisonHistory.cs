using System.Globalization;
using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;

namespace GameSaveCenter.Worker.Persistence;

public sealed partial class SqliteStateStore
{
    private sealed record ComparedDeviceVersion(string PlayniteId, string GameName, string LocalBackupId, string RemoteBackupId);

    public async Task ObserveDeviceComparisonAsync(DeviceStateSyncResultDto result,
        IReadOnlyDictionary<string, DeviceBackupSummaryDto> localByGame, bool complete, CancellationToken token)
    {
        foreach (var row in result.Comparisons)
            row.ComparisonUtc = result.GeneratedUtc;

        await using var connection = Open();
        await connection.OpenAsync(token).ConfigureAwait(false);
        using var transaction = connection.BeginTransaction(deferred: !complete);
        var coverageByDevice = result.Coverage
            .Where(coverage => WorkerOptions.IsValidDeviceId(coverage.DeviceId))
            .GroupBy(coverage => coverage.DeviceId, StringComparer.OrdinalIgnoreCase).ToList();
        complete = complete && coverageByDevice.All(group => group.Count() == 1);
        var newRows = new List<DeviceConflictStatusDto>();
        foreach (var group in coverageByDevice)
        {
            var coverage = group.First();
            var rows = result.Comparisons.Where(row => string.Equals(row.RemoteDeviceId, coverage.DeviceId, StringComparison.OrdinalIgnoreCase)).ToArray();
            var deviceComplete = complete && coverage.Compared == coverage.Total
                && rows.Select(row => row.PlayniteId).Distinct(StringComparer.OrdinalIgnoreCase).Count() == rows.Length
                && rows.All(row => !string.IsNullOrWhiteSpace(row.RemoteBackupId));
            DateTime? previousUtc = null;
            var previousVersions = new List<ComparedDeviceVersion>();
            await using (var read = connection.CreateCommand())
            {
                read.Transaction = transaction;
                read.CommandText = "SELECT observed_utc,versions_json FROM device_comparison_history WHERE remote_device_id=$id;";
                read.Parameters.AddWithValue("$id", coverage.DeviceId);
                await using var reader = await read.ExecuteReaderAsync(token).ConfigureAwait(false);
                if (await reader.ReadAsync(token).ConfigureAwait(false))
                {
                    previousUtc = DateTime.Parse(reader.GetString(0), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                    previousVersions = JsonSerializer.Deserialize<List<ComparedDeviceVersion>>(reader.GetString(1), _json) ?? new();
                }
            }

            var fresh = !previousUtc.HasValue || result.GeneratedUtc > previousUtc.Value;
            var coverageDisplay = $"已比对远端 {coverage.Compared}/{coverage.Total} 条最新版本摘要";
            if (!deviceComplete) coverageDisplay += " · 本次覆盖不完整，保留上次基线";
            else if (!fresh) coverageDisplay += " · 已有更新的比较记录";
            var previous = previousVersions.ToDictionary(version => version.PlayniteId, StringComparer.OrdinalIgnoreCase);
            foreach (var row in rows)
            {
                row.PreviousComparisonUtc = previousUtc;
                row.ComparisonCoverageDisplay = coverageDisplay;
                if (!deviceComplete || !fresh || !previousUtc.HasValue) continue;
                if (!previous.TryGetValue(row.PlayniteId, out var old))
                    row.ComparisonChanges.Add("远端新增此游戏的版本摘要");
                else
                {
                    if (!string.Equals(old.RemoteBackupId, row.RemoteBackupId, StringComparison.Ordinal))
                        row.ComparisonChanges.Add($"远端最新版本由 {old.RemoteBackupId} 变为 {row.RemoteBackupId}");
                    if (!string.Equals(old.LocalBackupId, row.LocalBackupId, StringComparison.Ordinal))
                        row.ComparisonChanges.Add("本机最新版本已变化");
                }
                if (row.PreviousDecision != null) row.ComparisonChanges.Add("此前决定的版本组合已变化，需要人工复核");
            }

            if (!deviceComplete || !fresh) continue;
            var currentGameIds = new HashSet<string>(rows.Select(row => row.PlayniteId), StringComparer.OrdinalIgnoreCase);
            foreach (var old in previousVersions.Where(version => !currentGameIds.Contains(version.PlayniteId)))
            {
                localByGame.TryGetValue(old.PlayniteId, out var local);
                newRows.Add(new DeviceConflictStatusDto
                {
                    PlayniteId = old.PlayniteId,
                    GameName = local?.GameName ?? old.GameName,
                    LocalDevice = result.LocalDevice,
                    LocalBackupId = local?.BackupId ?? string.Empty,
                    LocalCreatedUtc = local?.CreatedUtc ?? default,
                    LocalParentBackupId = local?.ParentBackupId ?? string.Empty,
                    LocalTotalBytes = local?.TotalBytes ?? 0,
                    LocalFileCount = local?.FileCount ?? 0,
                    LocalReadinessStatus = local?.RestoreReadinessStatus ?? string.Empty,
                    LocalReadinessCheckedUtc = local?.RestoreReadinessCheckedUtc,
                    RemoteDevice = coverage.DeviceName,
                    RemoteDeviceId = coverage.DeviceId,
                    Reason = "OnlyOneSideAvailable",
                    ComparisonUtc = result.GeneratedUtc,
                    PreviousComparisonUtc = previousUtc,
                    ComparisonCoverageDisplay = coverageDisplay,
                    ComparisonChanges = { $"远端已完整读取，此次未列出原版本摘要 {old.RemoteBackupId}" }
                });
            }

            await using var write = connection.CreateCommand();
            write.Transaction = transaction;
            write.CommandText = @"
INSERT INTO device_comparison_history(remote_device_id,observed_utc,versions_json) VALUES($id,$utc,$versions)
ON CONFLICT(remote_device_id) DO UPDATE SET observed_utc=excluded.observed_utc,versions_json=excluded.versions_json;";
            write.Parameters.AddWithValue("$id", coverage.DeviceId);
            write.Parameters.AddWithValue("$utc", result.GeneratedUtc.ToString("O", CultureInfo.InvariantCulture));
            write.Parameters.AddWithValue("$versions", JsonSerializer.Serialize(rows.Select(row =>
                new ComparedDeviceVersion(row.PlayniteId, row.GameName, row.LocalBackupId, row.RemoteBackupId)).ToList(), _json));
            await write.ExecuteNonQueryAsync(token).ConfigureAwait(false);
        }

        foreach (var row in result.Comparisons.Where(row => row.ComparisonCoverageDisplay.Length == 0))
            row.ComparisonCoverageDisplay = "此设备摘要缺少稳定设备标识，未记录比较基线";
        await transaction.CommitAsync(token).ConfigureAwait(false);
        result.Comparisons.AddRange(newRows);
    }
}
