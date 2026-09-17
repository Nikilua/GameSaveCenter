using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R06TaskProgressBehaviorTests
{
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
