using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSaveCenter.Playnite.Settings
{
    /// <summary>
    /// Identifies a durable worklist reference without storing the referenced runtime object.
    /// StableObjectId must include any parent identity needed to make the reference unique.
    /// </summary>
    public enum WorklistObjectKind
    {
        BackupVersion = 1,
        MediaItem = 2,
        BackgroundTask = 3,
        ValidationFinding = 4
    }

    public enum WorklistItemAddResult
    {
        Added,
        AlreadyPresent,
        LimitReached,
        InvalidReference
    }

    /// <summary>A user-maintained list of stable references and manual follow-up notes.</summary>
    public sealed class WorklistDefinition
    {
        public const int MaximumWorklists = 20;
        public const int MaximumNameLength = 80;
        public const int MaximumItemsPerWorklist = 200;
        public const int MaximumStableObjectIdLength = 256;
        public const int MaximumNoteLength = 500;

        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public List<WorklistItemRecord> Items { get; set; } = new List<WorklistItemRecord>();

        public WorklistDefinition Clone()
            => new WorklistDefinition
            {
                Id = Id,
                Name = Name,
                Items = NormalizeItems(Items)
            };

        public bool Contains(WorklistObjectKind kind, string? stableObjectId)
        {
            if (!Enum.IsDefined(typeof(WorklistObjectKind), kind)) return false;
            var normalizedId = (stableObjectId ?? string.Empty).Trim();
            if (normalizedId.Length == 0 || normalizedId.Length > MaximumStableObjectIdLength) return false;
            return NormalizeItems(Items).Any(item => item.Kind == kind
                && string.Equals(item.StableObjectId, normalizedId, StringComparison.OrdinalIgnoreCase));
        }

        public WorklistItemAddResult TryAdd(WorklistItemRecord? item)
        {
            var normalized = NormalizeItem(item);
            if (normalized == null) return WorklistItemAddResult.InvalidReference;

            Items = NormalizeItems(Items);
            if (Contains(normalized.Kind, normalized.StableObjectId))
                return WorklistItemAddResult.AlreadyPresent;
            if (Items.Count >= MaximumItemsPerWorklist)
                return WorklistItemAddResult.LimitReached;

            Items.Add(normalized);
            return WorklistItemAddResult.Added;
        }

        public static WorklistDefinition? Normalize(WorklistDefinition? source)
        {
            if (source == null) return null;

            return new WorklistDefinition
            {
                Id = source.Id == Guid.Empty ? Guid.NewGuid() : source.Id,
                Name = NormalizeName(source.Name),
                Items = NormalizeItems(source.Items)
            };
        }

        public static List<WorklistDefinition> NormalizeMany(IEnumerable<WorklistDefinition>? source)
        {
            var result = new List<WorklistDefinition>();
            var positions = new Dictionary<Guid, int>();
            if (source == null) return result;

            foreach (var item in source.Take(MaximumWorklists * 4))
            {
                var normalized = Normalize(item);
                if (normalized == null) continue;

                if (positions.TryGetValue(normalized.Id, out var existingIndex))
                {
                    result[existingIndex] = normalized;
                    continue;
                }

                if (result.Count >= MaximumWorklists) continue;
                positions[normalized.Id] = result.Count;
                result.Add(normalized);
            }

            return result.Select(item => item.Clone()).ToList();
        }

        private static string NormalizeName(string? value)
        {
            var normalized = (value ?? string.Empty).Trim();
            if (normalized.Length == 0) normalized = "未命名清单";
            return normalized.Length <= MaximumNameLength
                ? normalized
                : normalized.Substring(0, MaximumNameLength);
        }

        private static List<WorklistItemRecord> NormalizeItems(IEnumerable<WorklistItemRecord>? source)
        {
            var result = new List<WorklistItemRecord>();
            var identities = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (source == null) return result;

            // Bound input scanning as well as the normalized result so malformed settings
            // cannot force an unbounded enumeration before the item cap is applied.
            foreach (var item in source.Take(MaximumItemsPerWorklist * 4))
            {
                var normalized = NormalizeItem(item);
                if (normalized == null) continue;

                var identity = BuildIdentity(normalized.Kind, normalized.StableObjectId);
                if (!identities.Add(identity)) continue;
                if (result.Count >= MaximumItemsPerWorklist) continue;
                result.Add(normalized);
            }

            return result;
        }

        private static WorklistItemRecord? NormalizeItem(WorklistItemRecord? item)
        {
            if (item == null || !Enum.IsDefined(typeof(WorklistObjectKind), item.Kind)) return null;
            var stableObjectId = (item.StableObjectId ?? string.Empty).Trim();
            if (stableObjectId.Length == 0 || stableObjectId.Length > MaximumStableObjectIdLength) return null;

            var note = (item.Note ?? string.Empty).Trim();
            if (note.Length > MaximumNoteLength) note = note.Substring(0, MaximumNoteLength);
            return new WorklistItemRecord
            {
                Kind = item.Kind,
                StableObjectId = stableObjectId,
                Note = note,
                IsManuallyComplete = item.IsManuallyComplete
            };
        }

        private static string BuildIdentity(WorklistObjectKind kind, string stableObjectId)
            => ((int)kind).ToString(System.Globalization.CultureInfo.InvariantCulture) + ":" + stableObjectId;
    }

    /// <summary>
    /// Stores only a kind, stable object key, user note and manual completion flag.
    /// It intentionally has no runtime DTO, path, credentials, command or operation token.
    /// </summary>
    public sealed class WorklistItemRecord
    {
        public WorklistObjectKind Kind { get; set; }
        public string StableObjectId { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public bool IsManuallyComplete { get; set; }

        public WorklistItemRecord Clone()
            => new WorklistItemRecord
            {
                Kind = Kind,
                StableObjectId = StableObjectId,
                Note = Note,
                IsManuallyComplete = IsManuallyComplete
            };
    }
}
