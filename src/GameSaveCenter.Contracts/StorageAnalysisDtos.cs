using System;
using System.Collections.Generic;

namespace GameSaveCenter.Contracts
{
    /// <summary>Read-only backup storage analysis returned by the Worker.</summary>
    public sealed class StorageAnalysisDto
    {
        public DateTime CheckedUtc { get; set; } = DateTime.UtcNow;
        public bool BackupDirectoryAvailable { get; set; }
        public string VolumeRoot { get; set; } = string.Empty;
        public long VolumeTotalBytes { get; set; }
        public long VolumeFreeBytes { get; set; }
        public long VolumeUsedBytes => Math.Max(0, VolumeTotalBytes - VolumeFreeBytes);
        public long RepositoryBytes { get; set; }
        public bool RepositoryMeasurementComplete { get; set; }
        public long IndexedBackupBytes { get; set; }
        public int BackupVersionCount { get; set; }
        public int MissingIndexedPathCount { get; set; }
        public long MissingIndexedBytes { get; set; }
        public int GrowthObservationSpanDays { get; set; }
        public int GrowthObservationDayCount { get; set; }
        public long? HistoricalGrowthMinimumBytesPer30Days { get; set; }
        public long? HistoricalGrowthMaximumBytesPer30Days { get; set; }
        public List<StorageTrendDto> Trends { get; set; } = new List<StorageTrendDto>();
        public List<StorageGameRankDto> TopGames { get; set; } = new List<StorageGameRankDto>();
        public List<StorageBudgetEntryDto> BudgetBreakdown { get; set; } = new List<StorageBudgetEntryDto>();
        public string PredictionSummary { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;

        public string VolumeTotalDisplay => FormatBytes(VolumeTotalBytes);
        public string VolumeFreeDisplay => FormatBytes(VolumeFreeBytes);
        public string VolumeUsedDisplay => FormatBytes(VolumeUsedBytes);
        public string RepositoryBytesDisplay => RepositoryMeasurementComplete ? FormatBytes(RepositoryBytes) : "未知";
        public string IndexedBackupBytesDisplay => FormatBytes(IndexedBackupBytes);
        public string MissingIndexedBytesDisplay => FormatBytes(MissingIndexedBytes);
        public bool HasHistoricalGrowthRange => HistoricalGrowthMinimumBytesPer30Days.HasValue
            && HistoricalGrowthMaximumBytesPer30Days.HasValue;
        public long HistoricalGrowthChartMaximumBytesPer30Days => Math.Max(1, HistoricalGrowthMaximumBytesPer30Days ?? 0);
        public string HistoricalGrowthMinimumDisplay => HistoricalGrowthMinimumBytesPer30Days.HasValue
            ? FormatBytes(HistoricalGrowthMinimumBytesPer30Days.Value)
            : "未知";
        public string HistoricalGrowthMaximumDisplay => HistoricalGrowthMaximumBytesPer30Days.HasValue
            ? FormatBytes(HistoricalGrowthMaximumBytesPer30Days.Value)
            : "未知";
        public string HistoricalGrowthRangeDisplay => HasHistoricalGrowthRange
            ? $"近 90 天样本覆盖 {GrowthObservationSpanDays} 天、{GrowthObservationDayCount} 个有新增日期；7/30/90 天滚动窗口折算的逻辑新增观察范围为 {HistoricalGrowthMinimumDisplay}–{HistoricalGrowthMaximumDisplay} / 30 天（不是置信区间）。"
            : $"历史样本不足，逻辑新增范围未知：近 90 天样本覆盖 {GrowthObservationSpanDays} 天、{GrowthObservationDayCount} 个有新增日期；至少需要覆盖 30 天且包含 3 个日期。";
        public string RemoteCapacitySummary => "远端可用容量未知：当前没有读取服务端配额或总容量；不纳入本地卷统计。";
        public string BudgetBreakdownSummary => "各项独立展示，不生成合计；镜像通常是存档副本，目录包含关系也可能造成重复。文件长度合计不等于磁盘簇实占，不能把分项相加。";
        public string MissingIndexedPathSummary => !BackupDirectoryAvailable
            ? "备份目录不可用，路径状态未知，不能按 0 解释。"
            : MissingIndexedPathCount <= 0
                ? "已索引版本的归档路径均可核查。"
                : $"{MissingIndexedPathCount} 个索引版本的归档路径失联；逻辑体积 {MissingIndexedBytesDisplay} 未计入磁盘实测，不代表占用为 0，请到存档中心核对。";

        private static string FormatBytes(long bytes) => ByteSizeFormatter.Format(bytes);
    }

    /// <summary>One independently measured local storage scope; categories are never added together.</summary>
    public sealed class StorageBudgetEntryDto
    {
        public string Name { get; set; } = string.Empty;
        public string ScopeDisplay { get; set; } = string.Empty;
        public string StatusDisplay { get; set; } = string.Empty;
        public string BoundaryDisplay { get; set; } = string.Empty;
        public bool IsConfigured { get; set; }
        public bool DirectoryAvailable { get; set; }
        public bool MeasurementComplete { get; set; }
        public long? FileLengthBytes { get; set; }
        public int? FileCount { get; set; }
        public bool HasLogicalMetric { get; set; }
        public string LogicalMetricLabel { get; set; } = string.Empty;
        public long? LogicalBytes { get; set; }
        public int? LogicalItemCount { get; set; }

        public string FileLengthDisplay => FileLengthBytes.HasValue ? FormatBytes(FileLengthBytes.Value) : "未知";
        public string FileCountDisplay => FileCount.HasValue ? $"{FileCount.Value} 个文件" : "数量未知";
        public string LogicalBytesDisplay => !HasLogicalMetric
            ? "未统计"
            : LogicalBytes.HasValue ? FormatBytes(LogicalBytes.Value) : "未知";
        public string LogicalMetricLabelDisplay => HasLogicalMetric ? LogicalMetricLabel : "索引逻辑大小";
        public string LogicalItemCountDisplay => !HasLogicalMetric
            ? string.Empty
            : LogicalItemCount.HasValue ? $"{LogicalItemCount.Value} 条记录" : "记录数未知";

        private static string FormatBytes(long bytes) => ByteSizeFormatter.Format(bytes);
    }

    /// <summary>Added indexed backup volume inside a rolling window.</summary>
    public sealed class StorageTrendDto
    {
        public int Days { get; set; }
        public long AddedBytes { get; set; }
        public int AddedVersionCount { get; set; }

        public string AddedBytesDisplay => FormatBytes(AddedBytes);

        private static string FormatBytes(long bytes) => ByteSizeFormatter.Format(bytes);
    }

    /// <summary>One game's indexed backup footprint for the storage leaderboard.</summary>
    public sealed class StorageGameRankDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public int BackupCount { get; set; }
        public long BackupBytes { get; set; }
        public string LatestBackupId { get; set; } = string.Empty;
        public DateTime? LatestBackupUtc { get; set; }

        public string BackupBytesDisplay => FormatBytes(BackupBytes);
        public string LatestBackupDisplay => LatestBackupUtc?.ToLocalTime().ToString("MM-dd HH:mm") ?? "—";
        public string LatestBackupRelativeDisplay => LatestBackupUtc.HasValue
            ? TimeDisplayFormatter.Relative(LatestBackupUtc.Value, DateTime.UtcNow)
            : "时间未知";
        public string LatestBackupFullDisplay => LatestBackupUtc.HasValue
            ? TimeDisplayFormatter.Full(LatestBackupUtc.Value)
            : "时间未知";
        public string LatestBackupRawUtcDisplay => TimeDisplayFormatter.RawUtc(LatestBackupUtc ?? DateTime.MinValue);
        public string LatestBackupIdDisplay => string.IsNullOrWhiteSpace(LatestBackupId) ? "版本不可用" : LatestBackupId;

        private static string FormatBytes(long bytes) => ByteSizeFormatter.Format(bytes);
    }
}
