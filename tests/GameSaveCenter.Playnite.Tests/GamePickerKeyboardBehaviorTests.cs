using System;
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

public sealed class GamePickerKeyboardBehaviorTests
{
    [Fact]
    public void ProductionGamePickerOpensAndSelectsTheExactDuplicateNameById()
    {
        RunSta(() =>
        {
            var first = Game("playnite-a", "同名游戏");
            var target = Game("playnite-b", "同名游戏");
            using var picker = CreatePicker(first, target);
            using var host = CreatePickerHost(picker, out var shell, out var overlay, out var search, out var list);
            var dashboard = AttachPickerForInput(shell, picker);
            shell.DataContext = dashboard;
            host.Window.UpdateLayout();

            var contextButton = (Button)shell.FindName("GameContextButton")!;
            contextButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            Assert.Equal(Visibility.Visible, overlay.Visibility);
            Assert.Same(search, Keyboard.FocusedElement);
            var targetItem = picker.Items.Single(item => item.PlayniteId == "playnite-b");
            Assert.Equal("同名游戏", picker.Items.Single(item => item.PlayniteId == "playnite-a").Name);
            Assert.Equal(targetItem.Name, picker.Items.Single(item => item.PlayniteId == "playnite-b").Name);

            list.SelectedItem = targetItem;
            host.Window.UpdateLayout();

            Assert.Equal(Visibility.Collapsed, overlay.Visibility);
            Assert.Same(target, dashboard.SelectedGame);
            Assert.Equal("playnite-b", dashboard.SelectedGame.PlayniteId);
            Assert.Same(contextButton, Keyboard.FocusedElement);
            var displayedName = GetVisualDescendants(contextButton)
                .OfType<TextBlock>()
                .First(block => block.GetBindingExpression(TextBlock.TextProperty)?.ParentBinding.Path?.Path == "SelectedGame.Name");
            Assert.Equal("同名游戏", displayedName.Text);
        });
    }

    [Fact]
    public void EnterWithNoVisibleResultKeepsThePreviouslySelectedGameAndPickerOpen()
    {
        RunSta(() =>
        {
            using var picker = CreatePicker("旧游戏", "另一款");
            var previousGame = picker.SelectedGame;
            picker.SearchText = "不存在的游戏";
            picker.RefreshNow();

            using var host = CreatePickerHost(picker, out var shell, out var overlay, out var search, out var list);
            list.SelectedItem = null;
            overlay.Visibility = Visibility.Visible;
            AttachPickerForInput(shell, picker);
            Keyboard.Focus(search);

            var key = RaisePreviewKey(search, host.Window, Key.Enter);

            Assert.Same(previousGame, picker.SelectedGame);
            Assert.Equal(Visibility.Visible, overlay.Visibility);
            Assert.False(key.Handled);
        });
    }

    [Fact]
    public void ImeProcessedAndArrowKeysRemainInThePickerInputRoute()
    {
        RunSta(() =>
        {
            using var picker = CreatePicker("中文游戏");
            var previousGame = picker.SelectedGame;
            using var host = CreatePickerHost(picker, out var shell, out var overlay, out var search, out _);
            overlay.Visibility = Visibility.Visible;
            AttachPickerForInput(shell, picker);
            Keyboard.Focus(search);

            foreach (var keyValue in new[] { Key.Up, Key.Down, Key.Left, Key.Right, Key.ImeProcessed })
            {
                var key = RaisePreviewKey(search, host.Window, keyValue);

                Assert.False(key.Handled);
                Assert.Equal(Visibility.Visible, overlay.Visibility);
                Assert.Same(previousGame, picker.SelectedGame);
            }
        });
    }

    [Fact]
    public void ActiveTextCompositionKeepsPickerOpenUntilCompositionCommits()
    {
        RunSta(() =>
        {
            using var picker = CreatePicker("旧游戏", "中文游戏");
            var previousGame = picker.SelectedGame;
            var candidate = picker.Items.Single(item => item.Name == "中文游戏");
            using var host = CreatePickerHost(picker, out var shell, out var overlay, out var search, out var list);
            list.SelectedItem = candidate;
            overlay.Visibility = Visibility.Visible;
            AttachPickerForInput(shell, picker);
            Keyboard.Focus(search);

            RaiseTextComposition(search, host.Window, TextCompositionManager.PreviewTextInputStartEvent, "zhong");
            RaiseTextComposition(search, host.Window, TextCompositionManager.PreviewTextInputUpdateEvent, "zhongg");
            var compositionEnter = RaisePreviewKey(search, host.Window, Key.Enter);

            Assert.False(compositionEnter.Handled);
            Assert.Equal(Visibility.Visible, overlay.Visibility);
            Assert.Same(previousGame, picker.SelectedGame);

            RaiseTextComposition(search, host.Window, TextCompositionManager.PreviewTextInputEvent, "中");
            var committedEnter = RaisePreviewKey(search, host.Window, Key.Enter);

            Assert.True(committedEnter.Handled);
            Assert.Equal(Visibility.Collapsed, overlay.Visibility);
            Assert.Same(candidate.Game, picker.SelectedGame);
        });
    }

    [Fact]
    public void EnterConfirmsTheVisibleListCandidateAndReturnsFocusToTheContextButton()
    {
        RunSta(() =>
        {
            using var picker = CreatePicker("旧游戏", "目标游戏");
            picker.SearchText = "目标游戏";
            picker.RefreshNow();
            var candidate = picker.Items.Single(item => item.Name == "目标游戏");

            using var host = CreatePickerHost(picker, out var shell, out var overlay, out var search, out var list);
            // Set the ListBox selection before injecting the lightweight dashboard shell
            // state so the SelectionChanged commit path does not close the picker early.
            list.SelectedItem = candidate;
            AttachPickerForInput(shell, picker);
            overlay.Visibility = Visibility.Visible;
            Keyboard.Focus(search);

            var key = RaisePreviewKey(search, host.Window, Key.Enter);
            var contextButton = (Button)shell.FindName("GameContextButton")!;

            Assert.True(key.Handled);
            Assert.Equal(Visibility.Collapsed, overlay.Visibility);
            Assert.Same(candidate.Game, picker.SelectedGame);
            Assert.Same(contextButton, Keyboard.FocusedElement);
        });
    }

    [Fact]
    public void ProductionSearchClearButtonClearsTextAndReturnsFocusToTheSearchBox()
    {
        RunSta(() =>
        {
            using var picker = CreatePicker("旧游戏", "目标游戏");
            using var host = CreatePickerHost(picker, out var shell, out var overlay, out var search, out _, showActivated: true);
            var clearButton = (Button)shell.FindName("GameSearchClearButton")!;

            search.Text = "目标";
            overlay.Visibility = Visibility.Visible;
            host.Window.Activate();
            Keyboard.Focus(search);

            var click = new RoutedEventArgs(Button.ClickEvent);
            clearButton.RaiseEvent(click);

            Assert.True(click.Handled);
            Assert.Equal(string.Empty, search.Text);
            Assert.Same(search, Keyboard.FocusedElement);
        });
    }

    [Fact]
    public void EscapeClosesTheProductionPickerAndReturnsFocusToTheContextButton()
    {
        RunSta(() =>
        {
            using var picker = CreatePicker("旧游戏", "目标游戏");
            using var host = CreatePickerHost(picker, out var shell, out var overlay, out var search, out _, showActivated: true);
            AttachPickerForInput(shell, picker);
            overlay.Visibility = Visibility.Visible;
            host.Window.Activate();
            Keyboard.Focus(search);

            var key = RaisePreviewKey(search, host.Window, Key.Escape);
            var contextButton = (Button)shell.FindName("GameContextButton")!;

            Assert.True(key.Handled);
            Assert.Equal(Visibility.Collapsed, overlay.Visibility);
            Assert.Same(contextButton, Keyboard.FocusedElement);
        });
    }

    private static GamePickerViewModel CreatePicker(params string[] names)
    {
        var picker = new GamePickerViewModel();
        picker.StatusFilter = "全部";
        picker.SetItems(names.Select(Game).ToArray(), names[0]);
        return picker;
    }

    private static GamePickerViewModel CreatePicker(params GameStatusDto[] games)
    {
        var picker = new GamePickerViewModel();
        picker.StatusFilter = "全部";
        picker.SetItems(games, games[0].PlayniteId);
        return picker;
    }

    private static GameStatusDto Game(string playniteId, string name)
        => new GameStatusDto
        {
            PlayniteId = playniteId,
            Name = name,
            Platform = GamePlatformKind.Other,
            IsInstalled = true,
            LudusaviMatched = true
        };

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
        bool showActivated = false)
    {
        shell = new AcrylicProductionShellView();
        var window = new Window
        {
            Content = shell,
            Width = 900,
            Height = 640,
            ShowInTaskbar = false,
            ShowActivated = showActivated,
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

    private static DashboardViewModel AttachPickerForInput(AcrylicProductionShellView shell, GamePickerViewModel picker)
    {
        var dashboard = (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
        var pickerField = typeof(DashboardViewModel).GetField("gamePicker", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(pickerField);
        pickerField!.SetValue(dashboard, picker);

        var viewModelField = typeof(AcrylicProductionShellView).GetField("viewModel", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(viewModelField);
        viewModelField!.SetValue(shell, dashboard);
        return dashboard;
    }

    private static IEnumerable<DependencyObject> GetVisualDescendants(DependencyObject root)
    {
        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var index = 0; index < count; index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            yield return child;
            foreach (var descendant in GetVisualDescendants(child))
                yield return descendant;
        }
    }

    private static KeyEventArgs RaisePreviewKey(FrameworkElement source, Window host, Key keyValue)
    {
        var key = new KeyEventArgs(
            Keyboard.PrimaryDevice,
            PresentationSource.FromVisual(host)!,
            0,
            keyValue)
        {
            RoutedEvent = Keyboard.PreviewKeyDownEvent
        };
        source.RaiseEvent(key);
        return key;
    }

    private static void RaiseTextComposition(
        FrameworkElement source,
        Window host,
        RoutedEvent routedEvent,
        string text)
    {
        var composition = new TextComposition(InputManager.Current, source, text);
        var args = new TextCompositionEventArgs(Keyboard.PrimaryDevice, composition)
        {
            RoutedEvent = routedEvent
        };
        source.RaiseEvent(args);
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
