using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R05TooltipTimingBehaviorTests
{
    [Fact]
    public void ProductionTooltipWrapsLongPathsAndEscClosesWithoutMovingFocus()
    {
        RunSta(() =>
        {
            var shell = new AcrylicProductionShellView();
            var root = new Grid { Margin = new Thickness(18) };
            var path = @"C:\Synthetic\Profiles\GameSaveCenter\Profiles\LongGameName\SaveData\slot-0001\screenshots\2026\09\17\a-very-long-file-name-that-must-remain-readable-without-changing-the-real-files.png";
            var owner = new TextBox
            {
                Width = 180,
                Text = path,
                IsReadOnly = true,
                Style = (Style)shell.FindResource("GscWpfUiPathDetailTextBox")
            };
            var tooltip = new ToolTip
            {
                Content = path,
                Style = (Style)shell.FindResource(typeof(ToolTip)),
                PlacementTarget = owner
            };
            owner.ToolTip = tooltip;
            root.Children.Add(owner);
            shell.Content = root;

            var window = new Window
            {
                Content = shell,
                Width = 260,
                Height = 150,
                ShowInTaskbar = false,
                ShowActivated = true,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };

            try
            {
                window.Show();
                window.UpdateLayout();
                owner.Focus();
                Keyboard.Focus(owner);
                Assert.Same(owner, Keyboard.FocusedElement);
                Assert.Equal(350, ToolTipService.GetInitialShowDelay(shell));
                Assert.Equal(100, ToolTipService.GetBetweenShowDelay(shell));

                tooltip.IsOpen = true;
                DrainDispatcher();
                window.UpdateLayout();

                Assert.True(tooltip.IsOpen);
                Assert.False(tooltip.Focusable);
                Assert.Equal(PlacementMode.Mouse, tooltip.Placement);
                Assert.Equal(420, tooltip.MaxWidth);
                Assert.Same(owner, Keyboard.FocusedElement);

                var text = FindVisualChildren<TextBlock>(tooltip).Single(block => block.Text == path);
                Assert.Equal(TextWrapping.Wrap, text.TextWrapping);
                Assert.Equal(TextTrimming.None, text.TextTrimming);
                Assert.True(text.ActualHeight > tooltip.FontSize,
                    $"长路径应在 Tooltip 内换行，实际高度为 {text.ActualHeight}，字体大小为 {tooltip.FontSize}。 ");
                Assert.InRange(tooltip.ActualWidth, 1, tooltip.MaxWidth + 1);
                Assert.InRange(text.ActualWidth, 1, tooltip.MaxWidth + 1);

                var focusedBeforeEscape = Keyboard.FocusedElement;
                var escape = new KeyEventArgs(
                    Keyboard.PrimaryDevice,
                    PresentationSource.FromVisual(window)!,
                    0,
                    Key.Escape)
                {
                    RoutedEvent = Keyboard.PreviewKeyDownEvent
                };
                owner.RaiseEvent(escape);
                DrainDispatcher();

                Assert.True(escape.Handled);
                Assert.False(tooltip.IsOpen);
                Assert.Same(focusedBeforeEscape, Keyboard.FocusedElement);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static T[] FindVisualChildren<T>(DependencyObject root) where T : DependencyObject
    {
        var results = new System.Collections.Generic.List<T>();
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match) results.Add(match);
            results.AddRange(FindVisualChildren<T>(child));
        }

        return results.ToArray();
    }

    private static void DrainDispatcher()
    {
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
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
