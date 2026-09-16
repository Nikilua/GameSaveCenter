using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Views.Development;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class NumericCellReadabilityTests
{
    [Fact]
    public void DevelopmentFixtureKeepsNumericSamplesFullyReadable()
    {
        RunSta(() =>
        {
            using var host = CreateFixtureWindow();
            var grid = FindVisualChildren<DataGrid>(host.Window)
                .Single(candidate => AutomationProperties.GetName(candidate) == "校对数据表");
            var measurements = NumericCellReadability.Measure(grid);

            Assert.Equal(
                new[] { "1,024 / 99,999", "-9,999,999,999", "512 GiB", "1.25 TiB" },
                measurements.Select(measurement => measurement.Text).ToArray());
            Assert.All(measurements, measurement =>
            {
                Assert.True(measurement.HorizontalFit, Describe(measurement));
                Assert.True(measurement.VerticalFit, Describe(measurement));
                Assert.Equal(TextWrapping.NoWrap, measurement.Wrapping);
                Assert.Equal(TextTrimming.None, measurement.Trimming);
            });
        });
    }

    [Fact]
    public void NumericDetectorRejectsNarrowColumnWhenRowHeightStillFits()
    {
        RunSta(() =>
        {
            var grid = new DataGrid
            {
                Width = 180,
                Height = 110,
                MinColumnWidth = 0,
                AutoGenerateColumns = false,
                CanUserAddRows = false,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                RowHeight = 52,
                ColumnHeaderHeight = 42,
                ItemsSource = new[] { new NumericRow { Value = "-99,999,999,999,999" } }
            };
            var style = new Style(typeof(TextBlock));
            style.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Left));
            style.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.NoWrap));
            style.Setters.Add(new Setter(TextBlock.TextTrimmingProperty, TextTrimming.None));
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = "数值",
                Binding = new Binding(nameof(NumericRow.Value)),
                Width = 56,
                MinWidth = 0,
                ElementStyle = style
            });

            using var window = new WindowHost(new Window
            {
                Content = grid,
                Width = 180,
                Height = 110,
                ShowInTaskbar = false,
                ShowActivated = false,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            });
            window.Window.Show();
            window.Window.UpdateLayout();

            var measurement = Assert.Single(NumericCellReadability.Measure(grid));
            Assert.True(measurement.VerticalFit, Describe(measurement));
            Assert.False(measurement.HorizontalFit, Describe(measurement));
            Assert.False(measurement.IsReadable);
        });
    }

    private static WindowHost CreateFixtureWindow()
    {
        var view = new UiFrameworkProbeView
        {
            Width = 1120,
            Height = 980
        };
        var window = new Window
        {
            Content = view,
            Width = 1120,
            Height = 980,
            ShowInTaskbar = false,
            ShowActivated = false,
            WindowStyle = WindowStyle.None,
            Opacity = 0.01
        };
        window.Show();
        window.UpdateLayout();
        return new WindowHost(window);
    }

    private static string Describe(NumericCellReadability.Measurement measurement)
        => $"{measurement.Text}: text={measurement.TextWidth:0.##} available={measurement.AvailableWidth:0.##} "
            + $"textHeight={measurement.TextHeight:0.##} cellHeight={measurement.CellHeight:0.##}";

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

    private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var index = 0; index < System.Windows.Media.VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(root, index);
            if (child is T match)
                yield return match;
            foreach (var nested in FindVisualChildren<T>(child))
                yield return nested;
        }
    }

    private sealed class NumericRow
    {
        public string Value { get; set; } = string.Empty;
    }

    private sealed class WindowHost : IDisposable
    {
        public WindowHost(Window window) => Window = window;

        public Window Window { get; }

        public void Dispose()
        {
            if (Window.IsVisible)
                Window.Close();
        }
    }
}
