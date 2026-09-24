using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("R23ProductionResourcesWpf")]
public sealed class R23ProductionResourceStateBehaviorTests
{
    [Fact]
    public void AcrylicNavigationExposesFocusAndDisabledStatesAfterSelectionClearsInBothThemes()
    {
        RunSta(() =>
        {
            var resources = LoadProductionResources();
            var root = new Grid { Resources = resources };
            var navigation = new RadioButton
            {
                Style = Assert.IsType<Style>(resources["AcrylicNavItem"]),
                Content = "存档中心",
                Width = 180,
                Height = 48
            };
            var external = new TextBox { Width = 120, Height = 32, Margin = new Thickness(0, 60, 0, 0) };
            root.Children.Add(navigation);
            root.Children.Add(external);
            var window = CreateWindow(root, 320, 160);

            try
            {
                window.Show();
                FlushLayout(window);
                navigation.ApplyTemplate();
                var chrome = Assert.IsType<Border>(navigation.Template.FindName("NavChrome", navigation));
                Color? lightPrimaryText = null;

                foreach (var mode in Themes)
                {
                    ApplyTheme(root, mode);
                    var primaryText = BrushColor(resources["GscPrimaryTextBrush"]);
                    if (mode == GameSaveCenterThemeMode.Light)
                        lightPrimaryText = primaryText;
                    else
                        Assert.NotEqual(lightPrimaryText, primaryText);
                    navigation.IsEnabled = true;
                    navigation.IsChecked = false;
                    FlushLayout(window);

                    Assert.Equal(Colors.Transparent, BrushColor(chrome.Background));
                    Assert.Equal(1d, chrome.Opacity);
                    Assert.Equal(new Thickness(1), chrome.BorderThickness);

                    navigation.IsChecked = true;
                    FlushLayout(window);
                    Assert.Equal(BrushColor(resources["GscAccentTintStrongBrush"]), BrushColor(chrome.Background));
                    Assert.Equal(BrushColor(resources["GscAccentTintStrongBrush"]), BrushColor(chrome.BorderBrush));

                    navigation.IsChecked = false;
                    Assert.Same(navigation, Keyboard.Focus(navigation));
                    FlushLayout(window);
                    Assert.True(navigation.IsKeyboardFocusWithin);
                    Assert.Same(resources["GscSharedFocusVisual"], navigation.FocusVisualStyle);
                    Assert.Equal(BrushColor(resources["GscAccentBrush"]), BrushColor(chrome.BorderBrush));
                    Assert.Equal(new Thickness(2), chrome.BorderThickness);

                    Assert.Same(external, Keyboard.Focus(external));
                    FlushLayout(window);
                    Assert.False(navigation.IsKeyboardFocusWithin);
                    Assert.Equal(Colors.Transparent, BrushColor(chrome.BorderBrush));
                    Assert.Equal(new Thickness(1), chrome.BorderThickness);

                    navigation.IsEnabled = false;
                    FlushLayout(window);
                    Assert.False(navigation.IsEnabled);
                    Assert.Equal(0.46d, chrome.Opacity);
                    Assert.NotSame(navigation, Keyboard.Focus(navigation));
                    Assert.Equal(0.46d, chrome.Opacity);
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void SettingsSectionTabsUseTheirGeneratedStyleForSelectionFocusAndDisabledNegativeInBothThemes()
    {
        RunSta(() =>
        {
            var resources = LoadProductionResources();
            var root = new Grid { Resources = resources };
            var tabs = new ListBox
            {
                Style = Assert.IsType<Style>(resources["GscSettingsSectionTabs"]),
                ItemsSource = new[] { "常规", "存档" },
                Width = 240,
                Height = 150,
                SelectedIndex = 0
            };
            var external = new TextBox { Width = 120, Height = 32, Margin = new Thickness(0, 170, 0, 0) };
            root.Children.Add(tabs);
            root.Children.Add(external);
            var window = CreateWindow(root, 320, 250);

            try
            {
                window.Show();
                FlushLayout(window);
                var first = Assert.IsType<ListBoxItem>(tabs.ItemContainerGenerator.ContainerFromIndex(0));
                var second = Assert.IsType<ListBoxItem>(tabs.ItemContainerGenerator.ContainerFromIndex(1));
                first.ApplyTemplate();
                second.ApplyTemplate();
                var firstChrome = Assert.IsType<Border>(first.Template.FindName("TabChrome", first));
                var secondChrome = Assert.IsType<Border>(second.Template.FindName("TabChrome", second));
                Color? lightPrimaryText = null;

                foreach (var mode in Themes)
                {
                    ApplyTheme(root, mode);
                    var primaryText = BrushColor(resources["GscPrimaryTextBrush"]);
                    if (mode == GameSaveCenterThemeMode.Light)
                        lightPrimaryText = primaryText;
                    else
                        Assert.NotEqual(lightPrimaryText, primaryText);
                    tabs.SelectedIndex = 0;
                    first.IsEnabled = true;
                    second.IsEnabled = true;
                    FlushLayout(window);

                    Assert.Equal(BrushColor(resources["GscAccentTintBrush"]), BrushColor(firstChrome.Background));
                    Assert.NotEqual(BrushColor(resources["GscAccentTintBrush"]), BrushColor(secondChrome.Background));
                    Assert.Same(resources["GscSettingsSectionTabItem"], first.Style);
                    Assert.Same(resources["GscSharedFocusVisual"], first.FocusVisualStyle);

                    Assert.Same(first, Keyboard.Focus(first));
                    FlushLayout(window);
                    Assert.True(first.IsKeyboardFocusWithin);
                    Assert.Same(external, Keyboard.Focus(external));
                    FlushLayout(window);
                    Assert.False(first.IsKeyboardFocusWithin);

                    tabs.SelectedIndex = 0;
                    second.IsEnabled = false;
                    FlushLayout(window);
                    Assert.Equal(0, tabs.SelectedIndex);
                    Assert.Equal(0.45d, secondChrome.Opacity);
                    Assert.Equal(BrushColor(resources["GscDisabledTextBrush"]), BrushColor(second.Foreground));
                    Assert.NotSame(second, Keyboard.Focus(second));
                    Assert.Equal(0.45d, secondChrome.Opacity);
                    Assert.Equal(BrushColor(resources["GscAccentTintBrush"]), BrushColor(firstChrome.Background));
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void EachProductionPageGridKeepsSelectedFocusAndDisabledRowStatesAcrossThemes()
    {
        RunSta(() =>
        {
            var cases = new (string Name, Func<UserControl> Create, string Field, string StyleKey, string RowStyleKey)[]
            {
                ("Task", () => new TaskCenterView(), "TaskGrid", "TaskDataGrid", "GscStableDataGridRow"),
                ("Media Inbox", () => new MediaCenterView(), "MediaInboxGrid", "MediaDataGrid", "MediaInboxStableRowStyle"),
                ("Save", () => new SaveCenterView(), "SaveHistoryGrid", "SaveDataGrid", "GscStableDataGridRow"),
                ("Maintenance", () => new MaintenanceView(), "FindingsGrid", "MaintenanceDataGrid", "GscStableDataGridRow")
            };
            var lightPrimaryTextByPage = new Dictionary<string, Color>(StringComparer.Ordinal);

            foreach (var theme in Themes)
            {
                foreach (var fixture in cases)
                {
                    var page = fixture.Create();
                    var grid = Assert.IsType<DataGrid>(page.GetType()
                        .GetField(fixture.Field, BindingFlags.Instance | BindingFlags.NonPublic)!
                        .GetValue(page));
                    var rows = fixture.Name == "Media Inbox"
                        ? new ObservableCollection<object> { CreateMedia("one"), CreateMedia("two") }
                        : new ObservableCollection<object> { new ProbeRow("one"), new ProbeRow("two") };
                    grid.ItemsSource = rows;
                    ApplyTheme(page, theme);
                    var primaryText = BrushColor(page.Resources["GscPrimaryTextBrush"]);
                    if (theme == GameSaveCenterThemeMode.Light)
                        lightPrimaryTextByPage[fixture.Name] = primaryText;
                    else
                        Assert.NotEqual(lightPrimaryTextByPage[fixture.Name], primaryText);

                    var root = new Grid();
                    root.Children.Add(page);
                    var external = new TextBox
                    {
                        Width = 120,
                        Height = 28,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top
                    };
                    root.Children.Add(external);
                    var window = CreateWindow(root, 1280, 900);
                    try
                    {
                        window.Show();
                        FlushLayout(window);
                        Assert.NotNull(grid.Style);
                        Assert.Same(page.Resources[fixture.StyleKey], grid.Style);
                        Assert.Equal(typeof(DataGrid), grid.Style!.TargetType);
                        Assert.NotNull(grid.RowStyle);
                        Assert.Same(page.Resources[fixture.RowStyleKey], grid.RowStyle);
                        Assert.Equal(typeof(DataGridRow), grid.RowStyle!.TargetType);

                        var row = grid.ItemContainerGenerator.ContainerFromItem(rows[0]) as DataGridRow;
                        Assert.NotNull(row);
                        Assert.Same(external, Keyboard.Focus(external));
                        FlushLayout(window);
                        var unselectedGeometry = CaptureCellAndTextGeometry(row!, grid);

                        grid.SelectedItem = rows[0];
                        Assert.Same(external, Keyboard.Focus(external));
                        FlushLayout(window);
                        Assert.True(row!.IsSelected, $"{fixture.Name} did not apply selection to the realized row");
                        Assert.False(Selector.GetIsSelectionActive(row), $"{fixture.Name} selection should be inactive while focus is outside the grid");
                        var inactiveSelectionGeometry = CaptureCellAndTextGeometry(row, grid);
                        AssertRowGeometryUnchanged(unselectedGeometry, inactiveSelectionGeometry, fixture.Name, theme, "inactive selection");

                        var cell = FindVisualChildren<DataGridCell>(row)
                            .FirstOrDefault(candidate => candidate.IsVisible && candidate.ActualWidth > 0);
                        Assert.NotNull(cell);
                        Assert.Same(cell, Keyboard.Focus(cell));
                        FlushLayout(window);
                        Assert.True(row.IsKeyboardFocusWithin, $"{fixture.Name} row did not retain keyboard focus");
                        var keyboardFocusedGeometry = CaptureCellAndTextGeometry(row, grid);
                        AssertRowGeometryUnchanged(unselectedGeometry, keyboardFocusedGeometry, fixture.Name, theme, "keyboard-focused selection");
                        var chrome = Assert.IsType<Border>(row.Template.FindName("RowChrome", row));
                        Assert.Equal(BrushColor(page.Resources["GscAccentBrush"]), BrushColor(chrome.BorderBrush));
                        Assert.Equal(new Thickness(2), chrome.BorderThickness);

                        grid.IsEnabled = false;
                        FlushLayout(window);
                        Assert.False(row.IsEnabled, $"{fixture.Name} row remained enabled with its DataGrid disabled");
                        Assert.False(cell!.IsEnabled, $"{fixture.Name} cell remained enabled with its DataGrid disabled");
                        Assert.Equal(0.42d, row.Opacity);
                        Assert.Equal(1d, chrome.Opacity);
                        Assert.Same(external, Keyboard.Focus(external));
                        FlushLayout(window);
                        Assert.NotSame(cell, Keyboard.Focus(cell));
                    }
                    finally
                    {
                        window.Close();
                    }
                }
            }
        });
    }

    private static readonly GameSaveCenterThemeMode[] Themes =
    {
        GameSaveCenterThemeMode.Light,
        GameSaveCenterThemeMode.Dark
    };

    private static MediaItemDto CreateMedia(string id)
        => new()
        {
            MediaId = id,
            Kind = MediaKind.Screenshot,
            Source = MediaSourceKind.WindowsScreenshot,
            ArchivePath = "C:\\archive\\" + id + ".png",
            OriginalPath = "C:\\source\\" + id + ".png",
            Sha256 = id,
            CapturedUtc = DateTime.UtcNow,
            ClassificationState = "Assigned"
        };

    private static ResourceDictionary LoadProductionResources()
        => Assert.IsType<ResourceDictionary>(XamlReader.Parse(@"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""><ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/Redesign.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/AcrylicProductionResources.xaml""/>
</ResourceDictionary.MergedDictionaries></ResourceDictionary>"));

    private static void ApplyTheme(FrameworkElement host, GameSaveCenterThemeMode mode)
    {
        var palette = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(
            host,
            glassEnabled: true,
            strengthPercent: 78,
            themeMode: mode,
            highContrastOverride: false);
        AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(host.Resources, palette, glassEnabled: true, motionEnabled: true);
        host.UpdateLayout();
    }

    private static Window CreateWindow(UIElement content, double width, double height)
        => new()
        {
            Content = content,
            Width = width,
            Height = height,
            WindowStyle = WindowStyle.None,
            ShowInTaskbar = false,
            ShowActivated = true,
            Opacity = 0.01
        };

    private static Color BrushColor(object? value)
        => Assert.IsType<SolidColorBrush>(value).Color;

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
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

    private static Rect[] CaptureCellAndTextGeometry(DataGridRow row, DataGrid grid)
    {
        var cells = FindVisualChildren<DataGridCell>(row)
            .Where(cell => cell.Visibility == Visibility.Visible && cell.ActualWidth > 0 && cell.ActualHeight > 0)
            .OrderBy(cell => cell.Column.DisplayIndex)
            .ToArray();
        if (cells.Length == 0)
            throw new InvalidOperationException("The realized row has no visible cells to measure.");

        return cells.SelectMany(cell =>
        {
            var content = FindVisualChildren<TextBlock>(cell)
                .Where(text => text.Visibility == Visibility.Visible && text.ActualWidth > 0 && text.ActualHeight > 0)
                .Select(text => BoundsRelativeTo(text, grid));
            return new[] { BoundsRelativeTo(cell, grid) }.Concat(content);
        }).ToArray();
    }

    private static Rect BoundsRelativeTo(FrameworkElement element, FrameworkElement ancestor)
    {
        var origin = element.TransformToAncestor(ancestor).Transform(new Point(0, 0));
        return new Rect(origin.X, origin.Y, element.ActualWidth, element.ActualHeight);
    }

    private static void AssertRowGeometryUnchanged(Rect[] expected, Rect[] actual, string page, GameSaveCenterThemeMode theme, string state)
    {
        Assert.Equal(expected.Length, actual.Length);
        for (var index = 0; index < expected.Length; index++)
        {
            Assert.True(Math.Abs(expected[index].X - actual[index].X) <= 0.25,
                $"{page}/{theme} {state} shifted cell/content {index} horizontally from {expected[index]} to {actual[index]}.");
            Assert.True(Math.Abs(expected[index].Y - actual[index].Y) <= 0.25,
                $"{page}/{theme} {state} shifted cell/content {index} vertically from {expected[index]} to {actual[index]}.");
            Assert.True(Math.Abs(expected[index].Width - actual[index].Width) <= 0.25,
                $"{page}/{theme} {state} changed cell/content {index} width from {expected[index]} to {actual[index]}.");
            Assert.True(Math.Abs(expected[index].Height - actual[index].Height) <= 0.25,
                $"{page}/{theme} {state} changed cell/content {index} height from {expected[index]} to {actual[index]}.");
        }
    }

    private static void FlushLayout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
        window.UpdateLayout();
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

    private sealed class ProbeRow
    {
        public ProbeRow(string name) => Name = name;
        public string Name { get; }
    }
}

[CollectionDefinition("R23ProductionResourcesWpf", DisableParallelization = true)]
public sealed class R23ProductionResourcesWpfCollection
{
}
