using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Newtonsoft.Json;
using PlayniteButton = GameSaveCenter.Playnite.Controls.Button;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R10RecentAccessBehaviorTests
{
    [Fact]
    public void RecentAccessRecordsAreBoundedStableIdOnlyAndPruneRemovedGames()
    {
        var now = DateTime.UtcNow;
        var source = Enumerable.Range(0, 10)
            .Select(index => new RecentAccessRecord
            {
                PlayniteId = $"game-{index}",
                Workspace = index == 0 ? "Unknown" : "Saves",
                TabIndex = 99,
                LastAccessUtc = now.AddMinutes(-index)
            })
            .ToList();
        source.Add(new RecentAccessRecord
        {
            PlayniteId = " game-1 ",
            Workspace = "Media",
            TabIndex = 2,
            LastAccessUtc = now.AddMinutes(1)
        });
        source.Add(new RecentAccessRecord { PlayniteId = "   " });

        var normalized = RecentAccessRecord.NormalizeMany(source);

        Assert.Equal(RecentAccessRecord.MaxEntries, normalized.Count);
        Assert.Equal("game-1", normalized[0].PlayniteId);
        Assert.Equal("Media", normalized[0].Workspace);
        Assert.Equal(2, normalized[0].TabIndex);
        Assert.Equal("Overview", normalized.Single(item => item.PlayniteId == "game-0").Workspace);
        Assert.DoesNotContain(normalized, item => item.PlayniteId == "game-9");

        var existing = RecentAccessRecord.KeepExistingGames(normalized, new[] { "game-1", "game-3" });
        Assert.Equal(new[] { "game-1", "game-3" }, existing.Select(item => item.PlayniteId));
        var olderExisting = RecentAccessRecord.KeepExistingGames(source, new[] { "game-9" });
        Assert.Equal("game-9", Assert.Single(olderExisting).PlayniteId);

        var json = JsonConvert.SerializeObject(normalized);
        Assert.Contains("PlayniteId", json);
        Assert.Contains("Workspace", json);
        Assert.DoesNotContain("GameName", json);
        Assert.DoesNotContain("Path", json);
        Assert.DoesNotContain("Archive", json);
    }

    [Fact]
    public void RecentAccessSwitchIsKeyboardReachableAndKeepsTaskHistorySeparate()
    {
        Exception? exception = null;
        var clicks = 0;
        var thread = new System.Threading.Thread(() =>
        {
            try
            {
                var data = new OverviewRecentAccessData(() => clicks++);
                data.RecentAccessItems.Add(new RecentAccessItem(
                    new RecentAccessRecord
                    {
                        PlayniteId = "game-1",
                        Workspace = RecentAccessRecord.MediaWorkspace,
                        TabIndex = 1,
                        LastAccessUtc = new DateTime(2026, 9, 19, 1, 2, 3, DateTimeKind.Utc)
                    },
                    "长名称测试游戏"));
                data.OverviewTasks.Add(new TaskStatusDto
                {
                    TaskId = "task-1",
                    TaskType = "备份",
                    GameName = "任务历史游戏",
                    State = TaskState.Succeeded,
                    Message = "最近任务记录",
                    CreatedUtc = DateTime.UtcNow
                });

                var overview = new OverviewView
                {
                    DataContext = data,
                    Width = 1280,
                    Height = 980
                };
                var recentAccessCard = Assert.IsType<Border>(overview.FindName("OverviewRecentAccessCard"));
                var recentTasksCard = Assert.IsType<Border>(overview.FindName("OverviewRecentActivityCard"));
                Assert.Equal(Visibility.Collapsed, recentAccessCard.Visibility);
                Assert.Equal(Visibility.Visible, recentTasksCard.Visibility);
                using var host = new WindowHost(overview);
                host.Window.UpdateLayout();

                var recentAccessList = Assert.IsType<ListBox>(overview.FindName("OverviewRecentAccessList"));
                var taskHistoryList = Assert.IsType<ListBox>(overview.FindName("OverviewActivityList"));
                Assert.Same(data.RecentAccessItems, recentAccessList.ItemsSource);
                Assert.Same(data.OverviewTasks, taskHistoryList.ItemsSource);
                Assert.NotSame(recentAccessList.ItemsSource, taskHistoryList.ItemsSource);

                var showRecentAccess = Assert.IsType<PlayniteButton>(overview.FindName("OverviewShowRecentAccessButton"));
                Assert.Equal("查看最近访问", AutomationProperties.GetName(showRecentAccess));
                Assert.True(showRecentAccess.IsTabStop);
                Assert.NotNull(showRecentAccess.FocusVisualStyle);
                Assert.True(showRecentAccess.Focus());
                Assert.Same(showRecentAccess, Keyboard.FocusedElement);
                typeof(ButtonBase).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(showRecentAccess, Array.Empty<object>());
                host.Window.UpdateLayout();

                Assert.Equal(Visibility.Visible, recentAccessCard.Visibility);
                Assert.Equal(Visibility.Collapsed, recentTasksCard.Visibility);
                Assert.True(recentAccessList.IsVisible);
                Assert.False(taskHistoryList.IsVisible);

                var button = FindVisualDescendants<PlayniteButton>(overview)
                    .SingleOrDefault(candidate => AutomationProperties.GetName(candidate) == "打开最近访问对象");
                Assert.True(button is not null, "The recent-access view should realize its action row.");
                Assert.Same(data.OpenRecentAccessCommand, button!.Command);
                Assert.Equal("game-1", ((RecentAccessItem)button.CommandParameter!).PlayniteId);
                Assert.True(button.Command!.CanExecute(button.CommandParameter));
                Assert.True(button.IsVisible);
                var recentAccessTile = FindVisualDescendants<Border>(button)
                    .Single(border => border.Width == 34 && border.Height == 34);
                Assert.Equal(
                    Assert.IsType<SolidColorBrush>(overview.TryFindResource("GscControlFillBrush")).Color,
                    Assert.IsType<SolidColorBrush>(recentAccessTile.Background).Color);

                typeof(ButtonBase).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(button, Array.Empty<object>());

                var showRecentTasks = Assert.IsType<PlayniteButton>(overview.FindName("OverviewShowRecentTasksButton"));
                Assert.Equal("返回最近任务", AutomationProperties.GetName(showRecentTasks));
                Assert.True(showRecentTasks.IsTabStop);
                Assert.NotNull(showRecentTasks.FocusVisualStyle);
                Assert.True(showRecentTasks.Focus());
                Assert.Same(showRecentTasks, Keyboard.FocusedElement);
                typeof(ButtonBase).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(showRecentTasks, Array.Empty<object>());
                host.Window.UpdateLayout();

                Assert.Equal(Visibility.Collapsed, recentAccessCard.Visibility);
                Assert.Equal(Visibility.Visible, recentTasksCard.Visibility);
                Assert.False(recentAccessList.IsVisible);
                Assert.True(taskHistoryList.IsVisible);
                Assert.Same(data.RecentAccessItems, recentAccessList.ItemsSource);
                Assert.Same(data.OverviewTasks, taskHistoryList.ItemsSource);
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });
        thread.SetApartmentState(System.Threading.ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.True(exception is null, exception?.ToString());
        Assert.Equal(1, clicks);
    }

    [Fact]
    public void SelectedOverviewAndSharedListItemsKeepTheirContentPosition()
    {
        Exception? exception = null;
        var thread = new System.Threading.Thread(() =>
        {
            try
            {
                var data = new OverviewRecentAccessData(() => { });
                var firstTask = new TaskStatusDto
                {
                    TaskId = "selected-task",
                    TaskType = "备份",
                    GameName = "选择位置测试",
                    State = TaskState.Running,
                    Message = "执行中",
                    CreatedUtc = new DateTime(2026, 11, 2, 2, 0, 0, DateTimeKind.Utc)
                };
                data.OverviewTasks.Add(firstTask);
                data.OverviewTasks.Add(new TaskStatusDto
                {
                    TaskId = "unselected-task",
                    TaskType = "同步媒体",
                    GameName = "未选中项",
                    State = TaskState.Succeeded,
                    Message = "完成",
                    CreatedUtc = new DateTime(2026, 11, 2, 1, 0, 0, DateTimeKind.Utc)
                });

                {
                    var overview = new OverviewView
                    {
                        DataContext = data,
                        Width = 1280,
                        Height = 980
                    };
                    using var host = new WindowHost(overview);
                    var list = Assert.IsType<ListBox>(overview.FindName("OverviewActivityList"));

                    AssertContainerContentDoesNotMove(list, firstTask, "TaskItemChrome", "OverviewTaskStatusPill", host.Window);
                    var selectedChrome = FindItemChrome(list, firstTask, "TaskItemChrome");
                    Assert.Equal(new Thickness(4, 2, 4, 2), selectedChrome.Margin);
                    Assert.Equal(new CornerRadius(14), selectedChrome.CornerRadius);
                    AssertAccentBorder(list, selectedChrome);
                }

                var sharedOverview = new OverviewView
                {
                    DataContext = data,
                    Width = 1280,
                    Height = 980
                };
                var sharedList = Assert.IsType<ListBox>(sharedOverview.FindName("OverviewActivityList"));
                sharedList.ItemContainerStyle = Assert.IsType<Style>(sharedOverview.TryFindResource(typeof(ListBoxItem)));
                using var sharedHost = new WindowHost(sharedOverview);
                AssertContainerContentDoesNotMove(sharedList, firstTask, "ItemChrome", "OverviewTaskStatusPill", sharedHost.Window);
                var sharedChrome = FindItemChrome(sharedList, firstTask, "ItemChrome");
                AssertAccentBorder(sharedList, sharedChrome);
                Assert.Equal(8, sharedChrome.CornerRadius.TopLeft);
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });
        thread.SetApartmentState(System.Threading.ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.True(exception is null, exception?.ToString());
    }

    private static void AssertContainerContentDoesNotMove(
        ListBox list,
        TaskStatusDto task,
        string chromeName,
        string iconTileName,
        Window hostWindow)
    {
        list.SelectedItem = null;
        hostWindow.UpdateLayout();
        var container = Assert.IsType<ListBoxItem>(list.ItemContainerGenerator.ContainerFromItem(task));
        var unselectedContainerBounds = container.TransformToAncestor(list)
            .TransformBounds(new Rect(new Point(), container.RenderSize));
        var unselectedContainerMargin = container.Margin;
        var unselectedRenderTransform = container.RenderTransform.Value;
        var unselectedLayoutTransform = container.LayoutTransform.Value;
        var tile = FindVisualDescendants<Border>(container).Single(border => border.Name == iconTileName);
        var unselectedBounds = tile.TransformToAncestor(container)
            .TransformBounds(new Rect(new Point(), tile.RenderSize));

        list.SelectedItem = task;
        hostWindow.UpdateLayout();
        Assert.True(container.IsSelected);
        var selectedBounds = tile.TransformToAncestor(container)
            .TransformBounds(new Rect(new Point(), tile.RenderSize));
        AssertBoundsClose(unselectedBounds, selectedBounds);
        AssertContainerSlotUnchanged(
            list,
            container,
            unselectedContainerBounds,
            unselectedContainerMargin,
            unselectedRenderTransform,
            unselectedLayoutTransform);

        var chrome = FindItemChrome(list, task, chromeName);
        Assert.True(chrome.BorderThickness.Left > 0);
        Assert.True(chrome.CornerRadius.TopLeft > 0);
        Assert.True(container.Focus());
        hostWindow.UpdateLayout();
        var focusedBounds = tile.TransformToAncestor(container)
            .TransformBounds(new Rect(new Point(), tile.RenderSize));
        AssertBoundsClose(unselectedBounds, focusedBounds);
        AssertContainerSlotUnchanged(
            list,
            container,
            unselectedContainerBounds,
            unselectedContainerMargin,
            unselectedRenderTransform,
            unselectedLayoutTransform);
    }

    private static void AssertContainerSlotUnchanged(
        ListBox list,
        ListBoxItem container,
        Rect expectedBounds,
        Thickness expectedMargin,
        Matrix expectedRenderTransform,
        Matrix expectedLayoutTransform)
    {
        var actualBounds = container.TransformToAncestor(list)
            .TransformBounds(new Rect(new Point(), container.RenderSize));
        AssertBoundsClose(expectedBounds, actualBounds);
        Assert.Equal(expectedMargin, container.Margin);
        Assert.Equal(expectedRenderTransform, container.RenderTransform.Value);
        Assert.Equal(expectedLayoutTransform, container.LayoutTransform.Value);
    }

    private static Border FindItemChrome(ListBox list, TaskStatusDto task, string name)
    {
        var container = Assert.IsType<ListBoxItem>(list.ItemContainerGenerator.ContainerFromItem(task));
        return FindVisualDescendants<Border>(container).Single(border => border.Name == name);
    }

    private static void AssertAccentBorder(ListBox list, Border chrome)
    {
        var expected = Assert.IsType<SolidColorBrush>(list.TryFindResource("GscAccentBrush"));
        var actual = Assert.IsType<SolidColorBrush>(chrome.BorderBrush);
        Assert.Equal(expected.Color, actual.Color);
    }

    private static void AssertBoundsClose(Rect expected, Rect actual)
    {
        Assert.InRange(Math.Abs(expected.X - actual.X), 0, 0.01);
        Assert.InRange(Math.Abs(expected.Y - actual.Y), 0, 0.01);
        Assert.InRange(Math.Abs(expected.Width - actual.Width), 0, 0.01);
        Assert.InRange(Math.Abs(expected.Height - actual.Height), 0, 0.01);
    }

    private sealed class WindowHost : IDisposable
    {
        public WindowHost(FrameworkElement content)
        {
            Window = new Window
            {
                Content = content,
                Width = 1280,
                Height = 980,
                ShowInTaskbar = false,
                ShowActivated = false,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };
            Window.Show();
            Window.UpdateLayout();
            content.UpdateLayout();
        }

        public Window Window { get; }

        public void Dispose() => Window.Close();
    }

    private static List<T> FindVisualDescendants<T>(DependencyObject root)
        where T : DependencyObject
    {
        var matches = new List<T>();
        Visit(root, matches);
        return matches;
    }

    private static void Visit<T>(DependencyObject? current, List<T> matches)
        where T : DependencyObject
    {
        if (current == null) return;
        if (current is T match) matches.Add(match);
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(current); index++)
            Visit(VisualTreeHelper.GetChild(current, index), matches);
    }

    private sealed class OverviewRecentAccessData
    {
        public OverviewRecentAccessData(Action onClick)
        {
            OpenRecentAccessCommand = new CountingCommand(onClick);
            OpenActivityCommand = new CountingCommand(() => { });
            OpenCloudQueueCommand = new CountingCommand(() => { });
            RefreshCommand = new CountingCommand(() => { });
        }

        public DashboardSnapshotDto Snapshot { get; } = new();
        public ObservableCollection<RecentAccessItem> RecentAccessItems { get; } = new();
        public ObservableCollection<TaskStatusDto> OverviewTasks { get; } = new();
        public ObservableCollection<ActivityEntryDto> Activities { get; } = new();
        public ICommand OpenRecentAccessCommand { get; }
        public ICommand OpenActivityCommand { get; }
        public ICommand OpenCloudQueueCommand { get; }
        public ICommand RefreshCommand { get; }
    }

    private sealed class CountingCommand : ICommand
    {
        private readonly Action execute;

        public CountingCommand(Action execute) => this.execute = execute;

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => execute();
    }
}
