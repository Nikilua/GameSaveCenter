using System;
using System.Collections.Generic;

namespace GameSaveCenter.Contracts;

/// <summary>Server-side query for one media collection.</summary>
public sealed class MediaQueryDto
{
    public string PlayniteId { get; set; } = string.Empty;
    public string Search { get; set; } = string.Empty;
    public MediaKind? Kind { get; set; }
    public bool FavoriteOnly { get; set; }
    public int Limit { get; set; } = 200;
    /// <summary>Opaque cursor returned by the previous page.</summary>
    public string Cursor { get; set; } = string.Empty;
}

/// <summary>One stable, cursor-paginated media response.</summary>
public sealed class MediaPageDto
{
    public List<MediaItemDto> Items { get; set; } = new List<MediaItemDto>();
    public int TotalCount { get; set; }
    public bool HasMore { get; set; }
    public string NextCursor { get; set; } = string.Empty;
}
