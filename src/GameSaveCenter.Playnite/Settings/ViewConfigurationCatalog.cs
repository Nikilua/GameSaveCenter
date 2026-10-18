using System;
using System.Collections.Generic;
using System.Linq;
using GameSaveCenter.Playnite.Infrastructure;

namespace GameSaveCenter.Playnite.Settings
{
    internal enum ViewConfigurationMutationResult
    {
        Success,
        InvalidName,
        DuplicateName,
        LimitReached,
        MissingConfiguration
    }

    /// <summary>Validates workspace-scoped named view mutations against durable settings.</summary>
    internal static class ViewConfigurationCatalog
    {
        internal const int MaximumNameLength = 48;
        internal const int MaximumConfigurations = 32;

        internal static IReadOnlyList<ViewConfigurationDefinition> GetForWorkspace(
            GameSaveCenterSettings settings,
            string workspaceKey)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrWhiteSpace(workspaceKey)) return Array.Empty<ViewConfigurationDefinition>();
            return settings.ViewConfigurations
                .Where(item => string.Equals(item.WorkspaceKey, workspaceKey, StringComparison.Ordinal))
                .Select(item => item.Clone())
                .ToArray();
        }

        internal static ViewConfigurationMutationResult SaveCurrent(
            GameSaveCenterSettings settings,
            string workspaceKey,
            string? requestedName,
            ViewConfigurationDefinition currentState,
            string? selectedId,
            bool saveCopy,
            out ViewConfigurationDefinition? saved)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (currentState == null) throw new ArgumentNullException(nameof(currentState));
            saved = null;
            var existing = string.IsNullOrWhiteSpace(selectedId)
                ? null
                : settings.ViewConfigurations.FirstOrDefault(item =>
                    item.Id == selectedId && item.WorkspaceKey == workspaceKey);
            if (!saveCopy && !string.IsNullOrWhiteSpace(selectedId) && existing == null)
                return ViewConfigurationMutationResult.MissingConfiguration;

            var isNew = saveCopy || existing == null;
            var name = isNew ? NormalizeName(requestedName) : existing!.Name;
            if (name.Length == 0) return ViewConfigurationMutationResult.InvalidName;
            if (settings.ViewConfigurations.Any(item =>
                    item.WorkspaceKey == workspaceKey
                    && (isNew || !ReferenceEquals(item, existing))
                    && string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)))
                return ViewConfigurationMutationResult.DuplicateName;
            if (isNew && settings.ViewConfigurations.Count >= MaximumConfigurations)
                return ViewConfigurationMutationResult.LimitReached;

            var snapshot = currentState.Clone();
            snapshot.SchemaVersion = ViewConfigurationDefinition.CurrentSchemaVersion;
            snapshot.Id = isNew ? Guid.NewGuid().ToString("N") : existing!.Id;
            snapshot.Name = name;
            snapshot.WorkspaceKey = workspaceKey;
            var normalized = ViewConfigurationDefinition.Normalize(snapshot);
            if (normalized == null) return ViewConfigurationMutationResult.InvalidName;

            var configurations = settings.ViewConfigurations.Select(item => item.Clone()).ToList();
            if (isNew)
                configurations.Add(normalized);
            else
            {
                var index = configurations.FindIndex(item => item.Id == existing!.Id);
                if (index < 0) return ViewConfigurationMutationResult.MissingConfiguration;
                configurations[index] = normalized;
            }
            settings.ViewConfigurations = configurations;
            saved = normalized.Clone();
            return ViewConfigurationMutationResult.Success;
        }

        internal static ViewConfigurationMutationResult Rename(
            GameSaveCenterSettings settings,
            string workspaceKey,
            string? selectedId,
            string? requestedName,
            out ViewConfigurationDefinition? renamed)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            renamed = null;
            var existing = Find(settings, workspaceKey, selectedId);
            if (existing == null) return ViewConfigurationMutationResult.MissingConfiguration;
            var name = NormalizeName(requestedName);
            if (name.Length == 0) return ViewConfigurationMutationResult.InvalidName;
            if (settings.ViewConfigurations.Any(item =>
                    item.WorkspaceKey == workspaceKey
                    && item.Id != existing.Id
                    && string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)))
                return ViewConfigurationMutationResult.DuplicateName;

            var copy = existing.Clone();
            copy.Name = name;
            var configurations = settings.ViewConfigurations.Select(item => item.Id == existing.Id ? copy : item.Clone()).ToList();
            settings.ViewConfigurations = configurations;
            renamed = copy.Clone();
            return ViewConfigurationMutationResult.Success;
        }

        internal static ViewConfigurationMutationResult Delete(
            GameSaveCenterSettings settings,
            string workspaceKey,
            string? selectedId,
            out ViewConfigurationDefinition? deleted)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            deleted = Find(settings, workspaceKey, selectedId)?.Clone();
            if (deleted == null) return ViewConfigurationMutationResult.MissingConfiguration;
            var deletedId = deleted.Id;
            settings.ViewConfigurations = settings.ViewConfigurations
                .Where(item => item.Id != deletedId)
                .Select(item => item.Clone())
                .ToList();
            if (settings.GetActiveViewConfigurationId(workspaceKey) == deletedId)
                settings.SetActiveViewConfigurationId(workspaceKey, null);
            return ViewConfigurationMutationResult.Success;
        }

        internal static string BuildCurrentLayoutResetImpact(
            GameSaveCenterSettings settings,
            string workspaceKey,
            ViewConfigurationDefinition? selected)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            var workspace = GetWorkspaceDisplayName(workspaceKey);
            if (selected == null)
                return $"将仅重置“{workspace}”表格的默认列宽、显示列和排序。当前筛选值与筛选预设、其他表格及已保存视图均保留。";

            var presetName = settings.FilterPresets.FirstOrDefault(item => item.Id == selected.FilterPresetId)?.Name;
            var presetDescription = string.IsNullOrWhiteSpace(presetName)
                ? "当前筛选预设绑定"
                : $"筛选预设“{presetName}”";
            return $"将把“{workspace}”中的已保存视图“{selected.Name}”恢复为默认列宽、显示全部列、默认排序和默认表格密度。视图名称与{presetDescription}保留，其他已保存视图、当前筛选值和业务设置不变。";
        }

        internal static string BuildAllLayoutsResetImpact(GameSaveCenterSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            var viewNames = settings.ViewConfigurations
                .GroupBy(item => item.WorkspaceKey, StringComparer.Ordinal)
                .Select(group => $"{GetWorkspaceDisplayName(group.Key)}：{string.Join("、", group.Select(item => item.Name))}")
                .ToArray();
            var views = viewNames.Length == 0
                ? "没有已保存的命名视图"
                : string.Join("；", viewNames);
            return $"将删除全部 {settings.ViewConfigurations.Count} 个已保存命名视图（{views}），清除所有表格的自定义列宽/列显隐，并将默认表格密度恢复为舒适。将保留全部 {settings.FilterPresets.Count} 个筛选预设、当前筛选值、业务策略、路径、云端目标和设备身份。";
        }

        internal static ViewConfigurationMutationResult ResetConfigurationLayout(
            GameSaveCenterSettings settings,
            string workspaceKey,
            string? selectedId,
            ViewConfigurationDefinition resetLayout,
            out ViewConfigurationDefinition? reset)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (resetLayout == null) throw new ArgumentNullException(nameof(resetLayout));
            reset = null;
            var existing = Find(settings, workspaceKey, selectedId);
            if (existing == null) return ViewConfigurationMutationResult.MissingConfiguration;

            var snapshot = resetLayout.Clone();
            snapshot.Id = existing.Id;
            snapshot.Name = existing.Name;
            snapshot.WorkspaceKey = existing.WorkspaceKey;
            snapshot.FilterPresetId = existing.FilterPresetId;
            var normalized = ViewConfigurationDefinition.Normalize(snapshot);
            if (normalized == null) return ViewConfigurationMutationResult.InvalidName;

            settings.ViewConfigurations = settings.ViewConfigurations
                .Select(item => item.Id == existing.Id ? normalized : item.Clone())
                .ToList();
            reset = normalized.Clone();
            return ViewConfigurationMutationResult.Success;
        }

        internal static bool ResetAllLayouts(GameSaveCenterSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            var hasChanges = settings.ViewConfigurations.Count > 0
                || settings.ActiveViewConfigurationIds.Count > 0
                || settings.DataGridColumnWidths.Count > 0
                || settings.DataGridHiddenColumnKeys.Count > 0
                || !string.Equals(settings.DefaultTableDensityMode, TableDensityPolicy.ComfortableMode, StringComparison.Ordinal);
            if (!hasChanges) return false;

            settings.ViewConfigurations = new List<ViewConfigurationDefinition>();
            settings.ActiveViewConfigurationIds = new Dictionary<string, string>(StringComparer.Ordinal);
            settings.DataGridColumnWidths = new Dictionary<string, double>(StringComparer.Ordinal);
            settings.DataGridHiddenColumnKeys = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            settings.DefaultTableDensityMode = TableDensityPolicy.ComfortableMode;
            return true;
        }

        internal static string NormalizeName(string? value)
        {
            var name = (value ?? string.Empty).Trim();
            return name.Length <= MaximumNameLength ? name : string.Empty;
        }

        private static ViewConfigurationDefinition? Find(
            GameSaveCenterSettings settings,
            string workspaceKey,
            string? selectedId)
        {
            if (string.IsNullOrWhiteSpace(selectedId)) return null;
            return settings.ViewConfigurations.FirstOrDefault(item =>
                item.Id == selectedId && item.WorkspaceKey == workspaceKey);
        }

        private static string GetWorkspaceDisplayName(string workspaceKey)
            => workspaceKey switch
            {
                "tasks" => "任务队列",
                "save-history" => "存档历史",
                "save-candidates" => "存档候选路径",
                _ => workspaceKey
            };
    }
}
