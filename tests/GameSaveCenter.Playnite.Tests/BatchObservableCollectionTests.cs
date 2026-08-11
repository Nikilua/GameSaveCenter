using System.Collections.Specialized;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class BatchObservableCollectionTests
    {
        [Fact]
        public void ReplaceAll_WithContentComparer_SkipsUnchangedSnapshot()
        {
            var collection = new BatchObservableCollection<TaskStatusDto>();
            var notificationCount = 0;
            collection.CollectionChanged += (_, _) => notificationCount++;

            var first = new[] { Task("t1", 10, TaskState.Running), Task("t2", 20, TaskState.Running) };
            Assert.True(collection.ReplaceAll(first, SnapshotComparers.Task));
            Assert.Equal(1, notificationCount);

            var identical = new[] { Task("t1", 10, TaskState.Running), Task("t2", 20, TaskState.Running) };
            Assert.False(collection.ReplaceAll(identical, SnapshotComparers.Task));
            Assert.Equal(1, notificationCount);
        }

        [Fact]
        public void ReplaceAll_WithContentComparer_DetectsProgressAndStatusChanges()
        {
            var collection = new BatchObservableCollection<TaskStatusDto>();
            collection.ReplaceAll(new[] { Task("t1", 10, TaskState.Running) }, SnapshotComparers.Task);

            Assert.True(collection.ReplaceAll(new[] { Task("t1", 55, TaskState.Running) }, SnapshotComparers.Task));
            Assert.True(collection.ReplaceAll(new[] { Task("t1", 100, TaskState.Succeeded) }, SnapshotComparers.Task));
            Assert.True(collection.ReplaceAll(new[] { Task("t1", 100, TaskState.Failed, "ERR", "boom") }, SnapshotComparers.Task));
        }

        [Fact]
        public void ReplaceAll_WithoutComparer_KeepsReferenceEqualityBehavior()
        {
            var collection = new BatchObservableCollection<string> { "a", "b" };
            Assert.False(collection.ReplaceAll(new[] { "a", "b" }));

            var changed = new[] { "a", "c" };
            Assert.True(collection.ReplaceAll(changed));
            Assert.Equal(new[] { "a", "c" }, collection);
        }

        private static TaskStatusDto Task(string id, int progress, TaskState state, string errorCode = "", string errorMessage = "")
            => new TaskStatusDto
            {
                TaskId = id,
                TaskType = "Backup",
                State = state,
                ProgressPercent = progress,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            };
    }
}
