using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R06DetailsBudgetBehaviorTests
{
    [Fact]
    public void SaveCandidateSelectionRestoresStablePathBeforePendingFallback()
    {
        var previouslySelected = new SavePathCandidateDto
        {
            PlayniteId = "game-1",
            Path = @"D:\Saves\accepted\slot-2.sav",
            Status = "Accepted"
        };
        var refreshed = new[]
        {
            new SavePathCandidateDto
            {
                PlayniteId = "game-1",
                Path = @"D:\Saves\pending\slot-1.sav",
                Status = "Pending"
            },
            new SavePathCandidateDto
            {
                PlayniteId = "game-1",
                Path = @"d:\saves\accepted\slot-2.sav",
                Status = "Accepted"
            }
        };

        var restored = DashboardViewModel.RestoreSaveCandidateSelection(refreshed, previouslySelected);
        var fallback = DashboardViewModel.RestoreSaveCandidateSelection(refreshed, new SavePathCandidateDto
        {
            PlayniteId = "game-1",
            Path = @"D:\Saves\removed\slot-9.sav",
            Status = "Rejected"
        });

        Assert.Same(refreshed[1], restored);
        Assert.Same(refreshed[0], fallback);
    }

    [Fact]
    public void LongTaskDetailsStayInInspectorAndSelectionUpdatesTheDetailObject()
    {
        Exception? exception = null;
        var detailScrolls = false;
        var selectedTaskId = string.Empty;
        var detailText = string.Empty;
        var detailExpanderExpanded = false;
        var readableRows = 0;
        var maximumRowHeight = 0d;

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var first = new TaskStatusDto
                {
                    TaskId = "task-a",
                    TaskType = "Backup",
                    GameId = "game-1",
                    GameName = "短详情游戏",
                    State = TaskState.Succeeded,
                    ProgressPercent = 100,
                    CreatedUtc = DateTime.UtcNow.AddMinutes(-2),
                    Message = "首条任务详情"
                };
                var second = new TaskStatusDto
                {
                    TaskId = "task-b",
                    TaskType = "Validation",
                    GameId = "game-1",
                    GameName = "长诊断游戏",
                    State = TaskState.Failed,
                    CreatedUtc = DateTime.UtcNow.AddMinutes(-1),
                    ErrorCode = "E_LONG_DETAIL",
                    ErrorMessage = string.Join("；", Enumerable.Repeat("远端校验失败，保留本地版本并等待用户确认", 160))
                };
                var items = new ObservableCollection<TaskStatusDto> { first, second };
                var probe = new TaskDetailsProbe(items, first);
                var view = new TaskCenterView { DataContext = probe };
                var viewType = typeof(TaskCenterView);
                var grid = (DataGrid)viewType.GetField("TaskGrid", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
                var detailViewer = view.TaskDetailScrollViewerElement;
                var detailCard = view.TaskDetailCardElement;
                var expander = (Expander)viewType.GetField("TaskTechnicalDetailsExpander", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;

                window = new Window
                {
                    Content = view,
                    Width = 1120,
                    Height = 700,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    WindowStyle = WindowStyle.None,
                    Opacity = 0.01
                };
                window.Show();
                view.ApplyResponsiveLayout(1120, 700);
                window.UpdateLayout();

                grid.SelectedIndex = 0;
                window.UpdateLayout();
                grid.SelectedIndex = 1;
                window.UpdateLayout();
                expander.IsExpanded = true;
                window.UpdateLayout();

                selectedTaskId = probe.SelectedTask?.TaskId ?? string.Empty;
                detailText = string.Join("\n", FindVisualChildren<TextBlock>(detailCard).Select(textBlock => textBlock.Text));
                detailExpanderExpanded = expander.IsExpanded;
                detailScrolls = detailViewer.ScrollableHeight > 0.5;
                var rows = FindVisualChildren<DataGridRow>(grid)
                    .Where(row => row.Visibility == Visibility.Visible && row.ActualHeight > 0)
                    .ToArray();
                maximumRowHeight = rows.Length == 0 ? 0 : rows.Max(row => row.ActualHeight);
                readableRows = rows.Count(row => IsFullyInside(row, grid));
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
        Assert.Equal("task-b", selectedTaskId);
        Assert.Contains("task-b", detailText);
        Assert.Contains("E_LONG_DETAIL", detailText);
        Assert.DoesNotContain("task-a", detailText);
        Assert.True(detailExpanderExpanded);
        Assert.True(detailScrolls, "long diagnostic text must use the independent detail scroller");
        Assert.True(readableRows >= 2, $"only {readableRows} task rows stayed fully inside the list");
        Assert.InRange(maximumRowHeight, 1, 60);
    }

    private static bool IsFullyInside(FrameworkElement element, FrameworkElement boundary)
    {
        var bounds = element.TransformToAncestor(boundary).TransformBounds(
            new Rect(0, 0, element.ActualWidth, element.ActualHeight));
        return bounds.Height > 0
            && bounds.Top >= -0.5
            && bounds.Bottom <= boundary.ActualHeight + 0.5;
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
                yield return match;
            foreach (var nested in FindVisualChildren<T>(child))
                yield return nested;
        }
    }

    private sealed class TaskDetailsProbe : INotifyPropertyChanged
    {
        private TaskStatusDto? selectedTask;

        public TaskDetailsProbe(IEnumerable<TaskStatusDto> tasks, TaskStatusDto selectedTask)
        {
            var collection = new ObservableCollection<TaskStatusDto>(tasks);
            TasksView = CollectionViewSource.GetDefaultView(collection);
            this.selectedTask = selectedTask;
        }

        public ICollectionView TasksView { get; }

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
}
