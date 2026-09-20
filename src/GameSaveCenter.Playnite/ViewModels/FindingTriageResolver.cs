using System;
using System.Collections.Generic;
using System.Linq;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.ViewModels;

public enum FindingImpactGroup
{
    Immediate,
    Recommended,
    Information
}

public sealed class FindingTriageGroup
{
    public FindingTriageGroup(FindingImpactGroup group, string title, string description, int count)
    {
        Group = group;
        Title = title;
        Description = description;
        Count = count;
    }

    public FindingImpactGroup Group { get; }
    public string Title { get; }
    public string Description { get; }
    public int Count { get; }
    public string CountDisplay => $"{Count} 项";
}

internal sealed class FindingTriageResult
{
    public FindingTriageResult(IReadOnlyList<ValidationFindingDto> items, IReadOnlyList<FindingTriageGroup> groups)
    {
        Items = items;
        Groups = groups;
    }

    public IReadOnlyList<ValidationFindingDto> Items { get; }
    public IReadOnlyList<FindingTriageGroup> Groups { get; }
}

/// <summary>
/// Keeps the maintenance list actionable without changing the durable finding model.
/// A finding key uses the game, stable code and user-facing issue title so separate
/// health-inspection backup versions remain separate while duplicate sources collapse.
/// </summary>
internal static class FindingTriageResolver
{
    public static FindingTriageResult Resolve(IEnumerable<ValidationFindingDto>? source)
    {
        var items = (source ?? Enumerable.Empty<ValidationFindingDto>())
            .Where(item => item != null)
            .GroupBy(BuildProblemKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => group
                .OrderByDescending(item => item.Severity)
                .ThenByDescending(item => HasEvidenceTime(item))
                .ThenByDescending(item => item.CreatedUtc)
                .ThenBy(item => item.Title ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .First())
            .OrderBy(item => GroupRank(item.Severity))
            .ThenByDescending(item => HasEvidenceTime(item))
            .ThenByDescending(item => item.CreatedUtc)
            .ThenBy(item => item.GameName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.Title ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var groups = new[]
        {
            BuildGroup(FindingImpactGroup.Immediate, "需立即处理", "会阻断恢复或存在数据风险的项目。", items),
            BuildGroup(FindingImpactGroup.Recommended, "建议处理", "不会立即阻断操作，但应尽快复核的项目。", items),
            BuildGroup(FindingImpactGroup.Information, "信息项", "保留背景证据，不要求立即操作。", items)
        };
        return new FindingTriageResult(items, groups);
    }

    private static FindingTriageGroup BuildGroup(
        FindingImpactGroup group,
        string title,
        string description,
        IReadOnlyList<ValidationFindingDto> items)
        => new(group, title, description, items.Count(item => Classify(item.Severity) == group));

    private static string BuildProblemKey(ValidationFindingDto item)
    {
        var game = Normalize(item.PlayniteId);
        var code = Normalize(item.Code);
        var title = Normalize(item.Title);
        if (string.IsNullOrWhiteSpace(title)) title = Normalize(item.Detail);
        return $"{game}\u001f{code}\u001f{title}";
    }

    private static FindingImpactGroup Classify(FindingSeverity severity)
        => severity >= FindingSeverity.Error
            ? FindingImpactGroup.Immediate
            : severity == FindingSeverity.Warning
                ? FindingImpactGroup.Recommended
                : FindingImpactGroup.Information;

    private static int GroupRank(FindingSeverity severity) => Classify(severity) switch
    {
        FindingImpactGroup.Immediate => 0,
        FindingImpactGroup.Recommended => 1,
        _ => 2
    };

    private static bool HasEvidenceTime(ValidationFindingDto item) => item.CreatedUtc != DateTime.MinValue;

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        return string.Join(" ", value!.Trim().Split(new[] { ' ', (char)9, (char)13, (char)10 }, StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant();
    }
}
