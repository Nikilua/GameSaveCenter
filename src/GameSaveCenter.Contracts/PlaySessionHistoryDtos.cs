using System;
using System.Collections.Generic;

namespace GameSaveCenter.Contracts;

public sealed class PlaySessionHistoryQueryDto
{
    public string PlayniteId { get; set; } = string.Empty;
    public string BeforeStoppedUtcKey { get; set; } = string.Empty;
    public string BeforeSessionId { get; set; } = string.Empty;
}

public sealed class PlaySessionHistoryEntryDto
{
    public string SessionId { get; set; } = string.Empty;
    public string PlayniteId { get; set; } = string.Empty;
    public DateTime StartedUtc { get; set; }
    public DateTime StoppedUtc { get; set; }
    public long ElapsedSeconds { get; set; }
    public List<TaskStatusDto> Tasks { get; set; } = new List<TaskStatusDto>();
}

public sealed class PlaySessionHistoryPageDto
{
    public List<PlaySessionHistoryEntryDto> Items { get; set; } = new List<PlaySessionHistoryEntryDto>();
    public string NextStoppedUtcKey { get; set; } = string.Empty;
    public string NextSessionId { get; set; } = string.Empty;
    public bool HasMore { get; set; }
}

public sealed class GameMediaIdentityDto
{
    public string PlayniteId { get; set; } = string.Empty;
    public string MediaId { get; set; } = string.Empty;
}
