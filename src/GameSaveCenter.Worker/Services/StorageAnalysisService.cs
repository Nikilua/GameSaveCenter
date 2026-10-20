using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>
/// Read-only local storage analysis. It never deletes or moves files, and reports
/// archive, mirror, staging and media scopes independently without adding them together.
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
        var rows = await _store.GetStorageAnalysisRowsAsync(token).ConfigureAwait(false);
        result.BackupVersionCount = rows.Count;
        result.IndexedBackupBytes = rows.Sum(x => Math.Max(0, x.TotalBytes));
        var missingRows = rows
            .Where(row => string.IsNullOrWhiteSpace(row.ArchivePath) || !File.Exists(row.ArchivePath))
            .ToList();
        result.MissingIndexedPathCount = missingRows.Count;
        result.MissingIndexedBytes = missingRows.Sum(x => Math.Max(0, x.TotalBytes));

        var mediaIndex = await _store.GetAllMediaStorageSummaryAsync(token).ConfigureAwait(false);
        var archive = await MeasureDirectoryAsync(_options.LudusaviBackupDirectory,
            !string.IsNullOrWhiteSpace(_options.LudusaviBackupDirectory), "存档目录", token).ConfigureAwait(false);
        var mirrorConfigured = _options.EnableLocalMirror && !string.IsNullOrWhiteSpace(_options.LocalMirrorPath);
        var mirror = await MeasureDirectoryAsync(_options.LocalMirrorPath, mirrorConfigured, "本地镜像", token,
            !_options.EnableLocalMirror ? "未启用" : "已启用，但未设置目录").ConfigureAwait(false);
        var staging = await MeasureDirectoryAsync(_options.RemoteBackupStagingDirectory,
            !string.IsNullOrWhiteSpace(_options.RemoteBackupStagingDirectory), "远端暂存", token).ConfigureAwait(false);
        var media = await MeasureDirectoryAsync(_options.MediaArchiveDirectory,
            !string.IsNullOrWhiteSpace(_options.MediaArchiveDirectory), "媒体目录", token).ConfigureAwait(false);

        result.BackupDirectoryAvailable = archive.DirectoryAvailable;
        result.RepositoryMeasurementComplete = archive.MeasurementComplete;
        if (archive.MeasurementComplete) result.RepositoryBytes = archive.FileLengthBytes ?? 0;
        if (archive.DirectoryAvailable)
        {
            result.VolumeRoot = GetVolumeRoot(_options.LudusaviBackupDirectory);
            FillVolume(result);
        }

        result.BudgetBreakdown = new List<StorageBudgetEntryDto>
        {
            CreateBudgetEntry("本地存档", "配置的 Ludusavi 存档目录；包含未索引文件。", archive,
                true, "SQLite 已索引版本逻辑大小", result.IndexedBackupBytes, result.BackupVersionCount,
                "文件长度合计不等于卷簇实占；失联索引仍单独提示。"),
            CreateBudgetEntry("第二本地镜像", "配置的镜像目录；它通常是存档副本。", mirror,
                false, string.Empty, null, null,
                "只显示镜像目录文件长度（含镜像状态标记）；可能与本地存档重复，不作相加。"),
            CreateBudgetEntry("远端恢复暂存", "Worker 的隔离暂存目录；包含当前保留的暂存文件。", staging,
                false, string.Empty, null, null,
                "暂存副本与远端来源及本地存档分开显示；不存在或不可读时保持未知。"),
            CreateBudgetEntry("媒体归档", "配置的媒体归档目录；目录外或未登记文件不进入索引逻辑量。", media,
                true, "全部登记媒体记录逻辑大小", mediaIndex.TotalBytes, mediaIndex.TotalCount,
                "逻辑量包含已归类、待分类及已忽略的登记记录；目录实测还会包含未登记文件。")
        };
        MarkOverlappingScopes(result.BudgetBreakdown, new[]
        {
            _options.LudusaviBackupDirectory,
            mirrorConfigured ? _options.LocalMirrorPath : string.Empty,
            _options.RemoteBackupStagingDirectory,
            _options.MediaArchiveDirectory
        });

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

    private async Task<DirectoryMeasurement> MeasureDirectoryAsync(
        string path,
        bool configured,
        string scopeName,
        CancellationToken token,
        string? notConfiguredStatus = null)
    {
        if (!configured || string.IsNullOrWhiteSpace(path))
            return new DirectoryMeasurement(false, false, false, null, null,
                notConfiguredStatus ?? "未设置");

        if (!Directory.Exists(path))
            return new DirectoryMeasurement(true, false, false, null, null, "目录不存在或不可访问");

        try
        {
            return await Task.Run(() =>
            {
                long total = 0;
                var count = 0;
                foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    token.ThrowIfCancellationRequested();
                    var length = new FileInfo(file).Length;
                    total = total > long.MaxValue - length ? long.MaxValue : total + length;
                    if (count < int.MaxValue) count++;
                }
                return new DirectoryMeasurement(true, true, true, total, count,
                    count == 0 ? "目录为空" : "已完成目录测量");
            }, token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Storage analysis could not completely measure {ScopeName} directory", scopeName);
            return new DirectoryMeasurement(true, true, false, null, null, "读取不完整，大小未知");
        }
    }

    private static StorageBudgetEntryDto CreateBudgetEntry(
        string name,
        string scopeDisplay,
        DirectoryMeasurement measurement,
        bool hasLogicalMetric,
        string logicalMetricLabel,
        long? logicalBytes,
        int? logicalItemCount,
        string boundaryDisplay)
        => new()
        {
            Name = name,
            ScopeDisplay = scopeDisplay,
            StatusDisplay = measurement.StatusDisplay,
            BoundaryDisplay = boundaryDisplay,
            IsConfigured = measurement.IsConfigured,
            DirectoryAvailable = measurement.DirectoryAvailable,
            MeasurementComplete = measurement.MeasurementComplete,
            FileLengthBytes = measurement.FileLengthBytes,
            FileCount = measurement.FileCount,
            HasLogicalMetric = hasLogicalMetric,
            LogicalMetricLabel = logicalMetricLabel,
            LogicalBytes = logicalBytes,
            LogicalItemCount = logicalItemCount
        };

    private static void MarkOverlappingScopes(IReadOnlyList<StorageBudgetEntryDto> entries, IReadOnlyList<string> paths)
    {
        for (var left = 0; left < entries.Count; left++)
        for (var right = left + 1; right < entries.Count; right++)
        {
            if (string.IsNullOrWhiteSpace(paths[left]) || string.IsNullOrWhiteSpace(paths[right])
                || !PathsOverlap(paths[left], paths[right])) continue;
            entries[left].BoundaryDisplay += $" 路径与“{entries[right].Name}”相同或互相包含，文件可能重复；请勿相加。";
            entries[right].BoundaryDisplay += $" 路径与“{entries[left].Name}”相同或互相包含，文件可能重复；请勿相加。";
        }
    }

    private static bool PathsOverlap(string first, string second)
    {
        try
        {
            var firstFull = NormalizeDirectoryPath(first);
            var secondFull = NormalizeDirectoryPath(second);
            return string.Equals(firstFull, secondFull, StringComparison.OrdinalIgnoreCase)
                || firstFull.StartsWith(secondFull + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                || secondFull.StartsWith(firstFull + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static string NormalizeDirectoryPath(string path)
    {
        var full = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return full.Length == 0 ? Path.GetPathRoot(Path.GetFullPath(path)) ?? string.Empty : full;
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
        if (!result.BackupDirectoryAvailable)
            return $"存档目录不可用，存档目录体积未知；其余本地镜像、暂存和媒体分项仍分别显示。{result.MissingIndexedPathSummary}";
        var volume = string.IsNullOrWhiteSpace(result.VolumeRoot)
            ? "卷信息不可用"
            : $"卷 {result.VolumeRoot} 剩余 {result.VolumeFreeDisplay} / 共 {result.VolumeTotalDisplay}";
        var directorySize = result.RepositoryMeasurementComplete ? result.RepositoryBytesDisplay : "未知（目录读取不完整）";
        var footprint = $"索引 {result.BackupVersionCount} 个版本，索引逻辑体积 {result.IndexedBackupBytesDisplay}，目录文件长度 {directorySize}";
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

    private sealed record DirectoryMeasurement(
        bool IsConfigured,
        bool DirectoryAvailable,
        bool MeasurementComplete,
        long? FileLengthBytes,
        int? FileCount,
        string StatusDisplay);
}
