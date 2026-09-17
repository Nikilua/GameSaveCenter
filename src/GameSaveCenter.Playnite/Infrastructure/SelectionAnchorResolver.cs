using System;
using System.Collections.Generic;

namespace GameSaveCenter.Playnite.Infrastructure;

/// <summary>
/// Restores a list selection by stable identity first, then by the old row position.
/// The positional fallback is deliberately clamped so deleting the last row selects
/// its new predecessor instead of jumping to the first row.
/// </summary>
public static class SelectionAnchorResolver
{
    public static T? Restore<T>(
        IList<T> items,
        string? stableId,
        int previousIndex,
        Func<T, string?> keySelector)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));
        if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

        if (!string.IsNullOrWhiteSpace(stableId))
        {
            for (var index = 0; index < items.Count; index++)
            {
                if (string.Equals(keySelector(items[index]), stableId, StringComparison.OrdinalIgnoreCase))
                    return items[index];
            }
        }

        if (items.Count == 0)
            return default;

        var fallbackIndex = previousIndex < 0
            ? 0
            : Math.Min(previousIndex, items.Count - 1);
        return items[fallbackIndex];
    }
}
