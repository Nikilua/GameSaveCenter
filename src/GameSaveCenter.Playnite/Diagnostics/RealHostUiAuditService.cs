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
    /// real-host-audit.request sentinel exists. It captures three clearly separated kinds
    /// of evidence: embedded-current viewport, controlled-host viewport, and individual
    /// ScrollSurfaceFull captures. It never triggers business actions.
    /// </summary>
    internal static class RealHostUiAuditService
    {
        private static readonly ILogger Logger = LogManager.GetLogger();
        private static readonly HashSet<string> CompletedRoots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly object StateLock = new object();
        private static readonly Dictionary<string, AuditCaptureSession> Sessions =
            new Dictionary<string, AuditCaptureSession>(StringComparer.OrdinalIgnoreCase);
        private static bool dashboardCaptureStarted;
        private static bool settingsCaptureStarted;
        private static string? requestedOutputRoot;
        private static Window? auditDashboardWindow;
        private static Window? auditSettingsWindow;

        internal static void NotifyUserToOpenDashboard(GameSaveCenterPlugin plugin)
        {
            const string message = "UI 审计已准备好，请在 Playnite 左侧点击 GameSaveCenter 打开插件界面。";
            Logger.Info("Real host audit is waiting for the user to open GameSaveCenter in Playnite.");
            try
            {
                plugin.PlayniteApi.Notifications.Add("GameSaveCenter.UI Audit", message, NotificationType.Info);
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Could not show the Playnite notification for the real host audit prompt.");
            }
        }

        /// <summary>
        /// Opens the real DashboardView inside a dedicated borderless window when Playnite's
        /// own window is not available. The window client area equals the requested profile;
        /// the Dashboard stretches and its Width/Height stay unset.
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
                    dashboard.ClearValue(FrameworkElement.WidthProperty);
                    dashboard.ClearValue(FrameworkElement.HeightProperty);
                    dashboard.HorizontalAlignment = HorizontalAlignment.Stretch;
                    dashboard.VerticalAlignment = VerticalAlignment.Stretch;
                    var window = new Window
                    {
                        Title = "GameSaveCenter Real Host Audit",
                        Width = auditBounds.Width,
                        Height = auditBounds.Height,
                        Left = auditBounds.Left,
                        Top = auditBounds.Top,
                        WindowStartupLocation = WindowStartupLocation.Manual,
                        SizeToContent = SizeToContent.Manual,
                        WindowStyle = WindowStyle.None,
                        ResizeMode = ResizeMode.NoResize,
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
        /// Opens the settings view in a dedicated borderless window when Playnite cannot show
        /// the plugin settings dialog in the current desktop session.
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
                    settings.ClearValue(FrameworkElement.WidthProperty);
                    settings.ClearValue(FrameworkElement.HeightProperty);
                    settings.HorizontalAlignment = HorizontalAlignment.Stretch;
                    settings.VerticalAlignment = VerticalAlignment.Stretch;
                    var window = new Window
                    {
                        Title = "GameSaveCenter Settings Real Host Audit",
                        Width = auditBounds.Width,
                        Height = auditBounds.Height,
                        Left = auditBounds.Left,
                        Top = auditBounds.Top,
                        WindowStartupLocation = WindowStartupLocation.Manual,
                        SizeToContent = SizeToContent.Manual,
                        WindowStyle = WindowStyle.None,
                        ResizeMode = ResizeMode.NoResize,
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
            var kind = dashboard.AuditHostKind;
            var captureKey = root + "|" + kind.ToString().ToLowerInvariant() + "-dashboard";
            if (!CompletedRoots.Add(captureKey))
                return;
            var session = ResetSession(root);
            session.CommitSha = ResolveCommitSha();
            Logger.Info("Real host audit requested; scheduling " + kind + " Dashboard capture to " + root);

            var dispatcher = dashboard.Dispatcher;
            dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(async () =>
            {
                try
                {
                    await CaptureDashboardAsync(dashboard, root, kind, session);
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
            var captureKey = settingsRoot + "|settings";
            if (!CompletedRoots.Add(captureKey))
                return;
            var session = GetOrCreateSession(root);

            var dispatcher = settingsView.Dispatcher;
            dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                try
                {
                    CaptureSettings(settingsView, settingsRoot, session);
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

        private static async System.Threading.Tasks.Task CaptureDashboardAsync(
            DashboardView dashboard,
            string outputRoot,
            AuditHostKind kind,
            AuditCaptureSession session)
        {
            Directory.CreateDirectory(outputRoot);
            Logger.Info("Real host audit capture started: " + outputRoot);
            var dpi = VisualTreeHelper.GetDpi(dashboard);
            var isEmbedded = IsGenuinelyEmbeddedDashboard(dashboard);
            if (kind == AuditHostKind.EmbeddedPlaynite && !isEmbedded)
                Logger.Warn("Audit origin hint is EmbeddedPlaynite but the dashboard is not hosted by Playnite; classified as controlled.");
            var mode = isEmbedded ? CaptureModeKind.EmbeddedCurrent : CaptureModeKind.ControlledHostWindow;
            var metadata = new UiHostMetadata
            {
                Mode = mode.ToString().ToLowerInvariant(),
                CaptureOrigin = isEmbedded ? "EmbeddedPlaynite" : "DedicatedAuditWindow",
                DashboardWasAlreadyHostedByPlaynite = isEmbedded,
                DedicatedAuditWindowUsed = !isEmbedded,
                ProfileSizeApplied = !isEmbedded,
                ThemeOverrideApplied = !isEmbedded,
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

            UiDiagnosticsExporters.WriteJson(metadata, Path.Combine(outputRoot, "metadata.json"));
            UiDiagnosticsExporters.WriteJson(BuildResourceSnapshot(dashboard), Path.Combine(outputRoot, "resource-snapshot.json"));
            UiDiagnosticsExporters.WriteJson(
                UiDiagnosticsExporters.BuildStyleFingerprints(dashboard),
                Path.Combine(outputRoot, "style-fingerprints.json"));
            UiDiagnosticsExporters.WriteJson(
                UiDiagnosticsExporters.BuildVisualTree(dashboard),
                Path.Combine(outputRoot, "visual-tree-dashboard.json"));

            if (isEmbedded)
            {
                session.EmbeddedDashboardCaptured = true;
                await CaptureEmbeddedCurrentAsync(dashboard, outputRoot, metadata, session.EmbeddedDashboard);
            }
            else
            {
                session.ControlledDashboardCaptured = true;
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
                        await CaptureControlledAtSizeAsync(dashboard, outputRoot, size, theme, metadata, session.ControlledDashboard);
                    }
                }
            }

            ScheduleFinalize(outputRoot, session);
            RequestSettingsCapture(dashboard);
            CloseAuditWindow(auditDashboardWindow);
            TryDeleteSentinel();
        }

        private static async System.Threading.Tasks.Task CaptureEmbeddedCurrentAsync(
            DashboardView dashboard,
            string outputRoot,
            UiHostMetadata metadata,
            List<CaptureManifestEntry> manifest)
        {
            // Embedded contract: capture exactly what Playnite hosts right now. Do not resize
            // the Dashboard, do not override the theme, and do not resize any host window.
            var baseDir = Path.Combine(outputRoot, "embedded-current", "dashboard");
            var viewportDir = Path.Combine(baseDir, "viewport");
            var scrollDir = Path.Combine(baseDir, "scroll-surfaces");
            var layoutDir = Path.Combine(outputRoot, "layout", "embedded-current");
            Directory.CreateDirectory(viewportDir);
            Directory.CreateDirectory(scrollDir);
            Directory.CreateDirectory(layoutDir);

            dashboard.ApplyWorkspaceForAudit(WorkspaceKind.Overview);
            await WaitForRenderAsync(dashboard.Dispatcher);
            foreach (var workspace in WorkspaceKinds)
            {
                dashboard.ApplyWorkspaceForAudit(workspace);
                await WaitForRenderAsync(dashboard.Dispatcher);
                var safe = workspace.ToString().ToLowerInvariant();
                SaveViewport(
                    dashboard,
                    Path.Combine(viewportDir, $"{safe}.png"),
                    outputRoot,
                    "workspace-" + safe,
                    workspace.ToString(),
                    string.Empty,
                    metadata,
                    manifest);
                CaptureScrollSurfaces(dashboard, scrollDir, outputRoot, "workspace-" + safe, workspace.ToString(), string.Empty, metadata.CaptureOrigin, manifest, "Dashboard");
                UiDiagnosticsExporters.WriteJson(
                    new Dictionary<string, double>
                    {
                        ["dashboardWidth"] = dashboard.ActualWidth,
                        ["dashboardHeight"] = dashboard.ActualHeight,
                        ["detailsTabWidth"] = dashboard.DetailsTabControlForAudit?.ActualWidth ?? 0,
                        ["detailsTabHeight"] = dashboard.DetailsTabControlForAudit?.ActualHeight ?? 0
                    },
                    Path.Combine(layoutDir, $"workspace-{safe}.json"));
            }

            await CaptureAllInnerTabs(dashboard, viewportDir, scrollDir, outputRoot, "embedded-current", metadata, manifest);
        }

        private static async System.Threading.Tasks.Task CaptureControlledAtSizeAsync(
            DashboardView dashboard,
            string outputRoot,
            AuditWindowSize size,
            GameSaveCenterThemeMode theme,
            UiHostMetadata metadata,
            List<CaptureManifestEntry> manifest)
        {
            var window = auditDashboardWindow;
            if (window != null)
            {
                // Borderless host: Window outer size equals client content size. The profile
                // is applied to the window and the Dashboard stretches to fill the client.
                window.Width = size.Width;
                window.Height = size.Height;
                window.Left = 0;
                window.Top = 0;
            }
            dashboard.ClearValue(FrameworkElement.WidthProperty);
            dashboard.ClearValue(FrameworkElement.HeightProperty);
            dashboard.HorizontalAlignment = HorizontalAlignment.Stretch;
            dashboard.VerticalAlignment = VerticalAlignment.Stretch;
            dashboard.ApplyThemeForAudit(theme);
            dashboard.ApplyWorkspaceForAudit(WorkspaceKind.Overview);
            var stable = await StabilizeControlledLayoutAsync(dashboard, size, outputRoot, theme);

            var sizeOk =
                Math.Abs(dashboard.ActualWidth - size.Width) <= 2d
                && Math.Abs(dashboard.ActualHeight - size.Height) <= 2d;
            if (!sizeOk)
            {
                WriteGate(
                    "CAPTURE_PROFILE_SIZE_MISMATCH",
                    $"Profile {size.Key} requested {size.Width:0}x{size.Height:0}, Dashboard actual {dashboard.ActualWidth:0}x{dashboard.ActualHeight:0}.",
                    outputRoot);
            }

            var themeKey = theme.ToString().ToLowerInvariant();
            var viewportDir = Path.Combine(outputRoot, "controlled", size.Key, themeKey, "viewport");
            var scrollDir = Path.Combine(outputRoot, "controlled", size.Key, themeKey, "scroll-surfaces");
            var windowDir = Path.Combine(outputRoot, "controlled", size.Key, themeKey, "window");
            var layoutDir = Path.Combine(outputRoot, "layout", "controlled", size.Key, themeKey);
            Directory.CreateDirectory(viewportDir);
            Directory.CreateDirectory(scrollDir);
            Directory.CreateDirectory(windowDir);
            Directory.CreateDirectory(layoutDir);

            metadata.Mode = "controlled-host-window";
            metadata.CaptureOrigin = "DedicatedAuditWindow";
            metadata.DashboardWasAlreadyHostedByPlaynite = false;
            metadata.DedicatedAuditWindowUsed = true;
            metadata.ProfileSizeApplied = true;
            metadata.ThemeOverrideApplied = true;
            metadata.DashboardWidth = Math.Round(dashboard.ActualWidth, 2);
            metadata.DashboardHeight = Math.Round(dashboard.ActualHeight, 2);
            metadata.DetailsTabControlWidth = Math.Round(dashboard.DetailsTabControlForAudit?.ActualWidth ?? 0, 2);
            metadata.DetailsTabControlHeight = Math.Round(dashboard.DetailsTabControlForAudit?.ActualHeight ?? 0, 2);
            metadata.ThemeMode = theme.ToString();
            UiDiagnosticsExporters.WriteJson(metadata, Path.Combine(outputRoot, "metadata-" + size.Key + "-" + themeKey + ".json"));
            UiDiagnosticsExporters.WriteJson(
                stable,
                Path.Combine(layoutDir, "responsive-stable.json"));

            foreach (var workspace in WorkspaceKinds)
            {
                dashboard.ApplyWorkspaceForAudit(workspace);
                await WaitForRenderAsync(dashboard.Dispatcher);
                var safe = workspace.ToString().ToLowerInvariant();
                SaveViewport(
                    dashboard,
                    Path.Combine(viewportDir, $"{safe}.png"),
                    outputRoot,
                    "workspace-" + safe,
                    workspace.ToString(),
                    string.Empty,
                    metadata,
                    manifest);
                CaptureScrollSurfaces(dashboard, scrollDir, outputRoot, "workspace-" + safe, workspace.ToString(), string.Empty, metadata.CaptureOrigin, manifest, "Dashboard");
                if (window != null)
                {
                    UiDiagnosticsExporters.SavePng(
                        window,
                        Path.Combine(windowDir, $"controlled-window-{safe}.png"),
                        GetRenderScale(window));
                }
                UiDiagnosticsExporters.WriteJson(
                    new Dictionary<string, double>
                    {
                        ["dashboardWidth"] = dashboard.ActualWidth,
                        ["dashboardHeight"] = dashboard.ActualHeight,
                        ["detailsTabWidth"] = dashboard.DetailsTabControlForAudit?.ActualWidth ?? 0,
                        ["detailsTabHeight"] = dashboard.DetailsTabControlForAudit?.ActualHeight ?? 0
                    },
                    Path.Combine(layoutDir, $"workspace-{safe}.json"));
            }

            await CaptureAllInnerTabs(dashboard, viewportDir, scrollDir, outputRoot, "controlled-" + size.Key + "-" + themeKey, metadata, manifest);
        }

        private static async System.Threading.Tasks.Task<Dictionary<string, object>> StabilizeControlledLayoutAsync(
            DashboardView dashboard,
            AuditWindowSize size,
            string outputRoot,
            GameSaveCenterThemeMode theme)
        {
            var lastWidth = double.NaN;
            var lastDetails = double.NaN;
            var passCount = 0;
            var stable = false;
            for (var pass = 0; pass < 3; pass++)
            {
                dashboard.ApplyWorkspaceForAudit(WorkspaceKind.Overview);
                dashboard.UpdateLayout();
                await dashboard.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.DataBind);
                await dashboard.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Loaded);
                await dashboard.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
                await dashboard.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                dashboard.UpdateLayout();
                await dashboard.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
                passCount++;
                var width = dashboard.ActualWidth;
                var details = dashboard.DetailsTabControlForAudit?.ActualWidth ?? 0;
                if (Math.Abs(width - lastWidth) <= 0.5d && Math.Abs(details - lastDetails) <= 0.5d)
                {
                    stable = true;
                    break;
                }
                lastWidth = width;
                lastDetails = details;
            }
            return new Dictionary<string, object>
            {
                ["ProfileKey"] = size.Key,
                ["Theme"] = theme.ToString(),
                ["ResponsivePassCount"] = passCount,
                ["ResponsiveStable"] = stable,
                ["DashboardActualWidth"] = Math.Round(dashboard.ActualWidth, 2),
                ["DetailsTabActualWidth"] = Math.Round(dashboard.DetailsTabControlForAudit?.ActualWidth ?? 0, 2)
            };
        }

        private static async System.Threading.Tasks.Task CaptureAllInnerTabs(
            DashboardView dashboard,
            string viewportDir,
            string scrollDir,
            string outputRoot,
            string routePrefix,
            UiHostMetadata metadata,
            List<CaptureManifestEntry> manifest)
        {
            var outer = dashboard.DetailsTabControlForAudit;
            var captured = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var workspace in WorkspaceKinds)
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
                        viewportDir,
                        scrollDir,
                        outputRoot,
                        routePrefix + "-" + workspacePrefix,
                        workspace.ToString(),
                        captured,
                        metadata,
                        manifest);
                }
            }
        }

        private static async System.Threading.Tasks.Task CaptureTabControlRecursive(
            DashboardView dashboard,
            TabControl tabControl,
            string viewportDir,
            string scrollDir,
            string outputRoot,
            string routePrefix,
            string workspace,
            HashSet<string> captured,
            UiHostMetadata metadata,
            List<CaptureManifestEntry> manifest)
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
                var dedupeKey = routePrefix + "|" + workspace + "|" + index + "|" + safe;
                if (!captured.Add(dedupeKey))
                    continue;
                var tabRoute = routePrefix + "-" + safe;
                SaveViewport(
                    dashboard,
                    Path.Combine(viewportDir, $"tab-{safe}.png"),
                    outputRoot,
                    tabRoute,
                    workspace,
                    safe,
                    metadata,
                    manifest);
                CaptureScrollSurfaces(dashboard, scrollDir, outputRoot, tabRoute, workspace, safe, metadata.CaptureOrigin, manifest, "Dashboard");

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
                        viewportDir,
                        scrollDir,
                        outputRoot,
                        tabRoute,
                        workspace,
                        captured,
                        metadata,
                        manifest);
                }
            }
        }

        private static void SaveViewport(
            FrameworkElement root,
            string path,
            string outputRoot,
            string route,
            string workspace,
            string tab,
            UiHostMetadata metadata,
            List<CaptureManifestEntry> manifest)
        {
            var scale = GetRenderScale(root);
            UiDiagnosticsExporters.SavePng(root, path, scale);
            CheckChildLayoutOverflow(root, outputRoot);
            var expected = UiDiagnosticsExporters.ExpectedBitmapSize(root, scale);
            var actual = UiDiagnosticsExporters.ReadPngSize(path);
            var validated = actual.HasValue
                && Math.Abs(actual.Value.Width - expected.Width) <= 2
                && Math.Abs(actual.Value.Height - expected.Height) <= 2
                && IsBoundsWithinViewport(
                    new Rect(0, 0, root.ActualWidth, root.ActualHeight),
                    expected.Width / scale,
                    expected.Height / scale,
                    2d);
            if (!validated)
            {
                WriteGate(
                    "CAPTURE_VIEWPORT_CLIPPED",
                    $"Expected {expected.Width}x{expected.Height}, actual {actual?.Width}x{actual?.Height} for {path}.",
                    outputRoot);
            }
            manifest.Add(new CaptureManifestEntry
            {
                File = RelativeTo(path, outputRoot),
                Scope = metadata.Mode.IndexOf("settings", StringComparison.OrdinalIgnoreCase) >= 0 ? "Settings" : "Dashboard",
                CaptureType = "Viewport",
                Origin = metadata.CaptureOrigin,
                Route = route,
                Workspace = workspace,
                Tab = tab,
                DashboardWidthDip = Math.Round(root.ActualWidth, 2),
                DashboardHeightDip = Math.Round(root.ActualHeight, 2),
                DpiScaleX = Math.Round(metadata.DpiScaleX, 2),
                DpiScaleY = Math.Round(metadata.DpiScaleY, 2),
                RenderScale = scale,
                OutputWidthPx = actual?.Width ?? 0,
                OutputHeightPx = actual?.Height ?? 0,
                CompletenessValidated = validated
            });
        }

        private static void CaptureScrollSurfaces(
            FrameworkElement root,
            string outDir,
            string outputRoot,
            string route,
            string workspace,
            string tab,
            string origin,
            List<CaptureManifestEntry> manifest,
            string scope)
        {
            var scrollers = FindMeaningfulScrollSurfaces(root);
            var index = 0;
            foreach (var scroller in scrollers)
            {
                var name = string.IsNullOrWhiteSpace(scroller.Name) ? "scroller-" + index : scroller.Name;
                index++;
                var file = $"{SafeFileName(route)}__{SafeFileName(name)}.png";
                var path = Path.Combine(outDir, file);
                var status = DecideScrollSurfaceStatus(scroller, out var reason);
                var segments = scroller.ViewportHeight > 0
                    ? (int)Math.Ceiling(scroller.ExtentHeight / scroller.ViewportHeight)
                    : 1;
                var entry = new CaptureManifestEntry
                {
                    File = RelativeTo(path, outputRoot),
                    Scope = scope,
                    CaptureType = "ScrollSurfaceFull",
                    Origin = origin,
                    Route = route,
                    Workspace = workspace,
                    Tab = tab,
                    ScrollerName = name,
                    ViewportHeight = Math.Round(scroller.ViewportHeight, 2),
                    ExtentHeight = Math.Round(scroller.ExtentHeight, 2),
                    SegmentCount = segments,
                    CompletenessValidated = false
                };
                if (status == ScrollSurfaceStatus.CapturedAndValidated)
                {
                    try
                    {
                        UiDiagnosticsExporters.SaveScrollViewerFull(scroller, path);
                        var size = UiDiagnosticsExporters.ReadPngSize(path);
                        // The stitch renderer outputs 1.0x for the slice path and up to 1.5x
                        // for the direct content fast path. Accept both while still proving
                        // the page was captured to its full extent.
                        var expectedMin = (int)Math.Ceiling(scroller.ExtentHeight * 0.95);
                        var expectedMax = (int)Math.Ceiling(scroller.ExtentHeight * 1.55);
                        var heightOk = size.HasValue
                            && size.Value.Height >= (int)Math.Ceiling(scroller.ViewportHeight)
                            && size.Value.Height >= expectedMin
                            && size.Value.Height <= expectedMax;
                        entry.OutputWidthPx = size?.Width ?? 0;
                        entry.OutputHeightPx = size?.Height ?? 0;
                        entry.CompletenessValidated = heightOk;
                        entry.CaptureStatus = heightOk ? "CapturedAndValidated" : "CapturedUnvalidated";
                        entry.Reason = heightOk ? null : "Output height does not match scroll extent.";
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug(ex, "Real host audit scroll surface capture failed: " + file);
                        entry.CaptureStatus = "Failed";
                        entry.Reason = ex.Message;
                    }
                }
                else
                {
                    entry.CaptureStatus = status.ToString();
                    entry.Reason = reason;
                }
                manifest.Add(entry);
            }
        }

        internal static ScrollSurfaceStatus DecideScrollSurfaceStatus(ScrollViewer scroller, out string reason)
        {
            reason = string.Empty;
            if (IsVirtualizedDataGridScroller(scroller))
            {
                reason = "Virtualized DataGrid with logical item scrolling; use viewport and scroll-regression evidence instead of a pixel stitch.";
                return ScrollSurfaceStatus.SkippedVirtualized;
            }
            var segments = scroller.ViewportHeight > 0
                ? (int)Math.Ceiling(scroller.ExtentHeight / scroller.ViewportHeight)
                : 1;
            if (segments > 60)
            {
                reason = "Scroll surface requires more than 60 stitch segments.";
                return ScrollSurfaceStatus.SkippedTooLarge;
            }
            return ScrollSurfaceStatus.CapturedAndValidated;
        }

        internal static bool IsVirtualizedDataGridScroller(ScrollViewer scroller)
        {
            if (string.Equals(scroller.Name, "DG_ScrollViewer", StringComparison.OrdinalIgnoreCase))
                return true;
            var parent = VisualTreeHelper.GetParent(scroller);
            while (parent != null)
            {
                if (parent is DataGrid)
                    return true;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return false;
        }

        internal static bool IsGenuinelyEmbeddedDashboard(DashboardView dashboard)
        {
            if (!dashboard.IsLoaded || PresentationSource.FromVisual(dashboard) == null)
                return false;
            var window = Window.GetWindow(dashboard);
            return window != null && !ReferenceEquals(window, auditDashboardWindow);
        }

        internal static bool IsAuditFallbackWindow(Window? window, Window? auditWindow)
        {
            return window != null && auditWindow != null && ReferenceEquals(window, auditWindow);
        }

        private static string ResolveCommitSha()
        {
            var env = Environment.GetEnvironmentVariable("GSC_UI_AUDIT_COMMIT");
            if (!string.IsNullOrWhiteSpace(env))
                return env.Trim();
            try
            {
                var informational = typeof(DashboardView).Assembly
                    .GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
                    .OfType<System.Reflection.AssemblyInformationalVersionAttribute>()
                    .FirstOrDefault();
                if (informational != null && !string.IsNullOrWhiteSpace(informational.InformationalVersion))
                    return informational.InformationalVersion;
            }
            catch
            {
                // Fall through to unknown.
            }
            return "unknown";
        }

        internal static List<ScrollViewer> FindMeaningfulScrollSurfaces(FrameworkElement root)
        {
            return FindVisualChildren<ScrollViewer>(root)
                .Where(scroller =>
                    scroller.Visibility == Visibility.Visible
                    && scroller.ActualWidth >= 60
                    && scroller.ActualHeight >= 60
                    && (scroller.ScrollableHeight > 8 || scroller.ScrollableWidth > 8))
                .Where(scroller => !string.Equals(scroller.Name, "DG_ScrollViewer", StringComparison.OrdinalIgnoreCase))
                .Where(scroller => !string.Equals(scroller.Name, "PART_ContentHost", StringComparison.OrdinalIgnoreCase))
                .Where(scroller => !IsInternalTemplateScroller(scroller))
                .Distinct()
                .ToList();
        }

        internal static bool IsInternalTemplateScroller(ScrollViewer scroller)
        {
            if (string.Equals(scroller.Name, "DG_ScrollViewer", StringComparison.OrdinalIgnoreCase)
                || string.Equals(scroller.Name, "PART_ContentHost", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            var parent = VisualTreeHelper.GetParent(scroller);
            while (parent != null)
            {
                if (parent is TextBox || parent is ComboBox)
                    return true;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return false;
        }

        internal static bool IsBoundsWithinViewport(Rect bounds, double viewportWidth, double viewportHeight, double tolerance)
        {
            return bounds.Left >= -tolerance
                && bounds.Top >= -tolerance
                && bounds.Right <= viewportWidth + tolerance
                && bounds.Bottom <= viewportHeight + tolerance;
        }

        private static Rect NormalizeBoundsToRootDips(Rect bounds, FrameworkElement root)
        {
            // Playnite can report TransformToAncestor coordinates in device pixels
            // while ActualWidth/ActualHeight remain DIPs. Normalize only when the
            // DPI-adjusted rectangle fits; genuine fixed-layout overflow stays intact
            // and still trips the audit gate.
            var dpi = VisualTreeHelper.GetDpi(root);
            var scaleX = dpi.DpiScaleX > 1.01 ? dpi.DpiScaleX : 1d;
            var scaleY = dpi.DpiScaleY > 1.01 ? dpi.DpiScaleY : 1d;
            if (scaleX == 1d && scaleY == 1d)
                return bounds;

            if (IsBoundsWithinViewport(bounds, root.ActualWidth, root.ActualHeight, 2d))
                return bounds;

            var normalized = new Rect(
                bounds.X / scaleX,
                bounds.Y / scaleY,
                bounds.Width / scaleX,
                bounds.Height / scaleY);
            return IsBoundsWithinViewport(normalized, root.ActualWidth, root.ActualHeight, 2d)
                ? normalized
                : bounds;
        }

        private static string RelativeTo(string path, string root)
        {
            var full = Path.GetFullPath(path);
            var baseDir = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return full.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase)
                ? full.Substring(baseDir.Length)
                : full;
        }

        private static void WriteGate(string code, string message, string outputRoot)
        {
            try
            {
                var dir = Path.Combine(outputRoot, "gates");
                Directory.CreateDirectory(dir);
                UiDiagnosticsExporters.WriteJson(
                    new Dictionary<string, string> { ["Code"] = code, ["Message"] = message },
                    Path.Combine(dir, code + ".json"));
                Logger.Error(code + ": " + message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Could not write audit gate " + code);
            }
        }

        internal static void CheckChildLayoutOverflow(FrameworkElement root, string outputRoot)
        {
            try
            {
                var rootWidth = root.ActualWidth;
                var rootHeight = root.ActualHeight;
                if (rootWidth <= 0 || rootHeight <= 0)
                    return;
                var real = new List<string>();
                var scrollable = new List<string>();
                var decorative = new List<string>();
                var falsePositive = new List<string>();
                foreach (var child in FindVisualChildren<FrameworkElement>(root))
                {
                    if (string.IsNullOrWhiteSpace(child.Name)
                        || child.Visibility != Visibility.Visible
                        || child.ActualWidth <= 0
                        || child.ActualHeight <= 0)
                    {
                        continue;
                    }
                    var bounds = child.TransformToAncestor(root).TransformBounds(
                        new Rect(0, 0, child.ActualWidth, child.ActualHeight));
                    bounds = NormalizeBoundsToRootDips(bounds, root);
                    if (!(bounds.Right > rootWidth + 2d || bounds.Bottom > rootHeight + 2d
                        || bounds.Left < -2d || bounds.Top < -2d))
                    {
                        continue;
                    }
                    var classification = ClassifyOverflow(child);
                    var detail = $"{child.Name}: {bounds.Left:0},{bounds.Top:0}-{bounds.Right:0},{bounds.Bottom:0} (root {rootWidth:0}x{rootHeight:0})";
                    switch (classification)
                    {
                        case OverflowClassification.RealFixedLayoutOverflow:
                            real.Add(detail);
                            break;
                        case OverflowClassification.IntentionalScrollableOverflow:
                            scrollable.Add(detail);
                            break;
                        case OverflowClassification.DecorativeOverflow:
                            decorative.Add(detail);
                            break;
                        default:
                            falsePositive.Add(detail);
                            break;
                    }
                }
                if (real.Count > 0)
                {
                    WriteGate(
                        "CHILD_LAYOUT_OVERFLOW",
                        "Fixed layout children exceed Dashboard bounds: " + string.Join("; ", real.Take(8)),
                        outputRoot);
                }
                else
                {
                    // A single audit output folder is reused across the responsive
                    // matrix. Do not leave a gate from an earlier, narrower pass in the
                    // final evidence after a later pass has validated the layout.
                    var staleGate = Path.Combine(outputRoot, "gates", "CHILD_LAYOUT_OVERFLOW.json");
                    if (File.Exists(staleGate))
                    {
                        File.Delete(staleGate);
                    }
                }
                UiDiagnosticsExporters.WriteJson(
                    new Dictionary<string, object>
                    {
                        ["RealFixedLayoutOverflow"] = real,
                        ["IntentionalScrollableOverflow"] = scrollable,
                        ["DecorativeOverflow"] = decorative,
                        ["AuditFalsePositive"] = falsePositive
                    },
                    Path.Combine(outputRoot, "gates", "overflow-classification.json"));
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, "Real host audit child layout overflow check failed.");
            }
        }

        internal static OverflowClassification ClassifyOverflow(FrameworkElement element)
        {
            if (IsInsideScrollableContent(element))
                return OverflowClassification.IntentionalScrollableOverflow;
            if (IsDecorativeOverflow(element))
                return OverflowClassification.DecorativeOverflow;
            return OverflowClassification.RealFixedLayoutOverflow;
        }

        internal static bool IsInsideScrollableContent(DependencyObject element)
        {
            var current = VisualTreeHelper.GetParent(element);
            while (current != null)
            {
                if (current is ScrollViewer
                    || current is ScrollContentPresenter
                    || current is ItemsPresenter
                    || current is System.Windows.Controls.Primitives.DataGridCellsPresenter
                    || current is DataGrid)
                {
                    return true;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return false;
        }

        internal static bool IsDecorativeOverflow(FrameworkElement element)
        {
            if (element.Effect is System.Windows.Media.Effects.DropShadowEffect
                || element.Effect is System.Windows.Media.Effects.BlurEffect)
            {
                return true;
            }
            if (element.IsHitTestVisible == false)
                return true;
            return string.Equals(element.Name, "AmbientGlowLayer", StringComparison.OrdinalIgnoreCase);
        }

        internal enum OverflowClassification
        {
            RealFixedLayoutOverflow,
            IntentionalScrollableOverflow,
            DecorativeOverflow,
            AuditFalsePositive
        }

        private static AuditCaptureSession GetOrCreateSession(string root)
        {
            lock (StateLock)
            {
                if (!Sessions.TryGetValue(root, out var session))
                {
                    session = new AuditCaptureSession();
                    Sessions[root] = session;
                }
                return session;
            }
        }

        private static AuditCaptureSession ResetSession(string root)
        {
            lock (StateLock)
            {
                var session = new AuditCaptureSession();
                Sessions[root] = session;
                return session;
            }
        }

        private static void ScheduleFinalize(string outputRoot, AuditCaptureSession session)
        {
            System.Threading.Tasks.Task.Run(async () =>
            {
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(150));
                FinalizeSession(outputRoot, session);
            });
        }

        private static void FinalizeSession(string outputRoot, AuditCaptureSession session)
        {
            lock (session.Sync)
            {
                if (session.Finalized)
                    return;
                session.Finalized = true;
            }
            try
            {
                var highGates = 0;
                if (!session.EmbeddedDashboardCaptured)
                {
                    WriteGate(
                        "REAL_EMBEDDED_DASHBOARD_NOT_CAPTURED",
                        "The real Playnite-hosted Dashboard was not captured; Controlled Host evidence is not production visual truth.",
                        outputRoot);
                    highGates++;
                }
                if (string.IsNullOrWhiteSpace(session.CommitSha) || session.CommitSha == "unknown")
                {
                    WriteGate(
                        "AUDIT_SOURCE_REVISION_MISSING",
                        "GSC_UI_AUDIT_COMMIT was not set and no assembly informational version was available.",
                        outputRoot);
                    highGates++;
                }
                var gateFiles = Directory.Exists(Path.Combine(outputRoot, "gates"))
                    ? Directory.GetFiles(Path.Combine(outputRoot, "gates"), "*.json").Length
                    : 0;
                Directory.CreateDirectory(Path.Combine(outputRoot, "embedded-current", "dashboard"));
                Directory.CreateDirectory(Path.Combine(outputRoot, "controlled"));
                if (session.EmbeddedDashboard.Count > 0)
                {
                    UiDiagnosticsExporters.WriteJson(
                        session.EmbeddedDashboard,
                        Path.Combine(outputRoot, "embedded-current", "dashboard", "capture-manifest.json"));
                }
                if (session.ControlledDashboard.Count > 0)
                {
                    UiDiagnosticsExporters.WriteJson(
                        session.ControlledDashboard,
                        Path.Combine(outputRoot, "controlled", "capture-manifest.json"));
                }
                UiDiagnosticsExporters.WriteJson(
                    session.Settings,
                    Path.Combine(outputRoot, "settings", "capture-manifest.json"));
                var aggregate = new List<CaptureManifestEntry>();
                aggregate.AddRange(session.EmbeddedDashboard);
                aggregate.AddRange(session.ControlledDashboard);
                aggregate.AddRange(session.Settings);
                UiDiagnosticsExporters.WriteJson(aggregate, Path.Combine(outputRoot, "capture-manifest.json"));
                UiDiagnosticsExporters.WriteJson(
                    new Dictionary<string, object>
                    {
                        ["CommitSha"] = session.CommitSha,
                        ["EmbeddedDashboardCaptured"] = session.EmbeddedDashboardCaptured,
                        ["EmbeddedSettingsCaptured"] = session.EmbeddedSettingsCaptured,
                        ["ControlledDashboardCaptured"] = session.ControlledDashboardCaptured,
                        ["ProductionVisualSourceOfTruthAvailable"] = session.EmbeddedDashboardCaptured,
                        ["EmbeddedDashboardOrigin"] = session.EmbeddedDashboardCaptured ? "EmbeddedPlaynite" : "None",
                        ["EmbeddedSettingsOrigin"] = session.EmbeddedSettingsCaptured ? "EmbeddedPlaynite" : "None",
                        ["HighGateCount"] = highGates + gateFiles
                    },
                    Path.Combine(outputRoot, "summary.json"));
                CreateZip(outputRoot);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Real host audit finalize failed.");
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

        private static void CaptureSettings(GameSaveCenterSettingsView settingsView, string outputRoot, AuditCaptureSession session)
        {
            Directory.CreateDirectory(outputRoot);
            Logger.Info("Real host settings capture started: " + outputRoot);
            var settingsEmbedded = auditSettingsWindow == null;
            if (settingsEmbedded)
                session.EmbeddedSettingsCaptured = true;
            else
                session.ControlledSettingsCaptured = true;
            var maximized = ComputeAuditWindowBounds(settingsView);
            var sizes = new[]
            {
                new AuditWindowSize("maximized", maximized.Width, maximized.Height),
                new AuditWindowSize("1600x1000", Math.Min(1600d, maximized.Width), Math.Min(1000d, maximized.Height)),
                new AuditWindowSize("1366x768", Math.Min(1366d, maximized.Width), Math.Min(768d, maximized.Height)),
                new AuditWindowSize("1280x720", Math.Min(1280d, maximized.Width), Math.Min(720d, maximized.Height)),
                new AuditWindowSize("1024x768", Math.Min(1024d, maximized.Width), Math.Min(768d, maximized.Height))
            };

            var themes = settingsEmbedded
                ? new[] { (settingsView.DataContext as GameSaveCenterSettings)?.ThemeMode ?? GameSaveCenterThemeMode.FollowPlaynite }
                : new[] { GameSaveCenterThemeMode.Light, GameSaveCenterThemeMode.Dark };
            foreach (var size in settingsEmbedded ? new[] { sizes[0] } : sizes)
            {
                foreach (var theme in themes)
                {
                    CaptureSettingsAtSize(settingsView, outputRoot, size, theme, session.Settings);
                }
            }

            FinalizeSession(Path.GetDirectoryName(outputRoot) ?? outputRoot, session);
            Logger.Info("Real host settings capture finished: " + outputRoot);
            CloseAuditWindow(auditSettingsWindow);
            TryDeleteSentinel();
        }

        private static void CaptureSettingsAtSize(
            GameSaveCenterSettingsView settingsView,
            string outputRoot,
            AuditWindowSize size,
            GameSaveCenterThemeMode theme,
            List<CaptureManifestEntry> manifest)
        {
            var window = auditSettingsWindow;
            var controlled = window != null;
            if (window != null)
            {
                // Borderless controlled host: profile is the client size.
                window.Width = size.Width;
                window.Height = size.Height;
                window.Left = 0;
                window.Top = 0;
            }
            if (controlled)
            {
                settingsView.ClearValue(FrameworkElement.WidthProperty);
                settingsView.ClearValue(FrameworkElement.HeightProperty);
                settingsView.HorizontalAlignment = HorizontalAlignment.Stretch;
                settingsView.VerticalAlignment = VerticalAlignment.Stretch;
                settingsView.ApplyThemeForAudit(theme);
            }
            settingsView.UpdateLayout();
            settingsView.Dispatcher.Invoke(() => { }, DispatcherPriority.Render);

            var themeKey = theme.ToString().ToLowerInvariant();
            var dpi = VisualTreeHelper.GetDpi(settingsView);
            var metadata = new UiHostMetadata
            {
                Mode = controlled ? "controlled-host-settings" : "embedded-current-settings",
                CaptureOrigin = controlled ? "DedicatedAuditWindow" : "EmbeddedPlaynite",
                DashboardWasAlreadyHostedByPlaynite = !controlled,
                DedicatedAuditWindowUsed = controlled,
                ProfileSizeApplied = controlled,
                ThemeOverrideApplied = controlled,
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
                ThemeMode = theme.ToString(),
                HighContrast = SystemParameters.HighContrast
            };

            var sizeOk = !controlled
                || (Math.Abs(settingsView.ActualWidth - size.Width) <= 2d
                    && Math.Abs(settingsView.ActualHeight - size.Height) <= 2d);
            if (!sizeOk)
            {
                WriteGate(
                    "CAPTURE_PROFILE_SIZE_MISMATCH",
                    $"Settings profile {size.Key} requested {size.Width:0}x{size.Height:0}, actual {settingsView.ActualWidth:0}x{settingsView.ActualHeight:0}.",
                    outputRoot);
            }

            UiDiagnosticsExporters.WriteJson(metadata, Path.Combine(outputRoot, "metadata-" + size.Key + "-" + themeKey + ".json"));
            UiDiagnosticsExporters.WriteJson(
                UiDiagnosticsExporters.BuildResourceSnapshot(settingsView.Resources, "SettingsView"),
                Path.Combine(outputRoot, "resource-snapshot-" + size.Key + "-" + themeKey + ".json"));
            UiDiagnosticsExporters.WriteJson(
                UiDiagnosticsExporters.BuildStyleFingerprints(settingsView),
                Path.Combine(outputRoot, "style-fingerprints-" + size.Key + "-" + themeKey + ".json"));
            UiDiagnosticsExporters.WriteJson(
                UiDiagnosticsExporters.BuildVisualTree(settingsView),
                Path.Combine(outputRoot, "visual-tree-" + size.Key + "-" + themeKey + ".json"));

            var baseDir = controlled
                ? Path.Combine(outputRoot, "controlled", size.Key, themeKey)
                : Path.Combine(outputRoot, "embedded-current");
            var viewportDir = Path.Combine(baseDir, "viewport");
            var scrollDir = Path.Combine(baseDir, "scroll-surfaces");
            var windowDir = Path.Combine(baseDir, "window");
            Directory.CreateDirectory(viewportDir);
            Directory.CreateDirectory(scrollDir);
            Directory.CreateDirectory(windowDir);
            SaveViewport(settingsView, Path.Combine(viewportDir, "settings.png"), outputRoot, "settings", "Settings", string.Empty, metadata, manifest);
            if (window != null)
            {
                UiDiagnosticsExporters.SavePng(
                    window,
                    Path.Combine(windowDir, "controlled-window-settings.png"),
                    GetRenderScale(window));
            }
            CaptureSettingsTabs(settingsView, baseDir, outputRoot, size.Key, themeKey, metadata, manifest);
        }

        private static void CaptureSettingsTabs(
            GameSaveCenterSettingsView settingsView,
            string baseDir,
            string outputRoot,
            string sizeKey,
            string themeKey,
            UiHostMetadata metadata,
            List<CaptureManifestEntry> manifest)
        {
            var tabControls = FindVisualChildren<TabControl>(settingsView).ToList();
            var viewportDir = Path.Combine(baseDir, "viewport");
            var scrollDir = Path.Combine(baseDir, "scroll-surfaces");
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
                    var dedupeKey = sizeKey + "|" + themeKey + "|" + index + "|" + safe;
                    if (!captured.Add(dedupeKey))
                        continue;
                    SaveViewport(
                        settingsView,
                        Path.Combine(viewportDir, $"settings-{index}-{safe}.png"),
                        outputRoot,
                        "settings-" + safe,
                        "Settings",
                        safe,
                        metadata,
                        manifest);
                    CaptureScrollSurfaces(settingsView, scrollDir, outputRoot, "settings-" + safe, "Settings", safe, metadata.CaptureOrigin, manifest, "Settings");
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

        internal static string SafeFileName(string value)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var builder = new System.Text.StringBuilder(value.Length);
            var lastDash = false;
            foreach (var ch in value)
            {
                if (invalid.Contains(ch))
                {
                    if (!lastDash && builder.Length > 0)
                    {
                        builder.Append('-');
                        lastDash = true;
                    }
                }
                else
                {
                    builder.Append(ch);
                    lastDash = false;
                }
            }
            var result = builder.ToString().ToLowerInvariant().Trim().Trim('-', '.', ' ');
            return result.Length > 0 ? result : "item";
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
            // DashboardView applies its final responsive constraints at ApplicationIdle,
            // after Grid row/column changes have been arranged. Capture only after that
            // pass so the audit does not report a transient pre-convergence DesiredSize.
            await dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            await System.Threading.Tasks.Task.Delay(90);
        }

        private static Rect ComputeAuditWindowBounds(FrameworkElement reference)
        {
            // Playnite is DPI-unaware in this host, so SystemParameters.WorkArea is already
            // the correct logical work area (e.g. 1707x912 DIP for a 2560x1440 display).
            var workArea = SystemParameters.WorkArea;
            return new Rect(
                workArea.Left,
                workArea.Top,
                Math.Max(640, workArea.Width),
                Math.Max(480, workArea.Height));
        }

        private static double GetRenderScale(FrameworkElement reference)
        {
            var dpi = VisualTreeHelper.GetDpi(reference);
            var scale = Math.Max(dpi.DpiScaleX, dpi.DpiScaleY);
            return scale > 0 ? Math.Min(1.5d, scale) : 1d;
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
                using var currentProcess = Process.GetCurrentProcess();
                var processPath = currentProcess.MainModule?.FileName;
                if (!string.IsNullOrWhiteSpace(processPath) && File.Exists(processPath))
                {
                    var info = FileVersionInfo.GetVersionInfo(processPath);
                    if (!string.IsNullOrWhiteSpace(info.FileVersion) && info.FileVersion != "1.0.0.0")
                        return info.FileVersion;
                    if (!string.IsNullOrWhiteSpace(info.ProductVersion) && info.ProductVersion != "1.0.0.0")
                        return info.ProductVersion;
                }
                var version = currentProcess.MainModule?.FileVersionInfo;
                if (version == null)
                    return "unknown";
                return !string.IsNullOrWhiteSpace(version.FileVersion) && version.FileVersion != "1.0.0.0"
                    ? version.FileVersion
                    : !string.IsNullOrWhiteSpace(version.ProductVersion) && version.ProductVersion != "1.0.0.0"
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

        private static readonly WorkspaceKind[] WorkspaceKinds =
        {
            WorkspaceKind.Overview,
            WorkspaceKind.Saves,
            WorkspaceKind.Trainers,
            WorkspaceKind.Media,
            WorkspaceKind.Tasks,
            WorkspaceKind.Maintenance
        };

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
            public string Mode { get; set; } = "controlled-host-window";
            public string CaptureOrigin { get; set; } = "DedicatedAuditWindow";
            public bool DashboardWasAlreadyHostedByPlaynite { get; set; }
            public bool DedicatedAuditWindowUsed { get; set; }
            public bool ProfileSizeApplied { get; set; }
            public bool ThemeOverrideApplied { get; set; }
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

        internal enum CaptureModeKind
        {
            EmbeddedCurrent,
            ControlledHostWindow
        }

        internal enum AuditHostKind
        {
            EmbeddedPlaynite,
            ControlledAuditWindow
        }

        internal enum ScrollSurfaceStatus
        {
            CapturedAndValidated,
            CapturedUnvalidated,
            SkippedVirtualized,
            SkippedTooLarge,
            Failed
        }

        internal sealed class AuditCaptureSession
        {
            internal object Sync { get; } = new object();
            internal bool Finalized { get; set; }
            internal string CommitSha { get; set; } = "unknown";
            internal bool EmbeddedDashboardCaptured { get; set; }
            internal bool ControlledDashboardCaptured { get; set; }
            internal bool EmbeddedSettingsCaptured { get; set; }
            internal bool ControlledSettingsCaptured { get; set; }
            internal List<CaptureManifestEntry> EmbeddedDashboard { get; } = new List<CaptureManifestEntry>();
            internal List<CaptureManifestEntry> ControlledDashboard { get; } = new List<CaptureManifestEntry>();
            internal List<CaptureManifestEntry> Settings { get; } = new List<CaptureManifestEntry>();
        }

        internal sealed class CaptureManifestEntry
        {
            public string File { get; set; } = string.Empty;
            public string Scope { get; set; } = string.Empty;
            public string CaptureType { get; set; } = "Viewport";
            public string Origin { get; set; } = "DedicatedAuditWindow";
            public string Route { get; set; } = string.Empty;
            public string Workspace { get; set; } = string.Empty;
            public string Tab { get; set; } = string.Empty;
            public double DashboardWidthDip { get; set; }
            public double DashboardHeightDip { get; set; }
            public double DpiScaleX { get; set; } = 1.0;
            public double DpiScaleY { get; set; } = 1.0;
            public double RenderScale { get; set; } = 1.0;
            public int OutputWidthPx { get; set; }
            public int OutputHeightPx { get; set; }
            public string? ScrollerName { get; set; }
            public double? ViewportHeight { get; set; }
            public double? ExtentHeight { get; set; }
            public int? SegmentCount { get; set; }
            public bool CompletenessValidated { get; set; }
            public string? CaptureStatus { get; set; }
            public string? Reason { get; set; }
        }
    }
}
