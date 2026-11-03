using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.Views;
using Xunit;
using static GameSaveCenter.Playnite.Tests.R06SelectionStateBehaviorTests;

namespace GameSaveCenter.Playnite.Tests;

[Collection("SelectionFrameWpf")]
public sealed class R06TaskGridSelectionFrameBehaviorTests
{
    [Fact]
    public void SelectedFrameContainsVisibleTextAtTheHorizontalViewportEdgeInEveryTheme()
    {
        RunSta(() =>
        {
            var window = new Window
            {
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                ShowActivated = false,
                Opacity = 0.01
            };

            try
            {
                foreach (var scenario in new[]
                {
                    (GameSaveCenterThemeMode.Light, Width: 760d, Height: 640d),
                    (GameSaveCenterThemeMode.Dark, Width: 760d, Height: 640d),
                    (GameSaveCenterThemeMode.Light, Width: 1040d, Height: 700d),
                    (GameSaveCenterThemeMode.Dark, Width: 1040d, Height: 700d),
                    (GameSaveCenterThemeMode.Light, Width: 1600d, Height: 900d),
                    (GameSaveCenterThemeMode.Dark, Width: 1600d, Height: 900d)
                })
                {
                    var selected = new TaskStatusDto
                    {
                        TaskId = "selected-frame-tail",
                        TaskType = "MediaInbox",
                        GameName = "长游戏名称用于横向表格末端测量",
                        State = TaskState.Failed,
                        ErrorCode = "WORKER_RESTARTED_RETRYABLE",
                        ErrorMessage = "Worker 意外退出，任务中断；请确认目标文件状态后重新执行。末端文本必须落在选择框内。",
                        CreatedUtc = new DateTime(2026, 10, 20, 10, 36, 0, DateTimeKind.Utc)
                    };
                    var fixture = new TaskGridSelectionFixture(selected);
                    var view = new TaskCenterView { DataContext = fixture, UiAnimationsEnabled = false };
                    var taskGrid = Assert.IsType<DataGrid>(view.FindName("TaskGrid"));

                    window.Width = scenario.Width;
                    window.Height = scenario.Height;
                    window.Content = view;
                    ApplyTheme(view, view.Resources, scenario.Item1);
                    view.ApplyResponsiveLayout(scenario.Width, scenario.Height);
                    if (!window.IsVisible)
                        window.Show();
                    PumpLayout(window);
                    taskGrid.SelectedItem = selected;
                    PumpLayout(window);

                    var row = taskGrid.ItemContainerGenerator.ContainerFromItem(selected) as DataGridRow;
                    if (row == null)
                        throw new InvalidOperationException($"Task row was not realized for {scenario.Item1} {scenario.Width} DIP; items={taskGrid.Items.Count}, source={taskGrid.ItemsSource?.GetType().Name ?? "null"}, grid={taskGrid.ActualWidth:0.##}x{taskGrid.ActualHeight:0.##}, visible={taskGrid.Visibility}, loaded={taskGrid.IsLoaded}, viewLoaded={view.IsLoaded}.");
                    var chrome = ChromeFor(row);
                    var chromeBounds = VisibleBoundsRelativeTo(chrome, taskGrid);
                    var visibleContent = FindVisualChildren<DataGridCell>(row)
                        .Where(cell => cell.Visibility == Visibility.Visible && cell.ActualWidth > 0 && cell.ActualHeight > 0)
                        .SelectMany(cell => FindVisualChildren<FrameworkElement>(cell)
                            .Where(element => element is TextBlock or ProgressBar
                                || element is Border border && !string.Equals(border.Name, "CellChrome", StringComparison.Ordinal))
                            .Select(element => new
                            {
                                cell.Column.DisplayIndex,
                                Element = element,
                                Bounds = VisibleBoundsRelativeTo(element, taskGrid),
                                Value = element is TextBlock text ? text.Text : element.Name
                            }))
                        .Where(item => !item.Bounds.IsEmpty && item.Bounds.Width > 0 && item.Bounds.Height > 0)
                        .ToArray();
                    var rightmost = visibleContent.OrderByDescending(item => item.Bounds.Right).FirstOrDefault()
                        ?? throw new InvalidOperationException($"{scenario.Item1} {scenario.Width} DIP: no visible task cell content was measured.");
                    var diagnostic = $"{scenario.Item1} {scenario.Width} DIP: grid={taskGrid.ActualWidth:0.##}, row={row.ActualWidth:0.##}, chromeActual={chrome.ActualWidth:0.##}, chromeBinding={System.Windows.Data.BindingOperations.GetBindingExpression(chrome, FrameworkElement.WidthProperty)?.Status}, visibleChrome={chromeBounds}, ancestors={DescribeVisualAncestors(chrome, taskGrid)}, rightmostCell[{rightmost.DisplayIndex}]={rightmost.Bounds}, element={rightmost.Element.GetType().Name}/{rightmost.Element.Name} '{rightmost.Value}'";

                    Assert.True(chromeBounds.Width > 0 && chromeBounds.Height > 0, $"Selected frame is not visible: {diagnostic}");
                    Assert.All(visibleContent, item => Assert.True(
                        item.Bounds.Right <= chromeBounds.Right - 1d,
                        $"Visible task content escaped the selected frame: {diagnostic}; offendingCell[{item.DisplayIndex}]={item.Bounds} element={item.Element.GetType().Name}/{item.Element.Name} '{item.Value}'."));
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    private sealed class TaskGridSelectionFixture
    {
        public TaskGridSelectionFixture(TaskStatusDto selected)
        {
            SelectedTask = selected;
            TasksView = new ObservableCollection<TaskStatusDto> { selected };
        }

        public ObservableCollection<TaskStatusDto> TasksView { get; }
        public TaskStatusDto SelectedTask { get; set; }
    }
}

[CollectionDefinition("SelectionFrameWpf", DisableParallelization = true)]
public sealed class SelectionFrameWpfCollection
{
}
