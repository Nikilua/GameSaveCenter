using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System;

namespace GameSaveCenter.Playnite.Views
{
    public partial class MaintenanceView : UserControl
    {
        private double responsiveWidth;
        private double responsiveHeight;

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
            var lastStyle = TryFindResource("GscLastColumnHeader") as Style;

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

        public UniformGrid DiagnosticHealthPanelElement => DiagnosticHealthPanel;
        public DataGrid FindingsGridElement => FindingsGrid;

        public void ApplyResponsiveLayout(double width, double height)
        {
            responsiveWidth = width;
            responsiveHeight = height;
            DiagnosticHealthPanel.Columns = width >= 1320 ? 3 : width >= 760 ? 2 : 1;
            var inspectorWidth = MaintenanceDiagnosticsLayout.TryFindResource("GscInspectorWidth") is GridLength gl ? gl : new GridLength(360);
            // Health cards remain useful context even in compact windows. Grid star rows keep
            // diagnostics tables finite while their own controls handle overflow.
            DiagnosticHealthPanel.Visibility = Visibility.Visible;
            // Keep a predictable findings viewport instead of allowing the action cards,
            // health cards and diagnostic summary to squeeze the table down to one row.
            // MaintenanceDiagnosticsScrollSurface owns overflow outside this finite table.
            const double tableMinHeight = 236d;
            FindingsGrid.MinHeight = tableMinHeight;
            FindingsGrid.Height = Math.Max(tableMinHeight, Math.Min(460d, height * 0.50));
            MaintenanceDeviceGrid.MinHeight = tableMinHeight;
            MaintenanceAuditFindingsGrid.MinHeight = tableMinHeight;
            MaintenanceProcessGrid.MinHeight = tableMinHeight;
            var compact = height < 760 || width < 980;
            // The retention page is a left-aligned reading form capped at 1050.
            // Give the StackPanel an explicit viewport width so the cards fill
            // the form instead of collapsing to their content width, mirroring
            // the SaveCenter policy page. The 4 is the right padding of
            // GscPageScrollViewer.
            MaintenanceRetentionStack.Width = Math.Max(0, Math.Min(width - 4, 1050));
            MaintenanceRetentionMetrics.Columns = width >= 720 ? 3 : width >= 480 ? 2 : 1;
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
            var stackDiagnostics = width < 1120;
            var showDiagnosticsInspector = MaintenanceDiagnosticsInspector.Visibility == Visibility.Visible;
            var diagnosticsSideBySide = showDiagnosticsInspector && !stackDiagnostics;
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
            MaintenanceDiagnosticSummaryGrid.MinHeight = stackDiagnostics ? 96 : 140;
            MaintenanceDiagnosticSummaryGrid.MaxHeight = stackDiagnostics ? Math.Max(120, height * 0.20) : 280;
            var stackProcess = width < 1040;
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
            var stackDevice = width < 1180;
            MaintenanceDeviceLayout.ColumnDefinitions[1].Width = stackDevice ? new GridLength(0) : new GridLength(14);
            MaintenanceDeviceLayout.ColumnDefinitions[2].Width = stackDevice ? new GridLength(0) : inspectorWidth;
            MaintenanceDeviceLayout.RowDefinitions[3].Height = stackDevice ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
            Grid.SetColumn(MaintenanceDeviceInspectorScrollViewer, stackDevice ? 0 : 2);
            Grid.SetColumnSpan(MaintenanceDeviceInspectorScrollViewer, stackDevice ? 3 : 1);
            Grid.SetRow(MaintenanceDeviceInspectorScrollViewer, stackDevice ? 3 : 2);
            MaintenanceDeviceInspectorScrollViewer.Margin = new Thickness(0, 10, 0, 0);
            // Single inspector scroll owner owns both the manual decision and the
            // protected remote restore. It fills the table row on wide screens with
            // its own internal scroll and only gets the measured remaining budget when
            // stacking below the table on compact windows. This reserves a readable
            // table viewport at common windowed and high-DPI logical heights.
            var deviceAvailableHeight = MaintenanceDeviceLayout.ActualHeight > 0
                ? MaintenanceDeviceLayout.ActualHeight
                    - MaintenanceDeviceLayout.RowDefinitions[0].ActualHeight
                    - MaintenanceDeviceLayout.RowDefinitions[1].ActualHeight
                : Math.Max(320, height - 250);
            var deviceInspectorHeight = Math.Max(96, Math.Min(420, deviceAvailableHeight - tableMinHeight - 10));
            MaintenanceDeviceInspectorScrollViewer.MaxHeight = stackDevice ? deviceInspectorHeight : double.PositiveInfinity;

            var stackAudit = width < 1120 || height < 700;
            var showAuditInspector = MaintenanceAuditInspector.Visibility == Visibility.Visible;
            var auditSideBySide = showAuditInspector && !stackAudit;
            MaintenanceAuditLayout.ColumnDefinitions[1].Width = auditSideBySide ? new GridLength(14) : new GridLength(0);
            MaintenanceAuditLayout.ColumnDefinitions[2].Width = auditSideBySide ? inspectorWidth : new GridLength(0);
            MaintenanceAuditLayout.RowDefinitions[1].Height = showAuditInspector && stackAudit ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
            Grid.SetColumn(MaintenanceAuditInspector, auditSideBySide ? 2 : 0);
            Grid.SetColumnSpan(MaintenanceAuditInspector, stackAudit ? 3 : 1);
            Grid.SetRow(MaintenanceAuditInspector, stackAudit ? 1 : 0);
            MaintenanceAuditInspector.Margin = showAuditInspector && stackAudit ? new Thickness(0, 10, 0, 0) : new Thickness(0);
            // The detail inspector owns only the selected finding; the recent audit log
            // lives in its own full-width strip. In stacked mode both share the measured
            // vertical budget so the findings table keeps a readable viewport.
            var auditAvailableHeight = MaintenanceAuditLayout.ActualHeight > 0
                ? MaintenanceAuditLayout.ActualHeight - MaintenanceAuditLayout.RowDefinitions[2].ActualHeight
                : Math.Max(320, height - 200);
            var auditInspectorHeight = Math.Max(96, Math.Min(420, auditAvailableHeight - tableMinHeight - 10));
            MaintenanceAuditInspector.MaxHeight = showAuditInspector && stackAudit ? auditInspectorHeight : double.PositiveInfinity;
            MaintenanceAuditLogGrid.MinHeight = stackAudit ? 96 : 140;
            MaintenanceAuditLogGrid.MaxHeight = stackAudit ? Math.Max(120, height * 0.20) : 280;
        }
    }
}
