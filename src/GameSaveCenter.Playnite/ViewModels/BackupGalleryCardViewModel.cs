using System;
using System.ComponentModel;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed class BackupGalleryCardViewModel : INotifyPropertyChanged
    {
        private readonly Func<bool> canSelect;
        private bool isSelected;

        internal BackupGalleryCardViewModel(BackupCoverGalleryEntryDto entry, Func<bool> canSelect)
        {
            Backup = entry.Backup;
            Cover = entry.Cover;
            this.canSelect = canSelect;
        }

        public BackupVersionDto Backup { get; }
        public BackupCoverDto Cover { get; }
        public string Title => string.IsNullOrWhiteSpace(Backup.Comment) ? "未命名进度" : Backup.Comment;
        public bool CanToggleForRecap => IsSelected || canSelect();
        internal void NotifyRecapAvailability() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CanToggleForRecap)));

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected == value) return;
                if (!value || canSelect()) isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
