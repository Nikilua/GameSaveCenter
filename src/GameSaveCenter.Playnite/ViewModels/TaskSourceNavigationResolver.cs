using System;
using System.Collections.Generic;
using System.Linq;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.ViewModels;

/// <summary>Resolves task source targets by durable identity only.</summary>
public static class TaskSourceNavigationResolver
{
    public static GameStatusDto? ResolveExactGame(
        TaskSourceReferenceDto? source,
        IEnumerable<GameStatusDto>? games)
    {
        var playniteId = source?.PlayniteId?.Trim();
        if (string.IsNullOrWhiteSpace(playniteId))
            playniteId = source?.StableId?.Trim();
        if (string.IsNullOrWhiteSpace(playniteId)) return null;

        return (games ?? Enumerable.Empty<GameStatusDto>())
            .FirstOrDefault(candidate => string.Equals(candidate.PlayniteId, playniteId, StringComparison.OrdinalIgnoreCase));
    }

    public static BackupVersionDto? ResolveExactBackupVersion(
        string? stableId,
        IEnumerable<BackupVersionDto>? versions)
    {
        if (string.IsNullOrWhiteSpace(stableId)) return null;
        return (versions ?? Enumerable.Empty<BackupVersionDto>())
            .FirstOrDefault(candidate => string.Equals(candidate.BackupId, stableId, StringComparison.OrdinalIgnoreCase));
    }
}
