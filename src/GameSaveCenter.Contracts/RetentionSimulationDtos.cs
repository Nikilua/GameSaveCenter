using System;
using System.Collections.Generic;

namespace GameSaveCenter.Contracts
{
    /// <summary>Read-only global retention simulation returned by the Worker.</summary>
    public sealed class RetentionSimulationPreviewDto
    {
        /// <summary>Opaque Worker-owned handle for this exact preview snapshot.</summary>
        public string PreviewId { get; set; } = string.Empty;
        public DateTime GeneratedUtc { get; set; } = DateTime.UtcNow;
        public int ExistingVersionCount { get; set; }
        public int KeepVersionCount { get; set; }
        public int DeleteCandidateCount { get; set; }
        public int UserLockedCount { get; set; }
        public int HealthProtectedCount { get; set; }
        public int PreRestoreCount { get; set; }
        public long EstimatedReleaseBytes { get; set; }
        public string Summary { get; set; } = string.Empty;
        public List<RetentionSimulationItemDto> Items { get; set; } = new List<RetentionSimulationItemDto>();

        public string EstimatedReleaseDisplay => FormatBytes(EstimatedReleaseBytes);

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes:0} B";
            if (bytes < 1024L * 1024) return $"{bytes / 1024d:0.##} KiB";
            if (bytes < 1024L * 1024 * 1024) return $"{bytes / 1024d / 1024d:0.##} MiB";
            return $"{bytes / 1024d / 1024d / 1024d:0.##} GiB";
        }
    }

    /// <summary>One candidate row in the retention simulation. Deletion is never implied by preview.</summary>
    public sealed class RetentionSimulationItemDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public string BackupId { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; }
        public long TotalBytes { get; set; }
        public string ArchivePath { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
        public bool IsPreRestore { get; set; }
        public bool IsHealthProtected { get; set; }

        public string CreatedDisplay => CreatedUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
        public string TotalBytesDisplay => FormatBytes(TotalBytes);

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes:0} B";
            if (bytes < 1024L * 1024) return $"{bytes / 1024d:0.##} KiB";
            if (bytes < 1024L * 1024 * 1024) return $"{bytes / 1024d / 1024d:0.##} MiB";
            return $"{bytes / 1024d / 1024d / 1024d:0.##} GiB";
        }
    }

    /// <summary>Request to apply the simulated cleanup after an explicit second confirmation.</summary>
    public sealed class RetentionSimulationApplyRequestDto
    {
        public bool Confirmed { get; set; }
        /// <summary>Worker-generated preview handle. The apply operation never trusts client-supplied candidate counts.</summary>
        public string PreviewId { get; set; } = string.Empty;
        /// <summary>UTC timestamp of the read-only preview the user confirmed.</summary>
        public DateTime PreviewGeneratedUtc { get; set; }
        /// <summary>Legacy display value retained for wire compatibility; the Worker does not use it for authorization.</summary>
        public int ExpectedCandidateCount { get; set; }
        /// <summary>Legacy display value retained for wire compatibility; the Worker does not use it for authorization.</summary>
        public long ExpectedReleaseBytes { get; set; }
    }

    /// <summary>Result of applying a user-confirmed retention cleanup.</summary>
    public sealed class RetentionSimulationResultDto
    {
        public int DeletedCount { get; set; }
        public int SkippedProtectedCount { get; set; }
        public int SkippedMissingCount { get; set; }
        public int SkippedUnsupportedCount { get; set; }
        public int SkippedBusyCount { get; set; }
        public int SkippedChangedCount { get; set; }
        public int FailedCount { get; set; }
        public int PendingQuarantineCount { get; set; }
        public long PendingQuarantineBytes { get; set; }
        public long FreedBytes { get; set; }
        public string Summary { get; set; } = string.Empty;
    }
}
