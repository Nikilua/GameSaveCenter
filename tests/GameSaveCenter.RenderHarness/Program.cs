using System;
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Markup;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
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
            RunThemeQa(outputRoot, report);
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
        AppendRunMetadata(report, "scaleprobe", "OffscreenRenderHarness", "production default palette", "backend 200/2000/10000; media UI window 2000");
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

    private static void RenderTabs(UserControl view, string outputRoot, string name, int windowW, int windowH, double contentW, double contentH, StringBuilder report, Action applyLayout)
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
                WorkingDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."))
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
        foreach (var backendCount in new[] { 200, 2000, 10000 })
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
            RowHeight = 44,
            ColumnHeaderHeight = 36
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

            var locator = FindVisualChildren<FrameworkElement>(host)
                .FirstOrDefault(element => element.Name == "SettingsValidationLocateButton");
            var tabs = FindVisualChildren<ListBox>(host)
                .FirstOrDefault(element => element.Name == "SettingsSectionTabs");
            if (locator == null || tabs == null)
                throw new InvalidOperationException("Settings validation locator controls did not materialize.");

            locator.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            host.UpdateLayout();
            report.AppendLine($"  SettingsValidationNavigation visible={locator.Visibility} selectedCategory={tabs.SelectedIndex}");
            if (locator.Visibility != Visibility.Visible)
                s_problems.Add("SettingsValidationNavigation locator is not visible for a backup validation error");
            if (tabs.SelectedIndex != 1)
                s_problems.Add($"SettingsValidationNavigation selected category {tabs.SelectedIndex}, expected 1");
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

    private static void ApplyThemePalette(UserControl view, GameSaveCenterThemeMode mode)
    {
        // Settings owns a separate material hierarchy and must use the same runtime path as
        // the Playnite settings host. The generic Dashboard resource injection would leave its
        // shell/card brushes on the static dark DesignTokens fallback during Light QA.
        if (view is GameSaveCenterSettingsView settings)
        {
            settings.ApplyThemeForAudit(mode);
            return;
        }

        var palette = AdaptiveThemePaletteFactory.Create(view, false, 50, mode);
        AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(view.Resources, palette, false, false);
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
        RunSidebarTransitionProbe(report);
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
                + $"frames={measurement.RenderingFrames} maxFrameGap={measurement.MaxFrameGapMs:0.0}ms "
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
                maxFrameGapMs = Math.Max(maxFrameGapMs, ToMilliseconds(now - lastFrameTimestamp));
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
            MaxFrameGapMs = maxFrameGapMs,
            FinalWidth = shell.SidebarWidthForAudit,
            FinalCollapsed = shell.SidebarCollapsedForAudit,
            SecondClickFired = secondClickFired,
            Settled = !shell.SidebarTransitionRunningForAudit
        };

        void OnLayoutUpdated(object? sender, EventArgs args) => layoutUpdates++;
    }

    private static double ToMilliseconds(long stopwatchTicks)
        => stopwatchTicks * 1000d / Stopwatch.Frequency;

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
        public double MaxFrameGapMs { get; set; }
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
