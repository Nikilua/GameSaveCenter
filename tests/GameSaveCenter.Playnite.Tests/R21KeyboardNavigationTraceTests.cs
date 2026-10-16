using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.Views;
using Xunit;
using Xunit.Abstractions;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R21KeyboardNavigationTraceTests
{
    private readonly ITestOutputHelper _output;

    public R21KeyboardNavigationTraceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void EightProductionEntrancesRecordForwardAndReverseKeyboardFocusTraces()
    {
        var traces = new List<EntryTrace>();

        RunSta(() =>
        {
            EnsureHostTextBlockStyle();
            foreach (var entry in CreateEntries())
            {
                using var host = new TraceHost(entry.Create());
                traces.Add(TraceEntry(entry, host));
            }
        });

        Assert.Equal(8, traces.Count);
        Assert.All(traces, trace =>
        {
            Assert.True(trace.Forward.Count >= 2, trace.ToFailureMessage());
            Assert.True(trace.Reverse.Count >= 2, trace.ToFailureMessage());
            Assert.True(trace.Forward.Any(stop => trace.ExpectedNames.Contains(stop.AutomationName, StringComparer.Ordinal)), trace.ToFailureMessage());
            Assert.True(trace.Reverse.All(stop => stop.IsInsideEntry), trace.ToFailureMessage());
            Assert.True(trace.Forward.All(stop => stop.IsInsideEntry), trace.ToFailureMessage());

            foreach (var target in trace.VisibleInteractiveTargets)
            {
                Assert.True(
                    IsTargetReached(target, trace.Forward),
                    trace.Name + " forward Tab cannot reach " + target.Description + "; " + trace.ToFailureMessage());
                Assert.True(
                    IsTargetReached(target, trace.Reverse),
                    trace.Name + " reverse Shift+Tab cannot reach " + target.Description + "; " + trace.ToFailureMessage());
            }

            var forwardTargets = GetInteractiveTrace(trace, trace.Forward);
            var reverseTargets = GetInteractiveTrace(trace, trace.Reverse);
            var pivotIndex = forwardTargets.FindIndex(target => ReferenceEquals(target.Target, trace.Forward[0].Target));
            var expectedReverse = pivotIndex >= 0
                ? new[] { forwardTargets[pivotIndex] }.Concat(forwardTargets.Skip(pivotIndex + 1).Reverse()).ToList()
                : forwardTargets.AsEnumerable().Reverse().ToList();
            Assert.True(
                expectedReverse.Select(target => target.Target).SequenceEqual(reverseTargets.Select(target => target.Target)),
                trace.Name + " Shift+Tab order is not the reverse of Tab order; " + trace.ToFailureMessage());
        });

        foreach (var trace in traces)
            _output.WriteLine(trace.ToFailureMessage());
    }

    private static List<InteractiveTarget> GetInteractiveTrace(EntryTrace trace, IEnumerable<FocusStop> stops)
    {
        var result = new List<InteractiveTarget>();
        foreach (var stop in stops)
        {
            var target = trace.VisibleInteractiveTargets.FirstOrDefault(candidate => ReferenceEquals(candidate.Target, stop.Target));
            if (target == null
                && stop.DataGrid is { } owner
                && trace.VisibleInteractiveTargets.FirstOrDefault(candidate => ReferenceEquals(candidate.Target, owner)) is { } dataGrid)
                target = dataGrid;
            if (target != null && !result.Any(existing => ReferenceEquals(existing.Target, target.Target)))
                result.Add(target);
        }

        return result;
    }

    private static bool IsTargetReached(InteractiveTarget target, IEnumerable<FocusStop> stops)
        => stops.Any(stop => ReferenceEquals(stop.Target, target.Target)
            || (target.Target is DataGrid && ReferenceEquals(stop.DataGrid, target.Target)));

    private static void EnsureHostTextBlockStyle()
    {
        var application = Application.Current ?? new Application();
        application.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        if (application.Resources["BaseTextBlockStyle"] == null)
            application.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
    }

    [Fact]
    public void ShellNavigationAndSettingsEntryHaveStableKeyboardAutomationNames()
    {
        RunSta(() =>
        {
            using var host = new TraceHost(new AcrylicProductionShellView());
            var shell = (AcrylicProductionShellView)host.Entry;
            var names = EnumerateFocusable(host.Scope)
                .Select(element => AutomationProperties.GetName(element))
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToHashSet(StringComparer.Ordinal);

            foreach (var name in new[]
            {
                "打开首页",
                "打开存档中心",
                "打开修改器中心",
                "打开媒体中心",
                "打开任务中心",
                "打开维护中心",
                "打开 GameSaveCenter 设置"
            })
                Assert.Contains(name, names);

            var navigation = new[]
            {
                (RadioButton)shell.FindName("NavOverview")!,
                (RadioButton)shell.FindName("NavSaves")!,
                (RadioButton)shell.FindName("NavTrainers")!,
                (RadioButton)shell.FindName("NavMedia")!,
                (RadioButton)shell.FindName("NavTasks")!,
                (RadioButton)shell.FindName("NavMaintenance")!,
                (RadioButton)shell.FindName("NavSettings")!
            };

            Assert.All(navigation, item =>
            {
                Assert.True(item.Focusable);
                Assert.True(KeyboardNavigation.GetIsTabStop(item));
                Assert.True(item.IsVisible);
                Assert.True(item.IsEnabled);
            });
        });
    }

    [Fact]
    public void SelectedFailedTaskKeepsFiltersTableDetailsAndFooterInKeyboardOrder()
    {
        EntryTrace? trace = null;
        TaskStatusDto? selectedTask = null;
        TaskStatusDto? selectedGridItem = null;
        var visibleRowCount = 0;
        var detailText = string.Empty;
        var taskGridHadKeyboardFocus = false;
        var selectedCellWasReached = false;
        var selectedCellWasReachedReverse = false;
        var retryActionEnabled = false;
        var cancelActionEnabled = true;
        var retryCommandStayedBound = false;
        var loadMoreCommandStayedBound = false;
        var loadMoreIsVisible = false;
        var loadMoreReachedForward = false;
        var loadMoreReachedReverse = false;
        var loadMoreTraceIndex = -1;
        var taskGridTraceIndex = -1;
        var focusStops = string.Empty;

        RunSta(() =>
        {
            var succeeded = new TaskStatusDto
            {
                TaskId = "keyboard-success",
                TaskType = "Backup",
                GameId = "synthetic-game-1",
                GameName = "成功任务样例",
                State = TaskState.Succeeded,
                ProgressPercent = 100,
                CreatedUtc = DateTime.UtcNow.AddMinutes(-3),
                Message = "合成备份完成"
            };
            var running = new TaskStatusDto
            {
                TaskId = "keyboard-running",
                TaskType = "MediaSync",
                GameId = "synthetic-game-2",
                GameName = "运行任务样例",
                State = TaskState.Running,
                ProgressPercent = 42,
                StageMessage = "正在执行",
                CreatedUtc = DateTime.UtcNow.AddMinutes(-2),
                StartedUtc = DateTime.UtcNow.AddMinutes(-1),
                Message = "合成任务仍在运行"
            };
            var failed = new TaskStatusDto
            {
                TaskId = "keyboard-failed",
                TaskType = "Validation",
                GameId = "synthetic-game-3",
                GameName = "失败任务样例",
                State = TaskState.Failed,
                ProgressPercent = 64,
                CreatedUtc = DateTime.UtcNow.AddMinutes(-1),
                StartedUtc = DateTime.UtcNow.AddSeconds(-50),
                FinishedUtc = DateTime.UtcNow.AddSeconds(-5),
                ErrorCode = "E_KEYBOARD_SAMPLE",
                ErrorMessage = "仅用于任务中心焦点顺序复核的合成错误详情。"
            };
            var fixture = new TaskKeyboardFixture(new[] { succeeded, running, failed }, failed, hasMoreHistory: true);
            var view = new TaskCenterView { DataContext = fixture };
            using var host = new TraceHost(view, 1280, 760);
            view.ApplyResponsiveLayout(1280, 760);
            host.Pump();

            var grid = Assert.IsType<DataGrid>(view.FindName("TaskGrid"));
            var technicalDetails = Assert.IsType<Expander>(view.FindName("TaskTechnicalDetailsExpander"));
            grid.ScrollIntoView(failed);
            technicalDetails.IsExpanded = true;
            host.Pump();

            selectedTask = fixture.SelectedTask;
            selectedGridItem = grid.SelectedItem as TaskStatusDto;
            visibleRowCount = grid.Items.Count;
            var retryButton = FindNamedButton(view, "安全重试任务");
            var cancelButton = FindNamedButton(view, "取消任务");
            var loadMoreButton = FindTaskLoadMoreButton(view);
            retryActionEnabled = retryButton.IsEnabled;
            cancelActionEnabled = cancelButton.IsEnabled;
            retryCommandStayedBound = ReferenceEquals(retryButton.Command, fixture.RetryTaskCommand);
            loadMoreCommandStayedBound = ReferenceEquals(loadMoreButton.Command, fixture.LoadMoreTasksCommand);
            loadMoreIsVisible = loadMoreButton.IsVisible;
            detailText = string.Join(" ", EnumerateElements(view)
                .OfType<TextBlock>()
                .Select(text => text.Text));
            Keyboard.Focus(grid);
            taskGridHadKeyboardFocus = grid.IsKeyboardFocusWithin;

            var selectedRow = EnumerateElements(grid)
                .OfType<DataGridRow>()
                .FirstOrDefault(row => ReferenceEquals(row.Item, failed));
            var selectedCell = selectedRow == null
                ? null
                : EnumerateElements(selectedRow)
                    .OfType<DataGridCell>()
                    .FirstOrDefault(cell => cell.IsVisible && cell.ActualWidth > 0 && cell.ActualHeight > 0);

            var entry = new EntrySpec("TaskCenter failed selection", () => view, new[]
            {
                "搜索任务", "任务状态筛选", "任务列表", "任务可选择技术详情", "安全重试任务"
            });
            trace = TraceEntry(entry, host);
            loadMoreReachedForward = trace.Forward.Any(stop => ReferenceEquals(stop.Target, loadMoreButton));
            loadMoreReachedReverse = trace.Reverse.Any(stop => ReferenceEquals(stop.Target, loadMoreButton));
            if (!trace.VisibleInteractiveTargets.Any(target => ReferenceEquals(target.Target, grid)))
                trace.VisibleInteractiveTargets.Add(new InteractiveTarget(grid, "任务列表"));
            var forwardTargets = GetInteractiveTrace(trace, trace.Forward);
            loadMoreTraceIndex = forwardTargets.FindIndex(target => ReferenceEquals(target.Target, loadMoreButton));
            taskGridTraceIndex = forwardTargets.FindIndex(target => ReferenceEquals(target.Target, grid));
            selectedCellWasReached = selectedCell != null
                && trace.Forward.Any(stop => ReferenceEquals(stop.Target, selectedCell));
            selectedCellWasReachedReverse = selectedCell != null
                && trace.Reverse.Any(stop => ReferenceEquals(stop.Target, selectedCell));
            focusStops = string.Join(" > ", trace.Forward.Select(stop => stop.AutomationName + ":" + stop.TypeName));
        });

        Assert.NotNull(trace);
        Assert.Equal("keyboard-failed", selectedTask?.TaskId);
        Assert.Same(selectedTask, selectedGridItem);
        Assert.Equal(3, visibleRowCount);
        Assert.Contains("E_KEYBOARD_SAMPLE", detailText, StringComparison.Ordinal);
        Assert.True(taskGridHadKeyboardFocus, "the populated task grid must accept keyboard focus");
        Assert.True(retryActionEnabled, "the failed task keeps its retry action available");
        Assert.False(cancelActionEnabled, "a terminal failed task must not expose cancellation as an enabled action");
        Assert.True(retryCommandStayedBound, "the inspector retry button must retain its bound command");
        Assert.True(loadMoreIsVisible, "available task history must reveal the load-more action");
        Assert.True(loadMoreCommandStayedBound, "the visible load-more action must retain its bound command");
        Assert.True(loadMoreReachedForward, "Tab must reach the load-more action");
        Assert.True(loadMoreReachedReverse, "Shift+Tab must reach the load-more action");
        Assert.True(loadMoreTraceIndex >= 0 && taskGridTraceIndex > loadMoreTraceIndex,
            "the load-more action must precede the task grid in forward order");
        Assert.True(selectedCellWasReached, "Tab must reach a cell in the selected task row; stops=[" + focusStops + "]");
        Assert.True(selectedCellWasReachedReverse, "Shift+Tab must reach a cell in the selected task row; stops=[" + focusStops + "]");
        Assert.All(trace!.Forward.Concat(trace.Reverse), stop =>
            Assert.True(stop.IsInsideEntry, "focus escaped the TaskCenter host at " + stop.AutomationName));

        var forward = GetInteractiveTrace(trace!, trace!.Forward);
        var reverse = GetInteractiveTrace(trace!, trace!.Reverse);
        foreach (var target in trace.VisibleInteractiveTargets)
        {
            Assert.True(IsTargetReached(target, trace.Forward), "forward Tab cannot reach " + target.Description + "; " + trace.ToFailureMessage());
            Assert.True(IsTargetReached(target, trace.Reverse), "reverse Shift+Tab cannot reach " + target.Description + "; " + trace.ToFailureMessage());
        }
        var expectedSequence = new[]
        {
            "搜索任务",
            "任务状态筛选",
            "任务类型筛选",
            "任务时间范围",
            "任务列表",
            "任务可选择技术详情",
            "查看关联游戏详情",
            "复制任务摘要和错误码",
            "安全重试任务"
        };
        AssertInOrder(forward, expectedSequence, "forward Tab");

        var forwardTargets = forward.Select(target => target.Target).ToArray();
        var reverseTargets = reverse.Select(target => target.Target).ToArray();
        var pivotIndex = forwardTargets.Length == 0
            ? -1
            : Array.FindIndex(forwardTargets, target => ReferenceEquals(target, trace!.Forward[0].Target));
        var expectedReverse = pivotIndex >= 0
            ? new[] { forwardTargets[pivotIndex] }.Concat(forwardTargets.Skip(pivotIndex + 1).Reverse()).ToArray()
            : forwardTargets.Reverse().ToArray();
        Assert.Equal(expectedReverse, reverseTargets);
        _output.WriteLine(trace.ToFailureMessage());
        _output.WriteLine("selected={0}; rows={1}; failed details present={2}; retry enabled={3}; cancel enabled={4}; selected row cell reached both ways={5}",
            selectedTask?.TaskId,
            visibleRowCount,
            detailText.Contains("E_KEYBOARD_SAMPLE"),
            retryActionEnabled,
            cancelActionEnabled,
            selectedCellWasReached && selectedCellWasReachedReverse);
    }

    [Fact]
    public void LoadMoreButtonIsNotKeyboardReachableWhenTaskHistoryIsExhausted()
    {
        EntryTrace? trace = null;
        Button? loadMoreButton = null;
        var buttonIsCollapsed = false;
        var commandStayedBound = false;
        var reachedForward = false;
        var reachedReverse = false;

        RunSta(() =>
        {
            var failed = new TaskStatusDto
            {
                TaskId = "keyboard-exhausted-history",
                TaskType = "Validation",
                GameId = "synthetic-game-exhausted",
                GameName = "合成任务",
                State = TaskState.Failed,
                ProgressPercent = 100,
                CreatedUtc = DateTime.UtcNow.AddMinutes(-1),
                ErrorCode = "E_KEYBOARD_HISTORY_END",
                ErrorMessage = "用于键盘可达性负例的合成失败详情。"
            };
            var fixture = new TaskKeyboardFixture(new[] { failed }, failed, hasMoreHistory: false);
            var view = new TaskCenterView { DataContext = fixture };
            using var host = new TraceHost(view, 1280, 760);
            view.ApplyResponsiveLayout(1280, 760);
            host.Pump();

            loadMoreButton = FindTaskLoadMoreButton(view);
            buttonIsCollapsed = loadMoreButton.Visibility == Visibility.Collapsed && !loadMoreButton.IsVisible;
            commandStayedBound = ReferenceEquals(loadMoreButton.Command, fixture.LoadMoreTasksCommand);
            trace = TraceEntry(new EntrySpec("TaskCenter exhausted history", () => view, new[]
            {
                "搜索任务", "任务状态筛选", "任务列表", "安全重试任务"
            }), host);
            reachedForward = trace.Forward.Any(stop => ReferenceEquals(stop.Target, loadMoreButton));
            reachedReverse = trace.Reverse.Any(stop => ReferenceEquals(stop.Target, loadMoreButton));
        });

        Assert.True(buttonIsCollapsed, "exhausted history must keep the load-more action collapsed");
        Assert.True(commandStayedBound, "collapsing the action must not detach its existing command binding");
        Assert.NotNull(trace);
        Assert.False(reachedForward, "Tab must not reach a collapsed load-more action");
        Assert.False(reachedReverse, "Shift+Tab must not reach a collapsed load-more action");
        Assert.DoesNotContain(trace!.VisibleInteractiveTargets, target => ReferenceEquals(target.Target, loadMoreButton));
        Assert.All(trace.Forward.Concat(trace.Reverse), stop =>
            Assert.True(stop.IsInsideEntry, "focus escaped the TaskCenter host at " + stop.AutomationName));
    }

    private static Button FindTaskLoadMoreButton(FrameworkElement root)
        => Assert.Single(EnumerateElements(root).OfType<Button>(), button =>
            string.Equals(button.Content?.ToString(), "加载更多", StringComparison.Ordinal));

    private static void AssertInOrder(
        IReadOnlyList<InteractiveTarget> trace,
        IReadOnlyList<string> expectedNames,
        string direction)
    {
        var previousIndex = -1;
        foreach (var name in expectedNames)
        {
            var index = trace.ToList().FindIndex(previousIndex + 1, target => target.Description == name);
            Assert.True(index > previousIndex, direction + " skips or reorders " + name + "; reached=["
                + string.Join(" > ", trace.Select(target => target.Description)) + "]");
            previousIndex = index;
        }
    }

    private static ButtonBase FindNamedButton(FrameworkElement root, string automationName)
        => Assert.Single(EnumerateElements(root).OfType<ButtonBase>(),
            button => AutomationProperties.GetName(button) == automationName);

    private static IReadOnlyList<EntrySpec> CreateEntries()
        => new[]
        {
            new EntrySpec("Dashboard Shell", () => new AcrylicProductionShellView(), new[]
            {
                "打开首页", "打开存档中心", "打开修改器中心", "打开媒体中心",
                "打开任务中心", "打开维护中心", "打开 GameSaveCenter 设置"
            }),
            new EntrySpec("Overview", () => new OverviewView(), new[] { "刷新概览", "备份全部游戏", "同步媒体" }),
            new EntrySpec("SaveCenter", () => new SaveCenterView(), new[] { "立即扫描存档路径", "重新校验存档路径", "重新加载存档详情" }),
            new EntrySpec("TrainerCenter", () => new TrainerCenterView(), new[] { "导入修改器" }),
            new EntrySpec("MediaCenter", () => new MediaCenterView(), new[] { "打开媒体归类批次历史" }),
            new EntrySpec("TaskCenter", () => new TaskCenterView(), new[] { "搜索任务", "刷新任务列表" }),
            new EntrySpec("Maintenance", () => new MaintenanceView(), new[] { "刷新维护状态", "刷新诊断" }),
            new EntrySpec("Settings", () => new GameSaveCenterSettingsView(), new[] { "搜索设置", "设置分类导航" })
        };

    private static EntryTrace TraceEntry(EntrySpec entry, TraceHost host)
    {
        var candidates = EnumerateFocusable(host.Scope).ToList();
        Assert.True(candidates.Count >= 2, entry.Name + " has too few focusable elements: " + candidates.Count + "; window=" + host.Window.IsVisible + ", loaded=" + host.Entry.IsLoaded + ", visible=" + host.Entry.IsVisible + ", size=" + host.Entry.ActualWidth + "x" + host.Entry.ActualHeight + ", visualChildren=" + VisualTreeHelper.GetChildrenCount(host.Entry) + "; " + DescribeControls(host.Scope));

        var first = candidates[0];
        Keyboard.Focus(first);
        host.Pump();
        var forward = WalkFocus(host.Scope, FocusNavigationDirection.Next, entry.Name);

        Keyboard.Focus(first);
        host.Pump();
        var reverse = WalkFocus(host.Scope, FocusNavigationDirection.Previous, entry.Name);

        var visibleInteractiveTargets = EnumerateElements(host.Scope)
            .Where(element => element.IsVisible
                && element.IsEnabled
                && element.Focusable
                && KeyboardNavigation.GetIsTabStop(element)
                && (element is ButtonBase || element is TextBoxBase || element is Selector))
            .Select(element => new InteractiveTarget(
                element,
                AutomationProperties.GetName(element) is { Length: > 0 } automationName
                    ? automationName
                    : !string.IsNullOrWhiteSpace(element.Name) ? element.Name : element.GetType().Name))
            .ToList();

        return new EntryTrace(entry.Name, entry.ExpectedNames, forward, reverse, visibleInteractiveTargets);
    }

    private static List<FocusStop> WalkFocus(
        FrameworkElement scope,
        FocusNavigationDirection direction,
        string entryName)
    {
        var trace = new List<FocusStop>();
        var visited = new HashSet<DependencyObject>();
        var current = Keyboard.FocusedElement;

        for (var index = 0; index < 128 && current is UIElement currentElement; index++)
        {
            if (current is DependencyObject currentObject && !visited.Add(currentObject))
                break;

            var stop = Describe(current, scope);
            trace.Add(stop);

            if (!currentElement.MoveFocus(new TraversalRequest(direction)))
                break;
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Input, new Action(() => { }));
            current = Keyboard.FocusedElement;
            if (current == null || !IsDescendantOf(current as DependencyObject, scope))
                break;
        }

        if (trace.Count == 0)
            throw new Xunit.Sdk.XunitException(entryName + " produced no focus trace.");
        return trace;
    }

    private static FocusStop Describe(IInputElement? element, FrameworkElement scope)
    {
        var dependencyObject = element as DependencyObject;
        var frameworkElement = element as FrameworkElement;
        var automationName = frameworkElement == null ? string.Empty : AutomationProperties.GetName(frameworkElement);
        if (string.IsNullOrWhiteSpace(automationName))
            automationName = !string.IsNullOrWhiteSpace(frameworkElement?.Name)
                ? frameworkElement!.Name
                : element?.GetType().Name ?? "未知焦点元素";

        return new FocusStop(
            automationName,
            element?.GetType().Name ?? "null",
            IsDescendantOf(dependencyObject, scope),
            element,
            FindAncestorDataGrid(dependencyObject));
    }

    private static DataGrid? FindAncestorDataGrid(DependencyObject? element)
    {
        var current = element;
        while (current != null)
        {
            if (current is DataGrid grid)
                return grid;
            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private static IEnumerable<FrameworkElement> EnumerateFocusable(DependencyObject root)
    {
        if (root is FrameworkElement element
            && element.IsVisible
            && element.IsEnabled
            && element.Focusable
            && (KeyboardNavigation.GetIsTabStop(element) || element is ButtonBase || element is TextBoxBase || element is Selector))
            yield return element;

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            foreach (var child in EnumerateFocusable(VisualTreeHelper.GetChild(root, index)))
                yield return child;
        }
    }

    private static string DescribeControls(DependencyObject root)
        => string.Join(", ", EnumerateElements(root)
            .Where(element => element is ButtonBase || element is TextBoxBase || element is Selector)
            .Take(12)
            .Select(element => (element.Name ?? element.GetType().Name) + ":visible=" + element.IsVisible + ",focus=" + element.Focusable + ",tab=" + KeyboardNavigation.GetIsTabStop(element)));

    private static IEnumerable<FrameworkElement> EnumerateElements(DependencyObject root)
    {
        if (root is FrameworkElement element)
            yield return element;

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            foreach (var child in EnumerateElements(VisualTreeHelper.GetChild(root, index)))
                yield return child;
        }
    }

    private static bool IsDescendantOf(DependencyObject? element, DependencyObject ancestor)
    {
        var current = element;
        while (current != null)
        {
            if (ReferenceEquals(current, ancestor))
                return true;
            current = VisualTreeHelper.GetParent(current);
        }

        return false;
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

    private sealed class EntrySpec
    {
        public EntrySpec(string name, Func<UserControl> create, IReadOnlyList<string> expectedNames)
        {
            Name = name;
            Create = create;
            ExpectedNames = expectedNames;
        }

        public string Name { get; }
        public Func<UserControl> Create { get; }
        public IReadOnlyList<string> ExpectedNames { get; }
    }

    private sealed class EntryTrace
    {
        public EntryTrace(
            string name,
            IReadOnlyList<string> expectedNames,
            List<FocusStop> forward,
            List<FocusStop> reverse,
            List<InteractiveTarget> visibleInteractiveTargets)
        {
            Name = name;
            ExpectedNames = expectedNames;
            Forward = forward;
            Reverse = reverse;
            VisibleInteractiveTargets = visibleInteractiveTargets;
        }

        public string Name { get; }
        public IReadOnlyList<string> ExpectedNames { get; }
        public List<FocusStop> Forward { get; }
        public List<FocusStop> Reverse { get; }
        public List<InteractiveTarget> VisibleInteractiveTargets { get; }

        public string ToFailureMessage()
            => Name + " forward=[" + string.Join(" > ", Forward.Select(stop => stop.AutomationName))
                + "] reverse=[" + string.Join(" > ", Reverse.Select(stop => stop.AutomationName))
                + "] visible-interactive=[" + string.Join(", ", VisibleInteractiveTargets.Select(target => target.Description))
                + "] expected=[" + string.Join(", ", ExpectedNames) + "]";
    }

    private sealed class FocusStop
    {
        public FocusStop(string automationName, string typeName, bool isInsideEntry, IInputElement? target, DataGrid? dataGrid)
        {
            AutomationName = automationName;
            TypeName = typeName;
            IsInsideEntry = isInsideEntry;
            Target = target;
            DataGrid = dataGrid;
        }

        public string AutomationName { get; }
        public string TypeName { get; }
        public bool IsInsideEntry { get; }
        public IInputElement? Target { get; }
        public DataGrid? DataGrid { get; }
    }

    private sealed class InteractiveTarget
    {
        public InteractiveTarget(FrameworkElement target, string description)
        {
            Target = target;
            Description = description;
        }

        public FrameworkElement Target { get; }
        public string Description { get; }
    }

    private sealed class TaskKeyboardFixture : INotifyPropertyChanged
    {
        private TaskStatusDto? selectedTask;

        public TaskKeyboardFixture(IEnumerable<TaskStatusDto> tasks, TaskStatusDto selectedTask, bool hasMoreHistory)
        {
            TasksView = CollectionViewSource.GetDefaultView(new ObservableCollection<TaskStatusDto>(tasks));
            this.selectedTask = selectedTask;
            TaskHistoryHasMore = hasMoreHistory;
            OpenSelectedTaskGameCommand = new NoOpCommand(() => !string.IsNullOrWhiteSpace(SelectedTask?.GameId));
            CopyTaskErrorCommand = new NoOpCommand(() => SelectedTask?.State == TaskState.Failed);
            RetryTaskCommand = new NoOpCommand(() => SelectedTask?.State == TaskState.Failed);
            CancelTaskCommand = new NoOpCommand(() => SelectedTask?.CanCancel == true);
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

        public string TaskSearchText { get; set; } = "合成任务";
        public string TaskStatusFilter { get; set; } = "全部";
        public string TaskTypeFilter { get; set; } = "全部";
        public string TaskHistoryScope { get; set; } = "全部时间";
        public string TaskHistoryRange { get; set; } = "全部时间";
        public string TaskFilterPresetNameDraft { get; set; } = "复核预设";
        public string TaskPageState { get; } = "Loaded";
        public string TaskLoadedSummary { get; } = "3 条合成记录";
        public string TaskPageStatusSummary { get; } = "测试数据已加载";
        public string TaskPageStatusSummaryFullDisplay { get; } = "键盘顺序测试的合成任务状态";
        public string TaskActiveFiltersSummary { get; } = "全部任务";
        public bool TaskPageHasItems => true;
        public bool TaskPageLoadFailed => false;
        public bool IsTaskPageLoading => false;
        public bool TaskHistoryHasMore { get; }
        public bool TaskHasActiveFilters => false;
        public bool HasTaskNavigationTarget => false;
        public int TaskTotalCount => 3;
        public int RunningTaskCount => TasksView.Cast<TaskStatusDto>().Count(task => task.State == TaskState.Running);
        public int RetryableTaskCount => TasksView.Cast<TaskStatusDto>().Count(task => task.State == TaskState.Failed);
        public int CompletedTaskCount => TasksView.Cast<TaskStatusDto>().Count(task => task.State == TaskState.Succeeded);
        public int TaskWaitingSummary => 0;
        public string TaskRetrySummary => "失败 1";
        public ObservableCollection<string> TaskStatusFilterOptions { get; } = new() { "全部", "失败", "运行中" };
        public ObservableCollection<string> TaskTypeFilterOptions { get; } = new() { "全部", "存档校验", "媒体同步" };
        public ObservableCollection<string> TaskHistoryScopeOptions { get; } = new() { "全部时间", "最近" };
        public ObservableCollection<string> TaskHistoryRangeOptions { get; } = new() { "全部时间", "30 天" };
        public ObservableCollection<TaskFilterPresetProbe> TaskFilterPresets { get; } = new() { new TaskFilterPresetProbe("默认") };
        public TaskFilterPresetProbe? SelectedTaskFilterPreset { get; set; }

        public ICommand RefreshCommand { get; } = new NoOpCommand();
        public ICommand RetryAllTasksCommand { get; } = new NoOpCommand();
        public ICommand LoadMoreTasksCommand { get; } = new NoOpCommand();
        public ICommand ClearTaskFiltersCommand { get; } = new NoOpCommand();
        public ICommand ClearTaskNavigationContextCommand { get; } = new NoOpCommand();
        public ICommand ApplyTaskFilterPresetCommand { get; } = new NoOpCommand();
        public ICommand SaveTaskFilterPresetCommand { get; } = new NoOpCommand();
        public ICommand RenameTaskFilterPresetCommand { get; } = new NoOpCommand();
        public ICommand DeleteTaskFilterPresetCommand { get; } = new NoOpCommand();
        public ICommand OpenSelectedTaskGameCommand { get; }
        public ICommand CopyTaskErrorCommand { get; }
        public ICommand RetryTaskCommand { get; }
        public ICommand CancelTaskCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private sealed class TaskFilterPresetProbe
    {
        public TaskFilterPresetProbe(string name) => Name = name;
        public string Name { get; }
    }

    private sealed class NoOpCommand : ICommand
    {
        private readonly Func<bool> canExecute;

        public NoOpCommand(Func<bool>? canExecute = null) => this.canExecute = canExecute ?? (() => true);
        public bool CanExecute(object? parameter) => canExecute();
        public void Execute(object? parameter) { }
        public event EventHandler? CanExecuteChanged { add { } remove { } }
    }

    private sealed class TraceHost : IDisposable
    {
        public TraceHost(UserControl entry, double width = 1040, double height = 700)
        {
            Entry = entry;
            Scope = new Grid();
            FocusManager.SetIsFocusScope(Scope, true);
            KeyboardNavigation.SetTabNavigation(Scope, KeyboardNavigationMode.Cycle);
            KeyboardNavigation.SetDirectionalNavigation(Scope, KeyboardNavigationMode.Contained);
            Scope.Children.Add(entry);

            Window = new Window
            {
                Content = Scope,
                Width = width,
                Height = height,
                ShowInTaskbar = false,
                ShowActivated = true,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };
            Window.Show();
            Window.UpdateLayout();
        }

        public UserControl Entry { get; }
        public Grid Scope { get; }
        public Window Window { get; }

        public void Pump()
            => Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => Window.UpdateLayout()));

        public void Dispose()
        {
            if (Window.IsVisible)
                Window.Close();
        }
    }
}
