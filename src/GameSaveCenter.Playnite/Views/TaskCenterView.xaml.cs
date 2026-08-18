using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

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

        private void OnTaskDetailScrollViewerIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (isApplyingLayout || !IsLoaded) return;
            ApplyResponsiveLayout(
                TaskPageScrollSurface.ActualWidth > 0 ? TaskPageScrollSurface.ActualWidth : TaskWorkspaceLayout.ActualWidth,
                TaskPageScrollSurface.ActualHeight > 0 ? TaskPageScrollSurface.ActualHeight : TaskWorkspaceLayout.ActualHeight);
        }

        public UniformGrid TaskSummaryPanelElement => TaskSummaryPanel;
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
                // The 1040-DIP demo minimum leaves roughly 700 DIP for the workspace after
                // the labeled shell. Keep the summary cards in two columns there so they do
                // not consume the entire first viewport before the queue becomes reachable.
                TaskSummaryPanel.Columns = width >= 900 ? 4 : width >= 680 ? 2 : 1;
                // Compact panes use a single compact four-card strip so the queue and
                // table can stay inside the finite workspace without a page scrollbar.
                if (width >= 520 && width < 900)
                {
                    TaskSummaryPanel.Columns = 4;
                }
                // Keep task summary metrics available at every height; the table and inspector
                // own their finite scroll surfaces instead of scrolling the whole workspace.
                TaskSummaryPanel.Visibility = Visibility.Visible;
                // The action row stays horizontal on all common compact widths; only a
                // genuinely narrow pane stacks the three commands vertically.
                TaskDetailActions.Orientation = width < 520 ? Orientation.Vertical : Orientation.Horizontal;

                // The primary toolbar is a finite Grid rather than a WrapPanel. The
                // game selector is intentionally kept in the optional disclosure so
                // the four primary controls never reflow into each other.
                var compactFilters = width < 760;
                TaskMoreFiltersExpander.Visibility = compactFilters ? Visibility.Visible : Visibility.Collapsed;
                TaskFiltersPanel.RowDefinitions[1].Height = compactFilters
                    ? new GridLength(1, GridUnitType.Auto)
                    : new GridLength(0);
                Grid.SetRow(TaskSearchLabel, 0);
                Grid.SetColumn(TaskSearchLabel, 0);
                Grid.SetRow(TaskSearchTextBox, 0);
                Grid.SetColumn(TaskSearchTextBox, 1);
                Grid.SetRow(TaskStatusFilterLabel, compactFilters ? 1 : 0);
                Grid.SetColumn(TaskStatusFilterLabel, compactFilters ? 0 : 2);
                Grid.SetRow(TaskStatusFilterComboBox, compactFilters ? 1 : 0);
                Grid.SetColumn(TaskStatusFilterComboBox, compactFilters ? 1 : 3);
                Grid.SetRow(TaskTypeFilterLabel, compactFilters ? 1 : 0);
                Grid.SetColumn(TaskTypeFilterLabel, compactFilters ? 2 : 4);
                Grid.SetRow(TaskTypeFilterComboBox, compactFilters ? 1 : 0);
                Grid.SetColumn(TaskTypeFilterComboBox, compactFilters ? 3 : 5);
                Grid.SetRow(TaskRefreshButton, compactFilters ? 1 : 0);
                Grid.SetColumn(TaskRefreshButton, compactFilters ? 4 : 6);

                // Give the common desktop width a stable rhythm like the Demo's
                // search/status/type toolbar.  The old WrapPanel relied on each
                // control's theme default width, which made the search box collapse
                // to a sliver in the real Playnite host even though no elements
                // technically overlapped.
                if (width >= 980)
                {
                    TaskSearchTextBox.Width = width >= 1200 ? 260 : 220;
                    TaskStatusFilterComboBox.Width = 140;
                    TaskTypeFilterComboBox.Width = 140;
                    TaskGameFilterComboBox.Width = 180;
                }
                else
                {
                    TaskSearchTextBox.Width = double.NaN;
                    TaskStatusFilterComboBox.Width = double.NaN;
                    TaskTypeFilterComboBox.Width = double.NaN;
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
