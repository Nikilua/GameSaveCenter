using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameSaveCenter.Playnite.Settings
{
    /// <summary>
    /// Recovers plugin startup when a persisted view preference has a shape that the
    /// current settings model cannot read. The original file is kept as a backup before
    /// the recovered settings are allowed to be written back.
    /// </summary>
    internal static class PluginSettingsLoadRecovery
    {
        private static readonly string[] ViewPreferenceProperties =
        {
            "ViewConfigurations",
            "ActiveViewConfigurationIds",
            "DefaultTableDensityMode",
            "DataGridColumnWidths",
            "DataGridHiddenColumnKeys"
        };

        internal static PluginSettingsLoadRecoveryResult Load(
            Func<GameSaveCenterSettings?> load,
            string settingsPath)
        {
            if (load == null) throw new ArgumentNullException(nameof(load));

            try
            {
                return new PluginSettingsLoadRecoveryResult(load(), false, true, null, string.Empty, null);
            }
            catch (Exception loadException)
            {
                return Recover(settingsPath, loadException);
            }
        }

        private static PluginSettingsLoadRecoveryResult Recover(string settingsPath, Exception loadException)
        {
            string? json = null;
            bool originalExists;
            try
            {
                originalExists = File.Exists(settingsPath);
                if (originalExists)
                    json = File.ReadAllText(settingsPath);
            }
            catch (Exception readException)
            {
                return new PluginSettingsLoadRecoveryResult(
                    new GameSaveCenterSettings(), true, false,
                    new AggregateException(loadException, readException),
                    "Saved plugin settings could not be read after deserialization failed; defaults are active and the original file was left untouched.",
                    null);
            }

            if (!originalExists)
            {
                return new PluginSettingsLoadRecoveryResult(
                    new GameSaveCenterSettings(), true, true, loadException,
                    "Saved plugin settings could not be loaded and no config file was present; defaults are active.",
                    null);
            }

            var recovered = RecoverJson(json ?? string.Empty);
            string? backupPath = null;
            Exception? recoveryException = recovered.Exception == null
                ? loadException
                : new AggregateException(loadException, recovered.Exception);
            var canPersistRecoveredSettings = false;
            try
            {
                backupPath = CreateBackupPath(settingsPath);
                File.Copy(settingsPath, backupPath, false);
                canPersistRecoveredSettings = true;
            }
            catch (Exception backupException)
            {
                recoveryException = new AggregateException(recoveryException!, backupException);
                recovered.Diagnostic += " The original config could not be backed up, so it will not be overwritten automatically.";
            }

            if (!string.IsNullOrEmpty(backupPath) && canPersistRecoveredSettings)
                recovered.Diagnostic += " Original file backed up as " + Path.GetFileName(backupPath) + ".";

            return new PluginSettingsLoadRecoveryResult(
                recovered.Settings,
                true,
                canPersistRecoveredSettings,
                recoveryException,
                recovered.Diagnostic,
                backupPath);
        }

        private static RecoveredJson RecoverJson(string json)
        {
            JToken parsed;
            try
            {
                parsed = JToken.Parse(json);
            }
            catch (JsonException parseException)
            {
                return new RecoveredJson(
                    new GameSaveCenterSettings(),
                    "Saved config JSON is malformed; defaults are active.",
                    parseException);
            }

            if (!(parsed is JObject root))
            {
                return new RecoveredJson(
                    new GameSaveCenterSettings(),
                    "Saved config JSON does not contain a settings object; defaults are active.",
                    null);
            }

            var discarded = new List<string>();
            foreach (var propertyName in ViewPreferenceProperties)
            {
                var property = root.Properties().FirstOrDefault(item =>
                    string.Equals(item.Name, propertyName, StringComparison.OrdinalIgnoreCase));
                if (property == null) continue;
                discarded.Add(property.Name);
                property.Remove();
            }

            try
            {
                var settings = JsonConvert.DeserializeObject<GameSaveCenterSettings>(root.ToString(Formatting.None));
                if (settings == null)
                    throw new JsonSerializationException("Settings object deserialized to null.");

                var diagnostic = discarded.Count == 0
                    ? "Saved config contains invalid non-view settings; defaults are active."
                    : "Invalid view preferences were discarded: " + string.Join(", ", discarded) + ".";
                return new RecoveredJson(settings, diagnostic, null);
            }
            catch (Exception recoveryException)
            {
                return new RecoveredJson(
                    new GameSaveCenterSettings(),
                    "Saved config contains invalid non-view settings; defaults are active.",
                    recoveryException);
            }
        }

        private static string CreateBackupPath(string settingsPath)
            => settingsPath + ".recovery-" + DateTime.UtcNow.ToString("yyyyMMddTHHmmssfffZ") + "-" + Guid.NewGuid().ToString("N") + ".bak";

        private sealed class RecoveredJson
        {
            internal RecoveredJson(GameSaveCenterSettings settings, string diagnostic, Exception? exception)
            {
                Settings = settings;
                Diagnostic = diagnostic;
                Exception = exception;
            }

            internal GameSaveCenterSettings Settings { get; }
            internal string Diagnostic { get; set; }
            internal Exception? Exception { get; }
        }
    }

    internal sealed class PluginSettingsLoadRecoveryResult
    {
        internal PluginSettingsLoadRecoveryResult(
            GameSaveCenterSettings? settings,
            bool wasRecovered,
            bool canPersistRecoveredSettings,
            Exception? exception,
            string diagnostic,
            string? backupPath)
        {
            Settings = settings;
            WasRecovered = wasRecovered;
            CanPersistRecoveredSettings = canPersistRecoveredSettings;
            Exception = exception;
            Diagnostic = diagnostic;
            BackupPath = backupPath;
        }

        internal GameSaveCenterSettings? Settings { get; }
        internal bool WasRecovered { get; }
        internal bool CanPersistRecoveredSettings { get; }
        internal Exception? Exception { get; }
        internal string Diagnostic { get; }
        internal string? BackupPath { get; }
    }
}
