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
        private bool showingAllBackupCoverCandidates;
        private BackupCoverCandidate? selectedBackupCoverCandidate;

        public BackupCoverDto CurrentBackupCover => currentBackupCover;
        public bool IsBackupCoverChooserOpen => backupCoverCandidatePage != null;
        public string BackupCoverCandidateSummary => !IsBackupCoverChooserOpen ? string.Empty
            : showingAllBackupCoverCandidates
                ? (BackupCoverCandidates.Count == 0 ? "当前游戏暂无可选截图" : "全部截图 · 按拍摄时间")
                : (BackupCoverCandidates.Count == 0 ? "备份前后 24 小时内没有截图" : "备份前后 24 小时 · 按时间接近程度");
        public BatchObservableCollection<BackupCoverCandidate> BackupCoverCandidates { get; } = new BatchObservableCollection<BackupCoverCandidate>();
        public BackupCoverCandidate? SelectedBackupCoverCandidate
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
        public ICommand BrowseAllBackupCoverCandidatesCommand { get; private set; } = null!;
        public ICommand LoadMoreBackupCoverCandidatesCommand { get; private set; } = null!;
        public ICommand SetBackupCoverCommand { get; private set; } = null!;
        public ICommand ClearBackupCoverCommand { get; private set; } = null!;

        private bool CanManageBackupCover => !IsBusy && CurrentWorkspace == WorkspaceKind.Saves
            && SelectedBackup != null && SelectedGame != null;

        private void InitializeBackupCoverCommands()
        {
            ChooseBackupCoverCommand = new RelayCommand(_ => Run(() => LoadBackupCoverCandidatesAsync(false)), _ => CanManageBackupCover);
            BrowseAllBackupCoverCandidatesCommand = new RelayCommand(
                _ => Run(() => LoadBackupCoverCandidatesAsync(false, browseAll: true)),
                _ => CanManageBackupCover && IsBackupCoverChooserOpen && !showingAllBackupCoverCandidates);
            LoadMoreBackupCoverCandidatesCommand = new RelayCommand(
                _ => Run(() => LoadBackupCoverCandidatesAsync(true, browseAll: true)),
                _ => CanManageBackupCover && showingAllBackupCoverCandidates && backupCoverCandidatePage?.HasMore == true);
            SetBackupCoverCommand = new RelayCommand(
                candidate => RunBackupCoverUpdate(((BackupCoverCandidate)candidate!).Media.MediaId),
                candidate => CanManageBackupCover && candidate is BackupCoverCandidate item && BackupCoverCandidates.Contains(item));
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
            showingAllBackupCoverCandidates = false;
            BackupCoverCandidates?.Clear();
            SelectedBackupCoverCandidate = null;
            OnPropertyChanged(nameof(IsBackupCoverChooserOpen));
            OnPropertyChanged(nameof(BackupCoverCandidateSummary));
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

        private async Task LoadBackupCoverCandidatesAsync(bool append, bool browseAll = false)
        {
            var gameId = SelectedGame?.PlayniteId;
            var backup = SelectedBackup;
            if (string.IsNullOrEmpty(gameId) || backup == null || string.IsNullOrEmpty(backup.BackupId)) return;
            browseAll |= backup.CreatedUtc == DateTime.MinValue;
            var page = await plugin.RequestAsync<MediaPageDto>(MessageTypes.ListMediaPage, new MediaQueryDto
            {
                PlayniteId = gameId!,
                Kind = MediaKind.Screenshot,
                Limit = 48,
                NearCapturedUtc = browseAll ? (DateTime?)null : backup.CreatedUtc,
                Cursor = append ? backupCoverCandidatePage?.NextCursor ?? string.Empty : string.Empty
            }).ConfigureAwait(false);
            ApplyOnUi(() =>
            {
                if (!IsBackupCoverContext(gameId!, backup.BackupId)) return;
                if (!append)
                {
                    SelectedBackupCoverCandidate = null;
                    BackupCoverCandidates.Clear();
                }
                foreach (var item in page.Items) BackupCoverCandidates.Add(new BackupCoverCandidate(item, backup.CreatedUtc));
                backupCoverCandidatePage = page;
                showingAllBackupCoverCandidates = browseAll;
                OnPropertyChanged(nameof(IsBackupCoverChooserOpen));
                OnPropertyChanged(nameof(BackupCoverCandidateSummary));
                RaiseCommandStates();
            });
        }

        public sealed class BackupCoverCandidate
        {
            public BackupCoverCandidate(MediaItemDto media, DateTime backupUtc)
            {
                Media = media;
                Display = media.FileName;
                if (backupUtc == DateTime.MinValue || media.CapturedUtc == DateTime.MinValue) return;

                var difference = media.CapturedUtc.ToUniversalTime() - backupUtc.ToUniversalTime();
                if (difference == TimeSpan.Zero)
                {
                    Display = $"与备份同一时间 · {Display}";
                    return;
                }

                var direction = difference < TimeSpan.Zero ? "前" : "后";
                var minutes = (long)Math.Abs(difference.TotalMinutes);
                var distance = minutes switch
                {
                    0 => "不到 1 分钟",
                    < 60 => $"{minutes} 分钟",
                    _ => $"{minutes / 60} 小时 {minutes % 60} 分钟"
                };
                Display = $"备份{direction} {distance} · {Display}";
            }

            public MediaItemDto Media { get; }
            public string Display { get; }
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
