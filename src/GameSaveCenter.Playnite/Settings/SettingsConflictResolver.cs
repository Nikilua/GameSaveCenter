using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameSaveCenter.Playnite.Settings
{
    /// <summary>
    /// Performs a three-way merge between the edit baseline, the current draft and the
    /// latest persisted settings. This keeps conflict policy independent from Playnite UI.
    /// </summary>
    internal static class SettingsConflictResolver
    {
        internal static SettingsConflictResolution Merge(
            GameSaveCenterSettings baseline,
            GameSaveCenterSettings draft,
            GameSaveCenterSettings persisted)
        {
            if (baseline == null) throw new ArgumentNullException(nameof(baseline));
            if (draft == null) throw new ArgumentNullException(nameof(draft));
            if (persisted == null) throw new ArgumentNullException(nameof(persisted));

            var conflicts = new List<SettingsConflictField>();
            var externalChanges = new List<SettingsConflictField>();
            var valuesToMerge = new List<PropertyValue>();
            foreach (var property in GetEditableProperties())
            {
                var baselineValue = property.GetValue(baseline, null);
                var draftValue = property.GetValue(draft, null);
                var persistedValue = property.GetValue(persisted, null);
                var draftChanged = !AreEquivalent(baselineValue, draftValue);
                var persistedChanged = !AreEquivalent(baselineValue, persistedValue);
                if (!persistedChanged) continue;

                var field = new SettingsConflictField(property.Name, SettingsConflictCatalog.GetDisplayName(property.Name));
                if (!draftChanged)
                {
                    externalChanges.Add(field);
                    valuesToMerge.Add(new PropertyValue(property, persistedValue));
                }
                else if (!AreEquivalent(draftValue, persistedValue))
                {
                    conflicts.Add(field);
                }
            }

            // Do not partially merge a draft when a conflict exists. The caller can show
            // the complete conflict set, then keep the user's current values for review.
            if (conflicts.Count == 0)
            {
                foreach (var value in valuesToMerge)
                    value.Property.SetValue(draft, CloneValue(value.Value, value.Property.PropertyType), null);
            }

            return new SettingsConflictResolution(conflicts, externalChanges);
        }

        private static IEnumerable<PropertyInfo> GetEditableProperties()
        {
            foreach (var property in typeof(GameSaveCenterSettings).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!property.CanRead || !property.CanWrite || property.Name == nameof(GameSaveCenterSettings.DeviceId)
                    || property.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Length != 0)
                    continue;
                yield return property;
            }
        }

        private static bool AreEquivalent(object? left, object? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left == null || right == null) return false;
            try
            {
                return JToken.DeepEquals(JToken.FromObject(left), JToken.FromObject(right));
            }
            catch
            {
                return Equals(left, right);
            }
        }

        private static object? CloneValue(object? value, Type valueType)
        {
            if (value == null) return null;
            try
            {
                return JToken.FromObject(value).ToObject(valueType);
            }
            catch
            {
                return value;
            }
        }

        private sealed class PropertyValue
        {
            internal PropertyValue(PropertyInfo property, object? value)
            {
                Property = property;
                Value = value;
            }

            internal PropertyInfo Property { get; }
            internal object? Value { get; }
        }
    }

    public sealed class SettingsConflictResolution
    {
        internal SettingsConflictResolution(
            IEnumerable<SettingsConflictField> conflicts,
            IEnumerable<SettingsConflictField> externalChanges)
        {
            Conflicts = new List<SettingsConflictField>(conflicts);
            ExternalChanges = new List<SettingsConflictField>(externalChanges);
        }

        public IReadOnlyList<SettingsConflictField> Conflicts { get; }
        public IReadOnlyList<SettingsConflictField> ExternalChanges { get; }
        public bool HasConflicts => Conflicts.Count > 0;
        public bool HasExternalChanges => ExternalChanges.Count > 0;
    }

    public sealed class SettingsConflictField
    {
        internal SettingsConflictField(string propertyName, string displayName)
        {
            PropertyName = propertyName;
            DisplayName = displayName;
        }

        public string PropertyName { get; }
        public string DisplayName { get; }
    }

    public sealed class SettingsConflictDetectedEventArgs : EventArgs
    {
        public SettingsConflictDetectedEventArgs(SettingsConflictResolution resolution)
        {
            Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        }

        public SettingsConflictResolution Resolution { get; }
        public string Summary => "设置保存冲突：后台或另一个设置入口已修改字段「"
                                 + string.Join("、", Resolution.Conflicts.Select(field => field.DisplayName))
                                 + "」。当前草稿未写入，请检查后再次保存或取消。";
    }

    public sealed class SettingsConflictException : InvalidOperationException
    {
        public SettingsConflictException(string message) : base(message) { }
    }

    internal static class SettingsConflictCatalog
    {
        internal static string GetDisplayName(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(GameSaveCenterSettings.WorkerExecutable): return "Worker 程序";
                case nameof(GameSaveCenterSettings.LudusaviExecutable): return "Ludusavi 程序";
                case nameof(GameSaveCenterSettings.LudusaviBackupDirectory): return "存档目录";
                case nameof(GameSaveCenterSettings.RcloneExecutable): return "Rclone 程序";
                case nameof(GameSaveCenterSettings.RcloneDestination): return "云端目标";
                case nameof(GameSaveCenterSettings.MediaArchiveDirectory): return "媒体目录";
                case nameof(GameSaveCenterSettings.LocalMirrorPath): return "本地镜像目录";
                case nameof(GameSaveCenterSettings.ThemeMode): return "主题";
                case nameof(GameSaveCenterSettings.BackupFormat): return "备份格式";
                case nameof(GameSaveCenterSettings.EnableCloudUpload): return "云端上传";
                default: return propertyName;
            }
        }
    }
}
