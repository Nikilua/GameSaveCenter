using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Newtonsoft.Json;
using Playnite.SDK;

namespace GameSaveCenter.Playnite.Diagnostics
{
    /// <summary>
    /// Developer-only Tier B audit. It only runs when GSC_REAL_HOST_AUDIT is set or a
    /// real-host-audit.request sentinel exists, captures the actual loaded Dashboard
    /// visual tree with the real AdaptiveTheme palette, and never triggers business actions.
    /// </summary>
    internal static class RealHostUiAuditService
    {
        private static readonly ILogger Logger = LogManager.GetLogger();
        private static readonly HashSet<string> CompletedRoots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static void TryCaptureDashboard(DashboardView dashboard)
        {
            var outputRoot = ResolveRequestedOutput();
            if (string.IsNullOrWhiteSpace(outputRoot))
                return;
            var root = outputRoot!.Trim();
            if (!CompletedRoots.Add(root))
                return;
            Logger.Info("Real host audit requested; scheduling Dashboard capture to " + root);

            var dispatcher = dashboard.Dispatcher;
            dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(async () =>
            {
                try
                {
                    await CaptureDashboardAsync(dashboard, root);
                }
                catch (Exception ex)
                {
                    TryWriteError(root, ex);
                }
            }));
        }

        public static void TryCaptureSettings(GameSaveCenterSettingsView settingsView)
        {
            var outputRoot = ResolveRequestedOutput();
            if (string.IsNullOrWhiteSpace(outputRoot))
                return;
            var root = outputRoot!.Trim();
            var settingsRoot = Path.Combine(root, "settings");
            if (!CompletedRoots.Add(settingsRoot))
                return;

            var dispatcher = settingsView.Dispatcher;
            dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                try
                {
                    CaptureSettings(settingsView, settingsRoot);
                }
                catch (Exception ex)
                {
                    TryWriteError(settingsRoot, ex);
                }
            }));
        }

        public static string? ResolveRequestedOutput()
        {
            var env = Environment.GetEnvironmentVariable("GSC_REAL_HOST_AUDIT");
            if (!string.IsNullOrWhiteSpace(env))
                return env.Trim();

            var sentinel = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GameSaveCenter",
                "real-host-audit.request");
            if (File.Exists(sentinel))
            {
                var text = File.ReadAllText(sentinel).Trim();
                if (!string.IsNullOrWhiteSpace(text))
                    return text;
            }

            return null;
        }

        private static async System.Threading.Tasks.Task CaptureDashboardAsync(DashboardView dashboard, string outputRoot)
        {
            Directory.CreateDirectory(outputRoot);
            Logger.Info("Real host audit capture started: " + outputRoot);
            var dpi = VisualTreeHelper.GetDpi(dashboard);
            var metadata = new UiHostMetadata
            {
                Mode = "real-playnite-host",
                CommitSha = Environment.GetEnvironmentVariable("GSC_UI_AUDIT_COMMIT") ?? "unknown",
                PluginVersion = typeof(DashboardView).Assembly.GetName().Version?.ToString() ?? "unknown",
                PlayniteSdkVersion = typeof(global::Playnite.SDK.IPlayniteAPI).Assembly.GetName().Version?.ToString() ?? "unknown",
                PlayniteDesktopVersion = GetPlayniteDesktopVersion(),
                WindowsVersion = Environment.OSVersion.VersionString,
                DpiScaleX = Math.Round(dpi.DpiScaleX, 2),
                DpiScaleY = Math.Round(dpi.DpiScaleY, 2),
                PixelsPerDip = Math.Round(dpi.PixelsPerDip, 2),
                DashboardWidth = Math.Round(dashboard.ActualWidth, 2),
                DashboardHeight = Math.Round(dashboard.ActualHeight, 2),
                DetailsTabControlWidth = Math.Round(dashboard.DetailsTabControlForAudit?.ActualWidth ?? 0, 2),
                DetailsTabControlHeight = Math.Round(dashboard.DetailsTabControlForAudit?.ActualHeight ?? 0, 2),
                ThemeMode = dashboard.PluginForAudit.Settings.ThemeMode.ToString(),
                GlassEnabled = dashboard.PluginForAudit.Settings.EnableGlassEffects,
                GlassStrength = dashboard.PluginForAudit.Settings.GlassEffectStrength,
                AnimationsEnabled = dashboard.PluginForAudit.Settings.EnableUiAnimations,
                HighContrast = SystemParameters.HighContrast
            };

            var resources = BuildResourceSnapshot(dashboard);
            var fingerprints = UiDiagnosticsExporters.BuildStyleFingerprints(dashboard);
            var visualTree = UiDiagnosticsExporters.BuildVisualTree(dashboard);

            UiDiagnosticsExporters.WriteJson(metadata, Path.Combine(outputRoot, "metadata.json"));
            UiDiagnosticsExporters.WriteJson(resources, Path.Combine(outputRoot, "resource-snapshot.json"));
            UiDiagnosticsExporters.WriteJson(fingerprints, Path.Combine(outputRoot, "style-fingerprints.json"));
            UiDiagnosticsExporters.WriteJson(visualTree, Path.Combine(outputRoot, "visual-tree-dashboard.json"));

            var screenshots = Path.Combine(outputRoot, "screenshots");
            Directory.CreateDirectory(screenshots);
            UiDiagnosticsExporters.SavePng(dashboard, Path.Combine(screenshots, "dashboard-initial.png"));

            foreach (var workspace in new[] { WorkspaceKind.Overview, WorkspaceKind.Saves, WorkspaceKind.Trainers, WorkspaceKind.Media, WorkspaceKind.Tasks, WorkspaceKind.Maintenance })
            {
                dashboard.ApplyWorkspaceForAudit(workspace);
                await WaitForRenderAsync(dashboard.Dispatcher);
                var safe = workspace.ToString().ToLowerInvariant();
                UiDiagnosticsExporters.SavePng(dashboard, Path.Combine(screenshots, $"workspace-{safe}.png"));
                UiDiagnosticsExporters.WriteJson(
                    UiDiagnosticsExporters.BuildVisualTree(dashboard),
                    Path.Combine(outputRoot, "visual-tree", $"workspace-{safe}.json"));
                UiDiagnosticsExporters.WriteJson(
                    new Dictionary<string, double>
                    {
                        ["dashboardWidth"] = dashboard.ActualWidth,
                        ["dashboardHeight"] = dashboard.ActualHeight,
                        ["detailsTabWidth"] = dashboard.DetailsTabControlForAudit?.ActualWidth ?? 0,
                        ["detailsTabHeight"] = dashboard.DetailsTabControlForAudit?.ActualHeight ?? 0
                    },
                    Path.Combine(outputRoot, "layout", $"workspace-{safe}.json"));
            }

            await CaptureInnerTabs(dashboard, outputRoot, screenshots);
            CreateZip(outputRoot);
            RequestSettingsCapture(dashboard);
            TryDeleteSentinel();
        }

        private static async System.Threading.Tasks.Task CaptureInnerTabs(DashboardView dashboard, string outputRoot, string screenshots)
        {
            var targets = new Dictionary<WorkspaceKind, string[]>
            {
                [WorkspaceKind.Saves] = new[] { "比较与保留" },
                [WorkspaceKind.Media] = new[] { "当前游戏媒体" },
                [WorkspaceKind.Trainers] = new[] { "FLiNG 在线库" },
                [WorkspaceKind.Maintenance] = new[] { "设备状态" }
            };

            foreach (var pair in targets)
            {
                dashboard.ApplyWorkspaceForAudit(pair.Key);
                await WaitForRenderAsync(dashboard.Dispatcher);
                foreach (var header in pair.Value)
                {
                    if (!SelectInnerTab(dashboard, header))
                        continue;
                    await WaitForRenderAsync(dashboard.Dispatcher);
                    var safe = header.ToLowerInvariant().Replace(' ', '-');
                    UiDiagnosticsExporters.SavePng(dashboard, Path.Combine(screenshots, $"tab-{pair.Key.ToString().ToLowerInvariant()}-{safe}.png"));
                }
            }
        }

        private static void RequestSettingsCapture(DashboardView dashboard)
        {
            dashboard.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                try
                {
                    dashboard.PluginForAudit.PlayniteApi.MainView.OpenPluginSettings(dashboard.PluginForAudit.Id);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to open GameSaveCenter settings for the real host audit.");
                }
            }));
        }

        private static bool SelectInnerTab(DashboardView dashboard, string header)
        {
            var outer = dashboard.DetailsTabControlForAudit;
            var tabControls = FindVisualChildren<TabControl>(dashboard)
                .Where(tabControl => !ReferenceEquals(tabControl, outer))
                .ToList();
            foreach (var tabControl in tabControls)
            {
                foreach (var item in tabControl.Items)
                {
                    if (item is TabItem tab && string.Equals(tab.Header?.ToString(), header, StringComparison.Ordinal))
                    {
                        tabControl.SelectedItem = tab;
                        return true;
                    }
                }
            }
            return false;
        }

        private static void CaptureSettings(GameSaveCenterSettingsView settingsView, string outputRoot)
        {
            Directory.CreateDirectory(outputRoot);
            var dpi = VisualTreeHelper.GetDpi(settingsView);
            var metadata = new UiHostMetadata
            {
                Mode = "real-playnite-host-settings",
                CommitSha = Environment.GetEnvironmentVariable("GSC_UI_AUDIT_COMMIT") ?? "unknown",
                PluginVersion = typeof(GameSaveCenterSettingsView).Assembly.GetName().Version?.ToString() ?? "unknown",
                PlayniteSdkVersion = typeof(global::Playnite.SDK.IPlayniteAPI).Assembly.GetName().Version?.ToString() ?? "unknown",
                PlayniteDesktopVersion = GetPlayniteDesktopVersion(),
                WindowsVersion = Environment.OSVersion.VersionString,
                DpiScaleX = Math.Round(dpi.DpiScaleX, 2),
                DpiScaleY = Math.Round(dpi.DpiScaleY, 2),
                PixelsPerDip = Math.Round(dpi.PixelsPerDip, 2),
                DashboardWidth = Math.Round(settingsView.ActualWidth, 2),
                DashboardHeight = Math.Round(settingsView.ActualHeight, 2),
                HighContrast = SystemParameters.HighContrast
            };
            UiDiagnosticsExporters.WriteJson(metadata, Path.Combine(outputRoot, "metadata.json"));
            UiDiagnosticsExporters.WriteJson(
                UiDiagnosticsExporters.BuildResourceSnapshot(settingsView.Resources, "SettingsView"),
                Path.Combine(outputRoot, "resource-snapshot.json"));
            UiDiagnosticsExporters.WriteJson(
                UiDiagnosticsExporters.BuildStyleFingerprints(settingsView),
                Path.Combine(outputRoot, "style-fingerprints.json"));
            UiDiagnosticsExporters.WriteJson(
                UiDiagnosticsExporters.BuildVisualTree(settingsView),
                Path.Combine(outputRoot, "visual-tree.json"));
            var screenshots = Path.Combine(outputRoot, "screenshots");
            Directory.CreateDirectory(screenshots);
            UiDiagnosticsExporters.SavePng(settingsView, Path.Combine(screenshots, "settings.png"));
            CreateZip(outputRoot);
            TryDeleteSentinel();
        }

        private static List<UiResourceRecord> BuildResourceSnapshot(DashboardView dashboard)
        {
            var records = UiDiagnosticsExporters.BuildResourceSnapshot(dashboard.Resources, "DashboardView");
            if (Application.Current?.Resources != null)
                records.AddRange(UiDiagnosticsExporters.BuildResourceSnapshot(Application.Current.Resources, "Application"));
            return records;
        }

        private static async System.Threading.Tasks.Task WaitForRenderAsync(Dispatcher dispatcher)
        {
            await dispatcher.InvokeAsync(() => { }, DispatcherPriority.Loaded);
            await dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
            await System.Threading.Tasks.Task.Delay(90);
        }

        private static void CreateZip(string outputRoot)
        {
            var parent = Path.GetDirectoryName(outputRoot.TrimEnd(Path.DirectorySeparatorChar)) ?? outputRoot;
            var zipPath = Path.Combine(parent, "GameSaveCenter-ui-host-audit.zip");
            if (File.Exists(zipPath))
                File.Delete(zipPath);
            ZipFile.CreateFromDirectory(outputRoot, zipPath, CompressionLevel.Optimal, false);
        }

        private static void TryDeleteSentinel()
        {
            try
            {
                var sentinel = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "GameSaveCenter",
                    "real-host-audit.request");
                if (File.Exists(sentinel))
                    File.Delete(sentinel);
            }
            catch
            {
                // Sentinel cleanup is best-effort; the env-var path does not rely on it.
            }
        }

        private static void TryWriteError(string outputRoot, Exception ex)
        {
            try
            {
                Directory.CreateDirectory(outputRoot);
                File.WriteAllText(Path.Combine(outputRoot, "error.log"), ex.ToString());
            }
            catch
            {
                // No further fallback is useful for a diagnostics-only service.
            }
        }

        private static string GetPlayniteDesktopVersion()
        {
            try
            {
                using var process = Process.GetCurrentProcess();
                var version = process.MainModule?.FileVersionInfo;
                if (version == null)
                    return "unknown";
                return !string.IsNullOrWhiteSpace(version.FileVersion)
                    ? version.FileVersion
                    : !string.IsNullOrWhiteSpace(version.ProductVersion)
                        ? version.ProductVersion
                        : "unknown";
            }
            catch
            {
                return "unknown";
            }
        }

        private static List<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            var results = new List<T>();
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T match)
                    results.Add(match);
                results.AddRange(FindVisualChildren<T>(child));
            }
            return results;
        }

        internal sealed class UiHostMetadata
        {
            public string Mode { get; set; } = "real-playnite-host";
            public string CommitSha { get; set; } = "unknown";
            public string PluginVersion { get; set; } = "unknown";
            public string PlayniteDesktopVersion { get; set; } = "unknown";
            public string PlayniteSdkVersion { get; set; } = "unknown";
            public string WindowsVersion { get; set; } = string.Empty;
            public double DpiScaleX { get; set; } = 1.0;
            public double DpiScaleY { get; set; } = 1.0;
            public double PixelsPerDip { get; set; } = 1.0;
            public double DashboardWidth { get; set; }
            public double DashboardHeight { get; set; }
            public double DetailsTabControlWidth { get; set; }
            public double DetailsTabControlHeight { get; set; }
            public string ThemeMode { get; set; } = string.Empty;
            public bool GlassEnabled { get; set; }
            public int GlassStrength { get; set; }
            public bool AnimationsEnabled { get; set; }
            public bool HighContrast { get; set; }
        }
    }
}
