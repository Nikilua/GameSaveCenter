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
        private string backupHistorySearchText = string.Empty;
        private int backupHistoryTotalCount;
        private bool backupHistoryHasMore;
        private bool isLoadingMoreBackupHistory;
        private string backupHistoryCursorCreatedUtcKey = string.Empty;
        private string backupHistoryCursorBackupId = string.Empty;
        private BackupLineageDisplay selectedBackupLineage = new BackupLineageDisplay();

        private ICollectionView loadedBackupHistoryView = null!;
        private ICollectionView searchedBackupHistoryView = null!;
        private BatchObservableCollection<BackupVersionDto> backupSearchResults = null!;
        private CancellationTokenSource? backupSearchRequest;
        private string? backupSearchGameId;
        private BackupVersionPageDto? backupSearchPage;
        private (DateTime? FromUtc, DateTime? BeforeUtc) backupSearchBounds;
        private string backupSearchStatus = string.Empty;

        public ICollectionView BackupHistoryView => backupSearchPage == null ? loadedBackupHistoryView : searchedBackupHistoryView;
        public ICommand SearchAllBackupHistoryCommand { get; private set; } = null!;

        public IReadOnlyList<string> BackupHistoryRangeOptions => BackupHistoryDateRange.Options;

        public int BackupHistoryTotalCount => Math.Max(backupHistoryTotalCount, Backups?.Count ?? 0);
        public bool BackupHistoryHasMore => backupHistoryHasMore;
        public bool IsLoadingMoreBackupHistory => isLoadingMoreBackupHistory;
        public string SelectedBackupLineageSummary => selectedBackupLineage?.Summary ?? string.Empty;
        public IReadOnlyList<string> SelectedBackupLineageItems => selectedBackupLineage?.Items ?? Array.Empty<string>();

        /// <summary>
        /// Filters only metadata already present in Backups. This never requests a new
        /// page or reads archive contents, so typing remains a view-only operation.
        /// </summary>
        public string BackupHistorySearchText
        {
            get => backupHistorySearchText;
            set
            {
                var normalized = value ?? string.Empty;
                if (string.Equals(backupHistorySearchText, normalized, StringComparison.Ordinal)) return;

                backupHistorySearchText = normalized;
                ResetBackupHistorySearch();
                OnPropertyChanged(nameof(BackupHistorySearchText));
                RefreshBackupHistoryView();
                OnPropertyChanged(nameof(BackupHistoryRangeSummary));
                RaiseCommandStates();
                if (backupHistoryCalendarDay.HasValue) Observe(SearchBackupHistoryAsync(loadMore: false));
            }
        }

        public string BackupHistoryRange
        {
            get => backupHistoryRange;
            set
            {
                var normalized = BackupHistoryDateRange.Options.Contains(value)
                    ? value
                    : BackupHistoryDateRange.All;
                if (string.Equals(backupHistoryRange, normalized, StringComparison.Ordinal)) return;
                ClearBackupCalendarSelection();

                backupHistoryRange = normalized;
                ResetBackupHistorySearch();
                OnPropertyChanged(nameof(BackupHistoryRange));
                RefreshBackupHistoryView();
                OnPropertyChanged(nameof(BackupHistoryHasActiveRange));
                OnPropertyChanged(nameof(BackupHistoryRangeSummary));
                RaiseCommandStates();
            }
        }

        public bool BackupHistoryHasActiveRange
            => backupHistoryCalendarDay.HasValue || !string.Equals(BackupHistoryRange, BackupHistoryDateRange.All, StringComparison.Ordinal);

        public string BackupHistoryRangeSummary
        {
            get
            {
                if (!string.IsNullOrEmpty(backupSearchStatus)) return backupSearchStatus;
                if (backupHistoryCalendarDay.HasValue && backupSearchPage != null)
                    return $"本地日期 {backupHistoryCalendarDay:yyyy-MM-dd} · 显示 {backupSearchResults.Count}/{backupSearchPage.TotalCount} 个版本";
                if (backupSearchPage != null)
                    return $"全部历史搜索“{TruncateSearchSummary(BackupHistorySearchText.Trim())}” · 显示 {backupSearchResults.Count}/{backupSearchPage.TotalCount} 个匹配版本";

                var total = BackupHistoryTotalCount;
                var loaded = Backups.Count;
                var visible = BackupHistoryView == null
                    ? loaded
                    : BackupHistoryView.Cast<BackupVersionDto>().Count();
                var range = BackupHistoryDateRange.Describe(BackupHistoryRange, DateTime.Now);
                var search = BackupHistorySearchText.Trim();
                var searchSummary = string.IsNullOrWhiteSpace(search)
                    ? string.Empty
                    : $" · 搜索“{TruncateSearchSummary(search)}”";
                return string.Equals(range, BackupHistoryDateRange.All, StringComparison.Ordinal)
                    ? loaded == total
                        ? $"{range}{searchSummary} · 显示 {visible}/{loaded} · 共 {total} 个版本"
                        : $"{range}{searchSummary} · 显示 {visible}/{loaded} · 已载入 {loaded}/{total} 个版本"
                    : $"{range}{searchSummary} · 显示 {visible}/{loaded} · 已载入 {loaded}/{total}";
            }
        }

        public ICommand ClearBackupHistoryRangeCommand { get; private set; } = null!;
        public ICommand JumpToRecentBackupCommand { get; private set; } = null!;
        public ICommand JumpToEarlierBackupCommand { get; private set; } = null!;
        public ICommand LoadMoreBackupHistoryCommand { get; private set; } = null!;

        partial void OnBackupHistoryInitialize()
        {
            InitializeBackupCoverCommands();
            InitializeMediaBackupLinksCommands();
            InitializeHistoryComparisonCommands();
            InitializeBackupCalendarCommands();
            InitializeBackupGalleryCommands();
            InitializePlaySessionHistory();
            InitializeProgressRecapCommands();
            InitializeLineageGraphCommands();
            pendingBackupArrivals = Array.Empty<BackupVersionDto>();
            recentBackupArrivals = Array.Empty<BackupVersionDto>();
            loadedBackupHistoryView = new CollectionViewSource { Source = Backups }.View;
            loadedBackupHistoryView.Filter = FilterBackupHistory;
            backupSearchResults = new BatchObservableCollection<BackupVersionDto>();
            searchedBackupHistoryView = new CollectionViewSource { Source = backupSearchResults }.View;
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
                _ =>
                {
                    if (backupSearchPage != null) Observe(SearchBackupHistoryAsync(loadMore: true));
                    else Run(LoadMoreBackupHistoryAsync);
                },
                _ => !IsBusy && !IsLoadingMoreBackupHistory && backupSearchRequest == null
                    && (backupSearchPage?.HasMore ?? BackupHistoryHasMore) && SelectedGame != null);
            SearchAllBackupHistoryCommand = new RelayCommand(
                _ => Observe(SearchBackupHistoryAsync(loadMore: false)),
                _ => !IsBusy && CurrentWorkspace == WorkspaceKind.Saves && SelectedGame != null
                    && !string.IsNullOrWhiteSpace(BackupHistorySearchText));
            RefreshSelectedBackupLineage();
        }

        private bool FilterBackupHistory(object item)
        {
            if (!(item is BackupVersionDto backup)
                || !BackupHistoryDateRange.Matches(backup, BackupHistoryRange, DateTime.Now))
                return false;
            if (backupHistoryCalendarDay.HasValue && (backup.CreatedUtc == DateTime.MinValue
                || BackupHistoryDateRange.ConvertUtcToLocal(backup.CreatedUtc, TimeZoneInfo.Local).Date != backupHistoryCalendarDay.Value))
                return false;

            var search = BackupHistorySearchText.Trim();
            if (search.Length == 0) return true;

            return ContainsBackupHistoryText(backup.BackupId, search)
                || ContainsBackupHistoryText(backup.LudusaviName, search)
                || ContainsBackupHistoryText(backup.Comment, search)
                || ContainsBackupHistoryText(backup.SourceDevice, search)
                || ContainsBackupHistoryText(backup.OperatingSystem, search)
                || ContainsBackupHistoryText(backup.BackupTypeDisplay, search)
                || ContainsBackupHistoryText(backup.RetentionProtectionDisplay, search)
                || ContainsBackupHistoryText(backup.RestoreReadinessStatusDisplay, search);
        }

        private static bool ContainsBackupHistoryText(string? value, string search)
            => value != null
               && value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;

        private static string TruncateSearchSummary(string value)
            => value.Length <= 28 ? value : value.Substring(0, 27) + "…";

        private void OnBackupHistoryCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(BackupHistoryRangeSummary));
            OnPropertyChanged(nameof(BackupHistoryTotalCount));
            RefreshSelectedBackupLineage();
            RaiseCommandStates();
        }

        private void RefreshBackupHistoryView()
        {
            loadedBackupHistoryView?.Refresh();
        }

        private void ResetBackupHistorySearch()
        {
            var request = backupSearchRequest;
            backupSearchRequest = null;
            backupSearchGameId = null;
            request?.Cancel();
            var hadResults = backupSearchPage != null;
            backupSearchPage = null;
            backupSearchStatus = string.Empty;
            if (isShowingFocusedHistoryBackup)
            {
                isShowingFocusedHistoryBackup = false;
                OnPropertyChanged(nameof(IsShowingFocusedHistoryBackup));
            }
            if (hadResults) OnPropertyChanged(nameof(BackupHistoryView));
            if (hadResults) backupSearchResults.Clear();
            OnPropertyChanged(nameof(BackupHistoryRangeSummary));
        }

        private async Task SearchBackupHistoryAsync(bool loadMore)
        {
            var gameId = SelectedGame?.PlayniteId;
            var term = BackupHistorySearchText.Trim();
            if (string.IsNullOrWhiteSpace(gameId) || (term.Length == 0 && !backupHistoryCalendarDay.HasValue)) return;

            backupSearchRequest?.Cancel();
            using var request = new CancellationTokenSource();
            backupSearchRequest = request;
            backupSearchGameId = gameId;
            var cursor = loadMore ? backupSearchPage : null;
            (DateTime? FromUtc, DateTime? BeforeUtc) bounds = loadMore ? backupSearchBounds : backupHistoryCalendarDay.HasValue
                ? ((DateTime?)backupHistoryCalendarDay.Value.ToUniversalTime(), (DateTime?)backupHistoryCalendarDay.Value.AddDays(1).ToUniversalTime())
                : BackupHistoryDateRange.GetUtcBounds(BackupHistoryRange, DateTime.Now);
            backupSearchStatus = loadMore ? "正在加载更多匹配版本…" : "正在读取匹配的历史版本…";
            OnPropertyChanged(nameof(BackupHistoryRangeSummary));
            RaiseCommandStates();
            try
            {
                var page = await plugin.RequestAsync<BackupVersionPageDto>(
                    MessageTypes.ListBackupHistoryPage,
                    new BackupHistoryPageQueryDto
                    {
                        PlayniteId = gameId!,
                        Limit = BackupHistoryPageSize,
                        SearchText = term,
                        CreatedFromUtc = bounds.FromUtc,
                        CreatedBeforeUtc = bounds.BeforeUtc,
                        BeforeCreatedUtcKey = cursor?.NextCreatedUtcKey ?? string.Empty,
                        BeforeBackupId = cursor?.NextBackupId ?? string.Empty
                    },
                    cancellationToken: request.Token).ConfigureAwait(false);
                ApplyOnUi(() =>
                {
                    if (!ReferenceEquals(backupSearchRequest, request)
                        || !IsSelectedGame(gameId!) || CurrentWorkspace != WorkspaceKind.Saves) return;

                    var previousSelection = SelectedBackup?.BackupId;
                    if (loadMore)
                    {
                        var knownIds = new HashSet<string>(backupSearchResults.Select(item => item.BackupId), StringComparer.Ordinal);
                        foreach (var item in page.Items)
                            if (knownIds.Add(item.BackupId)) backupSearchResults.Add(item);
                    }
                    else
                    {
                        backupSearchResults.ReplaceAll(page.Items, SnapshotComparers.Backup);
                    }
                    var changedView = backupSearchPage == null;
                    backupSearchPage = page;
                    backupSearchBounds = bounds;
                    backupSearchStatus = string.Empty;
                    if (changedView) OnPropertyChanged(nameof(BackupHistoryView));
                    if (previousSelection != null)
                        SelectedBackup = backupSearchResults.FirstOrDefault(item => item.BackupId == previousSelection)!;
                });
            }
            catch (OperationCanceledException) when (request.IsCancellationRequested) { }
            catch (Exception ex)
            {
                ApplyOnUi(() =>
                {
                    if (!ReferenceEquals(backupSearchRequest, request)) return;
                    backupSearchStatus = "历史搜索失败，可重新搜索。";
                    ReportDashboardFailure(ex, true);
                });
            }
            finally
            {
                ApplyOnUi(() =>
                {
                    if (!ReferenceEquals(backupSearchRequest, request)) return;
                    backupSearchRequest = null;
                    OnPropertyChanged(nameof(BackupHistoryRangeSummary));
                    RaiseCommandStates();
                });
            }
        }

        private void ClearBackupHistoryRange()
        {
            ClearBackupCalendarSelection();
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
            RefreshBackupLineageGraph();
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
