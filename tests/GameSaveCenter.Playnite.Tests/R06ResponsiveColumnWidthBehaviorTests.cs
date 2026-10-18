using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("R06ResponsiveColumnWpf")]
public sealed class R06ResponsiveColumnWidthBehaviorTests
{
    [Fact]
    public void WindowResizeTemporarilyCompressesAndRestoresColumnsWithoutLosingSelectionOrFocus()
    {
        RunSta(() =>
        {
            var settings = new GameSaveCenterSettings();
            settings.SetDataGridColumnWidth("responsive-test", "primary", 250);
            settings.SetDataGridColumnWidth("responsive-test", "secondary", 220);
            settings.SetDataGridColumnWidth("responsive-test", "tertiary", 190);
            var persistCount = 0;
            var row = new GridRow { Value = "保持焦点和选择" };
            var grid = new DataGrid
            {
                AutoGenerateColumns = false,
                CanUserResizeColumns = true,
                IsReadOnly = true,
                SelectionUnit = DataGridSelectionUnit.FullRow,
                ItemsSource = new[] { row },
                Height = 130
            };
            ScrollViewer.SetHorizontalScrollBarVisibility(grid, ScrollBarVisibility.Auto);
            ScrollViewer.SetVerticalScrollBarVisibility(grid, ScrollBarVisibility.Auto);
            grid.Columns.Add(CreateColumn("主列", nameof(GridRow.Value), 250, 110));
            grid.Columns.Add(CreateColumn("次列", nameof(GridRow.Value), 220, 90));
            grid.Columns.Add(CreateColumn("补充", nameof(GridRow.Value), 190, 90));

            var window = new Window
            {
                Content = grid,
                Width = 460,
                Height = 170,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = true,
                Opacity = 0.01
            };

            try
            {
                using (var controller = new DataGridColumnLayoutController(
                    grid,
                    "responsive-test",
                    new[] { "primary", "secondary", "tertiary" },
                    settings,
                    () => persistCount++))
                {
                    window.Show();
                    FlushLayout(window);
                    window.Activate();
                    grid.SelectedItem = row;
                    grid.CurrentCell = new DataGridCellInfo(row, grid.Columns[1]);
                    grid.ScrollIntoView(row, grid.Columns[1]);
                    Assert.Same(grid, Keyboard.Focus(grid));
                    FlushLayout(window);

                    Assert.True(grid.ActualWidth > 400, $"窄窗 DataGrid 实际宽度={grid.ActualWidth}");
                    Assert.True(grid.Columns[1].Width.Value < 220, $"窄窗次列宽度={grid.Columns[1].Width.Value}");
                    Assert.True(grid.Columns[0].Width.Value >= 110);
                    Assert.True(grid.Columns[1].Width.Value >= 90);
                    Assert.True(grid.Columns[2].Width.Value >= 90);
                    Assert.Equal(250, controller.CaptureConfigurationWidths()["primary"], 3);
                    Assert.Equal(220, controller.CaptureConfigurationWidths()["secondary"], 3);
                    Assert.Equal(190, controller.CaptureConfigurationWidths()["tertiary"], 3);
                    Assert.Equal(250, settings.DataGridColumnWidths["v1/responsive-test/primary"], 3);
                    Assert.Equal(220, settings.DataGridColumnWidths["v1/responsive-test/secondary"], 3);
                    Assert.Equal(190, settings.DataGridColumnWidths["v1/responsive-test/tertiary"], 3);
                    Assert.True(grid.IsKeyboardFocusWithin);
                    Assert.Equal(0, persistCount);

                    window.Width = 1040;
                    FlushLayout(window);

                    Assert.True(grid.ActualWidth > 900, $"宽窗 DataGrid 实际宽度={grid.ActualWidth}");
                    Assert.Equal(250, grid.Columns[0].Width.Value, 3);
                    Assert.Equal(220, grid.Columns[1].Width.Value, 3);
                    Assert.Equal(190, grid.Columns[2].Width.Value, 3);
                    Assert.Same(row, grid.SelectedItem);
                    Assert.Same(grid.Columns[1], grid.CurrentColumn);
                    Assert.True(grid.IsKeyboardFocusWithin);
                    Assert.Equal(0, persistCount);
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static DataGridTextColumn CreateColumn(string header, string bindingPath, double width, double minWidth)
        => new DataGridTextColumn
        {
            Header = header,
            Binding = new Binding(bindingPath),
            Width = new DataGridLength(width, DataGridLengthUnitType.Pixel),
            MinWidth = minWidth
        };

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

    private sealed class GridRow
    {
        public string Value { get; set; } = string.Empty;
    }
}

[CollectionDefinition("R06ResponsiveColumnWpf", DisableParallelization = true)]
public sealed class R06ResponsiveColumnWpfCollection
{
}
