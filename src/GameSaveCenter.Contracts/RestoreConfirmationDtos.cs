using System.Collections.Generic;

namespace GameSaveCenter.Contracts;

public sealed class RestoreConfirmationPreviewQueryDto
{
    public string PlayniteId { get; set; } = string.Empty;
    public string BackupId { get; set; } = string.Empty;
}

public sealed class RestoreConfirmationFileDto
{
    public string Path { get; set; } = string.Empty;
    public string Change { get; set; } = string.Empty;
}

public sealed class RestoreConfirmationPreviewDto
{
    public bool HasFileList { get; set; }
    public int? AddedCount { get; set; }
    public int? ModifiedCount { get; set; }
    public int? RemovedCount { get; set; }
    public int FileCount { get; set; }
    public List<string> TargetDirectories { get; set; } = new List<string>();
    public List<RestoreConfirmationFileDto> Files { get; set; } = new List<RestoreConfirmationFileDto>();
}
