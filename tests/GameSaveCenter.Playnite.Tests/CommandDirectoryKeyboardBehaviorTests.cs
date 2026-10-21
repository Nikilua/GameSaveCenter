using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Views;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class CommandDirectoryKeyboardBehaviorTests
{
    [Fact]
    public void ArrowKeysSelectResultsAndEnterExecutesOnlyTheSelectedCommandOnce()
    {
        RunSta(() =>
        {
            var firstCommand = new RecordingCommand();
            var secondCommand = new RecordingCommand();
            using var host = CreateHost(
                Item("workspace.first", "第一个", firstCommand),
                Item("workspace.second", "第二个", secondCommand));
            host.OpenSearch();

            Assert.Contains("向上或向下选择命令", AutomationProperties.GetHelpText(host.Search));
            Assert.Equal(0, host.Results.SelectedIndex);
            var up = RaisePreviewKey(host.Search, host.Window, Key.Up);
            Assert.True(up.Handled);
            Assert.Equal(0, host.Results.SelectedIndex);
            var down = RaisePreviewKey(host.Search, host.Window, Key.Down);
            Assert.True(down.Handled);
            Assert.Equal(1, host.Results.SelectedIndex);
            var downAtEnd = RaisePreviewKey(host.Search, host.Window, Key.Down);
            Assert.True(downAtEnd.Handled);
            Assert.Equal(1, host.Results.SelectedIndex);
            Assert.Same(host.Search, Keyboard.FocusedElement);

            var enter = RaisePreviewKey(host.Search, host.Window, Key.Enter);
            PumpDispatcher(host.Window.Dispatcher);

            Assert.True(enter.Handled);
            Assert.Equal(0, firstCommand.ExecutionCount);
            Assert.Equal(1, secondCommand.ExecutionCount);
            Assert.Equal(Visibility.Collapsed, host.Overlay.Visibility);
            Assert.Same(host.Trigger, Keyboard.FocusedElement);
        });
    }

    [Fact]
    public void ImeCompositionKeepsEnterAndEscapeOutOfCommandRoutingUntilCommit()
    {
        RunSta(() =>
        {
            var command = new RecordingCommand();
            using var host = CreateHost(Item("workspace.tasks", "查看任务", command));
            host.OpenSearch();

            RaiseTextComposition(host.Search, host.Window, TextCompositionManager.PreviewTextInputStartEvent, "zhong");
            RaiseTextComposition(host.Search, host.Window, TextCompositionManager.PreviewTextInputUpdateEvent, "zhongg");

            var composingEnter = RaisePreviewKey(host.Search, host.Window, Key.Enter);
            var composingEscape = RaisePreviewKey(host.Search, host.Window, Key.Escape);

            Assert.False(composingEnter.Handled);
            Assert.False(composingEscape.Handled);
            Assert.Equal(0, command.ExecutionCount);
            Assert.Equal(Visibility.Visible, host.Overlay.Visibility);

            RaiseTextComposition(host.Search, host.Window, TextCompositionManager.PreviewTextInputEvent, "中");
            var committedEnter = RaisePreviewKey(host.Search, host.Window, Key.Enter);
            PumpDispatcher(host.Window.Dispatcher);

            Assert.True(committedEnter.Handled);
            Assert.Equal(1, command.ExecutionCount);
            Assert.Equal(Visibility.Collapsed, host.Overlay.Visibility);
        });
    }

    [Fact]
    public void EmptyResultsConsumeEnterWithoutExecutingAndEscapeReturnsFocus()
    {
        RunSta(() =>
        {
            var command = new RecordingCommand();
            using var host = CreateHost(Item("workspace.tasks", "查看任务", command));
            host.OpenSearch();
            host.Search.Text = "no match";

            var enter = RaisePreviewKey(host.Search, host.Window, Key.Enter);

            Assert.True(enter.Handled);
            Assert.Equal(0, command.ExecutionCount);
            Assert.Equal(Visibility.Visible, host.Overlay.Visibility);

            var escape = RaisePreviewKey(host.Search, host.Window, Key.Escape);
            PumpDispatcher(host.Window.Dispatcher);

            Assert.True(escape.Handled);
            Assert.Equal(Visibility.Collapsed, host.Overlay.Visibility);
            Assert.Same(host.Trigger, Keyboard.FocusedElement);
        });
    }

    [Fact]
    public void DisabledResultsStayOpenAndEditingKeysAreNotCaptured()
    {
        RunSta(() =>
        {
            var command = new RecordingCommand();
            using var host = CreateHost(Item("workspace.disabled", "暂不可用", command, canExecute: false));
            host.OpenSearch();

            var characterInput = RaisePreviewKey(host.Search, host.Window, Key.A);
            var left = RaisePreviewKey(host.Search, host.Window, Key.Left);
            var right = RaisePreviewKey(host.Search, host.Window, Key.Right);
            var enter = RaisePreviewKey(host.Search, host.Window, Key.Enter);

            Assert.False(characterInput.Handled);
            Assert.False(left.Handled);
            Assert.False(right.Handled);
            Assert.True(enter.Handled);
            Assert.Equal(0, command.ExecutionCount);
            Assert.Equal(Visibility.Visible, host.Overlay.Visibility);

            host.Overlay.Visibility = Visibility.Collapsed;
            var outsideEnter = RaisePreviewKey(host.Search, host.Window, Key.Enter);
            var outsideEscape = RaisePreviewKey(host.Search, host.Window, Key.Escape);
            Assert.False(outsideEnter.Handled);
            Assert.False(outsideEscape.Handled);
        });
    }

    [Fact]
    public void OpenShortcutIsBoundToThePluginShellAndDoesNotCaptureTextEditing()
    {
        RunSta(() =>
        {
            using var host = CreateHost(Item("workspace.tasks", "查看任务", new RecordingCommand()));
            var shortcut = Assert.IsType<KeyBinding>(Assert.Single(host.Shell.InputBindings));
            Assert.Equal(Key.P, shortcut.Key);
            Assert.Equal(ModifierKeys.Control | ModifierKeys.Shift, shortcut.Modifiers);
            Assert.Contains("Ctrl+Shift+P", AutomationProperties.GetHelpText(host.Trigger));

            var command = Assert.IsType<RoutedUICommand>(shortcut.Command);
            Assert.True(command.CanExecute(null, host.Shell));
            command.Execute(null, host.Shell);
            PumpDispatcher(host.Window.Dispatcher);
            Assert.Equal(Visibility.Visible, host.Overlay.Visibility);
            Assert.Same(host.Search, Keyboard.FocusedElement);

            host.Overlay.Visibility = Visibility.Collapsed;
            var shellRoot = Assert.IsType<Grid>(Assert.IsType<Border>(host.Shell.Content).Child);
            var editor = new TextBox { Width = 160, Height = 28 };
            shellRoot.Children.Add(editor);
            host.Window.Activate();
            host.Window.UpdateLayout();
            Assert.Same(editor, Keyboard.Focus(editor));
            Assert.False(command.CanExecute(null, host.Shell));
        });
    }

    [Fact]
    public void PinnedCommandsSortBeforeRecentCommandsAndCatalogItems()
    {
        RunSta(() =>
        {
            using var host = CreateHost(
                Item("workspace.first", "第一个", new RecordingCommand()),
                Item("workspace.second", "第二个", new RecordingCommand()),
                Item("workspace.third", "第三个", new RecordingCommand()));
            SetField(host.Shell, "pinnedCommandIds", new List<string> { "workspace.third" });
            SetField(host.Shell, "recentCommandIds", new List<string> { "workspace.second" });

            var updateResults = typeof(AcrylicProductionShellView).GetMethod(
                "UpdateCommandDirectorySearchResults",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(updateResults);
            updateResults!.Invoke(host.Shell, null);
            host.Window.UpdateLayout();

            var ordered = Assert.IsAssignableFrom<IEnumerable<CommandDirectoryItem>>(host.Results.ItemsSource);
            Assert.Equal(
                new[] { "workspace.third", "workspace.second", "workspace.first" },
                ordered.Select(item => item.Id));
        });
    }

    private static CommandDirectoryItem Item(string id, string name, RecordingCommand command, bool canExecute = true)
        => new CommandDirectoryItem(
            id,
            name,
            Array.Empty<string>(),
            "页面导航",
            CommandDirectoryScope.Workspace,
            command,
            additionalCanExecute: () => canExecute,
            unavailableReason: () => "当前命令暂不可用。");

    private static TestHost CreateHost(params CommandDirectoryItem[] items)
    {
        var shell = new AcrylicProductionShellView();
        SetField(shell, "viewModel", FormatterServices.GetUninitializedObject(typeof(DashboardViewModel)));
        SetField(shell, "commandDirectoryItems", items);
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

        var host = new TestHost(
            window,
            (Grid)shell.FindName("CommandDirectoryOverlay")!,
            (TextBox)shell.FindName("CommandDirectorySearchTextBox")!,
            (ListBox)shell.FindName("CommandDirectoryResultsListBox")!,
            (Button)shell.FindName("HeaderCommandDirectoryButton")!);
        var updateResults = typeof(AcrylicProductionShellView).GetMethod(
            "UpdateCommandDirectorySearchResults",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(updateResults);
        updateResults!.Invoke(shell, null);
        window.UpdateLayout();
        return host;
    }

    private static void SetField(object target, string name, object value)
    {
        var field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        field!.SetValue(target, value);
    }

    private static KeyEventArgs RaisePreviewKey(FrameworkElement source, Window host, Key key)
    {
        var args = new KeyEventArgs(
            Keyboard.PrimaryDevice,
            PresentationSource.FromVisual(host)!,
            0,
            key)
        {
            RoutedEvent = Keyboard.PreviewKeyDownEvent
        };
        source.RaiseEvent(args);
        return args;
    }

    private static void RaiseTextComposition(FrameworkElement source, Window host, RoutedEvent routedEvent, string text)
    {
        var composition = new TextComposition(InputManager.Current, source, text);
        var args = new TextCompositionEventArgs(Keyboard.PrimaryDevice, composition)
        {
            RoutedEvent = routedEvent
        };
        source.RaiseEvent(args);
    }

    private static void PumpDispatcher(Dispatcher dispatcher)
        => dispatcher.Invoke(DispatcherPriority.Input, new Action(() => { }));

    private static void RunSta(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception error) { exception = error; }
            finally { Dispatcher.CurrentDispatcher.InvokeShutdown(); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(20)), "WPF test thread did not finish within 20 seconds.");
        if (exception != null)
            throw new InvalidOperationException("The command directory WPF test failed.", exception);
    }

    private sealed class TestHost : IDisposable
    {
        public TestHost(Window window, Grid overlay, TextBox search, ListBox results, Button trigger)
        {
            Window = window;
            Shell = (AcrylicProductionShellView)window.Content;
            Overlay = overlay;
            Search = search;
            Results = results;
            Trigger = trigger;
        }

        public Window Window { get; }
        public AcrylicProductionShellView Shell { get; }
        public Grid Overlay { get; }
        public TextBox Search { get; }
        public ListBox Results { get; }
        public Button Trigger { get; }

        public void OpenSearch()
        {
            Overlay.Visibility = Visibility.Visible;
            Window.Activate();
            Keyboard.Focus(Search);
        }

        public void Dispose()
        {
            try { Window.Close(); }
            catch { }
        }
    }

    private sealed class RecordingCommand : ICommand
    {
        public int ExecutionCount { get; private set; }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => ExecutionCount++;
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
