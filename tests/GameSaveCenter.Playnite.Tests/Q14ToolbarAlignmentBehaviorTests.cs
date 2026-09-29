using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Controls;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.Views;
using Xunit;
using Xunit.Abstractions;

namespace GameSaveCenter.Playnite.Tests;

public sealed class Q14ToolbarAlignmentBehaviorTests
{
    private const double ExpectedControlHeight = 36d;
    private const double GeometryTolerance = 0.75d;
    private readonly ITestOutputHelper output;

    public Q14ToolbarAlignmentBehaviorTests(ITestOutputHelper output) => this.output = output;

    [Fact]
    public void ProductionTaskToolbarsKeepMixedControlGeometryAndTextCenters()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        Exception? exception = null;
        var reports = new System.Collections.Generic.List<string>();
        var thread = new Thread(() =>
        {
            try
            {
                foreach (var scenario in new[]
                {
                    (GameSaveCenterThemeMode.Light, Width: 980d, Height: 640d),
                    (GameSaveCenterThemeMode.Dark, Width: 980d, Height: 640d),
                    (GameSaveCenterThemeMode.Light, Width: 1040d, Height: 700d),
                    (GameSaveCenterThemeMode.Dark, Width: 1040d, Height: 700d),
                    (GameSaveCenterThemeMode.Light, Width: 1280d, Height: 720d),
                    (GameSaveCenterThemeMode.Dark, Width: 1280d, Height: 720d),
                    (GameSaveCenterThemeMode.Light, Width: 1600d, Height: 900d),
                    (GameSaveCenterThemeMode.Dark, Width: 1600d, Height: 900d),
                    (GameSaveCenterThemeMode.Light, Width: 760d, Height: 640d),
                    (GameSaveCenterThemeMode.Dark, Width: 760d, Height: 640d)
                })
                {
                    reports.Add(CaptureToolbarGeometry(scenario.Item1, scenario.Width, scenario.Height));
                }
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
        Assert.Equal(10, reports.Count);
        foreach (var report in reports)
            output.WriteLine(report);
    }

    private static string CaptureToolbarGeometry(GameSaveCenterThemeMode theme, double width, double height)
    {
        EnsureApplication();
        var view = new TaskCenterView
        {
            DataContext = new TaskToolbarFixture()
        };
        var palette = AdaptiveThemePaletteFactory.Create(view, glassEnabled: true, strengthPercent: 78, theme);
        AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(view.Resources, palette, glassEnabled: true, motionEnabled: true);

        var filters = GetField<Grid>(view, "TaskFiltersPanel");
        var searchHost = GetField<Grid>(view, "TaskSearchBoxHost");
        var search = GetField<TextBox>(view, "TaskSearchTextBox");
        var status = GetField<ComboBox>(view, "TaskStatusFilterComboBox");
        var type = GetField<ComboBox>(view, "TaskTypeFilterComboBox");
        var scope = GetField<ComboBox>(view, "TaskHistoryScopeComboBox");
        var range = GetField<ComboBox>(view, "TaskHistoryRangeComboBox");
        var typeGroup = GetField<StackPanel>(view, "TaskTypeFilterGroup");
        var scopeGroup = GetField<StackPanel>(view, "TaskHistoryScopeFilterGroup");
        var rangeGroup = GetField<StackPanel>(view, "TaskHistoryRangeFilterGroup");
        var refresh = GetField<GameSaveCenter.Playnite.Controls.Button>(view, "TaskRefreshButton");
        var presetRow = GetField<WrapPanel>(view, "TaskFilterPresetRow");
        var presetControls = presetRow.Children.OfType<Control>().ToArray();

        search.Text = "Bongo Cat";
        search.Text = "搜索任务";
        status.SelectedIndex = 0;
        type.SelectedIndex = 0;
        scope.SelectedIndex = 0;
        range.SelectedIndex = 0;
        foreach (var combo in presetControls.OfType<ComboBox>())
        {
            combo.ItemsSource = new[] { "全部", "失败" };
            combo.SelectedIndex = 0;
        }
        foreach (var textBox in presetControls.OfType<TextBox>())
            textBox.Text = "我的预设";

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
            view.ApplyResponsiveLayout(width, height);
            window.Show();
            FlushLayout(window);
            view.ApplyResponsiveLayout(view.ActualWidth, view.ActualHeight);
            FlushLayout(window);

            var mainRow = new System.Collections.Generic.List<Control> { search, status, refresh };
            foreach (var filter in new[]
            {
                (Combo: type, Group: typeGroup),
                (Combo: scope, Group: scopeGroup),
                (Combo: range, Group: rangeGroup)
            })
            {
                if (ReferenceEquals(filter.Group.Parent, filters) && Grid.GetRow(filter.Group) == 0)
                    mainRow.Add(filter.Combo);
            }

            Assert.Contains(searchHost, filters.Children.Cast<UIElement>());
            Assert.Equal(0, Grid.GetRow(searchHost));
            Assert.True(mainRow.Count >= 3, $"{theme} {width}x{height}: the production main filter row lost a control.");
            Assert.All(mainRow, control =>
                AssertGeometryNear(control.ActualHeight, ExpectedControlHeight, GeometryTolerance,
                    $"{theme} {width}x{height}: {control.GetType().Name} height={control.ActualHeight:0.###}"));

            var mainCenters = mainRow
                .Select(control => CenterY(control, filters))
                .Append(CenterY(search, filters))
                .ToArray();
            Assert.True(mainCenters.Max() - mainCenters.Min() <= GeometryTolerance,
                $"{theme} {width}x{height}: main row centers={string.Join(",", mainCenters.Select(x => x.ToString("0.###")))}");

            var presetBounds = presetControls.Select(control => GetBounds(control, presetRow)).ToArray();
            Assert.NotEmpty(presetBounds);
            Assert.All(presetBounds, bounds =>
                AssertGeometryNear(bounds.Height, ExpectedControlHeight, GeometryTolerance,
                    $"{theme} {width}x{height}: preset control height={bounds.Height:0.###}"));
            Assert.True(presetBounds.Max(bounds => bounds.Top) - presetBounds.Min(bounds => bounds.Top) <= GeometryTolerance,
                $"{theme} {width}x{height}: preset row tops={string.Join(",", presetBounds.Select(x => x.Top.ToString("0.###")))}");

            var presetContentCenters = presetControls
                .Select(control => ContentCenterY(control, presetRow))
                .ToArray();
            Assert.True(presetContentCenters.Max() - presetContentCenters.Min() <= 1.5,
                $"{theme} {width}x{height}: preset text centers={string.Join(",", presetContentCenters.Select(x => x.ToString("0.###")))}");

            var baseSnapshot = Snapshot(mainRow, search, filters, presetControls, presetRow);
            var searchExpression = BindingOperations.GetBindingExpression(search, TextBox.TextProperty);
            Assert.NotNull(searchExpression);
            Validation.MarkInvalid(searchExpression!, new ValidationError(
                new ExceptionValidationRule(), searchExpression, "synthetic toolbar validation error", null));
            FlushLayout(window);
            Assert.True(Validation.GetHasError(search));
            AssertSnapshotWithin(baseSnapshot, Snapshot(mainRow, search, filters, presetControls, presetRow), GeometryTolerance, "validation error");
            Validation.ClearInvalid(searchExpression!);

            Assert.Same(search, Keyboard.Focus(search));
            FlushLayout(window);
            Assert.True(search.IsKeyboardFocusWithin);
            AssertSnapshotWithin(baseSnapshot, Snapshot(mainRow, search, filters, presetControls, presetRow), GeometryTolerance, "keyboard focus");

            refresh.IsEnabled = false;
            FlushLayout(window);
            Assert.False(refresh.IsEnabled);
            AssertSnapshotWithin(baseSnapshot, Snapshot(mainRow, search, filters, presetControls, presetRow), GeometryTolerance, "disabled refresh");
            refresh.IsEnabled = true;

            refresh.IsBusy = true;
            window.UpdateLayout();
            AssertSnapshotWithin(baseSnapshot, Snapshot(mainRow, search, filters, presetControls, presetRow), GeometryTolerance, "busy refresh");
            refresh.IsBusy = false;

            var mainDescription = string.Join("; ", mainRow.Select(control => Describe(control, filters)));
            var presetDescription = string.Join("; ", presetControls.Select(control => Describe(control, presetRow)));
            return $"theme={theme}; window={width:0}x{height:0} DIP; wpfDpi={VisualTreeHelper.GetDpi(view).DpiScaleX:0.##}x{VisualTreeHelper.GetDpi(view).DpiScaleY:0.##}; main=[{mainDescription}]; presets=[{presetDescription}]; error/focus/disabled/busy geometry stable";
        }
        finally
        {
            refresh.IsBusy = false;
            if (window.IsVisible)
                window.Close();
        }
    }

    private static string Snapshot(
        System.Collections.Generic.IEnumerable<Control> mainRow,
        TextBox search,
        FrameworkElement mainAncestor,
        Control[] presetControls,
        FrameworkElement presetAncestor)
        => string.Join("|", mainRow.Append(search).Select(control => Describe(control, mainAncestor))
            .Concat(presetControls.Select(control => Describe(control, presetAncestor))));

    private static string Describe(FrameworkElement element, FrameworkElement ancestor)
    {
        var bounds = GetBounds(element, ancestor);
        var centerY = ContentCenterY((Control)element, ancestor);
        var measurements = string.Join(",", new[] { bounds.Left, bounds.Top, bounds.Width, bounds.Height, centerY }
            .Select(value => value.ToString("0.##", CultureInfo.InvariantCulture)));
        return $"{element.GetType().Name}:{measurements}";
    }

    private static void AssertSnapshotWithin(string expected, string actual, double tolerance, string state)
    {
        var expectedControls = expected.Split('|');
        var actualControls = actual.Split('|');
        Assert.Equal(expectedControls.Length, actualControls.Length);
        for (var index = 0; index < expectedControls.Length; index++)
        {
            var expectedParts = expectedControls[index].Split(':');
            var actualParts = actualControls[index].Split(':');
            Assert.Equal(expectedParts[0], actualParts[0]);
            var expectedValues = expectedParts[1].Split(',').Select(ParseDip).ToArray();
            var actualValues = actualParts[1].Split(',').Select(ParseDip).ToArray();
            Assert.Equal(expectedValues.Length, actualValues.Length);
            for (var valueIndex = 0; valueIndex < expectedValues.Length; valueIndex++)
            {
                Assert.True(Math.Abs(expectedValues[valueIndex] - actualValues[valueIndex]) <= tolerance,
                    $"{state} changed {expectedParts[0]} geometry component {valueIndex}: " +
                    $"{expectedValues[valueIndex]:0.##} -> {actualValues[valueIndex]:0.##} DIP (limit {tolerance:0.##}).");
            }
        }
    }

    private static double ParseDip(string value) => double.Parse(value, CultureInfo.InvariantCulture);

    private static double CenterY(FrameworkElement element, FrameworkElement ancestor)
    {
        var bounds = GetBounds(element, ancestor);
        return bounds.Top + bounds.Height / 2;
    }

    private static double ContentCenterY(Control control, FrameworkElement ancestor)
    {
        Rect localBounds;
        if (control is TextBox textBox)
        {
            localBounds = textBox.GetRectFromCharacterIndex(0);
            Assert.False(localBounds.IsEmpty, $"{control.GetType().Name} did not expose its first rendered character geometry.");
        }
        else if (control is ComboBox comboBox)
        {
            var selectedText = FindVisualChildren<TextBlock>(comboBox).FirstOrDefault(block => !string.IsNullOrWhiteSpace(block.Text));
            if (selectedText is not null)
            {
                var selectedTextBounds = GetBounds(selectedText, ancestor);
                return selectedTextBounds.Top + selectedTextBounds.Height / 2;
            }

            var presenter = FindVisualChildren<ContentPresenter>(comboBox).FirstOrDefault();
            Assert.True(presenter is not null, "The production filter ComboBox did not apply its selection content presenter.");
            var presenterBounds = GetBounds(presenter!, ancestor);
            return presenterBounds.Top + presenterBounds.Height / 2;
        }
        else
        {
            var text = FindVisualChildren<TextBlock>(control).FirstOrDefault(block => !string.IsNullOrWhiteSpace(block.Text));
            if (text is not null)
            {
                var textBounds = GetBounds(text, ancestor);
                return textBounds.Top + textBounds.Height / 2;
            }

            var icon = FindVisualChildren<ThemeAwareIcon>(control).FirstOrDefault();
            if (icon is not null)
            {
                var iconBounds = GetBounds(icon, ancestor);
                return iconBounds.Top + iconBounds.Height / 2;
            }

            var presenter = FindVisualChildren<ContentPresenter>(control).FirstOrDefault();
            Assert.True(presenter is not null,
                $"{control.GetType().Name} had no rendered text, content presenter, or icon; " +
                $"content={(control is ContentControl contentControl ? Convert.ToString(contentControl.Content) : "<not-content-control>")}; " +
                $"visuals={string.Join(",", FindVisualChildren<DependencyObject>(control).Select(item => item.GetType().Name))}");
            var presenterBounds = GetBounds(presenter!, ancestor);
            return presenterBounds.Top + presenterBounds.Height / 2;
        }

        var transform = control.TransformToAncestor(ancestor);
        var bounds = transform.TransformBounds(localBounds);
        return bounds.Top + bounds.Height / 2;
    }

    private static Rect GetBounds(FrameworkElement element, FrameworkElement ancestor)
        => element.TransformToAncestor(ancestor).TransformBounds(
            new Rect(new Point(0, 0), new Size(element.ActualWidth, element.ActualHeight)));

    private static void AssertGeometryNear(double actual, double expected, double tolerance, string message)
        => Assert.True(Math.Abs(actual - expected) <= tolerance,
            $"{message}; expected {expected:0.###} ± {tolerance:0.###} DIP");

    private static T GetField<T>(TaskCenterView view, string name)
        where T : class
        => (T)(typeof(TaskCenterView).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(view)
            ?? throw new InvalidOperationException($"Missing TaskCenterView field {name}."));

    private static T? FindVisualChild<T>(DependencyObject root)
        where T : DependencyObject
        => FindVisualChildren<T>(root).FirstOrDefault();

    private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
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
        => Assert.IsType<ResourceDictionary>(XamlReader.Parse(@"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""><ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/Redesign.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/AcrylicProductionResources.xaml""/>
</ResourceDictionary.MergedDictionaries></ResourceDictionary>"));

    private sealed class TaskToolbarFixture
    {
        public string TaskSearchText { get; set; } = "搜索任务";
        public string TaskStatusFilter { get; set; } = "全部";
        public string TaskTypeFilter { get; set; } = "全部";
        public string TaskHistoryScope { get; set; } = "全部时间";
        public string TaskHistoryRange { get; set; } = "全部时间";
        public string TaskFilterPresetNameDraft { get; set; } = "我的预设";
        public TaskFilterPreset? SelectedTaskFilterPreset { get; set; }
        public ObservableCollection<string> TaskStatusFilterOptions { get; } = new() { "全部", "失败" };
        public ObservableCollection<string> TaskTypeFilterOptions { get; } = new() { "全部", "媒体同步" };
        public ObservableCollection<string> TaskHistoryScopeOptions { get; } = new() { "全部时间", "最近" };
        public ObservableCollection<string> TaskHistoryRangeOptions { get; } = new() { "全部时间", "30 天" };
        public ObservableCollection<TaskFilterPreset> TaskFilterPresets { get; } = new()
        {
            new TaskFilterPreset { Name = "默认" },
            new TaskFilterPreset { Name = "失败任务" }
        };
    }

    private sealed class TaskFilterPreset
    {
        public string Name { get; set; } = string.Empty;
    }
}
