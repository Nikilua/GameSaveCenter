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
        private static readonly List<CaptureManifestEntry> CaptureManifest = new List<CaptureManifestEntry>();
        private static bool dashboardCaptureStarted;
        private static bool settingsCaptureStarted;
        private static string? requestedOutputRoot;
        private static Window? auditDashboardWindow;
        private static Window? auditSettingsWindow;

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
            var isEmbedded = auditDashboardWindow == null;
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
                await CaptureEmbeddedCurrentAsync(dashboard, outputRoot, metadata);
            }
            else
            {
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
                        await CaptureControlledAtSizeAsync(dashboard, outputRoot, size, theme, metadata);
                    }
                }
            }

            UiDiagnosticsExporters.WriteJson(CaptureManifest, Path.Combine(outputRoot, "capture-manifest.json"));
            CreateZip(outputRoot);
            RequestSettingsCapture(dashboard);
            CloseAuditWindow(auditDashboardWindow);
            TryDeleteSentinel();
        }

        private static async System.Threading.Tasks.Task CaptureEmbeddedCurrentAsync(
            DashboardView dashboard,
            string outputRoot,
            UiHostMetadata metadata)
        {
            // Embedded contract: capture exactly what Playnite hosts right now. Do not resize
            // the Dashboard, do not override the theme, and do not resize any host window.
            var viewportDir = Path.Combine(outputRoot, "embedded-current", "viewport");
            var scrollDir = Path.Combine(outputRoot, "embedded-current", "scroll-surfaces");
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
                    metadata);
                CaptureScrollSurfaces(dashboard, scrollDir, outputRoot, "workspace-" + safe, workspace.ToString(), string.Empty, metadata.CaptureOrigin);
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

            await CaptureAllInnerTabs(dashboard, viewportDir, scrollDir, outputRoot, "embedded-current", metadata);
        }

        private static async System.Threading.Tasks.Task CaptureControlledAtSizeAsync(
            DashboardView dashboard,
            string outputRoot,
            AuditWindowSize size,
            GameSaveCenterThemeMode theme,
            UiHostMetadata metadata)
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
            await WaitForRenderAsync(dashboard.Dispatcher);

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
                    metadata);
                CaptureScrollSurfaces(dashboard, scrollDir, outputRoot, "workspace-" + safe, workspace.ToString(), string.Empty, metadata.CaptureOrigin);
                if (window != null)
                {
                    UiDiagnosticsExporters.SavePng(window, Path.Combine(windowDir, $"controlled-window-{safe}.png"), 1d);
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

            await CaptureAllInnerTabs(dashboard, viewportDir, scrollDir, outputRoot, "controlled-" + size.Key + "-" + themeKey, metadata);
        }

        private static async System.Threading.Tasks.Task CaptureAllInnerTabs(
            DashboardView dashboard,
            string viewportDir,
            string scrollDir,
            string outputRoot,
            string routePrefix,
            UiHostMetadata metadata)
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
                        metadata);
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
            UiHostMetadata metadata)
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
                    metadata);
                CaptureScrollSurfaces(dashboard, scrollDir, outputRoot, tabRoute, workspace, safe, metadata.CaptureOrigin);

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
                        metadata);
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
            UiHostMetadata metadata)
        {
            var scale = GetRenderScale(root);
            UiDiagnosticsExporters.SavePng(root, path, scale);
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
            CaptureManifest.Add(new CaptureManifestEntry
            {
                File = RelativeTo(path, outputRoot),
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
            string origin)
        {
            var scrollers = FindMeaningfulScrollSurfaces(root);
            var index = 0;
            foreach (var scroller in scrollers)
            {
                var name = string.IsNullOrWhiteSpace(scroller.Name) ? "scroller-" + index : scroller.Name;
                index++;
                var file = $"{SafeFileName(route)}__{SafeFileName(name)}.png";
                var segments = scroller.ViewportHeight > 0
                    ? (int)Math.Ceiling(scroller.ExtentHeight / scroller.ViewportHeight)
                    : 1;
                // Extremely tall/virtualized surfaces can take many minutes to stitch. Keep
                // the surface in the manifest and mark it honestly instead of blocking the
                // whole audit for one table.
                var captured = segments <= 60;
                if (captured)
                {
                    try
                    {
                        UiDiagnosticsExporters.SaveScrollViewerFull(scroller, Path.Combine(outDir, file));
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug(ex, "Real host audit scroll surface capture failed: " + file);
                        captured = false;
                    }
                }
                CaptureManifest.Add(new CaptureManifestEntry
                {
                    File = RelativeTo(Path.Combine(outDir, file), outputRoot),
                    CaptureType = "ScrollSurfaceFull",
                    Origin = origin,
                    Route = route,
                    Workspace = workspace,
                    Tab = tab,
                    DpiScaleX = 1.0,
                    DpiScaleY = 1.0,
                    RenderScale = 1.0,
                    ScrollerName = name,
                    ViewportHeight = Math.Round(scroller.ViewportHeight, 2),
                    ExtentHeight = Math.Round(scroller.ExtentHeight, 2),
                    SegmentCount = segments,
                    CompletenessValidated = captured
                });
            }
        }

        internal static List<ScrollViewer> FindMeaningfulScrollSurfaces(FrameworkElement root)
        {
            return FindVisualChildren<ScrollViewer>(root)
                .Where(scroller =>
                    scroller.Visibility == Visibility.Visible
                    && scroller.ActualWidth >= 60
                    && scroller.ActualHeight >= 60
                    && (scroller.ScrollableHeight > 8 || scroller.ScrollableWidth > 8))
                .Distinct()
                .ToList();
        }

        internal static bool IsBoundsWithinViewport(Rect bounds, double viewportWidth, double viewportHeight, double tolerance)
        {
            return bounds.Left >= -tolerance
                && bounds.Top >= -tolerance
                && bounds.Right <= viewportWidth + tolerance
                && bounds.Bottom <= viewportHeight + tolerance;
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
            var settingsEmbedded = auditSettingsWindow == null;
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
                    CaptureSettingsAtSize(settingsView, outputRoot, size, theme);
                }
            }

            UiDiagnosticsExporters.WriteJson(CaptureManifest, Path.Combine(outputRoot, "capture-manifest.json"));
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
                Mode = controlled ? "controlled-host-window-settings" : "embedded-current-settings",
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
            SaveViewport(settingsView, Path.Combine(viewportDir, "settings.png"), outputRoot, "settings", "Settings", string.Empty, metadata);
            if (window != null)
            {
                UiDiagnosticsExporters.SavePng(window, Path.Combine(windowDir, "controlled-window-settings.png"), 1d);
            }
            CaptureSettingsTabs(settingsView, baseDir, outputRoot, size.Key, themeKey, metadata);
        }

        private static void CaptureSettingsTabs(
            GameSaveCenterSettingsView settingsView,
            string baseDir,
            string outputRoot,
            string sizeKey,
            string themeKey,
            UiHostMetadata metadata)
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
                        metadata);
                    CaptureScrollSurfaces(settingsView, scrollDir, outputRoot, "settings-" + safe, "Settings", safe, metadata.CaptureOrigin);
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

        internal sealed class CaptureManifestEntry
        {
            public string File { get; set; } = string.Empty;
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
        }
    }
}
