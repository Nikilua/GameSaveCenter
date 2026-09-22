using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Core.Services;

public sealed class PolicyTemplateBatchTarget : INotifyPropertyChanged
{
    private bool isSelected;

    public PolicyTemplateBatchTarget(string playniteId, string gameName, int changeCount, bool isSelected)
    {
        PlayniteId = playniteId;
        GameName = string.IsNullOrWhiteSpace(gameName) ? "未命名游戏" : gameName;
        ChangeCount = changeCount;
        this.isSelected = isSelected;
    }

    public string PlayniteId { get; }
    public string GameName { get; }
    public int ChangeCount { get; }
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (isSelected == value) return;
            isSelected = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsExcluded));
            OnPropertyChanged(nameof(SelectionDisplay));
        }
    }

    public bool IsExcluded => !IsSelected;
    public string SelectionDisplay => IsSelected ? "目标" : "排除（未勾选）";
    public string ChangeDisplay => ChangeCount == 0 ? "无字段变化" : $"{ChangeCount} 项字段将覆盖";

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public static class PolicyTemplateBatchPreview
{
    public const int MaxTargetCount = 100;

    public static IReadOnlyList<PolicyTemplateBatchTarget> Build(
        IEnumerable<GameStatusDto> games,
        BackupPolicyDto templatePolicy,
        IEnumerable<string>? selectedPlayniteIds = null)
    {
        if (games == null) throw new ArgumentNullException(nameof(games));
        if (templatePolicy == null) throw new ArgumentNullException(nameof(templatePolicy));

        var selected = new HashSet<string>(
            (selectedPlayniteIds ?? Enumerable.Empty<string>()).Where(id => !string.IsNullOrWhiteSpace(id)),
            StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var normalizedTemplate = BackupPolicyTemplateCatalog.ClonePolicy(templatePolicy);
        var result = new List<PolicyTemplateBatchTarget>();
        foreach (var game in games)
        {
            if (game == null || string.IsNullOrWhiteSpace(game.PlayniteId) || !seen.Add(game.PlayniteId)) continue;
            var changeCount = BackupPolicyDiff.Compare(game.Policy, normalizedTemplate).Count;
            result.Add(new PolicyTemplateBatchTarget(game.PlayniteId, game.Name, changeCount, selected.Contains(game.PlayniteId)));
        }
        return result;
    }

    public static IReadOnlyList<PolicyTemplateBatchTarget> Select(IEnumerable<PolicyTemplateBatchTarget> targets)
    {
        if (targets == null) throw new ArgumentNullException(nameof(targets));
        return targets
            .Where(target => target != null && target.IsSelected && !string.IsNullOrWhiteSpace(target.PlayniteId))
            .GroupBy(target => target.PlayniteId, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
    }

    public static string BuildConfirmation(string templateName, IEnumerable<PolicyTemplateBatchTarget> targets)
    {
        var all = (targets ?? Enumerable.Empty<PolicyTemplateBatchTarget>()).Where(target => target != null).ToList();
        var selected = Select(all);
        var excludedCount = all.Count - selected.Count;
        var changeCount = selected.Sum(target => target.ChangeCount);
        if (selected.Count == 0)
            return "请先明确勾选至少一个目标游戏；当前筛选不会自动选择全部游戏。";
        if (selected.Count > MaxTargetCount)
            return $"当前选择 {selected.Count} 个游戏，超过单次最多 {MaxTargetCount} 个目标；请先减少选择。";

        var lines = selected.Take(20).Select(target => $"{target.GameName}：{target.ChangeCount} 项字段变化").ToList();
        if (selected.Count > lines.Count) lines.Add($"……另有 {selected.Count - lines.Count} 个目标未展开");
        return $"模板“{(string.IsNullOrWhiteSpace(templateName) ? "未命名模板" : templateName)}”将应用到 {selected.Count} 个目标游戏；排除 {excludedCount} 个；预计覆盖 {changeCount} 项字段。{Environment.NewLine}{Environment.NewLine}"
            + string.Join(Environment.NewLine, lines)
            + Environment.NewLine + Environment.NewLine
            + "只处理明确勾选的目标；筛选隐藏项仍按稳定游戏 ID 保留其选择，不会扩大为全部游戏。";
    }
}
