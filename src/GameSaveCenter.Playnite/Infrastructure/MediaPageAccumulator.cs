using System;
using System.Collections.Generic;
using System.Linq;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Appends cursor-paged media without rebuilding the already retained prefix. The
    /// retained window is bounded because WPF virtualization limits realized rows, not DTOs.
    /// </summary>
    public sealed class MediaPageAccumulator
    {
        public const int DefaultCapacity = 2000;

        private readonly BatchObservableCollection<MediaItemDto> target;
        private readonly Dictionary<string, int> index = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly int capacity;

        public MediaPageAccumulator(BatchObservableCollection<MediaItemDto> target, int capacity = DefaultCapacity)
        {
            this.target = target ?? throw new ArgumentNullException(nameof(target));
            this.capacity = Math.Max(1, capacity);
            RebuildIndex();
        }

        public int Capacity => capacity;

        public bool ReplaceFirstPage(IEnumerable<MediaItemDto> incoming, string? selectedId)
        {
            var normalized = Normalize(incoming);
            var changed = target.ReplaceAll(normalized, SnapshotComparers.Media);
            RebuildIndex();
            return TrimToCapacity(selectedId) || changed;
        }

        public bool AppendPage(IEnumerable<MediaItemDto> incoming, string? selectedId)
        {
            EnsureIndex();
            var normalized = Normalize(incoming);
            if (normalized.Count == 0) return false;

            var updates = new List<(int Position, MediaItemDto Item)>();
            var additions = new List<MediaItemDto>();
            foreach (var item in normalized)
            {
                if (index.TryGetValue(item.MediaId, out var position))
                {
                    if (!SnapshotComparers.Media(target[position], item))
                        updates.Add((position, item));
                }
                else
                {
                    additions.Add(item);
                }
            }

            if (updates.Count == 0 && additions.Count == 0)
                return false;

            target.ApplyBatch(() =>
            {
                foreach (var update in updates)
                    target[update.Position] = update.Item;
                foreach (var item in additions)
                {
                    index[item.MediaId] = target.Count;
                    target.Add(item);
                }
                TrimInBatch(selectedId);
            });
            RebuildIndex();
            return true;
        }

        public void Clear()
        {
            index.Clear();
        }

        private bool TrimToCapacity(string? selectedId)
        {
            if (target.Count <= capacity) return false;
            target.ApplyBatch(() => TrimInBatch(selectedId));
            RebuildIndex();
            return true;
        }

        private bool TrimInBatch(string? selectedId)
        {
            if (target.Count <= capacity) return false;
            var retained = target.Skip(target.Count - capacity).ToList();
            if (!string.IsNullOrWhiteSpace(selectedId)
                && !retained.Any(x => string.Equals(x.MediaId, selectedId, StringComparison.OrdinalIgnoreCase)))
            {
                var selected = target.FirstOrDefault(x => string.Equals(x.MediaId, selectedId, StringComparison.OrdinalIgnoreCase));
                if (selected != null)
                {
                    retained.RemoveAt(0);
                    retained.Insert(0, selected);
                }
            }
            target.Clear();
            foreach (var item in retained)
                target.Add(item);
            return true;
        }

        private void EnsureIndex()
        {
            if (index.Count == target.Count) return;
            RebuildIndex();
        }

        private void RebuildIndex()
        {
            index.Clear();
            for (var position = 0; position < target.Count; position++)
                index[target[position].MediaId] = position;
        }

        private static List<MediaItemDto> Normalize(IEnumerable<MediaItemDto> incoming)
        {
            var byId = new Dictionary<string, MediaItemDto>(StringComparer.OrdinalIgnoreCase);
            var order = new List<string>();
            foreach (var item in incoming ?? Enumerable.Empty<MediaItemDto>())
            {
                if (!byId.ContainsKey(item.MediaId)) order.Add(item.MediaId);
                byId[item.MediaId] = item;
            }
            return order.Select(id => byId[id]).ToList();
        }
    }
}
