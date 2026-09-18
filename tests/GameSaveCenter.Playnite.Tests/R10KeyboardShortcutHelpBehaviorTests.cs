using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Linq;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R10KeyboardShortcutHelpBehaviorTests
{
    [Fact]
    public void CatalogListsOnlyCommandsThatCanExecute()
    {
        var enabled = KeyboardShortcutHelpCatalog.Create(WorkspaceKind.Media, new TestCommand(true));
        var disabled = KeyboardShortcutHelpCatalog.Create(WorkspaceKind.Media, new TestCommand(false));

        var item = Assert.Single(enabled);
        Assert.Equal("Ctrl+F", item.Gesture);
        Assert.Contains("媒体中心", item.Description, StringComparison.Ordinal);
        Assert.Empty(disabled);
    }

    [Fact]
    public void CatalogKeepsCurrentWorkspaceInHelpDescription()
    {
        var item = Assert.Single(KeyboardShortcutHelpCatalog.Create(WorkspaceKind.Maintenance, new TestCommand(true)));

        Assert.Contains("维护中心", item.Description, StringComparison.Ordinal);
        Assert.Contains("弹层打开时由弹层继续处理输入", item.Description, StringComparison.Ordinal);
    }

    [Fact]
    public void ClickingHelpButtonOpensPopupWithCurrentPageEntry()
    {
        RunSta(() =>
        {
            var shell = new AcrylicProductionShellView();
            var dashboard = (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
            var workspaceField = typeof(DashboardViewModel).GetField("currentWorkspace", BindingFlags.Instance | BindingFlags.NonPublic);
            var viewModelField = typeof(AcrylicProductionShellView).GetField("viewModel", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(workspaceField);
            Assert.NotNull(viewModelField);
            workspaceField!.SetValue(dashboard, WorkspaceKind.Media);
            viewModelField!.SetValue(shell, dashboard);
            shell.FocusWorkspaceSearchRequested = () => { };

            using var host = new WindowHost(shell);
            var button = (Button)shell.FindName("HeaderKeyboardHelpButton")!;
            var popup = (System.Windows.Controls.Primitives.Popup)shell.FindName("KeyboardShortcutHelpPopup")!;
            var items = (ItemsControl)shell.FindName("KeyboardShortcutHelpItemsControl")!;

            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            host.Window.UpdateLayout();

            Assert.True(popup.IsOpen);
            var item = Assert.Single(items.Items.Cast<KeyboardShortcutHelpItem>());
            Assert.Equal("Ctrl+F", item.Gesture);
            Assert.Contains("媒体中心", item.Description, StringComparison.Ordinal);

            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.False(popup.IsOpen);
        });
    }

    [Fact]
    public void ProductionHelpUsesShellEntryAndActualSearchCommand()
    {
        var root = TestRepositoryContext.Root;
        var shellCode = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml.cs"));
        var shellXaml = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml"));
        var catalog = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "KeyboardShortcutHelpCatalog.cs"));

        Assert.Contains("FocusWorkspaceSearchCommand = new RelayCommand", shellCode, StringComparison.Ordinal);
        Assert.Contains("KeyboardShortcutHelpCatalog.Create", shellCode, StringComparison.Ordinal);
        Assert.Contains("Command.CanExecute(null)", catalog, StringComparison.Ordinal);
        Assert.Contains("HeaderKeyboardHelpButton", shellXaml, StringComparison.Ordinal);
        Assert.Contains("KeyboardShortcutHelpPopup", shellXaml, StringComparison.Ordinal);
        Assert.Contains("仅显示当前页面当前可用的操作", shellXaml, StringComparison.Ordinal);
    }

    private sealed class TestCommand : ICommand
    {
        private readonly bool canExecute;

        public TestCommand(bool canExecute) => this.canExecute = canExecute;

        public bool CanExecute(object? parameter) => canExecute;
        public void Execute(object? parameter) => throw new NotSupportedException();
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
    }

    private sealed class WindowHost : IDisposable
    {
        public WindowHost(FrameworkElement content)
        {
            Window = new Window
            {
                Content = content,
                Width = 900,
                Height = 640,
                ShowInTaskbar = false,
                ShowActivated = false,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };
            Window.Show();
            Window.UpdateLayout();
        }

        public Window Window { get; }

        public void Dispose()
        {
            Window.Close();
            Dispatcher.CurrentDispatcher.InvokeShutdown();
        }
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
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
