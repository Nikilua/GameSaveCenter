using System;
using System.Collections.Generic;

namespace GameSaveCenter.Playnite.Settings
{
    /// <summary>
    /// A durable, UI-only filter combination. It deliberately contains scalar values
    /// instead of TaskStatusDto, GameStatusDto, MediaItemDto or other live objects.
    /// </summary>
    public sealed class FilterPresetDefinition
    {
        public const string TasksWorkspace = "Tasks";
        public const string MediaWorkspace = "Media";

        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = string.Empty;
        public string Workspace { get; set; } = TasksWorkspace;

        public string TaskSearchText { get; set; } = string.Empty;
        public string TaskStatusFilter { get; set; } = "全部";
        public string TaskGameFilter { get; set; } = "全部";
        public string TaskTypeFilter { get; set; } = "全部";
        public string TaskHistoryScope { get; set; } = "最近任务";
        public string TaskHistoryRange { get; set; } = "全部时间";

        public string MediaSearchText { get; set; } = string.Empty;
        public string MediaFilter { get; set; } = "全部";
        public string MediaInboxMode { get; set; } = "待归类";

        public FilterPresetDefinition Clone()
        {
            return new FilterPresetDefinition
            {
                Id = Id,
                Name = Name,
                Workspace = Workspace,
                TaskSearchText = TaskSearchText,
                TaskStatusFilter = TaskStatusFilter,
                TaskGameFilter = TaskGameFilter,
                TaskTypeFilter = TaskTypeFilter,
                TaskHistoryScope = TaskHistoryScope,
                TaskHistoryRange = TaskHistoryRange,
                MediaSearchText = MediaSearchText,
                MediaFilter = MediaFilter,
                MediaInboxMode = MediaInboxMode
            };
        }

        public static FilterPresetDefinition? Normalize(FilterPresetDefinition? source)
        {
            if (source == null) return null;

            var name = NormalizeText(source.Name, 48);
            if (string.IsNullOrWhiteSpace(name)) return null;

            var workspace = IsSupportedWorkspace(source.Workspace) ? source.Workspace : string.Empty;
            if (workspace.Length == 0) return null;

            var result = source.Clone();
            result.Id = Guid.TryParseExact(source.Id, "N", out _) ? source.Id : Guid.NewGuid().ToString("N");
            result.Name = name;
            result.Workspace = workspace;
            result.TaskSearchText = NormalizeText(source.TaskSearchText, 200);
            result.TaskStatusFilter = IsOneOf(source.TaskStatusFilter, "全部", "运行中", "等待中", "失败", "已完成")
                ? source.TaskStatusFilter
                : "全部";
            result.TaskGameFilter = NormalizeText(source.TaskGameFilter, 128);
            if (result.TaskGameFilter.Length == 0) result.TaskGameFilter = "全部";
            result.TaskTypeFilter = NormalizeText(source.TaskTypeFilter, 128);
            if (result.TaskTypeFilter.Length == 0) result.TaskTypeFilter = "全部";
            result.TaskHistoryScope = source.TaskHistoryScope == "全部历史" ? "全部历史" : "最近任务";
            result.TaskHistoryRange = IsOneOf(source.TaskHistoryRange, "全部时间", "今天", "昨天", "近7天", "近30天")
                ? source.TaskHistoryRange
                : "全部时间";
            result.MediaSearchText = NormalizeText(source.MediaSearchText, 200);
            result.MediaFilter = IsOneOf(source.MediaFilter, "全部", "截图", "录像", "收藏")
                ? source.MediaFilter
                : "全部";
            result.MediaInboxMode = source.MediaInboxMode == "已忽略" ? "已忽略" : "待归类";
            return result;
        }

        public static List<FilterPresetDefinition> NormalizeMany(IEnumerable<FilterPresetDefinition>? source)
        {
            var result = new List<FilterPresetDefinition>();
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (source == null) return result;

            foreach (var item in source)
            {
                if (result.Count >= 32) break;
                var normalized = Normalize(item);
                if (normalized == null || !ids.Add(normalized.Id)) continue;
                result.Add(normalized);
            }

            return result;
        }

        public static bool IsSupportedWorkspace(string? workspace)
            => workspace == TasksWorkspace || workspace == MediaWorkspace;

        private static bool IsOneOf(string? value, params string[] values)
        {
            foreach (var candidate in values)
                if (string.Equals(value, candidate, StringComparison.Ordinal)) return true;
            return false;
        }

        private static string NormalizeText(string? value, int maxLength)
        {
            var normalized = (value ?? string.Empty).Trim();
            return normalized.Length <= maxLength ? normalized : normalized.Substring(0, maxLength);
        }
    }
}
