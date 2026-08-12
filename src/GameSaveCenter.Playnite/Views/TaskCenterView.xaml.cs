using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.ViewModels;

namespace GameSaveCenter.Playnite.Views
{
    /// <summary>Physical task-center workspace; it deliberately has no current-game picker.</summary>
    public partial class TaskCenterView : UserControl
    {
        public TaskCenterView()
        {
            InitializeComponent();
            TaskDetailScrollViewer.IsVisibleChanged += OnTaskDetailScrollViewerIsVisibleChanged;
            DataContextChanged += OnDataContextChanged;
            Loaded += OnTaskCenterLoaded;
            Unloaded += OnTaskCenterUnloaded;
            filterRestoreTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            filterRestoreTimer.Tick += OnFilterRestoreTimerTick;
        }

        private DashboardViewModel? boundViewModel;
        private readonly DispatcherTimer filterRestoreTimer;
        private int filterRestorePasses;

        private void OnTaskCenterLoaded(object sender, RoutedEventArgs e)
        {
            StartFilterRestoreTimer();
            EnsureTaskFilterDefaults();
        }

        private void OnTaskCenterUnloaded(object sender, RoutedEventArgs e)
            => filterRestoreTimer.Stop();

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (boundViewModel != null)
            {
                boundViewModel.TaskGameFilterOptions.CollectionChanged -= OnTaskFilterOptionsChanged;
                boundViewModel.TaskTypeFilterOptions.CollectionChanged -= OnTaskFilterOptionsChanged;
                boundViewModel = null;
            }

            if (DataContext is DashboardViewModel viewModel)
            {
                boundViewModel = viewModel;
                viewModel.TaskGameFilterOptions.CollectionChanged += OnTaskFilterOptionsChanged;
                viewModel.TaskTypeFilterOptions.CollectionChanged += OnTaskFilterOptionsChanged;
                EnsureTaskFilterDefaults();
                StartFilterRestoreTimer();
            }
        }

        private void OnTaskFilterOptionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // The game/type option collections are rebuilt on Worker snapshots. Restore the
            // default selection after WPF has re-materialized the new items.
            StartFilterRestoreTimer();
            Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new Action(EnsureTaskFilterDefaults));
        }

        private void StartFilterRestoreTimer()
        {
            if (filterRestoreTimer.IsEnabled)
                return;
            filterRestorePasses = 0;
            filterRestoreTimer.Start();
        }

        private void OnFilterRestoreTimerTick(object? sender, EventArgs e)
        {
            filterRestorePasses++;
            EnsureTaskFilterDefaults();
            if (filterRestorePasses >= 25
                || (TaskStatusFilterComboBox.SelectedItem != null
                    && TaskGameFilterComboBox.SelectedItem != null
                    && TaskTypeFilterComboBox.SelectedItem != null))
            {
                filterRestoreTimer.Stop();
                filterRestorePasses = 0;
            }
        }

        private void EnsureTaskFilterDefaults()
        {
            if (DataContext is not DashboardViewModel viewModel)
                return;
            UiFilterSelection.RestoreDefault(TaskStatusFilterComboBox, viewModel.TaskStatusFilter);
            UiFilterSelection.RestoreDefault(TaskGameFilterComboBox, viewModel.TaskGameFilter);
            UiFilterSelection.RestoreDefault(TaskTypeFilterComboBox, viewModel.TaskTypeFilter);
        }

        private void OnTaskFilterSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // RebuildTaskFilters replaces the dynamic game/type collections. WPF clears
            // SelectedItem while the collection is repopulated, and that transient state
            // can win the binding race even though the ViewModel has already restored
            // “全部”. Restore only an actually empty selection, preserving real choices.
            if (sender is not ComboBox combo || combo.Items.Count == 0 || combo.SelectedIndex >= 0)
                return;

            Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new Action(() =>
            {
                if (combo.Items.Count > 0 && combo.SelectedIndex < 0)
                    combo.SelectedIndex = 0;
            }));
        }

        private void OnTaskDetailScrollViewerIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!IsLoaded) return;
            ApplyResponsiveLayout(
                TaskPageScrollSurface.ActualWidth > 0 ? TaskPageScrollSurface.ActualWidth : TaskWorkspaceLayout.ActualWidth,
                TaskPageScrollSurface.ActualHeight > 0 ? TaskPageScrollSurface.ActualHeight : TaskWorkspaceLayout.ActualHeight);
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
            var tableViewportHeight = Math.Max(tableMinHeight, Math.Min(460d, height * 0.50));
            TaskGrid.MinHeight = tableMinHeight;
            TaskGrid.Height = double.NaN;
            TaskGrid.MaxHeight = tableViewportHeight;
            // The 1040-DIP demo minimum leaves roughly 700 DIP for the workspace after
            // the labeled shell. Keep the summary cards in two columns there so they do
            // not consume the entire first viewport before the queue becomes reachable.
            TaskSummaryPanel.Columns = width >= 900 ? 4 : width >= 680 ? 2 : 1;
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
            var inspectorHeight = Math.Max(160, Math.Min(420, workspaceHeight - tableViewportHeight - 10));
            TaskDetailScrollViewer.MaxHeight = showInspector && stack
                ? inspectorHeight
                : double.PositiveInfinity;
        }
    }
}
