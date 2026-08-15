using System;
using System.Collections.Generic;

namespace GameSaveCenter.RenderHarness.UiAudit;

public sealed class UiAuditMetadata
{
    public string GeneratedUtc { get; set; } = string.Empty;
    public string CommitSha { get; set; } = string.Empty;
    public string PluginVersion { get; set; } = string.Empty;
    public string PlayniteVersion { get; set; } = string.Empty;
    public string WindowsVersion { get; set; } = string.Empty;
    public double DpiScale { get; set; } = 1.0;
    public List<UiSizeRecord> Sizes { get; } = new List<UiSizeRecord>();
    public string OutputRoot { get; set; } = string.Empty;
    public string ZipPath { get; set; } = string.Empty;
}

public sealed class UiSizeRecord
{
    public string Key { get; set; } = string.Empty;
    public double RequestedWidth { get; set; }
    public double RequestedHeight { get; set; }
    public double ActualWidth { get; set; }
    public double ActualHeight { get; set; }
    public double ContentWidth { get; set; }
    public double ContentHeight { get; set; }
    public string Note { get; set; } = string.Empty;
}

public sealed class UiStaticManifest
{
    public string GeneratedUtc { get; set; } = string.Empty;
    public string RepositoryRoot { get; set; } = string.Empty;
    public List<UiRouteNode> Routes { get; } = new List<UiRouteNode>();
    public List<UiElementRecord> Elements { get; } = new List<UiElementRecord>();
    public UiManifestSummary Summary { get; } = new UiManifestSummary();
}

public sealed class UiManifestSummary
{
    public int ViewCount { get; set; }
    public int TabCount { get; set; }
    public int ButtonCount { get; set; }
    public int DataGridCount { get; set; }
    public int ScrollViewerCount { get; set; }
    public int ConditionalUiCount { get; set; }
    public int ExpanderCount { get; set; }
    public int TextBlockCount { get; set; }
    public int ComboBoxCount { get; set; }
    public int CheckBoxCount { get; set; }
    public int TextBoxCount { get; set; }
}

public sealed class UiRouteNode
{
    public string RouteId { get; set; } = string.Empty;
    public string Workspace { get; set; } = string.Empty;
    public string ViewFile { get; set; } = string.Empty;
    public string ViewType { get; set; } = string.Empty;
    public string SourceFile { get; set; } = string.Empty;
    public int SourceLine { get; set; }
    public bool StaticOnly { get; set; }
    public List<UiTabRecord> Tabs { get; } = new List<UiTabRecord>();
    public List<UiElementRecord> Elements { get; } = new List<UiElementRecord>();
}

public sealed class UiTabRecord
{
    public string Header { get; set; } = string.Empty;
    public int Index { get; set; }
    public string RouteId { get; set; } = string.Empty;
    public string SourceFile { get; set; } = string.Empty;
    public int SourceLine { get; set; }
    public List<UiElementRecord> Elements { get; } = new List<UiElementRecord>();
    public List<UiDataGridRecord> DataGrids { get; } = new List<UiDataGridRecord>();
    public List<UiScrollViewerRecord> ScrollViewers { get; } = new List<UiScrollViewerRecord>();
    public List<UiExpanderRecord> Expanders { get; } = new List<UiExpanderRecord>();
    public List<UiElementRecord> ConditionalElements { get; } = new List<UiElementRecord>();
}

public sealed class UiElementRecord
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Header { get; set; } = string.Empty;
    public string Binding { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string CommandParameter { get; set; } = string.Empty;
    public string VisibilityBinding { get; set; } = string.Empty;
    public string IsEnabledBinding { get; set; } = string.Empty;
    public string Style { get; set; } = string.Empty;
    public string GridRow { get; set; } = string.Empty;
    public string GridColumn { get; set; } = string.Empty;
    public string GridRowSpan { get; set; } = string.Empty;
    public string GridColumnSpan { get; set; } = string.Empty;
    public string Margin { get; set; } = string.Empty;
    public string Padding { get; set; } = string.Empty;
    public string Width { get; set; } = string.Empty;
    public string Height { get; set; } = string.Empty;
    public string MinWidth { get; set; } = string.Empty;
    public string MaxWidth { get; set; } = string.Empty;
    public string MinHeight { get; set; } = string.Empty;
    public string MaxHeight { get; set; } = string.Empty;
    public string HorizontalAlignment { get; set; } = string.Empty;
    public string VerticalAlignment { get; set; } = string.Empty;
    public string ParentType { get; set; } = string.Empty;
    public string ParentName { get; set; } = string.Empty;
    public bool Conditional { get; set; }
    public string Condition { get; set; } = string.Empty;
    public string SourceFile { get; set; } = string.Empty;
    public int SourceLine { get; set; }
}

public sealed class UiDataGridRecord
{
    public string Name { get; set; } = string.Empty;
    public string ItemsSource { get; set; } = string.Empty;
    public string SelectedItem { get; set; } = string.Empty;
    public int ColumnCount { get; set; }
    public List<UiDataGridColumnRecord> Columns { get; } = new List<UiDataGridColumnRecord>();
    public string VirtualizingPanel { get; set; } = string.Empty;
    public string ScrollViewer { get; set; } = string.Empty;
    public string SourceFile { get; set; } = string.Empty;
    public int SourceLine { get; set; }
}

public sealed class UiDataGridColumnRecord
{
    public string Header { get; set; } = string.Empty;
    public string Binding { get; set; } = string.Empty;
    public string Width { get; set; } = string.Empty;
    public string MinWidth { get; set; } = string.Empty;
    public string MaxWidth { get; set; } = string.Empty;
    public string CanUserResize { get; set; } = string.Empty;
    public string HeaderStyle { get; set; } = string.Empty;
    public int SourceLine { get; set; }
}

public sealed class UiScrollViewerRecord
{
    public string Name { get; set; } = string.Empty;
    public string HorizontalScrollBarVisibility { get; set; } = string.Empty;
    public string VerticalScrollBarVisibility { get; set; } = string.Empty;
    public string ParentScrollViewers { get; set; } = string.Empty;
    public bool ContainsDataGridOrListBox { get; set; }
    public string SourceFile { get; set; } = string.Empty;
    public int SourceLine { get; set; }
}

public sealed class UiExpanderRecord
{
    public string Name { get; set; } = string.Empty;
    public string Header { get; set; } = string.Empty;
    public string IsExpanded { get; set; } = string.Empty;
    public string SourceFile { get; set; } = string.Empty;
    public int SourceLine { get; set; }
}

public sealed class UiRuntimeRoute
{
    public string RouteId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public Type ViewType { get; set; } = null!;
    public bool IsSettings { get; set; }
    public bool IsKnown { get; set; }
    public string Failure { get; set; } = string.Empty;
}

public sealed class UiRuntimeTabRoute
{
    public string Slug { get; set; } = string.Empty;
    public string Header { get; set; } = string.Empty;
    public int OuterTabIndex { get; set; } = -1;
    public int InnerTabIndex { get; set; } = -1;
    public string ExpectedPrimaryElement { get; set; } = string.Empty;
}

public sealed class UiLayoutReport
{
    public string RouteId { get; set; } = string.Empty;
    public string RouteSlug { get; set; } = string.Empty;
    public string TabHeader { get; set; } = string.Empty;
    public string ExpectedPrimaryElement { get; set; } = string.Empty;
    public string ActualPrimaryElement { get; set; } = string.Empty;
    public string SizeKey { get; set; } = string.Empty;
    public double Width { get; set; }
    public double Height { get; set; }
    public double WorkspaceHeight { get; set; }
    public double MainListHeight { get; set; }
    public double VerticalFillRatio { get; set; }
    public double TopExternalGap { get; set; }
    public double BottomExternalGap { get; set; }
    public List<UiRuntimeScrollViewer> ScrollViewers { get; } = new List<UiRuntimeScrollViewer>();
    public List<UiRuntimeDataGrid> DataGrids { get; } = new List<UiRuntimeDataGrid>();
    public List<UiRuntimeListBox> ListBoxes { get; } = new List<UiRuntimeListBox>();
    public List<UiRuntimeToolbar> Toolbars { get; } = new List<UiRuntimeToolbar>();
    public List<UiAuditWarning> Warnings { get; } = new List<UiAuditWarning>();
}

public sealed class UiRuntimeScrollViewer
{
    public string Name { get; set; } = string.Empty;
    public double ActualWidth { get; set; }
    public double ActualHeight { get; set; }
    public double ViewportWidth { get; set; }
    public double ViewportHeight { get; set; }
    public double ExtentWidth { get; set; }
    public double ExtentHeight { get; set; }
    public double ScrollableWidth { get; set; }
    public double ScrollableHeight { get; set; }
    public double VerticalOffset { get; set; }
    public double HorizontalOffset { get; set; }
    public string VerticalScrollBarVisibility { get; set; } = string.Empty;
    public string HorizontalScrollBarVisibility { get; set; } = string.Empty;
    public string ParentChain { get; set; } = string.Empty;
    public bool ContainsDataGridOrListBox { get; set; }
    public bool IsInternalToDataGridOrListBox { get; set; }
    public bool Scrollable { get; set; }
}

public sealed class UiRuntimeDataGrid
{
    public string Name { get; set; } = string.Empty;
    public double ActualWidth { get; set; }
    public double ActualHeight { get; set; }
    public double ColumnFillRatio { get; set; }
    public bool StarFillEnabled { get; set; }
    public bool StarFillApplied { get; set; }
    public int ColumnCount { get; set; }
    public int ItemsCount { get; set; }
    public double ColumnHeaderHeight { get; set; }
    public double EstimatedRowHeight { get; set; }
    public double EstimatedVisibleRows { get; set; }
    public string Virtualization { get; set; } = string.Empty;
    public List<UiRuntimeDataGridColumn> Columns { get; } = new List<UiRuntimeDataGridColumn>();
    public List<string> Warnings { get; } = new List<string>();
}

public sealed class UiRuntimeDataGridColumn
{
    public string Header { get; set; } = string.Empty;
    public double ActualWidth { get; set; }
    public string Width { get; set; } = string.Empty;
    public double MinWidth { get; set; }
    public double MaxWidth { get; set; }
}

public sealed class UiRuntimeListBox
{
    public string Name { get; set; } = string.Empty;
    public double ActualWidth { get; set; }
    public double ActualHeight { get; set; }
    public int ItemsCount { get; set; }
    public string Virtualization { get; set; } = string.Empty;
}

public sealed class UiRuntimeToolbar
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double ActualHeight { get; set; }
    public int ChildrenCount { get; set; }
    public bool Expanded { get; set; }
}

public sealed class UiAuditWarning
{
    public string Severity { get; set; } = "INFO";
    public string Code { get; set; } = string.Empty;
    public string RouteId { get; set; } = string.Empty;
    public string Tab { get; set; } = string.Empty;
    public string SizeKey { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public sealed class UiRuntimeSnapshot
{
    public string RouteId { get; set; } = string.Empty;
    public string TabHeader { get; set; } = string.Empty;
    public string SizeKey { get; set; } = string.Empty;
    public string ViewportPng { get; set; } = string.Empty;
    public List<string> FullPagePngs { get; } = new List<string>();
    public List<string> FullScrollPngs { get; } = new List<string>();
    public string VisualTreeJson { get; set; } = string.Empty;
    public string LayoutJson { get; set; } = string.Empty;
}

public sealed class UiAuditRunResult
{
    public UiAuditMetadata Metadata { get; } = new UiAuditMetadata();
    public UiStaticManifest Manifest { get; set; } = new UiStaticManifest();
    public List<UiRuntimeSnapshot> Snapshots { get; } = new List<UiRuntimeSnapshot>();
    public List<UiLayoutReport> LayoutReports { get; } = new List<UiLayoutReport>();
    public List<UiAuditWarning> Warnings { get; } = new List<UiAuditWarning>();
    public List<string> FailedRoutes { get; } = new List<string>();
    public string LogPath { get; set; } = string.Empty;
    public string ZipPath { get; set; } = string.Empty;
}

public sealed class UiVisualNode
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AutomationName { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
    public bool IsEnabled { get; set; }
    public double ActualWidth { get; set; }
    public double ActualHeight { get; set; }
    public double DesiredWidth { get; set; }
    public double DesiredHeight { get; set; }
    public double RenderWidth { get; set; }
    public double RenderHeight { get; set; }
    public string Margin { get; set; } = string.Empty;
    public string HorizontalAlignment { get; set; } = string.Empty;
    public string VerticalAlignment { get; set; } = string.Empty;
    public int GridRow { get; set; }
    public int GridColumn { get; set; }
    public int GridRowSpan { get; set; }
    public int GridColumnSpan { get; set; }
    public double MinWidth { get; set; }
    public double MaxWidth { get; set; }
    public double MinHeight { get; set; }
    public double MaxHeight { get; set; }
    public double Opacity { get; set; }
    public string Text { get; set; } = string.Empty;
    public string ParentType { get; set; } = string.Empty;
    public string ParentName { get; set; } = string.Empty;
    public int Depth { get; set; }
}
