using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSaveCenter.Playnite.Views
{
    internal static class CommandDirectorySearch
    {
        public const int EmptyQueryResultLimit = 8;

        public static IReadOnlyList<CommandDirectoryItem> Filter(
            IEnumerable<CommandDirectoryItem> items,
            string? query)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            if (string.IsNullOrWhiteSpace(query))
                return items.Take(EmptyQueryResultLimit).ToArray();

            var term = query!.Trim();
            return items.Where(item => Matches(item, term)).ToArray();
        }

        private static bool Matches(CommandDirectoryItem item, string term)
        {
            if (item.Name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0
                || item.Category.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            return item.Aliases.Any(alias => alias.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
