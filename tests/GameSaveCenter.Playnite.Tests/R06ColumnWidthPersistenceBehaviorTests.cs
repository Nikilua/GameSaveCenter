using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R06ColumnWidthPersistenceBehaviorTests
{
    [Fact]
    public void UserPixelWidthsRestorePerViewAfterControllerRecreation()
    {
        RunSta(() =>
        {
            var settings = new GameSaveCenterSettings();
            var persistCount = 0;
            var firstGrid = CreateGrid(2);
            using (var firstController = new DataGridColumnLayoutController(
                firstGrid,
                "tasks",
                new[] { "primary", "secondary" },
                settings,
                () => persistCount++))
            {
                firstGrid.Columns[0].Width = new DataGridLength(248, DataGridLengthUnitType.Pixel);
                firstController.FlushPendingPersistence();
                Assert.Equal(1, persistCount);
            }

            Assert.True(settings.DataGridColumnWidths.ContainsKey("v1/tasks/primary"));
            Assert.False(settings.DataGridColumnWidths.ContainsKey("v1/other-view/primary"));

            var restoredGrid = CreateGrid(2);
            using (var restoredController = new DataGridColumnLayoutController(
                restoredGrid,
                "tasks",
                new[] { "primary", "secondary" },
                settings,
                () => { }))
            {
                Assert.Equal(DataGridLengthUnitType.Pixel, restoredGrid.Columns[0].Width.UnitType);
                Assert.Equal(248, restoredGrid.Columns[0].Width.Value, 3);
                Assert.Equal(DataGridLengthUnitType.Star, restoredGrid.Columns[1].Width.UnitType);
            }

            var otherViewGrid = CreateGrid(2);
            using (var otherViewController = new DataGridColumnLayoutController(
                otherViewGrid,
                "media-inbox",
                new[] { "primary", "secondary" },
                settings,
                () => { }))
            {
                Assert.Equal(DataGridLengthUnitType.Pixel, otherViewGrid.Columns[0].Width.UnitType);
                Assert.Equal(120, otherViewGrid.Columns[0].Width.Value, 3);
            }
        });
    }

    [Fact]
    public void InvalidVersionAndUnknownKeysAreIgnoredAndStoredWidthRespectsMinimum()
    {
        RunSta(() =>
        {
            var settings = new GameSaveCenterSettings
            {
                DataGridColumnWidths = new Dictionary<string, double>
                {
                    ["v0/tasks/primary"] = 512,
                    ["v1/tasks/obsolete"] = 512,
                    ["v1/tasks/primary"] = 12
                }
            };
            var grid = CreateGrid(2);
            grid.MinColumnWidth = 64;
            grid.Columns[0].MinWidth = 128;

            using (var controller = new DataGridColumnLayoutController(
                grid,
                "tasks",
                new[] { "primary", "secondary" },
                settings,
                () => { }))
            {
                Assert.Equal(DataGridLengthUnitType.Pixel, grid.Columns[0].Width.UnitType);
                Assert.Equal(128, grid.Columns[0].Width.Value, 3);
                Assert.Equal(DataGridLengthUnitType.Star, grid.Columns[1].Width.UnitType);
            }
        });
    }

    [Fact]
    public void ResetRestoresResponsiveDefaultsAndRemovesOnlyCurrentViewKeys()
    {
        RunSta(() =>
        {
            var settings = new GameSaveCenterSettings();
            var grid = CreateGrid(2);
            var persistCount = 0;
            using (var controller = new DataGridColumnLayoutController(
                grid,
                "save-history",
                new[] { "time", "note" },
                settings,
                () => persistCount++))
            {
                var defaultFirst = grid.Columns[0].Width;
                var defaultSecond = grid.Columns[1].Width;
                grid.Columns[0].Width = new DataGridLength(300, DataGridLengthUnitType.Pixel);
                grid.Columns[1].Width = new DataGridLength(360, DataGridLengthUnitType.Pixel);
                controller.FlushPendingPersistence();
                controller.ResetToDefaults();

                Assert.Equal(defaultFirst, grid.Columns[0].Width);
                Assert.Equal(defaultSecond, grid.Columns[1].Width);
                Assert.False(settings.DataGridColumnWidths.ContainsKey("v1/save-history/time"));
                Assert.False(settings.DataGridColumnWidths.ContainsKey("v1/save-history/note"));
                Assert.Equal(2, persistCount);
            }

            settings.SetDataGridColumnWidth("tasks", "primary", 220);
            Assert.True(settings.DataGridColumnWidths.ContainsKey("v1/tasks/primary"));
        });
    }

    [Fact]
    public void NarrowWindowKeepsMinimumColumnReachableThroughExistingHorizontalScroll()
    {
        RunSta(() =>
        {
            EnsureApplicationResources();
            var settings = new GameSaveCenterSettings
            {
                DataGridColumnWidths = new Dictionary<string, double>
                {
                    ["v1/tasks/primary"] = 180,
                    ["v1/tasks/secondary"] = 180
                }
            };
            var grid = CreateGrid(2);
            grid.MinColumnWidth = 64;
            grid.Columns[0].MinWidth = 128;
            grid.Columns[1].MinWidth = 128;
            grid.Width = 220;
            grid.Height = 110;
            grid.ItemsSource = new[] { new GridRow { Value = "合成行" } };
            var window = new Window
            {
                Content = grid,
                Width = 240,
                Height = 140,
                ShowInTaskbar = false,
                ShowActivated = false,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };

            try
            {
                using (var controller = new DataGridColumnLayoutController(
                    grid,
                    "tasks",
                    new[] { "primary", "secondary" },
                    settings,
                    () => { }))
                {
                    window.Show();
                    window.UpdateLayout();
                    DrainDispatcher();
                    var scrollViewer = FindVisualChild<ScrollViewer>(grid);
                    Assert.NotNull(scrollViewer);
                    Assert.True(grid.Columns[0].ActualWidth >= 128, $"主列宽度={grid.Columns[0].ActualWidth}");
                    Assert.Equal(Visibility.Visible, scrollViewer!.ComputedHorizontalScrollBarVisibility);
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void ProductionTablesExposeStableScopesAndResetActions()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var save = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
        var saveCode = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml.cs"));
        var task = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));
        var taskCode = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml.cs"));
        var media = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
        var mediaCode = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml.cs"));
        var settings = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettings.cs"));
        var controller = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Infrastructure", "DataGridColumnLayoutController.cs"));

        Assert.Contains("x:Name=\"SaveHistoryGrid\"", save);
        Assert.Contains("x:Name=\"SaveCandidateGrid\"", save);
        Assert.Contains("Click=\"OnResetColumnLayoutClick\"", save);
        Assert.Contains("\"save-history\"", saveCode);
        Assert.Contains("\"save-candidates\"", saveCode);
        Assert.Contains("x:Name=\"TaskGrid\"", task);
        Assert.Contains("Click=\"OnResetColumnLayoutClick\"", task);
        Assert.Contains("\"tasks\"", taskCode);
        Assert.Contains("x:Name=\"MediaInboxGrid\"", media);
        Assert.Contains("Click=\"OnResetColumnLayoutClick\"", media);
        Assert.Contains("\"media-inbox\"", mediaCode);
        Assert.Contains("DataGridColumnWidths", settings);
        Assert.Contains("\"v1/\"", settings);
        Assert.Contains("BeginLayoutPass", controller);
        Assert.Contains("EndLayoutPass", controller);
        Assert.Contains("DataGridLengthUnitType.Pixel", controller);
    }

    [Fact]
    public void TaskColumnPersistenceKeysMatchTheProductionGridColumns()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var taskXamlPath = Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml");
        var taskCodePath = Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml.cs");
        var document = XDocument.Load(taskXamlPath);
        var xName = XName.Get("Name", "http://schemas.microsoft.com/winfx/2006/xaml");
        var taskGrid = document.Descendants().Single(element =>
            element.Name.LocalName == "DataGrid"
            && string.Equals((string?)element.Attribute(xName), "TaskGrid", StringComparison.Ordinal));
        var columns = taskGrid.Elements().Single(element => element.Name.LocalName == "DataGrid.Columns").Elements().Count();
        var taskCode = File.ReadAllText(taskCodePath);
        var keyMatch = Regex.Match(
            taskCode,
            "\\\"tasks\\\"\\s*,\\s*new\\[\\]\\s*\\{(?<keys>.*?)\\}",
            RegexOptions.Singleline);

        Assert.True(keyMatch.Success, "TaskCenterView must declare a stable key list for the tasks grid.");
        var keys = Regex.Matches(keyMatch.Groups["keys"].Value, "\\\"(?<key>[^\\\"]+)\\\"")
            .Cast<Match>()
            .Select(match => match.Groups["key"].Value)
            .ToArray();
        Assert.Equal(columns, keys.Length);
        Assert.Equal(new[] { "local-time", "task", "stage", "game", "state", "progress", "detail" }, keys);
    }

    private static DataGrid CreateGrid(int columnCount)
    {
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            CanUserResizeColumns = true
        };
        ScrollViewer.SetHorizontalScrollBarVisibility(grid, ScrollBarVisibility.Auto);
        ScrollViewer.SetVerticalScrollBarVisibility(grid, ScrollBarVisibility.Auto);
        for (var index = 0; index < columnCount; index++)
        {
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = "列" + index,
                Binding = new Binding(nameof(GridRow.Value)),
                Width = index == columnCount - 1
                    ? new DataGridLength(1, DataGridLengthUnitType.Star)
                    : new DataGridLength(120, DataGridLengthUnitType.Pixel)
            });
        }
        return grid;
    }

    private static T? FindVisualChild<T>(DependencyObject root) where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match) return match;
            var nested = FindVisualChild<T>(child);
            if (nested != null) return nested;
        }
        return null;
    }

    private static void EnsureApplicationResources()
    {
        var application = Application.Current ?? new Application();
        if (!application.Resources.Contains("BaseTextBlockStyle"))
            application.Resources.Add("BaseTextBlockStyle", new Style(typeof(TextBlock)));
    }

    private static void DrainDispatcher()
    {
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
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

    private sealed class GridRow
    {
        public string Value { get; set; } = string.Empty;
    }
}
