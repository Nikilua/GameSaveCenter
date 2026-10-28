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
using System.Xml.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;
using Xunit.Abstractions;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R05FocusBoundaryBehaviorTests
{
    private readonly ITestOutputHelper output;

    public R05FocusBoundaryBehaviorTests(ITestOutputHelper output) => this.output = output;
    [Theory]
    [InlineData(640d, false)]
    [InlineData(320d, true)]
    public void OpeningPickerEntersContentAndTabDoesNotReachBehindTheOverlay(double height, bool compact)
    {
        RunSta(() =>
        {
            using var picker = CreatePicker("旧游戏", "目标游戏");
            using var host = CreatePickerHost(picker, out var shell, out var overlay, out var search, out var list, height);
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
            Assert.Equal(compact ? Visibility.Collapsed : Visibility.Visible, ((FrameworkElement)shell.FindName("PickerFilterRow")!).Visibility);
            if (compact)
            {
                Assert.False(status.IsVisible);
                Assert.False(status.Focus());
                Assert.False(status.IsKeyboardFocusWithin);
                Assert.Same(search, Keyboard.FocusedElement);
            }

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
    public void DashboardConfirmationKeepsTabFocusInsideAndRestoresItsTriggerAfterClose()
    {
        RunSta(() =>
        {
            var plugin = (GameSaveCenterPlugin)FormatterServices.GetUninitializedObject(typeof(GameSaveCenterPlugin));
            var animationsEnabled = false;
            var dashboard = new DashboardView(plugin, () => animationsEnabled, useMinimalDialogBehaviorFixture: true);
            var overlay = (Grid)dashboard.FindName("DialogOverlay")!;
            var dialogHost = Assert.IsType<Grid>(VisualTreeHelper.GetParent(overlay));
            var trigger = new Button
            {
                Content = "Synthetic dialog trigger",
                Width = 160,
                Height = 36,
                Focusable = true,
                IsTabStop = true,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(16)
            };
            Panel.SetZIndex(trigger, 50);
            dialogHost.Children.Insert(0, trigger);
            var card = (Border)dashboard.FindName("DialogCard")!;
            var cancel = (Button)dashboard.FindName("DialogCancelButton")!;
            var confirm = (Button)dashboard.FindName("DialogConfirmButton")!;
            card.Resources["GscMotionNormal"] = new Duration(TimeSpan.FromMilliseconds(240));

            var host = new Window
            {
                Content = dashboard,
                Width = 1100,
                Height = 760,
                ShowInTaskbar = false,
                ShowActivated = true,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };

            try
            {
                host.Show();
                host.UpdateLayout();
                Assert.True(trigger.IsVisible);
                Assert.True(trigger.IsEnabled);
                Assert.True(trigger.Focusable);
                Assert.True(trigger.Focus());
                Assert.Same(trigger, Keyboard.Focus(trigger));

                var reducedMotionRequest = new UiConfirmationEventArgs("确认", "合成测试", "继续", "取消", true);
                RaiseConfirmationRequest(dashboard, plugin, reducedMotionRequest);

                Assert.True(reducedMotionRequest.Handled);
                Assert.True(WaitFor(host, () => ReferenceEquals(Keyboard.FocusedElement, cancel)));
                Assert.Equal(Visibility.Visible, overlay.Visibility);
                Assert.Same(cancel, Keyboard.FocusedElement);

                Assert.True(cancel.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next)));
                Assert.Same(confirm, Keyboard.FocusedElement);
                Assert.True(IsDescendantOf(Keyboard.FocusedElement, overlay));
                Assert.True(confirm.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next)));
                Assert.Same(cancel, Keyboard.FocusedElement);
                Assert.True(IsDescendantOf(Keyboard.FocusedElement, overlay));
                Assert.True(cancel.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous)));
                Assert.Same(confirm, Keyboard.FocusedElement);
                Assert.True(IsDescendantOf(Keyboard.FocusedElement, overlay));

                cancel.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.Equal(Visibility.Collapsed, overlay.Visibility);
                Assert.True(WaitFor(host, () => ReferenceEquals(Keyboard.FocusedElement, trigger)));
                Assert.False(reducedMotionRequest.Completion.Task.Result);

                animationsEnabled = true;
                Assert.True(trigger.Focus());
                Assert.Same(trigger, Keyboard.Focus(trigger));
                var animatedRequest = new UiConfirmationEventArgs("确认", "合成测试", "继续", "取消", true);
                RaiseConfirmationRequest(dashboard, plugin, animatedRequest);

                Assert.True(WaitFor(host, () =>
                    ReferenceEquals(Keyboard.FocusedElement, cancel)
                    && !DependencyPropertyHelper.GetValueSource(card, UIElement.OpacityProperty).IsAnimated));
                Assert.Equal(Visibility.Visible, overlay.Visibility);

                cancel.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.Equal(Visibility.Visible, overlay.Visibility);
                Assert.True(IsDescendantOf(Keyboard.FocusedElement, overlay));
                // A second click and a new request during the exit must not turn the
                // already-cancelled decision into confirmation or replace its content.
                confirm.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                var overlappingRequest = new UiConfirmationEventArgs("另一项", "不能覆盖退出中的对话框", "继续", "取消", true);
                RaiseConfirmationRequest(dashboard, plugin, overlappingRequest);
                Assert.True(overlappingRequest.Handled);
                Assert.False(overlappingRequest.Completion.Task.Result);
                Assert.False(animatedRequest.Completion.Task.Result);
                Assert.Equal("确认", ((TextBlock)dashboard.FindName("DialogTitleText")!).Text);
                Assert.True(WaitFor(host, () =>
                    overlay.Visibility == Visibility.Collapsed
                    && ReferenceEquals(Keyboard.FocusedElement, trigger)));
                Assert.False(animatedRequest.Completion.Task.Result);
            }
            finally
            {
                host.Close();
            }
        });
    }

    [Fact]
    public void DashboardDialogDeclaresAnOverlayFocusScopeAndReturnFocusPath()
    {
        var root = TestRepositoryContext.Root;
        var xaml = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml"));
        var code = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml.cs"));
        var document = XDocument.Parse(xaml);
        XNamespace xamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
        var cancelButton = document.Descendants().Single(element => (string?)element.Attribute(xamlNamespace + "Name") == "DialogCancelButton");
        var confirmButton = document.Descendants().Single(element => (string?)element.Attribute(xamlNamespace + "Name") == "DialogConfirmButton");

        Assert.Contains("x:Name=\"DialogOverlay\"", xaml);
        Assert.Contains("FocusManager.IsFocusScope=\"True\"", xaml);
        Assert.Contains("KeyboardNavigation.TabNavigation=\"Cycle\"", xaml);
        Assert.Contains("KeyboardNavigation.DirectionalNavigation=\"Contained\"", xaml);
        Assert.Equal("OnDialogCancelClick", (string?)cancelButton.Attribute("Click"));
        Assert.Equal("OnDialogConfirmClick", (string?)confirmButton.Attribute("Click"));
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
                Width = 900,
                Height = 640,
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
                var focused = combo.Focus();
                var keyboardFocus = Keyboard.Focus(combo);
                output.WriteLine("Before open: Focus()={0}, keyboard={1}, visible={2}, focusWithin={3}, combo={4}x{5}, overlay={6}x{7}, filterRow={8}", focused, (keyboardFocus as FrameworkElement)?.Name, combo.IsVisible, combo.IsKeyboardFocusWithin, combo.ActualWidth, combo.ActualHeight, overlay.ActualWidth, overlay.ActualHeight, ((FrameworkElement)shell.FindName("PickerFilterRow")!).Visibility);
                Assert.True(combo.IsVisible);
                Assert.True(focused);
                Assert.Same(combo, keyboardFocus);
                Assert.True(combo.IsKeyboardFocusWithin);
                combo.IsDropDownOpen = true;
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));

                Assert.True(combo.IsDropDownOpen);
                var items = Enumerable.Range(0, combo.Items.Count)
                    .Select(index => combo.ItemContainerGenerator.ContainerFromIndex(index))
                    .OfType<ComboBoxItem>()
                    .ToList();
                Assert.NotEmpty(items);
                Assert.All(items, item => Assert.False(KeyboardNavigation.GetIsTabStop(item)));

                output.WriteLine("After open: {0}, focusWithin={1}", DescribeFocus(Keyboard.FocusedElement, overlay), combo.IsKeyboardFocusWithin);
                Assert.True(combo.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next)));
                output.WriteLine("After traversal: {0}, focusWithin={1}, open={2}", DescribeFocus(Keyboard.FocusedElement, overlay), combo.IsKeyboardFocusWithin, combo.IsDropDownOpen);
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
        out ListBox list,
        double height = 640d)
    {
        shell = new AcrylicProductionShellView();
        var window = new Window
        {
            Content = shell,
            Width = 900,
            Height = height,
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

    private static void RaiseConfirmationRequest(
        DashboardView dashboard,
        GameSaveCenterPlugin plugin,
        UiConfirmationEventArgs request)
    {
        var handler = typeof(DashboardView).GetMethod("OnUiConfirmationRequested", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(handler);
        handler!.Invoke(dashboard, new object[] { plugin, request });
    }

    private static bool WaitFor(Window window, Func<bool> predicate)
    {
        var frame = new DispatcherFrame();
        var deadline = DateTime.UtcNow.AddSeconds(3);
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
