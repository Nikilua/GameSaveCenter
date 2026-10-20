using System.Collections.Generic;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class MediaComparisonCandidateNavigatorTests
    {
        [Fact]
        public void MovesWithinFilteredScreenshotsAndSkipsMissingOrRejectedCandidates()
        {
            var reference = Screenshot("reference", "reference.png");
            var current = Screenshot("current", "current.png");
            var missing = Screenshot("missing", "missing.png");
            var next = Screenshot("next", "next.png");
            var last = Screenshot("last", "last.png");
            var results = new List<MediaItemDto>
            {
                reference,
                current,
                new MediaItemDto { MediaId = "video", Kind = MediaKind.VideoClip, ArchivePath = "clip.mp4" },
                missing,
                next,
                last
            };
            var navigator = new MediaComparisonCandidateNavigator();
            var existing = new HashSet<string> { "current.png", "next.png", "last.png" };

            var candidate = navigator.FindAdjacent(results, "current", "reference", 1, existing.Contains, out var skipped);

            Assert.Same(next, candidate);
            Assert.Equal(1, skipped);
            Assert.Equal(4, navigator.CountCandidates(results, "reference"));
            Assert.True(navigator.CanMove(results, "current", "reference", 1));

            navigator.MarkUnavailable("next");
            candidate = navigator.FindAdjacent(results, "current", "reference", 1, existing.Contains, out skipped);
            Assert.Same(last, candidate);
            Assert.Equal(2, skipped);

            candidate = navigator.FindAdjacent(results, "last", "reference", -1, existing.Contains, out skipped);
            Assert.Same(current, candidate);
            Assert.Equal(2, skipped);
        }

        [Fact]
        public void DoesNotNavigateOutsideCurrentResultsOrReplaceLockedReference()
        {
            var reference = Screenshot("reference", "reference.png");
            var current = Screenshot("current", "current.png");
            var results = new[] { reference, current };
            var navigator = new MediaComparisonCandidateNavigator();

            Assert.False(navigator.CanMove(results, "current", "reference", 1));
            Assert.False(navigator.CanMove(results, "current", "reference", -1));
            Assert.Null(navigator.FindAdjacent(results, "current", "reference", 1, _ => true, out _));
            Assert.Null(navigator.FindAdjacent(results, "current", "reference", -1, _ => true, out _));
        }

        private static MediaItemDto Screenshot(string id, string path)
            => new MediaItemDto { MediaId = id, Kind = MediaKind.Screenshot, ArchivePath = path };
    }
}
