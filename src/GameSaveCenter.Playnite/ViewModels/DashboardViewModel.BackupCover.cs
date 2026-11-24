using System;
using System.Threading.Tasks;
using System.Windows.Input;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed partial class DashboardViewModel
    {
        private readonly LatestRequestCoordinator backupCoverReads = new LatestRequestCoordinator();
        private BackupCoverDto currentBackupCover = new BackupCoverDto();
        private string backupCoverError = string.Empty;
        private MediaPageDto? backupCoverCandidatePage;
        private MediaItemDto? selectedBackupCoverCandidate;

        public BackupCoverDto CurrentBackupCover => currentBackupCover;
        public bool IsBackupCoverChooserOpen => backupCoverCandidatePage != null;
        public BatchObservableCollection<MediaItemDto> BackupCoverCandidates { get; } = new BatchObservableCollection<MediaItemDto>();
        public MediaItemDto? SelectedBackupCoverCandidate
        {
            get => selectedBackupCoverCandidate;
            set => SetValue(ref selectedBackupCoverCandidate, value);
        }
        public string BackupCoverStatus => !string.IsNullOrEmpty(backupCoverError) ? backupCoverError : currentBackupCover?.State switch
        {
            BackupCoverState.Available => "已设置存档封面",
            BackupCoverState.Offline => "封面截图离线或已移除",
            BackupCoverState.Reclassified => "封面截图已重新归类",
            _ => "未设置封面"
        };

        public ICommand ChooseBackupCoverCommand { get; private set; } = null!;
        public ICommand LoadMoreBackupCoverCandidatesCommand { get; private set; } = null!;
        public ICommand SetBackupCoverCommand { get; private set; } = null!;
        public ICommand ClearBackupCoverCommand { get; private set; } = null!;

        private bool CanManageBackupCover => !IsBusy && CurrentWorkspace == WorkspaceKind.Saves
            && SelectedBackup != null && SelectedGame != null;

        private void InitializeBackupCoverCommands()
        {
            ChooseBackupCoverCommand = new RelayCommand(_ => Run(() => LoadBackupCoverCandidatesAsync(false)), _ => CanManageBackupCover);
            LoadMoreBackupCoverCandidatesCommand = new RelayCommand(
                _ => Run(() => LoadBackupCoverCandidatesAsync(true)), _ => CanManageBackupCover && backupCoverCandidatePage?.HasMore == true);
            SetBackupCoverCommand = new RelayCommand(
                candidate => RunBackupCoverUpdate(((MediaItemDto)candidate!).MediaId),
                candidate => CanManageBackupCover && candidate is MediaItemDto);
            ClearBackupCoverCommand = new RelayCommand(_ => RunBackupCoverUpdate(string.Empty),
                _ => CanManageBackupCover && !string.IsNullOrEmpty(currentBackupCover?.MediaId));
        }

        private void ResetBackupCover()
        {
            backupCoverReads?.Cancel();
            backupCoverError = string.Empty;
            CloseBackupCoverChooser();
            ApplyBackupCover(new BackupCoverDto());
        }

        private void CloseBackupCoverChooser()
        {
            backupCoverCandidatePage = null;
            BackupCoverCandidates?.Clear();
            SelectedBackupCoverCandidate = null;
            OnPropertyChanged(nameof(IsBackupCoverChooserOpen));
        }

        private void RefreshBackupCover()
        {
            ResetBackupCover();
            if (backupCoverReads == null || CurrentWorkspace != WorkspaceKind.Saves
                || SelectedBackup == null || SelectedGame == null) return;
            backupCoverError = "正在读取存档封面…";
            OnPropertyChanged(nameof(BackupCoverStatus));
            Observe(ReadBackupCoverAsync(SelectedGame.PlayniteId, SelectedBackup.BackupId));
        }

        private async Task ReadBackupCoverAsync(string gameId, string backupId)
        {
            var request = backupCoverReads.Begin();
            try
            {
                var cover = await plugin.RequestAsync<BackupCoverDto>(MessageTypes.GetBackupCover,
                    new BackupCoverQueryDto { PlayniteId = gameId, BackupId = backupId },
                    cancellationToken: request.Token).ConfigureAwait(false);
                ApplyOnUi(() =>
                {
                    if (backupCoverReads.IsCurrent(request) && IsBackupCoverContext(gameId, backupId))
                    {
                        backupCoverError = string.Empty;
                        ApplyBackupCover(cover);
                    }
                });
            }
            catch (OperationCanceledException) when (request.Token.IsCancellationRequested) { }
            catch (Exception)
            {
                ApplyOnUi(() =>
                {
                    if (!backupCoverReads.IsCurrent(request) || !IsBackupCoverContext(gameId, backupId)) return;
                    backupCoverError = "无法读取存档封面";
                    OnPropertyChanged(nameof(BackupCoverStatus));
                });
            }
            finally { backupCoverReads.End(request); }
        }

        private async Task LoadBackupCoverCandidatesAsync(bool append)
        {
            var gameId = SelectedGame?.PlayniteId;
            var backupId = SelectedBackup?.BackupId;
            if (string.IsNullOrEmpty(gameId) || string.IsNullOrEmpty(backupId)) return;
            var page = await plugin.RequestAsync<MediaPageDto>(MessageTypes.ListMediaPage, new MediaQueryDto
            {
                PlayniteId = gameId!,
                Kind = MediaKind.Screenshot,
                Limit = 48,
                Cursor = append ? backupCoverCandidatePage?.NextCursor ?? string.Empty : string.Empty
            }).ConfigureAwait(false);
            ApplyOnUi(() =>
            {
                if (!IsBackupCoverContext(gameId!, backupId!)) return;
                if (!append) CloseBackupCoverChooser();
                foreach (var item in page.Items) BackupCoverCandidates.Add(item);
                backupCoverCandidatePage = page;
                backupCoverError = page.TotalCount == 0 ? "当前游戏暂无可选截图" : string.Empty;
                OnPropertyChanged(nameof(IsBackupCoverChooserOpen));
                OnPropertyChanged(nameof(BackupCoverStatus));
                RaiseCommandStates();
            });
        }

        private void RunBackupCoverUpdate(string mediaId)
        {
            var gameId = SelectedGame?.PlayniteId;
            var backupId = SelectedBackup?.BackupId;
            if (string.IsNullOrEmpty(gameId) || string.IsNullOrEmpty(backupId)) return;
            Run(() => UpdateBackupCoverAsync(gameId!, backupId!, mediaId));
        }

        private async Task UpdateBackupCoverAsync(string gameId, string backupId, string mediaId)
        {
            if (!IsBackupCoverContext(gameId, backupId)) return;
            backupCoverReads.Cancel();
            var cover = await plugin.RequestAsync<BackupCoverDto>(MessageTypes.SetBackupCover,
                new BackupCoverUpdateDto { PlayniteId = gameId, BackupId = backupId, MediaId = mediaId }).ConfigureAwait(false);
            ApplyOnUi(() =>
            {
                if (!IsBackupCoverContext(gameId, backupId)) return;
                backupCoverReads.Cancel();
                backupCoverError = string.Empty;
                CloseBackupCoverChooser();
                ApplyBackupCover(cover);
            });
        }

        private bool IsBackupCoverContext(string gameId, string backupId)
            => CurrentWorkspace == WorkspaceKind.Saves && IsSelectedGame(gameId)
                && string.Equals(SelectedBackup?.BackupId, backupId, StringComparison.Ordinal);

        private void ApplyBackupCover(BackupCoverDto cover)
        {
            currentBackupCover = cover;
            OnPropertyChanged(nameof(CurrentBackupCover));
            OnPropertyChanged(nameof(BackupCoverStatus));
            RaiseCommandStates();
        }
    }
}
