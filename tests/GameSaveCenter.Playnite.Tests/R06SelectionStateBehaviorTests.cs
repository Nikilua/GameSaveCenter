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
                RowStyle = (Style)resources["GscStableDataGridRow"],
                ItemsSource = items
            };
            grid.Columns.Add(new DataGridTextColumn { Header = "状态", Binding = new Binding("StateDisplay") });

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

                grid.SelectedItem = items[0];
                Keyboard.Focus(external);
                PumpLayout(window);
                var failedChrome = ChromeFor(failedRow);
                var failedData = Assert.IsType<TaskStatusDto>(failedRow.DataContext);
                Assert.Equal(TaskState.Failed, failedData.State);
                Assert.Equal(BrushColor(resources["GscSelectionInactiveBrush"]), BrushColor(failedChrome.Background));
                Assert.NotEqual(BrushColor(resources["GscErrorTintBrush"]), BrushColor(failedChrome.Background));
                Assert.NotEqual(BrushColor(resources["GscErrorBrush"]), BrushColor(failedChrome.BorderBrush));

                grid.SelectedItem = items[1];
                succeededRow.Focusable = true;
                Assert.Same(succeededRow, Keyboard.Focus(succeededRow));
                PumpLayout(window);
                var activeChrome = ChromeFor(succeededRow);
                Assert.True(succeededRow.IsKeyboardFocusWithin);
                Assert.Equal(2, activeChrome.BorderThickness.Left);
                Assert.Equal(BrushColor(resources["GscAccentTintBrush"]), BrushColor(activeChrome.Background));

                Keyboard.Focus(external);
                PumpLayout(window);
                var inactiveChrome = ChromeFor(succeededRow);
                Assert.False(Selector.GetIsSelectionActive(succeededRow));
                Assert.Equal(BrushColor(resources["GscSelectionInactiveBrush"]), BrushColor(inactiveChrome.Background));
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
