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
        result.RepositoryBytes = await CalculateRepositoryBytesAsync(_options.LudusaviBackupDirectory, token).ConfigureAwait(false);

        var now = DateTime.UtcNow;
        foreach (var days in new[] { 7, 30, 90 })
        {
            var cutoff = now.AddDays(-days);
            var added = rows.Where(x => x.CreatedUtc >= cutoff).ToList();
            result.Trends.Add(new StorageTrendDto
            {
                Days = days,
                AddedBytes = added.Sum(x => Math.Max(0, x.TotalBytes)),
                AddedVersionCount = added.Count
            });
        }

        result.TopGames = rows
            .GroupBy(x => x.PlayniteId, StringComparer.OrdinalIgnoreCase)
            .Select(group => new StorageGameRankDto
            {
                PlayniteId = group.Key,
                GameName = group.OrderByDescending(x => x.CreatedUtc).First().LudusaviName,
                BackupCount = group.Count(),
                BackupBytes = group.Sum(x => Math.Max(0, x.TotalBytes)),
                LatestBackupUtc = group.Max(x => x.CreatedUtc)
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

    private static string BuildPrediction(StorageAnalysisDto result)
    {
        if (result.VolumeTotalBytes <= 0 || result.VolumeFreeBytes <= 0) return string.Empty;
        var trend30 = result.Trends.FirstOrDefault(x => x.Days == 30)?.AddedBytes ?? 0;
        if (trend30 <= 0)
            return "近 30 天没有可用的增长趋势，无法可靠估算容量耗尽时间。";

        var dailyBytes = trend30 / 30d;
        var target = result.VolumeTotalBytes * 0.9d;
        var used = Math.Max(0, result.VolumeTotalBytes - result.VolumeFreeBytes);
        if (target <= used) return "估算：磁盘已接近 90% 用量，请尽快清理或迁移备份目录。";
        var days = (target - used) / dailyBytes;
        var months = days / 30.44d;
        return $"按最近 30 天新增速度估算，约 {months:0.#} 个月后达到磁盘 90% 用量。";
    }

    private static string BuildSummary(StorageAnalysisDto result)
    {
        var volume = string.IsNullOrWhiteSpace(result.VolumeRoot)
            ? "卷信息不可用"
            : $"卷 {result.VolumeRoot} 剩余 {result.VolumeFreeDisplay} / 共 {result.VolumeTotalDisplay}";
        var footprint = $"索引 {result.BackupVersionCount} 个版本，索引体积 {result.IndexedBackupBytesDisplay}，目录实测 {result.RepositoryBytesDisplay}";
        var trend = result.Trends.FirstOrDefault(x => x.Days == 30)?.AddedBytesDisplay ?? "0 B";
        return $"{volume}；{footprint}。近 30 天新增 {trend}（估算）";
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
