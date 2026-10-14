using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R10SearchShortcutFocusBehaviorTests
{
    [Fact]
    public void CtrlFFocusesTheSearchTargetAndDoesNotConsumeOverlayOrEditingInput()
    {
        RunSta(() =>
        {
            var shell = new AcrylicProductionShellView();
            var mediaView = new MediaCenterView();
            var searchTarget = Assert.IsType<TextBox>(mediaView.FindName("MediaSearchTextBox"));
            var mediaTabs = Assert.IsType<TabControl>(mediaView.FindName("MediaTabControl"));
            mediaTabs.SelectedIndex = 1;
            var origin = new Button { Content = "Keyboard origin", Width = 180, Height = 34 };
            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetRow(mediaView, 0);
            Grid.SetRow(origin, 1);
            root.Children.Add(mediaView);
            root.Children.Add(origin);
            var window = new Window
            {
                Content = root,
                Width = 480,
                Height = 180,
                ShowInTaskbar = false,
                ShowActivated = true,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };

            try
            {
                window.Show();
                window.UpdateLayout();
                Assert.True(origin.Focus());
                Assert.Same(origin, Keyboard.Focus(origin));

                var focusRequests = 0;
                shell.FocusWorkspaceSearchRequested = () =>
                {
                    focusRequests++;
                    Assert.True(searchTarget.IsVisible);
                    DashboardView.FocusWorkspaceSearchTarget(
                        WorkspaceKind.Media,
                        workspace => workspace == WorkspaceKind.Media ? mediaView : null,
                        new TextBox());
                };
                Assert.True(shell.FocusWorkspaceSearchCommand.CanExecute(null));
                KeyEventHandler successfulRoute = (_, e) => DashboardView.TryHandleWorkspaceSearchShortcut(
                    e,
                    ModifierKeys.Control,
                    dialogVisible: false,
                    gamePickerOpen: false,
                    compactGameBrowserOpen: false,
                    focusWorkspaceSearchCommand: shell.FocusWorkspaceSearchCommand);
                origin.PreviewKeyDown += successfulRoute;

                var ctrlF = RaisePreviewKey(origin, window, Key.F);
                origin.PreviewKeyDown -= successfulRoute;

                Assert.True(ctrlF.Handled);
                Assert.Equal(1, focusRequests);
                Assert.Same(searchTarget, Keyboard.FocusedElement);
                Assert.True(searchTarget.IsKeyboardFocusWithin);

                var rejectedInputs = new[]
                {
                    (Key: Key.F, Modifiers: ModifierKeys.Control, Dialog: true, Picker: false, Compact: false),
                    (Key: Key.F, Modifiers: ModifierKeys.Control, Dialog: false, Picker: true, Compact: false),
                    (Key: Key.F, Modifiers: ModifierKeys.Control, Dialog: false, Picker: false, Compact: true),
                    (Key: Key.Z, Modifiers: ModifierKeys.Control, Dialog: false, Picker: false, Compact: false),
                    (Key: Key.C, Modifiers: ModifierKeys.Control, Dialog: false, Picker: false, Compact: false),
                    (Key: Key.F, Modifiers: ModifierKeys.None, Dialog: false, Picker: false, Compact: false)
                };

                foreach (var input in rejectedInputs)
                {
                    Assert.True(origin.Focus());
                    Assert.Same(origin, Keyboard.Focus(origin));
                    var requestsBeforeInput = focusRequests;
                    KeyEventArgs? observed = null;
                    KeyEventHandler handler = (_, e) =>
                    {
                        observed = e;
                        DashboardView.TryHandleWorkspaceSearchShortcut(
                            e,
                            input.Modifiers,
                            input.Dialog,
                            input.Picker,
                            input.Compact,
                            shell.FocusWorkspaceSearchCommand);
                    };
                    origin.PreviewKeyDown += handler;

                    var key = RaisePreviewKey(origin, window, input.Key);

                    origin.PreviewKeyDown -= handler;
                    Assert.NotNull(observed);
                    Assert.False(key.Handled);
                    Assert.Equal(requestsBeforeInput, focusRequests);
                    Assert.Same(origin, Keyboard.FocusedElement);
                }
            }
            finally
            {
                if (window.IsVisible)
                    window.Close();
            }
        });
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
