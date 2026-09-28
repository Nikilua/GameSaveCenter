using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("Q13ScrollBarThumbWpf")]
public sealed class Q13ScrollBarThumbBehaviorTests
{
    [Fact]
    public void ProductionThumbCapsStaySymmetricAndHoverFollowsThemeInBothOrientations()
    {
        RunSta(() =>
        {
            EnsureApplication();
            var root = new Grid { Resources = LoadProductionResources() };
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(220) });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) });
            root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
            root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });

            var vertical = CreateScrollBar(Orientation.Vertical, 220);
            var horizontal = CreateScrollBar(Orientation.Horizontal, 220);
            Grid.SetRow(vertical, 0);
            Grid.SetColumn(vertical, 0);
            Grid.SetRow(horizontal, 1);
            Grid.SetColumn(horizontal, 1);
            root.Children.Add(vertical);
            root.Children.Add(horizontal);

            var window = new Window
            {
                Content = root,
                Width = 280,
                Height = 280,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = false,
                Opacity = 0.01
            };

            try
            {
                window.Show();
                FlushLayout(window);
                foreach (var theme in new[] { GameSaveCenterThemeMode.Light, GameSaveCenterThemeMode.Dark })
                {
                    ApplyTheme(root, theme);
                    AssertScrollBar(vertical, theme);
                    AssertScrollBar(horizontal, theme);
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static void AssertScrollBar(ScrollBar scrollBar, GameSaveCenterThemeMode theme)
    {
        scrollBar.ApplyTemplate();
        var track = Assert.IsType<Track>(scrollBar.Template!.FindName("PART_Track", scrollBar));
        var thumb = Assert.IsType<Thumb>(track.Thumb);
        var chrome = Assert.IsType<Rectangle>(thumb.Template!.FindName("ThumbChrome", thumb));
        var isVertical = scrollBar.Orientation == Orientation.Vertical;

        Assert.True(scrollBar.IsLoaded && track.IsLoaded && thumb.IsLoaded);
        Assert.Equal(12, isVertical ? scrollBar.ActualWidth : scrollBar.ActualHeight, 0.5);
        Assert.Equal(220, isVertical ? scrollBar.ActualHeight : scrollBar.ActualWidth, 0.5);
        Assert.Equal(isVertical ? new Thickness(0, 4, 0, 4) : new Thickness(4, 0, 4, 0), track.Margin);
        Assert.Equal(36, isVertical ? thumb.MinHeight : thumb.MinWidth, 0.1);
        var thumbLength = isVertical ? thumb.ActualHeight : thumb.ActualWidth;
        var thumbThickness = isVertical ? thumb.ActualWidth : thumb.ActualHeight;
        Assert.True(thumbLength >= 36 - 0.5 && thumbThickness >= 12 - 0.5,
            $"{theme}/{scrollBar.Orientation}: thumb hit target is below its minimum: {thumb.ActualWidth:0.##}x{thumb.ActualHeight:0.##} DIP.");

        Assert.Equal(isVertical ? new Thickness(2, 1, 2, 1) : new Thickness(1, 2, 1, 2), chrome.Margin);
        Assert.Equal(4, chrome.RadiusX, 0.1);
        Assert.Equal(4, chrome.RadiusY, 0.1);
        Assert.Same(chrome, Assert.Single(FindVisualChildren<Rectangle>(thumb)));
        var chromeBounds = BoundsRelativeTo(chrome, thumb);
        Assert.Equal(chrome.Margin.Left, chromeBounds.Left, 0.5);
        Assert.Equal(chrome.Margin.Top, chromeBounds.Top, 0.5);
        Assert.Equal(thumb.ActualWidth - chrome.Margin.Left - chrome.Margin.Right, chrome.ActualWidth, 0.5);
        Assert.Equal(thumb.ActualHeight - chrome.Margin.Top - chrome.Margin.Bottom, chrome.ActualHeight, 0.5);

        AssertThumbAtOppositeTrackEnds(scrollBar, track, thumb, isVertical);
        AssertHoverUsesThemeResources(scrollBar, thumb, chrome, theme);
    }

    private static void AssertThumbAtOppositeTrackEnds(ScrollBar scrollBar, Track track, Thumb thumb, bool isVertical)
    {
        scrollBar.Value = scrollBar.Minimum;
        FlushLayout(scrollBar);
        var atMinimum = ReadThumbBounds(thumb, track, isVertical);
        scrollBar.Value = scrollBar.Maximum;
        FlushLayout(scrollBar);
        var atMaximum = ReadThumbBounds(thumb, track, isVertical);
        var trackLength = isVertical ? track.ActualHeight : track.ActualWidth;
        var thumbLength = isVertical ? thumb.ActualHeight : thumb.ActualWidth;

        Assert.True(Math.Abs(atMinimum.NearGap) <= 0.75 || Math.Abs(atMinimum.FarGap) <= 0.75,
            $"{scrollBar.Orientation} minimum endpoint does not reach either track end: {atMinimum}.");
        Assert.True(Math.Abs(atMaximum.NearGap) <= 0.75 || Math.Abs(atMaximum.FarGap) <= 0.75,
            $"{scrollBar.Orientation} maximum endpoint does not reach either track end: {atMaximum}.");
        Assert.NotEqual(Math.Abs(atMinimum.NearGap) <= 0.75, Math.Abs(atMaximum.NearGap) <= 0.75);
        Assert.Equal(trackLength - thumbLength, atMinimum.NearGap + atMinimum.FarGap, 0.75);
        Assert.Equal(trackLength - thumbLength, atMaximum.NearGap + atMaximum.FarGap, 0.75);
        Assert.Equal(thumbLength, isVertical ? thumb.ActualHeight : thumb.ActualWidth, 0.1);
    }

    private static (double NearGap, double FarGap) ReadThumbBounds(Thumb thumb, Track track, bool isVertical)
    {
        var bounds = BoundsRelativeTo(thumb, track);
        var near = isVertical ? bounds.Top : bounds.Left;
        var length = isVertical ? bounds.Height : bounds.Width;
        var trackLength = isVertical ? track.ActualHeight : track.ActualWidth;
        return (near, trackLength - near - length);
    }

    private static void AssertHoverUsesThemeResources(
        ScrollBar scrollBar,
        Thumb thumb,
        Rectangle chrome,
        GameSaveCenterThemeMode theme)
    {
        var normal = BrushColor(thumb.Background);
        var expectedNormal = BrushColor(scrollBar.FindResource("GscScrollThumbBrush"));
        var expectedHover = BrushColor(scrollBar.FindResource("GscScrollThumbHoverBrush"));
        Assert.Equal(expectedNormal, normal);
        Assert.NotEqual(expectedNormal, expectedHover);

        Assert.True(SetMouseOverProbe(thumb, true), $"{theme}/{scrollBar.Orientation}: WPF hover probe did not enter the Thumb.");
        FlushLayout(scrollBar);
        Assert.True(thumb.IsMouseOver);
        Assert.Equal(expectedHover, BrushColor(thumb.Background));
        Assert.Equal(expectedHover, BrushColor(chrome.Fill));

        Assert.True(SetMouseOverProbe(thumb, false), $"{theme}/{scrollBar.Orientation}: WPF hover probe did not leave the Thumb.");
        FlushLayout(scrollBar);
        Assert.False(thumb.IsMouseOver);
        Assert.Equal(expectedNormal, BrushColor(thumb.Background));
        Assert.Equal(expectedNormal, BrushColor(chrome.Fill));
    }

    private static ScrollBar CreateScrollBar(Orientation orientation, double length)
    {
        var scrollBar = new ScrollBar
        {
            Orientation = orientation,
            Minimum = 0,
            Maximum = 1000,
            ViewportSize = 1,
            Value = 0,
            SmallChange = 1,
            LargeChange = 10
        };
        if (orientation == Orientation.Horizontal)
            scrollBar.Width = length;
        else
            scrollBar.Height = length;
        return scrollBar;
    }

    private static ResourceDictionary LoadProductionResources()
        => Assert.IsType<ResourceDictionary>(XamlReader.Parse(@"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""><ResourceDictionary.MergedDictionaries>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/DesignTokens.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/WpfUiProduction.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/Redesign.xaml""/>
    <ResourceDictionary Source=""/GameSaveCenter.Playnite;component/Themes/AcrylicProductionResources.xaml""/>
</ResourceDictionary.MergedDictionaries></ResourceDictionary>"));

    private static void ApplyTheme(FrameworkElement host, GameSaveCenterThemeMode mode)
    {
        var palette = AdaptiveThemePaletteFactory.CreateWithHighContrastOverride(
            host,
            glassEnabled: true,
            strengthPercent: 78,
            themeMode: mode,
            highContrastOverride: false);
        AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(host.Resources, palette, glassEnabled: true, motionEnabled: true);
        host.UpdateLayout();
    }

    private static Color BrushColor(object? value)
        => Assert.IsType<SolidColorBrush>(value).Color;

    private static bool SetMouseOverProbe(UIElement element, bool value)
    {
        // Drive WPF's own hover state transition without moving the user's OS cursor.
        var changeMouseOver = typeof(MouseDevice).GetMethod("ChangeMouseOver", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("WPF MouseDevice.ChangeMouseOver is unavailable.");
        changeMouseOver.Invoke(Mouse.PrimaryDevice, new object?[] { value ? element : null, Environment.TickCount });
        return element is FrameworkElement frameworkElement && frameworkElement.IsMouseOver == value;
    }

    private static Rect BoundsRelativeTo(FrameworkElement element, FrameworkElement ancestor)
    {
        var origin = element.TransformToAncestor(ancestor).Transform(new Point(0, 0));
        return new Rect(origin.X, origin.Y, element.ActualWidth, element.ActualHeight);
    }

    private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
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

    private static void FlushLayout(DependencyObject element)
    {
        var dispatcher = element.Dispatcher;
        dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
        if (element is FrameworkElement frameworkElement)
            frameworkElement.UpdateLayout();
    }

    private static void EnsureApplication()
    {
        if (Application.Current == null)
            _ = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
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

[CollectionDefinition("Q13ScrollBarThumbWpf", DisableParallelization = true)]
public sealed class Q13ScrollBarThumbWpfCollection
{
}
