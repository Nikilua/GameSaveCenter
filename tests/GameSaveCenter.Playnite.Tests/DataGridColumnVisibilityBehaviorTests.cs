using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using Newtonsoft.Json;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("R09FocusWpf")]
public sealed class DataGridColumnVisibilityBehaviorTests
{
    [Fact]
    public void OptionalColumnsPersistByStableKeyWhileIdentityAndStateRemainVisible()
    {
        RunSta(() =>
        {
            var settings = new GameSaveCenterSettings();
            var persistCount = 0;
            var firstGrid = CreateGrid();
            var firstButton = CreateButtonWithProductionResources();
            using (var controller = CreateController(firstGrid, firstButton, settings, () => persistCount++))
            {
                Assert.NotNull(controller.Menu.Style);
                Assert.All(controller.Menu.Items.OfType<MenuItem>(), item => Assert.NotNull(item.Style));
                var picker = Assert.IsType<MenuItem>(controller.Menu.Items
                    .OfType<MenuItem>()
                    .Single(item => AutomationProperties.GetName(item) == "备注"));
                Toggle(picker, false);

                Assert.Equal(Visibility.Collapsed, firstGrid.Columns[4].Visibility);
                Assert.Equal(1, persistCount);
                Assert.True(settings.TryGetDataGridHiddenColumnKeys("tasks", out var hiddenKeys));
                Assert.Equal(new[] { "detail" }, hiddenKeys);

                var requiredTime = GetColumnItem(controller, "时间（必要列）");
                var requiredTask = GetColumnItem(controller, "任务（必要列）");
                var requiredState = GetColumnItem(controller, "状态（必要列）");
                Assert.False(requiredTime.IsEnabled);
                Assert.False(requiredTask.IsEnabled);
                Assert.False(requiredState.IsEnabled);
                Assert.All(new[] { requiredTime, requiredTask, requiredState }, item =>
                {
                    Assert.True(ToolTipService.GetShowOnDisabled(item));
                    Assert.Equal(AutomationProperties.GetHelpText(item), item.ToolTip);
                });
                Assert.Null(picker.ToolTip);
                Assert.True(requiredTime.IsChecked);
                Assert.True(requiredTask.IsChecked);
                Assert.True(requiredState.IsChecked);

                Toggle(requiredTime, false);
                Assert.Equal(Visibility.Visible, firstGrid.Columns[0].Visibility);
            }

            var restoredGrid = CreateGrid();
            using (CreateController(restoredGrid, CreateButtonWithProductionResources(), settings, () => { }))
            {
                Assert.Equal(Visibility.Collapsed, restoredGrid.Columns[4].Visibility);
                Assert.Equal(Visibility.Visible, restoredGrid.Columns[0].Visibility);
                Assert.Equal(Visibility.Visible, restoredGrid.Columns[1].Visibility);
                Assert.Equal(Visibility.Visible, restoredGrid.Columns[3].Visibility);
            }
        });
    }

    [Fact]
    public void ResetRestoresAllColumnsAndOnlyRemovesCurrentGridPreference()
    {
        RunSta(() =>
        {
            var settings = new GameSaveCenterSettings
            {
                DataGridHiddenColumnKeys = new Dictionary<string, List<string>>
                {
                    ["v1/tasks"] = new List<string> { "detail", "obsolete" },
                    ["v1/save-history"] = new List<string> { "note" }
                }
            };
            var grid = CreateGrid();
            var persistCount = 0;

            using (var controller = CreateController(grid, CreateButtonWithProductionResources(), settings, () => persistCount++))
            {
                Assert.Equal(Visibility.Collapsed, grid.Columns[4].Visibility);
                Assert.Equal(Visibility.Visible, grid.Columns[0].Visibility);
                var reset = GetColumnItem(controller, "恢复默认列显示");
                reset.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent, reset));

                Assert.All(grid.Columns, column => Assert.Equal(Visibility.Visible, column.Visibility));
                Assert.False(settings.TryGetDataGridHiddenColumnKeys("tasks", out _));
                Assert.True(settings.TryGetDataGridHiddenColumnKeys("save-history", out var otherViewKeys));
                Assert.Equal(new[] { "note" }, otherViewKeys);
                Assert.Equal(1, persistCount);
            }
        });
    }

    [Fact]
    public void ColumnMenuNavigationKeepsEnabledItemsInOrderAndCycles()
    {
        RunSta(() =>
        {
            var grid = CreateGrid();
            var button = CreateButtonWithProductionResources();
            var content = new StackPanel();
            content.Children.Add(button);
            content.Children.Add(grid);
            var window = new Window
            {
                Content = content,
                Width = 640,
                Height = 420,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };

            using (var controller = CreateController(grid, button, new GameSaveCenterSettings(), () => { }))
            {
                try
                {
                    window.Show();
                    window.Activate();
                    Assert.True(button.Focus());
                    Assert.Same(button, Keyboard.FocusedElement);
                    window.UpdateLayout();
                    controller.Menu.IsOpen = true;
                    window.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));

                    var menuItems = controller.Menu.Items.OfType<MenuItem>().ToArray();
                    Assert.Equal(
                        new[] { "时间（必要列）", "任务（必要列）", "阶段", "游戏", "备注", "状态（必要列）", "恢复默认列显示" },
                        menuItems.Select(AutomationProperties.GetName));
                    var presenter = Assert.Single(FindVisualChildren<ItemsPresenter>(controller.Menu));
                    Assert.Equal(KeyboardNavigationMode.Cycle, KeyboardNavigation.GetDirectionalNavigation(presenter));
                    Assert.All(new[] { menuItems[0], menuItems[1], menuItems[5] }, item =>
                    {
                        Assert.False(item.IsEnabled);
                        Assert.True(ToolTipService.GetShowOnDisabled(item));
                        Assert.False(string.IsNullOrWhiteSpace(AutomationProperties.GetHelpText(item)));
                    });
                    Assert.Single(controller.Menu.Items.OfType<Separator>());
                    Assert.All(menuItems.Where(item => item.IsEnabled), item =>
                    {
                        Assert.True(item.Focusable);
                        Assert.False(string.IsNullOrWhiteSpace(AutomationProperties.GetName(item)));
                    });
                }
                finally
                {
                    controller.Menu.IsOpen = false;
                    window.Close();
                }
            }
        });
    }

    [Fact]
    public void HiddenColumnPreferencesRoundTripAndResetWithLocalUiPreferences()
    {
        var settings = new GameSaveCenterSettings
        {
            DataGridHiddenColumnKeys = new Dictionary<string, List<string>>
            {
                ["v1/tasks"] = new List<string> { "detail", " detail ", "", new string('x', 81) }
            }
        };

        var restored = JsonConvert.DeserializeObject<GameSaveCenterSettings>(JsonConvert.SerializeObject(settings));
        Assert.NotNull(restored);
        Assert.True(restored!.TryGetDataGridHiddenColumnKeys("tasks", out var hiddenKeys));
        Assert.Equal(new[] { "detail" }, hiddenKeys);

        SettingsResetCatalog.ResetAll(restored);
        Assert.Empty(restored.DataGridHiddenColumnKeys);
    }

    [Fact]
    public void ProductionTablesExposeThePickerAndSharedThemeResources()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var save = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
        var saveCode = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml.cs"));
        var tasks = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));
        var taskCode = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml.cs"));
        var styles = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "WpfUiProduction.xaml"));
        var controller = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Infrastructure", "DataGridColumnVisibilityController.cs"));

        Assert.Contains("x:Name=\"SaveHistoryColumnVisibilityButton\"", save);
        Assert.Contains("设置存档历史表格显示列", save);
        Assert.Contains("new[] { \"time\", \"type\", \"state\" }", saveCode);
        Assert.Contains("x:Name=\"SaveCandidateColumnVisibilityButton\"", save);
        Assert.Contains("设置存档候选路径表格显示列", save);
        Assert.Contains("new[] { \"status\", \"path\" }", saveCode);
        Assert.Contains("x:Name=\"TaskGridColumnVisibilityButton\"", tasks);
        Assert.Contains("设置任务列表显示列", tasks);
        Assert.Contains("new[] { \"local-time\", \"task\", \"state\" }", taskCode);
        Assert.Contains("GscColumnPickerContextMenuStyle", styles);
        Assert.Contains("GscColumnPickerMenuItemStyle", styles);
        Assert.Contains("<ControlTemplate TargetType=\"ContextMenu\">", styles);
        Assert.Contains("CornerRadius=\"12\"", styles.Substring(styles.IndexOf("x:Key=\"GscColumnPickerContextMenuStyle\"", StringComparison.Ordinal)));
        Assert.Contains("Grid.ColumnDefinitions", styles.Substring(styles.IndexOf("x:Key=\"GscColumnPickerMenuItemStyle\"", StringComparison.Ordinal)));
        Assert.Contains("AnimateMenuEntrance();", controller);
        Assert.Contains("GscToggleMotionEnabled", controller);
        Assert.DoesNotContain("MediaInboxColumnVisibilityButton", System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml")));
    }

    private static DataGridColumnVisibilityController CreateController(
        DataGrid grid,
        Button button,
        GameSaveCenterSettings settings,
        Action persist)
        => new DataGridColumnVisibilityController(
            grid,
            button,
            "tasks",
            new[] { "local-time", "task", "stage", "game", "detail", "state" },
            new[] { "local-time", "task", "state" },
            settings,
            persist);

    private static DataGrid CreateGrid()
    {
        var grid = new DataGrid();
        foreach (var header in new[] { "时间", "任务", "阶段", "游戏", "备注", "状态" })
            grid.Columns.Add(new DataGridTextColumn { Header = header });
        return grid;
    }

    private static Button CreateButtonWithProductionResources()
    {
        var button = new Button { Content = "显示列" };
        button.Resources.MergedDictionaries.Add(LoadProductionResources());
        return button;
    }

    private static ResourceDictionary LoadProductionResources()
        => Assert.IsType<ResourceDictionary>(System.Windows.Markup.XamlReader.Parse(@"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""><ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/Redesign.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/AcrylicProductionResources.xaml""/>
</ResourceDictionary.MergedDictionaries></ResourceDictionary>"));

    private static MenuItem GetColumnItem(DataGridColumnVisibilityController controller, string name)
        => Assert.IsType<MenuItem>(controller.Menu.Items
            .OfType<MenuItem>()
            .Single(item => AutomationProperties.GetName(item) == name));

    private static void Toggle(MenuItem item, bool isChecked)
    {
        item.IsChecked = isChecked;
        item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent, item));
    }

    private static T[] FindVisualChildren<T>(DependencyObject root) where T : DependencyObject
    {
        var matches = new List<T>();
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match) matches.Add(match);
            matches.AddRange(FindVisualChildren<T>(child));
        }
        return matches.ToArray();
    }

    private static void RunSta(Action action)
    {
        // The isolated test runner gives this WPF behavior class its own process.
        // These checks cover keyboard navigation and UI Automation, not stylus or
        // touch input; keeping WPF's hardware stylus stack out avoids a crash in
        // Popup mouse-capture transitions on stylus-enabled hosts.
        AppContext.SetSwitch("Switch.System.Windows.Input.Stylus.DisableStylusAndTouchSupport", true);

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
}
