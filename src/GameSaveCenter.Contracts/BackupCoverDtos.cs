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
