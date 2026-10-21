using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSaveCenter.Playnite.Settings
{
    /// <summary>Ephemeral snapshot passed between the production shell and plugin settings.</summary>
    internal sealed class CommandDirectoryPreferenceSnapshot
    {
        public CommandDirectoryPreferenceSnapshot(
            IEnumerable<string>? pinnedCommandIds,
            IEnumerable<string>? recentCommandIds)
        {
            PinnedCommandIds = CommandDirectoryPreferencePolicy.NormalizePinned(pinnedCommandIds);
            RecentCommandIds = CommandDirectoryPreferencePolicy.NormalizeRecent(recentCommandIds);
        }

        public IReadOnlyList<string> PinnedCommandIds { get; }
        public IReadOnlyList<string> RecentCommandIds { get; }
    }

    /// <summary>
    /// Normalizes the command directory's stable-ID-only preferences. Pin order is the
    /// user's insertion order; recent order is most recently invoked first.
    /// </summary>
    internal static class CommandDirectoryPreferencePolicy
    {
        public const int MaximumPinnedCommandCount = 5;
        public const int MaximumRecentCommandCount = 20;
        private const int MaximumCommandIdLength = 128;

        public static List<string> NormalizePinned(IEnumerable<string>? source)
            => NormalizeIds(source, MaximumPinnedCommandCount, null);

        public static List<string> NormalizeRecent(IEnumerable<string>? source)
            => NormalizeIds(source, MaximumRecentCommandCount, null);

        public static CommandDirectoryPreferenceSnapshot NormalizeForCatalog(
            IEnumerable<string>? pinnedCommandIds,
            IEnumerable<string>? recentCommandIds,
            IEnumerable<string> knownCommandIds)
        {
            if (knownCommandIds == null) throw new ArgumentNullException(nameof(knownCommandIds));
            var canonicalIds = BuildCanonicalIds(knownCommandIds);
            return new CommandDirectoryPreferenceSnapshot(
                NormalizeIds(pinnedCommandIds, MaximumPinnedCommandCount, canonicalIds),
                NormalizeIds(recentCommandIds, MaximumRecentCommandCount, canonicalIds));
        }

        public static bool TryTogglePinned(
            IEnumerable<string>? current,
            string? commandId,
            IEnumerable<string> knownCommandIds,
            out List<string> updated,
            out bool isPinned)
        {
            if (knownCommandIds == null) throw new ArgumentNullException(nameof(knownCommandIds));
            var canonicalIds = BuildCanonicalIds(knownCommandIds);
            var normalized = NormalizeIds(current, MaximumPinnedCommandCount, canonicalIds);
            var requestedId = (commandId ?? string.Empty).Trim();
            if (!canonicalIds.TryGetValue(requestedId, out var canonicalId))
            {
                updated = normalized;
                isPinned = false;
                return false;
            }

            var existingIndex = normalized.FindIndex(id =>
                string.Equals(id, canonicalId, StringComparison.OrdinalIgnoreCase));
            if (existingIndex >= 0)
            {
                normalized.RemoveAt(existingIndex);
                updated = normalized;
                isPinned = false;
                return true;
            }

            if (normalized.Count >= MaximumPinnedCommandCount)
            {
                updated = normalized;
                isPinned = false;
                return false;
            }

            normalized.Add(canonicalId);
            updated = normalized;
            isPinned = true;
            return true;
        }

        public static List<string> RecordRecent(
            IEnumerable<string>? current,
            string? commandId,
            IEnumerable<string> knownCommandIds)
        {
            if (knownCommandIds == null) throw new ArgumentNullException(nameof(knownCommandIds));
            var canonicalIds = BuildCanonicalIds(knownCommandIds);
            var normalized = NormalizeIds(current, MaximumRecentCommandCount, canonicalIds);
            var requestedId = (commandId ?? string.Empty).Trim();
            if (!canonicalIds.TryGetValue(requestedId, out var canonicalId))
                return normalized;

            normalized.RemoveAll(id => string.Equals(id, canonicalId, StringComparison.OrdinalIgnoreCase));
            normalized.Insert(0, canonicalId);
            if (normalized.Count > MaximumRecentCommandCount)
                normalized.RemoveRange(MaximumRecentCommandCount, normalized.Count - MaximumRecentCommandCount);
            return normalized;
        }

        public static bool HaveSameIds(IReadOnlyList<string>? left, IReadOnlyList<string>? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left == null || right == null || left.Count != right.Count) return false;
            for (var index = 0; index < left.Count; index++)
            {
                if (!string.Equals(left[index], right[index], StringComparison.Ordinal)) return false;
            }
            return true;
        }

        private static Dictionary<string, string> BuildCanonicalIds(IEnumerable<string> source)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var value in source)
            {
                var id = (value ?? string.Empty).Trim();
                if (!IsValidId(id) || result.ContainsKey(id)) continue;
                result.Add(id, id);
            }
            return result;
        }

        private static List<string> NormalizeIds(
            IEnumerable<string>? source,
            int maximumCount,
            IReadOnlyDictionary<string, string>? canonicalIds)
        {
            var result = new List<string>();
            if (source == null || maximumCount <= 0) return result;

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var value in source)
            {
                var id = (value ?? string.Empty).Trim();
                if (!IsValidId(id)) continue;
                if (canonicalIds != null)
                {
                    if (!canonicalIds.TryGetValue(id, out var canonicalId)) continue;
                    id = canonicalId;
                }
                if (!seen.Add(id)) continue;
                result.Add(id);
                if (result.Count >= maximumCount) break;
            }
            return result;
        }

        private static bool IsValidId(string value)
            => value.Length > 0
               && value.Length <= MaximumCommandIdLength
               && !value.Any(char.IsWhiteSpace);
    }
}
