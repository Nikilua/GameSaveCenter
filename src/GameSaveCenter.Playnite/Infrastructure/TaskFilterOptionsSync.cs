using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Stable incremental sync for dynamic ComboBox option collections. It never clears or
    /// replaces the collection, so WPF never loses a currently selected item during a
    /// snapshot refresh; "全部" always stays at index 0 and values remain sorted.
    /// </summary>
    public static class TaskFilterOptionsSync
    {
        public static void Sync(ObservableCollection<string> target, IEnumerable<string> values)
        {
            var desired = values
                .Where(value => !string.IsNullOrWhiteSpace(value)
                                && !string.Equals(value.Trim(), "全部", StringComparison.OrdinalIgnoreCase))
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (target.Count == 0 || !string.Equals(target[0], "全部", StringComparison.Ordinal))
                target.Insert(0, "全部");

            var desiredSet = new HashSet<string>(desired, StringComparer.OrdinalIgnoreCase);
            for (var i = target.Count - 1; i >= 1; i--)
            {
                if (!desiredSet.Contains(target[i]))
                    target.RemoveAt(i);
            }

            var insertIndex = 1;
            foreach (var value in desired)
            {
                var existing = IndexOfFrom(target, value, insertIndex);
                if (existing < 0)
                {
                    target.Insert(insertIndex, value);
                    insertIndex++;
                }
                else
                {
                    insertIndex = existing + 1;
                }
            }
        }

        private static int IndexOfFrom(ObservableCollection<string> target, string value, int startIndex)
        {
            for (var i = startIndex; i < target.Count; i++)
            {
                if (string.Equals(target[i], value, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }
    }
}
