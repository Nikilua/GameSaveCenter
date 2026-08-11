using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace GameSaveCenter.Playnite.Views
{
    /// <summary>Physical task-center workspace; it deliberately has no current-game picker.</summary>
    public partial class TaskCenterView : UserControl
    {
        public TaskCenterView()
        {
            InitializeComponent();
            TaskDetailScrollViewer.IsVisibleChanged += OnTaskDetailScrollViewerIsVisibleChanged;
        }

        private void OnTaskDetailScrollViewerIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsLoaded) return;
            ApplyResponsiveLayout(TaskWorkspaceLayout.ActualWidth, TaskWorkspaceLayout.ActualHeight);
        }

        public UniformGrid TaskSummaryPanelElement => TaskSummaryPanel;
        public Border TaskDetailCardElement => TaskDetailCard;
        public ScrollViewer TaskDetailScrollViewerElement => TaskDetailScrollViewer;

        public void ApplyResponsiveLayout(double width, double height)
        {
            // The queue is the primary Demo-aligned surface.  When the detail
            // inspector stacks below it, reserve a readable table viewport and
            // let only the inspector consume the remaining finite height.
            const double tableMinHeight = 236d;
            TaskGrid.MinHeight = tableMinHeight;
            TaskSummaryPanel.Columns = width >= 1120 ? 4 : width >= 760 ? 2 : 1;
            // Keep task summary metrics available at every height; the table and inspector
            // own their finite scroll surfaces instead of scrolling the whole workspace.
            TaskSummaryPanel.Visibility = Visibility.Visible;
            TaskDetailActions.Orientation = width < 760 ? Orientation.Vertical : Orientation.Horizontal;
            // Match the demo's task workspace: the queue remains the primary
            // surface and the selected task is a right-side inspector. At
            // compact widths the inspector drops below the table instead of
            // compressing the task columns into an unreadable strip.
            var stack = width < 1080;
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
            var workspaceHeight = TaskWorkspaceLayout.ActualHeight > 0
                ? TaskWorkspaceLayout.ActualHeight
                    - TaskSummaryPanel.ActualHeight
                    - TaskQueuePanel.ActualHeight
                : Math.Max(320, height - 200);
            var inspectorHeight = Math.Max(96, Math.Min(420, workspaceHeight - tableMinHeight - 10));
            TaskDetailScrollViewer.MaxHeight = showInspector && stack
                ? inspectorHeight
                : double.PositiveInfinity;
        }
    }
}
