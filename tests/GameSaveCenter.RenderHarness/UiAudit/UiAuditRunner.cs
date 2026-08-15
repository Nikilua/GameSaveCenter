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
using GameSaveCenter.Playnite.Infrastructure;
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
            var fidelityCodes = new[]
            {
                "TEXT_FIT",
                "HEADER_CONTENT_FIDELITY",
                "ACTIVE_TAB_VISIBILITY",
                "CONTROL_USABILITY_GEOMETRY",
                "ESSENTIAL_COLUMN_VISIBILITY"
            };
            var fidelityFailures = result.Warnings
                .Where(warning => fidelityCodes.Contains(warning.Code))
                .GroupBy(warning => warning.Code + "|" + warning.RouteId + "|" + warning.Tab + "|" + warning.SizeKey + "|" + warning.Message)
                .Count();
            if (fidelityFailures > 0)
            {
                Console.WriteLine("FIDELITY FAILURES " + fidelityFailures);
                foreach (var warning in result.Warnings.Where(warning => fidelityCodes.Contains(warning.Code)).Take(60))
                    Console.WriteLine("  " + warning.RouteId + "/" + warning.Tab + "/" + warning.SizeKey + ": " + warning.Message);
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
        host.UpdateLayout();

        var tabControls = FindVisualChildren<TabControl>(host)
            .Where(tabControl => !HasTabControlAncestor(tabControl, host))
            .ToList();
        if (tabControls.Count == 0)
        {
            RenderTab(
                result,
                route,
                view,
                host,
                null,
                -1,
                -1,
                "页面",
                MakeSlug(route.RouteId, "page"),
                GetExpectedPrimary(route.RouteId, "页面", null),
                size,
                outputRoot,
                applyLayout,
                log);
            return;
        }

        foreach (var tabControl in tabControls)
        {
            var tabCount = tabControl.Items.Count;
            for (var i = 0; i < tabCount; i++)
            {
                var tabItem = tabControl.Items[i] as TabItem;
                var header = ResolveTabHeader(tabItem, i);
                tabControl.SelectedIndex = i;
                host.UpdateLayout();
                applyLayout();
                host.UpdateLayout();
                if (route.IsSettings)
                    RevealSettingsShell(host);

                var selectedContent = tabItem?.Content as FrameworkElement ?? view;
                var nestedTabControls = FindVisualChildren<TabControl>(selectedContent)
                    .Where(nested => IsDescendantOf(nested, selectedContent))
                    .ToList();
                if (nestedTabControls.Count == 0)
                {
                    RenderTab(
                        result,
                        route,
                        view,
                        host,
                        tabControl,
                        i,
                        -1,
                        header,
                        MakeSlug(route.RouteId, header),
                        GetExpectedPrimary(route.RouteId, header, null),
                        size,
                        outputRoot,
                        applyLayout,
                        log);
                    continue;
                }

                foreach (var nestedTabControl in nestedTabControls)
                {
                    var nestedCount = nestedTabControl.Items.Count;
                    for (var inner = 0; inner < nestedCount; inner++)
                    {
                        var nestedItem = nestedTabControl.Items[inner] as TabItem;
                        var nestedHeader = ResolveTabHeader(nestedItem, inner);
                        nestedTabControl.SelectedIndex = inner;
                        host.UpdateLayout();
                        applyLayout();
                        host.UpdateLayout();
                        if (route.IsSettings)
                            RevealSettingsShell(host);
                        RenderTab(
                            result,
                            route,
                            view,
                            host,
                            nestedTabControl,
                            i,
                            inner,
                            header + " / " + nestedHeader,
                            MakeSlug(route.RouteId, header, nestedHeader),
                            GetExpectedPrimary(route.RouteId, header, nestedHeader),
                            size,
                            outputRoot,
                            applyLayout,
                            log);
                    }
                }
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
        int innerTabIndex,
        string tabHeader,
        string routeSlug,
        string expectedPrimaryElement,
        UiSizeRecord size,
        string outputRoot,
        Action applyLayout,
        StringBuilder log)
    {
        var prefix = SafeFileName(routeSlug);
        var screenshotsDir = Path.Combine(outputRoot, "screenshots", size.Key);
        var visualTreeDir = Path.Combine(outputRoot, "visual-tree", size.Key);
        var layoutDir = Path.Combine(outputRoot, "layout", size.Key);
        Directory.CreateDirectory(screenshotsDir);
        Directory.CreateDirectory(visualTreeDir);
        Directory.CreateDirectory(layoutDir);

        var actualPrimary = ResolveActualPrimaryElement(host, expectedPrimaryElement);
        if (!string.IsNullOrEmpty(expectedPrimaryElement)
            && !string.Equals(actualPrimary, expectedPrimaryElement, StringComparison.OrdinalIgnoreCase))
        {
            var message = $"{routeSlug} expected primary {expectedPrimaryElement}, actual {actualPrimary}";
            result.FailedRoutes.Add(message);
            log.AppendLine("FAILED ROUTE " + message);
            return;
        }

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

        var layout = UiLayoutAnalyzer.Analyze(
            host,
            route.RouteId,
            tabHeader,
            size.Key,
            size.ContentWidth,
            size.ContentHeight,
            routeSlug,
            expectedPrimaryElement,
            actualPrimary);
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
        }
        else
        {
            var method = view.GetType().GetMethod(
                "ApplyResponsiveLayout",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null)
                method.Invoke(view, new object[] { width, height });
        }

        foreach (var grid in FindVisualChildren<DataGrid>(view))
            DataGridStarFill.Redistribute(grid);
    }

    private static void RevealSettingsShell(DependencyObject root)
    {
        var shell = FindVisualChildren<FrameworkElement>(root)
            .FirstOrDefault(element => element.Name == "SettingsShell");
        if (shell != null)
            shell.Opacity = 1;
    }

    private static bool HasTabControlAncestor(TabControl tabControl, DependencyObject root)
    {
        var parent = VisualTreeHelper.GetParent(tabControl);
        while (parent != null && !ReferenceEquals(parent, root))
        {
            if (parent is TabControl)
                return true;
            parent = VisualTreeHelper.GetParent(parent);
        }
        return false;
    }

    private static bool IsDescendantOf(DependencyObject current, DependencyObject ancestor)
    {
        var parent = VisualTreeHelper.GetParent(current);
        while (parent != null)
        {
            if (ReferenceEquals(parent, ancestor))
                return true;
            parent = VisualTreeHelper.GetParent(parent);
        }
        return false;
    }

    private static string ResolveTabHeader(TabItem? tabItem, int index)
    {
        if (tabItem == null)
            return "Tab " + index;
        if (tabItem.Header is string text && !string.IsNullOrWhiteSpace(text))
            return text;
        if (tabItem.Header is FrameworkElement headerElement)
        {
            var candidates = FindVisualChildren<TextBlock>(headerElement)
                .Where(tb => !string.IsNullOrWhiteSpace(tb.Text))
                .OrderByDescending(tb => tb.Text.Length)
                .ToList();
            var textBlock = candidates.FirstOrDefault(tb =>
                    tb.Text.Any(ch => ch >= 0x4E00 && ch <= 0x9FFF))
                ?? candidates.FirstOrDefault(tb =>
                    !tb.Text.All(ch => ch >= 0xE000 && ch <= 0xF8FF));
            if (textBlock != null)
                return textBlock.Text.Trim();
        }
        return "Tab " + index;
    }

    private static string MakeSlug(params string[] parts)
    {
        var slug = string.Join(
            "-",
            parts
                .Select(part => SafeFileName((part ?? string.Empty).Trim().Replace(' ', '-')))
                .Where(part => !string.IsNullOrWhiteSpace(part)));
        return string.IsNullOrWhiteSpace(slug) ? "page" : slug;
    }

    private static string GetExpectedPrimary(string routeId, string outerHeader, string? innerHeader)
    {
        switch (routeId)
        {
            case "maintenance":
                if (outerHeader == "诊断")
                    return innerHeader == "问题列表" ? "FindingsGrid" : string.Empty;
                if (outerHeader == "设备状态")
                    return "MaintenanceDeviceGrid";
                if (outerHeader == "异常与审计")
                    return innerHeader == "发现的问题"
                        ? "MaintenanceAuditFindingsGrid"
                        : innerHeader == "审计记录"
                            ? "MaintenanceAuditLogGrid"
                            : string.Empty;
                if (outerHeader == "进程映射")
                    return "MaintenanceProcessGrid";
                return string.Empty;
            case "save-center":
                if (outerHeader == "历史版本")
                    return "SaveHistoryGrid";
                if (outerHeader == "路径与校验")
                    return "SaveCandidateGrid";
                return string.Empty;
            case "media-center":
                if (outerHeader == "待归类")
                    return "MediaInboxGrid";
                if (outerHeader == "当前游戏媒体")
                    return "MediaGrid";
                return string.Empty;
            case "task-center":
                return "TaskGrid";
            case "trainer-center":
                return outerHeader == "已绑定工具" ? "TrainerToolsList" : string.Empty;
            default:
                return string.Empty;
        }
    }

    private static string ResolveActualPrimaryElement(DependencyObject root, string expected)
    {
        if (!string.IsNullOrEmpty(expected))
        {
            var expectedElement = FindVisualChildren<FrameworkElement>(root)
                .FirstOrDefault(element =>
                    element.Name == expected
                    && element.Visibility == Visibility.Visible
                    && element.ActualHeight > 0
                    && (element is DataGrid || element is ListBox));
            return expectedElement?.Name ?? string.Empty;
        }

        var first = FindVisualChildren<FrameworkElement>(root)
            .FirstOrDefault(element =>
                element.Visibility == Visibility.Visible
                && element.ActualHeight > 0
                && (element is DataGrid || element is ListBox));
        return first?.Name ?? string.Empty;
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
