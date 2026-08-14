using System;

namespace GameSaveCenter.Contracts
{
    /// <summary>Request containing the portable Playnite plugin settings snapshot.</summary>
    public sealed class MetadataBackupCreateRequestDto
    {
        public string PluginSettingsJson { get; set; } = string.Empty;
    }

    /// <summary>Result of exporting GameSaveCenter's own metadata for disaster recovery.</summary>
public sealed class MetadataBackupResultDto
{
    public string PackagePath { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public long PackageBytes { get; set; }
    public int IncludedFileCount { get; set; }
    public bool PluginSettingsIncluded { get; set; }
    public string Summary { get; set; } = string.Empty;
}

public sealed class MetadataRestorePreviewDto
{
    public bool Valid { get; set; }
    public string PackagePath { get; set; } = string.Empty;
    public int SchemaVersion { get; set; }
    public string DatabaseSha256 { get; set; } = string.Empty;
    public string SettingsSha256 { get; set; } = string.Empty;
    public string PluginSettingsSha256 { get; set; } = string.Empty;
    public string PluginSettingsJson { get; set; } = string.Empty;
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
    public string PluginSettingsJson { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}

public sealed class MetadataRestoreRollbackRequestDto
{
    public string PreRestorePath { get; set; } = string.Empty;
    public bool Confirmed { get; set; }
}

public sealed class MetadataRestoreRollbackResultDto
{
    public bool RolledBack { get; set; }
    public string Summary { get; set; } = string.Empty;
}
}
