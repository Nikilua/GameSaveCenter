using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSaveCenter.Playnite.Settings
{
    /// <summary>
    /// A durable, UI-only view arrangement. It stores stable identifiers and scalar values,
    /// never live DTOs, file paths, or file contents.
    /// </summary>
    public sealed class ViewConfigurationDefinition
    {
        public const int CurrentSchemaVersion = 1;
        private const int MaximumConfigurations = 32;
        private const int MaximumVisibleColumns = 64;
        private const int MaximumColumnWidths = 64;
        private const double MaximumColumnWidth = 4096d;

        public int SchemaVersion { get; set; } = CurrentSchemaVersion;
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = string.Empty;
        public string WorkspaceKey { get; set; } = string.Empty;
        public string FilterPresetId { get; set; } = string.Empty;
        public string SortColumnKey { get; set; } = string.Empty;
        public string SortDirection { get; set; } = "Ascending";
        public List<string> VisibleColumnKeys { get; set; } = new List<string>();
        public Dictionary<string, double> ColumnWidths { get; set; } = new Dictionary<string, double>(StringComparer.Ordinal);
        public string DensityMode { get; set; } = "Comfortable";

        public ViewConfigurationDefinition Clone()
        {
            return new ViewConfigurationDefinition
            {
                SchemaVersion = SchemaVersion,
                Id = Id,
                Name = Name,
                WorkspaceKey = WorkspaceKey,
                FilterPresetId = FilterPresetId,
                SortColumnKey = SortColumnKey,
                SortDirection = SortDirection,
                VisibleColumnKeys = new List<string>(VisibleColumnKeys ?? new List<string>()),
                ColumnWidths = new Dictionary<string, double>(ColumnWidths ?? new Dictionary<string, double>(), StringComparer.Ordinal),
                DensityMode = DensityMode
            };
        }

        /// <summary>
        /// Normalizes known schema version 1 fields. Missing settings collections use an
        /// empty list; unsupported schema versions are ignored and fall back to defaults.
        /// Unknown JSON fields are ignored by the settings serializer.
        /// </summary>
        public static ViewConfigurationDefinition? Normalize(ViewConfigurationDefinition? source)
        {
            if (source == null || source.SchemaVersion != CurrentSchemaVersion) return null;

            var name = NormalizeText(source.Name, 48);
            var workspaceKey = NormalizeText(source.WorkspaceKey, 64);
            if (name.Length == 0 || workspaceKey.Length == 0) return null;

            var result = source.Clone();
            result.SchemaVersion = CurrentSchemaVersion;
            result.Id = Guid.TryParseExact(source.Id, "N", out var parsedId) ? parsedId.ToString("N") : Guid.NewGuid().ToString("N");
            result.Name = name;
            result.WorkspaceKey = workspaceKey;
            result.FilterPresetId = NormalizeId(source.FilterPresetId);
            result.SortColumnKey = NormalizeKey(source.SortColumnKey, 80);
            result.SortDirection = string.Equals(source.SortDirection, "Descending", StringComparison.OrdinalIgnoreCase)
                ? "Descending"
                : "Ascending";
            result.VisibleColumnKeys = NormalizeColumnKeys(source.VisibleColumnKeys);
            result.ColumnWidths = NormalizeColumnWidths(source.ColumnWidths);
            result.DensityMode = string.Equals(source.DensityMode, "Compact", StringComparison.OrdinalIgnoreCase)
                ? "Compact"
                : "Comfortable";
            return result;
        }

        public static List<ViewConfigurationDefinition> NormalizeMany(IEnumerable<ViewConfigurationDefinition>? source)
        {
            var result = new List<ViewConfigurationDefinition>();
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (source == null) return result;

            foreach (var item in source)
            {
                if (result.Count >= MaximumConfigurations) break;
                var normalized = Normalize(item);
                if (normalized == null || !ids.Add(normalized.Id)) continue;
                result.Add(normalized);
            }

            return result;
        }

        private static List<string> NormalizeColumnKeys(IEnumerable<string>? source)
        {
            var result = new List<string>();
            var keys = new HashSet<string>(StringComparer.Ordinal);
            if (source == null) return result;

            foreach (var item in source)
            {
                var key = NormalizeKey(item, 80);
                if (key.Length == 0 || !keys.Add(key)) continue;
                result.Add(key);
                if (result.Count >= MaximumVisibleColumns) break;
            }

            return result;
        }

        private static Dictionary<string, double> NormalizeColumnWidths(IDictionary<string, double>? source)
        {
            var result = new Dictionary<string, double>(StringComparer.Ordinal);
            if (source == null) return result;

            foreach (var pair in source)
            {
                var key = NormalizeKey(pair.Key, 80);
                var width = pair.Value;
                if (result.Count >= MaximumColumnWidths || key.Length == 0 || !IsFinite(width) || width <= 0 || width > MaximumColumnWidth)
                    continue;
                result[key] = width;
            }

            return result;
        }

        private static string NormalizeId(string? value)
        {
            var normalized = NormalizeText(value, 64);
            return normalized.Length == 0 || Guid.TryParseExact(normalized, "N", out _) ? normalized : string.Empty;
        }

        private static string NormalizeKey(string? value, int maxLength)
            => NormalizeText(value, maxLength);

        private static string NormalizeText(string? value, int maxLength)
        {
            var normalized = (value ?? string.Empty).Trim();
            return normalized.Length <= maxLength ? normalized : normalized.Substring(0, maxLength);
        }

        private static bool IsFinite(double value)
            => !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
