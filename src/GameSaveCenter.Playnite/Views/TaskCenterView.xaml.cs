using System;
using System.Windows;
using System.Windows.Controls;

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
                var stack = width < 1200;
                var filterWidth = TaskQueuePanel.ActualWidth > 0
                    ? Math.Max(0, TaskQueuePanel.ActualWidth - TaskQueuePanel.Padding.Left - TaskQueuePanel.Padding.Right)
                    : Math.Max(0, width - 20);
                TaskFiltersPanel.Width = filterWidth;
                TaskPrimaryFiltersRow.Width = filterWidth;
                TaskGameFilterHost.Width = filterWidth;
                TaskGrid.MinHeight = tableMinHeight;
                TaskGrid.Height = double.NaN;
                TaskGrid.MaxHeight = double.PositiveInfinity;
                // Keep the single Demo-aligned statistics band available at every height;
                // the table and inspector own their finite scroll surfaces instead of
                // scrolling the whole workspace. Its four columns wrap their labels on
                // compact panes without introducing a second card row.
                TaskSummaryPanel.Visibility = Visibility.Visible;
                // The action row stays horizontal on all common compact widths; only a
                // genuinely narrow pane stacks the three commands vertically.
                TaskDetailActions.Orientation = width < 520 ? Orientation.Vertical : Orientation.Horizontal;

                // Responsive move: the game filter is a secondary filter on compact panes.
                // It lives in "更多筛选" there, and moves back to the primary row on wide hosts.
                var moveGameFilter = width < 760;
                if (moveGameFilter)
                {
                    if (!TaskMoreFiltersHost.Children.Contains(TaskGameFilterHost))
                    {
                        TaskFiltersPanel.Children.Remove(TaskGameFilterHost);
                        TaskMoreFiltersHost.Children.Add(TaskGameFilterHost);
                    }
                    TaskMoreFiltersExpander.Visibility = Visibility.Visible;
                }
                else
                {
                    if (!TaskFiltersPanel.Children.Contains(TaskGameFilterHost))
                    {
                        TaskMoreFiltersHost.Children.Remove(TaskGameFilterHost);
                        TaskFiltersPanel.Children.Add(TaskGameFilterHost);
                    }
                    TaskMoreFiltersExpander.Visibility = Visibility.Collapsed;
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
                TaskWorkspaceLayout.RowDefinitions[3].Height = showInspector && stack
                    ? new GridLength(1, GridUnitType.Auto)
                    : new GridLength(0);
                Grid.SetColumn(TaskDetailScrollViewer, stack ? 0 : 2);
                Grid.SetColumnSpan(TaskDetailScrollViewer, stack ? 3 : 1);
                Grid.SetRow(TaskDetailScrollViewer, stack ? 3 : 2);
                TaskDetailScrollViewer.Margin = showInspector && stack ? new Thickness(0, 10, 0, 0) : new Thickness(0);
                var viewportHeight = TaskPageScrollSurface.ActualHeight > 0
                    ? TaskPageScrollSurface.ActualHeight
                    : Math.Max(320, height);
                var workspaceHeight = viewportHeight > 0
                    ? viewportHeight
                        - TaskSummaryPanel.ActualHeight
                        - TaskQueuePanel.ActualHeight
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
