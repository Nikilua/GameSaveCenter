using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>
/// Read-only backup storage analysis: volume usage, indexed backup footprint,
/// 7/30/90 day growth and a simple trend estimate. It never deletes or moves files.
/// </summary>
public sealed class StorageAnalysisService
{
    private readonly WorkerOptions _options;
    private readonly SqliteStateStore _store;
    private readonly ILogger<StorageAnalysisService> _logger;

    public StorageAnalysisService(WorkerOptions options, SqliteStateStore store, ILogger<StorageAnalysisService> logger)
    {
        _options = options;
        _store = store;
        _logger = logger;
    }

    public async Task<StorageAnalysisDto> AnalyzeAsync(CancellationToken token)
    {
        var result = new StorageAnalysisDto { CheckedUtc = DateTime.UtcNow };
        if (string.IsNullOrWhiteSpace(_options.LudusaviBackupDirectory) || !Directory.Exists(_options.LudusaviBackupDirectory))
        {
            result.Summary = "存档目录不可用，无法分析备份存储。请先在设置中选择有效目录。";
            return result;
        }

        result.BackupDirectoryAvailable = true;
        result.VolumeRoot = GetVolumeRoot(_options.LudusaviBackupDirectory);
        FillVolume(result);

        var rows = await _store.GetStorageAnalysisRowsAsync(token).ConfigureAwait(false);
        result.BackupVersionCount = rows.Count;
        result.IndexedBackupBytes = rows.Sum(x => Math.Max(0, x.TotalBytes));
        var missingRows = rows
            .Where(row => string.IsNullOrWhiteSpace(row.ArchivePath) || !File.Exists(row.ArchivePath))
            .ToList();
        result.MissingIndexedPathCount = missingRows.Count;
        result.MissingIndexedBytes = missingRows.Sum(x => Math.Max(0, x.TotalBytes));
        result.RepositoryBytes = await CalculateRepositoryBytesAsync(_options.LudusaviBackupDirectory, token).ConfigureAwait(false);

        var now = DateTime.UtcNow;
        foreach (var days in new[] { 7, 30, 90 })
        {
            var cutoff = now.AddDays(-days);
            var added = rows.Where(x => x.CreatedUtc >= cutoff && x.CreatedUtc <= now).ToList();
            result.Trends.Add(new StorageTrendDto
            {
                Days = days,
                AddedBytes = added.Sum(x => Math.Max(0, x.TotalBytes)),
                AddedVersionCount = added.Count
            });
        }

        PopulateHistoricalGrowthRange(result, rows, now);

        result.TopGames = rows
            .GroupBy(x => x.PlayniteId, StringComparer.OrdinalIgnoreCase)
            .Select(group =>
            {
                var latest = group.OrderByDescending(x => x.CreatedUtc).First();
                return new StorageGameRankDto
                {
                    PlayniteId = group.Key,
                    GameName = latest.LudusaviName,
                    BackupCount = group.Count(),
                    BackupBytes = group.Sum(x => Math.Max(0, x.TotalBytes)),
                    LatestBackupId = latest.BackupId,
                    LatestBackupUtc = latest.CreatedUtc
                };
            })
            .OrderByDescending(x => x.BackupBytes)
            .ThenByDescending(x => x.BackupCount)
            .Take(5)
            .ToList();

        result.PredictionSummary = BuildPrediction(result);
        result.Summary = BuildSummary(result);
        return result;
    }

    private void FillVolume(StorageAnalysisDto result)
    {
        if (string.IsNullOrWhiteSpace(result.VolumeRoot)) return;
        try
        {
            var drive = new DriveInfo(result.VolumeRoot);
            result.VolumeTotalBytes = Math.Max(0, drive.TotalSize);
            result.VolumeFreeBytes = Math.Max(0, drive.AvailableFreeSpace);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Storage analysis could not read volume {Root}", result.VolumeRoot);
        }
    }

    private static async Task<long> CalculateRepositoryBytesAsync(string root, CancellationToken token)
    {
        return await Task.Run(() =>
        {
            long total = 0;
            foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    total += new FileInfo(file).Length;
                }
                catch
                {
                    // A locked or unreadable file should not fail the whole analysis.
                }
            }
            return total;
        }, token).ConfigureAwait(false);
    }

    private static void PopulateHistoricalGrowthRange(StorageAnalysisDto result, IReadOnlyCollection<BackupVersionDto> rows, DateTime now)
    {
        var recentRows = rows
            .Where(row => row.CreatedUtc <= now && row.CreatedUtc >= now.AddDays(-90))
            .ToList();
        var observationDays = recentRows
            .Select(row => row.CreatedUtc.Date)
            .Distinct()
            .OrderBy(day => day)
            .ToArray();
        result.GrowthObservationDayCount = observationDays.Length;
        result.GrowthObservationSpanDays = observationDays.Length > 1
            ? Math.Max(0, (int)(observationDays[^1] - observationDays[0]).TotalDays)
            : 0;

        if (result.GrowthObservationDayCount < 3 || result.GrowthObservationSpanDays < 30)
            return;

        var rates = result.Trends
            .Where(trend => trend.Days > 0 && trend.Days <= result.GrowthObservationSpanDays)
            .Select(trend => BytesPerThirtyDays(trend.AddedBytes, trend.Days))
            .ToArray();
        if (rates.Length < 2)
            return;

        result.HistoricalGrowthMinimumBytesPer30Days = rates.Min();
        result.HistoricalGrowthMaximumBytesPer30Days = rates.Max();
    }

    private static long BytesPerThirtyDays(long bytes, int days)
    {
        var rate = decimal.Round(Math.Max(0, bytes) * 30m / days, 0, MidpointRounding.AwayFromZero);
        return rate >= long.MaxValue ? long.MaxValue : (long)rate;
    }

    private static string BuildPrediction(StorageAnalysisDto result)
    {
        var localVolume = result.VolumeTotalBytes > 0
            ? $"本地卷剩余 {result.VolumeFreeDisplay} / 共 {result.VolumeTotalDisplay}。"
            : "本地卷容量或剩余空间未知。";
        if (!result.HasHistoricalGrowthRange)
        {
            return $"{result.HistoricalGrowthRangeDisplay} {localVolume}样本不足时不输出耗尽日期。逻辑新增量也不能直接换算成磁盘实占。";
        }

        return $"{localVolume}历史逻辑新增范围仅描述索引体积，压缩、保留和其他写入未知；不据此推算磁盘耗尽日期。";
    }

    private static string BuildSummary(StorageAnalysisDto result)
    {
        var volume = string.IsNullOrWhiteSpace(result.VolumeRoot)
            ? "卷信息不可用"
            : $"卷 {result.VolumeRoot} 剩余 {result.VolumeFreeDisplay} / 共 {result.VolumeTotalDisplay}";
        var footprint = $"索引 {result.BackupVersionCount} 个版本，索引体积 {result.IndexedBackupBytesDisplay}，目录实测 {result.RepositoryBytesDisplay}";
        var trend = result.Trends.FirstOrDefault(x => x.Days == 30)?.AddedBytesDisplay ?? "0 B";
        return $"{volume}；{footprint}。近 30 天新增 {trend}（索引逻辑历史统计）。{result.MissingIndexedPathSummary}";
    }

    private static string GetVolumeRoot(string path)
    {
        try
        {
            var root = Path.GetPathRoot(Path.GetFullPath(path));
            return string.IsNullOrWhiteSpace(root) ? string.Empty : root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        catch
        {
            return string.Empty;
        }
    }
}
