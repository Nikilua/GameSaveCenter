using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Threading;
using System.Xml.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R06SelectionStateBehaviorTests
{
    [Fact]
    public void ProductionRowsKeepFailureInItsStatusCellWithoutReplacingSelectionChrome()
    {
        RunSta(() =>
        {
            EnsureApplicationResources();
            var resources = LoadProductionResources();
            var items = new ObservableCollection<TaskStatusDto>
            {
                Task("failed", TaskState.Failed),
                Task("succeeded", TaskState.Succeeded)
            };
            var grid = new DataGrid
            {
                AutoGenerateColumns = false,
                CanUserAddRows = false,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                Height = 150,
                Width = 520,
                RowStyle = (Style)resources["GscStableDataGridRow"],
                ItemsSource = items
            };
            grid.Columns.Add(new DataGridTextColumn { Header = "状态", Binding = new Binding("StateDisplay"), Width = 170 });
            grid.Columns.Add(new DataGridTextColumn { Header = "游戏", Binding = new Binding(nameof(TaskStatusDto.GameName)), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });

            var external = new TextBox { Text = "外部焦点" };
            var root = new StackPanel();
            root.Children.Add(grid);
            root.Children.Add(external);
            var window = new Window
            {
                Width = 640,
                Height = 240,
                Content = root,
                Resources = resources,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None
            };

            try
            {
                window.Show();
                PumpLayout(window);
                var failedRow = RowFor(grid, items[0]);
                var succeededRow = RowFor(grid, items[1]);
                Assert.NotNull(failedRow);
                Assert.NotNull(succeededRow);

                Assert.Same(external, Keyboard.Focus(external));
                PumpLayout(window);
                var failedNormalGeometry = CaptureCellAndTextGeometry(failedRow, grid);
                var succeededNormalGeometry = CaptureCellAndTextGeometry(succeededRow, grid);

                grid.SelectedItem = items[0];
                Keyboard.Focus(external);
                PumpLayout(window);
                var failedChrome = ChromeFor(failedRow);
                var failedBackground = BackgroundFor(failedRow);
                var failedSelectedGeometry = CaptureCellAndTextGeometry(failedRow, grid);
                var failedData = Assert.IsType<TaskStatusDto>(failedRow.DataContext);
                Assert.Equal(TaskState.Failed, failedData.State);
                Assert.False(Selector.GetIsSelectionActive(failedRow));
                Assert.Equal(BrushColor(resources["GscSelectionInactiveBrush"]), BrushColor(failedBackground.Background));
                Assert.NotEqual(BrushColor(resources["GscErrorTintBrush"]), BrushColor(failedBackground.Background));
                Assert.NotEqual(BrushColor(resources["GscErrorBrush"]), BrushColor(failedChrome.BorderBrush));
                Assert.Equal(new Thickness(4, 2, 4, 2), failedBackground.Margin);
                AssertGeometryUnchanged(failedNormalGeometry, failedSelectedGeometry, "inactive selection");

                grid.SelectedItem = items[1];
                succeededRow.Focusable = true;
                Assert.Same(succeededRow, Keyboard.Focus(succeededRow));
                PumpLayout(window);
                var activeChrome = ChromeFor(succeededRow);
                var activeBackground = BackgroundFor(succeededRow);
                var succeededFocusedGeometry = CaptureCellAndTextGeometry(succeededRow, grid);
                Assert.True(succeededRow.IsKeyboardFocusWithin);
                Assert.Equal(2, activeChrome.BorderThickness.Left);
                Assert.Equal(BrushColor(resources["GscAccentTintBrush"]), BrushColor(activeBackground.Background));
                Assert.Equal(new Thickness(3, 1, 4, 1), activeChrome.Margin);
                Assert.Equal(new Thickness(3, 1, 4, 1), activeBackground.Margin);
                AssertGeometryUnchanged(succeededNormalGeometry, succeededFocusedGeometry, "keyboard-focused selection");

                Keyboard.Focus(external);
                PumpLayout(window);
                var inactiveChrome = ChromeFor(succeededRow);
                var inactiveBackground = BackgroundFor(succeededRow);
                Assert.False(Selector.GetIsSelectionActive(succeededRow));
                Assert.Equal(BrushColor(resources["GscSelectionInactiveBrush"]), BrushColor(inactiveBackground.Background));
                Assert.Equal(BrushColor(resources["GscMutedStatusBrush"]), BrushColor(inactiveChrome.BorderBrush));

                VerifyDashboardCompatibilityRow();
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void SharedStateContractLeavesCellContentSurfaceForStatusBadges()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var production = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "WpfUiProduction.xaml"));
        var tokens = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "DesignTokens.xaml"));
        var media = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));

        Assert.Contains("Selector.IsSelectionActive", production);
        Assert.Contains("Path=DataContext.State", production);
        Assert.Contains("GscSelectionInactiveBrush", tokens);
        Assert.Contains("GscErrorTintBrush", production);
        Assert.DoesNotContain("<Trigger Property=\"IsMouseOver\" Value=\"True\">\n                        <Setter Property=\"Background\" Value=\"{DynamicResource GscAccentTintStrongBrush}\"/>", media);
        Assert.Contains("CellChrome\" Property=\"Background\" Value=\"Transparent\"", production);
    }

    [Fact]
    public void DashboardCompatibilityRowOverlayContractKeepsContentAsSibling()
    {
        AssertDashboardRowOverlayContract();
    }

    private static void VerifyDashboardCompatibilityRow()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        EnsureApplicationResources();
        var resources = LoadDashboardDataGridResources();
        var rows = new ObservableCollection<TaskStatusDto>
        {
            Task("dashboard-row", TaskState.Running),
            Task("dashboard-row-2", TaskState.Succeeded)
        };
        var grid = new DataGrid
        {
            AutoGenerateColumns = false,
            CanUserAddRows = false,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            SelectionMode = DataGridSelectionMode.Single,
            SelectionUnit = DataGridSelectionUnit.FullRow,
            Height = 150,
            Width = 520,
            RowStyle = Assert.IsType<Style>(resources["DashboardCompatibilityDataGridRow"]),
            ItemsSource = rows
        };
        grid.Columns.Add(new DataGridTextColumn { Header = "状态", Binding = new Binding("StateDisplay"), Width = 170 });
        grid.Columns.Add(new DataGridTextColumn { Header = "游戏", Binding = new Binding(nameof(TaskStatusDto.GameName)), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });

        var external = new TextBox { Text = "外部焦点" };
        var root = new StackPanel { Resources = resources };
        root.Children.Add(grid);
        root.Children.Add(external);
        var window = new Window
        {
            Width = 640,
            Height = 240,
            Content = root,
            ShowInTaskbar = false,
            WindowStyle = WindowStyle.None
        };

        try
        {
            window.Show();
            PumpLayout(window);
            var row = RowFor(grid, rows[0]);
            Assert.Same(external, Keyboard.Focus(external));

            foreach (var theme in new[] { GameSaveCenterThemeMode.Light, GameSaveCenterThemeMode.Dark })
            {
                ApplyTheme(root, resources, theme);
                row.IsSelected = false;
                Assert.Same(external, Keyboard.Focus(external));
                PumpLayout(window);
                var unselectedGeometry = CaptureCellGeometry(row, grid);

                row.IsSelected = true;
                Assert.Same(external, Keyboard.Focus(external));
                PumpLayout(window);

                Assert.True(row.IsSelected, $"{theme}: compatibility row did not enter selected state");
                Assert.False(Selector.GetIsSelectionActive(row), $"{theme}: selection should stay inactive while focus is external");
                AssertGeometryUnchanged(unselectedGeometry, CaptureCellGeometry(row, grid), $"{theme} selected overlay");

                var background = Assert.IsType<Border>(row.Template.FindName("RowBackground", row));
                var chrome = Assert.IsType<Border>(row.Template.FindName("RowChrome", row));
                Assert.Equal(new Thickness(4, 2, 4, 2), background.Margin);
                Assert.Equal(new CornerRadius(14), background.CornerRadius);
                Assert.Equal(new Thickness(4, 2, 4, 2), chrome.Margin);
                Assert.Equal(new CornerRadius(14), chrome.CornerRadius);
                Assert.True(chrome.BorderThickness.Left > 0, $"{theme}: selection outline was not retained");
                Assert.False(chrome.IsHitTestVisible);
            }
        }
        finally
        {
            window.Close();
        }
    }

    private static DataGridRow RowFor(DataGrid grid, TaskStatusDto item)
        => grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow
            ?? throw new InvalidOperationException("DataGrid row was not generated.");

    internal static Border ChromeFor(DataGridRow row)
        => row.Template.FindName("RowChrome", row) as Border
            ?? throw new InvalidOperationException("Shared DataGrid row chrome was not generated.");

    private static Border BackgroundFor(DataGridRow row)
        => row.Template.FindName("RowBackground", row) as Border
            ?? throw new InvalidOperationException("Shared DataGrid row background was not generated.");

    private static Rect[] CaptureCellAndTextGeometry(DataGridRow row, DataGrid grid)
    {
        var cells = FindVisualChildren<DataGridCell>(row)
            .Where(cell => cell.Visibility == Visibility.Visible && cell.ActualWidth > 0 && cell.ActualHeight > 0)
            .OrderBy(cell => cell.Column.DisplayIndex)
            .ToArray();
        if (cells.Length != 2)
            throw new InvalidOperationException($"Expected two realized DataGrid cells, got {cells.Length}.");

        return cells.SelectMany(cell =>
        {
            var text = FindVisualChildren<TextBlock>(cell).FirstOrDefault()
                ?? throw new InvalidOperationException("A realized text cell has no TextBlock content.");
            return new[] { BoundsRelativeTo(cell, grid), BoundsRelativeTo(text, grid) };
        }).ToArray();
    }

    private static Rect[] CaptureCellGeometry(DataGridRow row, DataGrid grid)
    {
        var cells = FindVisualChildren<DataGridCell>(row)
            .Where(cell => cell.Visibility == Visibility.Visible && cell.ActualWidth > 0 && cell.ActualHeight > 0)
            .OrderBy(cell => cell.Column.DisplayIndex)
            .ToArray();
        if (cells.Length != 2)
            throw new InvalidOperationException($"Expected two realized DataGrid cells, got {cells.Length}.");

        return cells.Select(cell => BoundsRelativeTo(cell, row)).ToArray();
    }

    internal static Rect BoundsRelativeTo(FrameworkElement element, FrameworkElement ancestor)
    {
        var origin = element.TransformToAncestor(ancestor).Transform(new Point(0, 0));
        return new Rect(origin.X, origin.Y, element.ActualWidth, element.ActualHeight);
    }

    internal static Rect VisibleBoundsRelativeTo(FrameworkElement element, FrameworkElement ancestor)
    {
        var bounds = BoundsRelativeTo(element, ancestor);
        DependencyObject? current = element;
        while (current != null && !ReferenceEquals(current, ancestor))
        {
            if (current is ScrollContentPresenter scrollContent && scrollContent.ClipToBounds)
                bounds.Intersect(BoundsRelativeTo(scrollContent, ancestor));
            else if (current is FrameworkElement frameworkElement
                     && frameworkElement.ClipToBounds
                     && frameworkElement.ActualWidth > 0
                     && frameworkElement.ActualHeight > 0)
                bounds.Intersect(BoundsRelativeTo(frameworkElement, ancestor));
            if (current is FrameworkElement clippedElement && clippedElement.Clip is Geometry clip && !clip.Bounds.IsEmpty)
            {
                var transform = clippedElement.TransformToAncestor(ancestor);
                var topLeft = transform.Transform(clip.Bounds.TopLeft);
                var bottomRight = transform.Transform(clip.Bounds.BottomRight);
                bounds.Intersect(new Rect(topLeft, bottomRight));
            }
            current = VisualTreeHelper.GetParent(current);
        }
        return bounds;
    }

    private static void AssertGeometryUnchanged(Rect[] expected, Rect[] actual, string state)
    {
        Assert.Equal(expected.Length, actual.Length);
        for (var index = 0; index < expected.Length; index++)
        {
            Assert.True(Math.Abs(expected[index].X - actual[index].X) <= 0.25,
                $"{state} shifted cell/content {index} horizontally from {expected[index]} to {actual[index]}.");
            Assert.True(Math.Abs(expected[index].Y - actual[index].Y) <= 0.25,
                $"{state} shifted cell/content {index} vertically from {expected[index]} to {actual[index]}.");
            Assert.True(Math.Abs(expected[index].Width - actual[index].Width) <= 0.25,
                $"{state} changed cell/content {index} width from {expected[index]} to {actual[index]}.");
            Assert.True(Math.Abs(expected[index].Height - actual[index].Height) <= 0.25,
                $"{state} changed cell/content {index} height from {expected[index]} to {actual[index]}.");
        }
    }

    internal static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match) yield return match;
            foreach (var nested in FindVisualChildren<T>(child)) yield return nested;
        }
    }

    private static Color BrushColor(object value)
        => Assert.IsType<SolidColorBrush>(value).Color;

    private static TaskStatusDto Task(string id, TaskState state)
        => new TaskStatusDto
        {
            TaskId = id,
            TaskType = "Backup",
            GameName = "Synthetic Game",
            State = state,
            ProgressPercent = state == TaskState.Succeeded ? 100 : -1,
            CreatedUtc = new DateTime(2026, 9, 18)
        };

    private static ResourceDictionary LoadProductionResources()
        => (ResourceDictionary)XamlReader.Parse(@"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""><ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/Redesign.xaml""/>
</ResourceDictionary.MergedDictionaries></ResourceDictionary>");

    private static ResourceDictionary LoadDashboardDataGridResources()
    {
        XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
        var dashboardPath = Path.Combine(TestRepositoryContext.Root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml");
        var dashboard = XDocument.Load(dashboardPath);
        var focusStyle = dashboard.Descendants(presentation + "Style")
            .Single(style => style.Attribute(xaml + "Key")?.Value == "GscFocusVisual");
        var rowStyle = dashboard.Descendants(presentation + "Style")
            .Single(style => style.Attribute("TargetType")?.Value == "DataGridRow" && style.Attribute(xaml + "Key") == null);
        rowStyle.SetAttributeValue(xaml + "Key", "DashboardCompatibilityDataGridRow");
        var resources = XDocument.Parse(@"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""><ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/Redesign.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/AcrylicReferenceTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/AcrylicReferenceColorsDark.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/AcrylicReferenceControls.xaml""/>
</ResourceDictionary.MergedDictionaries></ResourceDictionary>");
        resources.Root!.Add(new XElement(focusStyle));
        resources.Root.Add(new XElement(rowStyle));
        return Assert.IsType<ResourceDictionary>(XamlReader.Parse(resources.ToString(SaveOptions.DisableFormatting)));
    }

    private static void AssertDashboardRowOverlayContract()
    {
        XNamespace presentation = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
        var dashboardPath = Path.Combine(TestRepositoryContext.Root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml");
        var dashboard = XDocument.Load(dashboardPath);
        var template = dashboard.Descendants(presentation + "ControlTemplate")
            .Single(candidate => candidate.Attribute("TargetType")?.Value == "DataGridRow");
        var root = template.Elements(presentation + "Grid").Single();
        var content = root.Elements(presentation + "SelectiveScrollingGrid").Single();
        var background = root.Elements(presentation + "Border")
            .Single(element => element.Attribute(xaml + "Name")?.Value == "RowBackground");
        var chrome = root.Elements(presentation + "Border")
            .Single(element => element.Attribute(xaml + "Name")?.Value == "RowChrome");

        Assert.Same(root, content.Parent);
        Assert.Same(root, background.Parent);
        Assert.Same(root, chrome.Parent);
        Assert.Equal("False", chrome.Attribute("IsHitTestVisible")?.Value);
        Assert.Contains(template.Descendants(presentation + "Trigger"), trigger =>
            trigger.Attribute("Property")?.Value == "IsSelected"
            && trigger.Descendants(presentation + "Setter").Any(setter =>
                setter.Attribute("TargetName")?.Value == "RowChrome"
                && setter.Attribute("Property")?.Value == "Margin"));
        Assert.Contains(template.Descendants(presentation + "Trigger"), trigger =>
            trigger.Attribute("Property")?.Value == "IsSelected"
            && trigger.Descendants(presentation + "Setter").Any(setter =>
                setter.Attribute("TargetName")?.Value == "RowChrome"
                && setter.Attribute("Property")?.Value == "BorderBrush"
                && setter.Attribute("Value")?.Value == "{DynamicResource GscAccentBrush}"));
    }

    internal static void ApplyTheme(FrameworkElement root, ResourceDictionary resources, GameSaveCenterThemeMode mode)
    {
        var palette = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(
            root,
            glassEnabled: true,
            strengthPercent: 78,
            themeMode: mode,
            highContrastOverride: false);
        AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(resources, palette, glassEnabled: true, motionEnabled: true);
        root.UpdateLayout();
    }

    internal static void PumpLayout(Window window)
    {
        window.UpdateLayout();
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
        window.UpdateLayout();
    }

    internal static void EnsureApplicationResources()
    {
        var application = Application.Current ?? new Application();
        if (!application.Resources.Contains("BaseTextBlockStyle"))
            application.Resources.Add("BaseTextBlockStyle", new Style(typeof(TextBlock)));
    }

    internal static void RunSta(Action action)
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
}
