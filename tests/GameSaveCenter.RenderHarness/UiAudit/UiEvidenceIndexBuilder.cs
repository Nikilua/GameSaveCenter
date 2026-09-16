using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameSaveCenter.RenderHarness.UiAudit;

/// <summary>
/// Builds a short, deterministic index from aggregate audit output to concrete
/// controls and states. It deliberately samples individual entries instead of
/// repeating a single global "passed" statement for an entire page.
/// </summary>
public static class UiEvidenceIndexBuilder
{
    public const int RequiredSampleCount = 20;

    public static string Build(UiAuditRunResult result)
    {
        if (result == null)
            throw new ArgumentNullException(nameof(result));

        var entries = CollectEntries(result);
        if (entries.Count < RequiredSampleCount)
        {
            throw new InvalidOperationException(
                $"证据索引需要至少 {RequiredSampleCount} 个具体控件/状态样本，当前只有 {entries.Count} 个。" );
        }

        var selected = entries.Take(RequiredSampleCount).ToList();
        var builder = new StringBuilder();
        builder.AppendLine("# UI Evidence Index");
        builder.AppendLine();
        builder.AppendLine("本索引从同一轮聚合审计的静态 manifest、运行时布局和 fidelity 输出中抽取具体控件/状态。每行都给出可搜索的结果入口、构建代码身份、样本条件和未验边界；它不是把一个总的自动通过结论扩展成全量签收。");
        builder.AppendLine();
        builder.AppendLine("## 运行身份");
        builder.AppendLine();
        builder.AppendLine("- Commit：`" + (string.IsNullOrWhiteSpace(result.Metadata.CommitSha) ? "unknown" : result.Metadata.CommitSha) + "`");
        builder.AppendLine("- 插件版本：`" + (string.IsNullOrWhiteSpace(result.Metadata.PluginVersion) ? "unknown" : result.Metadata.PluginVersion) + "`");
        builder.AppendLine("- 证据来源：受控 WPF 离屏窗口，逻辑 DPI `" + result.Metadata.DpiScale.ToString("0.##") + "`");
        builder.AppendLine("- 本索引抽样：" + selected.Count + " / 可索引条目 " + entries.Count + "；聚合输出中的静态和运行时条目可继续按同样字段检索。");
        builder.AppendLine();
        builder.AppendLine("## 具体索引");
        builder.AppendLine();
        builder.AppendLine("| ID | 页面 / Tab | 控件 | 状态 | 结果入口 | 代码身份 | 样本 | 未验边界 |");
        builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- |");
        foreach (var entry in selected)
        {
            builder.AppendLine(
                "| " + entry.Id
                + " | " + EscapeCell(entry.Surface)
                + " | " + EscapeCell(entry.Control)
                + " | " + EscapeCell(entry.State)
                + " | " + EscapeCell(entry.Result)
                + " | `" + EscapeCell(entry.Commit) + "`"
                + " | " + EscapeCell(entry.Sample)
                + " | " + EscapeCell(entry.Boundary) + " |");
        }

        builder.AppendLine();
        builder.AppendLine("## 读取规则");
        builder.AppendLine();
        builder.AppendLine("- `UI_MANIFEST.md` / `UI_MANIFEST.json` 是静态控件和源码行入口；`LAYOUT_REPORT.md` 是运行时 DataGrid/ScrollViewer 几何入口；`UI_FIDELITY_MATRIX.md` 是交互入口在审计快照中的可见性入口。");
        builder.AppendLine("- `结果入口` 中的 route、Tab、控件名和 `SourceFile:SourceLine` 是检索键；若该条只有静态入口，说明本轮没有对应的运行时几何样本，不能写成运行时通过。");
        builder.AppendLine("- 该索引只描述当前受控审计样本；真实 Playnite 嵌入、物理 DPI、OS 输入/IME、presented frame、ETW 和宿主性能必须在各自证据中单独确认。");
        return builder.ToString();
    }

    private static List<UiEvidenceIndexEntry> CollectEntries(UiAuditRunResult result)
    {
        var entries = new List<UiEvidenceIndexEntry>();
        var entryNumber = 1;

        foreach (var route in result.Manifest.Routes.OrderBy(item => item.RouteId, StringComparer.Ordinal))
        {
            foreach (var tab in route.Tabs.OrderBy(item => item.Index).ThenBy(item => item.Header, StringComparer.Ordinal))
            {
                var layout = FindPreferredLayout(result, route.RouteId, tab.Header);
                foreach (var grid in tab.DataGrids.OrderBy(item => item.SourceLine).ThenBy(item => item.Name, StringComparer.Ordinal))
                {
                    var runtimeGrid = layout?.DataGrids.FirstOrDefault(item => item.Name == grid.Name);
                    var resultEntry = runtimeGrid == null
                        ? "UI_MANIFEST.md -> " + Surface(route, tab) + " -> DataGrid=" + grid.Name + " -> static source=" + Source(grid.SourceFile, grid.SourceLine)
                        : "LAYOUT_REPORT.md -> " + Surface(route, tab) + " -> size=" + layout!.SizeKey + " -> DataGrid=" + grid.Name;
                    var sample = runtimeGrid == null
                        ? "静态列数=" + grid.ColumnCount + "; ItemsSource=" + EmptyAsUnknown(grid.ItemsSource)
                        : "items=" + runtimeGrid.ItemsCount + "; visible~=" + runtimeGrid.EstimatedVisibleRows.ToString("0.0") + "; virtualization=" + EmptyAsUnknown(runtimeGrid.Virtualization);

                    entries.Add(new UiEvidenceIndexEntry
                    {
                        Id = "E" + entryNumber++.ToString("00"),
                        Surface = Surface(route, tab),
                        Control = "DataGrid " + grid.Name,
                        State = runtimeGrid == null ? "静态结构" : "运行时布局 / " + layout!.SizeKey,
                        Result = resultEntry,
                        Commit = Commit(result),
                        Sample = sample,
                        Boundary = Boundary(runtimeGrid == null, conditional: false)
                    });
                }

                foreach (var element in tab.Elements
                             .Where(IsIndexableControl)
                             .OrderBy(item => item.SourceLine)
                             .ThenBy(item => item.Type, StringComparer.Ordinal)
                             .ThenBy(item => DisplayName(item), StringComparer.Ordinal))
                {
                    var name = DisplayName(element);
                    var conditional = element.Conditional || !string.IsNullOrWhiteSpace(element.VisibilityBinding);
                    var snapshots = result.Snapshots.Count(snapshot => snapshot.RouteId == route.RouteId);
                    entries.Add(new UiEvidenceIndexEntry
                    {
                        Id = "E" + entryNumber++.ToString("00"),
                        Surface = Surface(route, tab),
                        Control = element.Type + " " + name,
                        State = conditional ? "条件状态" : "默认状态",
                        Result = "UI_FIDELITY_MATRIX.md -> " + Surface(route, tab) + " -> element=" + name
                            + "; UI_MANIFEST.md -> source=" + Source(element.SourceFile, element.SourceLine),
                        Commit = Commit(result),
                        Sample = "snapshots=" + snapshots
                            + "; command=" + EmptyAsUnknown(element.Command)
                            + "; visibility=" + EmptyAsUnknown(element.VisibilityBinding),
                        Boundary = Boundary(runtimeObserved: snapshots > 0, conditional)
                    });
                }
            }
        }

        return entries;
    }

    private static UiLayoutReport? FindPreferredLayout(UiAuditRunResult result, string routeId, string tabHeader)
        => result.LayoutReports
            .Where(item => item.RouteId == routeId && item.TabHeader == tabHeader)
            .OrderBy(item => item.SizeKey == "standard" ? 0 : 1)
            .ThenBy(item => item.SizeKey, StringComparer.Ordinal)
            .FirstOrDefault();

    private static bool IsIndexableControl(UiElementRecord element)
        => element.Type is "Button" or "ToggleButton" or "CheckBox" or "ComboBox" or "TextBox"
            or "PasswordBox" or "Slider" or "Expander";

    private static string Surface(UiRouteNode route, UiTabRecord tab)
        => route.RouteId + " / " + (string.IsNullOrWhiteSpace(tab.Header) ? "Tab " + tab.Index : tab.Header);

    private static string DisplayName(UiElementRecord element)
        => !string.IsNullOrWhiteSpace(element.Name)
            ? element.Name
            : !string.IsNullOrWhiteSpace(element.Text)
                ? element.Text
                : "unnamed";

    private static string Source(string file, int line)
        => (string.IsNullOrWhiteSpace(file) ? "unknown" : file) + ":" + line;

    private static string Commit(UiAuditRunResult result)
        => string.IsNullOrWhiteSpace(result.Metadata.CommitSha) ? "unknown" : result.Metadata.CommitSha;

    private static string EmptyAsUnknown(string value)
        => string.IsNullOrWhiteSpace(value) ? "unknown" : value;

    private static string Boundary(bool runtimeObserved, bool conditional)
    {
        var boundary = runtimeObserved
            ? "受控 WPF 离屏 logical DIP；真实 Playnite 宿主、物理 DPI、presented frame、ETW 和宿主性能未验"
            : "仅静态 manifest；没有本轮运行时几何样本，也未验真实 Playnite 宿主、物理 DPI、presented frame、ETW 或性能";
        return conditional ? boundary + "；条件/禁用/错误分支未由本索引主动切换" : boundary;
    }

    private static string EscapeCell(string value)
        => (value ?? string.Empty).Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");

    private sealed class UiEvidenceIndexEntry
    {
        public string Id { get; set; } = string.Empty;
        public string Surface { get; set; } = string.Empty;
        public string Control { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public string Commit { get; set; } = string.Empty;
        public string Sample { get; set; } = string.Empty;
        public string Boundary { get; set; } = string.Empty;
    }
}
