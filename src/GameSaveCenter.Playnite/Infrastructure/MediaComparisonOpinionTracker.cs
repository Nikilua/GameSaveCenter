using System;
using System.Collections.Generic;

namespace GameSaveCenter.Playnite.Infrastructure
{
    public enum MediaComparisonOpinionKind
    {
        Keep,
        Review
    }

    /// <summary>
    /// Holds reversible, in-memory human opinions for one media comparison session.
    /// These marks are intentionally separate from media metadata and file operations.
    /// </summary>
    public sealed class MediaComparisonOpinionTracker
    {
        private readonly Dictionary<string, MediaComparisonOpinionKind> opinions =
            new Dictionary<string, MediaComparisonOpinionKind>(StringComparer.OrdinalIgnoreCase);

        public int CountToKeep => Count(MediaComparisonOpinionKind.Keep);
        public int CountToReview => Count(MediaComparisonOpinionKind.Review);

        public MediaComparisonOpinionKind? Get(string? mediaId)
        {
            if (string.IsNullOrWhiteSpace(mediaId))
                return null;

            return opinions.TryGetValue(mediaId!, out var opinion) ? opinion : (MediaComparisonOpinionKind?)null;
        }

        public bool Set(string? mediaId, MediaComparisonOpinionKind opinion)
        {
            if (string.IsNullOrWhiteSpace(mediaId) || !Enum.IsDefined(typeof(MediaComparisonOpinionKind), opinion))
                return false;
            if (Get(mediaId) == opinion)
                return false;

            opinions[mediaId!] = opinion;
            return true;
        }

        public bool Clear(string? mediaId)
        {
            if (string.IsNullOrWhiteSpace(mediaId))
                return false;

            return opinions.Remove(mediaId!);
        }

        public void Reset()
            => opinions.Clear();

        public string Summary
            => $"本次比较意见：待保留 {CountToKeep} 项 · 待检查 {CountToReview} 项";

        private int Count(MediaComparisonOpinionKind opinion)
        {
            var count = 0;
            foreach (var value in opinions.Values)
            {
                if (value == opinion)
                    count++;
            }

            return count;
        }
    }
}
