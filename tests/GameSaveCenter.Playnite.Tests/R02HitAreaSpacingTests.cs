using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

using GscButton = GameSaveCenter.Playnite.Controls.Button;
using ThemeAwareIcon = GameSaveCenter.Playnite.Controls.ThemeAwareIcon;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R02HitAreaSpacingTests
{
    [Fact]
    public void AdjacentCopyAndDeleteButtonsHaveIndependentHitTargets()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        Exception? exception = null;
        Rect copyBounds = Rect.Empty;
        Rect deleteBounds = Rect.Empty;
        Thickness copyMargin = new();
        GscButton? copyButton = null;
        GscButton? deleteButton = null;
        GscButton? copyHit = null;
        GscButton? deleteHit = null;
        GscButton? gapHit = null;

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var resources = LoadProductionResources();
                var row = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Width = 100,
                    Height = 40
                };
                copyButton = CreateIconButton(resources, "GscIconOnlyButtonBase", "GscIconBtnCopy", "复制详情");
                deleteButton = CreateIconButton(resources, "GscIconOnlyDangerButton", "GscIconBtnDelete", "删除策略模板");
                row.Children.Add(copyButton);
                row.Children.Add(deleteButton);
                var host = new Border { Resources = resources, Child = row };
                window = CreateWindow(host, 120, 60);
                window.Show();
                window.UpdateLayout();

                copyBounds = BoundsRelativeTo(copyButton, row);
                deleteBounds = BoundsRelativeTo(deleteButton, row);
                copyMargin = copyButton.Margin;
                copyHit = HitButtonAt(window, Center(copyButton, window));
                deleteHit = HitButtonAt(window, Center(deleteButton, window));
                var gapInRow = new Point((copyBounds.Right + deleteBounds.Left) / 2, copyBounds.Top + copyBounds.Height / 2);
                gapHit = HitButtonAt(window, row.TransformToAncestor(window).Transform(gapInRow));
            }
            catch (Exception caught)
            {
                exception = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
        Assert.Equal(34, copyBounds.Width);
        Assert.Equal(34, copyBounds.Height);
        Assert.Equal(34, deleteBounds.Width);
        Assert.Equal(34, deleteBounds.Height);
        Assert.Equal(6, copyMargin.Right);
        Assert.False(copyBounds.IntersectsWith(deleteBounds));
        Assert.InRange(deleteBounds.Left - copyBounds.Right, 5.5, 6.5);
        Assert.Same(copyButton, copyHit);
        Assert.Same(deleteButton, deleteHit);
        Assert.True(gapHit is null, $"copy={Format(copyBounds)}, delete={Format(deleteBounds)}, gapHit={(gapHit is null ? "none" : "button")}");
    }

    [Fact]
    public void NarrowWrapKeepsCopyAndDeleteInOneActionRow()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        Exception? exception = null;
        Rect[] bounds = Array.Empty<Rect>();
        GscButton?[] buttons = Array.Empty<GscButton?>();
        GscButton?[] hits = Array.Empty<GscButton?>();

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                var resources = LoadProductionResources();
                var row = new WrapPanel { Width = 82 };
                var copy = CreateIconButton(resources, "GscIconOnlyButtonBase", "GscIconBtnCopy", "复制详情");
                var delete = CreateIconButton(resources, "GscIconOnlyDangerButton", "GscIconBtnDelete", "删除策略模板");
                var retry = CreateIconButton(resources, "GscIconOnlyButtonBase", "GscIconBtnRetry", "安全重试任务");
                row.Children.Add(copy);
                row.Children.Add(delete);
                row.Children.Add(retry);
                buttons = new[] { copy, delete, retry };
                var host = new Border { Resources = resources, Child = row };
                window = CreateWindow(host, 120, 120);
                window.Show();
                window.UpdateLayout();

                bounds = new[]
                {
                    BoundsRelativeTo(copy, row),
                    BoundsRelativeTo(delete, row),
                    BoundsRelativeTo(retry, row)
                };
                hits = new[]
                {
                    HitButtonAt(window, Center(copy, window)),
                    HitButtonAt(window, Center(delete, window)),
                    HitButtonAt(window, Center(retry, window))
                };
            }
            catch (Exception caught)
            {
                exception = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
        Assert.Equal(3, bounds.Length);
        Assert.All(bounds, bound =>
        {
            Assert.Equal(34, bound.Width);
            Assert.Equal(34, bound.Height);
        });
        Assert.InRange(Math.Abs(bounds[0].Top - bounds[1].Top), 0, 0.5);
        Assert.True(bounds[2].Top >= bounds[0].Bottom - 0.5, $"third action row started at {bounds[2].Top:0.##}, first row ended at {bounds[0].Bottom:0.##}");
        Assert.InRange(bounds[0].Left, 0, 0.5);
        Assert.InRange(bounds[1].Left - bounds[0].Right, 5.5, 6.5);
        Assert.InRange(bounds[2].Left, 0, 0.5);
        Assert.False(HasStrictOverlap(bounds[0], bounds[1]), Format(bounds));
        Assert.False(HasStrictOverlap(bounds[0], bounds[2]), Format(bounds));
        Assert.False(HasStrictOverlap(bounds[1], bounds[2]), Format(bounds));
        Assert.Equal(buttons.Length, hits.Length);
        for (var index = 0; index < hits.Length; index++)
            Assert.Same(buttons[index], hits[index]);
    }

    private static Window CreateWindow(UIElement content, double width, double height)
        => new()
        {
            Content = content,
            Width = width,
            Height = height,
            ShowInTaskbar = false,
            ShowActivated = false,
            WindowStyle = WindowStyle.None,
            Opacity = 0.01
        };

    private static GscButton CreateIconButton(ResourceDictionary resources, string styleKey, string iconKey, string name)
    {
        var button = new GscButton
        {
            Style = Assert.IsType<Style>(resources[styleKey]),
            ToolTip = name
        };
        AutomationProperties.SetName(button, name);
        button.Content = new ThemeAwareIcon
        {
            Style = Assert.IsType<Style>(resources["GscLineIcon"]),
            IconData = Assert.IsAssignableFrom<Geometry>(resources[iconKey]),
            Width = 18,
            Height = 18
        };
        return button;
    }

    private static GscButton? HitButtonAt(Window window, Point point)
        => FindButton(VisualTreeHelper.HitTest(window, point)?.VisualHit);

    private static GscButton? FindButton(DependencyObject? element)
    {
        while (element is not null)
        {
            if (element is GscButton button)
                return button;
            element = VisualTreeHelper.GetParent(element);
        }

        return null;
    }

    private static Point Center(FrameworkElement element, Visual ancestor)
    {
        var bounds = BoundsRelativeTo(element, ancestor);
        return new Point(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);
    }

    private static Rect BoundsRelativeTo(FrameworkElement element, Visual ancestor)
        => element.TransformToAncestor(ancestor).TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));

    private static string Format(Rect bound)
        => $"({bound.Left:0.##},{bound.Top:0.##},{bound.Width:0.##}x{bound.Height:0.##})";

    private static string Format(IReadOnlyList<Rect> values)
        => string.Join("; ", values.Select(Format));

    private static bool HasStrictOverlap(Rect first, Rect second)
        => Math.Min(first.Right, second.Right) - Math.Max(first.Left, second.Left) > 0.5
            && Math.Min(first.Bottom, second.Bottom) - Math.Max(first.Top, second.Top) > 0.5;

    private static ResourceDictionary LoadProductionResources()
        => (ResourceDictionary)XamlReader.Parse(@"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""><ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/Redesign.xaml""/>
</ResourceDictionary.MergedDictionaries></ResourceDictionary>");
}
