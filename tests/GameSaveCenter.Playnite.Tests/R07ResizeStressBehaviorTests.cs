using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;
using Xunit.Abstractions;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R07ResizeStressBehaviorTests
{
    private readonly ITestOutputHelper output;

    public R07ResizeStressBehaviorTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void ResizeSequenceKeepsOpenTaskDetailsAndPickerFocusReachable()
    {
        Exception? exception = null;
        var samples = new List<ResizeSample>();

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var shell = new AcrylicProductionShellView();
                typeof(AcrylicProductionShellView)
                    .GetMethod("CreatePages", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(shell, null);

                var taskView = shell.GetWorkspaceView<TaskCenterView>(WorkspaceKind.Tasks)!;
                var taskType = typeof(TaskCenterView);
                var taskGrid = (DataGrid)taskType.GetField("TaskGrid", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(taskView)!;
                var selected = new TaskStatusDto
                {
                    TaskId = "r07-resize-task",
                    TaskType = "Validation",
                    GameName = "resize 详情保持",
                    State = TaskState.Failed,
                    ErrorCode = "R07-08",
                    ErrorMessage = string.Join("；", Enumerable.Repeat("短窗详情仍可读", 60))
                };
                var context = new ResizeTaskContext(selected);
                taskView.DataContext = context;
                taskGrid.ItemsSource = context.Tasks;
                taskGrid.SelectedItem = selected;
                context.SelectedTask = selected;

                var pickerOverlay = (Grid)typeof(AcrylicProductionShellView)
                    .GetField("PickerOverlay", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(shell)!;
                var pickerPanel = (Border)typeof(AcrylicProductionShellView)
                    .GetField("PickerPanel", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(shell)!;
                var pageHost = (ContentControl)shell.PageHostForAudit;
                pageHost.Content = taskView;
                pickerOverlay.Visibility = Visibility.Visible;

                window = new Window
                {
                    Content = shell,
                    Width = 1366,
                    Height = 900,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    WindowStyle = WindowStyle.None,
                    Opacity = 0.01
                };
                window.Show();
                ApplySize(window, shell, 1366, 900);
                if (!taskView.TaskDetailScrollViewerElement.IsVisible)
                    throw new InvalidOperationException("selected task detail was not visible at the wide baseline.");

                shell.GameSearchBoxForFocus.Focus();
                Keyboard.Focus(shell.GameSearchBoxForFocus);
                FlushLayout(window);

                foreach (var size in new[]
                {
                    (width: 1366d, height: 900d, label: "wide"),
                    (width: 960d, height: 700d, label: "narrow"),
                    (width: 960d, height: 560d, label: "short"),
                    (width: 1440d, height: 900d, label: "wide-again"),
                    (width: 1366d, height: 900d, label: "restored")
                })
                {
                    ApplySize(window, shell, size.width, size.height);
                    samples.Add(new ResizeSample(
                        size.label,
                        taskView.TaskDetailScrollViewerElement.Visibility,
                        pickerOverlay.Visibility,
                        shell.GameSearchBoxForFocus.IsKeyboardFocusWithin,
                        taskGrid.ActualHeight,
                        taskGrid.MaxHeight,
                        taskView.TaskDetailScrollViewerElement.MaxHeight,
                        pickerPanel.ActualHeight,
                        pickerPanel.MaxHeight));
                }

                Assert.All(samples, sample =>
                {
                    Assert.Equal(Visibility.Visible, sample.DetailVisibility);
                    Assert.Equal(Visibility.Visible, sample.PickerVisibility);
                    Assert.True(sample.SearchHasFocus, $"search focus was lost during {sample.Label}");
                    Assert.True(sample.TaskGridActualHeight > 0, $"task grid became blank during {sample.Label}");
                    Assert.True(double.IsPositiveInfinity(sample.TaskGridMaxHeight), $"task grid retained a finite MaxHeight during {sample.Label}: {sample.TaskGridMaxHeight}");
                    Assert.True(IsFiniteOrPositiveInfinity(sample.DetailMaxHeight), $"detail MaxHeight became invalid during {sample.Label}: {sample.DetailMaxHeight}");
                    Assert.True(sample.PickerActualHeight > 0, $"picker menu became blank during {sample.Label}");
                    Assert.True(IsFiniteOrPositiveInfinity(sample.PickerMaxHeight), $"picker MaxHeight became invalid during {sample.Label}: {sample.PickerMaxHeight}");
                    if (sample.Label == "wide-again" || sample.Label == "restored")
                        Assert.True(double.IsPositiveInfinity(sample.DetailMaxHeight), $"wide detail retained a finite MaxHeight during {sample.Label}: {sample.DetailMaxHeight}");
                });
                foreach (var sample in samples)
                    output.WriteLine(sample.ToString());
            }
            catch (Exception caught)
            {
                exception = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
    }

    private static void ApplySize(Window window, AcrylicProductionShellView shell, double width, double height)
    {
        window.Width = width;
        window.Height = height;
        FlushLayout(window);
        shell.ApplyResponsiveLayout(width, height);
        FlushLayout(window);
    }

    private static void FlushLayout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
        window.UpdateLayout();
    }

    private static bool IsFiniteOrPositiveInfinity(double value)
        => !double.IsNaN(value) && !double.IsNegativeInfinity(value);

    private sealed class ResizeTaskContext : INotifyPropertyChanged
    {
        private TaskStatusDto? selectedTask;

        public ResizeTaskContext(TaskStatusDto selectedTask)
        {
            Tasks = new ObservableCollection<TaskStatusDto> { selectedTask };
            this.selectedTask = selectedTask;
        }

        public ObservableCollection<TaskStatusDto> Tasks { get; }

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

        public bool IsCancellingTask => false;
        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private sealed class ResizeSample
    {
        public ResizeSample(
            string label,
            Visibility detailVisibility,
            Visibility pickerVisibility,
            bool searchHasFocus,
            double taskGridActualHeight,
            double taskGridMaxHeight,
            double detailMaxHeight,
            double pickerActualHeight,
            double pickerMaxHeight)
        {
            Label = label;
            DetailVisibility = detailVisibility;
            PickerVisibility = pickerVisibility;
            SearchHasFocus = searchHasFocus;
            TaskGridActualHeight = taskGridActualHeight;
            TaskGridMaxHeight = taskGridMaxHeight;
            DetailMaxHeight = detailMaxHeight;
            PickerActualHeight = pickerActualHeight;
            PickerMaxHeight = pickerMaxHeight;
        }

        public string Label { get; }
        public Visibility DetailVisibility { get; }
        public Visibility PickerVisibility { get; }
        public bool SearchHasFocus { get; }
        public double TaskGridActualHeight { get; }
        public double TaskGridMaxHeight { get; }
        public double DetailMaxHeight { get; }
        public double PickerActualHeight { get; }
        public double PickerMaxHeight { get; }

        public override string ToString()
            => $"{Label}: detail={DetailVisibility}; picker={PickerVisibility}; focus={SearchHasFocus}; "
                + $"gridActual={TaskGridActualHeight:0.###}; gridMax={TaskGridMaxHeight:0.###}; "
                + $"detailMax={DetailMaxHeight:0.###}; pickerActual={PickerActualHeight:0.###}; pickerMax={PickerMaxHeight:0.###}";
    }
}
