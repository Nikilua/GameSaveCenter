using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R05PopupBoundaryBehaviorTests
{
    [Fact]
    public void ProductionPickerFitsAShortShellViewportAndKeepsTheSelectedItemVisible()
    {
        RunSta(() =>
        {
            using var picker = CreatePicker(200);
            using var host = CreatePickerHost(picker, out var shell, out var overlay, out var list);
            list.SelectedIndex = list.Items.Count - 1;
            AttachPickerForInput(shell, picker);
            overlay.Visibility = Visibility.Visible;
            host.Window.UpdateLayout();

            var panel = (FrameworkElement)shell.FindName("PickerPanel")!;
            var scrollViewer = FindVisualChild<ScrollViewer>(list);
            Assert.NotNull(scrollViewer);
            Assert.True(panel.ActualHeight <= overlay.ActualHeight + 1,
                $"Picker panel exceeds short shell (panel={panel.ActualWidth}x{panel.ActualHeight}, overlay={overlay.ActualWidth}x{overlay.ActualHeight}).");
            Assert.True(panel.ActualWidth <= overlay.ActualWidth + 1,
                $"Picker panel exceeds short shell width (panel={panel.ActualWidth}, overlay={overlay.ActualWidth}).");
            Assert.True(scrollViewer!.ScrollableHeight > 0);

            list.ScrollIntoView(list.SelectedItem);
            host.Window.UpdateLayout();
            Assert.True(IsItemVisible(list, list.SelectedIndex),
                $"The selected item must remain visible after short-window layout (index={list.SelectedIndex}, scroll={scrollViewer.VerticalOffset}, extent={scrollViewer.ExtentHeight}, viewport={scrollViewer.ViewportHeight}, actual={scrollViewer.ActualHeight}, realized={list.ItemContainerGenerator.ContainerFromIndex(list.SelectedIndex) != null}).");
            Assert.True(scrollViewer.VerticalScrollBarVisibility == ScrollBarVisibility.Auto);
        });
    }

    [Fact]
    public void SharedComboBoxPopupFlipsInsideTheDesktopWorkAreaAndBoundsItsScrollViewport()
    {
        RunSta(() =>
        {
            var shell = new AcrylicProductionShellView();
            var combo = new ComboBox
            {
                Width = 180,
                MaxDropDownHeight = 120,
                VerticalAlignment = VerticalAlignment.Bottom,
                Style = (Style)shell.FindResource("GscWpfUiComboBox"),
                ItemsSource = Enumerable.Range(0, 100).Select(index => $"选项 {index:000}").ToArray(),
                SelectedIndex = 99
            };
            var root = new Grid { Children = { combo } };
            var workArea = SystemParameters.WorkArea;
            var window = new Window
            {
                Content = root,
                Width = 220,
                Height = 180,
                Left = Math.Max(workArea.Left, workArea.Right - 240),
                Top = Math.Max(workArea.Top, workArea.Bottom - 200),
                ShowInTaskbar = false,
                ShowActivated = true,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };

            try
            {
                window.Show();
                window.UpdateLayout();
                combo.IsDropDownOpen = true;
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
                window.UpdateLayout();

                var popup = combo.Template.FindName("PART_Popup", combo) as Popup;
                Assert.NotNull(popup);
                Assert.True(popup!.IsOpen);
                Assert.NotNull(popup.Child);

                var popupSize = popup.Child!.RenderSize;
                var popupOrigin = popup.Child.PointToScreen(new Point(0, 0));
                var popupRect = new Rect(popupOrigin, popupSize);
                var transformToDevice = PresentationSource.FromVisual(window)!.CompositionTarget.TransformToDevice;
                var physicalWorkArea = new Rect(
                    workArea.Left * transformToDevice.M11,
                    workArea.Top * transformToDevice.M22,
                    workArea.Width * transformToDevice.M11,
                    workArea.Height * transformToDevice.M22);
                Assert.True(physicalWorkArea.Contains(popupRect.TopLeft), $"Popup origin {popupRect.TopLeft} escaped work area {physicalWorkArea}.");
                Assert.True(physicalWorkArea.Contains(popupRect.BottomRight), $"Popup end {popupRect.BottomRight} escaped work area {physicalWorkArea}.");
                Assert.InRange(popupSize.Height, 1, combo.MaxDropDownHeight + 16);

                var scrollViewer = FindVisualChild<ScrollViewer>(popup.Child);
                Assert.NotNull(scrollViewer);
                Assert.True(scrollViewer!.ScrollableHeight > 0);
                Assert.True(scrollViewer.VerticalScrollBarVisibility == ScrollBarVisibility.Auto);
                var selected = combo.ItemContainerGenerator.ContainerFromIndex(combo.SelectedIndex) as ComboBoxItem;
                Assert.NotNull(selected);
                Assert.True(IsComboItemVisible(selected!, scrollViewer), "The selected Popup option should remain visible after edge placement.");
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static GamePickerViewModel CreatePicker(int count)
    {
        var picker = new GamePickerViewModel();
        picker.StatusFilter = "全部";
        picker.SetItems(Enumerable.Range(0, count).Select(index => Game($"Game {index:0000}")).ToArray(), $"Game {count - 1:0000}");
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
        out ListBox list)
    {
        shell = new AcrylicProductionShellView();
        var window = new Window
        {
            Content = shell,
            Width = 720,
            Height = 360,
            ShowInTaskbar = false,
            ShowActivated = true,
            WindowStyle = WindowStyle.None,
            Opacity = 0.01
        };
        window.Show();
        window.UpdateLayout();

        overlay = (Grid)shell.FindName("PickerOverlay")!;
        list = (ListBox)shell.FindName("PickerList")!;
        list.ItemsSource = picker.ItemsView;
        return new WindowHost(window);
    }

    private static void AttachPickerForInput(AcrylicProductionShellView shell, GamePickerViewModel picker)
    {
        var dashboard = (DashboardViewModel)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
        var pickerField = typeof(DashboardViewModel).GetField("gamePicker", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(pickerField);
        pickerField!.SetValue(dashboard, picker);

        var viewModelField = typeof(AcrylicProductionShellView).GetField("viewModel", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(viewModelField);
        viewModelField!.SetValue(shell, dashboard);
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

    private static bool IsItemVisible(ListBox list, int index)
    {
        var item = list.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
        if (item == null) return false;
        var scrollViewer = FindVisualChild<ScrollViewer>(list);
        if (scrollViewer == null) return false;
        var top = item.TransformToAncestor(scrollViewer).Transform(new Point(0, 0)).Y;
        var bottom = top + item.ActualHeight;
        return top >= -1 && bottom <= scrollViewer.ActualHeight + 1;
    }

    private static bool IsComboItemVisible(ComboBoxItem item, ScrollViewer scrollViewer)
    {
        var top = item.TransformToAncestor(scrollViewer).Transform(new Point(0, 0)).Y;
        var bottom = top + item.ActualHeight;
        return top >= -1 && bottom <= scrollViewer.ActualHeight + 1;
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
