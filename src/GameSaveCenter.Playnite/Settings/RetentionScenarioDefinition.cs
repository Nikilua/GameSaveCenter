using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace GameSaveCenter.Playnite.Settings;

/// <summary>
/// Bounded, scalar-only saved assumptions for the retention comparison UI. This is not an
/// effective policy and contains neither a cleanup preview handle nor a cached result set.
/// </summary>
public sealed class RetentionScenarioDefinition
{
    public const int CurrentSchemaVersion = 1;
    public const int MaximumSavedDrafts = 32;
    public const int MaximumNameLength = 48;
    public const int MaximumKeepAllForHours = 876000;
    public const int MaximumKeepDailyDays = 36500;
    public const int MaximumKeepWeeklyWeeks = 5200;
    public const int MaximumKeepMonthlyMonths = 1200;
    public const int MaximumGrowthMiBPer30Days = 1000000;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastSnapshotUtc { get; set; }
    public int KeepAllForHours { get; set; } = 24;
    public int KeepDailyDays { get; set; } = 30;
    public int KeepWeeklyWeeks { get; set; } = 12;
    public int KeepMonthlyMonths { get; set; } = 24;
    public int? GrowthMinimumMiBPer30Days { get; set; }
    public int? GrowthMaximumMiBPer30Days { get; set; }

    public RetentionScenarioDefinition Clone()
        => new RetentionScenarioDefinition
        {
            SchemaVersion = SchemaVersion,
            Id = Id,
            Name = Name,
            CreatedUtc = CreatedUtc,
            UpdatedUtc = UpdatedUtc,
            LastSnapshotUtc = LastSnapshotUtc,
            KeepAllForHours = KeepAllForHours,
            KeepDailyDays = KeepDailyDays,
            KeepWeeklyWeeks = KeepWeeklyWeeks,
            KeepMonthlyMonths = KeepMonthlyMonths,
            GrowthMinimumMiBPer30Days = GrowthMinimumMiBPer30Days,
            GrowthMaximumMiBPer30Days = GrowthMaximumMiBPer30Days
        };

    public RetentionScenarioDefinition CopyWithName(string name)
    {
        var normalizedName = NormalizeName(name);
        if (normalizedName.Length == 0) throw new ArgumentException("情景名称不能为空。", nameof(name));

        var now = DateTime.UtcNow;
        var copy = Clone();
        copy.Id = Guid.NewGuid().ToString("N");
        copy.Name = normalizedName;
        copy.CreatedUtc = now;
        copy.UpdatedUtc = now;
        return copy;
    }

    public RetentionScenarioDefinition Rename(string name)
    {
        var normalizedName = NormalizeName(name);
        if (normalizedName.Length == 0) throw new ArgumentException("情景名称不能为空。", nameof(name));

        var renamed = Clone();
        renamed.Name = normalizedName;
        renamed.UpdatedUtc = DateTime.UtcNow;
        return renamed;
    }

    public static RetentionScenarioDefinition? Normalize(RetentionScenarioDefinition? source)
    {
        if (source == null || source.SchemaVersion != CurrentSchemaVersion) return null;

        var name = NormalizeText(source.Name, MaximumNameLength);
        if (name.Length == 0
            || !IsInRange(source.KeepAllForHours, MaximumKeepAllForHours)
            || !IsInRange(source.KeepDailyDays, MaximumKeepDailyDays)
            || !IsInRange(source.KeepWeeklyWeeks, MaximumKeepWeeklyWeeks)
            || !IsInRange(source.KeepMonthlyMonths, MaximumKeepMonthlyMonths)
            || !IsInRange(source.GrowthMinimumMiBPer30Days, MaximumGrowthMiBPer30Days)
            || !IsInRange(source.GrowthMaximumMiBPer30Days, MaximumGrowthMiBPer30Days)
            || !source.GrowthMinimumMiBPer30Days.HasValue
            || !source.GrowthMaximumMiBPer30Days.HasValue
            || source.GrowthMinimumMiBPer30Days.Value > source.GrowthMaximumMiBPer30Days.Value)
            return null;

        var now = DateTime.UtcNow;
        var id = Guid.TryParseExact(source.Id, "N", out var parsedId)
            ? parsedId.ToString("N")
            : Guid.NewGuid().ToString("N");
        var createdUtc = NormalizeUtc(source.CreatedUtc, now);
        var updatedUtc = NormalizeUtc(source.UpdatedUtc, createdUtc);
        if (updatedUtc < createdUtc) updatedUtc = createdUtc;

        var result = source.Clone();
        result.SchemaVersion = CurrentSchemaVersion;
        result.Id = id;
        result.Name = name;
        result.CreatedUtc = createdUtc;
        result.UpdatedUtc = updatedUtc;
        result.LastSnapshotUtc = source.LastSnapshotUtc.HasValue
            ? NormalizeUtc(source.LastSnapshotUtc.Value, createdUtc)
            : (DateTime?)null;
        return result;
    }

    public static List<RetentionScenarioDefinition> NormalizeMany(IEnumerable<RetentionScenarioDefinition>? source)
    {
        var result = new List<RetentionScenarioDefinition>();
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in source ?? Enumerable.Empty<RetentionScenarioDefinition>())
        {
            if (result.Count >= MaximumSavedDrafts) break;
            var normalized = Normalize(item);
            if (normalized == null || !ids.Add(normalized.Id) || !names.Add(normalized.Name)) continue;
            result.Add(normalized);
        }

        return result;
    }

    public static string NormalizeName(string? value)
        => NormalizeText(value, MaximumNameLength);

    private static DateTime NormalizeUtc(DateTime value, DateTime fallback)
    {
        if (value == default) return DateTime.SpecifyKind(fallback, DateTimeKind.Utc);
        if (value.Kind == DateTimeKind.Utc) return value;
        if (value.Kind == DateTimeKind.Unspecified) return DateTime.SpecifyKind(value, DateTimeKind.Utc);
        return value.ToUniversalTime();
    }

    private static bool IsInRange(int value, int maximum) => value >= 0 && value <= maximum;
    private static bool IsInRange(int? value, int maximum) => value.HasValue && IsInRange(value.Value, maximum);

    private static string NormalizeText(string? value, int maximumLength)
    {
        var normalized = (value ?? string.Empty).Trim();
        return normalized.Length <= maximumLength ? normalized : normalized.Substring(0, maximumLength);
    }
}
