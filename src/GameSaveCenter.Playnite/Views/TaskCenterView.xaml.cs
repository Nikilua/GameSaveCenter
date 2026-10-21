using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.ViewModels;

namespace GameSaveCenter.Playnite.Views
{
    /// <summary>Physical task-center workspace; it deliberately has no current-game picker.</summary>
    public partial class TaskCenterView : UserControl
    {
        internal const double CompactFilterBreakpoint = 1500d;

        private bool isApplyingLayout;
        private double responsiveWidth;
        private double responsiveHeight;
        private double lastRequestedHeight;
        private readonly ResponsiveDetailBreakpointLatch detailBreakpoint = new ResponsiveDetailBreakpointLatch();
        private readonly WrapPanelRowGapController taskFilterPresetRowGap;
        private bool taskInspectorOpen;
        private DataGridColumnLayoutController? columnLayout;
        private DataGridColumnVisibilityController? columnVisibility;
        private DataGridStableSortController? sortController;
        private DataGridViewConfigurationController? viewConfigurationController;
        private ScrollViewer? taskGridScrollViewer;
        private DashboardViewModel? navigationViewModel;
        private bool restoringTaskGridScroll;

        public TaskCenterView()
        {
            InitializeComponent();
            taskFilterPresetRowGap = new WrapPanelRowGapController(TaskFilterPresetRow);
            TaskPageScrollViewer.ScrollChanged += OnTaskPageScrollChanged;
            DataGridScrollDiagnostics.Attach(TaskGrid, "TaskGrid", GetScrollDiagnosticContext);
            TaskGrid.Loaded += OnTaskGridLoaded;
            TaskGrid.Unloaded += OnTaskGridUnloaded;
            TaskDetailScrollViewer.IsVisibleChanged += OnTaskDetailScrollViewerIsVisibleChanged;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            DataContextChanged += OnDataContextChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SubscribeNavigationViewModel();
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(TryRestoreTaskGridScroll));
            if (columnLayout != null || !(DataContext is DashboardViewModel viewModel)) return;
            columnLayout = new DataGridColumnLayoutController(
                TaskGrid,
                "tasks",
                new[] { "local-time", "task", "stage", "game", "state", "progress", "detail" },
                viewModel.PluginSettings,
                viewModel.PersistUiPreference);
            columnVisibility = new DataGridColumnVisibilityController(
                TaskGrid,
                TaskGridColumnVisibilityButton,
                "tasks",
                new[] { "local-time", "task", "stage", "game", "state", "progress", "detail" },
                new[] { "local-time", "task", "state" },
                viewModel.PluginSettings,
                viewModel.PersistUiPreference,
                columnLayout.RefreshResponsiveLayout);
            sortController = ProductionDataGridSortProfiles.AttachTasks(TaskGrid);
            viewConfigurationController = new DataGridViewConfigurationController(
                this,
                "tasks",
                viewModel.PluginSettings,
                columnLayout,
                columnVisibility,
                sortController,
                TaskViewConfigurationComboBox,
                TaskViewConfigurationNameBox,
                TaskViewConfigurationSaveButton,
                TaskViewConfigurationCopyButton,
                TaskViewConfigurationRenameButton,
                TaskViewConfigurationDeleteButton,
                TaskViewConfigurationDirtyStatusText,
                TaskViewConfigurationRestoreButton,
                TaskQueueResetColumnWidthButton,
                viewModel.PersistUiPreference,
                viewModel.GetSelectedTaskFilterPresetId,
                viewModel.ApplyTaskFilterPresetById,
                viewModel.SetTaskViewConfigurationFilterPersistenceEnabled,
                viewModel,
                viewModel.CaptureTaskFilterStateFingerprint,
                viewModel.GetTaskFilterPresetFingerprint,
                viewModel.ConfirmViewConfigurationMutationAsync,
                viewModel.ReportViewConfigurationStatus);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            CaptureTaskGridScroll();
            DetachTaskGridScrollViewer();
            UnsubscribeNavigationViewModel();
            viewConfigurationController?.Dispose();
            columnVisibility?.Dispose();
            columnLayout?.Dispose();
            sortController?.Dispose();
            columnLayout = null;
            columnVisibility = null;
            viewConfigurationController = null;
            sortController = null;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UnsubscribeNavigationViewModel();
            SubscribeNavigationViewModel();
            if (!IsLoaded) return;
            OnUnloaded(this, new RoutedEventArgs());
            OnLoaded(this, new RoutedEventArgs());
        }

        private string GetScrollDiagnosticContext()
            => DataContext is DashboardViewModel viewModel
                ? viewModel.GetTaskScrollDiagnosticContext()
                : "vm=none";

        private void OnTaskGridLoaded(object sender, RoutedEventArgs e)
        {
            AttachTaskGridScrollViewer();
            TryRestoreTaskGridScroll();
        }

        private void OnTaskGridUnloaded(object sender, RoutedEventArgs e)
        {
            CaptureTaskGridScroll();
            DetachTaskGridScrollViewer();
        }

        private void SubscribeNavigationViewModel()
        {
            if (navigationViewModel != null || !(DataContext is DashboardViewModel viewModel)) return;
            navigationViewModel = viewModel;
            navigationViewModel.PropertyChanged += OnNavigationViewModelPropertyChanged;
        }

        private void UnsubscribeNavigationViewModel()
        {
            if (navigationViewModel == null) return;
            navigationViewModel.PropertyChanged -= OnNavigationViewModelPropertyChanged;
            navigationViewModel = null;
        }

        private void OnNavigationViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DashboardViewModel.IsWorklistExportPreviewOpen))
            {
                if (Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished) return;
                var restoreFocus = WorklistExportPreviewOverlay.IsKeyboardFocusWithin;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
                {
                    if (navigationViewModel?.IsWorklistExportPreviewOpen == true)
                        FocusElement(WorklistExportPreviewTextBox);
                    else if (restoreFocus)
                        FocusElement(WorklistExportPreviewButton);
                }));
                return;
            }

            if (e.PropertyName != nameof(DashboardViewModel.PendingTaskGridScrollRestore)
                && e.PropertyName != nameof(DashboardViewModel.TaskPageState)) return;
            if (Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished) return;
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(TryRestoreTaskGridScroll));
        }

        private void AttachTaskGridScrollViewer()
        {
            if (taskGridScrollViewer != null) return;
            taskGridScrollViewer = FindVisualChild<ScrollViewer>(TaskGrid);
            if (taskGridScrollViewer != null)
                taskGridScrollViewer.ScrollChanged += OnTaskGridScrollChanged;
        }

        private void DetachTaskGridScrollViewer()
        {
            if (taskGridScrollViewer == null) return;
            taskGridScrollViewer.ScrollChanged -= OnTaskGridScrollChanged;
            taskGridScrollViewer = null;
        }

        private void OnTaskGridScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!restoringTaskGridScroll)
                navigationViewModel?.SetTaskGridScrollOffset(e.VerticalOffset);
        }

        private void CaptureTaskGridScroll()
        {
            if (taskGridScrollViewer != null)
                navigationViewModel?.SetTaskGridScrollOffset(taskGridScrollViewer.VerticalOffset);
        }

        private void TryRestoreTaskGridScroll()
        {
            var viewModel = navigationViewModel;
            var viewer = taskGridScrollViewer;
            var pending = viewModel?.PendingTaskGridScrollRestore;
            if (viewModel == null || viewer == null || !pending.HasValue) return;
            var target = Math.Max(0, pending.Value);
            if (target > 0 && viewer.ScrollableHeight <= 0) return;
            restoringTaskGridScroll = true;
            try
            {
                viewer.ScrollToVerticalOffset(Math.Min(target, viewer.ScrollableHeight));
                viewModel.CompleteTaskGridScrollRestore();
            }
            finally
            {
                restoringTaskGridScroll = false;
            }
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

        private void OnClearSearchTextBoxClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement source && source.Tag is TextBox textBox)
            {
                textBox.Clear();
                textBox.Focus();
                Keyboard.Focus(textBox);
            }
            e.Handled = true;
        }

        private void OnWorklistBrowserSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is DashboardViewModel viewModel && WorklistBrowserGrid != null)
                viewModel.SetWorklistBrowserSelection(WorklistBrowserGrid.SelectedItems.Cast<WorklistBrowserEntry>());
        }

        private void OnTaskDetailScrollViewerIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (isApplyingLayout || !IsLoaded) return;
            ApplyResponsiveLayout(
                TaskPageScrollSurface.ActualWidth > 0 ? TaskPageScrollSurface.ActualWidth : TaskWorkspaceLayout.ActualWidth,
                lastRequestedHeight > 0 ? lastRequestedHeight : responsiveHeight > 0 ? responsiveHeight : TaskWorkspaceLayout.ActualHeight);
        }

        private void OnTaskPageScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (isApplyingLayout || !IsLoaded || TaskPageScrollViewer.ViewportHeight <= 0)
                return;

            if (Math.Abs(e.ViewportHeightChange) < 0.1
                && Math.Abs(e.ViewportWidthChange) < 0.1
                && Math.Abs(e.ExtentHeightChange) < 0.1)
                return;

            ApplyResponsiveLayout(
                responsiveWidth > 0 ? responsiveWidth : TaskPageScrollSurface.ActualWidth,
                TaskPageScrollViewer.ViewportHeight);
        }

        public Border TaskSummaryPanelElement => TaskSummaryPanel;
        public Border TaskDetailCardElement => TaskDetailCard;
        public ScrollViewer TaskDetailScrollViewerElement => TaskDetailScrollViewer;

        public void ApplyResponsiveLayout(double width, double height)
        {
            if (isApplyingLayout) return;
            isApplyingLayout = true;
            try
            {
                responsiveWidth = width;
                const double compactInspectorMaxHeight = 136d;
                var wasWideDetailLayout = detailBreakpoint.IsInitialized && !detailBreakpoint.IsCompact;
                var stack = detailBreakpoint.Evaluate(width);
                // Use the shell size for the first pass. The page ScrollViewer reports its
                // settled viewport after measure and refines this value without feeding the
                // growing content extent back into the table budget.
                var requestedHeight = height > 0 ? height : lastRequestedHeight > 0 ? lastRequestedHeight : ActualHeight;
                if (height > 0)
                    lastRequestedHeight = height;
                var availableHeight = WorkspaceTableViewportPolicy.ResolvePageViewportHeight(
                    requestedHeight,
                    TaskPageScrollViewer.ViewportHeight);
                responsiveHeight = availableHeight;
                if (wasWideDetailLayout && stack
                    && TaskGrid.SelectedItem != null
                    && TaskDetailScrollViewer.Visibility == Visibility.Visible)
                {
                    // Preserve the selected task's existing inspector when the same
                    // ScrollViewer moves below the queue at the compact breakpoint.
                    taskInspectorOpen = true;
                }
                var tableMinHeight = TableDensityPolicy.CalculateMinimumGridHeight(this);
                TaskGrid.MinHeight = tableMinHeight;
                // Responsive stacking changes the page layout; the shared user density
                // remains the only source for table row height in every breakpoint.
                TaskGrid.ClearValue(DataGrid.RowHeightProperty);
                TaskGrid.RowStyle = (Style)FindResource(stack ? "TaskCompactDataGridRow" : "GscStableDataGridRow");
                // Keep the Demo's four metrics available at every width. Compact pages move
                // the summary into the collapsed overview disclosure, leaving the queue its
                // first viewport while the page viewer carries lower details.
                var shortHeight = availableHeight > 0 && availableHeight < 720;
                TaskSummaryPanel.Visibility = Visibility.Visible;
                TaskSummaryPanel.MinHeight = shortHeight ? 52 : 84;
                TaskSummaryPanel.Padding = shortHeight ? new Thickness(6, 5, 6, 5) : new Thickness(6, 14, 6, 14);
                // Keep the four count/action entry points, but move their explanatory
                // sublines out of a short first viewport. Toolbars retain their ToolTip
                // and filter semantics; this only frees vertical room for real task rows.
                TaskWaitingSummaryText.Visibility = shortHeight ? Visibility.Collapsed : Visibility.Visible;
                TaskRetrySummaryText.Visibility = shortHeight ? Visibility.Collapsed : Visibility.Visible;
                // The action row stays horizontal on all common compact widths; only a
                // genuinely narrow pane stacks the three commands vertically.
                TaskDetailActions.Orientation = width < 520 ? Orientation.Vertical : Orientation.Horizontal;

                // The primary toolbar is a finite Grid rather than a WrapPanel.
                // Compact layouts place secondary filters and view tools in the
                // disclosure so search, status, and refresh keep a stable first row.
                // The full filter row needs enough room for its five fixed-width filters,
                // the refresh action, and a 420 DIP search field.  At narrower host widths
                // the Grid can still arrange its columns while the TextBox overflows its
                // search cell, hiding the right edge under the next filter.  Keep secondary
                // filters in the disclosure until the complete row fits.
                var compactFilters = width < CompactFilterBreakpoint;
                // The shared controller uses arranged rows, including changes to
                // text, visibility and host sizing, rather than a guessed breakpoint.
                taskFilterPresetRowGap.SetRowGap(ResponsiveLayoutCoordinator.CompactActionRowGap);
                TaskMoreFiltersExpander.Visibility = compactFilters ? Visibility.Visible : Visibility.Collapsed;
                TaskFilterBar.Padding = compactFilters ? new Thickness(8, 6, 8, 6) : new Thickness(10, 8, 10, 8);
                TaskMoreFiltersExpander.Margin = compactFilters ? new Thickness(0, 0, 0, 4) : new Thickness(0, 0, 0, 8);
                TaskQueuePanel.Padding = stack ? new Thickness(6, 2, 6, 2) : new Thickness(6, 10, 6, 10);
                TaskQueueHeaderLayoutGrid.Margin = stack
                    ? new Thickness(8, 0, 8, 4)
                    : new Thickness(12, 2, 12, 8);
                TaskQueueLastUpdatedSummary.Visibility = stack ? Visibility.Collapsed : Visibility.Visible;
                SetCompactFilterPlacement(compactFilters);
                // The second row hosts the named filter presets. Keep it measurable
                // after every responsive pass; collapsing it would silently remove
                // the save/apply controls on the compact host path.
                TaskFiltersPanel.RowDefinitions[1].Height = GridLength.Auto;
                Grid.SetRow(TaskSearchBoxHost, 0);
                Grid.SetColumn(TaskSearchBoxHost, 0);
                Grid.SetRow(TaskRefreshButton, 0);
                Grid.SetColumn(TaskRefreshButton, compactFilters ? 2 : 6);

                Grid.SetColumnSpan(TaskSearchBoxHost, 1);
                Grid.SetRow(TaskSearchBoxHost, 0);

                // Give the common desktop width a stable rhythm like the Demo's
                // search/status/type toolbar.  The old WrapPanel relied on each
                // control's theme default width, which made the search box collapse
                // to a sliver in the real Playnite host even though no elements
                // technically overlapped.
                if (!compactFilters)
                {
                    // Keep the merged search field visibly useful in the real
                    // Playnite host.  The host can measure the filter Grid with
                    // a smaller desired width than the available viewport; a
                    // responsive floor prevents the field from collapsing back
                    // to the old label-plus-sliver appearance.
                    TaskSearchBoxHost.MinWidth = 420;
                    TaskStatusFilterComboBox.Width = 140;
                    TaskTypeFilterComboBox.Width = 140;
                    TaskHistoryScopeComboBox.Width = 120;
                    TaskHistoryRangeComboBox.Width = 120;
                    TaskGameFilterComboBox.Width = 180;
                }
                else if (width >= 760)
                {
                    TaskSearchBoxHost.MinWidth = 300;
                    TaskStatusFilterComboBox.Width = double.NaN;
                    TaskTypeFilterComboBox.Width = double.NaN;
                    TaskHistoryScopeComboBox.Width = double.NaN;
                    TaskHistoryRangeComboBox.Width = double.NaN;
                    TaskGameFilterComboBox.Width = double.NaN;
                }
                else
                {
                    TaskSearchBoxHost.MinWidth = 180;
                    TaskStatusFilterComboBox.Width = double.NaN;
                    TaskTypeFilterComboBox.Width = double.NaN;
                    TaskHistoryScopeComboBox.Width = double.NaN;
                    TaskHistoryRangeComboBox.Width = double.NaN;
                    TaskGameFilterComboBox.Width = double.NaN;
                }

                // Match the demo's task workspace: the queue remains the primary
                // surface and the selected task is a right-side inspector. On compact
                // hosts the inspector is a drawer behind the compact details button.
                if (stack)
                {
                    var hasTaskSelection = TaskGrid.SelectedItem != null;
                    if (hasTaskSelection)
                    {
                        TaskDetailScrollViewer.Visibility = taskInspectorOpen
                            ? Visibility.Visible
                            : Visibility.Collapsed;
                        TaskCompactDetailsButton.Content = taskInspectorOpen
                            ? "收起任务详情 ›"
                            : "查看任务详情 ›";
                        TaskCompactDetailsButton.Visibility = taskInspectorOpen
                            ? Visibility.Collapsed
                            : Visibility.Visible;
                        TaskCompactCloseDetailsButton.Visibility = taskInspectorOpen
                            ? Visibility.Visible
                            : Visibility.Collapsed;
                    }
                    else
                    {
                        TaskCompactDetailsButton.Visibility = Visibility.Collapsed;
                        TaskCompactCloseDetailsButton.Visibility = Visibility.Collapsed;
                        taskInspectorOpen = false;
                    }
                }
                else
                {
                    TaskCompactDetailsButton.Visibility = Visibility.Collapsed;
                    TaskCompactCloseDetailsButton.Visibility = Visibility.Collapsed;
                    TaskDetailScrollViewer.Visibility = TaskGrid.SelectedItem != null
                        ? Visibility.Visible
                        : Visibility.Collapsed;
                }
                var showInspector = TaskDetailScrollViewer.Visibility == Visibility.Visible;
                var inspectorWidth = TaskWorkspaceLayout.TryFindResource("GscInspectorWidth") is GridLength gl ? gl : new GridLength(360);
                TaskWorkspaceLayout.ColumnDefinitions[1].Width = showInspector && !stack ? new GridLength(14) : new GridLength(0);
                TaskWorkspaceLayout.ColumnDefinitions[2].Width = showInspector && !stack ? inspectorWidth : new GridLength(0);
                TaskWorkspaceLayout.RowDefinitions[4].Height = showInspector && stack
                    ? new GridLength(1, GridUnitType.Auto)
                    : new GridLength(0);
                Grid.SetColumn(TaskDetailScrollViewer, stack ? 0 : 2);
                Grid.SetColumnSpan(TaskDetailScrollViewer, stack ? 3 : 1);
                Grid.SetRow(TaskDetailScrollViewer, stack ? 4 : 3);
                // A small bottom inset avoids the 0.8-DIP PageHost overflow produced
                // by the host's fractional layout rounding at the compact breakpoint.
                TaskDetailScrollViewer.Margin = showInspector && stack ? new Thickness(0, 10, 0, 2) : new Thickness(0);
                // Keep the inspector finite beside or below the queue. The page viewer
                // owns any remaining vertical overflow, while the task grid stays in its
                // own bounded table frame.
                var wideInspectorMaxHeight = Math.Max(160d, Math.Min(420d, availableHeight * 0.55d));
                TaskDetailScrollViewer.MaxHeight = showInspector
                    ? (stack ? compactInspectorMaxHeight : wideInspectorMaxHeight)
                    : double.PositiveInfinity;
                // Keep only the virtualized queue finite. The page ScrollViewer measures
                // the full summary, queue, and inspector naturally; assigning a cached
                // height to their shared root could clip those later sections.
                var queueChromeHeight = WorkspaceTableViewportPolicy.MeasureChromeHeight(
                    TaskQueuePanel.ActualHeight,
                    TaskGrid.ActualHeight,
                    fallbackHeight: 148d);
                var pageChromeHeight = TaskWorkspaceLayout.RowDefinitions[0].ActualHeight
                    + TaskWorkspaceLayout.RowDefinitions[1].ActualHeight
                    + TaskWorkspaceLayout.RowDefinitions[2].ActualHeight
                    + queueChromeHeight;
                var stackedInspectorHeight = showInspector && stack
                    ? compactInspectorMaxHeight + TaskDetailScrollViewer.Margin.Top + TaskDetailScrollViewer.Margin.Bottom
                    : 0;
                var tableBudget = WorkspaceTableViewportPolicy.CalculateTableHeight(
                    availableHeight,
                    tableMinHeight,
                    pageChromeHeight,
                    stackedInspectorHeight);
                var tableViewportHeight = Math.Max(tableMinHeight, Math.Min(620d, tableBudget));
                TaskGrid.Height = tableViewportHeight;
                TaskGrid.MaxHeight = tableViewportHeight;
            }
            finally
            {
                isApplyingLayout = false;
            }
        }

        private void OnTaskSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            taskInspectorOpen = false;
            if (IsLoaded)
                ApplyResponsiveLayout(
                    TaskPageScrollSurface.ActualWidth > 0 ? TaskPageScrollSurface.ActualWidth : TaskWorkspaceLayout.ActualWidth,
                    lastRequestedHeight > 0 ? lastRequestedHeight : responsiveHeight > 0 ? responsiveHeight : TaskWorkspaceLayout.ActualHeight);
        }

        private void OnClearTaskFiltersClick(object sender, RoutedEventArgs e)
        {
            // The command performs the reset. Mark the click handled so the button
            // inside the disclosure header never toggles the Expander as a side effect.
            e.Handled = true;
        }

        private void SetCompactFilterPlacement(bool compact)
        {
            var alreadyCompact = ReferenceEquals(TaskSummaryPanel.Parent, TaskMoreFiltersHost);
            // These live controls inherit their DataContext from different parents
            // before and after the move. Snapshot their selections first so the
            // transient null inherited context cannot push the style's first item
            // back into the task filter state through the TwoWay bindings.
            var typeSelection = TaskTypeFilterComboBox.SelectedItem;
            var scopeSelection = TaskHistoryScopeComboBox.SelectedItem;
            var rangeSelection = TaskHistoryRangeComboBox.SelectedItem;
            var gameSelection = TaskGameFilterComboBox.SelectedItem;

            if (compact == alreadyCompact)
            {
                // The XAML starts with the overview in the main workspace, so the
                // first wide-layout pass still has to move the game filter out of
                // the collapsed disclosure and into the visible toolbar.
                if (!compact && !ReferenceEquals(TaskGameFilterGroup.Parent, TaskFiltersPanel))
                {
                    RemoveChild(TaskMoreFiltersHost, TaskGameFilterGroup);
                    AddChild(TaskFiltersPanel, TaskGameFilterGroup);
                    Grid.SetColumn(TaskGameFilterGroup, 5);
                    Grid.SetColumn(TaskRefreshButton, 6);
                    SetCompactFilterGroupSpacing(bottom: 0d);
                    RestoreFilterSelection(TaskGameFilterComboBox, gameSelection);
                }
                return;
            }

            if (compact)
            {
                RemoveChild(TaskFiltersPanel, TaskGameFilterGroup);
                RemoveChild(TaskFiltersPanel, TaskTypeFilterGroup);
                RemoveChild(TaskFiltersPanel, TaskHistoryScopeFilterGroup);
                RemoveChild(TaskFiltersPanel, TaskHistoryRangeFilterGroup);
                RemoveChild(TaskFiltersPanel, TaskFilterPresetRow);
                RemoveChild(TaskFiltersPanel, TaskViewConfigurationRow);
                RemoveChild(TaskFiltersPanel, TaskNavigationSourceBanner);
                RemoveChild(TaskFiltersPanel, TaskLeavePageHint);
                RemoveChild(TaskWorkspaceLayout, TaskSummaryPanel);

                TaskMoreFiltersHost.Children.Clear();
                TaskMoreFiltersHost.Children.Add(TaskSummaryPanel);
                TaskMoreFiltersHost.Children.Add(TaskTypeFilterGroup);
                TaskMoreFiltersHost.Children.Add(TaskHistoryScopeFilterGroup);
                TaskMoreFiltersHost.Children.Add(TaskHistoryRangeFilterGroup);
                TaskMoreFiltersHost.Children.Add(TaskGameFilterGroup);
                TaskMoreFiltersHost.Children.Add(TaskFilterPresetRow);
                TaskMoreFiltersHost.Children.Add(TaskViewConfigurationRow);
                TaskMoreFiltersHost.Children.Add(TaskNavigationSourceBanner);
                TaskMoreFiltersHost.Children.Add(TaskLeavePageHint);
                TaskSummaryPanel.Margin = new Thickness(0, 0, 0, 8);
                SetCompactFilterGroupSpacing(bottom: 12d);
                RestoreFilterSelection(TaskTypeFilterComboBox, typeSelection);
                RestoreFilterSelection(TaskHistoryScopeComboBox, scopeSelection);
                RestoreFilterSelection(TaskHistoryRangeComboBox, rangeSelection);
                RestoreFilterSelection(TaskGameFilterComboBox, gameSelection);
                return;
            }

            RemoveChild(TaskMoreFiltersHost, TaskTypeFilterGroup);
            RemoveChild(TaskMoreFiltersHost, TaskHistoryScopeFilterGroup);
            RemoveChild(TaskMoreFiltersHost, TaskHistoryRangeFilterGroup);
            TaskMoreFiltersHost.Children.Clear();
            AddChild(TaskWorkspaceLayout, TaskSummaryPanel);
            AddChild(TaskFiltersPanel, TaskTypeFilterGroup);
            AddChild(TaskFiltersPanel, TaskHistoryScopeFilterGroup);
            AddChild(TaskFiltersPanel, TaskHistoryRangeFilterGroup);
            AddChild(TaskFiltersPanel, TaskGameFilterGroup);
            AddChild(TaskFiltersPanel, TaskFilterPresetRow);
            AddChild(TaskFiltersPanel, TaskViewConfigurationRow);
            AddChild(TaskFiltersPanel, TaskNavigationSourceBanner);
            AddChild(TaskFiltersPanel, TaskLeavePageHint);
            Grid.SetColumn(TaskGameFilterGroup, 5);
            Grid.SetColumn(TaskRefreshButton, 6);
            TaskSummaryPanel.Margin = new Thickness(0, 0, 0, 8);

            SetCompactFilterGroupSpacing(bottom: 0d);
            RestoreFilterSelection(TaskTypeFilterComboBox, typeSelection);
            RestoreFilterSelection(TaskHistoryScopeComboBox, scopeSelection);
            RestoreFilterSelection(TaskHistoryRangeComboBox, rangeSelection);
            RestoreFilterSelection(TaskGameFilterComboBox, gameSelection);
        }

        private void SetCompactFilterGroupSpacing(double bottom)
        {
            var margin = new Thickness(0, 0, 10, bottom);
            TaskTypeFilterGroup.Margin = margin;
            TaskHistoryScopeFilterGroup.Margin = margin;
            TaskHistoryRangeFilterGroup.Margin = margin;
            // Compact groups get a shared row gap; the wide filter Grid does not
            // carry disclosure spacing beneath the game selector.
            TaskGameFilterGroup.Margin = new Thickness(0, 0, 10, bottom);
        }

        private static void RestoreFilterSelection(ComboBox comboBox, object? selectedItem)
        {
            if (selectedItem == null || comboBox.Items.Contains(selectedItem))
                comboBox.SetCurrentValue(ComboBox.SelectedItemProperty, selectedItem);
        }

        private static void RemoveChild(Panel parent, UIElement child)
        {
            if (parent.Children.Contains(child)) parent.Children.Remove(child);
        }

        private static void AddChild(Panel parent, UIElement child)
        {
            if (!parent.Children.Contains(child)) parent.Children.Add(child);
        }

        private void OnTaskCompactDetailsClick(object sender, RoutedEventArgs e)
        {
            if (TaskGrid.SelectedItem == null) return;
            taskInspectorOpen = !taskInspectorOpen;
            ApplyResponsiveLayout(
                TaskPageScrollSurface.ActualWidth > 0 ? TaskPageScrollSurface.ActualWidth : ActualWidth,
                lastRequestedHeight > 0 ? lastRequestedHeight : responsiveHeight > 0 ? responsiveHeight : ActualHeight);
            FocusElement(taskInspectorOpen ? TaskDetailScrollViewer : TaskCompactDetailsButton);
        }

        private void OnTaskDetailPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape || !taskInspectorOpen)
                return;

            taskInspectorOpen = false;
            ApplyResponsiveLayout(
                TaskPageScrollSurface.ActualWidth > 0 ? TaskPageScrollSurface.ActualWidth : ActualWidth,
                lastRequestedHeight > 0 ? lastRequestedHeight : responsiveHeight > 0 ? responsiveHeight : ActualHeight);
            FocusElement(TaskCompactDetailsButton);
            e.Handled = true;
        }

        private async void OnWorklistExportCopyClick(object sender, RoutedEventArgs e)
        {
            var target = sender as FrameworkElement ?? WorklistExportCopyButton;
            await WorklistExportClipboard.CopyAsync(WorklistExportPreviewTextBox.Text, target);
            e.Handled = true;
        }

        private void OnWorklistExportPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape || !(DataContext is DashboardViewModel viewModel)
                || !viewModel.IsWorklistExportPreviewOpen)
                return;

            viewModel.CloseWorklistExportPreviewCommand.Execute(null);
            FocusElement(WorklistExportPreviewButton);
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
