using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Settings;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed partial class DashboardViewModel
    {
        private const string WorklistManualGrouping = "不分组";
        private static readonly string[] WorklistGroupModes = { WorklistManualGrouping, "按对象类型", "按引用状态" };
        private readonly ObservableCollection<WorklistDefinition> worklistBrowserLists = new ObservableCollection<WorklistDefinition>();
        private readonly ObservableCollection<WorklistBrowserEntry> worklistBrowserEntries = new ObservableCollection<WorklistBrowserEntry>();
        private readonly List<WorklistBrowserEntry> selectedWorklistBrowserEntries = new List<WorklistBrowserEntry>();
        private ICollectionView worklistBrowserView = null!;
        private WorklistDefinition? selectedWorklist;
        private WorklistBrowserEntry? selectedWorklistBrowserEntry;
        private string worklistSearchText = string.Empty;
        private string worklistGroupMode = WorklistManualGrouping;
        private string worklistLookupSummary = "引用状态尚未刷新。";
        private bool isWorklistBrowserOpen;
        private bool isResolvingWorklistReferences;
        private bool isMutatingWorklist;
        private int worklistResolutionGeneration;

        public ICommand OpenWorklistBrowserCommand { get; private set; } = null!;
        public ICommand CloseWorklistBrowserCommand { get; private set; } = null!;
        public ICommand RefreshWorklistReferencesCommand { get; private set; } = null!;
        public ICommand ContinueSelectedWorklistItemCommand { get; private set; } = null!;
        public ICommand MoveWorklistItemUpCommand { get; private set; } = null!;
        public ICommand MoveWorklistItemDownCommand { get; private set; } = null!;
        public ICommand RemoveSelectedWorklistItemsCommand { get; private set; } = null!;

        public ObservableCollection<WorklistDefinition> WorklistBrowserLists => worklistBrowserLists;
        public ObservableCollection<WorklistBrowserEntry> WorklistBrowserEntries => worklistBrowserEntries;
        public ICollectionView WorklistBrowserView => worklistBrowserView;
        public IReadOnlyList<string> WorklistGroupModeOptions => WorklistGroupModes;
        public IReadOnlyCollection<WorklistBrowserEntry> SelectedWorklistBrowserEntries => selectedWorklistBrowserEntries;
        public bool WorklistBrowserHasLists => worklistBrowserLists.Count > 0;
        public bool WorklistBrowserHasItems => worklistBrowserEntries.Count > 0;
        public string WorklistSelectionSummary => selectedWorklistBrowserEntries.Count == 0
            ? "未选择条目"
            : $"已选择 {selectedWorklistBrowserEntries.Count} 项";
        public string WorklistLookupSummary
        {
            get => worklistLookupSummary;
            private set
            {
                if (string.Equals(worklistLookupSummary, value, StringComparison.Ordinal)) return;
                worklistLookupSummary = value ?? string.Empty;
                OnPropertyChanged(nameof(WorklistLookupSummary));
            }
        }

        public bool IsWorklistBrowserOpen
        {
            get => isWorklistBrowserOpen;
            private set
            {
                if (isWorklistBrowserOpen == value) return;
                isWorklistBrowserOpen = value;
                OnPropertyChanged(nameof(IsWorklistBrowserOpen));
                if (value) Observe(RefreshWorklistReferencesAsync());
            }
        }

        public WorklistDefinition? SelectedWorklist
        {
            get => selectedWorklist;
            set
            {
                if (ReferenceEquals(selectedWorklist, value)) return;
                selectedWorklist = value;
                OnPropertyChanged(nameof(SelectedWorklist));
                OnPropertyChanged(nameof(WorklistBrowserHasLists));
                RebuildWorklistBrowserEntries();
                PersistActiveWorklistSelection(value);
                if (IsWorklistBrowserOpen) Observe(RefreshWorklistReferencesAsync());
                RaiseCommandStates();
            }
        }

        public WorklistBrowserEntry? SelectedWorklistBrowserEntry
        {
            get => selectedWorklistBrowserEntry;
            set
            {
                if (ReferenceEquals(selectedWorklistBrowserEntry, value)) return;
                selectedWorklistBrowserEntry = value;
                OnPropertyChanged(nameof(SelectedWorklistBrowserEntry));
                RaiseCommandStates();
            }
        }

        public string WorklistSearchText
        {
            get => worklistSearchText;
            set
            {
                var normalized = (value ?? string.Empty).Trim();
                if (normalized.Length > 200) normalized = normalized.Substring(0, 200);
                if (string.Equals(worklistSearchText, normalized, StringComparison.Ordinal)) return;
                worklistSearchText = normalized;
                OnPropertyChanged(nameof(WorklistSearchText));
                worklistBrowserView?.Refresh();
                RaiseCommandStates();
            }
        }

        public string WorklistGroupMode
        {
            get => worklistGroupMode;
            set
            {
                var normalized = WorklistGroupModes.Contains(value) ? value : WorklistManualGrouping;
                if (string.Equals(worklistGroupMode, normalized, StringComparison.Ordinal)) return;
                worklistGroupMode = normalized;
                foreach (var entry in worklistBrowserEntries) entry.SetGroupMode(normalized);
                if (worklistBrowserView != null)
                {
                    worklistBrowserView.GroupDescriptions.Clear();
                    if (normalized != WorklistManualGrouping)
                        worklistBrowserView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(WorklistBrowserEntry.GroupKey)));
                    worklistBrowserView.Refresh();
                }
                OnPropertyChanged(nameof(WorklistGroupMode));
                RaiseCommandStates();
            }
        }

        private void InitializeWorklistBrowser()
        {
            worklistBrowserView = CollectionViewSource.GetDefaultView(worklistBrowserEntries);
            worklistBrowserView.Filter = FilterWorklistBrowserEntry;
            worklistBrowserView.SortDescriptions.Add(new SortDescription(nameof(WorklistBrowserEntry.Position), ListSortDirection.Ascending));

            OpenWorklistBrowserCommand = new RelayCommand(_ => IsWorklistBrowserOpen = true);
            CloseWorklistBrowserCommand = new RelayCommand(_ => IsWorklistBrowserOpen = false);
            RefreshWorklistReferencesCommand = new RelayCommand(
                _ => Observe(RefreshWorklistReferencesAsync()),
                _ => selectedWorklist != null && !isResolvingWorklistReferences);
            ContinueSelectedWorklistItemCommand = new RelayCommand(
                _ => Observe(ContinueSelectedWorklistItemAsync()),
                _ => !IsBusy && !isMutatingWorklist && !isResolvingWorklistReferences
                    && selectedWorklistBrowserEntries.Count == 1
                    && selectedWorklistBrowserEntries[0].CanContinue);
            MoveWorklistItemUpCommand = new RelayCommand(_ => MoveSelectedWorklistItem(-1), _ => CanMoveSelectedWorklistItem(-1));
            MoveWorklistItemDownCommand = new RelayCommand(_ => MoveSelectedWorklistItem(1), _ => CanMoveSelectedWorklistItem(1));
            RemoveSelectedWorklistItemsCommand = new RelayCommand(
                _ => Observe(RemoveSelectedWorklistItemsAsync()),
                _ => selectedWorklist != null && selectedWorklistBrowserEntries.Count > 0 && !isMutatingWorklist);

            RefreshWorklistBrowserFromSettings();
        }

        public void SetWorklistBrowserSelection(IEnumerable<WorklistBrowserEntry> entries)
        {
            selectedWorklistBrowserEntries.Clear();
            selectedWorklistBrowserEntries.AddRange((entries ?? Enumerable.Empty<WorklistBrowserEntry>())
                .Where(entry => entry != null)
                .Distinct());
            SelectedWorklistBrowserEntry = selectedWorklistBrowserEntries.LastOrDefault();
            OnPropertyChanged(nameof(SelectedWorklistBrowserEntries));
            OnPropertyChanged(nameof(WorklistSelectionSummary));
            RaiseCommandStates();
        }

        private bool FilterWorklistBrowserEntry(object value)
        {
            if (!(value is WorklistBrowserEntry entry)) return false;
            var query = worklistSearchText;
            if (query.Length == 0) return true;
            return ContainsWorklistSearchValue(entry.KindDisplay, query)
                || ContainsWorklistSearchValue(entry.StableObjectId, query)
                || ContainsWorklistSearchValue(entry.Note, query)
                || ContainsWorklistSearchValue(entry.StateDisplay, query)
                || ContainsWorklistSearchValue(entry.IsManuallyComplete ? "已完成" : "未完成", query);
        }

        private static bool ContainsWorklistSearchValue(string value, string query)
            => !string.IsNullOrEmpty(value) && value.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0;

        private void RefreshWorklistBrowserFromSettings(string? selectedIdentity = null)
        {
            var preferredListId = selectedWorklist?.Id;
            var preferredEntryIdentity = selectedIdentity ?? (selectedWorklistBrowserEntry == null
                ? null
                : GetWorklistIdentity(selectedWorklistBrowserEntry.Kind, selectedWorklistBrowserEntry.StableObjectId));
            var settings = plugin.Settings;
            var lists = WorklistDefinition.NormalizeMany(settings.Worklists);
            worklistBrowserLists.Clear();
            foreach (var list in lists) worklistBrowserLists.Add(list);

            var activeListId = Guid.TryParse(settings.ActiveWorklistId, out var parsedActiveId) ? parsedActiveId : Guid.Empty;
            selectedWorklist = worklistBrowserLists.FirstOrDefault(list => list.Id == preferredListId)
                ?? worklistBrowserLists.FirstOrDefault(list => list.Id == activeListId)
                ?? worklistBrowserLists.FirstOrDefault();
            OnPropertyChanged(nameof(SelectedWorklist));
            OnPropertyChanged(nameof(WorklistBrowserHasLists));
            RebuildWorklistBrowserEntries(preferredEntryIdentity);
        }

        private void RebuildWorklistBrowserEntries(string? selectedIdentity = null)
        {
            worklistResolutionGeneration++;
            worklistBrowserEntries.Clear();
            selectedWorklistBrowserEntries.Clear();
            var items = selectedWorklist?.Items ?? new List<WorklistItemRecord>();
            for (var index = 0; index < items.Count; index++)
            {
                var entry = new WorklistBrowserEntry(items[index], index);
                entry.SetGroupMode(worklistGroupMode);
                worklistBrowserEntries.Add(entry);
                if (selectedIdentity != null
                    && string.Equals(GetWorklistIdentity(entry.Kind, entry.StableObjectId), selectedIdentity, StringComparison.OrdinalIgnoreCase))
                    selectedWorklistBrowserEntry = entry;
            }
            if (selectedIdentity == null) selectedWorklistBrowserEntry = null;
            if (selectedWorklistBrowserEntry != null) selectedWorklistBrowserEntries.Add(selectedWorklistBrowserEntry);
            if (worklistBrowserView != null) worklistBrowserView.Refresh();
            OnPropertyChanged(nameof(SelectedWorklistBrowserEntry));
            OnPropertyChanged(nameof(SelectedWorklistBrowserEntries));
            OnPropertyChanged(nameof(WorklistBrowserHasItems));
            OnPropertyChanged(nameof(WorklistSelectionSummary));
            UpdateWorklistLookupSummary();
            RaiseCommandStates();
        }

        private async Task RefreshWorklistReferencesAsync()
        {
            if (isResolvingWorklistReferences || selectedWorklist == null) return;
            var entries = worklistBrowserEntries.ToArray();
            if (entries.Length == 0)
            {
                WorklistLookupSummary = "当前清单没有条目。";
                return;
            }

            isResolvingWorklistReferences = true;
            var generation = ++worklistResolutionGeneration;
            var listId = selectedWorklist.Id;
            RaiseCommandStates();
            try
            {
                var response = await plugin.RequestAsync<WorklistReferenceQueryResultDto>(
                    MessageTypes.ResolveWorklistReferences,
                    new WorklistReferenceQueryDto
                    {
                        References = entries.Select(entry => new WorklistReferenceQueryItemDto
                        {
                            Kind = (int)entry.Kind,
                            StableObjectId = entry.StableObjectId
                        }).ToList()
                    },
                    TimeSpan.FromSeconds(20));
                if (generation != worklistResolutionGeneration || selectedWorklist?.Id != listId) return;

                var resultLookup = (response?.References ?? new List<WorklistReferenceQueryResultItemDto>())
                    .GroupBy(item => GetWorklistIdentity((WorklistObjectKind)item.Kind, item.StableObjectId), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
                foreach (var entry in entries)
                {
                    var identity = GetWorklistIdentity(entry.Kind, entry.StableObjectId);
                    entry.SetResolution(resultLookup.TryGetValue(identity, out var resolution)
                        ? resolution.State
                        : WorklistReferenceState.Unavailable,
                        resolution?.PlayniteId);
                }
                UpdateWorklistLookupSummary();
                worklistBrowserView.Refresh();
            }
            catch (Exception exception)
            {
                if (generation != worklistResolutionGeneration || selectedWorklist?.Id != listId) return;
                Logger.Error(exception, "GameSaveCenter could not resolve saved worklist references.");
                foreach (var entry in entries) entry.SetState(WorklistReferenceState.Unavailable);
                WorklistLookupSummary = "Worker 未能检查这些引用；状态未知，可稍后重试。";
                worklistBrowserView.Refresh();
            }
            finally
            {
                isResolvingWorklistReferences = false;
                if (generation == worklistResolutionGeneration)
                    RaiseCommandStates();
                else if (IsWorklistBrowserOpen)
                    Observe(RefreshWorklistReferencesAsync());
            }
        }

        private async Task ContinueSelectedWorklistItemAsync()
        {
            if (selectedWorklist == null || selectedWorklistBrowserEntries.Count != 1 || isMutatingWorklist)
                return;
            var entry = selectedWorklistBrowserEntries[0];
            if (!WorklistContinuationResolver.TryResolve(entry, out var target, out var message))
            {
                StatusMessage = message;
                return;
            }

            var listName = selectedWorklist.Name;
            isMutatingWorklist = true;
            RaiseCommandStates();
            try
            {
                switch (target!.Kind)
                {
                    case WorklistObjectKind.BackupVersion:
                        ContinueToWorklistBackup(target, listName);
                        break;
                    case WorklistObjectKind.MediaItem:
                        ContinueToWorklistMedia(target, listName);
                        break;
                    case WorklistObjectKind.BackgroundTask:
                        await ContinueToWorklistTaskAsync(target, entry, listName);
                        break;
                    case WorklistObjectKind.ValidationFinding:
                        await ContinueToWorklistFindingAsync(target, entry, listName);
                        break;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "GameSaveCenter could not continue from a worklist reference.");
                ApplyOnUi(() =>
                {
                    IsWorklistBrowserOpen = true;
                    StatusMessage = "无法从待办打开该对象；清单与源对象未改变，可以刷新状态或移除引用。";
                });
            }
            finally
            {
                ApplyOnUi(() =>
                {
                    isMutatingWorklist = false;
                    RaiseCommandStates();
                });
            }
        }

        private void ContinueToWorklistBackup(WorklistContinuationTarget target, string listName)
        {
            var game = TaskSourceNavigationResolver.ResolveExactGame(
                new GameSaveCenter.Contracts.TaskSourceReferenceDto
                {
                    StableId = target.PlayniteId,
                    PlayniteId = target.PlayniteId
                }, Games);
            if (game == null)
            {
                StatusMessage = $"待办存档版本的游戏 ID（{target.PlayniteId}）不在当前库中；未按名称替换目标，清单仍可移除该引用。";
                return;
            }

            PushNavigationReturnTarget("返回待办", $"来源：人工待办 · {listName} · 存档版本");
            pendingTaskBackupId = target.ObjectId;
            pendingFindingBackupId = string.Empty;
            pendingStorageBackupId = string.Empty;
            SaveTabIndex = 0;
            CurrentWorkspace = WorkspaceKind.Saves;
            SelectedBackup = null!;
            SelectedGame = game;
            StatusMessage = $"正在打开待办存档版本“{target.ObjectId}”；仅按 Playnite ID 与版本 ID 定位，不会执行恢复。";
            RequestWorkspaceLoad();
        }

        private void ContinueToWorklistMedia(WorklistContinuationTarget target, string listName)
        {
            var game = TaskSourceNavigationResolver.ResolveExactGame(
                new GameSaveCenter.Contracts.TaskSourceReferenceDto
                {
                    StableId = target.PlayniteId,
                    PlayniteId = target.PlayniteId
                }, Games);
            if (game == null)
            {
                StatusMessage = $"待办媒体所属游戏（{target.PlayniteId}）不在当前库中；未按名称替换目标，清单仍可移除该引用。";
                return;
            }

            PushNavigationReturnTarget("返回待办", $"来源：人工待办 · {listName} · 媒体项");
            pendingWorklistMediaId = target.ObjectId;
            MediaFilter = "全部";
            MediaSearchText = target.ObjectId;
            mediaSearchRefresh.Cancel();
            mediaPageQueryRefresh.Cancel();
            SelectedMedia = null!;
            CurrentWorkspace = WorkspaceKind.Media;
            SelectedGame = game;
            StatusMessage = $"正在打开待办媒体“{target.ObjectId}”；媒体页将按稳定编号筛选，不会修改文件。";
            RequestWorkspaceLoad();
        }

        private async Task ContinueToWorklistTaskAsync(WorklistContinuationTarget target, WorklistBrowserEntry entry, string listName)
        {
            var previousNavigationCount = navigationHistory.Count;
            PushNavigationReturnTarget("返回待办", $"来源：人工待办 · {listName} · 后台任务");
            PrepareExactWorklistTaskFilter(target.ObjectId);
            pendingNavigationTaskId = target.ObjectId;
            pendingNavigationTaskIndex = -1;
            restoringNavigationTaskSelection = true;
            IsWorklistBrowserOpen = false;
            CurrentWorkspace = WorkspaceKind.Tasks;
            try
            {
                await LoadTaskPageAsync(true);
            }
            catch
            {
                ApplyOnUi(() =>
                {
                    if (navigationHistory.Count > previousNavigationCount && navigationHistory.TryPop(out _))
                        OnNavigationHistoryChanged();
                    IsWorklistBrowserOpen = true;
                });
                throw;
            }
            var taskFound = false;
            ApplyOnUi(() =>
            {
                taskFound = SelectedTask != null
                    && string.Equals(SelectedTask.TaskId, target.ObjectId, StringComparison.OrdinalIgnoreCase);
                if (taskFound)
                {
                    StatusMessage = $"已从待办续接后台任务“{target.ObjectId}”。该操作只打开详情，不会重试或改变任务状态。";
                    return;
                }

                entry.SetResolution(WorklistReferenceState.NotFound, entry.TargetPlayniteId);
                WorklistLookupSummary = "该任务在打开期间已从当前索引消失；可以移除清单引用。";
                if (navigationHistory.Count > previousNavigationCount && navigationHistory.TryPop(out _))
                    OnNavigationHistoryChanged();
                IsWorklistBrowserOpen = true;
                StatusMessage = $"待办任务“{target.ObjectId}”已不存在于当前索引；已返回清单，引用仍可移除。";
            });
        }

        private void ClearPendingWorklistMediaContinuation()
        {
            var mediaId = pendingWorklistMediaId;
            if (string.IsNullOrWhiteSpace(mediaId)) return;
            pendingWorklistMediaId = null;
            StatusMessage = $"待办媒体“{mediaId}”的详情加载失败；没有选择其他媒体，也没有修改文件。可返回清单重试或移除引用。";
        }

        private async Task ContinueToWorklistFindingAsync(WorklistContinuationTarget target, WorklistBrowserEntry entry, string listName)
        {
            var findingResult = await plugin.RequestAsync<GameSaveCenter.Contracts.ValidationFindingQueryResultDto>(
                GameSaveCenter.Contracts.MessageTypes.GetValidationFinding,
                new GameSaveCenter.Contracts.ValidationFindingQueryDto { FindingId = target.ObjectId },
                TimeSpan.FromSeconds(20));
            var finding = findingResult?.Finding;
            if (finding == null || !string.Equals(finding.FindingId, target.ObjectId, StringComparison.OrdinalIgnoreCase))
            {
                ApplyOnUi(() =>
                {
                    entry.SetResolution(WorklistReferenceState.NotFound, entry.TargetPlayniteId);
                    WorklistLookupSummary = "该校验问题已从当前索引消失；可以移除清单引用。";
                    StatusMessage = $"待办校验问题“{target.ObjectId}”已不存在于当前索引；清单与其他条目仍可使用。";
                });
                return;
            }

            var gameName = string.Empty;
            ApplyOnUi(() =>
            {
                var game = Games.FirstOrDefault(candidate =>
                    string.Equals(candidate.PlayniteId, finding.PlayniteId, StringComparison.OrdinalIgnoreCase));
                gameName = game?.Name ?? (string.IsNullOrWhiteSpace(finding.PlayniteId) ? "全局" : $"原游戏已不在库中（{finding.PlayniteId}）");
                PushNavigationReturnTarget("返回待办", $"来源：人工待办 · {listName} · 校验问题");
                CurrentWorkspace = WorkspaceKind.Maintenance;
                MaintenanceTabIndex = 0;
            });
            finding.GameName = gameName;
            await LoadDiagnosticsAsync();
            ApplyOnUi(() =>
            {
                var current = Findings.FirstOrDefault(candidate =>
                    string.Equals(candidate.FindingId, target.ObjectId, StringComparison.OrdinalIgnoreCase));
                if (current == null)
                {
                    Findings.Add(finding);
                    current = finding;
                }
                SelectedFinding = current;
                StatusMessage = $"已从待办续接校验问题“{target.ObjectId}”；只显示诊断，不会自动执行建议操作。";
            });
        }

        private void PrepareExactWorklistTaskFilter(string taskId)
        {
            taskSearchRefresh.Cancel();
            taskHistoryQueryRefresh.Cancel();
            taskHistoryQueryQueued = false;
            taskSearchText = taskId;
            taskStatusFilter = "全部";
            taskGameFilter = "全部";
            taskTypeFilter = "全部";
            pendingTaskGameFilter = "全部";
            pendingTaskTypeFilter = "全部";
            pendingTaskDynamicFilterRestore = false;
            taskHistoryScope = "全部历史";
            taskHistoryRange = "全部时间";
            taskHistoryActive = true;
            taskNavigationGameId = string.Empty;
            taskNavigationGameName = string.Empty;
            OnPropertyChanged(nameof(TaskSearchText));
            OnPropertyChanged(nameof(TaskStatusFilter));
            OnPropertyChanged(nameof(TaskGameFilter));
            OnPropertyChanged(nameof(TaskTypeFilter));
            OnPropertyChanged(nameof(TaskHistoryScope));
            OnPropertyChanged(nameof(TaskHistoryRange));
            OnPropertyChanged(nameof(HasTaskNavigationTarget));
            OnPropertyChanged(nameof(TaskNavigationSourceSummary));
            OnPropertyChanged(nameof(TaskLoadedSummary));
            OnPropertyChanged(nameof(TaskHasActiveFilters));
            OnPropertyChanged(nameof(TaskActiveFiltersSummary));
            TasksView.Refresh();
            uiStateSave?.Schedule();
        }

        private void UpdateWorklistLookupSummary()
        {
            var total = worklistBrowserEntries.Count;
            if (total == 0)
            {
                WorklistLookupSummary = "当前清单没有条目。";
                return;
            }
            var indexed = worklistBrowserEntries.Count(entry => entry.State == WorklistReferenceState.Indexed);
            var resolved = worklistBrowserEntries.Count(entry => entry.State == WorklistReferenceState.Resolved);
            var missing = worklistBrowserEntries.Count(entry => entry.State == WorklistReferenceState.NotFound);
            var invalid = worklistBrowserEntries.Count(entry => entry.State == WorklistReferenceState.InvalidReference);
            var unknown = total - indexed - resolved - missing - invalid;
            WorklistLookupSummary = $"共 {total} 项 · 有效 {indexed} · 已解决 {resolved} · 失效 {missing} · 无效 {invalid} · 待检查 {unknown}";
        }

        private bool CanMoveSelectedWorklistItem(int direction)
        {
            if (isMutatingWorklist || isResolvingWorklistReferences || selectedWorklist == null
                || selectedWorklistBrowserEntries.Count != 1
                || worklistGroupMode != WorklistManualGrouping
                || worklistSearchText.Length > 0)
                return false;
            var selected = selectedWorklistBrowserEntries[0];
            var index = selectedWorklist.Items.FindIndex(item => item.Kind == selected.Kind
                && string.Equals(item.StableObjectId, selected.StableObjectId, StringComparison.OrdinalIgnoreCase));
            return index >= 0 && index + direction >= 0 && index + direction < selectedWorklist.Items.Count;
        }

        private void MoveSelectedWorklistItem(int direction)
        {
            if (!CanMoveSelectedWorklistItem(direction) || selectedWorklist == null) return;
            var selected = selectedWorklistBrowserEntries[0];
            var identity = GetWorklistIdentity(selected.Kind, selected.StableObjectId);
            var items = selectedWorklist.Items.Select(item => item.Clone()).ToList();
            var index = items.FindIndex(item => item.Kind == selected.Kind
                && string.Equals(item.StableObjectId, selected.StableObjectId, StringComparison.OrdinalIgnoreCase));
            if (index < 0 || index + direction < 0 || index + direction >= items.Count) return;
            var moved = items[index];
            items.RemoveAt(index);
            items.Insert(index + direction, moved);
            if (PersistWorklistItems(items, identity))
                StatusMessage = direction < 0 ? "已将待办上移；源对象未改变。" : "已将待办下移；源对象未改变。";
        }

        private async Task RemoveSelectedWorklistItemsAsync()
        {
            if (selectedWorklist == null || selectedWorklistBrowserEntries.Count == 0 || isMutatingWorklist) return;
            var selectedReferences = new HashSet<string>(selectedWorklistBrowserEntries
                .Select(entry => GetWorklistIdentity(entry.Kind, entry.StableObjectId)), StringComparer.OrdinalIgnoreCase);
            var listId = selectedWorklist.Id;
            var listName = selectedWorklist.Name;
            var selectedCount = selectedReferences.Count;
            isMutatingWorklist = true;
            RaiseCommandStates();
            try
            {
                var confirmed = await plugin.ConfirmAsync(
                    "从待办清单移除条目",
                    $"范围：仅从“{listName}”移除当前选中的 {selectedCount} 条引用。原存档、媒体、任务和校验记录都不会删除或修改。",
                    "移除所选",
                    "返回",
                    isDangerous: true);
                if (!confirmed)
                {
                    StatusMessage = "已取消移除；清单和源对象均未改变。";
                    return;
                }

                var currentLists = WorklistDefinition.NormalizeMany(plugin.Settings.Worklists);
                if (selectedWorklist?.Id != listId)
                {
                    StatusMessage = "确认期间切换了清单；没有移除条目，请重新选择后重试。";
                    return;
                }

                var current = currentLists.FirstOrDefault(list => list.Id == listId);
                if (current == null || selectedReferences.Any(identity => !current.Items.Any(item =>
                        string.Equals(GetWorklistIdentity(item.Kind, item.StableObjectId), identity, StringComparison.OrdinalIgnoreCase))))
                {
                    StatusMessage = "清单在确认期间已变化；没有移除条目，请重新选择后重试。";
                    return;
                }
                var remaining = current.Items
                    .Where(item => !selectedReferences.Contains(GetWorklistIdentity(item.Kind, item.StableObjectId)))
                    .Select(item => item.Clone())
                    .ToList();
                if (PersistWorklistItems(remaining, null))
                    StatusMessage = $"已从“{listName}”移除 {selectedCount} 条引用；源对象未改变。";
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "GameSaveCenter could not remove selected worklist references.");
                StatusMessage = "移除待办失败；清单和源对象未改变，请稍后重试。";
            }
            finally
            {
                isMutatingWorklist = false;
                RaiseCommandStates();
            }
        }

        private bool PersistWorklistItems(List<WorklistItemRecord> items, string? selectedIdentity)
        {
            if (selectedWorklist == null) return false;
            var settings = plugin.Settings;
            var previousLists = WorklistDefinition.NormalizeMany(settings.Worklists);
            var previousActiveId = settings.ActiveWorklistId;
            var updatedLists = previousLists.Select(list =>
            {
                if (list.Id != selectedWorklist.Id) return list;
                var updated = list.Clone();
                updated.Items = items.Select(item => item.Clone()).ToList();
                return updated;
            }).ToList();
            if (!updatedLists.Any(list => list.Id == selectedWorklist.Id)) return false;

            settings.Worklists = updatedLists;
            settings.ActiveWorklistId = selectedWorklist.Id.ToString("D");
            try
            {
                plugin.SavePluginSettings(settings);
            }
            catch (Exception exception)
            {
                settings.Worklists = previousLists;
                settings.ActiveWorklistId = previousActiveId;
                Logger.Error(exception, "GameSaveCenter could not persist worklist ordering or removal.");
                StatusMessage = "清单修改无法保存；已恢复原有条目。";
                return false;
            }

            RefreshWorklistBrowserFromSettings(selectedIdentity);
            return true;
        }

        private void PersistActiveWorklistSelection(WorklistDefinition? value)
        {
            if (plugin?.Settings == null) return;
            var settings = plugin.Settings;
            var previousId = settings.ActiveWorklistId;
            var newId = value?.Id.ToString("D") ?? string.Empty;
            if (string.Equals(previousId, newId, StringComparison.OrdinalIgnoreCase)) return;
            settings.ActiveWorklistId = newId;
            try
            {
                plugin.SavePluginSettings(settings);
            }
            catch (Exception exception)
            {
                settings.ActiveWorklistId = previousId;
                Logger.Error(exception, "GameSaveCenter could not save the active worklist selection.");
                StatusMessage = "当前待办清单无法保存；已恢复先前选择。";
                RefreshWorklistBrowserFromSettings();
            }
        }

        private static string GetWorklistIdentity(WorklistObjectKind kind, string? stableObjectId)
            => ((int)kind).ToString(System.Globalization.CultureInfo.InvariantCulture) + ":" + (stableObjectId ?? string.Empty).Trim();
    }
}
