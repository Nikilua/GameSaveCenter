using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace GameSaveCenter.RenderHarness.UiAudit;

/// <summary>
/// Static source audit. It reads the real XAML under Views/Settings and produces a
/// stable route map plus a zero-omission element manifest without changing production UI.
/// </summary>
public static class UiStaticManifestBuilder
{
    private static readonly XNamespace XamlNamespace =
        "http://schemas.microsoft.com/winfx/2006/xaml";

    private static readonly HashSet<string> RelevantTypes = new HashSet<string>
    {
        "Button",
        "ToggleButton",
        "CheckBox",
        "RadioButton",
        "ComboBox",
        "TextBox",
        "PasswordBox",
        "DataGrid",
        "ListBox",
        "ListView",
        "ItemsControl",
        "TreeView",
        "TabControl",
        "TabItem",
        "Expander",
        "ScrollViewer",
        "ProgressBar",
        "Slider",
        "Border",
        "TextBlock",
        "Label",
        "ContentControl",
        "Menu",
        "MenuItem"
    };

    public static UiStaticManifest Build(string repositoryRoot)
    {
        var manifest = new UiStaticManifest
        {
            GeneratedUtc = DateTime.UtcNow.ToString("O"),
            RepositoryRoot = repositoryRoot
        };

        var viewsRoot = Path.Combine(repositoryRoot, "src", "GameSaveCenter.Playnite", "Views");
        var settingsRoot = Path.Combine(repositoryRoot, "src", "GameSaveCenter.Playnite", "Settings");

        var dashboardPath = Path.Combine(viewsRoot, "DashboardView.xaml");
        if (File.Exists(dashboardPath))
            BuildRouteFromFile(manifest, dashboardPath, "dashboard", "Dashboard 外壳", isDashboard: true);

        foreach (var file in Directory.EnumerateFiles(viewsRoot, "*.xaml", SearchOption.AllDirectories)
                     .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            if (string.Equals(Path.GetFileName(file), "DashboardView.xaml", StringComparison.OrdinalIgnoreCase))
                continue;
            BuildRouteFromFile(manifest, file, null, null, isDashboard: false);
        }

        if (Directory.Exists(settingsRoot))
        {
            foreach (var file in Directory.EnumerateFiles(settingsRoot, "*.xaml", SearchOption.TopDirectoryOnly)
                         .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                BuildRouteFromFile(manifest, file, "settings", "设置", isDashboard: false);
            }
        }

        ApplySummary(manifest);
        return manifest;
    }

    private static void BuildRouteFromFile(
        UiStaticManifest manifest,
        string file,
        string? forcedRouteId,
        string? forcedWorkspace,
        bool isDashboard)
    {
        XDocument document;
        try
        {
            document = XDocument.Load(file, LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);
        }
        catch (XmlException)
        {
            return;
        }

        var root = document.Root;
        if (root == null)
            return;

        var viewType = GetAttr(root, "Class") ?? Path.GetFileNameWithoutExtension(file);
        var fileName = Path.GetFileNameWithoutExtension(file);
        var friendly = GetFriendlyRouteInfo(fileName);
        var routeId = forcedRouteId ?? friendly.RouteId;
        var workspace = forcedWorkspace ?? friendly.Workspace;

        var route = new UiRouteNode
        {
            RouteId = routeId,
            Workspace = workspace,
            ViewFile = Relativize(manifest.RepositoryRoot, file),
            ViewType = viewType,
            SourceFile = Relativize(manifest.RepositoryRoot, file),
            SourceLine = GetLine(root)
        };

        AddTabs(route, root, routeId);

        // The Dashboard shell owns navigation, header, footer and dialog surfaces. The
        // individual workspace files are discovered separately, so both layers stay visible
        // in the manifest and future pages added to either layer are picked up automatically.
        if (isDashboard)
        {
            AddElementsToRoute(route, root, null, routeId, manifest.RepositoryRoot);
        }
        else
        {
            foreach (var tabControl in root.Descendants().Where(IsTabControl))
            {
                var tabIndex = 0;
                foreach (var tabElement in tabControl.Elements().Where(IsTabItem))
                {
                    var header = GetTabHeader(tabElement);
                    var existing = route.Tabs.FirstOrDefault(tab =>
                        string.Equals(tab.Header, header, StringComparison.OrdinalIgnoreCase));
                    if (existing != null)
                    {
                        AddElementsToRoute(route, tabElement, existing, routeId, manifest.RepositoryRoot);
                        tabIndex++;
                        continue;
                    }

                    var record = new UiTabRecord
                    {
                        Header = header,
                        Index = tabIndex++,
                        RouteId = routeId,
                        SourceFile = route.SourceFile,
                        SourceLine = GetLine(tabElement)
                    };
                    AddElementsToRoute(route, tabElement, record, routeId, manifest.RepositoryRoot);
                    route.Tabs.Add(record);
                }
            }
        }

        if (route.Tabs.Count == 0 && !isDashboard)
        {
            var pageTab = new UiTabRecord
            {
                Header = "页面",
                Index = 0,
                RouteId = routeId,
                SourceFile = route.SourceFile,
                SourceLine = 1
            };
            AddElementsToRoute(route, root, pageTab, routeId, manifest.RepositoryRoot);
            route.Tabs.Add(pageTab);
        }

        manifest.Routes.Add(route);
        manifest.Elements.AddRange(route.Elements);
        manifest.Elements.AddRange(route.Tabs.SelectMany(tab => tab.Elements));
    }

    private static void AddTabs(UiRouteNode route, XElement root, string routeId)
    {
        foreach (var tabControl in root.Descendants().Where(IsTabControl))
        {
            var tabIndex = 0;
            foreach (var tabElement in tabControl.Elements().Where(IsTabItem))
            {
                var header = GetTabHeader(tabElement);
                if (route.Tabs.Any(tab => string.Equals(tab.Header, header, StringComparison.OrdinalIgnoreCase)))
                    continue;
                route.Tabs.Add(new UiTabRecord
                {
                    Header = header,
                    Index = tabIndex++,
                    RouteId = routeId,
                    SourceFile = route.SourceFile,
                    SourceLine = GetLine(tabElement)
                });
            }
        }
    }

    private static void AddElementsToRoute(
        UiRouteNode route,
        XElement root,
        UiTabRecord? tab,
        string routeId,
        string repositoryRoot)
    {
        foreach (var element in root.DescendantsAndSelf())
        {
            var localName = element.Name.LocalName;
            if (!RelevantTypes.Contains(localName))
                continue;

            var record = BuildElementRecord(element, repositoryRoot);
            if (record == null)
                continue;

            record.Conditional = IsConditional(element);
            if (record.Conditional)
                record.Condition = DescribeCondition(element);

            if (tab != null)
                tab.Elements.Add(record);
            else
                route.Elements.Add(record);

            switch (localName)
            {
                case "DataGrid":
                    var gridRecord = BuildDataGridRecord(element, repositoryRoot);
                    if (gridRecord != null)
                    {
                        if (tab != null) tab.DataGrids.Add(gridRecord);
                        else route.Tabs.FirstOrDefault()?.DataGrids.Add(gridRecord);
                    }
                    break;
                case "ScrollViewer":
                    var scrollerRecord = BuildScrollViewerRecord(element, repositoryRoot);
                    if (scrollerRecord != null)
                    {
                        if (tab != null) tab.ScrollViewers.Add(scrollerRecord);
                        else route.Tabs.FirstOrDefault()?.ScrollViewers.Add(scrollerRecord);
                    }
                    break;
                case "Expander":
                    var expanderRecord = BuildExpanderRecord(element, repositoryRoot);
                    if (expanderRecord != null)
                    {
                        if (tab != null) tab.Expanders.Add(expanderRecord);
                        else route.Tabs.FirstOrDefault()?.Expanders.Add(expanderRecord);
                    }
                    break;
            }

            if (record.Conditional)
            {
                if (tab != null) tab.ConditionalElements.Add(record);
                else route.Tabs.FirstOrDefault()?.ConditionalElements.Add(record);
            }
        }
    }

    private static UiElementRecord? BuildElementRecord(XElement element, string repositoryRoot)
    {
        var name = GetXName(element);
        var record = new UiElementRecord
        {
            Type = element.Name.LocalName,
            Name = name,
            Text = ExtractText(element),
            Header = GetAttr(element, "Header") ?? string.Empty,
            Binding = GetBindingExpression(element, "Text", "ItemsSource", "SelectedItem", "IsChecked", "Value", "Content"),
            Command = GetBindingExpression(element, "Command"),
            CommandParameter = GetBindingExpression(element, "CommandParameter"),
            VisibilityBinding = GetBindingExpression(element, "Visibility"),
            IsEnabledBinding = GetBindingExpression(element, "IsEnabled"),
            Style = GetAttr(element, "Style") ?? string.Empty,
            GridRow = GetAttr(element, "Grid.Row") ?? string.Empty,
            GridColumn = GetAttr(element, "Grid.Column") ?? string.Empty,
            GridRowSpan = GetAttr(element, "Grid.RowSpan") ?? string.Empty,
            GridColumnSpan = GetAttr(element, "Grid.ColumnSpan") ?? string.Empty,
            Margin = GetAttr(element, "Margin") ?? string.Empty,
            Padding = GetAttr(element, "Padding") ?? string.Empty,
            Width = GetAttr(element, "Width") ?? string.Empty,
            Height = GetAttr(element, "Height") ?? string.Empty,
            MinWidth = GetAttr(element, "MinWidth") ?? string.Empty,
            MaxWidth = GetAttr(element, "MaxWidth") ?? string.Empty,
            MinHeight = GetAttr(element, "MinHeight") ?? string.Empty,
            MaxHeight = GetAttr(element, "MaxHeight") ?? string.Empty,
            HorizontalAlignment = GetAttr(element, "HorizontalAlignment") ?? string.Empty,
            VerticalAlignment = GetAttr(element, "VerticalAlignment") ?? string.Empty,
            ParentType = element.Parent?.Name.LocalName ?? string.Empty,
            ParentName = element.Parent == null ? string.Empty : GetXName(element.Parent),
            SourceFile = Relativize(repositoryRoot, element.Document?.BaseUri ?? string.Empty),
            SourceLine = GetLine(element)
        };
        return record;
    }

    private static UiDataGridRecord? BuildDataGridRecord(XElement element, string repositoryRoot)
    {
        var columns = element.Descendants()
            .Where(child => child.Name.LocalName.EndsWith("Column", StringComparison.Ordinal))
            .ToList();
        var record = new UiDataGridRecord
        {
            Name = GetXName(element),
            ItemsSource = GetBindingExpression(element, "ItemsSource"),
            SelectedItem = GetBindingExpression(element, "SelectedItem"),
            ColumnCount = columns.Count,
            VirtualizingPanel = string.Join("; ", element.Descendants()
                .Where(child => child.Name.LocalName == "VirtualizingStackPanel" || child.Name.LocalName == "VirtualizingPanel")
                .Select(child => child.Name.LocalName)
                .Distinct()),
            ScrollViewer = string.Join("; ", element.Descendants()
                .Where(child => child.Name.LocalName == "ScrollViewer")
                .Select(child => GetXName(child))
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct()),
            SourceFile = Relativize(repositoryRoot, element.Document?.BaseUri ?? string.Empty),
            SourceLine = GetLine(element)
        };
        foreach (var column in columns)
        {
            record.Columns.Add(new UiDataGridColumnRecord
            {
                Header = GetAttr(column, "Header") ?? string.Empty,
                Binding = GetBindingExpression(column, "Binding"),
                Width = GetAttr(column, "Width") ?? string.Empty,
                MinWidth = GetAttr(column, "MinWidth") ?? string.Empty,
                MaxWidth = GetAttr(column, "MaxWidth") ?? string.Empty,
                CanUserResize = GetAttr(column, "CanUserResize") ?? string.Empty,
                HeaderStyle = GetAttr(column, "HeaderStyle") ?? string.Empty,
                SourceLine = GetLine(column)
            });
        }
        return record;
    }

    private static UiScrollViewerRecord BuildScrollViewerRecord(XElement element, string repositoryRoot)
    {
        return new UiScrollViewerRecord
        {
            Name = GetXName(element),
            HorizontalScrollBarVisibility = GetAttr(element, "HorizontalScrollBarVisibility") ?? string.Empty,
            VerticalScrollBarVisibility = GetAttr(element, "VerticalScrollBarVisibility") ?? string.Empty,
            ParentScrollViewers = string.Join(" > ", element.Ancestors()
                .Where(ancestor => ancestor.Name.LocalName == "ScrollViewer")
                .Select(GetXName)
                .Where(name => !string.IsNullOrEmpty(name))),
            ContainsDataGridOrListBox = element.Descendants()
                .Any(child => child.Name.LocalName == "DataGrid" || child.Name.LocalName == "ListBox"),
            SourceFile = Relativize(repositoryRoot, element.Document?.BaseUri ?? string.Empty),
            SourceLine = GetLine(element)
        };
    }

    private static UiExpanderRecord BuildExpanderRecord(XElement element, string repositoryRoot)
    {
        return new UiExpanderRecord
        {
            Name = GetXName(element),
            Header = GetAttr(element, "Header") ?? string.Empty,
            IsExpanded = GetAttr(element, "IsExpanded") ?? string.Empty,
            SourceFile = Relativize(repositoryRoot, element.Document?.BaseUri ?? string.Empty),
            SourceLine = GetLine(element)
        };
    }

    private static bool IsConditional(XElement element)
    {
        var visibility = GetAttr(element, "Visibility");
        if (visibility != null && (visibility.Contains("{Binding") || visibility.Contains("{StaticResource") || visibility.Contains("{DynamicResource")))
            return true;

        var enabled = GetAttr(element, "IsEnabled");
        if (enabled != null && enabled.Contains("{Binding"))
            return true;

        return element.Descendants()
            .Any(child => child.Name.LocalName is "DataTrigger" or "MultiDataTrigger" or "Trigger");
    }

    private static string DescribeCondition(XElement element)
    {
        var parts = new List<string>();
        var visibility = GetAttr(element, "Visibility");
        if (!string.IsNullOrEmpty(visibility))
            parts.Add("Visibility=" + visibility);
        var enabled = GetAttr(element, "IsEnabled");
        if (!string.IsNullOrEmpty(enabled))
            parts.Add("IsEnabled=" + enabled);
        parts.AddRange(element.Descendants()
            .Where(child => child.Name.LocalName is "DataTrigger" or "MultiDataTrigger" or "Trigger")
            .Take(5)
            .Select(trigger =>
            {
                var binding = GetBindingExpression(trigger, "Binding", "Value");
                return trigger.Name.LocalName + ":" + binding;
            }));
        return string.Join(" | ", parts.Where(part => !string.IsNullOrEmpty(part)));
    }

    private static string GetBindingExpression(XElement element, params string[] attributeNames)
    {
        foreach (var attributeName in attributeNames)
        {
            var attribute = element.Attributes()
                .FirstOrDefault(attr => attr.Name.LocalName == attributeName);
            if (attribute == null)
                continue;
            var value = attribute.Value;
            if (string.IsNullOrWhiteSpace(value))
                continue;
            var match = Regex.Match(value, @"\{Binding\s+(?<path>[^,}]+)");
            if (match.Success)
                return "{Binding " + match.Groups["path"].Value.Trim() + "}";
            if (value.StartsWith("{Binding", StringComparison.Ordinal))
                return value;
        }

        foreach (var child in element.Elements())
        {
            if (!child.Name.LocalName.EndsWith("Binding", StringComparison.Ordinal))
                continue;
            var path = child.Attribute("Path")?.Value ?? child.Attribute("Value")?.Value ?? string.Empty;
            if (!string.IsNullOrEmpty(path))
                return "{Binding " + path + "}";
        }

        return string.Empty;
    }

    private static string GetTabHeader(XElement tabElement)
    {
        var header = GetAttr(tabElement, "Header");
        if (!string.IsNullOrWhiteSpace(header))
            return header.Trim();

        var candidates = tabElement.Descendants()
            .Where(child => child.Name.LocalName == "TextBlock" || child.Name.LocalName == "Run")
            .Select(ExtractText)
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .ToList();
        return candidates.FirstOrDefault(text => text.Any(char.IsLetterOrDigit))
            ?? candidates.FirstOrDefault()
            ?? "Tab " + GetLine(tabElement);
    }

    private static (string RouteId, string Workspace) GetFriendlyRouteInfo(string fileName)
    {
        return fileName switch
        {
            "DashboardView" => ("dashboard", "Dashboard 外壳"),
            "OverviewView" => ("overview", "首页"),
            "SaveCenterView" => ("save-center", "存档中心"),
            "TrainerCenterView" => ("trainer-center", "修改器中心"),
            "MediaCenterView" => ("media-center", "媒体中心"),
            "TaskCenterView" => ("task-center", "任务中心"),
            "MaintenanceView" => ("maintenance", "维护中心"),
            "UiFrameworkProbeView" => ("dev-probe", "Development 探针"),
            _ => (Slugify(fileName), fileName)
        };
    }

    private static string ExtractText(XElement element)
    {
        var direct = GetAttr(element, "Text") ?? GetAttr(element, "Content") ?? GetAttr(element, "Header");
        if (!string.IsNullOrWhiteSpace(direct))
            return direct.Trim();

        foreach (var run in element.Descendants().Where(child => child.Name.LocalName == "Run"))
        {
            var text = GetAttr(run, "Text");
            if (!string.IsNullOrWhiteSpace(text))
                return text.Trim();
        }

        foreach (var textBlock in element.Descendants().Where(child => child.Name.LocalName == "TextBlock"))
        {
            var text = GetAttr(textBlock, "Text");
            if (!string.IsNullOrWhiteSpace(text))
                return text.Trim();
        }

        return string.Empty;
    }

    private static void ApplySummary(UiStaticManifest manifest)
    {
        var elements = manifest.Elements;
        var tabs = manifest.Routes.SelectMany(route => route.Tabs).ToList();
        manifest.Summary.ViewCount = manifest.Routes.Count;
        manifest.Summary.TabCount = tabs.Count;
        manifest.Summary.ButtonCount = elements.Count(element => element.Type is "Button" or "ToggleButton");
        manifest.Summary.DataGridCount = elements.Count(element => element.Type == "DataGrid");
        manifest.Summary.ScrollViewerCount = elements.Count(element => element.Type == "ScrollViewer");
        manifest.Summary.ConditionalUiCount = elements.Count(element => element.Conditional);
        manifest.Summary.ExpanderCount = elements.Count(element => element.Type == "Expander");
        manifest.Summary.TextBlockCount = elements.Count(element => element.Type == "TextBlock");
        manifest.Summary.ComboBoxCount = elements.Count(element => element.Type == "ComboBox");
        manifest.Summary.CheckBoxCount = elements.Count(element => element.Type == "CheckBox");
        manifest.Summary.TextBoxCount = elements.Count(element => element.Type == "TextBox" || element.Type == "PasswordBox");
    }

    private static bool IsTabControl(XElement element)
        => element.Name.LocalName == "TabControl";

    private static bool IsTabItem(XElement element)
        => element.Name.LocalName == "TabItem";

    private static string GetAttr(XElement element, string localName)
        => element.Attributes()
            .FirstOrDefault(attr => attr.Name.LocalName == localName)?.Value ?? string.Empty;

    private static string GetXName(XElement element)
        => element.Attribute(XamlNamespace + "Name")?.Value ?? string.Empty;

    private static int GetLine(XObject element)
    {
        if (element is IXmlLineInfo lineInfo && lineInfo.HasLineInfo())
            return lineInfo.LineNumber;
        return 0;
    }

    private static string Relativize(string root, string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;
        var full = Path.GetFullPath(path);
        var normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        if (full.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            return full.Substring(normalizedRoot.Length);
        return path;
    }

    private static string Slugify(string value)
    {
        var slug = Regex.Replace(value, "[^A-Za-z0-9]+", "-").Trim('-');
        return string.IsNullOrEmpty(slug) ? "view" : slug.ToLowerInvariant();
    }
}
