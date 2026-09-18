using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R07DetailsBreakpointBehaviorTests
{
    [Fact]
    public void DraggingAcrossDetailBreakpointKeepsSelectionFocusAndScrollPosition()
    {
        Exception? exception = null;
        var selectedId = string.Empty;
        var selectedReferencePreserved = false;
        var detailFocusPreserved = false;
        var detailScrollPreserved = false;
        var structuralModes = string.Empty;

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var selected = new TaskStatusDto
                {
                    TaskId = "r07-task",
                    TaskType = "Validation",
                    GameName = "断点详情测试",
                    State = TaskState.Failed,
                    ErrorCode = "R07"
                };
                // Use actual visible text so the detail ScrollViewer has a real
                // scrollable surface; no production or user data is involved.
                selected.ErrorMessage = string.Join("；", Repeat("失败详情保留选择与滚动位置", 80));

                var context = new TaskDetailsContext(selected);
                var view = new TaskCenterView { DataContext = context };
                var viewType = typeof(TaskCenterView);
                var grid = (DataGrid)viewType.GetField("TaskGrid", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var detailViewer = view.TaskDetailScrollViewerElement;
                var expander = (Expander)viewType.GetField("TaskTechnicalDetailsExpander", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                grid.ItemsSource = context.Tasks;
                grid.SelectedItem = selected;
                context.SelectedTask = selected;

                window = new Window
                {
                    Content = view,
                    Width = 1000,
                    Height = 700,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    WindowStyle = WindowStyle.None,
                    Opacity = 0.01
                };
                window.Show();
                view.ApplyResponsiveLayout(1000, 700);
                expander.IsExpanded = true;
                window.UpdateLayout();
                Keyboard.Focus(detailViewer);
                detailViewer.ScrollToVerticalOffset(Math.Min(24, detailViewer.ScrollableHeight));
                window.UpdateLayout();

                var beforeOffset = detailViewer.VerticalOffset;
                var selectedObject = grid.SelectedItem;

                view.ApplyResponsiveLayout(979, 700);
                view.ApplyResponsiveLayout(981, 700);
                view.ApplyResponsiveLayout(971, 700);
                window.UpdateLayout();
                var compactOffset = detailViewer.VerticalOffset;
                var compactRow = Grid.GetRow(detailViewer);
                var compactColumn = Grid.GetColumn(detailViewer);
                var compactVisible = detailViewer.Visibility == Visibility.Visible;

                view.ApplyResponsiveLayout(979, 700);
                view.ApplyResponsiveLayout(988, 700);
                window.UpdateLayout();
                var wideOffset = detailViewer.VerticalOffset;

                selectedId = context.SelectedTask?.TaskId ?? string.Empty;
                selectedReferencePreserved = ReferenceEquals(selectedObject, grid.SelectedItem);
                detailFocusPreserved = detailViewer.IsKeyboardFocusWithin;
                detailScrollPreserved = Math.Abs(beforeOffset - compactOffset) <= 1
                    && Math.Abs(beforeOffset - wideOffset) <= 1;
                structuralModes = $"{compactVisible}:{compactRow}:{compactColumn}";
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
        Assert.Equal("r07-task", selectedId);
        Assert.True(selectedReferencePreserved);
        Assert.True(detailFocusPreserved);
        Assert.True(detailScrollPreserved);
        Assert.Equal("True:4:0", structuralModes);
    }

    private static string[] Repeat(string value, int count)
    {
        var values = new string[count];
        for (var index = 0; index < values.Length; index++)
            values[index] = value;
        return values;
    }

    private sealed class TaskDetailsContext : INotifyPropertyChanged
    {
        private TaskStatusDto? selectedTask;

        public TaskDetailsContext(TaskStatusDto selectedTask)
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
}
