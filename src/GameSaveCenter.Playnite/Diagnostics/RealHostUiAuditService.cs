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
        private static readonly object StateLock = new object();
        private static bool dashboardCaptureStarted;
        private static bool settingsCaptureStarted;
        private static string? requestedOutputRoot;
        private static Window? auditDashboardWindow;
        private static Window? auditSettingsWindow;

        /// <summary>
        /// Opens the real DashboardView inside a dedicated window when Playnite's own window
        /// is not available (for example a hidden or locked desktop session). The dashboard
        /// Loaded handler starts the same Tier B capture as the sidebar path.
        /// </summary>
        internal static void EnsureDashboardCaptured(GameSaveCenterPlugin plugin)
        {
            lock (StateLock)
            {
                if (dashboardCaptureStarted)
                    return;
            }
            Logger.Info("Real host audit fallback: hosting GameSaveCenter dashboard in a dedicated window.");
            var dispatcher = plugin.PlayniteApi.MainView.UIDispatcher;
            dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                try
                {
                    var dashboard = plugin.CreateDashboardViewForAudit();
                    if (dashboard == null)
                        return;
                    var workArea = SystemParameters.WorkArea;
                    var window = new Window
                    {
                        Title = "GameSaveCenter Real Host Audit",
                        Width = Math.Min(1440, workArea.Width),
                        Height = Math.Min(900, workArea.Height),
                        Left = workArea.Left,
                        Top = workArea.Top,
                        WindowStartupLocation = WindowStartupLocation.Manual,
                        WindowStyle = WindowStyle.ToolWindow,
                        ResizeMode = ResizeMode.CanResize,
                        ShowInTaskbar = false,
                        Content = dashboard
                    };
                    auditDashboardWindow = window;
                    window.Show();
                }
                catch (Exception ex)
                {
                    TryWriteError(ResolveRequestedOutput() ?? "ui-host-audit", ex);
                }
            }));
        }

        /// <summary>
        /// Opens the settings view in a dedicated window when Playnite cannot show the
        /// plugin settings dialog in the current desktop session.
        /// </summary>
        internal static void EnsureSettingsCaptured(string outputRoot, Dispatcher uiDispatcher)
        {
            lock (StateLock)
            {
                if (settingsCaptureStarted)
                    return;
            }
            Logger.Info("Real host audit fallback: hosting GameSaveCenter settings in a dedicated window.");
            uiDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                try
                {
                    Logger.Info("Real host audit settings fallback: creating settings view on the UI thread.");
                    var settings = new GameSaveCenterSettingsView();
                    var workArea = SystemParameters.WorkArea;
                    var window = new Window
                    {
                        Title = "GameSaveCenter Settings Real Host Audit",
                        Width = Math.Min(1440, workArea.Width),
                        Height = Math.Min(900, workArea.Height),
                        Left = workArea.Left,
                        Top = workArea.Top,
                        WindowStartupLocation = WindowStartupLocation.Manual,
                        WindowStyle = WindowStyle.ToolWindow,
                        ResizeMode = ResizeMode.CanResize,
                        ShowInTaskbar = false,
                        Content = settings
                    };
                    auditSettingsWindow = window;
                    Logger.Info("Real host audit settings fallback: showing settings window.");
                    window.Show();
                    Logger.Info("Real host audit settings fallback: settings window shown.");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Real host audit settings fallback failed.");
                    TryWriteError(outputRoot, ex);
                }
            }));
        }

        public static void TryCaptureDashboard(DashboardView dashboard)
        {
            var outputRoot = ResolveRequestedOutput();
            if (string.IsNullOrWhiteSpace(outputRoot))
                return;
            lock (StateLock)
            {
                requestedOutputRoot = outputRoot!.Trim();
            }
            lock (StateLock)
            {
                dashboardCaptureStarted = true;
            }
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
            {
                lock (StateLock)
                {
                    outputRoot = requestedOutputRoot;
                }
            }
            if (string.IsNullOrWhiteSpace(outputRoot))
                return;
            lock (StateLock)
            {
                settingsCaptureStarted = true;
            }
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
                SaveWindowScreenshot(dashboard, Path.Combine(screenshots, $"window-{safe}.png"));
                CaptureScrollSurfaces(dashboard, outputRoot, "workspace-" + safe);
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

            await CaptureAllInnerTabs(dashboard, outputRoot, screenshots);
            CreateZip(outputRoot);
            RequestSettingsCapture(dashboard);
            TryDeleteSentinel();
        }

        private static async System.Threading.Tasks.Task CaptureAllInnerTabs(DashboardView dashboard, string outputRoot, string screenshots)
        {
            var outer = dashboard.DetailsTabControlForAudit;
            var tabControls = FindVisualChildren<TabControl>(dashboard)
                .Where(tabControl => !ReferenceEquals(tabControl, outer))
                .ToList();
            var captured = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var tabControl in tabControls)
            {
                for (var index = 0; index < tabControl.Items.Count; index++)
                {
                    if (!(tabControl.Items[index] is TabItem tab) || tab.Visibility != Visibility.Visible)
                        continue;
                    tabControl.SelectedItem = tab;
                    await WaitForRenderAsync(dashboard.Dispatcher);
                    var rawHeader = tab.Header?.ToString() ?? "tab-" + index;
                    var safe = SafeFileName(rawHeader);
                    if (!captured.Add(safe))
                        continue;
                    var file = $"tab-{index}-{safe}.png";
                    UiDiagnosticsExporters.SavePng(dashboard, Path.Combine(screenshots, file));
                    SaveWindowScreenshot(dashboard, Path.Combine(screenshots, $"window-tab-{safe}.png"));
                    CaptureScrollSurfaces(dashboard, outputRoot, "tab-" + safe);
                }
            }
        }

        private static void CaptureScrollSurfaces(DashboardView dashboard, string outputRoot, string prefix)
        {
            var fullDir = Path.Combine(outputRoot, "full-scroll");
            Directory.CreateDirectory(fullDir);
            foreach (var scroller in FindVisualChildren<ScrollViewer>(dashboard))
            {
                if (scroller.Visibility != Visibility.Visible || scroller.ScrollableHeight <= 0.5)
                    continue;
                var name = string.IsNullOrWhiteSpace(scroller.Name) ? "scroller" : scroller.Name;
                try
                {
                    UiDiagnosticsExporters.SaveScrollViewerFull(
                        scroller,
                        Path.Combine(fullDir, $"{prefix}-{name}.png"));
                }
                catch (Exception ex)
                {
                    Logger.Debug(ex, "Real host audit skipped a scroll surface: " + name);
                }
            }
        }

        private static void SaveWindowScreenshot(FrameworkElement anchor, string path)
        {
            var window = Window.GetWindow(anchor);
            if (window == null)
            {
                UiDiagnosticsExporters.SavePng(anchor, path);
                return;
            }
            try
            {
                UiDiagnosticsExporters.SavePng(window, path);
            }
            catch
            {
                UiDiagnosticsExporters.SavePng(anchor, path);
            }
        }

        private static void RequestSettingsCapture(DashboardView dashboard)
        {
            var outputRoot = ResolveRequestedOutput();
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
            _ = FireAndForgetSettingsFallback(outputRoot, dashboard.Dispatcher);
        }

        private static async System.Threading.Tasks.Task FireAndForgetSettingsFallback(string? outputRoot, Dispatcher uiDispatcher)
        {
            await System.Threading.Tasks.Task.Delay(8000);
            if (string.IsNullOrWhiteSpace(outputRoot))
                return;
            lock (StateLock)
            {
                if (settingsCaptureStarted)
                    return;
            }
            EnsureSettingsCaptured(outputRoot!, uiDispatcher);
        }

        private static void CaptureSettings(GameSaveCenterSettingsView settingsView, string outputRoot)
        {
            Directory.CreateDirectory(outputRoot);
            Logger.Info("Real host settings capture started: " + outputRoot);
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
            SaveWindowScreenshot(settingsView, Path.Combine(screenshots, "window-settings.png"));
            CaptureSettingsTabs(settingsView, outputRoot);
            foreach (var scroller in FindVisualChildren<ScrollViewer>(settingsView))
            {
                if (scroller.Visibility != Visibility.Visible || scroller.ScrollableHeight <= 0.5)
                    continue;
                var name = string.IsNullOrWhiteSpace(scroller.Name) ? "scroller" : scroller.Name;
                try
                {
                    UiDiagnosticsExporters.SaveScrollViewerFull(
                        scroller,
                        Path.Combine(outputRoot, "full-scroll", "settings-" + name + ".png"));
                }
                catch (Exception ex)
                {
                    Logger.Debug(ex, "Real host settings audit skipped a scroll surface: " + name);
                }
            }
            CreateZip(Path.GetDirectoryName(outputRoot)!);
            Logger.Info("Real host settings capture finished: " + outputRoot);
            CloseAuditWindow(auditSettingsWindow);
            TryDeleteSentinel();
        }

        private static void CaptureSettingsTabs(GameSaveCenterSettingsView settingsView, string outputRoot)
        {
            var tabControls = FindVisualChildren<TabControl>(settingsView).ToList();
            var screenshots = Path.Combine(outputRoot, "screenshots");
            var captured = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var tabControl in tabControls)
            {
                for (var index = 0; index < tabControl.Items.Count; index++)
                {
                    if (!(tabControl.Items[index] is TabItem tab) || tab.Visibility != Visibility.Visible)
                        continue;
                    tabControl.SelectedItem = tab;
                    settingsView.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
                    var raw = ResolveTabHeaderName(tab, index);
                    var safe = SafeFileName(raw);
                    var dedupeKey = index + "|" + safe;
                    if (!captured.Add(dedupeKey))
                        continue;
                    UiDiagnosticsExporters.SavePng(settingsView, Path.Combine(screenshots, $"settings-{index}-{safe}.png"));
                    SaveWindowScreenshot(settingsView, Path.Combine(screenshots, $"window-settings-{safe}.png"));
                    foreach (var scroller in FindVisualChildren<ScrollViewer>(settingsView))
                    {
                        if (scroller.Visibility != Visibility.Visible || scroller.ScrollableHeight <= 0.5)
                            continue;
                        var name = string.IsNullOrWhiteSpace(scroller.Name) ? "scroller" : scroller.Name;
                        try
                        {
                            UiDiagnosticsExporters.SaveScrollViewerFull(
                                scroller,
                                Path.Combine(outputRoot, "full-scroll", $"settings-{safe}-{name}.png"));
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug(ex, "Real host settings tab audit skipped a scroll surface: " + name);
                        }
                    }
                }
            }
        }

        private static string ResolveTabHeaderName(TabItem tab, int index)
        {
            var header = tab.Header as DependencyObject;
            if (header != null)
            {
                var labels = FindVisualChildren<TextBlock>(header)
                    .Select(block => block.Text?.Trim())
                    .Where(text => !string.IsNullOrWhiteSpace(text))
                    .ToList();
                if (labels.Count > 0)
                    return labels[labels.Count - 1]!;
            }
            var direct = tab.Header?.ToString();
            if (!string.IsNullOrWhiteSpace(direct)
                && !direct!.Equals("System.Windows.Controls.Grid", StringComparison.Ordinal))
            {
                return direct!;
            }
            return "tab-" + index;
        }

        private static string SafeFileName(string value)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return string.Join("-", value.Select(ch => invalid.Contains(ch) ? '-' : ch)).ToLowerInvariant();
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
            try
            {
                var zipPath = Path.Combine(parent, "GameSaveCenter-ui-host-audit.zip");
                if (File.Exists(zipPath))
                    File.Delete(zipPath);
                ZipFile.CreateFromDirectory(outputRoot, zipPath, CompressionLevel.Optimal, false);
            }
            catch (Exception ex)
            {
                // A viewer/Explorer can keep the well-known archive open. Write a unique
                // archive instead of failing the whole audit; the folder remains the source
                // of truth and the settings pass still runs.
                var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                var unique = Path.Combine(parent, $"GameSaveCenter-ui-host-audit-{stamp}.zip");
                ZipFile.CreateFromDirectory(outputRoot, unique, CompressionLevel.Optimal, false);
                Logger.Warn(ex, "Real host audit wrote a unique archive because the default archive was locked: " + unique);
            }
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

        private static void CloseDashboardWindow(DashboardView dashboard)
        {
            try
            {
                var window = Window.GetWindow(dashboard);
                if (window != null
                    && ReferenceEquals(window, auditDashboardWindow)
                    && window.IsLoaded)
                {
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Real host audit could not close the dashboard window.");
            }
        }

        private static void CloseAuditWindow(Window? window)
        {
            if (window == null)
                return;
            try
            {
                window.Dispatcher.Invoke(() =>
                {
                    if (window.IsLoaded)
                        window.Close();
                });
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Real host audit could not close a dedicated audit window.");
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
