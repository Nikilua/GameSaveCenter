using System;
using System.Collections.Generic;

namespace GameSaveCenter.Contracts;

public sealed class BackupCalendarQueryDto
{
    public string PlayniteId { get; set; } = string.Empty;
    public List<DateTime> UtcDayBoundaries { get; set; } = new List<DateTime>();
}

public sealed class BackupCalendarDayDto
{
    public int Day { get; set; }
    public int Count { get; set; }
}
