using System;
using System.Collections.ObjectModel;
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
using GameSaveCenter.Contracts;
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
                window.UpdateLayout();
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
                Assert.Equal(new Thickness(4, 2, 12, 2), failedBackground.Margin);
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
                Assert.Equal(new Thickness(3, 1, 11, 1), activeChrome.Margin);
                Assert.Equal(new Thickness(3, 1, 11, 1), activeBackground.Margin);
                AssertGeometryUnchanged(succeededNormalGeometry, succeededFocusedGeometry, "keyboard-focused selection");

                Keyboard.Focus(external);
                PumpLayout(window);
                var inactiveChrome = ChromeFor(succeededRow);
                var inactiveBackground = BackgroundFor(succeededRow);
                Assert.False(Selector.GetIsSelectionActive(succeededRow));
                Assert.Equal(BrushColor(resources["GscSelectionInactiveBrush"]), BrushColor(inactiveBackground.Background));
                Assert.Equal(BrushColor(resources["GscMutedStatusBrush"]), BrushColor(inactiveChrome.BorderBrush));
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

    private static DataGridRow RowFor(DataGrid grid, TaskStatusDto item)
        => grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow
            ?? throw new InvalidOperationException("DataGrid row was not generated.");

    private static Border ChromeFor(DataGridRow row)
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

    private static Rect BoundsRelativeTo(FrameworkElement element, FrameworkElement ancestor)
    {
        var origin = element.TransformToAncestor(ancestor).Transform(new Point(0, 0));
        return new Rect(origin.X, origin.Y, element.ActualWidth, element.ActualHeight);
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

    private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
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

    private static void PumpLayout(Window window)
    {
        window.UpdateLayout();
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
        window.UpdateLayout();
    }

    private static void EnsureApplicationResources()
    {
        var application = Application.Current ?? new Application();
        if (!application.Resources.Contains("BaseTextBlockStyle"))
            application.Resources.Add("BaseTextBlockStyle", new Style(typeof(TextBlock)));
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
}
