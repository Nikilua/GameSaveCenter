using System;

namespace GameSaveCenter.Contracts
{
    /// <summary>Read-only status of the optional second local mirror.</summary>
    public sealed class LocalMirrorStatusDto
    {
        public bool Enabled { get; set; }
        public bool Available { get; set; }
        public string MirrorPath { get; set; } = string.Empty;
        public DateTime? LastSyncUtc { get; set; }
        public int CopiedCount { get; set; }
        public int VerifiedCount { get; set; }
        public long TotalBytes { get; set; }
        public string Message { get; set; } = string.Empty;

        public string LastSyncDisplay => LastSyncUtc?.ToLocalTime().ToString("yyyy-MM-dd HH:mm") ?? "尚未同步";
        public string AvailableDisplay => Available ? "可用" : "不可用";
        public string TotalBytesDisplay => FormatBytes(TotalBytes);

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes:0} B";
            if (bytes < 1024L * 1024) return $"{bytes / 1024d:0.##} KiB";
            if (bytes < 1024L * 1024 * 1024) return $"{bytes / 1024d / 1024d:0.##} MiB";
            return $"{bytes / 1024d / 1024d / 1024d:0.##} GiB";
        }
    }

    /// <summary>Result of a user-initiated mirror sync. Mirror-only files are never deleted.</summary>
    public sealed class LocalMirrorSyncResultDto
    {
        public int CopiedCount { get; set; }
        public int VerifiedCount { get; set; }
        public int SkippedCount { get; set; }
        public long TotalBytes { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
