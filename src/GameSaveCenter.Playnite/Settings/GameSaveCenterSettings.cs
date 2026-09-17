using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Services;
using GameSaveCenter.Playnite.Infrastructure;
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
        private int settingsSaveInProgress;
        private bool enableLocalMirror;
        private bool autoStartWorker = true;
        private bool onboardingCompleted;
        private bool enableProcessDetection = true;
        private bool enableSessionSavePathDetection = true;
        private bool enableMediaSync = true;
        private bool enableSteamMedia = true;
        private bool enableXboxGameBarMedia = true;
        private bool enableWindowsScreenshotMedia = true;
        private bool enablePlatformAdjacentMedia = true;
        private bool enableCustomMedia = true;
        private bool enableCloudUpload;
        private bool cloudUploadQueuePaused;
        private bool enableDashboardAutoRefresh = true;
        private bool enableTaskNotifications = true;
        private bool safeModeEnabled;
        private bool safeModeRequested;
        private bool enableUiAnimations = true;
        private bool enableGlassEffects = true;
        private bool followSelectedGameBackground = true;
        private bool sidebarCollapsed;
        private bool healthInspectionEnabled = true;
        private Dictionary<string, double> dataGridColumnWidths = new Dictionary<string, double>(StringComparer.Ordinal);

        /// <summary>Raised after Playnite commits the current edit buffer.</summary>
        public event EventHandler? SettingsCommitted;

        /// <summary>Raised after Playnite cancels the current edit buffer.</summary>
        public event EventHandler? SettingsReverted;

        /// <summary>Raised when Playnite begins writing the current edit buffer.</summary>
        public event EventHandler? SettingsSaveStarted;

        /// <summary>Raised when the saved settings are being applied to Worker.</summary>
        public event EventHandler? SettingsApplyStarted;

        /// <summary>Raised when the Worker accepts the saved settings.</summary>
        public event EventHandler? SettingsApplyCompleted;

        /// <summary>Raised when writing settings or applying them to Worker fails.</summary>
        public event EventHandler<SettingsSaveFailedEventArgs>? SettingsSaveFailed;

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
        public bool EnableLocalMirror { get => enableLocalMirror; set => SetBoolean(ref enableLocalMirror, value, nameof(EnableLocalMirror)); }
        public string LocalMirrorPath { get; set; } = string.Empty;
        public bool AutoStartWorker { get => autoStartWorker; set => SetBoolean(ref autoStartWorker, value, nameof(AutoStartWorker)); }
        /// <summary>Whether the first-use environment preparation card was completed or skipped.</summary>
        public bool OnboardingCompleted { get => onboardingCompleted; set => SetBoolean(ref onboardingCompleted, value, nameof(OnboardingCompleted)); }
        public bool EnableProcessDetection { get => enableProcessDetection; set => SetBoolean(ref enableProcessDetection, value, nameof(EnableProcessDetection)); }
        public bool EnableSessionSavePathDetection { get => enableSessionSavePathDetection; set => SetBoolean(ref enableSessionSavePathDetection, value, nameof(EnableSessionSavePathDetection)); }
        public bool EnableMediaSync { get => enableMediaSync; set => SetBoolean(ref enableMediaSync, value, nameof(EnableMediaSync)); }
        public bool EnableSteamMedia { get => enableSteamMedia; set => SetBoolean(ref enableSteamMedia, value, nameof(EnableSteamMedia)); }
        public bool EnableXboxGameBarMedia { get => enableXboxGameBarMedia; set => SetBoolean(ref enableXboxGameBarMedia, value, nameof(EnableXboxGameBarMedia)); }
        public bool EnableWindowsScreenshotMedia { get => enableWindowsScreenshotMedia; set => SetBoolean(ref enableWindowsScreenshotMedia, value, nameof(EnableWindowsScreenshotMedia)); }
        public bool EnablePlatformAdjacentMedia { get => enablePlatformAdjacentMedia; set => SetBoolean(ref enablePlatformAdjacentMedia, value, nameof(EnablePlatformAdjacentMedia)); }
        public bool EnableCustomMedia { get => enableCustomMedia; set => SetBoolean(ref enableCustomMedia, value, nameof(EnableCustomMedia)); }
        public bool EnableCloudUpload { get => enableCloudUpload; set => SetBoolean(ref enableCloudUpload, value, nameof(EnableCloudUpload)); }
        public bool CloudUploadQueuePaused { get => cloudUploadQueuePaused; set => SetBoolean(ref cloudUploadQueuePaused, value, nameof(CloudUploadQueuePaused)); }
        public int CloudUploadAllowedStartMinute { get; set; }
        public int CloudUploadAllowedEndMinute { get; set; } = 1440;
        public bool EnableDashboardAutoRefresh { get => enableDashboardAutoRefresh; set => SetBoolean(ref enableDashboardAutoRefresh, value, nameof(EnableDashboardAutoRefresh)); }
        public bool EnableTaskNotifications { get => enableTaskNotifications; set => SetBoolean(ref enableTaskNotifications, value, nameof(EnableTaskNotifications)); }
        public NotificationLevel NotificationLevel { get; set; } = NotificationLevel.Summary;
        public bool SafeModeEnabled { get => safeModeEnabled; set => SetBoolean(ref safeModeEnabled, value, nameof(SafeModeEnabled)); }
        public bool SafeModeRequested { get => safeModeRequested; set => SetBoolean(ref safeModeRequested, value, nameof(SafeModeRequested)); }
        public GameSaveCenterThemeMode ThemeMode { get; set; } = GameSaveCenterThemeMode.FollowPlaynite;
        public bool EnableUiAnimations { get => enableUiAnimations; set => SetBoolean(ref enableUiAnimations, value, nameof(EnableUiAnimations)); }
        public bool EnableGlassEffects { get => enableGlassEffects; set => SetBoolean(ref enableGlassEffects, value, nameof(EnableGlassEffects)); }
        public int GlassEffectStrength { get; set; } = 78;
        /// <summary>Whether the shell may decode and follow the selected game's background image.</summary>
        public bool FollowSelectedGameBackground { get => followSelectedGameBackground; set => SetBoolean(ref followSelectedGameBackground, value, nameof(FollowSelectedGameBackground)); }
        /// <summary>Whether the production shell navigation rail is currently collapsed.</summary>
        public bool SidebarCollapsed { get => sidebarCollapsed; set => SetBoolean(ref sidebarCollapsed, value, nameof(SidebarCollapsed)); }
        public int DashboardRefreshSeconds { get; set; } = 10;
        public int RecentProtectionWindowDays { get; set; } = 30;
        public int ProcessPollingSeconds { get; set; } = 5;
        public int DefaultBackupIntervalMinutes { get; set; } = 30;
        public BackupStorageFormat BackupFormat { get; set; } = BackupStorageFormat.Zip;
        public string Compression { get; set; } = "zstd";
        public int CompressionLevel { get; set; } = 3;
        public int FullBackupLimit { get; set; } = 3;
        public int DifferentialBackupLimit { get; set; } = 5;
        public bool HealthInspectionEnabled { get => healthInspectionEnabled; set => SetBoolean(ref healthInspectionEnabled, value, nameof(HealthInspectionEnabled)); }
        public int HealthInspectionIntervalMinutes { get; set; } = 1440;
        public int HealthInspectionStaleAfterDays { get; set; } = 30;
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
        public string TaskHistoryScopeState { get; set; } = "最近任务";
        public string TaskHistoryRangeState { get; set; } = "全部时间";
        public string MediaFilterState { get; set; } = "全部";
        public string MediaSearchTextState { get; set; } = string.Empty;

        /// <summary>
        /// User-adjusted DataGrid widths keyed by the versioned view/column identity.
        /// Unknown keys are intentionally retained for forward-compatible imports but
        /// are ignored by the current column layout controller.
        /// </summary>
        public Dictionary<string, double> DataGridColumnWidths
        {
            get => dataGridColumnWidths;
            set => dataGridColumnWidths = CloneDataGridColumnWidths(value);
        }

        internal bool TryGetDataGridColumnWidth(string viewKey, string columnKey, out double width)
        {
            width = 0;
            if (string.IsNullOrWhiteSpace(viewKey) || string.IsNullOrWhiteSpace(columnKey)) return false;
            return dataGridColumnWidths.TryGetValue(BuildDataGridColumnWidthKey(viewKey, columnKey), out width)
                && IsFinite(width)
                && width > 0;
        }

        internal void SetDataGridColumnWidth(string viewKey, string columnKey, double width)
        {
            if (string.IsNullOrWhiteSpace(viewKey) || string.IsNullOrWhiteSpace(columnKey) || !IsFinite(width) || width <= 0)
                return;
            dataGridColumnWidths[BuildDataGridColumnWidthKey(viewKey, columnKey)] = width;
        }

        internal void ResetDataGridColumnWidths(string viewKey)
        {
            if (string.IsNullOrWhiteSpace(viewKey)) return;
            var prefix = BuildDataGridColumnWidthKey(viewKey, string.Empty);
            foreach (var key in new List<string>(dataGridColumnWidths.Keys))
            {
                if (key.StartsWith(prefix, StringComparison.Ordinal))
                    dataGridColumnWidths.Remove(key);
            }
        }

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

        /// <summary>Returns whether Playnite currently owns an editable settings buffer.</summary>
        public bool HasPendingEdit => editingClone != null;

        /// <summary>
        /// Returns the baseline captured by Playnite for the current edit session. A detached
        /// settings view uses this value so recreating the view does not make a live draft look
        /// saved merely because the new view was bound after the user had already typed.
        /// </summary>
        public string GetEditBaselineFingerprint()
            => editingClone?.CreateSettingsFingerprint() ?? CreateSettingsFingerprint();

        public void CancelEdit()
        {
            if (editingClone == null) return;
            var clone = editingClone;
            editingClone = null;
            CopyFrom(clone);
            SettingsReverted?.Invoke(this, EventArgs.Empty);
        }

        public void EndEdit()
        {
            if (plugin == null || editingClone == null
                || Interlocked.CompareExchange(ref settingsSaveInProgress, 1, 0) != 0)
                return;

            var settingsPersisted = false;
            SettingsSaveStarted?.Invoke(this, EventArgs.Empty);
            try
            {
                plugin.SavePluginSettings(this);
                settingsPersisted = true;
                plugin.NotifyVisualSettingsChanged();
                editingClone = null;
                SettingsCommitted?.Invoke(this, EventArgs.Empty);
                SettingsApplyStarted?.Invoke(this, EventArgs.Empty);
                plugin.ApplySettingsAsync(exception =>
                {
                    try
                    {
                        if (exception == null)
                            SettingsApplyCompleted?.Invoke(this, EventArgs.Empty);
                        else
                            SettingsSaveFailed?.Invoke(this, new SettingsSaveFailedEventArgs(exception, true));
                    }
                    finally
                    {
                        Volatile.Write(ref settingsSaveInProgress, 0);
                    }
                });
            }
            catch (Exception exception)
            {
                SettingsSaveFailed?.Invoke(this, new SettingsSaveFailedEventArgs(exception, settingsPersisted));
                Volatile.Write(ref settingsSaveInProgress, 0);
                throw;
            }
        }

        /// <summary>
        /// Creates a stable comparison value for the editable settings surface. The device
        /// identity is deliberately omitted because it is installation state rather than a
        /// user-editable setting and portable imports preserve the destination identity.
        /// </summary>
        public string CreateSettingsFingerprint()
        {
            var snapshot = Clone();
            snapshot.DeviceId = string.Empty;
            return JsonConvert.SerializeObject(snapshot, Formatting.None);
        }

        internal SettingsPathValidationSnapshot CreatePathValidationSnapshot()
            => new SettingsPathValidationSnapshot(
                WorkerExecutable,
                LudusaviExecutable,
                LudusaviBackupDirectory,
                RcloneExecutable,
                MediaArchiveDirectory,
                EnableLocalMirror,
                LocalMirrorPath);

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            errors.AddRange(SettingsPathValidationService.Validate(CreatePathValidationSnapshot(), CancellationToken.None));
            AddValueRangeErrors(errors);
            return errors.Count == 0;
        }

        /// <summary>
        /// Returns only cheap value/range errors for the live editor. Path availability is
        /// supplied separately by the cancellable background validator.
        /// </summary>
        internal bool VerifySettingsWithoutPathAvailability(out List<string> errors)
        {
            errors = new List<string>();
            AddValueRangeErrors(errors);
            return errors.Count == 0;
        }

        private void AddValueRangeErrors(List<string> errors)
        {
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
            if (HealthInspectionEnabled)
            {
                if (HealthInspectionIntervalMinutes < 15 || HealthInspectionIntervalMinutes > 10080)
                    errors.Add("恢复可用性巡检间隔必须为 15–10080 分钟。");
                if (HealthInspectionStaleAfterDays < 1 || HealthInspectionStaleAfterDays > 3650)
                    errors.Add("恢复可用性验证有效期必须为 1–3650 天。");
            }
            if (CompressionLevel < -7 || CompressionLevel > 22)
                errors.Add("压缩等级必须为 -7–22；zstd 建议使用 3。");
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
            CloudUploadQueuePaused = CloudUploadQueuePaused,
            CloudUploadAllowedStartMinute = CloudUploadAllowedStartMinute,
            CloudUploadAllowedEndMinute = CloudUploadAllowedEndMinute,
            BackupFormat = BackupFormat,
            Compression = Compression,
            CompressionLevel = CompressionLevel,
            FullBackupLimit = FullBackupLimit,
            DifferentialBackupLimit = DifferentialBackupLimit,
            HealthInspectionEnabled = HealthInspectionEnabled,
            HealthInspectionIntervalMinutes = HealthInspectionIntervalMinutes,
            HealthInspectionStaleAfterDays = HealthInspectionStaleAfterDays
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

        private void SetBoolean(ref bool field, bool value, string propertyName)
        {
            if (field == value) return;
            field = value;
            OnPropertyChanged(propertyName);
        }

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
            CloudUploadQueuePaused = other.CloudUploadQueuePaused;
            CloudUploadAllowedStartMinute = other.CloudUploadAllowedStartMinute;
            CloudUploadAllowedEndMinute = other.CloudUploadAllowedEndMinute <= 0 ? 1440 : other.CloudUploadAllowedEndMinute;
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
            FollowSelectedGameBackground = other.FollowSelectedGameBackground;
            SidebarCollapsed = other.SidebarCollapsed;
            DashboardRefreshSeconds = other.DashboardRefreshSeconds;
            RecentProtectionWindowDays = RecentProtectionAssessmentService.NormalizeWindowDays(other.RecentProtectionWindowDays);
            ProcessPollingSeconds = other.ProcessPollingSeconds;
            DefaultBackupIntervalMinutes = other.DefaultBackupIntervalMinutes;
            BackupFormat = other.BackupFormat;
            Compression = other.Compression;
            CompressionLevel = other.CompressionLevel;
            FullBackupLimit = other.FullBackupLimit;
            DifferentialBackupLimit = other.DifferentialBackupLimit;
            HealthInspectionEnabled = other.HealthInspectionEnabled;
            HealthInspectionIntervalMinutes = other.HealthInspectionIntervalMinutes < 15 ? 1440 : other.HealthInspectionIntervalMinutes;
            HealthInspectionStaleAfterDays = other.HealthInspectionStaleAfterDays < 1 ? 30 : other.HealthInspectionStaleAfterDays;
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
            TaskHistoryScopeState = other.TaskHistoryScopeState == "全部历史" ? "全部历史" : "最近任务";
            TaskHistoryRangeState = IsSupportedTaskHistoryRange(other.TaskHistoryRangeState) ? other.TaskHistoryRangeState : "全部时间";
            MediaFilterState = string.IsNullOrWhiteSpace(other.MediaFilterState) ? "全部" : other.MediaFilterState;
            MediaSearchTextState = other.MediaSearchTextState ?? string.Empty;
            DataGridColumnWidths = other.DataGridColumnWidths;
        }

        private static string BuildDataGridColumnWidthKey(string viewKey, string columnKey)
            => "v1/" + viewKey.Trim() + "/" + columnKey.Trim();

        private static Dictionary<string, double> CloneDataGridColumnWidths(Dictionary<string, double>? source)
        {
            var result = new Dictionary<string, double>(StringComparer.Ordinal);
            if (source == null) return result;
            foreach (var pair in source)
            {
                if (result.Count >= 256 || string.IsNullOrWhiteSpace(pair.Key) || pair.Key.Length > 180 || !IsFinite(pair.Value) || pair.Value <= 0)
                    continue;
                result[pair.Key] = pair.Value;
            }
            return result;
        }

        private static bool IsFinite(double value)
            => !double.IsNaN(value) && !double.IsInfinity(value);

        private static bool IsSupportedTaskHistoryRange(string value)
            => value == "全部时间" || value == "今天" || value == "昨天" || value == "近7天" || value == "近30天";

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
            if (value.HealthInspectionIntervalMinutes < 15 || value.HealthInspectionIntervalMinutes > 10080) errors.Add("恢复巡检间隔超出 15–10080");
            if (value.HealthInspectionStaleAfterDays < 1 || value.HealthInspectionStaleAfterDays > 3650) errors.Add("恢复验证有效期超出 1–3650");
            if (value.CloudUploadAllowedStartMinute < 0 || value.CloudUploadAllowedStartMinute > 1439) errors.Add("云端允许时段起始分钟超出 0–1439");
            if (value.CloudUploadAllowedEndMinute < 1 || value.CloudUploadAllowedEndMinute > 1440) errors.Add("云端允许时段结束分钟超出 1–1440");
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
