using System;
using System.Collections.Generic;
using System.Linq;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.ViewModels;

/// <summary>Creates a bounded, direction-preserving projection of a backup diff for the compare page.</summary>
public static class BackupDiffPathFilter
{
    public static BackupDiffPathFilterResult Apply(BackupDiffDto? diff, string? query, string? kind, int visibleLimit)
    {
        var limit = Math.Max(1, visibleLimit);
        var normalizedQuery = (query ?? string.Empty).Trim();
        var normalizedKind = string.IsNullOrWhiteSpace(kind) ? "全部" : kind!;
        var added = Match(diff?.Added, normalizedQuery, normalizedKind, "新增");
        var modified = Match(diff?.Modified, normalizedQuery, normalizedKind, "修改");
        var removed = Match(diff?.Removed, normalizedQuery, normalizedKind, "删除");
        return new BackupDiffPathFilterResult(
            added,
            modified,
            removed,
            added.Take(limit).ToArray(),
            modified.Take(limit).ToArray(),
            removed.Take(limit).ToArray());
    }

    private static IReadOnlyList<string> Match(IEnumerable<string>? source, string query, string kind, string expectedKind)
    {
        if (!string.Equals(kind, "全部", StringComparison.Ordinal) && !string.Equals(kind, expectedKind, StringComparison.Ordinal))
            return Array.Empty<string>();
        return (source ?? Enumerable.Empty<string>())
            .Where(path => !string.IsNullOrWhiteSpace(path)
                && (query.Length == 0 || path.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0))
            .ToArray();
    }
}

public sealed class BackupDiffPathFilterResult
{
    public BackupDiffPathFilterResult(
        IReadOnlyList<string> added,
        IReadOnlyList<string> modified,
        IReadOnlyList<string> removed,
        IReadOnlyList<string> visibleAdded,
        IReadOnlyList<string> visibleModified,
        IReadOnlyList<string> visibleRemoved)
    {
        Added = added;
        Modified = modified;
        Removed = removed;
        VisibleAdded = visibleAdded;
        VisibleModified = visibleModified;
        VisibleRemoved = visibleRemoved;
    }

    public IReadOnlyList<string> Added { get; }
    public IReadOnlyList<string> Modified { get; }
    public IReadOnlyList<string> Removed { get; }
    public IReadOnlyList<string> VisibleAdded { get; }
    public IReadOnlyList<string> VisibleModified { get; }
    public IReadOnlyList<string> VisibleRemoved { get; }
    public bool HasMore => Added.Count > VisibleAdded.Count
        || Modified.Count > VisibleModified.Count
        || Removed.Count > VisibleRemoved.Count;
    public int MatchCount => Added.Count + Modified.Count + Removed.Count;
    public int VisibleCount => VisibleAdded.Count + VisibleModified.Count + VisibleRemoved.Count;
}
