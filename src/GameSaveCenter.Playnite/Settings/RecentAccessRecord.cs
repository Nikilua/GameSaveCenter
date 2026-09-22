using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSaveCenter.Playnite.Settings
{
    /// <summary>
    /// Durable, UI-only recent access state. It deliberately stores no display name,
    /// archive path or other mutable/local data; names are resolved from the current
    /// game snapshot when the overview is rendered.
    /// </summary>
    public sealed class RecentAccessRecord
    {
        public const int MaxEntries = 8;
        public const string OverviewWorkspace = "Overview";
        public const string SavesWorkspace = "Saves";
        public const string TrainersWorkspace = "Trainers";
        public const string MediaWorkspace = "Media";
        public const string TasksWorkspace = "Tasks";
        public const string MaintenanceWorkspace = "Maintenance";

        public string PlayniteId { get; set; } = string.Empty;
        public string Workspace { get; set; } = OverviewWorkspace;
        public int TabIndex { get; set; }
        public DateTime LastAccessUtc { get; set; }

        public RecentAccessRecord Clone()
            => new RecentAccessRecord
            {
                PlayniteId = PlayniteId,
                Workspace = Workspace,
                TabIndex = TabIndex,
                LastAccessUtc = LastAccessUtc
            };

        public static RecentAccessRecord? Normalize(RecentAccessRecord? source)
        {
            if (source == null) return null;
            var playniteId = (source.PlayniteId ?? string.Empty).Trim();
            if (playniteId.Length == 0 || playniteId.Length > 128) return null;

            var timestamp = source.LastAccessUtc;
            if (timestamp == default)
                timestamp = DateTime.UtcNow;
            else if (timestamp.Kind == DateTimeKind.Unspecified)
                timestamp = DateTime.SpecifyKind(timestamp, DateTimeKind.Utc);
            else
                timestamp = timestamp.ToUniversalTime();

            return new RecentAccessRecord
            {
                PlayniteId = playniteId,
                Workspace = NormalizeWorkspace(source.Workspace),
                TabIndex = Math.Max(0, Math.Min(3, source.TabIndex)),
                LastAccessUtc = timestamp
            };
        }

        public static List<RecentAccessRecord> NormalizeMany(IEnumerable<RecentAccessRecord>? source)
        {
            var byId = new Dictionary<string, RecentAccessRecord>(StringComparer.OrdinalIgnoreCase);
            if (source != null)
            {
                foreach (var item in source)
                {
                    var normalized = Normalize(item);
                    if (normalized == null) continue;
                    if (!byId.TryGetValue(normalized.PlayniteId, out var previous)
                        || normalized.LastAccessUtc >= previous.LastAccessUtc)
                        byId[normalized.PlayniteId] = normalized;
                }
            }

            return byId.Values
                .OrderByDescending(item => item.LastAccessUtc)
                .ThenBy(item => item.PlayniteId, StringComparer.OrdinalIgnoreCase)
                .Take(MaxEntries)
                .Select(item => item.Clone())
                .ToList();
        }

        public static List<RecentAccessRecord> Upsert(
            IEnumerable<RecentAccessRecord>? source,
            string playniteId,
            string workspace,
            int tabIndex,
            DateTime lastAccessUtc)
        {
            var records = NormalizeMany(source);
            records.Add(new RecentAccessRecord
            {
                PlayniteId = playniteId,
                Workspace = workspace,
                TabIndex = tabIndex,
                LastAccessUtc = lastAccessUtc
            });
            return NormalizeMany(records);
        }

        public static List<RecentAccessRecord> KeepExistingGames(
            IEnumerable<RecentAccessRecord>? source,
            IEnumerable<string>? existingPlayniteIds)
        {
            var ids = new HashSet<string>(
                (existingPlayniteIds ?? Enumerable.Empty<string>())
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(id => id.Trim()),
                StringComparer.OrdinalIgnoreCase);
            return NormalizeMany(
                (source ?? Enumerable.Empty<RecentAccessRecord>())
                    .Where(item => item != null && ids.Contains(item.PlayniteId)))
                .ToList();
        }

        public static bool IsSupportedWorkspace(string? workspace)
            => workspace == OverviewWorkspace
               || workspace == SavesWorkspace
               || workspace == TrainersWorkspace
               || workspace == MediaWorkspace
               || workspace == TasksWorkspace
               || workspace == MaintenanceWorkspace;

        private static string NormalizeWorkspace(string? workspace)
            => IsSupportedWorkspace(workspace) ? workspace! : OverviewWorkspace;
    }
}
