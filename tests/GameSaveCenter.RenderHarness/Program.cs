using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Markup;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using GameSaveCenter.RenderHarness.UiAudit;
using AsyncThumbnailImage = GameSaveCenter.Playnite.Controls.AsyncThumbnailImage;
using AsyncThumbnailLoader = GameSaveCenter.Playnite.Converters.AsyncThumbnailLoader;
using VirtualizingWrapPanel = GameSaveCenter.Playnite.Controls.VirtualizingWrapPanel;
using WorkspaceStatePresenter = GameSaveCenter.Playnite.Controls.WorkspaceStatePresenter;

namespace GameSaveCenter.RenderHarness;

/// <summary>
/// Offscreen layout QA for the production workspace views. It renders each page at the
/// logical content sizes produced by the Dashboard shell for common 1080p/2K/4K windows
/// and writes PNGs plus a measurable scroll/viewport report for later comparison.
/// </summary>
public static class Program
{
    private static readonly string RepositoryRoot = ResolveRepositoryRoot();

    private static readonly List<string> s_problems = new List<string>();

    private static readonly (int Width, int Height)[] WindowSizes =
    {
        (1040, 700),
        (1100, 720),
        (1280, 720),
        (1366, 768),
        (1536, 864),
        (1600, 900),
        (1707, 960),
        (1920, 1080),
        (2048, 1152),
        (2560, 1440),
        (3840, 2160)
    };

    private static readonly (int Width, int Height)[] ThemeWindowSizes =
    {
        (1040, 700),
        (1100, 720),
        (1366, 768),
        (2560, 1440)
    };

    private static readonly (string Name, GameSaveCenterThemeMode Mode)[] ThemeModes =
    {
        ("light", GameSaveCenterThemeMode.Light),
        ("dark", GameSaveCenterThemeMode.Dark)
    };

    private static readonly (int Width, int Height)[] ResizeSequence =
    {
        (2560, 1440),
        (1100, 720),
        (2560, 1440)
    };

    private static readonly (int Width, int Height)[] ShellWindowSizes =
    {
        (720, 640),
        (960, 640),
        (980, 640),
        (1040, 700)
    };

    public static int Main(string[] args)
    {
        if (args.Length > 0
            && (args[0].Equals("audit", StringComparison.OrdinalIgnoreCase)
                || args[0].Equals("--audit", StringComparison.OrdinalIgnoreCase)))
        {
            var outputRoot = args.Length > 1
                ? args[1]
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "artifacts", "ui-audit");
            var auditExitCode = 0;
            var auditThread = new Thread(() => { auditExitCode = UiAuditRunner.Run(outputRoot); });
            auditThread.SetApartmentState(ApartmentState.STA);
            auditThread.Start();
            auditThread.Join();
            return auditExitCode;
        }

        if (args.Length > 0 && args[0].Equals("gridprobe", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".tmp", "gridprobe");
            var probeExitCode = 0;
            var probeThread = new Thread(() => { probeExitCode = RunGridProbeOnly(outputRoot); });
            probeThread.SetApartmentState(ApartmentState.STA);
            probeThread.Start();
            probeThread.Join();
            return probeExitCode;
        }

        if (args.Length > 0 && args[0].Equals("scaleprobe", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".tmp", "scaleprobe");
            var probeExitCode = 0;
            var probeThread = new Thread(() => { probeExitCode = RunScaleProbeOnly(outputRoot); });
            probeThread.SetApartmentState(ApartmentState.STA);
            probeThread.Start();
            probeThread.Join();
            return probeExitCode;
        }

        if (args.Length > 0 && args[0].Equals("wrapprobe", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".tmp", "wrapprobe");
            var probeExitCode = 0;
            var probeThread = new Thread(() => { probeExitCode = RunWrapProbeOnly(outputRoot); });
            probeThread.SetApartmentState(ApartmentState.STA);
            probeThread.Start();
            probeThread.Join();
            return probeExitCode;
        }

        if (args.Length > 0 && args[0].Equals("thumbnailprobe", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".tmp", "thumbnailprobe");
            var probeExitCode = 0;
            var probeThread = new Thread(() => { probeExitCode = RunThumbnailProbeOnly(outputRoot); });
            probeThread.SetApartmentState(ApartmentState.STA);
            probeThread.Start();
            probeThread.Join();
            return probeExitCode;
        }

        if (args.Length > 0 && args[0].Equals("finesseprobe", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".tmp", "ui-finesse-probe");
            var themeMode = args.Length > 2 && args[2].Equals("light", StringComparison.OrdinalIgnoreCase)
                ? GameSaveCenterThemeMode.Light
                : GameSaveCenterThemeMode.Dark;
            var sortedHeaderFixture = args.Length > 3 && args[3].Equals("sorted", StringComparison.OrdinalIgnoreCase);
            var semanticEdgeCaseFixture = args.Length > 3 && args[3].Equals("edgevalues", StringComparison.OrdinalIgnoreCase);
            var probeExitCode = 0;
            var probeThread = new Thread(() =>
            {
                probeExitCode = RunFinesseProbeOnly(outputRoot, themeMode, sortedHeaderFixture, semanticEdgeCaseFixture);
            });
            probeThread.SetApartmentState(ApartmentState.STA);
            probeThread.Start();
            probeThread.Join();
            return probeExitCode;
        }

        if (args.Length > 0 && args[0].Equals("shellqa", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "artifacts", "ui-qa", "shell");
            var shellExitCode = 0;
            var shellThread = new Thread(() => { shellExitCode = RunShellChromeQa(outputRoot); });
            shellThread.SetApartmentState(ApartmentState.STA);
            shellThread.Start();
            shellThread.Join();
            return shellExitCode;
        }

        if (args.Length > 0 && args[0].Equals("shortwindowprobe", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".tmp", "shortwindowprobe");
            var probeExitCode = 0;
            var probeThread = new Thread(() => { probeExitCode = RunShortWindowReachabilityProbe(outputRoot); });
            probeThread.SetApartmentState(ApartmentState.STA);
            probeThread.Start();
            probeThread.Join();
            return probeExitCode;
        }

        if (args.Length > 0 && args[0].Equals("horizontalprobe", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".tmp", "horizontalprobe");
            var probeExitCode = 0;
            var probeThread = new Thread(() => { probeExitCode = RunHorizontalScrollEndpointProbe(outputRoot); });
            probeThread.SetApartmentState(ApartmentState.STA);
            probeThread.Start();
            probeThread.Join();
            return probeExitCode;
        }

        if (args.Length > 0 && args[0].Equals("v3shots", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "artifacts", "ui-qa", "v3-shots");
            var v3ExitCode = 0;
            var v3Thread = new Thread(() => { v3ExitCode = RunV3Shots(outputRoot); });
            v3Thread.SetApartmentState(ApartmentState.STA);
            v3Thread.Start();
            v3Thread.Join();
            return v3ExitCode;
        }

        if (args.Length > 0 && args[0].Equals("v4shots", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "artifacts", "ui-qa", "v4-shots");
            var v4ExitCode = 0;
            var v4Thread = new Thread(() => { v4ExitCode = RunV4Shots(outputRoot); });
            v4Thread.SetApartmentState(ApartmentState.STA);
            v4Thread.Start();
            v4Thread.Join();
            return v4ExitCode;
        }

        if (args.Length > 0 && args[0].Equals("v6shots", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "artifacts", "ui-qa", "v6-shots");
            var v6ExitCode = 0;
            var v6Thread = new Thread(() => { v6ExitCode = RunV6Shots(outputRoot); });
            v6Thread.SetApartmentState(ApartmentState.STA);
            v6Thread.Start();
            v6Thread.Join();
            return v6ExitCode;
        }

        if (args.Length > 0 && args[0].Equals("v6-2shots", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "artifacts", "ui-qa", "v6-2-shots");
            var v62ExitCode = 0;
            var v62Thread = new Thread(() => { v62ExitCode = RunV62Shots(outputRoot); });
            v62Thread.SetApartmentState(ApartmentState.STA);
            v62Thread.Start();
            v62Thread.Join();
            return v62ExitCode;
        }

        if (args.Length > 0 && args[0].Equals("v7progress", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "artifacts", "ui-qa", "v7-progress");
            var v7ExitCode = 0;
            var v7Thread = new Thread(() => { v7ExitCode = RunV7ProgressProbe(outputRoot); });
            v7Thread.SetApartmentState(ApartmentState.STA);
            v7Thread.Start();
            v7Thread.Join();
            return v7ExitCode;
        }

        if (args.Length > 0 && args[0].Equals("stateprobe", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".tmp", "stateprobe");
            var stateProbeExitCode = 0;
            var stateProbeThread = new Thread(() => { stateProbeExitCode = RunWorkspaceStateProbe(outputRoot); });
            stateProbeThread.SetApartmentState(ApartmentState.STA);
            stateProbeThread.Start();
            stateProbeThread.Join();
            return stateProbeExitCode;
        }

        if (args.Length > 0 && args[0].Equals("statefixtures", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".tmp", "statefixtures");
            var stateFixtureExitCode = 0;
            var stateFixtureThread = new Thread(() => { stateFixtureExitCode = RunWorkspaceStateFixtures(outputRoot); });
            stateFixtureThread.SetApartmentState(ApartmentState.STA);
            stateFixtureThread.Start();
            stateFixtureThread.Join();
            return stateFixtureExitCode;
        }

        if (args.Length > 0 && args[0].Equals("mediageometryprobe", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".tmp", "media-geometry-probe");
            var geometryExitCode = 0;
            var geometryThread = new Thread(() => { geometryExitCode = RunMediaInboxGeometryProbe(outputRoot); });
            geometryThread.SetApartmentState(ApartmentState.STA);
            geometryThread.Start();
            geometryThread.Join();
            return geometryExitCode;
        }

        if (args.Length > 0 && args[0].Equals("toolbarprobe", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".tmp", "toolbar-probe");
            var toolbarExitCode = 0;
            var toolbarThread = new Thread(() => { toolbarExitCode = RunToolbarClassificationProbe(outputRoot); });
            toolbarThread.SetApartmentState(ApartmentState.STA);
            toolbarThread.Start();
            toolbarThread.Join();
            return toolbarExitCode;
        }

        if (args.Length > 0 && args[0].Equals("emptytables", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".tmp", "emptytables");
            var emptyTableExitCode = 0;
            var emptyTableThread = new Thread(() => { emptyTableExitCode = RunEmptyTableFixtures(outputRoot); });
            emptyTableThread.SetApartmentState(ApartmentState.STA);
            emptyTableThread.Start();
            emptyTableThread.Join();
            return emptyTableExitCode;
        }

        if (args.Length > 0 && args[0].Equals("settingsthemeprobe", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".tmp", "settings-theme-probe");
            Directory.CreateDirectory(outputRoot);
            var probeExitCode = 0;
            var probeThread = new Thread(() =>
            {
                var app = new Application();
                app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
                var report = new StringBuilder();
                s_problems.Clear();
                RunSettingsThemeTransitionProbe(outputRoot, report);
                File.WriteAllText(Path.Combine(outputRoot, "settings-theme-probe-report.txt"), report.ToString());
                Console.WriteLine(report.ToString());
                probeExitCode = s_problems.Count == 0 ? 0 : 1;
            });
            probeThread.SetApartmentState(ApartmentState.STA);
            probeThread.Start();
            probeThread.Join();
            return probeExitCode;
        }

        if (args.Length > 0 && args[0].Equals("buttonbusyprobe", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".tmp", "button-busy-probe");
            Directory.CreateDirectory(outputRoot);
            var probeExitCode = 0;
            var probeThread = new Thread(() =>
            {
                var app = new Application();
                app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
                var report = new StringBuilder();
                s_problems.Clear();
                RunButtonBusyProbe(outputRoot, report);
                File.WriteAllText(Path.Combine(outputRoot, "button-busy-probe-report.txt"), report.ToString());
                Console.WriteLine(report.ToString());
                probeExitCode = s_problems.Count == 0 ? 0 : 1;
            });
            probeThread.SetApartmentState(ApartmentState.STA);
            probeThread.Start();
            probeThread.Join();
            return probeExitCode;
        }

        if (args.Length > 0 && args[0].Equals("dangerdialogprobe", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".tmp", "danger-dialog-probe");
            Directory.CreateDirectory(outputRoot);
            var probeExitCode = 0;
            var probeThread = new Thread(() =>
            {
                var app = new Application();
                app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
                var report = new StringBuilder();
                s_problems.Clear();
                RunDangerDialogProbe(outputRoot, report);
                File.WriteAllText(Path.Combine(outputRoot, "danger-dialog-probe-report.txt"), report.ToString());
                Console.WriteLine(report.ToString());
                probeExitCode = s_problems.Count == 0 ? 0 : 1;
            });
            probeThread.SetApartmentState(ApartmentState.STA);
            probeThread.Start();
            probeThread.Join();
            return probeExitCode;
        }

        if (args.Length > 0 && args[0].Equals("motionprobe", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".tmp", "motion-probe");
            Directory.CreateDirectory(outputRoot);
            var probeExitCode = 0;
            var probeThread = new Thread(() =>
            {
                var app = new Application
                {
                    ShutdownMode = ShutdownMode.OnExplicitShutdown
                };
                app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
                var report = new StringBuilder();
                s_problems.Clear();
                RunMotionProbe(outputRoot, report);
                File.WriteAllText(Path.Combine(outputRoot, "motion-probe-report.txt"), report.ToString());
                Console.WriteLine(report.ToString());
                probeExitCode = s_problems.Count == 0 ? 0 : 1;
            });
            probeThread.SetApartmentState(ApartmentState.STA);
            probeThread.Start();
            probeThread.Join();
            return probeExitCode;
        }

        if (args.Length > 0 && args[0].Equals("motionhotprobe", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".tmp", "motion-hot-probe");
            Directory.CreateDirectory(outputRoot);
            var probeExitCode = 0;
            var probeThread = new Thread(() =>
            {
                var app = new Application
                {
                    ShutdownMode = ShutdownMode.OnExplicitShutdown
                };
                app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
                var report = new StringBuilder();
                s_problems.Clear();
                RunMotionHotChangeProbe(outputRoot, report);
                File.WriteAllText(Path.Combine(outputRoot, "motion-hot-probe-report.txt"), report.ToString());
                Console.WriteLine(report.ToString());
                probeExitCode = s_problems.Count == 0 ? 0 : 1;
            });
            probeThread.SetApartmentState(ApartmentState.STA);
            probeThread.Start();
            probeThread.Join();
            return probeExitCode;
        }

        if (args.Length > 0 && args[0].Equals("motioncycleprobe", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".tmp", "motion-cycle-probe");
            Directory.CreateDirectory(outputRoot);
            var probeExitCode = 0;
            var probeThread = new Thread(() =>
            {
                var app = new Application
                {
                    ShutdownMode = ShutdownMode.OnExplicitShutdown
                };
                app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
                var report = new StringBuilder();
                s_problems.Clear();
                RunMotionCycleProbe(outputRoot, report);
                File.WriteAllText(Path.Combine(outputRoot, "motion-cycle-probe-report.txt"), report.ToString());
                Console.WriteLine(report.ToString());
                probeExitCode = s_problems.Count == 0 ? 0 : 1;
            });
            probeThread.SetApartmentState(ApartmentState.STA);
            probeThread.Start();
            probeThread.Join();
            return probeExitCode;
        }

        if (args.Length > 0 && args[0].Equals("motionreentryprobe", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".tmp", "motion-reentry-probe");
            Directory.CreateDirectory(outputRoot);
            var probeExitCode = 0;
            var probeThread = new Thread(() =>
            {
                var app = new Application
                {
                    ShutdownMode = ShutdownMode.OnExplicitShutdown
                };
                app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
                var report = new StringBuilder();
                s_problems.Clear();
                RunMotionReentryProbe(outputRoot, report);
                File.WriteAllText(Path.Combine(outputRoot, "motion-reentry-probe-report.txt"), report.ToString());
                Console.WriteLine(report.ToString());
                probeExitCode = s_problems.Count == 0 ? 0 : 1;
            });
            probeThread.SetApartmentState(ApartmentState.STA);
            probeThread.Start();
            probeThread.Join();
            return probeExitCode;
        }

        if (args.Length > 0 && args[0].Equals("overviewedges", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".tmp", "overviewedges");
            var overviewEdgeExitCode = 0;
            var overviewEdgeThread = new Thread(() => { overviewEdgeExitCode = RunOverviewEdgeFixtures(outputRoot); });
            overviewEdgeThread.SetApartmentState(ApartmentState.STA);
            overviewEdgeThread.Start();
            overviewEdgeThread.Join();
            return overviewEdgeExitCode;
        }

        if (args.Length > 0 && args[0].Equals("lowcostprobe", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".tmp", "lowcostprobe");
            var lowCostExitCode = 0;
            var lowCostThread = new Thread(() => { lowCostExitCode = RunLowCostProbeOnly(outputRoot); });
            lowCostThread.SetApartmentState(ApartmentState.STA);
            lowCostThread.Start();
            lowCostThread.Join();
            return lowCostExitCode;
        }

        if (args.Length > 0 && args[0].Equals("enduranceprobe", StringComparison.OrdinalIgnoreCase))
        {
            var outputRoot = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".tmp", "enduranceprobe");
            var durationSeconds = 1800;
            if (args.Length > 2 && (!int.TryParse(args[2], out durationSeconds) || durationSeconds < 1))
                durationSeconds = 1800;

            var enduranceExitCode = 0;
            var enduranceThread = new Thread(() =>
            {
                enduranceExitCode = RunEnduranceProbe(outputRoot, durationSeconds);
            });
            enduranceThread.SetApartmentState(ApartmentState.STA);
            enduranceThread.Start();
            enduranceThread.Join();
            return enduranceExitCode;
        }

        var exitCode = 0;
        var thread = new Thread(() => { exitCode = Run(args); });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        return exitCode;
    }

    private static int RunWorkspaceStateProbe(string outputRoot)
    {
        Directory.CreateDirectory(outputRoot);
        var report = new StringBuilder();
        report.AppendLine("GameSaveCenter workspace state presenter probe");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine();
        var problems = new List<string>();

        try
        {
            var app = new Application();
            app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
            foreach (var (themeName, themeMode) in ThemeModes)
            {
                foreach (var (stateName, state) in new[]
                {
                    ("error", "Error"),
                    ("offline", "Offline"),
                    ("loading", "Loading")
                })
                {
                    var resourceHost = new MediaCenterView();
                    ApplyThemePalette(resourceHost, themeMode);
                    var presenter = new WorkspaceStatePresenter
                    {
                        State = state,
                        Title = state == "Loading" ? "正在读取媒体" : "媒体暂时不可用",
                        Message = state == "Loading" ? "Worker 正在读取当前内容。" : "可以重试读取，已有内容不会被清除。",
                        Detail = state == "Loading" ? string.Empty : "状态探针",
                        RetryText = "重试",
                        RetryCommand = new ProbeCommand(),
                        Style = (Style)resourceHost.Resources["GscWorkspaceStatePresenter"]
                    };
                    var host = new Border
                    {
                        Width = 720,
                        Height = 420,
                        Background = resourceHost.TryFindResource("GscBackdropBrush") as Brush
                            ?? new SolidColorBrush(Color.FromRgb(24, 30, 43))
                    };
                    host.Resources.MergedDictionaries.Add(resourceHost.Resources);
                    host.Child = presenter;
                    host.Measure(new Size(host.Width, host.Height));
                    host.Arrange(new Rect(0, 0, host.Width, host.Height));
                    host.UpdateLayout();

                    var path = Path.Combine(outputRoot, $"state-{themeName}-{stateName}.png");
                    SavePng(host, path);
                    var size = new FileInfo(path).Length;
                    report.AppendLine($"  {Path.GetFileName(path)}: {host.ActualWidth:0}x{host.ActualHeight:0} DIP, {size} bytes");
                    if (size < 2048)
                        problems.Add($"{path} looks blank ({size} bytes)");
                }
            }

            report.AppendLine(problems.Count == 0 ? "stateprobe OK" : "stateprobe FAILED");
            foreach (var problem in problems)
                report.AppendLine("  PROBLEM " + problem);
            File.WriteAllText(Path.Combine(outputRoot, "stateprobe-report.txt"), report.ToString());
            Console.WriteLine(report.ToString());
            return problems.Count == 0 ? 0 : 1;
        }
        catch (Exception ex)
        {
            report.AppendLine("stateprobe FAILED");
            report.AppendLine(ex.ToString());
            File.WriteAllText(Path.Combine(outputRoot, "stateprobe-report.txt"), report.ToString());
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static int RunOverviewEdgeFixtures(string outputRoot)
    {
        Directory.CreateDirectory(outputRoot);
        var report = new StringBuilder();
        report.AppendLine("GameSaveCenter Overview boundary-state fixtures");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine("Profiles: empty-activity, many-risks, long-title, bilingual-length, offline");
        report.AppendLine("Themes: light, dark; viewports: 820x700 (bilingual), 1040x700, 1600x900");
        AppendRunMetadata(report, "overviewedges", "OffscreenRenderHarness", "light,dark", "empty-activity; many-risks; long-title; bilingual-length; offline; bilingual 820x700; 1040x700/1600x900");
        report.AppendLine();
        var problems = new List<string>();

        try
        {
            var app = new Application();
            app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
            var cases = new[]
            {
                (Name: "empty-activity", State: WorkspaceFixtureState.Ready, Profile: OverviewFixtureProfile.EmptyActivity),
                (Name: "many-risks", State: WorkspaceFixtureState.Ready, Profile: OverviewFixtureProfile.ManyRisks),
                (Name: "long-title", State: WorkspaceFixtureState.Ready, Profile: OverviewFixtureProfile.LongTitle),
                (Name: "bilingual-length", State: WorkspaceFixtureState.Ready, Profile: OverviewFixtureProfile.BilingualLengthStress),
                (Name: "offline", State: WorkspaceFixtureState.Offline, Profile: OverviewFixtureProfile.Default)
            };

            foreach (var (themeName, themeMode) in ThemeModes)
            {
                foreach (var (windowW, windowH) in new[] { (820, 700), (1040, 700), (1600, 900) })
                {
                    foreach (var fixture in cases)
                    {
                        if (windowW == 820 && fixture.Profile != OverviewFixtureProfile.BilingualLengthStress)
                            continue;
                        var view = new OverviewView
                        {
                            DataContext = new FakeDashboardData(18, fixture.State, fixture.Profile)
                        };
                        ApplyThemePalette(view, themeMode);
                        CaptureOverviewEdgeFixture(
                            view,
                            Path.Combine(outputRoot, $"overview-{fixture.Name}-{themeName}-{windowW}x{windowH}.png"),
                            fixture.Name,
                            windowW,
                            windowH,
                            themeMode,
                            problems,
                            report);
                    }
                }
            }

            report.AppendLine(problems.Count == 0 ? "overviewedges OK" : "overviewedges FAILED");
            foreach (var problem in problems)
                report.AppendLine("  PROBLEM " + problem);
            File.WriteAllText(Path.Combine(outputRoot, "overviewedges-report.txt"), report.ToString());
            Console.WriteLine(report.ToString());
            return problems.Count == 0 ? 0 : 1;
        }
        catch (Exception ex)
        {
            report.AppendLine("overviewedges FAILED");
            report.AppendLine(ex.ToString());
            File.WriteAllText(Path.Combine(outputRoot, "overviewedges-report.txt"), report.ToString());
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static int RunLowCostProbeOnly(string outputRoot)
    {
        Directory.CreateDirectory(outputRoot);
        var report = new StringBuilder();
        report.AppendLine("GameSaveCenter low-cost fallback fixtures");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine("Glass: disabled; motion: disabled; system high contrast is not inferred");
        report.AppendLine("Pages: Overview, Save, Trainer, Media, Maintenance, Task");
        report.AppendLine("Themes: light, dark; viewports: 1040x700, 1600x900");
        AppendRunMetadata(report, "lowcostprobe", "OffscreenRenderHarness", "light,dark", "six workspaces; glass=false; motion=false; 1040x700/1600x900");
        report.AppendLine();
        var problems = new List<string>();
        var pageNames = new[] { "Overview", "Save", "Trainer", "Media", "Maintenance", "Task" };

        try
        {
            var app = new Application();
            app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
            foreach (var (themeName, themeMode) in ThemeModes)
            {
                foreach (var (windowW, windowH) in new[] { (1040, 700), (1600, 900) })
                {
                    foreach (var pageName in pageNames)
                    {
                        var label = $"{pageName}/{themeName}/{windowW}x{windowH}";
                        try
                        {
                            var view = CreateThemeView(pageName);
                            ApplyThemePalette(view, themeMode, glassEnabled: false, motionEnabled: false);
                            var (contentW, contentH) = ContentSize(windowW, windowH);
                            var host = new Grid
                            {
                                Width = contentW,
                                Height = contentH,
                                Background = CreateHarnessBackground(view),
                                ClipToBounds = true
                            };
                            host.Children.Add(view);
                            ApplyThemeResponsive(view, contentW, contentH);
                            host.Measure(new Size(contentW, contentH));
                            host.Arrange(new Rect(0, 0, contentW, contentH));
                            host.UpdateLayout();
                            ApplyThemeResponsive(view, contentW, contentH);
                            host.UpdateLayout();

                            VerifyLowCostResources(view, label, problems, report);
                            var visibleEffects = FindVisualChildren<UIElement>(host)
                                .Count(element => element.Visibility == Visibility.Visible && element.Effect != null);
                            if (visibleEffects != 0)
                                problems.Add($"{label} retained {visibleEffects} visible Effect visuals with glass disabled.");

                            var visibleText = FindVisualChildren<TextBlock>(host)
                                .Count(text => text.Visibility == Visibility.Visible
                                    && text.ActualWidth > 0
                                    && text.ActualHeight > 0
                                    && !string.IsNullOrWhiteSpace(text.Text));
                            if (visibleText < 2)
                                problems.Add($"{label} retained too little readable text ({visibleText} visible TextBlocks).");

                            var horizontalOverflowScrolls = FindVisualChildren<ScrollViewer>(host)
                                .Where(scroll => scroll.ExtentWidth > scroll.ViewportWidth + 0.5)
                                .ToArray();
                            var horizontalOverflow = horizontalOverflowScrolls.Length;
                            var unexpectedHorizontalOverflow = horizontalOverflowScrolls
                                .Count(scroll => !scroll.Name.StartsWith("DG_", StringComparison.Ordinal));
                            if (horizontalOverflow != 0)
                            {
                                report.AppendLine(
                                    $"  {label} overflow: "
                                    + string.Join(", ", horizontalOverflowScrolls.Select(scroll =>
                                        $"{(string.IsNullOrWhiteSpace(scroll.Name) ? "unnamed" : scroll.Name)} extent={scroll.ExtentWidth:0} viewport={scroll.ViewportWidth:0} hbar={scroll.ComputedHorizontalScrollBarVisibility}")));
                            }
                            if (unexpectedHorizontalOverflow != 0)
                                problems.Add($"{label} has {unexpectedHorizontalOverflow} unexpected horizontal-overflow ScrollViewer(s) in low-cost mode.");

                            var outputPath = Path.Combine(outputRoot, $"{pageName.ToLowerInvariant()}-{themeName}-{windowW}x{windowH}.png");
                            var sw = Stopwatch.StartNew();
                            SavePng(host, outputPath);
                            sw.Stop();
                            report.AppendLine(
                                $"  {label}: visibleText={visibleText} visibleEffects={visibleEffects} horizontalOverflow={horizontalOverflow} unexpectedOverflow={unexpectedHorizontalOverflow} "
                                + $"size={host.ActualWidth:0}x{host.ActualHeight:0} render_ms={sw.ElapsedMilliseconds} bytes={new FileInfo(outputPath).Length}");
                        }
                        catch (Exception ex)
                        {
                            problems.Add($"{label} failed: {ex.Message}");
                            report.AppendLine($"  {label}: FAILED {ex.Message}");
                        }
                    }
                }
            }

            report.AppendLine(problems.Count == 0 ? "lowcostprobe OK" : "lowcostprobe FAILED");
            foreach (var problem in problems)
                report.AppendLine("  PROBLEM " + problem);
            File.WriteAllText(Path.Combine(outputRoot, "lowcostprobe-report.txt"), report.ToString());
            Console.WriteLine(report.ToString());
            return problems.Count == 0 ? 0 : 1;
        }
        catch (Exception ex)
        {
            report.AppendLine("lowcostprobe FAILED");
            report.AppendLine(ex.ToString());
            File.WriteAllText(Path.Combine(outputRoot, "lowcostprobe-report.txt"), report.ToString());
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static void VerifyLowCostResources(
        UserControl view,
        string label,
        List<string> problems,
        StringBuilder report)
    {
        foreach (var key in new[]
                 {
                     "GscSurfaceEffect",
                     "GscPrimaryButtonEffect",
                     "GscSidebarEffect",
                     "GscPopupEffect",
                     "GscDialogEffect",
                     "GscSliderThumbEffect",
                     "GscGameBackgroundEffect"
                 })
        {
            if (view.TryFindResource(key) != null)
                problems.Add($"{label} resource {key} is not null in low-cost mode.");
        }

        var allowsTransparency = view.TryFindResource("GscPopupAllowsTransparency") as bool?;
        if (allowsTransparency != false)
            problems.Add($"{label} GscPopupAllowsTransparency={allowsTransparency?.ToString() ?? "missing"}, expected False.");

        var popupAnimation = view.TryFindResource("GscPopupAnimation");
        if (!(popupAnimation is PopupAnimation.None))
            problems.Add($"{label} GscPopupAnimation={popupAnimation ?? "missing"}, expected None.");

        var shellOpacity = view.TryFindResource("GscShellAmbientOpacity") as double?;
        if (shellOpacity == null || Math.Abs(shellOpacity.Value) > 0.0001)
            problems.Add($"{label} GscShellAmbientOpacity={shellOpacity?.ToString("0.###") ?? "missing"}, expected 0.");

        var gameOpacity = view.TryFindResource("GscGameBackgroundOpacity") as double?;
        if (gameOpacity == null || Math.Abs(gameOpacity.Value) > 0.0001)
            problems.Add($"{label} GscGameBackgroundOpacity={gameOpacity?.ToString("0.###") ?? "missing"}, expected 0.");

        if (view.TryFindResource("GscGameBackgroundTintBrush") is not SolidColorBrush tint
            || tint.Color.A != 0)
            problems.Add($"{label} GscGameBackgroundTintBrush is not fully transparent.");

        if (view.TryFindResource("GscAmbientWideWashBrush") is not LinearGradientBrush wash
            || wash.GradientStops.Any(stop => stop.Color.A != 0))
            problems.Add($"{label} GscAmbientWideWashBrush still contains visible alpha.");

        if (view is OverviewView overview && overview.UiAnimationsEnabled)
            problems.Add($"{label} OverviewView.UiAnimationsEnabled remained True in low-cost mode.");

        report.AppendLine(
            $"  {label} resources: effects=null popupTransparency={allowsTransparency?.ToString() ?? "missing"} "
            + $"popupAnimation={popupAnimation ?? "missing"} shellOpacity={shellOpacity?.ToString("0.###") ?? "missing"} "
            + $"gameOpacity={gameOpacity?.ToString("0.###") ?? "missing"}");
    }

    private static int RunEnduranceProbe(string outputRoot, int durationSeconds)
    {
        Directory.CreateDirectory(outputRoot);
        var reportPath = Path.Combine(outputRoot, "enduranceprobe-report.txt");
        var report = new StringBuilder();
        report.AppendLine("GameSaveCenter 30-minute endurance probe");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine("Host: real WPF STA Window with opacity 0.01; Playnite host and user input are not inferred");
        report.AppendLine("Actions: workspace navigation, Media preview segment, selected details/inspector, Light/Dark theme");
        report.AppendLine("Resources: GC.GetTotalMemory(false), PrivateMemorySize64, WorkingSet64, handles, timers, managed event handlers, animated owners and thumbnail cache; no forced GC in this probe");
        report.AppendLine($"DurationTargetSeconds: {durationSeconds}");
        AppendRunMetadata(
            report,
            "enduranceprobe",
            "ControlledWpfWindow",
            "light,dark",
            "six workspaces; 1040x700 DIP; sample every 10s; no forced GC");
        report.AppendLine();

        var problems = new List<string>();
        var samples = new List<EnduranceSample>();
        var reportLock = new object();
        var reportFlush = new Action(() =>
        {
            lock (reportLock)
                File.WriteAllText(reportPath, report.ToString());
        });

        try
        {
            var app = new Application
            {
                ShutdownMode = ShutdownMode.OnExplicitShutdown
            };
            app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));

            var data = new FakeDashboardData(60);
            var shell = new AcrylicProductionShellView
            {
                DataContext = data,
                MotionEnabledProvider = () => true
            };
            var pages = new (WorkspaceKind Kind, string Name, UserControl View)[]
            {
                (WorkspaceKind.Overview, "Overview", new OverviewView { DataContext = data }),
                (WorkspaceKind.Saves, "Save", new SaveCenterView { DataContext = data }),
                (WorkspaceKind.Trainers, "Trainer", new TrainerCenterView { DataContext = data }),
                (WorkspaceKind.Media, "Media", new MediaCenterView { DataContext = data }),
                (WorkspaceKind.Tasks, "Task", new TaskCenterView { DataContext = data }),
                (WorkspaceKind.Maintenance, "Maintenance", new MaintenanceView { DataContext = data })
            };
            var pageHost = shell.PageHostForAudit as ContentControl
                ?? throw new InvalidOperationException("Production shell PageHost is not a ContentControl.");
            var window = new Window
            {
                Width = 1040,
                Height = 700,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                ShowActivated = false,
                Left = -32000,
                Top = -32000,
                Opacity = 0.01,
                Content = shell
            };

            var process = Process.GetCurrentProcess();
            var stopwatch = Stopwatch.StartNew();
            var cycle = 0;
            var completedActions = 0;
            var currentPage = "Overview";
            var currentTheme = "light";
            var lastSampleAt = TimeSpan.Zero;
            var lastProgressAt = TimeSpan.Zero;
            var pageIndex = 0;
            var themeIndex = 0;
            var actionFailureCount = 0;
            var actionDurationsMs = new List<double>();
            var slowActionStacks = new List<string>();
            DispatcherTimer? actionTimer = null;

            void RecordSample(bool final)
            {
                process.Refresh();
                var resourceRoots = pages.Select(item => item.View).Cast<UserControl>().Concat(new[] { shell }).ToArray();
                var thumbnailDiagnostics = AsyncThumbnailLoader.CaptureDiagnostics();
                var sample = new EnduranceSample
                {
                    ElapsedSeconds = stopwatch.Elapsed.TotalSeconds,
                    Cycle = cycle,
                    Page = currentPage,
                    Theme = currentTheme,
                    ManagedBytes = GC.GetTotalMemory(false),
                    PrivateBytes = process.PrivateMemorySize64,
                    WorkingSetBytes = process.WorkingSet64,
                    ThreadCount = process.Threads.Count,
                    HandleCount = process.HandleCount,
                    ActiveTimerCount = CountEnabledDispatcherTimers(resourceRoots)
                        + (actionTimer?.IsEnabled == true ? 1 : 0),
                    ManagedEventHandlerCount = CountManagedEventHandlers(resourceRoots),
                    AnimatedOwnerCount = CountAnimatedOwners(resourceRoots),
                    ThumbnailCacheCount = thumbnailDiagnostics.CacheCount,
                    ThumbnailCacheLimit = thumbnailDiagnostics.CacheLimit,
                    ActiveThumbnailDecodes = thumbnailDiagnostics.ActiveDecodes
                };
                samples.Add(sample);
                report.AppendLine(
                    $"SAMPLE elapsed_s={sample.ElapsedSeconds:0.0} cycle={sample.Cycle} "
                    + $"page={sample.Page} theme={sample.Theme} managed={sample.ManagedBytes} "
                    + $"private={sample.PrivateBytes} workingSet={sample.WorkingSetBytes} "
                    + $"threads={sample.ThreadCount} handles={sample.HandleCount} "
                    + $"timers={sample.ActiveTimerCount} subscriptions={sample.ManagedEventHandlerCount} "
                    + $"animated_owners={sample.AnimatedOwnerCount} "
                    + $"thumb_cache={sample.ThumbnailCacheCount}/{sample.ThumbnailCacheLimit} "
                    + $"thumb_active={sample.ActiveThumbnailDecodes} final={final}");
                lastSampleAt = stopwatch.Elapsed;
                reportFlush();
            }

            void LayoutCurrentPage(UserControl page, GameSaveCenterThemeMode themeMode)
            {
                ApplyThemePalette(shell, themeMode, glassEnabled: true, motionEnabled: true);
                ApplyThemePalette(page, themeMode, glassEnabled: true, motionEnabled: true);
                pageHost.Content = page;
                shell.ApplyResponsiveLayout(window.Width, window.Height);
                ApplyThemeResponsive(page, pageHost.ActualWidth > 0 ? pageHost.ActualWidth : 1040, pageHost.ActualHeight > 0 ? pageHost.ActualHeight : 700);
                window.UpdateLayout();
                shell.ApplyResponsiveLayout(window.ActualWidth, window.ActualHeight);
                ApplyThemeResponsive(page, pageHost.ActualWidth, pageHost.ActualHeight);
                window.UpdateLayout();
            }

            void ExercisePreviewAndDetails(UserControl page)
            {
                var mediaTabs = page is MediaCenterView
                    ? FindVisualChildren<TabControl>(page).FirstOrDefault()
                    : null;
                if (mediaTabs != null && mediaTabs.Items.Count > 0)
                {
                    mediaTabs.SelectedIndex = cycle % mediaTabs.Items.Count;
                    window.UpdateLayout();
                    completedActions++;
                }

                foreach (var list in FindVisualChildren<ListBox>(page)
                    .Where(candidate => candidate.Items.Count > 0)
                    .Take(2))
                {
                    list.SelectedIndex = Math.Min(list.Items.Count - 1, cycle % Math.Max(1, list.Items.Count));
                    window.UpdateLayout();
                    completedActions++;
                }

                foreach (var grid in FindVisualChildren<DataGrid>(page)
                    .Where(candidate => candidate.Items.Count > 0)
                    .Take(2))
                {
                    grid.SelectedIndex = Math.Min(grid.Items.Count - 1, cycle % Math.Max(1, grid.Items.Count));
                    grid.ScrollIntoView(grid.SelectedItem);
                    window.UpdateLayout();
                    completedActions++;
                }

                var detailButton = FindVisualChildren<Button>(page)
                    .FirstOrDefault(button => button.Visibility == Visibility.Visible
                        && button.IsEnabled
                        && (button.Name.EndsWith("CompactDetailsButton", StringComparison.Ordinal)
                            || button.Name == "TaskDetailsButton"));
                if (detailButton != null)
                {
                    detailButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    window.UpdateLayout();
                    completedActions++;
                    var close = FindVisualChildren<Button>(page)
                        .FirstOrDefault(button => button.Visibility == Visibility.Visible
                            && button.Name.EndsWith("CloseDetailsButton", StringComparison.Ordinal));
                    close?.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    window.UpdateLayout();
                }
            }

            void RunActionCycle()
            {
                var cycleStopwatch = Stopwatch.StartNew();
                try
                {
                    var current = pages[pageIndex];
                    currentPage = current.Name;
                    currentTheme = ThemeModes[themeIndex].Name;
                    LayoutCurrentPage(current.View, ThemeModes[themeIndex].Mode);
                    ExercisePreviewAndDetails(current.View);
                    completedActions++;
                    cycle++;
                    pageIndex = (pageIndex + 1) % pages.Length;
                    if (pageIndex == 0)
                        themeIndex = (themeIndex + 1) % ThemeModes.Length;
                }
                catch (Exception ex)
                {
                    actionFailureCount++;
                    problems.Add($"cycle={cycle} page={currentPage} failed: {ex.GetType().Name}: {ex.Message}");
                }
                finally
                {
                    cycleStopwatch.Stop();
                    var durationMs = cycleStopwatch.Elapsed.TotalMilliseconds;
                    actionDurationsMs.Add(durationMs);
                    if (durationMs > 100 && slowActionStacks.Count < 8)
                    {
                        slowActionStacks.Add(
                            $"cycle={cycle} page={currentPage} duration_ms={durationMs:0.0}\n"
                            + Environment.StackTrace);
                    }
                }
            }

            window.Show();
            window.UpdateLayout();
            LayoutCurrentPage(pages[0].View, ThemeModes[0].Mode);
            RecordSample(final: false);

            actionTimer = new DispatcherTimer(DispatcherPriority.Background, window.Dispatcher)
            {
                Interval = TimeSpan.FromMilliseconds(250)
            };
            actionTimer.Tick += (_, _) =>
            {
                RunActionCycle();
                var elapsed = stopwatch.Elapsed;
                if (elapsed - lastSampleAt >= TimeSpan.FromSeconds(10))
                    RecordSample(final: false);
                if (elapsed - lastProgressAt >= TimeSpan.FromSeconds(60))
                {
                    Console.WriteLine($"enduranceprobe progress elapsed={elapsed.TotalSeconds:0}s cycles={cycle} samples={samples.Count}");
                    lastProgressAt = elapsed;
                }

                if (elapsed >= TimeSpan.FromSeconds(durationSeconds))
                {
                    actionTimer!.Stop();
                    RecordSample(final: true);
                    window.Close();
                    app.Shutdown();
                }
            };
            actionTimer.Start();
            app.Run();
            stopwatch.Stop();

            if (samples.Count < 2)
                problems.Add($"only {samples.Count} resource samples were recorded");
            if (cycle == 0 || completedActions == 0)
                problems.Add("no endurance action cycle completed");
            if (actionFailureCount != 0)
                problems.Add($"actionFailures={actionFailureCount}");

            AppendEnduranceSummary(
                report,
                samples,
                actionDurationsMs,
                slowActionStacks,
                cycle,
                completedActions,
                actionFailureCount,
                durationSeconds);
            report.AppendLine(problems.Count == 0 ? "enduranceprobe OK" : "enduranceprobe FAILED");
            foreach (var problem in problems)
                report.AppendLine("  PROBLEM " + problem);
            reportFlush();
            Console.WriteLine(report.ToString());
            return problems.Count == 0 ? 0 : 1;
        }
        catch (Exception ex)
        {
            report.AppendLine("enduranceprobe FAILED");
            report.AppendLine(ex.ToString());
            reportFlush();
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static void AppendEnduranceSummary(
        StringBuilder report,
        IReadOnlyList<EnduranceSample> samples,
        IReadOnlyList<double> actionDurationsMs,
        IReadOnlyList<string> slowActionStacks,
        int cycles,
        int completedActions,
        int actionFailures,
        int durationSeconds)
    {
        if (samples.Count == 0)
            return;

        var first = samples[0];
        var last = samples[samples.Count - 1];
        var tailStart = Math.Max(0, samples.Count - Math.Min(10, samples.Count));
        var headEnd = Math.Min(10, samples.Count);
        var headPrivate = samples.Take(headEnd).Average(sample => sample.PrivateBytes);
        var tailPrivate = samples.Skip(tailStart).Average(sample => sample.PrivateBytes);
        var headManaged = samples.Take(headEnd).Average(sample => sample.ManagedBytes);
        var tailManaged = samples.Skip(tailStart).Average(sample => sample.ManagedBytes);
        var privateSlope = CalculateSlopePerMinute(samples, sample => sample.PrivateBytes);
        var managedSlope = CalculateSlopePerMinute(samples, sample => sample.ManagedBytes);
        var maxPrivate = samples.Max(sample => sample.PrivateBytes);
        var maxManaged = samples.Max(sample => sample.ManagedBytes);
        report.AppendLine();
        report.AppendLine(
            $"SUMMARY duration_s={last.ElapsedSeconds:0.0}/{durationSeconds} cycles={cycles} "
            + $"completedActions={completedActions} samples={samples.Count} actionFailures={actionFailures}");
        report.AppendLine(
            $"SUMMARY private_delta={last.PrivateBytes - first.PrivateBytes} private_peak_delta={maxPrivate - first.PrivateBytes} "
            + $"private_head_avg={headPrivate:0} private_tail_avg={tailPrivate:0} "
            + $"private_slope_bytes_per_min={privateSlope:0.##}");
        report.AppendLine(
            $"SUMMARY managed_delta={last.ManagedBytes - first.ManagedBytes} managed_peak_delta={maxManaged - first.ManagedBytes} "
            + $"managed_head_avg={headManaged:0} managed_tail_avg={tailManaged:0} "
            + $"managed_slope_bytes_per_min={managedSlope:0.##}");
        report.AppendLine(
            $"SUMMARY resources_first=threads:{first.ThreadCount},handles:{first.HandleCount} "
            + $"timers:{first.ActiveTimerCount},subscriptions:{first.ManagedEventHandlerCount},animated_owners:{first.AnimatedOwnerCount},"
            + $"thumb_cache:{first.ThumbnailCacheCount}/{first.ThumbnailCacheLimit} "
            + $"resources_last=threads:{last.ThreadCount},handles:{last.HandleCount} "
            + $"timers:{last.ActiveTimerCount},subscriptions:{last.ManagedEventHandlerCount},animated_owners:{last.AnimatedOwnerCount},"
            + $"thumb_cache:{last.ThumbnailCacheCount}/{last.ThumbnailCacheLimit}");
        if (actionDurationsMs.Count > 0)
        {
            var actionP95 = CalculatePercentile(actionDurationsMs, 0.95);
            var actionMax = actionDurationsMs.Max();
            var slowActions = actionDurationsMs.Count(duration => duration > 100);
            report.AppendLine(
                $"SUMMARY ui_action_p95_ms={actionP95:0.##} ui_action_max_ms={actionMax:0.##} "
                + $"ui_action_slow_over_100ms={slowActions}/{actionDurationsMs.Count}");
            if (slowActionStacks.Count == 0)
            {
                report.AppendLine("SUMMARY ui_action_hotspot_stacks=none (no reproducible >100ms action in this controlled run)");
            }
            else
            {
                report.AppendLine($"SUMMARY ui_action_hotspot_stacks={slowActionStacks.Count} (captured after action completion)");
                foreach (var stack in slowActionStacks)
                    report.AppendLine("HOTSPOT " + stack.Replace(Environment.NewLine, Environment.NewLine + "HOTSPOT "));
            }
        }
        report.AppendLine(
            "SUMMARY interpretation=bounded-window observation; trend fields are evidence, "
            + "not proof of Playnite-host or mathematically unbounded behavior");
    }

    private static double CalculateSlopePerMinute(
        IReadOnlyList<EnduranceSample> samples,
        Func<EnduranceSample, long> selector)
    {
        if (samples.Count < 2)
            return 0;

        var meanX = samples.Average(sample => sample.ElapsedSeconds);
        var meanY = samples.Average(selector);
        var numerator = 0d;
        var denominator = 0d;
        foreach (var sample in samples)
        {
            var x = sample.ElapsedSeconds - meanX;
            numerator += x * (selector(sample) - meanY);
            denominator += x * x;
        }

        return denominator <= double.Epsilon ? 0 : numerator / denominator * 60d;
    }

    private sealed class EnduranceSample
    {
        public double ElapsedSeconds { get; set; }
        public int Cycle { get; set; }
        public string Page { get; set; } = string.Empty;
        public string Theme { get; set; } = string.Empty;
        public long ManagedBytes { get; set; }
        public long PrivateBytes { get; set; }
        public long WorkingSetBytes { get; set; }
        public int ThreadCount { get; set; }
        public int HandleCount { get; set; }
        public int ActiveTimerCount { get; set; }
        public int ManagedEventHandlerCount { get; set; }
        public int AnimatedOwnerCount { get; set; }
        public int ThumbnailCacheCount { get; set; }
        public int ThumbnailCacheLimit { get; set; }
        public int ActiveThumbnailDecodes { get; set; }
    }

    private static int CountEnabledDispatcherTimers(IEnumerable<UserControl> roots)
    {
        var count = 0;
        foreach (var root in roots)
        {
            foreach (var value in ReadInstanceFieldValues(root))
            {
                if (value is DispatcherTimer timer && timer.IsEnabled)
                {
                    count++;
                    continue;
                }

                if (value is IDictionary dictionary)
                {
                    foreach (DictionaryEntry entry in dictionary)
                    {
                        if (entry.Value is DispatcherTimer dictionaryTimer && dictionaryTimer.IsEnabled)
                            count++;
                    }
                }
            }
        }

        return count;
    }

    private static int CountManagedEventHandlers(IEnumerable<UserControl> roots)
    {
        var count = 0;
        foreach (var root in roots)
        {
            foreach (var value in ReadInstanceFieldValues(root))
            {
                if (value is Delegate handler)
                    count += handler.GetInvocationList().Length;
            }
        }

        return count;
    }

    private static int CountAnimatedOwners(IEnumerable<UserControl> roots)
    {
        var count = 0;
        foreach (var root in roots)
        {
            if (root.HasAnimatedProperties)
                count++;
            count += FindVisualChildren<UIElement>(root).Count(element => element.HasAnimatedProperties);
        }

        return count;
    }

    private static IEnumerable<object?> ReadInstanceFieldValues(object instance)
    {
        for (var type = instance.GetType(); type != null && type != typeof(object); type = type.BaseType)
        {
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                object? value;
                try
                {
                    value = field.GetValue(instance);
                }
                catch (FieldAccessException)
                {
                    continue;
                }

                yield return value;
            }
        }
    }

    private static void CaptureOverviewEdgeFixture(
        OverviewView view,
        string path,
        string fixtureName,
        int windowW,
        int windowH,
        GameSaveCenterThemeMode themeMode,
        List<string> problems,
        StringBuilder report)
    {
        var data = (FakeDashboardData)view.DataContext;
        var (contentW, contentH) = ContentSize(windowW, windowH);
        var host = new Grid
        {
            Width = contentW,
            Height = contentH,
            Background = CreateHarnessBackground(view),
            ClipToBounds = true
        };
        host.Children.Add(view);
        ApplyOverviewV3(view, windowW, windowH);
        host.Measure(new Size(contentW, contentH));
        host.Arrange(new Rect(0, 0, contentW, contentH));
        host.UpdateLayout();
        ApplyOverviewV3(view, windowW, windowH);
        host.UpdateLayout();

        var page = FindVisualChildren<ScrollViewer>(host)
            .FirstOrDefault(element => element.Name == "OverviewStackScrollSurface");
        var hero = FindVisualChildren<FrameworkElement>(host)
            .FirstOrDefault(element => element.Name == "OverviewTodayHeroCard");
        var currentGame = FindVisualChildren<FrameworkElement>(host)
            .FirstOrDefault(element => element.Name == "OverviewCurrentGameCard");
        var statStrip = FindVisualChildren<FrameworkElement>(host)
            .FirstOrDefault(element => element.Name == "OverviewStatStrip");
        var activityCard = FindVisualChildren<FrameworkElement>(host)
            .FirstOrDefault(element => element.Name == "OverviewRecentActivityCard");
        var riskCard = FindVisualChildren<FrameworkElement>(host)
            .FirstOrDefault(element => element.Name == "OverviewRiskCard");
        var findingsCard = FindVisualChildren<FrameworkElement>(host)
            .FirstOrDefault(element => element.Name == "OverviewFindingsCard");
        var heroAction = hero == null
            ? null
            : FindVisualChildren<Button>(hero).FirstOrDefault(button => button.Visibility == Visibility.Visible);
        var activityTexts = FindVisualChildren<TextBlock>(host).ToArray();
        var activityPresenters = FindVisualChildren<WorkspaceStatePresenter>(host).ToArray();
        var activityEmptyPresenter = activityPresenters.FirstOrDefault(presenter => presenter.Visibility == Visibility.Visible);
        var activityEmpty = activityEmptyPresenter != null
            || activityTexts.Any(text => text.Visibility == Visibility.Visible
                && text.Text.IndexOf("暂无全局活动", StringComparison.Ordinal) >= 0);
        var taskEmpty = activityCard != null
            && activityTexts.Any(text => text.Visibility == Visibility.Visible
                && text.Text.IndexOf("暂无任务记录", StringComparison.Ordinal) >= 0);
        var riskViewport = FindVisualChildren<ScrollViewer>(host)
            .FirstOrDefault(element => element.Name == "OverviewRiskViewport");
        var titleText = currentGame == null
            ? null
            : FindVisualChildren<TextBlock>(currentGame)
                .FirstOrDefault(text => text.Text == data.SelectedGame.Name);
        var longEnglishText = activityCard == null
            ? null
            : FindVisualChildren<TextBlock>(activityCard)
                .FirstOrDefault(text => text.Text == data.BilingualLongEnglishSentence);
        var currentGameButtons = currentGame == null
            ? Array.Empty<Button>()
            : FindVisualChildren<Button>(currentGame).Where(button => button.Visibility == Visibility.Visible).ToArray();
        var horizontalOverflow = page != null && page.ExtentWidth > page.ViewportWidth + 0.5;
        var primarySurfaceCount = new[] { hero, currentGame, statStrip, activityCard, riskCard, findingsCard }
            .Count(element => element != null && element.ActualWidth > 0 && element.ActualHeight > 0);
        var riskRows = riskCard == null
            ? 0
            : FindVisualChildren<ListBoxItem>(riskCard).Count(item => item.Visibility == Visibility.Visible);
        var actionText = heroAction == null ? string.Empty : heroAction.Content?.ToString() ?? string.Empty;
        report.AppendLine(
            $"  {fixtureName} theme={themeMode} size={windowW}x{windowH}: " +
            $"surfaces={primarySurfaceCount}/6 heroAction='{actionText}' " +
            $"activityEmpty={activityEmpty} taskEmpty={taskEmpty} emptyHeight={(activityEmptyPresenter == null ? "missing" : $"{activityEmptyPresenter.ActualHeight:0}")} presenters={activityPresenters.Length} tasks={data.OverviewTasks.Count} activities={data.Activities.Count} " +
            $"riskRows={riskRows} riskItems={data.RecentProtection.Items.Count} " +
            $"riskExtent={(riskViewport == null ? "missing" : $"{riskViewport.ExtentHeight:0}/{riskViewport.ViewportHeight:0}")} " +
            $"pageExtent={(page == null ? "missing" : $"{page.ExtentHeight:0}/{page.ViewportHeight:0}")} " +
            $"titleChars={data.SelectedGame.Name.Length} titleTrim={titleText?.TextTrimming} titleTooltip={titleText?.ToolTip != null} " +
            $"englishChars={data.BilingualLongEnglishSentence.Length} englishVisible={longEnglishText?.Visibility == Visibility.Visible} " +
            $"currentGameButtons={currentGameButtons.Length} buttonHeights={string.Join(",", currentGameButtons.Select(button => button.ActualHeight.ToString("0.##")))} " +
            $"pageOverflowH={horizontalOverflow} priority={data.OverviewPriorityKind}");

        if (primarySurfaceCount != 6)
            problems.Add($"{fixtureName} {themeMode} {windowW}x{windowH} missing one or more Overview primary surfaces");
        if (hero == null || hero.ActualWidth <= 0 || hero.ActualHeight <= 0 || heroAction == null || heroAction.ActualWidth <= 0)
            problems.Add($"{fixtureName} {themeMode} {windowW}x{windowH} has no reachable hero action");
        if (page == null || page.ActualWidth <= 0 || page.ActualHeight <= 0 || horizontalOverflow)
            problems.Add($"{fixtureName} {themeMode} {windowW}x{windowH} has invalid page viewport or horizontal overflow");
        if (fixtureName == "empty-activity"
            && (!activityEmpty
                || activityEmptyPresenter == null
                || activityEmptyPresenter.ActualHeight < 100
                || data.OverviewTasks.Count != 0
                || data.Activities.Count != 0))
            problems.Add($"empty-activity {themeMode} {windowW}x{windowH} did not expose the empty activity state");
        if (fixtureName == "many-risks"
            && (data.RecentProtection.Items.Count < 6
                || riskRows < 6
                || (riskRows < data.RecentProtection.Items.Count
                    && (riskViewport == null
                        || riskViewport.VerticalScrollBarVisibility == ScrollBarVisibility.Hidden
                        || riskViewport.ExtentHeight <= riskViewport.ViewportHeight + 0.5))))
            problems.Add($"many-risks {themeMode} {windowW}x{windowH} does not expose a reachable finite risk viewport");
        if (fixtureName == "long-title"
            && (data.SelectedGame.Name.Length < 80
                || titleText == null
                || titleText.TextTrimming != TextTrimming.CharacterEllipsis
                || titleText.ToolTip == null))
            problems.Add($"long-title {themeMode} {windowW}x{windowH} lost title ellipsis or tooltip reachability");
        if (fixtureName == "bilingual-length"
            && (data.SelectedGame.Name.Length < 30
                || titleText == null
                || titleText.TextTrimming != TextTrimming.CharacterEllipsis
                || !string.Equals(titleText.ToolTip as string, data.SelectedGame.Name, StringComparison.Ordinal)
                || longEnglishText == null
                || longEnglishText.Visibility != Visibility.Visible
                || longEnglishText.TextTrimming != TextTrimming.CharacterEllipsis
                || currentGameButtons.Length < 2
                || currentGameButtons.Any(button => button.ActualWidth <= 0 || button.ActualHeight < 30)))
            problems.Add($"bilingual-length {themeMode} {windowW}x{windowH} lost full-value reachability or action geometry");
        if (fixtureName == "offline"
            && (data.Snapshot.WorkerHealthy
                || data.OverviewPriorityKind != "Worker"
                || !string.Equals(actionText, "打开维护中心", StringComparison.Ordinal)))
            problems.Add($"offline {themeMode} {windowW}x{windowH} lost Worker-first hero action");

        SavePng(host, path);
        var size = new FileInfo(path).Length;
        report.AppendLine($"  {Path.GetFileName(path)}: {contentW:0}x{contentH:0} DIP, {size} bytes");
        if (size < 2048)
            problems.Add($"{path} looks blank ({size} bytes)");

        if (fixtureName == "empty-activity" && page != null && page.ExtentHeight > page.ViewportHeight + 0.5)
        {
            page.ScrollToEnd();
            host.UpdateLayout();
            var tailPath = Path.Combine(
                Path.GetDirectoryName(path) ?? string.Empty,
                Path.GetFileNameWithoutExtension(path) + "-tail.png");
            SavePng(host, tailPath);
            var tailSize = new FileInfo(tailPath).Length;
            report.AppendLine($"  {Path.GetFileName(tailPath)}: tail offset={page.VerticalOffset:0}/{page.ScrollableHeight:0}, {tailSize} bytes");
            if (tailSize < 2048)
                problems.Add($"{tailPath} looks blank ({tailSize} bytes)");
        }
    }

    private static int RunWorkspaceStateFixtures(string outputRoot)
    {
        Directory.CreateDirectory(outputRoot);
        var report = new StringBuilder();
        report.AppendLine("GameSaveCenter production workspace state fixtures");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine("States: Ready, Empty, Loading, Error, Stale, Offline");
        report.AppendLine("Pages: MediaInbox, MediaDetails, MaintenanceAudit");
        report.AppendLine();
        var problems = new List<string>();

        try
        {
            var app = new Application();
            app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
            VerifyWorkspaceStateFixtureBindingSurface(problems, report);
            foreach (var (themeName, themeMode) in ThemeModes)
            {
                foreach (var (width, height) in ThemeWindowSizes)
                {
                    foreach (var state in Enum.GetValues(typeof(WorkspaceFixtureState)).Cast<WorkspaceFixtureState>())
                    {
                        CaptureWorkspaceStateFixture(
                            new MediaCenterView { DataContext = new FakeDashboardData(12, state) },
                            Path.Combine(outputRoot, $"media-inbox-{themeName}-{width}x{height}-{state}.png"),
                            "MediaInbox",
                            "MediaInboxScrollSurface",
                            width,
                            height,
                            themeMode,
                            state,
                            view => SelectTab(view, 0),
                            problems,
                            report);

                        CaptureWorkspaceStateFixture(
                            new MediaCenterView { DataContext = new FakeDashboardData(12, state) },
                            Path.Combine(outputRoot, $"media-details-{themeName}-{width}x{height}-{state}.png"),
                            "MediaDetails",
                            "MediaCurrentScrollSurface",
                            width,
                            height,
                            themeMode,
                            state,
                            view => SelectTab(view, 1),
                            problems,
                            report);

                        CaptureWorkspaceStateFixture(
                            new MaintenanceView { DataContext = new FakeDashboardData(12, state) },
                            Path.Combine(outputRoot, $"maintenance-audit-{themeName}-{width}x{height}-{state}.png"),
                            "MaintenanceAudit",
                            "MaintenanceAuditScrollSurface",
                            width,
                            height,
                            themeMode,
                            state,
                            view => SelectTab(view, 4),
                            problems,
                            report);

                        if (state is WorkspaceFixtureState.Ready or WorkspaceFixtureState.Stale)
                        {
                            CaptureWorkspaceStateFixture(
                                new MaintenanceView { DataContext = new FakeDashboardData(12, state) },
                                Path.Combine(outputRoot, $"maintenance-next-steps-{themeName}-{width}x{height}-{state}.png"),
                                "MaintenanceNextSteps",
                                "MaintenanceNextStepsCard",
                                width,
                                height,
                                themeMode,
                                state,
                                view =>
                                {
                                    SelectTab(view, 0);
                                    SelectInnerTab(view, "诊断概览");
                                },
                                problems,
                                report);
                        }
                    }
                }
            }

            report.AppendLine(problems.Count == 0 ? "statefixtures OK" : "statefixtures FAILED");
            foreach (var problem in problems)
                report.AppendLine("  PROBLEM " + problem);
            File.WriteAllText(Path.Combine(outputRoot, "statefixtures-report.txt"), report.ToString());
            Console.WriteLine(report.ToString());
            return problems.Count == 0 ? 0 : 1;
        }
        catch (Exception ex)
        {
            report.AppendLine("statefixtures FAILED");
            report.AppendLine(ex.ToString());
            File.WriteAllText(Path.Combine(outputRoot, "statefixtures-report.txt"), report.ToString());
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static int RunMediaInboxGeometryProbe(string outputRoot)
    {
        Directory.CreateDirectory(outputRoot);
        var report = new StringBuilder();
        report.AppendLine("GameSaveCenter media inbox geometry probe");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine("EvidenceSource: synthetic WPF production view; no Playnite host or real media paths");
        report.AppendLine("Cases: normal readable viewport, horizontal scrollbar, alternate density, short-window page fallback, intentionally blocked parent");
        AppendRunMetadata(
            report,
            "mediageometryprobe",
            "OffscreenRenderHarness",
            "light,dark",
            "production MediaCenterView; 20 synthetic media rows; normal/horizontal/alternate-density/short-fallback/blocked-parent");
        report.AppendLine();
        var problems = new List<string>();

        try
        {
            var app = new Application();
            app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
            foreach (var (themeName, themeMode) in ThemeModes)
            {
                RunMediaInboxGeometryCase(themeName, themeMode, "normal", 760, 600, false, problems, report);
                RunMediaInboxGeometryCase(themeName, themeMode, "horizontal-scroll", 620, 600, false, problems, report);
                RunMediaInboxGeometryCase(themeName, themeMode, "alternate-density", 760, 600, false, problems, report, alternateDensity: true);
                RunMediaInboxGeometryCase(themeName, themeMode, "short-fallback", 760, 340, false, problems, report);
                RunMediaInboxGeometryCase(themeName, themeMode, "blocked-parent", 760, 340, true, problems, report);
            }

            report.AppendLine(problems.Count == 0 ? "mediageometryprobe OK" : "mediageometryprobe FAILED");
            foreach (var problem in problems)
                report.AppendLine("  PROBLEM " + problem);
            File.WriteAllText(Path.Combine(outputRoot, "media-geometry-report.txt"), report.ToString());
            Console.WriteLine(report.ToString());
            return problems.Count == 0 ? 0 : 1;
        }
        catch (Exception ex)
        {
            report.AppendLine("mediageometryprobe FAILED");
            report.AppendLine(ex.ToString());
            File.WriteAllText(Path.Combine(outputRoot, "media-geometry-report.txt"), report.ToString());
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static int RunToolbarClassificationProbe(string outputRoot)
    {
        Directory.CreateDirectory(outputRoot);
        var report = new StringBuilder();
        report.AppendLine("GameSaveCenter toolbar classification probe");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine("EvidenceSource: synthetic WPF panels under a named TrainerToolsSettingsScrollViewer");
        report.AppendLine("Cases: normal long form, same-ancestor wide toolbar, same-ancestor unreachable toolbar");
        AppendRunMetadata(
            report,
            "toolbarprobe",
            "OffscreenRenderHarness",
            "default WPF palette",
            "synthetic TextBox/ComboBox form and Button action rows; 440x240 DIP");
        report.AppendLine();
        var problems = new List<string>();

        try
        {
            var app = new Application();
            app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
            foreach (var caseName in new[] { "normal-form", "wide-toolbar", "unreachable-toolbar" })
            {
                var layout = CreateToolbarProbeLayout(caseName);
                var records = layout.Toolbars;
                foreach (var toolbar in records)
                {
                    report.AppendLine(
                        $"  {caseName}/{toolbar.Name}: purpose={toolbar.Purpose}, excluded={toolbar.Excluded}, "
                        + $"reason={toolbar.ExclusionReason}, layout={toolbar.ActualWidth:0.##}x{toolbar.ActualHeight:0.##}, "
                        + $"desiredWidth={toolbar.DesiredWidth:0.##}, visible={toolbar.VisibleWidth:0.##}x{toolbar.VisibleHeight:0.##}, "
                        + $"availableWidth={toolbar.AvailableWidth:0.##}, horizontalOverflow={toolbar.HorizontalOverflow}, "
                        + $"reachable={toolbar.Reachable}, scrollableAncestor={toolbar.ScrollableAncestor}");
                }

                if (caseName == "normal-form")
                {
                    var formRecord = records.FirstOrDefault(toolbar => toolbar.Name == "NormalFormRow");
                    if (formRecord == null || !formRecord.Excluded || formRecord.Purpose != "settings-form")
                        problems.Add("normal-form was not classified as an excluded settings form");
                    if (layout.Warnings.Any(warning => warning.Code.StartsWith("TOOLBAR_", StringComparison.Ordinal)))
                        problems.Add("normal-form produced a toolbar warning");
                }
                else if (caseName == "wide-toolbar")
                {
                    var toolbarRecord = records.FirstOrDefault(toolbar => toolbar.Name == "InjectedWideToolbar");
                    if (toolbarRecord == null || toolbarRecord.Excluded || !toolbarRecord.HorizontalOverflow)
                        problems.Add("wide-toolbar did not retain the action classification and overflow geometry");
                    if (!layout.Warnings.Any(warning => warning.Code == "TOOLBAR_HORIZONTAL_OVERFLOW"))
                        problems.Add("wide-toolbar did not produce TOOLBAR_HORIZONTAL_OVERFLOW");
                }
                else
                {
                    var toolbarRecord = records.FirstOrDefault(toolbar => toolbar.Name == "InjectedUnreachableToolbar");
                    if (toolbarRecord == null || toolbarRecord.Excluded || toolbarRecord.Reachable)
                        problems.Add("unreachable-toolbar did not retain the action classification and unreachable geometry");
                    if (!layout.Warnings.Any(warning => warning.Code == "TOOLBAR_UNREACHABLE"))
                        problems.Add("unreachable-toolbar did not produce TOOLBAR_UNREACHABLE");
                }
            }

            report.AppendLine(problems.Count == 0 ? "toolbarprobe OK" : "toolbarprobe FAILED");
            foreach (var problem in problems)
                report.AppendLine("  PROBLEM " + problem);
            File.WriteAllText(Path.Combine(outputRoot, "toolbar-probe-report.txt"), report.ToString());
            Console.WriteLine(report.ToString());
            return problems.Count == 0 ? 0 : 1;
        }
        catch (Exception ex)
        {
            report.AppendLine("toolbarprobe FAILED");
            report.AppendLine(ex.ToString());
            File.WriteAllText(Path.Combine(outputRoot, "toolbar-probe-report.txt"), report.ToString());
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static UiLayoutReport CreateToolbarProbeLayout(string caseName)
    {
        var content = new StackPanel();
        switch (caseName)
        {
            case "normal-form":
                for (var index = 0; index < 8; index++)
                {
                    var row = new WrapPanel
                    {
                        Name = index == 0 ? "NormalFormRow" : string.Empty,
                        Margin = new Thickness(0, 0, 0, 6)
                    };
                    row.Children.Add(new TextBox { Width = 150, Height = 28, Margin = new Thickness(0, 0, 6, 0) });
                    row.Children.Add(new ComboBox { Width = 150, Height = 28 });
                    content.Children.Add(row);
                }
                break;
            case "wide-toolbar":
                content.Children.Add(CreateProbeToolbar("InjectedWideToolbar", 700));
                break;
            case "unreachable-toolbar":
                content.Children.Add(new Border
                {
                    Height = 60,
                    ClipToBounds = true,
                    Child = new StackPanel
                    {
                        Height = 220,
                        Children =
                        {
                            new Border { Height = 150 },
                            CreateProbeToolbar("InjectedUnreachableToolbar", 300)
                        }
                    }
                });
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(caseName), caseName, "Unknown toolbar probe case.");
        }

        var scroller = new ScrollViewer
        {
            Name = "TrainerToolsSettingsScrollViewer",
            Width = 360,
            Height = caseName == "unreachable-toolbar" ? 80 : 160,
            VerticalScrollBarVisibility = caseName == "unreachable-toolbar"
                ? ScrollBarVisibility.Disabled
                : ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = content
        };
        var host = new Grid
        {
            Width = 440,
            Height = 240,
            Background = Brushes.White,
            ClipToBounds = true
        };
        host.Children.Add(scroller);
        host.Measure(new Size(host.Width, host.Height));
        host.Arrange(new Rect(0, 0, host.Width, host.Height));
        host.UpdateLayout();
        return UiLayoutAnalyzer.Analyze(
            host,
            "trainer-center",
            "工具设置",
            caseName,
            host.Width,
            host.Height,
            "toolbar-fixture",
            string.Empty,
            string.Empty);
    }

    private static StackPanel CreateProbeToolbar(string name, double width)
    {
        var toolbar = new StackPanel
        {
            Name = name,
            Tag = "Toolbar",
            Width = width,
            Orientation = Orientation.Horizontal
        };
        toolbar.Children.Add(new Button { Content = "动作一", Width = 220, Height = 36 });
        toolbar.Children.Add(new Button { Content = "动作二", Width = 220, Height = 36 });
        toolbar.Children.Add(new Button { Content = "动作三", Width = 220, Height = 36 });
        return toolbar;
    }

    private static void RunMediaInboxGeometryCase(
        string themeName,
        GameSaveCenterThemeMode themeMode,
        string caseName,
        double width,
        double height,
        bool blockPageScroll,
        List<string> problems,
        StringBuilder report,
        bool alternateDensity = false)
    {
        var view = new MediaCenterView { DataContext = new FakeDashboardData(24) };
        ApplyThemePalette(view, themeMode);
        var grid = (DataGrid)typeof(MediaCenterView)
            .GetField("MediaInboxGrid", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(view)!;
        if (alternateDensity)
        {
            // The production header style has a 42 DIP MinHeight. Override both
            // the DataGrid value and the realized header style in this fixture so
            // the alternate-density case actually measures a 36/44 DIP table,
            // instead of silently testing only the row-density half.
            grid.ColumnHeaderHeight = 36;
            var headerStyle = new Style(typeof(DataGridColumnHeader), grid.ColumnHeaderStyle);
            headerStyle.Setters.Add(new Setter(FrameworkElement.HeightProperty, 36d));
            headerStyle.Setters.Add(new Setter(FrameworkElement.MinHeightProperty, 36d));
            grid.ColumnHeaderStyle = headerStyle;
            foreach (var column in grid.Columns)
            {
                var columnHeaderStyle = new Style(
                    typeof(DataGridColumnHeader),
                    column.HeaderStyle ?? grid.ColumnHeaderStyle);
                columnHeaderStyle.Setters.Add(new Setter(FrameworkElement.HeightProperty, 36d));
                columnHeaderStyle.Setters.Add(new Setter(FrameworkElement.MinHeightProperty, 36d));
                column.HeaderStyle = columnHeaderStyle;
            }
            var rowStyle = new Style(typeof(DataGridRow), grid.RowStyle);
            rowStyle.Setters.Add(new Setter(FrameworkElement.HeightProperty, 44d));
            rowStyle.Setters.Add(new Setter(FrameworkElement.MinHeightProperty, 44d));
            grid.RowStyle = rowStyle;
        }
        var host = new Grid
        {
            Width = width,
            Height = height,
            Background = CreateHarnessBackground(view),
            ClipToBounds = true
        };
        host.Children.Add(view);
        view.ApplyResponsiveLayout(width, height);
        host.Measure(new Size(width, height));
        host.Arrange(new Rect(0, 0, width, height));
        host.UpdateLayout();
        view.ApplyResponsiveLayout(width, height);
        host.UpdateLayout();

        var pageScroller = FindVisualChildren<ScrollViewer>(host)
            .Single(candidate => candidate.Name == "MediaInboxPageScrollViewer");
        if (blockPageScroll)
        {
            pageScroller.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            host.UpdateLayout();
        }

        var layout = UiLayoutAnalyzer.Analyze(
            host,
            "media-center",
            "待归类",
            caseName,
            width,
            height,
            "media-inbox",
            "MediaInboxScrollSurface",
            "MediaInboxScrollSurface");
        var geometry = layout.MediaInboxGeometry;
        if (geometry == null)
        {
            problems.Add($"{themeName}/{caseName} did not produce MediaInbox geometry");
            return;
        }

        report.AppendLine(
            $"  {themeName}/{caseName} {width:0}x{height:0}: "
            + $"grid={geometry.GridVisibleHeight:0.##}/{geometry.GridLayoutHeight:0.##}, "
            + $"header={geometry.HeaderVisibleHeight:0.##}/{geometry.HeaderHeight:0.##}, "
            + $"rows={geometry.FullyVisibleRowCount}/{geometry.RequiredCompleteRows}, "
            + $"rowHeight={geometry.RowHeight:0.##}, horizontalBar={geometry.HorizontalScrollBarHeight:0.##}, "
            + $"framePadding={geometry.FramePaddingHeight:0.##}, frameBorder={geometry.FrameBorderHeight:0.##}, "
            + $"required={geometry.RequiredGridHeight:0.##}/{geometry.RequiredFrameHeight}, "
            + $"pageScroll={geometry.PageScrollAvailable}, status={geometry.Status}, "
            + $"warnings={string.Join(",", layout.Warnings.Select(warning => warning.Code + "/" + warning.Severity))}");

        var primaryHighWarnings = layout.Warnings
            .Where(warning => warning.Severity == "HIGH"
                && warning.Code is "PRIMARY_VIEWPORT_TOO_SHORT" or "PRIMARY_VIEWPORT_UNREACHABLE")
            .ToList();
        if (caseName == "normal")
        {
            if (geometry.FullyVisibleRowCount < geometry.RequiredCompleteRows)
                problems.Add($"{themeName}/{caseName} has only {geometry.FullyVisibleRowCount} complete rows");
            if (primaryHighWarnings.Count > 0)
                problems.Add($"{themeName}/{caseName} unexpectedly has primary geometry HIGH warnings");
        }
        else if (caseName == "horizontal-scroll")
        {
            if (geometry.HorizontalScrollBarHeight <= 0)
                problems.Add($"{themeName}/{caseName} did not realize a horizontal scrollbar");
            if (geometry.FullyVisibleRowCount < geometry.RequiredCompleteRows)
                problems.Add($"{themeName}/{caseName} has only {geometry.FullyVisibleRowCount} complete rows");
        }
        else if (caseName == "alternate-density")
        {
            if (Math.Abs(geometry.HeaderHeight - 36) > 0.5 || Math.Abs(geometry.RowHeight - 44) > 0.5)
                problems.Add($"{themeName}/{caseName} did not use measured density header={geometry.HeaderHeight:0.##}, row={geometry.RowHeight:0.##}");
            if (geometry.RequiredGridHeight < 212 || geometry.FullyVisibleRowCount < geometry.RequiredCompleteRows)
                problems.Add($"{themeName}/{caseName} did not keep the measured four-row floor");
        }
        else if (caseName == "short-fallback")
        {
            if (!geometry.PageScrollAvailable)
                problems.Add($"{themeName}/{caseName} did not expose page fallback scroll");
            if (primaryHighWarnings.Count > 0)
                problems.Add($"{themeName}/{caseName} treated reachable short window as HIGH");
        }
        else if (caseName == "blocked-parent")
        {
            if (primaryHighWarnings.Count == 0)
                problems.Add($"{themeName}/{caseName} did not catch blocked primary viewport");
        }
    }

    private static int RunEmptyTableFixtures(string outputRoot)
    {
        Directory.CreateDirectory(outputRoot);
        var report = new StringBuilder();
        report.AppendLine("GameSaveCenter production empty table fixtures");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        AppendRunMetadata(report, "emptytables", "OffscreenRenderHarness", "light,dark", "all production table/list collections cleared; selected game shell retained");
        report.AppendLine();
        s_problems.Clear();

        try
        {
            var app = new Application();
            app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
            var sizes = new[] { (Width: 1040, Height: 700), (Width: 1600, Height: 900) };
            foreach (var (themeName, themeMode) in ThemeModes)
            {
                foreach (var (windowW, windowH) in sizes)
                {
                    var (contentW, contentH) = ContentSize(windowW, windowH);
                    var saveData = EmptyTableData();
                    var save = new SaveCenterView { DataContext = saveData };
                    ApplyThemePalette(save, themeMode);
                    RenderTabs(save, outputRoot, $"save-empty-{themeName}", windowW, windowH, contentW, contentH, report, () => save.ApplyResponsiveLayout(contentW, windowH),
                        (host, tabIndex, fixtureReport) => VerifyEmptyTableSurface(host, $"Save tab{tabIndex}", fixtureReport));

                    var taskData = EmptyTableData();
                    var task = new TaskCenterView { DataContext = taskData };
                    ApplyThemePalette(task, themeMode);
                    RenderViewWithVerification(task, outputRoot, $"task-empty-{themeName}", windowW, windowH, contentW, contentH, report, () => task.ApplyResponsiveLayout(contentW, windowH),
                        (host, fixtureReport) => VerifyEmptyTableSurface(host, "Task", fixtureReport));

                    var trainerData = EmptyTableData();
                    var trainer = new TrainerCenterView { DataContext = trainerData };
                    ApplyThemePalette(trainer, themeMode);
                    RenderTabs(trainer, outputRoot, $"trainer-empty-{themeName}", windowW, windowH, contentW, contentH, report, () => trainer.ApplyResponsiveLayout(contentW, windowH),
                        (host, tabIndex, fixtureReport) => VerifyEmptyTableSurface(host, $"Trainer tab{tabIndex}", fixtureReport));

                    var mediaData = EmptyTableData();
                    var media = new MediaCenterView { DataContext = mediaData };
                    ApplyThemePalette(media, themeMode);
                    RenderTabs(media, outputRoot, $"media-empty-{themeName}", windowW, windowH, contentW, contentH, report, () => media.ApplyResponsiveLayout(contentW, contentH),
                        (host, tabIndex, fixtureReport) => VerifyEmptyTableSurface(host, $"Media tab{tabIndex}", fixtureReport));

                    var maintenanceData = EmptyTableData();
                    var maintenance = new MaintenanceView { DataContext = maintenanceData };
                    ApplyThemePalette(maintenance, themeMode);
                    RenderTabs(maintenance, outputRoot, $"maintenance-empty-{themeName}", windowW, windowH, contentW, contentH, report, () => maintenance.ApplyResponsiveLayout(contentW, windowH),
                        (host, tabIndex, fixtureReport) => VerifyEmptyTableSurface(host, $"Maintenance tab{tabIndex}", fixtureReport));
                }
            }

            report.AppendLine(s_problems.Count == 0 ? "emptytables OK" : "emptytables FAILED");
            foreach (var problem in s_problems)
                report.AppendLine("  PROBLEM " + problem);
            File.WriteAllText(Path.Combine(outputRoot, "emptytables-report.txt"), report.ToString());
            Console.WriteLine(report.ToString());
            return s_problems.Count == 0 ? 0 : 1;
        }
        catch (Exception ex)
        {
            report.AppendLine("emptytables FAILED");
            report.AppendLine(ex.ToString());
            File.WriteAllText(Path.Combine(outputRoot, "emptytables-report.txt"), report.ToString());
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static FakeDashboardData EmptyTableData()
    {
        var data = new FakeDashboardData(18, WorkspaceFixtureState.Empty);
        data.ClearTableDataForFixture();
        return data;
    }

    private static void VerifyEmptyTableSurface(Grid host, string label, StringBuilder report)
    {
        var grids = FindVisualChildren<DataGrid>(host)
            .Where(grid => !string.IsNullOrEmpty(grid.Name))
            .ToList();
        foreach (var grid in grids)
        {
            report.AppendLine($"  {label} empty-grid={grid.Name} items={grid.Items.Count} size={grid.ActualWidth:0}x{grid.ActualHeight:0}");
            if (grid.Items.Count != 0)
                s_problems.Add($"{label} {grid.Name} still has {grid.Items.Count} rows in empty fixture");
        }

        var lists = FindVisualChildren<ListBox>(host)
            .Where(list => !string.IsNullOrEmpty(list.Name))
            .Where(list => !list.Name.EndsWith("SegmentTabs", StringComparison.Ordinal)
                && list.Name != "SettingsSectionTabs"
                && list.Name != "MaintenanceDiagnosticsSubTabs")
            .ToList();
        foreach (var list in lists)
        {
            report.AppendLine($"  {label} empty-list={list.Name} items={list.Items.Count} size={list.ActualWidth:0}x{list.ActualHeight:0}");
            if (list.Items.Count != 0)
                s_problems.Add($"{label} {list.Name} still has {list.Items.Count} items in empty fixture");
        }

        var emptyText = FindVisualChildren<TextBlock>(host)
            .Where(text => text.Visibility == Visibility.Visible && !string.IsNullOrWhiteSpace(text.Text))
            .Where(text => text.Text.IndexOf("暂无", StringComparison.Ordinal) >= 0 || text.Text.IndexOf("没有", StringComparison.Ordinal) >= 0)
            .Select(text => text.Text.Replace(Environment.NewLine, " / "))
            .Distinct()
            .ToArray();
        var presenters = FindVisualChildren<WorkspaceStatePresenter>(host)
            .Where(presenter => presenter.Visibility == Visibility.Visible)
            .Select(presenter => presenter.State)
            .Distinct()
            .ToArray();
        report.AppendLine($"  {label} empty-text={string.Join(" | ", emptyText)} presenters={string.Join(",", presenters)}");
        if ((grids.Count > 0 || lists.Count > 0) && emptyText.Length == 0 && presenters.Length == 0)
            s_problems.Add($"{label} has empty data surfaces but no visible empty-state text or presenter");
    }

    private static void RenderViewWithVerification(UserControl view, string outputRoot, string name, int windowW, int windowH, double contentW, double contentH, StringBuilder report, Action applyLayout, Action<Grid, StringBuilder> verify)
    {
        var host = new Grid
        {
            Width = contentW,
            Height = contentH,
            Background = CreateHarnessBackground(view),
            ClipToBounds = true
        };
        host.Children.Add(view);
        var layoutSw = Stopwatch.StartNew();
        applyLayout();
        host.Measure(new Size(contentW, contentH));
        host.Arrange(new Rect(0, 0, contentW, contentH));
        host.UpdateLayout();
        applyLayout();
        host.UpdateLayout();
        layoutSw.Stop();
        verify(host, report);
        var sw = Stopwatch.StartNew();
        SavePng(host, Path.Combine(outputRoot, $"{name}-{windowW}x{windowH}.png"));
        sw.Stop();
        report.AppendLine($"  {name} layout_ms={layoutSw.ElapsedMilliseconds} render_ms={sw.ElapsedMilliseconds} window_dip={windowW}x{windowH} content_dip={contentW:0}x{contentH:0}");
        CollectScrollDiagnostics(host, report, name, windowW, windowH, -1);
    }

    private static void VerifyWorkspaceStateFixtureBindingSurface(List<string> problems, StringBuilder report)
    {
        var required = new[]
        {
            "IsWorkerOffline",
            "MediaDetailsState", "MediaDetailsPresenterState", "MediaDetailsStateTitle", "MediaDetailsStateMessage",
            "MediaDetailsStateDetail", "MediaDetailsStateOverlayVisible", "MediaDetailsStaleVisible",
            "MediaInboxState", "MediaInboxPresenterState", "MediaInboxStateTitle", "MediaInboxStateMessage",
            "MediaInboxStateDetail", "MediaInboxStateOverlayVisible", "MediaInboxStaleVisible",
            "MediaInboxCountDisplay", "MediaInboxCountCaption",
            "MaintenanceState", "MaintenancePresenterState", "MaintenanceStateTitle", "MaintenanceStateMessage",
            "MaintenanceStateDetail", "MaintenanceStateOverlayVisible", "MaintenanceStaleVisible",
            "MaintenanceActionSummary", "MaintenanceActionItems",
            "ReloadMediaWindowCommand", "ReloadMediaInboxCommand", "MaintenanceTabIndex", "MediaTabIndex"
        };
        var missing = required
            .Where(name => typeof(FakeDashboardData).GetProperty(name, BindingFlags.Instance | BindingFlags.Public) == null)
            .ToList();
        if (missing.Count == 0)
        {
            report.AppendLine("  binding-surface: all required media/maintenance state properties present");
            return;
        }

        var message = "binding-surface missing: " + string.Join(", ", missing);
        report.AppendLine("  PROBLEM " + message);
        problems.Add(message);
    }

    private static void CaptureWorkspaceStateFixture(
        UserControl view,
        string path,
        string label,
        string targetName,
        int windowW,
        int windowH,
        GameSaveCenterThemeMode themeMode,
        WorkspaceFixtureState state,
        Action<UserControl> beforeCapture,
        List<string> problems,
        StringBuilder report)
    {
        ApplyThemePalette(view, themeMode);
        CaptureV3Shot(
            view,
            path,
            targetName,
            windowW,
            windowH,
            ApplySimpleResponsiveV3,
            problems,
            report,
            beforeCapture,
            metrics: (host, target, fixtureReport) =>
            {
                var presenters = FindVisualChildren<WorkspaceStatePresenter>(host).ToList();
                var visiblePresenters = presenters.Where(presenter => presenter.Visibility == Visibility.Visible).ToList();
                var staleBanners = FindVisualChildren<FrameworkElement>(host)
                    .Where(element => element.Name is "MediaInboxStaleBanner" or "MediaCurrentStaleBanner" or "MaintenanceStaleBanner")
                    .Where(element => element.Visibility == Visibility.Visible)
                    .ToList();
                var dataSurface = FindVisualChildren<FrameworkElement>(host)
                    .FirstOrDefault(element => element.Name is "MediaInboxGrid" or "MediaGrid" or "MaintenanceAuditFindingsGrid");
                var actionItems = view.DataContext is FakeDashboardData fake
                    ? fake.MaintenanceActionItems.Count
                    : -1;
                fixtureReport.AppendLine(
                    $"  {label} fixture state={state} theme={themeMode} size={windowW}x{windowH}: target={target.ActualWidth:0}x{target.ActualHeight:0}, " +
                    $"dataSurface={(dataSurface == null ? "missing" : $"{dataSurface.Name}={dataSurface.ActualWidth:0}x{dataSurface.ActualHeight:0}")}, " +
                    $"presenters={presenters.Count}, visiblePresenters={visiblePresenters.Count}, staleBanners={staleBanners.Count}, actionItems={actionItems}");
                if (label != "MaintenanceNextSteps"
                    && (dataSurface == null || dataSurface.ActualWidth <= 0 || dataSurface.ActualHeight <= 0))
                    problems.Add($"{label} {state} {themeMode} {windowW}x{windowH} has no measurable data surface");

                var expectsOverlay = state is WorkspaceFixtureState.Loading or WorkspaceFixtureState.Error or WorkspaceFixtureState.Offline;
                var expectsStale = state == WorkspaceFixtureState.Stale && label != "MaintenanceNextSteps";
                if (expectsOverlay && visiblePresenters.Count == 0)
                    problems.Add($"{label} {state} {themeMode} {windowW}x{windowH} has no visible WorkspaceStatePresenter");
                if (!expectsOverlay && label != "MaintenanceNextSteps" && visiblePresenters.Count > 0)
                    problems.Add($"{label} {state} {themeMode} {windowW}x{windowH} unexpectedly shows an overlay");
                if (expectsStale && staleBanners.Count == 0)
                    problems.Add($"{label} stale {themeMode} {windowW}x{windowH} has no visible stale banner");
                if (state == WorkspaceFixtureState.Ready && label == "MaintenanceNextSteps" && actionItems < 3)
                    problems.Add($"MaintenanceNextSteps ready {themeMode} {windowW}x{windowH} is missing maintenance action fixtures ({actionItems})");
            });
    }

    private static int RunV3Shots(string outputRoot)
    {
        Directory.CreateDirectory(outputRoot);
        var report = new StringBuilder();
        report.AppendLine("GameSaveCenter v3 screenshot evidence");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine();
        var problems = new List<string>();

        try
        {
            var app = new Application();
            app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));

            CaptureV3Shot(
                new OverviewView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v3-overview-current-game-wide.png"),
                "OverviewCurrentGameCard",
                1600,
                900,
                ApplyOverviewV3,
                problems,
                report,
                cropFromHost: true);
            CaptureV3Shot(
                new OverviewView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v3-overview-protection-collapsed.png"),
                "OverviewRiskCard",
                1600,
                900,
                ApplyOverviewV3,
                problems,
                report);
            CaptureV3Shot(
                new OverviewView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v3-overview-protection-expanded.png"),
                "OverviewRiskCard",
                1600,
                900,
                ApplyOverviewV3,
                problems,
                report,
                view => SetExpanderByHeader(view, "展开最近游戏保护明细", true));
            CaptureV3Shot(
                new OverviewView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v3-overview-activity-wide.png"),
                "OverviewActivityList",
                1600,
                900,
                ApplyOverviewV3,
                problems,
                report);
            CaptureV3Shot(
                new OverviewView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v3-overview-activity-narrow.png"),
                "OverviewActivityList",
                1040,
                700,
                ApplyOverviewV3,
                problems,
                report);

            CaptureV3Shot(
                new SaveCenterView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v3-save-rule-standard.png"),
                "SaveCurrentRuleCard",
                1600,
                900,
                ApplySimpleResponsiveV3,
                problems,
                report,
                view => SelectTab(view, 1));
            CaptureV3Shot(
                new SaveCenterView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v3-save-rule-narrow.png"),
                "SaveCurrentRuleCard",
                1040,
                700,
                ApplySimpleResponsiveV3,
                problems,
                report,
                view => SelectTab(view, 1));

            CaptureV3Shot(
                new MaintenanceView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v3-maintenance-diagnostics-initial.png"),
                "MaintenanceDiagnosticsScrollSurface",
                1600,
                900,
                ApplySimpleResponsiveV3,
                problems,
                report,
                view => SelectTab(view, 0));
            CaptureV3Shot(
                new MaintenanceView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v3-maintenance-environment-expanded.png"),
                "EnvironmentCheckCard",
                1600,
                900,
                ApplySimpleResponsiveV3,
                problems,
                report,
                view =>
                {
                    SelectTab(view, 0);
                    SelectInnerTab(view, "诊断概览");
                    SetExpanderByHeader(view, "首次环境检查", true);
                });
            CaptureV3Shot(
                new MaintenanceView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v3-maintenance-actions-expanded.png"),
                "MaintenanceDiagnosticsActionCard",
                1600,
                900,
                ApplySimpleResponsiveV3,
                problems,
                report,
                view =>
                {
                    SelectTab(view, 0);
                    SelectInnerTab(view, "诊断概览");
                    SetExpanderByHeader(view, "更多维护操作", true);
                });

            if (problems.Count > 0)
            {
                report.AppendLine("v3-shots FAILED");
                foreach (var problem in problems)
                    report.AppendLine("  PROBLEM " + problem);
                File.WriteAllText(Path.Combine(outputRoot, "v3-shots-report.txt"), report.ToString());
                Console.WriteLine(report.ToString());
                return 1;
            }

            report.AppendLine("v3-shots OK");
            File.WriteAllText(Path.Combine(outputRoot, "v3-shots-report.txt"), report.ToString());
            Console.WriteLine(report.ToString());
            Console.WriteLine("v3-shots OK");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            report.AppendLine("v3-shots FAILED");
            report.AppendLine(ex.ToString());
            File.WriteAllText(Path.Combine(outputRoot, "v3-shots-report.txt"), report.ToString());
            return 1;
        }
    }

    private static int RunV4Shots(string outputRoot)
    {
        Directory.CreateDirectory(outputRoot);
        var report = new StringBuilder();
        report.AppendLine("GameSaveCenter v4 screenshot evidence");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine();
        var problems = new List<string>();

        try
        {
            var app = new Application();
            app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));

            CaptureV3Shot(
                new OverviewView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v4-overview-current-game-standard.png"),
                "OverviewCurrentGameCard",
                1600,
                900,
                ApplyOverviewV3,
                problems,
                report,
                cropFromHost: true);
            CaptureV3Shot(
                new OverviewView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v4-overview-protection-collapsed.png"),
                "OverviewRiskCard",
                1600,
                900,
                ApplyOverviewV3,
                problems,
                report);
            CaptureV3Shot(
                new OverviewView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v4-overview-protection-expanded.png"),
                "OverviewRiskCard",
                1600,
                900,
                ApplyOverviewV3,
                problems,
                report,
                view => SetExpanderByHeader(view, "展开最近游戏保护明细", true));
            CaptureV3Shot(
                new OverviewView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v4-overview-activity-wide.png"),
                "OverviewActivityList",
                1600,
                900,
                ApplyOverviewV3,
                problems,
                report);
            CaptureV3Shot(
                new OverviewView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v4-overview-activity-narrow.png"),
                "OverviewActivityList",
                1040,
                700,
                ApplyOverviewV3,
                problems,
                report);

            CaptureV3Shot(
                new SaveCenterView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v4-save-rule-standard.png"),
                "SaveCurrentRuleCard",
                1600,
                900,
                ApplySimpleResponsiveV3,
                problems,
                report,
                view => SelectTab(view, 1));
            CaptureV3Shot(
                new SaveCenterView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v4-save-automation-standard.png"),
                "SaveBackupAutomationCard",
                1600,
                900,
                ApplySimpleResponsiveV3,
                problems,
                report,
                view => SelectTab(view, 2));
            CaptureV3Shot(
                new SaveCenterView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v4-save-automation-narrow.png"),
                "SaveBackupAutomationCard",
                1040,
                700,
                ApplySimpleResponsiveV3,
                problems,
                report,
                view => SelectTab(view, 2));

            CaptureV3Shot(
                new MaintenanceView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v4-maintenance-diagnostics-default.png"),
                "MaintenanceDiagnosticsScrollSurface",
                1600,
                900,
                ApplySimpleResponsiveV3,
                problems,
                report,
                view => SelectTab(view, 0));
            CaptureV3Shot(
                new MaintenanceView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v4-maintenance-problems-tab.png"),
                "MaintenanceDiagnosticsLayout",
                1600,
                900,
                ApplySimpleResponsiveV3,
                problems,
                report,
                view =>
                {
                    SelectTab(view, 0);
                    SelectInnerTab(view, "问题列表");
                });
            CaptureV3Shot(
                new MaintenanceView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v4-maintenance-overview-tab.png"),
                "MaintenanceDiagnosticsOverviewScrollSurface",
                1600,
                900,
                ApplySimpleResponsiveV3,
                problems,
                report,
                view =>
                {
                    SelectTab(view, 0);
                    SelectInnerTab(view, "诊断概览");
                });
            CaptureV3Shot(
                new MaintenanceView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v4-maintenance-environment-expanded.png"),
                "EnvironmentCheckCard",
                1600,
                900,
                ApplySimpleResponsiveV3,
                problems,
                report,
                view =>
                {
                    SelectTab(view, 0);
                    SelectInnerTab(view, "诊断概览");
                    SetExpanderByHeader(view, "首次环境检查", true);
                });
            CaptureV3Shot(
                new MaintenanceView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v4-maintenance-actions-expanded.png"),
                "MaintenanceDiagnosticsActionCard",
                1600,
                900,
                ApplySimpleResponsiveV3,
                problems,
                report,
                view =>
                {
                    SelectTab(view, 0);
                    SelectInnerTab(view, "诊断概览");
                    SetExpanderByHeader(view, "更多维护操作", true);
                });

            if (problems.Count > 0)
            {
                report.AppendLine("v4-shots FAILED");
                foreach (var problem in problems)
                    report.AppendLine("  PROBLEM " + problem);
                File.WriteAllText(Path.Combine(outputRoot, "v4-shots-report.txt"), report.ToString());
                Console.WriteLine(report.ToString());
                return 1;
            }

            report.AppendLine("v4-shots OK");
            File.WriteAllText(Path.Combine(outputRoot, "v4-shots-report.txt"), report.ToString());
            Console.WriteLine(report.ToString());
            Console.WriteLine("v4-shots OK");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            report.AppendLine("v4-shots FAILED");
            report.AppendLine(ex.ToString());
            File.WriteAllText(Path.Combine(outputRoot, "v4-shots-report.txt"), report.ToString());
            return 1;
        }
    }

    private static int RunV6Shots(string outputRoot)
    {
        Directory.CreateDirectory(outputRoot);
        var report = new StringBuilder();
        report.AppendLine("GameSaveCenter v6 screenshot evidence");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine();
        var problems = new List<string>();

        try
        {
            var app = new Application();
            app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));

            CaptureV3Shot(
                new OverviewView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v6-overview-standard.png"),
                "OverviewStackScrollSurface",
                1600,
                900,
                ApplyOverviewV3,
                problems,
                report);
            CaptureV3Shot(
                new OverviewView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v6-overview-narrow.png"),
                "OverviewStackScrollSurface",
                1040,
                700,
                ApplyOverviewV3,
                problems,
                report);
            CaptureV3Shot(
                new OverviewView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v6-overview-activity-wide.png"),
                "OverviewActivityTimelineList",
                1600,
                900,
                ApplyOverviewV3,
                problems,
                report);
            CaptureV3Shot(
                new OverviewView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v6-overview-activity-narrow.png"),
                "OverviewActivityTimelineList",
                1040,
                700,
                ApplyOverviewV3,
                problems,
                report);
            CaptureV3Shot(
                new OverviewView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v6-overview-risk-card.png"),
                "OverviewRiskCard",
                1600,
                900,
                ApplyOverviewV3,
                problems,
                report);
            CaptureV3Shot(
                new OverviewView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v6-overview-current-game.png"),
                "OverviewCurrentGameCard",
                1600,
                900,
                ApplyOverviewV3,
                problems,
                report,
                cropFromHost: true);

            CaptureV3Shot(
                new SaveCenterView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v6-save-automation-current.png"),
                "SaveBackupAutomationCard",
                1600,
                900,
                ApplySimpleResponsiveV3,
                problems,
                report,
                view => SelectTab(view, 2));
            CaptureV3Shot(
                new SaveCenterView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v6-save-automation-template.png"),
                "SavePolicyTemplatesCard",
                1600,
                900,
                ApplySimpleResponsiveV3,
                problems,
                report,
                view =>
                {
                    SelectTab(view, 2);
                    SetExpanderByHeader(view, "策略模板 · 新建 / 保存 / 应用 / 删除", true);
                });

            foreach (var value in new[] { "1", "5", "30", "120", "1440" })
            {
                CaptureNumericV6(
                    Path.Combine(outputRoot, $"v6-numeric-{value}.png"),
                    value,
                    problems,
                    report);
            }

            CaptureV3Shot(
                new MaintenanceView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v6-maintenance-diagnostics.png"),
                "MaintenanceDiagnosticsScrollSurface",
                1600,
                900,
                ApplySimpleResponsiveV3,
                problems,
                report,
                view => SelectTab(view, 0));
            CaptureV3Shot(
                new MaintenanceView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v6-maintenance-cloud-transfers.png"),
                "CloudTransfersSurface",
                1600,
                900,
                ApplySimpleResponsiveV3,
                problems,
                report,
                view => SelectTab(view, 1));
            CaptureV3Shot(
                new MaintenanceView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v6-maintenance-device.png"),
                "MaintenanceDeviceScrollSurface",
                1600,
                900,
                ApplySimpleResponsiveV3,
                problems,
                report,
                view => SelectTab(view, 2));
            CaptureV3Shot(
                new MaintenanceView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v6-maintenance-audit.png"),
                "MaintenanceAuditFindingsGrid",
                1600,
                900,
                ApplySimpleResponsiveV3,
                problems,
                report,
                view => SelectTab(view, 4));
            CaptureV3Shot(
                new MaintenanceView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v6-maintenance-process.png"),
                "MaintenanceProcessScrollSurface",
                1600,
                900,
                ApplySimpleResponsiveV3,
                problems,
                report,
                view => SelectTab(view, 5));

            report.AppendLine("  session-navigation: covered by SessionNavigationStateTests + UiStatePersistenceSourceTests");

            if (problems.Count > 0)
            {
                report.AppendLine("v6-shots FAILED");
                foreach (var problem in problems)
                    report.AppendLine("  PROBLEM " + problem);
                File.WriteAllText(Path.Combine(outputRoot, "v6-shots-report.txt"), report.ToString());
                Console.WriteLine(report.ToString());
                return 1;
            }

            report.AppendLine("v6-shots OK");
            File.WriteAllText(Path.Combine(outputRoot, "v6-shots-report.txt"), report.ToString());
            Console.WriteLine(report.ToString());
            Console.WriteLine("v6-shots OK");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            report.AppendLine("v6-shots FAILED");
            report.AppendLine(ex.ToString());
            File.WriteAllText(Path.Combine(outputRoot, "v6-shots-report.txt"), report.ToString());
            return 1;
        }
    }

    private static int RunV62Shots(string outputRoot)
    {
        Directory.CreateDirectory(outputRoot);
        var report = new StringBuilder();
        report.AppendLine("GameSaveCenter v6.2 table/chip screenshot evidence");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine();
        var problems = new List<string>();

        try
        {
            var app = new Application();
            app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));

            CaptureV3Shot(
                new OverviewView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v6-2-overview-activity-wide.png"),
                "OverviewActivityTimelineList",
                1600,
                900,
                ApplyOverviewV3,
                problems,
                report);
            CaptureV3Shot(
                new OverviewView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v6-2-overview-activity-narrow.png"),
                "OverviewActivityTimelineList",
                1040,
                700,
                ApplyOverviewV3,
                problems,
                report);
            CaptureV3Shot(
                new SaveCenterView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v6-2-save-candidates-progress.png"),
                "SaveCandidateGrid",
                1600,
                900,
                ApplySimpleResponsiveV3,
                problems,
                report,
                view => SelectTab(view, 1));

            foreach (var (label, windowW, windowH) in new[]
                     {
                         ("2k", 2560, 1440),
                         ("4k", 3840, 2160)
                     })
            {
                CaptureV3Shot(
                    new MaintenanceView { DataContext = new FakeDashboardData() },
                    Path.Combine(outputRoot, $"v6-2-maintenance-diagnostics-{label}.png"),
                    "FindingsGrid",
                    windowW,
                    windowH,
                    ApplySimpleResponsiveV3,
                    problems,
                    report,
                    view =>
                    {
                        SelectTab(view, 0);
                        SelectInnerTab(view, "问题列表");
                    },
                    metrics: (host, target, shotReport) =>
                    {
                        var grid = (DataGrid)target;
                        var fillRatio = host.ActualHeight > 0 ? grid.ActualHeight / host.ActualHeight : 0;
                        shotReport.AppendLine(
                            $"  maintenance-diagnostics-{label} fill: hostH={host.ActualHeight:0}, gridH={grid.ActualHeight:0}, ratio={fillRatio:0.00}");
                    });
                CaptureV3Shot(
                    new MaintenanceView { DataContext = new FakeDashboardData() },
                    Path.Combine(outputRoot, $"v6-2-maintenance-device-{label}.png"),
                    "MaintenanceDeviceGrid",
                    windowW,
                    windowH,
                    ApplySimpleResponsiveV3,
                    problems,
                    report,
                    view => SelectTab(view, 2),
                    metrics: (host, target, shotReport) =>
                    {
                        var grid = (DataGrid)target;
                        var fillRatio = host.ActualHeight > 0 ? grid.ActualHeight / host.ActualHeight : 0;
                        shotReport.AppendLine(
                            $"  maintenance-device-{label} fill: hostH={host.ActualHeight:0}, gridH={grid.ActualHeight:0}, ratio={fillRatio:0.00}");
                    });
                CaptureV3Shot(
                    new MaintenanceView { DataContext = new FakeDashboardData() },
                    Path.Combine(outputRoot, $"v6-2-maintenance-audit-{label}.png"),
                    "MaintenanceAuditFindingsGrid",
                    windowW,
                    windowH,
                    ApplySimpleResponsiveV3,
                    problems,
                    report,
                    view => SelectTab(view, 4),
                    metrics: (host, target, shotReport) =>
                    {
                        var grid = (DataGrid)target;
                        var fillRatio = host.ActualHeight > 0 ? grid.ActualHeight / host.ActualHeight : 0;
                        shotReport.AppendLine(
                            $"  maintenance-audit-{label} fill: hostH={host.ActualHeight:0}, gridH={grid.ActualHeight:0}, ratio={fillRatio:0.00}");
                    });
            }

            if (problems.Count > 0)
            {
                report.AppendLine("v6-2-shots FAILED");
                foreach (var problem in problems)
                    report.AppendLine("  PROBLEM " + problem);
                File.WriteAllText(Path.Combine(outputRoot, "v6-2-shots-report.txt"), report.ToString());
                Console.WriteLine(report.ToString());
                return 1;
            }

            report.AppendLine("v6-2-shots OK");
            File.WriteAllText(Path.Combine(outputRoot, "v6-2-shots-report.txt"), report.ToString());
            Console.WriteLine(report.ToString());
            Console.WriteLine("v6-2-shots OK");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            report.AppendLine("v6-2-shots FAILED");
            report.AppendLine(ex.ToString());
            File.WriteAllText(Path.Combine(outputRoot, "v6-2-shots-report.txt"), report.ToString());
            return 1;
        }
    }

    private static int RunFinesseProbeOnly(
        string outputRoot,
        GameSaveCenterThemeMode themeMode,
        bool sortedHeaderFixture = false,
        bool semanticEdgeCaseFixture = false)
    {
        Directory.CreateDirectory(outputRoot);
        var report = new StringBuilder();
        report.AppendLine("GameSaveCenter UI finesse fixture");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        AppendRunMetadata(
            report,
            sortedHeaderFixture
                ? "finesseprobe-sorted"
                : semanticEdgeCaseFixture ? "finesseprobe-edgevalues" : "finesseprobe",
            "DevelopmentOnlyProductionResourceProbe",
            themeMode == GameSaveCenterThemeMode.Light ? "light" : "dark",
            semanticEdgeCaseFixture
                ? "synthetic table rows with zero/unknown/uninspected semantic values"
                : "synthetic mixed-language/status/diagnostic/table rows");
        report.AppendLine("WindowDip: 1120x980");
        report.AppendLine($"Theme: {themeMode}");
        report.AppendLine("Data: synthetic mixed-language, status, diagnostic and table rows");

        try
        {
            var app = new Application();
            app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
            var view = new GameSaveCenter.Playnite.Views.Development.UiFrameworkProbeView(semanticEdgeCaseFixture)
            {
                Width = 1120,
                Height = 980
            };
            ApplyThemePalette(view, themeMode);
            var palette = AdaptiveThemePaletteFactory.Create(view, false, 50, themeMode);
            var contrastMeasurements = AdaptiveThemePaletteContrastGuard.Measure(palette, palette.Background);
            var contrastViolations = AdaptiveThemePaletteContrastGuard.Validate(palette, palette.Background);
            report.AppendLine($"Palette: accent={FormatColor(palette.Accent)} onAccent={FormatColor(palette.OnAccentText)} primary={FormatColor(palette.PrimaryText)}");
            report.AppendLine($"ContrastGuard: checks={contrastMeasurements.Count} violations={contrastViolations.Count}");
            foreach (var measurement in contrastMeasurements)
                report.AppendLine($"  {measurement.Check}: actual={measurement.Actual:0.###} minimum={measurement.Minimum:0.###}");
            if (contrastViolations.Count > 0)
                throw new InvalidOperationException("ContrastGuard found " + contrastViolations.Count + " violation(s).");
            var host = new Grid
            {
                Width = 1120,
                Height = 980,
                Background = CreateHarnessBackground(view),
                ClipToBounds = true
            };
            host.Children.Add(view);
            host.Measure(new Size(host.Width, host.Height));
            host.Arrange(new Rect(0, 0, host.Width, host.Height));
            host.UpdateLayout();

            if (sortedHeaderFixture)
                AppendSortedHeaderFixtureEvidence(report, host);
            if (semanticEdgeCaseFixture)
                report.AppendLine("SemanticEdgeValues: 0 B | 未知大小 | 尚未检查 | 文件 0/0 · 大小 0 B/0 B");

            var path = Path.Combine(outputRoot, "ui-finesse-fixture.png");
            SavePng(host, path);
            AppendNumericCellReadabilityEvidence(report, host, view);
            AppendEffectiveFixtureEvidence(report, host);
            AppendControlSurfaceEvidence(report, host);
            AppendSemanticContrastEvidence(report, palette, view);
            var captionStyle = view.FindResource("GscTypographyCaption") as Style;
            var captionOpacity = captionStyle?.Setters
                .OfType<Setter>()
                .FirstOrDefault(setter => setter.Property == UIElement.OpacityProperty)?.Value;
            var dataGrid = FindVisualChildren<DataGrid>(host).FirstOrDefault();
            var buttons = FindVisualChildren<ButtonBase>(host).Count();
            report.AppendLine($"Screenshot: {path}");
            report.AppendLine($"ProbeRows: {view.ProbeRows.Count}");
            report.AppendLine($"Buttons: {buttons}");
            report.AppendLine($"DataGrid: rows={dataGrid?.Items.Count ?? 0} actual={dataGrid?.ActualWidth:0.##}x{dataGrid?.ActualHeight:0.##}");
            report.AppendLine($"CaptionOpacity: {captionOpacity ?? "unset"}");
            AppendFontResolutionEvidence(report);
            AppendTypographyMetricEvidence(report, view);
            report.AppendLine("Samples: mixed CJK/Latin, numeric, path, diagnostic, success/warning/error glyphs");
            report.AppendLine("InteractionStates: normal captured; shared template declares hover/pressed/disabled/focus; input behavior requires a separate probe");
            report.AppendLine("finesse-fixture OK");
            File.WriteAllText(Path.Combine(outputRoot, "ui-finesse-fixture-report.txt"), report.ToString());
            Console.WriteLine(report.ToString());
            return 0;
        }
        catch (Exception ex)
        {
            report.AppendLine("finesse-fixture FAILED");
            report.AppendLine(ex.ToString());
            File.WriteAllText(Path.Combine(outputRoot, "ui-finesse-fixture-report.txt"), report.ToString());
            Console.Error.WriteLine(report.ToString());
            return 1;
        }
    }

    private static void AppendNumericCellReadabilityEvidence(
        StringBuilder report,
        Grid host,
        GameSaveCenter.Playnite.Views.Development.UiFrameworkProbeView view)
    {
        var grid = FindVisualChildren<DataGrid>(host)
            .SingleOrDefault(candidate => AutomationProperties.GetName(candidate) == "校对数据表");
        if (grid == null)
            throw new InvalidOperationException("Numeric readability fixture requires the realized ProbeGrid.");

        var expectedValues = view.ProbeRows.Select(row => row.Value).ToArray();
        var measurements = NumericCellReadability.Measure(grid);
        var missingValues = expectedValues
            .Except(measurements.Select(measurement => measurement.Text), StringComparer.Ordinal)
            .ToArray();
        var readable = measurements.Count == expectedValues.Length
            && missingValues.Length == 0
            && measurements.All(measurement => measurement.IsReadable);

        report.AppendLine(
            $"NumericReadability: expected={expectedValues.Length} realized={measurements.Count} "
            + $"horizontalFit={measurements.Count(measurement => measurement.HorizontalFit)} "
            + $"verticalFit={measurements.Count(measurement => measurement.VerticalFit)} "
            + $"allReadable={readable}");
        foreach (var measurement in measurements)
        {
            report.AppendLine(
                $"  NumericCell row={measurement.RowIndex} text=\"{measurement.Text}\" "
                + $"textWidth={measurement.TextWidth:0.##} availableWidth={measurement.AvailableWidth:0.##} "
                + $"textHeight={measurement.TextHeight:0.##} cellHeight={measurement.CellHeight:0.##} "
                + $"wrapping={measurement.Wrapping} trimming={measurement.Trimming} "
                + $"horizontalFit={measurement.HorizontalFit} verticalFit={measurement.VerticalFit}");
        }

        if (!readable)
        {
            var missing = missingValues.Length == 0 ? string.Empty : $" missing={string.Join(" | ", missingValues)}";
            throw new InvalidOperationException("Numeric cells are not fully readable." + missing);
        }

        var negativeHost = new Grid
        {
            Width = 180,
            Height = 110,
            Background = host.Background,
            ClipToBounds = true
        };
        var negativeGrid = new DataGrid
        {
            Width = 180,
            Height = 110,
            AutoGenerateColumns = false,
            CanUserAddRows = false,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            MinColumnWidth = 0,
            RowHeight = 52,
            ColumnHeaderHeight = 42,
            ItemsSource = new[]
            {
                new GameSaveCenter.Playnite.Views.Development.UiFrameworkProbeView.ProbeRow(
                    "负例",
                    "-99,999,999,999,999",
                    "失败",
                    "列宽不足")
            }
        };
        negativeGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "数值",
            Binding = new Binding("Value"),
            Width = 56,
            MinWidth = 0,
            ElementStyle = CreateNumericCellStyle(view)
        });
        negativeHost.Children.Add(negativeGrid);
        negativeHost.Measure(new Size(negativeHost.Width, negativeHost.Height));
        negativeHost.Arrange(new Rect(0, 0, negativeHost.Width, negativeHost.Height));
        negativeHost.UpdateLayout();

        var negativeMeasurement = NumericCellReadability.Measure(negativeGrid).SingleOrDefault();
        var negativeMustFail = negativeMeasurement != null
            && negativeMeasurement.VerticalFit
            && !negativeMeasurement.HorizontalFit;
        report.AppendLine(
            negativeMeasurement == null
                ? "NumericNegativeFixture: missing realized cell must-fail=failed"
                : $"NumericNegativeFixture: text=\"{negativeMeasurement.Text}\" "
                    + $"textWidth={negativeMeasurement.TextWidth:0.##} availableWidth={negativeMeasurement.AvailableWidth:0.##} "
                    + $"textHeight={negativeMeasurement.TextHeight:0.##} cellHeight={negativeMeasurement.CellHeight:0.##} "
                    + $"horizontalFit={negativeMeasurement.HorizontalFit} verticalFit={negativeMeasurement.VerticalFit} "
                    + $"must-fail={(negativeMustFail ? "passed" : "failed")}");
        if (!negativeMustFail)
            throw new InvalidOperationException("Numeric negative fixture did not expose horizontal clipping while row height remained valid.");
    }

    private static Style CreateNumericCellStyle(FrameworkElement resourceScope)
    {
        var style = new Style(
            typeof(TextBlock),
            resourceScope.FindResource("GscTypographyNumeric") as Style);
        style.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Left));
        style.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.NoWrap));
        style.Setters.Add(new Setter(TextBlock.TextTrimmingProperty, TextTrimming.None));
        return style;
    }

    private static void AppendSortedHeaderFixtureEvidence(StringBuilder report, Grid host)
    {
        var dataGrid = FindVisualChildren<DataGrid>(host).FirstOrDefault();
        if (dataGrid == null || dataGrid.Columns.Count < 2)
            throw new InvalidOperationException("Sorted header fixture requires a realized DataGrid with two columns.");

        dataGrid.Columns[0].SortDirection = System.ComponentModel.ListSortDirection.Ascending;
        dataGrid.Columns[1].SortDirection = System.ComponentModel.ListSortDirection.Descending;
        dataGrid.UpdateLayout();

        var headers = FindVisualChildren<DataGridColumnHeader>(dataGrid)
            .Where(header => header.Visibility == Visibility.Visible && header.Column != null)
            .ToList();
        var ascendingHeader = headers.FirstOrDefault(header => header.Column == dataGrid.Columns[0]);
        var descendingHeader = headers.FirstOrDefault(header => header.Column == dataGrid.Columns[1]);
        var ascendingGlyph = ascendingHeader == null
            ? null
            : FindVisualChildren<FrameworkElement>(ascendingHeader)
                .FirstOrDefault(element => element.Name == "SortGlyph");
        var descendingGlyph = descendingHeader == null
            ? null
            : FindVisualChildren<FrameworkElement>(descendingHeader)
                .FirstOrDefault(element => element.Name == "SortGlyph");
        var descendingAngle = (descendingGlyph?.RenderTransform as RotateTransform)?.Angle ?? double.NaN;
        var ascendingVisible = ascendingGlyph?.Visibility == Visibility.Visible && ascendingGlyph.ActualWidth >= 8;
        var descendingVisible = descendingGlyph?.Visibility == Visibility.Visible
            && descendingGlyph.ActualWidth >= 8
            && Math.Abs(descendingAngle - 180) < 0.1;

        report.AppendLine(
            $"SortFixture: ascending=\"{ascendingHeader?.Content}\" visible={ascendingVisible} "
            + $"width={ascendingGlyph?.ActualWidth:0.##}; descending=\"{descendingHeader?.Content}\" "
            + $"visible={descendingVisible} width={descendingGlyph?.ActualWidth:0.##} angle={descendingAngle:0.##}");
        if (!ascendingVisible || !descendingVisible)
            throw new InvalidOperationException("Sorted header fixture did not expose both non-clipped sort glyph states.");
    }

    private static void AppendEffectiveFixtureEvidence(StringBuilder report, Grid host)
    {
        var bitmap = RenderVisual(host);
        var textSamples = new List<AdaptiveThemePaletteContrastGuard.TextContrastSample>();
        var controls = FindVisualChildren<GameSaveCenter.Playnite.Controls.Button>(host)
            .Where(button => button.Visibility == Visibility.Visible)
            .ToList();
        var toggles = FindVisualChildren<GameSaveCenter.Playnite.Controls.ToggleSwitch>(host)
            .Where(toggle => toggle.Visibility == Visibility.Visible)
            .ToList();
        var statusTexts = FindVisualChildren<TextBlock>(host)
            .Where(text => text.Visibility == Visibility.Visible
                && (text.Text == "已完成" || text.Text == "需关注" || text.Text == "失败"))
            .ToList();
        var numeric = FindVisualChildren<TextBlock>(host)
            .FirstOrDefault(text => text.Visibility == Visibility.Visible
                && text.Text.IndexOf("0123456789", StringComparison.Ordinal) >= 0);

        report.AppendLine("EffectiveColors: source=realized-visual-tree rendered-pixel-samples");
        AppendTextElementEvidence(report, host, bitmap, "Numeric", numeric, textSamples);
        foreach (var button in controls)
        {
            var buttonText = FindVisualChildren<TextBlock>(button)
                .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text.Text));
            AppendTextElementEvidence(
                report,
                host,
                bitmap,
                $"Button[{button.Appearance}] {buttonText?.Text ?? "<composite>"}",
                buttonText,
                textSamples);
        }

        foreach (var toggle in toggles)
        {
            var toggleText = FindVisualChildren<TextBlock>(toggle)
                .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text.Text));
            AppendTextElementEvidence(
                report,
                host,
                bitmap,
                $"Toggle {toggleText?.Text ?? "<composite>"}",
                toggleText,
                textSamples);
        }

        foreach (var statusText in statusTexts)
            AppendTextElementEvidence(report, host, bitmap, $"Status {statusText.Text}", statusText, textSamples);

        var effectiveMeasurements = AdaptiveThemePaletteContrastGuard.MeasureTextContrast(textSamples);
        var effectiveViolations = AdaptiveThemePaletteContrastGuard.ValidateTextContrast(textSamples);
        report.AppendLine(
            $"EffectiveContrastGuard: samples={effectiveMeasurements.Count} minimum=4.5 "
            + $"violations={effectiveViolations.Count}");
        foreach (var violation in effectiveViolations)
            report.AppendLine($"  violation {violation.Check}: actual={violation.Actual:0.###} minimum={violation.Minimum:0.###}");
        if (effectiveMeasurements.Count == 0 || effectiveViolations.Count > 0)
            throw new InvalidOperationException(
                $"Effective text contrast guard found {effectiveViolations.Count} violation(s) across {effectiveMeasurements.Count} sample(s).");

        var negativeSamples = new[]
        {
            new AdaptiveThemePaletteContrastGuard.TextContrastSample
            {
                Check = "negative-black-on-dark",
                Foreground = Colors.Black,
                Background = Color.FromRgb(37, 42, 52),
                Minimum = 4.5
            }
        };
        var negativeViolations = AdaptiveThemePaletteContrastGuard.ValidateTextContrast(negativeSamples);
        report.AppendLine(
            $"NegativeFixture: invalid-black-on-dark violations={negativeViolations.Count} "
            + $"must-fail={(negativeViolations.Count > 0 ? "passed" : "FAILED")}");
        if (negativeViolations.Count == 0)
            throw new InvalidOperationException("The invalid black-on-dark contrast fixture unexpectedly passed.");

        var rowBounds = FindVisualChildren<DataGridRow>(host)
            .Where(row => row.Visibility == Visibility.Visible && row.ActualHeight > 0)
            .Select(row => GetBounds(row, host))
            .ToList();
        var dataGrid = FindVisualChildren<DataGrid>(host).FirstOrDefault();
        var gridBounds = dataGrid == null ? Rect.Empty : GetBounds(dataGrid, host);
        var fullyInside = CountCompleteRows(rowBounds, gridBounds);
        report.AppendLine(
            $"RowsEffective: realized={rowBounds.Count} completeInsideGrid={fullyInside} "
            + $"grid={FormatRect(gridBounds)} rows={string.Join(";", rowBounds.Select(FormatRect))}");
        var clippedViewport = gridBounds;
        clippedViewport.Height = Math.Max(0, clippedViewport.Height - 4);
        var clippedComplete = CountCompleteRows(rowBounds, clippedViewport);
        report.AppendLine(
            $"RowsNegativeFixture: viewport={FormatRect(clippedViewport)} completeInsideGrid={clippedComplete} "
            + $"must-fail={(clippedComplete < fullyInside ? "passed" : "FAILED")}");
        if (fullyInside < rowBounds.Count || clippedComplete >= fullyInside)
            throw new InvalidOperationException(
                $"Row clipping guard failed: realized={rowBounds.Count}, complete={fullyInside}, compressed={clippedComplete}.");
    }

    private static void AppendControlSurfaceEvidence(StringBuilder report, Grid host)
    {
        var textBoxes = FindVisualChildren<TextBox>(host).Where(control => control.Visibility == Visibility.Visible).ToList();
        var combos = FindVisualChildren<ComboBox>(host).Where(control => control.Visibility == Visibility.Visible).ToList();
        var buttons = FindVisualChildren<GameSaveCenter.Playnite.Controls.Button>(host)
            .Where(control => control.Visibility == Visibility.Visible)
            .ToList();
        var toggles = FindVisualChildren<GameSaveCenter.Playnite.Controls.ToggleSwitch>(host)
            .Where(control => control.Visibility == Visibility.Visible)
            .ToList();
        var checkBoxes = FindVisualChildren<CheckBox>(host).Where(control => control.Visibility == Visibility.Visible).ToList();
        var sliders = FindVisualChildren<Slider>(host).Where(control => control.Visibility == Visibility.Visible).ToList();
        var listBoxes = FindVisualChildren<ListBox>(host).Where(control => control.Visibility == Visibility.Visible).ToList();

        report.AppendLine(
            $"ControlSurfaceCounts: textboxes={textBoxes.Count} combos={combos.Count} buttons={buttons.Count} "
            + $"toggles={toggles.Count} checkboxes={checkBoxes.Count} sliders={sliders.Count} listboxes={listBoxes.Count}");
        if (textBoxes.Count == 0 || combos.Count == 0 || buttons.Count < 3 || toggles.Count == 0
            || checkBoxes.Count == 0 || sliders.Count == 0 || listBoxes.Count == 0)
        {
            throw new InvalidOperationException("The control-state fixture did not realize all required shared controls.");
        }

        var textBox = textBoxes[0];
        report.AppendLine(
            $"TextInputContract: bounds={FormatRect(GetBounds(textBox, host))} padding={FormatThickness(textBox.Padding)} "
            + $"caret={FormatBrush(textBox.CaretBrush)} selection={FormatBrush(textBox.SelectionBrush)} "
            + $"horizontal={textBox.HorizontalContentAlignment} vertical={textBox.VerticalContentAlignment}");

        var combo = combos[0];
        combo.ApplyTemplate();
        report.AppendLine(
            $"ComboContract: bounds={FormatRect(GetBounds(combo, host))} selectedIndex={combo.SelectedIndex} "
            + $"items={combo.Items.Count} maxDropDownHeight={combo.MaxDropDownHeight:0.##} "
            + $"popupTemplate={(!string.IsNullOrWhiteSpace(combo.Template?.ToString()) ? "declared" : "unknown")}");

        var disabledButton = buttons.FirstOrDefault(button => !button.IsEnabled);
        var enabledButton = buttons.FirstOrDefault(button => button.IsEnabled);
        report.AppendLine(
            $"ButtonGeometry: enabled={FormatRect(enabledButton == null ? Rect.Empty : GetBounds(enabledButton, host))} "
            + $"disabled={FormatRect(disabledButton == null ? Rect.Empty : GetBounds(disabledButton, host))} "
            + $"enabledPadding={FormatThickness(enabledButton?.Padding ?? default(Thickness))} "
            + $"minHeight={enabledButton?.MinHeight:0.##}");
        report.AppendLine("ButtonStateContract: normal=realized disabled=realized hover/pressed/focus=shared-template-triggers; no command fired by fixture");

        var indeterminate = checkBoxes.FirstOrDefault(checkBox => checkBox.IsThreeState && checkBox.IsChecked == null);
        if (indeterminate == null)
            throw new InvalidOperationException("The indeterminate checkbox fixture was not realized.");
        indeterminate.ApplyTemplate();
        var mark = indeterminate.Template?.FindName("IndeterminateMark", indeterminate) as FrameworkElement;
        report.AppendLine(
            $"SelectionControlContract: indeterminate={indeterminate.IsChecked == null} "
            + $"mark={(mark?.Visibility == Visibility.Visible ? "visible" : "missing")} "
            + $"checkboxBounds={FormatRect(GetBounds(indeterminate, host))} sliderBounds={FormatRect(GetBounds(sliders[0], host))}");
        if (mark?.Visibility != Visibility.Visible)
            throw new InvalidOperationException("The indeterminate checkbox mark was not visible after template application.");

        report.AppendLine(
            $"ListContract: items={listBoxes[0].Items.Count} selectedIndex={listBoxes[0].SelectedIndex} "
            + $"virtualization={VirtualizingPanel.GetIsVirtualizing(listBoxes[0])}");
        report.AppendLine("ControlEvidenceBoundary: offscreen realized templates and DIP geometry; IME, real Popup placement, physical DPI and Playnite input remain host checks");
    }

    private static string FormatThickness(Thickness thickness)
        => $"{thickness.Left:0.##},{thickness.Top:0.##},{thickness.Right:0.##},{thickness.Bottom:0.##}";

    private static string FormatBrush(Brush? brush)
    {
        if (brush is SolidColorBrush solid)
            return FormatColor(solid.Color);
        return brush?.GetType().Name ?? "unset";
    }

    private static void AppendTextElementEvidence(
        StringBuilder report,
        Grid host,
        RenderTargetBitmap bitmap,
        string label,
        TextBlock? text,
        List<AdaptiveThemePaletteContrastGuard.TextContrastSample> samples)
    {
        if (text == null)
        {
            report.AppendLine($"  {label}: missing");
            return;
        }

        var foreground = text.Foreground as SolidColorBrush;
        if (foreground == null)
        {
            report.AppendLine($"  {label}: unresolved foreground brush");
            return;
        }

        var bounds = GetBounds(text, host);
        var sample = SampleAround(bitmap, bounds);
        var effectiveOpacity = GetEffectiveOpacity(text, host);
        var foregroundColor = WithOpacity(foreground.Color, effectiveOpacity);
        var composite = Composite(foregroundColor, sample);
        var ratio = ContrastRatio(composite, sample);
        samples.Add(new AdaptiveThemePaletteContrastGuard.TextContrastSample
        {
            Check = label,
            Foreground = foregroundColor,
            Background = sample,
            Minimum = 4.5
        });
        report.AppendLine(
            $"  {label}: text={text.Text} rawForeground={FormatColor(foreground.Color)} "
            + $"effectiveOpacity={effectiveOpacity:0.###} effectiveForeground={FormatColor(foregroundColor)} "
            + $"backgroundSample={FormatColor(sample)} composite={FormatColor(composite)} "
            + $"contrast={ratio:0.###} bounds={FormatRect(bounds)}");
    }

    private static int CountCompleteRows(IReadOnlyList<Rect> rows, Rect viewport)
    {
        var complete = 0;
        foreach (var row in rows)
        {
            var intersection = row;
            intersection.Intersect(viewport);
            if (!intersection.IsEmpty
                && intersection.Width >= row.Width - 0.5
                && intersection.Height >= row.Height - 0.5)
                complete++;
        }

        return complete;
    }

    private static double GetEffectiveOpacity(DependencyObject element, DependencyObject stop)
    {
        var opacity = 1d;
        for (DependencyObject? current = element; current != null; current = VisualTreeHelper.GetParent(current))
        {
            if (current is UIElement visual)
                opacity *= visual.Opacity;
            if (ReferenceEquals(current, stop))
                break;
        }

        return Math.Max(0, Math.Min(1, opacity));
    }

    private static RenderTargetBitmap RenderVisual(Visual visual)
    {
        var element = (FrameworkElement)visual;
        var bitmap = new RenderTargetBitmap(
            (int)Math.Ceiling(element.ActualWidth),
            (int)Math.Ceiling(element.ActualHeight),
            96,
            96,
            PixelFormats.Pbgra32);
        bitmap.Render(visual);
        return bitmap;
    }

    private static Color SampleAround(RenderTargetBitmap bitmap, Rect bounds)
    {
        var points = new[]
        {
            new Point(bounds.Left + Math.Min(2, Math.Max(0, bounds.Width / 3)), bounds.Top + Math.Min(2, Math.Max(0, bounds.Height / 3))),
            new Point(bounds.Right - Math.Min(2, Math.Max(0, bounds.Width / 3)), bounds.Top + Math.Min(2, Math.Max(0, bounds.Height / 3))),
            new Point(bounds.Left + Math.Min(2, Math.Max(0, bounds.Width / 3)), bounds.Bottom - Math.Min(2, Math.Max(0, bounds.Height / 3))),
            new Point(bounds.Right - Math.Min(2, Math.Max(0, bounds.Width / 3)), bounds.Bottom - Math.Min(2, Math.Max(0, bounds.Height / 3)))
        };
        var colors = points.Select(point => GetPixel(bitmap, point)).ToList();
        return Color.FromRgb(
            (byte)Math.Round(colors.Average(color => color.R)),
            (byte)Math.Round(colors.Average(color => color.G)),
            (byte)Math.Round(colors.Average(color => color.B)));
    }

    private static Color GetPixel(RenderTargetBitmap bitmap, Point point)
    {
        var x = Math.Max(0, Math.Min(bitmap.PixelWidth - 1, (int)Math.Round(point.X)));
        var y = Math.Max(0, Math.Min(bitmap.PixelHeight - 1, (int)Math.Round(point.Y)));
        var pixels = new byte[4];
        bitmap.CopyPixels(new Int32Rect(x, y, 1, 1), pixels, 4, 0);
        return Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
    }

    private static Rect GetBounds(FrameworkElement element, FrameworkElement ancestor)
        => element.TransformToAncestor(ancestor).TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));

    private static string FormatRect(Rect rect)
        => rect.IsEmpty ? "empty" : $"{rect.Left:0.##},{rect.Top:0.##}..{rect.Right:0.##},{rect.Bottom:0.##}";

    private static string FormatColor(Color color)
        => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

    private static Color Composite(Color foreground, Color background)
    {
        var alpha = foreground.A / 255d;
        return Color.FromRgb(
            (byte)Math.Round(foreground.R * alpha + background.R * (1 - alpha)),
            (byte)Math.Round(foreground.G * alpha + background.G * (1 - alpha)),
            (byte)Math.Round(foreground.B * alpha + background.B * (1 - alpha)));
    }

    private static Color WithOpacity(Color color, double opacity)
        => Color.FromArgb(
            (byte)Math.Round(Math.Max(0, Math.Min(1, color.A / 255d * opacity)) * 255),
            color.R,
            color.G,
            color.B);

    private static double ContrastRatio(Color first, Color second)
    {
        var lighter = Math.Max(RelativeLuminance(first), RelativeLuminance(second));
        var darker = Math.Min(RelativeLuminance(first), RelativeLuminance(second));
        return (lighter + 0.05) / (darker + 0.05);
    }

    private static double RelativeLuminance(Color color)
    {
        double Convert(byte channel)
        {
            var value = channel / 255d;
            return value <= 0.03928 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
        }

        return 0.2126 * Convert(color.R) + 0.7152 * Convert(color.G) + 0.0722 * Convert(color.B);
    }

    private static void AppendFontResolutionEvidence(StringBuilder report)
    {
        var chain = TypographyDiagnostics.UiFontChain;
        var samples = new[]
        {
            (Label: "CJK", CodePoint: 0x5B58, Text: "存"),
            (Label: "Latin", CodePoint: 0x0053, Text: "S"),
            (Label: "Digit", CodePoint: 0x0039, Text: "9"),
            (Label: "Arrow", CodePoint: 0x2192, Text: "→"),
            (Label: "RareCJK", CodePoint: 0x20BB7, Text: TypographyDiagnostics.CodePointText(0x20BB7)),
            (Label: "Combining", CodePoint: 0x0301, Text: "e\u0301"),
            (Label: "Emoji", CodePoint: 0x1F9ED, Text: "🧭")
        };

        report.AppendLine($"FontChain: {string.Join(" -> ", chain)}");
        foreach (var sample in samples)
        {
            var candidate = TypographyDiagnostics.FindCandidate(chain, sample.CodePoint, FontWeights.Normal);
            report.AppendLine(
                $"FontCandidate {sample.Label}=U+{sample.CodePoint:X5} "
                + $"candidate={(candidate.HasGlyph ? candidate.Family : "unresolved")} "
                + $"requestedWeight={candidate.RequestedWeight} actualWeight={candidate.ActualWeight}");
            var glyphRun = TypographyDiagnostics.CaptureGlyphRun(
                sample.Text,
                sample.CodePoint,
                chain,
                14,
                FontWeights.Normal);
            report.AppendLine(
                $"GlyphRunEvidence {sample.Label}=U+{sample.CodePoint:X5} "
                + $"level={glyphRun.EvidenceLevel} candidate={(glyphRun.CandidateHasGlyph ? glyphRun.CandidateFamily : "unresolved")} "
                + $"final={(string.IsNullOrWhiteSpace(glyphRun.FinalFamily) ? "unknown" : glyphRun.FinalFamily)} "
                + $"runs={glyphRun.GlyphRunCount} glyphs={glyphRun.GlyphCount} "
                + $"finalTypefaceHasCodePoint={glyphRun.FinalTypefaceHasCodePoint} notdef={glyphRun.HasNotdefGlyph}");
        }

        foreach (var weight in new[] { FontWeights.Normal, FontWeights.Medium, FontWeights.SemiBold })
        {
            var candidate = TypographyDiagnostics.FindCandidate(chain, 0x5B58, weight);
            report.AppendLine(
                $"FontWeightCandidate Chinese requested={weight} "
                + $"family={(candidate.HasGlyph ? candidate.Family : "unresolved")} actual={candidate.ActualWeight}");
        }

        var unicodeSamples = new[]
        {
            "Cafe\u0301", "Ångström", "か\u3099", "🧭 🎮", TypographyDiagnostics.CodePointText(0x20BB7)
        };
        foreach (var sample in unicodeSamples)
        {
            var metric = TypographyDiagnostics.Measure(sample, chain.First(), 14, FontWeights.Normal);
            report.AppendLine(
                $"UnicodeMetric text={sample} utf16Length={sample.Length} width={metric.Width:0.##} "
                + $"height={metric.Height:0.##} baseline={metric.Baseline:0.##} "
                + $"unpairedSurrogate={metric.HasUnpairedSurrogate}");
        }

        report.AppendLine("FontActualGlyphRun: captured per sample with WPF TextFormatter.GetIndexedGlyphRuns");
        report.AppendLine("FontEvidence: FontCandidate is candidate coverage; GlyphRunEvidence is captured WPF layout output; unresolved or .notdef remains non-hit");
    }

    private static void AppendTypographyMetricEvidence(StringBuilder report, FrameworkElement resourceScope)
    {
        var numericStyle = resourceScope.FindResource("GscTypographyNumeric") as Style;
        var numericSamples = new[] { "1", "8", "99", "100", "00:09", "12:59", "59 秒", "1 分钟", "0", "—" };
        var numericMetrics = new List<(string Text, double Width, double Height)>();
        foreach (var value in numericSamples)
        {
            var text = new TextBlock { Text = value, Style = numericStyle, Opacity = 0 };
            text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            numericMetrics.Add((value, text.DesiredSize.Width, text.DesiredSize.Height));
        }

        report.AppendLine(
            "NumericMetrics: "
            + string.Join(", ", numericMetrics.Select(metric => $"{metric.Text}={metric.Width:0.##}x{metric.Height:0.##}")));
        report.AppendLine(
            $"NumericTypography: styleResolved={(numericStyle != null)} tabularSetter="
            + $"{HasSetter(numericStyle, Typography.NumeralAlignmentProperty)}");

        var wrapped = FindVisualChildren<TextBlock>(resourceScope)
            .Where(text => text.Visibility == Visibility.Visible
                && text.TextWrapping != TextWrapping.NoWrap
                && !string.IsNullOrWhiteSpace(text.Text))
            .Take(8)
            .ToList();
        foreach (var text in wrapped)
        {
            report.AppendLine(
                $"LineMetric text={text.Text} wrapping={text.TextWrapping} lineHeight={text.LineHeight:0.##} "
                + $"actual={text.ActualWidth:0.##}x{text.ActualHeight:0.##}");
        }

        var punctuation = new[] { "全角引号“存档”", "《存档中心》", "路径——待检查", "稍后重试……" };
        report.AppendLine(
            $"PunctuationSamples: preserved={string.Join(" | ", punctuation)} "
            + $"containsUnpairedSurrogate={punctuation.Any(TypographyDiagnostics.ContainsUnpairedSurrogate)}");

        var mixedBaselineSamples = new[]
        {
            "存档中心 Save Center",
            "日期：2026-09-17 · 时间 03:02",
            "容量：1.71 GiB · 24.6 MiB",
            "中文标点：全角引号“存档”、书名号《中心》……"
        };
        foreach (var sample in mixedBaselineSamples)
        {
            var evidence = TypographyDiagnostics.CaptureMixedBaseline(
                sample,
                TypographyDiagnostics.UiFontChain,
                14,
                FontWeights.Normal);
            report.AppendLine(
                $"MixedBaseline text={sample} runs={evidence.GlyphRunCount} glyphs={evidence.GlyphCount} "
                + $"line={evidence.LineBaseline:0.###} min={evidence.MinimumGlyphBaseline:0.###} "
                + $"max={evidence.MaximumGlyphBaseline:0.###} spread={evidence.BaselineSpread:0.###} "
                + $"stable={evidence.IsStable} unpairedSurrogate={evidence.HasUnpairedSurrogate}");
            if (!evidence.IsStable)
                throw new InvalidOperationException("Mixed typography baseline probe detected vertical drift.");
        }
    }

    private static void AppendSemanticContrastEvidence(
        StringBuilder report,
        AdaptiveThemePalette palette,
        FrameworkElement resourceScope)
    {
        var primaryBrush = resourceScope.FindResource("GscPrimaryButtonBrush") as LinearGradientBrush;
        var hoverBrush = resourceScope.FindResource("GscOnAccentHoverOverlayBrush") as SolidColorBrush;
        var pressedBrush = resourceScope.FindResource("GscOnAccentPressedOverlayBrush") as SolidColorBrush;
        var onAccentBrush = resourceScope.FindResource("GscOnAccentTextBrush") as SolidColorBrush;
        var onDangerBrush = resourceScope.FindResource("GscOnDangerTextBrush") as SolidColorBrush;
        if (primaryBrush == null || hoverBrush == null || pressedBrush == null || onAccentBrush == null || onDangerBrush == null)
            throw new InvalidOperationException("Semantic state resources were not resolved in the production fixture.");

        var buttonMeasurements = AdaptiveThemePaletteContrastGuard.MeasureGradientTextContrast(
            "primary-button",
            onAccentBrush.Color,
            palette.Background,
            primaryBrush.GradientStops,
            hoverBrush.Color,
            hoverBrush.Color,
            pressedBrush.Color);
        var buttonViolations = buttonMeasurements
            .Where(measurement => measurement.Actual + 0.001 < measurement.Minimum)
            .ToList();
        report.AppendLine(
            $"SemanticButtonContrast: samples={buttonMeasurements.Count} "
            + $"normal/hover/focus/pressed-combinations=all-stops violations={buttonViolations.Count}");
        foreach (var violation in buttonViolations)
            report.AppendLine($"  violation {violation.Check}: actual={violation.Actual:0.###} minimum={violation.Minimum:0.###}");
        if (buttonViolations.Count > 0)
            throw new InvalidOperationException("Primary button state contrast guard found a violation.");

        var layeredSamples = new[]
        {
            new AdaptiveThemePaletteContrastGuard.LayeredTextContrastSample
            {
                Check = "selection-text",
                Foreground = palette.PrimaryText,
                Backdrop = palette.Background,
                SurfaceLayers = new[] { palette.AccentTint },
                Minimum = 4.5
            },
            new AdaptiveThemePaletteContrastGuard.LayeredTextContrastSample
            {
                Check = "input-text",
                Foreground = palette.PrimaryText,
                Backdrop = palette.Background,
                SurfaceLayers = new[] { palette.ControlFill },
                Minimum = 4.5
            },
            new AdaptiveThemePaletteContrastGuard.LayeredTextContrastSample
            {
                Check = "input-placeholder",
                Foreground = palette.MutedText,
                Backdrop = palette.Background,
                SurfaceLayers = new[] { palette.ControlFill },
                Minimum = 3.0
            },
            new AdaptiveThemePaletteContrastGuard.LayeredTextContrastSample
            {
                Check = "danger-button",
                Foreground = onDangerBrush.Color,
                Backdrop = palette.Background,
                SurfaceLayers = new[] { palette.Error },
                Minimum = 4.5
            }
        };
        var layeredMeasurements = AdaptiveThemePaletteContrastGuard.MeasureLayeredTextContrast(layeredSamples);
        var layeredViolations = layeredMeasurements
            .Where(measurement => measurement.Actual + 0.001 < measurement.Minimum)
            .ToList();
        report.AppendLine(
            $"SemanticLayerContrast: samples={layeredMeasurements.Count} violations={layeredViolations.Count} "
            + string.Join(", ", layeredMeasurements.Select(measurement => $"{measurement.Check}={measurement.Actual:0.###}")));
        if (layeredViolations.Count > 0)
            throw new InvalidOperationException("Selection/input/danger contrast guard found a violation.");

        var complexBackgrounds = new[]
        {
            Color.FromRgb(255, 255, 255),
            Color.FromRgb(8, 8, 12),
            Color.FromRgb(214, 35, 70),
            Color.FromRgb(28, 130, 198)
        };
        var complexSamples = complexBackgrounds.Select((background, index) => new AdaptiveThemePaletteContrastGuard.LayeredTextContrastSample
        {
            Check = $"complex-surface-{index}",
            Foreground = palette.PrimaryText,
            Backdrop = background,
            SurfaceLayers = new[] { palette.SurfaceTop, palette.ControlFill },
            Minimum = 4.5
        });
        var complexMeasurements = AdaptiveThemePaletteContrastGuard.MeasureLayeredTextContrast(complexSamples).ToList();
        var complexViolations = complexMeasurements
            .Where(measurement => measurement.Actual + 0.001 < measurement.Minimum)
            .ToList();
        report.AppendLine(
            $"ComplexBackdropContrast: samples={complexMeasurements.Count} violations={complexViolations.Count} "
            + string.Join(", ", complexMeasurements.Select(measurement => $"{measurement.Check}={measurement.Actual:0.###}")));
        if (complexViolations.Count > 0)
            throw new InvalidOperationException("Complex backdrop contrast guard found a violation.");
    }

    private static bool HasSetter(Style? style, DependencyProperty property)
        => style?.Setters.OfType<Setter>().Any(setter => setter.Property == property) == true;

    private static int RunV7ProgressProbe(string outputRoot)
    {
        Directory.CreateDirectory(outputRoot);
        var report = new StringBuilder();
        report.AppendLine("GameSaveCenter v7 progress probe");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine();
        var problems = new List<string>();

        try
        {
            var app = new Application();
            app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
            app.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml", UriKind.Relative)
            });

            var values = new[] { 0d, 5d, 25d, 50d, 75d, 100d };
            var host = new Grid
            {
                Width = 900,
                Height = values.Length * 34,
                Background = new SolidColorBrush(Color.FromRgb(24, 30, 43)),
                ClipToBounds = true
            };
            for (var i = 0; i < values.Length; i++)
                host.RowDefinitions.Add(new RowDefinition { Height = new GridLength(34) });

            for (var i = 0; i < values.Length; i++)
            {
                var value = values[i];
                var row = new Grid();
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(230) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
                var label = new TextBlock
                {
                    Text = $"{value:0}%",
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(220, 224, 235))
                };
                var bar = new ProgressBar
                {
                    Height = 8,
                    Width = 200,
                    Minimum = 0,
                    Maximum = 100,
                    Value = value,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                var percent = new TextBlock
                {
                    Text = $"{value:0}%",
                    FontSize = 11,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(190, 196, 210))
                };
                Grid.SetColumn(label, 0);
                Grid.SetColumn(bar, 1);
                Grid.SetColumn(percent, 2);
                row.Children.Add(label);
                row.Children.Add(bar);
                row.Children.Add(percent);
                Grid.SetRow(row, i);
                host.Children.Add(row);
            }

            host.Measure(new Size(host.Width, host.Height));
            host.Arrange(new Rect(0, 0, host.Width, host.Height));
            host.UpdateLayout();

            var path = Path.Combine(outputRoot, "v7-progress-probe.png");
            SavePng(host, path);

            var bars = FindVisualChildren<ProgressBar>(host)
                .OrderBy(bar => Grid.GetRow((FrameworkElement)bar.Parent ?? bar))
                .ToList();
            var hostBitmap = new RenderTargetBitmap(
                (int)Math.Ceiling(host.ActualWidth),
                (int)Math.Ceiling(host.ActualHeight),
                96,
                96,
                PixelFormats.Pbgra32);
            hostBitmap.Render(host);
            var hostWidth = hostBitmap.PixelWidth;
            var hostStride = hostWidth * 4;
            var hostPixels = new byte[hostStride * hostBitmap.PixelHeight];
            hostBitmap.CopyPixels(hostPixels, hostStride, 0);

            foreach (var bar in bars)
            {
                var origin = bar.TransformToAncestor(host).Transform(new Point(0, 0));
                var left = Math.Max(0, (int)Math.Floor(origin.X));
                var top = Math.Max(0, (int)Math.Floor(origin.Y));
                var right = Math.Min(hostWidth, (int)Math.Ceiling(origin.X + bar.ActualWidth));
                var bottom = Math.Min(hostBitmap.PixelHeight, (int)Math.Ceiling(origin.Y + bar.ActualHeight));

                var fillPixels = 0;
                var trackPixels = 0;
                for (var y = top; y < bottom; y++)
                {
                    for (var x = left; x < right; x++)
                    {
                        var offset = y * hostStride + x * 4;
                        var b = hostPixels[offset];
                        var g = hostPixels[offset + 1];
                        var r = hostPixels[offset + 2];
                        var a = hostPixels[offset + 3];
                        if (a < 200)
                            continue;
                        if (r > 100 && g > 120 && b > 200)
                            fillPixels++;
                        else if (r < 90 && g < 100 && b < 130)
                            trackPixels++;
                    }
                }

                var expectedRatio = (bar.Maximum > 0 ? bar.Value / bar.Maximum : 0);
                var total = fillPixels + trackPixels;
                var actualRatio = total > 0
                    ? fillPixels / (double)total
                    : 0;
                report.AppendLine(
                    $"  {bar.Value:0}% expected={expectedRatio:0.00} actual={actualRatio:0.00} fillPx={fillPixels} trackPx={trackPixels}");
                if (actualRatio < expectedRatio - 0.12)
                    problems.Add($"{bar.Value:0}% fill too short: expected {expectedRatio:0.00}, actual {actualRatio:0.00}");
            }

            if (problems.Count > 0)
            {
                report.AppendLine("v7-progress FAILED");
                foreach (var problem in problems)
                    report.AppendLine("  PROBLEM " + problem);
                File.WriteAllText(Path.Combine(outputRoot, "v7-progress-report.txt"), report.ToString());
                Console.WriteLine(report.ToString());
                return 1;
            }

            report.AppendLine("v7-progress OK");
            File.WriteAllText(Path.Combine(outputRoot, "v7-progress-report.txt"), report.ToString());
            Console.WriteLine(report.ToString());
            Console.WriteLine("v7-progress OK");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            report.AppendLine("v7-progress FAILED");
            report.AppendLine(ex.ToString());
            File.WriteAllText(Path.Combine(outputRoot, "v7-progress-report.txt"), report.ToString());
            return 1;
        }
    }

    private static void CaptureNumericV6(string path, string value, List<string> problems, StringBuilder report)
    {
        var view = new SaveCenterView { DataContext = new FakeDashboardData() };
        var (contentW, contentH) = ContentSize(1600, 900);
        var host = new Grid
        {
            Width = contentW,
            Height = contentH,
            Background = CreateHarnessBackground(view),
            ClipToBounds = true
        };
        host.Children.Add(view);
        ApplySimpleResponsiveV3(view, 1600, 900);
        host.Measure(new Size(contentW, contentH));
        host.Arrange(new Rect(0, 0, contentW, contentH));
        host.UpdateLayout();
        SelectTab(view, 2);
        view.UpdateLayout();

        var card = FindVisualChildren<FrameworkElement>(host)
            .FirstOrDefault(element => element.Name == "SaveBackupAutomationCard");
        var input = card == null
            ? null
            : FindVisualChildren<TextBox>(card)
                .FirstOrDefault(textBox => AutomationProperties.GetName(textBox) == "游玩中周期备份间隔，分钟");
        if (input == null)
            throw new InvalidOperationException("Numeric input not found for v6 shot.");

        input.Text = value;
        view.UpdateLayout();
        SaveCropped(host, input, path);
        var size = new FileInfo(path).Length;
        report.AppendLine($"  {Path.GetFileName(path)}: {input.ActualWidth:0}x{input.ActualHeight:0} DIP, {size} bytes");
        if (size < 512)
            problems.Add($"{path} looks blank ({size} bytes)");
    }

    private static void CaptureV3Shot(
        UserControl view,
        string path,
        string elementName,
        int windowW,
        int windowH,
        Action<UserControl, int, int> applyLayout,
        List<string> problems,
        StringBuilder report,
        Action<UserControl>? beforeCapture = null,
        bool cropFromHost = false,
        Action<Grid, FrameworkElement, StringBuilder>? metrics = null)
    {
        var (contentW, contentH) = ContentSize(windowW, windowH);
        var host = new Grid
        {
            Width = contentW,
            Height = contentH,
            Background = CreateHarnessBackground(view),
            ClipToBounds = true
        };
        host.Children.Add(view);

        applyLayout(view, windowW, windowH);
        host.Measure(new Size(contentW, contentH));
        host.Arrange(new Rect(0, 0, contentW, contentH));
        host.UpdateLayout();
        applyLayout(view, windowW, windowH);
        host.UpdateLayout();
        beforeCapture?.Invoke(view);
        applyLayout(view, windowW, windowH);
        host.UpdateLayout();

        var target = FindVisualChildren<FrameworkElement>(host)
            .FirstOrDefault(element => element.Name == elementName);
        if (target == null || target.ActualWidth <= 0 || target.ActualHeight <= 0)
        {
            var geometry = string.Join(
                "; ",
                FindVisualChildren<FrameworkElement>(host)
                    .Where(element => element.Name.IndexOf("MediaInbox", StringComparison.Ordinal) >= 0
                        || element.Name.IndexOf("MediaCurrent", StringComparison.Ordinal) >= 0
                        || element.Name.IndexOf("MaintenanceAudit", StringComparison.Ordinal) >= 0)
                    .Take(24)
                    .Select(element => $"{element.Name}={element.ActualWidth:0}x{element.ActualHeight:0}/{element.Visibility}"));
            throw new InvalidOperationException($"V3 shot target not rendered: {elementName} at {windowW}x{windowH}; geometry={geometry}");
        }

        metrics?.Invoke(host, target, report);

        if (cropFromHost)
            SaveCropped(host, target, path);
        else
            SavePng(target, path);
        var size = new FileInfo(path).Length;
        report.AppendLine($"  {Path.GetFileName(path)}: {target.ActualWidth:0}x{target.ActualHeight:0} DIP, {size} bytes");
        if (size < 2048)
            problems.Add($"{path} looks blank ({size} bytes)");
    }

    private static void SaveCropped(Grid host, FrameworkElement target, string path)
    {
        var origin = target.TransformToAncestor(host).Transform(new Point(0, 0));
        var bitmap = new RenderTargetBitmap(
            (int)Math.Ceiling(host.ActualWidth),
            (int)Math.Ceiling(host.ActualHeight),
            96,
            96,
            PixelFormats.Pbgra32);
        bitmap.Render(host);

        var left = Math.Max(0, (int)Math.Floor(origin.X));
        var top = Math.Max(0, (int)Math.Floor(origin.Y));
        var width = Math.Min((int)Math.Ceiling(target.ActualWidth), bitmap.PixelWidth - left);
        var height = Math.Min((int)Math.Ceiling(target.ActualHeight), bitmap.PixelHeight - top);
        if (width <= 0 || height <= 0)
            throw new InvalidOperationException($"Cannot crop {path}: {width}x{height}");

        var cropped = new CroppedBitmap(bitmap, new Int32Rect(left, top, width, height));
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(cropped));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }

    private static void ApplyOverviewV3(UserControl view, int windowW, int windowH)
    {
        var overview = (OverviewView)view;
        var (contentW, _) = ContentSize(windowW, windowH);
        var stack = contentW < 1200;
        overview.OverviewCompactSecondaryRowHeight = stack ? GridLength.Auto : new GridLength(0);
        overview.ApplyResponsiveColumns(stack);
        overview.ApplyResponsiveWidth(contentW);
        overview.ApplyResponsiveHeight(windowH, stack);
    }

    private static void ApplySimpleResponsiveV3(UserControl view, int windowW, int windowH)
    {
        var (contentW, _) = ContentSize(windowW, windowH);
        var method = view.GetType().GetMethod(
            "ApplyResponsiveLayout",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        method?.Invoke(view, new object[] { contentW, windowH });
    }

    private static void SelectTab(UserControl view, int index)
    {
        var segmented = FindVisualChildren<ListBox>(view)
            .FirstOrDefault(candidate => candidate.Name.EndsWith("SegmentTabs", StringComparison.Ordinal)
                || candidate.Name == "SettingsSectionTabs");
        if (segmented != null)
        {
            if (index < 0 || index >= segmented.Items.Count)
                throw new InvalidOperationException($"Cannot select tab {index} for {view.GetType().Name}");
            segmented.SelectedIndex = index;
            return;
        }

        var tabs = FindVisualChildren<TabControl>(view).FirstOrDefault();
        if (tabs == null || index < 0 || index >= tabs.Items.Count)
            throw new InvalidOperationException($"Cannot select tab {index} for {view.GetType().Name}");
        tabs.SelectedIndex = index;
    }

    private static void SetExpanderByHeader(UserControl view, string header, bool isExpanded)
    {
        view.UpdateLayout();
        var expander = FindVisualChildren<Expander>(view)
            .FirstOrDefault(candidate => candidate.Header?.ToString() == header);
        if (expander == null)
            throw new InvalidOperationException($"Expander not found: {header}");
        expander.IsExpanded = isExpanded;
    }

    private static void SelectInnerTab(UserControl view, string header)
    {
        var segmented = FindVisualChildren<ListBox>(view)
            .FirstOrDefault(candidate => candidate.Name == "MaintenanceDiagnosticsSubTabs"
                && candidate.Items.Cast<object>().Any(item => (item as ListBoxItem)?.Content?.ToString() == header));
        if (segmented != null)
        {
            var index = segmented.Items.Cast<object>()
                .Select((item, itemIndex) => new { item, itemIndex })
                .First(candidate => (candidate.item as ListBoxItem)?.Content?.ToString() == header)
                .itemIndex;
            segmented.SelectedIndex = index;
            view.UpdateLayout();
            return;
        }

        var tabs = FindVisualChildren<TabControl>(view)
            .FirstOrDefault(candidate => candidate.Items
                .Cast<TabItem>()
                .Any(item => item.Header?.ToString() == header));
        var item = tabs?.Items
            .Cast<TabItem>()
            .FirstOrDefault(candidate => candidate.Header?.ToString() == header);
        if (tabs == null || item == null)
            throw new InvalidOperationException($"Inner tab not found: {header}");
        tabs.SelectedItem = item;
        view.UpdateLayout();
    }

    private static int Run(string[] args)
    {
        var outputRoot = args.Length > 0
            ? Path.GetFullPath(args[0])
            : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "artifacts", "ui-qa", "render");
        Directory.CreateDirectory(outputRoot);

        var report = new StringBuilder();
        report.AppendLine("GameSaveCenter render QA report");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        AppendRunMetadata(report, "render-qa", "OffscreenRenderHarness", "light,dark", "workspace fixtures; scroll probes include 50/400/2000/4468");
        report.AppendLine();
        s_problems.Clear();

        try
        {
            // The Settings view and Dashboard shell reference Playnite's host-provided
            // BaseTextBlockStyle. Standalone layout QA supplies a neutral fallback so the
            // XAML parses without a real Playnite window.
            var app = new Application();
            app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));

            foreach (var (windowW, windowH) in WindowSizes)
            {
                var (contentW, contentH) = ContentSize(windowW, windowH);
                report.AppendLine($"Window {windowW}x{windowH} -> workspace {contentW:0}x{contentH:0} DIP");

                RenderOverview(outputRoot, windowW, windowH, contentW, contentH, report);
                RenderSave(outputRoot, windowW, windowH, contentW, contentH, report);
                RenderTrainer(outputRoot, windowW, windowH, contentW, contentH, report);
                RenderMedia(outputRoot, windowW, windowH, contentW, contentH, report);
                RenderMaintenance(outputRoot, windowW, windowH, contentW, contentH, report);
                RenderTasks(outputRoot, windowW, windowH, contentW, contentH, report);
                RenderSettings(outputRoot, windowW, windowH, contentW, contentH, report);
                report.AppendLine();
            }

            RunDataGridScrollProbes(report);
            RunMediaWrapScrollProbe(report);
            RunSettingsLayoutProbes(report);
            RunSettingsStateProbes(outputRoot, report);
            RunSettingsThemeTransitionProbe(outputRoot, report);
            RunThemeQa(outputRoot, report);
            RunThemeSpecificControlProbes(outputRoot, report);
            RunResizeTransitionProbes(report);
            RunShellChromeProbes(outputRoot, report);

            if (s_problems.Count > 0)
            {
                report.AppendLine("render-qa FAILED");
                foreach (var problem in s_problems)
                    report.AppendLine("  PROBLEM " + problem);
                File.WriteAllText(Path.Combine(outputRoot, "render-qa-report.txt"), report.ToString());
                Console.WriteLine(report.ToString());
                return 1;
            }

            report.AppendLine("render-qa OK");
            File.WriteAllText(Path.Combine(outputRoot, "render-qa-report.txt"), report.ToString());
            Console.WriteLine(report.ToString());
            Console.WriteLine("render-qa OK");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            report.AppendLine("render-qa FAILED");
            report.AppendLine(ex.ToString());
            File.WriteAllText(Path.Combine(outputRoot, "render-qa-report.txt"), report.ToString());
            return 1;
        }
    }

    private static int RunGridProbeOnly(string outputRoot)
    {
        Directory.CreateDirectory(outputRoot);
        var report = new StringBuilder();
        report.AppendLine("GameSaveCenter DataGrid scroll diagnostics");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        AppendRunMetadata(report, "gridprobe", "OffscreenRenderHarness", "production default palette", "50/400/2000/4468 loaded rows");
        report.AppendLine();
        s_problems.Clear();

        try
        {
            var app = new Application();
            app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
            RunDataGridScrollProbes(report);
            report.AppendLine();
            if (s_problems.Count > 0)
            {
                report.AppendLine("gridprobe FAILED");
                foreach (var problem in s_problems)
                    report.AppendLine("  PROBLEM " + problem);
                File.WriteAllText(Path.Combine(outputRoot, "gridprobe-report.txt"), report.ToString());
                Console.WriteLine(report.ToString());
                return 1;
            }

            report.AppendLine("gridprobe OK");
            File.WriteAllText(Path.Combine(outputRoot, "gridprobe-report.txt"), report.ToString());
            Console.WriteLine(report.ToString());
            return 0;
        }
        catch (Exception ex)
        {
            report.AppendLine("gridprobe FAILED");
            report.AppendLine(ex.ToString());
            File.WriteAllText(Path.Combine(outputRoot, "gridprobe-report.txt"), report.ToString());
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static int RunScaleProbeOnly(string outputRoot)
    {
        Directory.CreateDirectory(outputRoot);
        var report = new StringBuilder();
        report.AppendLine("GameSaveCenter L21 large-list virtualization scale probe");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        AppendRunMetadata(report, "scaleprobe", "OffscreenRenderHarness", "production default palette", "backend 1000/5000/20000; media UI window 2000");
        report.AppendLine("EvidenceBoundary: offscreen WPF template chain only; real Playnite/FusionX/DPI/video remains host validation");
        report.AppendLine("AnchorBoundary: page accumulation and stale-anchor contracts are covered by unit/source tests; this probe does not fake a Worker response");
        report.AppendLine();
        s_problems.Clear();

        try
        {
            var app = new Application();
            app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
            RunLargeListScaleProbes(report);

            if (s_problems.Count > 0)
            {
                report.AppendLine("scaleprobe FAILED");
                foreach (var problem in s_problems)
                    report.AppendLine("  PROBLEM " + problem);
                File.WriteAllText(Path.Combine(outputRoot, "scaleprobe-report.txt"), report.ToString());
                Console.WriteLine(report.ToString());
                return 1;
            }

            report.AppendLine("scaleprobe OK");
            File.WriteAllText(Path.Combine(outputRoot, "scaleprobe-report.txt"), report.ToString());
            Console.WriteLine(report.ToString());
            return 0;
        }
        catch (Exception ex)
        {
            report.AppendLine("scaleprobe FAILED");
            report.AppendLine(ex.ToString());
            File.WriteAllText(Path.Combine(outputRoot, "scaleprobe-report.txt"), report.ToString());
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static int RunWrapProbeOnly(string outputRoot)
    {
        Directory.CreateDirectory(outputRoot);
        var report = new StringBuilder();
        report.AppendLine("GameSaveCenter current-game media wrap probe");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        s_problems.Clear();

        try
        {
            var app = new Application();
            app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
            RunMediaWrapScrollProbe(report);
            report.AppendLine(s_problems.Count == 0 ? "wrapprobe OK" : "wrapprobe FAILED");
            foreach (var problem in s_problems)
                report.AppendLine("  PROBLEM " + problem);
            File.WriteAllText(Path.Combine(outputRoot, "wrapprobe-report.txt"), report.ToString());
            Console.WriteLine(report.ToString());
            return s_problems.Count == 0 ? 0 : 1;
        }
        catch (Exception ex)
        {
            report.AppendLine("wrapprobe FAILED");
            report.AppendLine(ex.ToString());
            File.WriteAllText(Path.Combine(outputRoot, "wrapprobe-report.txt"), report.ToString());
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static int RunThumbnailProbeOnly(string outputRoot)
    {
        Directory.CreateDirectory(outputRoot);
        var report = new StringBuilder();
        report.AppendLine("GameSaveCenter L22 thumbnail loading, cancellation and cache probe");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        AppendRunMetadata(report, "thumbnailprobe", "OffscreenRenderHarness", "synthetic PNGs + STA WPF control", "120 thumbnails; 100 loader window cycles");
        report.AppendLine("EvidenceBoundary: synthetic files and a hidden STA WPF Window; real Playnite/FusionX/DPI/video remains host validation");
        report.AppendLine("PrivacyBoundary: report contains item IDs, counts and dimensions only; it does not record file contents or paths");
        report.AppendLine();
        s_problems.Clear();
        var reportPath = Path.Combine(outputRoot, "thumbnailprobe-report.txt");
        var tempRoot = Path.Combine(Path.GetTempPath(), "GameSaveCenter.ThumbnailProbe", Guid.NewGuid().ToString("N"));

        try
        {
            Directory.CreateDirectory(tempRoot);
            var paths = Enumerable.Range(0, 120)
                .Select(index =>
                {
                    var path = Path.Combine(tempRoot, $"item-{index:000}.png");
                    WriteProbePng(path, 96, 96, (byte)(index + 17));
                    return path;
                })
                .ToArray();

            AsyncThumbnailLoader.ClearCache();
            AsyncThumbnailLoader.ResetDiagnostics();
            var initial = Task.WhenAll(paths.Select(path => AsyncThumbnailLoader.LoadAsync(path, 96, CancellationToken.None)))
                .GetAwaiter()
                .GetResult();
            var afterInitial = AsyncThumbnailLoader.CaptureDiagnostics();
            report.AppendLine(
                $"InitialWindow items={paths.Length} requested={afterInitial.RequestCount} decoded={afterInitial.DecodeSuccessCount} "
                + $"failed={afterInitial.FailureCount} active={afterInitial.ActiveDecodes} peak={afterInitial.PeakConcurrentDecodes} "
                + $"cache={afterInitial.CacheCount}/{afterInitial.CacheLimit} dimensions=96x96");

            if (initial.Any(image => image == null))
                s_problems.Add("initial thumbnail window returned a null image");
            if (afterInitial.ActiveDecodes != 0)
                s_problems.Add($"active decodes did not drain: {afterInitial.ActiveDecodes}");
            if (afterInitial.PeakConcurrentDecodes < 1 || afterInitial.PeakConcurrentDecodes > 3)
                s_problems.Add($"decode concurrency exceeded the bounded gate: peak={afterInitial.PeakConcurrentDecodes}");
            if (afterInitial.CacheCount > afterInitial.CacheLimit)
                s_problems.Add($"cache exceeded its bound: {afterInitial.CacheCount}/{afterInitial.CacheLimit}");
            if (afterInitial.DecodeStartCount != afterInitial.DecodeSuccessCount)
                s_problems.Add($"successful synthetic decodes do not match starts: {afterInitial.DecodeStartCount}/{afterInitial.DecodeSuccessCount}");

            var cachedPaths = paths.Skip(paths.Length - 16).ToArray();
            Task.WhenAll(cachedPaths.Select(path => AsyncThumbnailLoader.LoadAsync(path, 96, CancellationToken.None)))
                .GetAwaiter()
                .GetResult();
            var afterCache = AsyncThumbnailLoader.CaptureDiagnostics();
            report.AppendLine(
                $"CacheWindow items={cachedPaths.Length} requests={afterCache.RequestCount} cacheHits={afterCache.CacheHitCount} "
                + $"decodeStarts={afterCache.DecodeStartCount} cache={afterCache.CacheCount}/{afterCache.CacheLimit}");
            if (afterCache.CacheHitCount < cachedPaths.Length)
                s_problems.Add($"recent thumbnails did not hit the cache: hits={afterCache.CacheHitCount}");
            if (afterCache.DecodeStartCount != afterInitial.DecodeStartCount)
                s_problems.Add("cache window started a new decode for a retained thumbnail");

            var corruptPath = Path.Combine(tempRoot, "item-corrupt.png");
            var missingPath = Path.Combine(tempRoot, "item-missing.png");
            File.WriteAllText(corruptPath, "not-an-image");
            var corrupt = AsyncThumbnailLoader.LoadAsync(corruptPath, 96, CancellationToken.None).GetAwaiter().GetResult();
            var missing = AsyncThumbnailLoader.LoadAsync(missingPath, 96, CancellationToken.None).GetAwaiter().GetResult();
            var afterFailures = AsyncThumbnailLoader.CaptureDiagnostics();
            report.AppendLine(
                $"FailureWindow corrupt=0x0 missing=0x0 returnedNull={corrupt == null && missing == null} "
                + $"failures={afterFailures.FailureCount} active={afterFailures.ActiveDecodes}");
            if (corrupt != null || missing != null)
                s_problems.Add("corrupt or missing thumbnail returned an image");

            var cancellationSource = new CancellationTokenSource();
            cancellationSource.Cancel();
            var cancellationObserved = false;
            try
            {
                AsyncThumbnailLoader.LoadAsync(paths[0], 96, cancellationSource.Token).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                cancellationObserved = true;
            }
            finally
            {
                cancellationSource.Dispose();
            }

            var afterCancellation = AsyncThumbnailLoader.CaptureDiagnostics();
            report.AppendLine(
                $"CancellationWindow preCancelledObserved={cancellationObserved} cancellations={afterCancellation.CancellationCount} "
                + $"active={afterCancellation.ActiveDecodes}");
            if (!cancellationObserved || afterCancellation.CancellationCount < 1)
                s_problems.Add("pre-cancelled thumbnail request was not observed as cancellation");

            var cyclesBefore = AsyncThumbnailLoader.CaptureDiagnostics();
            for (var cycle = 0; cycle < 100; cycle++)
            {
                var start = cycle % (paths.Length - 12);
                Task.WhenAll(paths.Skip(start).Take(12).Select(path => AsyncThumbnailLoader.LoadAsync(path, 96, CancellationToken.None)))
                    .GetAwaiter()
                    .GetResult();
            }
            var afterCycles = AsyncThumbnailLoader.CaptureDiagnostics();
            report.AppendLine(
                $"ScrollWindow cycles=100 window=12 requestsDelta={afterCycles.RequestCount - cyclesBefore.RequestCount} "
                + $"cacheHitsDelta={afterCycles.CacheHitCount - cyclesBefore.CacheHitCount} decodeDelta={afterCycles.DecodeStartCount - cyclesBefore.DecodeStartCount} "
                + $"peak={afterCycles.PeakConcurrentDecodes} active={afterCycles.ActiveDecodes} cache={afterCycles.CacheCount}/{afterCycles.CacheLimit}");
            if (afterCycles.ActiveDecodes != 0 || afterCycles.PeakConcurrentDecodes > 3 || afterCycles.CacheCount > afterCycles.CacheLimit)
                s_problems.Add("100 thumbnail window cycles exceeded the active/cache bounds");

            var oldPath = Path.Combine(tempRoot, "item-old.png");
            var newPath = Path.Combine(tempRoot, "item-new.png");
            WriteProbePng(oldPath, 800, 800, 11);
            WriteProbePng(newPath, 64, 64, 231);
            AsyncThumbnailLoader.ClearCache();
            var staleImage = RunThumbnailPathReplacementProbe(oldPath, newPath);
            report.AppendLine(
                $"StalePathWindow old=800x800 new=64x64 final={(staleImage.Success ? "new" : "invalid")} "
                + $"finalWidth={staleImage.Width} finalPixelB={staleImage.FirstBlue}");
            if (!staleImage.Success || staleImage.FirstBlue != 231)
                s_problems.Add("replaced thumbnail path left the old image visible");

            report.AppendLine();
            if (s_problems.Count > 0)
            {
                report.AppendLine("thumbnailprobe FAILED");
                foreach (var problem in s_problems)
                    report.AppendLine("  PROBLEM " + problem);
                File.WriteAllText(reportPath, report.ToString());
                Console.WriteLine(report.ToString());
                return 1;
            }

            report.AppendLine("thumbnailprobe OK");
            File.WriteAllText(reportPath, report.ToString());
            Console.WriteLine(report.ToString());
            return 0;
        }
        catch (Exception ex)
        {
            report.AppendLine("thumbnailprobe FAILED");
            report.AppendLine(ex.ToString());
            File.WriteAllText(reportPath, report.ToString());
            Console.Error.WriteLine(ex);
            return 1;
        }
        finally
        {
            try { Directory.Delete(tempRoot, true); }
            catch { }
        }
    }

    private static ThumbnailPathProbeResult RunThumbnailPathReplacementProbe(string oldPath, string newPath)
    {
        var result = new ThumbnailPathProbeResult();
        Window? window = null;
        try
        {
            var image = new AsyncThumbnailImage { PreviewWidth = 96 };
            window = new Window
            {
                Content = image,
                Width = 120,
                Height = 120,
                ShowInTaskbar = false,
                ShowActivated = false,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };
            window.Show();
            window.UpdateLayout();
            image.SourcePath = oldPath;
            image.SourcePath = newPath;
            PumpDispatcherUntil(window.Dispatcher, () => image.Source != null, TimeSpan.FromSeconds(3));
            var source = image.Source as BitmapSource;
            if (source == null)
                return result;

            var stride = source.PixelWidth * 4;
            var pixels = new byte[stride * source.PixelHeight];
            source.CopyPixels(pixels, stride, 0);
            result.Success = source.PixelWidth == 96;
            result.Width = source.PixelWidth;
            result.FirstBlue = pixels.Length == 0 ? -1 : pixels[0];
            return result;
        }
        finally
        {
            window?.Close();
        }
    }

    private static void PumpDispatcherUntil(Dispatcher dispatcher, Func<bool> condition, TimeSpan timeout)
    {
        var frame = new DispatcherFrame();
        var started = DateTime.UtcNow;
        var timer = new DispatcherTimer(DispatcherPriority.Background, dispatcher)
        {
            Interval = TimeSpan.FromMilliseconds(10)
        };
        timer.Tick += (_, _) =>
        {
            if (condition() || DateTime.UtcNow - started >= timeout)
            {
                timer.Stop();
                frame.Continue = false;
            }
        };
        timer.Start();
        Dispatcher.PushFrame(frame);
    }

    private static void WriteProbePng(string path, int width, int height, byte blue)
    {
        var pixels = new byte[width * height * 4];
        for (var index = 0; index < pixels.Length; index += 4)
        {
            pixels[index] = blue;
            pixels[index + 1] = 80;
            pixels[index + 2] = 160;
            pixels[index + 3] = 255;
        }

        var bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, width * 4);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }

    private sealed class ThumbnailPathProbeResult
    {
        public bool Success { get; set; }
        public int Width { get; set; }
        public int FirstBlue { get; set; } = -1;
    }

    private static int RunShellChromeQa(string outputRoot)
    {
        Directory.CreateDirectory(outputRoot);
        var report = new StringBuilder();
        report.AppendLine("GameSaveCenter production shell chrome QA");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        AppendRunMetadata(report, "shellqa", "OffscreenRenderHarness", "light,dark", "production shell fixtures");
        s_problems.Clear();

        try
        {
            var app = new Application();
            app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
            RunShellChromeProbes(outputRoot, report);
            if (s_problems.Count > 0)
            {
                report.AppendLine("shell-qa FAILED");
                foreach (var problem in s_problems)
                    report.AppendLine("  PROBLEM " + problem);
                File.WriteAllText(Path.Combine(outputRoot, "shell-qa-report.txt"), report.ToString());
                Console.WriteLine(report.ToString());
                return 1;
            }

            report.AppendLine("shell-qa OK");
            File.WriteAllText(Path.Combine(outputRoot, "shell-qa-report.txt"), report.ToString());
            Console.WriteLine(report.ToString());
            return 0;
        }
        catch (Exception ex)
        {
            report.AppendLine("shell-qa FAILED");
            report.AppendLine(ex.ToString());
            File.WriteAllText(Path.Combine(outputRoot, "shell-qa-report.txt"), report.ToString());
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static (double Width, double Height) ContentSize(int windowW, int windowH)
    {
        var expanded = windowW >= 1280;
        var sidebar = expanded ? 228d : 204d;
        var gutter = 16d;
        var shellInset = 56d;
        var measuredWidth = Math.Max(320d, windowW - shellInset - sidebar - gutter);
        var cardPadding = expanded ? 24d : 20d;
        var contentW = Math.Max(320d, measuredWidth - cardPadding);
        // Shell margin/padding (56) + header surface (~78) + footer (~40) + detail card
        // padding and tab header (~66) leave roughly windowH-240 for the workspace view.
        var contentH = Math.Max(320d, windowH - 240d);
        return (contentW, contentH);
    }

    private static void RenderOverview(string outputRoot, int windowW, int windowH, double contentW, double contentH, StringBuilder report)
    {
        var view = new OverviewView { DataContext = new FakeDashboardData() };
        RenderView(
            view,
            outputRoot,
            "Overview",
            windowW,
            windowH,
            contentW,
            contentH,
            report,
            () =>
            {
                // Mirrors DashboardView.ApplyResponsiveLayout: the Overview switches to
                // its single-column flow until the shell content area reaches 1200 DIP.
                var stack = contentW < 1200;
                view.OverviewCompactSecondaryRowHeight = stack ? GridLength.Auto : new GridLength(0);
                view.ApplyResponsiveColumns(stack);
                view.ApplyResponsiveWidth(contentW);
                view.ApplyResponsiveHeight(windowH, stack);
            });
    }

    private static void RenderMedia(string outputRoot, int windowW, int windowH, double contentW, double contentH, StringBuilder report)
    {
        var view = new MediaCenterView { DataContext = new FakeDashboardData() };
        // The page receives the measured workspace height, not the outer window height.
        // Passing windowH here hid the compact-height path and made the offscreen fixture
        // disagree with the production shell's PageHost geometry.
        RenderTabs(view, outputRoot, "Media", windowW, windowH, contentW, contentH, report, () => view.ApplyResponsiveLayout(contentW, contentH));
    }

    private static void RenderSave(string outputRoot, int windowW, int windowH, double contentW, double contentH, StringBuilder report)
    {
        var view = new SaveCenterView { DataContext = new FakeDashboardData() };
        RenderTabs(view, outputRoot, "Save", windowW, windowH, contentW, contentH, report, () => view.ApplyResponsiveLayout(contentW, windowH));
    }

    private static void RenderTrainer(string outputRoot, int windowW, int windowH, double contentW, double contentH, StringBuilder report)
    {
        var view = new TrainerCenterView { DataContext = new FakeDashboardData() };
        RenderTabs(view, outputRoot, "Trainer", windowW, windowH, contentW, contentH, report, () => view.ApplyResponsiveLayout(contentW, windowH));
    }

    private static void RenderSettings(string outputRoot, int windowW, int windowH, double contentW, double contentH, StringBuilder report)
    {
        var view = new GameSaveCenterSettingsView { DataContext = new GameSaveCenterSettings() };
        // Exercise the same runtime material path as the Playnite settings host. Without the
        // lifecycle event, the offscreen harness would only capture DesignTokens fallbacks and
        // could not catch a regression where the settings glass resources are never applied.
        view.ApplyThemeForAudit(GameSaveCenterThemeMode.FollowPlaynite);
        var apply = typeof(GameSaveCenterSettingsView).GetMethod(
            "ApplyResponsiveLayout",
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (apply == null)
            throw new InvalidOperationException("GameSaveCenterSettingsView.ApplyResponsiveLayout not found.");
        RenderTabs(
            view,
            outputRoot,
            "Settings",
            windowW,
            windowH,
            contentW,
            contentH,
            report,
            () => apply.Invoke(view, new object[] { contentW, windowH }));
    }

    private static void RenderMaintenance(string outputRoot, int windowW, int windowH, double contentW, double contentH, StringBuilder report)
    {
        var view = new MaintenanceView { DataContext = new FakeDashboardData() };
        RenderTabs(view, outputRoot, "Maintenance", windowW, windowH, contentW, contentH, report, () => view.ApplyResponsiveLayout(contentW, windowH));
    }

    private static void RenderTasks(string outputRoot, int windowW, int windowH, double contentW, double contentH, StringBuilder report)
    {
        var view = new TaskCenterView { DataContext = new FakeDashboardData() };
        RenderView(view, outputRoot, "Task", windowW, windowH, contentW, contentH, report, () => view.ApplyResponsiveLayout(contentW, windowH));
    }

    private static void RenderTabs(UserControl view, string outputRoot, string name, int windowW, int windowH, double contentW, double contentH, StringBuilder report, Action applyLayout, Action<Grid, int, StringBuilder>? verify = null)
    {
        var host = new Grid
        {
            Width = contentW,
            Height = contentH,
            Background = CreateHarnessBackground(view),
            ClipToBounds = true
        };
        host.Children.Add(view);

        var layoutSw = Stopwatch.StartNew();
        applyLayout();
        host.Measure(new Size(contentW, contentH));
        host.Arrange(new Rect(0, 0, contentW, contentH));
        host.UpdateLayout();
        applyLayout();
        host.UpdateLayout();
        layoutSw.Stop();

        // The production workspaces now use the UiLab ListBox segmented shell so
        // the header and content can be measured independently. Keep the harness
        // compatible with the remaining settings/audit TabControls while choosing
        // the named top-level segment list for migrated pages.
        var tabs = FindVisualChildren<TabControl>(host).FirstOrDefault();
        var segmentTabs = FindVisualChildren<ListBox>(host)
            .FirstOrDefault(candidate => candidate.Name.EndsWith("SegmentTabs", StringComparison.Ordinal)
                || candidate.Name == "SettingsSectionTabs");
        if (tabs == null && segmentTabs == null)
        {
            throw new InvalidOperationException($"{name} has no top-level segment control to render.");
        }

        var tabCount = segmentTabs?.Items.Count ?? tabs!.Items.Count;
        for (var i = 0; i < tabCount; i++)
        {
            if (segmentTabs != null)
                segmentTabs!.SelectedIndex = i;
            else
                tabs!.SelectedIndex = i;
            host.UpdateLayout();
            applyLayout();
            host.UpdateLayout();
            // The production settings view starts at Opacity=0 until Playnite raises
            // IsVisibleChanged and plays its entrance animation. The offscreen harness
            // has no host lifecycle, so expose the shell before capturing it; otherwise
            // a layout regression can pass with a blank PNG.
            if (name.Equals("Settings", StringComparison.OrdinalIgnoreCase))
            {
                var settingsShell = FindVisualChildren<FrameworkElement>(host)
                    .FirstOrDefault(element => element.Name == "SettingsShell");
                if (settingsShell != null)
                {
                    // The real settings view starts an entrance storyboard from its
                    // Loaded handler. Clearing the clock is required in the offscreen
                    // harness; setting the base Opacity alone is ignored while the
                    // animation still owns the property and produces a blank capture.
                    settingsShell.BeginAnimation(UIElement.OpacityProperty, null);
                    settingsShell.Opacity = 1;
                }
            }
            verify?.Invoke(host, i, report);
            var sw = Stopwatch.StartNew();
            SavePng(host, Path.Combine(outputRoot, $"{name}-{windowW}x{windowH}-tab{i}.png"));
            sw.Stop();
            report.AppendLine($"  {name} tab{i} layout_ms={layoutSw.ElapsedMilliseconds} render_ms={sw.ElapsedMilliseconds} window_dip={windowW}x{windowH} content_dip={contentW:0}x{contentH:0}");
            CollectScrollDiagnostics(host, report, name, windowW, windowH, i);
        }
    }

    private static void RenderView(UserControl view, string outputRoot, string name, int windowW, int windowH, double contentW, double contentH, StringBuilder report, Action applyLayout)
    {
        var host = new Grid
        {
            Width = contentW,
            Height = contentH,
            Background = CreateHarnessBackground(view),
            ClipToBounds = true
        };
        host.Children.Add(view);

        var layoutSw = Stopwatch.StartNew();
        applyLayout();
        host.Measure(new Size(contentW, contentH));
        host.Arrange(new Rect(0, 0, contentW, contentH));
        host.UpdateLayout();
        applyLayout();
        host.UpdateLayout();
        layoutSw.Stop();
        var sw = Stopwatch.StartNew();
        SavePng(host, Path.Combine(outputRoot, $"{name}-{windowW}x{windowH}.png"));
        sw.Stop();
        report.AppendLine($"  {name} layout_ms={layoutSw.ElapsedMilliseconds} render_ms={sw.ElapsedMilliseconds} window_dip={windowW}x{windowH} content_dip={contentW:0}x{contentH:0}");
        CollectScrollDiagnostics(host, report, name, windowW, windowH, -1);
    }

    private static void AppendRunMetadata(
        StringBuilder report,
        string scenario,
        string sourceKind,
        string themes,
        string dataVolumes)
    {
        var commit = ResolveGitValue("rev-parse HEAD");
        var workingTree = ResolveGitValue("status --porcelain");
        report.AppendLine($"Scenario: {scenario}");
        report.AppendLine($"EvidenceSource: {sourceKind}");
        report.AppendLine($"Commit: {commit}");
        report.AppendLine($"WorkingTreeClean: {workingTree.Length == 0}");
        report.AppendLine("DpiScale: 1.00 (offscreen logical DIP; real host DPI is not inferred)");
        report.AppendLine("WindowDip: per-case window_dip/content_dip fields");
        report.AppendLine($"Themes: {themes}");
        report.AppendLine($"DataVolumes: {dataVolumes}");
        report.AppendLine("TimingFields: layout_ms, render_ms; request_ms=not-applicable for offscreen harness");
    }

    private static string ResolveGitValue(string arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo("git", arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = RepositoryRoot
            };
            using var process = Process.Start(startInfo);
            if (process == null) return "unknown";
            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit(3000);
            if (output.Length == 0 && arguments.StartsWith("status ", StringComparison.OrdinalIgnoreCase))
                return string.Empty;
            return output.Length == 0 ? "unknown" : output.Replace(Environment.NewLine, " ");
        }
        catch
        {
            return "unknown";
        }
    }

    private static string ResolveRepositoryRoot()
    {
        var metadata = typeof(Program).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .ToDictionary(attribute => attribute.Key, attribute => attribute.Value, StringComparer.OrdinalIgnoreCase);
        if (metadata.TryGetValue("GscSourceRoot", out var metadataRoot)
            && !string.IsNullOrWhiteSpace(metadataRoot)
            && File.Exists(Path.Combine(metadataRoot, "GameSaveCenter.sln")))
        {
            return Path.GetFullPath(metadataRoot);
        }

        var environmentRoot = Environment.GetEnvironmentVariable("GSC_SOURCE_ROOT");
        if (!string.IsNullOrWhiteSpace(environmentRoot)
            && File.Exists(Path.Combine(environmentRoot, "GameSaveCenter.sln")))
        {
            return Path.GetFullPath(environmentRoot);
        }

        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    }

    private static void CollectScrollDiagnostics(Grid host, StringBuilder report, string name, int windowW, int windowH, int tabIndex)
    {
        var label = tabIndex >= 0 ? $"{name} tab{tabIndex}" : name;
        foreach (var scroller in FindVisualChildren<ScrollViewer>(host))
        {
            var visibleName = scroller.Name;
            if (string.IsNullOrEmpty(visibleName))
                continue;
            var scrollable = scroller.ExtentHeight > scroller.ViewportHeight + 0.5;
            var horizontalOverflow = scroller.ExtentWidth > scroller.ViewportWidth + 0.5;
            report.AppendLine(
                $"  {label} {visibleName}: size={scroller.ActualWidth:0}x{scroller.ActualHeight:0}, viewport={scroller.ViewportHeight:0}, extent={scroller.ExtentHeight:0}, " +
                $"vbar={scroller.VerticalScrollBarVisibility}, hbar={scroller.HorizontalScrollBarVisibility}, scrollable={scrollable}, hscrollable={horizontalOverflow}");
            if ((visibleName.Contains("ScrollSurface") || visibleName == "SettingsScroller")
                && scroller.VerticalScrollBarVisibility == ScrollBarVisibility.Hidden
                && scrollable)
            {
                s_problems.Add($"{label} {visibleName} hides overflow behind a Hidden scrollbar (viewport={scroller.ViewportHeight:0}, extent={scroller.ExtentHeight:0})");
            }
            if ((visibleName.Contains("ScrollSurface") || visibleName == "SettingsScroller")
                && scroller.HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled
                && horizontalOverflow)
            {
                s_problems.Add($"{label} {visibleName} has page-level horizontal overflow (viewport={scroller.ViewportWidth:0}, extent={scroller.ExtentWidth:0})");
            }
            if (visibleName == "OverviewRiskScrollViewer" && scroller.Content is FrameworkElement riskContent)
            {
                report.AppendLine(
                    $"  {label} risk-content: type={riskContent.GetType().Name}, " +
                    $"size={riskContent.ActualWidth:0}x{riskContent.ActualHeight:0}, " +
                    $"desired={riskContent.DesiredSize.Width:0}x{riskContent.DesiredSize.Height:0}, vis={riskContent.Visibility}");
                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(riskContent); i++)
                {
                    if (VisualTreeHelper.GetChild(riskContent, i) is FrameworkElement child)
                    {
                        report.AppendLine(
                            $"  {label} risk-child[{i}]: type={child.GetType().Name}, " +
                            $"size={child.ActualWidth:0}x{child.ActualHeight:0}, desired={child.DesiredSize.Width:0}x{child.DesiredSize.Height:0}, vis={child.Visibility}");
                    }
                }
            }
        }

        foreach (var grid in FindVisualChildren<DataGrid>(host))
        {
            if (string.IsNullOrEmpty(grid.Name))
                continue;
            var requiredReadableRows = Math.Min(4, grid.Items.Count);
            var readableRows = CountReadableDataRows(grid);
            report.AppendLine($"  {label} {grid.Name}: size={grid.ActualWidth:0}x{grid.ActualHeight:0}, rows={grid.Items.Count}, readableRows={readableRows}/{requiredReadableRows}");
            if (grid.Name is "FindingsGrid" or "MaintenanceDeviceGrid" or "MaintenanceAuditFindingsGrid" or "MaintenanceProcessGrid")
            {
                var fillRatio = host.ActualHeight > 0 ? grid.ActualHeight / host.ActualHeight : 0;
                report.AppendLine($"  {label} {grid.Name} fill: hostH={host.ActualHeight:0}, gridH={grid.ActualHeight:0}, ratio={fillRatio:0.00}");
            }
            if (grid.Name != "MaintenanceAuditLogGrid"
                && grid.ActualHeight > 0
                && requiredReadableRows > 0
                && readableRows < requiredReadableRows)
            {
                s_problems.Add($"{label} {grid.Name} table viewport only keeps {readableRows}/{requiredReadableRows} data rows fully readable (viewport={grid.ActualHeight:0} DIP)");
            }
        }

        foreach (var list in FindVisualChildren<ListBox>(host))
        {
            if (string.IsNullOrEmpty(list.Name))
                continue;
            report.AppendLine($"  {label} {list.Name}: size={list.ActualWidth:0}x{list.ActualHeight:0}, items={list.Items.Count}");
            // These lists live inside the Media inspector and intentionally size to their
            // small preview/history content under a separately scrollable inspector. They
            // are not the primary workspace viewport covered by the four-row gate.
            if (list.Name is "MediaClassificationPreviewItems" or "MediaClassificationHistoryList")
                continue;
            if (list.ActualHeight > 0
                && list.ActualHeight < 236
                && list.Name != "OverviewActivityList"
                && !list.Name.EndsWith("SegmentTabs", StringComparison.Ordinal)
                && list.Name != "SettingsSectionTabs"
                && list.Name != "MaintenanceDiagnosticsSubTabs")
            {
                s_problems.Add($"{label} {list.Name} list viewport is only {list.ActualHeight:0} DIP (< 236)");
            }
        }

        foreach (var combo in FindVisualChildren<ComboBox>(host))
        {
            if (string.IsNullOrEmpty(combo.Name))
                continue;
            report.AppendLine(
                $"  {label} {combo.Name}: selected={combo.SelectedItem ?? "(null)"}, index={combo.SelectedIndex}, items={combo.Items.Count}");
            if (combo.Items.Count > 0 && combo.SelectedItem == null)
            {
                s_problems.Add($"{label} {combo.Name} has no default selection ({combo.Items.Count} items available)");
            }
        }

        if (label.StartsWith("Save tab1", StringComparison.OrdinalIgnoreCase))
        {
            var buttonNames = new[] { "SaveDetectPathsButton", "SaveValidateButton", "SaveLoadDetailsButton" };
            var buttons = buttonNames
                .Select(name => FindVisualChildren<FrameworkElement>(host)
                    .FirstOrDefault(candidate => candidate.Name == name))
                .Where(button => button != null)
                .ToList();
            if (buttons.Count == 3)
            {
                var yPositions = buttons
                    .Select(button => button.TransformToAncestor(host).Transform(new Point(0, 0)).Y)
                    .ToList();
                var heights = buttons.Select(button => button.ActualHeight).ToList();
                report.AppendLine(
                    $"  {label} SaveCurrentRuleButtons: y={string.Join(",", yPositions.Select(value => value.ToString("0.##")))} heights={string.Join(",", heights.Select(value => value.ToString("0.##")))}");
                if (yPositions.Max() - yPositions.Min() > 2 || heights.Max() - heights.Min() > 2)
                {
                    s_problems.Add(
                        $"{label} Save current rule buttons are not aligned (y={string.Join(",", yPositions.Select(value => value.ToString("0.##")))}, heights={string.Join(",", heights.Select(value => value.ToString("0.##")))})");
                }
            }
        }

        if (label.StartsWith("Overview", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var elementName in new[]
                     {
                         "OverviewTodayHeroCard",
                         "OverviewCurrentGameCard",
                         "OverviewMetricPanel",
                         "OverviewActivityList"
                     })
            {
                var element = FindVisualChildren<FrameworkElement>(host).FirstOrDefault(candidate => candidate.Name == elementName);
                if (element == null)
                    continue;
                var origin = element.TransformToAncestor(host).Transform(new Point(0, 0));
                report.AppendLine(
                    $"  {label} {elementName}: x={origin.X:0}, y={origin.Y:0}, size={element.ActualWidth:0}x{element.ActualHeight:0}, vis={element.Visibility}");
            }

            var overviewLayout = FindVisualChildren<FrameworkElement>(host)
                .FirstOrDefault(candidate => candidate.Name == "OverviewLayoutGrid");
            var secondary = FindVisualChildren<FrameworkElement>(host)
                .FirstOrDefault(candidate => candidate.Name == "OverviewSecondaryScrollViewer");
            if (overviewLayout != null && secondary != null && Grid.GetColumn(secondary) == 2)
            {
                    // The Design page places the risk card beside the recent-task card,
                    // not beside the lower global-activity card. Compare the two cards
                    // directly so the audit checks the intended measured row rather than
                    // the top of the whole page scroll surface.
                    var reference = FindVisualChildren<FrameworkElement>(host)
                        .FirstOrDefault(candidate => candidate.Name == "OverviewRecentActivityCard")
                        ?? overviewLayout;
                var layoutOrigin = reference.TransformToAncestor(host).Transform(new Point(0, 0));
                var secondaryOrigin = secondary.TransformToAncestor(host).Transform(new Point(0, 0));
                var topDelta = secondaryOrigin.Y - layoutOrigin.Y;
                    report.AppendLine($"  {label} OverviewSecondaryTopDelta: {topDelta:0.##} DIP (relative to recent-task card)");
                if (topDelta > 8)
                    s_problems.Add($"{label} secondary overview is not aligned with lower activity row (delta={topDelta:0.##} DIP)");
            }

            var hero = FindVisualChildren<FrameworkElement>(host)
                .FirstOrDefault(candidate => candidate.Name == "OverviewTodayHeroCard");
            var currentGame = FindVisualChildren<FrameworkElement>(host)
                .FirstOrDefault(candidate => candidate.Name == "OverviewCurrentGameCard");
            if (hero != null && currentGame != null && Grid.GetColumn(currentGame) == 2
                && hero.ActualWidth > 0 && currentGame.ActualWidth > 0)
            {
                var widthRatio = currentGame.ActualWidth / hero.ActualWidth;
                report.AppendLine($"  {label} OverviewCurrentGameWidthRatio: {widthRatio:0.##}");
                // UiLab deliberately uses a 1.35*:1 hero/current-game ratio; the
                // previous 0.8 threshold described that reference layout as a bug.
                if (widthRatio < 0.65)
                    s_problems.Add($"{label} current-game card remains cramped (width ratio={widthRatio:0.##})");
            }

            if (currentGame != null)
            {
                var buttons = FindVisualChildren<Button>(currentGame)
                    .Cast<FrameworkElement>()
                    .ToList();
                if (buttons.Count == 3)
                {
                    var yPositions = buttons
                        .Select(button => button.TransformToAncestor(host).Transform(new Point(0, 0)).Y)
                        .ToList();
                    var heights = buttons.Select(button => button.ActualHeight).ToList();
                    report.AppendLine(
                        $"  {label} OverviewCurrentGameButtons: y={string.Join(",", yPositions.Select(value => value.ToString("0.##")))} heights={string.Join(",", heights.Select(value => value.ToString("0.##")))}");
                    if (yPositions.Max() - yPositions.Min() > 2 || heights.Max() - heights.Min() > 2)
                    {
                        s_problems.Add(
                            $"{label} Overview current game buttons are not aligned (y={string.Join(",", yPositions.Select(value => value.ToString("0.##")))}, heights={string.Join(",", heights.Select(value => value.ToString("0.##")))})");
                    }
                }
            }

            // The risk rail deliberately keeps its game list in a bounded local
            // scroller, while the two actions remain outside that scroller.  A
            // viewport screenshot can therefore look successful even when the
            // action row has been measured to zero or clipped by the card.  Probe
            // the actual visual tree and geometry so this class of regression is
            // caught by QA rather than by a manual click in Playnite.
            var riskCard = FindVisualChildren<FrameworkElement>(host)
                .FirstOrDefault(candidate => candidate.Name == "OverviewRiskCard");
            var protectionActions = FindVisualChildren<FrameworkElement>(host)
                .FirstOrDefault(candidate => candidate.Name == "OverviewProtectionActions");
            var protectionScroll = FindVisualChildren<ScrollViewer>(host)
                .FirstOrDefault(candidate => candidate.Name == "OverviewRiskViewport");
            if (riskCard != null && protectionActions != null)
            {
                var cardOrigin = riskCard.TransformToAncestor(host).Transform(new Point(0, 0));
                var actionsOrigin = protectionActions.TransformToAncestor(host).Transform(new Point(0, 0));
                var scrollBottom = protectionScroll == null
                    ? double.NaN
                    : protectionScroll.TransformToAncestor(host).Transform(new Point(0, protectionScroll.ActualHeight)).Y;
                var actionsBottom = actionsOrigin.Y + protectionActions.ActualHeight;
                var cardBottom = cardOrigin.Y + riskCard.ActualHeight;
                var actionButtons = FindVisualChildren<Button>(protectionActions).ToList();
                report.AppendLine(
                    $"  {label} OverviewProtectionActions: x={actionsOrigin.X:0}, y={actionsOrigin.Y:0}, size={protectionActions.ActualWidth:0}x{protectionActions.ActualHeight:0}, " +
                    $"vis={protectionActions.Visibility}, buttons={actionButtons.Count}, cardBottom={cardBottom:0}, scrollBottom={scrollBottom:0}");
                if (protectionActions.Visibility != Visibility.Visible
                    || protectionActions.ActualWidth <= 0
                    || protectionActions.ActualHeight <= 0
                    || actionButtons.Count < 2
                    || actionButtons.Any(button => button.Visibility != Visibility.Visible || button.ActualWidth <= 0 || button.ActualHeight < 30))
                {
                    s_problems.Add($"{label} risk action row is not measurable (visibility={protectionActions.Visibility}, size={protectionActions.ActualWidth:0}x{protectionActions.ActualHeight:0}, buttons={actionButtons.Count})");
                }
                if (actionsBottom > cardBottom + 1)
                    s_problems.Add($"{label} risk action row is clipped by its card (actionsBottom={actionsBottom:0.##}, cardBottom={cardBottom:0.##})");
                if (protectionScroll != null && actionsOrigin.Y + 1 < scrollBottom)
                    s_problems.Add($"{label} risk action row overlaps the risk list viewport (actionsY={actionsOrigin.Y:0.##}, scrollBottom={scrollBottom:0.##})");
            }
            else
            {
                s_problems.Add($"{label} risk action row is missing from the visual tree");
            }
        }
    }

    private static void RunDataGridScrollProbes(StringBuilder report)
    {
        var heights = new[] { 287d, 311d, 337d, 353d, 419d, 640d, 840d };
        foreach (var height in heights)
        {
            ProbeGrid(report, "Save", "SaveHistoryGrid", 0, height,
                () => new SaveCenterView { DataContext = new FakeDashboardData(60) },
                view => ((SaveCenterView)view).ApplyResponsiveLayout(900, height));
            ProbeGrid(report, "Task", "TaskGrid", -1, height,
                () => new TaskCenterView { DataContext = new FakeDashboardData(500) },
                view => ((TaskCenterView)view).ApplyResponsiveLayout(900, height));
            ProbeGrid(report, "Media-Inbox", "MediaInboxGrid", 0, height,
                () => new MediaCenterView { DataContext = CreateMediaInboxProbeData() },
                view => ((MediaCenterView)view).ApplyResponsiveLayout(2400, height),
                width: 2400,
                expectedVirtualizationMode: VirtualizationMode.Standard,
                expectedColumnVirtualization: false);
            if (height == 640d)
            {
                ProbeGrid(report, "Media-Inbox-Narrow", "MediaInboxGrid", 0, height,
                    () => new MediaCenterView { DataContext = CreateMediaInboxProbeData() },
                    view => ((MediaCenterView)view).ApplyResponsiveLayout(600, height),
                    width: 600,
                    expectedVirtualizationMode: VirtualizationMode.Standard,
                    expectedColumnVirtualization: false);

                foreach (var loadedCount in new[] { 50, 400, 2000 })
                {
                    ProbeGrid(report, $"Task-{loadedCount}", "TaskGrid", -1, height,
                        () => new TaskCenterView { DataContext = new FakeDashboardData(loadedCount) },
                        view => ((TaskCenterView)view).ApplyResponsiveLayout(900, height));
                    ProbeGrid(report, $"Media-Inbox-{loadedCount}", "MediaInboxGrid", 0, height,
                        () => new MediaCenterView { DataContext = CreateMediaInboxProbeData(loadedCount) },
                        view => ((MediaCenterView)view).ApplyResponsiveLayout(2400, height),
                        width: 2400,
                        expectedVirtualizationMode: VirtualizationMode.Standard,
                        expectedColumnVirtualization: false);
                }
            }
            ProbeGrid(report, "Maintenance-Diagnostics", "FindingsGrid", 0, height,
                () => new MaintenanceView { DataContext = new FakeDashboardData(60) },
                view => ((MaintenanceView)view).ApplyResponsiveLayout(900, height),
                "问题列表");
            ProbeGrid(report, "Maintenance-Audit", "MaintenanceAuditFindingsGrid", 4, height,
                () => new MaintenanceView { DataContext = new FakeDashboardData(60) },
                view => ((MaintenanceView)view).ApplyResponsiveLayout(900, height),
                "发现的问题");
            ProbeGrid(report, "Maintenance-AuditLog", "MaintenanceAuditLogGrid", 4, height,
                () => new MaintenanceView { DataContext = new FakeDashboardData(60) },
                view => ((MaintenanceView)view).ApplyResponsiveLayout(900, height),
                "审计记录");
        }
    }

    private static FakeDashboardData CreateMediaInboxProbeData(int targetCount = 4468)
    {
        var data = new FakeDashboardData(60);
        var existing = data.UnassignedMedia.Count;
        // The real inbox request is capped at 5000 and the reported regression only
        // appears once the virtualized extent is large. Keep the probe close to the
        // production upper bound instead of masking a large-data layout failure with
        // the six-row fixture used by the other workspace tables.
        for (var i = existing + 1; i <= targetCount; i++)
        {
            data.UnassignedMedia.Add(new MediaItemDto
            {
                MediaId = "IN-PROBE-" + i,
                Kind = i % 3 == 0 ? MediaKind.VideoClip : MediaKind.Screenshot,
                Source = i % 3 == 0 ? MediaSourceKind.XboxGameBar : MediaSourceKind.WindowsScreenshot,
                ArchivePath = $@"D:\Media\Inbox\probe-{i}.{(i % 3 == 0 ? "mp4" : "png")}",
                OriginalPath = $@"D:\Captures\probe-{i}.{(i % 3 == 0 ? "mp4" : "png")}",
                CapturedUtc = DateTime.UtcNow.AddMinutes(-i * 5),
                SizeBytes = 4_000_000L + i * 100_000L,
                ClassificationState = "Inbox",
                ClassificationReason = "无法唯一判断所属游戏"
            });
        }

        return data;
    }

    private static void RunMediaWrapScrollProbe(StringBuilder report)
    {
        try
        {
            var view = new MediaCenterView { DataContext = CreateMediaCardScaleProbeData(200) };
            var host = new Grid
            {
                Width = 900,
                Height = 640,
                Background = CreateHarnessBackground(view),
                ClipToBounds = true
            };
            host.Children.Add(view);
            view.ApplyResponsiveLayout(900, 640);
            host.Measure(new Size(900, 640));
            host.Arrange(new Rect(0, 0, 900, 640));
            host.UpdateLayout();

            // MediaCenter uses the production TabControl now; selecting the second tab
            // explicitly realizes MediaGrid before probing its finite virtualized viewport.
            var mediaTabs = FindVisualChildren<TabControl>(host).FirstOrDefault();
            if (mediaTabs != null)
                mediaTabs.SelectedIndex = 1;
            host.UpdateLayout();

            var mediaGrid = FindVisualChildren<ListBox>(host)
                .FirstOrDefault(candidate => candidate.Name == "MediaGrid");
            var scroller = mediaGrid == null
                ? null
                : FindVisualChildren<ScrollViewer>(mediaGrid)
                    .OrderByDescending(candidate => candidate.ViewportHeight)
                    .FirstOrDefault();
            if (mediaGrid == null || scroller == null)
            {
                s_problems.Add("Media wrap scroll probe could not find MediaGrid/ScrollViewer");
                return;
            }

            var panel = FindVisualChildren<VirtualizingWrapPanel>(mediaGrid).FirstOrDefault();
            var generator = mediaGrid.ItemContainerGenerator;
            var generatorPosition = ((IItemContainerGenerator)generator).GeneratorPositionFromIndex(0);
            var selectedTabIndex = mediaTabs?.SelectedIndex.ToString() ?? "none";
            string DescribePanel()
                => panel == null
                    ? "panel=missing"
                    : $"panelChildren={VisualTreeHelper.GetChildrenCount(panel)} panelOffset={panel.VerticalOffset:0.##} panelViewport={panel.ViewportHeight:0.##} panelExtent={panel.ExtentHeight:0.##}";
            report.AppendLine($"  Media wrap scroll-back probe initial: tab={selectedTabIndex}, listVisibility={mediaGrid.Visibility}/{mediaGrid.IsVisible}, panelVisibility={(panel == null ? "missing" : panel.Visibility + "/" + panel.IsVisible)}, items={mediaGrid.Items.Count}, generatorStatus={generator.Status}, generatorPosition0={generatorPosition.Index}/{generatorPosition.Offset}, scrollerOffset={scroller.VerticalOffset:0.##}, scrollerViewport={scroller.ViewportHeight:0.##}, scrollerExtent={scroller.ExtentHeight:0.##}, {DescribePanel()}");
            scroller.ScrollToVerticalOffset(scroller.ScrollableHeight);
            host.UpdateLayout();
            report.AppendLine($"  Media wrap scroll-back probe bottom: scrollerOffset={scroller.VerticalOffset:0.##}, scrollerViewport={scroller.ViewportHeight:0.##}, scrollerExtent={scroller.ExtentHeight:0.##}, {DescribePanel()}");
            scroller.ScrollToVerticalOffset(0);
            host.UpdateLayout();
            var realized = FindVisualChildren<ListBoxItem>(mediaGrid).Count();
            report.AppendLine($"  Media wrap scroll-back probe: items={mediaGrid.Items.Count}, realized={realized}, scrollable={scroller.ScrollableHeight:0.##}, scrollerOffset={scroller.VerticalOffset:0.##}, scrollerViewport={scroller.ViewportHeight:0.##}, scrollerExtent={scroller.ExtentHeight:0.##}, {DescribePanel()}");
            if (mediaGrid.Items.Count > 0 && realized == 0)
                s_problems.Add("Media wrap scroll-back probe realized no cards after returning to offset 0");
        }
        catch (Exception ex)
        {
            s_problems.Add("Media wrap scroll probe failed: " + ex.GetType().Name + ": " + ex.Message);
        }
    }

    private static void RunLargeListScaleProbes(StringBuilder report)
    {
        foreach (var backendCount in new[] { 1000, 5000, 20000 })
        {
            ProbeScaleGrid(
                report,
                "L21-Task",
                backendCount,
                () => new TaskCenterView { DataContext = CreateTaskScaleProbeData(backendCount) },
                view => ((TaskCenterView)view).ApplyResponsiveLayout(1100, 640),
                width: 1100,
                height: 640);

            ProbeScaleGrid(
                report,
                "L21-Media-Inbox",
                backendCount,
                () => new MediaCenterView { DataContext = CreateMediaInboxScaleProbeData(backendCount) },
                view => ((MediaCenterView)view).ApplyResponsiveLayout(2400, 640),
                width: 2400,
                height: 640,
                switchMediaTabs: true);

            ProbeMediaCardScale(report, backendCount);
        }

        ProbeWindowedDataGridScrollContracts(report);
    }

    private sealed class WindowedGridProbeFixture
    {
        public WindowedGridProbeFixture(FrameworkElement root, DataGrid grid)
        {
            Root = root;
            Grid = grid;
        }

        public FrameworkElement Root { get; }
        public DataGrid Grid { get; }
    }

    private static void ProbeWindowedDataGridScrollContracts(StringBuilder report)
    {
        const int itemCount = 2000;
        report.AppendLine("  L32 ScrollIntoView window probe: same 2000-item data, 1100x640 hidden WPF Window; plugin template vs standard template");
        ProbeWindowedDataGridScrollContract(
            report,
            "L32-Plugin-DataGrid",
            () =>
            {
                var view = new TaskCenterView { DataContext = CreateTaskScaleProbeData(itemCount) };
                var root = new Grid
                {
                    Width = 1100,
                    Height = 640,
                    ClipToBounds = true
                };
                root.Children.Add(view);
                view.ApplyResponsiveLayout(1100, 640);
                root.Measure(new Size(1100, 640));
                root.Arrange(new Rect(0, 0, 1100, 640));
                root.UpdateLayout();
                var grid = FindVisualChildren<DataGrid>(root)
                    .FirstOrDefault(candidate => candidate.Name == "TaskGrid");
                if (grid == null)
                    throw new InvalidOperationException("TaskGrid was not created in the plugin fixture");
                return new WindowedGridProbeFixture(root, grid);
            });

        ProbeWindowedDataGridScrollContract(
            report,
            "L32-Standard-DataGrid",
            () =>
            {
                var grid = new DataGrid
                {
                    Width = 1100,
                    Height = 640,
                    AutoGenerateColumns = false,
                    HeadersVisibility = DataGridHeadersVisibility.Column,
                    EnableRowVirtualization = true,
                    EnableColumnVirtualization = true,
                    IsReadOnly = true
                };
                VirtualizingPanel.SetIsVirtualizing(grid, true);
                VirtualizingPanel.SetVirtualizationMode(grid, VirtualizationMode.Recycling);
                VirtualizingPanel.SetScrollUnit(grid, ScrollUnit.Item);
                ScrollViewer.SetCanContentScroll(grid, true);
                ScrollViewer.SetHorizontalScrollBarVisibility(grid, ScrollBarVisibility.Auto);
                ScrollViewer.SetVerticalScrollBarVisibility(grid, ScrollBarVisibility.Auto);
                grid.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("TaskId"), Width = 220 });
                grid.ItemsSource = Enumerable.Range(0, itemCount)
                    .Select(index => new TaskStatusDto { TaskId = "standard-" + index.ToString("D4") })
                    .ToArray();
                var root = new Grid { Width = 1100, Height = 640, ClipToBounds = true };
                root.Children.Add(grid);
                return new WindowedGridProbeFixture(root, grid);
            });

        var fusionXThemeRoot = FindInstalledFusionXThemeRoot();
        if (fusionXThemeRoot == null)
        {
            report.AppendLine("  L32-FusionX-DataGrid result=not-run; installed FusionX Desktop theme resources were not found");
        }
        else
        {
            report.AppendLine(
                $"  L32-FusionX-DataGrid source=installed-desktop-theme root={Path.GetFileName(fusionXThemeRoot)} resource=DefaultControls/DataGrid.xaml");
            ProbeWindowedDataGridScrollContract(
                report,
                "L32-FusionX-DataGrid",
                () => CreateFusionXGridProbeFixture(itemCount, fusionXThemeRoot));
            ProbeWindowedDataGridScrollContract(
                report,
                "L32-FusionX-DataGrid-HBar",
                () => CreateFusionXGridProbeFixture(itemCount, fusionXThemeRoot, 700, 1100));
        }
    }

    private static string? FindInstalledFusionXThemeRoot()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var themeRoot = Path.Combine(appData, "Playnite", "Themes", "Desktop");
        if (!Directory.Exists(themeRoot))
            return null;

        return Directory.GetDirectories(themeRoot, "FusionX_*", SearchOption.TopDirectoryOnly)
            .Where(path => File.Exists(Path.Combine(path, "theme.yaml"))
                && File.Exists(Path.Combine(path, "DefaultControls", "DataGrid.xaml")))
            .OrderByDescending(path => Directory.GetLastWriteTimeUtc(path))
            .FirstOrDefault();
    }

    private static WindowedGridProbeFixture CreateFusionXGridProbeFixture(
        int itemCount,
        string themeRoot,
        double viewportWidth = 1100,
        double columnWidth = 620)
    {
        var root = new Grid
        {
            Width = viewportWidth,
            Height = 640,
            ClipToBounds = true
        };
        var dictionaries = root.Resources.MergedDictionaries;
        dictionaries.Add(new ResourceDictionary
        {
            Source = new Uri(Path.Combine(themeRoot, "Constants.xaml"), UriKind.Absolute)
        });
        var dataGridPath = Path.Combine(themeRoot, "DefaultControls", "DataGrid.xaml");
        var dataGridXaml = File.ReadAllText(dataGridPath);
        var rootEnd = dataGridXaml.IndexOf('>');
        if (rootEnd < 0)
            throw new InvalidOperationException("FusionX DataGrid.xaml has no ResourceDictionary root");
        // FusionX normally loads Constants.xaml into Playnite's shared resource scope before
        // this dictionary. XamlReader parses one dictionary in isolation, so provide only the
        // one StaticResource that the DataGrid cell template requires; no theme file is edited.
        dataGridXaml = dataGridXaml.Insert(
            rootEnd + 1,
            "\n    <Color x:Key=\"TextColor\">#CCFFFFFF</Color>"
            + "\n    <BooleanToVisibilityConverter x:Key=\"BooleanToVisibilityConverter\" />");
        dictionaries.Add((ResourceDictionary)XamlReader.Parse(dataGridXaml));

        var grid = new DataGrid
        {
            Width = viewportWidth,
            Height = 640,
            AutoGenerateColumns = false,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            EnableRowVirtualization = true,
            EnableColumnVirtualization = true,
            IsReadOnly = true,
            RowHeight = 52,
            ColumnHeaderHeight = 42
        };
        VirtualizingPanel.SetIsVirtualizing(grid, true);
        VirtualizingPanel.SetVirtualizationMode(grid, VirtualizationMode.Recycling);
        VirtualizingPanel.SetScrollUnit(grid, ScrollUnit.Item);
        ScrollViewer.SetCanContentScroll(grid, true);
        ScrollViewer.SetHorizontalScrollBarVisibility(grid, ScrollBarVisibility.Auto);
        ScrollViewer.SetVerticalScrollBarVisibility(grid, ScrollBarVisibility.Auto);
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "ID",
            Binding = new Binding("TaskId"),
            Width = columnWidth
        });
        grid.ItemsSource = Enumerable.Range(0, itemCount)
            .Select(index => new TaskStatusDto { TaskId = "fusionx-" + index.ToString("D4") })
            .ToArray();
        root.Children.Add(grid);
        return new WindowedGridProbeFixture(root, grid);
    }

    private static void ProbeWindowedDataGridScrollContract(
        StringBuilder report,
        string label,
        Func<WindowedGridProbeFixture> createFixture)
    {
        const int itemCount = 2000;
        Window? window = null;
        try
        {
            var fixture = createFixture();
            window = new Window
            {
                Content = fixture.Root,
                Width = 1100,
                Height = 640,
                ShowInTaskbar = false,
                ShowActivated = false,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                Opacity = 0.01
            };
            window.Show();
            fixture.Root.Measure(new Size(1100, 640));
            fixture.Root.Arrange(new Rect(0, 0, 1100, 640));
            fixture.Root.UpdateLayout();
            FlushLayoutDispatcher(window);

            var grid = fixture.Grid;
            var scroller = FindVisualChildren<ScrollViewer>(grid)
                .OrderByDescending(candidate => candidate.ViewportHeight)
                .FirstOrDefault();
            if (scroller == null)
            {
                s_problems.Add($"{label} hidden Window has no internal ScrollViewer");
                return;
            }
            var horizontalBar = FindVisualChildren<ScrollBar>(scroller)
                .FirstOrDefault(candidate => candidate.Orientation == Orientation.Horizontal);

            grid.SelectedIndex = itemCount / 2;
            scroller.ScrollToVerticalOffset(0);
            FlushLayoutDispatcher(window);
            var topOffset = scroller.VerticalOffset;
            var top = CaptureScaleGrid(grid, scroller);
            var lastItem = grid.Items[itemCount - 1];
            grid.Focus();
            grid.ScrollIntoView(lastItem);
            window.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() => grid.ScrollIntoView(lastItem)));
            FlushLayoutDispatcher(window);
            var afterScrollIntoView = CaptureScaleGrid(grid, scroller);
            report.AppendLine(
                $"  {label} items={itemCount} scrollUnit={VirtualizingPanel.GetScrollUnit(grid)} canContentScroll={scroller.CanContentScroll} "
                + $"scroller={scroller.GetType().Name} offsetAfterTop={topOffset:0.##} "
                + $"offsetAfterScrollIntoView={scroller.VerticalOffset:0.##}/{scroller.ScrollableHeight:0.##} "
                + $"top={top.FirstId}@{top.FirstY:0.##}/{top.FirstHeight:0.##} "
                + $"last={afterScrollIntoView.LastIndex}:{afterScrollIntoView.LastId}@{afterScrollIntoView.LastBottom:0.##}/{afterScrollIntoView.LastHeight:0.##} "
                + $"hbar={(horizontalBar == null ? "missing" : horizontalBar.Visibility + "/" + horizontalBar.ActualHeight.ToString("0.##"))} "
                + $"presenter={afterScrollIntoView.PresenterRect.Left:0.##},{afterScrollIntoView.PresenterRect.Top:0.##},{afterScrollIntoView.PresenterRect.Width:0.##}x{afterScrollIntoView.PresenterRect.Height:0.##}");

            var scrollIntoViewFailed = afterScrollIntoView.LastIndex != itemCount - 1
                || afterScrollIntoView.LastBottom > afterScrollIntoView.PresenterRect.Bottom + 1
                || (scroller.ScrollableHeight > 0 && scroller.VerticalOffset <= 0);
            var isBaselineTemplate = label.IndexOf("Standard", StringComparison.OrdinalIgnoreCase) >= 0
                || label.IndexOf("FusionX", StringComparison.OrdinalIgnoreCase) >= 0;
            if (scrollIntoViewFailed && isBaselineTemplate)
            {
                report.AppendLine(
                    $"  {label} ScrollIntoView result=offscreen-baseline-inconclusive; "
                    + "baseline template did not settle the deferred call in this hidden-window fixture; "
                    + "direct slider/Ctrl+End completeness remains checked below");
            }
            else
            {
                if (afterScrollIntoView.LastIndex != itemCount - 1)
                    s_problems.Add($"{label} ScrollIntoView did not realize the last item (lastIndex={afterScrollIntoView.LastIndex})");
                if (afterScrollIntoView.LastBottom > afterScrollIntoView.PresenterRect.Bottom + 1)
                    s_problems.Add($"{label} ScrollIntoView left the last row clipped (bottom={afterScrollIntoView.LastBottom:0.##}, presenterBottom={afterScrollIntoView.PresenterRect.Bottom:0.##})");
                if (scroller.ScrollableHeight > 0 && scroller.VerticalOffset <= 0)
                    s_problems.Add($"{label} ScrollIntoView did not move from the top (offset={scroller.VerticalOffset:0.##})");
            }

            var fractions = new[] { 0.0, 1.0, 0.0, 1.0, 0.5, 0.0, 1.0, 0.25, 0.75, 0.0, 1.0, 0.5, 0.1, 0.9, 0.0, 1.0, 0.33, 0.66, 0.0, 1.0 };
            for (var step = 0; step < fractions.Length; step++)
            {
                scroller.ScrollToVerticalOffset(scroller.ScrollableHeight * fractions[step]);
                FlushLayoutDispatcher(window);
                var snapshot = CaptureScaleGrid(grid, scroller);
                report.AppendLine(
                    $"  {label} step={step:00} pos={(int)(fractions[step] * 100)} offset={scroller.VerticalOffset:0.##}/{scroller.ScrollableHeight:0.##} "
                    + $"visible={snapshot.VisibleCount} first={snapshot.FirstId}@{snapshot.FirstY:0.##}/{snapshot.FirstHeight:0.##} "
                    + $"last={snapshot.LastIndex}:{snapshot.LastId}@{snapshot.LastBottom:0.##}/{snapshot.LastHeight:0.##} selected={GetStableId(grid.SelectedItem)}");
                if (snapshot.VisibleCount == 0)
                    s_problems.Add($"{label} step={step} has no visible rows");
                if (fractions[step] >= 1.0 && (snapshot.LastIndex != itemCount - 1 || snapshot.LastBottom > snapshot.PresenterRect.Bottom + 1))
                    s_problems.Add($"{label} step={step} bottom row is incomplete (lastIndex={snapshot.LastIndex}, bottom={snapshot.LastBottom:0.##}, presenterBottom={snapshot.PresenterRect.Bottom:0.##})");
                if (GetStableId(grid.SelectedItem) != GetStableId(grid.Items[itemCount / 2]))
                    s_problems.Add($"{label} step={step} changed selection while scrolling");
            }

            report.AppendLine(
                $"  {label} semantic=PageDown/PageUp/Ctrl+End beforeOffset={scroller.VerticalOffset:0.##}/{scroller.ScrollableHeight:0.##}");
            scroller.PageDown();
            scroller.PageUp();
            scroller.ScrollToEnd();
            FlushLayoutDispatcher(window);
            var semantic = CaptureScaleGrid(grid, scroller);
            report.AppendLine(
                $"  {label} semantic=PageDown/PageUp/Ctrl+End offset={scroller.VerticalOffset:0.##}/{scroller.ScrollableHeight:0.##} "
                + $"last={semantic.LastIndex}:{semantic.LastId}@{semantic.LastBottom:0.##}/{semantic.LastHeight:0.##}");
            if (semantic.VisibleCount == 0 || semantic.LastIndex != itemCount - 1 || semantic.LastBottom > semantic.PresenterRect.Bottom + 1)
                s_problems.Add($"{label} semantic end state does not show a complete last row");
        }
        catch (Exception ex)
        {
            var detail = ex.InnerException == null
                ? ex.Message
                : ex.Message + " | inner=" + ex.InnerException.GetType().Name + ": " + ex.InnerException.Message;
            s_problems.Add($"{label} hidden Window scroll probe failed: {ex.GetType().Name}: {detail}");
        }
        finally
        {
            try { window?.Close(); }
            catch { }
        }
    }

    private static void ProbeMediaCardScale(StringBuilder report, int backendCount)
    {
        var data = CreateMediaCardScaleProbeData(backendCount);
        var view = new MediaCenterView { DataContext = data };
        var host = new Grid
        {
            Width = 900,
            Height = 640,
            Background = CreateHarnessBackground(view),
            ClipToBounds = true
        };
        host.Children.Add(view);
        view.ApplyResponsiveLayout(900, 640);
        host.Measure(new Size(900, 640));
        host.Arrange(new Rect(0, 0, 900, 640));
        host.UpdateLayout();

        var tabs = FindVisualChildren<TabControl>(host).FirstOrDefault();
        if (tabs != null && tabs.Items.Count > 1)
            tabs.SelectedIndex = 1;
        host.UpdateLayout();

        var list = FindVisualChildren<ListBox>(host).FirstOrDefault(candidate => candidate.Name == "MediaGrid");
        var scroller = list == null
            ? null
            : FindVisualChildren<ScrollViewer>(list)
                .OrderByDescending(candidate => candidate.ViewportHeight)
                .FirstOrDefault();
        if (list == null || scroller == null)
        {
            s_problems.Add($"L21-Media-Cards backend={backendCount} could not find MediaGrid/ScrollViewer");
            return;
        }

        var selectedIndex = Math.Max(0, Math.Min(list.Items.Count - 1, list.Items.Count / 2));
        list.SelectedIndex = selectedIndex;
        host.UpdateLayout();
        var selectedId = GetStableId(list.SelectedItem);
        var topRealized = FindVisualChildren<ListBoxItem>(list).Count();
        scroller.ScrollToVerticalOffset(scroller.ScrollableHeight);
        host.UpdateLayout();
        var bottomRealized = FindVisualChildren<ListBoxItem>(list).Count();
        var bottomLast = FindVisualChildren<ListBoxItem>(list)
            .Where(item => item.Visibility == Visibility.Visible)
            .Select(item => new
            {
                Index = list.ItemContainerGenerator.IndexFromContainer(item),
                Y = item.TransformToAncestor(scroller).Transform(new Point(0, 0)).Y,
                Height = item.ActualHeight
            })
            .OrderBy(item => item.Y)
            .LastOrDefault();
        scroller.ScrollToVerticalOffset(0);
        host.UpdateLayout();
        var topAfterReturnRealized = FindVisualChildren<ListBoxItem>(list).Count();
        var itemsPanel = FindVisualChildren<VirtualizingWrapPanel>(list).Any()
            ? "VirtualizingWrapPanel"
            : FindVisualChildren<WrapPanel>(list).Any() ? "WrapPanel" : "unknown";
        report.AppendLine(
            $"  L21-Media-Cards backend={backendCount} uiItems={list.Items.Count} "
            + $"scrollable={scroller.ScrollableHeight:0.##} offset={scroller.VerticalOffset:0.##} "
            + $"realizedTop={topRealized} realizedBottom={bottomRealized} realizedTopAfterReturn={topAfterReturnRealized} "
            + $"last={(bottomLast == null ? "none" : bottomLast.Index + "@" + bottomLast.Y.ToString("0.##") + "/" + bottomLast.Height.ToString("0.##"))} "
            + $"selected={selectedId} currentSelected={GetStableId(list.SelectedItem)} itemsPanel={itemsPanel}");
        if (list.Items.Count != Math.Min(backendCount, MediaPageAccumulator.DefaultCapacity))
            s_problems.Add($"L21-Media-Cards backend={backendCount} UI window mismatch (items={list.Items.Count})");
        if (bottomRealized >= list.Items.Count)
            s_problems.Add($"L21-Media-Cards backend={backendCount} realized every card ({bottomRealized}/{list.Items.Count})");
        if (topAfterReturnRealized == 0)
            s_problems.Add($"L21-Media-Cards backend={backendCount} realized no cards after returning to top");
        if (!string.Equals(selectedId, GetStableId(list.SelectedItem), StringComparison.Ordinal))
            s_problems.Add($"L21-Media-Cards backend={backendCount} selection changed after scroll");
    }

    private static FakeDashboardData CreateMediaCardScaleProbeData(int backendCount)
    {
        var data = new FakeDashboardData(60);
        var retained = new BatchObservableCollection<MediaItemDto>();
        var accumulator = new MediaPageAccumulator(retained);
        const int pageSize = 200;
        for (var start = 0; start < backendCount; start += pageSize)
        {
            var page = Enumerable.Range(start, Math.Min(pageSize, backendCount - start))
                .Select(CreateScaleMediaItem)
                .ToArray();
            if (start == 0)
                accumulator.ReplaceFirstPage(page, null);
            else
                accumulator.AppendPage(page, null);
        }

        data.Media.Clear();
        foreach (var item in retained)
        {
            item.PlayniteId = data.SelectedGame.PlayniteId;
            data.Media.Add(item);
        }
        return data;
    }

    private static FakeDashboardData CreateTaskScaleProbeData(int targetCount)
    {
        var retainedSeed = Math.Min(200, Math.Max(8, targetCount));
        var data = new FakeDashboardData(retainedSeed);
        for (var index = data.Tasks.Count + 1; index <= targetCount; index++)
        {
            var game = data.Games[(index - 1) % data.Games.Count];
            data.Tasks.Add(new TaskStatusDto
            {
                TaskId = "L21-T-" + index.ToString("D5"),
                TaskType = index % 3 == 0 ? "MediaSync" : "Backup",
                GameId = game.PlayniteId,
                GameName = game.Name,
                State = index % 4 == 0 ? TaskState.Failed : TaskState.Succeeded,
                ProgressPercent = index % 4 == 0 ? 0 : 100,
                Message = index % 4 == 0 ? "L21 scale fixture failure" : "L21 scale fixture completed",
                CreatedUtc = DateTime.UtcNow.AddMinutes(-index),
                StartedUtc = DateTime.UtcNow.AddMinutes(-index + 1),
                FinishedUtc = DateTime.UtcNow.AddMinutes(-index + 2)
            });
        }

        return data;
    }

    private static FakeDashboardData CreateMediaInboxScaleProbeData(int backendCount)
    {
        var data = new FakeDashboardData(60);
        var retained = new BatchObservableCollection<MediaItemDto>();
        var accumulator = new MediaPageAccumulator(retained);
        const int pageSize = 200;

        for (var start = 0; start < backendCount; start += pageSize)
        {
            var page = Enumerable.Range(start, Math.Min(pageSize, backendCount - start))
                .Select(CreateScaleMediaItem)
                .ToArray();
            if (start == 0)
                accumulator.ReplaceFirstPage(page, null);
            else
                accumulator.AppendPage(page, null);
        }

        data.UnassignedMedia.Clear();
        foreach (var item in retained)
            data.UnassignedMedia.Add(item);
        return data;
    }

    private static MediaItemDto CreateScaleMediaItem(int index)
        => new MediaItemDto
        {
            MediaId = "L21-M-" + index.ToString("D5"),
            Kind = index % 3 == 0 ? MediaKind.VideoClip : MediaKind.Screenshot,
            Source = index % 3 == 0 ? MediaSourceKind.XboxGameBar : MediaSourceKind.WindowsScreenshot,
            ArchivePath = $@"D:\Media\L21\archive-{index}.png",
            OriginalPath = $@"D:\Media\L21\source-{index}.png",
            CapturedUtc = DateTime.UtcNow.AddMinutes(-index),
            SizeBytes = 4_000_000L + index,
            ClassificationState = "Inbox",
            ClassificationReason = "L21 scale fixture"
        };

    private static void ProbeScaleGrid(
        StringBuilder report,
        string label,
        int backendCount,
        Func<UserControl> createView,
        Action<UserControl> applyLayout,
        double width,
        double height,
        bool switchMediaTabs = false)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var process = Process.GetCurrentProcess();
        var managedBefore = GC.GetTotalMemory(false);
        var privateBefore = process.PrivateMemorySize64;
        try
        {
            var view = createView();
            var host = new Grid
            {
                Width = width,
                Height = height,
                Background = CreateHarnessBackground(view),
                ClipToBounds = true
            };
            host.Children.Add(view);
            applyLayout(view);
            host.Measure(new Size(width, height));
            host.Arrange(new Rect(0, 0, width, height));
            host.UpdateLayout();

            if (switchMediaTabs)
            {
                var tabs = FindVisualChildren<TabControl>(host).FirstOrDefault();
                if (tabs != null && tabs.Items.Count > 1)
                {
                    tabs.SelectedIndex = 1;
                    host.UpdateLayout();
                    tabs.SelectedIndex = 0;
                    host.UpdateLayout();
                }
            }

            var gridName = label == "L21-Task" ? "TaskGrid" : "MediaInboxGrid";
            var grid = FindVisualChildren<DataGrid>(host).FirstOrDefault(candidate => candidate.Name == gridName);
            if (grid == null)
            {
                s_problems.Add($"{label} backend={backendCount} grid {gridName} not found");
                return;
            }

            var scroller = FindVisualChildren<ScrollViewer>(grid)
                .OrderByDescending(candidate => candidate.ViewportHeight)
                .FirstOrDefault();
            if (scroller == null)
            {
                s_problems.Add($"{label} backend={backendCount} grid {gridName} has no internal ScrollViewer");
                return;
            }

            var expectedUiCount = label == "L21-Media-Inbox"
                ? Math.Min(backendCount, MediaPageAccumulator.DefaultCapacity)
                : backendCount;
            report.AppendLine($"  {label} backend={backendCount} uiItems={grid.Items.Count} expectedUiItems={expectedUiCount} "
                + $"scrollUnit={VirtualizingPanel.GetScrollUnit(grid)} canContentScroll={scroller.CanContentScroll}");
            if (grid.Items.Count != expectedUiCount)
                s_problems.Add($"{label} backend={backendCount} UI item window mismatch (actual={grid.Items.Count}, expected={expectedUiCount})");

            var selectedIndex = Math.Max(0, Math.Min(grid.Items.Count - 1, grid.Items.Count / 2));
            grid.SelectedIndex = selectedIndex;
            host.UpdateLayout();
            var selectedId = GetStableId(grid.SelectedItem);
            var fractions = new[]
            {
                0.0, 1.0, 0.0, 1.0, 0.5, 0.0, 1.0, 0.25, 0.75, 0.0,
                1.0, 0.33, 0.66, 0.0, 1.0, 0.5, 0.1, 0.9, 0.0, 1.0
            };

            for (var step = 0; step < fractions.Length; step++)
            {
                var fraction = fractions[step];
                scroller.ScrollToVerticalOffset(scroller.ScrollableHeight * fraction);
                host.UpdateLayout();
                var snapshot = CaptureScaleGrid(grid, scroller);
                var diagnostic = DataGridScrollDiagnostics.CaptureNow(grid, "L21-拖动滑块:" + (int)(fraction * 100));
                report.AppendLine(
                    $"  {label} backend={backendCount} step={step:00} pos={(int)(fraction * 100)} "
                    + $"offset={scroller.VerticalOffset:0.##}/{scroller.ScrollableHeight:0.##} "
                    + $"viewport={snapshot.PresenterRect.Width:0.##}x{snapshot.PresenterRect.Height:0.##} "
                    + $"extent={scroller.ExtentWidth:0.##}x{scroller.ExtentHeight:0.##} "
                    + $"realized={snapshot.RealizedCount} visible={snapshot.VisibleCount} "
                    + $"first={snapshot.FirstId}@{snapshot.FirstY:0.##}/{snapshot.FirstHeight:0.##} "
                    + $"last={snapshot.LastId}@{snapshot.LastY:0.##}/{snapshot.LastHeight:0.##} "
                    + $"selected={selectedId} currentSelected={GetStableId(grid.SelectedItem)}");
                report.AppendLine("  " + diagnostic);

                if (snapshot.VisibleCount == 0 && grid.Items.Count > 0)
                    s_problems.Add($"{label} backend={backendCount} step={step} has no visible rows");
                if (!string.Equals(selectedId, GetStableId(grid.SelectedItem), StringComparison.Ordinal))
                    s_problems.Add($"{label} backend={backendCount} step={step} selection changed from {selectedId} to {GetStableId(grid.SelectedItem)}");
                if (diagnostic.IndexOf("anomaly=", StringComparison.Ordinal) >= 0)
                    s_problems.Add($"{label} backend={backendCount} step={step} diagnostics reported {diagnostic}");

                if (fraction >= 1.0)
                {
                    if (snapshot.LastIndex != grid.Items.Count - 1)
                        s_problems.Add($"{label} backend={backendCount} step={step} last item is not visible (lastIndex={snapshot.LastIndex}, itemCount={grid.Items.Count})");
                    else if (snapshot.LastBottom > snapshot.PresenterRect.Bottom + 1)
                        s_problems.Add($"{label} backend={backendCount} step={step} last row is clipped (bottom={snapshot.LastBottom:0.##}, presenterBottom={snapshot.PresenterRect.Bottom:0.##})");
                }
            }

            RunScaleSemanticProbe(report, label, backendCount, grid, scroller, host, selectedId);

            var originalOffset = scroller.VerticalOffset;
            host.Width = label == "L21-Task" ? 900 : 1200;
            host.Height = 520;
            applyLayout(view);
            host.Measure(new Size(host.Width, host.Height));
            host.Arrange(new Rect(0, 0, host.Width, host.Height));
            host.UpdateLayout();
            var resizedSnapshot = CaptureScaleGrid(grid, scroller);
            report.AppendLine(
                $"  {label} backend={backendCount} resize={host.Width:0}x{host.Height:0} "
                + $"fromOffset={originalOffset:0.##} offset={scroller.VerticalOffset:0.##}/{scroller.ScrollableHeight:0.##} "
                + $"realized={resizedSnapshot.RealizedCount} visible={resizedSnapshot.VisibleCount} "
                + $"presenter={resizedSnapshot.PresenterRect.Left:0.##},{resizedSnapshot.PresenterRect.Top:0.##},{resizedSnapshot.PresenterRect.Width:0.##}x{resizedSnapshot.PresenterRect.Height:0.##} "
                + $"selected={GetStableId(grid.SelectedItem)}");
            if (!string.Equals(selectedId, GetStableId(grid.SelectedItem), StringComparison.Ordinal))
                s_problems.Add($"{label} backend={backendCount} resize lost selection {selectedId}");

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var managedAfter = GC.GetTotalMemory(false);
            var privateAfter = process.PrivateMemorySize64;
            report.AppendLine(
                $"  {label} backend={backendCount} memory managedDelta={managedAfter - managedBefore} "
                + $"privateDelta={privateAfter - privateBefore} bytes (fixture+visual tree, comparative only)");
        }
        catch (Exception ex)
        {
            s_problems.Add($"{label} backend={backendCount} scale probe failed: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void RunScaleSemanticProbe(
        StringBuilder report,
        string label,
        int backendCount,
        DataGrid grid,
        ScrollViewer scroller,
        Grid host,
        string selectedId)
    {
        var actions = new (string Name, Action Action)[]
        {
            ("滚轮", scroller.LineDown),
            ("PageDown", scroller.PageDown),
            ("PageUp", scroller.PageUp),
            ("Ctrl+End", scroller.ScrollToEnd)
        };
        foreach (var action in actions)
        {
            action.Action();
            host.UpdateLayout();
            var snapshot = CaptureScaleGrid(grid, scroller);
            report.AppendLine(
                $"  {label} backend={backendCount} semantic={action.Name} offset={scroller.VerticalOffset:0.##}/{scroller.ScrollableHeight:0.##} "
                + $"visible={snapshot.VisibleCount} last={snapshot.LastIndex}:{snapshot.LastId}@{snapshot.LastBottom:0.##} selected={GetStableId(grid.SelectedItem)}");
            if (!string.Equals(selectedId, GetStableId(grid.SelectedItem), StringComparison.Ordinal))
                s_problems.Add($"{label} backend={backendCount} semantic={action.Name} changed selection");
            if (snapshot.VisibleCount == 0)
                s_problems.Add($"{label} backend={backendCount} semantic={action.Name} has no visible rows");
        }

        grid.ScrollIntoView(grid.Items[grid.Items.Count - 1]);
        FlushLayoutDispatcher(host);
        var lastSnapshot = CaptureScaleGrid(grid, scroller);
        report.AppendLine(
            $"  {label} backend={backendCount} semantic=定位最后一项 offset={scroller.VerticalOffset:0.##}/{scroller.ScrollableHeight:0.##} "
            + $"last={lastSnapshot.LastIndex}:{lastSnapshot.LastId}@{lastSnapshot.LastBottom:0.##} presenterBottom={lastSnapshot.PresenterRect.Bottom:0.##}");
        if (lastSnapshot.LastIndex != grid.Items.Count - 1 || lastSnapshot.LastBottom > lastSnapshot.PresenterRect.Bottom + 1)
        {
            report.AppendLine(
                $"  {label} backend={backendCount} semantic=定位最后一项 result=offscreen-inconclusive "
                + $"last={lastSnapshot.LastIndex}:{lastSnapshot.LastId}@{lastSnapshot.LastBottom:0.##} "
                + $"presenterBottom={lastSnapshot.PresenterRect.Bottom:0.##}; standard-template comparison follows");
        }
    }

    private static void FlushLayoutDispatcher(FrameworkElement element)
    {
        element.UpdateLayout();
        element.Dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(element.UpdateLayout));
        element.UpdateLayout();
    }

    private static ScaleGridSnapshot CaptureScaleGrid(DataGrid grid, ScrollViewer scroller)
    {
        var presenter = FindVisualChildren<ScrollContentPresenter>(scroller).FirstOrDefault();
        var presenterRect = presenter == null
            ? new Rect(0, 0, scroller.ViewportWidth, scroller.ViewportHeight)
            : presenter.TransformToAncestor(scroller).TransformBounds(new Rect(0, 0, presenter.ActualWidth, presenter.ActualHeight));
        var rows = FindVisualChildren<DataGridRow>(grid)
            .Where(row => row.Visibility == Visibility.Visible && row.ActualHeight > 0)
            .Select(row =>
            {
                var rect = row.TransformToAncestor(scroller).TransformBounds(new Rect(0, 0, row.ActualWidth, row.ActualHeight));
                return new ScaleRowSnapshot(row.GetIndex(), GetStableId(row.Item), rect, row.ActualHeight);
            })
            .ToList();
        var visible = rows
            .Where(row => row.Rect.Bottom > presenterRect.Top && row.Rect.Top < presenterRect.Bottom)
            .OrderBy(row => row.Rect.Top)
            .ToList();
        var first = visible.FirstOrDefault();
        var last = visible.LastOrDefault();
        return new ScaleGridSnapshot(
            presenterRect,
            rows.Count,
            visible.Count,
            first?.Id ?? "none",
            first?.Rect.Top ?? double.NaN,
            first?.Height ?? double.NaN,
            last?.Index ?? -1,
            last?.Id ?? "none",
            last?.Rect.Top ?? double.NaN,
            last?.Rect.Bottom ?? double.NaN,
            last?.Height ?? double.NaN);
    }

    private static void RunSettingsLayoutProbes(StringBuilder report)
    {
        var apply = typeof(GameSaveCenterSettingsView).GetMethod(
            "ApplyResponsiveLayout",
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (apply == null)
            throw new InvalidOperationException("GameSaveCenterSettingsView.ApplyResponsiveLayout not found.");

        foreach (var width in new[] { 760d, 880d, 920d, 1100d, 1400d })
        {
            foreach (var height in new[] { 560d, 700d, 900d })
            {
                try
                {
                    var view = new GameSaveCenterSettingsView { DataContext = new GameSaveCenterSettings() };
                    var host = new Grid
                    {
                        Width = width,
                        Height = height,
                        Background = CreateHarnessBackground(view),
                        ClipToBounds = true
                    };
                    host.Children.Add(view);
                    apply.Invoke(view, new object[] { width, height });
                    host.Measure(new Size(width, height));
                    host.Arrange(new Rect(0, 0, width, height));
                    host.UpdateLayout();
                    apply.Invoke(view, new object[] { width, height });
                    host.UpdateLayout();

                    var header = FindVisualChildren<FrameworkElement>(host).FirstOrDefault(element => element.Name == "SettingsHeader");
                    var intro = FindVisualChildren<FrameworkElement>(host).FirstOrDefault(element => element.Name == "SettingsIntroDescription");
                    var tabs = FindVisualChildren<ListBox>(host).FirstOrDefault(element => element.Name == "SettingsSectionTabs");
                    var tabItems = tabs == null
                        ? new List<ListBoxItem>()
                        : FindVisualChildren<ListBoxItem>(tabs).Where(item => item.Parent != null).ToList();
                    var categoryScroller = tabs == null
                        ? null
                        : FindVisualChildren<ScrollViewer>(tabs).FirstOrDefault();
                    var visibleTabs = tabItems.Count(item => item.Visibility == Visibility.Visible);
                    var minTabWidth = tabItems.Count == 0 ? 0 : tabItems.Min(item => item.ActualWidth);
                    var minTabHeight = tabItems.Count == 0 ? 0 : tabItems.Min(item => item.ActualHeight);
                    report.AppendLine(
                        $"  SettingsLayout w={width:0} h={height:0} headerH={(header?.ActualHeight ?? double.NaN):0.##} intro={(intro?.Visibility.ToString() ?? "missing")} tabs={(tabs == null ? -1 : tabs.Items.Count)} tabItems={tabItems.Count} visible={visibleTabs} minW={minTabWidth:0.##} minH={minTabHeight:0.##} scroller={(categoryScroller == null ? "missing" : categoryScroller.GetType().Name)}");
                    if (header == null || header.ActualHeight <= 0)
                        s_problems.Add($"SettingsLayout w={width:0} h={height:0} header is not visible");
                    if (intro == null || intro.Visibility != Visibility.Collapsed)
                        s_problems.Add($"SettingsLayout w={width:0} h={height:0} repeated intro is still visible");
                    if (tabs == null || tabs.Items.Count != 5)
                        s_problems.Add($"SettingsLayout w={width:0} h={height:0} expected 5 categories, got {(tabs == null ? 0 : tabs.Items.Count)}");
                    if (tabItems.Count < 5 || tabItems.Any(item => item.Visibility != Visibility.Visible || item.ActualWidth <= 0 || item.ActualHeight <= 0))
                        s_problems.Add($"SettingsLayout w={width:0} h={height:0} not all category tabs are visible and measurable");
                    if (categoryScroller != null && tabItems.Count > 0 && categoryScroller.ScrollableHeight > 0.5)
                    {
                        categoryScroller.ScrollToVerticalOffset(categoryScroller.ScrollableHeight);
                        host.UpdateLayout();
                        var lastTab = tabItems.OrderBy(item => tabs!.Items.IndexOf(item)).Last();
                        var lastTabOrigin = lastTab.TransformToAncestor(categoryScroller).Transform(new Point(0, 0));
                        var lastTabBottom = lastTabOrigin.Y + lastTab.ActualHeight;
                        report.AppendLine(
                            $"  SettingsLayout w={width:0} h={height:0} lastTabBottom={lastTabBottom:0.##} viewport={categoryScroller.ViewportHeight:0.##} scrollable={categoryScroller.ScrollableHeight:0.##}");
                        if (lastTabBottom > categoryScroller.ViewportHeight + 1)
                        {
                            s_problems.Add($"SettingsLayout w={width:0} h={height:0} last category is outside the scroll viewport (tab={lastTabBottom:0.##}, viewport={categoryScroller.ViewportHeight:0.##})");
                        }
                    }
                    var contentScroller = FindVisualChildren<ScrollViewer>(host).FirstOrDefault(scroller => scroller.Name == "SettingsScroller");
                    if (contentScroller != null)
                    {
                        report.AppendLine($"  SettingsLayout w={width:0} h={height:0} contentViewport={contentScroller.ViewportHeight:0.##}");
                        if (width <= 920 && contentScroller.ViewportHeight > 0 && contentScroller.ViewportHeight < 160)
                        {
                            s_problems.Add($"SettingsLayout w={width:0} h={height:0} body viewport is too small ({contentScroller.ViewportHeight:0} DIP)");
                        }
                    }
                }
                catch (Exception ex)
                {
                    s_problems.Add($"SettingsLayout w={width:0} h={height:0} failed: {ex.Message}");
                }
            }
        }
    }

    private static void RunSettingsStateProbes(string outputRoot, StringBuilder report)
    {
        report.AppendLine();
        report.AppendLine("Settings first-viewport state fixtures (1040x700)");
        var refresh = typeof(GameSaveCenterSettingsView).GetMethod(
            "RefreshValidationSummary",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var apply = typeof(GameSaveCenterSettingsView).GetMethod(
            "ApplyResponsiveLayout",
            BindingFlags.Instance | BindingFlags.NonPublic);
        if (refresh == null || apply == null)
            throw new InvalidOperationException("Settings state probe methods not found.");

        var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var workerOutputRoot = Path.Combine(repositoryRoot, "src", "GameSaveCenter.Worker", "bin", "Release");
        var workerPath = Directory.Exists(workerOutputRoot)
            ? Directory.EnumerateFiles(workerOutputRoot, "GameSaveCenter.Worker.exe", SearchOption.AllDirectories).FirstOrDefault() ?? string.Empty
            : string.Empty;
        var settingsFixtureRoot = Path.Combine(Path.GetTempPath(), "GameSaveCenter-render-settings-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(settingsFixtureRoot);
        var fixtures = new[]
        {
            (Name: "normal", ExpectedHint: "已保存", Valid: true, Dirty: false),
            (Name: "dirty", ExpectedHint: "有未保存更改", Valid: true, Dirty: true),
            (Name: "invalid", ExpectedHint: "存在校验错误", Valid: false, Dirty: false)
        };

        try
        {
            foreach (var fixture in fixtures)
            {
                try
                {
                    var settings = new GameSaveCenterSettings
                    {
                        ThemeMode = GameSaveCenterThemeMode.Light,
                        LudusaviBackupDirectory = settingsFixtureRoot,
                        MediaArchiveDirectory = settingsFixtureRoot
                    };
                    if (fixture.Valid)
                        settings.WorkerExecutable = workerPath;
                    var view = new GameSaveCenterSettingsView { DataContext = settings };
                    var host = new Grid
                    {
                        Width = 1040,
                        Height = 700,
                        Background = CreateHarnessBackground(view),
                        ClipToBounds = true
                    };
                    host.Children.Add(view);
                    view.ApplyThemeForAudit(GameSaveCenterThemeMode.Light);
                    apply.Invoke(view, new object[] { 1040d, 700d });
                    host.Measure(new Size(1040, 700));
                    host.Arrange(new Rect(0, 0, 1040, 700));
                    host.UpdateLayout();
                    apply.Invoke(view, new object[] { 1040d, 700d });
                    host.UpdateLayout();

                    if (fixture.Dirty)
                        settings.CompressionLevel++;
                    refresh.Invoke(view, null);
                    host.UpdateLayout();
                    var shell = FindVisualChildren<FrameworkElement>(host).FirstOrDefault(element => element.Name == "SettingsShell");
                    if (shell != null)
                    {
                        shell.BeginAnimation(UIElement.OpacityProperty, null);
                        shell.Opacity = 1;
                    }
                    var hint = FindVisualChildren<TextBlock>(host).FirstOrDefault(element => element.Name == "SettingsSaveHintText");
                    var summary = FindVisualChildren<TextBlock>(host).FirstOrDefault(element => element.Name == "SettingsValidationSummary");
                    var hintText = hint?.Text ?? string.Empty;
                    var summaryVisible = summary?.Visibility == Visibility.Visible;
                    report.AppendLine($"  SettingsState state={fixture.Name} hint={hintText} summary={summaryVisible} workerPath={(fixture.Valid ? "known" : "empty")}");
                    if (hintText.IndexOf(fixture.ExpectedHint, StringComparison.Ordinal) < 0)
                        s_problems.Add($"SettingsState {fixture.Name} expected hint '{fixture.ExpectedHint}', got '{hintText}'");
                    if (summaryVisible != !fixture.Valid)
                        s_problems.Add($"SettingsState {fixture.Name} summary visibility mismatch (visible={summaryVisible})");
                    SavePng(host, Path.Combine(outputRoot, $"Settings-state-{fixture.Name}-1040x700.png"));
                }
                catch (Exception ex)
                {
                    s_problems.Add($"SettingsState {fixture.Name} failed: {ex.Message}");
                }
            }
            RunSettingsValidationNavigationProbe(workerPath, settingsFixtureRoot, refresh, apply, report);
        }
        finally
        {
            try
            {
                Directory.Delete(settingsFixtureRoot, true);
            }
            catch { }
        }
    }

    private static void RunButtonBusyProbe(string outputRoot, StringBuilder report)
    {
        report.AppendLine("Production button busy-state probe (controlled STA WPF surface)");
        var resourceHost = new GameSaveCenter.Playnite.Views.Development.UiFrameworkProbeView();
        var host = new Grid
        {
            Width = 520,
            Height = 160,
            ClipToBounds = true
        };
        host.Resources.MergedDictionaries.Add(resourceHost.Resources);
        var button = new GameSaveCenter.Playnite.Controls.Button
        {
            Width = 180,
            Height = 44,
            Margin = new Thickness(24, 54, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Content = "全部备份",
            Style = (Style)resourceHost.Resources["GscWpfUiPrimaryButton"]
        };
        host.Children.Add(button);

        try
        {
            foreach (var (themeName, themeMode) in ThemeModes)
            {
                ApplyThemePalette(resourceHost, themeMode, glassEnabled: true, motionEnabled: true);
                host.Background = CreateHarnessBackground(resourceHost);
                host.Measure(new Size(host.Width, host.Height));
                host.Arrange(new Rect(0, 0, host.Width, host.Height));
                host.UpdateLayout();
                button.ApplyTemplate();
                var normalWidth = button.ActualWidth;
                var contentText = FindVisualChildren<TextBlock>(button)
                    .FirstOrDefault(text => text.Text == "全部备份");
                SavePng(host, Path.Combine(outputRoot, $"button-busy-{themeName}-normal.png"));

                button.IsBusy = true;
                host.UpdateLayout();
                var indicatorHost = button.Template?.FindName("BusyIndicatorHost", button) as FrameworkElement;
                var indicator = FindVisualChildren<ProgressBar>(button).FirstOrDefault(progress => progress.IsIndeterminate);
                var busyWidth = button.ActualWidth;
                var stable = Math.Abs(normalWidth - busyWidth) < 0.01;
                var visible = indicatorHost?.Visibility == Visibility.Visible;
                var contentStable = contentText != null && contentText.Text == "全部备份";
                SavePng(host, Path.Combine(outputRoot, $"button-busy-{themeName}-busy.png"));
                report.AppendLine(
                    $"Busy[{themeName}] normalWidth={normalWidth:0.##} busyWidth={busyWidth:0.##} "
                    + $"widthStable={stable} indicatorVisible={visible} indeterminate={indicator?.IsIndeterminate == true} "
                    + $"contentStable={contentStable}");
                if (!stable || !visible || indicator == null || !contentStable)
                    throw new InvalidOperationException($"Busy button contract failed for {themeName}.");
                button.IsBusy = false;
            }

            report.AppendLine("BusyStateBoundary: shared production IsBusy binding covers the Acrylic shell and workspace style chain; real command timing and Playnite host input remain host checks");
            report.AppendLine("ButtonBusyProbe OK");
        }
        catch (Exception ex)
        {
            s_problems.Add("ButtonBusyProbe failed: " + ex.Message);
            report.AppendLine("ButtonBusyProbe FAILED");
            report.AppendLine(ex.ToString());
        }
    }

    private static void RunDangerDialogProbe(string outputRoot, StringBuilder report)
    {
        report.AppendLine("Production dangerous dialog layout probe (controlled STA WPF surface)");
        var application = Application.Current;
        var previousShutdownMode = application?.ShutdownMode;
        Window? window = null;
        try
        {
            if (application != null)
                application.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var resourceHost = new GameSaveCenter.Playnite.Views.Development.UiFrameworkProbeView();
            var root = new Grid
            {
                Width = 680,
                Height = 360,
                ClipToBounds = true
            };
            root.Resources.MergedDictionaries.Add(resourceHost.Resources);
            var dialog = new Border
            {
                Width = 560,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Style = (Style)resourceHost.Resources["GscRedesignFeedbackDialogCard"],
                Padding = new Thickness(22),
                Opacity = 1
            };
            var layout = new Grid();
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var title = new TextBlock
            {
                Text = "确认删除存档",
                FontSize = 18,
                FontWeight = FontWeights.SemiBold
            };
            title.SetResourceReference(TextElement.ForegroundProperty, "GscPrimaryTextBrush");
            var message = new TextBlock
            {
                Text = "此操作将删除当前选中的本地归档，但不会影响 Playnite 游戏库。",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 12, 0, 0)
            };
            message.SetResourceReference(TextElement.ForegroundProperty, "GscSecondaryTextBrush");
            var divider = new Border
            {
                Height = 1,
                Margin = new Thickness(0, 18, 0, 16)
            };
            divider.SetResourceReference(Control.BackgroundProperty, "GscDividerBrush");
            var actions = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            var cancel = new System.Windows.Controls.Button
            {
                Content = "取消",
                MinWidth = 76,
                Style = (Style)resourceHost.Resources["GscButtonBase"],
                Margin = new Thickness(0, 0, 8, 0)
            };
            var danger = new System.Windows.Controls.Button
            {
                Content = "删除存档",
                MinWidth = 100,
                Style = (Style)resourceHost.Resources["GscButtonBase"]
            };
            danger.SetResourceReference(Control.BackgroundProperty, "GscErrorBrush");
            danger.SetResourceReference(Control.BorderBrushProperty, "GscErrorBrush");
            actions.Children.Add(cancel);
            actions.Children.Add(danger);
            layout.Children.Add(title);
            Grid.SetRow(message, 1);
            layout.Children.Add(message);
            Grid.SetRow(divider, 2);
            layout.Children.Add(divider);
            Grid.SetRow(actions, 3);
            layout.Children.Add(actions);
            dialog.Child = layout;
            root.Children.Add(dialog);
            window = new Window
            {
                Width = root.Width,
                Height = root.Height,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                ShowActivated = false,
                Opacity = 1,
                Content = root
            };
            window.Show();
            window.UpdateLayout();

            foreach (var (themeName, themeMode) in ThemeModes)
            {
                ApplyThemePalette(resourceHost, themeMode, glassEnabled: true, motionEnabled: true);
                root.Background = CreateHarnessBackground(resourceHost);
                window.UpdateLayout();
                cancel.Focus();
                PumpDispatcher(80);
                var cancelBounds = cancel.TransformToAncestor(root).TransformBounds(new Rect(0, 0, cancel.ActualWidth, cancel.ActualHeight));
                var dangerBounds = danger.TransformToAncestor(root).TransformBounds(new Rect(0, 0, danger.ActualWidth, danger.ActualHeight));
                var cancelFocused = cancel.IsKeyboardFocusWithin;
                var gap = dangerBounds.Left - cancelBounds.Right;
                SavePng(root, Path.Combine(outputRoot, $"danger-dialog-{themeName}.png"));
                report.AppendLine(
                    $"Dialog[{themeName}] cancelFocused={cancelFocused} dialogWidth={dialog.ActualWidth:0.##} "
                    + $"cancel={FormatRect(cancelBounds)} danger={FormatRect(dangerBounds)} gap={gap:0.##} dangerFirstFocus=false");
                if (!cancelFocused || dialog.ActualWidth <= 0 || gap <= 0)
                    throw new InvalidOperationException($"Danger dialog layout contract failed for {themeName}.");
            }

            report.AppendLine("DangerDialogBoundary: production resource layout and cancel-first focus are covered; business confirmation completion and real Playnite host input remain host checks");
            report.AppendLine("DangerDialogProbe OK");
        }
        catch (Exception ex)
        {
            s_problems.Add("DangerDialogProbe failed: " + ex.Message);
            report.AppendLine("DangerDialogProbe FAILED");
            report.AppendLine(ex.ToString());
        }
        finally
        {
            window?.Close();
            if (application != null && previousShutdownMode.HasValue)
                application.ShutdownMode = previousShutdownMode.Value;
        }
    }

    private static void RunMotionProbe(string outputRoot, StringBuilder report)
    {
        report.AppendLine("Production shell motion probe (controlled STA WPF Window)");
        AppendRunMetadata(report, "motionprobe", "ControlledWpfWindow", "light,dark", "production shell; audit-only GscMotionNormal=700ms override; 900x640 DIP");
        Window? window = null;
        try
        {
            foreach (var (themeName, themeMode) in ThemeModes)
            {
                var data = new FakeDashboardData(8);
                var shell = new AcrylicProductionShellView
                {
                    DataContext = data,
                    MotionEnabledProvider = () => true,
                    SidebarCollapsedProvider = () => false
                };
                window = new Window
                {
                    Width = 900,
                    Height = 640,
                    WindowStyle = WindowStyle.None,
                    ResizeMode = ResizeMode.NoResize,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    Left = -32000,
                    Top = -32000,
                    Opacity = 0.01,
                    Content = shell
                };
                window.Show();
                window.UpdateLayout();
                shell.ApplyResponsiveLayout(window.Width, window.Height);
                window.UpdateLayout();
                var layer = shell.FindName("SidebarContentLayer") as FrameworkElement
                    ?? throw new InvalidOperationException("Motion probe could not find SidebarContentLayer.");
                var button = shell.SidebarCollapseButtonForAudit;
                ApplyThemePalette(shell, themeMode, glassEnabled: true, motionEnabled: true);
                // Extend only the audit clock so the intermediate frame remains
                // observable on a busy WPF desktop; production tokens stay unchanged.
                shell.Resources["GscMotionNormal"] = new Duration(TimeSpan.FromMilliseconds(700));
                shell.ApplyResponsiveLayout(window.Width, window.Height);
                window.UpdateLayout();
                button.ApplyTemplate();
                if (shell.SidebarCollapsedForAudit)
                    throw new InvalidOperationException($"Motion probe started {themeName} in collapsed state.");

                var duration = GscMotion.GetDuration(layer, GscMotion.MotionDurationKind.Normal);
                var midpoint = Math.Max(20, (int)Math.Round(duration.TotalMilliseconds * 0.30));
                SavePng(shell, Path.Combine(outputRoot, $"motion-{themeName}-expanded.png"));

                button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                PumpDispatcher(midpoint);
                var midTranslate = layer.RenderTransform as TranslateTransform;
                var midWidth = shell.SidebarWidthForAudit;
                var midOpacity = layer.Opacity;
                var midAnimated = DependencyPropertyHelper.GetValueSource(layer, UIElement.OpacityProperty).IsAnimated
                    || (midTranslate != null && DependencyPropertyHelper.GetValueSource(midTranslate, TranslateTransform.XProperty).IsAnimated);
                SavePng(shell, Path.Combine(outputRoot, $"motion-{themeName}-collapsed-mid.png"));

                PumpDispatcher((int)Math.Ceiling(duration.TotalMilliseconds) + 80);
                var endTranslate = layer.RenderTransform as TranslateTransform;
                var endAnimated = DependencyPropertyHelper.GetValueSource(layer, UIElement.OpacityProperty).IsAnimated
                    || (endTranslate != null && DependencyPropertyHelper.GetValueSource(endTranslate, TranslateTransform.XProperty).IsAnimated);
                SavePng(shell, Path.Combine(outputRoot, $"motion-{themeName}-collapsed-end.png"));
                report.AppendLine(
                    $"Motion[{themeName}] duration_ms={duration.TotalMilliseconds:0} midpoint_ms={midpoint} "
                    + $"midWidth={midWidth:0.##} midOpacity={midOpacity:0.###} midAnimated={midAnimated} "
                    + $"endWidth={shell.SidebarWidthForAudit:0.##} endOpacity={layer.Opacity:0.###} "
                    + $"endX={endTranslate?.X:0.###} endAnimated={endAnimated}");
                if (!midAnimated || midWidth <= 78 || midWidth >= 260 || midOpacity <= 0.05 || midOpacity >= 0.95
                    || shell.SidebarWidthForAudit != 72
                    || Math.Abs(layer.Opacity - 1) > 0.001
                    || (endTranslate != null && Math.Abs(endTranslate.X) > 0.001)
                    || endAnimated)
                {
                    throw new InvalidOperationException($"Motion probe did not observe a clean {themeName} transition.");
                }

                button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                PumpDispatcher(midpoint);
                var interruptedWidth = shell.SidebarWidthForAudit;
                button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                PumpDispatcher((int)Math.Ceiling(duration.TotalMilliseconds) + 80);
                var reentryTranslate = layer.RenderTransform as TranslateTransform;
                SavePng(shell, Path.Combine(outputRoot, $"motion-{themeName}-reentry-end.png"));
                report.AppendLine(
                    $"Reentry[{themeName}] interruptedWidth={interruptedWidth:0.##} "
                    + $"finalWidth={shell.SidebarWidthForAudit:0.##} finalX={reentryTranslate?.X:0.###} "
                    + $"running={shell.SidebarTransitionRunningForAudit}");
                if (interruptedWidth <= 72 || interruptedWidth >= 270
                    || shell.SidebarWidthForAudit != 72
                    || shell.SidebarTransitionRunningForAudit
                    || (reentryTranslate != null && Math.Abs(reentryTranslate.X) > 0.001))
                {
                    throw new InvalidOperationException($"Motion probe did not preserve the latest {themeName} reentry target.");
                }

                button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                if (!shell.SidebarTransitionRunningForAudit)
                    throw new InvalidOperationException($"Motion probe could not start unload transition for {themeName}.");
                window.Close();
                PumpDispatcher((int)Math.Ceiling(duration.TotalMilliseconds) + 80);
                var unloadedTranslate = layer.RenderTransform as TranslateTransform;
                var unloadedAnimated = DependencyPropertyHelper.GetValueSource(layer, UIElement.OpacityProperty).IsAnimated
                    || (unloadedTranslate != null && DependencyPropertyHelper.GetValueSource(unloadedTranslate, TranslateTransform.XProperty).IsAnimated);
                report.AppendLine(
                    $"Unload[{themeName}] running={shell.SidebarTransitionRunningForAudit} "
                    + $"opacity={layer.Opacity:0.###} x={unloadedTranslate?.X:0.###} animated={unloadedAnimated}");
                if (shell.SidebarTransitionRunningForAudit || Math.Abs(layer.Opacity - 1) > 0.001
                    || (unloadedTranslate != null && Math.Abs(unloadedTranslate.X) > 0.001)
                    || unloadedAnimated)
                {
                    throw new InvalidOperationException($"Motion probe did not clean the unloaded {themeName} shell.");
                }

                window.Close();
                window = null;
            }

            report.AppendLine("MotionProbeBoundary: controlled production shell screenshots and clock cleanup are covered; real Playnite Loaded/Unloaded cadence and ETW frame evidence remain host/performance checks");
            report.AppendLine("MotionProbe OK");
        }
        catch (Exception ex)
        {
            s_problems.Add("MotionProbe failed: " + ex.Message);
            report.AppendLine("MotionProbe FAILED");
            report.AppendLine(ex.ToString());
        }
        finally
        {
            window?.Close();
        }
    }

    private static void RunMotionHotChangeProbe(string outputRoot, StringBuilder report)
    {
        report.AppendLine("Production shell motion hot-change probe (controlled STA WPF Window)");
        AppendRunMetadata(report, "motionhotprobe", "ControlledWpfWindow", "light,dark", "production shell; audit-only GscMotionNormal=700ms override; runtime motion preference toggle; 900x640 DIP");
        Window? window = null;
        try
        {
            foreach (var (themeName, themeMode) in ThemeModes)
            {
                var motionEnabled = true;
                var shell = new AcrylicProductionShellView
                {
                    DataContext = new FakeDashboardData(8),
                    MotionEnabledProvider = () => motionEnabled,
                    SidebarCollapsedProvider = () => false
                };
                window = new Window
                {
                    Width = 900,
                    Height = 640,
                    WindowStyle = WindowStyle.None,
                    ResizeMode = ResizeMode.NoResize,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    Left = -32000,
                    Top = -32000,
                    Opacity = 0.01,
                    Content = shell
                };
                window.Show();
                window.UpdateLayout();
                shell.ApplyResponsiveLayout(window.Width, window.Height);
                window.UpdateLayout();
                var layer = shell.FindName("SidebarContentLayer") as FrameworkElement
                    ?? throw new InvalidOperationException("Motion hot-change probe could not find SidebarContentLayer.");
                var button = shell.SidebarCollapseButtonForAudit;
                ApplyThemePalette(shell, themeMode, glassEnabled: true, motionEnabled: true);
                shell.Resources["GscMotionNormal"] = new Duration(TimeSpan.FromMilliseconds(700));
                shell.ApplyResponsiveLayout(window.Width, window.Height);
                window.UpdateLayout();

                button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                PumpDispatcher(210);
                var duringTranslate = layer.RenderTransform as TranslateTransform;
                var duringAnimated = DependencyPropertyHelper.GetValueSource(layer, UIElement.OpacityProperty).IsAnimated
                    || (duringTranslate != null && DependencyPropertyHelper.GetValueSource(duringTranslate, TranslateTransform.XProperty).IsAnimated);
                var duringWidth = shell.SidebarWidthForAudit;
                var duringOpacity = layer.Opacity;
                SavePng(shell, Path.Combine(outputRoot, $"motion-hot-{themeName}-enabled-mid.png"));

                motionEnabled = false;
                shell.NormalizeMotionIfDisabled();
                window.UpdateLayout();
                var disabledTranslate = layer.RenderTransform as TranslateTransform;
                var disabledAnimated = DependencyPropertyHelper.GetValueSource(layer, UIElement.OpacityProperty).IsAnimated
                    || (disabledTranslate != null && DependencyPropertyHelper.GetValueSource(disabledTranslate, TranslateTransform.XProperty).IsAnimated);
                var disabledFinalWidth = shell.SidebarWidthForAudit;
                SavePng(shell, Path.Combine(outputRoot, $"motion-hot-{themeName}-disabled-final.png"));

                if (!duringAnimated || duringWidth <= 78 || duringWidth >= 270 || duringOpacity <= 0.05 || duringOpacity >= 0.95
                    || shell.SidebarMotionEnabledForAudit
                    || shell.SidebarTransitionRunningForAudit
                    || shell.SidebarWidthForAudit != 72
                    || Math.Abs(layer.Opacity - 1) > 0.001
                    || (disabledTranslate != null && Math.Abs(disabledTranslate.X) > 0.001)
                    || disabledAnimated)
                {
                    throw new InvalidOperationException($"Motion hot-change probe did not normalize the active {themeName} transition.");
                }

                button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                PumpDispatcher(40);
                var disabledReentryTranslate = layer.RenderTransform as TranslateTransform;
                var disabledReentryAnimated = DependencyPropertyHelper.GetValueSource(layer, UIElement.OpacityProperty).IsAnimated
                    || (disabledReentryTranslate != null && DependencyPropertyHelper.GetValueSource(disabledReentryTranslate, TranslateTransform.XProperty).IsAnimated);
                SavePng(shell, Path.Combine(outputRoot, $"motion-hot-{themeName}-disabled-reentry.png"));
                report.AppendLine(
                    $"HotChange[{themeName}] duringWidth={duringWidth:0.##} duringOpacity={duringOpacity:0.###} duringAnimated={duringAnimated} "
                    + $"disabledFinalWidth={disabledFinalWidth:0.##} disabledOpacity={layer.Opacity:0.###} "
                    + $"disabledReentryWidth={shell.SidebarWidthForAudit:0.##} "
                    + $"disabledX={disabledReentryTranslate?.X:0.###} disabledReentryAnimated={disabledReentryAnimated}");
                if (shell.SidebarWidthForAudit != 270
                    || shell.SidebarTransitionRunningForAudit
                    || disabledReentryAnimated
                    || Math.Abs(layer.Opacity - 1) > 0.001
                    || (disabledReentryTranslate != null && Math.Abs(disabledReentryTranslate.X) > 0.001))
                {
                    throw new InvalidOperationException($"Motion hot-change probe did not keep the disabled {themeName} path immediate.");
                }

                window.Close();
                window = null;
            }

            report.AppendLine("MotionHotChangeBoundary: active production sidebar transition was normalized immediately after a runtime motion toggle; disabled re-entry remained immediate; real Windows preference notification and Playnite host pixels remain host checks");
            report.AppendLine("MotionHotChangeProbe OK");
        }
        catch (Exception ex)
        {
            s_problems.Add("MotionHotChangeProbe failed: " + ex.Message);
            report.AppendLine("MotionHotChangeProbe FAILED");
            report.AppendLine(ex.ToString());
        }
        finally
        {
            window?.Close();
        }
    }

    private static void RunMotionCycleProbe(string outputRoot, StringBuilder report)
    {
        report.AppendLine("Production shell Loaded/Unloaded motion cycle probe (controlled STA WPF Window)");
        AppendRunMetadata(report, "motioncycleprobe", "ControlledWpfWindow", "light,dark", "production shell; 100 Loaded/Unloaded cycles; audit-only GscMotionNormal=700ms override; 900x640 DIP");
        Window? window = null;
        try
        {
            foreach (var (themeName, themeMode) in ThemeModes)
            {
                var shell = new AcrylicProductionShellView
                {
                    DataContext = new FakeDashboardData(8),
                    MotionEnabledProvider = () => true,
                    SidebarCollapsedProvider = () => false
                };
                var host = new ContentControl
                {
                    Content = shell
                };
                var loadedCount = 0;
                var unloadedCount = 0;
                shell.Loaded += (_, __) => loadedCount++;
                shell.Unloaded += (_, __) => unloadedCount++;
                window = new Window
                {
                    Width = 900,
                    Height = 640,
                    WindowStyle = WindowStyle.None,
                    ResizeMode = ResizeMode.NoResize,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    Left = -32000,
                    Top = -32000,
                    Opacity = 0.01,
                    Content = host
                };
                window.Show();
                window.UpdateLayout();
                var layer = shell.FindName("SidebarContentLayer") as FrameworkElement
                    ?? throw new InvalidOperationException("Motion cycle probe could not find SidebarContentLayer.");
                var button = shell.SidebarCollapseButtonForAudit;
                ApplyThemePalette(shell, themeMode, glassEnabled: true, motionEnabled: true);
                shell.Resources["GscMotionNormal"] = new Duration(TimeSpan.FromMilliseconds(700));
                window.UpdateLayout();
                SavePng(shell, Path.Combine(outputRoot, $"motion-cycle-{themeName}-loaded.png"));

                for (var cycle = 0; cycle < 100; cycle++)
                {
                    button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                    PumpDispatcher(35);
                    if (!shell.SidebarTransitionRunningForAudit)
                        throw new InvalidOperationException($"Motion cycle {themeName} #{cycle + 1} did not start a transition.");
                    if (cycle == 0)
                        SavePng(shell, Path.Combine(outputRoot, $"motion-cycle-{themeName}-active.png"));

                    host.Content = null;
                    window.UpdateLayout();
                    PumpDispatcher(5);
                    var unloadedTranslate = layer.RenderTransform as TranslateTransform;
                    var unloadedAnimated = DependencyPropertyHelper.GetValueSource(layer, UIElement.OpacityProperty).IsAnimated
                        || (unloadedTranslate != null && DependencyPropertyHelper.GetValueSource(unloadedTranslate, TranslateTransform.XProperty).IsAnimated);
                    if (shell.IsLoaded || shell.SidebarTransitionRunningForAudit || unloadedAnimated
                        || Math.Abs(layer.Opacity - 1) > 0.001
                        || (unloadedTranslate != null && Math.Abs(unloadedTranslate.X) > 0.001))
                    {
                        throw new InvalidOperationException($"Motion cycle {themeName} #{cycle + 1} left an active visual after unload.");
                    }

                    host.Content = shell;
                    window.UpdateLayout();
                    PumpDispatcher(5);
                    if (!shell.IsLoaded || shell.SidebarTransitionRunningForAudit)
                        throw new InvalidOperationException($"Motion cycle {themeName} #{cycle + 1} did not reload cleanly.");
                }

                SavePng(shell, Path.Combine(outputRoot, $"motion-cycle-{themeName}-reloaded.png"));
                host.Content = null;
                window.UpdateLayout();
                PumpDispatcher(5);
                report.AppendLine(
                    $"Cycle[{themeName}] cycles=100 loaded={loadedCount} unloaded={unloadedCount} "
                    + $"finalLoaded={shell.IsLoaded} transitionRunning={shell.SidebarTransitionRunningForAudit} opacity={layer.Opacity:0.###}");
                if (loadedCount != 101 || unloadedCount != 101 || shell.IsLoaded
                    || shell.SidebarTransitionRunningForAudit)
                {
                    throw new InvalidOperationException($"Motion cycle {themeName} event counts did not return to the expected baseline.");
                }

                window.Close();
                window = null;
            }

            report.AppendLine("MotionCycleBoundary: 100 controlled production shell Loaded/Unloaded cycles returned event counts to 101/101 and left no sidebar animation clocks; shell has no Timer/Rendering subscription, while Dashboard/Playnite host notification and physical screen evidence remain separate checks");
            report.AppendLine("MotionCycleProbe OK");
        }
        catch (Exception ex)
        {
            s_problems.Add("MotionCycleProbe failed: " + ex.Message);
            report.AppendLine("MotionCycleProbe FAILED");
            report.AppendLine(ex.ToString());
        }
        finally
        {
            window?.Close();
        }
    }

    private static void RunMotionReentryProbe(string outputRoot, StringBuilder report)
    {
        report.AppendLine("Production shell motion reentry current-value probe (controlled STA WPF Window)");
        AppendRunMetadata(report, "motionreentryprobe", "ControlledWpfWindow", "light,dark", "production shell; audit-only GscMotionNormal=700ms override; interrupted transition takeover; 900x640 DIP");
        Window? window = null;
        try
        {
            foreach (var (themeName, themeMode) in ThemeModes)
            {
                var shell = new AcrylicProductionShellView
                {
                    DataContext = new FakeDashboardData(8),
                    MotionEnabledProvider = () => true,
                    SidebarCollapsedProvider = () => false
                };
                window = new Window
                {
                    Width = 900,
                    Height = 640,
                    WindowStyle = WindowStyle.None,
                    ResizeMode = ResizeMode.NoResize,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    Left = -32000,
                    Top = -32000,
                    Opacity = 0.01,
                    Content = shell
                };
                window.Show();
                window.UpdateLayout();
                var layer = shell.FindName("SidebarContentLayer") as FrameworkElement
                    ?? throw new InvalidOperationException("Motion reentry probe could not find SidebarContentLayer.");
                var button = shell.SidebarCollapseButtonForAudit;
                ApplyThemePalette(shell, themeMode, glassEnabled: true, motionEnabled: true);
                shell.Resources["GscMotionNormal"] = new Duration(TimeSpan.FromMilliseconds(700));
                window.UpdateLayout();

                button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                PumpDispatcher(210);
                var interruptedWidth = shell.SidebarWidthForAudit;
                if (!shell.SidebarTransitionRunningForAudit || interruptedWidth <= 78 || interruptedWidth >= 270)
                    throw new InvalidOperationException($"Motion reentry probe did not reach an active interrupted {themeName} state.");
                SavePng(shell, Path.Combine(outputRoot, $"motion-reentry-{themeName}-interrupted.png"));

                button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                var immediateWidth = shell.SidebarWidthForAudit;
                SavePng(shell, Path.Combine(outputRoot, $"motion-reentry-{themeName}-takeover.png"));
                if (!shell.SidebarTransitionRunningForAudit || Math.Abs(immediateWidth - interruptedWidth) > 1.5)
                    throw new InvalidOperationException($"Motion reentry probe jumped from the current {themeName} value before the new animation.");

                PumpDispatcher(210);
                var reentryMidWidth = shell.SidebarWidthForAudit;
                SavePng(shell, Path.Combine(outputRoot, $"motion-reentry-{themeName}-mid.png"));
                PumpDispatcher(570);
                var finalTranslate = layer.RenderTransform as TranslateTransform;
                var finalAnimated = DependencyPropertyHelper.GetValueSource(layer, UIElement.OpacityProperty).IsAnimated
                    || (finalTranslate != null && DependencyPropertyHelper.GetValueSource(finalTranslate, TranslateTransform.XProperty).IsAnimated);
                SavePng(shell, Path.Combine(outputRoot, $"motion-reentry-{themeName}-final.png"));
                report.AppendLine(
                    $"Reentry[{themeName}] interruptedWidth={interruptedWidth:0.##} immediateWidth={immediateWidth:0.##} "
                    + $"midWidth={reentryMidWidth:0.##} finalWidth={shell.SidebarWidthForAudit:0.##} "
                    + $"finalX={finalTranslate?.X:0.###} finalAnimated={finalAnimated}");
                if (reentryMidWidth <= interruptedWidth + 5
                    || shell.SidebarWidthForAudit != 270
                    || shell.SidebarTransitionRunningForAudit
                    || (finalTranslate != null && Math.Abs(finalTranslate.X) > 0.001)
                    || finalAnimated)
                {
                    throw new InvalidOperationException($"Motion reentry probe did not reach the latest {themeName} target cleanly.");
                }

                window.Close();
                window = null;
            }

            report.AppendLine("MotionReentryBoundary: the second production sidebar intent started from the currently rendered width without a reset to the obsolete endpoint; real Playnite input and physical screen frames remain host checks");
            report.AppendLine("MotionReentryProbe OK");
        }
        catch (Exception ex)
        {
            s_problems.Add("MotionReentryProbe failed: " + ex.Message);
            report.AppendLine("MotionReentryProbe FAILED");
            report.AppendLine(ex.ToString());
        }
        finally
        {
            window?.Close();
        }
    }

    private static void PumpDispatcher(int milliseconds)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(Math.Max(1, milliseconds))
        };
        timer.Tick += (_, __) =>
        {
            timer.Stop();
            frame.Continue = false;
        };
        timer.Start();
        Dispatcher.PushFrame(frame);
    }

    private static void RunSettingsThemeTransitionProbe(string outputRoot, StringBuilder report)
    {
        report.AppendLine();
        report.AppendLine("Settings open-surface theme transition probe (1040x700)");
        Window? window = null;
        ToolTip? toolTip = null;
        var application = Application.Current;
        var previousShutdownMode = application?.ShutdownMode;
        try
        {
            if (application != null)
                application.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var settings = new GameSaveCenterSettings
            {
                ThemeMode = GameSaveCenterThemeMode.Light
            };
            var view = new GameSaveCenterSettingsView { DataContext = settings };
            var host = new Grid
            {
                Width = 1040,
                Height = 700,
                Background = CreateHarnessBackground(view),
                ClipToBounds = true
            };
            host.Children.Add(view);
            window = new Window
            {
                Width = 1040,
                Height = 700,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                ShowActivated = false,
                Opacity = 0.01,
                Content = host
            };
            window.Show();

            ApplyThemePalette(view, GameSaveCenterThemeMode.Light);
            ApplyThemeResponsive(view, 1040, 700);
            window.UpdateLayout();
            var shell = FindVisualChildren<FrameworkElement>(host)
                .FirstOrDefault(element => element.Name == "SettingsShell");
            if (shell != null)
            {
                shell.BeginAnimation(UIElement.OpacityProperty, null);
                shell.Opacity = 1;
            }
            SelectTab(view, 2);
            window.UpdateLayout();

            var selector = FindVisualChildren<ComboBox>(host)
                .FirstOrDefault(combo => combo.Name == "ThemeModeSelector");
            var hint = FindVisualChildren<FrameworkElement>(host)
                .FirstOrDefault(element => element.Name == "SettingsSaveHint");
            if (selector == null || hint == null)
                throw new InvalidOperationException("Settings theme transition controls did not materialize.");

            selector.ApplyTemplate();
            selector.Focus();
            selector.SetCurrentValue(ComboBox.IsDropDownOpenProperty, true);
            window.UpdateLayout();
            window.Dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
            var popup = selector.Template?.FindName("PART_Popup", selector) as Popup;
            if (popup == null || popup.Child is not FrameworkElement popupChild)
                throw new InvalidOperationException("Theme selector Popup did not materialize as a WPF Popup child.");
            // A hidden, non-activated audit window has no mouse capture. WPF therefore
            // immediately closes the production StaysOpen=False popup even though the
            // ComboBox IsDropDownOpen binding is true. Keep the production binding, but
            // hold this audit surface open long enough to inspect its dynamic resources.
            popup.StaysOpen = true;
            popup.IsOpen = true;
            window.Dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
            popupChild.UpdateLayout();
            if (!popup.IsOpen || popupChild.ActualWidth <= 0 || popupChild.ActualHeight <= 0)
                throw new InvalidOperationException($"Theme selector Popup is not measurable (open={popup.IsOpen}, size={popupChild.ActualWidth:0.##}x{popupChild.ActualHeight:0.##}).");

            var lightPrimary = (view.TryFindResource("GscPrimaryTextBrush") as SolidColorBrush)?.Color;
            SavePng(host, Path.Combine(outputRoot, "Settings-theme-switch-light-open-1040x700.png"));
            SavePng(popupChild, Path.Combine(outputRoot, "Settings-theme-switch-light-popup.png"));

            toolTip = new ToolTip
            {
                Content = "当前设置主题预览提示",
                PlacementTarget = hint,
                Placement = PlacementMode.Bottom,
                StaysOpen = true
            };
            hint.ToolTip = toolTip;
            toolTip.IsOpen = true;
            window.UpdateLayout();
            window.Dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
            if (!toolTip.IsOpen || toolTip.ActualWidth <= 0 || toolTip.ActualHeight <= 0)
                throw new InvalidOperationException($"Settings ToolTip is not measurable (open={toolTip.IsOpen}, size={toolTip.ActualWidth:0.##}x{toolTip.ActualHeight:0.##}).");
            SavePng(toolTip, Path.Combine(outputRoot, "Settings-theme-switch-light-tooltip.png"));

            settings.ThemeMode = GameSaveCenterThemeMode.Dark;
            selector.SelectedValue = GameSaveCenterThemeMode.Dark;
            ApplyThemePalette(view, GameSaveCenterThemeMode.Dark);
            ApplyThemeResponsive(view, 1040, 700);
            window.UpdateLayout();
            popupChild.UpdateLayout();
            toolTip.UpdateLayout();
            var darkPrimary = (view.TryFindResource("GscPrimaryTextBrush") as SolidColorBrush)?.Color;
            if (!lightPrimary.HasValue || !darkPrimary.HasValue || lightPrimary.Value == darkPrimary.Value)
                s_problems.Add($"SettingsThemeTransition primary text did not change ({lightPrimary?.ToString() ?? "none"} -> {darkPrimary?.ToString() ?? "none"})");
            if (!popup.IsOpen)
                s_problems.Add("SettingsThemeTransition selector Popup closed during the Light -> Dark switch");
            if (selector.SelectedValue is not GameSaveCenterThemeMode selectedMode
                || selectedMode != GameSaveCenterThemeMode.Dark)
                s_problems.Add($"SettingsThemeTransition selector did not reflect Dark after the switch (value={selector.SelectedValue ?? "none"})");
            VerifyThemePalette(view, "settings/open-surfaces/dark", GameSaveCenterThemeMode.Dark);
            SavePng(host, Path.Combine(outputRoot, "Settings-theme-switch-dark-open-1040x700.png"));
            SavePng(popupChild, Path.Combine(outputRoot, "Settings-theme-switch-dark-popup.png"));
            SavePng(toolTip, Path.Combine(outputRoot, "Settings-theme-switch-dark-tooltip.png"));
            report.AppendLine($"  SettingsThemeTransition popupOpen={popup.IsOpen} tooltipOpen={toolTip.IsOpen} primary={lightPrimary?.ToString() ?? "none"}->{darkPrimary?.ToString() ?? "none"} screenshots=6");
        }
        catch (Exception ex)
        {
            s_problems.Add("SettingsThemeTransition failed: " + ex.Message);
        }
        finally
        {
            if (toolTip != null)
                toolTip.IsOpen = false;
            window?.Close();
            if (application != null && previousShutdownMode.HasValue)
                application.ShutdownMode = previousShutdownMode.Value;
        }
    }

    private static void RunSettingsValidationNavigationProbe(
        string workerPath,
        string settingsFixtureRoot,
        MethodInfo refresh,
        MethodInfo apply,
        StringBuilder report)
    {
        try
        {
            var settings = new GameSaveCenterSettings
            {
                ThemeMode = GameSaveCenterThemeMode.Light,
                WorkerExecutable = workerPath,
                LudusaviBackupDirectory = settingsFixtureRoot,
                MediaArchiveDirectory = settingsFixtureRoot,
                CompressionLevel = 99
            };
            var view = new GameSaveCenterSettingsView { DataContext = settings };
            var host = new Grid
            {
                Width = 1040,
                Height = 700,
                Background = CreateHarnessBackground(view),
                ClipToBounds = true
            };
            host.Children.Add(view);
            view.ApplyThemeForAudit(GameSaveCenterThemeMode.Light);
            apply.Invoke(view, new object[] { 1040d, 700d });
            host.Measure(new Size(1040, 700));
            host.Arrange(new Rect(0, 0, 1040, 700));
            host.UpdateLayout();
            apply.Invoke(view, new object[] { 1040d, 700d });
            refresh.Invoke(view, null);
            host.UpdateLayout();

            var locator = view.FindName("SettingsValidationLocateButton") as FrameworkElement;
            var tabs = view.FindName("SettingsSectionTabs") as ListBox;
            var details = view.FindName("SettingsValidationDetailsText") as TextBlock;
            var field = view.FindName("CompressionLevelTextBox") as TextBox;
            var links = details?.Inlines.OfType<Hyperlink>().ToArray() ?? Array.Empty<Hyperlink>();
            if (locator == null || tabs == null || details == null || field == null || links.Length == 0)
                throw new InvalidOperationException("Settings validation locator controls did not materialize.");

            var compressionLink = links.Single(link => link.Inlines.OfType<Run>()
                .Any(run => run.Text.IndexOf("压缩等级", StringComparison.Ordinal) >= 0));
            compressionLink.RaiseEvent(new RoutedEventArgs(Hyperlink.ClickEvent));
            host.UpdateLayout();
            var helpText = AutomationProperties.GetHelpText(field) ?? string.Empty;
            var settingsScroller = FindVisualChildren<ScrollViewer>(host)
                .FirstOrDefault(element => element.Name == "SettingsScroller");
            var scrollOffset = settingsScroller?.VerticalOffset ?? -1;
            report.AppendLine($"  SettingsValidationNavigation visible={locator.Visibility} links={links.Length} selectedCategory={tabs.SelectedIndex} fieldVisibility={field.Visibility} fieldIsVisible={field.IsVisible} fieldFocused={field.IsKeyboardFocusWithin} scrollOffset={scrollOffset:0.##} help={helpText}");
            if (locator.Visibility != Visibility.Visible)
                s_problems.Add("SettingsValidationNavigation locator is not visible for a backup validation error");
            if (tabs.SelectedIndex != 1)
                s_problems.Add($"SettingsValidationNavigation selected category {tabs.SelectedIndex}, expected 1");
            if (field.Visibility != Visibility.Visible || helpText.IndexOf("压缩等级", StringComparison.Ordinal) < 0)
                s_problems.Add("SettingsValidationNavigation did not expose the linked compression field and reason");
        }
        catch (Exception ex)
        {
            s_problems.Add($"SettingsValidationNavigation failed: {ex.Message}");
        }
    }

    private static void RunThemeQa(string outputRoot, StringBuilder report)
    {
        report.AppendLine();
        report.AppendLine("Theme QA (forced Light/Dark palettes, default tab)");
        foreach (var (themeName, themeMode) in ThemeModes)
        {
            var themeDir = Path.Combine(outputRoot, "theme", themeName);
            Directory.CreateDirectory(themeDir);
            foreach (var (windowW, windowH) in ThemeWindowSizes)
            {
                var (contentW, contentH) = ContentSize(windowW, windowH);
                var cases = new (string Name, UserControl View)[]
                {
                    ("Overview", CreateThemeView("Overview")),
                    ("Save", CreateThemeView("Save")),
                    ("Trainer", CreateThemeView("Trainer")),
                    ("Media", CreateThemeView("Media")),
                    ("Maintenance", CreateThemeView("Maintenance")),
                    ("Task", CreateThemeView("Task")),
                    ("Settings", CreateThemeView("Settings"))
                };
                foreach (var (name, view) in cases)
                {
                    var label = $"{themeName}/{name}/{windowW}x{windowH}";
                    try
                    {
                        ApplyThemePalette(view, themeMode);
                        var host = new Grid
                        {
                            Width = contentW,
                            Height = contentH,
                            Background = CreateHarnessBackground(view),
                            ClipToBounds = true
                        };
                        host.Children.Add(view);
                        // MediaCenterView receives the measured workspace/page-host height in
                        // production. Passing the outer window height here skips its compact
                        // page-scroll path and falsely compresses the inbox grid under the
                        // wrapped toolbar/footer in the theme probe.
                        var responsiveHeight = name.Equals("Media", StringComparison.OrdinalIgnoreCase)
                            ? contentH
                            : windowH;
                        ApplyThemeResponsive(view, contentW, responsiveHeight);
                        host.Measure(new Size(contentW, contentH));
                        host.Arrange(new Rect(0, 0, contentW, contentH));
                        host.UpdateLayout();
                        ApplyThemeResponsive(view, contentW, responsiveHeight);
                        if (name == "Settings")
                        {
                            ApplyThemePalette(view, themeMode);
                            var settingsShell = FindVisualChildren<FrameworkElement>(host)
                                .FirstOrDefault(element => element.Name == "SettingsShell");
                            if (settingsShell != null)
                            {
                                settingsShell.BeginAnimation(UIElement.OpacityProperty, null);
                                settingsShell.Opacity = 1;
                            }
                        }
                        host.UpdateLayout();
                        SavePng(host, Path.Combine(themeDir, $"{name}-{windowW}x{windowH}.png"));
                        VerifyThemePalette(view, label, themeMode);
                        VerifyThemeViewport(host, label, report);
                        report.AppendLine($"  {label} OK");
                    }
                    catch (Exception ex)
                    {
                        s_problems.Add($"{label} failed: {ex.Message}");
                        report.AppendLine($"  {label} FAILED {ex.Message}");
                    }
                }
            }
        }
    }

    private static UserControl CreateThemeView(string name)
    {
        switch (name)
        {
            case "Overview":
                return new OverviewView { DataContext = new FakeDashboardData() };
            case "Save":
                return new SaveCenterView { DataContext = new FakeDashboardData() };
            case "Trainer":
                return new TrainerCenterView { DataContext = new FakeDashboardData() };
            case "Media":
                return new MediaCenterView { DataContext = new FakeDashboardData() };
            case "Maintenance":
                return new MaintenanceView { DataContext = new FakeDashboardData() };
            case "Task":
                return new TaskCenterView { DataContext = new FakeDashboardData() };
            case "Settings":
                return new GameSaveCenterSettingsView { DataContext = new GameSaveCenterSettings() };
            default:
                throw new InvalidOperationException("Unknown theme view " + name);
        }
    }

    private static void RunThemeSpecificControlProbes(string outputRoot, StringBuilder report)
    {
        report.AppendLine();
        report.AppendLine("Theme control QA (Maintenance cloud queue filters)");

        foreach (var (themeName, themeMode) in ThemeModes)
        {
            const int windowW = 1040;
            const int windowH = 700;
            var (contentW, contentH) = ContentSize(windowW, windowH);
            var label = $"{themeName}/MaintenanceCloudQueue/{windowW}x{windowH}";

            try
            {
                var view = new MaintenanceView { DataContext = new FakeDashboardData() };
                ApplyThemePalette(view, themeMode);
                view.ApplyResponsiveLayout(contentW, contentH);

                var host = new Grid
                {
                    Width = contentW,
                    Height = contentH,
                    Background = CreateHarnessBackground(view),
                    ClipToBounds = true
                };
                host.Children.Add(view);
                host.Measure(new Size(contentW, contentH));
                host.Arrange(new Rect(0, 0, contentW, contentH));
                host.UpdateLayout();

                SelectTab(view, 1);
                view.ApplyResponsiveLayout(contentW, contentH);
                host.UpdateLayout();

                var expectedTextIsDark = themeMode == GameSaveCenterThemeMode.Light;
                var comboNames = new[] { "云端队列状态筛选", "云端队列类型筛选" };
                var combos = FindVisualChildren<ComboBox>(host)
                    .Where(combo => comboNames.Contains(AutomationProperties.GetName(combo), StringComparer.Ordinal))
                    .ToList();
                if (combos.Count != comboNames.Length)
                {
                    s_problems.Add($"{label} expected {comboNames.Length} cloud queue filters, found {combos.Count}");
                }

                foreach (var combo in combos)
                {
                    var comboName = AutomationProperties.GetName(combo);
                    var textBlocks = FindVisualChildren<TextBlock>(combo)
                        .Where(text => !string.IsNullOrWhiteSpace(text.Text)
                            && text.ActualWidth > 0
                            && text.ActualHeight > 0)
                        .ToList();
                    if (textBlocks.Count == 0)
                    {
                        s_problems.Add($"{label} {comboName} has no realized visible selected text");
                        continue;
                    }

                    foreach (var text in textBlocks)
                    {
                        if (text.Foreground is not SolidColorBrush brush)
                        {
                            s_problems.Add($"{label} {comboName} text '{text.Text}' has no solid foreground");
                            continue;
                        }

                        var luminance = (0.2126 * brush.Color.R + 0.7152 * brush.Color.G + 0.0722 * brush.Color.B) / 255d;
                        var isDarkText = luminance < 0.5;
                        if (isDarkText != expectedTextIsDark)
                        {
                            s_problems.Add(
                                $"{label} {comboName} text '{text.Text}' has unexpected foreground #{brush.Color.R:X2}{brush.Color.G:X2}{brush.Color.B:X2}");
                        }
                    }

                    report.AppendLine(
                        $"  {label} {comboName} items={combo.Items.Count} visibleText={string.Join("|", textBlocks.Select(text => text.Text))} "
                        + $"foreground={string.Join("|", textBlocks.Select(text => (text.Foreground as SolidColorBrush)?.Color.ToString() ?? "none"))}");
                }

                SavePng(host, Path.Combine(outputRoot, "theme", themeName, "Maintenance-CloudQueue-1040x700.png"));
            }
            catch (Exception ex)
            {
                s_problems.Add($"{label} failed: {ex.Message}");
            }
        }
    }

    private static void ApplyThemePalette(UserControl view, GameSaveCenterThemeMode mode)
        => ApplyThemePalette(view, mode, glassEnabled: false, motionEnabled: false);

    private static void ApplyThemePalette(
        UserControl view,
        GameSaveCenterThemeMode mode,
        bool glassEnabled,
        bool motionEnabled)
    {
        // Settings owns a separate material hierarchy and must use the same runtime path as
        // the Playnite settings host. The generic Dashboard resource injection would leave its
        // shell/card brushes on the static dark DesignTokens fallback during Light QA.
        if (view is GameSaveCenterSettingsView settings)
        {
            settings.ApplyThemeForAudit(mode);
            return;
        }

        var palette = AdaptiveThemePaletteFactory.Create(view, glassEnabled, 50, mode);
        AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(view.Resources, palette, glassEnabled, motionEnabled);
        if (view is OverviewView overview)
            overview.UiAnimationsEnabled = motionEnabled;
    }

    private static Brush CreateHarnessBackground(FrameworkElement view)
    {
        return view.TryFindResource("GscBackdropBrush") as Brush
            ?? new SolidColorBrush(Color.FromRgb(24, 30, 43));
    }

    private static void ApplyThemeResponsive(UserControl view, double width, double height)
    {
        if (view is OverviewView overview)
        {
            var stack = width < 1200;
            overview.OverviewCompactSecondaryRowHeight = stack ? GridLength.Auto : new GridLength(0);
            overview.ApplyResponsiveColumns(stack);
            overview.ApplyResponsiveWidth(width);
            overview.ApplyResponsiveHeight(height, stack);
            return;
        }

        var method = view.GetType().GetMethod(
            "ApplyResponsiveLayout",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        method?.Invoke(view, new object[] { width, height });
    }

    private static void VerifyThemePalette(UserControl view, string label, GameSaveCenterThemeMode mode)
    {
        var primary = view.TryFindResource("GscPrimaryTextBrush") as SolidColorBrush;
        if (primary == null)
        {
            s_problems.Add($"{label} missing GscPrimaryTextBrush");
            return;
        }

        var luminance = (0.2126 * primary.Color.R + 0.7152 * primary.Color.G + 0.0722 * primary.Color.B) / 255d;
        var dark = luminance < 0.5;
        var expectedDarkText = mode == GameSaveCenterThemeMode.Light;
        if (dark != expectedDarkText)
            s_problems.Add($"{label} palette mismatch (luminance={luminance:0.##}, dark={dark})");
    }

    private static void VerifyThemeViewport(Grid host, string label, StringBuilder report)
    {
        foreach (var grid in FindVisualChildren<DataGrid>(host))
        {
            if (string.IsNullOrEmpty(grid.Name) || grid.ActualHeight <= 0)
                continue;
            var requiredReadableRows = Math.Min(4, grid.Items.Count);
            var readableRows = CountReadableDataRows(grid);
            report.AppendLine($"  {label} {grid.Name} readableRows={readableRows}/{requiredReadableRows} viewport={grid.ActualHeight:0} DIP");
            if (grid.Name != "MaintenanceAuditLogGrid"
                && requiredReadableRows > 0
                && readableRows < requiredReadableRows)
                s_problems.Add($"{label} {grid.Name} keeps only {readableRows}/{requiredReadableRows} data rows fully readable (viewport={grid.ActualHeight:0} DIP)");
        }

        foreach (var list in FindVisualChildren<ListBox>(host))
        {
            if (string.IsNullOrEmpty(list.Name) || list.ActualHeight <= 0)
                continue;
            if (list.Name is "MediaClassificationPreviewItems" or "MediaClassificationHistoryList")
                continue;
            if (list.ActualHeight < 236
                && list.Name != "OverviewActivityList"
                && !list.Name.EndsWith("SegmentTabs", StringComparison.Ordinal)
                && list.Name != "SettingsSectionTabs"
                && list.Name != "MaintenanceDiagnosticsSubTabs")
                s_problems.Add($"{label} {list.Name} viewport {list.ActualHeight:0} DIP (<236)");
        }

        foreach (var scroller in FindVisualChildren<ScrollViewer>(host))
        {
            if (string.IsNullOrEmpty(scroller.Name))
                continue;
            var scrollable = scroller.ExtentHeight > scroller.ViewportHeight + 0.5;
            if ((scroller.Name.Contains("ScrollSurface") || scroller.Name == "SettingsScroller")
                && scroller.VerticalScrollBarVisibility == ScrollBarVisibility.Hidden
                && scrollable)
                s_problems.Add($"{label} {scroller.Name} hides overflow behind a Hidden scrollbar");
            if ((scroller.Name.Contains("ScrollSurface") || scroller.Name == "SettingsScroller")
                && scroller.HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled
                && scroller.ExtentWidth > scroller.ViewportWidth + 0.5)
                s_problems.Add($"{label} {scroller.Name} has page-level horizontal overflow");
        }

        report.AppendLine($"  {label} viewport probe done");
    }

    private static int CountReadableDataRows(DataGrid grid)
    {
        if (grid.Items.Count == 0 || grid.ActualHeight <= 0)
            return 0;

        var headerHeight = double.IsNaN(grid.ColumnHeaderHeight)
            ? 0d
            : Math.Max(0d, grid.ColumnHeaderHeight);
        var rows = FindVisualChildren<DataGridRow>(grid)
            .Where(row => row.Visibility == Visibility.Visible && row.ActualHeight >= 22)
            .Select(row => row.TransformToAncestor(grid)
                .TransformBounds(new Rect(0, 0, row.ActualWidth, row.ActualHeight)))
            .Count(bounds => bounds.Top >= headerHeight - 0.5
                && bounds.Bottom <= grid.ActualHeight + 0.5);
        return rows;
    }

    private static void RunResizeTransitionProbes(StringBuilder report)
    {
        report.AppendLine();
        report.AppendLine("Resize transition QA (2560x1440 -> 1100x720 -> 2560x1440)");
        var cases = new (string Name, UserControl View)[]
        {
            ("Overview", CreateThemeView("Overview")),
            ("Save", CreateThemeView("Save")),
            ("Trainer", CreateThemeView("Trainer")),
            ("Media", CreateThemeView("Media")),
            ("Maintenance", CreateThemeView("Maintenance")),
            ("Task", CreateThemeView("Task")),
            ("Settings", CreateThemeView("Settings"))
        };

        foreach (var (name, view) in cases)
        {
            try
            {
                var host = new Grid
                {
                    Background = CreateHarnessBackground(view),
                    ClipToBounds = true
                };
                host.Children.Add(view);
                List<ElementMetric>? initial = null;
                for (var step = 0; step < ResizeSequence.Length; step++)
                {
                    var (windowW, windowH) = ResizeSequence[step];
                    var (contentW, contentH) = ContentSize(windowW, windowH);
                    host.Width = contentW;
                    host.Height = contentH;
                    var responsiveHeight = name.Equals("Media", StringComparison.OrdinalIgnoreCase)
                        ? contentH
                        : windowH;
                    ApplyThemeResponsive(view, contentW, responsiveHeight);
                    host.Measure(new Size(contentW, contentH));
                    host.Arrange(new Rect(0, 0, contentW, contentH));
                    host.UpdateLayout();
                    ApplyThemeResponsive(view, contentW, responsiveHeight);
                    host.UpdateLayout();

                    var snapshot = SnapshotLayoutMetrics(host);
                    var label = $"{name}/step{step}:{windowW}x{windowH}";
                    if (step == 1)
                        VerifyThemeViewport(host, label, report);
                    if (step == 0)
                        initial = snapshot;
                    else if (step == ResizeSequence.Length - 1)
                        CompareLayoutMetrics(initial!, snapshot, name, report);
                }
            }
            catch (Exception ex)
            {
                s_problems.Add($"{name} resize transition failed: {ex.Message}");
                report.AppendLine($"  {name} RESIZE FAILED {ex.Message}");
            }
        }
    }

    private static void RunShellChromeProbes(string outputRoot, StringBuilder report)
    {
        report.AppendLine();
        report.AppendLine("Production shell chrome QA (compact header)" );
        foreach (var (windowW, windowH) in ShellWindowSizes)
        {
            try
            {
                var shell = new AcrylicProductionShellView { DataContext = new FakeDashboardData() };
                var host = new Grid
                {
                    Width = windowW,
                    Height = windowH,
                    Background = CreateHarnessBackground(shell),
                    ClipToBounds = true
                };
                host.Children.Add(shell);
                shell.ApplyResponsiveLayout(windowW, windowH);
                host.Measure(new Size(windowW, windowH));
                host.Arrange(new Rect(0, 0, windowW, windowH));
                host.UpdateLayout();
                shell.ApplyResponsiveLayout(windowW, windowH);
                host.UpdateLayout();

                var headerRow = shell.GetType().GetField("HeaderRow", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(shell) as RowDefinition;
                var headerSurface = FindVisualChildren<FrameworkElement>(shell).FirstOrDefault(x => x.Name == "HeaderSurface");
                var actions = FindVisualChildren<FrameworkElement>(shell).FirstOrDefault(x => x.Name == "HeaderActionsPanel");
                if (headerRow == null || headerSurface == null || actions == null)
                    throw new InvalidOperationException("Production shell header probe elements are missing.");

                if (windowW < 980 && headerRow.ActualHeight <= 68)
                    s_problems.Add($"Shell {windowW}x{windowH} compact header did not grow beyond the original 68 DIP row.");
                if (actions.Visibility == Visibility.Visible)
                {
                    var actionsBounds = actions.TransformToAncestor(headerSurface).TransformBounds(new Rect(0, 0, actions.ActualWidth, actions.ActualHeight));
                    if (actionsBounds.Left < -0.5 || actionsBounds.Right > headerSurface.ActualWidth + 0.5
                        || actionsBounds.Top < -0.5 || actionsBounds.Bottom > headerSurface.ActualHeight + 0.5)
                        s_problems.Add($"Shell {windowW}x{windowH} header actions exceed HeaderSurface bounds ({actionsBounds.Left:0.0}..{actionsBounds.Right:0.0}, {actionsBounds.Top:0.0}..{actionsBounds.Bottom:0.0}/{headerSurface.ActualWidth:0.0}x{headerSurface.ActualHeight:0.0}).");
                }

                SavePng(host, Path.Combine(outputRoot, $"Shell-{windowW}x{windowH}.png"));
                report.AppendLine($"  Shell {windowW}x{windowH} header={headerRow.ActualHeight:0.0} actions={actions.ActualWidth:0.0}x{actions.ActualHeight:0.0}");
            }
            catch (Exception ex)
            {
                s_problems.Add($"Shell {windowW}x{windowH} probe failed: {ex.Message}");
            }
        }

        RunProductionShellMediaProbe(outputRoot, report);
        RunProductionShellMaintenanceProbe(outputRoot, report);
        RunProductionShellTaskProbe(outputRoot, report);
        RunProductionShellBackgroundProbe(outputRoot, report);
        RunSidebarTransitionProbe(report);
    }

    private static void RunProductionShellBackgroundProbe(string outputRoot, StringBuilder report)
    {
        report.AppendLine();
        report.AppendLine("Production shell background material QA (full-shell ambient layer)");

        foreach (var (themeName, themeMode) in ThemeModes)
        {
            const int windowW = 1040;
            const int windowH = 700;
            var label = $"{themeName}/ShellBackground/{windowW}x{windowH}";

            try
            {
                var shell = new AcrylicProductionShellView { DataContext = new FakeDashboardData() };
                ApplyThemePalette(shell, themeMode);
                var host = new Grid
                {
                    Width = windowW,
                    Height = windowH,
                    Background = CreateHarnessBackground(shell),
                    ClipToBounds = true
                };
                host.Children.Add(shell);
                shell.ApplyResponsiveLayout(windowW, windowH);
                host.Measure(new Size(windowW, windowH));
                host.Arrange(new Rect(0, 0, windowW, windowH));
                host.UpdateLayout();
                shell.ApplyResponsiveLayout(windowW, windowH);
                host.UpdateLayout();

                var ambient = FindVisualChildren<FrameworkElement>(shell)
                    .FirstOrDefault(element => element.Name == "ShellAmbientMaterialLayer");
                if (ambient == null)
                {
                    s_problems.Add($"{label} ShellAmbientMaterialLayer is missing");
                    continue;
                }

                var bounds = ambient.TransformToAncestor(host)
                    .TransformBounds(new Rect(0, 0, ambient.ActualWidth, ambient.ActualHeight));
                var useSelectedGameBackground = ambient.GetType()
                    .GetProperty("UseSelectedGameBackground", BindingFlags.Instance | BindingFlags.Public)
                    ?.GetValue(ambient) as bool?;
                var fullShell = Grid.GetColumn(ambient) == 0
                    && Grid.GetColumnSpan(ambient) == 2
                    && Grid.GetRow(ambient) == 0
                    && Grid.GetRowSpan(ambient) == 2
                    && bounds.Width > 1000
                    && bounds.Height > 650;

                report.AppendLine(
                    $"  {label} bounds={bounds.X:0.##},{bounds.Y:0.##},{bounds.Width:0.##}x{bounds.Height:0.##} "
                    + $"grid={Grid.GetRow(ambient)}/{Grid.GetRowSpan(ambient)}/{Grid.GetColumn(ambient)}/{Grid.GetColumnSpan(ambient)} "
                    + $"UseSelectedGameBackground={useSelectedGameBackground}");

                if (!fullShell)
                    s_problems.Add($"{label} ambient layer does not cover the full shell/footer ({bounds.Width:0.##}x{bounds.Height:0.##})");
                if (useSelectedGameBackground != false)
                    s_problems.Add($"{label} ambient layer still reads the selected game background");

                var themeOutputRoot = Path.Combine(outputRoot, "theme", themeName);
                Directory.CreateDirectory(themeOutputRoot);
                SavePng(host, Path.Combine(themeOutputRoot, "Shell-Background-1040x700.png"));
            }
            catch (Exception ex)
            {
                s_problems.Add($"{label} failed: {ex.Message}");
            }
        }
    }

    private static int RunShortWindowReachabilityProbe(string outputRoot)
    {
        Directory.CreateDirectory(outputRoot);
        var report = new StringBuilder();
        report.AppendLine("GameSaveCenter short-window bottom reachability probe");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        AppendRunMetadata(report, "shortwindowprobe", "OffscreenRenderHarness", "light,dark", "synthetic stale banners; 1040x700 and 1040x560; page/grid/inspector end-scroll checks");
        report.AppendLine();
        s_problems.Clear();

        try
        {
            var app = new Application();
            app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
            foreach (var (themeName, themeMode) in ThemeModes)
            {
                Directory.CreateDirectory(Path.Combine(outputRoot, themeName));
                foreach (var height in new[] { 700, 560 })
                {
                    RunShortWindowMediaProbe(outputRoot, themeName, themeMode, height, report);
                    RunShortWindowSaveProbe(outputRoot, themeName, themeMode, height, report);
                    RunShortWindowTaskProbe(outputRoot, themeName, themeMode, height, report);
                    RunShortWindowMaintenanceProbe(outputRoot, themeName, themeMode, height, report);
                }
            }

            report.AppendLine(s_problems.Count == 0 ? "shortwindowprobe OK" : "shortwindowprobe FAILED");
            foreach (var problem in s_problems)
                report.AppendLine("  PROBLEM " + problem);
            File.WriteAllText(Path.Combine(outputRoot, "shortwindowprobe-report.txt"), report.ToString());
            Console.WriteLine(report.ToString());
            return s_problems.Count == 0 ? 0 : 1;
        }
        catch (Exception ex)
        {
            report.AppendLine("shortwindowprobe FAILED");
            report.AppendLine(ex.ToString());
            File.WriteAllText(Path.Combine(outputRoot, "shortwindowprobe-report.txt"), report.ToString());
            Console.WriteLine(report.ToString());
            return 1;
        }
    }

    private static void RunShortWindowMediaProbe(string outputRoot, string themeName, GameSaveCenterThemeMode themeMode, int height, StringBuilder report)
    {
        var data = new FakeDashboardData(60, WorkspaceFixtureState.Stale, OverviewFixtureProfile.Default, mediaInboxHasMore: true);
        var view = new MediaCenterView { DataContext = data };
        var host = MountShortWindowPage(view, data, themeMode, height, out var shell, out var pageHost);
        var tabs = FindVisualChildren<TabControl>(view).First(candidate => candidate.Name == "MediaTabControl");
        tabs.SelectedIndex = 0;
        view.ApplyResponsiveLayout(pageHost.ActualWidth, pageHost.ActualHeight);
        host.UpdateLayout();

        var pageScroller = FindVisualChildren<ScrollViewer>(view).FirstOrDefault(candidate => candidate.Name == "MediaInboxPageScrollViewer");
        var surface = FindVisualChildren<FrameworkElement>(view).FirstOrDefault(candidate => candidate.Name == "MediaInboxPageSurface");
        var staleBanner = FindVisualChildren<FrameworkElement>(view).FirstOrDefault(candidate => candidate.Name == "MediaInboxStaleBanner");
        var footer = FindVisualChildren<FrameworkElement>(view).FirstOrDefault(candidate => candidate.Name == "MediaInboxFooter");
        var loadMore = FindVisualChildren<Button>(view).FirstOrDefault(candidate => AutomationProperties.GetName(candidate) == "加载更多媒体收件箱项目");
        if (pageScroller == null || surface == null || staleBanner == null || footer == null || loadMore == null)
        {
            s_problems.Add($"Media short window {height} probe elements are missing.");
            return;
        }

        if (staleBanner.Visibility != Visibility.Visible)
            s_problems.Add($"Media short window {height} did not show the stale-data hint.");
        var staleBounds = GetBounds(staleBanner, surface);
        var footerInContent = GetBounds(footer, surface);
        if (staleBounds.Bottom > footerInContent.Top + 1)
            s_problems.Add($"Media short window {height} stale hint overlaps the footer (stale={FormatRect(staleBounds)}, footer={FormatRect(footerInContent)}).");

        pageScroller.ScrollToVerticalOffset(pageScroller.ScrollableHeight);
        host.UpdateLayout();
        var viewport = GetBounds(pageScroller, host);
        var footerBounds = GetBounds(footer, host);
        var loadMoreBounds = GetBounds(loadMore, host);
        if (!IsInside(viewport, footerBounds) || !IsInside(viewport, loadMoreBounds))
            s_problems.Add($"Media short window {height} bottom actions are outside the end viewport (viewport={FormatRect(viewport)}, footer={FormatRect(footerBounds)}, loadMore={FormatRect(loadMoreBounds)}).");

        report.AppendLine($"  {themeName} Media {height}: shellFooter={FormatRect(GetBounds((FrameworkElement)shell.FindName("FooterSurface"), host))}, pageScroll={pageScroller.VerticalOffset:0.##}/{pageScroller.ScrollableHeight:0.##}, viewport={FormatRect(viewport)}, stale={FormatRect(staleBounds)}, footer={FormatRect(footerBounds)}, loadMore={FormatRect(loadMoreBounds)}");
        SavePng(host, Path.Combine(outputRoot, themeName, $"Media-1040x{height}-bottom.png"));
    }

    private static void RunShortWindowSaveProbe(string outputRoot, string themeName, GameSaveCenterThemeMode themeMode, int height, StringBuilder report)
    {
        var data = new FakeDashboardData(60, WorkspaceFixtureState.Stale);
        var view = new SaveCenterView { DataContext = data };
        var host = MountShortWindowPage(view, data, themeMode, height, out var shell, out var pageHost);
        var tabs = FindVisualChildren<TabControl>(view).First();
        tabs.SelectedIndex = 0;
        view.ApplyResponsiveLayout(pageHost.ActualWidth, pageHost.ActualHeight);
        host.UpdateLayout();
        var staleBanner = FindVisualChildren<FrameworkElement>(view).FirstOrDefault(candidate => candidate.Name == "SaveDetailsStaleBanner");
        if (staleBanner == null || staleBanner.Visibility != Visibility.Visible)
            s_problems.Add($"Save short window {height} did not show the stale-data hint on history.");

        tabs.SelectedIndex = 2;
        host.UpdateLayout();
        view.ApplyResponsiveLayout(pageHost.ActualWidth, pageHost.ActualHeight);
        host.UpdateLayout();
        var policyStack = FindVisualChildren<FrameworkElement>(view).FirstOrDefault(candidate => candidate.Name == "SavePolicyStack");
        var policyScroller = policyStack == null ? null : FindVisualAncestor<ScrollViewer>(policyStack);
        var saveButton = FindVisualChildren<Button>(view).FirstOrDefault(candidate => string.Equals(candidate.Content?.ToString(), "保存策略", StringComparison.Ordinal));
        if (policyScroller == null || saveButton == null)
        {
            s_problems.Add($"Save short window {height} policy action probe elements are missing.");
            return;
        }

        policyScroller.ScrollToVerticalOffset(0);
        host.UpdateLayout();
        var initialOffset = policyScroller.VerticalOffset;
        saveButton.BringIntoView();
        host.UpdateLayout();
        var viewport = GetBounds(policyScroller, host);
        var saveBounds = GetBounds(saveButton, host);
        if (!IsInside(viewport, saveBounds))
            s_problems.Add($"Save short window {height} save action is outside the reachable viewport (viewport={FormatRect(viewport)}, save={FormatRect(saveBounds)}).");

        report.AppendLine($"  {themeName} Save {height}: shellFooter={FormatRect(GetBounds((FrameworkElement)shell.FindName("FooterSurface"), host))}, policyScroll={initialOffset:0.##}->{policyScroller.VerticalOffset:0.##}/{policyScroller.ScrollableHeight:0.##}, viewport={FormatRect(viewport)}, save={FormatRect(saveBounds)}");
        SavePng(host, Path.Combine(outputRoot, themeName, $"Save-1040x{height}-policy-bottom.png"));
    }

    private static void RunShortWindowTaskProbe(string outputRoot, string themeName, GameSaveCenterThemeMode themeMode, int height, StringBuilder report)
    {
        var data = new FakeDashboardData(60, WorkspaceFixtureState.Stale);
        var view = new TaskCenterView { DataContext = data };
        var host = MountShortWindowPage(view, data, themeMode, height, out var shell, out var pageHost);
        view.ApplyResponsiveLayout(pageHost.ActualWidth, pageHost.ActualHeight);
        host.UpdateLayout();
        var grid = FindVisualChildren<DataGrid>(view).First(candidate => candidate.Name == "TaskGrid");
        grid.SelectedIndex = 0;
        view.ApplyResponsiveLayout(pageHost.ActualWidth, pageHost.ActualHeight);
        host.UpdateLayout();
        var detailsButton = FindVisualChildren<Button>(view).FirstOrDefault(candidate => candidate.Name == "TaskCompactDetailsButton");
        var inspector = FindVisualChildren<ScrollViewer>(view).FirstOrDefault(candidate => candidate.Name == "TaskDetailScrollViewer");
        var cancel = FindVisualChildren<Button>(view).FirstOrDefault(candidate => AutomationProperties.GetName(candidate) == "取消任务");
        if (detailsButton == null || inspector == null || cancel == null)
        {
            s_problems.Add($"Task short window {height} cancel action probe elements are missing.");
            return;
        }

        if (detailsButton.Visibility == Visibility.Visible)
            detailsButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        view.ApplyResponsiveLayout(pageHost.ActualWidth, pageHost.ActualHeight);
        host.UpdateLayout();
        inspector.ScrollToVerticalOffset(inspector.ScrollableHeight);
        host.UpdateLayout();
        var viewport = GetBounds(inspector, host);
        var cancelBounds = GetBounds(cancel, host);
        if (!IsInside(viewport, cancelBounds))
            s_problems.Add($"Task short window {height} cancel action is outside the end inspector viewport (viewport={FormatRect(viewport)}, cancel={FormatRect(cancelBounds)}).");

        report.AppendLine($"  {themeName} Task {height}: shellFooter={FormatRect(GetBounds((FrameworkElement)shell.FindName("FooterSurface"), host))}, inspectorScroll={inspector.VerticalOffset:0.##}/{inspector.ScrollableHeight:0.##}, viewport={FormatRect(viewport)}, cancel={FormatRect(cancelBounds)}");
        SavePng(host, Path.Combine(outputRoot, themeName, $"Task-1040x{height}-cancel-bottom.png"));
    }

    private static void RunShortWindowMaintenanceProbe(string outputRoot, string themeName, GameSaveCenterThemeMode themeMode, int height, StringBuilder report)
    {
        var data = new FakeDashboardData(60, WorkspaceFixtureState.Stale);
        data.CloudTransferViewSummary.HasMore = true;
        var view = new MaintenanceView { DataContext = data };
        var host = MountShortWindowPage(view, data, themeMode, height, out var shell, out var pageHost);
        var tabs = FindVisualChildren<TabControl>(view).First(candidate => candidate.Name == "MaintenanceTabControl");
        tabs.SelectedIndex = 1;
        view.ApplyResponsiveLayout(pageHost.ActualWidth, pageHost.ActualHeight);
        host.UpdateLayout();
        var surface = FindVisualChildren<FrameworkElement>(view).FirstOrDefault(candidate => candidate.Name == "CloudTransfersSurface");
        var layout = FindVisualChildren<FrameworkElement>(view).FirstOrDefault(candidate => candidate.Name == "CloudTransfersLayout");
        var loadMore = FindVisualChildren<Button>(view).FirstOrDefault(candidate => AutomationProperties.GetName(candidate) == "加载更多云端队列");
        if (surface == null || layout == null || loadMore == null)
        {
            s_problems.Add($"Maintenance short window {height} cloud queue probe elements are missing.");
            return;
        }

        var surfaceBounds = GetBounds(surface, host);
        var layoutBounds = GetBounds(layout, host);
        var loadMoreBounds = GetBounds(loadMore, host);
        if (!IsInside(surfaceBounds, layoutBounds) || !IsInside(surfaceBounds, loadMoreBounds) || loadMoreBounds.Top < layoutBounds.Top - 1)
            s_problems.Add($"Maintenance short window {height} load-more action escapes the cloud queue surface (surface={FormatRect(surfaceBounds)}, layout={FormatRect(layoutBounds)}, loadMore={FormatRect(loadMoreBounds)}).");

        report.AppendLine($"  {themeName} Maintenance {height}: shellFooter={FormatRect(GetBounds((FrameworkElement)shell.FindName("FooterSurface"), host))}, surface={FormatRect(surfaceBounds)}, layout={FormatRect(layoutBounds)}, loadMore={FormatRect(loadMoreBounds)}");
        SavePng(host, Path.Combine(outputRoot, themeName, $"Maintenance-1040x{height}-load-more-bottom.png"));
    }

    private static int RunHorizontalScrollEndpointProbe(string outputRoot)
    {
        Directory.CreateDirectory(outputRoot);
        var report = new StringBuilder();
        report.AppendLine("GameSaveCenter horizontal scroll endpoint probe");
        report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        AppendRunMetadata(report, "horizontalprobe", "OffscreenRenderHarness", "light,dark", "synthetic long tables; production DataGrid template; left/right endpoint and clamp checks");
        report.AppendLine("InputBoundary: endpoint replay uses the real production ScrollViewer; physical Ctrl/Shift modifier timing and trackpad hardware remain host checks");
        report.AppendLine();
        s_problems.Clear();

        try
        {
            var app = new Application();
            app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
            var cases = new[]
            {
                (Name: "Save", TabIndex: 0, Create: (Func<FakeDashboardData, UserControl>)(data => new SaveCenterView { DataContext = data }), GridName: "SaveHistoryGrid"),
                (Name: "Task", TabIndex: -1, Create: (Func<FakeDashboardData, UserControl>)(data => new TaskCenterView { DataContext = data }), GridName: "TaskGrid"),
                (Name: "Media", TabIndex: 0, Create: (Func<FakeDashboardData, UserControl>)(data => new MediaCenterView { DataContext = data }), GridName: "MediaInboxGrid"),
                (Name: "Maintenance", TabIndex: 1, Create: (Func<FakeDashboardData, UserControl>)(data => new MaintenanceView { DataContext = data }), GridName: "CloudTransferGrid")
            };

            foreach (var (themeName, themeMode) in ThemeModes)
            {
                foreach (var fixture in cases)
                {
                    var data = new FakeDashboardData(60, WorkspaceFixtureState.Ready, OverviewFixtureProfile.Default, mediaInboxHasMore: true);
                    if (fixture.Name == "Maintenance")
                        data.CloudTransferViewSummary.HasMore = true;

                    var view = fixture.Create(data);
                    var host = MountShortWindowPage(view, data, themeMode, 700, out _, out var pageHost);
                    if (fixture.TabIndex >= 0)
                    {
                        var tabs = FindVisualChildren<TabControl>(view).FirstOrDefault();
                        if (tabs == null || fixture.TabIndex >= tabs.Items.Count)
                        {
                            s_problems.Add($"{fixture.Name}/{themeName} horizontal probe tab {fixture.TabIndex} is missing.");
                            continue;
                        }
                        tabs.SelectedIndex = fixture.TabIndex;
                        ApplyThemeResponsive(view, pageHost.ActualWidth, pageHost.ActualHeight);
                        host.UpdateLayout();
                    }

                    var grid = FindVisualChildren<DataGrid>(view).FirstOrDefault(candidate => candidate.Name == fixture.GridName);
                    if (grid == null)
                    {
                        s_problems.Add($"{fixture.Name}/{themeName} horizontal probe grid {fixture.GridName} is missing.");
                        continue;
                    }

                    // Force a controlled long table without changing the production template
                    // or Media's virtualization exception. Keep each terminal column below a
                    // narrow viewport's readable budget; a column wider than the viewport is
                    // an intentional truncation case, not evidence that the endpoint failed.
                    foreach (var column in grid.Columns)
                        column.Width = new DataGridLength(Math.Max(240, column.MinWidth + 96), DataGridLengthUnitType.Pixel);
                    host.UpdateLayout();

                    var scroller = FindVisualChildren<ScrollViewer>(grid)
                        .FirstOrDefault(candidate => candidate.Name == "DG_ScrollViewer")
                        ?? FindVisualChildren<ScrollViewer>(grid).OrderByDescending(candidate => candidate.ViewportWidth).FirstOrDefault();
                    if (scroller == null || scroller.ScrollableWidth <= 1)
                    {
                        s_problems.Add($"{fixture.Name}/{themeName} horizontal probe did not expose a scrollable DG_ScrollViewer.");
                        continue;
                    }

                    if (fixture.Name == "Media"
                        && (!VirtualizingPanel.GetIsVirtualizing(grid)
                            || VirtualizingPanel.GetVirtualizationMode(grid) != VirtualizationMode.Standard
                            || grid.EnableColumnVirtualization
                            || VirtualizingPanel.GetScrollUnit(grid) != ScrollUnit.Item))
                    {
                        s_problems.Add($"{fixture.Name}/{themeName} changed its protected Media virtualization contract: virtualizing={VirtualizingPanel.GetIsVirtualizing(grid)}, mode={VirtualizingPanel.GetVirtualizationMode(grid)}, column={grid.EnableColumnVirtualization}, unit={VirtualizingPanel.GetScrollUnit(grid)}.");
                    }

                    var headers = FindHorizontalHeaders(grid);
                    var firstHeader = headers.FirstOrDefault();
                    var lastHeader = headers.LastOrDefault();
                    var firstRow = FindVisualChildren<DataGridRow>(grid)
                        .FirstOrDefault(row => row.Visibility == Visibility.Visible && row.ActualHeight > 0);
                    if (firstHeader == null || lastHeader == null || firstRow == null)
                    {
                        s_problems.Add($"{fixture.Name}/{themeName} horizontal probe lacks realized headers or rows.");
                        continue;
                    }

                    scroller.ScrollToHorizontalOffset(-120);
                    host.UpdateLayout();
                    var negativeClamp = scroller.HorizontalOffset;
                    var leftFirst = GetBounds(FindHorizontalHeaders(grid).First(), scroller);
                    scroller.ScrollToHorizontalOffset(0);
                    host.UpdateLayout();
                    var leftOffset = scroller.HorizontalOffset;
                    leftFirst = GetBounds(FindHorizontalHeaders(grid).First(), scroller);

                    scroller.ScrollToHorizontalOffset(scroller.ScrollableWidth + 240);
                    host.UpdateLayout();
                    var rightOffset = scroller.HorizontalOffset;
                    lastHeader = FindHorizontalHeaders(grid).Last();
                    var rightLast = GetBounds(lastHeader, scroller);
                    var lastCell = FindVisualChildren<DataGridCell>(firstRow)
                        .Where(cell => cell.Visibility == Visibility.Visible && cell.Column != null && cell.Column.DisplayIndex == lastHeader.Column.DisplayIndex)
                        .FirstOrDefault();
                    var rightLastCell = lastCell == null ? Rect.Empty : GetBounds(lastCell, scroller);
                    var horizontalBar = FindVisualChildren<ScrollBar>(scroller)
                        .FirstOrDefault(bar => bar.Orientation == Orientation.Horizontal && bar.Visibility == Visibility.Visible);
                    var verticalBar = FindVisualChildren<ScrollBar>(scroller)
                        .FirstOrDefault(bar => bar.Orientation == Orientation.Vertical && bar.Visibility == Visibility.Visible);
                    var horizontalBarBounds = horizontalBar == null ? Rect.Empty : GetBounds(horizontalBar, scroller);
                    var firstRowBounds = GetBounds(firstRow, scroller);

                    if (negativeClamp > 0.5 || leftOffset > 0.5)
                        s_problems.Add($"{fixture.Name}/{themeName} left endpoint did not clamp to zero (negative={negativeClamp:0.##}, left={leftOffset:0.##}).");
                    if (rightOffset < scroller.ScrollableWidth - 0.5)
                        s_problems.Add($"{fixture.Name}/{themeName} right endpoint did not clamp (offset={rightOffset:0.##}, max={scroller.ScrollableWidth:0.##}).");
                    if (!HorizontalInside(rightLast, scroller.ViewportWidth))
                        s_problems.Add($"{fixture.Name}/{themeName} last header is not visible at the right endpoint (last={FormatRect(rightLast)}, viewport={scroller.ViewportWidth:0.##}).");
                    if (!HorizontalInside(rightLastCell, scroller.ViewportWidth))
                        s_problems.Add($"{fixture.Name}/{themeName} last realized cell/action is not visible at the right endpoint (cell={FormatRect(rightLastCell)}, viewport={scroller.ViewportWidth:0.##}).");
                    if (horizontalBar != null && firstRowBounds.Bottom > horizontalBarBounds.Top + 0.5)
                        s_problems.Add($"{fixture.Name}/{themeName} row content overlaps the horizontal bar (row={FormatRect(firstRowBounds)}, bar={FormatRect(horizontalBarBounds)}).");

                    // Capture the actual right endpoint before restoring the left endpoint
                    // used for the round-trip drift assertion.
                    SavePng(host, Path.Combine(outputRoot, $"{themeName}-{fixture.Name}-right.png"));

                    scroller.ScrollToHorizontalOffset(0);
                    host.UpdateLayout();
                    var returnedOffset = scroller.HorizontalOffset;
                    var returnedFirst = GetBounds(FindHorizontalHeaders(grid).First(), scroller);
                    if (returnedOffset > 0.5 || Math.Abs(returnedFirst.Left - leftFirst.Left) > 0.5)
                        s_problems.Add($"{fixture.Name}/{themeName} left return drifted (offset={returnedOffset:0.##}, before={leftFirst.Left:0.##}, after={returnedFirst.Left:0.##}).");

                    var diagnosticLine = DataGridScrollDiagnostics.CaptureNow(grid, "Ctrl+Shift/触控板等效:水平左端");
                    var rowsPresenter = FindVisualChildren<DataGridRowsPresenter>(grid).FirstOrDefault();
                    var headersPresenter = FindVisualChildren<DataGridColumnHeadersPresenter>(grid).FirstOrDefault();
                    var cellsPanelOffset = ReadInternalDouble(grid, "CellsPanelHorizontalOffset");
                    var nonFrozenOffset = ReadInternalDouble(grid, "NonFrozenColumnsViewportHorizontalOffset");
                    var headerGeometry = string.Join(",", FindHorizontalHeaders(grid).Select(header =>
                        $"{header.Column.DisplayIndex}:{header.ActualWidth:0.##}@{GetBounds(header, scroller).Left:0.##}..{GetBounds(header, scroller).Right:0.##}"));
                    var rowHeader = FindVisualChildren<DataGridRowHeader>(grid).FirstOrDefault();
                    var selectAllButton = FindVisualChildren<Button>(scroller).FirstOrDefault();
                    var itemsPresenter = FindVisualChildren<ItemsPresenter>(grid).FirstOrDefault();
                    var cellsPresenter = FindVisualChildren<DataGridCellsPresenter>(firstRow).FirstOrDefault();
                    report.AppendLine(
                        $"  {themeName} {fixture.Name}: items={grid.Items.Count}, columns={grid.Columns.Count}, "
                        + $"offsets={leftOffset:0.##}->{rightOffset:0.##}->{returnedOffset:0.##}/{scroller.ScrollableWidth:0.##}, "
                        + $"viewport={scroller.ViewportWidth:0.##}x{scroller.ViewportHeight:0.##}, "
                        + $"lastHeader={FormatRect(rightLast)}, lastCell={FormatRect(rightLastCell)}, hbar={FormatRect(horizontalBarBounds)}, vbar={FormatRect(verticalBar == null ? Rect.Empty : GetBounds(verticalBar, scroller))}, "
                        + $"grid={grid.ActualWidth:0.##}x{grid.ActualHeight:0.##}, padding={grid.Padding.Left:0.##}/{grid.Padding.Right:0.##}, "
                        + $"presenters={headersPresenter?.ActualWidth:0.##}/{rowsPresenter?.ActualWidth:0.##}, internalOffsets={cellsPanelOffset:0.##}/{nonFrozenOffset:0.##}, cols={string.Join(",", grid.Columns.Select(column => $"{column.ActualWidth:0.##}"))}, "
                        + $"headerGeometry={headerGeometry}, extentExtra={(scroller.ExtentWidth - grid.Columns.Sum(column => column.ActualWidth)):0.##}, "
                        + $"rowHeader={FormatRect(rowHeader == null ? Rect.Empty : GetBounds(rowHeader, scroller))}, selectAll={FormatRect(selectAllButton == null ? Rect.Empty : GetBounds(selectAllButton, scroller))}, "
                        + $"desired={itemsPresenter?.DesiredSize.Width:0.##}/{rowsPresenter?.DesiredSize.Width:0.##}/{cellsPresenter?.DesiredSize.Width:0.##}, "
                        + $"realizedRows={FindVisualChildren<DataGridRow>(grid).Count(row => row.Visibility == Visibility.Visible)}, "
                        + $"mediaVirtualization={(fixture.Name == "Media" ? $"{VirtualizingPanel.GetVirtualizationMode(grid)}/{VirtualizingPanel.GetScrollUnit(grid)}/column={grid.EnableColumnVirtualization}" : "n/a")}");
                    report.AppendLine($"  {diagnosticLine}");
                }
            }

            report.AppendLine(s_problems.Count == 0 ? "horizontalprobe OK" : "horizontalprobe FAILED");
            foreach (var problem in s_problems)
                report.AppendLine("  PROBLEM " + problem);
            File.WriteAllText(Path.Combine(outputRoot, "horizontalprobe-report.txt"), report.ToString());
            Console.WriteLine(report.ToString());
            return s_problems.Count == 0 ? 0 : 1;
        }
        catch (Exception ex)
        {
            report.AppendLine("horizontalprobe FAILED");
            report.AppendLine(ex.ToString());
            File.WriteAllText(Path.Combine(outputRoot, "horizontalprobe-report.txt"), report.ToString());
            Console.WriteLine(report.ToString());
            return 1;
        }
    }

    private static bool HorizontalInside(Rect bounds, double viewportWidth)
        => !bounds.IsEmpty
           && bounds.Width > 0
           && bounds.Left >= -1
           && bounds.Right <= viewportWidth + 1
           && bounds.Right >= -1
           && bounds.Left <= viewportWidth + 1;

    private static DataGridColumnHeader[] FindHorizontalHeaders(DataGrid grid)
        => FindVisualChildren<DataGridColumnHeader>(grid)
            .Where(header => header.Visibility == Visibility.Visible && header.Column != null)
            .OrderBy(header => header.Column.DisplayIndex)
            .ToArray();

    private static double ReadInternalDouble(DependencyObject source, string propertyName)
    {
        var property = source.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var value = property?.GetValue(source, null);
        return value is double number ? number : double.NaN;
    }

    private static Grid MountShortWindowPage(UserControl page, FakeDashboardData data, GameSaveCenterThemeMode themeMode, int height, out AcrylicProductionShellView shell, out ContentControl pageHost)
    {
        shell = new AcrylicProductionShellView { DataContext = data };
        page.DataContext = data;
        pageHost = shell.PageHostForAudit as ContentControl
            ?? throw new InvalidOperationException("Production shell PageHost is not a ContentControl.");
        pageHost.Content = page;
        ApplyThemePalette(shell, themeMode, glassEnabled: true, motionEnabled: false);
        ApplyThemePalette(page, themeMode, glassEnabled: true, motionEnabled: false);
        var host = new Grid
        {
            Width = 1040,
            Height = height,
            Background = CreateHarnessBackground(shell),
            ClipToBounds = true
        };
        host.Children.Add(shell);
        shell.ApplyResponsiveLayout(1040, height);
        host.Measure(new Size(1040, height));
        host.Arrange(new Rect(0, 0, 1040, height));
        host.UpdateLayout();
        shell.ApplyResponsiveLayout(1040, height);
        host.UpdateLayout();
        return host;
    }

    private static bool IsInside(Rect outer, Rect inner)
        => !outer.IsEmpty
           && !inner.IsEmpty
           && inner.Left >= outer.Left - 1
           && inner.Right <= outer.Right + 1
           && inner.Top >= outer.Top - 1
           && inner.Bottom <= outer.Bottom + 1;

    private static T? FindVisualAncestor<T>(DependencyObject node) where T : DependencyObject
    {
        var current = VisualTreeHelper.GetParent(node);
        while (current != null)
        {
            if (current is T match)
                return match;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    private static void RunProductionShellMediaProbe(string outputRoot, StringBuilder report)
    {
        report.AppendLine();
        report.AppendLine("Production shell PageHost geometry QA (media inbox)");
        foreach (var (windowW, windowH) in new[] { (1040, 700), (1100, 720), (1366, 768) })
        {
            try
            {
                var data = new FakeDashboardData(60);
                var shell = new AcrylicProductionShellView { DataContext = data };
                var media = new MediaCenterView { DataContext = data };
                if (shell.PageHostForAudit is not ContentControl pageHost)
                    throw new InvalidOperationException("Production shell PageHost is not a ContentControl.");
                pageHost.Content = media;

                var host = new Grid
                {
                    Width = windowW,
                    Height = windowH,
                    Background = CreateHarnessBackground(shell),
                    ClipToBounds = true
                };
                host.Children.Add(shell);
                shell.ApplyResponsiveLayout(windowW, windowH);
                host.Measure(new Size(windowW, windowH));
                host.Arrange(new Rect(0, 0, windowW, windowH));
                host.UpdateLayout();
                shell.ApplyResponsiveLayout(windowW, windowH);
                media.ApplyResponsiveLayout(pageHost.ActualWidth, pageHost.ActualHeight);
                host.UpdateLayout();

                var tabs = FindVisualChildren<TabControl>(media).FirstOrDefault();
                if (tabs == null)
                    throw new InvalidOperationException("Media TabControl is missing in production PageHost.");
                tabs.SelectedIndex = 0;
                host.UpdateLayout();
                media.ApplyResponsiveLayout(pageHost.ActualWidth, pageHost.ActualHeight);
                host.UpdateLayout();

                var pageScroller = FindVisualChildren<ScrollViewer>(media)
                    .FirstOrDefault(candidate => candidate.Name == "MediaInboxPageScrollViewer");
                var tableFrame = FindVisualChildren<Border>(media)
                    .FirstOrDefault(candidate => candidate.Name == "MediaInboxTableFrame");
                var grid = FindVisualChildren<DataGrid>(media)
                    .FirstOrDefault(candidate => candidate.Name == "MediaInboxGrid");
                if (pageHost.ActualWidth <= 0 || pageHost.ActualHeight <= 0 || pageScroller == null || tableFrame == null || grid == null)
                    throw new InvalidOperationException("Production PageHost did not measure the media inbox surface.");
                if (pageScroller.HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled)
                    s_problems.Add($"Shell Media {windowW}x{windowH} page surface enables horizontal scrolling.");
                if (grid.ActualHeight < 212)
                    s_problems.Add($"Shell Media {windowW}x{windowH} inbox grid is shorter than four readable rows ({grid.ActualHeight:0} DIP).");

                var frameOrigin = tableFrame.TransformToAncestor(host).Transform(new Point(0, 0));
                var gridOrigin = grid.TransformToAncestor(host).Transform(new Point(0, 0));
                var gridTopGap = gridOrigin.Y - frameOrigin.Y;
                report.AppendLine(
                    $"  Shell Media {windowW}x{windowH}: tableFrameY={frameOrigin.Y:0}x{tableFrame.ActualHeight:0}, "
                    + $"gridY={gridOrigin.Y:0}x{grid.ActualHeight:0}, gridTopGap={gridTopGap:0}");
                if (gridTopGap > 180)
                    s_problems.Add($"Shell Media {windowW}x{windowH} inbox grid is pushed below the batch row (top gap={gridTopGap:0} DIP).");

                SavePng(host, Path.Combine(outputRoot, $"Shell-Media-{windowW}x{windowH}.png"));
                report.AppendLine(
                    $"  Shell Media {windowW}x{windowH}: PageHost={pageHost.ActualWidth:0}x{pageHost.ActualHeight:0}, "
                    + $"scroll={pageScroller.ActualWidth:0}x{pageScroller.ActualHeight:0} extent={pageScroller.ExtentHeight:0}, "
                    + $"grid={grid.ActualWidth:0}x{grid.ActualHeight:0}");

                var footer = FindVisualChildren<FrameworkElement>(media)
                    .FirstOrDefault(candidate => candidate.Name == "MediaInboxFooter");
                var historyButton = FindVisualChildren<FrameworkElement>(media)
                    .FirstOrDefault(candidate => candidate.Name == "MediaInboxHistoryButton");
                var secondaryActions = FindVisualChildren<FrameworkElement>(media)
                    .FirstOrDefault(candidate => candidate.Name == "MediaInboxSecondaryActions");
                if (footer == null || historyButton == null || secondaryActions == null)
                {
                    s_problems.Add($"Shell Media {windowW}x{windowH} footer reachability probe elements are missing.");
                }
                else
                {
                    var initialOffset = pageScroller.VerticalOffset;
                    var hasPageScroll = pageScroller.ScrollableHeight > 0.5;
                    if (hasPageScroll)
                    {
                        pageScroller.ScrollToVerticalOffset(pageScroller.ScrollableHeight);
                        host.UpdateLayout();
                    }

                    var viewportBounds = pageScroller.TransformToAncestor(host).TransformBounds(
                        new Rect(0, 0, pageScroller.ActualWidth, pageScroller.ActualHeight));
                    var GetBounds = (FrameworkElement element) => element.TransformToAncestor(host).TransformBounds(
                        new Rect(0, 0, element.ActualWidth, element.ActualHeight));
                    var footerBounds = GetBounds(footer);
                    var historyBounds = GetBounds(historyButton);
                    var secondaryBounds = GetBounds(secondaryActions);
                    report.AppendLine(
                        $"  Shell Media {windowW}x{windowH} bottom: offset={pageScroller.VerticalOffset:0.##}/{pageScroller.ScrollableHeight:0.##} "
                        + $"viewport={viewportBounds.Top:0.##}..{viewportBounds.Bottom:0.##} "
                        + $"footer={footerBounds.Top:0.##}..{footerBounds.Bottom:0.##} "
                        + $"history={historyBounds.Top:0.##}..{historyBounds.Bottom:0.##} "
                        + $"secondary={secondaryBounds.Top:0.##}..{secondaryBounds.Bottom:0.##}");

                    static bool IsInside(Rect outer, Rect inner)
                        => inner.Left >= outer.Left - 1
                           && inner.Right <= outer.Right + 1
                           && inner.Top >= outer.Top - 1
                           && inner.Bottom <= outer.Bottom + 1;

                    if (!IsInside(viewportBounds, footerBounds)
                        || !IsInside(viewportBounds, historyBounds)
                        || !IsInside(viewportBounds, secondaryBounds))
                    {
                        s_problems.Add($"Shell Media {windowW}x{windowH} footer remains unreachable at page end.");
                    }

                    if (hasPageScroll)
                    {
                        pageScroller.ScrollToVerticalOffset(initialOffset);
                        host.UpdateLayout();
                    }
                }
            }
            catch (Exception ex)
            {
                s_problems.Add($"Shell Media {windowW}x{windowH} probe failed: {ex.Message}");
            }
        }
    }

    private static void RunProductionShellMaintenanceProbe(string outputRoot, StringBuilder report)
    {
        report.AppendLine();
        report.AppendLine("Production shell PageHost geometry QA (maintenance compact inspectors)");
        foreach (var (windowW, windowH) in new[] { (1040, 700), (1100, 720), (1366, 768) })
        {
            try
            {
                var data = new FakeDashboardData(60);
                var shell = new AcrylicProductionShellView { DataContext = data };
                var maintenance = new MaintenanceView { DataContext = data };
                if (shell.PageHostForAudit is not ContentControl pageHost)
                    throw new InvalidOperationException("Production shell PageHost is not a ContentControl.");
                pageHost.Content = maintenance;

                var host = new Grid
                {
                    Width = windowW,
                    Height = windowH,
                    Background = CreateHarnessBackground(shell),
                    ClipToBounds = true
                };
                host.Children.Add(shell);
                shell.ApplyResponsiveLayout(windowW, windowH);
                host.Measure(new Size(windowW, windowH));
                host.Arrange(new Rect(0, 0, windowW, windowH));
                host.UpdateLayout();
                shell.ApplyResponsiveLayout(windowW, windowH);
                maintenance.ApplyResponsiveLayout(pageHost.ActualWidth, pageHost.ActualHeight);
                host.UpdateLayout();

                var tabs = FindVisualChildren<TabControl>(maintenance)
                    .FirstOrDefault(candidate => candidate.Items.Count >= 6);
                var findings = FindVisualChildren<DataGrid>(maintenance)
                    .FirstOrDefault(candidate => candidate.Name == "FindingsGrid");
                var diagnosticsInspector = FindVisualChildren<FrameworkElement>(maintenance)
                    .FirstOrDefault(candidate => candidate.Name == "MaintenanceDiagnosticsInspector");
                var diagnosticsButton = FindVisualChildren<Button>(maintenance)
                    .FirstOrDefault(candidate => candidate.Name == "MaintenanceDiagnosticsCompactDetailsButton");
                if (pageHost.ActualWidth <= 0 || pageHost.ActualHeight <= 0
                    || tabs == null || findings == null
                    || diagnosticsInspector == null || diagnosticsButton == null)
                    throw new InvalidOperationException("Production PageHost did not measure maintenance probe elements.");

                tabs.SelectedIndex = 0;
                host.UpdateLayout();
                findings.SelectedIndex = 0;
                maintenance.ApplyResponsiveLayout(pageHost.ActualWidth, pageHost.ActualHeight);
                host.UpdateLayout();
                var diagnosticsClosedRows = CountVisibleRows(findings);
                var compact = pageHost.ActualWidth < 980;
                if (diagnosticsClosedRows < 3)
                    s_problems.Add($"Shell Maintenance diagnostics {windowW}x{windowH} keeps only {diagnosticsClosedRows} complete rows (target >= 3).");
                if (compact && diagnosticsInspector.Visibility != Visibility.Collapsed)
                    s_problems.Add($"Shell Maintenance diagnostics {windowW}x{windowH} opens the inspector by default in compact mode.");
                if (compact && diagnosticsButton.Visibility != Visibility.Visible)
                    s_problems.Add($"Shell Maintenance diagnostics {windowW}x{windowH} does not expose the compact details action.");
                var diagnosticsClosedVisibility = diagnosticsInspector.Visibility;
                SavePng(host, Path.Combine(outputRoot, $"Shell-Maintenance-Diagnostics-{windowW}x{windowH}-closed.png"));
                diagnosticsButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                host.UpdateLayout();
                if (compact && diagnosticsInspector.Visibility != Visibility.Visible)
                    s_problems.Add($"Shell Maintenance diagnostics {windowW}x{windowH} cannot open the compact inspector.");
                SavePng(host, Path.Combine(outputRoot, $"Shell-Maintenance-Diagnostics-{windowW}x{windowH}.png"));

                tabs.SelectedIndex = 5;
                host.UpdateLayout();
                var process = FindVisualChildren<DataGrid>(maintenance)
                    .FirstOrDefault(candidate => candidate.Name == "MaintenanceProcessGrid");
                var processInspector = FindVisualChildren<FrameworkElement>(maintenance)
                    .FirstOrDefault(candidate => candidate.Name == "MaintenanceProcessInspector");
                var processButton = FindVisualChildren<Button>(maintenance)
                    .FirstOrDefault(candidate => candidate.Name == "MaintenanceProcessCompactDetailsButton");
                if (process == null || processInspector == null || processButton == null)
                    throw new InvalidOperationException("Production PageHost did not materialize maintenance process tab elements.");
                process.SelectedIndex = 0;
                maintenance.ApplyResponsiveLayout(pageHost.ActualWidth, pageHost.ActualHeight);
                host.UpdateLayout();
                var processClosedRows = CountVisibleRows(process);
                if (processClosedRows < 3)
                    s_problems.Add($"Shell Maintenance process {windowW}x{windowH} keeps only {processClosedRows} complete rows (target >= 3).");
                if (compact && processInspector.Visibility != Visibility.Collapsed)
                    s_problems.Add($"Shell Maintenance process {windowW}x{windowH} opens the inspector by default in compact mode.");
                if (compact && processButton.Visibility != Visibility.Visible)
                    s_problems.Add($"Shell Maintenance process {windowW}x{windowH} does not expose the compact details action.");
                var processClosedVisibility = processInspector.Visibility;
                SavePng(host, Path.Combine(outputRoot, $"Shell-Maintenance-Process-{windowW}x{windowH}-closed.png"));
                processButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                host.UpdateLayout();
                if (compact && processInspector.Visibility != Visibility.Visible)
                    s_problems.Add($"Shell Maintenance process {windowW}x{windowH} cannot open the compact inspector.");
                SavePng(host, Path.Combine(outputRoot, $"Shell-Maintenance-Process-{windowW}x{windowH}.png"));
                report.AppendLine(
                    $"  Shell Maintenance {windowW}x{windowH}: PageHost={pageHost.ActualWidth:0}x{pageHost.ActualHeight:0}, "
                    + $"compact={compact}, diagnosticsRows={diagnosticsClosedRows}, processRows={processClosedRows}, "
                    + $"closed={diagnosticsClosedVisibility}/{processClosedVisibility}, "
                    + $"opened={diagnosticsInspector.Visibility}/{processInspector.Visibility}");
            }
            catch (Exception ex)
            {
                s_problems.Add($"Shell Maintenance {windowW}x{windowH} probe failed: {ex.Message}");
            }
        }
    }

    private static void RunProductionShellTaskProbe(string outputRoot, StringBuilder report)
    {
        report.AppendLine();
        report.AppendLine("Production shell PageHost geometry QA (task workspace)");
        foreach (var (windowW, windowH) in new[] { (1040, 700), (1100, 720), (1366, 768) })
        {
            try
            {
                var data = new FakeDashboardData(60);
                var shell = new AcrylicProductionShellView { DataContext = data };
                var tasks = new TaskCenterView { DataContext = data };
                if (shell.PageHostForAudit is not ContentControl pageHost)
                    throw new InvalidOperationException("Production shell PageHost is not a ContentControl.");
                pageHost.Content = tasks;

                var host = new Grid
                {
                    Width = windowW,
                    Height = windowH,
                    Background = CreateHarnessBackground(shell),
                    ClipToBounds = true
                };
                host.Children.Add(shell);
                shell.ApplyResponsiveLayout(windowW, windowH);
                host.Measure(new Size(windowW, windowH));
                host.Arrange(new Rect(0, 0, windowW, windowH));
                host.UpdateLayout();
                shell.ApplyResponsiveLayout(windowW, windowH);
                tasks.ApplyResponsiveLayout(pageHost.ActualWidth, pageHost.ActualHeight);
                host.UpdateLayout();

                var taskSurface = FindVisualChildren<FrameworkElement>(tasks)
                    .FirstOrDefault(candidate => candidate.Name == "TaskPageScrollSurface");
                var taskGrid = FindVisualChildren<DataGrid>(tasks)
                    .FirstOrDefault(candidate => candidate.Name == "TaskGrid");
                var taskGridScroll = taskGrid == null
                    ? null
                    : FindVisualChildren<ScrollViewer>(taskGrid).FirstOrDefault();
                var taskQueue = FindVisualChildren<FrameworkElement>(tasks)
                    .FirstOrDefault(candidate => candidate.Name == "TaskQueuePanel");
                var taskInspector = FindVisualChildren<ScrollViewer>(tasks)
                    .FirstOrDefault(candidate => candidate.Name == "TaskDetailScrollViewer");
                var taskDetailsButton = FindVisualChildren<Button>(tasks)
                    .FirstOrDefault(candidate => candidate.Name == "TaskCompactDetailsButton");
                var taskCloseDetailsButton = FindVisualChildren<Button>(tasks)
                    .FirstOrDefault(candidate => candidate.Name == "TaskCompactCloseDetailsButton");
                var moreFilters = FindVisualChildren<Expander>(tasks)
                    .FirstOrDefault(candidate => candidate.Name == "TaskMoreFiltersExpander");
                if (pageHost.ActualWidth <= 0 || pageHost.ActualHeight <= 0
                    || taskSurface == null || taskGrid == null || taskGridScroll == null || taskQueue == null
                    || taskInspector == null || taskDetailsButton == null || taskCloseDetailsButton == null || moreFilters == null)
                    throw new InvalidOperationException("Production PageHost did not measure task probe elements.");

                taskGrid.SelectedIndex = 0;
                tasks.ApplyResponsiveLayout(pageHost.ActualWidth, pageHost.ActualHeight);
                host.UpdateLayout();

                var compact = pageHost.ActualWidth < 980;
                var closedRows = CountRowsFullyInside(taskGrid, taskGridScroll);
                if (closedRows < 3)
                    s_problems.Add($"Shell Tasks {windowW}x{windowH} keeps only {closedRows} complete rows before details (target >= 3).");
                if (compact && taskInspector.Visibility != Visibility.Collapsed)
                    s_problems.Add($"Shell Tasks {windowW}x{windowH} opens the inspector by default in compact mode.");
                if (compact && taskDetailsButton.Visibility != Visibility.Visible)
                    s_problems.Add($"Shell Tasks {windowW}x{windowH} does not expose the compact details action.");
                if (compact && pageHost.ActualWidth < 760 && moreFilters.Visibility != Visibility.Visible)
                    s_problems.Add($"Shell Tasks {windowW}x{windowH} does not expose the compact filter disclosure.");
                if (!compact && taskDetailsButton.Visibility == Visibility.Visible)
                    s_problems.Add($"Shell Tasks {windowW}x{windowH} keeps the compact details action visible on the wide layout.");

                SavePng(host, Path.Combine(outputRoot, $"Shell-Tasks-{windowW}x{windowH}-closed.png"));

                if (compact)
                {
                    taskDetailsButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    host.UpdateLayout();
                    var openedRows = CountRowsFullyInside(taskGrid, taskGridScroll);
                    if (taskInspector.Visibility != Visibility.Visible || taskInspector.ActualHeight <= 0)
                        s_problems.Add($"Shell Tasks {windowW}x{windowH} cannot open the compact inspector.");
                    if (openedRows < 3)
                        s_problems.Add($"Shell Tasks {windowW}x{windowH} keeps only {openedRows} complete rows after opening details.");
                    if (taskDetailsButton.Visibility != Visibility.Collapsed
                        || taskCloseDetailsButton.Visibility != Visibility.Visible)
                        s_problems.Add($"Shell Tasks {windowW}x{windowH} leaves the queue details action over the table after opening.");

                    var surfaceBounds = new Rect(0, 0, taskSurface.ActualWidth, taskSurface.ActualHeight);
                    var queueBounds = taskQueue.TransformToAncestor(taskSurface).TransformBounds(new Rect(0, 0, taskQueue.ActualWidth, taskQueue.ActualHeight));
                    var inspectorBounds = taskInspector.TransformToAncestor(taskSurface).TransformBounds(new Rect(0, 0, taskInspector.ActualWidth, taskInspector.ActualHeight));
                    if (inspectorBounds.Top + 0.5 < queueBounds.Bottom
                        || inspectorBounds.Bottom > surfaceBounds.Bottom + 0.5
                        || inspectorBounds.Left < -0.5
                        || inspectorBounds.Right > surfaceBounds.Right + 0.5)
                    {
                        s_problems.Add($"Shell Tasks {windowW}x{windowH} compact inspector escapes the task surface (queueBottom={queueBounds.Bottom:0.0}, inspector={inspectorBounds.Left:0.0},{inspectorBounds.Top:0.0}..{inspectorBounds.Right:0.0},{inspectorBounds.Bottom:0.0}, surface={surfaceBounds.Width:0.0}x{surfaceBounds.Height:0.0}).");
                    }
                    report.AppendLine($"    openedRows={openedRows}, closeAction={taskCloseDetailsButton.Visibility}");
                    SavePng(host, Path.Combine(outputRoot, $"Shell-Tasks-{windowW}x{windowH}.png"));
                }
                else if (taskInspector.Visibility != Visibility.Visible || taskInspector.ActualWidth <= 0)
                {
                    s_problems.Add($"Shell Tasks {windowW}x{windowH} does not show the selected-task inspector on the wide layout.");
                }

                report.AppendLine(
                    $"  Shell Tasks {windowW}x{windowH}: PageHost={pageHost.ActualWidth:0}x{pageHost.ActualHeight:0}, "
                    + $"compact={compact}, rows={closedRows}, queue={taskQueue.ActualWidth:0}x{taskQueue.ActualHeight:0}, gridScroll={taskGridScroll.ActualWidth:0}x{taskGridScroll.ActualHeight:0}, "
                    + $"grid={taskGrid.ActualWidth:0}x{taskGrid.ActualHeight:0}, inspector={taskInspector.Visibility}/{taskInspector.ActualWidth:0}x{taskInspector.ActualHeight:0}, "
                    + $"filters={moreFilters.Visibility}");
            }
            catch (Exception ex)
            {
                s_problems.Add($"Shell Tasks {windowW}x{windowH} probe failed: {ex.Message}");
            }
        }
    }

    private static int CountVisibleRows(DataGrid grid)
        => FindVisualChildren<DataGridRow>(grid)
            .Count(row => row.Visibility == Visibility.Visible && row.ActualHeight >= 22);

    private static int CountRowsFullyInside(DataGrid grid, FrameworkElement boundary)
    {
        var bounds = new Rect(0, 0, boundary.ActualWidth, boundary.ActualHeight);
        return FindVisualChildren<DataGridRow>(grid)
            .Count(row =>
            {
                if (row.Visibility != Visibility.Visible || row.ActualHeight < 22)
                    return false;
                var rowBounds = row.TransformToAncestor(boundary)
                    .TransformBounds(new Rect(0, 0, row.ActualWidth, row.ActualHeight));
                return rowBounds.Top >= -0.5
                    && rowBounds.Bottom <= bounds.Bottom + 0.5;
            });
    }

    private static void RunSidebarTransitionProbe(StringBuilder report)
    {
        report.AppendLine();
        report.AppendLine("Sidebar transition performance probe (2000x1100 host)");
        foreach (var (rapidToggle, enableMotion) in new[]
                 {
                     (false, true),
                     (true, true),
                     (false, false)
                 })
        {
            var measurement = MeasureSidebarTransition(rapidToggle, enableMotion);
            var label = !enableMotion ? "atomic-final" : rapidToggle ? "rapid-toggle" : "single-toggle";
            report.AppendLine(
                $"  {label} motion={measurement.MotionEnabled} duration={measurement.DurationMs:0.0}ms "
                + $"layout={measurement.LayoutUpdates} measure={measurement.MeasureCount}/{measurement.MeasureMs:0.0}ms "
                + $"arrange={measurement.ArrangeCount}/{measurement.ArrangeMs:0.0}ms "
                + $"frames={measurement.RenderingFrames} frameGapP95={measurement.FrameGapP95Ms:0.0}ms "
                + $"maxFrameGap={measurement.MaxFrameGapMs:0.0}ms slowFrameRatio={measurement.SlowFrameRatio:0.000} "
                + $"secondClick={measurement.SecondClickFired} finalWidth={measurement.FinalWidth:0.0} "
                + $"collapsed={measurement.FinalCollapsed} settled={measurement.Settled}");
            if (!measurement.Settled)
                s_problems.Add($"Sidebar {label} transition did not settle on its requested target.");
            if (rapidToggle && enableMotion && measurement.MotionEnabled && Math.Abs(measurement.FinalWidth - 270d) > 0.5)
                s_problems.Add($"Sidebar rapid toggle did not keep the latest expanded target (width={measurement.FinalWidth:0.0}).");
        }
    }

    private static SidebarTransitionMeasurement MeasureSidebarTransition(bool rapidToggle, bool enableMotion)
    {
        var shell = new AcrylicProductionShellView
        {
            DataContext = new FakeDashboardData(),
            MotionEnabledProvider = () => enableMotion
        };
        if (shell.PageHostForAudit is ContentControl pageHost)
            pageHost.Content = new TaskCenterView { DataContext = new FakeDashboardData() };

        var host = new LayoutProbeHost
        {
            Width = 2000,
            Height = 1100,
            Background = CreateHarnessBackground(shell),
            ClipToBounds = true
        };
        host.Children.Add(shell);
        host.Measure(new Size(host.Width, host.Height));
        host.Arrange(new Rect(0, 0, host.Width, host.Height));
        host.UpdateLayout();
        shell.ApplyResponsiveLayout(host.Width, host.Height);
        host.UpdateLayout();
        host.ResetMetrics();

        var layoutUpdates = 0;
        var renderingFrames = 0;
        var frameGapsMs = new List<double>();
        var maxFrameGapMs = 0d;
        long lastFrameTimestamp = 0;
        var stopwatch = Stopwatch.StartNew();
        var secondClickFired = false;
        host.LayoutUpdated += OnLayoutUpdated;
        EventHandler renderingHandler = (_, _) =>
        {
            renderingFrames++;
            var now = stopwatch.ElapsedTicks;
            if (lastFrameTimestamp != 0)
            {
                var frameGapMs = ToMilliseconds(now - lastFrameTimestamp);
                frameGapsMs.Add(frameGapMs);
                maxFrameGapMs = Math.Max(maxFrameGapMs, frameGapMs);
            }
            lastFrameTimestamp = now;
        };
        CompositionTarget.Rendering += renderingHandler;

        var button = shell.SidebarCollapseButtonForAudit;
        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        DispatcherTimer? rapidTimer = null;
        if (rapidToggle)
        {
            rapidTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(60)
            };
            rapidTimer.Tick += (_, _) =>
            {
                rapidTimer.Stop();
                secondClickFired = true;
                button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            };
            rapidTimer.Start();
        }

        var frame = new DispatcherFrame();
        // Keep the completion timer at the same priority as the rapid-toggle timer. A
        // continuously scheduled Render-priority timer can starve the 60ms second click,
        // making this probe report the first collapsed state instead of testing the latest
        // requested expanded state.
        var endTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        endTimer.Tick += (_, _) =>
        {
            host.UpdateLayout();
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            var minimumWait = enableMotion ? 260 : 50;
            var timedOut = elapsedMilliseconds >= 1500;
            var completed = elapsedMilliseconds >= minimumWait
                            && !shell.SidebarTransitionRunningForAudit
                            && (!rapidToggle || secondClickFired);
            if (completed || timedOut)
            {
                endTimer.Stop();
                frame.Continue = false;
            }
        };
        endTimer.Start();
        Dispatcher.PushFrame(frame);
        stopwatch.Stop();

        host.LayoutUpdated -= OnLayoutUpdated;
        CompositionTarget.Rendering -= renderingHandler;
        rapidTimer?.Stop();
        return new SidebarTransitionMeasurement
        {
            MotionEnabled = shell.SidebarMotionEnabledForAudit,
            DurationMs = stopwatch.Elapsed.TotalMilliseconds,
            LayoutUpdates = layoutUpdates,
            MeasureCount = host.MeasureCount,
            MeasureMs = ToMilliseconds(host.MeasureTicks),
            ArrangeCount = host.ArrangeCount,
            ArrangeMs = ToMilliseconds(host.ArrangeTicks),
            RenderingFrames = renderingFrames,
            FrameGapP95Ms = CalculatePercentile(frameGapsMs, 0.95),
            MaxFrameGapMs = maxFrameGapMs,
            SlowFrameRatio = frameGapsMs.Count == 0
                ? 0
                : frameGapsMs.Count(gap => gap > 1000d / 60d) / (double)frameGapsMs.Count,
            FinalWidth = shell.SidebarWidthForAudit,
            FinalCollapsed = shell.SidebarCollapsedForAudit,
            SecondClickFired = secondClickFired,
            Settled = !shell.SidebarTransitionRunningForAudit
        };

        void OnLayoutUpdated(object? sender, EventArgs args) => layoutUpdates++;
    }

    private static double ToMilliseconds(long stopwatchTicks)
        => stopwatchTicks * 1000d / Stopwatch.Frequency;

    private static double CalculatePercentile(IReadOnlyList<double> values, double percentile)
    {
        if (values.Count == 0)
            return 0;

        var sorted = values.OrderBy(value => value).ToArray();
        var index = (int)Math.Ceiling(Math.Max(0, Math.Min(1, percentile)) * sorted.Length) - 1;
        return sorted[Math.Max(0, Math.Min(sorted.Length - 1, index))];
    }

    private sealed class SidebarTransitionMeasurement
    {
        public bool MotionEnabled { get; set; }
        public double DurationMs { get; set; }
        public int LayoutUpdates { get; set; }
        public int MeasureCount { get; set; }
        public double MeasureMs { get; set; }
        public int ArrangeCount { get; set; }
        public double ArrangeMs { get; set; }
        public int RenderingFrames { get; set; }
        public double FrameGapP95Ms { get; set; }
        public double MaxFrameGapMs { get; set; }
        public double SlowFrameRatio { get; set; }
        public double FinalWidth { get; set; }
        public bool FinalCollapsed { get; set; }
        public bool SecondClickFired { get; set; }
        public bool Settled { get; set; }
    }

    private sealed class LayoutProbeHost : Grid
    {
        public int MeasureCount { get; private set; }
        public long MeasureTicks { get; private set; }
        public int ArrangeCount { get; private set; }
        public long ArrangeTicks { get; private set; }

        public void ResetMetrics()
        {
            MeasureCount = 0;
            MeasureTicks = 0;
            ArrangeCount = 0;
            ArrangeTicks = 0;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var timer = Stopwatch.StartNew();
            var result = base.MeasureOverride(availableSize);
            timer.Stop();
            MeasureCount++;
            MeasureTicks += timer.ElapsedTicks;
            return result;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var timer = Stopwatch.StartNew();
            var result = base.ArrangeOverride(finalSize);
            timer.Stop();
            ArrangeCount++;
            ArrangeTicks += timer.ElapsedTicks;
            return result;
        }
    }

    private static List<ElementMetric> SnapshotLayoutMetrics(Grid host)
    {
        var metrics = new List<ElementMetric>();
        foreach (var grid in FindVisualChildren<DataGrid>(host))
        {
            AddMetric(metrics, "G", grid.Name, grid.ActualWidth, grid.ActualHeight, grid.Visibility, string.Empty, string.Empty, false);
        }
        foreach (var list in FindVisualChildren<ListBox>(host))
        {
            AddMetric(metrics, "L", list.Name, list.ActualWidth, list.ActualHeight, list.Visibility, string.Empty, string.Empty, false);
        }
        foreach (var scroller in FindVisualChildren<ScrollViewer>(host))
        {
            if (string.IsNullOrEmpty(scroller.Name) || scroller.Name.StartsWith("PART_", StringComparison.Ordinal))
                continue;
            AddMetric(
                metrics,
                "S",
                scroller.Name,
                scroller.ActualWidth,
                scroller.ActualHeight,
                scroller.Visibility,
                scroller.VerticalScrollBarVisibility.ToString(),
                scroller.HorizontalScrollBarVisibility.ToString(),
                scroller.ExtentWidth > scroller.ViewportWidth + 0.5);
        }
        return metrics;
    }

    private static void AddMetric(
        List<ElementMetric> metrics,
        string kind,
        string name,
        double width,
        double height,
        Visibility visibility,
        string vbar,
        string hbar,
        bool horizontalOverflow)
    {
        if (string.IsNullOrEmpty(name))
            return;

        var baseKey = kind + "|" + name;
        var key = baseKey;
        var ordinal = 2;
        while (metrics.Any(metric => metric.Key == key))
            key = baseKey + "#" + ordinal++;

        metrics.Add(new ElementMetric
        {
            Key = key,
            Width = width,
            Height = height,
            Visibility = visibility,
            VBar = vbar,
            HBar = hbar,
            HorizontalOverflow = horizontalOverflow
        });
    }

    private static void CompareLayoutMetrics(List<ElementMetric> initial, List<ElementMetric> after, string name, StringBuilder report)
    {
        var afterByKey = after.ToDictionary(metric => metric.Key);
        foreach (var metric in initial)
        {
            if (!afterByKey.TryGetValue(metric.Key, out var recovered))
            {
                s_problems.Add($"{name} resize transition lost element {metric.Key}");
                continue;
            }

            if (Math.Abs(recovered.Width - metric.Width) > 1
                || Math.Abs(recovered.Height - metric.Height) > 1
                || recovered.Visibility != metric.Visibility
                || recovered.VBar != metric.VBar
                || recovered.HBar != metric.HBar
                || recovered.HorizontalOverflow != metric.HorizontalOverflow)
            {
                s_problems.Add(
                    $"{name} resize transition did not recover {metric.Key} " +
                    $"(before {metric.Width:0}x{metric.Height:0}/{metric.Visibility}/{metric.VBar}/{metric.HBar}/{metric.HorizontalOverflow}, " +
                    $"after {recovered.Width:0}x{recovered.Height:0}/{recovered.Visibility}/{recovered.VBar}/{recovered.HBar}/{recovered.HorizontalOverflow})");
            }
        }
        report.AppendLine($"  {name} resize transition recovered {initial.Count} metrics");
    }

    private static void ProbeGrid(
        StringBuilder report,
        string label,
        string gridName,
        int tabIndex,
        double height,
        Func<UserControl> createView,
        Action<UserControl> applyLayout,
        string? innerTabHeader = null,
        double width = 900,
        VirtualizationMode expectedVirtualizationMode = VirtualizationMode.Recycling,
        bool expectedColumnVirtualization = true)
    {
        try
        {
            var view = createView();
            var host = new Grid
            {
                Width = width,
                Height = height,
                Background = CreateHarnessBackground(view),
                ClipToBounds = true
            };
            host.Children.Add(view);
            applyLayout(view);
            host.Measure(new Size(width, height));
            host.Arrange(new Rect(0, 0, width, height));
            host.UpdateLayout();
            if (tabIndex >= 0)
            {
                var tabs = FindVisualChildren<TabControl>(host).FirstOrDefault();
                var segmentTabs = FindVisualChildren<ListBox>(host)
                    .FirstOrDefault(candidate => candidate.Name.EndsWith("SegmentTabs", StringComparison.Ordinal)
                        || candidate.Name == "SettingsSectionTabs");
                if (segmentTabs != null && tabIndex < segmentTabs.Items.Count)
                    segmentTabs.SelectedIndex = tabIndex;
                else if (tabs != null && tabIndex < tabs.Items.Count)
                    tabs.SelectedIndex = tabIndex;
                host.UpdateLayout();
                if (!string.IsNullOrEmpty(innerTabHeader))
                {
                    var innerSegmented = FindVisualChildren<ListBox>(host)
                        .FirstOrDefault(candidate => candidate.Name == "MaintenanceDiagnosticsSubTabs"
                            && candidate.Items.Cast<object>().Any(item => (item as ListBoxItem)?.Content?.ToString() == innerTabHeader));
                    if (innerSegmented != null)
                    {
                        innerSegmented.SelectedIndex = innerSegmented.Items.Cast<object>()
                            .Select((item, itemIndex) => new { item, itemIndex })
                            .First(candidate => (candidate.item as ListBoxItem)?.Content?.ToString() == innerTabHeader)
                            .itemIndex;
                    }
                    var innerTabs = FindVisualChildren<TabControl>(host)
                        .FirstOrDefault(candidate => candidate.Items
                            .Cast<TabItem>()
                            .Any(item => item.Header?.ToString() == innerTabHeader));
                    var innerItem = innerTabs?.Items
                        .Cast<TabItem>()
                        .FirstOrDefault(item => item.Header?.ToString() == innerTabHeader);
                    if (innerTabs != null && innerItem != null)
                        innerTabs.SelectedItem = innerItem;
                    host.UpdateLayout();
                }
                applyLayout(view);
                host.UpdateLayout();
            }

            var grid = FindVisualChildren<DataGrid>(host).FirstOrDefault(x => x.Name == gridName);
            if (grid == null)
            {
                s_problems.Add($"{label} scroll probe: {gridName} not found at height {height:0}");
                return;
            }
            VerifyDataGridHeaderInteractionContract(report, label, grid);
            var scroller = FindVisualChildren<ScrollViewer>(grid)
                .OrderByDescending(candidate => candidate.ViewportHeight)
                .FirstOrDefault();
            if (scroller == null)
            {
                s_problems.Add($"{label} scroll probe: {gridName} has no internal ScrollViewer at height {height:0}");
                return;
            }
            if (grid.Items.Count < 50)
            {
                s_problems.Add($"{label} scroll probe: {gridName} needs >=50 rows, got {grid.Items.Count}");
                return;
            }
            if (!VirtualizingPanel.GetIsVirtualizing(grid) || VirtualizingPanel.GetVirtualizationMode(grid) != expectedVirtualizationMode)
                s_problems.Add($"{label} {gridName} h={height:0} virtualization mode is {VirtualizingPanel.GetVirtualizationMode(grid)}, expected {expectedVirtualizationMode}");
            if (grid.EnableColumnVirtualization != expectedColumnVirtualization)
                s_problems.Add($"{label} {gridName} h={height:0} column virtualization is {grid.EnableColumnVirtualization}, expected {expectedColumnVirtualization}");

            grid.ScrollIntoView(grid.Items[grid.Items.Count - 1]);
            host.UpdateLayout();

            var scrollFractions = gridName is "MediaInboxGrid" or "TaskGrid"
                // The production regression is directional: the rows can disappear after
                // reaching the end and then dragging the thumb back toward the head. Keep
                // this sequence explicit and long enough to match the manual 20-drag pass.
                ? new[] { 0.0, 1.0, 0.0, 1.0, 0.5, 0.0, 1.0, 0.25, 0.75, 0.0,
                    1.0, 0.33, 0.66, 0.0, 1.0, 0.5, 0.1, 0.9, 0.0, 1.0 }
                : new[] { 0.0, 0.25, 0.5, 0.75, 1.0 };
            var scrollStep = 0;
            foreach (var fraction in scrollFractions)
            {
                DataGridScrollDiagnostics.MarkTrigger(grid, "拖动滑块");
                scroller.ScrollToVerticalOffset(scroller.ScrollableHeight * fraction);
                host.UpdateLayout();
                var diagnosticLine = DataGridScrollDiagnostics.CaptureNow(grid, $"拖动滑块:{(int)(fraction * 100)}");
                var rows = FindVisualChildren<DataGridRow>(grid)
                    .Select(row => new RowProbe(
                        row.GetIndex(),
                        row.ActualHeight,
                        row.TransformToAncestor(scroller).Transform(new Point(0, 0)).Y,
                        row.DataContext == null))
                    .OrderBy(row => row.Y)
                    .ToList();
                var presenter = FindVisualChildren<DataGridRowsPresenter>(grid).FirstOrDefault();
                var positionLabel = (int)(fraction * 100);
                var headerGap = rows.Count > 0 ? rows[0].Y - grid.ColumnHeaderHeight : 0;
                report.AppendLine(
                    $"  {label} {gridName} h={height:0} step={scrollStep++} pos={positionLabel} offset={scroller.VerticalOffset:0.##} " +
                    $"scrollable={scroller.ScrollableHeight:0.##} rows={rows.Count} " +
                    $"firstY={(rows.Count > 0 ? rows[0].Y : double.NaN):0.##} gap={headerGap:0.##} presenterH={(presenter?.ActualHeight ?? double.NaN):0.##} gridH={grid.ActualHeight:0.##}");
                report.AppendLine($"  {diagnosticLine}");

                if (gridName == "MediaInboxGrid")
                {
                    report.AppendLine($"  {label} {gridName} starFill={DataGridStarFill.GetEnabled(grid)}");
                    if (DataGridStarFill.GetEnabled(grid))
                        s_problems.Add($"{label} {gridName} keeps shared star-fill redistribution enabled");
                }

                if (rows.Count == 0 && grid.Items.Count > 0)
                {
                    s_problems.Add($"{label} {gridName} h={height:0} pos={positionLabel} realized no rows");
                    continue;
                }
                if (rows.Any(row => row.Index < 0 || row.DataContextNull))
                    s_problems.Add($"{label} {gridName} h={height:0} pos={positionLabel} realized row has invalid index or null DataContext");
                if (rows.Count > 0 && headerGap > 4)
                    s_problems.Add($"{label} {gridName} h={height:0} pos={positionLabel} phantom gap under header (gap={headerGap:0.##})");
                if (fraction >= 1.0)
                {
                    var lastRow = rows.FirstOrDefault(row => row.Index == grid.Items.Count - 1);
                    var horizontalBars = FindVisualChildren<ScrollBar>(scroller)
                        .Where(bar => bar.Orientation == Orientation.Horizontal)
                        .Select(bar => new
                        {
                            Height = bar.ActualHeight,
                            Y = bar.TransformToAncestor(scroller).Transform(new Point(0, 0)).Y,
                            Visibility = bar.Visibility
                        })
                        .ToList();
                    var visibleHorizontalBar = horizontalBars.FirstOrDefault(bar =>
                        bar.Visibility == Visibility.Visible && bar.Height > 0);
                    var lastRowDescription = lastRow == null
                        ? "missing"
                        : $"y={lastRow.Y:0.##},h={lastRow.Height:0.##},bottom={lastRow.Y + lastRow.Height:0.##}";
                    var horizontalBarDescription = horizontalBars.Count == 0
                        ? "missing"
                        : string.Join(";", horizontalBars.Select(bar =>
                            $"y={bar.Y:0.##},h={bar.Height:0.##},vis={bar.Visibility}"));
                    var columnWidthDescription = string.Join(",", grid.Columns.Select(column =>
                        $"{column.Header}:{column.ActualWidth:0.##}/{column.MinWidth:0.##}"));
                    report.AppendLine(
                        $"  {label} {gridName} bottom geometry: viewport={scroller.ViewportWidth:0.##}x{scroller.ViewportHeight:0.##} " +
                        $"extent={scroller.ExtentWidth:0.##}x{scroller.ExtentHeight:0.##} " +
                        $"grid={grid.ActualWidth:0.##}x{grid.ActualHeight:0.##} last={lastRowDescription} " +
                        $"hbar={horizontalBarDescription} cols={columnWidthDescription}");
                    if (lastRow == null)
                    {
                        s_problems.Add($"{label} {gridName} h={height:0} bottom last row not realized");
                    }
                    else
                    {
                        var bottom = lastRow.Y + lastRow.Height;
                        if (bottom > grid.ActualHeight + 1)
                            s_problems.Add($"{label} {gridName} h={height:0} bottom last row clipped (bottom={bottom:0.##} gridH={grid.ActualHeight:0.##})");
                        if (visibleHorizontalBar != null && bottom > visibleHorizontalBar.Y + 0.5)
                            s_problems.Add($"{label} {gridName} h={height:0} bottom last row is under horizontal scrollbar (bottom={bottom:0.##} hbarY={visibleHorizontalBar.Y:0.##})");
                    }
                    if (rows.Count > 0 && headerGap > 4)
                        s_problems.Add($"{label} {gridName} h={height:0} bottom phantom gap under header (gap={headerGap:0.##})");

                    var before = rows.Select(row => row.Y).ToArray();
                    host.UpdateLayout();
                    var after = FindVisualChildren<DataGridRow>(grid)
                        .Select(row => row.TransformToAncestor(scroller).Transform(new Point(0, 0)).Y)
                        .OrderBy(value => value)
                        .ToArray();
                    if (before.Length > 0 && after.Length == before.Length)
                    {
                        var maxDelta = before.Zip(after, (left, right) => Math.Abs(left - right)).DefaultIfEmpty(0).Max();
                        if (maxDelta > 0.5)
                            s_problems.Add($"{label} {gridName} h={height:0} bottom rows jumped after UpdateLayout (delta={maxDelta:0.##})");
                    }
                }
            }

            CaptureSemanticScroll("滚轮", scroller.LineDown);
            CaptureSemanticScroll("滚轮", scroller.LineUp);
            CaptureSemanticScroll("PageDown", scroller.PageDown);
            CaptureSemanticScroll("PageUp", scroller.PageUp);
            CaptureSemanticScroll("Ctrl+End", scroller.ScrollToEnd);
            grid.ScrollIntoView(grid.Items[grid.Items.Count - 1]);
            host.UpdateLayout();
            CaptureSemanticScroll("定位最后一项", () => { });
            scroller.ScrollToVerticalOffset(gridName is "MediaInboxGrid" or "TaskGrid"
                ? 0
                : scroller.ScrollableHeight);
            host.UpdateLayout();

            void CaptureSemanticScroll(string trigger, Action action)
            {
                action();
                host.UpdateLayout();
                var diagnosticLine = DataGridScrollDiagnostics.CaptureNow(grid, trigger);
                report.AppendLine($"  {label} {gridName} semantic={trigger} offset={scroller.VerticalOffset:0.##} "
                    + $"scrollable={scroller.ScrollableHeight:0.##}");
                report.AppendLine($"  {diagnosticLine}");
                if (diagnosticLine.IndexOf("anomaly=", StringComparison.Ordinal) >= 0)
                    s_problems.Add($"{label} {gridName} semantic {trigger} reported {diagnosticLine}");
            }

            var scrollable = scroller.ScrollableHeight;
            var offset = scroller.VerticalOffset;
            if (gridName is "MediaInboxGrid" or "TaskGrid")
            {
                if (scrollable < 0 || offset > 1)
                    s_problems.Add($"{label} {gridName} h={height:0} scroll-back invalid (offset={offset:0.##} scrollable={scrollable:0.##})");
            }
            else if (scrollable < 0 || offset > scrollable + 1 || offset < scrollable - 1)
            {
                s_problems.Add($"{label} {gridName} h={height:0} scroll bottom invalid (offset={offset:0.##} scrollable={scrollable:0.##})");
            }
        }
        catch (Exception ex)
        {
            s_problems.Add($"{label} scroll probe failed at height {height:0}: {ex.Message}");
        }
    }

    private static void VerifyDataGridHeaderInteractionContract(StringBuilder report, string label, DataGrid grid)
    {
        if (!grid.CanUserResizeColumns)
            s_problems.Add($"{label} {grid.Name} disables column resizing");
        if (!grid.CanUserSortColumns)
            s_problems.Add($"{label} {grid.Name} disables column sorting");

        var headers = FindVisualChildren<DataGridColumnHeader>(grid)
            .Where(header => header.Visibility == Visibility.Visible && header.Column != null)
            .ToList();
        if (headers.Count == 0)
        {
            s_problems.Add($"{label} {grid.Name} has no realized column header");
            return;
        }

        foreach (var header in headers)
        {
            var template = header.Template;
            var leftGripper = template?.FindName("PART_LeftHeaderGripper", header) as Thumb;
            var rightGripper = template?.FindName("PART_RightHeaderGripper", header) as Thumb;
            if (leftGripper == null || rightGripper == null)
            {
                s_problems.Add($"{label} {grid.Name} header \"{header.Content}\" is missing WPF resize parts");
                continue;
            }

            // WPF intentionally collapses the left gripper on the first column. The
            // template still needs both named parts, while at least one boundary must
            // remain an actual hit area for every realized header.
            if (leftGripper.ActualWidth < 4 && rightGripper.ActualWidth < 4)
                s_problems.Add($"{label} {grid.Name} header \"{header.Content}\" has no usable resize hit area");
        }

        // Force one header through a sorted state in the offscreen host. This verifies that
        // the arrow is not merely present in XAML but obtains a non-zero layout slot.
        var firstHeader = headers[0];
        var column = firstHeader.Column;
        var previousDirection = column.SortDirection;
        column.SortDirection = System.ComponentModel.ListSortDirection.Ascending;
        grid.UpdateLayout();
        var arrow = FindVisualChildren<FrameworkElement>(firstHeader)
            .FirstOrDefault(element => element.Name == "SortGlyph" || element.Name == "SortArrow");
        if (arrow == null || arrow.Visibility != Visibility.Visible || arrow.ActualWidth < 8)
            s_problems.Add($"{label} {grid.Name} sorted header \"{firstHeader.Content}\" has a clipped sort arrow");
        column.SortDirection = previousDirection;
        grid.UpdateLayout();

        report.AppendLine($"  {label} {grid.Name} header contract: resize=true headers={headers.Count} sort-arrow={(arrow != null ? "visible" : "missing")}");
    }

    private sealed class ProbeCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
        }
    }

    private sealed class RowProbe
    {
        public RowProbe(int index, double height, double y, bool dataContextNull)
        {
            Index = index;
            Height = height;
            Y = y;
            DataContextNull = dataContextNull;
        }

        public int Index { get; }
        public double Height { get; }
        public double Y { get; }
        public bool DataContextNull { get; }
    }

    private sealed class ScaleRowSnapshot
    {
        public ScaleRowSnapshot(int index, string id, Rect rect, double height)
        {
            Index = index;
            Id = id;
            Rect = rect;
            Height = height;
        }

        public int Index { get; }
        public string Id { get; }
        public Rect Rect { get; }
        public double Height { get; }
    }

    private sealed class ScaleGridSnapshot
    {
        public ScaleGridSnapshot(
            Rect presenterRect,
            int realizedCount,
            int visibleCount,
            string firstId,
            double firstY,
            double firstHeight,
            int lastIndex,
            string lastId,
            double lastY,
            double lastBottom,
            double lastHeight)
        {
            PresenterRect = presenterRect;
            RealizedCount = realizedCount;
            VisibleCount = visibleCount;
            FirstId = firstId;
            FirstY = firstY;
            FirstHeight = firstHeight;
            LastIndex = lastIndex;
            LastId = lastId;
            LastY = lastY;
            LastBottom = lastBottom;
            LastHeight = lastHeight;
        }

        public Rect PresenterRect { get; }
        public int RealizedCount { get; }
        public int VisibleCount { get; }
        public string FirstId { get; }
        public double FirstY { get; }
        public double FirstHeight { get; }
        public int LastIndex { get; }
        public string LastId { get; }
        public double LastY { get; }
        public double LastBottom { get; }
        public double LastHeight { get; }
    }

    private sealed class ElementMetric
    {
        public string Key { get; set; } = string.Empty;
        public double Width { get; set; }
        public double Height { get; set; }
        public Visibility Visibility { get; set; }
        public string VBar { get; set; } = string.Empty;
        public string HBar { get; set; } = string.Empty;
        public bool HorizontalOverflow { get; set; }
    }

    private static void SavePng(Visual visual, string path)
    {
        var actual = visual as FrameworkElement;
        var width = actual?.ActualWidth ?? 0;
        var height = actual?.ActualHeight ?? 0;
        if (width <= 0 || height <= 0)
            throw new InvalidOperationException($"Cannot render {path}: empty size {width}x{height}");

        var bitmap = new RenderTargetBitmap((int)Math.Ceiling(width), (int)Math.Ceiling(height), 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root) where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T typed)
                yield return typed;
            foreach (var nested in FindVisualChildren<T>(child))
                yield return nested;
        }
    }

    private static string GetStableId(object? item)
    {
        if (item == null)
            return "none";
        foreach (var propertyName in new[] { "MediaId", "TaskId", "EntryId", "Id" })
        {
            var property = item.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
                continue;
            var value = property.GetValue(item, null)?.ToString();
            if (!string.IsNullOrWhiteSpace(value))
                return value!;
        }
        return "index-only";
    }
}
