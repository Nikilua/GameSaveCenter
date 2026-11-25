using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed partial class DashboardViewModel
    {
        private const int BackupGalleryTabIndex = 4;
        private readonly LatestRequestCoordinator backupGalleryReads = new LatestRequestCoordinator();
        private BackupCoverGalleryPageDto? backupGalleryPage;
        private string? backupGalleryGameId;
        private bool isBackupGalleryLoading;
        private string backupGalleryStatus = string.Empty;

        public BatchObservableCollection<BackupGalleryCardViewModel> BackupGalleryItems { get; } = new BatchObservableCollection<BackupGalleryCardViewModel>();
        public string BackupGalleryStatus => backupGalleryStatus;
        public ICommand RefreshBackupGalleryCommand { get; private set; } = null!;
        public ICommand LoadMoreBackupGalleryCommand { get; private set; } = null!;
        public ICommand OpenBackupGalleryVersionCommand { get; private set; } = null!;
        public ICommand OpenHistoryFromBackupGalleryCommand { get; private set; } = null!;

        private bool CanUseBackupGallery => CurrentWorkspace == WorkspaceKind.Saves && SaveTabIndex == BackupGalleryTabIndex
            && SelectedGame != null;

        private void InitializeBackupGalleryCommands()
        {
            RefreshBackupGalleryCommand = new RelayCommand(_ => Observe(LoadBackupGalleryAsync(false)), _ => CanUseBackupGallery);
            LoadMoreBackupGalleryCommand = new RelayCommand(_ => Observe(LoadBackupGalleryAsync(true)),
                _ => CanUseBackupGallery && !isBackupGalleryLoading && backupGalleryPage?.HasMore == true);
            OpenBackupGalleryVersionCommand = new RelayCommand(
                item => Run(() => OpenBackupGalleryVersionAsync(((BackupGalleryCardViewModel)item!).Backup)),
                item => CanUseBackupGallery && !IsBusy && item is BackupGalleryCardViewModel);
            OpenHistoryFromBackupGalleryCommand = new RelayCommand(_ => OpenHistoryFromBackupGallery(), _ => CanUseBackupGallery && !IsBusy);
        }

        private void EnsureBackupGalleryLoaded()
        {
            if (backupGalleryReads != null && CanUseBackupGallery
                && !string.Equals(backupGalleryGameId, SelectedGame.PlayniteId, StringComparison.Ordinal))
                Observe(LoadBackupGalleryAsync(false));
        }

        private void ResetBackupGallery()
        {
            backupGalleryReads?.Cancel();
            backupGalleryGameId = null;
            backupGalleryPage = null;
            isBackupGalleryLoading = false;
            BackupGalleryItems?.Clear();
            InvalidateProgressRecap();
            backupGalleryStatus = string.Empty;
            OnPropertyChanged(nameof(BackupGalleryStatus));
        }

        private async Task LoadBackupGalleryAsync(bool append)
        {
            var gameId = SelectedGame?.PlayniteId;
            if (string.IsNullOrWhiteSpace(gameId)) return;
            var cursor = append ? backupGalleryPage : null;
            if (!append) ResetBackupGallery();
            backupGalleryGameId = gameId;
            var request = backupGalleryReads.Begin();
            isBackupGalleryLoading = true;
            backupGalleryStatus = append ? "正在加载更多进度…" : "正在读取进度画廊…";
            OnPropertyChanged(nameof(BackupGalleryStatus));
            RaiseCommandStates();
            try
            {
                var page = await plugin.RequestAsync<BackupCoverGalleryPageDto>(MessageTypes.ListBackupCoverGallery,
                    new BackupCoverGalleryQueryDto
                    {
                        PlayniteId = gameId!,
                        BeforeCreatedUtcKey = cursor?.NextCreatedUtcKey ?? string.Empty,
                        BeforeBackupId = cursor?.NextBackupId ?? string.Empty
                    }, cancellationToken: request.Token).ConfigureAwait(false);
                ApplyOnUi(() =>
                {
                    if (!backupGalleryReads.IsCurrent(request) || !IsSelectedGame(gameId!)) return;
                    var known = new HashSet<string>(BackupGalleryItems.Select(item => item.Backup.BackupId), StringComparer.Ordinal);
                    foreach (var item in page.Items)
                    {
                        if (!known.Add(item.Backup.BackupId)) continue;
                        var card = new BackupGalleryCardViewModel(item, () => SelectedProgressRecapCount < 9);
                        card.PropertyChanged += (_, change) =>
                        {
                            if (change.PropertyName != nameof(BackupGalleryCardViewModel.IsSelected)) return;
                            foreach (var visibleCard in BackupGalleryItems) visibleCard.NotifyRecapAvailability();
                            InvalidateProgressRecap();
                        };
                        BackupGalleryItems.Add(card);
                    }
                    backupGalleryPage = page;
                    backupGalleryStatus = page.TotalCount == 0 ? "当前游戏还没有配图的存档" : $"已载入 {BackupGalleryItems.Count}/{page.TotalCount} 个进度";
                });
            }
            catch (OperationCanceledException) when (request.Token.IsCancellationRequested) { }
            catch (Exception)
            {
                ApplyOnUi(() =>
                {
                    if (!backupGalleryReads.IsCurrent(request)) return;
                    backupGalleryStatus = "无法读取进度画廊";
                });
            }
            finally
            {
                ApplyOnUi(() =>
                {
                    if (!backupGalleryReads.IsCurrent(request)) return;
                    isBackupGalleryLoading = false;
                    OnPropertyChanged(nameof(BackupGalleryStatus));
                    RaiseCommandStates();
                });
                backupGalleryReads.End(request);
            }
        }

        private async Task OpenBackupGalleryVersionAsync(BackupVersionDto backup)
        {
            if (!IsSelectedGame(backup.PlayniteId)) return;
            var page = await ReadExactHistoryVersionAsync(backup.PlayniteId, backup.BackupId).ConfigureAwait(false);
            ApplyOnUi(() =>
            {
                if (!CanUseBackupGallery || !IsSelectedGame(backup.PlayniteId)) return;
                if (page.Items.Count == 0)
                {
                    backupGalleryStatus = "该存档版本已不存在";
                    OnPropertyChanged(nameof(BackupGalleryStatus));
                    return;
                }
                FocusHistoryVersion(page, "画廊中的存档版本");
            });
        }

        private void OpenHistoryFromBackupGallery()
        {
            ClearBackupCalendarSelection();
            BackupHistorySearchText = string.Empty;
            BackupHistoryRange = BackupHistoryDateRange.All;
            ResetBackupHistorySearch();
            SaveTabIndex = 0;
            RaiseCommandStates();
            if (Backups.Count == 0) RequestWorkspaceLoad();
        }
    }
}
