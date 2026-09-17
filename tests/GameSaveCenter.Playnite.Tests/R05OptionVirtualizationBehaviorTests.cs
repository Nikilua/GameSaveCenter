using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R05OptionVirtualizationBehaviorTests
{
    [Fact]
    public void ProductionPickerKeepsA2000ItemListOpenWhileKeyboardNavigationScrollsTheActiveItem()
    {
        RunSta(() =>
        {
            using var picker = CreatePicker(2000);
            using var host = CreatePickerHost(picker, out var shell, out var overlay, out _, out var list);
            list.SelectedIndex = 0;
            AttachPickerForInput(shell, picker);
            overlay.Visibility = Visibility.Visible;
            host.Window.UpdateLayout();

            Assert.Equal(2000, list.Items.Count);
            var initiallyRealized = CountRealizedItems(list);
            Assert.InRange(initiallyRealized, 1, 1999);

            var firstItem = list.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem;
            Assert.NotNull(firstItem);
            Assert.True(firstItem!.Focusable);
            firstItem.Focus();
            Keyboard.Focus(firstItem);
            Assert.Same(firstItem, Keyboard.FocusedElement);
            RaiseKeyDown(firstItem, host.Window, Key.Down);
            host.Window.UpdateLayout();
            Assert.Equal(1, list.SelectedIndex);
            Assert.Equal(Visibility.Visible, overlay.Visibility);

            RaiseKeyDown((FrameworkElement)Keyboard.FocusedElement!, host.Window, Key.PageDown);
            host.Window.UpdateLayout();
            Assert.InRange(list.SelectedIndex, 2, list.Items.Count - 1);
            Assert.True(IsItemVisible(list, list.SelectedIndex), "The keyboard active item should be visible after PageDown.");

            RaiseKeyDown((FrameworkElement)Keyboard.FocusedElement!, host.Window, Key.End);
            host.Window.UpdateLayout();
            Assert.Equal(list.Items.Count - 1, list.SelectedIndex);
            Assert.Equal(Visibility.Visible, overlay.Visibility);
            Assert.True(FindScrollViewer(list).VerticalOffset > 0, "End should keep the viewport away from the top.");
            Assert.True(IsItemVisible(list, list.SelectedIndex), "End should bring the last active item into the visible viewport.");

            RaiseKeyDown((FrameworkElement)Keyboard.FocusedElement!, host.Window, Key.Home);
            host.Window.UpdateLayout();
            Assert.Equal(0, list.SelectedIndex);
            Assert.InRange(FindScrollViewer(list).VerticalOffset, 0, 1);
            Assert.True(IsItemVisible(list, list.SelectedIndex), "Home should bring the first active item into the visible viewport.");
        });
    }

    [Fact]
    public void FilteringRetainsAHiddenSelectionProvidesRecoveryAndFallsBackAfterDeletion()
    {
        RunSta(() =>
        {
            using var picker = CreatePicker(2000);
            var selected = picker.Items[1000];
            picker.SelectGame(selected.Game);

            picker.SearchText = selected.Name;
            picker.RefreshNow();
            Assert.Same(selected, picker.SelectedItem);
            Assert.Equal(1, picker.FilteredCount);
            Assert.False(picker.SelectedGameHiddenByFilter);

            picker.SearchText = "没有这款游戏";
            picker.RefreshNow();
            Assert.Same(selected, picker.SelectedItem);
            Assert.Equal(0, picker.FilteredCount);
            Assert.True(picker.SelectedGameHiddenByFilter);
            Assert.True(picker.ShowSelectedGameCommand.CanExecute(null));

            picker.ShowSelectedGameCommand.Execute(null);
            picker.RefreshNow();
            Assert.Same(selected, picker.SelectedItem);
            Assert.False(picker.SelectedGameHiddenByFilter);
            Assert.Equal(2000, picker.FilteredCount);

            var remaining = picker.Items
                .Where(item => !string.Equals(item.PlayniteId, selected.PlayniteId, StringComparison.OrdinalIgnoreCase))
                .Select(item => item.Game)
                .ToArray();
            picker.SetItems(remaining, selected.PlayniteId);

            Assert.NotNull(picker.SelectedItem);
            Assert.NotSame(selected, picker.SelectedItem);
            Assert.Equal("Game 0000", picker.SelectedItem!.Name);
            Assert.False(picker.SelectedGameHiddenByFilter);
            Assert.Equal(1999, picker.FilteredCount);
        });
    }

    [Fact]
    public void ProductionPickerKeepsVirtualizedItemsBoundToTheFilteredView()
    {
        RunSta(() =>
        {
            using var picker = CreatePicker(2000);
            picker.SearchText = "Game 19";
            picker.RefreshNow();
            using var host = CreatePickerHost(picker, out _, out var overlay, out _, out var list);
            overlay.Visibility = Visibility.Visible;
            host.Window.UpdateLayout();

            Assert.True(picker.FilteredCount > 0);
            Assert.True(picker.FilteredCount < 2000);
            Assert.Equal(picker.FilteredCount, list.Items.Count);
            Assert.InRange(CountRealizedItems(list), 1, list.Items.Count - 1);
            Assert.All(list.Items.Cast<GamePickerItem>(), item => Assert.Contains("Game 19", item.Name, StringComparison.Ordinal));
        });
    }

    private static GamePickerViewModel CreatePicker(int count)
    {
        var picker = new GamePickerViewModel();
        picker.StatusFilter = "全部";
        picker.SetItems(Enumerable.Range(0, count).Select(index => Game($"Game {index:0000}")).ToArray(), "Game 0000");
        return picker;
    }

    private static GameStatusDto Game(string name)
        => new GameStatusDto
        {
            PlayniteId = name,
            Name = name,
            Platform = GamePlatformKind.Other,
            IsInstalled = true,
            LudusaviMatched = true
        };

    private static WindowHost CreatePickerHost(
        GamePickerViewModel picker,
        out AcrylicProductionShellView shell,
        out Grid overlay,
        out TextBox search,
        out ListBox list)
    {
        shell = new AcrylicProductionShellView();
        var window = new Window
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
        window.UpdateLayout();

        overlay = (Grid)shell.FindName("PickerOverlay")!;
        search = (TextBox)shell.FindName("GameSearchTextBox")!;
        list = (ListBox)shell.FindName("PickerList")!;
        list.ItemsSource = picker.ItemsView;
        return new WindowHost(window);
    }

    private static void AttachPickerForInput(AcrylicProductionShellView shell, GamePickerViewModel picker)
    {
        var dashboard = (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
        var pickerField = typeof(DashboardViewModel).GetField("gamePicker", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(pickerField);
        pickerField!.SetValue(dashboard, picker);

        var viewModelField = typeof(AcrylicProductionShellView).GetField("viewModel", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(viewModelField);
        viewModelField!.SetValue(shell, dashboard);
    }

    private static void RaiseKeyDown(FrameworkElement source, Window host, Key keyValue)
    {
        var preview = new KeyEventArgs(
            Keyboard.PrimaryDevice,
            PresentationSource.FromVisual(host)!,
            0,
            keyValue)
        {
            RoutedEvent = Keyboard.PreviewKeyDownEvent
        };
        source.RaiseEvent(preview);

        var key = new KeyEventArgs(
            Keyboard.PrimaryDevice,
            PresentationSource.FromVisual(host)!,
            0,
            keyValue)
        {
            RoutedEvent = Keyboard.KeyDownEvent
        };
        source.RaiseEvent(key);
    }

    private static ScrollViewer FindScrollViewer(DependencyObject root)
    {
        var scrollViewer = FindVisualChild<ScrollViewer>(root);
        Assert.NotNull(scrollViewer);
        return scrollViewer!;
    }

    private static int CountRealizedItems(ListBox list)
        => Enumerable.Range(0, list.Items.Count)
            .Count(index => list.ItemContainerGenerator.ContainerFromIndex(index) is ListBoxItem);

    private static bool IsRealized(ListBox list, int index)
        => list.ItemContainerGenerator.ContainerFromIndex(index) is ListBoxItem;

    private static bool IsItemVisible(ListBox list, int index)
    {
        var item = list.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
        if (item == null) return false;
        var scrollViewer = FindScrollViewer(list);
        var top = item.TransformToAncestor(scrollViewer).Transform(new Point(0, 0)).Y;
        var bottom = top + item.ActualHeight;
        return top >= -1 && bottom <= scrollViewer.ActualHeight + 1;
    }

    private static T? FindVisualChild<T>(DependencyObject root) where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match) return match;
            var nested = FindVisualChild<T>(child);
            if (nested != null) return nested;
        }
        return null;
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
