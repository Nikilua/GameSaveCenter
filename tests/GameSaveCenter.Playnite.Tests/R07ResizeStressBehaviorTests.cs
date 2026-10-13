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
        var shellSizeChangedCount = 0;

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var shell = new AcrylicProductionShellView();
                shell.SizeChanged += (_, __) => shellSizeChangedCount++;
                typeof(AcrylicProductionShellView)
                    .GetMethod("CreatePages", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(shell, null);

                var taskView = shell.GetWorkspaceView<TaskCenterView>(WorkspaceKind.Tasks)!;
                var taskType = typeof(TaskCenterView);
                var taskGrid = (DataGrid)taskType.GetField("TaskGrid", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(taskView)!;
                var compactDetailsButton = (Button)taskType.GetField("TaskCompactDetailsButton", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(taskView)!;
                var compactCloseDetailsButton = (Button)taskType.GetField("TaskCompactCloseDetailsButton", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(taskView)!;
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

                var pickerOverlay = (Grid)typeof(AcrylicProductionShellView)
                    .GetField("PickerOverlay", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(shell)!;
                var pickerPanel = (Border)typeof(AcrylicProductionShellView)
                    .GetField("PickerPanel", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(shell)!;
                var pageHost = (ContentControl)shell.PageHostForAudit;
                var taskPageScrollSurface = (Grid)taskType.GetField("TaskPageScrollSurface", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(taskView)!;
                var detailBreakpoint = taskType.GetField("detailBreakpoint", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(taskView)!;
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
                ApplySize(window, 960, 700);
                // Select through the loaded production grid. On compact layouts the
                // production selection handler closes the detail drawer, so reopen it
                // through its routed button click before measuring the resize sequence.
                taskGrid.SelectedItem = selected;
                taskGrid.ScrollIntoView(selected);
                FlushLayout(window);
                var taskDetail = taskView.TaskDetailScrollViewerElement;
                if (compactDetailsButton.Visibility != Visibility.Visible
                    || !compactDetailsButton.IsVisible
                    || !(compactDetailsButton.ActualWidth > 0)
                    || !(compactDetailsButton.ActualHeight > 0)
                    || taskDetail.Visibility != Visibility.Collapsed)
                {
                    throw new InvalidOperationException(
                        "compact selection did not start with the details drawer closed and its open action available: "
                        + $"detail={taskDetail.Visibility}, openButton={compactDetailsButton.Visibility}, "
                        + $"gridSelected={(taskGrid.SelectedItem as TaskStatusDto)?.TaskId ?? "<null>"}, "
                        + $"viewModelSelected={context.SelectedTask?.TaskId ?? "<null>"}, "
                        + $"requestedWindow={window.Width:0.###}x{window.Height:0.###}, actualWindow={window.ActualWidth:0.###}x{window.ActualHeight:0.###}, "
                        + $"shell={shell.ActualWidth:0.###}x{shell.ActualHeight:0.###}, pageHost={pageHost.ActualWidth:0.###}x{pageHost.ActualHeight:0.###}, "
                        + $"taskView={taskView.ActualWidth:0.###}x{taskView.ActualHeight:0.###}, pageSurface={taskPageScrollSurface.ActualWidth:0.###}x{taskPageScrollSurface.ActualHeight:0.###}, "
                        + $"breakpointInitialized={detailBreakpoint.GetType().GetProperty("IsInitialized")!.GetValue(detailBreakpoint)}, "
                        + $"breakpointCompact={detailBreakpoint.GetType().GetProperty("IsCompact")!.GetValue(detailBreakpoint)}.");
                }

                compactDetailsButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                FlushLayout(window);
                if (taskDetail.Visibility != Visibility.Visible
                    || !taskDetail.IsVisible
                    || !(taskDetail.ActualWidth > 0)
                    || !(taskDetail.ActualHeight > 0)
                    || compactDetailsButton.Visibility != Visibility.Collapsed
                    || compactCloseDetailsButton.Visibility != Visibility.Visible)
                {
                    throw new InvalidOperationException(
                        "routed compact details action did not open the selected task drawer: "
                        + $"detail={taskDetail.Visibility} {taskDetail.ActualWidth:0.###}x{taskDetail.ActualHeight:0.###}, "
                        + $"openButton={compactDetailsButton.Visibility}, closeButton={compactCloseDetailsButton.Visibility}.");
                }
                output.WriteLine(
                    $"compactDetailsAction=clicked, openButton={compactDetailsButton.Visibility}, "
                    + $"closeButton={compactCloseDetailsButton.Visibility}, "
                    + $"detail={taskDetail.Visibility} {taskDetail.ActualWidth:0.###}x{taskDetail.ActualHeight:0.###} DIP");

                ApplySize(window, 1366, 900);
                taskDetail = taskView.TaskDetailScrollViewerElement;
                if (!ReferenceEquals(taskGrid.SelectedItem, selected)
                    || !ReferenceEquals(context.SelectedTask, selected)
                    || taskDetail.Visibility != Visibility.Visible
                    || !taskDetail.IsVisible
                    || !(taskDetail.ActualWidth > 0)
                    || !(taskDetail.ActualHeight > 0))
                {
                    throw new InvalidOperationException(
                        "selected task detail did not remain visible at the wide baseline: "
                        + $"visibility={taskDetail.Visibility}, isVisible={taskDetail.IsVisible}, "
                        + $"actual={taskDetail.ActualWidth:0.###}x{taskDetail.ActualHeight:0.###}, "
                        + $"taskViewIsVisible={taskView.IsVisible}, windowIsVisible={window.IsVisible}, "
                        + $"gridSelected={(taskGrid.SelectedItem as TaskStatusDto)?.TaskId ?? "<null>"}, "
                        + $"viewModelSelected={context.SelectedTask?.TaskId ?? "<null>"}, gridItems={taskGrid.Items.Count}, "
                        + $"compactButton={compactDetailsButton.Visibility}, closeButton={compactCloseDetailsButton.Visibility}, "
                        + "the compact drawer had been opened through its routed action.");
                }

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
                    ApplySize(window, size.width, size.height);
                    samples.Add(new ResizeSample(
                        size.label,
                        size.width,
                        size.height,
                        window.ActualWidth,
                        window.ActualHeight,
                        shell.ActualWidth,
                        shell.ActualHeight,
                        pageHost.ActualWidth,
                        pageHost.ActualHeight,
                        (bool)detailBreakpoint.GetType().GetProperty("IsCompact")!.GetValue(detailBreakpoint)!,
                        taskView.TaskDetailScrollViewerElement.Visibility,
                        taskView.TaskDetailScrollViewerElement.ActualWidth,
                        taskView.TaskDetailScrollViewerElement.ActualHeight,
                        pickerOverlay.Visibility,
                        shell.GameSearchBoxForFocus.IsKeyboardFocusWithin,
                        taskGrid.ActualHeight,
                        taskGrid.MaxHeight,
                        taskView.TaskDetailScrollViewerElement.MaxHeight,
                        pickerPanel.ActualHeight,
                        pickerPanel.MaxHeight));
                }

                var wideSample = samples.Single(sample => sample.Label == "wide");
                var restoredSample = samples.Single(sample => sample.Label == "restored");
                var narrowSample = samples.Single(sample => sample.Label == "narrow");
                var shortSample = samples.Single(sample => sample.Label == "short");
                var shellWidthInset = wideSample.WindowActualWidth - wideSample.ShellActualWidth;
                var shellHeightInset = wideSample.WindowActualHeight - wideSample.ShellActualHeight;
                Assert.InRange(shellWidthInset, 0, 32);
                Assert.InRange(shellHeightInset, 0, 32);
                Assert.True(Math.Abs(restoredSample.PageHostActualWidth - wideSample.PageHostActualWidth) <= 1,
                    $"restoring the original window size must restore the measured PageHost width: wide={wideSample.PageHostActualWidth:0.###}, restored={restoredSample.PageHostActualWidth:0.###}");
                Assert.True(Math.Abs(restoredSample.PageHostActualHeight - wideSample.PageHostActualHeight) <= 1,
                    $"restoring the original window size must restore the measured PageHost height: wide={wideSample.PageHostActualHeight:0.###}, restored={restoredSample.PageHostActualHeight:0.###}");
                Assert.True(shortSample.PageHostActualHeight < narrowSample.PageHostActualHeight,
                    $"the PageHost height must shrink in the shorter window: narrow={narrowSample.PageHostActualHeight:0.###}, short={shortSample.PageHostActualHeight:0.###} DIP");

                Assert.All(samples, sample =>
                {
                    Assert.True(sample.WindowActualWidth > 0, $"window had no laid out width during {sample.Label}");
                    Assert.True(sample.WindowActualHeight > 0, $"window had no laid out height during {sample.Label}");
                    Assert.True(sample.PageHostActualWidth > 0, $"page host had no laid out width during {sample.Label}");
                    Assert.True(sample.PageHostActualHeight > 0, $"page host had no laid out height during {sample.Label}");
                    Assert.InRange(Math.Abs((sample.WindowActualWidth - sample.ShellActualWidth) - shellWidthInset), 0, 1);
                    Assert.InRange(Math.Abs((sample.WindowActualHeight - sample.ShellActualHeight) - shellHeightInset), 0, 1);
                    if (sample.PageHostActualWidth < ResponsiveLayoutCoordinator.DetailCompactThreshold)
                        Assert.True(sample.DetailBreakpointCompact, $"page host was below the compact breakpoint but detail layout was wide during {sample.Label}: {sample.PageHostActualWidth:0.###} DIP");
                    if (sample.PageHostActualWidth >= ResponsiveLayoutCoordinator.DetailWideExitThreshold)
                        Assert.False(sample.DetailBreakpointCompact, $"page host was above the wide breakpoint but detail layout was compact during {sample.Label}: {sample.PageHostActualWidth:0.###} DIP");
                    Assert.Equal(Visibility.Visible, sample.DetailVisibility);
                    Assert.True(sample.DetailActualWidth > 0, $"task details had no laid out width during {sample.Label}: {sample.DetailActualWidth}");
                    Assert.True(sample.DetailActualHeight > 0, $"task details had no laid out height during {sample.Label}: {sample.DetailActualHeight}");
                    Assert.Equal(Visibility.Visible, sample.PickerVisibility);
                    Assert.True(sample.SearchHasFocus, $"search focus was lost during {sample.Label}");
                    Assert.True(sample.TaskGridActualHeight > 0, $"task grid became blank during {sample.Label}");
                    Assert.True(double.IsPositiveInfinity(sample.TaskGridMaxHeight), $"task grid retained a finite MaxHeight during {sample.Label}: {sample.TaskGridMaxHeight}");
                    Assert.True(IsFiniteOrPositiveInfinity(sample.DetailMaxHeight), $"detail MaxHeight became invalid during {sample.Label}: {sample.DetailMaxHeight}");
                    Assert.True(sample.PickerActualHeight > 0, $"picker menu became blank during {sample.Label}");
                    Assert.True(IsFiniteOrPositiveInfinity(sample.PickerMaxHeight), $"picker MaxHeight became invalid during {sample.Label}: {sample.PickerMaxHeight}");
                    if (sample.DetailBreakpointCompact)
                        Assert.True(sample.DetailMaxHeight > 0 && !double.IsPositiveInfinity(sample.DetailMaxHeight), $"compact detail did not receive its finite height budget during {sample.Label}: {sample.DetailMaxHeight}");
                    else
                        Assert.True(double.IsPositiveInfinity(sample.DetailMaxHeight), $"wide detail retained a finite MaxHeight during {sample.Label}: {sample.DetailMaxHeight}");
                });

                Assert.True(shellSizeChangedCount >= 4, $"window resize did not reach the production shell SizeChanged route enough times: {shellSizeChangedCount}");
                Assert.True(samples.Single(sample => sample.Label == "narrow").PickerMaxHeight < samples.Single(sample => sample.Label == "wide").PickerMaxHeight,
                    "picker height budget did not decrease when the window narrowed");
                Assert.True(samples.Single(sample => sample.Label == "short").PickerMaxHeight < samples.Single(sample => sample.Label == "narrow").PickerMaxHeight,
                    "picker height budget did not decrease when the window became shorter");
                foreach (var sample in samples)
                    output.WriteLine(sample.ToString());
                output.WriteLine($"shellSizeChangedCount={shellSizeChangedCount}");
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

    private static void ApplySize(Window window, double width, double height)
    {
        window.Width = width;
        window.Height = height;
        FlushLayout(window);
    }

    private static void FlushLayout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
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
            // Let the loaded DataGrid selection drive the production binding and handler.
            this.selectedTask = null;
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
            double requestedWidth,
            double requestedHeight,
            double windowActualWidth,
            double windowActualHeight,
            double shellActualWidth,
            double shellActualHeight,
            double pageHostActualWidth,
            double pageHostActualHeight,
            bool detailBreakpointCompact,
            Visibility detailVisibility,
            double detailActualWidth,
            double detailActualHeight,
            Visibility pickerVisibility,
            bool searchHasFocus,
            double taskGridActualHeight,
            double taskGridMaxHeight,
            double detailMaxHeight,
            double pickerActualHeight,
            double pickerMaxHeight)
        {
            Label = label;
            RequestedWidth = requestedWidth;
            RequestedHeight = requestedHeight;
            WindowActualWidth = windowActualWidth;
            WindowActualHeight = windowActualHeight;
            ShellActualWidth = shellActualWidth;
            ShellActualHeight = shellActualHeight;
            PageHostActualWidth = pageHostActualWidth;
            PageHostActualHeight = pageHostActualHeight;
            DetailBreakpointCompact = detailBreakpointCompact;
            DetailVisibility = detailVisibility;
            DetailActualWidth = detailActualWidth;
            DetailActualHeight = detailActualHeight;
            PickerVisibility = pickerVisibility;
            SearchHasFocus = searchHasFocus;
            TaskGridActualHeight = taskGridActualHeight;
            TaskGridMaxHeight = taskGridMaxHeight;
            DetailMaxHeight = detailMaxHeight;
            PickerActualHeight = pickerActualHeight;
            PickerMaxHeight = pickerMaxHeight;
        }

        public string Label { get; }
        public double RequestedWidth { get; }
        public double RequestedHeight { get; }
        public double WindowActualWidth { get; }
        public double WindowActualHeight { get; }
        public double ShellActualWidth { get; }
        public double ShellActualHeight { get; }
        public double PageHostActualWidth { get; }
        public double PageHostActualHeight { get; }
        public bool DetailBreakpointCompact { get; }
        public Visibility DetailVisibility { get; }
        public double DetailActualWidth { get; }
        public double DetailActualHeight { get; }
        public Visibility PickerVisibility { get; }
        public bool SearchHasFocus { get; }
        public double TaskGridActualHeight { get; }
        public double TaskGridMaxHeight { get; }
        public double DetailMaxHeight { get; }
        public double PickerActualHeight { get; }
        public double PickerMaxHeight { get; }

        public override string ToString()
            => $"{Label}: requested={RequestedWidth:0.###}x{RequestedHeight:0.###}; window={WindowActualWidth:0.###}x{WindowActualHeight:0.###}; shell={ShellActualWidth:0.###}x{ShellActualHeight:0.###}; pageHost={PageHostActualWidth:0.###}x{PageHostActualHeight:0.###}; detailCompact={DetailBreakpointCompact}; detail={DetailVisibility}/{DetailActualWidth:0.###}x{DetailActualHeight:0.###}; picker={PickerVisibility}; focus={SearchHasFocus}; "
                + $"gridActual={TaskGridActualHeight:0.###}; gridMax={TaskGridMaxHeight:0.###}; "
                + $"detailMax={DetailMaxHeight:0.###}; pickerActual={PickerActualHeight:0.###}; pickerMax={PickerMaxHeight:0.###}";
    }
}
