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
                    .ToDictionary(group => group.Key, group => group.First().State, StringComparer.OrdinalIgnoreCase);
                foreach (var entry in entries)
                {
                    var identity = GetWorklistIdentity(entry.Kind, entry.StableObjectId);
                    entry.SetState(resultLookup.TryGetValue(identity, out var state)
                        && Enum.IsDefined(typeof(WorklistReferenceState), state)
                        ? state
                        : WorklistReferenceState.Unavailable);
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
