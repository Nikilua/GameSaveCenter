using System;
using System.Collections.Specialized;
using System.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class LargeLibraryPerformanceTests
    {
        [Fact]
        public void UnchangedGamePickerWith2000Games_EmitsNoSecondCollectionNotification()
        {
            using var picker = new GamePickerViewModel();
            var notifications = 0;
            picker.Items.CollectionChanged += (_, _) => notifications++;
            var games = Enumerable.Range(0, 2000).Select(i => Game("Game " + i)).ToArray();

            picker.SetItems(games);
            Assert.Equal(1, notifications);

            picker.SetItems(Enumerable.Range(0, 2000).Select(i => Game("Game " + i)).ToArray());
            Assert.Equal(1, notifications);
            Assert.Equal(2000, picker.Items.Count);
        }

        [Fact]
        public void ChangedGamePickerWith2000Games_EmitsOneResetWithoutPerItemAdds()
        {
            using var picker = new GamePickerViewModel();
            var resets = 0;
            var adds = 0;
            picker.Items.CollectionChanged += (_, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Reset) resets++;
                if (args.Action == NotifyCollectionChangedAction.Add) adds++;
            };
            var games = Enumerable.Range(0, 2000).Select(i => Game("Game " + i)).ToArray();
            picker.SetItems(games);

            var changed = Enumerable.Range(0, 2000).Select(i => Game("Game " + i)).ToArray();
            changed[1500].HealthState = "Attention";
            picker.SetItems(changed);

            Assert.Equal(2, resets);
            Assert.Equal(0, adds);
            Assert.Equal(2000, picker.Items.Count);
        }

        [Fact]
        public void UnchangedTasksWith2000Rows_EmitsNoSecondCollectionChanged()
        {
            var tasks = Enumerable.Range(0, 2000).Select(i => Task("task-" + i, i % 100)).ToArray();
            var collection = new BatchObservableCollection<TaskStatusDto>();
            var notifications = 0;
            collection.CollectionChanged += (_, _) => notifications++;

            Assert.True(collection.ReplaceAll(tasks, SnapshotComparers.Task));
            Assert.Equal(1, notifications);

            Assert.False(collection.ReplaceAll(tasks, SnapshotComparers.Task));
            Assert.Equal(1, notifications);
            Assert.Equal(2000, collection.Count);
        }

        private static GameStatusDto Game(string name)
            => new GameStatusDto
            {
                PlayniteId = name,
                Name = name,
                Platform = GamePlatformKind.Other,
                IsInstalled = true,
                LudusaviMatched = true,
                HealthState = "Ready"
            };

        private static TaskStatusDto Task(string id, int progress)
            => new TaskStatusDto
            {
                TaskId = id,
                TaskType = "Backup",
                State = TaskState.Running,
                ProgressPercent = progress
            };
    }
}
