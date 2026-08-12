using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.Views;

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
        (1280, 720),
        (1366, 768),
        (1600, 900),
        (1920, 1080)
    };

    public static int Main(string[] args)
    {
        var exitCode = 0;
        var thread = new Thread(() => { exitCode = Run(args); });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        return exitCode;
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
            report.AppendLine(
                $"  {label} {visibleName}: size={scroller.ActualWidth:0}x{scroller.ActualHeight:0}, viewport={scroller.ViewportHeight:0}, extent={scroller.ExtentHeight:0}, " +
                $"vbar={scroller.VerticalScrollBarVisibility}, scrollable={scrollable}");
            if ((visibleName.Contains("ScrollSurface") || visibleName == "SettingsScroller")
                && scroller.VerticalScrollBarVisibility == ScrollBarVisibility.Hidden
                && scrollable)
            {
                s_problems.Add($"{label} {visibleName} hides overflow behind a Hidden scrollbar (viewport={scroller.ViewportHeight:0}, extent={scroller.ExtentHeight:0})");
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
                view => ((MaintenanceView)view).ApplyResponsiveLayout(900, height));
            ProbeGrid(report, "Maintenance-AuditLog", "MaintenanceAuditLogGrid", 3, height,
                () => new MaintenanceView { DataContext = new FakeDashboardData(60) },
                view => ((MaintenanceView)view).ApplyResponsiveLayout(900, height));
        }
    }

    private static void ProbeGrid(
        StringBuilder report,
        string label,
        string gridName,
        int tabIndex,
        double height,
        Func<UserControl> createView,
        Action<UserControl> applyLayout)
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
                        row.TransformToAncestor(scroller).Transform(new Point(0, 0)).Y))
                    .OrderBy(row => row.Y)
                    .ToList();
                var presenter = FindVisualChildren<DataGridRowsPresenter>(grid).FirstOrDefault();
                var positionLabel = (int)(fraction * 100);
                report.AppendLine(
                    $"  {label} {gridName} h={height:0} pos={positionLabel} offset={scroller.VerticalOffset:0.##} " +
                    $"scrollable={scroller.ScrollableHeight:0.##} rows={rows.Count} " +
                    $"firstY={(rows.Count > 0 ? rows[0].Y : double.NaN):0.##} presenterH={(presenter?.ActualHeight ?? double.NaN):0.##} gridH={grid.ActualHeight:0.##}");

                if (rows.Count == 0 && grid.Items.Count > 0)
                {
                    s_problems.Add($"{label} {gridName} h={height:0} pos={positionLabel} realized no rows");
                    continue;
                }
                if (fraction > 0 && rows.Count > 0 && rows[0].Y > 64)
                    s_problems.Add($"{label} {gridName} h={height:0} pos={positionLabel} blank under header (firstY={rows[0].Y:0.##})");
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
                    if (rows.Count > 0 && rows[0].Y > 64)
                        s_problems.Add($"{label} {gridName} h={height:0} bottom blank under header (firstY={rows[0].Y:0.##})");

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
        public RowProbe(int index, double height, double y)
        {
            Index = index;
            Height = height;
            Y = y;
        }

        public int Index { get; }
        public double Height { get; }
        public double Y { get; }
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
