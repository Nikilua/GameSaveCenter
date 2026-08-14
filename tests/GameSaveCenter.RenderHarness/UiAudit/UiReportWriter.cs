using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace GameSaveCenter.RenderHarness.UiAudit;

public static class UiReportWriter
{
    public static void WriteAll(UiAuditRunResult result, string outputRoot)
    {
        WriteRouteMap(result, outputRoot);
        WriteManifest(result, outputRoot);
        WriteLayoutReport(result, outputRoot);
        WriteAuditSummary(result, outputRoot);
        WriteFidelityMatrix(result, outputRoot);
        WriteReadme(result, outputRoot);

        File.WriteAllText(
            Path.Combine(outputRoot, "UI_ROUTE_MAP.json"),
            JsonConvert.SerializeObject(result.Manifest.Routes, Formatting.Indented));
        File.WriteAllText(
            Path.Combine(outputRoot, "UI_MANIFEST.json"),
            JsonConvert.SerializeObject(result.Manifest, Formatting.Indented));
    }

    private static void WriteRouteMap(UiAuditRunResult result, string outputRoot)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# UI Route Map");
        builder.AppendLine();
        builder.AppendLine("生成时间：" + result.Metadata.GeneratedUtc);
        builder.AppendLine("Commit：" + result.Metadata.CommitSha);
        builder.AppendLine();
        builder.AppendLine("路由来自真实 XAML 源码自动发现；新增页面加入 Dashboard 或 Views 目录后会自动出现在本文件。");
        builder.AppendLine();

        foreach (var route in result.Manifest.Routes)
        {
            builder.AppendLine("## " + route.Workspace + " (`" + route.RouteId + "`)");
            builder.AppendLine();
            builder.AppendLine("- View：" + route.ViewType);
            builder.AppendLine("- 文件：" + route.ViewFile);
            builder.AppendLine("- Tab 数量：" + route.Tabs.Count);
            foreach (var tab in route.Tabs)
            {
                builder.AppendLine("  - " + tab.Header);
                if (tab.DataGrids.Count > 0)
                    builder.AppendLine("    - DataGrid：" + string.Join("、", tab.DataGrids.Select(grid => grid.Name)));
                if (tab.ScrollViewers.Count > 0)
                    builder.AppendLine("    - ScrollViewer：" + string.Join("、", tab.ScrollViewers.Select(scroller => scroller.Name)));
                if (tab.ConditionalElements.Count > 0)
                    builder.AppendLine("    - 条件 UI：" + tab.ConditionalElements.Count + " 个");
            }
            builder.AppendLine();
        }

        WriteFile(outputRoot, "UI_ROUTE_MAP.md", builder.ToString());
    }

    private static void WriteManifest(UiAuditRunResult result, string outputRoot)
    {
        var summary = result.Manifest.Summary;
        var builder = new StringBuilder();
        builder.AppendLine("# UI Manifest");
        builder.AppendLine();
        builder.AppendLine("本文件是页面功能完整性的事实来源之一；截图没有显示不代表元素不存在。");
        builder.AppendLine();
        builder.AppendLine("## 汇总");
        builder.AppendLine();
        builder.AppendLine($"- View 数量：{summary.ViewCount}");
        builder.AppendLine($"- Tab 数量：{summary.TabCount}");
        builder.AppendLine($"- Button/ToggleButton 数量：{summary.ButtonCount}");
        builder.AppendLine($"- DataGrid 数量：{summary.DataGridCount}");
        builder.AppendLine($"- ScrollViewer 数量：{summary.ScrollViewerCount}");
        builder.AppendLine($"- 条件 UI 数量：{summary.ConditionalUiCount}");
        builder.AppendLine($"- Expander 数量：{summary.ExpanderCount}");
        builder.AppendLine($"- ComboBox 数量：{summary.ComboBoxCount}");
        builder.AppendLine($"- CheckBox 数量：{summary.CheckBoxCount}");
        builder.AppendLine($"- TextBox/PasswordBox 数量：{summary.TextBoxCount}");
        builder.AppendLine($"- TextBlock 数量：{summary.TextBlockCount}");
        builder.AppendLine();

        foreach (var route in result.Manifest.Routes)
        {
            builder.AppendLine("## " + route.Workspace);
            builder.AppendLine();
            builder.AppendLine("文件：" + route.ViewFile);
            builder.AppendLine();
            foreach (var tab in route.Tabs)
            {
                builder.AppendLine("### " + tab.Header);
                builder.AppendLine();
                WriteElementGroup(builder, "操作与输入", tab.Elements
                    .Where(element => element.Type is "Button" or "ToggleButton" or "CheckBox" or "RadioButton" or "ComboBox" or "TextBox" or "PasswordBox" or "Slider"));
                WriteElementGroup(builder, "数据表", tab.DataGrids
                    .Select(grid => new UiElementRecord
                    {
                        Type = "DataGrid",
                        Name = grid.Name,
                        Text = grid.ColumnCount + " 列",
                        Binding = grid.ItemsSource,
                        SourceFile = grid.SourceFile,
                        SourceLine = grid.SourceLine
                    }));
                WriteElementGroup(builder, "滚动容器", tab.ScrollViewers
                    .Select(scroller => new UiElementRecord
                    {
                        Type = "ScrollViewer",
                        Name = scroller.Name,
                        Text = "V=" + scroller.VerticalScrollBarVisibility + ", H=" + scroller.HorizontalScrollBarVisibility,
                        SourceFile = scroller.SourceFile,
                        SourceLine = scroller.SourceLine
                    }));
                WriteElementGroup(builder, "折叠区域", tab.Expanders
                    .Select(expander => new UiElementRecord
                    {
                        Type = "Expander",
                        Name = expander.Name,
                        Text = expander.Header,
                        SourceFile = expander.SourceFile,
                        SourceLine = expander.SourceLine
                    }));
                WriteElementGroup(builder, "条件显示 UI", tab.ConditionalElements);
                builder.AppendLine();
            }
        }

        WriteFile(outputRoot, "UI_MANIFEST.md", builder.ToString());
    }

    private static void WriteElementGroup(StringBuilder builder, string title, IEnumerable<UiElementRecord> elements)
    {
        var list = elements.ToList();
        if (list.Count == 0)
            return;
        builder.AppendLine("#### " + title);
        builder.AppendLine();
        foreach (var element in list.OrderBy(item => item.Type).ThenBy(item => item.SourceLine))
        {
            var label = element.Name ?? string.Empty;
            var detail = element.Text ?? string.Empty;
            if (string.IsNullOrEmpty(label) && !string.IsNullOrEmpty(detail))
                label = detail;
            var parts = new List<string> { element.Type, label };
            if (!string.IsNullOrEmpty(element.Command))
                parts.Add("Command=" + element.Command);
            if (!string.IsNullOrEmpty(element.VisibilityBinding))
                parts.Add("Visibility=" + element.VisibilityBinding);
            if (!string.IsNullOrEmpty(element.Style))
                parts.Add("Style=" + element.Style);
            if (!string.IsNullOrEmpty(element.SourceFile))
                parts.Add(element.SourceFile + ":" + element.SourceLine);
            builder.AppendLine("- " + string.Join(" | ", parts));
        }
        builder.AppendLine();
    }

    private static void WriteLayoutReport(UiAuditRunResult result, string outputRoot)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Layout Report");
        builder.AppendLine();
        builder.AppendLine("按页面、Tab、窗口尺寸记录运行时 ScrollViewer 与 DataGrid 几何。");
        builder.AppendLine();

        foreach (var group in result.LayoutReports.GroupBy(report => report.RouteId + " / " + report.TabHeader))
        {
            builder.AppendLine("## " + group.Key);
            builder.AppendLine();
            foreach (var report in group.OrderBy(item => item.SizeKey))
            {
                builder.AppendLine("### " + report.SizeKey + " " + report.Width + "x" + report.Height);
                builder.AppendLine();
                if (report.ScrollViewers.Count == 0)
                    builder.AppendLine("无命名/可测量 ScrollViewer。");
                foreach (var scroller in report.ScrollViewers)
                {
                    builder.AppendLine(
                        $"- ScrollViewer {scroller.Name}: {scroller.ActualWidth:0}x{scroller.ActualHeight:0}, viewport={scroller.ViewportHeight:0}, extent={scroller.ExtentHeight:0}, scrollable={scroller.ScrollableHeight:0}, nested={!string.IsNullOrEmpty(scroller.ParentChain)}");
                }
                foreach (var grid in report.DataGrids)
                {
                    builder.AppendLine(
                        $"- DataGrid {grid.Name}: {grid.ActualWidth:0}x{grid.ActualHeight:0}, rows={grid.ItemsCount}, visible~{grid.EstimatedVisibleRows:0.0}, virtualization={grid.Virtualization}");
                    foreach (var warning in grid.Warnings.Distinct())
                        builder.AppendLine("  - " + warning);
                }
                foreach (var list in report.ListBoxes)
                {
                    builder.AppendLine(
                        $"- ListBox {list.Name}: {list.ActualWidth:0}x{list.ActualHeight:0}, items={list.ItemsCount}");
                }
                foreach (var toolbar in report.Toolbars)
                {
                    builder.AppendLine(
                        $"- Toolbar {toolbar.Name}: {toolbar.ActualHeight:0} DIP, children={toolbar.ChildrenCount}");
                }
                var tabWarnings = report.Warnings
                    .Where(warning => warning.RouteId == report.RouteId && warning.Tab == report.TabHeader && warning.SizeKey == report.SizeKey)
                    .ToList();
                if (tabWarnings.Count > 0)
                {
                    builder.AppendLine("Warnings:");
                    foreach (var warning in tabWarnings)
                        builder.AppendLine($"- [{warning.Severity}] {warning.Code}: {warning.Message}");
                }
                builder.AppendLine();
            }
        }

        WriteFile(outputRoot, "LAYOUT_REPORT.md", builder.ToString());
    }

    private static void WriteAuditSummary(UiAuditRunResult result, string outputRoot)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Audit Summary");
        builder.AppendLine();
        builder.AppendLine("生成时间：" + result.Metadata.GeneratedUtc);
        builder.AppendLine("Commit：" + result.Metadata.CommitSha);
        builder.AppendLine();
        builder.AppendLine("## 发现");
        builder.AppendLine();
        builder.AppendLine($"- 静态 View 数量：{result.Manifest.Summary.ViewCount}");
        builder.AppendLine($"- 静态 Tab 数量：{result.Manifest.Summary.TabCount}");
        builder.AppendLine($"- 静态 Button/ToggleButton：{result.Manifest.Summary.ButtonCount}");
        builder.AppendLine($"- 静态 DataGrid：{result.Manifest.Summary.DataGridCount}");
        builder.AppendLine($"- 静态 ScrollViewer：{result.Manifest.Summary.ScrollViewerCount}");
        builder.AppendLine($"- 条件 UI：{result.Manifest.Summary.ConditionalUiCount}");
        builder.AppendLine($"- 运行时快照数量：{result.Snapshots.Count}");
        builder.AppendLine($"- 运行时警告数量：{result.Warnings.Count}");
        builder.AppendLine($"- 失败路由：{result.FailedRoutes.Count}");
        builder.AppendLine();

        foreach (var severity in new[] { "HIGH", "MEDIUM", "INFO" })
        {
            var warnings = result.Warnings.Where(warning => warning.Severity == severity).ToList();
            builder.AppendLine("## " + severity);
            builder.AppendLine();
            if (warnings.Count == 0)
            {
                builder.AppendLine("无。");
                continue;
            }
            foreach (var warning in warnings
                         .GroupBy(item => item.Code + "|" + item.RouteId + "|" + item.Tab + "|" + item.SizeKey)
                         .Select(group => group.First())
                         .OrderBy(item => item.RouteId))
            {
                builder.AppendLine($"- [{warning.Code}] {warning.RouteId}/{warning.Tab}/{warning.SizeKey}: {warning.Message}");
            }
            builder.AppendLine();
        }

        if (result.FailedRoutes.Count > 0)
        {
            builder.AppendLine("## FAILED ROUTES");
            builder.AppendLine();
            foreach (var failed in result.FailedRoutes)
                builder.AppendLine("- " + failed);
            builder.AppendLine();
        }

        WriteFile(outputRoot, "AUDIT_SUMMARY.md", builder.ToString());
    }

    private static void WriteFidelityMatrix(UiAuditRunResult result, string outputRoot)
    {
        var visualNodes = LoadVisualNodes(outputRoot);
        var builder = new StringBuilder();
        builder.AppendLine("# UI Fidelity Matrix");
        builder.AppendLine();
        builder.AppendLine("| Route | Section | Element | Type | Command | Conditional | Screenshot visible |");
        builder.AppendLine("|---|---|---|---|---|---|---|");

        foreach (var route in result.Manifest.Routes)
        {
            foreach (var tab in route.Tabs)
            {
                foreach (var element in tab.Elements
                             .Where(element => element.Type is "Button" or "ToggleButton" or "CheckBox" or "ComboBox" or "TextBox" or "Expander")
                             .OrderBy(element => element.SourceLine))
                {
                    var visible = IsVisibleInSnapshots(route.RouteId, tab.Index, element, visualNodes);
                    builder.AppendLine(
                        $"| {EscapeCell(route.Workspace)} | {EscapeCell(tab.Header)} | {EscapeCell(element.Name ?? element.Text ?? string.Empty)} | {element.Type} | {EscapeCell(element.Command)} | {(element.Conditional ? "Yes" : "No")} | {(visible ? "Yes" : "No")} |");
                }
            }
        }

        WriteFile(outputRoot, "UI_FIDELITY_MATRIX.md", builder.ToString());
    }

    private static void WriteReadme(UiAuditRunResult result, string outputRoot)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# GameSaveCenter UI Audit");
        builder.AppendLine();
        builder.AppendLine("生成时间：" + result.Metadata.GeneratedUtc);
        builder.AppendLine("Commit：" + result.Metadata.CommitSha);
        builder.AppendLine("插件版本：" + result.Metadata.PluginVersion);
        builder.AppendLine("Playnite SDK：" + result.Metadata.PlayniteVersion);
        builder.AppendLine("Windows：" + result.Metadata.WindowsVersion);
        builder.AppendLine();
        builder.AppendLine("## 如何生成");
        builder.AppendLine();
        builder.AppendLine("```powershell");
        builder.AppendLine(".\\scripts\\capture-ui-audit.ps1");
        builder.AppendLine("```");
        builder.AppendLine();
        builder.AppendLine("或双击根目录 `GameSaveCenter-UI-Audit.cmd`。");
        builder.AppendLine();
        builder.AppendLine("## 如何阅读");
        builder.AppendLine();
        builder.AppendLine("- `UI_MANIFEST.md` / `UI_MANIFEST.json`：功能完整性的事实来源，包含页面、Tab、按钮、数据表、滚动容器、条件 UI。");
        builder.AppendLine("- `UI_ROUTE_MAP.md`：当前自动发现的页面结构树。");
        builder.AppendLine("- `UI_FIDELITY_MATRIX.md`：每个入口在截图中是否可见的逐项对照。");
        builder.AppendLine("- `LAYOUT_REPORT.md`：运行时滚动容器和 DataGrid 几何。");
        builder.AppendLine("- `AUDIT_SUMMARY.md`：自动检测到的高/中/低风险与失败路由。");
        builder.AppendLine("- `screenshots/`：窗口视口截图；`-full-*.png` 是从顶到底拼接的整页滚动截图；`-scroll-*.png` 是表格/列表内部滚动内容的完整拼接。");
        builder.AppendLine("- `visual-tree/` / `layout/`：按页面、Tab、窗口尺寸导出的 JSON。");
        builder.AppendLine();
        builder.AppendLine("## 重要边界");
        builder.AppendLine();
        builder.AppendLine("截图未显示的元素不等于页面不存在；条件 UI 请以 `UI_MANIFEST.md` 为准。");
        builder.AppendLine("截图和 JSON 已对用户目录等路径做文本脱敏，但位图截图本身可能仍包含可见路径；对外分享前请自行复核。");
        builder.AppendLine("审计只切换页面/Tab、展开 UI 状态并滚动截图，不执行备份、恢复、删除、迁移、下载或设置保存。");
        builder.AppendLine();
        builder.AppendLine("ZIP：" + result.Metadata.ZipPath);
        builder.AppendLine();

        WriteFile(outputRoot, "README.md", builder.ToString());
    }

    private static List<UiVisualNode> LoadVisualNodes(string outputRoot)
    {
        var nodes = new List<UiVisualNode>();
        var root = Path.Combine(outputRoot, "visual-tree");
        if (!Directory.Exists(root))
            return nodes;
        foreach (var file in Directory.EnumerateFiles(root, "*.json", SearchOption.AllDirectories))
        {
            try
            {
                var parsed = JsonConvert.DeserializeObject<List<UiVisualNode>>(File.ReadAllText(file));
                if (parsed != null)
                    nodes.AddRange(parsed);
            }
            catch
            {
                // A corrupt visual tree file should not prevent the report from being written.
            }
        }
        return nodes;
    }

    private static bool IsVisibleInSnapshots(
        string routeId,
        int tabIndex,
        UiElementRecord element,
        List<UiVisualNode> nodes)
    {
        var prefix = routeId + (tabIndex >= 0 ? "-tab" + tabIndex : "-page");
        var routeNodes = nodes.Where(node =>
            node.Name == element.Name
            || (!string.IsNullOrEmpty(element.Text) && node.Text == element.Text)
            || (string.IsNullOrEmpty(element.Name) && node.Type == element.Type && !string.IsNullOrEmpty(element.Text) && node.Text == element.Text)).ToList();
        if (routeNodes.Count == 0)
            return false;
        return true;
    }

    private static string EscapeCell(string value)
        => value.Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");

    private static void WriteFile(string outputRoot, string fileName, string content)
    {
        File.WriteAllText(
            Path.Combine(outputRoot, fileName),
            UiAuditSanitizer.SanitizeMarkdown(content));
    }
}
