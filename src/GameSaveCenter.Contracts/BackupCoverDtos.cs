using System.Collections.Generic;

namespace GameSaveCenter.Contracts;

public enum BackupCoverState
{
    None,
    Available,
    Offline,
    Reclassified
}

public sealed class BackupCoverDto
{
    public string MediaId { get; set; } = string.Empty;
    public string ArchivePath { get; set; } = string.Empty;
    public BackupCoverState State { get; set; }
}

public sealed class BackupCoverQueryDto
{
    public string PlayniteId { get; set; } = string.Empty;
    public string BackupId { get; set; } = string.Empty;
}

public sealed class BackupCoverUpdateDto
{
    public string PlayniteId { get; set; } = string.Empty;
    public string BackupId { get; set; } = string.Empty;
    public string MediaId { get; set; } = string.Empty;
}

public sealed class BackupCoverMediaQueryDto
{
    public string PlayniteId { get; set; } = string.Empty;
    public string MediaId { get; set; } = string.Empty;
    public string BackupId { get; set; } = string.Empty;
    public string BeforeCreatedUtcKey { get; set; } = string.Empty;
    public string BeforeBackupId { get; set; } = string.Empty;
}

public sealed class BackupCoverGalleryQueryDto
{
    public string PlayniteId { get; set; } = string.Empty;
    public string BeforeCreatedUtcKey { get; set; } = string.Empty;
    public string BeforeBackupId { get; set; } = string.Empty;
}

public sealed class BackupCoverGalleryEntryDto
{
    public BackupVersionDto Backup { get; set; } = new BackupVersionDto();
    public BackupCoverDto Cover { get; set; } = new BackupCoverDto();
}

public sealed class BackupCoverGalleryPageDto
{
    public List<BackupCoverGalleryEntryDto> Items { get; set; } = new List<BackupCoverGalleryEntryDto>();
    public int TotalCount { get; set; }
    public bool HasMore { get; set; }
    public string NextCreatedUtcKey { get; set; } = string.Empty;
    public string NextBackupId { get; set; } = string.Empty;
}
