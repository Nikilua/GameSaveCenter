using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R10FilterSourceBehaviorTests
{
    [Fact]
    public void SourceBannerShowsOnlyForAnActiveContextAndBoundClearKeepsDraftFilters()
    {
        RunSta(() =>
        {
            var view = new TaskCenterView();
            var binding = new TestNavigationBinding
            {
                HasTaskNavigationTarget = true,
                TaskNavigationSourceSummary = "已带入游戏“合成游戏”的诊断条件。"
            };
            view.DataContext = binding;

            using var host = new WindowHost(view);
            host.Window.UpdateLayout();

            var banner = (Border)view.FindName("TaskNavigationSourceBanner")!;
            var clear = FindButton(banner, "清除带入的任务筛选");
            Assert.Equal(Visibility.Visible, banner.Visibility);
            var sourceText = (TextBlock)view.FindName("TaskNavigationSourceText")!;
            Assert.Contains("合成游戏", sourceText.Text, StringComparison.Ordinal);
            Assert.True(clear.Command!.CanExecute(null));

            clear.Command.Execute(null);

            Assert.Equal(1, binding.ClearCount);
            Assert.Equal("用户正在编辑的搜索草稿", binding.TaskSearchText);
            Assert.Equal("失败", binding.TaskStatusFilter);

            binding.HasTaskNavigationTarget = false;
            binding.NotifyNavigationVisibility();
            host.Window.UpdateLayout();
            Assert.Equal(Visibility.Collapsed, banner.Visibility);
            Assert.False(clear.Command.CanExecute(null));
        });
    }

    [Fact]
    public void ProductionClearButtonRemovesOnlyNavigationFilterAndCancelsActivePageRequest()
    {
        RunSta(() =>
        {
            var dashboard = (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
            var tasks = new BatchObservableCollection<TaskStatusDto>
            {
                new()
                {
                    TaskId = "source-task",
                    TaskType = "Backup",
                    GameId = "game-source",
                    GameName = "合成来源游戏",
                    State = TaskState.Failed,
                    Message = "存档备份失败"
                },
                new()
                {
                    TaskId = "other-task",
                    TaskType = "Backup",
                    GameId = "game-other",
                    GameName = "合成其他游戏",
                    State = TaskState.Failed,
                    Message = "存档备份失败"
                }
            };

            SetAutoPropertyBackingField(dashboard, nameof(DashboardViewModel.Tasks), tasks);
            SetField(dashboard, "taskSearchText", "存档");
            SetField(dashboard, "taskStatusFilter", "失败");
            SetField(dashboard, "taskGameFilter", "全部");
            SetField(dashboard, "taskTypeFilter", "存档备份");
            SetField(dashboard, "taskHistoryScope", "最近任务");
            SetField(dashboard, "taskHistoryRange", "全部时间");
            SetField(dashboard, "taskNavigationGameId", "game-source");
            SetField(dashboard, "taskNavigationGameName", "合成来源游戏");
            SetField(dashboard, "taskHistoryActive", false);

            var taskView = CollectionViewSource.GetDefaultView(tasks);
            var filterTask = typeof(DashboardViewModel).GetMethod("FilterTask", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(filterTask);
            taskView.Filter = item => (bool)filterTask!.Invoke(dashboard, new[] { item })!;
            SetAutoPropertyBackingField(dashboard, nameof(DashboardViewModel.TasksView), taskView);

            var pageRequests = new LatestRequestCoordinator();
            SetField(dashboard, "taskPageRequests", pageRequests);
            var activeRequest = pageRequests.Begin();

            var initializeNavigation = typeof(DashboardViewModel).GetMethod("OnNavigationStateInitialize", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(initializeNavigation);
            initializeNavigation!.Invoke(dashboard, null);

            var view = new TaskCenterView();
            var loadedIntegration = typeof(TaskCenterView).GetMethod(
                "OnLoaded",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            Assert.NotNull(loadedIntegration);
            view.Loaded -= (RoutedEventHandler)Delegate.CreateDelegate(typeof(RoutedEventHandler), view, loadedIntegration!);
            view.DataContext = dashboard;
            using var host = new WindowHost(view);
            host.Window.UpdateLayout();

            var banner = (Border)view.FindName("TaskNavigationSourceBanner")!;
            var sourceText = (TextBlock)view.FindName("TaskNavigationSourceText")!;
            var clear = FindButton(banner, "清除带入的任务筛选");
            Assert.Equal(Visibility.Visible, banner.Visibility);
            Assert.Contains("合成来源游戏", sourceText.Text, StringComparison.Ordinal);
            Assert.Same(dashboard.ClearTaskNavigationContextCommand, clear.Command);
            Assert.True(clear.Command!.CanExecute(null));
            Assert.Equal(new[] { "source-task" }, taskView.Cast<TaskStatusDto>().Select(task => task.TaskId));

            clear.Command.Execute(null);
            host.Window.UpdateLayout();

            Assert.True(activeRequest.Token.IsCancellationRequested);
            Assert.False(dashboard.HasTaskNavigationTarget);
            Assert.Equal(string.Empty, dashboard.TaskNavigationSourceSummary);
            Assert.Equal(Visibility.Collapsed, banner.Visibility);
            Assert.False(clear.Command.CanExecute(null));
            Assert.Equal(new[] { "source-task", "other-task" }, taskView.Cast<TaskStatusDto>().Select(task => task.TaskId));
            Assert.Equal("存档", dashboard.TaskSearchText);
            Assert.Equal("失败", dashboard.TaskStatusFilter);
            Assert.Equal("全部", dashboard.TaskGameFilter);
            Assert.Equal("存档备份", dashboard.TaskTypeFilter);
            Assert.Equal("最近任务", dashboard.TaskHistoryScope);
            Assert.Equal("全部时间", dashboard.TaskHistoryRange);

            pageRequests.Dispose();
        });
    }

    [Fact]
    public void SourceClearImplementationTouchesOnlyTransientNavigationFields()
    {
        var root = TestRepositoryContext.Root;
        var implementation = System.IO.File.ReadAllText(System.IO.Path.Combine(
            root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.Navigation.cs"));
        var xaml = System.IO.File.ReadAllText(System.IO.Path.Combine(
            root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));
        var start = implementation.IndexOf("private void ClearTaskNavigationContext", StringComparison.Ordinal);
        var end = implementation.IndexOf("internal void SetTaskGridScrollOffset", start, StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start);

        var clearBody = implementation.Substring(start, end - start);
        Assert.Contains("taskNavigationGameId = string.Empty", clearBody, StringComparison.Ordinal);
        Assert.Contains("taskNavigationGameName = string.Empty", clearBody, StringComparison.Ordinal);
        Assert.DoesNotContain("taskSearchText =", clearBody, StringComparison.Ordinal);
        Assert.DoesNotContain("taskStatusFilter =", clearBody, StringComparison.Ordinal);
        Assert.DoesNotContain("taskGameFilter =", clearBody, StringComparison.Ordinal);
        Assert.DoesNotContain("taskTypeFilter =", clearBody, StringComparison.Ordinal);
        Assert.Contains("ClearTaskNavigationContextCommand", xaml, StringComparison.Ordinal);
        Assert.Contains("仅清除带入的游戏条件，保留其他任务筛选", xaml, StringComparison.Ordinal);
    }

    private static Button FindButton(DependencyObject root, string automationName)
    {
        for (var index = 0; index < System.Windows.Media.VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(root, index);
            if (child is Button button
                && string.Equals(AutomationProperties.GetName(button), automationName, StringComparison.Ordinal))
                return button;
            var nested = FindButtonOrNull(child, automationName);
            if (nested != null) return nested;
        }

        throw new InvalidOperationException($"未找到按钮：{automationName}");
    }

    private static Button? FindButtonOrNull(DependencyObject root, string automationName)
    {
        if (root is Button button
            && string.Equals(AutomationProperties.GetName(button), automationName, StringComparison.Ordinal))
            return button;
        for (var index = 0; index < System.Windows.Media.VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var nested = FindButtonOrNull(System.Windows.Media.VisualTreeHelper.GetChild(root, index), automationName);
            if (nested != null) return nested;
        }

        return null;
    }

    private static void SetAutoPropertyBackingField(object target, string propertyName, object value)
    {
        var field = typeof(DashboardViewModel).GetField($"<{propertyName}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        field!.SetValue(target, value);
    }

    private static void SetField(object target, string fieldName, object value)
    {
        var field = typeof(DashboardViewModel).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        field!.SetValue(target, value);
    }

    private sealed class TestNavigationBinding : INotifyPropertyChanged
    {
        public bool HasTaskNavigationTarget { get; set; }
        public string TaskNavigationSourceSummary { get; set; } = string.Empty;
        public string TaskSearchText { get; set; } = "用户正在编辑的搜索草稿";
        public string TaskStatusFilter { get; set; } = "失败";
        public string TaskGameFilter { get; set; } = "全部";
        public string TaskTypeFilter { get; set; } = "全部";
        public string TaskHistoryScope { get; set; } = "最近任务";
        public string TaskHistoryRange { get; set; } = "全部时间";
        public int ClearCount { get; private set; }
        public ICommand ClearTaskNavigationContextCommand { get; }

        public TestNavigationBinding()
        {
            ClearTaskNavigationContextCommand = new RelayCommand(_ =>
            {
                ClearCount++;
                HasTaskNavigationTarget = false;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasTaskNavigationTarget)));
            }, _ => HasTaskNavigationTarget);
        }

        public void NotifyNavigationVisibility()
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasTaskNavigationTarget)));

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private sealed class WindowHost : IDisposable
    {
        public WindowHost(FrameworkElement content)
        {
            Window = new Window
            {
                Content = content,
                Width = 1000,
                Height = 720,
                ShowInTaskbar = false,
                ShowActivated = false,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };
            Window.Show();
            Window.UpdateLayout();
        }

        public Window Window { get; }

        public void Dispose()
        {
            Window.Close();
            Dispatcher.CurrentDispatcher.InvokeShutdown();
        }
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception exception) { failure = exception; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure != null) throw new Xunit.Sdk.XunitException(failure.ToString());
    }
}
