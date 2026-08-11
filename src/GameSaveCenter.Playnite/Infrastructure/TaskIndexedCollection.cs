using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Keeps an ObservableCollection of task rows in sync with a TaskId index so high-frequency
    /// progress events update the matching row in O(1) instead of copying the list and scanning
    /// it linearly on every event.
    /// </summary>
    public sealed class TaskIndexedCollection
    {
        private const int MaxTaskRows = 200;
        private readonly Dictionary<string, int> indexById = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public void Rebuild(ObservableCollection<TaskStatusDto> target)
        {
            indexById.Clear();
            for (var i = 0; i < target.Count; i++)
                indexById[target[i].TaskId] = i;
        }

        public void Merge(ObservableCollection<TaskStatusDto> target, TaskStatusDto change)
        {
            if (change == null) return;
            if (TryGetValidIndex(target, change.TaskId, out var existing))
            {
                target[existing] = change;
                return;
            }

            Rebuild(target);
            if (TryGetValidIndex(target, change.TaskId, out existing))
            {
                target[existing] = change;
                return;
            }

            target.Insert(0, change);
            Rebuild(target);
            while (target.Count > MaxTaskRows)
            {
                var removed = target[target.Count - 1];
                target.RemoveAt(target.Count - 1);
                indexById.Remove(removed.TaskId);
            }
        }

        private bool TryGetValidIndex(ObservableCollection<TaskStatusDto> target, string taskId, out int index)
        {
            if (indexById.TryGetValue(taskId, out index)
                && index >= 0
                && index < target.Count
                && string.Equals(target[index].TaskId, taskId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            index = -1;
            return false;
        }
    }
}
