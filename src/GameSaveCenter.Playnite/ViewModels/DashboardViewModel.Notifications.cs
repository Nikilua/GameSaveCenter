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
        private bool isNotificationInboxLoading;
        private long notificationInboxRequestGeneration;

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

        public NotificationInboxItem? SelectedNotification
        {
            get => selectedNotification;
            set => SetValue(ref selectedNotification, value);
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
        public string NotificationInboxLoadStatus => IsNotificationInboxLoading ? "正在更新通知…" : string.Empty;
        public int NotificationUnreadCount => NotificationRecords.Count(record => !record.IsRead);
        public string NotificationInboxCountSummary => NotificationRecords.Count == 0
            ? "最近 30 天 · 暂无记录"
            : $"最近 30 天 · 未读 {NotificationUnreadCount} · 当前显示 {NotificationInboxItems.Count}/{NotificationRecords.Count}";
        public bool NotificationInboxHasNoRecords => !IsNotificationInboxLoading && NotificationRecords.Count == 0 && !HasNotificationInboxError;
        public bool NotificationInboxHasNoFilterResults => !IsNotificationInboxLoading && NotificationRecords.Count > 0 && NotificationInboxItems.Count == 0;

        partial void OnNotificationInboxInitialize()
        {
            RefreshNotificationInboxCommand = new RelayCommand(
                _ => Run(LoadNotificationInboxAsync),
                _ => !IsNotificationInboxLoading);
            NotificationRecords.CollectionChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(NotificationUnreadCount));
                OnPropertyChanged(nameof(NotificationInboxCountSummary));
                OnPropertyChanged(nameof(NotificationInboxHasNoRecords));
            };
            NotificationInboxItems.CollectionChanged += (_, __) =>
            {
                OnPropertyChanged(nameof(NotificationInboxCountSummary));
                OnPropertyChanged(nameof(NotificationInboxHasNoFilterResults));
            };
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
