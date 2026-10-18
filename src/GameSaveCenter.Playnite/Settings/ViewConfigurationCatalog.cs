using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSaveCenter.Playnite.Settings
{
    internal enum ViewConfigurationMutationResult
    {
        Success,
        InvalidName,
        DuplicateName,
        LimitReached,
        MissingConfiguration
    }

    /// <summary>Validates workspace-scoped named view mutations against durable settings.</summary>
    internal static class ViewConfigurationCatalog
    {
        internal const int MaximumNameLength = 48;
        internal const int MaximumConfigurations = 32;

        internal static IReadOnlyList<ViewConfigurationDefinition> GetForWorkspace(
            GameSaveCenterSettings settings,
            string workspaceKey)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrWhiteSpace(workspaceKey)) return Array.Empty<ViewConfigurationDefinition>();
            return settings.ViewConfigurations
                .Where(item => string.Equals(item.WorkspaceKey, workspaceKey, StringComparison.Ordinal))
                .Select(item => item.Clone())
                .ToArray();
        }

        internal static ViewConfigurationMutationResult SaveCurrent(
            GameSaveCenterSettings settings,
            string workspaceKey,
            string? requestedName,
            ViewConfigurationDefinition currentState,
            string? selectedId,
            bool saveCopy,
            out ViewConfigurationDefinition? saved)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (currentState == null) throw new ArgumentNullException(nameof(currentState));
            saved = null;
            var existing = string.IsNullOrWhiteSpace(selectedId)
                ? null
                : settings.ViewConfigurations.FirstOrDefault(item =>
                    item.Id == selectedId && item.WorkspaceKey == workspaceKey);
            if (!saveCopy && !string.IsNullOrWhiteSpace(selectedId) && existing == null)
                return ViewConfigurationMutationResult.MissingConfiguration;

            var isNew = saveCopy || existing == null;
            var name = isNew ? NormalizeName(requestedName) : existing!.Name;
            if (name.Length == 0) return ViewConfigurationMutationResult.InvalidName;
            if (settings.ViewConfigurations.Any(item =>
                    item.WorkspaceKey == workspaceKey
                    && (isNew || !ReferenceEquals(item, existing))
                    && string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)))
                return ViewConfigurationMutationResult.DuplicateName;
            if (isNew && settings.ViewConfigurations.Count >= MaximumConfigurations)
                return ViewConfigurationMutationResult.LimitReached;

            var snapshot = currentState.Clone();
            snapshot.SchemaVersion = ViewConfigurationDefinition.CurrentSchemaVersion;
            snapshot.Id = isNew ? Guid.NewGuid().ToString("N") : existing!.Id;
            snapshot.Name = name;
            snapshot.WorkspaceKey = workspaceKey;
            var normalized = ViewConfigurationDefinition.Normalize(snapshot);
            if (normalized == null) return ViewConfigurationMutationResult.InvalidName;

            var configurations = settings.ViewConfigurations.Select(item => item.Clone()).ToList();
            if (isNew)
                configurations.Add(normalized);
            else
            {
                var index = configurations.FindIndex(item => item.Id == existing!.Id);
                if (index < 0) return ViewConfigurationMutationResult.MissingConfiguration;
                configurations[index] = normalized;
            }
            settings.ViewConfigurations = configurations;
            saved = normalized.Clone();
            return ViewConfigurationMutationResult.Success;
        }

        internal static ViewConfigurationMutationResult Rename(
            GameSaveCenterSettings settings,
            string workspaceKey,
            string? selectedId,
            string? requestedName,
            out ViewConfigurationDefinition? renamed)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            renamed = null;
            var existing = Find(settings, workspaceKey, selectedId);
            if (existing == null) return ViewConfigurationMutationResult.MissingConfiguration;
            var name = NormalizeName(requestedName);
            if (name.Length == 0) return ViewConfigurationMutationResult.InvalidName;
            if (settings.ViewConfigurations.Any(item =>
                    item.WorkspaceKey == workspaceKey
                    && item.Id != existing.Id
                    && string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)))
                return ViewConfigurationMutationResult.DuplicateName;

            var copy = existing.Clone();
            copy.Name = name;
            var configurations = settings.ViewConfigurations.Select(item => item.Id == existing.Id ? copy : item.Clone()).ToList();
            settings.ViewConfigurations = configurations;
            renamed = copy.Clone();
            return ViewConfigurationMutationResult.Success;
        }

        internal static ViewConfigurationMutationResult Delete(
            GameSaveCenterSettings settings,
            string workspaceKey,
            string? selectedId,
            out ViewConfigurationDefinition? deleted)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            deleted = Find(settings, workspaceKey, selectedId)?.Clone();
            if (deleted == null) return ViewConfigurationMutationResult.MissingConfiguration;
            var deletedId = deleted.Id;
            settings.ViewConfigurations = settings.ViewConfigurations
                .Where(item => item.Id != deletedId)
                .Select(item => item.Clone())
                .ToList();
            if (settings.GetActiveViewConfigurationId(workspaceKey) == deletedId)
                settings.SetActiveViewConfigurationId(workspaceKey, null);
            return ViewConfigurationMutationResult.Success;
        }

        internal static string NormalizeName(string? value)
        {
            var name = (value ?? string.Empty).Trim();
            return name.Length <= MaximumNameLength ? name : string.Empty;
        }

        private static ViewConfigurationDefinition? Find(
            GameSaveCenterSettings settings,
            string workspaceKey,
            string? selectedId)
        {
            if (string.IsNullOrWhiteSpace(selectedId)) return null;
            return settings.ViewConfigurations.FirstOrDefault(item =>
                item.Id == selectedId && item.WorkspaceKey == workspaceKey);
        }
    }
}
