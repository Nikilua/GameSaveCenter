using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using GameSaveCenter.Playnite.Infrastructure;

namespace GameSaveCenter.RenderHarness.UiAudit;

public static class UiLayoutAnalyzer
{
    public static UiLayoutReport Analyze(
        DependencyObject root,
        string routeId,
        string tabHeader,
        string sizeKey,
        double width,
        double height,
        string routeSlug,
        string expectedPrimaryElement,
        string actualPrimaryElement)
    {
        var report = new UiLayoutReport
        {
            RouteId = routeId,
            RouteSlug = routeSlug,
            TabHeader = tabHeader,
            ExpectedPrimaryElement = expectedPrimaryElement,
            ActualPrimaryElement = actualPrimaryElement,
            SizeKey = sizeKey,
            Width = Math.Round(width, 2),
            Height = Math.Round(height, 2)
        };

        AnalyzeScrollViewers(root, report);
        AnalyzeSingleLineTextBoxes(root, report);
        AnalyzeDataGrids(root, report);
        AnalyzeListBoxes(root, report);
        AnalyzeToolbars(root, report);
        AnalyzeClipping(root, report);
        AnalyzeHeaderContentFidelity(root, report);
        AnalyzeActiveTabVisibility(root, report);
        AnalyzeControlUsabilityGeometry(root, report);
        AnalyzeEssentialColumnVisibility(root, report);
        AnalyzeShortSemanticValueTrimming(root, report);
        AnalyzeInteractiveInspectorUsability(root, report);
        AnalyzeVisualCorrectionV2(root, report);
        AnalyzeVerticalFill(root, report);
        return report;
    }

    private static void AnalyzeVisualCorrectionV2(DependencyObject root, UiLayoutReport report)
    {
        var visualRoot = root as Visual;
        if (report.RouteId == "overview")
        {
            var forbidden = new[] { "OverviewPrimaryScrollSurface", "OverviewSecondaryScrollViewer", "OverviewRiskScrollViewer" };
            foreach (var scroller in FindVisualChildren<ScrollViewer>(root))
            {
                if (forbidden.Contains(scroller.Name)
                    && scroller.ScrollableHeight > 0.5
                    && scroller.VerticalScrollBarVisibility != ScrollBarVisibility.Disabled)
                {
                    report.Warnings.Add(new UiAuditWarning
                    {
                        Severity = "HIGH",
                        Code = "OV-001 SINGLE_PAGE_SCROLL",
                        RouteId = report.RouteId,
                        Tab = report.TabHeader,
                        SizeKey = report.SizeKey,
                        Message = $"{scroller.Name} 仍作为 Overview 内部纵向滚动上下文"
                    });
                }
            }

            var riskScroller = FindVisualChildren<ScrollViewer>(root).FirstOrDefault(s => s.Name == "OverviewRiskScrollViewer");
            if (riskScroller != null
                && riskScroller.ScrollableHeight > 0.5
                && riskScroller.VerticalScrollBarVisibility != ScrollBarVisibility.Disabled)
            {
                report.Warnings.Add(new UiAuditWarning
                {
                    Severity = "HIGH",
                    Code = "OV-002 RISK_NO_INTERNAL_SCROLL",
                    RouteId = report.RouteId,
                    Tab = report.TabHeader,
                    SizeKey = report.SizeKey,
                    Message = "风险与提醒内部仍存在纵向滚动"
                });
            }

            foreach (var name in new[] { "OverviewActivityList", "OverviewActivityTimelineList" })
            {
                var container = FindVisualChildren<FrameworkElement>(root).FirstOrDefault(e => e.Name == name);
                if (container == null) continue;
                var internalScroll = FindVisualChildren<ScrollViewer>(container)
                    .Any(s => s.ScrollableHeight > 0.5
                        && s.VerticalScrollBarVisibility != ScrollBarVisibility.Disabled);
                if (internalScroll)
                {
                    report.Warnings.Add(new UiAuditWarning
                    {
                        Severity = "HIGH",
                        Code = "OV-003 ACTIVITY_NO_NESTED_SCROLL",
                        RouteId = report.RouteId,
                        Tab = report.TabHeader,
                        SizeKey = report.SizeKey,
                        Message = $"{name} 与页面滚动产生竞争"
                    });
                }
            }

            var riskCard = FindVisualChildren<FrameworkElement>(root).FirstOrDefault(e => e.Name == "OverviewRiskCard");
            if (riskCard != null && riskCard.ActualHeight > 1000)
            {
                report.Warnings.Add(new UiAuditWarning
                {
                    Severity = "HIGH",
                    Code = "OV-005 RISK_DEAD_SPACE",
                    RouteId = report.RouteId,
                    Tab = report.TabHeader,
                    SizeKey = report.SizeKey,
                    Message = $"风险卡异常过高 ActualHeight={riskCard.ActualHeight:0} DIP"
                });
            }
        }

        if (report.RouteId == "save-center")
        {
            var card = FindVisualChildren<FrameworkElement>(root).FirstOrDefault(e => e.Name == "SaveCurrentRuleCard");
            if (card != null && card.ActualHeight > 190)
            {
                report.Warnings.Add(new UiAuditWarning
                {
                    Severity = "HIGH",
                    Code = "SAVE-001 CURRENT_RULE_CARD",
                    RouteId = report.RouteId,
                    Tab = report.TabHeader,
                    SizeKey = report.SizeKey,
                    Message = $"当前存档规则卡过高：{card.ActualHeight:0} DIP"
                });
            }

            var buttonNames = new[] { "SaveDetectPathsButton", "SaveValidateButton", "SaveLoadDetailsButton" };
            var buttons = buttonNames
                .Select(name => FindVisualChildren<FrameworkElement>(root).FirstOrDefault(e => e.Name == name))
                .Where(e => e != null)
                .ToList();
            if (buttons.Count == 3)
            {
                var minHeight = buttons.Min(b => b.ActualHeight);
                var maxHeight = buttons.Max(b => b.ActualHeight);
                if (maxHeight - minHeight > 2)
                {
                    report.Warnings.Add(new UiAuditWarning
                    {
                        Severity = "HIGH",
                        Code = "SAVE-002 ACTION_GEOMETRY",
                        RouteId = report.RouteId,
                        Tab = report.TabHeader,
                        SizeKey = report.SizeKey,
                        Message = $"三个按钮高度不一致：{minHeight:0}..{maxHeight:0} DIP"
                    });
                }
            }
        }

        if (report.RouteId == "maintenance")
        {
            if (report.TabHeader == "诊断")
            {
                var diagnosticsSurface = FindVisualChildren<FrameworkElement>(root).FirstOrDefault(e => e.Name == "MaintenanceDiagnosticsScrollSurface");
                if (diagnosticsSurface is ScrollViewer diagScroller
                    && diagScroller.ExtentHeight > diagScroller.ViewportHeight + 0.5)
                {
                    report.Warnings.Add(new UiAuditWarning
                    {
                        Severity = "HIGH",
                        Code = "MAINT-001 NO_PARENT_SCROLL",
                        RouteId = report.RouteId,
                        Tab = report.TabHeader,
                        SizeKey = report.SizeKey,
                        Message = "Diagnostics 外层仍作为 FindingsGrid 的可滚 parent"
                    });
                }

                var findings = FindVisualChildren<DataGrid>(root).FirstOrDefault(g => g.Name == "FindingsGrid");
                if (findings != null && visualRoot != null)
                {
                    var top = findings.TransformToAncestor(visualRoot).Transform(new Point(0, 0)).Y;
                    if (top > report.Height + 0.5)
                    {
                        report.Warnings.Add(new UiAuditWarning
                        {
                            Severity = "HIGH",
                            Code = "MAINT-002 FINDINGS_FIRST_VIEWPORT",
                            RouteId = report.RouteId,
                            Tab = report.TabHeader,
                            SizeKey = report.SizeKey,
                            Message = $"FindingsGrid Header 不在初始 viewport：top={top:0} DIP"
                        });
                    }
                }
            }

            if (report.TabHeader == "异常与审计")
            {
                var visibleGrids = FindVisualChildren<DataGrid>(root)
                    .Where(g => g.Visibility == Visibility.Visible && g.ActualHeight > 0)
                    .ToList();
                if (visibleGrids.Count > 1)
                {
                    report.Warnings.Add(new UiAuditWarning
                    {
                        Severity = "HIGH",
                        Code = "MAINT-003 AUDIT_SINGLE_GRID",
                        RouteId = report.RouteId,
                        Tab = report.TabHeader,
                        SizeKey = report.SizeKey,
                        Message = $"Audit 页同一时刻可见主 DataGrid {visibleGrids.Count} 张"
                    });
                }
            }
        }
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
                && !IsInside(scroller, root, typeof(TextBox), typeof(PasswordBox), typeof(ComboBox)))
            {
                var trueParentChild = containsList
                    && !isInternal
                    && scroller.VerticalScrollBarVisibility != ScrollBarVisibility.Disabled
                    && scroller.VerticalScrollBarVisibility != ScrollBarVisibility.Hidden
                    && HasContainedListWithOwnVerticalScroll(scroller);
                if (trueParentChild)
                {
                    report.Warnings.Add(new UiAuditWarning
                    {
                        Severity = "HIGH",
                        Code = "TRUE_PARENT_CHILD_SCROLL_CONFLICT",
                        RouteId = report.RouteId,
                        Tab = report.TabHeader,
                        SizeKey = report.SizeKey,
                        Message = $"真实父子滚动冲突：{scroller.Name} (chain={parentChain})"
                    });
                }
                else if (HasScrollableScrollViewerAncestor(scroller, root, isVertical: true))
                {
                    report.Warnings.Add(new UiAuditWarning
                    {
                        Severity = "INFO",
                        Code = isInternal ? "EXPECTED_SIBLING_SCROLL" : "NESTED_VERTICAL_SCROLL",
                        RouteId = report.RouteId,
                        Tab = report.TabHeader,
                        SizeKey = report.SizeKey,
                        Message = $"嵌套纵向滚动上下文：{scroller.Name} (chain={parentChain})"
                    });
                }
                else if (isInternal)
                {
                    report.Warnings.Add(new UiAuditWarning
                    {
                        Severity = "INFO",
                        Code = "EXPECTED_INTERNAL_SCROLL",
                        RouteId = report.RouteId,
                        Tab = report.TabHeader,
                        SizeKey = report.SizeKey,
                        Message = $"控件内部滚动上下文：{scroller.Name} (chain={parentChain})"
                    });
                }
            }
        }
    }

    private static bool HasContainedListWithOwnVerticalScroll(DependencyObject scroller)
    {
        foreach (var grid in FindVisualChildren<DataGrid>(scroller))
        {
            var visibility = ScrollViewer.GetVerticalScrollBarVisibility(grid);
            if (visibility == ScrollBarVisibility.Auto || visibility == ScrollBarVisibility.Visible)
                return true;
        }

        foreach (var list in FindVisualChildren<ListBox>(scroller))
        {
            var visibility = ScrollViewer.GetVerticalScrollBarVisibility(list);
            if (visibility == ScrollBarVisibility.Auto || visibility == ScrollBarVisibility.Visible)
                return true;
        }

        return false;
    }

    private static void AnalyzeVerticalFill(DependencyObject root, UiLayoutReport report)
    {
        FrameworkElement? workspace = null;
        FrameworkElement? primary = null;

        if (report.RouteId == "task-center")
        {
            workspace = FindVisualChildren<FrameworkElement>(root)
                .FirstOrDefault(element => element.Name == "TaskWorkspaceLayout");
            primary = FindVisualChildren<FrameworkElement>(root)
                .FirstOrDefault(element => element.Name == "TaskGrid");
        }
        else if (report.RouteId == "media-center")
        {
            if (report.TabHeader == "待归类")
            {
                workspace = FindVisualChildren<FrameworkElement>(root)
                    .FirstOrDefault(element => element.Name == "MediaInboxScrollSurface");
                primary = FindVisualChildren<FrameworkElement>(root)
                    .FirstOrDefault(element => element.Name == "MediaInboxGrid");
            }
            else if (report.TabHeader == "当前游戏媒体")
            {
                workspace = FindVisualChildren<FrameworkElement>(root)
                    .FirstOrDefault(element => element.Name == "MediaCurrentLayout");
                primary = FindVisualChildren<FrameworkElement>(root)
                    .FirstOrDefault(element => element.Name == "MediaGrid");
            }
        }

        if (primary == null)
        {
            primary = FindVisualChildren<FrameworkElement>(root)
                .FirstOrDefault(element =>
                    (element is DataGrid || element is ListBox)
                    && element.Visibility == Visibility.Visible
                    && element.ActualHeight > 0);
        }

        double? workspaceHeight = null;
        if (workspace is Grid workspaceGrid)
        {
            if (report.RouteId == "task-center"
                && workspaceGrid.RowDefinitions.Count > 2)
            {
                workspaceHeight = workspaceGrid.RowDefinitions[2].ActualHeight;
            }
            else if (report.RouteId == "media-center"
                && report.TabHeader == "待归类")
            {
                var inner = FindVisualChildren<Grid>(workspaceGrid)
                    .FirstOrDefault(grid => grid.RowDefinitions.Count == 3);
                if (inner != null)
                    workspaceHeight = inner.RowDefinitions[1].ActualHeight;
            }
            else if (report.RouteId == "media-center"
                && report.TabHeader == "当前游戏媒体"
                && workspaceGrid.RowDefinitions.Count > 2)
            {
                workspaceHeight = workspaceGrid.RowDefinitions[2].ActualHeight;
            }
        }
        report.WorkspaceHeight = workspaceHeight.HasValue && workspaceHeight.Value > 0
            ? workspaceHeight.Value
            : workspace != null && workspace.ActualHeight > 0
                ? workspace.ActualHeight
                : report.Height;
        report.MainListHeight = primary?.ActualHeight ?? 0;
        report.VerticalFillRatio = report.WorkspaceHeight > 0
            ? Math.Round(report.MainListHeight / report.WorkspaceHeight, 2)
            : 0;
        report.TopExternalGap = 0;
        report.BottomExternalGap = 0;
        if (workspace != null
            && primary != null
            && workspace.ActualHeight > 0
            && primary.ActualHeight > 0)
        {
            report.TopExternalGap = 0;
            report.BottomExternalGap = Math.Max(
                0,
                Math.Round(report.WorkspaceHeight - primary.ActualHeight, 2));
        }

        var tracked = report.RouteId == "task-center"
            || (report.RouteId == "media-center"
                && (report.TabHeader == "待归类" || report.TabHeader == "当前游戏媒体"));
        if (tracked
            && report.SizeKey is "2k" or "wide" or "maximized"
            && report.VerticalFillRatio > 0
            && report.VerticalFillRatio < 0.92)
        {
            report.Warnings.Add(new UiAuditWarning
            {
                Severity = "HIGH",
                Code = "VERTICAL_FILL_TOO_LOW",
                RouteId = report.RouteId,
                Tab = report.TabHeader,
                SizeKey = report.SizeKey,
                Message = $"主表纵向填充率 {report.VerticalFillRatio:0.00}，要求 >=0.92"
            });
        }
    }

    private static void AnalyzeSingleLineTextBoxes(DependencyObject root, UiLayoutReport report)
    {
        foreach (var textBox in FindVisualChildren<TextBox>(root))
        {
            if (textBox.AcceptsReturn)
                continue;
            var contentHost = FindVisualChildren<ScrollViewer>(textBox)
                .FirstOrDefault(scroller => scroller.Name == "PART_ContentHost");
            if (contentHost == null || contentHost.ScrollableHeight <= 0.5)
                continue;

            report.Warnings.Add(new UiAuditWarning
            {
                Severity = "MEDIUM",
                Code = "SINGLE_LINE_CONTENTHOST_VERTICAL_SCROLL",
                RouteId = report.RouteId,
                Tab = report.TabHeader,
                SizeKey = report.SizeKey,
                Message = $"{textBox.Name} PART_ContentHost ScrollableHeight={contentHost.ScrollableHeight:0.00} DIP"
            });
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
                StarFillEnabled = DataGridStarFill.GetEnabled(grid),
                StarFillApplied = DataGridStarFill.GetApplied(grid),
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

            var columnSum = grid.Columns.Sum(column => column.ActualWidth);
            var usableWidth = grid.ActualWidth;
            var dgScroller = FindVisualChildren<ScrollViewer>(grid)
                .FirstOrDefault(scroller => scroller.Name == "DG_ScrollViewer");
            if (dgScroller != null
                && dgScroller.ComputedVerticalScrollBarVisibility == Visibility.Visible)
            {
                usableWidth -= SystemParameters.VerticalScrollBarWidth;
            }
            record.ColumnFillRatio = usableWidth > 0
                ? Math.Round(columnSum / usableWidth, 2)
                : 0;

            if (report.SizeKey is "2k" or "wide" or "maximized"
                && record.ColumnFillRatio < 0.90)
            {
                report.Warnings.Add(new UiAuditWarning
                {
                    Severity = "HIGH",
                    Code = "COLUMN_FILL_TOO_LOW",
                    RouteId = report.RouteId,
                    Tab = report.TabHeader,
                    SizeKey = report.SizeKey,
                    Message = $"{grid.Name} 列填充率 {record.ColumnFillRatio:0.00}，要求 >=0.90"
                });
            }
            else if (report.SizeKey == "standard"
                && record.ColumnFillRatio < 0.88)
            {
                report.Warnings.Add(new UiAuditWarning
                {
                    Severity = "HIGH",
                    Code = "COLUMN_FILL_TOO_LOW",
                    RouteId = report.RouteId,
                    Tab = report.TabHeader,
                    SizeKey = report.SizeKey,
                    Message = $"{grid.Name} 列填充率 {record.ColumnFillRatio:0.00}，要求 >=0.88"
                });
            }

            var headerWhiteRatio = UiScreenshotService.ProbeHeaderWhiteRatio(grid);
            if (headerWhiteRatio > 0.10)
            {
                report.Warnings.Add(new UiAuditWarning
                {
                    Severity = "HIGH",
                    Code = "HEADER_WHITE_BLOCK",
                    RouteId = report.RouteId,
                    Tab = report.TabHeader,
                    SizeKey = report.SizeKey,
                    Message = $"{grid.Name} 表头白色像素占比 {headerWhiteRatio:0.00}"
                });
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
            if (text.Visibility != Visibility.Visible || text.ActualWidth <= 0 || text.ActualHeight <= 0)
                continue;
            if (text.TextWrapping == TextWrapping.Wrap || text.TextTrimming != TextTrimming.None)
                continue;
            var desiredWidth = Math.Max(0, text.DesiredSize.Width - text.Margin.Left - text.Margin.Right);
            var textWidth = ComputeUnconstrainedTextWidth(text);
            var availableWidth = text.ActualWidth;
            if (desiredWidth <= availableWidth + 2 && textWidth <= availableWidth + 2)
                continue;

            var textSnippet = string.IsNullOrEmpty(text.Text)
                ? string.Empty
                : " 文本=" + (text.Text.Length > 24 ? text.Text.Substring(0, 24) + "…" : text.Text);
            var parentName = VisualTreeHelper.GetParent(text) is FrameworkElement parent
                ? " 父=" + (string.IsNullOrEmpty(parent.Name) ? parent.GetType().Name : parent.Name)
                : string.Empty;
            var isTextFit = textWidth > availableWidth + 2;
            report.Warnings.Add(new UiAuditWarning
            {
                Severity = isTextFit ? "MEDIUM" : "INFO",
                Code = isTextFit ? "TEXT_FIT" : "POSSIBLE_CLIPPING",
                RouteId = report.RouteId,
                Tab = report.TabHeader,
                SizeKey = report.SizeKey,
                Message = $"{text.Name ?? text.GetType().Name} 文本宽度 {textWidth:0} DIP 大于实际宽度 {availableWidth:0} DIP{parentName}{textSnippet}"
            });
        }
    }

    private static double ComputeUnconstrainedTextWidth(TextBlock text)
    {
        if (string.IsNullOrEmpty(text.Text))
            return 0;
        var typeface = new Typeface(text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch);
        var formatted = new FormattedText(
            text.Text,
            CultureInfo.CurrentUICulture,
            text.FlowDirection,
            typeface,
            text.FontSize,
            text.Foreground,
            VisualTreeHelper.GetDpi(text).PixelsPerDip);
        return formatted.WidthIncludingTrailingWhitespace;
    }

    private static void AnalyzeHeaderContentFidelity(DependencyObject root, UiLayoutReport report)
    {
        foreach (var header in FindVisualChildren<DataGridColumnHeader>(root))
        {
            if (header.Visibility != Visibility.Visible || header.ActualWidth <= 0 || header.ActualHeight <= 0)
                continue;
            // WPF's generated filler header has no backing column and must stay exempt.
            if (header.Column == null || header.Content == null)
                continue;

            var hasRenderedContent = HasRenderedHeaderContent(header);
            if (!hasRenderedContent)
            {
                var headerText = header.Content.ToString() ?? string.Empty;
                report.Warnings.Add(new UiAuditWarning
                {
                    Severity = "MEDIUM",
                    Code = "HEADER_CONTENT_FIDELITY",
                    RouteId = report.RouteId,
                    Tab = report.TabHeader,
                    SizeKey = report.SizeKey,
                    Message = $"DataGridColumnHeader 元数据存在但未渲染内容：文本=\"{headerText}\" 列宽={header.ActualWidth:0} DIP"
                });
            }
        }
    }

    private static bool HasRenderedHeaderContent(DependencyObject node)
    {
        if (node is TextBlock textBlock && textBlock.Visibility == Visibility.Visible && textBlock.ActualWidth > 0)
            return true;
        if (node is ContentPresenter presenter && presenter.Visibility == Visibility.Visible)
            return true;
        var childCount = VisualTreeHelper.GetChildrenCount(node);
        for (var i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(node, i);
            if (HasRenderedHeaderContent(child))
                return true;
        }
        return false;
    }

    private static void AnalyzeActiveTabVisibility(DependencyObject root, UiLayoutReport report)
    {
        foreach (var scroller in FindVisualChildren<ScrollViewer>(root))
        {
            if (scroller.ScrollableWidth <= 0.5
                || scroller.HorizontalScrollBarVisibility == ScrollBarVisibility.Disabled
                || scroller.ViewportWidth <= 0)
                continue;

            foreach (var tab in FindVisualChildren<TabItem>(root))
            {
                if (!tab.IsSelected || tab.Visibility != Visibility.Visible || tab.ActualWidth <= 0)
                    continue;
                if (!IsDescendantOf(tab, scroller))
                    continue;

                var origin = tab.TransformToAncestor(scroller).Transform(new Point(0, 0));
                var left = origin.X;
                var right = left + tab.ActualWidth;
                const double tolerance = 2;
                if (left < -tolerance || right > scroller.ViewportWidth + tolerance)
                {
                    report.Warnings.Add(new UiAuditWarning
                    {
                        Severity = "MEDIUM",
                        Code = "ACTIVE_TAB_VISIBILITY",
                        RouteId = report.RouteId,
                        Tab = report.TabHeader,
                        SizeKey = report.SizeKey,
                        Message = $"选中分类不在横向 viewport 内：left={left:0.##} right={right:0.##} viewport={scroller.ViewportWidth:0.##}"
                    });
                }
            }
        }
    }

    private static void AnalyzeControlUsabilityGeometry(DependencyObject root, UiLayoutReport report)
    {
        if (report.RouteId != "media-center" || report.TabHeader != "当前游戏媒体")
            return;
        var search = FindVisualChildren<TextBox>(root)
            .FirstOrDefault(textBox => textBox.Name == "MediaSearchTextBox");
        if (search == null || search.Visibility != Visibility.Visible || search.ActualWidth <= 0)
            return;
        if (search.ActualWidth < 160)
        {
            report.Warnings.Add(new UiAuditWarning
            {
                Severity = "MEDIUM",
                Code = "CONTROL_USABILITY_GEOMETRY",
                RouteId = report.RouteId,
                Tab = report.TabHeader,
                SizeKey = report.SizeKey,
                Message = $"MediaSearchTextBox 宽度 {search.ActualWidth:0} DIP 低于 160 DIP 可用下限"
            });
        }
    }

    private static void AnalyzeEssentialColumnVisibility(DependencyObject root, UiLayoutReport report)
    {
        if (report.RouteId != "save-center" || report.TabHeader != "历史版本")
            return;
        var grid = FindVisualChildren<DataGrid>(root)
            .FirstOrDefault(candidate => candidate.Name == "SaveHistoryGrid");
        if (grid == null || grid.ActualWidth <= 0)
            return;

        var ordered = grid.Columns
            .OrderBy(column => column.DisplayIndex)
            .ToList();
        var rightEdge = 0d;
        foreach (var column in ordered)
        {
            var width = column.ActualWidth > 0 ? column.ActualWidth : 0;
            rightEdge += width;
            if (string.Equals(column.Header as string, "状态", StringComparison.Ordinal)
                && rightEdge > grid.ActualWidth + 1)
            {
                report.Warnings.Add(new UiAuditWarning
                {
                    Severity = "MEDIUM",
                    Code = "ESSENTIAL_COLUMN_VISIBILITY",
                    RouteId = report.RouteId,
                    Tab = report.TabHeader,
                    SizeKey = report.SizeKey,
                    Message = $"SaveHistory 状态列右缘 {rightEdge:0} DIP 超出视口 {grid.ActualWidth:0} DIP，且横向滚动已禁用"
                });
                break;
            }
        }
    }

    private static void AnalyzeShortSemanticValueTrimming(DependencyObject root, UiLayoutReport report)
    {
        if (report.RouteId != "save-center" || report.TabHeader != "历史版本")
            return;
        foreach (var text in FindVisualChildren<TextBlock>(root))
        {
            if (text.Visibility != Visibility.Visible || text.ActualWidth <= 0)
                continue;
            if (!string.Equals(text.Tag as string, "SaveHistorySize", StringComparison.Ordinal))
                continue;
            var textWidth = ComputeUnconstrainedTextWidth(text);
            if (textWidth > text.ActualWidth + 2)
            {
                report.Warnings.Add(new UiAuditWarning
                {
                    Severity = "MEDIUM",
                    Code = "SHORT_SEMANTIC_VALUE_TRIMMING",
                    RouteId = report.RouteId,
                    Tab = report.TabHeader,
                    SizeKey = report.SizeKey,
                    Message = $"SaveHistory 大小短值被裁切：文本宽度 {textWidth:0} DIP 大于可用宽度 {text.ActualWidth:0} DIP，文本=\"{text.Text}\""
                });
            }
        }
    }

    private static void AnalyzeInteractiveInspectorUsability(DependencyObject root, UiLayoutReport report)
    {
        var inspector = FindVisualChildren<ScrollViewer>(root)
            .FirstOrDefault(scroller => scroller.Name == "MaintenanceDeviceInspectorScrollViewer");
        if (inspector == null || inspector.Visibility != Visibility.Visible || inspector.ViewportHeight <= 0)
            return;
        if (inspector.ExtentHeight <= 300)
            return;
        var hasInteractiveContent =
            FindVisualChildren<ComboBox>(inspector).Any()
            || FindVisualChildren<TextBox>(inspector).Any()
            || FindVisualChildren<Button>(inspector).Any();
        if (!hasInteractiveContent)
            return;
        if (inspector.ViewportHeight < 150 || inspector.ViewportHeight / inspector.ExtentHeight < 0.3)
        {
            report.Warnings.Add(new UiAuditWarning
            {
                Severity = "MEDIUM",
                Code = "INTERACTIVE_INSPECTOR_USABILITY",
                RouteId = report.RouteId,
                Tab = report.TabHeader,
                SizeKey = report.SizeKey,
                Message = $"MaintenanceDeviceInspector 交互内容 viewport {inspector.ViewportHeight:0} DIP / extent {inspector.ExtentHeight:0} DIP 过小"
            });
        }
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
