using System;
using System.Collections.Generic;

namespace GameSaveCenter.Contracts
{
    /// <summary>Read-only diagnostic query for one Playnite game identity.</summary>
    public sealed class GameDiscoveryDiagnosticRequestDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public string CurrentStatusFilter { get; set; } = "全部";
        public string CurrentPlatformFilter { get; set; } = "全部";
        public string CurrentSearchText { get; set; } = string.Empty;
    }

    /// <summary>Descriptor-only update; matching is intentionally not part of this action.</summary>
    public sealed class GameDescriptorSyncRequestDto
    {
        public GameDescriptorDto Descriptor { get; set; } = new GameDescriptorDto();
    }

    /// <summary>Explicit one-game match retry request.</summary>
    public sealed class GameMatchRetryRequestDto
    {
        public string PlayniteId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Sanitized explanation of the Playnite-to-Worker catalog path. No local path is
    /// included; callers receive only existence signals and timestamps.
    /// </summary>
    public sealed class GameDiscoveryDiagnosticDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public GamePlatformKind Platform { get; set; }
        public bool PlayniteExists { get; set; }
        public bool WorkerRecordExists { get; set; }
        public bool WorkerReachable { get; set; } = true;
        public string WorkerMessage { get; set; } = string.Empty;
        public bool SourceMissing { get; set; }
        public bool PlayniteIsInstalled { get; set; }
        public bool IsInstalled { get; set; }
        public string InstallStateSource { get; set; } = GameInstallStateSources.Unknown;
        public bool HasInstallDirectoryConfigured { get; set; }
        public bool InstallDirectoryPresent { get; set; }
        public DateTime? DescriptorSyncedUtc { get; set; }
        public bool LudusaviMatched { get; set; }
        public string LudusaviName { get; set; } = string.Empty;
        public double MatchConfidence { get; set; }
        public DateTime? LastMatchAttemptUtc { get; set; }
        public string MatchState { get; set; } = "Unknown";
        public int BackupVersionCount { get; set; }
        public DateTime? LastBackupUtc { get; set; }
        public string CurrentStatusFilter { get; set; } = "全部";
        public string CurrentPlatformFilter { get; set; } = "全部";
        public string CurrentSearchText { get; set; } = string.Empty;
        public List<string> FilterExclusionReasons { get; set; } = new List<string>();
    }
}
