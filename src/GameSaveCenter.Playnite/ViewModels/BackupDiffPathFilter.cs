using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;

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

    internal IReadOnlyList<BackupDiffDirectory> GroupByDirectory()
        => Added.Select(path => new BackupDiffDirectoryPath(path, "新增"))
            .Concat(Modified.Select(path => new BackupDiffDirectoryPath(path, "修改")))
            .Concat(Removed.Select(path => new BackupDiffDirectoryPath(path, "删除")))
            .GroupBy(entry => entry.Directory, StringComparer.OrdinalIgnoreCase)
            .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => new BackupDiffDirectory(group.Key, group.ToArray()))
            .ToArray();
}

public sealed class BackupDiffDirectoryPath
{
    public BackupDiffDirectoryPath(string path, string kind)
    {
        FullPath = path;
        Kind = kind;
        var separator = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));
        Directory = separator < 0 ? string.Empty : path.Substring(0, separator).Replace('\\', '/');
        FileName = path.Substring(separator + 1);
    }

    public string Directory { get; }
    public string FullPath { get; }
    public string FileName { get; }
    public string Kind { get; }
    public string CopyName => $"复制完整{Kind}路径：{FullPath}";
}

public sealed class BackupDiffDirectory
{
    internal const int PageSize = 120;
    private readonly IReadOnlyList<BackupDiffDirectoryPath> paths;
    private readonly RelayCommand loadMoreCommand;

    public BackupDiffDirectory(string directory, IReadOnlyList<BackupDiffDirectoryPath> paths)
    {
        Name = directory.Length == 0 ? "根目录" : directory;
        this.paths = paths;
        AddedCount = paths.Count(path => path.Kind == "新增");
        ModifiedCount = paths.Count(path => path.Kind == "修改");
        RemovedCount = paths.Count(path => path.Kind == "删除");
        loadMoreCommand = new RelayCommand(_ => LoadMore(), _ => VisiblePaths.Count < paths.Count);
        LoadMore();
    }

    public string Name { get; }
    public int AddedCount { get; }
    public int ModifiedCount { get; }
    public int RemovedCount { get; }
    public string Counts => $"新增 {AddedCount}　修改 {ModifiedCount}　删除 {RemovedCount}";
    public string AutomationName => $"{Name}，{Counts}";
    public override string ToString() => AutomationName;
    public bool IsExpanded { get; set; }
    public BatchObservableCollection<BackupDiffDirectoryPath> VisiblePaths { get; } = new BatchObservableCollection<BackupDiffDirectoryPath>();
    public ICommand LoadMoreCommand => loadMoreCommand;

    private void LoadMore()
    {
        foreach (var path in paths.Skip(VisiblePaths.Count).Take(PageSize))
            VisiblePaths.Add(path);
        loadMoreCommand.RaiseCanExecuteChanged();
    }
}
