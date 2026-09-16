using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

using GscButton = GameSaveCenter.Playnite.Controls.Button;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R03TextScaleTests
{
    [Fact]
    public void ProductionInputButtonAndHeaderGrowWithLargerTypography()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        Exception? failure = null;
        var measurements = new List<ControlMeasurement>();

        var thread = new Thread(() =>
        {
            try
            {
                var resources = LoadProductionResources();
                resources["GscBodyFontSize"] = 24d;
                resources["GscCaptionFontSize"] = 20d;

                var textBox = new TextBox
                {
                    Style = Assert.IsType<Style>(resources["GscWpfUiTextBox"]),
                    Text = "较大文本输入\r\n第二行也应保持可见",
                    AcceptsReturn = true,
                    TextWrapping = TextWrapping.Wrap,
                    Width = 220,
                    Margin = new Thickness(0, 0, 0, 8)
                };
                var button = new GscButton
                {
                    Style = Assert.IsType<Style>(resources["GscWpfUiMediaBatchButton"]),
                    Content = "应用当前备注",
                    Margin = new Thickness(0, 0, 0, 8)
                };
                var grid = new DataGrid
                {
                    Style = Assert.IsType<Style>(resources["GscRedesignWorkspaceDataGrid"]),
                    Width = 120,
                    Height = 150,
                    ItemsSource = new[] { "样本" },
                    AutoGenerateColumns = false,
                    HeadersVisibility = DataGridHeadersVisibility.Column
                };
                grid.Columns.Add(new DataGridTextColumn
                {
                    Header = "本地时间",
                    Binding = new Binding("."),
                    Width = new DataGridLength(96)
                });

                var host = new StackPanel { Width = 260 };
                host.Children.Add(textBox);
                host.Children.Add(button);
                host.Children.Add(grid);
                using var window = new WindowHost(new Window
                {
                    Resources = resources,
                    Content = host,
                    Width = 300,
                    Height = 420,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    WindowStyle = WindowStyle.None,
                    Opacity = 0.01
                });
                window.Window.Show();
                window.Window.UpdateLayout();

                var header = FindVisualChildren<DataGridColumnHeader>(grid)
                    .Single(candidate => FindVisualChildren<TextBlock>(candidate).Any(text => text.Text == "本地时间"));
                var headerText = FindVisualChildren<TextBlock>(header).Single(text => text.Text == "本地时间");
                var textBoxHost = FindVisualChildren<ScrollViewer>(textBox).Single();
                measurements.Add(new ControlMeasurement("input", textBox.ActualHeight, textBox.DesiredSize.Height, textBoxHost.ActualHeight, textBoxHost.DesiredSize.Height));
                measurements.Add(new ControlMeasurement("button", button.ActualHeight, button.DesiredSize.Height, FindVisualChildren<TextBlock>(button).Single(text => text.Text == "应用当前备注").ActualHeight, button.ActualHeight));
                measurements.Add(new ControlMeasurement("header", header.ActualHeight, header.DesiredSize.Height, headerText.ActualHeight, headerText.DesiredSize.Height));

                Assert.True(textBox.ActualHeight > 36, Describe(measurements[0]));
                Assert.True(button.ActualHeight > 30, Describe(measurements[1]));
                Assert.True(header.ActualHeight >= 42, Describe(measurements[2]));
                Assert.True(textBoxHost.ActualHeight >= textBoxHost.DesiredSize.Height - 0.5, Describe(measurements[0]));
                Assert.True(headerText.ActualHeight >= headerText.DesiredSize.Height - 0.5, Describe(measurements[2]));
                Assert.Equal(TextWrapping.Wrap, FindVisualChildren<TextBlock>(button).Single(text => text.Text == "应用当前备注").TextWrapping);
                Assert.Equal(TextTrimming.None, FindVisualChildren<TextBlock>(button).Single(text => text.Text == "应用当前备注").TextTrimming);
            }
            catch (Exception caught)
            {
                failure = caught;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(failure);
        Assert.Equal(3, measurements.Count);
    }

    [Fact]
    public void ProductionTableHeadersAndTextControlsDoNotReintroduceHardHeightOverrides()
    {
        var root = TestRepositoryContext.Root;
        var files = new[]
        {
            "src/GameSaveCenter.Playnite/Themes/WpfUiProduction.xaml",
            "src/GameSaveCenter.Playnite/Themes/Redesign.xaml",
            "src/GameSaveCenter.Playnite/Views/DashboardView.xaml",
            "src/GameSaveCenter.Playnite/Views/TaskCenterView.xaml",
            "src/GameSaveCenter.Playnite/Views/MediaCenterView.xaml",
            "src/GameSaveCenter.Playnite/Views/Development/UiFrameworkProbeView.xaml"
        };

        foreach (var file in files)
        {
            var text = System.IO.File.ReadAllText(System.IO.Path.Combine(root, file));
            Assert.DoesNotContain("ColumnHeaderHeight\" Value=\"{DynamicResource GscTableHeaderHeight}", text);
        }

        var production = System.IO.File.ReadAllText(System.IO.Path.Combine(root, files[0]));
        Assert.Contains("x:Key=\"GscDataGridHeaderTextTemplate\"", production);
        Assert.Contains("TextWrapping=\"Wrap\"", production);
        Assert.Contains("TextTrimming=\"None\"", production);
        Assert.Contains("MinHeight\" Value=\"{DynamicResource GscTableHeaderHeight}", production);
        Assert.DoesNotContain("<Setter Property=\"Height\" Value=\"{DynamicResource GscButtonHeight}\"/>", production);
        var pathDetailStart = production.IndexOf("x:Key=\"GscWpfUiPathDetailTextBox\"", StringComparison.Ordinal);
        var comboBoxStart = production.IndexOf("x:Key=\"GscWpfUiComboBox\"", pathDetailStart, StringComparison.Ordinal);
        Assert.True(pathDetailStart >= 0 && comboBoxStart > pathDetailStart);
        var pathDetailStyle = production.Substring(pathDetailStart, comboBoxStart - pathDetailStart);
        Assert.DoesNotContain("<Setter Property=\"Height\"", pathDetailStyle);
    }

    private static string Describe(ControlMeasurement measurement)
        => $"{measurement.Name}: actual={measurement.Actual:0.##}, desired={measurement.Desired:0.##}, childActual={measurement.ChildActual:0.##}, childDesired={measurement.ChildDesired:0.##}";

    private static ResourceDictionary LoadProductionResources()
        => (ResourceDictionary)XamlReader.Parse(@"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
                    xmlns:ui=""clr-namespace:GameSaveCenter.Playnite.Controls;assembly=GameSaveCenter.Playnite""><ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/Redesign.xaml""/>
</ResourceDictionary.MergedDictionaries></ResourceDictionary>");

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

    private sealed class ControlMeasurement
    {
        public ControlMeasurement(string name, double actual, double desired, double childActual, double childDesired)
        {
            Name = name;
            Actual = actual;
            Desired = desired;
            ChildActual = childActual;
            ChildDesired = childDesired;
        }

        public string Name { get; }
        public double Actual { get; }
        public double Desired { get; }
        public double ChildActual { get; }
        public double ChildDesired { get; }
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
