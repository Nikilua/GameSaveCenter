using System;
using System.Collections.Generic;
using System.IO;
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
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R05FocusBoundaryBehaviorTests
{
    [Fact]
    public void OpeningPickerEntersContentAndTabDoesNotReachBehindTheOverlay()
    {
        RunSta(() =>
        {
            using var picker = CreatePicker("旧游戏", "目标游戏");
            using var host = CreatePickerHost(picker, out var shell, out var overlay, out var search, out var list);
            var contextButton = (Button)shell.FindName("GameContextButton")!;
            var status = (ComboBox)shell.FindName("GamePickerStatusComboBox")!;
            var platform = (ComboBox)shell.FindName("GamePickerPlatformComboBox")!;
            var sort = (ComboBox)shell.FindName("GamePickerSortComboBox")!;

            status.ItemsSource = new[] { "全部", "已安装" };
            platform.ItemsSource = new[] { "全部", "Steam" };
            sort.ItemsSource = new[] { "名称", "最近游玩" };
            list.ItemsSource = picker.ItemsView;
            AttachPickerForInput(shell, picker);
            contextButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            host.Window.UpdateLayout();

            Assert.Equal(Visibility.Visible, overlay.Visibility);
            Assert.Same(search, Keyboard.FocusedElement);

            var visited = new List<IInputElement>();
            var returnedToSearch = false;
            for (var index = 0; index < 20; index++)
            {
                var current = Keyboard.FocusedElement;
                Assert.NotNull(current);
                Assert.True(IsDescendantOf(current!, overlay), DescribeFocus(current, overlay));
                visited.Add(current!);

                var element = current as UIElement;
                Assert.NotNull(element);
                Assert.True(element!.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next)));
                if (visited.Count > 1 && ReferenceEquals(Keyboard.FocusedElement, search))
                {
                    returnedToSearch = true;
                    break;
                }
            }

            Assert.True(returnedToSearch);
            Assert.True(visited.Distinct(ReferenceEqualityComparer.Instance).Count() >= 2);
            Assert.NotSame(contextButton, Keyboard.FocusedElement);

            search.Focus();
            Keyboard.Focus(search);
            Assert.True(search.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous)));
            Assert.True(IsDescendantOf(Keyboard.FocusedElement!, overlay), DescribeFocus(Keyboard.FocusedElement, overlay));
            Assert.Same(visited[visited.Count - 1], Keyboard.FocusedElement);
            Assert.NotSame(contextButton, Keyboard.FocusedElement);

            var escape = new KeyEventArgs(
                Keyboard.PrimaryDevice,
                PresentationSource.FromVisual(host.Window)!,
                0,
                Key.Escape)
            {
                RoutedEvent = Keyboard.PreviewKeyDownEvent
            };
            search.RaiseEvent(escape);

            Assert.True(escape.Handled);
            Assert.Equal(Visibility.Collapsed, overlay.Visibility);
            Assert.Same(contextButton, Keyboard.FocusedElement);
        });
    }

    [Fact]
    public void DetailOverlayDeclaresLocalTabCycleAndRestoresTheOpeningTrigger()
    {
        var root = TestRepositoryContext.Root;
        var xaml = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml"));
        var code = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml.cs"));

        Assert.Contains("x:Name=\"DialogOverlay\"", xaml);
        Assert.Contains("FocusManager.IsFocusScope=\"True\"", xaml);
        Assert.Contains("KeyboardNavigation.TabNavigation=\"Cycle\"", xaml);
        Assert.Contains("KeyboardNavigation.DirectionalNavigation=\"Contained\"", xaml);
        Assert.Contains("dialogReturnFocus", code);
        Assert.Contains("Keyboard.Focus(returnFocus);", code);
    }

    [Fact]
    public void ComboBoxPopupKeepsDirectionalSelectionInsideItemsAndTabReturnsToTheForm()
    {
        RunSta(() =>
        {
            var shell = new AcrylicProductionShellView();
            var combo = (ComboBox)shell.FindName("GamePickerStatusComboBox")!;
            combo.ItemsSource = new[] { "全部", "已安装", "需处理" };
            var host = new Window
            {
                Content = shell,
                Width = 260,
                Height = 180,
                ShowInTaskbar = false,
                ShowActivated = true,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };

            try
            {
                host.Show();
                host.UpdateLayout();
                var overlay = (Grid)shell.FindName("PickerOverlay")!;
                overlay.Visibility = Visibility.Visible;
                host.UpdateLayout();
                combo.Focus();
                Keyboard.Focus(combo);
                combo.IsDropDownOpen = true;
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));

                Assert.True(combo.IsDropDownOpen);
                var items = Enumerable.Range(0, combo.Items.Count)
                    .Select(index => combo.ItemContainerGenerator.ContainerFromIndex(index))
                    .OfType<ComboBoxItem>()
                    .ToList();
                Assert.NotEmpty(items);
                Assert.All(items, item => Assert.False(KeyboardNavigation.GetIsTabStop(item)));

                Assert.True(combo.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next)));
                Assert.False(combo.IsDropDownOpen);
                Assert.False(Keyboard.FocusedElement is ComboBoxItem);
                Assert.True(IsDescendantOf(Keyboard.FocusedElement, overlay), DescribeFocus(Keyboard.FocusedElement, overlay));
            }
            finally
            {
                host.Close();
            }
        });
    }

    private static GamePickerViewModel CreatePicker(params string[] names)
    {
        var picker = new GamePickerViewModel();
        picker.StatusFilter = "全部";
        picker.SetItems(names.Select(Game).ToArray(), names[0]);
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

    private static bool IsDescendantOf(IInputElement? element, DependencyObject ancestor)
    {
        if (!(element is DependencyObject current)) return false;
        while (current != null)
        {
            if (ReferenceEquals(current, ancestor)) return true;
            current = VisualTreeHelper.GetParent(current);
        }
        return false;
    }

    private static string DescribeFocus(IInputElement? element, DependencyObject ancestor)
        => $"Focus={element?.GetType().Name ?? "null"}, inside={IsDescendantOf(element, ancestor)}";

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

    private sealed class ReferenceEqualityComparer : IEqualityComparer<IInputElement>
    {
        internal static readonly ReferenceEqualityComparer Instance = new();

        public bool Equals(IInputElement? x, IInputElement? y) => ReferenceEquals(x, y);

        public int GetHashCode(IInputElement obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}
