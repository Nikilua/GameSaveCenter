using System.Collections.ObjectModel;
using System.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class TaskIndexedCollectionTests
    {
        [Fact]
        public void Merge_UpdatesExistingRowInPlace()
        {
            var tasks = new ObservableCollection<TaskStatusDto> { Task("a", 10), Task("b", 20) };
            var index = new TaskIndexedCollection();
            index.Rebuild(tasks);

            index.Merge(tasks, Task("a", 55));

            Assert.Equal(2, tasks.Count);
            Assert.Equal(55, tasks[0].ProgressPercent);
            Assert.Equal("a", tasks[0].TaskId);
            Assert.Equal(20, tasks[1].ProgressPercent);
        }

        [Fact]
        public void Merge_InsertsNewTaskAtTop()
        {
            var tasks = new ObservableCollection<TaskStatusDto> { Task("a", 10) };
            var index = new TaskIndexedCollection();
            index.Rebuild(tasks);

            index.Merge(tasks, Task("new", 0));

            Assert.Equal(new[] { "new", "a" }, tasks.Select(x => x.TaskId).ToArray());
        }

        [Fact]
        public void Merge_CapsCollectionAtTwoHundredRows()
        {
            var tasks = new ObservableCollection<TaskStatusDto>();
            var index = new TaskIndexedCollection();
            index.Rebuild(tasks);

            for (var i = 0; i < 250; i++)
                index.Merge(tasks, Task("task-" + i, i));

            Assert.Equal(200, tasks.Count);
            Assert.Equal("task-249", tasks[0].TaskId);
        }

        [Fact]
        public void Rebuild_RestoresIndexAfterExternalReplacement()
        {
            var tasks = new ObservableCollection<TaskStatusDto> { Task("a", 10) };
            var index = new TaskIndexedCollection();
            index.Rebuild(tasks);
            tasks[0] = Task("b", 30);

            index.Rebuild(tasks);
            index.Merge(tasks, Task("b", 60));

            Assert.Equal(60, tasks[0].ProgressPercent);
        }

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
