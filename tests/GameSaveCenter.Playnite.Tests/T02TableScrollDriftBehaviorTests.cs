using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using Xunit;
using Xunit.Abstractions;

namespace GameSaveCenter.Playnite.Tests;

public sealed class T02TableScrollDriftBehaviorTests
{
    private readonly ITestOutputHelper output;

    public T02TableScrollDriftBehaviorTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void ProductionCellsAndSelectionChromeRemainStableAcrossRepeatedEndToEndScroll()
    {
        Exception? failure = null;
        var observations = new List<string>();
        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var app = Application.Current ?? new Application();
                app.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                foreach (var theme in new[] { GameSaveCenterThemeMode.Light, GameSaveCenterThemeMode.Dark })
                {
                    window = CreateWindow(theme, out var grid, out var items);
                    window.Show();
                    Pump(window);

                    var scroll = Descendants<ScrollViewer>(grid).First();
                    Assert.True(scroll.ScrollableHeight > 0);
                    Assert.True(scroll.ScrollableWidth > 0);

                    var baselineCellTops = new Dictionary<string, double>();
                    var baselineChromeBounds = new Dictionary<int, Rect>();
                    for (var cycle = 0; cycle < 8; cycle++)
                    {
                        foreach (var index in new[] { 0, items.Count - 1, 0 })
                        {
                            grid.SelectedItem = items[index];
                            grid.ScrollIntoView(items[index]);
                            var requestedHorizontalOffset = cycle % 2 == 0 ? 0 : scroll.ScrollableWidth;
                            scroll.ScrollToHorizontalOffset(requestedHorizontalOffset);
                            Pump(window);

                            Assert.InRange(Math.Abs(scroll.HorizontalOffset - requestedHorizontalOffset), 0, 0.5);
                            if (index == 0)
                            {
                                Assert.InRange(scroll.VerticalOffset, 0, 0.5);
                            }
                            else
                            {
                                Assert.True(scroll.VerticalOffset > 0,
                                    $"the last row must reach the lower viewport edge; offset={scroll.VerticalOffset}.");
                            }

                            var row = Assert.IsType<DataGridRow>(grid.ItemContainerGenerator.ContainerFromItem(items[index]));
                            Assert.True(row.IsSelected);

                            var rowHeader = Assert.IsType<DataGridRowHeader>(FindDescendant<DataGridRowHeader>(row));
                            Assert.Equal(Visibility.Collapsed, rowHeader.Visibility);

                            var chrome = Assert.IsType<Border>(row.Template.FindName("RowChrome", row));
                            Assert.True(chrome.BorderThickness.Left > 0);
                            Assert.True(chrome.CornerRadius.TopLeft > 0);
                            var chromeBounds = BoundsRelativeTo(chrome, row);
                            Assert.InRange(chromeBounds.Top, -0.5, row.ActualHeight + 0.5);
                            Assert.InRange(chromeBounds.Bottom, 0, row.ActualHeight + 0.5);
                            if (baselineChromeBounds.TryGetValue(index, out var previousChromeBounds))
                            {
                                AssertGeometryNear(previousChromeBounds, chromeBounds, 0.25,
                                    $"{theme} selected outline for row {index} moved during cycle {cycle}");
                            }
                            else
                            {
                                baselineChromeBounds[index] = chromeBounds;
                            }

                            var cells = Descendants<DataGridCell>(row)
                                .Where(cell => cell.IsVisible && cell.ActualWidth > 0 && cell.ActualHeight > 0)
                                .ToArray();
                            Assert.NotEmpty(cells);
                            var cellBoundsByDisplayIndex = new List<(int DisplayIndex, Rect Bounds)>();
                            foreach (var cell in cells)
                            {
                                var cellBounds = BoundsRelativeTo(cell, row);
                                cellBoundsByDisplayIndex.Add((cell.Column.DisplayIndex, cellBounds));
                                Assert.InRange(cellBounds.Top, -0.5, row.ActualHeight - cell.ActualHeight + 0.5);

                                var key = $"{index}:{cell.Column.DisplayIndex}";
                                if (baselineCellTops.TryGetValue(key, out var previousTop))
                                {
                                    Assert.InRange(Math.Abs(cellBounds.Top - previousTop), 0, 0.25);
                                }
                                else
                                {
                                    baselineCellTops[key] = cellBounds.Top;
                                }
                            }

                            observations.Add(
                                $"theme={theme} cycle={cycle} item={index} verticalOffset={Format(scroll.VerticalOffset)} "
                                + $"horizontalOffset={Format(scroll.HorizontalOffset)} rowHeader={rowHeader.Visibility} "
                                + $"rowHeight={Format(row.ActualHeight)} selectionChrome={Format(chromeBounds)} "
                                + "cells=["
                                + string.Join(";", cellBoundsByDisplayIndex.Select(cell =>
                                    $"column={cell.DisplayIndex}:{Format(cell.Bounds)}"))
                                + "]");
                        }
                    }

                    window.Close();
                    window = null;
                }
            }
            catch (Exception ex)
            {
                failure = ex;
            }
            finally
            {
                window?.Close();
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        foreach (var observation in observations)
        {
            output.WriteLine(observation);
        }

        if (failure is not null)
        {
            ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }

    private static Window CreateWindow(GameSaveCenterThemeMode theme, out DataGrid grid, out List<ProbeRow> items)
    {
        if (!Application.Current!.Resources.Contains("BaseTextBlockStyle"))
        {
            Application.Current.Resources.Add("BaseTextBlockStyle", new Style(typeof(TextBlock)));
        }

        var resources = (ResourceDictionary)XamlReader.Parse(
            "<ResourceDictionary xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" "
            + "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">"
            + "<ResourceDictionary.MergedDictionaries>"
            + "<ResourceDictionary Source=\"/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml\"/>"
            + "<ResourceDictionary Source=\"/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml\"/>"
            + "<ResourceDictionary Source=\"/GameSaveCenter.Playnite;component/Themes/Redesign.xaml\"/>"
            + "</ResourceDictionary.MergedDictionaries></ResourceDictionary>");

        items = Enumerable.Range(0, 240)
            .Select(index => new ProbeRow { Label = $"Synthetic row {index}" })
            .ToList();
        grid = new DataGrid
        {
            Width = 500,
            Height = 230,
            AutoGenerateColumns = false,
            CanUserAddRows = false,
            EnableRowVirtualization = true,
            EnableColumnVirtualization = true,
            ItemsSource = items,
            RowStyle = (Style)resources["GscStableDataGridRow"]
        };
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "First",
            Binding = new Binding(nameof(ProbeRow.Label)),
            Width = 360
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Second",
            Binding = new Binding(nameof(ProbeRow.Label)),
            Width = 360
        });

        var window = new Window
        {
            Width = 560,
            Height = 300,
            Content = grid,
            Resources = resources,
            ShowInTaskbar = false,
            ShowActivated = false,
            WindowStyle = WindowStyle.None
        };
        var palette = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(window, true, 78, theme, false);
        AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(window.Resources, palette, true, true);
        return window;
    }

    private static Rect BoundsRelativeTo(FrameworkElement element, FrameworkElement ancestor)
    {
        var transform = element.TransformToAncestor(ancestor);
        return transform.TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
    }

    private static string Format(Rect bounds)
        => $"({Format(bounds.X)},{Format(bounds.Y)},{Format(bounds.Width)},{Format(bounds.Height)})";

    private static string Format(double value)
        => value.ToString("0.###", CultureInfo.InvariantCulture);

    private static void AssertGeometryNear(Rect expected, Rect actual, double tolerance, string message)
    {
        Assert.True(Math.Abs(expected.X - actual.X) <= tolerance
            && Math.Abs(expected.Y - actual.Y) <= tolerance
            && Math.Abs(expected.Width - actual.Width) <= tolerance
            && Math.Abs(expected.Height - actual.Height) <= tolerance,
            $"{message}: expected {expected}, actual {actual}.");
    }

    private static void Pump(Window window)
    {
        window.UpdateLayout();
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
        window.UpdateLayout();
    }

    private static T? FindDescendant<T>(DependencyObject root) where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
            {
                return match;
            }

            var nested = FindDescendant<T>(child);
            if (nested is not null)
            {
                return nested;
            }
        }

        return null;
    }

    private static IEnumerable<T> Descendants<T>(DependencyObject root) where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
            {
                yield return match;
            }

            foreach (var nested in Descendants<T>(child))
            {
                yield return nested;
            }
        }
    }

    public sealed class ProbeRow
    {
        public string Label { get; set; } = string.Empty;
    }
}
