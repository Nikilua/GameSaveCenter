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
                    var auditBounds = ComputeAuditWindowBounds(dashboard);
                    var window = new Window
                    {
                        Title = "GameSaveCenter Real Host Audit",
                        Width = auditBounds.Width,
                        Height = auditBounds.Height,
                        Left = auditBounds.Left,
                        Top = auditBounds.Top,
                        WindowStartupLocation = WindowStartupLocation.Manual,
                        SizeToContent = SizeToContent.Manual,
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
        internal static void EnsureSettingsCaptured(string outputRoot, Dispatcher uiDispatcher, GameSaveCenterSettings pluginSettings)
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
                    settings.DataContext = pluginSettings;
                    var auditBounds = ComputeAuditWindowBounds(settings);
                    var window = new Window
                    {
                        Title = "GameSaveCenter Settings Real Host Audit",
                        Width = auditBounds.Width,
                        Height = auditBounds.Height,
                        Left = auditBounds.Left,
                        Top = auditBounds.Top,
                        WindowStartupLocation = WindowStartupLocation.Manual,
                        SizeToContent = SizeToContent.Manual,
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

            var maximized = ComputeAuditWindowBounds(dashboard);
            var sizes = new[]
            {
                new AuditWindowSize("maximized", maximized.Width, maximized.Height),
                new AuditWindowSize("1600x1000", Math.Min(1600d, maximized.Width), Math.Min(1000d, maximized.Height)),
                new AuditWindowSize("1366x768", Math.Min(1366d, maximized.Width), Math.Min(768d, maximized.Height)),
                new AuditWindowSize("1280x720", Math.Min(1280d, maximized.Width), Math.Min(720d, maximized.Height)),
                new AuditWindowSize("1024x768", Math.Min(1024d, maximized.Width), Math.Min(768d, maximized.Height))
            };

            foreach (var size in sizes)
            {
                foreach (var theme in new[] { GameSaveCenterThemeMode.Light, GameSaveCenterThemeMode.Dark })
                {
                    await CaptureDashboardAtSizeAsync(dashboard, outputRoot, size, theme, metadata);
                }
            }

            CreateZip(outputRoot);
            RequestSettingsCapture(dashboard);
            CloseAuditWindow(auditDashboardWindow);
            TryDeleteSentinel();
        }

        private static async System.Threading.Tasks.Task CaptureDashboardAtSizeAsync(
            DashboardView dashboard,
            string outputRoot,
            AuditWindowSize size,
            GameSaveCenterThemeMode theme,
            UiHostMetadata metadata)
        {
            var window = auditDashboardWindow;
            if (window != null)
            {
                window.Width = size.Width;
                window.Height = size.Height;
                window.Left = 0;
                window.Top = 0;
            }
            dashboard.Width = size.Width;
            dashboard.Height = size.Height;
            dashboard.UpdateLayout();
            dashboard.ApplyThemeForAudit(theme);
            dashboard.ApplyWorkspaceForAudit(WorkspaceKind.Overview);
            await WaitForRenderAsync(dashboard.Dispatcher);

            var sizeDir = Path.Combine(outputRoot, "screenshots", size.Key, theme.ToString().ToLowerInvariant());
            Directory.CreateDirectory(sizeDir);
            var sizeLayoutDir = Path.Combine(outputRoot, "layout", size.Key, theme.ToString().ToLowerInvariant());
            Directory.CreateDirectory(sizeLayoutDir);

            metadata.DashboardWidth = Math.Round(dashboard.ActualWidth, 2);
            metadata.DashboardHeight = Math.Round(dashboard.ActualHeight, 2);
            metadata.DetailsTabControlWidth = Math.Round(dashboard.DetailsTabControlForAudit?.ActualWidth ?? 0, 2);
            metadata.DetailsTabControlHeight = Math.Round(dashboard.DetailsTabControlForAudit?.ActualHeight ?? 0, 2);
            metadata.ThemeMode = theme.ToString();
            UiDiagnosticsExporters.WriteJson(
                metadata,
                Path.Combine(outputRoot, "metadata-" + size.Key + "-" + theme.ToString().ToLowerInvariant() + ".json"));

            SaveFullPage(dashboard, Path.Combine(sizeDir, "dashboard-initial.png"));
            SaveWindowScreenshot(dashboard, Path.Combine(sizeDir, "window-dashboard.png"));

            foreach (var workspace in new[]
            {
                WorkspaceKind.Overview,
                WorkspaceKind.Saves,
                WorkspaceKind.Trainers,
                WorkspaceKind.Media,
                WorkspaceKind.Tasks,
                WorkspaceKind.Maintenance
            })
            {
                dashboard.ApplyWorkspaceForAudit(workspace);
                await WaitForRenderAsync(dashboard.Dispatcher);
                var safe = workspace.ToString().ToLowerInvariant();
                SaveFullPage(dashboard, Path.Combine(sizeDir, $"workspace-{safe}.png"));
                SaveWindowScreenshot(dashboard, Path.Combine(sizeDir, $"window-{safe}.png"));
                UiDiagnosticsExporters.WriteJson(
                    UiDiagnosticsExporters.BuildVisualTree(dashboard),
                    Path.Combine(outputRoot, "visual-tree", size.Key, theme.ToString().ToLowerInvariant(), $"workspace-{safe}.json"));
                UiDiagnosticsExporters.WriteJson(
                    new Dictionary<string, double>
                    {
                        ["dashboardWidth"] = dashboard.ActualWidth,
                        ["dashboardHeight"] = dashboard.ActualHeight,
                        ["detailsTabWidth"] = dashboard.DetailsTabControlForAudit?.ActualWidth ?? 0,
                        ["detailsTabHeight"] = dashboard.DetailsTabControlForAudit?.ActualHeight ?? 0
                    },
                    Path.Combine(sizeLayoutDir, $"workspace-{safe}.json"));
            }

            await CaptureAllInnerTabs(dashboard, sizeDir, size.Key, theme);
        }

        private static async System.Threading.Tasks.Task CaptureAllInnerTabs(
            DashboardView dashboard,
            string screenshots,
            string sizeKey,
            GameSaveCenterThemeMode theme)
        {
            var outer = dashboard.DetailsTabControlForAudit;
            var captured = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var workspace in new[]
            {
                WorkspaceKind.Overview,
                WorkspaceKind.Saves,
                WorkspaceKind.Trainers,
                WorkspaceKind.Media,
                WorkspaceKind.Tasks,
                WorkspaceKind.Maintenance
            })
            {
                dashboard.ApplyWorkspaceForAudit(workspace);
                await WaitForRenderAsync(dashboard.Dispatcher);
                var tabControls = FindVisualChildren<TabControl>(dashboard)
                    .Where(tabControl => !ReferenceEquals(tabControl, outer))
                    .Where(tabControl => tabControl.Visibility == Visibility.Visible)
                    .ToList();
                var workspacePrefix = workspace.ToString().ToLowerInvariant();
                foreach (var tabControl in tabControls)
                {
                    await CaptureTabControlRecursive(
                        dashboard,
                        tabControl,
                        screenshots,
                        sizeKey,
                        theme,
                        workspacePrefix,
                        captured);
                }
            }
        }

        private static async System.Threading.Tasks.Task CaptureTabControlRecursive(
            DashboardView dashboard,
            TabControl tabControl,
            string screenshots,
            string sizeKey,
            GameSaveCenterThemeMode theme,
            string pathPrefix,
            HashSet<string> captured)
        {
            for (var index = 0; index < tabControl.Items.Count; index++)
            {
                if (!(tabControl.Items[index] is TabItem tab) || tab.Visibility != Visibility.Visible)
                    continue;
                tabControl.SelectedItem = tab;
                tab.UpdateLayout();
                tabControl.UpdateLayout();
                await WaitForRenderAsync(dashboard.Dispatcher);
                await dashboard.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                var raw = ResolveTabHeaderName(tab, index);
                var safe = SafeFileName(raw);
                var dedupeKey = sizeKey + "|" + theme + "|" + pathPrefix + "|" + index + "|" + safe;
                if (!captured.Add(dedupeKey))
                    continue;
                var file = $"tab-{pathPrefix}-{safe}.png";
                SaveFullPage(dashboard, Path.Combine(screenshots, file));
                SaveWindowScreenshot(dashboard, Path.Combine(screenshots, $"window-tab-{pathPrefix}-{safe}.png"));

                // Capture nested tab pages (tabs inside a tab), e.g. Audit -> Audit Log.
                var nested = new List<TabControl>();
                foreach (var candidate in FindVisualChildren<TabControl>(tab))
                {
                    if (candidate.Visibility == Visibility.Visible && !nested.Contains(candidate))
                        nested.Add(candidate);
                }
                if (tab.Content is DependencyObject contentRoot)
                {
                    foreach (var candidate in FindVisualChildren<TabControl>(contentRoot))
                    {
                        if (candidate.Visibility == Visibility.Visible && !nested.Contains(candidate))
                            nested.Add(candidate);
                    }
                }
                foreach (var nestedControl in nested)
                {
                    await CaptureTabControlRecursive(
                        dashboard,
                        nestedControl,
                        screenshots,
                        sizeKey,
                        theme,
                        pathPrefix + "-" + safe,
                        captured);
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

        private static void SaveFullPage(FrameworkElement root, string path)
        {
            var rootWidth = root.ActualWidth > 0 ? root.ActualWidth : 1200d;
            var contentExtent = FindVisualChildren<ScrollViewer>(root)
                .Where(scroller =>
                    scroller.Visibility == Visibility.Visible
                    && scroller.ActualWidth > 0
                    && scroller.ViewportHeight > 0
                    && scroller.ScrollableHeight > 0.5
                    && scroller.ExtentHeight >= 200)
                .Where(scroller => !HasAncestor<DataGrid>(scroller))
                .Where(scroller => scroller.ActualWidth >= rootWidth * 0.45)
                .Select(scroller => scroller.ExtentHeight)
                .DefaultIfEmpty(root.ActualHeight)
                .Max();

            // Some pages have no page-level ScrollViewer (their content is arranged to the
            // viewport). Render the whole root at its full content extent so the screenshot
            // is a full page instead of only the visible top portion.
            var originalHeight = root.Height;
            var fullHeight = Math.Min(
                3000d,
                Math.Max(root.ActualHeight * 1.35d, Math.Max(contentExtent, root.ActualHeight + 240d)));
            try
            {
                root.Height = fullHeight;
                root.UpdateLayout();
                UiDiagnosticsExporters.SavePng(root, path, GetRenderScale(root));
            }
            finally
            {
                root.Height = originalHeight;
                root.UpdateLayout();
            }
        }

        private static bool HasAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(current);
            while (parent != null)
            {
                if (parent is T)
                    return true;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return false;
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
            _ = FireAndForgetSettingsFallback(outputRoot, dashboard.Dispatcher, dashboard.PluginForAudit.Settings);
        }

        private static async System.Threading.Tasks.Task FireAndForgetSettingsFallback(
            string? outputRoot,
            Dispatcher uiDispatcher,
            GameSaveCenterSettings pluginSettings)
        {
            await System.Threading.Tasks.Task.Delay(8000);
            if (string.IsNullOrWhiteSpace(outputRoot))
                return;
            lock (StateLock)
            {
                if (settingsCaptureStarted)
                    return;
            }
            EnsureSettingsCaptured(outputRoot!, uiDispatcher, pluginSettings);
        }

        private static void CaptureSettings(GameSaveCenterSettingsView settingsView, string outputRoot)
        {
            Directory.CreateDirectory(outputRoot);
            Logger.Info("Real host settings capture started: " + outputRoot);
            var maximized = ComputeAuditWindowBounds(settingsView);
            var sizes = new[]
            {
                new AuditWindowSize("maximized", maximized.Width, maximized.Height),
                new AuditWindowSize("1600x1000", Math.Min(1600d, maximized.Width), Math.Min(1000d, maximized.Height)),
                new AuditWindowSize("1366x768", Math.Min(1366d, maximized.Width), Math.Min(768d, maximized.Height)),
                new AuditWindowSize("1280x720", Math.Min(1280d, maximized.Width), Math.Min(720d, maximized.Height)),
                new AuditWindowSize("1024x768", Math.Min(1024d, maximized.Width), Math.Min(768d, maximized.Height))
            };

            foreach (var size in sizes)
            {
                foreach (var theme in new[] { GameSaveCenterThemeMode.Light, GameSaveCenterThemeMode.Dark })
                {
                    CaptureSettingsAtSize(settingsView, outputRoot, size, theme);
                }
            }

            CreateZip(Path.GetDirectoryName(outputRoot)!);
            Logger.Info("Real host settings capture finished: " + outputRoot);
            CloseAuditWindow(auditSettingsWindow);
            TryDeleteSentinel();
        }

        private static void CaptureSettingsAtSize(
            GameSaveCenterSettingsView settingsView,
            string outputRoot,
            AuditWindowSize size,
            GameSaveCenterThemeMode theme)
        {
            var window = auditSettingsWindow;
            if (window != null)
            {
                window.Width = size.Width;
                window.Height = size.Height;
                window.Left = 0;
                window.Top = 0;
            }
            settingsView.Width = size.Width;
            settingsView.Height = size.Height;
            settingsView.ApplyThemeForAudit(theme);
            settingsView.UpdateLayout();
            settingsView.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);

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
            UiDiagnosticsExporters.WriteJson(
                metadata,
                Path.Combine(outputRoot, "metadata-" + size.Key + "-" + theme.ToString().ToLowerInvariant() + ".json"));
            UiDiagnosticsExporters.WriteJson(
                UiDiagnosticsExporters.BuildResourceSnapshot(settingsView.Resources, "SettingsView"),
                Path.Combine(outputRoot, "resource-snapshot-" + size.Key + ".json"));
            UiDiagnosticsExporters.WriteJson(
                UiDiagnosticsExporters.BuildStyleFingerprints(settingsView),
                Path.Combine(outputRoot, "style-fingerprints-" + size.Key + ".json"));
            UiDiagnosticsExporters.WriteJson(
                UiDiagnosticsExporters.BuildVisualTree(settingsView),
                Path.Combine(outputRoot, "visual-tree-" + size.Key + ".json"));

            var screenshots = Path.Combine(outputRoot, "screenshots", size.Key, theme.ToString().ToLowerInvariant());
            Directory.CreateDirectory(screenshots);
            SaveFullPage(settingsView, Path.Combine(screenshots, "settings.png"));
            SaveWindowScreenshot(settingsView, Path.Combine(screenshots, "window-settings.png"));
            CaptureSettingsTabs(settingsView, outputRoot, size.Key, theme);
        }

        private static void CaptureSettingsTabs(
            GameSaveCenterSettingsView settingsView,
            string outputRoot,
            string sizeKey,
            GameSaveCenterThemeMode theme)
        {
            var tabControls = FindVisualChildren<TabControl>(settingsView).ToList();
            var screenshots = Path.Combine(outputRoot, "screenshots", sizeKey, theme.ToString().ToLowerInvariant());
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
                    var dedupeKey = sizeKey + "|" + theme + "|" + index + "|" + safe;
                    if (!captured.Add(dedupeKey))
                        continue;
                    SaveFullPage(settingsView, Path.Combine(screenshots, $"settings-{index}-{safe}.png"));
                    SaveWindowScreenshot(settingsView, Path.Combine(screenshots, $"window-settings-{safe}.png"));
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

        private static Rect ComputeAuditWindowBounds(FrameworkElement reference)
        {
            // Playnite is DPI-unaware in this host, so SystemParameters.WorkArea is already
            // the correct logical work area (e.g. 1707x912 DIP for a 2560x1440 display).
            // GetSystemMetrics would return virtualized small metrics here and must not be used.
            var workArea = SystemParameters.WorkArea;
            return new Rect(
                workArea.Left,
                workArea.Top,
                Math.Max(640, workArea.Width),
                Math.Max(480, workArea.Height));
        }

        private static double GetRenderScale(FrameworkElement reference)
        {
            // The multi-size sweep renders many large bitmaps in one 32-bit process. Keep
            // the export at logical resolution so the full page is captured for every window
            // size without exhausting memory; DPI evidence is recorded in metadata.json.
            return 1d;
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

        private sealed class AuditWindowSize
        {
            internal AuditWindowSize(string key, double width, double height)
            {
                Key = key;
                Width = width;
                Height = height;
            }

            internal string Key { get; }
            internal double Width { get; }
            internal double Height { get; }
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
