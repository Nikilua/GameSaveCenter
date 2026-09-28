using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;
using Xunit.Abstractions;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R08PageSwitchBehaviorTests
{
    private readonly ITestOutputHelper output;

    public R08PageSwitchBehaviorTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void CachedProductionPagesPreserveBindingSelectionAndScrollAcrossNavigation()
    {
        Exception? exception = null;
        var samples = new List<NavigationSample>();

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var shell = new AcrylicProductionShellView();
                typeof(AcrylicProductionShellView)
                    .GetMethod("CreatePages", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(shell, null);

                var viewModel = (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
                typeof(DashboardViewModel)
                    .GetField("gamePicker", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .SetValue(viewModel, new GamePickerViewModel());
                typeof(DashboardViewModel)
                    .GetField("navigationHistory", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .SetValue(viewModel, new WorkspaceNavigationStack());
                typeof(AcrylicProductionShellView)
                    .GetField("viewModel", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .SetValue(shell, viewModel);

                // Keep page construction real while preventing an uninitialized VM
                // from starting any business subscriptions in the page Loaded hooks.
                foreach (var page in shell.WorkspaceViews)
                    page.DataContext = new object();

                var taskPage = shell.GetWorkspaceView<TaskCenterView>(WorkspaceKind.Tasks)!;
                var taskGrid = (DataGrid)typeof(TaskCenterView)
                    .GetField("TaskGrid", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(taskPage)!;
                var tasks = Enumerable.Range(0, 96)
                    .Select(index => new TaskStatusDto
                    {
                        TaskId = "r08-05-task-" + index,
                        TaskType = "Validation",
                        GameName = "合成游戏 " + index,
                        State = TaskState.Succeeded,
                        Message = "合成任务完成"
                    })
                    .ToList();
                taskGrid.ItemsSource = tasks;

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

                shell.NavigateTo(WorkspaceKind.Tasks);
                window.Show();
                FlushLayout(window);
                taskGrid.SelectedItem = tasks[55];
                FlushLayout(window);

                var gridScrollViewer = FindVisualChild<ScrollViewer>(taskGrid);
                if (gridScrollViewer == null || gridScrollViewer.ScrollableHeight <= 0)
                    throw new InvalidOperationException("The synthetic task grid did not expose a finite scroll viewport.");

                var targetOffset = Math.Min(10d, gridScrollViewer.ScrollableHeight);
                gridScrollViewer.ScrollToVerticalOffset(targetOffset);
                FlushLayout(window);
                var offsetBefore = gridScrollViewer.VerticalOffset;
                var selectedBefore = taskGrid.SelectedItem;
                var dataContextBefore = taskPage.DataContext;
                var taskPageReference = taskPage;
                var measureBeforeSwitch = shell.MeasurePassesForAudit;
                var arrangeBeforeSwitch = shell.ArrangePassesForAudit;

                shell.NavigateTo(WorkspaceKind.Media);
                FlushLayout(window);
                var measureAfterMedia = shell.MeasurePassesForAudit;
                var arrangeAfterMedia = shell.ArrangePassesForAudit;

                shell.NavigateTo(WorkspaceKind.Tasks);
                FlushLayout(window);
                var measureAfterTasks = shell.MeasurePassesForAudit;
                var arrangeAfterTasks = shell.ArrangePassesForAudit;
                var offsetAfter = gridScrollViewer.VerticalOffset;

                Assert.Same(taskPageReference, shell.GetWorkspaceView<TaskCenterView>(WorkspaceKind.Tasks));
                Assert.Same(taskPageReference, ((ContentControl)shell.PageHostForAudit).Content);
                Assert.Same(dataContextBefore, taskPage.DataContext);
                Assert.Same(tasks, taskGrid.ItemsSource);
                Assert.Same(selectedBefore, taskGrid.SelectedItem);
                Assert.InRange(Math.Abs(offsetAfter - offsetBefore), 0, 0.1);
                Assert.True(measureAfterMedia > measureBeforeSwitch, "Replacing the active page should cause a measurable host layout pass.");
                Assert.True(arrangeAfterMedia > arrangeBeforeSwitch, "Replacing the active page should cause a measurable host arrange pass.");
                Assert.True(measureAfterTasks > measureAfterMedia, "Returning to the cached page should complete a measurable host layout pass.");
                Assert.True(arrangeAfterTasks > arrangeAfterMedia, "Returning to the cached page should complete a measurable host arrange pass.");

                var measureBeforeSamePage = shell.MeasurePassesForAudit;
                var arrangeBeforeSamePage = shell.ArrangePassesForAudit;
                shell.NavigateTo(WorkspaceKind.Tasks);
                FlushLayout(window);
                Assert.Equal(measureBeforeSamePage, shell.MeasurePassesForAudit);
                Assert.Equal(arrangeBeforeSamePage, shell.ArrangePassesForAudit);

                var pageHost = (FrameworkElement)shell.PageHostForAudit;
                Assert.Equal(1, pageHost.Opacity);
                Assert.Null(pageHost.Effect);
                Assert.True(pageHost.RenderTransform == null || pageHost.RenderTransform == Transform.Identity);

                samples.Add(new NavigationSample(
                    offsetBefore,
                    offsetAfter,
                    measureAfterMedia - measureBeforeSwitch,
                    arrangeAfterMedia - arrangeBeforeSwitch,
                    measureAfterTasks - measureAfterMedia,
                    arrangeAfterTasks - arrangeAfterMedia,
                    taskPageReference == shell.GetWorkspaceView<TaskCenterView>(WorkspaceKind.Tasks),
                    ReferenceEquals(selectedBefore, taskGrid.SelectedItem)));
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
        Assert.Single(samples);
        output.WriteLine(samples[0].ToString());
    }

    [Fact]
    public void CachedWorkspacePagesRestoreTabsAndTaskFiltersWithoutRefreshing()
    {
        Exception? exception = null;
        var summary = string.Empty;
        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var shell = new AcrylicProductionShellView();
                typeof(AcrylicProductionShellView)
                    .GetMethod("CreatePages", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(shell, null);

                var dashboard = (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
                typeof(DashboardViewModel)
                    .GetField("gamePicker", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .SetValue(dashboard, new GamePickerViewModel());
                typeof(DashboardViewModel)
                    .GetField("navigationHistory", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .SetValue(dashboard, new WorkspaceNavigationStack());
                typeof(AcrylicProductionShellView)
                    .GetField("viewModel", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .SetValue(shell, dashboard);

                // Bind production controls to a state-only fixture. No worker, repository,
                // Playnite profile, or query service is attached to these synthetic pages.
                var state = new NavigationViewState();
                foreach (var page in shell.WorkspaceViews)
                    page.DataContext = state;

                var taskPage = shell.GetWorkspaceView<TaskCenterView>(WorkspaceKind.Tasks)!;
                var mediaPage = shell.GetWorkspaceView<MediaCenterView>(WorkspaceKind.Media)!;
                var savePage = shell.GetWorkspaceView<SaveCenterView>(WorkspaceKind.Saves)!;
                var maintenancePage = shell.GetWorkspaceView<MaintenanceView>(WorkspaceKind.Maintenance)!;
                var taskGrid = (DataGrid)typeof(TaskCenterView)
                    .GetField("TaskGrid", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(taskPage)!;
                var tasks = Enumerable.Range(0, 96)
                    .Select(index => new TaskStatusDto
                    {
                        TaskId = "q11-07-task-" + index,
                        TaskType = "Validation",
                        GameName = "合成游戏 " + index,
                        State = TaskState.Failed,
                        Message = "保留筛选和选择"
                    })
                    .ToList();
                taskGrid.ItemsSource = tasks;

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

                shell.NavigateTo(WorkspaceKind.Tasks);
                window.Show();
                FlushLayout(window);

                var search = (TextBox)typeof(TaskCenterView)
                    .GetField("TaskSearchTextBox", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(taskPage)!;
                var status = (ComboBox)typeof(TaskCenterView)
                    .GetField("TaskStatusFilterComboBox", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(taskPage)!;
                var type = (ComboBox)typeof(TaskCenterView)
                    .GetField("TaskTypeFilterComboBox", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(taskPage)!;
                var scope = (ComboBox)typeof(TaskCenterView)
                    .GetField("TaskHistoryScopeComboBox", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(taskPage)!;
                var range = (ComboBox)typeof(TaskCenterView)
                    .GetField("TaskHistoryRangeComboBox", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(taskPage)!;

                Assert.Equal(state.TaskSearchText, search.Text);
                Assert.Equal(state.TaskStatusFilter, status.SelectedItem);
                Assert.Equal(state.TaskTypeFilter, type.SelectedItem);
                Assert.Equal(state.TaskHistoryScope, scope.SelectedItem);
                Assert.Equal(state.TaskHistoryRange, range.SelectedItem);

                search.Text = "失败保留";
                status.SelectedItem = "失败";
                type.SelectedItem = "媒体归类";
                scope.SelectedItem = "全部历史";
                range.SelectedItem = "近30天";
                FlushLayout(window);
                Assert.Equal("失败保留", state.TaskSearchText);
                Assert.Equal("失败", state.TaskStatusFilter);
                Assert.Equal("媒体归类", state.TaskTypeFilter);
                Assert.Equal("全部历史", state.TaskHistoryScope);
                Assert.Equal("近30天", state.TaskHistoryRange);

                taskGrid.SelectedItem = tasks[55];
                FlushLayout(window);
                var taskScrollViewer = FindVisualChild<ScrollViewer>(taskGrid)
                    ?? throw new InvalidOperationException("The synthetic task grid did not expose its scroll viewer.");
                if (taskScrollViewer.ScrollableHeight <= 0)
                    throw new InvalidOperationException("The synthetic task grid did not expose a finite scroll viewport.");
                taskScrollViewer.ScrollToVerticalOffset(Math.Min(18d, taskScrollViewer.ScrollableHeight));
                FlushLayout(window);
                var taskOffsetBefore = taskScrollViewer.VerticalOffset;
                var taskSelectionBefore = taskGrid.SelectedItem;
                var taskItemsBefore = taskGrid.ItemsSource;
                var taskPageBefore = taskPage;
                var mediaPageBefore = mediaPage;
                var savePageBefore = savePage;
                var maintenancePageBefore = maintenancePage;
                var taskDataContextBefore = taskPage.DataContext;

                shell.NavigateTo(WorkspaceKind.Media);
                FlushLayout(window);
                var mediaTabs = (TabControl)typeof(MediaCenterView)
                    .GetField("MediaTabControl", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(mediaPage)!;
                mediaTabs.SelectedIndex = 2;
                FlushLayout(window);
                Assert.Equal(state.MediaTabIndex, mediaTabs.SelectedIndex);
                Assert.Equal(2, state.MediaTabIndex);

                shell.NavigateTo(WorkspaceKind.Saves);
                FlushLayout(window);
                var saveTabs = FindVisualChild<TabControl>(savePage)
                    ?? throw new InvalidOperationException("The production save page did not expose its workspace tabs.");
                saveTabs.SelectedIndex = 3;
                FlushLayout(window);
                Assert.Equal(state.SaveTabIndex, saveTabs.SelectedIndex);
                Assert.Equal(3, state.SaveTabIndex);

                shell.NavigateTo(WorkspaceKind.Maintenance);
                FlushLayout(window);
                var maintenanceTabs = (TabControl)typeof(MaintenanceView)
                    .GetField("MaintenanceTabControl", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(maintenancePage)!;
                maintenanceTabs.SelectedIndex = 5;
                FlushLayout(window);
                Assert.Equal(state.MaintenanceTabIndex, maintenanceTabs.SelectedIndex);
                Assert.Equal(5, state.MaintenanceTabIndex);

                shell.NavigateTo(WorkspaceKind.Overview);
                FlushLayout(window);
                shell.NavigateTo(WorkspaceKind.Tasks);
                FlushLayout(window);

                Assert.Same(taskPageBefore, shell.GetWorkspaceView<TaskCenterView>(WorkspaceKind.Tasks));
                Assert.Same(taskDataContextBefore, taskPage.DataContext);
                Assert.Same(taskItemsBefore, taskGrid.ItemsSource);
                Assert.Same(taskSelectionBefore, taskGrid.SelectedItem);
                Assert.InRange(Math.Abs(taskOffsetBefore - taskScrollViewer.VerticalOffset), 0, 0.1);
                Assert.Equal("失败", status.SelectedItem);
                Assert.Equal("媒体归类", type.SelectedItem);
                Assert.Equal("全部历史", scope.SelectedItem);
                Assert.Equal("近30天", range.SelectedItem);
                Assert.Equal("失败保留", search.Text);

                shell.NavigateTo(WorkspaceKind.Media);
                FlushLayout(window);
                Assert.Same(mediaPageBefore, shell.GetWorkspaceView<MediaCenterView>(WorkspaceKind.Media));
                Assert.Equal(state.MediaTabIndex, mediaTabs.SelectedIndex);

                shell.NavigateTo(WorkspaceKind.Saves);
                FlushLayout(window);
                Assert.Same(savePageBefore, shell.GetWorkspaceView<SaveCenterView>(WorkspaceKind.Saves));
                Assert.Same(saveTabs, FindVisualChild<TabControl>(savePage));
                Assert.Equal(state.SaveTabIndex, saveTabs.SelectedIndex);

                shell.NavigateTo(WorkspaceKind.Maintenance);
                FlushLayout(window);
                Assert.Same(maintenancePageBefore, shell.GetWorkspaceView<MaintenanceView>(WorkspaceKind.Maintenance));
                Assert.Equal(state.MaintenanceTabIndex, maintenanceTabs.SelectedIndex);
                Assert.Same(dashboard, typeof(AcrylicProductionShellView)
                    .GetField("viewModel", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(shell));
                Assert.Equal(0, state.RefreshCommand.ExecutionCount);
                Assert.Equal(96, taskGrid.Items.Count);

                summary = $"task filters retained; tabs media/save/maintenance={mediaTabs.SelectedIndex}/{saveTabs.SelectedIndex}/{maintenanceTabs.SelectedIndex}; "
                    + $"task selection={((TaskStatusDto)taskGrid.SelectedItem).TaskId}; scroll={taskOffsetBefore:0.##}->{taskScrollViewer.VerticalOffset:0.##}; "
                    + $"cached pages retained; refresh executions={state.RefreshCommand.ExecutionCount}";
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
        Assert.False(string.IsNullOrWhiteSpace(summary));
        output.WriteLine(summary);
    }

    [Fact]
    public void RapidNavigationUpdatesPageTitleSelectionAndKeepsFocusOnTheChosenNavigationItem()
    {
        Exception? exception = null;
        var observation = string.Empty;

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var shell = new AcrylicProductionShellView();
                typeof(AcrylicProductionShellView)
                    .GetMethod("CreatePages", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(shell, null);

                var plugin = (GameSaveCenterPlugin)FormatterServices.GetUninitializedObject(typeof(GameSaveCenterPlugin));
                var dashboard = (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
                typeof(DashboardViewModel)
                    .GetField("plugin", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .SetValue(dashboard, plugin);
                typeof(DashboardViewModel)
                    .GetField("gamePicker", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .SetValue(dashboard, new GamePickerViewModel());
                typeof(DashboardViewModel)
                    .GetField("navigationHistory", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .SetValue(dashboard, new WorkspaceNavigationStack());
                typeof(AcrylicProductionShellView)
                    .GetField("viewModel", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .SetValue(shell, dashboard);

                // Production views remain real, but state-only DataContexts prevent Loaded
                // hooks from attaching a Worker/repository or issuing business queries.
                foreach (var page in shell.WorkspaceViews)
                    page.DataContext = new object();

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

                shell.NavigateTo(WorkspaceKind.Overview);
                window.Show();
                FlushLayout(window);

                var overviewNavigation = (RadioButton)shell.FindName("NavOverview")!;
                var tasksNavigation = (RadioButton)shell.FindName("NavTasks")!;
                var navigationItems = new[]
                {
                    overviewNavigation,
                    (RadioButton)shell.FindName("NavSaves")!,
                    (RadioButton)shell.FindName("NavTrainers")!,
                    (RadioButton)shell.FindName("NavMedia")!,
                    tasksNavigation,
                    (RadioButton)shell.FindName("NavMaintenance")!
                };
                var pageHost = (ContentControl)shell.PageHostForAudit;
                var title = (TextBlock)shell.FindName("PageTitleText")!;
                var route = new[]
                {
                    (WorkspaceKind.Tasks, tasksNavigation, "任务中心"),
                    (WorkspaceKind.Overview, overviewNavigation, "首页"),
                    (WorkspaceKind.Tasks, tasksNavigation, "任务中心"),
                    (WorkspaceKind.Overview, overviewNavigation, "首页"),
                    (WorkspaceKind.Tasks, tasksNavigation, "任务中心"),
                    (WorkspaceKind.Overview, overviewNavigation, "首页"),
                    (WorkspaceKind.Tasks, tasksNavigation, "任务中心")
                };

                var completedRoutes = 0;
                foreach (var (workspace, navigation, expectedTitle) in route)
                {
                    navigation.Focus();
                    Keyboard.Focus(navigation);
                    if (!ReferenceEquals(Keyboard.FocusedElement, navigation))
                        throw new InvalidOperationException($"Could not establish keyboard focus on {workspace} navigation.");

                    // Changing IsChecked raises the real RadioButton Checked route and
                    // reaches production OnNavChecked; only Overview/Tasks are used so
                    // the isolated fixture cannot start game-scoped loads.
                    navigation.IsChecked = true;

                    Assert.Equal(workspace, dashboard.CurrentWorkspace);
                    Assert.Same(shell.GetWorkspaceView(workspace), pageHost.Content);
                    Assert.Equal(expectedTitle, title.Text);
                    Assert.Single(navigationItems, item => item.IsChecked == true);
                    Assert.True(navigation.IsChecked);
                    Assert.Same(navigation, Keyboard.FocusedElement);
                    Assert.Equal(1, pageHost.Opacity);
                    Assert.Null(pageHost.Effect);
                    Assert.True(pageHost.RenderTransform == null || pageHost.RenderTransform == Transform.Identity);
                    completedRoutes++;
                }

                FlushLayout(window);
                Assert.Same(shell.GetWorkspaceView(WorkspaceKind.Tasks), pageHost.Content);
                Assert.Equal("任务中心", title.Text);
                Assert.Same(tasksNavigation, Keyboard.FocusedElement);
                observation = $"routes={completedRoutes}; final={dashboard.CurrentWorkspace}; title={title.Text}; focus=NavTasks; pageEffects=none";
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
        Assert.False(string.IsNullOrWhiteSpace(observation));
        output.WriteLine(observation);
    }

    [Fact]
    public void ProductionPageHostDoesNotDeclareFullPageBlurOrEntranceAnimation()
    {
        var root = TestRepositoryContext.Root;
        var source = System.IO.File.ReadAllText(System.IO.Path.Combine(
            root, "src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml.cs"));
        var markup = System.IO.File.ReadAllText(System.IO.Path.Combine(
            root, "src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml"));

        Assert.Contains("var contentChanged = !ReferenceEquals(PageHost.Content, page);", source);
        Assert.Contains("if (contentChanged)", source);
        Assert.Contains("PageHost.Content = page;", source);
        Assert.DoesNotContain("AnimateEntrance(PageHost", source);
        Assert.DoesNotContain("BlurEffect", markup);
        Assert.DoesNotContain("PageHost.Effect", source);
    }

    private static void FlushLayout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
        window.UpdateLayout();
    }

    private static T? FindVisualChild<T>(DependencyObject root) where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match) return match;
            var descendant = FindVisualChild<T>(child);
            if (descendant != null) return descendant;
        }
        return null;
    }

    private sealed class NavigationSample
    {
        public NavigationSample(
            double offsetBefore,
            double offsetAfter,
            int measureToMedia,
            int arrangeToMedia,
            int measureBack,
            int arrangeBack,
            bool cachedPage,
            bool selectionPreserved)
        {
            OffsetBefore = offsetBefore;
            OffsetAfter = offsetAfter;
            MeasureToMedia = measureToMedia;
            ArrangeToMedia = arrangeToMedia;
            MeasureBack = measureBack;
            ArrangeBack = arrangeBack;
            CachedPage = cachedPage;
            SelectionPreserved = selectionPreserved;
        }

        public double OffsetBefore { get; }
        public double OffsetAfter { get; }
        public int MeasureToMedia { get; }
        public int ArrangeToMedia { get; }
        public int MeasureBack { get; }
        public int ArrangeBack { get; }
        public bool CachedPage { get; }
        public bool SelectionPreserved { get; }

        public override string ToString()
            => $"offset={OffsetBefore:0.###}->{OffsetAfter:0.###}; "
                + $"to-media measure/arrange={MeasureToMedia}/{ArrangeToMedia}; "
                + $"back measure/arrange={MeasureBack}/{ArrangeBack}; "
                + $"cached={CachedPage}; selection={SelectionPreserved}";
    }

    private sealed class NavigationViewState
    {
        public string TaskSearchText { get; set; } = "初始搜索";
        public string TaskStatusFilter { get; set; } = "全部";
        public string TaskTypeFilter { get; set; } = "全部";
        public string TaskHistoryScope { get; set; } = "最近任务";
        public string TaskHistoryRange { get; set; } = "全部时间";
        public int MediaTabIndex { get; set; }
        public int SaveTabIndex { get; set; }
        public int MaintenanceTabIndex { get; set; }
        public ExecutionCountCommand RefreshCommand { get; } = new ExecutionCountCommand();
        public ObservableCollection<string> TaskStatusFilterOptions { get; } = new ObservableCollection<string> { "全部", "失败", "成功" };
        public ObservableCollection<string> TaskTypeFilterOptions { get; } = new ObservableCollection<string> { "全部", "媒体归类", "Validation" };
        public ObservableCollection<string> TaskHistoryScopeOptions { get; } = new ObservableCollection<string> { "最近任务", "全部历史" };
        public ObservableCollection<string> TaskHistoryRangeOptions { get; } = new ObservableCollection<string> { "全部时间", "近30天" };
    }

    private sealed class ExecutionCountCommand : ICommand
    {
        public int ExecutionCount { get; private set; }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => ExecutionCount++;
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
