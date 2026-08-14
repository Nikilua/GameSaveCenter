using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace GameSaveCenter.RenderHarness.UiAudit;

public static class UiLayoutAnalyzer
{
    public static UiLayoutReport Analyze(
        DependencyObject root,
        string routeId,
        string tabHeader,
        string sizeKey,
        double width,
        double height)
    {
        var report = new UiLayoutReport
        {
            RouteId = routeId,
            TabHeader = tabHeader,
            SizeKey = sizeKey,
            Width = Math.Round(width, 2),
            Height = Math.Round(height, 2)
        };

        AnalyzeScrollViewers(root, report);
        AnalyzeDataGrids(root, report);
        AnalyzeListBoxes(root, report);
        AnalyzeToolbars(root, report);
        AnalyzeClipping(root, report);
        return report;
    }

    private static void AnalyzeScrollViewers(DependencyObject root, UiLayoutReport report)
    {
        foreach (var scroller in FindVisualChildren<ScrollViewer>(root))
        {
            var scrollable = scroller.ExtentHeight > scroller.ViewportHeight + 0.5
                || scroller.ExtentWidth > scroller.ViewportWidth + 0.5;
            var parentChain = BuildParentChain(scroller, root);
            var isInternal = IsInside(scroller, root, typeof(DataGrid), typeof(ListBox));
            var containsList = FindVisualChildren<DataGrid>(scroller).Any()
                || FindVisualChildren<ListBox>(scroller).Any();
            var record = new UiRuntimeScrollViewer
            {
                Name = scroller.Name,
                ActualWidth = Math.Round(scroller.ActualWidth, 2),
                ActualHeight = Math.Round(scroller.ActualHeight, 2),
                ViewportWidth = Math.Round(scroller.ViewportWidth, 2),
                ViewportHeight = Math.Round(scroller.ViewportHeight, 2),
                ExtentWidth = Math.Round(scroller.ExtentWidth, 2),
                ExtentHeight = Math.Round(scroller.ExtentHeight, 2),
                ScrollableWidth = Math.Round(scroller.ScrollableWidth, 2),
                ScrollableHeight = Math.Round(scroller.ScrollableHeight, 2),
                VerticalOffset = Math.Round(scroller.VerticalOffset, 2),
                HorizontalOffset = Math.Round(scroller.HorizontalOffset, 2),
                VerticalScrollBarVisibility = scroller.VerticalScrollBarVisibility.ToString(),
                HorizontalScrollBarVisibility = scroller.HorizontalScrollBarVisibility.ToString(),
                ParentChain = parentChain,
                ContainsDataGridOrListBox = containsList,
                IsInternalToDataGridOrListBox = isInternal,
                Scrollable = scrollable
            };
            report.ScrollViewers.Add(record);

            if (scroller.ExtentHeight > scroller.ViewportHeight + 0.5
                && scroller.ActualHeight >= 20
                && !IsInside(scroller, root, typeof(TextBox), typeof(PasswordBox), typeof(ComboBox))
                && HasScrollableScrollViewerAncestor(scroller, root, isVertical: true))
            {
                report.Warnings.Add(new UiAuditWarning
                {
                    Severity = "INFO",
                    Code = "NESTED_VERTICAL_SCROLL",
                    RouteId = report.RouteId,
                    Tab = report.TabHeader,
                    SizeKey = report.SizeKey,
                    Message = $"嵌套纵向滚动上下文：{scroller.Name} (chain={parentChain})"
                });
            }
        }
    }

    private static void AnalyzeDataGrids(DependencyObject root, UiLayoutReport report)
    {
        foreach (var grid in FindVisualChildren<DataGrid>(root))
        {
            var headerHeight = grid.ColumnHeaderHeight;
            if (double.IsNaN(headerHeight) || headerHeight <= 0)
            {
                var headers = FindVisualChildren<DataGridColumnHeadersPresenter>(grid).FirstOrDefault();
                headerHeight = headers != null && headers.ActualHeight > 0 ? headers.ActualHeight : 32;
            }

            var row = FindVisualChildren<DataGridRow>(grid).FirstOrDefault();
            var rowHeight = row != null && row.ActualHeight > 0 ? row.ActualHeight : 36;
            var visibleRows = grid.ActualHeight > headerHeight
                ? (grid.ActualHeight - headerHeight) / rowHeight
                : 0;

            var record = new UiRuntimeDataGrid
            {
                Name = grid.Name,
                ActualWidth = Math.Round(grid.ActualWidth, 2),
                ActualHeight = Math.Round(grid.ActualHeight, 2),
                ColumnCount = grid.Columns.Count,
                ItemsCount = grid.Items.Count,
                ColumnHeaderHeight = Math.Round(headerHeight, 2),
                EstimatedRowHeight = Math.Round(rowHeight, 2),
                EstimatedVisibleRows = Math.Round(visibleRows, 2),
                Virtualization = $"IsVirtualizing={VirtualizingPanel.GetIsVirtualizing(grid)},Mode={VirtualizingPanel.GetVirtualizationMode(grid)}"
            };

            var totalMinWidth = 0d;
            foreach (var column in grid.Columns)
            {
                var minWidth = column.MinWidth;
                var maxWidth = column.MaxWidth;
                var width = column.ActualWidth;
                totalMinWidth += minWidth;
                record.Columns.Add(new UiRuntimeDataGridColumn
                {
                    Header = column.Header?.ToString() ?? string.Empty,
                    ActualWidth = Math.Round(width, 2),
                    Width = column.Width.ToString(),
                    MinWidth = minWidth,
                    MaxWidth = maxWidth
                });
                if (width < minWidth - 0.5)
                {
                    record.Warnings.Add("POSSIBLE_COLUMN_PRESSURE");
                }
            }

            if (grid.ActualHeight > 0 && (visibleRows < 4 || grid.ActualHeight < 236))
            {
                record.Warnings.Add("TABLE_VIEWPORT_TOO_SHORT");
                report.Warnings.Add(new UiAuditWarning
                {
                    Severity = "HIGH",
                    Code = "TABLE_VIEWPORT_TOO_SHORT",
                    RouteId = report.RouteId,
                    Tab = report.TabHeader,
                    SizeKey = report.SizeKey,
                    Message = $"{grid.Name} 估算可见行数 {visibleRows:0.0}，实际高度 {grid.ActualHeight:0} DIP"
                });
            }
            if (grid.ActualWidth > 0 && totalMinWidth > grid.ActualWidth + 0.5)
            {
                report.Warnings.Add(new UiAuditWarning
                {
                    Severity = "MEDIUM",
                    Code = "POSSIBLE_COLUMN_PRESSURE",
                    RouteId = report.RouteId,
                    Tab = report.TabHeader,
                    SizeKey = report.SizeKey,
                    Message = $"{grid.Name} 列最小宽度合计 {totalMinWidth:0} DIP 超过实际宽度 {grid.ActualWidth:0} DIP"
                });
            }
            report.DataGrids.Add(record);
        }
    }

    private static void AnalyzeListBoxes(DependencyObject root, UiLayoutReport report)
    {
        foreach (var list in FindVisualChildren<ListBox>(root))
        {
            report.ListBoxes.Add(new UiRuntimeListBox
            {
                Name = list.Name,
                ActualWidth = Math.Round(list.ActualWidth, 2),
                ActualHeight = Math.Round(list.ActualHeight, 2),
                ItemsCount = list.Items.Count,
                Virtualization = $"IsVirtualizing={VirtualizingPanel.GetIsVirtualizing(list)},Mode={VirtualizingPanel.GetVirtualizationMode(list)}"
            });
        }
    }

    private static void AnalyzeToolbars(DependencyObject root, UiLayoutReport report)
    {
        foreach (var panel in FindVisualChildren<Panel>(root)
                     .Where(panel =>
                         panel is WrapPanel
                         || (panel is StackPanel stack && stack.Orientation == Orientation.Horizontal)))
        {
            if (panel.ActualHeight <= 90 || panel.Children.Count < 2)
                continue;
            report.Toolbars.Add(new UiRuntimeToolbar
            {
                Name = panel.Name ?? string.Empty,
                Type = panel.GetType().Name,
                ActualHeight = Math.Round(panel.ActualHeight, 2),
                ChildrenCount = panel.Children.Count,
                Expanded = true
            });
            report.Warnings.Add(new UiAuditWarning
            {
                Severity = "MEDIUM",
                Code = "TOOLBAR_VERTICAL_EXPANSION",
                RouteId = report.RouteId,
                Tab = report.TabHeader,
                SizeKey = report.SizeKey,
                Message = $"{panel.Name ?? panel.GetType().Name} 高度 {panel.ActualHeight:0} DIP，包含 {panel.Children.Count} 个子元素"
            });
        }
    }

    private static void AnalyzeClipping(DependencyObject root, UiLayoutReport report)
    {
        foreach (var text in FindVisualChildren<TextBlock>(root))
        {
            if (!text.IsVisible || text.ActualWidth <= 0 || text.ActualHeight <= 0)
                continue;
            if (text.TextWrapping == TextWrapping.Wrap || text.TextTrimming != TextTrimming.None)
                continue;
            if (text.DesiredSize.Width > text.ActualWidth + 2)
            {
                report.Warnings.Add(new UiAuditWarning
                {
                    Severity = "INFO",
                    Code = "POSSIBLE_CLIPPING",
                    RouteId = report.RouteId,
                    Tab = report.TabHeader,
                    SizeKey = report.SizeKey,
                    Message = $"{text.Name ?? text.GetType().Name} 期望宽度 {text.DesiredSize.Width:0} DIP 大于实际宽度 {text.ActualWidth:0} DIP"
                });
            }
        }
    }

    private static bool HasScrollableScrollViewerAncestor(
        DependencyObject current,
        DependencyObject root,
        bool isVertical)
    {
        var parent = VisualTreeHelper.GetParent(current);
        while (parent != null && !ReferenceEquals(parent, root))
        {
            if (parent is ScrollViewer scroller)
            {
                if (isVertical && scroller.ExtentHeight > scroller.ViewportHeight + 0.5)
                    return true;
                if (!isVertical && scroller.ExtentWidth > scroller.ViewportWidth + 0.5)
                    return true;
            }
            parent = VisualTreeHelper.GetParent(parent);
        }
        return false;
    }

    private static bool IsInside(DependencyObject current, DependencyObject root, params Type[] types)
    {
        var parent = VisualTreeHelper.GetParent(current);
        while (parent != null && !ReferenceEquals(parent, root))
        {
            var parentType = parent.GetType();
            foreach (var type in types)
            {
                if (type.IsAssignableFrom(parentType))
                    return true;
            }
            parent = VisualTreeHelper.GetParent(parent);
        }
        return false;
    }

    private static string BuildParentChain(DependencyObject current, DependencyObject root)
    {
        var chain = new List<string>();
        var parent = VisualTreeHelper.GetParent(current);
        while (parent != null && !ReferenceEquals(parent, root))
        {
            var element = parent as FrameworkElement;
            var name = element == null || string.IsNullOrEmpty(element.Name)
                ? parent.GetType().Name
                : element.Name + " (" + parent.GetType().Name + ")";
            chain.Add(name);
            parent = VisualTreeHelper.GetParent(parent);
        }
        chain.Reverse();
        return string.Join(" > ", chain);
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
