using System;
using System.Collections.Generic;
using System.Linq;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Navigates the currently loaded, filtered media window without changing the
    /// detail selection or the locked comparison reference.
    /// </summary>
    public sealed class MediaComparisonCandidateNavigator
    {
        private readonly HashSet<string> unavailableMediaIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public int CountCandidates(IEnumerable<MediaItemDto>? results, string? referenceMediaId)
            => GetCandidates(results, referenceMediaId).Count(item => !unavailableMediaIds.Contains(item.MediaId));

        public bool CanMove(IEnumerable<MediaItemDto>? results, string? currentMediaId, string? referenceMediaId, int direction)
        {
            ValidateDirection(direction);
            var candidates = GetCandidates(results, referenceMediaId);
            var currentIndex = FindIndex(candidates, currentMediaId);
            var index = currentIndex < 0 ? (direction > 0 ? 0 : candidates.Count - 1) : currentIndex + direction;
            for (; index >= 0 && index < candidates.Count; index += direction)
            {
                if (!unavailableMediaIds.Contains(candidates[index].MediaId))
                    return true;
            }

            return false;
        }

        public MediaItemDto? FindAdjacent(
            IEnumerable<MediaItemDto>? results,
            string? currentMediaId,
            string? referenceMediaId,
            int direction,
            Func<string, bool>? fileExists,
            out int skippedUnavailable)
        {
            ValidateDirection(direction);
            var candidates = GetCandidates(results, referenceMediaId);
            var currentIndex = FindIndex(candidates, currentMediaId);
            var index = currentIndex < 0 ? (direction > 0 ? 0 : candidates.Count - 1) : currentIndex + direction;
            var exists = fileExists ?? System.IO.File.Exists;
            skippedUnavailable = 0;

            for (; index >= 0 && index < candidates.Count; index += direction)
            {
                var candidate = candidates[index];
                if (unavailableMediaIds.Contains(candidate.MediaId)
                    || string.IsNullOrWhiteSpace(candidate.ArchivePath)
                    || !exists(candidate.ArchivePath))
                {
                    skippedUnavailable++;
                    continue;
                }

                return candidate;
            }

            return null;
        }

        public void MarkUnavailable(string? mediaId)
        {
            if (!string.IsNullOrWhiteSpace(mediaId))
                unavailableMediaIds.Add(mediaId!);
        }

        public void ResetUnavailable()
            => unavailableMediaIds.Clear();

        private static List<MediaItemDto> GetCandidates(IEnumerable<MediaItemDto>? results, string? referenceMediaId)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var candidates = new List<MediaItemDto>();
            foreach (var item in results ?? Enumerable.Empty<MediaItemDto>())
            {
                var mediaId = item?.MediaId;
                if (item == null
                    || item.Kind != MediaKind.Screenshot
                    || string.IsNullOrWhiteSpace(mediaId)
                    || string.Equals(mediaId, referenceMediaId, StringComparison.OrdinalIgnoreCase)
                    || !seen.Add(mediaId!))
                    continue;

                candidates.Add(item);
            }

            return candidates;
        }

        private static int FindIndex(IReadOnlyList<MediaItemDto> candidates, string? mediaId)
        {
            if (string.IsNullOrWhiteSpace(mediaId))
                return -1;
            for (var index = 0; index < candidates.Count; index++)
            {
                if (string.Equals(candidates[index].MediaId, mediaId, StringComparison.OrdinalIgnoreCase))
                    return index;
            }

            return -1;
        }

        private static void ValidateDirection(int direction)
        {
            if (direction != -1 && direction != 1)
                throw new ArgumentOutOfRangeException(nameof(direction), "候选方向必须为 -1 或 1。");
        }
    }
}
