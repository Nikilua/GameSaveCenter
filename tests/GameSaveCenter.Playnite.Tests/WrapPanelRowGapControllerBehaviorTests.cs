using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class WrapPanelRowGapControllerBehaviorTests
{
    [Fact]
    public void ActualRowsReactToResizeVisibilityContentAndReloadAndRestoreAuthoredMargins()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        RunSta(() =>
        {
            if (Application.Current is null)
                _ = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            var panel = new WrapPanel { VerticalAlignment = VerticalAlignment.Top };
            var label = new TextBlock { Text = "Label", Width = 60, Height = 18, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(2, 0, 8, 5) };
            var middle = new Border { Width = 60, Height = 36, Margin = new Thickness(0, 0, 10, 8) };
            var last = new Border { Width = 60, Height = 36 };
            panel.Children.Add(label);
            panel.Children.Add(middle);
            panel.Children.Add(last);
            var controller = new WrapPanelRowGapController(panel);
            controller.SetRowGap(20);
            var window = new Window { Content = panel, Width = 220, Height = 180, WindowStyle = WindowStyle.None, ResizeMode = ResizeMode.NoResize, ShowInTaskbar = false, ShowActivated = false, Opacity = 0.01 };
            try
            {
                window.Show();
                Flush(window);
                AssertMargins(panel, 5, 8, 0);
                AssertSingleRow(panel);
                // A centered short label must not be mistaken for a second row.
                Assert.NotEqual(label.TranslatePoint(new Point(), panel).Y, middle.TranslatePoint(new Point(), panel).Y);

                window.Width = 140;
                Flush(window);
                AssertMargins(panel, 20, 20, 20);
                Assert.True(last.TranslatePoint(new Point(), panel).Y - (middle.TranslatePoint(new Point(), panel).Y + middle.ActualHeight) >= 19.5);

                last.Visibility = Visibility.Collapsed;
                Flush(window);
                AssertMargins(panel, 5, 8, 0);
                AssertSingleRow(panel);
                label.Width = 110;
                Flush(window);
                AssertMargins(panel, 20, 20, 20);
                controller.SetRowGap(0);
                Flush(window);
                AssertMargins(panel, 5, 8, 0);
                controller.SetRowGap(20);
                Flush(window);
                AssertMargins(panel, 20, 20, 20);

                for (var reload = 0; reload < 3; reload++)
                {
                    window.Content = null;
                    Flush(window);
                    Assert.False(panel.IsLoaded);
                    label.Width = 60;
                    window.Content = panel;
                    Flush(window);
                    Assert.True(panel.IsLoaded);
                    AssertMargins(panel, 5, 8, 0);
                    label.Width = 110;
                    Flush(window);
                    AssertMargins(panel, 20, 20, 20);
                }

                panel.Children.Remove(label);
                var replacement = new Border { Width = 50, Height = 36, Margin = new Thickness(3, 0, 6, 11) };
                panel.Children.Add(replacement);
                Flush(window);
                AssertMargins(panel, 8, 0, 11);
                AssertSingleRow(panel);
                Assert.Equal(new Thickness(3, 0, 6, 11), replacement.Margin);
            }
            finally { window.Close(); }
        });
    }

    [Fact]
    public void ManuallyArrangedPanelWithoutLoadedEventUsesActualRows()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        RunSta(() =>
        {
            var panel = new WrapPanel();
            panel.Children.Add(new Border { Width = 90, Height = 36, Margin = new Thickness(0, 0, 0, 4) });
            panel.Children.Add(new Border { Width = 90, Height = 36, Margin = new Thickness(0, 0, 0, 6) });
            var controller = new WrapPanelRowGapController(panel);
            controller.SetRowGap(20);
            Arrange(panel, 120);
            Assert.False(panel.IsLoaded);
            AssertMargins(panel, 20, 20);
            Arrange(panel, 220);
            AssertMargins(panel, 4, 6);
            AssertSingleRow(panel);
        });
    }

    private static void Arrange(WrapPanel panel, double width)
    {
        for (var pass = 0; pass < 3; pass++)
        {
            panel.Measure(new Size(width, double.PositiveInfinity));
            panel.Arrange(new Rect(0, 0, width, panel.DesiredSize.Height));
            panel.UpdateLayout();
        }
    }

    private static void AssertMargins(WrapPanel panel, params double[] expected)
        => Assert.Equal(expected, panel.Children.OfType<FrameworkElement>().Select(child => child.Margin.Bottom).ToArray());

    private static void AssertSingleRow(WrapPanel panel)
        => Assert.Single(panel.Children.OfType<FrameworkElement>().Where(child => child.Visibility != Visibility.Collapsed)
            .Select(child => LayoutInformation.GetLayoutSlot(child).Y).Distinct());

    private static void Flush(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(() => { }, DispatcherPriority.Background);
        window.UpdateLayout();
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() => { try { action(); } catch (Exception exception) { failure = exception; } });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        Assert.Null(failure);
    }
}
