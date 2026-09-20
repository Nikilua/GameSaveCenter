using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Controls;
using GameSaveCenter.Playnite.Views;
using Xunit;
using WpfButton = System.Windows.Controls.Button;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R07StatusBannerBudgetBehaviorTests
{
    [Fact]
    public void TaskFailureWithRetainedRowsKeepsRetryBannerAndTableViewport()
    {
        RunSta(() =>
        {
            var state = new TaskBannerContext
            {
                TaskPageLoadFailed = true,
                TaskPageErrorMessage = "Worker 读取任务记录失败；保留旧数据供只读检查。"
            };
            state.Tasks.Add(new TaskStatusDto
            {
                TaskId = "r07-banner-task",
                TaskType = "Validation",
                GameName = "横幅预算测试",
                State = TaskState.Failed,
                ErrorCode = "R07-BANNER",
                ErrorMessage = "旧任务记录仍可查看。"
            });

            var view = new TaskCenterView { DataContext = state };
            var window = Show(view, 1000, 700);
            try
            {
                view.ApplyResponsiveLayout(1000, 700);
                PumpLayout(window);

                var banner = (Border)view.FindName("TaskStaleDataBanner")!;
                var grid = (DataGrid)view.FindName("TaskGrid")!;
                var queue = (Border)view.FindName("TaskQueuePanel")!;
                var retry = FindVisualChildren<WpfButton>(banner).Single(button => Equals(button.Content, "重试"));

                Assert.Equal(Visibility.Visible, banner.Visibility);
                Assert.Same(state.RefreshCommand, retry.Command);
                Assert.True(banner.ActualHeight > 0);
                Assert.True(grid.MinHeight >= 236);
                Assert.True(grid.ActualHeight >= grid.MinHeight - 1);
                Assert.True(queue.ActualHeight >= banner.ActualHeight + grid.MinHeight);

                state.IsTaskPageLoading = true;
                state.TaskPageLoadFailed = false;
                state.NotifyState();
                PumpLayout(window);
                Assert.Equal(Visibility.Visible, banner.Visibility);
                Assert.True(grid.ActualHeight >= grid.MinHeight - 1);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void EmptyTaskFailureKeepsTheFailurePresenterReachable()
    {
        RunSta(() =>
        {
            var state = new TaskBannerContext
            {
                TaskPageLoadFailed = true,
                TaskPageErrorMessage = "Worker 没有返回任务列表。"
            };
            var view = new TaskCenterView { DataContext = state };
            var window = Show(view, 1000, 700);
            try
            {
                view.ApplyResponsiveLayout(1000, 700);
                PumpLayout(window);

                var banner = (Border)view.FindName("TaskStaleDataBanner")!;
                var errorPresenter = FindVisualChildren<WorkspaceStatePresenter>(view)
                    .Single(presenter => presenter.State == "Error");

                Assert.Equal(Visibility.Collapsed, banner.Visibility);
                Assert.Equal(Visibility.Visible, errorPresenter.Visibility);
                Assert.Same(state.RefreshCommand, errorPresenter.RetryCommand);
                Assert.Contains("任务记录暂时无法读取", errorPresenter.Message);
                Assert.DoesNotContain("Worker", errorPresenter.Message);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void SaveStaleBannerKeepsRetryAndHistoryTableFloor()
    {
        RunSta(() =>
        {
            var state = new SaveBannerContext();
            state.Backups.Add(new BackupVersionDto { BackupId = "r07-save", FileCount = 1 });
            var view = new SaveCenterView { DataContext = state };
            var window = Show(view, 1040, 700);
            try
            {
                view.ApplyResponsiveLayout(1040, 700);
                PumpLayout(window);

                var banner = (Border)view.FindName("SaveDetailsStaleBanner")!;
                var grid = (DataGrid)view.FindName("SaveHistoryGrid")!;
                var retry = FindVisualChildren<WpfButton>(banner).Single(button => Equals(button.Content, "重试"));

                Assert.Equal(Visibility.Visible, banner.Visibility);
                Assert.Same(state.LoadDetailsCommand, retry.Command);
                Assert.True(grid.MinHeight >= 236);
                Assert.True(grid.ActualHeight >= grid.MinHeight - 1);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void MaintenanceStaleBannerAndSafeModeKeepExplicitRecoveryActions()
    {
        RunSta(() =>
        {
            var state = new MaintenanceBannerContext();
            state.Findings.Add(new ValidationFindingDto
            {
                PlayniteId = "r07-maintenance",
                GameName = "维护预算测试",
                Severity = FindingSeverity.Warning,
                Code = "R07-MAINTENANCE",
                Title = "保留旧诊断",
                Detail = "本次刷新失败，保留上次诊断供只读查看。"
            });
            state.SelectedFinding = state.Findings[0];

            var view = new MaintenanceView { DataContext = state };
            var window = Show(view, 1040, 700);
            try
            {
                var tabs = (TabControl)view.FindName("MaintenanceTabControl")!;
                tabs.SelectedIndex = 4;
                view.ApplyResponsiveLayout(1040, 700);
                PumpLayout(window);

                var banner = (Border)view.FindName("MaintenanceStaleBanner")!;
                var grid = (DataGrid)view.FindName("MaintenanceAuditFindingsGrid")!;
                var retry = FindVisualChildren<WpfButton>(banner).Single(button => Equals(button.Content, "重试"));
                Assert.Equal(Visibility.Visible, banner.Visibility);
                Assert.Same(state.RefreshDiagnosticsCommand, retry.Command);
                Assert.True(grid.MinHeight >= 260);
                Assert.True(grid.ActualHeight >= grid.MinHeight - 1);

                tabs.SelectedIndex = 0;
                var subTabs = (TabControl)view.FindName("MaintenanceDiagnosticsSubTabs")!;
                subTabs.SelectedIndex = 1;
                var moreActions = (Expander)view.FindName("MaintenanceActionsDisclosure")!;
                moreActions.IsExpanded = true;
                PumpLayout(window);
                var safeModeText = FindVisualChildren<TextBlock>(view)
                    .Single(text => text.Text != null && text.Text.IndexOf("安全模式已开启", StringComparison.Ordinal) >= 0);
                var safeModeBorder = FindVisualAncestor<Border>(safeModeText);
                var exit = FindVisualChildren<WpfButton>(safeModeBorder!).Single(button => Equals(button.Content, "恢复正常模式"));
                Assert.Equal(Visibility.Visible, safeModeBorder!.Visibility);
                Assert.Same(state.ExitSafeModeCommand, exit.Command);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static Window Show(UserControl view, double width, double height)
    {
        var window = new Window
        {
            Width = width,
            Height = height,
            Content = view,
            ShowInTaskbar = false,
            ShowActivated = false,
            WindowStyle = WindowStyle.None,
            Opacity = 0.01
        };
        window.Show();
        return window;
    }

    private static void RunSta(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception caught) { exception = caught; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        Assert.Null(exception);
    }

    private static void PumpLayout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
        window.UpdateLayout();
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
        where T : DependencyObject
    {
        if (root == null) yield break;
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match) yield return match;
            foreach (var nested in FindVisualChildren<T>(child)) yield return nested;
        }
    }

    private static T? FindVisualAncestor<T>(DependencyObject node)
        where T : DependencyObject
    {
        for (var current = VisualTreeHelper.GetParent(node); current != null; current = VisualTreeHelper.GetParent(current))
            if (current is T match) return match;
        return null;
    }

    private sealed class TaskBannerContext : INotifyPropertyChanged
    {
        public ObservableCollection<TaskStatusDto> Tasks { get; } = new ObservableCollection<TaskStatusDto>();
        public ICollectionView TasksView { get; }
        public TaskStatusDto? SelectedTask { get; set; }
        public bool IsTaskPageLoading { get; set; }
        public bool TaskPageLoadFailed { get; set; }
        public string TaskPageErrorMessage { get; set; } = string.Empty;
        public bool TaskPageHasItems => Tasks.Count > 0;
        public string TaskPageState => TaskPageLoadFailed ? (TaskPageHasItems ? "ErrorWithData" : "Error") : "Ready";
        public string TaskPageStatusSummary => TaskPageLoadFailed
            ? (TaskPageHasItems ? "读取失败，已保留旧数据。" : "读取任务失败，请重试。")
            : IsTaskPageLoading ? "正在刷新，已保留旧数据。" : "最近更新：合成时间";
        public ICommand RefreshCommand { get; } = new TestCommand();
        public int TaskTotalCount => Tasks.Count;
        public string TaskTotalCountLabel => "任务总数";
        public int RunningTaskCount => 0;
        public string TaskWaitingSummary => string.Empty;
        public int RetryableTaskCount => 0;
        public string TaskRetrySummary => string.Empty;
        public int CompletedTaskCount => 0;
        public string TaskLoadedSummary => "合成数据";
        public string TaskActiveFiltersSummary => string.Empty;
        public bool TaskHasActiveFilters => false;
        public bool TaskHistoryHasMore => false;
        public bool IsCancellingTask => false;

        public TaskBannerContext()
        {
            TasksView = new ListCollectionView((IList)Tasks);
            Tasks.CollectionChanged += (_, __) => NotifyState();
        }

        public void NotifyState()
        {
            foreach (var name in new[] { nameof(TaskPageHasItems), nameof(TaskPageState), nameof(TaskPageStatusSummary), nameof(TaskTotalCount) })
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private sealed class SaveBannerContext
    {
        public ObservableCollection<BackupVersionDto> Backups { get; } = new ObservableCollection<BackupVersionDto>();
        public ObservableCollection<SavePathCandidateDto> SaveCandidates { get; } = new ObservableCollection<SavePathCandidateDto>();
        public object? SelectedGame => null;
        public object? SelectedBackup { get; set; }
        public object? SelectedCandidate { get; set; }
        public ICommand LoadDetailsCommand { get; } = new TestCommand();
        public string SaveDetailsState => "Stale";
        public string SaveDetailsPresenterState => "Degraded";
        public string SaveDetailsStateTitle => "存档列表显示已过期";
        public string SaveDetailsStateMessage => "仍保留上次成功读取的内容。";
        public string SaveDetailsStateDetail => "本次刷新失败，保留上次成功读取的内容。";
        public bool SaveHistoryStateOverlayVisible => false;
        public bool SaveCandidateStateOverlayVisible => false;
        public bool SaveDetailsStaleVisible => true;
        public string SaveCandidateEmptyText => "暂无待处理候选。";
    }

    private sealed class MaintenanceBannerContext
    {
        public ObservableCollection<ValidationFindingDto> Findings { get; } = new ObservableCollection<ValidationFindingDto>();
        public ObservableCollection<object> Audit { get; } = new ObservableCollection<object>();
        public ValidationFindingDto? SelectedFinding { get; set; }
        public int MaintenanceTabIndex { get; set; } = 0;
        public string MaintenanceState => "Stale";
        public string MaintenancePresenterState => "Degraded";
        public string MaintenanceStateTitle => "维护信息显示已过期";
        public string MaintenanceStateMessage => "仍保留上次成功读取的诊断信息。";
        public string MaintenanceStateDetail => "本次刷新失败，保留上次成功读取的诊断信息。";
        public bool MaintenanceStateOverlayVisible => false;
        public bool MaintenanceStaleVisible => true;
        public bool IsWorkerOffline => false;
        public bool IsCloudDegraded => false;
        public WorkerSettingsSnapshotDto Snapshot { get; } = new WorkerSettingsSnapshotDto { SafeModeEnabled = true };
        public ICommand RefreshDiagnosticsCommand { get; } = new TestCommand();
        public ICommand ExitSafeModeCommand { get; } = new TestCommand();
    }

    private sealed class TestCommand : ICommand
    {
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) { }
        public event EventHandler? CanExecuteChanged { add { } remove { } }
    }
}
