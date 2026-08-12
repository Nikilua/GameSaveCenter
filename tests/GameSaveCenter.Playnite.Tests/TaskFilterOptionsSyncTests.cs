using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class TaskFilterOptionsSyncTests
    {
        [Fact]
        public void Sync_KeepsAllAtIndexZeroAndSortsValues()
        {
            var target = new ObservableCollection<string>();

            TaskFilterOptionsSync.Sync(target, new[] { "Steam", "Xbox", "全部", "Epic" });

            Assert.Equal("全部", target[0]);
            Assert.Equal(new[] { "全部", "Epic", "Steam", "Xbox" }, target);
        }

        [Fact]
        public void Sync_PreservesExistingSelectionAcrossFiftyRebuilds()
        {
            var target = new ObservableCollection<string> { "全部", "Steam" };
            var resets = 0;
            target.CollectionChanged += (_, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Reset) resets++;
            };

            for (var i = 0; i < 50; i++)
            {
                TaskFilterOptionsSync.Sync(target, new[] { "Steam", "Xbox", "Epic" });
                Assert.Contains("Steam", target);
            }

            Assert.Equal(0, resets);
            Assert.Equal("全部", target[0]);
        }

        [Fact]
        public void Sync_RemovesMissingValuesAndInsertsNewOnes()
        {
            var target = new ObservableCollection<string> { "全部", "Steam", "Xbox" };

            TaskFilterOptionsSync.Sync(target, new[] { "Steam", "Epic" });

            Assert.DoesNotContain("Xbox", target);
            Assert.Contains("Epic", target);
            Assert.Equal(new[] { "全部", "Epic", "Steam" }, target);
        }

        [Fact]
        public void Sync_NeverEmitsResetNotification()
        {
            var target = new ObservableCollection<string>();
            var resets = 0;
            target.CollectionChanged += (_, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Reset) resets++;
            };

            for (var i = 0; i < 50; i++)
            {
                TaskFilterOptionsSync.Sync(target, Enumerable.Range(0, 20).Select(x => "Game-" + x));
                TaskFilterOptionsSync.Sync(target, Enumerable.Range(0, 20).Select(x => "Game-" + (x % 15)));
            }

            Assert.Equal(0, resets);
            Assert.Equal("全部", target[0]);
        }

        [Fact]
        public void Sync_KeepsDefaultAllAfterFiftyRebuilds()
        {
            var target = new ObservableCollection<string>();
            for (var i = 0; i < 50; i++)
            {
                TaskFilterOptionsSync.Sync(target, new[] { "全部", "Game-" + (i % 10) });
            }

            Assert.Equal("全部", target[0]);
        }
    }
}
