using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed partial class DashboardViewModel
    {
        private const int BackupHistoryPageSize = 100;
        private string backupHistoryRange = BackupHistoryDateRange.All;
        private int backupHistoryTotalCount;
        private bool backupHistoryHasMore;
        private bool isLoadingMoreBackupHistory;
        private string backupHistoryCursorCreatedUtcKey = string.Empty;
        private string backupHistoryCursorBackupId = string.Empty;
        private BackupLineageDisplay selectedBackupLineage = new BackupLineageDisplay();

        public ICollectionView BackupHistoryView { get; private set; } = null!;

        public IReadOnlyList<string> BackupHistoryRangeOptions => BackupHistoryDateRange.Options;

        public int BackupHistoryTotalCount => Math.Max(backupHistoryTotalCount, Backups?.Count ?? 0);
        public bool BackupHistoryHasMore => backupHistoryHasMore;
        public bool IsLoadingMoreBackupHistory => isLoadingMoreBackupHistory;
        public string SelectedBackupLineageSummary => selectedBackupLineage?.Summary ?? string.Empty;
        public IReadOnlyList<string> SelectedBackupLineageItems => selectedBackupLineage?.Items ?? Array.Empty<string>();

        public string BackupHistoryRange
        {
            get => backupHistoryRange;
            set
            {
                var normalized = BackupHistoryDateRange.Options.Contains(value)
                    ? value
                    : BackupHistoryDateRange.All;
                if (string.Equals(backupHistoryRange, normalized, StringComparison.Ordinal)) return;

                backupHistoryRange = normalized;
                OnPropertyChanged(nameof(BackupHistoryRange));
                RefreshBackupHistoryView();
                OnPropertyChanged(nameof(BackupHistoryHasActiveRange));
                OnPropertyChanged(nameof(BackupHistoryRangeSummary));
                RaiseCommandStates();
            }
        }

        public bool BackupHistoryHasActiveRange
            => !string.Equals(BackupHistoryRange, BackupHistoryDateRange.All, StringComparison.Ordinal);

        public string BackupHistoryRangeSummary
        {
            get
            {
                var total = BackupHistoryTotalCount;
                var loaded = Backups.Count;
                var visible = BackupHistoryView == null
                    ? loaded
                    : BackupHistoryView.Cast<BackupVersionDto>().Count();
                var range = BackupHistoryDateRange.Describe(BackupHistoryRange, DateTime.Now);
                return string.Equals(range, BackupHistoryDateRange.All, StringComparison.Ordinal)
                    ? loaded == total
                        ? $"{range} · 共 {total} 个版本"
                        : $"{range} · 已载入 {loaded}/{total} 个版本"
                    : $"{range} · 显示 {visible}/{loaded} 个已载入版本 · 已载入 {loaded}/{total}";
            }
        }

        public ICommand ClearBackupHistoryRangeCommand { get; private set; } = null!;
        public ICommand JumpToRecentBackupCommand { get; private set; } = null!;
        public ICommand JumpToEarlierBackupCommand { get; private set; } = null!;
        public ICommand LoadMoreBackupHistoryCommand { get; private set; } = null!;

        partial void OnBackupHistoryInitialize()
        {
            BackupHistoryView = new CollectionViewSource { Source = Backups }.View;
            BackupHistoryView.Filter = FilterBackupHistory;
            Backups.CollectionChanged += OnBackupHistoryCollectionChanged;
            ClearBackupHistoryRangeCommand = new RelayCommand(
                _ => ClearBackupHistoryRange(),
                _ => !IsBusy && BackupHistoryHasActiveRange);
            JumpToRecentBackupCommand = new RelayCommand(
                _ => JumpToBackup(true),
                _ => !IsBusy && CanJumpBackupHistory());
            JumpToEarlierBackupCommand = new RelayCommand(
                _ => JumpToBackup(false),
                _ => !IsBusy && CanJumpBackupHistory());
            LoadMoreBackupHistoryCommand = new RelayCommand(
                _ => Run(LoadMoreBackupHistoryAsync),
                _ => !IsBusy && !IsLoadingMoreBackupHistory && BackupHistoryHasMore && SelectedGame != null);
            RefreshSelectedBackupLineage();
        }

        private bool FilterBackupHistory(object item)
            => item is BackupVersionDto backup
               && BackupHistoryDateRange.Matches(backup, BackupHistoryRange, DateTime.Now);

        private void OnBackupHistoryCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(BackupHistoryRangeSummary));
            OnPropertyChanged(nameof(BackupHistoryTotalCount));
            RefreshSelectedBackupLineage();
            RaiseCommandStates();
        }

        private void RefreshBackupHistoryView()
        {
            BackupHistoryView?.Refresh();
        }

        private void ClearBackupHistoryRange()
        {
            BackupHistoryRange = BackupHistoryDateRange.All;
            StatusMessage = "已清空历史日期范围，恢复显示全部版本。";
        }

        private void SetBackupHistoryPageState(BackupVersionPageDto page)
        {
            backupHistoryTotalCount = Math.Max(0, page.TotalCount);
            backupHistoryHasMore = page.HasMore;
            backupHistoryCursorCreatedUtcKey = page.NextCreatedUtcKey ?? string.Empty;
            backupHistoryCursorBackupId = page.NextBackupId ?? string.Empty;
            OnPropertyChanged(nameof(BackupHistoryTotalCount));
            OnPropertyChanged(nameof(BackupHistoryHasMore));
            OnPropertyChanged(nameof(BackupHistoryRangeSummary));
            RefreshSelectedBackupLineage();
            RaiseCommandStates();
        }

        private void RefreshSelectedBackupLineage()
        {
            selectedBackupLineage = BackupLineageDisplayBuilder.Build(
                SelectedBackup,
                Backups ?? new BatchObservableCollection<BackupVersionDto>(),
                BackupHistoryTotalCount);
            OnPropertyChanged(nameof(SelectedBackupLineageSummary));
            OnPropertyChanged(nameof(SelectedBackupLineageItems));
        }

        private async Task LoadMoreBackupHistoryAsync()
        {
            var game = SelectedGame;
            if (game == null || !BackupHistoryHasMore || Backups.Count == 0) return;

            var gameId = game.PlayniteId;
            var requestGeneration = Interlocked.Read(ref detailsLoadGeneration);
            var requestWorkspace = CurrentWorkspace;
            isLoadingMoreBackupHistory = true;
            OnPropertyChanged(nameof(IsLoadingMoreBackupHistory));
            RaiseCommandStates();
            try
            {
                var page = await plugin.RequestAsync<BackupVersionPageDto>(
                    MessageTypes.ListBackupHistoryPage,
                    new BackupHistoryPageQueryDto
                    {
                        PlayniteId = gameId,
                        Limit = BackupHistoryPageSize,
                        BeforeCreatedUtcKey = backupHistoryCursorCreatedUtcKey,
                        BeforeBackupId = backupHistoryCursorBackupId
                    }).ConfigureAwait(false);

                if (!IsCurrentDetailsLoad(gameId, CancellationToken.None, requestGeneration, requestWorkspace)
                    || requestWorkspace != WorkspaceKind.Saves)
                {
                    return;
                }

                ApplyOnUi(() =>
                {
                    if (!IsCurrentDetailsLoad(gameId, CancellationToken.None, requestGeneration, requestWorkspace)) return;
                    var selectedId = SelectedBackup?.BackupId;
                    var knownIds = new HashSet<string>(Backups.Select(item => item.BackupId), StringComparer.OrdinalIgnoreCase);
                    Backups.ApplyBatch(() =>
                    {
                        foreach (var item in page.Items)
                        {
                            if (knownIds.Add(item.BackupId)) Backups.Add(item);
                        }
                    });
                    if (!string.IsNullOrWhiteSpace(selectedId))
                        SelectedBackup = Backups.FirstOrDefault(item => string.Equals(item.BackupId, selectedId, StringComparison.OrdinalIgnoreCase))!;
                    SetBackupHistoryPageState(page);
                    MarkRestorePlanDraftsForSnapshot(
                        gameId,
                        SelectedGame,
                        Backups,
                        !page.HasMore && Backups.Count >= page.TotalCount);
                });
            }
            finally
            {
                ApplyOnUi(() =>
                {
                    isLoadingMoreBackupHistory = false;
                    OnPropertyChanged(nameof(IsLoadingMoreBackupHistory));
                    RaiseCommandStates();
                });
            }
        }

        private async Task<BackupVersionPageDto> LoadBackupHistoryPagesAsync(
            string playniteId,
            bool forceRefresh,
            string? targetBackupId,
            CancellationToken cancellationToken)
        {
            var page = await plugin.RequestAsync<BackupVersionPageDto>(
                MessageTypes.ListBackupHistoryPage,
                new BackupHistoryPageQueryDto
                {
                    PlayniteId = playniteId,
                    Limit = BackupHistoryPageSize,
                    ForceRefresh = forceRefresh
                },
                cancellationToken: cancellationToken).ConfigureAwait(false);
            var items = new List<BackupVersionDto>(page.Items);

            while (!string.IsNullOrWhiteSpace(targetBackupId)
                && !items.Any(item => string.Equals(item.BackupId, targetBackupId, StringComparison.OrdinalIgnoreCase))
                && page.HasMore
                && items.Count < 500)
            {
                page = await plugin.RequestAsync<BackupVersionPageDto>(
                    MessageTypes.ListBackupHistoryPage,
                    new BackupHistoryPageQueryDto
                    {
                        PlayniteId = playniteId,
                        Limit = Math.Min(BackupHistoryPageSize, 500 - items.Count),
                        BeforeCreatedUtcKey = page.NextCreatedUtcKey,
                        BeforeBackupId = page.NextBackupId
                    },
                    cancellationToken: cancellationToken).ConfigureAwait(false);
                items.AddRange(page.Items);
            }

            page.Items = items;
            return page;
        }

        private bool CanJumpBackupHistory()
            => BackupHistoryView != null && BackupHistoryView.Cast<BackupVersionDto>().Any();

        private void JumpToBackup(bool recentFirst)
        {
            var target = BackupHistoryDateRange.OrderForNavigation(
                    BackupHistoryView.Cast<BackupVersionDto>(), recentFirst)
                .FirstOrDefault();
            if (target == null)
            {
                StatusMessage = $"当前{BackupHistoryRange}没有可跳转的历史版本。";
                return;
            }

            SelectedBackup = target;
            StatusMessage = BuildBackupJumpStatusMessage(target, recentFirst);
            StatusMessageFullDisplay = BuildBackupJumpStatusFullDisplay(target, recentFirst);
        }

        internal static string BuildBackupJumpStatusMessage(BackupVersionDto target, bool recentFirst)
            => recentFirst
                ? $"已跳到最近版本：{target.CreatedRelativeDisplay}。"
                : $"已跳到较早版本：{target.CreatedRelativeDisplay}。";

        internal static string BuildBackupJumpStatusFullDisplay(BackupVersionDto target, bool recentFirst)
            => recentFirst
                ? $"已跳到最近版本：{target.CreatedFullDisplay}。"
                : $"已跳到较早版本：{target.CreatedFullDisplay}。";
    }
}
