using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class MediaComparisonOpinionTrackerTests
    {
        [Fact]
        public void OpinionUsesStableMediaIdCanBeReplacedAndCanBeUndone()
        {
            var tracker = new MediaComparisonOpinionTracker();

            Assert.True(tracker.Set("media-a", MediaComparisonOpinionKind.Keep));
            Assert.Equal(MediaComparisonOpinionKind.Keep, tracker.Get("MEDIA-A"));
            Assert.Equal(1, tracker.CountToKeep);
            Assert.Equal(0, tracker.CountToReview);
            Assert.False(tracker.Set("media-a", MediaComparisonOpinionKind.Keep));

            Assert.True(tracker.Set("MEDIA-A", MediaComparisonOpinionKind.Review));
            Assert.Equal(MediaComparisonOpinionKind.Review, tracker.Get("media-a"));
            Assert.Equal(0, tracker.CountToKeep);
            Assert.Equal(1, tracker.CountToReview);

            Assert.True(tracker.Clear("media-a"));
            Assert.Null(tracker.Get("media-a"));
            Assert.Equal(0, tracker.CountToKeep);
            Assert.Equal(0, tracker.CountToReview);
            Assert.False(tracker.Clear("media-a"));
        }

        [Fact]
        public void EmptyIdsAndUndefinedOpinionsAreIgnored()
        {
            var tracker = new MediaComparisonOpinionTracker();

            Assert.False(tracker.Set(null, MediaComparisonOpinionKind.Keep));
            Assert.False(tracker.Set(" ", MediaComparisonOpinionKind.Keep));
            Assert.False(tracker.Set("media-a", (MediaComparisonOpinionKind)99));
            Assert.Null(tracker.Get(null));
            Assert.False(tracker.Clear(" "));
            Assert.Equal(0, tracker.CountToKeep + tracker.CountToReview);
        }

        [Fact]
        public void HandoffSnapshotUsesStableIdsInDeterministicOrder()
        {
            var tracker = new MediaComparisonOpinionTracker();
            tracker.Set("media-z", MediaComparisonOpinionKind.Review);
            tracker.Set("Media-A", MediaComparisonOpinionKind.Keep);

            var snapshot = tracker.GetMarkedMediaIds();
            tracker.Clear("media-z");

            Assert.Equal(new[] { "Media-A", "media-z" }, snapshot);
            Assert.Equal(1, tracker.MarkedCount);
        }
    }
}
