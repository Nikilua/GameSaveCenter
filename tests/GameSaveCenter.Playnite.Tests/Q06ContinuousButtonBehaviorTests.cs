using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Controls;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;
using PlayniteButton = GameSaveCenter.Playnite.Controls.Button;
using WpfButton = System.Windows.Controls.Button;

namespace GameSaveCenter.Playnite.Tests;

public sealed class Q06ContinuousButtonBehaviorTests
{
    [Fact]
    public void RapidFrameworkClicksRespectBusyGateAndKeepSiblingActionEnabled()
    {
        Exception? exception = null;
        var submissions = 0;
        var busy = false;

        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var resourceHost = new MediaCenterView();
                RelayCommand? command = null;
                command = new RelayCommand(
                    _ =>
                    {
                        submissions++;
                        busy = true;
                        command!.RaiseCanExecuteChanged();
                    },
                    _ => !busy);

                var presenter = new WorkspaceStatePresenter
                {
                    State = "Error",
                    Title = "合成状态",
                    Message = "连续点击门禁样本",
                    RetryText = "重试",
                    RetryCommand = command,
                    Style = (Style)resourceHost.Resources["GscWorkspaceStatePresenter"],
                    Width = 360,
                    Height = 220
                };
                var siblingAction = new PlayniteButton
                {
                    Content = "其他安全操作",
                    Style = (Style)resourceHost.Resources["GscWpfUiActionButton"]
                };
                var root = new Grid();
                root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                root.Children.Add(presenter);
                Grid.SetRow(siblingAction, 1);
                root.Children.Add(siblingAction);

                window = CreateWindow(root);
                window.Show();
                window.UpdateLayout();
                presenter.ApplyTemplate();
                var retryButton = (WpfButton)presenter.Template!.FindName("RetryButton", presenter)!;
                PumpCommandState();

                Assert.True(retryButton.IsEnabled);
                Assert.True(siblingAction.IsEnabled);

                InvokeFrameworkClick(retryButton);
                PumpCommandState();
                Assert.Equal(1, submissions);
                Assert.False(retryButton.IsEnabled);
                Assert.False(command.CanExecute(null));
                Assert.True(siblingAction.IsEnabled);

                // Model a second click already queued at the framework dispatch boundary.
                // The command gate must still reject it after the button enters its busy state.
                InvokeFrameworkClick(retryButton);
                Assert.Equal(1, submissions);

                busy = false;
                command.RaiseCanExecuteChanged();
                PumpCommandState();
                Assert.True(retryButton.IsEnabled);
                InvokeFrameworkClick(retryButton);
                Assert.Equal(2, submissions);
                Assert.True(siblingAction.IsEnabled);
            }
            catch (Exception caught)
            {
                exception = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        Assert.Null(exception);
        Assert.Equal(2, submissions);
    }

    private static void InvokeFrameworkClick(WpfButton button)
        => typeof(ButtonBase).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(button, Array.Empty<object>());

    private static Window CreateWindow(UIElement content)
        => new()
        {
            Content = content,
            Width = 460,
            Height = 320,
            ShowInTaskbar = false,
            ShowActivated = false,
            WindowStyle = WindowStyle.None,
            Opacity = 0.01
        };

    private static void PumpCommandState()
    {
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Input, new Action(() => { }));
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
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
            catch (Exception caught)
            {
                failure = caught;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure != null)
            throw new Xunit.Sdk.XunitException(failure.ToString());
    }
}
