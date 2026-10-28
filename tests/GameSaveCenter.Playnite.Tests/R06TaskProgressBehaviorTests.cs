using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R06TaskProgressBehaviorTests
{
    [Fact]
    public void ProductionProgressCellKeepsItsGeometryAcrossUnknownRunningAndTerminalStates()
    {
        RunSta(() =>
        {
            var tasks = new ObservableCollection<TaskStatusDto> { Task("progress-transition", TaskState.Running, -1) };
            var view = new TaskCenterView();
            var grid = (DataGrid)view.FindName("TaskGrid")!;
            grid.ItemsSource = tasks;
            var window = new Window { Content = view, Width = 1400, Height = 700, ShowInTaskbar = false };
            AutomationProperties.SetName(window, "Production task progress transition fixture");
            try
            {
                window.Show();
                view.ApplyResponsiveLayout(1400, 700);
                double? barWidth = null;
                double? labelRightEdge = null;
                foreach (var next in new[] {
                    Task("progress-transition", TaskState.Running, -1),
                    Task("progress-transition", TaskState.Running, 0),
                    Task("progress-transition", TaskState.Running, 73),
                    Task("progress-transition", TaskState.Cancelled, 73),
                    Task("progress-transition", TaskState.Succeeded, 100) })
                {
                    tasks[0] = next;
                    PumpLayout(window);
                    var cell = grid.Columns.Single(column => Equals(column.Header, "进度")).GetCellContent(next);
                    Assert.NotNull(cell);
                    var bar = FindChildren<ProgressBar>(cell!).Single();
                    var text = FindChildren<TextBlock>(cell!).Single();
                    var rightEdge = text.TransformToAncestor(cell!).Transform(new Point(text.ActualWidth, 0)).X;
                    Assert.Equal(next.ProgressDisplay, text.Text);
                    Assert.Equal(next.ProgressValue, bar.Value);
                    Assert.True(bar.ActualWidth > 0 && text.ActualWidth > 0);
                    if (barWidth.HasValue)
                    {
                        Assert.Equal(barWidth.Value, bar.ActualWidth, 2);
                        Assert.Equal(labelRightEdge!.Value, rightEdge, 2);
                    }
                    barWidth = bar.ActualWidth;
                    labelRightEdge = rightEdge;
                    if (next.State == TaskState.Cancelled)
                    {
                        Assert.Equal("已取消", next.StateDisplay);
                        Assert.Equal("73%", text.Text);
                    }
                }
            }
            finally { window.Close(); }
        });
    }

    [Fact]
    public void ProductionTaskRowKeepsStableHeightAcrossStageProgressAndLongTextUpdates()
    {
        RunSta(() =>
        {
            var initial = Task("row-stability", TaskState.Running, -1);
            initial.StageMessage = "正在准备";
            var tasks = new ObservableCollection<TaskStatusDto> { initial };
            var view = new TaskCenterView();
            var grid = (DataGrid)view.FindName("TaskGrid")!;
            grid.ItemsSource = tasks;
            var window = new Window { Content = view, Width = 1400, Height = 700, ShowInTaskbar = false };

            try
            {
                window.Show();
                view.ApplyResponsiveLayout(1400, 700);
                PumpLayout(window);

                double? stableHeight = null;
                var fullGameName = string.Concat(Enumerable.Repeat("超长中文游戏名称与 Long Game Name ", 8));
                var updates = new[]
                {
                    new TaskStatusDto
                    {
                        TaskId = "row-stability", TaskType = "Backup", GameName = fullGameName,
                        State = TaskState.Running, ProgressPercent = 35, StageMessage = "正在扫描存档",
                        CreatedUtc = initial.CreatedUtc
                    },
                    new TaskStatusDto
                    {
                        TaskId = "row-stability", TaskType = "Backup", GameName = fullGameName,
                        State = TaskState.Failed, ProgressPercent = 35, StageMessage = "正在扫描存档",
                        ErrorCode = "SYNTHETIC_FAILURE", ErrorMessage = new string('诊', 320),
                        CreatedUtc = initial.CreatedUtc
                    },
                    new TaskStatusDto
                    {
                        TaskId = "row-stability", TaskType = "Backup", GameName = fullGameName,
                        State = TaskState.Cancelled, ProgressPercent = 35, StageMessage = "正在扫描存档",
                        CreatedUtc = initial.CreatedUtc
                    },
                    new TaskStatusDto
                    {
                        TaskId = "row-stability", TaskType = "Backup", GameName = fullGameName,
                        State = TaskState.Succeeded, ProgressPercent = 100, StageMessage = "备份完成",
                        CreatedUtc = initial.CreatedUtc
                    }
                };

                foreach (var next in updates)
                {
                    tasks[0] = next;
                    PumpLayout(window);

                    var row = Assert.IsType<DataGridRow>(grid.ItemContainerGenerator.ContainerFromIndex(0));
                    Assert.True(row.ActualHeight > 0);
                    if (stableHeight.HasValue)
                        Assert.Equal(stableHeight.Value, row.ActualHeight, 2);
                    stableHeight = row.ActualHeight;

                    var stageColumn = grid.Columns.Single(column => Equals(column.Header, "阶段"));
                    var stageText = Assert.IsType<TextBlock>(stageColumn.GetCellContent(next));
                    Assert.Equal(next.StageDisplay, stageText.Text);
                    var progressColumn = grid.Columns.Single(column => Equals(column.Header, "进度"));
                    var progressCell = progressColumn.GetCellContent(next);
                    Assert.NotNull(progressCell);
                    var progressText = FindChildren<TextBlock>(progressCell!).Single();
                    Assert.Equal(next.ProgressDisplay, progressText.Text);

                    var gameColumn = grid.Columns.Single(column => Equals(column.Header, "游戏"));
                    var gameText = Assert.IsType<TextBlock>(gameColumn.GetCellContent(next));
                    BindingOperations.GetBindingExpression(gameText, AutomationProperties.HelpTextProperty)?.UpdateTarget();
                    Assert.Equal(fullGameName, AutomationProperties.GetHelpText(gameText));
                }
            }
            finally { window.Close(); }
        });
    }

    private static IEnumerable<T> FindChildren<T>(DependencyObject root) where T : DependencyObject
    {
        if (root is T match) yield return match;
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
            foreach (var child in FindChildren<T>(VisualTreeHelper.GetChild(root, index))) yield return child;
    }

    [Fact]
    public void ProgressReplacementKeepsSelectedTaskAndLogicalScrollAnchor()
    {
        RunSta(() =>
        {
            var tasks = new BatchObservableCollection<TaskStatusDto>(
                Enumerable.Range(0, 80).Select(index => Task("task-" + index, TaskState.Running, index % 80)));
            var taskIndex = new TaskIndexedCollection();
            taskIndex.Rebuild(tasks);
            var actions = new List<NotifyCollectionChangedAction>();
            tasks.CollectionChanged += (_, args) => actions.Add(args.Action);

            var grid = new DataGrid
            {
                AutoGenerateColumns = false,
                CanUserAddRows = false,
                EnableRowVirtualization = true,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                Height = 190,
                Width = 520,
                ItemsSource = tasks
            };
            grid.Columns.Add(new DataGridTextColumn { Header = "任务", Binding = new Binding("TaskId") });
            grid.Columns.Add(new DataGridTextColumn { Header = "进度", Binding = new Binding("ProgressDisplay") });
            var selection = new TaskSelectionState { SelectedTask = tasks[42] };
            grid.DataContext = selection;
            grid.SetBinding(DataGrid.SelectedItemProperty, new Binding(nameof(TaskSelectionState.SelectedTask))
            {
                Mode = BindingMode.TwoWay
            });

            var window = new Window
            {
                Width = 560,
                Height = 240,
                Content = grid,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None
            };

            try
            {
                window.Show();
                grid.SelectedItem = tasks[42];
                grid.ScrollIntoView(tasks[42]);
                PumpLayout(window);
                var viewer = FindScrollViewer(grid);
                Assert.NotNull(viewer);
                Assert.True(viewer!.VerticalOffset > 0, "The selected synthetic row should be below the first viewport.");
                var beforeOffset = viewer.VerticalOffset;

                taskIndex.Merge(tasks, Task("task-42", TaskState.Running, 67));
                // This mirrors ApplyTaskEventAsync: the collection replacement is followed
                // by an ID-based SelectedTask assignment on the same UI turn.
                selection.SelectedTask = tasks[42];
                PumpLayout(window);

                Assert.DoesNotContain(NotifyCollectionChangedAction.Reset, actions);
                Assert.Equal(new[] { NotifyCollectionChangedAction.Replace }, actions.ToArray());
                Assert.Equal(80, tasks.Count);
                Assert.Equal(42, grid.SelectedIndex);
                Assert.Equal("task-42", (grid.SelectedItem as TaskStatusDto)?.TaskId);
                Assert.Equal(67, tasks[42].ProgressPercent);
                Assert.InRange(viewer.VerticalOffset, beforeOffset - 1, beforeOffset + 1);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void ProgressDisplayAndCancellationKeepUnknownAndTerminalStatesDistinct()
    {
        var queued = Task("queued", TaskState.Queued, 0);
        var unknown = Task("unknown", TaskState.Running, -1);
        var running = Task("running", TaskState.Running, 0);
        var clamped = Task("clamped", TaskState.Running, 120);
        var cancelled = Task("cancelled", TaskState.Cancelled, 100);
        var succeeded = Task("succeeded", TaskState.Succeeded, 100);

        Assert.Equal("—", queued.ProgressDisplay);
        Assert.Equal("—", unknown.ProgressDisplay);
        Assert.Equal("0%", running.ProgressDisplay);
        Assert.Equal(100, clamped.ProgressValue);
        Assert.Equal("100%", clamped.ProgressDisplay);
        Assert.Equal("已取消", cancelled.StateDisplay);
        Assert.Equal("成功", succeeded.StateDisplay);
        Assert.NotEqual(cancelled.StateDisplay, succeeded.StateDisplay);
        Assert.False(SnapshotComparers.Task(cancelled, succeeded));
    }

    [Fact]
    public void TaskCenterUsesRowProgressBindingsAndSeparateCancellationStatus()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var task = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));

        Assert.Contains("Value=\"{Binding ProgressValue, Mode=OneWay}\"", task);
        Assert.Contains("Text=\"{Binding ProgressDisplay, Mode=OneWay}\"", task);
        Assert.Contains("Value=\"{Binding SelectedTask.ProgressValue, Mode=OneWay}\"", task);
        Assert.Contains("Text=\"{Binding SelectedTask.ProgressDisplay, Mode=OneWay}\"", task);
        Assert.Contains("StateDisplay, Mode=OneWay", task);
        Assert.DoesNotContain("Text=\"{Binding ProgressPercent", task);
        Assert.DoesNotContain("Text=\"{Binding SelectedTask.ProgressPercent", task);
        Assert.Contains("TaskCancellationStatusText", task);
        Assert.Contains("Binding=\"{Binding IsCancellingTask}\"", task);
        Assert.Contains("taskIndex.Merge(Tasks, change.Task);", viewModel);
        Assert.Contains("TaskSummary = page?.Summary", viewModel);
        Assert.Contains("public int TaskTotalCount => TaskSummary.TotalCount", viewModel);
    }

    [Fact]
    public void CancellationStatusTextFollowsTheRealTaskCenterBinding()
    {
        RunSta(() =>
        {
            var state = new CancellationState();
            var view = new TaskCenterView { DataContext = state };
            var status = (TextBlock)view.FindName("TaskCancellationStatusText")!;
            var window = new Window
            {
                Width = 720,
                Height = 480,
                Content = view,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None
            };

            try
            {
                window.Show();
                PumpLayout(window);
                Assert.Equal(Visibility.Collapsed, status.Visibility);

                state.IsCancellingTask = true;
                PumpLayout(window);

                Assert.Equal(Visibility.Visible, status.Visibility);
                Assert.Equal("正在取消…", status.Text);
                Assert.Contains("Worker", AutomationProperties.GetHelpText(status));
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static TaskStatusDto Task(string id, TaskState state, int progress)
        => new TaskStatusDto
        {
            TaskId = id,
            TaskType = "Backup",
            GameName = "Synthetic Game",
            State = state,
            ProgressPercent = progress,
            CreatedUtc = new DateTime(2026, 9, 18)
        };

    private sealed class TaskSelectionState : INotifyPropertyChanged
    {
        private TaskStatusDto? selectedTask;

        public TaskStatusDto? SelectedTask
        {
            get => selectedTask;
            set
            {
                if (ReferenceEquals(selectedTask, value)) return;
                selectedTask = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedTask)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private sealed class CancellationState : INotifyPropertyChanged
    {
        private bool isCancellingTask;

        public bool IsCancellingTask
        {
            get => isCancellingTask;
            set
            {
                if (isCancellingTask == value) return;
                isCancellingTask = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCancellingTask)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private static ScrollViewer? FindScrollViewer(DependencyObject root)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is ScrollViewer viewer) return viewer;
            var nested = FindScrollViewer(child);
            if (nested != null) return nested;
        }
        return null;
    }

    private static void PumpLayout(Window window)
    {
        window.UpdateLayout();
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
        window.UpdateLayout();
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure != null)
            throw new Xunit.Sdk.XunitException(failure.ToString());
    }
}
