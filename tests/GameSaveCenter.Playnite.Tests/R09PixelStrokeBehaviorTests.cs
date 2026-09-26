using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GameSaveCenter.Playnite.Diagnostics;
using IoPath = System.IO.Path;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R09PixelStrokeBehaviorTests
{
    private static readonly double[] SimulatedDpiScales = { 1d, 1.25d, 1.5d, 1.75d, 2d };

    [Fact]
    public void ProductionSelectedChromeAndNavigationChromeKeepOneDipRoundedStrokeContract()
    {
        RunSta(() =>
        {
            var resources = LoadProductionResources();
            var host = new Grid
            {
                Width = 420,
                Height = 180,
                Resources = resources,
                UseLayoutRounding = true,
                SnapsToDevicePixels = true
            };
            var selectedTab = new TabItem
            {
                Style = Assert.IsType<Style>(resources["GscRedesignWorkspaceTabItem"]),
                Header = "选中",
                IsSelected = true,
                Width = 120,
                Height = 48,
                Margin = new Thickness(12, 12, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            var navigation = new RadioButton
            {
                Style = Assert.IsType<Style>(resources["AcrylicNavItem"]),
                Content = "导航",
                IsChecked = true,
                Width = 120,
                Height = 48,
                Margin = new Thickness(12, 72, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            host.Children.Add(selectedTab);
            host.Children.Add(navigation);

            var window = new Window
            {
                Content = host,
                Width = 420,
                Height = 180,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = false,
                Opacity = 0.01
            };

            try
            {
                window.Show();
                window.UpdateLayout();
                selectedTab.ApplyTemplate();
                navigation.ApplyTemplate();

                var tabChrome = Assert.IsType<Border>(selectedTab.Template.FindName("Chrome", selectedTab));
                var navigationChrome = Assert.IsType<Border>(navigation.Template.FindName("NavChrome", navigation));

                Assert.True(tabChrome.SnapsToDevicePixels);
                Assert.True(tabChrome.UseLayoutRounding);
                Assert.Equal(1d, tabChrome.BorderThickness.Left);
                Assert.NotNull(tabChrome.BorderBrush);
                Assert.True(navigationChrome.SnapsToDevicePixels);
                Assert.True(navigationChrome.UseLayoutRounding);
                Assert.Equal(1d, navigationChrome.BorderThickness.Left);
                Assert.NotNull(navigationChrome.BorderBrush);

                var strokeProbe = CreateStrokeProbe();
                var roundedProbe = strokeProbe.Root;
                var probeWindow = new Window
                {
                    Content = roundedProbe,
                    Width = 240,
                    Height = 140,
                    WindowStyle = WindowStyle.None,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    Opacity = 1
                };

                try
                {
                    probeWindow.Show();
                    probeWindow.UpdateLayout();
                    var dividerOrigin = strokeProbe.Divider.TransformToAncestor(roundedProbe).Transform(new Point(0, 0));
                    var roundedOrigin = strokeProbe.Rounded.TransformToAncestor(roundedProbe).Transform(new Point(0, 0));
                    foreach (var scale in SimulatedDpiScales)
                    {
                        var path = IoPath.Combine(
                            IoPath.GetTempPath(),
                            "gsc-r09-stroke-" + scale.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "-" + Guid.NewGuid().ToString("N") + ".png");
                        try
                        {
                            UiDiagnosticsExporters.SavePng(roundedProbe, path, scale);
                            using var stream = File.OpenRead(path);
                            var frame = BitmapDecoder.Create(
                                stream,
                                BitmapCreateOptions.PreservePixelFormat,
                                BitmapCacheOption.OnLoad).Frames[0];
                            var pixels = ReadPixels(frame);

                            Assert.Equal((int)Math.Ceiling(220 * scale), frame.PixelWidth);
                            Assert.Equal((int)Math.Ceiling(120 * scale), frame.PixelHeight);
                            AssertThicknessIsOneOrTwoPixels(pixels, frame.PixelWidth, scale, dividerOrigin.X, dividerOrigin.Y, strokeProbe.Divider.ActualWidth, strokeProbe.Divider.ActualHeight);
                            AssertThicknessIsOneOrTwoPixels(pixels, frame.PixelWidth, scale, roundedOrigin.X, roundedOrigin.Y, strokeProbe.Rounded.ActualWidth, strokeProbe.Rounded.ActualHeight);
                            AssertRoundedCornerDoesNotSquareOff(pixels, frame.PixelWidth, scale, roundedOrigin.X, roundedOrigin.Y, 10);
                        }
                        finally
                        {
                            if (File.Exists(path))
                                File.Delete(path);
                        }
                    }
                }
                finally
                {
                    probeWindow.Close();
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void DashboardCopiesKeepTheSamePixelSnappingContractAsSharedChrome()
    {
        var root = TestRepositoryContext.Root;
        var dashboard = File.ReadAllText(IoPath.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml"));
        var redesign = File.ReadAllText(IoPath.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "Redesign.xaml"));
        var acrylic = File.ReadAllText(IoPath.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "AcrylicProductionResources.xaml"));

        Assert.Contains("SnapsToDevicePixels=\"True\" UseLayoutRounding=\"True\"", dashboard);
        Assert.Contains("SnapsToDevicePixels=\"True\"", redesign);
        Assert.Contains("SnapsToDevicePixels=\"True\" UseLayoutRounding=\"True\"", acrylic);
        Assert.DoesNotContain("SnapsToDevicePixels=\"False\"", redesign);
        Assert.DoesNotContain("SnapsToDevicePixels=\"False\"", dashboard);
    }

    private static ResourceDictionary LoadProductionResources()
    {
        return Assert.IsType<ResourceDictionary>(XamlReader.Parse(@"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""><ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/Redesign.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/AcrylicProductionResources.xaml""/>
</ResourceDictionary.MergedDictionaries></ResourceDictionary>"));
    }

    private static StrokeProbe CreateStrokeProbe()
    {
        var root = new Canvas
        {
            Width = 220,
            Height = 120,
            Background = Brushes.Black,
            UseLayoutRounding = true,
            SnapsToDevicePixels = true
        };
        var divider = new Border
        {
            Width = 120,
            Height = 1,
            Background = Brushes.White,
            SnapsToDevicePixels = true,
            UseLayoutRounding = true
        };
        root.Children.Add(divider);
        Canvas.SetLeft(divider, 14);
        Canvas.SetTop(divider, 8);
        var rounded = new Border
        {
            Width = 120,
            Height = 50,
            Background = Brushes.Black,
            BorderBrush = Brushes.White,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            SnapsToDevicePixels = true,
            UseLayoutRounding = true
        };
        root.Children.Add(rounded);
        Canvas.SetLeft(rounded, 14);
        Canvas.SetTop(rounded, 32);
        root.Measure(new Size(220, 120));
        root.Arrange(new Rect(0, 0, 220, 120));
        root.UpdateLayout();
        return new StrokeProbe(root, divider, rounded);
    }

    private sealed class StrokeProbe
    {
        public StrokeProbe(Canvas root, Border divider, Border rounded)
        {
            Root = root;
            Divider = divider;
            Rounded = rounded;
        }

        public Canvas Root { get; }
        public Border Divider { get; }
        public Border Rounded { get; }
    }

    private static byte[] ReadPixels(BitmapFrame frame)
    {
        var pixels = new byte[frame.PixelWidth * frame.PixelHeight * 4];
        frame.CopyPixels(pixels, frame.PixelWidth * 4, 0);
        return pixels;
    }

    private static void AssertThicknessIsOneOrTwoPixels(
        IReadOnlyList<byte> pixels,
        int pixelWidth,
        double scale,
        double x,
        double y,
        double width,
        double height)
    {
        var sampleX = Math.Min(pixelWidth - 1, (int)Math.Round((x + width / 2) * scale));
        var expectedRow = (int)Math.Round(y * scale);
        var lastRow = pixels.Count / (pixelWidth * 4) - 1;
        var firstRow = Math.Max(0, expectedRow - 8);
        var searchLastRow = Math.Min(lastRow, expectedRow + 8);
        var brightRows = Enumerable.Range(firstRow, Math.Max(0, searchLastRow - firstRow + 1))
            .Where(row => IsBright(pixels, pixelWidth, sampleX, row))
            .ToArray();
        var longestRun = LongestConsecutiveRun(brightRows);
        var brightPixelRows = Enumerable.Range(0, lastRow + 1)
            .Where(row => Enumerable.Range(0, pixelWidth).Any(column => IsBright(pixels, pixelWidth, column, row)))
            .ToArray();
        var brightPixelColumns = Enumerable.Range(0, pixelWidth)
            .Where(column => Enumerable.Range(0, lastRow + 1).Any(row => IsBright(pixels, pixelWidth, column, row)))
            .ToArray();
        var sampleRows = Enumerable.Range(Math.Max(0, expectedRow - 3), Math.Min(lastRow, expectedRow + 3) - Math.Max(0, expectedRow - 3) + 1)
            .Select(row => $"{row}:{pixels[(row * pixelWidth + sampleX) * 4]}")
            .ToArray();

        var minimumPixels = Math.Max(1, (int)Math.Floor(scale));
        var maximumPixels = Math.Max(2, (int)Math.Ceiling(scale) + 1);
        Assert.True(
            longestRun >= minimumPixels && longestRun <= maximumPixels,
            $"scale={scale:0.00}, expectedRow={expectedRow}, sampleX={sampleX}, brightRows=[{string.Join(",", brightRows)}], " +
            $"samples=[{string.Join(",", sampleRows)}], " +
            $"allBrightBounds=rows:{(brightPixelRows.Length == 0 ? "none" : $"{brightPixelRows[0]}..{brightPixelRows[brightPixelRows.Length - 1]}")}, " +
            $"columns:{(brightPixelColumns.Length == 0 ? "none" : $"{brightPixelColumns[0]}..{brightPixelColumns[brightPixelColumns.Length - 1]}")}");
    }

    private static void AssertRoundedCornerDoesNotSquareOff(
        IReadOnlyList<byte> pixels,
        int pixelWidth,
        double scale,
        double x,
        double y,
        double radius)
    {
        var cornerX = (int)Math.Round(x * scale);
        var expectedRow = (int)Math.Round(y * scale);
        var lastRow = pixels.Count / (pixelWidth * 4) - 1;
        var nearCornerX = Math.Min(pixelWidth - 1, (int)Math.Round((x + radius + 4) * scale));
        var searchFirstRow = Math.Max(0, expectedRow - 8);
        var searchLastRow = Math.Min(lastRow, expectedRow + 8);
        var topRow = Enumerable.Range(searchFirstRow, Math.Max(0, searchLastRow - searchFirstRow + 1))
            .Where(row => IsBright(pixels, pixelWidth, nearCornerX, row))
            .DefaultIfEmpty(-1)
            .First();

        Assert.True(topRow >= 0, "Rounded stroke lost its top connection after the corner.");
        Assert.False(IsBright(pixels, pixelWidth, cornerX, topRow), "Rounded stroke leaked into the outer square corner.");
    }

    private static int LongestConsecutiveRun(IReadOnlyList<int> values)
    {
        var longest = 0;
        var current = 0;
        for (var index = 0; index < values.Count; index++)
        {
            current = index == 0 || values[index] == values[index - 1] + 1 ? current + 1 : 1;
            longest = Math.Max(longest, current);
        }
        return longest;
    }

    private static bool IsBright(IReadOnlyList<byte> pixels, int width, int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y * width * 4 + x * 4 + 2 >= pixels.Count)
            return false;
        var offset = (y * width + x) * 4;
        return pixels[offset] > 80 && pixels[offset + 1] > 80 && pixels[offset + 2] > 80;
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
                System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        Assert.Null(failure);
    }
}
