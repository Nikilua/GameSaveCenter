using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Automation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.Views;
using GameSaveCenter.RenderHarness.UiAudit;

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

        var exitCode = 0;
        var thread = new Thread(() => { exitCode = Run(args); });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        return exitCode;
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
                Path.Combine(outputRoot, "v6-maintenance-device.png"),
                "MaintenanceDeviceScrollSurface",
                1600,
                900,
                ApplySimpleResponsiveV3,
                problems,
                report,
                view => SelectTab(view, 1));
            CaptureV3Shot(
                new MaintenanceView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v6-maintenance-audit.png"),
                "MaintenanceAuditFindingsGrid",
                1600,
                900,
                ApplySimpleResponsiveV3,
                problems,
                report,
                view => SelectTab(view, 3));
            CaptureV3Shot(
                new MaintenanceView { DataContext = new FakeDashboardData() },
                Path.Combine(outputRoot, "v6-maintenance-process.png"),
                "MaintenanceProcessScrollSurface",
                1600,
                900,
                ApplySimpleResponsiveV3,
                problems,
                report,
                view => SelectTab(view, 4));

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
                    view => SelectTab(view, 0),
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
                    view => SelectTab(view, 1),
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
                    view => SelectTab(view, 3),
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
            Background = new SolidColorBrush(Color.FromRgb(24, 30, 43)),
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
            Background = new SolidColorBrush(Color.FromRgb(24, 30, 43)),
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
            throw new InvalidOperationException($"V3 shot target not rendered: {elementName} at {windowW}x{windowH}");

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
            RunSettingsLayoutProbes(report);
            RunThemeQa(outputRoot, report);
            RunResizeTransitionProbes(report);

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
        RenderTabs(view, outputRoot, "Media", windowW, windowH, contentW, contentH, report, () => view.ApplyResponsiveLayout(contentW, windowH));
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
            Background = new SolidColorBrush(Color.FromRgb(24, 30, 43)),
            ClipToBounds = true
        };
        host.Children.Add(view);

        applyLayout();
        host.Measure(new Size(contentW, contentH));
        host.Arrange(new Rect(0, 0, contentW, contentH));
        host.UpdateLayout();
        applyLayout();
        host.UpdateLayout();

        var tabs = FindVisualChildren<TabControl>(host).FirstOrDefault();
        if (tabs == null)
        {
            throw new InvalidOperationException($"{name} has no TabControl to render.");
        }

        var tabCount = tabs.Items.Count;
        for (var i = 0; i < tabCount; i++)
        {
            tabs.SelectedIndex = i;
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
            report.AppendLine($"  {name} tab{i} render_ms={sw.ElapsedMilliseconds}");
            CollectScrollDiagnostics(host, report, name, windowW, windowH, i);
        }
    }

    private static void RenderView(UserControl view, string outputRoot, string name, int windowW, int windowH, double contentW, double contentH, StringBuilder report, Action applyLayout)
    {
        var host = new Grid
        {
            Width = contentW,
            Height = contentH,
            Background = new SolidColorBrush(Color.FromRgb(24, 30, 43)),
            ClipToBounds = true
        };
        host.Children.Add(view);

        applyLayout();
        host.Measure(new Size(contentW, contentH));
        host.Arrange(new Rect(0, 0, contentW, contentH));
        host.UpdateLayout();
        applyLayout();
        host.UpdateLayout();
        var sw = Stopwatch.StartNew();
        SavePng(host, Path.Combine(outputRoot, $"{name}-{windowW}x{windowH}.png"));
        sw.Stop();
        report.AppendLine($"  {name} render_ms={sw.ElapsedMilliseconds}");
        CollectScrollDiagnostics(host, report, name, windowW, windowH, -1);
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
            report.AppendLine($"  {label} {grid.Name}: size={grid.ActualWidth:0}x{grid.ActualHeight:0}, rows={grid.Items.Count}");
            if (grid.Name is "FindingsGrid" or "MaintenanceDeviceGrid" or "MaintenanceAuditFindingsGrid" or "MaintenanceProcessGrid")
            {
                var fillRatio = host.ActualHeight > 0 ? grid.ActualHeight / host.ActualHeight : 0;
                report.AppendLine($"  {label} {grid.Name} fill: hostH={host.ActualHeight:0}, gridH={grid.ActualHeight:0}, ratio={fillRatio:0.00}");
            }
            if (grid.ActualHeight > 0
                && grid.ActualHeight < 236
                && grid.Name != "MaintenanceAuditLogGrid")
            {
                s_problems.Add($"{label} {grid.Name} table viewport is only {grid.ActualHeight:0} DIP (< 236)");
            }
        }

        foreach (var list in FindVisualChildren<ListBox>(host))
        {
            if (string.IsNullOrEmpty(list.Name))
                continue;
            report.AppendLine($"  {label} {list.Name}: size={list.ActualWidth:0}x{list.ActualHeight:0}, items={list.Items.Count}");
            if (list.ActualHeight > 0
                && list.ActualHeight < 236
                && list.Name != "OverviewActivityList")
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
                         "OverviewHomeToolbar",
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
                var lowerActivity = FindVisualChildren<FrameworkElement>(host)
                    .FirstOrDefault(candidate => candidate.Name == "OverviewRecentActivityCard");
                var reference = lowerActivity ?? overviewLayout;
                var layoutOrigin = reference.TransformToAncestor(host).Transform(new Point(0, 0));
                var secondaryOrigin = secondary.TransformToAncestor(host).Transform(new Point(0, 0));
                var topDelta = secondaryOrigin.Y - layoutOrigin.Y;
                report.AppendLine($"  {label} OverviewSecondaryTopDelta: {topDelta:0.##} DIP (relative to lower activity row)");
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
                if (widthRatio < 0.8)
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
        }
    }

    private static void RunDataGridScrollProbes(StringBuilder report)
    {
        var heights = new[] { 287d, 311d, 337d, 353d, 419d };
        foreach (var height in heights)
        {
            ProbeGrid(report, "Save", "SaveHistoryGrid", 0, height,
                () => new SaveCenterView { DataContext = new FakeDashboardData(60) },
                view => ((SaveCenterView)view).ApplyResponsiveLayout(900, height));
            ProbeGrid(report, "Task", "TaskGrid", -1, height,
                () => new TaskCenterView { DataContext = new FakeDashboardData(60) },
                view => ((TaskCenterView)view).ApplyResponsiveLayout(900, height));
            ProbeGrid(report, "Maintenance-Diagnostics", "FindingsGrid", 0, height,
                () => new MaintenanceView { DataContext = new FakeDashboardData(60) },
                view => ((MaintenanceView)view).ApplyResponsiveLayout(900, height));
            ProbeGrid(report, "Maintenance-Audit", "MaintenanceAuditFindingsGrid", 3, height,
                () => new MaintenanceView { DataContext = new FakeDashboardData(60) },
                view => ((MaintenanceView)view).ApplyResponsiveLayout(900, height),
                "发现的问题");
            ProbeGrid(report, "Maintenance-AuditLog", "MaintenanceAuditLogGrid", 3, height,
                () => new MaintenanceView { DataContext = new FakeDashboardData(60) },
                view => ((MaintenanceView)view).ApplyResponsiveLayout(900, height),
                "审计记录");
        }
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
                        Background = new SolidColorBrush(Color.FromRgb(24, 30, 43)),
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
                    var tabs = FindVisualChildren<TabControl>(host).FirstOrDefault();
                    var tabItems = FindVisualChildren<TabItem>(host).Where(item => item.Parent != null).ToList();
                    var headerScroller = FindVisualChildren<ScrollViewer>(host).FirstOrDefault(scroller => scroller.Name == "SettingsHeaderScroller");
                    var visibleTabs = tabItems.Count(item => item.Visibility == Visibility.Visible);
                    var minTabWidth = tabItems.Count == 0 ? 0 : tabItems.Min(item => item.ActualWidth);
                    var minTabHeight = tabItems.Count == 0 ? 0 : tabItems.Min(item => item.ActualHeight);
                    report.AppendLine(
                        $"  SettingsLayout w={width:0} h={height:0} headerH={(header?.ActualHeight ?? double.NaN):0.##} tabs={(tabs == null ? -1 : tabs.Items.Count)} tabItems={tabItems.Count} visible={visibleTabs} minW={minTabWidth:0.##} minH={minTabHeight:0.##} scroller={(headerScroller == null ? "missing" : headerScroller.GetType().Name)}");
                    if (header == null || header.ActualHeight <= 0)
                        s_problems.Add($"SettingsLayout w={width:0} h={height:0} header is not visible");
                    if (tabs == null || tabs.Items.Count != 5)
                        s_problems.Add($"SettingsLayout w={width:0} h={height:0} expected 5 categories, got {(tabs == null ? 0 : tabs.Items.Count)}");
                    if (tabItems.Count < 5 || tabItems.Any(item => item.Visibility != Visibility.Visible || item.ActualWidth <= 0 || item.ActualHeight <= 0))
                        s_problems.Add($"SettingsLayout w={width:0} h={height:0} not all category tabs are visible and measurable");
                    if (headerScroller == null)
                        s_problems.Add($"SettingsLayout w={width:0} h={height:0} category rail has no scroll access");
                    else if (tabItems.Count > 0)
                    {
                        headerScroller.ScrollToVerticalOffset(headerScroller.ScrollableHeight);
                        host.UpdateLayout();
                        var lastTab = tabItems.OrderBy(item => tabs!.Items.IndexOf(item)).Last();
                        var lastTabOrigin = lastTab.TransformToAncestor(headerScroller).Transform(new Point(0, 0));
                        var lastTabBottom = lastTabOrigin.Y + lastTab.ActualHeight;
                        var chrome = FindVisualChildren<FrameworkElement>(lastTab)
                            .FirstOrDefault(element => element.Name == "Chrome");
                        var chromeBottom = double.NaN;
                        var chromeSafety = double.NaN;
                        if (chrome != null)
                        {
                            var chromeOrigin = chrome.TransformToAncestor(headerScroller).Transform(new Point(0, 0));
                            chromeBottom = chromeOrigin.Y + chrome.ActualHeight;
                            chromeSafety = lastTab.ActualHeight - chrome.ActualHeight;
                        }
                        report.AppendLine(
                            $"  SettingsLayout w={width:0} h={height:0} lastTabBottom={lastTabBottom:0.##} chromeBottom={chromeBottom:0.##} chromeSafety={chromeSafety:0.##} viewport={headerScroller.ViewportHeight:0.##} scrollable={headerScroller.ScrollableHeight:0.##}");
                        if (lastTabBottom > headerScroller.ViewportHeight + 1 || chromeBottom > headerScroller.ViewportHeight + 1 || chromeSafety < 1)
                        {
                            s_problems.Add($"SettingsLayout w={width:0} h={height:0} last category chrome lacks bottom safety (tab={lastTabBottom:0.##}, chrome={chromeBottom:0.##}, safety={chromeSafety:0.##}, viewport={headerScroller.ViewportHeight:0.##})");
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
                            Background = new SolidColorBrush(Color.FromRgb(24, 30, 43)),
                            ClipToBounds = true
                        };
                        host.Children.Add(view);
                        ApplyThemeResponsive(view, contentW, windowH);
                        host.Measure(new Size(contentW, contentH));
                        host.Arrange(new Rect(0, 0, contentW, contentH));
                        host.UpdateLayout();
                        ApplyThemeResponsive(view, contentW, windowH);
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
        var palette = AdaptiveThemePaletteFactory.Create(view, false, 50, mode);
        AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(view.Resources, palette, false, false);
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
            if (grid.ActualHeight < 236 && grid.Name != "MaintenanceAuditLogGrid")
                s_problems.Add($"{label} {grid.Name} viewport {grid.ActualHeight:0} DIP (<236)");
        }

        foreach (var list in FindVisualChildren<ListBox>(host))
        {
            if (string.IsNullOrEmpty(list.Name) || list.ActualHeight <= 0)
                continue;
            if (list.ActualHeight < 236 && list.Name != "OverviewActivityList")
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
                    Background = new SolidColorBrush(Color.FromRgb(24, 30, 43)),
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
                    ApplyThemeResponsive(view, contentW, windowH);
                    host.Measure(new Size(contentW, contentH));
                    host.Arrange(new Rect(0, 0, contentW, contentH));
                    host.UpdateLayout();
                    ApplyThemeResponsive(view, contentW, windowH);
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
        metrics.Add(new ElementMetric
        {
            Key = kind + "|" + name,
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
        string? innerTabHeader = null)
    {
        try
        {
            var view = createView();
            var host = new Grid
            {
                Width = 900,
                Height = height,
                Background = new SolidColorBrush(Color.FromRgb(24, 30, 43)),
                ClipToBounds = true
            };
            host.Children.Add(view);
            applyLayout(view);
            host.Measure(new Size(900, height));
            host.Arrange(new Rect(0, 0, 900, height));
            host.UpdateLayout();
            if (tabIndex >= 0)
            {
                var tabs = FindVisualChildren<TabControl>(host).FirstOrDefault();
                if (tabs != null && tabIndex < tabs.Items.Count)
                    tabs.SelectedIndex = tabIndex;
                host.UpdateLayout();
                if (!string.IsNullOrEmpty(innerTabHeader))
                {
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
            if (!VirtualizingPanel.GetIsVirtualizing(grid) || VirtualizingPanel.GetVirtualizationMode(grid) != VirtualizationMode.Recycling)
                s_problems.Add($"{label} {gridName} h={height:0} virtualization/recycling is disabled");

            grid.ScrollIntoView(grid.Items[grid.Items.Count - 1]);
            host.UpdateLayout();

            foreach (var fraction in new[] { 0.0, 0.25, 0.5, 0.75, 1.0 })
            {
                scroller.ScrollToVerticalOffset(scroller.ScrollableHeight * fraction);
                host.UpdateLayout();
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
                    $"  {label} {gridName} h={height:0} pos={positionLabel} offset={scroller.VerticalOffset:0.##} " +
                    $"scrollable={scroller.ScrollableHeight:0.##} rows={rows.Count} " +
                    $"firstY={(rows.Count > 0 ? rows[0].Y : double.NaN):0.##} gap={headerGap:0.##} presenterH={(presenter?.ActualHeight ?? double.NaN):0.##} gridH={grid.ActualHeight:0.##}");

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
                    if (lastRow == null)
                    {
                        s_problems.Add($"{label} {gridName} h={height:0} bottom last row not realized");
                    }
                    else
                    {
                        var bottom = lastRow.Y + lastRow.Height;
                        if (bottom > grid.ActualHeight + 1)
                            s_problems.Add($"{label} {gridName} h={height:0} bottom last row clipped (bottom={bottom:0.##} gridH={grid.ActualHeight:0.##})");
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

            var scrollable = scroller.ScrollableHeight;
            var offset = scroller.VerticalOffset;
            if (scrollable < 0 || offset > scrollable + 1 || offset < scrollable - 1)
                s_problems.Add($"{label} {gridName} h={height:0} scroll bottom invalid (offset={offset:0.##} scrollable={scrollable:0.##})");
        }
        catch (Exception ex)
        {
            s_problems.Add($"{label} scroll probe failed at height {height:0}: {ex.Message}");
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
}
