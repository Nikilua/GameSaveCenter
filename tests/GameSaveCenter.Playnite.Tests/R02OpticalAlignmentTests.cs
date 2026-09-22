using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Markup;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R02OpticalAlignmentTests
{
    [Fact]
    public void SharedTextButtonsKeepTheSameBaselineForChineseAndEnglishLabels()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        Exception? exception = null;
        double chineseBaseline = 0;
        double englishBaseline = 0;
        double chineseHeight = 0;
        double englishHeight = 0;

        var thread = new Thread(() =>
        {
            try
            {
                var resources = LoadProductionResources();
                var chinese = new GameSaveCenter.Playnite.Controls.Button
                {
                    Style = Assert.IsType<Style>(resources["GscWpfUiPrimaryButton"]),
                    Content = "重新校验",
                    Width = 180,
                    Height = 36
                };
                var english = new GameSaveCenter.Playnite.Controls.Button
                {
                    Style = Assert.IsType<Style>(resources["GscWpfUiSecondaryButton"]),
                    Content = "View Details",
                    Width = 180,
                    Height = 36
                };
                var panel = new StackPanel { Orientation = Orientation.Horizontal };
                panel.Children.Add(chinese);
                panel.Children.Add(english);
                var host = new Border { Resources = resources, Child = panel };
                host.Measure(new Size(400, 60));
                host.Arrange(new Rect(0, 0, 400, 60));
                host.UpdateLayout();
                chinese.ApplyTemplate();
                english.ApplyTemplate();
                host.UpdateLayout();

                var chineseText = FindVisualChild<TextBlock>(chinese);
                var englishText = FindVisualChild<TextBlock>(english);
                Assert.NotNull(chineseText);
                Assert.NotNull(englishText);
                chineseBaseline = BaselineRelativeToButton(chinese, chineseText!);
                englishBaseline = BaselineRelativeToButton(english, englishText!);
                chineseHeight = chinese.ActualHeight;
                englishHeight = english.ActualHeight;
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
        Assert.Equal(36, chineseHeight);
        Assert.Equal(36, englishHeight);
        Assert.InRange(Math.Abs(chineseBaseline - englishBaseline), 0, 0.5);
    }

    [Fact]
    public void SharedButtonRendersVisualContentWithoutStringifyingTheControlTree()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        Exception? exception = null;
        StackPanel? renderedContent = null;
        string? renderedText = null;

        var thread = new Thread(() =>
        {
            try
            {
                var resources = LoadProductionResources();
                var content = new StackPanel { Orientation = Orientation.Horizontal };
                content.Children.Add(new TextBlock { Text = "立即备份" });
                var button = new GameSaveCenter.Playnite.Controls.Button
                {
                    Style = Assert.IsType<Style>(resources["GscWpfUiPrimaryButton"]),
                    Content = content,
                    Width = 160,
                    Height = 36
                };
                var host = new Border { Resources = resources, Child = button };
                host.Measure(new Size(180, 60));
                host.Arrange(new Rect(0, 0, 180, 60));
                host.UpdateLayout();
                button.ApplyTemplate();
                host.UpdateLayout();

                renderedContent = FindVisualChild<StackPanel>(button);
                renderedText = FindVisualChild<TextBlock>(button)?.Text;

                var textButton = new GameSaveCenter.Playnite.Controls.Button
                {
                    Style = Assert.IsType<Style>(resources["GscWpfUiPrimaryButton"]),
                    Content = "重新校验",
                    Width = 160,
                    Height = 36
                };
                var textHost = new Border { Resources = resources, Child = textButton };
                textHost.Measure(new Size(180, 60));
                textHost.Arrange(new Rect(0, 0, 180, 60));
                textHost.UpdateLayout();
                textButton.ApplyTemplate();
                textHost.UpdateLayout();
                Assert.Equal("重新校验", FindVisualChild<TextBlock>(textButton)?.Text);
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
        Assert.NotNull(renderedContent);
        Assert.Equal("立即备份", renderedText);
        Assert.DoesNotContain("System.Windows.Controls", renderedText, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(16d)]
    [InlineData(20d)]
    public void CompositeIconAndCountButtonsKeepAnEightDipGapAndSharedCenter(double iconSize)
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        Exception? exception = null;
        double gap = 0;
        double centerDifference = 0;
        double buttonHeight = 0;

        var thread = new Thread(() =>
        {
            try
            {
                var resources = LoadProductionResources();
                var icon = new GameSaveCenter.Playnite.Controls.ThemeAwareIcon
                {
                    Style = Assert.IsType<Style>(resources["GscLineIcon"]),
                    IconData = Assert.IsAssignableFrom<Geometry>(resources["GscIconBtnRefresh"]),
                    Width = iconSize,
                    Height = iconSize,
                    VerticalAlignment = VerticalAlignment.Center
                };
                var count = new TextBlock
                {
                    Text = "8",
                    Margin = new Thickness(8, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 14
                };
                var content = new Grid { VerticalAlignment = VerticalAlignment.Center };
                content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(iconSize) });
                content.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                Grid.SetColumn(count, 1);
                content.Children.Add(icon);
                content.Children.Add(count);

                var button = new GameSaveCenter.Playnite.Controls.Button
                {
                    Style = Assert.IsType<Style>(resources["GscRedesignHeaderVisualButton"]),
                    Content = content,
                    Width = 160,
                    Height = 36
                };
                var host = new Border { Resources = resources, Child = button };
                host.Measure(new Size(180, 60));
                host.Arrange(new Rect(0, 0, 180, 60));
                host.UpdateLayout();
                button.ApplyTemplate();
                host.UpdateLayout();

                var iconBounds = BoundsRelativeToButton(button, icon);
                var countBounds = BoundsRelativeToButton(button, count);
                gap = countBounds.Left - iconBounds.Right;
                centerDifference = Math.Abs(iconBounds.Top + iconBounds.Height / 2
                                            - (countBounds.Top + countBounds.Height / 2));
                buttonHeight = button.ActualHeight;
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
        Assert.Equal(36, buttonHeight);
        Assert.InRange(gap, 7.5, 8.5);
        Assert.InRange(centerDifference, 0, 1.5);
    }

    private static ResourceDictionary LoadProductionResources()
        => (ResourceDictionary)XamlReader.Parse(@"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""><ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/Redesign.xaml""/>
</ResourceDictionary.MergedDictionaries></ResourceDictionary>");

    private static double BaselineRelativeToButton(FrameworkElement button, TextBlock text)
    {
        var origin = text.TransformToAncestor(button).Transform(new Point(0, 0));
        return origin.Y + text.BaselineOffset;
    }

    private static Rect BoundsRelativeToButton(FrameworkElement button, FrameworkElement element)
    {
        var origin = element.TransformToAncestor(button).Transform(new Point(0, 0));
        return new Rect(origin, new Size(element.ActualWidth, element.ActualHeight));
    }

    private static T? FindVisualChild<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
            {
                return match;
            }

            var nested = FindVisualChild<T>(child);
            if (nested is not null)
            {
                return nested;
            }
        }

        return null;
    }
}
