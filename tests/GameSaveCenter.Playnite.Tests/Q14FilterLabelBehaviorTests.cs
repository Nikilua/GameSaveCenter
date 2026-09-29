using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.Views;
using Xunit;
using Xunit.Abstractions;

namespace GameSaveCenter.Playnite.Tests;

[Collection("Q14FilterLabelWpf")]
public sealed class Q14FilterLabelBehaviorTests
{
    private const double GeometryTolerance = 0.75d;
    private readonly ITestOutputHelper output;

    public Q14FilterLabelBehaviorTests(ITestOutputHelper output) => this.output = output;

    [Fact]
    public void ProductionTaskFiltersKeepLabelsAttachedAndWrapWholeGroups()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        Exception? failure = null;
        var reports = new List<string>();
        var thread = new Thread(() =>
        {
            try
            {
                foreach (var scenario in new[]
                {
                    (GameSaveCenterThemeMode.Light, Width: 1280d, Height: 720d),
                    (GameSaveCenterThemeMode.Dark, Width: 1280d, Height: 720d),
                    (GameSaveCenterThemeMode.Light, Width: 980d, Height: 700d),
                    (GameSaveCenterThemeMode.Dark, Width: 980d, Height: 700d),
                    (GameSaveCenterThemeMode.Light, Width: 979d, Height: 700d),
                    (GameSaveCenterThemeMode.Dark, Width: 979d, Height: 700d),
                    (GameSaveCenterThemeMode.Light, Width: 760d, Height: 640d),
                    (GameSaveCenterThemeMode.Dark, Width: 760d, Height: 640d),
                    (GameSaveCenterThemeMode.Light, Width: 620d, Height: 640d),
                    (GameSaveCenterThemeMode.Dark, Width: 620d, Height: 640d)
                })
                {
                    reports.Add(VerifyScenario(scenario.Item1, scenario.Width, scenario.Height));
                }
            }
            catch (Exception caught)
            {
                failure = caught;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(failure);
        Assert.Equal(10, reports.Count);
        foreach (var report in reports)
            output.WriteLine(report);
    }

    private static string VerifyScenario(GameSaveCenterThemeMode theme, double width, double height)
    {
        EnsureApplication();
        var fixture = new TaskFilterFixture();
        var view = new TaskCenterView { DataContext = fixture };
        var palette = AdaptiveThemePaletteFactory.Create(view, glassEnabled: true, strengthPercent: 78, theme);
        AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(view.Resources, palette, glassEnabled: true, motionEnabled: true);

        var filters = GetField<Grid>(view, "TaskFiltersPanel");
        var moreFilters = GetField<WrapPanel>(view, "TaskMoreFiltersHost");
        var expander = GetField<Expander>(view, "TaskMoreFiltersExpander");
        var status = CreatePair(view, "Status", "状态：", "任务状态筛选", "TaskStatusFilterGroup", "TaskStatusFilterLabel", "TaskStatusFilterComboBox");
        var type = CreatePair(view, "Type", "类型：", "任务类型筛选", "TaskTypeFilterGroup", "TaskTypeFilterLabel", "TaskTypeFilterComboBox");
        var scope = CreatePair(view, "Scope", "范围：", "任务范围", "TaskHistoryScopeFilterGroup", "TaskHistoryScopeLabel", "TaskHistoryScopeComboBox");
        var range = CreatePair(view, "Range", "时间：", "任务时间范围", "TaskHistoryRangeFilterGroup", "TaskHistoryRangeLabel", "TaskHistoryRangeComboBox");
        var game = CreatePair(view, "Game", "游戏：", "任务游戏筛选", "TaskGameFilterGroup", "TaskGameFilterLabel", "TaskGameFilterComboBox");
        var search = GetField<Grid>(view, "TaskSearchBoxHost");
        var pairs = new[] { status, type, scope, range, game };
        var compact = width < 980d;

        var window = new Window
        {
            Content = view,
            Width = width,
            Height = height,
            WindowStyle = WindowStyle.None,
            ResizeMode = ResizeMode.NoResize,
            ShowInTaskbar = false,
            ShowActivated = false,
            Opacity = 0.01
        };

        try
        {
            window.Show();
            FlushLayout(window);
            view.ApplyResponsiveLayout(width, height);
            expander.IsExpanded = compact;
            FlushLayout(window);

            Assert.Contains(search, filters.Children.Cast<UIElement>());
            Assert.Contains(status.Group, filters.Children.Cast<UIElement>());
            Assert.Equal(compact, expander.Visibility == Visibility.Visible);
            AssertPair(status, filters, "全部");

            foreach (var pair in pairs.Skip(1))
            {
                var expectedSelection = pair.Name switch
                {
                    "Type" => "媒体同步",
                    "Scope" => "当前游戏",
                    "Range" => "30 天",
                    "Game" => "Bongo Cat",
                    _ => throw new InvalidOperationException("Unknown filter pair " + pair.Name)
                };
                var pairHost = compact ? (FrameworkElement)moreFilters : filters;
                AssertPair(pair, pairHost, expectedSelection: null);
                if (pair.Group.ActualWidth > 0 && pair.Group.ActualHeight > 0)
                {
                    // Change the production ComboBox selection after the source and
                    // ItemsSource are live, as a user would. This isolates the
                    // responsive reparenting behavior from first-item style defaults.
                    pair.Combo.SelectedItem = expectedSelection;
                    FlushLayout(window);
                    AssertPair(pair, pairHost, expectedSelection);
                }
                Panel expectedParent = compact || pair.Name == "Game" ? moreFilters : filters;
                Assert.Same(expectedParent, pair.Group.Parent);
                if (!compact && pair.Name != "Game")
                    Assert.Equal(0d, pair.Group.Margin.Bottom);
            }

            var reportedPanelWidth = moreFilters.ActualWidth;
            var reportGroups = compact
                ? pairs.Skip(1).ToArray()
                : pairs.Skip(1).Where(pair => pair.Name != "Game").ToArray();
            var reportedRows = reportGroups
                .Select(pair => GetBounds(pair.Group, compact ? (FrameworkElement)moreFilters : (FrameworkElement)filters).Top.ToString("0.##"))
                .ToArray();

            if (compact)
            {
                Assert.Same(filters, status.Group.Parent);
                Assert.Equal(4, moreFilters.Children.Count);
                var visiblePairs = pairs.Skip(1).ToArray();
                var rowTops = new List<double>();
                foreach (var pair in visiblePairs)
                {
                    Assert.Equal(10d, pair.Group.Margin.Right);
                    Assert.Equal(8d, pair.Group.Margin.Bottom);
                    var bounds = GetBounds(pair.Group, moreFilters);
                    Assert.True(bounds.Left >= -GeometryTolerance,
                        $"{theme} {width:0} DIP {pair.Name} group left={bounds.Left:0.###}.");
                    Assert.True(bounds.Right <= moreFilters.ActualWidth + GeometryTolerance,
                        $"{theme} {width:0} DIP {pair.Name} group right={bounds.Right:0.###}, host width={moreFilters.ActualWidth:0.###}.");
                    rowTops.Add(bounds.Top);
                }

                Assert.True(moreFilters.ActualWidth > 0, $"{theme} {width:0} DIP more-filter host was not arranged.");
                if (width <= 700d)
                {
                    var rowTopsDescription = string.Join(",", rowTops.Select(value => value.ToString("0.###")));
                    Assert.True(rowTops.Max() - rowTops.Min() > 1d,
                        $"{theme} {width:0} DIP should wrap whole field groups to multiple rows; tops={rowTopsDescription}.");
                    var groupBounds = visiblePairs
                        .Select(pair => GetBounds(pair.Group, moreFilters))
                        .ToArray();
                    var firstRowTop = groupBounds.Min(bounds => bounds.Top);
                    var firstRowBottom = groupBounds
                        .Where(bounds => Math.Abs(bounds.Top - firstRowTop) <= GeometryTolerance)
                        .Max(bounds => bounds.Bottom);
                    var nextRowTop = groupBounds
                        .Where(bounds => bounds.Top > firstRowTop + GeometryTolerance)
                        .Min(bounds => bounds.Top);
                    var rowGeometry = string.Join(";", visiblePairs.Select(pair =>
                    {
                        var bounds = GetBounds(pair.Group, moreFilters);
                        var slot = System.Windows.Controls.Primitives.LayoutInformation.GetLayoutSlot(pair.Group);
                        return $"{pair.Name}:top={bounds.Top:0.###},height={bounds.Height:0.###},bottomMargin={pair.Group.Margin.Bottom:0.###},slot={slot.Top:0.###}/{slot.Height:0.###}";
                    }));
                    Assert.True(nextRowTop - firstRowBottom >= 7.25d,
                        $"{theme} {width:0} DIP wrapped filter rows should retain at least 7.25 DIP clear gap; gap={nextRowTop - firstRowBottom:0.###} DIP; {rowGeometry}.");
                }

                // Reparent the live fields across the breakpoint and back. This proves
                // bindings and selected values survive the responsive move.
                window.Width = 1280d;
                FlushLayout(window);
                view.ApplyResponsiveLayout(1280d, height);
                expander.IsExpanded = false;
                FlushLayout(window);
                Assert.Same(filters, type.Group.Parent);
                Assert.Same(filters, scope.Group.Parent);
                Assert.Same(filters, range.Group.Parent);
                Assert.Equal("媒体同步", type.Combo.SelectedItem);
                Assert.Equal("当前游戏", scope.Combo.SelectedItem);
                Assert.Equal("30 天", range.Combo.SelectedItem);
                Assert.Equal("Bongo Cat", game.Combo.SelectedItem);

                window.Width = width;
                FlushLayout(window);
                view.ApplyResponsiveLayout(width, height);
                expander.IsExpanded = true;
                FlushLayout(window);
                Assert.Same(moreFilters, type.Group.Parent);
                Assert.Same(moreFilters, scope.Group.Parent);
                Assert.Same(moreFilters, range.Group.Parent);
                Assert.Equal("媒体同步", type.Combo.SelectedItem);
                Assert.Equal("当前游戏", scope.Combo.SelectedItem);
                Assert.Equal("30 天", range.Combo.SelectedItem);
                Assert.Equal("Bongo Cat", game.Combo.SelectedItem);
            }

            var reportedRowsText = string.Join(",", reportedRows);
            return $"theme={theme}; requestedWindow={width:0}x{height:0} DIP; wpfDpi={VisualTreeHelper.GetDpi(view).DpiScaleX:0.##}x{VisualTreeHelper.GetDpi(view).DpiScaleY:0.##}; compact={compact}; panelWidth={reportedPanelWidth:0.##}; rows={reportedRowsText}; label/control gaps=4 DIP; bindings and selections survive resize";
        }
        finally
        {
            if (window.IsVisible)
                window.Close();
        }
    }

    private static FilterPair CreatePair(
        TaskCenterView view,
        string name,
        string labelText,
        string automationName,
        string groupField,
        string labelField,
        string comboField)
    {
        var group = GetField<StackPanel>(view, groupField);
        var label = GetField<TextBlock>(view, labelField);
        var combo = GetField<ComboBox>(view, comboField);
        Assert.Equal(labelText, label.Text);
        Assert.Equal(automationName, AutomationProperties.GetName(combo));
        Assert.Equal(new Thickness(0, 0, 4, 0), label.Margin);
        Assert.Equal(default, combo.Margin);
        Assert.Equal(2, group.Children.Count);
        Assert.Same(label, group.Children[0]);
        Assert.Same(combo, group.Children[1]);
        Assert.NotNull(BindingOperations.GetBinding(combo, Selector.SelectedItemProperty));
        return new FilterPair(name, group, label, combo);
    }

    private static void AssertPair(FilterPair pair, FrameworkElement ancestor, string? expectedSelection)
    {
        Assert.NotNull(BindingOperations.GetBinding(pair.Combo, Selector.SelectedItemProperty));
        if (pair.Group.ActualWidth == 0 || pair.Group.ActualHeight == 0)
            return;

        if (expectedSelection != null)
            Assert.Equal(expectedSelection, pair.Combo.SelectedItem);

        var label = GetBounds(pair.Label, pair.Group);
        var combo = GetBounds(pair.Combo, pair.Group);
        var gap = combo.Left - label.Right;
        Assert.True(Math.Abs(gap - 4d) <= GeometryTolerance,
            $"{pair.Name} label/control gap was {gap:0.###} DIP, expected 4 DIP.");
        Assert.True(Math.Abs((label.Top + label.Height / 2) - (combo.Top + combo.Height / 2)) <= GeometryTolerance,
            $"{pair.Name} label and control centers do not align vertically.");
        Assert.True(pair.Group.TransformToAncestor(ancestor) != null);
    }

    private static Rect GetBounds(FrameworkElement element, FrameworkElement ancestor)
        => element.TransformToAncestor(ancestor).TransformBounds(
            new Rect(new Point(0, 0), new Size(element.ActualWidth, element.ActualHeight)));

    private static T GetField<T>(TaskCenterView view, string name)
        where T : class
        => (T)(typeof(TaskCenterView).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(view)
            ?? throw new InvalidOperationException($"Missing TaskCenterView field {name}."));

    private static void FlushLayout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
        window.UpdateLayout();
    }

    private static void EnsureApplication()
    {
        var application = Application.Current ?? new Application();
        application.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        if (application.Resources.MergedDictionaries.Count == 0)
            application.Resources.MergedDictionaries.Add(LoadProductionResources());
    }

    private static ResourceDictionary LoadProductionResources()
    {
        var resources = new ResourceDictionary();
        foreach (var source in new[]
        {
            "/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml",
            "/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml",
            "/GameSaveCenter.Playnite;component/Themes/Redesign.xaml",
            "/GameSaveCenter.Playnite;component/Themes/AcrylicProductionResources.xaml"
        })
        {
            resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(source, UriKind.Relative) });
        }

        return resources;
    }

    private sealed class FilterPair
    {
        public FilterPair(string name, StackPanel group, TextBlock label, ComboBox combo)
        {
            Name = name;
            Group = group;
            Label = label;
            Combo = combo;
        }

        public string Name { get; }
        public StackPanel Group { get; }
        public TextBlock Label { get; }
        public ComboBox Combo { get; }
    }

    private sealed class TaskFilterFixture
    {
        public string TaskStatusFilter { get; set; } = "全部";
        public string TaskTypeFilter { get; set; } = "媒体同步";
        public string TaskHistoryScope { get; set; } = "当前游戏";
        public string TaskHistoryRange { get; set; } = "30 天";
        public string TaskGameFilter { get; set; } = "Bongo Cat";
        public ObservableCollection<string> TaskStatusFilterOptions { get; } = new() { "全部", "失败" };
        public ObservableCollection<string> TaskTypeFilterOptions { get; } = new() { "全部", "媒体同步" };
        public ObservableCollection<string> TaskHistoryScopeOptions { get; } = new() { "全部", "当前游戏" };
        public ObservableCollection<string> TaskHistoryRangeOptions { get; } = new() { "全部时间", "30 天" };
        public ObservableCollection<string> TaskGameFilterOptions { get; } = new() { "全部", "Bongo Cat" };
    }
}

[CollectionDefinition("Q14FilterLabelWpf", DisableParallelization = true)]
public sealed class Q14FilterLabelWpfCollection
{
}
