using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GameSaveCenter.Playnite.Settings;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed partial class DashboardViewModel
    {
        private FilterPresetDefinition? selectedTaskFilterPreset;
        private FilterPresetDefinition? selectedMediaFilterPreset;
        private string taskFilterPresetNameDraft = string.Empty;
        private string mediaFilterPresetNameDraft = string.Empty;

        public ObservableCollection<FilterPresetDefinition> TaskFilterPresets { get; } = new ObservableCollection<FilterPresetDefinition>();
        public ObservableCollection<FilterPresetDefinition> MediaFilterPresets { get; } = new ObservableCollection<FilterPresetDefinition>();

        public FilterPresetDefinition? SelectedTaskFilterPreset
        {
            get => selectedTaskFilterPreset;
            set
            {
                SetValue(ref selectedTaskFilterPreset, value);
                if (value != null)
                    TaskFilterPresetNameDraft = value.Name;
                RaiseCommandStates();
            }
        }

        public FilterPresetDefinition? SelectedMediaFilterPreset
        {
            get => selectedMediaFilterPreset;
            set
            {
                SetValue(ref selectedMediaFilterPreset, value);
                if (value != null)
                    MediaFilterPresetNameDraft = value.Name;
                RaiseCommandStates();
            }
        }

        public string TaskFilterPresetNameDraft
        {
            get => taskFilterPresetNameDraft;
            set
            {
                SetValue(ref taskFilterPresetNameDraft, value ?? string.Empty);
                RaiseCommandStates();
            }
        }

        public string MediaFilterPresetNameDraft
        {
            get => mediaFilterPresetNameDraft;
            set
            {
                SetValue(ref mediaFilterPresetNameDraft, value ?? string.Empty);
                RaiseCommandStates();
            }
        }

        internal void InitializeFilterPresets()
        {
            TaskFilterPresets.Clear();
            MediaFilterPresets.Clear();
            foreach (var preset in plugin.Settings.FilterPresets ?? new List<FilterPresetDefinition>())
            {
                var normalized = FilterPresetDefinition.Normalize(preset);
                if (normalized == null) continue;
                if (normalized.Workspace == FilterPresetDefinition.TasksWorkspace)
                    TaskFilterPresets.Add(normalized);
                else if (normalized.Workspace == FilterPresetDefinition.MediaWorkspace)
                    MediaFilterPresets.Add(normalized);
            }
        }

        private FilterPresetDefinition CreateTaskFilterPreset(string name)
        {
            return new FilterPresetDefinition
            {
                Name = name,
                Workspace = FilterPresetDefinition.TasksWorkspace,
                TaskSearchText = TaskSearchText,
                TaskStatusFilter = TaskStatusFilter,
                TaskGameFilter = TaskGameFilter,
                TaskTypeFilter = TaskTypeFilter,
                TaskHistoryScope = TaskHistoryScope,
                TaskHistoryRange = TaskHistoryRange
            };
        }

        private FilterPresetDefinition CreateMediaFilterPreset(string name)
        {
            return new FilterPresetDefinition
            {
                Name = name,
                Workspace = FilterPresetDefinition.MediaWorkspace,
                MediaSearchText = MediaSearchText,
                MediaFilter = MediaFilter,
                MediaInboxMode = MediaInboxMode
            };
        }

        private void ApplyTaskFilterPreset()
        {
            var preset = SelectedTaskFilterPreset;
            if (preset == null) return;

            TaskSearchText = preset.TaskSearchText;
            TaskStatusFilter = TaskStatusFilterOptions.Contains(preset.TaskStatusFilter) ? preset.TaskStatusFilter : "全部";
            TaskGameFilter = TaskGameFilterOptions.Contains(preset.TaskGameFilter) ? preset.TaskGameFilter : "全部";
            TaskTypeFilter = TaskTypeFilterOptions.Contains(preset.TaskTypeFilter) ? preset.TaskTypeFilter : "全部";
            TaskHistoryScope = preset.TaskHistoryScope;
            TaskHistoryRange = preset.TaskHistoryRange;
            StatusMessage = $"已应用任务筛选预设“{preset.Name}”";
        }

        private void ApplyMediaFilterPreset()
        {
            var preset = SelectedMediaFilterPreset;
            if (preset == null) return;

            MediaInboxMode = preset.MediaInboxMode;
            MediaSearchText = preset.MediaSearchText;
            MediaFilter = MediaFilterOptions.Contains(preset.MediaFilter) ? preset.MediaFilter : "全部";
            StatusMessage = $"已应用媒体筛选预设“{preset.Name}”";
        }

        private async Task SaveTaskFilterPresetAsync()
        {
            try
            {
                var name = NormalizePresetName(TaskFilterPresetNameDraft);
                if (name.Length == 0)
                {
                    StatusMessage = "请输入任务筛选预设名称。";
                    return;
                }

                var snapshot = FilterPresetDefinition.Normalize(CreateTaskFilterPreset(name));
                if (snapshot == null) throw new InvalidOperationException("任务筛选预设内容无效。");
                var existing = TaskFilterPresets.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    if (!await plugin.ConfirmAsync("覆盖筛选预设", $"已存在同名任务筛选预设“{name}”。是否覆盖？", "覆盖", "取消").ConfigureAwait(true))
                        return;
                    snapshot.Id = existing.Id;
                    ReplacePreset(TaskFilterPresets, existing, snapshot);
                }
                else
                {
                    TaskFilterPresets.Add(snapshot);
                }

                SelectedTaskFilterPreset = snapshot;
                TaskFilterPresetNameDraft = snapshot.Name;
                PersistFilterPresets();
                ConfirmSuccess($"已保存任务筛选预设“{snapshot.Name}”");
            }
            catch (Exception error)
            {
                ReportDashboardFailure(error, true);
            }
        }

        private async Task SaveMediaFilterPresetAsync()
        {
            try
            {
                var name = NormalizePresetName(MediaFilterPresetNameDraft);
                if (name.Length == 0)
                {
                    StatusMessage = "请输入媒体筛选预设名称。";
                    return;
                }

                var snapshot = FilterPresetDefinition.Normalize(CreateMediaFilterPreset(name));
                if (snapshot == null) throw new InvalidOperationException("媒体筛选预设内容无效。");
                var existing = MediaFilterPresets.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    if (!await plugin.ConfirmAsync("覆盖筛选预设", $"已存在同名媒体筛选预设“{name}”。是否覆盖？", "覆盖", "取消").ConfigureAwait(true))
                        return;
                    snapshot.Id = existing.Id;
                    ReplacePreset(MediaFilterPresets, existing, snapshot);
                }
                else
                {
                    MediaFilterPresets.Add(snapshot);
                }

                SelectedMediaFilterPreset = snapshot;
                MediaFilterPresetNameDraft = snapshot.Name;
                PersistFilterPresets();
                ConfirmSuccess($"已保存媒体筛选预设“{snapshot.Name}”");
            }
            catch (Exception error)
            {
                ReportDashboardFailure(error, true);
            }
        }

        private async Task RenameTaskFilterPresetAsync()
        {
            await RenameFilterPresetAsync(TaskFilterPresets, SelectedTaskFilterPreset, TaskFilterPresetNameDraft, value => SelectedTaskFilterPreset = value, "任务").ConfigureAwait(true);
        }

        private async Task RenameMediaFilterPresetAsync()
        {
            await RenameFilterPresetAsync(MediaFilterPresets, SelectedMediaFilterPreset, MediaFilterPresetNameDraft, value => SelectedMediaFilterPreset = value, "媒体").ConfigureAwait(true);
        }

        private async Task RenameFilterPresetAsync(
            ObservableCollection<FilterPresetDefinition> presets,
            FilterPresetDefinition? selected,
            string draft,
            Action<FilterPresetDefinition> setSelected,
            string workspaceName)
        {
            try
            {
                if (selected == null) return;
                var name = NormalizePresetName(draft);
                if (name.Length == 0)
                {
                    StatusMessage = $"请输入{workspaceName}筛选预设名称。";
                    return;
                }
                if (string.Equals(name, selected.Name, StringComparison.Ordinal)) return;
                if (presets.Any(x => !ReferenceEquals(x, selected) && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)))
                {
                    StatusMessage = $"已有同名{workspaceName}筛选预设，请使用其他名称。";
                    return;
                }
                if (!await plugin.ConfirmAsync("重命名筛选预设", $"是否将“{selected.Name}”重命名为“{name}”？", "重命名", "取消").ConfigureAwait(true))
                    return;

                var renamed = selected.Clone();
                renamed.Name = name;
                ReplacePreset(presets, selected, renamed);
                setSelected(renamed);
                PersistFilterPresets();
                ConfirmSuccess($"已重命名{workspaceName}筛选预设为“{name}”");
            }
            catch (Exception error)
            {
                ReportDashboardFailure(error, true);
            }
        }

        private async Task DeleteTaskFilterPresetAsync()
        {
            await DeleteFilterPresetAsync(TaskFilterPresets, SelectedTaskFilterPreset, value => SelectedTaskFilterPreset = value, "任务").ConfigureAwait(true);
        }

        private async Task DeleteMediaFilterPresetAsync()
        {
            await DeleteFilterPresetAsync(MediaFilterPresets, SelectedMediaFilterPreset, value => SelectedMediaFilterPreset = value, "媒体").ConfigureAwait(true);
        }

        private async Task DeleteFilterPresetAsync(
            ObservableCollection<FilterPresetDefinition> presets,
            FilterPresetDefinition? selected,
            Action<FilterPresetDefinition?> setSelected,
            string workspaceName)
        {
            try
            {
                if (selected == null) return;
                if (!await plugin.ConfirmAsync("删除筛选预设", $"是否删除{workspaceName}筛选预设“{selected.Name}”？此操作不可自动恢复。", "删除", "取消", true).ConfigureAwait(true))
                    return;

                var index = presets.IndexOf(selected);
                presets.Remove(selected);
                setSelected(index >= 0 && index < presets.Count ? presets[index] : null);
                PersistFilterPresets();
                ConfirmSuccess($"已删除{workspaceName}筛选预设“{selected.Name}”");
            }
            catch (Exception error)
            {
                ReportDashboardFailure(error, true);
            }
        }

        private void PersistFilterPresets()
        {
            plugin.Settings.FilterPresets = TaskFilterPresets.Concat(MediaFilterPresets).Select(x => x.Clone()).ToList();
            plugin.SavePluginSettings(plugin.Settings);
        }

        private static void ReplacePreset(ObservableCollection<FilterPresetDefinition> presets, FilterPresetDefinition oldValue, FilterPresetDefinition newValue)
        {
            var index = presets.IndexOf(oldValue);
            if (index >= 0) presets[index] = newValue;
        }

        private static string NormalizePresetName(string? value)
        {
            var normalized = (value ?? string.Empty).Trim();
            return normalized.Length <= 48 ? normalized : normalized.Substring(0, 48);
        }
    }
}
