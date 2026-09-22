using System;
using System.Collections.Generic;
using System.Linq;

using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>Local-calendar range rules for the Save history view.</summary>
    internal static class BackupHistoryDateRange
    {
        internal const string All = "全部时间";
        internal const string Today = "今天";
        internal const string Yesterday = "昨天";
        internal const string Last7Days = "近7天";
        internal const string Last30Days = "近30天";

        internal static IReadOnlyList<string> Options { get; } = new[]
        {
            All, Today, Yesterday, Last7Days, Last30Days
        };

        internal static bool Matches(BackupVersionDto version, string range, DateTime localNow)
        {
            if (version == null) return false;
            if (string.Equals(range, All, StringComparison.Ordinal)) return true;
            if (version.CreatedUtc == DateTime.MinValue) return false;
            if (!TryResolve(range, localNow, out var from, out var to)) return true;

            var localDate = version.CreatedLocal.Date;
            return localDate >= from && localDate <= to;
        }

        internal static string Describe(string range, DateTime localNow)
        {
            if (string.Equals(range, All, StringComparison.Ordinal)) return All;
            if (!TryResolve(range, localNow, out var from, out var to)) return All;
            return from == to
                ? $"本地日期 {from:yyyy-MM-dd}"
                : $"本地日期 {from:yyyy-MM-dd} 至 {to:yyyy-MM-dd}";
        }

        internal static IEnumerable<BackupVersionDto> OrderForNavigation(
            IEnumerable<BackupVersionDto> source,
            bool recentFirst)
        {
            if (source == null) return Enumerable.Empty<BackupVersionDto>();

            var known = source.Where(item => item != null && item.CreatedUtc != DateTime.MinValue);
            var unknown = source.Where(item => item != null && item.CreatedUtc == DateTime.MinValue)
                .OrderBy(item => item.BackupId, StringComparer.OrdinalIgnoreCase);
            var orderedKnown = recentFirst
                ? known.OrderByDescending(item => item.CreatedUtc).ThenBy(item => item.BackupId, StringComparer.OrdinalIgnoreCase)
                : known.OrderBy(item => item.CreatedUtc).ThenBy(item => item.BackupId, StringComparer.OrdinalIgnoreCase);

            return orderedKnown.Concat(unknown);
        }

        private static bool TryResolve(string range, DateTime localNow, out DateTime from, out DateTime to)
        {
            var today = localNow.Date;
            switch (range)
            {
                case Today:
                    from = today;
                    to = today;
                    return true;
                case Yesterday:
                    from = today.AddDays(-1);
                    to = from;
                    return true;
                case Last7Days:
                    from = today.AddDays(-6);
                    to = today;
                    return true;
                case Last30Days:
                    from = today.AddDays(-29);
                    to = today;
                    return true;
                default:
                    from = default;
                    to = default;
                    return false;
            }
        }
    }
}
