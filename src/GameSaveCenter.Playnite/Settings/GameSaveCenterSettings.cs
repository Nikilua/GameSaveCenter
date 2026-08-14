using System;
using System.Collections.Generic;
using System.IO;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Services;
using Newtonsoft.Json;
using Playnite.SDK;
using Playnite.SDK.Data;

namespace GameSaveCenter.Playnite.Settings
{
    /// <summary>Serializable non-secret plugin settings.</summary>
    public sealed class GameSaveCenterSettings : ObservableObject, ISettings
    {
        private readonly GameSaveCenterPlugin? plugin;
        private GameSaveCenterSettings? editingClone;
        private string deviceId = Guid.NewGuid().ToString("N");
        private bool deviceIdWasLoaded;

        public GameSaveCenterSettings() { }

        public GameSaveCenterSettings(GameSaveCenterPlugin plugin)
        {
            this.plugin = plugin;
            var saved = plugin.LoadPluginSettings<GameSaveCenterSettings>();
            var migrateDeviceIdentity = saved != null && !saved.deviceIdWasLoaded;
            if (saved != null) CopyFrom(saved);
            if (migrateDeviceIdentity) DeviceId = Guid.NewGuid().ToString("N");
            var pluginInstallPath = Path.GetDirectoryName(typeof(GameSaveCenterPlugin).Assembly.Location) ?? plugin.GetPluginUserDataPath();
            if (EnsureDefaults(pluginInstallPath) || migrateDeviceIdentity)
                plugin.SavePluginSettings(this);
        }

        public string WorkerExecutable { get; set; } = string.Empty;
        public string DeviceId
        {
            get => deviceId;
            set
            {
                deviceId = value ?? string.Empty;
                deviceIdWasLoaded = true;
            }
        }
        public string LudusaviExecutable { get; set; } = string.Empty;
        public string LudusaviBackupDirectory { get; set; } = string.Empty;
        public string RcloneExecutable { get; set; } = string.Empty;
        public string RcloneDestination { get; set; } = string.Empty;
        public string MediaArchiveDirectory { get; set; } = string.Empty;
        public bool EnableLocalMirror { get; set; }
        public string LocalMirrorPath { get; set; } = string.Empty;
        public bool AutoStartWorker { get; set; } = true;
        /// <summary>Whether the first-use environment preparation card was completed or skipped.</summary>
        public bool OnboardingCompleted { get; set; }
        public bool EnableProcessDetection { get; set; } = true;
        public bool EnableSessionSavePathDetection { get; set; } = true;
        public bool EnableMediaSync { get; set; } = true;
        public bool EnableSteamMedia { get; set; } = true;
        public bool EnableXboxGameBarMedia { get; set; } = true;
        public bool EnableWindowsScreenshotMedia { get; set; } = true;
        public bool EnablePlatformAdjacentMedia { get; set; } = true;
        public bool EnableCustomMedia { get; set; } = true;
        public bool EnableCloudUpload { get; set; }
        public bool EnableDashboardAutoRefresh { get; set; } = true;
        public bool EnableTaskNotifications { get; set; } = true;
        public NotificationLevel NotificationLevel { get; set; } = NotificationLevel.Summary;
        public bool SafeModeEnabled { get; set; }
        public bool SafeModeRequested { get; set; }
        public GameSaveCenterThemeMode ThemeMode { get; set; } = GameSaveCenterThemeMode.FollowPlaynite;
        public bool EnableUiAnimations { get; set; } = true;
        public bool EnableGlassEffects { get; set; } = true;
        public int GlassEffectStrength { get; set; } = 78;
        public int DashboardRefreshSeconds { get; set; } = 10;
        public int RecentProtectionWindowDays { get; set; } = 30;
        public int ProcessPollingSeconds { get; set; } = 5;
        public int DefaultBackupIntervalMinutes { get; set; } = 30;
        public BackupStorageFormat BackupFormat { get; set; } = BackupStorageFormat.Zip;
        public string Compression { get; set; } = "zstd";
        public int CompressionLevel { get; set; } = 3;
        public int FullBackupLimit { get; set; } = 3;
        public int DifferentialBackupLimit { get; set; } = 5;
        // Lightweight global game-picker state. These values are UI preferences only;
        // game data remains in the Worker/SQLite cache.
        public string GamePickerSearchText { get; set; } = string.Empty;
        public string GamePickerStatusFilter { get; set; } = "已安装";
        public string GamePickerPlatformFilter { get; set; } = "全部";
        public string GamePickerSortMode { get; set; } = "名称";
        public string GamePickerSelectedGameId { get; set; } = string.Empty;
        public string LastWorkspace { get; set; } = "Overview";
        public string TaskStatusFilterState { get; set; } = "全部";
        public string TaskGameFilterState { get; set; } = "全部";
        public string TaskTypeFilterState { get; set; } = "全部";
        public string TaskSearchTextState { get; set; } = string.Empty;
        public string MediaFilterState { get; set; } = "全部";
        public string MediaSearchTextState { get; set; } = string.Empty;

        public string ExportPortableJson()
        {
            var portable = Clone();
            // Device identity belongs to this installation. Importing it on another PC
            // would collapse two independent branches into one cloud namespace.
            portable.DeviceId = string.Empty;
            var package = new PortableSettingsPackage
            {
                SchemaVersion = 1,
                ExportedUtc = DateTime.UtcNow,
                Settings = portable
            };
            return JsonConvert.SerializeObject(package, Formatting.Indented);
        }

        public SettingsImportReport ImportPortableJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new InvalidDataException("设置文件为空。");
            if (json.Length > 1024 * 1024) throw new InvalidDataException("设置文件超过 1 MiB 安全上限。");
            var package = JsonConvert.DeserializeObject<PortableSettingsPackage>(json)
                          ?? throw new InvalidDataException("设置文件格式无效。");
            if (package.SchemaVersion != 1) throw new InvalidDataException($"不支持的设置架构版本：{package.SchemaVersion}。");
            var imported = package.Settings ?? throw new InvalidDataException("设置文件不包含 settings 节点。");
            var valueErrors = ValidateValueRanges(imported);
            if (valueErrors.Count > 0) throw new InvalidDataException("设置值无效：" + string.Join("；", valueErrors));

            CopyFrom(imported);
            var report = new SettingsImportReport { SchemaVersion = package.SchemaVersion, ExportedUtc = package.ExportedUtc };
            AddMissingFile(report, "Worker", WorkerExecutable);
            AddMissingFile(report, "Ludusavi", LudusaviExecutable);
            AddMissingFile(report, "Rclone", RcloneExecutable);
            AddMissingDirectory(report, "存档目录", LudusaviBackupDirectory);
            AddMissingDirectory(report, "媒体目录", MediaArchiveDirectory);
            if (EnableLocalMirror) AddMissingDirectory(report, "本地镜像", LocalMirrorPath);
            return report;
        }

        /// <summary>
        /// Validates a portable settings package on a detached instance so metadata restore
        /// can reject bad plugin settings before the Worker replaces the database.
        /// </summary>
        public static void ValidatePortableJson(string json)
        {
            var validator = new GameSaveCenterSettings();
            validator.ImportPortableJson(json);
        }

        public void BeginEdit() => editingClone = Clone();

        public void CancelEdit()
        {
            if (editingClone != null) CopyFrom(editingClone);
        }

        public void EndEdit()
        {
            if (plugin == null) return;
            plugin.SavePluginSettings(this);
            plugin.NotifyVisualSettingsChanged();
            plugin.ApplySettingsAsync();
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            if (string.IsNullOrWhiteSpace(WorkerExecutable) || !File.Exists(Environment.ExpandEnvironmentVariables(WorkerExecutable)))
                errors.Add("未找到 GameSaveCenter Worker。请先运行打包脚本，或选择正确的 Worker 可执行文件。");
            else if (!IsWorkerExecutable(WorkerExecutable))
                errors.Add("Worker 路径必须指向 GameSaveCenter.Worker.exe，不能选择 Ludusavi 或其他程序。");
            if (EnableLocalMirror && string.IsNullOrWhiteSpace(LocalMirrorPath))
                errors.Add("启用本地镜像时必须填写镜像目录。");
            if (!string.IsNullOrWhiteSpace(LudusaviExecutable) && !File.Exists(Environment.ExpandEnvironmentVariables(LudusaviExecutable)))
                errors.Add("Ludusavi 路径不存在。");
            if (!string.IsNullOrWhiteSpace(RcloneExecutable) && !File.Exists(Environment.ExpandEnvironmentVariables(RcloneExecutable)))
                errors.Add("Rclone 路径不存在。");
            if (DefaultBackupIntervalMinutes < 1 || DefaultBackupIntervalMinutes > 1440)
                errors.Add("定时备份间隔必须为 1–1440 分钟。");
            if (ProcessPollingSeconds < 2 || ProcessPollingSeconds > 60)
                errors.Add("进程检测间隔必须为 2–60 秒。");
            if (DashboardRefreshSeconds < 5 || DashboardRefreshSeconds > 300)
                errors.Add("管理面板自动刷新间隔必须为 5–300 秒。");
            if (!RecentProtectionAssessmentService.IsSupportedWindowDays(RecentProtectionWindowDays))
                errors.Add("最近保护统计窗口必须为 7、30 或 90 天。");
            if (GlassEffectStrength < 20 || GlassEffectStrength > 100)
                errors.Add("毛玻璃强度必须为 20–100。");
            if (FullBackupLimit < 1 || FullBackupLimit > 255)
                errors.Add("完整备份保留数量必须为 1–255。");
            if (DifferentialBackupLimit < 0 || DifferentialBackupLimit > 255)
                errors.Add("差异备份保留数量必须为 0–255。");
            if (CompressionLevel < -7 || CompressionLevel > 22)
                errors.Add("压缩等级必须为 -7–22；zstd 建议使用 3。");
            return errors.Count == 0;
        }

        public WorkerSettingsDto ToWorkerSettings() => new WorkerSettingsDto
        {
            DeviceId = DeviceId,
            SafeModeEnabled = SafeModeEnabled,
            SafeModeRequested = SafeModeRequested,
            LudusaviExecutable = Expand(LudusaviExecutable),
            LudusaviBackupDirectory = Expand(LudusaviBackupDirectory),
            RcloneExecutable = Expand(RcloneExecutable),
            RcloneDestination = RcloneDestination ?? string.Empty,
            MediaArchiveDirectory = Expand(MediaArchiveDirectory),
            EnableLocalMirror = EnableLocalMirror,
            LocalMirrorPath = Expand(LocalMirrorPath),
            ProcessPollingSeconds = ProcessPollingSeconds,
            DefaultBackupIntervalMinutes = DefaultBackupIntervalMinutes,
            EnableProcessDetection = EnableProcessDetection,
            EnableSessionSavePathDetection = EnableSessionSavePathDetection,
            EnableMediaSync = EnableMediaSync,
            EnableSteamMedia = EnableSteamMedia,
            EnableXboxGameBarMedia = EnableXboxGameBarMedia,
            EnableWindowsScreenshotMedia = EnableWindowsScreenshotMedia,
            EnablePlatformAdjacentMedia = EnablePlatformAdjacentMedia,
            EnableCustomMedia = EnableCustomMedia,
            EnableCloudUpload = EnableCloudUpload,
            BackupFormat = BackupFormat,
            Compression = Compression,
            CompressionLevel = CompressionLevel,
            FullBackupLimit = FullBackupLimit,
            DifferentialBackupLimit = DifferentialBackupLimit
        };

        private bool EnsureDefaults(string pluginInstallPath)
        {
            var changed = false;
            if (!Guid.TryParseExact(DeviceId, "N", out _))
            {
                DeviceId = Guid.NewGuid().ToString("N");
                changed = true;
            }
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var pictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            var packagedWorker = Path.Combine(pluginInstallPath, "Worker", "GameSaveCenter.Worker.exe");
            if (!string.IsNullOrWhiteSpace(WorkerExecutable) &&
                !IsWorkerExecutable(WorkerExecutable) &&
                string.IsNullOrWhiteSpace(LudusaviExecutable) &&
                IsLudusaviExecutable(WorkerExecutable) &&
                File.Exists(Expand(WorkerExecutable)))
            {
                // Repair the 0.4.2 settings mix-up without losing the user's valid Ludusavi path.
                LudusaviExecutable = WorkerExecutable;
                changed = true;
            }
            if (string.IsNullOrWhiteSpace(WorkerExecutable) || !IsWorkerExecutable(WorkerExecutable))
            {
                WorkerExecutable = packagedWorker;
                changed = true;
            }
            if (string.IsNullOrWhiteSpace(LudusaviBackupDirectory))
            {
                LudusaviBackupDirectory = Path.Combine(documents, "GameSaveCenter", "Saves");
                changed = true;
            }
            if (string.IsNullOrWhiteSpace(MediaArchiveDirectory))
            {
                MediaArchiveDirectory = Path.Combine(pictures, "GameSaveCenter");
                changed = true;
            }
            return changed;
        }

        private GameSaveCenterSettings Clone() => JsonConvert.DeserializeObject<GameSaveCenterSettings>(JsonConvert.SerializeObject(this)) ?? new GameSaveCenterSettings();

        private void CopyFrom(GameSaveCenterSettings other)
        {
            WorkerExecutable = other.WorkerExecutable;
            if (other.deviceIdWasLoaded && Guid.TryParseExact(other.DeviceId, "N", out _))
                DeviceId = other.DeviceId;
            LudusaviExecutable = other.LudusaviExecutable;
            LudusaviBackupDirectory = other.LudusaviBackupDirectory;
            RcloneExecutable = other.RcloneExecutable;
            RcloneDestination = other.RcloneDestination;
            MediaArchiveDirectory = other.MediaArchiveDirectory;
            EnableLocalMirror = other.EnableLocalMirror;
            LocalMirrorPath = other.LocalMirrorPath;
            AutoStartWorker = other.AutoStartWorker;
            OnboardingCompleted = other.OnboardingCompleted;
            EnableProcessDetection = other.EnableProcessDetection;
            EnableSessionSavePathDetection = other.EnableSessionSavePathDetection;
            EnableMediaSync = other.EnableMediaSync;
            EnableSteamMedia = other.EnableSteamMedia;
            EnableXboxGameBarMedia = other.EnableXboxGameBarMedia;
            EnableWindowsScreenshotMedia = other.EnableWindowsScreenshotMedia;
            EnablePlatformAdjacentMedia = other.EnablePlatformAdjacentMedia;
            EnableCustomMedia = other.EnableCustomMedia;
            EnableCloudUpload = other.EnableCloudUpload;
            EnableDashboardAutoRefresh = other.EnableDashboardAutoRefresh;
            EnableTaskNotifications = other.EnableTaskNotifications;
            NotificationLevel = Enum.IsDefined(typeof(NotificationLevel), other.NotificationLevel)
                ? other.NotificationLevel
                : NotificationLevel.Summary;
            SafeModeEnabled = other.SafeModeEnabled;
            SafeModeRequested = other.SafeModeRequested;
            ThemeMode = other.ThemeMode;
            EnableUiAnimations = other.EnableUiAnimations;
            EnableGlassEffects = other.EnableGlassEffects;
            GlassEffectStrength = other.GlassEffectStrength <= 0 ? 78 : other.GlassEffectStrength;
            DashboardRefreshSeconds = other.DashboardRefreshSeconds;
            RecentProtectionWindowDays = RecentProtectionAssessmentService.NormalizeWindowDays(other.RecentProtectionWindowDays);
            ProcessPollingSeconds = other.ProcessPollingSeconds;
            DefaultBackupIntervalMinutes = other.DefaultBackupIntervalMinutes;
            BackupFormat = other.BackupFormat;
            Compression = other.Compression;
            CompressionLevel = other.CompressionLevel;
            FullBackupLimit = other.FullBackupLimit;
            DifferentialBackupLimit = other.DifferentialBackupLimit;
            GamePickerSearchText = other.GamePickerSearchText ?? string.Empty;
            GamePickerStatusFilter = string.IsNullOrWhiteSpace(other.GamePickerStatusFilter) ? "已安装" : other.GamePickerStatusFilter;
            GamePickerPlatformFilter = string.IsNullOrWhiteSpace(other.GamePickerPlatformFilter) ? "全部" : other.GamePickerPlatformFilter;
            GamePickerSortMode = string.IsNullOrWhiteSpace(other.GamePickerSortMode) ? "名称" : other.GamePickerSortMode;
            GamePickerSelectedGameId = other.GamePickerSelectedGameId ?? string.Empty;
            LastWorkspace = string.IsNullOrWhiteSpace(other.LastWorkspace) ? "Overview" : other.LastWorkspace;
            TaskStatusFilterState = string.IsNullOrWhiteSpace(other.TaskStatusFilterState) ? "全部" : other.TaskStatusFilterState;
            TaskGameFilterState = string.IsNullOrWhiteSpace(other.TaskGameFilterState) ? "全部" : other.TaskGameFilterState;
            TaskTypeFilterState = string.IsNullOrWhiteSpace(other.TaskTypeFilterState) ? "全部" : other.TaskTypeFilterState;
            TaskSearchTextState = other.TaskSearchTextState ?? string.Empty;
            MediaFilterState = string.IsNullOrWhiteSpace(other.MediaFilterState) ? "全部" : other.MediaFilterState;
            MediaSearchTextState = other.MediaSearchTextState ?? string.Empty;
        }

        private static List<string> ValidateValueRanges(GameSaveCenterSettings value)
        {
            var errors = new List<string>();
            if (value.DefaultBackupIntervalMinutes < 1 || value.DefaultBackupIntervalMinutes > 1440) errors.Add("备份间隔超出 1–1440");
            if (value.ProcessPollingSeconds < 2 || value.ProcessPollingSeconds > 60) errors.Add("进程检测间隔超出 2–60");
            if (value.DashboardRefreshSeconds < 5 || value.DashboardRefreshSeconds > 300) errors.Add("面板刷新间隔超出 5–300");
            if (!RecentProtectionAssessmentService.IsSupportedWindowDays(value.RecentProtectionWindowDays)) errors.Add("最近保护窗口必须为 7、30 或 90");
            if (value.GlassEffectStrength < 20 || value.GlassEffectStrength > 100) errors.Add("毛玻璃强度超出 20–100");
            if (value.FullBackupLimit < 1 || value.FullBackupLimit > 255) errors.Add("完整版本数超出 1–255");
            if (value.DifferentialBackupLimit < 0 || value.DifferentialBackupLimit > 255) errors.Add("差异版本数超出 0–255");
            if (value.CompressionLevel < -7 || value.CompressionLevel > 22) errors.Add("压缩等级超出 -7–22");
            if (!Enum.IsDefined(typeof(GameSaveCenterThemeMode), value.ThemeMode)) errors.Add("未知主题模式");
            if (!Enum.IsDefined(typeof(BackupStorageFormat), value.BackupFormat)) errors.Add("未知备份格式");
            if (!Enum.IsDefined(typeof(NotificationLevel), value.NotificationLevel)) errors.Add("未知通知级别");
            return errors;
        }

        private static void AddMissingFile(SettingsImportReport report, string label, string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && !File.Exists(Expand(path))) report.MissingPaths.Add($"{label}：{path}");
        }

        private static void AddMissingDirectory(SettingsImportReport report, string label, string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && !Directory.Exists(Expand(path))) report.MissingPaths.Add($"{label}：{path}");
        }

        private static string Expand(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : Environment.ExpandEnvironmentVariables(value);

        internal static bool IsWorkerExecutable(string value)
            => string.Equals(Path.GetFileName(Expand(value)), "GameSaveCenter.Worker.exe", StringComparison.OrdinalIgnoreCase);

        private static bool IsLudusaviExecutable(string value)
            => string.Equals(Path.GetFileName(Expand(value)), "ludusavi.exe", StringComparison.OrdinalIgnoreCase);

        private sealed class PortableSettingsPackage
        {
            public int SchemaVersion { get; set; }
            public DateTime ExportedUtc { get; set; }
            public GameSaveCenterSettings? Settings { get; set; }
        }
    }

    public sealed class SettingsImportReport
    {
        public int SchemaVersion { get; set; }
        public DateTime ExportedUtc { get; set; }
        public List<string> MissingPaths { get; } = new List<string>();
        public string Summary => MissingPaths.Count == 0
            ? "设置已载入，未发现缺失路径。点击 Playnite 的保存按钮后生效。"
            : $"设置已载入，但有 {MissingPaths.Count} 个路径需要重新选择：\n" + string.Join("\n", MissingPaths);
    }
}
