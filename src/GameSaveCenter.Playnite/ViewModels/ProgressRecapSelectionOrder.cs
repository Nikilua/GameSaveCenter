using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.ViewModels
{
    internal interface IProgressRecapOrderMoveSource
    {
        event EventHandler<ProgressRecapOrderMoveStartingEventArgs>? ProgressRecapOrderMoveStarting;
    }

    internal sealed class ProgressRecapOrderMoveStartingEventArgs : EventArgs
    {
        internal ProgressRecapOrderMoveStartingEventArgs(
            ProgressRecapOrderItemViewModel movedItem,
            ProgressRecapOrderItemViewModel adjacentItem,
            int direction)
        {
            MovedItem = movedItem;
            AdjacentItem = adjacentItem;
            Direction = direction;
        }

        internal ProgressRecapOrderItemViewModel MovedItem { get; }
        internal ProgressRecapOrderItemViewModel AdjacentItem { get; }
        internal int Direction { get; }
    }

    internal readonly struct ProgressRecapIdentity : IEquatable<ProgressRecapIdentity>
    {
        internal ProgressRecapIdentity(string playniteId, string backupId)
        {
            PlayniteId = playniteId ?? string.Empty;
            BackupId = backupId ?? string.Empty;
        }

        internal string PlayniteId { get; }
        internal string BackupId { get; }

        internal static ProgressRecapIdentity From(BackupVersionDto backup)
            => new ProgressRecapIdentity(backup.PlayniteId, backup.BackupId);

        public bool Equals(ProgressRecapIdentity other)
            => StringComparer.OrdinalIgnoreCase.Equals(PlayniteId, other.PlayniteId)
                && StringComparer.OrdinalIgnoreCase.Equals(BackupId, other.BackupId);

        public override bool Equals(object? obj) => obj is ProgressRecapIdentity other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (StringComparer.OrdinalIgnoreCase.GetHashCode(PlayniteId) * 397)
                    ^ StringComparer.OrdinalIgnoreCase.GetHashCode(BackupId);
            }
        }
    }

    internal sealed class ProgressRecapSelectionOrder
    {
        private readonly ObservableCollection<ProgressRecapOrderItemViewModel> items =
            new ObservableCollection<ProgressRecapOrderItemViewModel>();

        internal ObservableCollection<ProgressRecapOrderItemViewModel> Items => items;
        internal int Count => items.Count;

        internal bool Add(BackupGalleryCardViewModel card)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));
            var identity = ProgressRecapIdentity.From(card.Backup);
            if (items.Count >= 9 || items.Any(item => item.Identity.Equals(identity))) return false;

            items.Add(new ProgressRecapOrderItemViewModel(card));
            UpdatePositions();
            return true;
        }

        internal bool Remove(ProgressRecapIdentity identity)
        {
            var index = IndexOf(identity);
            if (index < 0) return false;

            var removed = items[index];
            items.RemoveAt(index);
            removed.UpdatePosition(0, 0);
            UpdatePositions();
            return true;
        }

        internal bool CanMove(ProgressRecapOrderItemViewModel item, int direction)
        {
            if (direction != -1 && direction != 1) return false;
            var index = items.IndexOf(item);
            return index >= 0 && index + direction >= 0 && index + direction < items.Count;
        }

        internal bool Move(ProgressRecapOrderItemViewModel item, int direction)
        {
            if (!CanMove(item, direction)) return false;
            var index = items.IndexOf(item);
            items.Move(index, index + direction);
            UpdatePositions();
            return true;
        }

        internal void Clear()
        {
            if (items.Count == 0) return;
            foreach (var item in items) item.UpdatePosition(0, 0);
            items.Clear();
        }

        private int IndexOf(ProgressRecapIdentity identity)
        {
            for (var index = 0; index < items.Count; index++)
                if (items[index].Identity.Equals(identity)) return index;
            return -1;
        }

        private void UpdatePositions()
        {
            for (var index = 0; index < items.Count; index++)
                items[index].UpdatePosition(index + 1, items.Count);
        }
    }

    public sealed class ProgressRecapOrderItemViewModel : INotifyPropertyChanged
    {
        private int position;
        private int totalCount;

        internal ProgressRecapOrderItemViewModel(BackupGalleryCardViewModel card)
        {
            Backup = card.Backup;
            Cover = card.Cover;
            Identity = ProgressRecapIdentity.From(card.Backup);
        }

        public BackupVersionDto Backup { get; }
        public BackupCoverDto Cover { get; }
        public string Title => string.IsNullOrWhiteSpace(Backup.Comment) ? "未命名进度" : Backup.Comment;
        public string PositionLabel => position > 0 ? $"第 {position} 项，共 {totalCount} 项" : string.Empty;
        public string AutomationName => $"{PositionLabel}：{Title}";
        public string MoveUpAutomationName => $"{AutomationName}，上移";
        public string MoveDownAutomationName => $"{AutomationName}，下移";
        public string RemoveAutomationName => $"{AutomationName}，移除";

        internal ProgressRecapIdentity Identity { get; }

        internal void UpdatePosition(int nextPosition, int nextTotalCount)
        {
            if (position == nextPosition && totalCount == nextTotalCount) return;
            position = nextPosition;
            totalCount = nextTotalCount;
            OnPropertyChanged(nameof(PositionLabel));
            OnPropertyChanged(nameof(AutomationName));
            OnPropertyChanged(nameof(MoveUpAutomationName));
            OnPropertyChanged(nameof(MoveDownAutomationName));
            OnPropertyChanged(nameof(RemoveAutomationName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
