using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.Views;
using Newtonsoft.Json;

namespace GameSaveCenter.RenderHarness.UiAudit;

public static class UiAuditRunner
{
    public static int Run(string outputRoot)
    {
        outputRoot = Path.GetFullPath(outputRoot);
        Directory.CreateDirectory(outputRoot);
        foreach (var sub in new[] { "screenshots", "visual-tree", "layout", "raw" })
            Directory.CreateDirectory(Path.Combine(outputRoot, sub));

        var log = new StringBuilder();
        var result = new UiAuditRunResult();
        var repositoryRoot = FindRepositoryRoot();
        result.Metadata.OutputRoot = outputRoot;
        result.Metadata.ZipPath = Path.Combine(
            Path.GetDirectoryName(outputRoot.TrimEnd(Path.DirectorySeparatorChar)) ?? outputRoot,
            "GameSaveCenter-ui-audit.zip");

        try
        {
            var app = new Application();
            app.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));

            result.Manifest = UiStaticManifestBuilder.Build(repositoryRoot);
            result.Metadata.GeneratedUtc = DateTime.UtcNow.ToString("O");
            result.Metadata.CommitSha = GetCommitSha(repositoryRoot);
            result.Metadata.PluginVersion =
                typeof(DashboardView).Assembly.GetName().Version?.ToString() ?? "unknown";
            result.Metadata.PlayniteVersion =
                typeof(global::Playnite.SDK.IPlayniteAPI).Assembly.GetName().Version?.ToString() ?? "unknown";
            result.Metadata.WindowsVersion = Environment.OSVersion.VersionString;
            result.Metadata.DpiScale = 1.0;

            var sizes = BuildSizes();
            result.Metadata.Sizes.AddRange(sizes);

            var routes = UiPageRouteFactory.CreateRuntimeRoutes();
            log.AppendLine("UI audit started");
            log.AppendLine("Repository: " + repositoryRoot);
            log.AppendLine("Commit: " + result.Metadata.CommitSha);
            log.AppendLine("Plugin: " + result.Metadata.PluginVersion);
            log.AppendLine("Playnite SDK: " + result.Metadata.PlayniteVersion);
            log.AppendLine("Windows: " + result.Metadata.WindowsVersion);
            log.AppendLine("Static routes: " + result.Manifest.Routes.Count);
            log.AppendLine("Runtime routes: " + routes.Count);

            foreach (var size in sizes)
            {
                log.AppendLine($"Size {size.Key}: {size.ActualWidth:0}x{size.ActualHeight:0} DIP");
                foreach (var route in routes)
                {
                    try
                    {
                        RenderRoute(result, route, size, outputRoot, log);
                    }
                    catch (Exception ex)
                    {
                        var message = $"{route.RouteId} failed: {ex.Message}";
                        result.FailedRoutes.Add(message);
                        log.AppendLine("FAILED " + message);
                        log.AppendLine(ex.ToString());
                    }
                }
            }

            UiReportWriter.WriteAll(result, outputRoot);
            WriteMetadata(result, outputRoot);
            WriteLog(result, outputRoot, log);

            CreateZip(result, outputRoot);
            result.ZipPath = result.Metadata.ZipPath;

            Console.WriteLine("UI audit complete");
            Console.WriteLine("Output: " + outputRoot);
            Console.WriteLine("Zip: " + result.Metadata.ZipPath);
            if (result.FailedRoutes.Count > 0)
            {
                Console.WriteLine("FAILED ROUTES");
                foreach (var failed in result.FailedRoutes)
                    Console.WriteLine("  " + failed);
                return 1;
            }
            return 0;
        }
        catch (Exception ex)
        {
            log.AppendLine("FATAL " + ex);
            WriteLog(result, outputRoot, log);
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static List<UiSizeRecord> BuildSizes()
    {
        var sizes = new List<UiSizeRecord>();
        var workArea = SystemParameters.WorkArea;
        var maximized = new UiSizeRecord
        {
            Key = "maximized",
            RequestedWidth = Math.Round(workArea.Width, 0),
            RequestedHeight = Math.Round(workArea.Height, 0),
            ActualWidth = Math.Max(1040, Math.Round(workArea.Width, 0)),
            ActualHeight = Math.Max(700, Math.Round(workArea.Height, 0)),
            Note = "SystemParameters.WorkArea"
        };
        sizes.Add(maximized);
        sizes.AddRange(new[]
        {
            NewSize("2k", 2560, 1440),
            NewSize("wide", 1920, 1080),
            NewSize("standard", 1440, 900),
            NewSize("compact", 1200, 760),
            NewSize("narrow-1100", 1100, 720),
            NewSize("narrow", 1040, 700)
        });
        foreach (var size in sizes)
        {
            var content = ContentSize(size.ActualWidth, size.ActualHeight);
            size.ContentWidth = Math.Round(content.Width, 0);
            size.ContentHeight = Math.Round(content.Height, 0);
        }
        return sizes;
    }

    private static UiSizeRecord NewSize(string key, double width, double height)
        => new UiSizeRecord
        {
            Key = key,
            RequestedWidth = width,
            RequestedHeight = height,
            ActualWidth = width,
            ActualHeight = height,
            Note = "logical DIP"
        };

    private static (double Width, double Height) ContentSize(double windowW, double windowH)
    {
        var expanded = windowW >= 1280;
        var sidebar = expanded ? 228d : 204d;
        var gutter = 16d;
        var shellInset = 56d;
        var measuredWidth = Math.Max(320d, windowW - shellInset - sidebar - gutter);
        var cardPadding = expanded ? 24d : 20d;
        var contentW = Math.Max(320d, measuredWidth - cardPadding);
        var contentH = Math.Max(320d, windowH - 240d);
        return (contentW, contentH);
    }

    private static void RenderRoute(
        UiAuditRunResult result,
        UiRuntimeRoute route,
        UiSizeRecord size,
        string outputRoot,
        StringBuilder log)
    {
        UserControl view;
        try
        {
            view = (UserControl)Activator.CreateInstance(route.ViewType);
            view.DataContext = route.IsSettings
                ? new GameSaveCenterSettings()
                : new FakeDashboardData();
        }
        catch (Exception ex)
        {
            result.FailedRoutes.Add($"{route.RouteId} cannot instantiate: {ex.Message}");
            log.AppendLine($"SKIP {route.RouteId} cannot instantiate: {ex.Message}");
            return;
        }

        var host = new Grid
        {
            Width = size.ContentWidth,
            Height = size.ContentHeight,
            Background = new SolidColorBrush(Color.FromRgb(24, 30, 43)),
            ClipToBounds = true
        };
        host.Children.Add(view);

        // Dashboard and its workspace views receive the full shell height (not the
        // reduced workspace content height) when they apply responsive layout. Keep
        // the audit consistent with production and render-qa, otherwise stacked
        // inspector/table budgets are computed against a shorter synthetic height.
        Action applyLayout = () => ApplyLayout(view, route, size.ContentWidth, size.ActualHeight);
        applyLayout();
        host.Measure(new Size(size.ContentWidth, size.ContentHeight));
        host.Arrange(new Rect(0, 0, size.ContentWidth, size.ContentHeight));
        host.UpdateLayout();
        applyLayout();
        host.UpdateLayout();

        var tabControls = FindVisualChildren<TabControl>(host).ToList();
        if (tabControls.Count == 0)
        {
            RenderTab(result, route, view, host, null, -1, "页面", size, outputRoot, applyLayout, log);
            return;
        }

        foreach (var tabControl in tabControls)
        {
            var tabCount = tabControl.Items.Count;
            for (var i = 0; i < tabCount; i++)
            {
                var tabItem = tabControl.Items[i] as TabItem;
                var header = tabItem?.Header?.ToString() ?? "Tab " + i;
                tabControl.SelectedIndex = i;
                host.UpdateLayout();
                applyLayout();
                host.UpdateLayout();
                if (route.IsSettings)
                    RevealSettingsShell(host);
                RenderTab(result, route, view, host, tabControl, i, header, size, outputRoot, applyLayout, log);
            }
        }
    }

    private static void RenderTab(
        UiAuditRunResult result,
        UiRuntimeRoute route,
        UserControl view,
        Grid host,
        TabControl? tabControl,
        int tabIndex,
        string tabHeader,
        UiSizeRecord size,
        string outputRoot,
        Action applyLayout,
        StringBuilder log)
    {
        var safeRoute = SafeFileName(route.RouteId);
        var safeTab = tabIndex < 0 ? "page" : "tab" + tabIndex;
        var prefix = safeRoute + "-" + safeTab;
        var screenshotsDir = Path.Combine(outputRoot, "screenshots", size.Key);
        var visualTreeDir = Path.Combine(outputRoot, "visual-tree", size.Key);
        var layoutDir = Path.Combine(outputRoot, "layout", size.Key);
        Directory.CreateDirectory(screenshotsDir);
        Directory.CreateDirectory(visualTreeDir);
        Directory.CreateDirectory(layoutDir);

        var snapshot = new UiRuntimeSnapshot
        {
            RouteId = route.RouteId,
            TabHeader = tabHeader,
            SizeKey = size.Key
        };

        var viewportPng = Path.Combine(screenshotsDir, prefix + ".png");
        UiScreenshotService.SavePng(host, viewportPng);
        snapshot.ViewportPng = RelativeToOutput(outputRoot, viewportPng);

        var visualTree = UiVisualTreeInspector.Inspect(host);
        var visualTreeJson = Path.Combine(visualTreeDir, prefix + ".json");
        File.WriteAllText(visualTreeJson, UiAuditSanitizer.SanitizeJson(JsonConvert.SerializeObject(visualTree, Formatting.Indented)));
        snapshot.VisualTreeJson = RelativeToOutput(outputRoot, visualTreeJson);

        var layout = UiLayoutAnalyzer.Analyze(host, route.RouteId, tabHeader, size.Key, size.ContentWidth, size.ContentHeight);
        var layoutJson = Path.Combine(layoutDir, prefix + ".json");
        File.WriteAllText(layoutJson, UiAuditSanitizer.SanitizeJson(JsonConvert.SerializeObject(layout, Formatting.Indented)));
        snapshot.LayoutJson = RelativeToOutput(outputRoot, layoutJson);
        result.LayoutReports.Add(layout);
        result.Warnings.AddRange(layout.Warnings);

        var scrollerIndex = 0;
        foreach (var scroller in FindVisualChildren<ScrollViewer>(host))
        {
            if (scroller.ExtentHeight <= scroller.ViewportHeight + 0.5)
                continue;
            if (scroller.ActualHeight < 20 || scroller.ScrollableHeight < 10)
                continue;
            if (IsTextInputScroller(scroller, host))
                continue;
            var isInternal = IsInternalScroller(scroller, host);
            if (isInternal)
                continue;
            var scrollerName = string.IsNullOrEmpty(scroller.Name) ? "scroller" + scrollerIndex : scroller.Name;
            var path = Path.Combine(screenshotsDir, prefix + "-full-" + SafeFileName(scrollerName) + ".png");
            var capture = UiScreenshotService.CaptureScrollViewerFull(scroller, path);
            if (capture != null)
            {
                snapshot.FullPagePngs.Add(RelativeToOutput(outputRoot, path));
                log.AppendLine(
                    $"  {route.RouteId}/{tabHeader} {size.Key} full-scroll {scrollerName}: {capture.Width}x{capture.Height} slices={capture.SliceCount}");
            }
            scrollerIndex++;
        }

        var internalIndex = 0;
        foreach (var grid in FindVisualChildren<DataGrid>(host))
        {
            if (grid.Items.Count == 0 || grid.ActualHeight <= 0)
                continue;
            var gridName = string.IsNullOrEmpty(grid.Name) ? "grid" + internalIndex : grid.Name;
            var path = Path.Combine(screenshotsDir, prefix + "-scroll-" + SafeFileName(gridName) + ".png");
            var capture = UiScreenshotService.CaptureDataGridFull(grid, path);
            if (capture != null)
                snapshot.FullScrollPngs.Add(RelativeToOutput(outputRoot, path));
            internalIndex++;
        }

        var listIndex = 0;
        foreach (var list in FindVisualChildren<ListBox>(host))
        {
            if (list.Items.Count == 0 || list.ActualHeight <= 0)
                continue;
            var listName = string.IsNullOrEmpty(list.Name) ? "list" + listIndex : list.Name;
            var path = Path.Combine(screenshotsDir, prefix + "-scroll-" + SafeFileName(listName) + ".png");
            var capture = UiScreenshotService.CaptureListBoxFull(list, path);
            if (capture != null)
                snapshot.FullScrollPngs.Add(RelativeToOutput(outputRoot, path));
            listIndex++;
        }

        result.Snapshots.Add(snapshot);
    }

    private static bool IsInternalScroller(DependencyObject current, DependencyObject root)
    {
        var parent = VisualTreeHelper.GetParent(current);
        while (parent != null && !ReferenceEquals(parent, root))
        {
            if (parent is DataGrid || parent is ListBox)
                return true;
            parent = VisualTreeHelper.GetParent(parent);
        }
        return false;
    }

    private static bool IsTextInputScroller(DependencyObject current, DependencyObject root)
    {
        var parent = VisualTreeHelper.GetParent(current);
        while (parent != null && !ReferenceEquals(parent, root))
        {
            if (parent is TextBox || parent is PasswordBox || parent is ComboBox)
                return true;
            parent = VisualTreeHelper.GetParent(parent);
        }
        return false;
    }

    private static void ApplyLayout(UserControl view, UiRuntimeRoute route, double width, double height)
    {
        if (route.IsKnown && route.ViewType == typeof(OverviewView))
        {
            var overview = (OverviewView)view;
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
        if (method != null)
            method.Invoke(view, new object[] { width, height });
    }

    private static void RevealSettingsShell(DependencyObject root)
    {
        var shell = FindVisualChildren<FrameworkElement>(root)
            .FirstOrDefault(element => element.Name == "SettingsShell");
        if (shell != null)
            shell.Opacity = 1;
    }

    private static string RelativeToOutput(string outputRoot, string path)
    {
        var root = Path.GetFullPath(outputRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var full = Path.GetFullPath(path);
        return full.StartsWith(root, StringComparison.OrdinalIgnoreCase)
            ? full.Substring(root.Length)
            : path;
    }

    private static string SafeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = value.Select(ch => invalid.Contains(ch) ? '-' : ch).ToArray();
        return new string(chars);
    }

    private static string GetCommitSha(string repositoryRoot)
    {
        var env = Environment.GetEnvironmentVariable("GSC_UI_AUDIT_COMMIT");
        if (!string.IsNullOrWhiteSpace(env))
            return env.Trim();
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "rev-parse --short HEAD",
                WorkingDirectory = repositoryRoot,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(startInfo);
            if (process == null)
                return "unknown";
            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            return string.IsNullOrEmpty(output) ? "unknown" : output;
        }
        catch
        {
            return "unknown";
        }
    }

    private static void WriteMetadata(UiAuditRunResult result, string outputRoot)
    {
        var path = Path.Combine(outputRoot, "audit-metadata.json");
        File.WriteAllText(path, JsonConvert.SerializeObject(result.Metadata, Formatting.Indented));
        File.WriteAllText(
            Path.Combine(outputRoot, "raw", "manifest-raw.json"),
            UiAuditSanitizer.SanitizeJson(JsonConvert.SerializeObject(result.Manifest, Formatting.Indented)));
    }

    private static void WriteLog(UiAuditRunResult result, string outputRoot, StringBuilder log)
    {
        var path = Path.Combine(outputRoot, "ui-audit.log");
        result.LogPath = path;
        File.WriteAllText(path, UiAuditSanitizer.Sanitize(log.ToString()));
    }

    private static void CreateZip(UiAuditRunResult result, string outputRoot)
    {
        var zipPath = result.Metadata.ZipPath;
        if (File.Exists(zipPath))
            File.Delete(zipPath);
        System.IO.Compression.ZipFile.CreateFromDirectory(
            outputRoot,
            zipPath,
            System.IO.Compression.CompressionLevel.Optimal,
            includeBaseDirectory: false);
    }

    private static string FindRepositoryRoot()
    {
        var directory = AppContext.BaseDirectory;
        for (var i = 0; i < 16; i++)
        {
            if (File.Exists(Path.Combine(directory, "GameSaveCenter.sln")))
                return directory;
            var parent = Directory.GetParent(directory);
            if (parent == null)
                break;
            directory = parent.FullName;
        }
        return AppContext.BaseDirectory;
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
