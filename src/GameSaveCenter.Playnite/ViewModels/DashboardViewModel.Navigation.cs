using System;
using System.Collections.Generic;
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
        private string pendingStorageBackupId = string.Empty;
        private int pendingNavigationTaskIndex = -1;
        private bool restoringNavigationTaskSelection;

        public ICommand OpenSelectedTaskGameCommand { get; private set; } = null!;
        public ICommand OpenSelectedTaskSourceCommand { get; private set; } = null!;
        public ICommand ReturnToNavigationSourceCommand { get; private set; } = null!;
        public ICommand ClearTaskNavigationContextCommand { get; private set; } = null!;
        public ICommand OpenStorageGameCommand { get; private set; } = null!;
        public ICommand OpenStorageBackupCommand { get; private set; } = null!;

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

        public IReadOnlyList<TaskSourceReferenceDto> SelectedTaskSourceReferences
        {
            get
            {
                var task = SelectedTask;
                if (task == null) return Array.Empty<TaskSourceReferenceDto>();

                var references = new List<TaskSourceReferenceDto>();
                foreach (var reference in task.SourceReferences ?? new List<TaskSourceReferenceDto>())
                {
                    if (reference == null || !reference.HasStableIdentity) continue;
                    if (references.Any(existing => existing.Kind == reference.Kind
                        && string.Equals(existing.StableId, reference.StableId, StringComparison.OrdinalIgnoreCase)))
                        continue;
                    references.Add(reference.Clone());
                }

                // Older durable tasks predate SourceReferences. Reconstruct only identities
                // already carried by the task; display names never become a navigation key.
                if (task.RestoreReport != null && !string.IsNullOrWhiteSpace(task.RestoreReport.BackupId)
                    && !references.Any(reference => reference.Kind == TaskSourceReferenceKind.BackupVersion
                        && string.Equals(reference.StableId, task.RestoreReport.BackupId, StringComparison.OrdinalIgnoreCase)))
                {
                    references.Add(new TaskSourceReferenceDto
                    {
                        Kind = TaskSourceReferenceKind.BackupVersion,
                        StableId = task.RestoreReport.BackupId,
                        PlayniteId = task.RestoreReport.PlayniteId,
                        DisplayName = task.RestoreReport.BackupId,
                        Detail = "从恢复报告恢复的目标版本；版本消失时保留任务诊断"
                    });
                }

                if (!string.IsNullOrWhiteSpace(task.GameId)
                    && !references.Any(reference => reference.Kind == TaskSourceReferenceKind.Game
                        && string.Equals(reference.StableId, task.GameId, StringComparison.OrdinalIgnoreCase)))
                {
                    references.Insert(0, new TaskSourceReferenceDto
                    {
                        Kind = TaskSourceReferenceKind.Game,
                        StableId = task.GameId,
                        PlayniteId = task.GameId,
                        DisplayName = task.GameName,
                        Detail = "任务关联的 Playnite 游戏 ID"
                    });
                }

                return references;
            }
        }

        public bool HasSelectedTaskSources => SelectedTaskSourceReferences.Count > 0;

        public IReadOnlyList<TaskSourceReferenceDto> SelectedTaskObjectReferences
            => SelectedTaskSourceReferences
                .Where(reference => reference.Kind != TaskSourceReferenceKind.Game)
                .ToList();

        public bool HasSelectedTaskObjectReferences => SelectedTaskObjectReferences.Count > 0;

        internal double TaskGridScrollOffset => taskGridScrollOffset;
        internal double MaintenanceFindingsScrollOffset => maintenanceFindingsScrollOffset;
        internal double? PendingTaskGridScrollRestore => pendingTaskGridScrollRestore;
        internal double? PendingMaintenanceFindingsScrollRestore => pendingMaintenanceFindingsScrollRestore;

        partial void OnNavigationStateInitialize()
        {
            OpenSelectedTaskGameCommand = new RelayCommand(
                _ => OpenSelectedTaskGame(),
                _ => !IsBusy && HasSelectedTaskGameTarget);
            OpenSelectedTaskSourceCommand = new RelayCommand(
                value => OpenSelectedTaskSource(value as TaskSourceReferenceDto),
                value => !IsBusy && value is TaskSourceReferenceDto reference && reference.HasStableIdentity);
            ReturnToNavigationSourceCommand = new RelayCommand(
                _ => ReturnToNavigationSource(),
                _ => !IsBusy && HasNavigationReturnTarget);
            ClearTaskNavigationContextCommand = new RelayCommand(
                _ => ClearTaskNavigationContext(),
                _ => !IsBusy && HasTaskNavigationTarget);
            OpenStorageGameCommand = new RelayCommand(
                value => OpenStorageGame(value as StorageGameRankDto),
                value => !IsBusy && value is StorageGameRankDto rank && !string.IsNullOrWhiteSpace(rank.PlayniteId));
            OpenStorageBackupCommand = new RelayCommand(
                value => OpenStorageBackup(value as StorageGameRankDto),
                value => !IsBusy && value is StorageGameRankDto rank
                    && !string.IsNullOrWhiteSpace(rank.PlayniteId)
                    && !string.IsNullOrWhiteSpace(rank.LatestBackupId));
        }

        private void ClearTaskNavigationContext()
        {
            if (!HasTaskNavigationTarget) return;

            var gameName = taskNavigationGameName;
            taskNavigationGameId = string.Empty;
            taskNavigationGameName = string.Empty;
            OnPropertyChanged(nameof(HasTaskNavigationTarget));
            OnPropertyChanged(nameof(TaskNavigationSourceSummary));
            OnPropertyChanged(nameof(TaskHasActiveFilters));
            OnPropertyChanged(nameof(TaskActiveFiltersSummary));

            // Keep the existing request cancellation and finite-list refresh path. The
            // source target is transient and must not overwrite the user's other draft
            // filters while the service query is invalidated.
            RefreshTasksView();
            RequestTaskHistoryRefresh(immediate: true, force: taskHistoryActive);
            StatusMessage = $"已清除带入的游戏条件“{gameName}”，保留其他任务筛选。";
            RaiseCommandStates();
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

        private void OpenSelectedTaskSource(TaskSourceReferenceDto? source)
        {
            if (source == null || !source.HasStableIdentity) return;

            switch (source.Kind)
            {
                case TaskSourceReferenceKind.Game:
                    OpenTaskGame(string.IsNullOrWhiteSpace(source.PlayniteId) ? source.StableId : source.PlayniteId);
                    break;
                case TaskSourceReferenceKind.BackupVersion:
                    OpenTaskBackupVersion(source);
                    break;
                case TaskSourceReferenceKind.MediaBatch:
                    OpenTaskMediaBatch(source);
                    break;
                case TaskSourceReferenceKind.CloudTransfer:
                    OpenTaskCloudTransfer(source);
                    break;
                default:
                    StatusMessage = $"任务来源“{source.IdentityDisplay}”类型未知，已保留任务诊断，未执行跳转。";
                    break;
            }
        }

        private void OpenStorageGame(StorageGameRankDto? rank)
        {
            if (rank == null || string.IsNullOrWhiteSpace(rank.PlayniteId)) return;

            var game = TaskSourceNavigationResolver.ResolveExactGame(
                new TaskSourceReferenceDto { StableId = rank.PlayniteId, PlayniteId = rank.PlayniteId }, Games);
            if (game == null)
            {
                StatusMessage = $"存储统计中的游戏（{rank.PlayniteId}）已不在当前快照中，未切换到其他游戏。请先刷新游戏库。";
                return;
            }

            PushNavigationReturnTarget("返回维护中心", $"来源：维护中心 · 存储分析 · {rank.GameName}");
            pendingStorageBackupId = string.Empty;
            SaveTabIndex = 0;
            CurrentWorkspace = WorkspaceKind.Saves;
            SelectedBackup = null!;
            SelectedGame = game;
            StatusMessage = $"已打开存储占用游戏“{game.Name}”详情。可以使用顶部“{NavigationReturnLabel}”返回维护中心。";
            RequestWorkspaceLoad();
        }

        private void OpenStorageBackup(StorageGameRankDto? rank)
        {
            if (rank == null || string.IsNullOrWhiteSpace(rank.PlayniteId) || string.IsNullOrWhiteSpace(rank.LatestBackupId)) return;

            var game = TaskSourceNavigationResolver.ResolveExactGame(
                new TaskSourceReferenceDto { StableId = rank.PlayniteId, PlayniteId = rank.PlayniteId }, Games);
            if (game == null)
            {
                StatusMessage = $"存储统计中的游戏（{rank.PlayniteId}）已不在当前快照中，未跳转到其他游戏或版本。";
                return;
            }

            PushNavigationReturnTarget("返回维护中心", $"来源：维护中心 · 存储分析 · {rank.GameName}");
            pendingStorageBackupId = rank.LatestBackupId;
            SaveTabIndex = 0;
            CurrentWorkspace = WorkspaceKind.Saves;
            SelectedBackup = null!;
            SelectedGame = game;
            StatusMessage = $"正在打开存储分析对应版本“{rank.LatestBackupId}”；仅按稳定游戏/版本 ID 查找。";
            RequestWorkspaceLoad();
        }

        private void OpenSelectedTaskGame()
        {
            var task = SelectedTask;
            if (task == null || string.IsNullOrWhiteSpace(task.GameId)) return;

            OpenTaskGame(task.GameId);
        }

        private void OpenTaskGame(string gameId)
        {
            var task = SelectedTask;
            if (task == null || string.IsNullOrWhiteSpace(gameId)) return;

            var game = TaskSourceNavigationResolver.ResolveExactGame(
                new TaskSourceReferenceDto { StableId = gameId, PlayniteId = gameId }, Games);
            if (game == null)
            {
                StatusMessage = $"任务来源游戏（{gameId}）已不在当前快照中，未切换到其他游戏。请先刷新游戏库。";
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

        private void OpenTaskBackupVersion(TaskSourceReferenceDto source)
        {
            var task = SelectedTask;
            var resolvedGameId = string.IsNullOrWhiteSpace(source.PlayniteId) ? task?.GameId ?? string.Empty : source.PlayniteId;
            if (string.IsNullOrWhiteSpace(resolvedGameId))
            {
                StatusMessage = $"任务来源版本“{source.IdentityDisplay}”缺少可验证的游戏 ID，保留诊断但未跳转。";
                return;
            }

            var game = TaskSourceNavigationResolver.ResolveExactGame(
                new TaskSourceReferenceDto
                {
                    StableId = source.StableId,
                    PlayniteId = resolvedGameId
                }, Games);
            if (game == null)
            {
                StatusMessage = $"任务来源游戏（{resolvedGameId}）已不在当前快照中，未切换到其他游戏，也未跳转同名版本。";
                return;
            }

            PushNavigationReturnTarget("返回任务", $"来源：任务中心 · {task?.GameName ?? game.Name}");
            pendingTaskBackupId = source.StableId;
            SaveTabIndex = 0;
            CurrentWorkspace = WorkspaceKind.Saves;
            SelectedBackup = null!;
            SelectedGame = game;
            StatusMessage = $"正在打开任务来源版本“{source.StableId}”；仅按稳定版本 ID 查找。";
            RequestWorkspaceLoad();
        }

        private void OpenTaskMediaBatch(TaskSourceReferenceDto source)
        {
            var task = SelectedTask;
            PushNavigationReturnTarget("返回任务", $"来源：任务中心 · {task?.GameName ?? source.IdentityDisplay}");
            pendingMediaClassificationBatchId = source.StableId;
            MediaTabIndex = 0;
            CurrentWorkspace = WorkspaceKind.Media;
            RequestWorkspaceLoad();
            StatusMessage = $"正在打开任务来源媒体批次“{source.StableId}”；仅按稳定批次 ID 查找。";
        }

        private void OpenTaskCloudTransfer(TaskSourceReferenceDto source)
        {
            CloudTransferKind kind;
            if (source.StableId.StartsWith("Backup:", StringComparison.OrdinalIgnoreCase))
                kind = CloudTransferKind.Backup;
            else if (source.StableId.StartsWith("Media:", StringComparison.OrdinalIgnoreCase))
                kind = CloudTransferKind.Media;
            else
            {
                StatusMessage = $"任务来源云队列“{source.IdentityDisplay}”缺少可验证的队列键，保留诊断但未跳转。";
                return;
            }

            PushNavigationReturnTarget("返回任务", $"来源：任务中心 · {SelectedTask?.GameName ?? source.IdentityDisplay}");
            OpenCloudQueue(source.StableId, transferKind: kind);
            StatusMessage = $"正在打开任务来源云队列“{source.StableId}”；不会按游戏名称替换对象。";
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
            OnPropertyChanged(nameof(HasTaskNavigationTarget));
            OnPropertyChanged(nameof(TaskNavigationSourceSummary));
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
