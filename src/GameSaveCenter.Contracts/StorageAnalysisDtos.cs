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
        public long IndexedBackupBytes { get; set; }
        public int BackupVersionCount { get; set; }
        public List<StorageTrendDto> Trends { get; set; } = new List<StorageTrendDto>();
        public List<StorageGameRankDto> TopGames { get; set; } = new List<StorageGameRankDto>();
        public string PredictionSummary { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;

        public string VolumeTotalDisplay => FormatBytes(VolumeTotalBytes);
        public string VolumeFreeDisplay => FormatBytes(VolumeFreeBytes);
        public string VolumeUsedDisplay => FormatBytes(VolumeUsedBytes);
        public string RepositoryBytesDisplay => FormatBytes(RepositoryBytes);
        public string IndexedBackupBytesDisplay => FormatBytes(IndexedBackupBytes);

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes:0} B";
            if (bytes < 1024L * 1024) return $"{bytes / 1024d:0.##} KiB";
            if (bytes < 1024L * 1024 * 1024) return $"{bytes / 1024d / 1024d:0.##} MiB";
            return $"{bytes / 1024d / 1024d / 1024d:0.##} GiB";
        }
    }

    /// <summary>Added indexed backup volume inside a rolling window.</summary>
    public sealed class StorageTrendDto
    {
        public int Days { get; set; }
        public long AddedBytes { get; set; }
        public int AddedVersionCount { get; set; }

        public string AddedBytesDisplay => FormatBytes(AddedBytes);

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes:0} B";
            if (bytes < 1024L * 1024) return $"{bytes / 1024d:0.##} KiB";
            if (bytes < 1024L * 1024 * 1024) return $"{bytes / 1024d / 1024d:0.##} MiB";
            return $"{bytes / 1024d / 1024d / 1024d:0.##} GiB";
        }
    }

    /// <summary>One game's indexed backup footprint for the storage leaderboard.</summary>
    public sealed class StorageGameRankDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public int BackupCount { get; set; }
        public long BackupBytes { get; set; }
        public DateTime? LatestBackupUtc { get; set; }

        public string BackupBytesDisplay => FormatBytes(BackupBytes);
        public string LatestBackupDisplay => LatestBackupUtc?.ToLocalTime().ToString("MM-dd HH:mm") ?? "—";

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes:0} B";
            if (bytes < 1024L * 1024) return $"{bytes / 1024d:0.##} KiB";
            if (bytes < 1024L * 1024 * 1024) return $"{bytes / 1024d / 1024d:0.##} MiB";
            return $"{bytes / 1024d / 1024d / 1024d:0.##} GiB";
        }
    }
}
