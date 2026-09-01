using System;
using System.Collections.Generic;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Thread-safe bounded set used for short-lived task notification de-duplication.
    /// </summary>
    internal sealed class BoundedTaskIdSet
    {
        internal const int DefaultCapacity = 4096;

        private readonly object gate = new object();
        private readonly HashSet<string> values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Queue<string> order = new Queue<string>();
        private readonly int capacity;

        internal BoundedTaskIdSet(int capacity = DefaultCapacity)
        {
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
            this.capacity = capacity;
        }

        internal int Count
        {
            get
            {
                lock (gate) return values.Count;
            }
        }

        internal bool Contains(string taskId)
        {
            if (string.IsNullOrWhiteSpace(taskId)) return false;
            lock (gate) return values.Contains(taskId);
        }

        internal bool TryAdd(string taskId)
        {
            if (string.IsNullOrWhiteSpace(taskId)) return false;
            lock (gate)
            {
                if (!values.Add(taskId)) return false;
                order.Enqueue(taskId);
                while (order.Count > capacity)
                    values.Remove(order.Dequeue());
                return true;
            }
        }
    }
}
