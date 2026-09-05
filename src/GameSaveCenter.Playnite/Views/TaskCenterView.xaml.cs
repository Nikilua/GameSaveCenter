using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace GameSaveCenter.Playnite.Views
{
    /// <summary>Physical task-center workspace; it deliberately has no current-game picker.</summary>
    public partial class TaskCenterView : UserControl
    {
        private bool isApplyingLayout;
        private bool taskInspectorOpen;

        public TaskCenterView()
        {
            InitializeComponent();
            TaskDetailScrollViewer.IsVisibleChanged += OnTaskDetailScrollViewerIsVisibleChanged;
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

        private void OnTaskDetailScrollViewerIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (isApplyingLayout || !IsLoaded) return;
            ApplyResponsiveLayout(
                TaskPageScrollSurface.ActualWidth > 0 ? TaskPageScrollSurface.ActualWidth : TaskWorkspaceLayout.ActualWidth,
                TaskPageScrollSurface.ActualHeight > 0 ? TaskPageScrollSurface.ActualHeight : TaskWorkspaceLayout.ActualHeight);
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
                // The queue is the primary Demo-aligned surface.  When the detail
                // inspector stacks below it, reserve a readable table viewport and
                // let only the inspector consume the remaining finite height.
                const double tableMinHeight = 236d;
                var stack = width < 980;
                TaskGrid.MinHeight = tableMinHeight;
                TaskGrid.Height = double.NaN;
                TaskGrid.MaxHeight = double.PositiveInfinity;
                // Keep the Demo's four metrics in one continuous strip at every width. On a
                // short window, tighten only the secondary summary chrome so the queue still
                // gets a useful first viewport; the table and inspector keep their own scroll
                // surfaces instead of scrolling the whole workspace.
                var shortHeight = height > 0 && height < 700;
                TaskSummaryPanel.Visibility = Visibility.Visible;
                TaskSummaryPanel.MinHeight = shortHeight ? 64 : 84;
                TaskSummaryPanel.Padding = shortHeight ? new Thickness(6, 8, 6, 8) : new Thickness(6, 14, 6, 14);
                // The action row stays horizontal on all common compact widths; only a
                // genuinely narrow pane stacks the three commands vertically.
                TaskDetailActions.Orientation = width < 520 ? Orientation.Vertical : Orientation.Horizontal;

                // The primary toolbar is a finite Grid rather than a WrapPanel. The
                // game selector is intentionally kept in the optional disclosure so
                // the primary controls never reflow into each other.
                var compactFilters = width < 760;
                TaskMoreFiltersExpander.Visibility = compactFilters ? Visibility.Visible : Visibility.Collapsed;
                TaskFiltersPanel.RowDefinitions[1].Height = compactFilters
                    ? new GridLength(1, GridUnitType.Auto)
                    : new GridLength(0);
                Grid.SetRow(TaskSearchBoxHost, 0);
                Grid.SetColumn(TaskSearchBoxHost, 0);
                Grid.SetRow(TaskStatusFilterLabel, compactFilters ? 1 : 0);
                Grid.SetColumn(TaskStatusFilterLabel, compactFilters ? 0 : 1);
                Grid.SetRow(TaskStatusFilterComboBox, compactFilters ? 1 : 0);
                Grid.SetColumn(TaskStatusFilterComboBox, compactFilters ? 1 : 2);
                Grid.SetRow(TaskTypeFilterLabel, compactFilters ? 1 : 0);
                Grid.SetColumn(TaskTypeFilterLabel, compactFilters ? 2 : 3);
                Grid.SetRow(TaskTypeFilterComboBox, compactFilters ? 1 : 0);
                Grid.SetColumn(TaskTypeFilterComboBox, compactFilters ? 3 : 4);
                Grid.SetRow(TaskHistoryScopeComboBox, compactFilters ? 1 : 0);
                Grid.SetColumn(TaskHistoryScopeComboBox, compactFilters ? 4 : 5);
                Grid.SetRow(TaskHistoryRangeComboBox, compactFilters ? 1 : 0);
                Grid.SetColumn(TaskHistoryRangeComboBox, compactFilters ? 5 : 6);
                Grid.SetRow(TaskRefreshButton, compactFilters ? 1 : 0);
                Grid.SetColumn(TaskRefreshButton, compactFilters ? 6 : 7);

                Grid.SetColumnSpan(TaskSearchBoxHost, compactFilters ? 7 : 1);
                Grid.SetRow(TaskSearchBoxHost, 0);

                // Give the common desktop width a stable rhythm like the Demo's
                // search/status/type toolbar.  The old WrapPanel relied on each
                // control's theme default width, which made the search box collapse
                // to a sliver in the real Playnite host even though no elements
                // technically overlapped.
                if (width >= 980)
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
                        TaskCompactDetailsButton.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        TaskCompactDetailsButton.Visibility = Visibility.Collapsed;
                        taskInspectorOpen = false;
                    }
                }
                else
                {
                    TaskCompactDetailsButton.Visibility = Visibility.Collapsed;
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
                TaskDetailScrollViewer.Margin = showInspector && stack ? new Thickness(0, 10, 0, 0) : new Thickness(0);
                var viewportHeight = TaskPageScrollSurface.ActualHeight > 0
                    ? TaskPageScrollSurface.ActualHeight
                    : Math.Max(320, height);
                var workspaceHeight = viewportHeight > 0
                    ? viewportHeight
                        - TaskSummaryPanel.ActualHeight
                        - TaskFilterBar.ActualHeight
                        - TaskMoreFiltersExpander.ActualHeight
                    : Math.Max(320, height - 200);
                // A 96-DIP strip below the queue was too small to read task details at the
                // demo-minimum and common 1366-DIP windows. Keep the finite cap so the
                // inspector owns its own scroll, but give stacked mode a readable floor.
                var inspectorHeight = Math.Max(160, Math.Min(420, workspaceHeight - tableMinHeight - 10));
                TaskDetailScrollViewer.MaxHeight = showInspector && stack
                    ? inspectorHeight
                    : double.PositiveInfinity;
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
                    TaskPageScrollSurface.ActualHeight > 0 ? TaskPageScrollSurface.ActualHeight : TaskWorkspaceLayout.ActualHeight);
        }

        private void OnTaskCompactDetailsClick(object sender, RoutedEventArgs e)
        {
            if (TaskGrid.SelectedItem == null) return;
            taskInspectorOpen = !taskInspectorOpen;
            ApplyResponsiveLayout(
                TaskPageScrollSurface.ActualWidth > 0 ? TaskPageScrollSurface.ActualWidth : ActualWidth,
                TaskPageScrollSurface.ActualHeight > 0 ? TaskPageScrollSurface.ActualHeight : ActualHeight);
        }
    }
}
