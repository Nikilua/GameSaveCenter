using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class MediaPageAccumulatorTests
    {
        [Fact]
        public void Appending250PagesKeepsBoundedWindowAndSelectedItem()
        {
            var collection = new BatchObservableCollection<MediaItemDto>();
            var accumulator = new MediaPageAccumulator(collection, capacity: 2000);
            var resets = 0;
            var adds = 0;
            collection.CollectionChanged += (_, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Reset) resets++;
                if (args.Action == NotifyCollectionChangedAction.Add) adds++;
            };
            var timer = Stopwatch.StartNew();

            for (var page = 0; page < 250; page++)
            {
                var items = Enumerable.Range(0, 200)
                    .Select(offset => Media("media-" + (page * 200 + offset)))
                    .ToArray();
                if (page == 0)
                    accumulator.ReplaceFirstPage(items, "media-0");
                else
                    accumulator.AppendPage(items, "media-0");
            }

            timer.Stop();
            Assert.Equal(2000, collection.Count);
            Assert.Contains(collection, x => x.MediaId == "media-0");
            Assert.Equal("media-49999", collection[collection.Count - 1].MediaId);
            Assert.Equal(250, resets);
            Assert.Equal(0, adds);
            Assert.InRange(timer.ElapsedMilliseconds, 0, 5000);
        }

        [Fact]
        public void AppendingOverlappingPageUpdatesByIdWithoutDuplicating()
        {
            var collection = new BatchObservableCollection<MediaItemDto>();
            var accumulator = new MediaPageAccumulator(collection, capacity: 10);
            accumulator.ReplaceFirstPage(Enumerable.Range(0, 10).Select(i => Media("media-" + i)), null);

            var updated = Media("media-5");
            updated.Comment = "updated";
            accumulator.AppendPage(new[] { updated, Media("media-10") }, null);

            Assert.Equal(10, collection.Count);
            Assert.Equal("updated", collection.Single(x => x.MediaId == "media-5").Comment);
            Assert.DoesNotContain(collection, x => x.MediaId == "media-0");
            Assert.Contains(collection, x => x.MediaId == "media-10");
        }

        [Fact]
        public void CrossingTheEleventhPageMakesEvictedSelectionExplicitButKeepsPinnedSelection()
        {
            var collection = new BatchObservableCollection<MediaItemDto>();
            var accumulator = new MediaPageAccumulator(collection, capacity: 2000);

            for (var page = 0; page < 11; page++)
            {
                var items = Enumerable.Range(0, 200)
                    .Select(offset => Media("media-" + (page * 200 + offset)))
                    .ToArray();
                if (page == 0)
                    accumulator.ReplaceFirstPage(items, null);
                else
                    accumulator.AppendPage(items, null);
            }

            Assert.Equal(2000, collection.Count);
            Assert.DoesNotContain(collection, item => item.MediaId == "media-0");

            var pinnedCollection = new BatchObservableCollection<MediaItemDto>();
            var pinnedAccumulator = new MediaPageAccumulator(pinnedCollection, capacity: 2000);
            for (var page = 0; page < 11; page++)
            {
                var items = Enumerable.Range(0, 200)
                    .Select(offset => Media("media-" + (page * 200 + offset)))
                    .ToArray();
                if (page == 0)
                    pinnedAccumulator.ReplaceFirstPage(items, "media-0");
                else
                    pinnedAccumulator.AppendPage(items, "media-0");
            }

            Assert.Equal(2000, pinnedCollection.Count);
            Assert.Contains(pinnedCollection, item => item.MediaId == "media-0");
            Assert.Contains(pinnedCollection, item => item.MediaId == "media-2199");
        }

        [Theory]
        [InlineData(200, 200)]
        [InlineData(2000, 2000)]
        [InlineData(10000, 2000)]
        public void BackendScaleKeepsTheMediaWindowBounded(int backendCount, int expectedCount)
        {
            var collection = new BatchObservableCollection<MediaItemDto>();
            var accumulator = new MediaPageAccumulator(collection);

            for (var start = 0; start < backendCount; start += 200)
            {
                var page = Enumerable.Range(start, Math.Min(200, backendCount - start))
                    .Select(offset => Media("scale-" + offset))
                    .ToArray();
                if (start == 0)
                    accumulator.ReplaceFirstPage(page, null);
                else
                    accumulator.AppendPage(page, null);
            }

            Assert.Equal(expectedCount, collection.Count);
            Assert.Equal("scale-" + (backendCount - 1), collection[collection.Count - 1].MediaId);
        }

        private static MediaItemDto Media(string id)
            => new MediaItemDto
            {
                MediaId = id,
                Kind = MediaKind.Screenshot,
                Source = MediaSourceKind.WindowsScreenshot,
                ArchivePath = "C:\\archive\\" + id + ".png",
                OriginalPath = "C:\\source\\" + id + ".png",
                Sha256 = id,
                CapturedUtc = DateTime.UtcNow,
                ClassificationState = "Assigned"
            };
    }
}
