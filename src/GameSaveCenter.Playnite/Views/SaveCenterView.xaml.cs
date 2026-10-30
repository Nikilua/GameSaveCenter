using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.ViewModels;

namespace GameSaveCenter.Playnite.Views
{
    public partial class SaveCenterView : UserControl
    {
        private double responsiveWidth;
        private double responsiveHeight;
        private double lastRequestedHeight;
        private bool isApplyingLayout;
        private readonly ResponsiveDetailBreakpointLatch detailBreakpoint = new ResponsiveDetailBreakpointLatch();
        private bool historyInspectorOpen;
        private bool candidateInspectorOpen;
        private bool? candidateViewControlsCompact;
        private DataGridColumnLayoutController? historyColumnLayout;
        private DataGridColumnLayoutController? candidateColumnLayout;
        private DataGridColumnVisibilityController? historyColumnVisibility;
        private DataGridColumnVisibilityController? candidateColumnVisibility;
        private DataGridStableSortController? historySort;
        private DataGridStableSortController? candidateSort;
        private DataGridViewConfigurationController? historyViewConfiguration;
        private DataGridViewConfigurationController? candidateViewConfiguration;
        private readonly WrapPanelRowGapController historyActionRowGap;

        internal Action? PreferencePersistenceOverrideForBehaviorTest { get; set; }

        public SaveCenterView()
        {
            InitializeComponent();
            SaveHistoryStateColumn.DisplayIndex = 2;
            historyActionRowGap = new WrapPanelRowGapController(SaveHistorySummaryActions);
            historyActionRowGap.SetRowGap(ResponsiveLayoutCoordinator.CompactActionRowGap);
            SaveHistoryActionsScrollViewer.IsVisibleChanged += InspectorIsVisibleChanged;
            SaveCandidateInspectorScrollViewer.IsVisibleChanged += InspectorIsVisibleChanged;
            SaveHistoryPageScrollViewer.ScrollChanged += PageScrollViewerChanged;
            SaveCandidatePageScrollViewer.ScrollChanged += PageScrollViewerChanged;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            DataContextChanged += OnDataContextChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) => AttachColumnLayouts();

        private void OnUnloaded(object sender, RoutedEventArgs e) => DetachColumnLayouts();

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsLoaded) return;
            DetachColumnLayouts();
            if (!(DataContext is DashboardViewModel))
                SaveHistoryGrid.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("Backups"));
            AttachColumnLayouts();
        }

        private void AttachColumnLayouts()
        {
            if (historyColumnLayout != null || !(DataContext is DashboardViewModel viewModel)) return;
            var persistPreference = PreferencePersistenceOverrideForBehaviorTest ?? viewModel.PersistUiPreference;
            historyColumnLayout = new DataGridColumnLayoutController(
                SaveHistoryGrid,
                "save-history",
                new[] { "time", "type", "file-count", "size", "device", "note", "state" },
                viewModel.PluginSettings,
                persistPreference);
            candidateColumnLayout = new DataGridColumnLayoutController(
                SaveCandidateGrid,
                "save-candidates",
                new[] { "confidence", "status", "path", "reason" },
                viewModel.PluginSettings,
                persistPreference);
            historyColumnVisibility = new DataGridColumnVisibilityController(
                SaveHistoryGrid,
                SaveHistoryColumnVisibilityButton,
                "save-history",
                new[] { "time", "type", "file-count", "size", "device", "note", "state" },
                new[] { "time", "type", "state" },
                viewModel.PluginSettings,
                persistPreference,
                historyColumnLayout.RefreshResponsiveLayout);
            candidateColumnVisibility = new DataGridColumnVisibilityController(
                SaveCandidateGrid,
                SaveCandidateColumnVisibilityButton,
                "save-candidates",
                new[] { "confidence", "status", "path", "reason" },
                new[] { "status", "path" },
                viewModel.PluginSettings,
                persistPreference,
                candidateColumnLayout.RefreshResponsiveLayout);
            SaveHistoryGrid.SetBinding(ItemsControl.ItemsSourceProperty, new Binding(nameof(DashboardViewModel.BackupHistoryView)));
            historySort = ProductionDataGridSortProfiles.AttachSaveHistory(SaveHistoryGrid);
            candidateSort = ProductionDataGridSortProfiles.AttachSaveCandidates(SaveCandidateGrid);
            historyViewConfiguration = CreateViewConfigurationController(
                viewModel, "save-history", historyColumnLayout, historyColumnVisibility, historySort,
                SaveHistoryViewConfigurationComboBox, SaveHistoryViewConfigurationNameBox,
                SaveHistoryViewConfigurationSaveButton, SaveHistoryViewConfigurationCopyButton,
                SaveHistoryViewConfigurationRenameButton, SaveHistoryViewConfigurationDeleteButton,
                SaveHistoryViewConfigurationDirtyStatusText, SaveHistoryViewConfigurationRestoreButton,
                SaveHistoryViewConfigurationResetButton);
            candidateViewConfiguration = CreateViewConfigurationController(
                viewModel, "save-candidates", candidateColumnLayout, candidateColumnVisibility, candidateSort,
                SaveCandidateViewConfigurationComboBox, SaveCandidateViewConfigurationNameBox,
                SaveCandidateViewConfigurationSaveButton, SaveCandidateViewConfigurationCopyButton,
                SaveCandidateViewConfigurationRenameButton, SaveCandidateViewConfigurationDeleteButton,
                SaveCandidateViewConfigurationDirtyStatusText, SaveCandidateViewConfigurationRestoreButton,
                SaveCandidateViewConfigurationResetButton);
        }

        private DataGridViewConfigurationController CreateViewConfigurationController(
            DashboardViewModel viewModel,
            string workspaceKey,
            DataGridColumnLayoutController layout,
            DataGridColumnVisibilityController visibility,
            DataGridStableSortController sort,
            ComboBox selector,
            TextBox nameBox,
            Button saveButton,
            Button copyButton,
            Button renameButton,
            Button deleteButton,
            TextBlock dirtyStatus,
            Button restoreButton,
            Button resetLayoutButton)
            => new DataGridViewConfigurationController(
                this, workspaceKey, viewModel.PluginSettings, layout, visibility, sort,
                selector, nameBox, saveButton, copyButton, renameButton, deleteButton,
                dirtyStatus, restoreButton, resetLayoutButton,
                PreferencePersistenceOverrideForBehaviorTest ?? viewModel.PersistUiPreference,
                null, null, null,
                null, null, null,
                viewModel.ConfirmViewConfigurationMutationAsync,
                viewModel.ReportViewConfigurationStatus);

        private void DetachColumnLayouts()
        {
            historyViewConfiguration?.Dispose();
            candidateViewConfiguration?.Dispose();
            candidateColumnVisibility?.Dispose();
            historyColumnVisibility?.Dispose();
            candidateColumnLayout?.Dispose();
            historyColumnLayout?.Dispose();
            candidateSort?.Dispose();
            historySort?.Dispose();
            candidateColumnLayout = null;
            historyColumnLayout = null;
            candidateColumnVisibility = null;
            historyColumnVisibility = null;
            candidateSort = null;
            historySort = null;
            historyViewConfiguration = null;
            candidateViewConfiguration = null;
        }

        private void InspectorIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (isApplyingLayout || !IsLoaded || responsiveWidth <= 0 || responsiveHeight <= 0)
                return;

            ApplyResponsiveLayout(responsiveWidth, lastRequestedHeight > 0 ? lastRequestedHeight : responsiveHeight);
        }

        private void PageScrollViewerChanged(object sender, ScrollChangedEventArgs e)
        {
            if (isApplyingLayout || !IsLoaded || responsiveWidth <= 0
                || !(sender is ScrollViewer viewer) || !viewer.IsVisible || viewer.ViewportHeight <= 0)
                return;

            if (Math.Abs(e.ViewportHeightChange) < 0.1
                && Math.Abs(e.ViewportWidthChange) < 0.1
                && Math.Abs(e.ExtentHeightChange) < 0.1)
                return;

            ApplyResponsiveLayout(responsiveWidth, viewer.ViewportHeight);
        }

        public void ApplyResponsiveLayout(double width, double height)
        {
            if (isApplyingLayout) return;
            isApplyingLayout = true;
            historyColumnLayout?.BeginLayoutPass();
            candidateColumnLayout?.BeginLayoutPass();
            try
            {
                responsiveWidth = width;
                // The shell gives the first-pass PageHost size. Once WPF measures the
                // selected tab, its ScrollViewer reports the settled viewport and this
                // method runs again with that value.
                var measuredViewportHeight = SaveCandidatePageScrollViewer.IsVisible
                    ? SaveCandidatePageScrollViewer.ViewportHeight
                    : SaveHistoryPageScrollViewer.ViewportHeight;
                var requestedHeight = height > 0 ? height : lastRequestedHeight;
                if (height > 0)
                    lastRequestedHeight = height;
                var pageViewportHeight = WorkspaceTableViewportPolicy.ResolvePageViewportHeight(
                    requestedHeight,
                    measuredViewportHeight);
                responsiveHeight = pageViewportHeight;
                var tableMinHeight = TableDensityPolicy.CalculateMinimumGridHeight(this);
                SaveHistoryGrid.MinHeight = tableMinHeight;
                SaveCandidateGrid.MinHeight = tableMinHeight;
                var historyPageBudgetHeight = Math.Max(0,
                    pageViewportHeight - SaveHistoryLayout.Margin.Top - SaveHistoryLayout.Margin.Bottom);
                var historyFrameChromeHeight = GetTableFrameChromeHeight(
                    SaveHistoryTableLayout,
                    SaveHistoryTableFrame,
                    tableRow: 1,
                    fallbackHeight: 168d);
                var candidateFrameChromeHeight = GetTableFrameChromeHeight(
                    SaveCandidateTableLayout,
                    SaveCandidateTableFrame,
                    tableRow: 0,
                    fallbackHeight: 52d);
                // Demo keeps the inspector beside the main surface at the normal
                // 1040x700 breakpoint.  The previous 1200 DIP cutoff pushed the
                // Playnite host into the drawer layout even when there was enough
                // room, which made the production page structurally different.
                var wasWideDetailLayout = detailBreakpoint.IsInitialized && !detailBreakpoint.IsCompact;
                var compact = detailBreakpoint.Evaluate(width);
                if (!candidateViewControlsCompact.HasValue || candidateViewControlsCompact.Value != compact)
                {
                    candidateViewControlsCompact = compact;
                    SaveCandidateViewConfigurationExpander.IsExpanded = !compact;
                }
                SaveHistoryRestoreAvailabilityHint.Visibility = compact
                    ? Visibility.Collapsed
                    : Visibility.Visible;
                SaveHistoryInspectorAvailabilityHint.Visibility = compact
                    ? Visibility.Visible
                    : Visibility.Collapsed;
                if (wasWideDetailLayout && compact)
                {
                    // The same inspector is already visible beside the selected row.
                    // Keep it open when the layout moves below the table so focus and
                    // its ScrollViewer offset remain attached to the selected object.
                    if (SaveHistoryGrid.SelectedItem != null
                        && SaveHistoryActionsScrollViewer.Visibility == Visibility.Visible)
                        historyInspectorOpen = true;
                    if (SaveCandidateGrid.SelectedItem != null
                        && SaveCandidateInspectorScrollViewer.Visibility == Visibility.Visible)
                        candidateInspectorOpen = true;
                }
                var ruleCardCompact = width < 700;
                if (SaveCurrentRuleCard != null)
                {
                    SaveCurrentRuleCard.Padding = compact
                        ? new Thickness(8, 0, 8, 0)
                        : new Thickness(12, 10, 12, 10);
                    SaveCurrentRuleSublineText.Visibility = compact
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                    SaveCandidatePageSurface.RowDefinitions[2].Height = compact
                        ? new GridLength(0)
                        : new GridLength(14);
                }
                var inspectorWidth = SaveHistoryLayout.TryFindResource("GscWorkspaceInspectorWidth") is GridLength gl
                    ? gl
                    : new GridLength(1, GridUnitType.Star);
                if (SaveCurrentRuleActions != null)
                {
                    SaveCurrentRuleActionsRow.Height = ruleCardCompact
                        ? GridLength.Auto
                        : new GridLength(0);
                    SaveCurrentRuleActionsColumn.Width = ruleCardCompact
                        ? new GridLength(0)
                        : GridLength.Auto;
                    Grid.SetRow(SaveCurrentRuleActions, ruleCardCompact ? 1 : 0);
                    Grid.SetColumn(SaveCurrentRuleActions, ruleCardCompact ? 0 : 3);
                    Grid.SetColumnSpan(SaveCurrentRuleActions, ruleCardCompact ? 4 : 1);
                    SaveCurrentRuleActions.Margin = ruleCardCompact
                        ? new Thickness(0, 12, 0, 0)
                        : new Thickness(14, 0, 0, 0);
                    SaveCurrentRuleActions.HorizontalAlignment = ruleCardCompact
                        ? HorizontalAlignment.Stretch
                        : HorizontalAlignment.Right;
                }
                // On compact hosts the selected-version inspector is a drawer, not a
                // permanent second row. The table keeps the finite star row by default;
                // every action remains one click away through the compact details button.
                if (compact)
                {
                    var hasHistorySelection = SaveHistoryGrid.SelectedItem != null;
                    if (hasHistorySelection)
                    {
                        SaveHistoryActionsScrollViewer.Visibility = historyInspectorOpen
                            ? Visibility.Visible
                            : Visibility.Collapsed;
                        SaveHistoryCompactDetailsButton.Content = historyInspectorOpen
                            ? "收起版本详情 ›"
                            : "查看版本详情 ›";
                        SaveHistoryCompactDetailsButton.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        SaveHistoryCompactDetailsButton.Visibility = Visibility.Collapsed;
                        historyInspectorOpen = false;
                    }

                    var hasCandidateSelection = SaveCandidateGrid.SelectedItem != null;
                    if (hasCandidateSelection)
                    {
                        SaveCandidateInspectorScrollViewer.Visibility = candidateInspectorOpen
                            ? Visibility.Visible
                            : Visibility.Collapsed;
                        SaveCandidateCompactDetailsButton.Content = candidateInspectorOpen
                            ? "收起候选详情 ›"
                            : "查看候选详情 ›";
                        SaveCandidateCompactDetailsButton.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        SaveCandidateCompactDetailsButton.Visibility = Visibility.Collapsed;
                        candidateInspectorOpen = false;
                    }
                }
                else
                {
                    SaveHistoryCompactDetailsButton.Visibility = Visibility.Collapsed;
                    SaveCandidateCompactDetailsButton.Visibility = Visibility.Collapsed;
                    SaveHistoryActionsScrollViewer.Visibility = SaveHistoryGrid.SelectedItem != null
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                    SaveCandidateInspectorScrollViewer.Visibility = SaveCandidateGrid.SelectedItem != null
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
            // The demo keeps the history table and the selected-version inspector
            // side by side when there is room. Compact pages stack the inspector;
            // their outer ScrollViewer keeps the complete finite table and detail card
            // reachable without letting either inner scrollbar escape its surface.
            var showHistoryInspector = SaveHistoryActionsScrollViewer.Visibility == Visibility.Visible;
            SaveHistoryInspectorFrame.Visibility = SaveHistoryActionsScrollViewer.Visibility;
            var historySideBySide = showHistoryInspector && !compact;
            SaveHistoryLayout.ColumnDefinitions[1].Width = historySideBySide ? new GridLength(14) : new GridLength(0);
            SaveHistoryLayout.ColumnDefinitions[2].Width = historySideBySide ? inspectorWidth : new GridLength(0);
            SaveHistoryLayout.RowDefinitions[1].Height = showHistoryInspector && compact ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
            Grid.SetColumn(SaveHistoryInspectorFrame, historySideBySide ? 2 : 0);
            Grid.SetColumnSpan(SaveHistoryInspectorFrame, compact ? 3 : 1);
            Grid.SetRow(SaveHistoryInspectorFrame, compact ? 1 : 0);
            SaveHistoryInspectorFrame.Margin = showHistoryInspector && compact ? new Thickness(0, 10, 0, 0) : new Thickness(0);
            var historyInspectorHeight = Math.Min(220, Math.Max(0, pageViewportHeight * 0.30));
            var historyInspectorBudget = showHistoryInspector && compact
                ? historyInspectorHeight + 30 + SaveHistoryInspectorFrame.Margin.Top + SaveHistoryInspectorFrame.Margin.Bottom
                : 0;
            var historyTableBudget = WorkspaceTableViewportPolicy.CalculateTableHeight(
                historyPageBudgetHeight,
                tableMinHeight,
                historyFrameChromeHeight,
                historyInspectorBudget);
            // Let the history grid use the complete height left by the current PageHost.
            // A fixed desktop-sized cap left large 4K workspaces with unused vertical space.
            var tableViewportHeight = Math.Max(tableMinHeight, historyTableBudget);
            SaveHistoryGrid.Height = tableViewportHeight;
            SaveHistoryGrid.MaxHeight = tableViewportHeight;
            SaveHistoryInspectorFrame.MaxHeight = showHistoryInspector
                ? (compact ? historyInspectorHeight + 30 : tableViewportHeight + historyFrameChromeHeight)
                : double.PositiveInfinity;
            SaveHistoryActionsScrollViewer.MaxHeight = showHistoryInspector
                ? (compact ? historyInspectorHeight : Math.Max(0, tableViewportHeight + historyFrameChromeHeight - 30))
                : double.PositiveInfinity;
            var showCandidateInspector = SaveCandidateInspectorScrollViewer.Visibility == Visibility.Visible;
            SaveCandidateInspectorFrame.Visibility = SaveCandidateInspectorScrollViewer.Visibility;
            var candidateSideBySide = showCandidateInspector && !compact;
            SaveCandidateLayout.ColumnDefinitions[1].Width = candidateSideBySide ? new GridLength(14) : new GridLength(0);
            SaveCandidateLayout.ColumnDefinitions[2].Width = candidateSideBySide ? inspectorWidth : new GridLength(0);
            SaveCandidateLayout.RowDefinitions[1].Height = showCandidateInspector && compact ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
            Grid.SetColumn(SaveCandidateInspectorFrame, candidateSideBySide ? 2 : 0);
            Grid.SetColumnSpan(SaveCandidateInspectorFrame, compact ? 3 : 1);
            Grid.SetRow(SaveCandidateInspectorFrame, compact ? 1 : 0);
            SaveCandidateInspectorFrame.Margin = showCandidateInspector && compact ? new Thickness(0, 10, 0, 0) : new Thickness(0);
            var candidateInspectorHeight = Math.Min(360, Math.Max(0, pageViewportHeight * 0.30));
            var candidateTopChromeHeight = SaveCandidatePageSurface.RowDefinitions[0].ActualHeight
                + SaveCandidatePageSurface.RowDefinitions[1].ActualHeight
                + SaveCandidatePageSurface.RowDefinitions[2].ActualHeight;
            var candidateInspectorBudget = showCandidateInspector && compact
                ? candidateInspectorHeight + 30 + SaveCandidateInspectorFrame.Margin.Top + SaveCandidateInspectorFrame.Margin.Bottom
                : 0;
            var candidateTableBudget = WorkspaceTableViewportPolicy.CalculateTableHeight(
                pageViewportHeight,
                tableMinHeight,
                candidateTopChromeHeight + candidateFrameChromeHeight,
                candidateInspectorBudget);
            var candidateTableViewportHeight = Math.Max(tableMinHeight, candidateTableBudget);
            SaveCandidateGrid.Height = candidateTableViewportHeight;
            SaveCandidateGrid.MaxHeight = candidateTableViewportHeight;
            SaveCandidateInspectorFrame.MaxHeight = showCandidateInspector
                ? (compact ? candidateInspectorHeight + 30 : candidateTableViewportHeight + candidateFrameChromeHeight)
                : double.PositiveInfinity;
            SaveCandidateInspectorScrollViewer.MaxHeight = showCandidateInspector
                ? (compact ? candidateInspectorHeight : Math.Max(0, candidateTableViewportHeight + candidateFrameChromeHeight - 30))
                : double.PositiveInfinity;
            var stackPolicy = width < 980;
            // Keep the three Demo cards in one row when there is room. Below
            // the compact breakpoint, stack the cards into explicit rows so
            // the template controls never share measure space with the list.
            SavePolicyStack.Width = Math.Max(0, width - 4);
            SavePolicyCardsLayout.ColumnDefinitions[1].Width = stackPolicy ? new GridLength(0) : new GridLength(14);
            SavePolicyCardsLayout.ColumnDefinitions[2].Width = stackPolicy ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
            SavePolicyCardsLayout.ColumnDefinitions[3].Width = stackPolicy ? new GridLength(0) : new GridLength(14);
            SavePolicyCardsLayout.ColumnDefinitions[4].Width = stackPolicy ? new GridLength(0) : new GridLength(1, GridUnitType.Star);
            SavePolicyCardsLayout.RowDefinitions[0].Height = stackPolicy ? new GridLength(1, GridUnitType.Auto) : new GridLength(1, GridUnitType.Star);
            SavePolicyCardsLayout.RowDefinitions[1].Height = stackPolicy ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
            SavePolicyCardsLayout.RowDefinitions[2].Height = stackPolicy ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
            SavePolicyCardsLayout.RowDefinitions[3].Height = new GridLength(0);

            Grid.SetColumn(SaveBackupAutomationCard, 0);
            Grid.SetColumnSpan(SaveBackupAutomationCard, stackPolicy ? 5 : 1);
            Grid.SetRow(SaveBackupAutomationCard, 0);

            Grid.SetColumn(SavePolicyMediaCard, stackPolicy ? 0 : 2);
            Grid.SetColumnSpan(SavePolicyMediaCard, stackPolicy ? 5 : 1);
            Grid.SetRow(SavePolicyMediaCard, stackPolicy ? 1 : 0);
            SavePolicyMediaCard.Margin = stackPolicy ? new Thickness(0, 14, 0, 0) : new Thickness(0);

            Grid.SetColumn(SavePolicyTemplatesCard, stackPolicy ? 0 : 4);
            Grid.SetColumnSpan(SavePolicyTemplatesCard, stackPolicy ? 5 : 1);
            Grid.SetRow(SavePolicyTemplatesCard, stackPolicy ? 2 : 0);
            SavePolicyTemplatesCard.Margin = stackPolicy ? new Thickness(0, 14, 0, 0) : new Thickness(0);


            // The Demo keeps comparison and retention as two peer cards inside one
            // horizontally scrollable canvas. Do not stack the cards into the same
            // narrow vertical flow: that was the source of the production page's
            // large-height/empty-column mismatch at small widths.
            SaveCompareLayout.ColumnDefinitions[1].Width = new GridLength(14);
            SaveCompareLayout.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
            SaveComparePageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            SaveComparePageScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            SaveCompareRetentionScrollViewer.Margin = new Thickness(0, 10, 0, 0);
            SaveCompareRetentionScrollViewer.MaxHeight = double.PositiveInfinity;
            SaveCompareMainScrollViewer.MaxHeight = double.PositiveInfinity;
            SaveCompareMainScrollViewer.MinHeight = 0;
            }
            finally
            {
                candidateColumnLayout?.EndLayoutPass();
                historyColumnLayout?.EndLayoutPass();
                isApplyingLayout = false;
            }
        }

        private static double GetTableFrameChromeHeight(
            Grid tableLayout,
            Border tableFrame,
            int tableRow,
            double fallbackHeight)
        {
            var fixedRowsHeight = 0d;
            for (var index = 0; index < tableLayout.RowDefinitions.Count; index++)
            {
                if (index != tableRow)
                    fixedRowsHeight += tableLayout.RowDefinitions[index].ActualHeight;
            }

            fixedRowsHeight += tableFrame.Padding.Top + tableFrame.Padding.Bottom
                + tableFrame.BorderThickness.Top + tableFrame.BorderThickness.Bottom;
            return fixedRowsHeight > 0 ? fixedRowsHeight : Math.Max(0, fallbackHeight);
        }

        private void OnSaveHistorySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            historyInspectorOpen = false;
            if (IsLoaded && responsiveWidth > 0 && responsiveHeight > 0)
                ApplyResponsiveLayout(responsiveWidth, lastRequestedHeight > 0 ? lastRequestedHeight : responsiveHeight);
        }

        private void OnSaveCandidateSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            candidateInspectorOpen = false;
            if (IsLoaded && responsiveWidth > 0 && responsiveHeight > 0)
                ApplyResponsiveLayout(responsiveWidth, lastRequestedHeight > 0 ? lastRequestedHeight : responsiveHeight);
        }

        private void OnSaveHistoryCompactDetailsClick(object sender, RoutedEventArgs e)
        {
            if (SaveHistoryGrid.SelectedItem == null) return;
            historyInspectorOpen = !historyInspectorOpen;
            ApplyResponsiveLayout(responsiveWidth > 0 ? responsiveWidth : ActualWidth, lastRequestedHeight > 0 ? lastRequestedHeight : responsiveHeight > 0 ? responsiveHeight : ActualHeight);
            FocusElement(historyInspectorOpen ? SaveHistoryActionsScrollViewer : SaveHistoryCompactDetailsButton);
        }

        private void OnSaveCandidateCompactDetailsClick(object sender, RoutedEventArgs e)
        {
            if (SaveCandidateGrid.SelectedItem == null) return;
            // This button lives in the view-controls disclosure header. Keep its
            // action separate from expanding or collapsing the disclosure itself.
            e.Handled = true;
            candidateInspectorOpen = !candidateInspectorOpen;
            ApplyResponsiveLayout(responsiveWidth > 0 ? responsiveWidth : ActualWidth, lastRequestedHeight > 0 ? lastRequestedHeight : responsiveHeight > 0 ? responsiveHeight : ActualHeight);
            FocusElement(candidateInspectorOpen ? SaveCandidateInspectorScrollViewer : SaveCandidateCompactDetailsButton);
        }

        private void OnSaveInspectorPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
                return;

            if (ReferenceEquals(sender, SaveHistoryActionsScrollViewer) && historyInspectorOpen)
                historyInspectorOpen = false;
            else if (ReferenceEquals(sender, SaveCandidateInspectorScrollViewer) && candidateInspectorOpen)
                candidateInspectorOpen = false;
            else
                return;

            ApplyResponsiveLayout(responsiveWidth > 0 ? responsiveWidth : ActualWidth, lastRequestedHeight > 0 ? lastRequestedHeight : responsiveHeight > 0 ? responsiveHeight : ActualHeight);
            FocusElement(ReferenceEquals(sender, SaveHistoryActionsScrollViewer)
                ? SaveHistoryCompactDetailsButton
                : SaveCandidateCompactDetailsButton);
            e.Handled = true;
        }

        private static void FocusElement(UIElement element)
        {
            if (!element.IsVisible || !element.IsEnabled)
                return;

            element.Focus();
            Keyboard.Focus(element);
        }
    }
}
