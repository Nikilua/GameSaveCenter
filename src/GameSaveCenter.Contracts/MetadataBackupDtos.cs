using System;

namespace GameSaveCenter.Contracts
{
    /// <summary>Result of exporting GameSaveCenter's own metadata for disaster recovery.</summary>
    public sealed class MetadataBackupResultDto
    {
        public string PackagePath { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public long PackageBytes { get; set; }
        public int IncludedFileCount { get; set; }
        public string Summary { get; set; } = string.Empty;
    }
}
