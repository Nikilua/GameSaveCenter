using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
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
                    trace.Forward.Any(stop => ReferenceEquals(stop.Target, target.Target)),
                    trace.Name + " forward Tab cannot reach " + target.Description + "; " + trace.ToFailureMessage());
                Assert.True(
                    trace.Reverse.Any(stop => ReferenceEquals(stop.Target, target.Target)),
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
            if (target != null && !result.Any(existing => ReferenceEquals(existing.Target, target.Target)))
                result.Add(target);
        }

        return result;
    }

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
            element);
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
        public FocusStop(string automationName, string typeName, bool isInsideEntry, IInputElement? target)
        {
            AutomationName = automationName;
            TypeName = typeName;
            IsInsideEntry = isInsideEntry;
            Target = target;
        }

        public string AutomationName { get; }
        public string TypeName { get; }
        public bool IsInsideEntry { get; }
        public IInputElement? Target { get; }
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

    private sealed class TraceHost : IDisposable
    {
        public TraceHost(UserControl entry)
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
                Width = 1040,
                Height = 700,
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
