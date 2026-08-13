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

public sealed class MetadataRestorePreviewDto
{
    public bool Valid { get; set; }
    public string PackagePath { get; set; } = string.Empty;
    public int SchemaVersion { get; set; }
    public string DatabaseSha256 { get; set; } = string.Empty;
    public string SettingsSha256 { get; set; } = string.Empty;
    public List<string> Entries { get; set; } = new List<string>();
    public string Summary { get; set; } = string.Empty;
}

public sealed class MetadataRestoreRequestDto
{
    public string PackagePath { get; set; } = string.Empty;
    public bool Confirmed { get; set; }
}

public sealed class MetadataRestoreResultDto
{
    public bool Restored { get; set; }
    public string PreRestorePath { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}
}
