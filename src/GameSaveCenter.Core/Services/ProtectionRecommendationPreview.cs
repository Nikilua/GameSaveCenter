using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSaveCenter.Core.Services;

/// <summary>Builds the human-readable confirmation preview for batch protection changes.</summary>
public static class ProtectionRecommendationPreview
{
    public static IReadOnlyList<RecentProtectionItem> Select(IEnumerable<RecentProtectionItem> items)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));

        return items
            .Where(item => item != null && item.IsSelectable && item.IsSelected && !string.IsNullOrWhiteSpace(item.PlayniteId))
            .GroupBy(item => item.PlayniteId, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .Take(100)
            .ToList();
    }

    public static string Build(IEnumerable<RecentProtectionItem> items)
    {
        var selected = Select(items);
        if (selected.Count == 0) return "请先选择需要启用自动保护的游戏。";

        var changes = selected.Select(item =>
            $"{(string.IsNullOrWhiteSpace(item.GameName) ? "未命名游戏" : item.GameName)} → 推荐自动保护（游戏中 + 游戏退出后）");

        return $"将修改 {selected.Count} 个游戏：{Environment.NewLine}{Environment.NewLine}"
            + string.Join(Environment.NewLine, changes)
            + Environment.NewLine + Environment.NewLine
            + "确认后只更新这些游戏的自动保护开关，不会执行备份、恢复或覆盖现有其他策略设置。";
    }
}
