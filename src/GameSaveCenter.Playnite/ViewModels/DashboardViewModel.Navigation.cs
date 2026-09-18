using System;
using System.Linq;
using System.Windows.Input;

using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed partial class DashboardViewModel
    {
        private readonly WorkspaceNavigationStack navigationHistory = new WorkspaceNavigationStack();
        private double taskGridScrollOffset;
        private double maintenanceFindingsScrollOffset;
        private double? pendingTaskGridScrollRestore;
        private double? pendingMaintenanceFindingsScrollRestore;
        private string pendingNavigationTaskId = string.Empty;
        private int pendingNavigationTaskIndex = -1;
        private bool restoringNavigationTaskSelection;

        public ICommand OpenSelectedTaskGameCommand { get; private set; } = null!;
        public ICommand ReturnToNavigationSourceCommand { get; private set; } = null!;

        public bool HasNavigationReturnTarget => navigationHistory.CanReturn;

        public string NavigationReturnLabel
            => navigationHistory.TryPeek(out var entry) ? entry.ReturnLabel : string.Empty;

        public string NavigationReturnToolTip
            => navigationHistory.TryPeek(out var entry)
                ? $"{entry.ReturnLabel}：{entry.SourceSummary}"
                : string.Empty;

        public string NavigationSourceSummary
            => navigationHistory.TryPeek(out var entry) ? entry.SourceSummary : string.Empty;

        public bool HasSelectedTaskGameTarget
            => SelectedTask != null && !string.IsNullOrWhiteSpace(SelectedTask.GameId);

        internal double TaskGridScrollOffset => taskGridScrollOffset;
        internal double MaintenanceFindingsScrollOffset => maintenanceFindingsScrollOffset;
        internal double? PendingTaskGridScrollRestore => pendingTaskGridScrollRestore;
        internal double? PendingMaintenanceFindingsScrollRestore => pendingMaintenanceFindingsScrollRestore;

        partial void OnNavigationStateInitialize()
        {
            OpenSelectedTaskGameCommand = new RelayCommand(
                _ => OpenSelectedTaskGame(),
                _ => !IsBusy && HasSelectedTaskGameTarget);
            ReturnToNavigationSourceCommand = new RelayCommand(
                _ => ReturnToNavigationSource(),
                _ => !IsBusy && HasNavigationReturnTarget);
        }

        internal void SetTaskGridScrollOffset(double offset)
            => taskGridScrollOffset = NormalizeOffset(offset);

        internal void SetMaintenanceFindingsScrollOffset(double offset)
            => maintenanceFindingsScrollOffset = NormalizeOffset(offset);

        internal void RequestTaskGridScrollRestore()
        {
            if (!navigationHistory.TryPeek(out var entry)) return;
            RequestTaskGridScrollRestore(entry);
        }

        private void RequestTaskGridScrollRestore(WorkspaceNavigationSnapshot entry)
        {
            pendingTaskGridScrollRestore = entry.TaskScrollOffset;
            OnPropertyChanged(nameof(PendingTaskGridScrollRestore));
        }

        internal void RequestMaintenanceFindingsScrollRestore()
        {
            if (!navigationHistory.TryPeek(out var entry)) return;
            RequestMaintenanceFindingsScrollRestore(entry);
        }

        private void RequestMaintenanceFindingsScrollRestore(WorkspaceNavigationSnapshot entry)
        {
            pendingMaintenanceFindingsScrollRestore = entry.MaintenanceFindingsScrollOffset;
            OnPropertyChanged(nameof(PendingMaintenanceFindingsScrollRestore));
        }

        internal void CompleteTaskGridScrollRestore()
        {
            if (!pendingTaskGridScrollRestore.HasValue) return;
            pendingTaskGridScrollRestore = null;
            OnPropertyChanged(nameof(PendingTaskGridScrollRestore));
        }

        internal void CompleteMaintenanceFindingsScrollRestore()
        {
            if (!pendingMaintenanceFindingsScrollRestore.HasValue) return;
            pendingMaintenanceFindingsScrollRestore = null;
            OnPropertyChanged(nameof(PendingMaintenanceFindingsScrollRestore));
        }

        private void PushNavigationReturnTarget(string returnLabel, string sourceSummary)
        {
            navigationHistory.Push(new WorkspaceNavigationSnapshot(
                CurrentWorkspace,
                SelectedGame?.PlayniteId ?? string.Empty,
                SaveTabIndex,
                MediaTabIndex,
                MaintenanceTabIndex,
                TaskSearchText,
                TaskStatusFilter,
                TaskGameFilter,
                TaskTypeFilter,
                TaskHistoryScope,
                TaskHistoryRange,
                taskNavigationGameId,
                taskNavigationGameName,
                SelectedTask?.TaskId ?? string.Empty,
                SelectedTask == null ? -1 : Tasks.IndexOf(SelectedTask),
                BuildFindingSelectionKey(SelectedFinding) ?? string.Empty,
                taskGridScrollOffset,
                maintenanceFindingsScrollOffset,
                returnLabel,
                sourceSummary));
            OnNavigationHistoryChanged();
        }

        private void OpenSelectedTaskGame()
        {
            var task = SelectedTask;
            if (task == null || string.IsNullOrWhiteSpace(task.GameId)) return;

            var game = Games.FirstOrDefault(candidate =>
                string.Equals(candidate.PlayniteId, task.GameId, StringComparison.OrdinalIgnoreCase));
            if (game == null)
            {
                StatusMessage = $"任务对应的游戏（{task.GameId}）已不在当前快照中，未切换到其他游戏。请先刷新游戏库。";
                return;
            }

            PushNavigationReturnTarget("返回任务", $"来源：任务中心 · {task.GameName}");
            SaveTabIndex = 0;
            CurrentWorkspace = WorkspaceKind.Saves;
            // Select after the workspace changes so the existing game-selection
            // observer never starts a details load against the task page.
            SelectedGame = game;
            StatusMessage = $"已打开任务对应的游戏“{game.Name}”详情。可以使用顶部“{NavigationReturnLabel}”返回任务。";
            RequestWorkspaceLoad();
        }

        private void ReturnToNavigationSource()
        {
            if (!navigationHistory.TryPop(out var entry)) return;

            // Change the workspace before re-selecting the game. Selection changes are
            // allowed to request game-scoped details, so restoring it while the target
            // page is still visible would start an unnecessary load in the wrong page.
            CurrentWorkspace = entry.Workspace;
            var selectedGameRestored = RestoreSelectedGame(entry.SelectedGameId);
            switch (entry.Workspace)
            {
                case WorkspaceKind.Maintenance:
                    MaintenanceTabIndex = entry.MaintenanceTabIndex;
                    SelectedFinding = Findings.FirstOrDefault(finding =>
                        string.Equals(BuildFindingSelectionKey(finding), entry.SelectedFindingKey, StringComparison.Ordinal));
                    RequestMaintenanceFindingsScrollRestore(entry);
                    RequestWorkspaceLoad();
                    if (!selectedGameRestored)
                        StatusMessage = "返回来源时找不到原游戏，未自动选择其他游戏；";
                    else if (SelectedFinding == null && !string.IsNullOrWhiteSpace(entry.SelectedFindingKey))
                        StatusMessage = "来源诊断项已不在最新结果中，已返回维护中心但没有替换选择。请先刷新诊断。";
                    else
                        StatusMessage = $"已返回{entry.SourceSummary}。";
                    break;
                case WorkspaceKind.Tasks:
                    RestoreTaskSnapshot(entry);
                    RequestTaskGridScrollRestore(entry);
                    Run(() => LoadTaskPageAsync(true));
                    if (!selectedGameRestored)
                        StatusMessage += " 原游戏已不在当前快照中，未自动选择其他游戏。";
                    break;
                default:
                    CurrentWorkspace = entry.Workspace;
                    RequestWorkspaceLoad();
                    StatusMessage = selectedGameRestored
                        ? $"已返回{entry.SourceSummary}。"
                        : $"已返回{entry.SourceSummary}，但原游戏已不在当前快照中，未自动选择其他游戏。";
                    break;
            }

            OnNavigationHistoryChanged();
        }

        private void RestoreTaskSnapshot(WorkspaceNavigationSnapshot entry)
        {
            taskSearchRefresh.Cancel();
            taskHistoryQueryRefresh.Cancel();
            taskSearchText = entry.TaskSearchText;
            taskStatusFilter = NormalizeTaskStatusFilter(entry.TaskStatusFilter);
            taskGameFilter = string.IsNullOrWhiteSpace(entry.TaskGameFilter) ? "全部" : entry.TaskGameFilter;
            taskTypeFilter = string.IsNullOrWhiteSpace(entry.TaskTypeFilter) ? "全部" : entry.TaskTypeFilter;
            pendingTaskGameFilter = taskGameFilter;
            pendingTaskTypeFilter = taskTypeFilter;
            pendingTaskDynamicFilterRestore = true;
            taskHistoryScope = TaskHistoryScopeOptions.Contains(entry.TaskHistoryScope) ? entry.TaskHistoryScope : "最近任务";
            taskHistoryRange = TaskHistoryRangeOptions.Contains(entry.TaskHistoryRange) ? entry.TaskHistoryRange : "全部时间";
            taskHistoryActive = !string.Equals(taskHistoryScope, "最近任务", StringComparison.Ordinal)
                                || !string.Equals(taskHistoryRange, "全部时间", StringComparison.Ordinal);
            taskNavigationGameId = entry.TaskNavigationGameId;
            taskNavigationGameName = entry.TaskNavigationGameName;
            pendingNavigationTaskId = entry.SelectedTaskId;
            pendingNavigationTaskIndex = entry.SelectedTaskIndex;
            restoringNavigationTaskSelection = !string.IsNullOrWhiteSpace(pendingNavigationTaskId);
            OnPropertyChanged(nameof(TaskSearchText));
            OnPropertyChanged(nameof(TaskStatusFilter));
            OnPropertyChanged(nameof(TaskGameFilter));
            OnPropertyChanged(nameof(TaskTypeFilter));
            OnPropertyChanged(nameof(TaskHistoryScope));
            OnPropertyChanged(nameof(TaskHistoryRange));
            OnPropertyChanged(nameof(TaskHasActiveFilters));
            OnPropertyChanged(nameof(TaskActiveFiltersSummary));
            RefreshTasksView();
            StatusMessage = $"已返回{entry.SourceSummary}，正在恢复筛选和任务位置。";
        }

        private bool RestoreSelectedGame(string selectedGameId)
        {
            if (string.IsNullOrWhiteSpace(selectedGameId))
            {
                SelectedGame = null!;
                return true;
            }

            var game = Games.FirstOrDefault(candidate =>
                string.Equals(candidate.PlayniteId, selectedGameId, StringComparison.OrdinalIgnoreCase));
            if (game != null)
            {
                SelectedGame = game;
                return true;
            }

            SelectedGame = null!;
            return false;
        }

        private string NormalizeTaskStatusFilter(string value)
            => TaskStatusFilterOptions.Contains(value) ? value : "全部";

        private void OnNavigationHistoryChanged()
        {
            OnPropertyChanged(nameof(HasNavigationReturnTarget));
            OnPropertyChanged(nameof(NavigationReturnLabel));
            OnPropertyChanged(nameof(NavigationReturnToolTip));
            OnPropertyChanged(nameof(NavigationSourceSummary));
            RaiseCommandStates();
        }

        private static double NormalizeOffset(double value)
            => double.IsNaN(value) || double.IsInfinity(value) || value < 0 ? 0 : value;
    }
}
