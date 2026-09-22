using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R21FocusVisualRegressionBehaviorTests
{
    [Fact]
    public void ProductionPickerKeepsVisibleFocusAcrossScrollThemeSwitchAndClose()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        Exception? failure = null;
        var trace = new List<string>();

        var thread = new Thread(() =>
        {
            Window? window = null;
            try
            {
                using var picker = CreatePicker(2000);
                var shell = new AcrylicProductionShellView();
                var dashboard = (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
                typeof(DashboardViewModel)
                    .GetField("gamePicker", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .SetValue(dashboard, picker);
                typeof(AcrylicProductionShellView)
                    .GetField("viewModel", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .SetValue(shell, dashboard);

                var contextButton = (Button)shell.FindName("GameContextButton")!;
                var overlay = (Grid)shell.FindName("PickerOverlay")!;
                var search = (TextBox)shell.FindName("GameSearchTextBox")!;
                var list = (ListBox)shell.FindName("PickerList")!;
                list.ItemsSource = picker.ItemsView;
                window = new Window
                {
                    Content = shell,
                    Width = 900,
                    Height = 640,
                    ShowInTaskbar = false,
                    ShowActivated = true,
                    WindowStyle = WindowStyle.None,
                    Opacity = 0.01
                };

                window.Show();
                window.Activate();
                FlushLayout(window);
                Assert.True(contextButton.IsVisible, "the production game picker trigger must remain visible before opening the overlay");

                shell.OpenGamePicker();
                FlushLayout(window);
                Assert.Equal(Visibility.Visible, overlay.Visibility);
                Assert.True(search.Focus());
                Assert.Same(search, Keyboard.Focus(search));
                FlushLayout(window);
                Assert.Same(search, Keyboard.FocusedElement);

                FlushLayout(window);
                var scrollViewer = FindScrollViewer(list);
                Assert.True(scrollViewer.ScrollableHeight > 0, "the synthetic picker must expose a scrollable viewport");

                list.ScrollIntoView(list.Items[list.Items.Count - 1]);
                Assert.True(WaitFor(window, () => list.ItemContainerGenerator.ContainerFromIndex(list.Items.Count - 1) is ListBoxItem),
                    "the production picker should realize the item requested by ScrollIntoView");
                var lastItem = Assert.IsType<ListBoxItem>(list.ItemContainerGenerator.ContainerFromIndex(list.Items.Count - 1));
                Assert.NotNull(lastItem.FocusVisualStyle);
                Assert.Same(search, Keyboard.FocusedElement);
                Assert.True(search.IsVisible && search.ActualWidth > 0 && search.ActualHeight > 0);
                Assert.NotNull(search.FocusVisualStyle);
                trace.Add($"scroll: selected={list.SelectedIndex}; focused={search.IsKeyboardFocusWithin}; lastVisible={lastItem.IsVisible}; lastSize={lastItem.ActualWidth:0.###}x{lastItem.ActualHeight:0.###}");

                var light = AdaptiveThemePaletteFactory.Create(shell, true, 78, GameSaveCenterThemeMode.Light);
                AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(shell.Resources, light, true, true);
                FlushLayout(window);
                var lightAccent = ReadBrushColor(shell.Resources["GscAccentBrush"]);
                Assert.Same(search, Keyboard.FocusedElement);
                Assert.True(search.IsVisible);
                Assert.NotNull(search.FocusVisualStyle);

                var dark = AdaptiveThemePaletteFactory.Create(shell, true, 78, GameSaveCenterThemeMode.Dark);
                AdaptiveThemePaletteFactory.ApplyRuntimeThemeResources(shell.Resources, dark, true, true);
                FlushLayout(window);
                var darkAccent = ReadBrushColor(shell.Resources["GscAccentBrush"]);
                Assert.NotEqual(lightAccent, darkAccent);
                Assert.Same(search, Keyboard.FocusedElement);
                Assert.True(search.IsVisible);
                Assert.NotNull(search.FocusVisualStyle);
                trace.Add($"theme: lightAccent={lightAccent}; darkAccent={darkAccent}; focused={search.IsKeyboardFocusWithin}; visible={search.IsVisible}");

                contextButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                FlushLayout(window);
                Assert.Equal(Visibility.Collapsed, overlay.Visibility);
                Assert.Same(contextButton, Keyboard.FocusedElement);
                Assert.True(contextButton.IsVisible && contextButton.Focusable && contextButton.IsKeyboardFocusWithin);
                Assert.NotNull(contextButton.FocusVisualStyle);
                trace.Add($"close: overlay={overlay.Visibility}; returned={contextButton.IsKeyboardFocusWithin}; visible={contextButton.IsVisible}");
            }
            catch (Exception caught)
            {
                failure = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(failure);
        Assert.NotEmpty(trace);
    }

    private static GamePickerViewModel CreatePicker(int count)
    {
        var picker = new GamePickerViewModel();
        picker.StatusFilter = "全部";
        picker.SetItems(
            Enumerable.Range(0, count)
                .Select(index => new GameStatusDto
                {
                    PlayniteId = $"r21-focus-{index:0000}",
                    Name = $"焦点回归游戏 {index:0000}",
                    Platform = GamePlatformKind.Other,
                    IsInstalled = true,
                    LudusaviMatched = true
                })
                .ToArray(),
            "r21-focus-0000");
        return picker;
    }

    private static ScrollViewer FindScrollViewer(DependencyObject root)
    {
        var scrollViewer = FindVisualChild<ScrollViewer>(root);
        Assert.NotNull(scrollViewer);
        return scrollViewer!;
    }

    private static T? FindVisualChild<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match) return match;
            var descendant = FindVisualChild<T>(child);
            if (descendant != null) return descendant;
        }

        return null;
    }

    private static Color ReadBrushColor(object value)
        => Assert.IsType<SolidColorBrush>(value).Color;

    private static void FlushLayout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.Loaded, new Action(() => { }));
        window.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() => { }));
        window.Dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
        window.UpdateLayout();
    }

    private static bool WaitFor(Window window, Func<bool> predicate)
    {
        var frame = new DispatcherFrame();
        var deadline = DateTime.UtcNow.AddMilliseconds(1500);
        var timer = new DispatcherTimer(DispatcherPriority.Background, window.Dispatcher)
        {
            Interval = TimeSpan.FromMilliseconds(10)
        };
        timer.Tick += (_, __) =>
        {
            if (predicate() || DateTime.UtcNow >= deadline)
            {
                timer.Stop();
                frame.Continue = false;
            }
        };
        timer.Start();
        Dispatcher.PushFrame(frame);
        return predicate();
    }
}
