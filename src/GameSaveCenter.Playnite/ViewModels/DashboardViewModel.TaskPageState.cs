using System;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed partial class DashboardViewModel
    {
        private bool taskPageHasLoaded;
        private bool taskPageIsLoading;
        private bool taskPageLoadFailed;
        private string taskPageErrorMessage = string.Empty;
        private DateTime? taskPageLastUpdatedUtc;

        public bool TaskPageHasLoaded
        {
            get => taskPageHasLoaded;
            private set
            {
                if (taskPageHasLoaded == value) return;
                taskPageHasLoaded = value;
                OnPropertyChanged(nameof(TaskPageHasLoaded));
                OnPropertyChanged(nameof(TaskPageState));
                OnPropertyChanged(nameof(TaskPageStatusSummary));
            }
        }

        public bool IsTaskPageLoading
        {
            get => taskPageIsLoading;
            private set
            {
                if (taskPageIsLoading == value) return;
                taskPageIsLoading = value;
                OnPropertyChanged(nameof(IsTaskPageLoading));
                OnPropertyChanged(nameof(TaskPageState));
                OnPropertyChanged(nameof(TaskPageStatusSummary));
                RaiseCommandStates();
            }
        }

        public bool TaskPageLoadFailed
        {
            get => taskPageLoadFailed;
            private set
            {
                if (taskPageLoadFailed == value) return;
                taskPageLoadFailed = value;
                OnPropertyChanged(nameof(TaskPageLoadFailed));
                OnPropertyChanged(nameof(TaskPageState));
                OnPropertyChanged(nameof(TaskPageStatusSummary));
            }
        }

        public string TaskPageErrorMessage
        {
            get => taskPageErrorMessage;
            private set
            {
                var normalized = value ?? string.Empty;
                if (string.Equals(taskPageErrorMessage, normalized, StringComparison.Ordinal)) return;
                taskPageErrorMessage = normalized;
                OnPropertyChanged(nameof(TaskPageErrorMessage));
                OnPropertyChanged(nameof(TaskPageStatusSummary));
            }
        }

        public DateTime? TaskPageLastUpdatedUtc
        {
            get => taskPageLastUpdatedUtc;
            private set
            {
                if (taskPageLastUpdatedUtc == value) return;
                taskPageLastUpdatedUtc = value;
                OnPropertyChanged(nameof(TaskPageLastUpdatedUtc));
                OnPropertyChanged(nameof(TaskPageLastUpdatedDisplay));
                OnPropertyChanged(nameof(TaskPageStatusSummary));
            }
        }

        public string TaskPageLastUpdatedDisplay => TaskPageLastUpdatedUtc.HasValue
            ? TaskPageLastUpdatedUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")
            : "未知";

        public bool TaskPageHasItems => Tasks.Count > 0;

        /// <summary>Presentation state for the task queue without conflating a filter miss with an empty store.</summary>
        public string TaskPageState
        {
            get
            {
                if (taskPageIsLoading && !TaskPageHasItems) return "Loading";
                if (taskPageLoadFailed) return TaskPageHasItems ? "ErrorWithData" : "Error";
                if (!taskPageHasLoaded) return "Loading";
                if (TasksView.IsEmpty) return TaskPageHasItems ? "FilterEmpty" : "Empty";
                return "Ready";
            }
        }

        public string TaskPageStatusSummary
        {
            get
            {
                if (taskPageIsLoading)
                    return TaskPageHasItems ? $"正在刷新，已保留 {Tasks.Count} 条旧数据。" : "正在加载任务记录…";
                if (taskPageLoadFailed)
                    return TaskPageHasItems
                        ? $"读取失败，已保留旧数据（最近更新：{TaskPageLastUpdatedDisplay}）。"
                        : "读取任务失败，请重试。";
                if (!taskPageHasLoaded) return "等待读取任务记录…";
                return $"最近更新：{TaskPageLastUpdatedDisplay}";
            }
        }

        private void BeginTaskPageLoad()
        {
            IsTaskPageLoading = true;
            TaskPageLoadFailed = false;
            TaskPageErrorMessage = string.Empty;
            NotifyTaskPageStateChanged();
        }

        private void CompleteTaskPageLoad()
        {
            TaskPageHasLoaded = true;
            IsTaskPageLoading = false;
            TaskPageLoadFailed = false;
            TaskPageErrorMessage = string.Empty;
            TaskPageLastUpdatedUtc = DateTime.UtcNow;
            NotifyTaskPageStateChanged();
        }

        private void CancelTaskPageLoad()
        {
            IsTaskPageLoading = false;
            NotifyTaskPageStateChanged();
        }

        private void FailTaskPageLoad(Exception error)
        {
            IsTaskPageLoading = false;
            TaskPageLoadFailed = true;
            TaskPageErrorMessage = error?.Message ?? "任务记录读取失败。";
            NotifyTaskPageStateChanged();
        }

        private void NotifyTaskPageStateChanged()
        {
            OnPropertyChanged(nameof(TaskPageHasItems));
            OnPropertyChanged(nameof(TaskPageState));
            OnPropertyChanged(nameof(TaskPageStatusSummary));
        }

        private void ClearTaskFilters()
        {
            taskSearchRefresh.Cancel();
            taskHistoryQueryRefresh.Cancel();
            var changed = !string.IsNullOrEmpty(taskSearchText)
                          || !string.Equals(taskStatusFilter, "全部", StringComparison.Ordinal)
                          || !string.Equals(taskGameFilter, "全部", StringComparison.Ordinal)
                          || !string.Equals(taskTypeFilter, "全部", StringComparison.Ordinal)
                          || !string.Equals(taskHistoryScope, "最近任务", StringComparison.Ordinal)
                          || !string.Equals(taskHistoryRange, "全部时间", StringComparison.Ordinal);
            taskSearchText = string.Empty;
            taskStatusFilter = "全部";
            taskGameFilter = "全部";
            taskTypeFilter = "全部";
            taskHistoryScope = "最近任务";
            taskHistoryRange = "全部时间";
            OnPropertyChanged(nameof(TaskSearchText));
            OnPropertyChanged(nameof(TaskStatusFilter));
            OnPropertyChanged(nameof(TaskGameFilter));
            OnPropertyChanged(nameof(TaskTypeFilter));
            OnPropertyChanged(nameof(TaskHistoryScope));
            OnPropertyChanged(nameof(TaskHistoryRange));
            TasksView.Refresh();
            NotifyTaskPageStateChanged();
            uiStateSave?.Schedule();
            if (changed && taskHistoryActive)
                Run(() => LoadTaskPageAsync(true));
        }
    }
}
