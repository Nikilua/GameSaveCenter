using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GameSaveCenter.Playnite.Views;

namespace GameSaveCenter.RenderHarness;

/// <summary>
/// Offscreen layout QA for the production workspace views. It renders each page at the
/// logical content sizes produced by the Dashboard shell for common 1080p/2K/4K windows
/// and writes PNGs plus a measurable scroll/viewport report for later comparison.
/// </summary>
public static class Program
{
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

        try
        {
            foreach (var (windowW, windowH) in WindowSizes)
            {
                var (contentW, contentH) = ContentSize(windowW, windowH);
                report.AppendLine($"Window {windowW}x{windowH} -> workspace {contentW:0}x{contentH:0} DIP");

                RenderOverview(outputRoot, windowW, windowH, contentW, contentH, report);
                RenderMedia(outputRoot, windowW, windowH, contentW, contentH, report);
                RenderMaintenance(outputRoot, windowW, windowH, contentW, contentH, report);
                RenderTasks(outputRoot, windowW, windowH, contentW, contentH, report);
                report.AppendLine();
            }

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
                var stack = contentW < 900;
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
            SavePng(host, Path.Combine(outputRoot, $"{name}-{windowW}x{windowH}-tab{i}.png"));
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
        SavePng(host, Path.Combine(outputRoot, $"{name}-{windowW}x{windowH}.png"));
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
        }

        foreach (var list in FindVisualChildren<ListBox>(host))
        {
            if (string.IsNullOrEmpty(list.Name))
                continue;
            report.AppendLine($"  {label} {list.Name}: size={list.ActualWidth:0}x{list.ActualHeight:0}, items={list.Items.Count}");
        }
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
