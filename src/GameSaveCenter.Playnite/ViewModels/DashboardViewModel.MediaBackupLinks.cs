using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed partial class DashboardViewModel
    {
        private BackupVersionPageDto? mediaBackupLinksPage;
        private bool isShowingFocusedHistoryBackup;
        private string mediaBackupLinksStatus = string.Empty;

        public BatchObservableCollection<BackupVersionDto> MediaBackupLinks { get; } = new BatchObservableCollection<BackupVersionDto>();
        public string MediaBackupLinksStatus => mediaBackupLinksStatus;
        public bool IsShowingFocusedHistoryBackup => isShowingFocusedHistoryBackup;
        public ICommand ListMediaBackupLinksCommand { get; private set; } = null!;
        public ICommand LoadMoreMediaBackupLinksCommand { get; private set; } = null!;
        public ICommand OpenMediaLinkedBackupCommand { get; private set; } = null!;
        public ICommand ClearLinkedBackupFocusCommand { get; private set; } = null!;

        private bool CanReadMediaBackupLinks => !IsBusy && CurrentWorkspace == WorkspaceKind.Media
            && SelectedMedia?.Kind == MediaKind.Screenshot && SelectedGame != null
            && string.Equals(SelectedMedia.PlayniteId, SelectedGame.PlayniteId, StringComparison.Ordinal);

        private void InitializeMediaBackupLinksCommands()
        {
            ListMediaBackupLinksCommand = new RelayCommand(_ => Run(() => LoadMediaBackupLinksAsync(false)), _ => CanReadMediaBackupLinks);
            LoadMoreMediaBackupLinksCommand = new RelayCommand(
                _ => Run(() => LoadMediaBackupLinksAsync(true)), _ => CanReadMediaBackupLinks && mediaBackupLinksPage?.HasMore == true);
            OpenMediaLinkedBackupCommand = new RelayCommand(
                backup => Run(() => OpenMediaLinkedBackupAsync(((BackupVersionDto)backup!).BackupId)),
                backup => CanReadMediaBackupLinks && backup is BackupVersionDto);
            ClearLinkedBackupFocusCommand = new RelayCommand(_ => ResetBackupHistorySearch(), _ => IsShowingFocusedHistoryBackup);
        }

        private void ResetMediaBackupLinks()
        {
            mediaBackupLinksPage = null;
            mediaBackupLinksStatus = string.Empty;
            MediaBackupLinks?.Clear();
            OnPropertyChanged(nameof(MediaBackupLinksStatus));
        }

        private async Task LoadMediaBackupLinksAsync(bool append)
        {
            var gameId = SelectedGame?.PlayniteId;
            var mediaId = SelectedMedia?.MediaId;
            if (string.IsNullOrEmpty(gameId) || string.IsNullOrEmpty(mediaId)) return;
            var cursor = append ? mediaBackupLinksPage : null;
            var page = await plugin.RequestAsync<BackupVersionPageDto>(MessageTypes.ListBackupCoversForMedia,
                new BackupCoverMediaQueryDto
                {
                    PlayniteId = gameId!,
                    MediaId = mediaId!,
                    BeforeCreatedUtcKey = cursor?.NextCreatedUtcKey ?? string.Empty,
                    BeforeBackupId = cursor?.NextBackupId ?? string.Empty
                }).ConfigureAwait(false);
            ApplyOnUi(() =>
            {
                if (!IsMediaBackupLinksContext(gameId!, mediaId!)) return;
                if (!append) MediaBackupLinks.Clear();
                foreach (var link in page.Items) MediaBackupLinks.Add(link);
                mediaBackupLinksPage = page;
                mediaBackupLinksStatus = page.TotalCount == 0 ? "这张截图没有关联存档" : $"关联 {page.TotalCount} 个存档版本";
                OnPropertyChanged(nameof(MediaBackupLinksStatus));
                RaiseCommandStates();
            });
        }

        private async Task OpenMediaLinkedBackupAsync(string backupId)
        {
            var gameId = SelectedGame?.PlayniteId;
            var media = SelectedMedia;
            if (string.IsNullOrEmpty(gameId) || media == null) return;
            var page = await plugin.RequestAsync<BackupVersionPageDto>(MessageTypes.ListBackupCoversForMedia,
                new BackupCoverMediaQueryDto { PlayniteId = gameId!, MediaId = media.MediaId, BackupId = backupId }).ConfigureAwait(false);
            ApplyOnUi(() =>
            {
                if (!IsMediaBackupLinksContext(gameId!, media.MediaId)) return;
                var backup = page.Items.FirstOrDefault();
                if (backup == null)
                {
                    var removed = MediaBackupLinks.FirstOrDefault(item => item.BackupId == backupId);
                    if (removed != null) MediaBackupLinks.Remove(removed);
                    mediaBackupLinksStatus = "该版本已不存在或已解除关联";
                    OnPropertyChanged(nameof(MediaBackupLinksStatus));
                    return;
                }
                PushNavigationReturnTarget("返回原截图", $"媒体中心 · {media.FileName}", media.MediaId);
                CurrentWorkspace = WorkspaceKind.Saves;
                FocusHistoryVersion(page, "截图关联的存档版本");
                RequestWorkspaceLoad();
            });
        }

        private void FocusHistoryVersion(BackupVersionPageDto page, string summary)
        {
            SaveTabIndex = 0;
            ClearBackupCalendarSelection();
            BackupHistorySearchText = string.Empty;
            BackupHistoryRange = BackupHistoryDateRange.All;
            ResetBackupHistorySearch();
            backupSearchResults.ReplaceAll(page.Items);
            backupSearchPage = page;
            backupSearchStatus = summary;
            OnPropertyChanged(nameof(BackupHistoryView));
            OnPropertyChanged(nameof(BackupHistoryRangeSummary));
            SelectedBackup = page.Items[0];
            isShowingFocusedHistoryBackup = true;
            OnPropertyChanged(nameof(IsShowingFocusedHistoryBackup));
            RaiseCommandStates();
        }

        private bool IsMediaBackupLinksContext(string gameId, string mediaId)
            => CurrentWorkspace == WorkspaceKind.Media && IsSelectedGame(gameId)
                && string.Equals(SelectedMedia?.MediaId, mediaId, StringComparison.Ordinal);
    }
}
