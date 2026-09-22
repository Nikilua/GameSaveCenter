using System;
using System.Collections.Generic;
using System.Linq;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.Settings
{
    public enum SettingsResetCategory
    {
        General,
        BackupRestore,
        Appearance,
        AutomationMedia
    }

    public sealed class SettingsResetFieldOption
    {
        public SettingsResetFieldOption(string key, string label, SettingsResetCategory category, string defaultDisplay)
        {
            Key = key;
            Label = label;
            Category = category;
            DefaultDisplay = defaultDisplay;
        }

        public string Key { get; }
        public string Label { get; }
        public SettingsResetCategory Category { get; }
        public string CategoryDisplay => SettingsResetCatalog.GetCategoryDisplay(Category);
        public string DefaultDisplay { get; }
        public string DisplayName => $"{Label}（默认：{DefaultDisplay}）";
    }

    public static class SettingsResetCatalog
    {
        private static readonly IReadOnlyList<SettingsResetFieldOption> fieldOptions = new[]
        {
            new SettingsResetFieldOption("EnableLocalMirror", "本地镜像开关", SettingsResetCategory.General, "关闭"),
            new SettingsResetFieldOption("OnboardingCompleted", "首次使用引导状态", SettingsResetCategory.General, "未完成"),
            new SettingsResetFieldOption("BackupFormat", "备份格式", SettingsResetCategory.BackupRestore, "ZIP（多版本）"),
            new SettingsResetFieldOption("Compression", "压缩方式", SettingsResetCategory.BackupRestore, "Zstandard"),
            new SettingsResetFieldOption("CompressionLevel", "压缩等级", SettingsResetCategory.BackupRestore, "3"),
            new SettingsResetFieldOption("FullBackupLimit", "完整版本数", SettingsResetCategory.BackupRestore, "3"),
            new SettingsResetFieldOption("DifferentialBackupLimit", "每组差异版本数", SettingsResetCategory.BackupRestore, "5"),
            new SettingsResetFieldOption("ThemeMode", "界面主题", SettingsResetCategory.Appearance, "跟随 Playnite"),
            new SettingsResetFieldOption("EnableUiAnimations", "界面动画", SettingsResetCategory.Appearance, "开启"),
            new SettingsResetFieldOption("EnableGlassEffects", "毛玻璃与环境光", SettingsResetCategory.Appearance, "开启"),
            new SettingsResetFieldOption("GlassEffectStrength", "毛玻璃强度", SettingsResetCategory.Appearance, "78"),
            new SettingsResetFieldOption("FollowSelectedGameBackground", "跟随当前游戏背景", SettingsResetCategory.Appearance, "开启"),
            new SettingsResetFieldOption("SidebarCollapsed", "侧栏折叠", SettingsResetCategory.Appearance, "展开"),
            new SettingsResetFieldOption("AutoStartWorker", "随 Playnite 启动 Worker", SettingsResetCategory.AutomationMedia, "开启"),
            new SettingsResetFieldOption("EnableProcessDetection", "外部进程检测", SettingsResetCategory.AutomationMedia, "开启"),
            new SettingsResetFieldOption("EnableSessionSavePathDetection", "会话存档路径检测", SettingsResetCategory.AutomationMedia, "开启"),
            new SettingsResetFieldOption("EnableMediaSync", "媒体同步", SettingsResetCategory.AutomationMedia, "开启"),
            new SettingsResetFieldOption("EnableSteamMedia", "Steam 截图来源", SettingsResetCategory.AutomationMedia, "开启"),
            new SettingsResetFieldOption("EnableXboxGameBarMedia", "Xbox Game Bar 来源", SettingsResetCategory.AutomationMedia, "开启"),
            new SettingsResetFieldOption("EnableWindowsScreenshotMedia", "Windows 截图来源", SettingsResetCategory.AutomationMedia, "开启"),
            new SettingsResetFieldOption("EnablePlatformAdjacentMedia", "游戏相邻目录来源", SettingsResetCategory.AutomationMedia, "开启"),
            new SettingsResetFieldOption("EnableCustomMedia", "自定义媒体来源", SettingsResetCategory.AutomationMedia, "开启"),
            new SettingsResetFieldOption("EnableCloudUpload", "云端上传", SettingsResetCategory.AutomationMedia, "关闭"),
            new SettingsResetFieldOption("CloudUploadQueuePaused", "暂停云端自动重试", SettingsResetCategory.AutomationMedia, "关闭"),
            new SettingsResetFieldOption("CloudUploadAllowedStartMinute", "云端允许时段起始", SettingsResetCategory.AutomationMedia, "0"),
            new SettingsResetFieldOption("CloudUploadAllowedEndMinute", "云端允许时段结束", SettingsResetCategory.AutomationMedia, "1440"),
            new SettingsResetFieldOption("EnableDashboardAutoRefresh", "管理面板自动刷新", SettingsResetCategory.AutomationMedia, "开启"),
            new SettingsResetFieldOption("EnableTaskNotifications", "任务通知", SettingsResetCategory.AutomationMedia, "开启"),
            new SettingsResetFieldOption("NotificationLevel", "通知级别", SettingsResetCategory.AutomationMedia, "退出摘要"),
            new SettingsResetFieldOption("SafeModeEnabled", "安全模式", SettingsResetCategory.AutomationMedia, "关闭"),
            new SettingsResetFieldOption("SafeModeRequested", "下次以安全模式启动", SettingsResetCategory.AutomationMedia, "关闭"),
            new SettingsResetFieldOption("DashboardRefreshSeconds", "管理面板刷新间隔", SettingsResetCategory.AutomationMedia, "10 秒"),
            new SettingsResetFieldOption("RecentProtectionWindowDays", "最近保护统计窗口", SettingsResetCategory.AutomationMedia, "30 天"),
            new SettingsResetFieldOption("ProcessPollingSeconds", "进程检测间隔", SettingsResetCategory.AutomationMedia, "5 秒"),
            new SettingsResetFieldOption("DefaultBackupIntervalMinutes", "默认游玩中备份间隔", SettingsResetCategory.AutomationMedia, "30 分钟"),
            new SettingsResetFieldOption("HealthInspectionEnabled", "恢复可用性巡检", SettingsResetCategory.AutomationMedia, "开启"),
            new SettingsResetFieldOption("HealthInspectionIntervalMinutes", "恢复巡检间隔", SettingsResetCategory.AutomationMedia, "1440 分钟"),
            new SettingsResetFieldOption("HealthInspectionStaleAfterDays", "重新验证有效期", SettingsResetCategory.AutomationMedia, "30 天")
        };

        public static IReadOnlyList<SettingsResetFieldOption> Fields => fieldOptions;

        public static string GetCategoryDisplay(SettingsResetCategory category)
            => category switch
            {
                SettingsResetCategory.General => "常规与目录",
                SettingsResetCategory.BackupRestore => "备份与恢复",
                SettingsResetCategory.Appearance => "外观与可访问性",
                SettingsResetCategory.AutomationMedia => "自动化与媒体",
                _ => "设置"
            };

        public static SettingsResetFieldOption? FindField(string key)
            => fieldOptions.FirstOrDefault(option => string.Equals(option.Key, key, StringComparison.Ordinal));

        public static bool ResetField(GameSaveCenterSettings settings, string key)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            switch (key)
            {
                case "EnableLocalMirror": settings.EnableLocalMirror = false; return true;
                case "OnboardingCompleted": settings.OnboardingCompleted = false; return true;
                case "BackupFormat": settings.BackupFormat = BackupStorageFormat.Zip; return true;
                case "Compression": settings.Compression = "zstd"; return true;
                case "CompressionLevel": settings.CompressionLevel = 3; return true;
                case "FullBackupLimit": settings.FullBackupLimit = 3; return true;
                case "DifferentialBackupLimit": settings.DifferentialBackupLimit = 5; return true;
                case "ThemeMode": settings.ThemeMode = GameSaveCenterThemeMode.FollowPlaynite; return true;
                case "EnableUiAnimations": settings.EnableUiAnimations = true; return true;
                case "EnableGlassEffects": settings.EnableGlassEffects = true; return true;
                case "GlassEffectStrength": settings.GlassEffectStrength = 78; return true;
                case "FollowSelectedGameBackground": settings.FollowSelectedGameBackground = true; return true;
                case "SidebarCollapsed": settings.SidebarCollapsed = false; return true;
                case "AutoStartWorker": settings.AutoStartWorker = true; return true;
                case "EnableProcessDetection": settings.EnableProcessDetection = true; return true;
                case "EnableSessionSavePathDetection": settings.EnableSessionSavePathDetection = true; return true;
                case "EnableMediaSync": settings.EnableMediaSync = true; return true;
                case "EnableSteamMedia": settings.EnableSteamMedia = true; return true;
                case "EnableXboxGameBarMedia": settings.EnableXboxGameBarMedia = true; return true;
                case "EnableWindowsScreenshotMedia": settings.EnableWindowsScreenshotMedia = true; return true;
                case "EnablePlatformAdjacentMedia": settings.EnablePlatformAdjacentMedia = true; return true;
                case "EnableCustomMedia": settings.EnableCustomMedia = true; return true;
                case "EnableCloudUpload": settings.EnableCloudUpload = false; return true;
                case "CloudUploadQueuePaused": settings.CloudUploadQueuePaused = false; return true;
                case "CloudUploadAllowedStartMinute": settings.CloudUploadAllowedStartMinute = 0; return true;
                case "CloudUploadAllowedEndMinute": settings.CloudUploadAllowedEndMinute = 1440; return true;
                case "EnableDashboardAutoRefresh": settings.EnableDashboardAutoRefresh = true; return true;
                case "EnableTaskNotifications": settings.EnableTaskNotifications = true; return true;
                case "NotificationLevel": settings.NotificationLevel = NotificationLevel.Summary; return true;
                case "SafeModeEnabled": settings.SafeModeEnabled = false; return true;
                case "SafeModeRequested": settings.SafeModeRequested = false; return true;
                case "DashboardRefreshSeconds": settings.DashboardRefreshSeconds = 10; return true;
                case "RecentProtectionWindowDays": settings.RecentProtectionWindowDays = 30; return true;
                case "ProcessPollingSeconds": settings.ProcessPollingSeconds = 5; return true;
                case "DefaultBackupIntervalMinutes": settings.DefaultBackupIntervalMinutes = 30; return true;
                case "HealthInspectionEnabled": settings.HealthInspectionEnabled = true; return true;
                case "HealthInspectionIntervalMinutes": settings.HealthInspectionIntervalMinutes = 1440; return true;
                case "HealthInspectionStaleAfterDays": settings.HealthInspectionStaleAfterDays = 30; return true;
                default: return false;
            }
        }

        public static int ResetCategory(GameSaveCenterSettings settings, SettingsResetCategory category)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            var count = 0;
            foreach (var field in fieldOptions.Where(option => option.Category == category))
                if (ResetField(settings, field.Key)) count++;
            return count;
        }

        public static int ResetAll(GameSaveCenterSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            var count = 0;
            foreach (var field in fieldOptions)
                if (ResetField(settings, field.Key)) count++;

            // These are local UI preferences, not connection or identity fields. They are
            // included only in the explicit all-default action, never in a category reset.
            settings.GamePickerSearchText = string.Empty;
            settings.GamePickerStatusFilter = "已安装";
            settings.GamePickerPlatformFilter = "全部";
            settings.GamePickerSortMode = "名称";
            settings.GamePickerSelectedGameId = string.Empty;
            settings.LastWorkspace = "Overview";
            settings.TaskStatusFilterState = "全部";
            settings.TaskGameFilterState = "全部";
            settings.TaskTypeFilterState = "全部";
            settings.TaskSearchTextState = string.Empty;
            settings.TaskHistoryScopeState = "最近任务";
            settings.TaskHistoryRangeState = "全部时间";
            settings.MediaFilterState = "全部";
            settings.MediaSearchTextState = string.Empty;
            settings.FilterPresets = new List<FilterPresetDefinition>();
            settings.DataGridColumnWidths = new Dictionary<string, double>(StringComparer.Ordinal);
            settings.RecentAccess = new List<RecentAccessRecord>();
            return count;
        }

        public static string BuildFieldImpact(SettingsResetFieldOption option)
            => $"将把“{option.Label}”恢复为“{option.DefaultDisplay}”。只修改当前 Playnite 编辑草稿；Worker、Ludusavi、Rclone、存档/媒体/镜像路径和云端目标不会被清空。取消设置时仍可恢复原草稿。";

        public static string BuildCategoryImpact(SettingsResetCategory category)
            => $"将恢复“{GetCategoryDisplay(category)}”中的 {fieldOptions.Count(option => option.Category == category)} 个安全设置字段。不会修改工具路径、存档/媒体/镜像路径、Rclone 云端目标或安装身份；操作只修改当前编辑草稿，Playnite 取消仍可恢复。";

        public static string BuildAllImpact()
            => $"将恢复 {fieldOptions.Count} 个安全设置字段，并清空筛选预设、列宽和最近访问等本地界面偏好。不会清除 Worker、Ludusavi、Rclone、存档/媒体/镜像路径、云端目标或设备身份；操作只修改当前编辑草稿，Playnite 取消仍可恢复。";
    }
}
