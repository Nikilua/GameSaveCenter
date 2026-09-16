using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using GameSaveCenter.Contracts;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R03NumericAlignmentTests
{
    [Fact]
    public void SharedNumericCellStyleKeepsRightAnchorAcrossDigitBoundaries()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        Exception? failure = null;
        double[] rightEdges = Array.Empty<double>();
        double[] widths = Array.Empty<double>();

        var thread = new Thread(() =>
        {
            try
            {
                var resources = LoadProductionResources();
                var style = Assert.IsType<Style>(resources["GscTypographyNumericCell"]);
                Assert.Equal(HorizontalAlignment.Right, SetterValue(style, TextBlock.HorizontalAlignmentProperty));
                Assert.Equal(TextAlignment.Right, SetterValue(style, TextBlock.TextAlignmentProperty));
                Assert.Equal(TextWrapping.NoWrap, SetterValue(style, TextBlock.TextWrappingProperty));
                Assert.Equal(TextTrimming.None, SetterValue(style, TextBlock.TextTrimmingProperty));

                var values = new[] { "9", "10", "99", "100" };
                var grid = new Grid { Width = 120, Height = values.Length * 32 };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                foreach (var value in values)
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(32) });
                var cells = values
                    .Select((value, index) =>
                    {
                        var cell = new TextBlock
                        {
                            Text = value,
                            Style = style,
                            Height = 32
                        };
                        Grid.SetRow(cell, index);
                        grid.Children.Add(cell);
                        return cell;
                    })
                    .ToArray();

                var host = new Border
                {
                    Resources = resources,
                    Width = 160,
                    Height = values.Length * 32,
                    Child = grid
                };
                host.Measure(new Size(160, values.Length * 32));
                host.Arrange(new Rect(0, 0, 160, values.Length * 32));
                host.UpdateLayout();

                rightEdges = cells.Select(cell =>
                {
                    Assert.True(cell.ActualWidth > 0);
                    var right = cell.TransformToAncestor(grid).Transform(new Point(cell.ActualWidth, 0));
                    return right.X;
                }).ToArray();
                widths = cells.Select(cell => cell.ActualWidth).ToArray();
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
        Assert.Equal(4, rightEdges.Length);
        Assert.True(widths[1] > widths[0], $"Expected 10 to measure wider than 9: {string.Join(",", widths)}");
        Assert.True(widths[3] > widths[2], $"Expected 100 to measure wider than 99: {string.Join(",", widths)}");
        Assert.All(rightEdges, edge => Assert.InRange(edge, 119.5, 120.5));
        Assert.InRange(rightEdges.Max() - rightEdges.Min(), 0, 0.5);
    }

    [Theory]
    [InlineData(0, 0, "—")]
    [InlineData(1, -1, "—")]
    [InlineData(1, 0, "0%")]
    [InlineData(1, 9, "9%")]
    [InlineData(1, 10, "10%")]
    [InlineData(1, 99, "99%")]
    [InlineData(1, 100, "100%")]
    [InlineData(1, 120, "100%")]
    public void TaskProgressUsesStablePercentTextAndExplicitUnknownPlaceholder(int state, int progress, string expected)
    {
        var task = new TaskStatusDto
        {
            State = (TaskState)state,
            ProgressPercent = progress
        };

        Assert.Equal(expected, task.ProgressDisplay);
        Assert.InRange(task.ProgressValue, 0, 100);
    }

    [Fact]
    public void ProductionTimeAndPercentColumnsUseSharedSemanticCellStyles()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var typography = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Themes", "Typography.xaml"));
        var save = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
        var media = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
        var maintenance = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MaintenanceView.xaml"));
        var task = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));

        Assert.Contains("x:Key=\"GscTypographyNumericCell\"", typography);
        Assert.Contains("x:Key=\"GscTypographyTimeCell\"", typography);
        Assert.Contains("x:Key=\"GscTypographyPercentCell\"", typography);
        Assert.Contains("BasedOn=\"{StaticResource GscTypographyTimeCell}\"", save);
        Assert.Contains("BasedOn=\"{StaticResource GscTypographyTimeCell}\"", media);
        Assert.Contains("BasedOn=\"{StaticResource GscTypographyTimeCell}\"", maintenance);
        Assert.Contains("BasedOn=\"{StaticResource GscTypographyTimeCell}\"", task);
        Assert.Contains("Style=\"{StaticResource GscTypographyPercentCell}\"", task);
        Assert.Contains("ProgressDisplay, Mode=OneWay", task);
        Assert.Contains("Value=\"{Binding ProgressValue, Mode=OneWay}\"", task);
        Assert.DoesNotContain("ProgressPercent, Mode=OneWay, StringFormat={}{0}%", task);
    }

    private static object SetterValue(Style style, DependencyProperty property)
        => style.Setters.OfType<Setter>().Single(setter => setter.Property == property).Value;

    private static ResourceDictionary LoadProductionResources()
        => (ResourceDictionary)XamlReader.Parse(@"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""><ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/Redesign.xaml""/>
</ResourceDictionary.MergedDictionaries></ResourceDictionary>");
}
