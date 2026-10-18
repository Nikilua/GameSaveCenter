using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed partial class DashboardViewModel
    {
        private const int NotificationInboxLimit = 500;
        private IReadOnlyList<NotificationInboxItem> allNotificationInboxItems = Array.Empty<NotificationInboxItem>();
        private NotificationInboxItem? selectedNotification;
        private string notificationSeverityFilter = NotificationInboxFilter.AllSeverities;
        private string notificationTimeFilter = NotificationInboxFilter.AllTime;
        private string notificationInboxSearchText = string.Empty;
        private string notificationInboxErrorMessage = string.Empty;
        private string notificationContextStatusMessage = string.Empty;
        private bool isNotificationInboxLoading;
        private bool isNotificationInboxMutating;
        private long notificationInboxRequestGeneration;
        private NotificationInboxReadActions notificationInboxReadActions = null!;

        public BatchObservableCollection<NotificationRecordDto> NotificationRecords { get; } = new BatchObservableCollection<NotificationRecordDto>();
        public BatchObservableCollection<NotificationInboxItem> NotificationInboxItems { get; } = new BatchObservableCollection<NotificationInboxItem>();
        public IReadOnlyList<string> NotificationSeverityOptions { get; } = new[] { "全部级别", "错误", "警告", "成功", "信息" };
        public IReadOnlyList<string> NotificationTimeOptions { get; } = new[]
        {
            NotificationInboxFilter.AllTime,
            NotificationInboxFilter.Last24Hours,
            NotificationInboxFilter.Last7Days,
            NotificationInboxFilter.Last30Days
        };

        public ICommand RefreshNotificationInboxCommand { get; private set; } = null!;
        public ICommand MarkSelectedNotificationReadCommand { get; private set; } = null!;
        public ICommand MarkVisibleNotificationsReadCommand { get; private set; } = null!;
        public ICommand ClearReadNotificationRecordsCommand { get; private set; } = null!;
        public ICommand OpenSelectedNotificationGameCommand { get; private set; } = null!;
        public ICommand OpenSelectedNotificationTaskCommand { get; private set; } = null!;

        public NotificationInboxItem? SelectedNotification
        {
            get => selectedNotification;
            set
            {
                if (!string.Equals(selectedNotification?.NotificationId, value?.NotificationId, StringComparison.Ordinal))
                    NotificationContextStatusMessage = string.Empty;
                SetValue(ref selectedNotification, value);
                OnPropertyChanged(nameof(HasSelectedNotificationGameId));
                OnPropertyChanged(nameof(HasSelectedNotificationTaskId));
                OnPropertyChanged(nameof(NotificationContextMessage));
                RaiseCommandStates();
            }
        }

        public bool HasSelectedNotificationGameId => SelectedNotification != null
            && !string.IsNullOrWhiteSpace(SelectedNotification.GameId);
        public bool HasSelectedNotificationTaskId => SelectedNotification != null
            && !string.IsNullOrWhiteSpace(SelectedNotification.TaskId);
        public string NotificationContextMessage => !string.IsNullOrWhiteSpace(notificationContextStatusMessage)
            ? notificationContextStatusMessage + " 已读只表示查看，不代表问题已解决。"
            : SelectedNotification == null
                ? "请选择一条通知查看关联入口。"
                : !HasSelectedNotificationGameId && !HasSelectedNotificationTaskId
                    ? "这条通知保留了可读摘要，但没有可定位的游戏或任务 ID。已读只表示查看，不代表问题已解决。"
                    : "关联内容按稳定 ID 定位；不会按名称猜测。已读只表示查看，不代表问题已解决。";

        private string NotificationContextStatusMessage
        {
            get => notificationContextStatusMessage;
            set
            {
                if (string.Equals(notificationContextStatusMessage, value ?? string.Empty, StringComparison.Ordinal)) return;
                notificationContextStatusMessage = value ?? string.Empty;
                OnPropertyChanged(nameof(NotificationContextMessage));
            }
        }

        public string NotificationSeverityFilter
        {
            get => notificationSeverityFilter;
            set
            {
                var normalized = NotificationSeverityOptions.Contains(value) ? value : NotificationInboxFilter.AllSeverities;
                if (string.Equals(notificationSeverityFilter, normalized, StringComparison.Ordinal)) return;
                notificationSeverityFilter = normalized;
                OnPropertyChanged(nameof(NotificationSeverityFilter));
                ApplyNotificationInboxFilters();
            }
        }

        public string NotificationTimeFilter
        {
            get => notificationTimeFilter;
            set
            {
                var normalized = NotificationTimeOptions.Contains(value) ? value : NotificationInboxFilter.AllTime;
                if (string.Equals(notificationTimeFilter, normalized, StringComparison.Ordinal)) return;
                notificationTimeFilter = normalized;
                OnPropertyChanged(nameof(NotificationTimeFilter));
                ApplyNotificationInboxFilters();
            }
        }

        public string NotificationInboxSearchText
        {
            get => notificationInboxSearchText;
            set
            {
                if (string.Equals(notificationInboxSearchText, value ?? string.Empty, StringComparison.Ordinal)) return;
                notificationInboxSearchText = value ?? string.Empty;
                OnPropertyChanged(nameof(NotificationInboxSearchText));
                ApplyNotificationInboxFilters();
            }
        }

        public bool IsNotificationInboxLoading
        {
            get => isNotificationInboxLoading;
            private set
            {
                if (isNotificationInboxLoading == value) return;
                SetValue(ref isNotificationInboxLoading, value);
                OnPropertyChanged(nameof(NotificationInboxLoadStatus));
                OnPropertyChanged(nameof(NotificationInboxHasNoRecords));
                OnPropertyChanged(nameof(NotificationInboxHasNoFilterResults));
                RaiseCommandStates();
            }
        }

        public bool IsNotificationInboxMutating
        {
            get => isNotificationInboxMutating;
            private set
            {
                if (isNotificationInboxMutating == value) return;
                SetValue(ref isNotificationInboxMutating, value);
                OnPropertyChanged(nameof(NotificationInboxLoadStatus));
                RaiseCommandStates();
            }
        }

        public string NotificationInboxErrorMessage
        {
            get => notificationInboxErrorMessage;
            private set
            {
                var normalized = value ?? string.Empty;
                if (string.Equals(notificationInboxErrorMessage, normalized, StringComparison.Ordinal)) return;
                SetValue(ref notificationInboxErrorMessage, normalized);
                OnPropertyChanged(nameof(HasNotificationInboxError));
                OnPropertyChanged(nameof(NotificationInboxHasNoRecords));
            }
        }

        public bool HasNotificationInboxError => !string.IsNullOrWhiteSpace(NotificationInboxErrorMessage);
        public string NotificationInboxLoadStatus => IsNotificationInboxMutating
            ? "正在更新通知状态…"
            : IsNotificationInboxLoading ? "正在更新通知…" : string.Empty;
        public int NotificationUnreadCount => NotificationRecords.Count(record => !record.IsRead);
        public int NotificationReadCount => NotificationRecords.Count(record => record.IsRead);
        public bool CanMarkSelectedNotificationRead =>
            !IsNotificationInboxLoading && !IsNotificationInboxMutating && SelectedNotification is { IsRead: false };
        public bool CanMarkVisibleNotificationsRead =>
            !IsNotificationInboxLoading && !IsNotificationInboxMutating && NotificationInboxItems.Any(item => !item.IsRead);
        public bool CanClearReadNotificationRecords =>
            !IsNotificationInboxLoading && !IsNotificationInboxMutating && NotificationReadCount > 0;
        public string NotificationInboxCountSummary => NotificationRecords.Count == 0
            ? "最近 30 天 · 暂无记录"
            : $"最近 30 天 · 未读 {NotificationUnreadCount} · 当前显示 {NotificationInboxItems.Count}/{NotificationRecords.Count}";
        public bool NotificationInboxHasNoRecords => !IsNotificationInboxLoading && NotificationRecords.Count == 0 && !HasNotificationInboxError;
        public bool NotificationInboxHasNoFilterResults => !IsNotificationInboxLoading && NotificationRecords.Count > 0 && NotificationInboxItems.Count == 0;

        partial void OnNotificationInboxInitialize()
        {
            RefreshNotificationInboxCommand = new RelayCommand(
                _ => Run(LoadNotificationInboxAsync),
                _ => !IsNotificationInboxLoading && !IsNotificationInboxMutating);
            notificationInboxReadActions = new NotificationInboxReadActions(
                (type, payload) => plugin.RequestAsync<int>(type, payload, TimeSpan.FromSeconds(20)),
                plugin.ConfirmAsync);
            MarkSelectedNotificationReadCommand = new RelayCommand(
                _ => Run(MarkSelectedNotificationReadAsync),
                _ => CanMarkSelectedNotificationRead);
            MarkVisibleNotificationsReadCommand = new RelayCommand(
                _ => Run(MarkVisibleNotificationsReadAsync),
                _ => CanMarkVisibleNotificationsRead);
            ClearReadNotificationRecordsCommand = new RelayCommand(
                _ => Run(ClearReadNotificationRecordsAsync),
                _ => CanClearReadNotificationRecords);
            OpenSelectedNotificationGameCommand = new RelayCommand(
                _ => OpenSelectedNotificationGame(),
                _ => !IsBusy && HasSelectedNotificationGameId);
            OpenSelectedNotificationTaskCommand = new RelayCommand(
                _ => Run(OpenSelectedNotificationTaskAsync),
                _ => !IsBusy && HasSelectedNotificationTaskId);
            NotificationRecords.CollectionChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(NotificationUnreadCount));
                OnPropertyChanged(nameof(NotificationReadCount));
                OnPropertyChanged(nameof(NotificationInboxCountSummary));
                OnPropertyChanged(nameof(NotificationInboxHasNoRecords));
                RaiseCommandStates();
            };
            NotificationInboxItems.CollectionChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(NotificationInboxCountSummary));
                OnPropertyChanged(nameof(NotificationInboxHasNoFilterResults));
                RaiseCommandStates();
            };
        }

        private Task MarkSelectedNotificationReadAsync()
        {
            var selected = SelectedNotification;
            return RunNotificationRecordMutationAsync(async () =>
                (int?)await notificationInboxReadActions.MarkSelectedAsReadAsync(selected));
        }

        private Task MarkVisibleNotificationsReadAsync()
        {
            var visibleItems = NotificationInboxItems.ToList();
            return RunNotificationRecordMutationAsync(async () =>
                (int?)await notificationInboxReadActions.MarkVisibleAsReadAsync(visibleItems));
        }

        private Task ClearReadNotificationRecordsAsync()
        {
            var readRecordCount = NotificationReadCount;
            return RunNotificationRecordMutationAsync(() =>
                notificationInboxReadActions.ClearReadRecordsAsync(readRecordCount));
        }

        private void OpenSelectedNotificationGame()
        {
            var notification = SelectedNotification;
            if (notification == null || string.IsNullOrWhiteSpace(notification.GameId)) return;

            var game = TaskSourceNavigationResolver.ResolveExactGame(
                new TaskSourceReferenceDto { StableId = notification.GameId, PlayniteId = notification.GameId },
                Games);
            if (game == null)
            {
                NotificationContextStatusMessage = $"关联游戏 ID“{notification.GameId}”不在当前游戏库快照中；通知摘要仍保留。请刷新游戏库后重试，未按名称跳转。";
                StatusMessage = NotificationContextStatusMessage;
                return;
            }

            PushNavigationReturnTarget("返回通知", $"来源通知：{notification.Title} · {notification.NotificationId}");
            SaveTabIndex = 0;
            CurrentWorkspace = WorkspaceKind.Saves;
            SelectedGame = game;
            NotificationContextStatusMessage = string.Empty;
            StatusMessage = $"已按稳定游戏 ID 打开“{game.Name}”详情。可以使用顶部“{NavigationReturnLabel}”返回通知。";
            RequestWorkspaceLoad();
        }

        private async Task OpenSelectedNotificationTaskAsync()
        {
            var notification = SelectedNotification;
            var taskId = notification?.TaskId?.Trim() ?? string.Empty;
            if (notification == null || string.IsNullOrWhiteSpace(taskId)) return;

            var page = await plugin.RequestAsync<TaskPageDto>(
                MessageTypes.GetTaskPage,
                new TaskQueryDto { Limit = 1, ExactTaskId = taskId },
                TimeSpan.FromSeconds(30));
            var task = TaskSourceNavigationResolver.ResolveExactTask(taskId, page?.Items);
            ApplyOnUi(() =>
            {
                if (!string.Equals(SelectedNotification?.NotificationId, notification.NotificationId, StringComparison.Ordinal)) return;
                if (task == null)
                {
                    NotificationContextStatusMessage = $"关联任务 ID“{taskId}”已不存在或已清理；通知标题和摘要仍可查看。";
                    StatusMessage = NotificationContextStatusMessage;
                    return;
                }

                PushNavigationReturnTarget("返回通知", $"来源通知：{notification.Title} · {notification.NotificationId}");
                taskSearchRefresh.Cancel();
                taskHistoryQueryRefresh.Cancel();
                taskSearchText = task.TaskId;
                taskStatusFilter = "全部";
                taskGameFilter = "全部";
                taskTypeFilter = "全部";
                pendingTaskGameFilter = "全部";
                pendingTaskTypeFilter = "全部";
                pendingTaskDynamicFilterRestore = false;
                taskHistoryScope = "全部历史";
                taskHistoryRange = "全部时间";
                taskNavigationGameId = string.Empty;
                taskNavigationGameName = string.Empty;
                pendingNavigationTaskId = task.TaskId;
                pendingNavigationTaskIndex = -1;
                restoringNavigationTaskSelection = true;

                OnPropertyChanged(nameof(TaskSearchText));
                OnPropertyChanged(nameof(TaskStatusFilter));
                OnPropertyChanged(nameof(TaskGameFilter));
                OnPropertyChanged(nameof(TaskTypeFilter));
                OnPropertyChanged(nameof(TaskHistoryScope));
                OnPropertyChanged(nameof(TaskHistoryRange));
                OnPropertyChanged(nameof(TaskLoadedSummary));
                OnPropertyChanged(nameof(TaskTotalCountLabel));
                OnPropertyChanged(nameof(HasTaskNavigationTarget));
                OnPropertyChanged(nameof(TaskNavigationSourceSummary));
                OnPropertyChanged(nameof(TaskHasActiveFilters));
                OnPropertyChanged(nameof(TaskActiveFiltersSummary));

                RefreshTasksView();
                CurrentWorkspace = WorkspaceKind.Tasks;
                RequestTaskHistoryRefresh(immediate: true, force: true);
                NotificationContextStatusMessage = string.Empty;
                StatusMessage = $"已按任务 ID 打开“{task.TaskTypeDisplay}”任务详情；可从任务来源继续进入版本或关联问题。";
                RaiseCommandStates();
            });
        }

        private async Task RunNotificationRecordMutationAsync(Func<Task<int?>> mutation)
        {
            if (IsNotificationInboxLoading || IsNotificationInboxMutating) return;

            IsNotificationInboxMutating = true;
            NotificationInboxErrorMessage = string.Empty;
            try
            {
                var result = await mutation();
                if (result.HasValue)
                    await LoadNotificationInboxAsync();
            }
            catch (Exception ex) when (!(ex is OutOfMemoryException) && !(ex is StackOverflowException))
            {
                NotificationInboxErrorMessage = "通知状态暂时无法更新。请检查后台服务后重试。";
            }
            finally
            {
                IsNotificationInboxMutating = false;
            }
        }

        private async Task LoadNotificationInboxAsync()
        {
            var generation = Interlocked.Increment(ref notificationInboxRequestGeneration);
            ApplyOnUi(() =>
            {
                IsNotificationInboxLoading = true;
                NotificationInboxErrorMessage = string.Empty;
            });

            try
            {
                var records = await plugin.RequestAsync<List<NotificationRecordDto>>(
                    MessageTypes.GetNotificationRecords,
                    new NotificationRecordsQueryDto { Limit = NotificationInboxLimit },
                    TimeSpan.FromSeconds(20)).ConfigureAwait(false);
                ApplyOnUi(() =>
                {
                    if (generation != Interlocked.Read(ref notificationInboxRequestGeneration)) return;
                    var selectedId = SelectedNotification?.NotificationId;
                    NotificationRecords.ReplaceAll(records ?? new List<NotificationRecordDto>());
                    allNotificationInboxItems = NotificationRecords
                        .Select(record => new NotificationInboxItem(record))
                        .ToList();
                    ApplyNotificationInboxFilters(selectedId);
                    NotificationInboxErrorMessage = string.Empty;
                });
            }
            catch (Exception ex) when (!(ex is OutOfMemoryException) && !(ex is StackOverflowException))
            {
                ApplyOnUi(() =>
                {
                    if (generation != Interlocked.Read(ref notificationInboxRequestGeneration)) return;
                    NotificationInboxErrorMessage = "通知暂时无法加载。请检查后台服务后重试。";
                });
            }
            finally
            {
                ApplyOnUi(() =>
                {
                    if (generation == Interlocked.Read(ref notificationInboxRequestGeneration))
                        IsNotificationInboxLoading = false;
                });
            }
        }

        private void ApplyNotificationInboxFilters(string? selectedId = null)
        {
            var search = NotificationInboxSearchText.Trim();
            var nowUtc = DateTime.UtcNow;
            var filtered = allNotificationInboxItems.Where(item =>
            {
                if (!NotificationInboxFilter.Matches(item.Record, NotificationSeverityFilter, NotificationTimeFilter, nowUtc))
                    return false;
                return search.Length == 0
                    || ContainsNotificationSearchValue(item.Title, search)
                    || ContainsNotificationSearchValue(item.Summary, search)
                    || ContainsNotificationSearchValue(item.GameId, search)
                    || ContainsNotificationSearchValue(item.TaskId, search);
            }).ToList();
            NotificationInboxItems.ReplaceAll(filtered, (left, right) =>
                string.Equals(left.NotificationId, right.NotificationId, StringComparison.Ordinal));
            SelectedNotification = filtered.FirstOrDefault(item =>
                !string.IsNullOrWhiteSpace(selectedId)
                    && string.Equals(item.NotificationId, selectedId, StringComparison.Ordinal))
                ?? (selectedNotification != null && filtered.Contains(selectedNotification) ? selectedNotification : filtered.FirstOrDefault());
            OnPropertyChanged(nameof(NotificationInboxCountSummary));
            OnPropertyChanged(nameof(NotificationInboxHasNoRecords));
            OnPropertyChanged(nameof(NotificationInboxHasNoFilterResults));
            RaiseCommandStates();
        }

        private static bool ContainsNotificationSearchValue(string? value, string search)
            => value != null
               && value.Length > 0
               && value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public sealed class NotificationInboxItem
    {
        public NotificationInboxItem(NotificationRecordDto record)
        {
            Record = record ?? throw new ArgumentNullException(nameof(record));
            NotificationId = record.NotificationId ?? string.Empty;
            Title = string.IsNullOrWhiteSpace(record.Title) ? "通知" : record.Title;
            Summary = record.Summary ?? string.Empty;
            GameId = record.GameId ?? string.Empty;
            TaskId = record.TaskId ?? string.Empty;
            Severity = record.Severity;
            SeverityDisplay = NotificationInboxFilter.SeverityName(record.Severity);
            CreatedUtc = record.CreatedUtc.Kind == DateTimeKind.Local
                ? record.CreatedUtc.ToUniversalTime()
                : DateTime.SpecifyKind(record.CreatedUtc, DateTimeKind.Utc);
            CreatedRelativeDisplay = TimeDisplayFormatter.Relative(CreatedUtc, DateTime.UtcNow);
            CreatedFullDisplay = TimeDisplayFormatter.Full(CreatedUtc);
            CreatedRawUtcDisplay = TimeDisplayFormatter.RawUtc(CreatedUtc);
            IsRead = record.IsRead;
        }

        public NotificationRecordDto Record { get; }
        public string NotificationId { get; }
        public string Title { get; }
        public string Summary { get; }
        public string GameId { get; }
        public string TaskId { get; }
        public NotificationSeverity Severity { get; }
        public string SeverityDisplay { get; }
        public DateTime CreatedUtc { get; }
        public string CreatedRelativeDisplay { get; }
        public string CreatedFullDisplay { get; }
        public string CreatedRawUtcDisplay { get; }
        public bool IsRead { get; }
        public string ReadStateDisplay => IsRead ? "已读" : "未读";
    }
}
