using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace GameSaveCenter.Playnite.Views
{
    public partial class MaintenanceView : UserControl
    {
        private double responsiveWidth;
        private double responsiveHeight;
        private bool isApplyingLayout;
        private bool deviceInspectorOpen;

        public MaintenanceView()
        {
            InitializeComponent();

            // Loaded is a direct WPF event. Re-assert the explicit local header styles once
            // per grid so generated headers never fall back to a Playnite host default.
            // The XAML HeaderStyle declarations own the theme; no visual-tree scanning here.
            FindingsGrid.Loaded += DataGridLoaded;
            MaintenanceDeviceGrid.Loaded += DataGridLoaded;
            MaintenanceAuditFindingsGrid.Loaded += DataGridLoaded;
            MaintenanceAuditLogGrid.Loaded += DataGridLoaded;
            MaintenanceProcessGrid.Loaded += DataGridLoaded;
            MaintenanceDiagnosticsInspector.IsVisibleChanged += InspectorIsVisibleChanged;
            MaintenanceAuditInspector.IsVisibleChanged += InspectorIsVisibleChanged;
            MaintenanceProcessInspector.IsVisibleChanged += InspectorIsVisibleChanged;
            MaintenanceDeviceInspectorScrollViewer.IsVisibleChanged += InspectorIsVisibleChanged;
            MaintenanceDeviceGrid.SelectionChanged += OnMaintenanceDeviceSelectionChanged;
        }

        private void DataGridLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid grid)
                ApplyGridHeaderTheme(grid);
        }

        private void ApplyGridHeaderTheme(DataGrid grid)
        {
            var normalStyle = TryFindResource("GscDataGridColumnHeaderStyle") as Style;
            var firstStyle = TryFindResource("MaintenanceFirstColumnHeader") as Style;
            var lastStyle = TryFindResource("MaintenanceLastColumnHeader") as Style;

            if (normalStyle != null)
                grid.ColumnHeaderStyle = normalStyle;

            for (var index = 0; index < grid.Columns.Count; index++)
            {
                var column = grid.Columns[index];
                column.HeaderStyle = index == 0 && firstStyle != null
                    ? firstStyle
                    : index == grid.Columns.Count - 1 && lastStyle != null
                        ? lastStyle
                        : normalStyle;
            }
        }

        private void InspectorIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsLoaded || responsiveWidth <= 0 || responsiveHeight <= 0)
                return;

            ApplyResponsiveLayout(responsiveWidth, responsiveHeight);
        }

        private void OnMaintenanceDeviceSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            deviceInspectorOpen = false;
            if (IsLoaded && responsiveWidth > 0 && responsiveHeight > 0)
                ApplyResponsiveLayout(responsiveWidth, responsiveHeight);
        }

        private void OnMaintenanceDeviceCompactDetailsClick(object sender, RoutedEventArgs e)
        {
            if (MaintenanceDeviceGrid.SelectedItem == null) return;
            deviceInspectorOpen = !deviceInspectorOpen;
            ApplyResponsiveLayout(
                responsiveWidth > 0 ? responsiveWidth : ActualWidth,
                responsiveHeight > 0 ? responsiveHeight : ActualHeight);
        }

        public UniformGrid DiagnosticHealthPanelElement => DiagnosticHealthPanel;
        public DataGrid FindingsGridElement => FindingsGrid;

        public void ApplyResponsiveLayout(double width, double height)
        {
            if (isApplyingLayout) return;
            isApplyingLayout = true;
            try
            {
            responsiveWidth = width;
            responsiveHeight = height;
            // At the demo minimum the measured workspace is about 700 DIP. Two health
            // cards per row preserve the six-card summary without pushing diagnostics
            // below an unnecessarily tall single-column block.
            // The Demo uses four compact health cards per row at the normal 1040-DIP
            // workspace width. Keep the production overview on that same rhythm;
            // only compact windows collapse to two or one column.
            DiagnosticHealthPanel.Columns = width >= 980 ? 4 : width >= 680 ? 2 : 1;
            var inspectorWidth = MaintenanceDiagnosticsLayout.TryFindResource("GscInspectorWidth") is GridLength gl ? gl : new GridLength(360);
            // Health cards remain useful context even in compact windows. Grid star rows keep
            // diagnostics tables finite while their own controls handle overflow.
            DiagnosticHealthPanel.Visibility = Visibility.Visible;
            // Environment checks are a fixed set of peer states. A UniformGrid keeps
            // every card aligned to a predictable column rhythm instead of letting a
            // WrapPanel create a ragged final row at different maximized/DPI widths.
            var environmentColumns = width >= 900 ? 3 : width >= 620 ? 2 : 1;
            var environmentPanel = FindVisualChild<UniformGrid>(EnvironmentCheckItems);
            if (environmentPanel != null)
                environmentPanel.Columns = environmentColumns;
            const double tableMinHeight = 236d;
            FindingsGrid.MinHeight = tableMinHeight;
            FindingsGrid.Height = double.NaN;
            FindingsGrid.MaxHeight = double.PositiveInfinity;
            MaintenanceDeviceGrid.MinHeight = Math.Max(tableMinHeight, 252d);
            MaintenanceDeviceGrid.Height = double.NaN;
            MaintenanceDeviceGrid.MaxHeight = double.PositiveInfinity;
            MaintenanceAuditFindingsGrid.MinHeight = tableMinHeight;
            MaintenanceAuditFindingsGrid.Height = double.NaN;
            MaintenanceAuditFindingsGrid.MaxHeight = double.PositiveInfinity;
            MaintenanceProcessGrid.MinHeight = Math.Max(tableMinHeight, 252d);
            MaintenanceProcessGrid.Height = double.NaN;
            MaintenanceProcessGrid.MaxHeight = double.PositiveInfinity;
            if (width < 800)
            {
                // Narrow audit windows keep at least four readable rows even when the
                // stacked device/process inspectors consume part of the finite workspace;
                // the page-level scroll surface remains the overflow owner.
                MaintenanceDeviceGrid.MinHeight = 280;
                MaintenanceProcessGrid.MinHeight = 280;
            }
            var compact = width < 980;
            // The retention page follows the Demo's wide reading canvas. Keep a
            // cap so the cards do not become uncomfortably wide on ultrawide
            // hosts, while still giving the three-card row enough room to read.
            // Give the StackPanel an explicit viewport width so the cards fill
            // the form instead of collapsing to their content width, mirroring
            // the SaveCenter policy page. The 4 is the right padding of
            // GscPageScrollViewer.
            MaintenanceRetentionStack.Width = Math.Max(0, Math.Min(width - 4, 1310));
            var stackRetentionDemoTop = width < 720;
            MaintenanceRetentionDemoTopLayout.Width = stackRetentionDemoTop
                ? MaintenanceRetentionStack.Width
                : Math.Min(1310, MaintenanceRetentionStack.Width);
            MaintenanceRetentionDemoTopLayout.ColumnDefinitions[1].Width = stackRetentionDemoTop
                ? new GridLength(0)
                : new GridLength(14);
            MaintenanceRetentionDemoTopLayout.ColumnDefinitions[2].Width = stackRetentionDemoTop
                ? new GridLength(0)
                : new GridLength(1, GridUnitType.Star);
            MaintenanceRetentionDemoTopLayout.RowDefinitions[1].Height = stackRetentionDemoTop
                ? new GridLength(1, GridUnitType.Auto)
                : new GridLength(0);
            Grid.SetColumn(MaintenanceRetentionDemoTopLayout.Children[1], stackRetentionDemoTop ? 0 : 2);
            Grid.SetRow(MaintenanceRetentionDemoTopLayout.Children[1], stackRetentionDemoTop ? 1 : 0);
            var stackRetentionDemoOperations = width < 980;
            MaintenanceRetentionDemoOperationsLayout.Width = stackRetentionDemoOperations
                ? MaintenanceRetentionStack.Width
                : Math.Min(1310, MaintenanceRetentionStack.Width);
            MaintenanceRetentionDemoOperationsLayout.ColumnDefinitions[1].Width = stackRetentionDemoOperations
                ? new GridLength(0)
                : new GridLength(14);
            MaintenanceRetentionDemoOperationsLayout.ColumnDefinitions[3].Width = stackRetentionDemoOperations
                ? new GridLength(0)
                : new GridLength(14);
            MaintenanceRetentionDemoOperationsLayout.ColumnDefinitions[2].Width = stackRetentionDemoOperations
                ? new GridLength(0)
                : new GridLength(1, GridUnitType.Star);
            MaintenanceRetentionDemoOperationsLayout.ColumnDefinitions[4].Width = stackRetentionDemoOperations
                ? new GridLength(0)
                : new GridLength(1, GridUnitType.Star);
            for (var row = 1; row < MaintenanceRetentionDemoOperationsLayout.RowDefinitions.Count; row++)
                MaintenanceRetentionDemoOperationsLayout.RowDefinitions[row].Height = stackRetentionDemoOperations
                    ? new GridLength(1, GridUnitType.Auto)
                    : new GridLength(0);
            if (MaintenanceRetentionDemoOperationsLayout.Children.Count >= 3)
            {
                Grid.SetColumn(MaintenanceRetentionDemoOperationsLayout.Children[1], stackRetentionDemoOperations ? 0 : 2);
                Grid.SetColumn(MaintenanceRetentionDemoOperationsLayout.Children[2], stackRetentionDemoOperations ? 0 : 4);
                Grid.SetRow(MaintenanceRetentionDemoOperationsLayout.Children[1], stackRetentionDemoOperations ? 1 : 0);
                Grid.SetRow(MaintenanceRetentionDemoOperationsLayout.Children[2], stackRetentionDemoOperations ? 2 : 0);
            }
            MaintenanceRetentionDemoMetrics.Columns = width >= 720 ? 3 : width >= 480 ? 2 : 1;
            MaintenanceStorageDemoMetrics.Columns = width >= 720 ? 3 : 1;
            MaintenanceRetentionDemoSimulationMetrics.Columns = width >= 720 ? 2 : 1;
            MaintenanceRetentionMetrics.Columns = width >= 720 ? 3 : width >= 480 ? 2 : 1;
            MaintenanceStorageMetrics.Columns = width >= 900 ? 4 : width >= 620 ? 2 : 1;
            MaintenanceStorageTrendPanel.Columns = width >= 720 ? 3 : 1;
            MaintenanceRetentionSimulationMetrics.Columns = width >= 900 ? 4 : width >= 620 ? 2 : 1;
            MaintenanceRetentionSimulationProtectionMetrics.Columns = width >= 720 ? 3 : 1;
            MaintenanceLocalMirrorMetrics.Columns = width >= 720 ? 3 : 1;
            // The two detail cards read well as peers on a wide form, but squeezing
            // long backup IDs into two narrow columns makes the preview look like a
            // clipped table. Stack them as natural-height sections in the narrow form;
            // the page ScrollViewer remains the single overflow owner.
            var stackRetentionDetails = width < 720;
            MaintenanceRetentionDetailsLayout.ColumnDefinitions[1].Width = stackRetentionDetails
                ? new GridLength(0)
                : new GridLength(14);
            MaintenanceRetentionDetailsLayout.ColumnDefinitions[2].Width = stackRetentionDetails
                ? new GridLength(0)
                : new GridLength(1, GridUnitType.Star);
            MaintenanceRetentionDetailsLayout.RowDefinitions[1].Height = stackRetentionDetails
                ? new GridLength(1, GridUnitType.Auto)
                : new GridLength(0);
            Grid.SetColumnSpan(MaintenanceRetentionKeepCard, stackRetentionDetails ? 3 : 1);
            Grid.SetRow(MaintenanceRetentionKeepCard, 0);
            Grid.SetColumn(MaintenanceRetentionDeleteCard, stackRetentionDetails ? 0 : 2);
            Grid.SetColumnSpan(MaintenanceRetentionDeleteCard, stackRetentionDetails ? 3 : 1);
            Grid.SetRow(MaintenanceRetentionDeleteCard, stackRetentionDetails ? 1 : 0);
            MaintenanceRetentionDeleteCard.Margin = stackRetentionDetails
                ? new Thickness(0, 14, 0, 0)
                : new Thickness(0);
            // The findings table has five readable columns plus an inspector. Keep the
            // inspector beside it only when the main table can still show those columns;
            // otherwise stack it before WPF starts compressing the text into a single strip.
            // Keep the findings inspector beside the table only when the table has enough
            // room for readable game/title/detail/action columns.  At common 1280-DIP and
            // high-DPI sizes the inspector must stack instead of forcing ellipses into every
            // column and exposing the host's white fallback header surface.
            // The shell's content width is smaller than the outer Playnite window
            // because the sidebar is already accounted for. Keep the Demo's two-column
            // findings/inspector composition at normal desktop widths and stack only
            // below the documented compact breakpoint.
            var stackDiagnostics = width < 980;
            var showDiagnosticsInspector = MaintenanceDiagnosticsInspector.Visibility == Visibility.Visible;
            var diagnosticsSideBySide = showDiagnosticsInspector && !stackDiagnostics;
            ApplyFindingsColumnLayout(width);
            MaintenanceDiagnosticsLayout.ColumnDefinitions[1].Width = diagnosticsSideBySide ? new GridLength(14) : new GridLength(0);
            MaintenanceDiagnosticsLayout.ColumnDefinitions[2].Width = diagnosticsSideBySide ? inspectorWidth : new GridLength(0);
            // The full diagnostic summary owns row 1 as an always-visible full-width strip;
            // the detail inspector stacks into row 2 only in compact windows.
            MaintenanceDiagnosticsLayout.RowDefinitions[2].Height = showDiagnosticsInspector && stackDiagnostics ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
            Grid.SetColumn(MaintenanceDiagnosticsInspector, diagnosticsSideBySide ? 2 : 0);
            Grid.SetColumnSpan(MaintenanceDiagnosticsInspector, stackDiagnostics ? 3 : 1);
            Grid.SetRow(MaintenanceDiagnosticsInspector, stackDiagnostics ? 2 : 0);
            MaintenanceDiagnosticsInspector.Margin = showDiagnosticsInspector && stackDiagnostics ? new Thickness(0, 10, 0, 0) : new Thickness(0);
            // The detail inspector owns only the selected finding; the full diagnostic
            // summary strip stays below the table. In stacked mode both share a finite
            // vertical budget so the findings table keeps the remaining rows.
            MaintenanceDiagnosticsInspector.MaxHeight = showDiagnosticsInspector && stackDiagnostics ? Math.Max(150, height * 0.34) : double.PositiveInfinity;
            var stackProcess = width < 980;
            var showProcessInspector = MaintenanceProcessInspector.Visibility == Visibility.Visible;
            var processSideBySide = showProcessInspector && !stackProcess;
            MaintenanceProcessLayout.ColumnDefinitions[1].Width = processSideBySide ? new GridLength(14) : new GridLength(0);
            MaintenanceProcessLayout.ColumnDefinitions[2].Width = processSideBySide ? inspectorWidth : new GridLength(0);
            MaintenanceProcessLayout.RowDefinitions[2].Height = showProcessInspector && stackProcess ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
            Grid.SetColumn(MaintenanceProcessInspector, processSideBySide ? 2 : 0);
            Grid.SetColumnSpan(MaintenanceProcessInspector, stackProcess ? 3 : 1);
            Grid.SetRow(MaintenanceProcessInspector, stackProcess ? 2 : 1);
            MaintenanceProcessInspector.Margin = showProcessInspector && stackProcess ? new Thickness(0, 10, 0, 0) : new Thickness(0);

            // Match the Demo process-mapping editor on wide workspaces: the EXE field
            // receives the flexible space, the game target stays readable at 240 DIP,
            // and the action button keeps the shared 38-DIP control height. At a narrow
            // width, move the target and action to a second row instead of compressing
            // three controls into an unreadable strip.
            var stackProcessEditor = width < 720;
            ProcessMappingEditorPrimaryRow.Height = new GridLength(1, GridUnitType.Auto);
            ProcessMappingEditorCompactRow.Height = stackProcessEditor
                ? new GridLength(1, GridUnitType.Auto)
                : new GridLength(0);
            ProcessMappingTargetColumn.Width = stackProcessEditor
                ? new GridLength(1, GridUnitType.Star)
                : new GridLength(240);
            Grid.SetColumnSpan(ProcessMappingExecutableTextBox, stackProcessEditor ? 5 : 1);
            Grid.SetRow(ProcessMappingExecutableTextBox, 0);
            Grid.SetColumn(ProcessMappingTargetGameComboBox, stackProcessEditor ? 0 : 2);
            Grid.SetColumnSpan(ProcessMappingTargetGameComboBox, stackProcessEditor ? 3 : 1);
            Grid.SetRow(ProcessMappingTargetGameComboBox, stackProcessEditor ? 1 : 0);
            Grid.SetColumn(ProcessMappingSaveButton, 4);
            Grid.SetRow(ProcessMappingSaveButton, stackProcessEditor ? 1 : 0);
            // The Demo keeps the device comparison table and conflict inspector in
            // separate columns at normal desktop widths. The Playnite shell leaves
            // less content width than its outer window, so 980 DIP is the compact
            // breakpoint used by the other page-level inspectors.
            var stackDevice = width < 980;
            if (stackDevice)
            {
                var hasDeviceSelection = MaintenanceDeviceGrid.SelectedItem != null;
                if (hasDeviceSelection)
                {
                    MaintenanceDeviceInspectorScrollViewer.Visibility = deviceInspectorOpen
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    MaintenanceDeviceCompactDetailsButton.Content = deviceInspectorOpen
                        ? "收起设备详情 ›"
                        : "查看设备详情 ›";
                    MaintenanceDeviceCompactDetailsButton.Visibility = Visibility.Visible;
                }
                else
                {
                    MaintenanceDeviceCompactDetailsButton.Visibility = Visibility.Collapsed;
                    deviceInspectorOpen = false;
                }
            }
            else
            {
                MaintenanceDeviceCompactDetailsButton.Visibility = Visibility.Collapsed;
                // Keep the inspector column in the desktop composition even when
                // the live Worker has not produced comparison rows yet. Hiding it
                // here caused the table to expand to full width and made the empty
                // production state structurally different from the Demo.
                MaintenanceDeviceInspectorScrollViewer.Visibility = Visibility.Visible;
            }
            var showDeviceInspector = MaintenanceDeviceInspectorScrollViewer.Visibility == Visibility.Visible;
            MaintenanceDeviceLayout.ColumnDefinitions[1].Width = !stackDevice ? new GridLength(14) : new GridLength(0);
            MaintenanceDeviceLayout.ColumnDefinitions[2].Width = !stackDevice ? inspectorWidth : new GridLength(0);
            MaintenanceDeviceLayout.RowDefinitions[3].Height = stackDevice ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
            Grid.SetColumn(MaintenanceDeviceInspectorScrollViewer, stackDevice ? 0 : 2);
            Grid.SetColumnSpan(MaintenanceDeviceInspectorScrollViewer, stackDevice ? 3 : 1);
            Grid.SetRow(MaintenanceDeviceInspectorScrollViewer, stackDevice ? 3 : 2);
            MaintenanceDeviceInspectorScrollViewer.Margin = showDeviceInspector && stackDevice
                ? new Thickness(0, 10, 0, 0)
                : new Thickness(0);
            var deviceAvailableHeight = MaintenanceDeviceLayout.ActualHeight > 0
                ? MaintenanceDeviceLayout.ActualHeight
                    - MaintenanceDeviceLayout.RowDefinitions[0].ActualHeight
                    - MaintenanceDeviceLayout.RowDefinitions[1].ActualHeight
                : Math.Max(320, height - 250);
            // A stacked device inspector is an interactive drawer, not a permanent
            // 90-DIP slit. When open it gets a real usable viewport and the table keeps
            // at least the header plus two full rows; closing it restores the table.
            var deviceTableMinHeight = showDeviceInspector && stackDevice
                ? 150
                : width < 800 ? 280 : Math.Max(tableMinHeight, 252d);
            MaintenanceDeviceGrid.MinHeight = deviceTableMinHeight;
            var deviceInspectorHeight = Math.Max(180, Math.Min(420, deviceAvailableHeight - deviceTableMinHeight - 10));
            MaintenanceDeviceInspectorScrollViewer.MaxHeight = showDeviceInspector && stackDevice
                ? deviceInspectorHeight
                : double.PositiveInfinity;

            var stackAudit = width < 980;
            var showAuditInspector = MaintenanceAuditInspector.Visibility == Visibility.Visible;
            var auditSideBySide = showAuditInspector && !stackAudit;
            MaintenanceAuditLayout.ColumnDefinitions[1].Width = auditSideBySide ? new GridLength(14) : new GridLength(0);
            MaintenanceAuditLayout.ColumnDefinitions[2].Width = auditSideBySide ? inspectorWidth : new GridLength(0);
            MaintenanceAuditLayout.RowDefinitions[1].Height = showAuditInspector && stackAudit ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
            Grid.SetColumn(MaintenanceAuditInspector, auditSideBySide ? 2 : 0);
            Grid.SetColumnSpan(MaintenanceAuditInspector, stackAudit ? 3 : 1);
            Grid.SetRow(MaintenanceAuditInspector, stackAudit ? 1 : 0);
            MaintenanceAuditInspector.Margin = showAuditInspector && stackAudit ? new Thickness(0, 10, 0, 0) : new Thickness(0);
            // The detail inspector owns only the selected finding. The audit log now
            // lives on its own secondary tab and fills that tab's star row.
            var auditAvailableHeight = MaintenanceAuditLayout.ActualHeight > 0
                ? MaintenanceAuditLayout.ActualHeight
                : Math.Max(320, height - 200);
            var auditInspectorHeight = Math.Max(96, Math.Min(420, auditAvailableHeight - tableMinHeight - 10));
            MaintenanceAuditInspector.MaxHeight = showAuditInspector && stackAudit ? auditInspectorHeight : double.PositiveInfinity;
            MaintenanceAuditLogGrid.MinHeight = tableMinHeight;
            MaintenanceAuditLogGrid.Height = double.NaN;
            MaintenanceAuditLogGrid.MaxHeight = double.PositiveInfinity;
            }
            finally
            {
                isApplyingLayout = false;
            }
        }

        private void ApplyFindingsColumnLayout(double width)
        {
            // At 1040 DIP the fixed inspector leaves a compact but usable findings
            // table. Reduce only the non-essential fixed columns in that range; the
            // detail/action columns remain star-sized and continue to ellipsize with
            // a tooltip. Wide windows retain the Demo's more generous proportions.
            ApplyFindingsColumnLayout(FindingsGrid, width, audit: false);
            ApplyFindingsColumnLayout(MaintenanceAuditFindingsGrid, width, audit: true);
        }

        private static void ApplyFindingsColumnLayout(DataGrid grid, double width, bool audit)
        {
            if (grid.Columns.Count == 0)
                return;

            var compact = width < 1180;
            if (audit)
            {
                if (grid.Columns.Count < 4)
                    return;

                grid.Columns[0].Width = compact ? new DataGridLength(84) : new DataGridLength(92);
                grid.Columns[1].Width = compact ? new DataGridLength(110) : new DataGridLength(180);
                grid.Columns[2].Width = compact ? new DataGridLength(128) : new DataGridLength(190);
                grid.Columns[3].MinWidth = compact ? 160 : 320;
                return;
            }

            if (grid.Columns.Count < 5)
                return;

            grid.Columns[0].Width = compact ? new DataGridLength(84) : new DataGridLength(92);
            grid.Columns[1].Width = compact ? new DataGridLength(100) : new DataGridLength(120);
            grid.Columns[2].Width = compact ? new DataGridLength(118) : new DataGridLength(160);
            grid.Columns[3].MinWidth = compact ? 110 : 180;
            grid.Columns[4].Width = new DataGridLength(compact ? 1 : 0.75, DataGridLengthUnitType.Star);
            grid.Columns[4].MinWidth = compact ? 110 : 140;
        }

        private static T? FindVisualChild<T>(DependencyObject? parent) where T : DependencyObject
        {
            if (parent == null) return null;
            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
            {
                var child = VisualTreeHelper.GetChild(parent, index);
                if (child is T match) return match;
                var nested = FindVisualChild<T>(child);
                if (nested != null) return nested;
            }
            return null;
        }
    }
}
