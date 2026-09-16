using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class GamePickerKeyboardBehaviorTests
{
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
            ShowActivated = false,
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
