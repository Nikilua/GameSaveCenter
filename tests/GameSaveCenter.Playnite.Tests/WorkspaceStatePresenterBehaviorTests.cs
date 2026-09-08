using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GameSaveCenter.Playnite.Controls;
using GameSaveCenter.Playnite.Views;
using Xunit;
using WpfButton = System.Windows.Controls.Button;

namespace GameSaveCenter.Playnite.Tests;

public sealed class WorkspaceStatePresenterBehaviorTests
{
    [Theory]
    [InlineData("Error")]
    [InlineData("Offline")]
    public void FailureRetryButtonIsHitTestableAndBoundToCommand(string state)
    {
        Exception? exception = null;
        var hitButton = false;
        var retryVisible = false;
        var retryCommandPresent = false;

        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var resourceHost = new MediaCenterView();
                var retryCommand = new CountingCommand(() => { });
                var presenter = CreatePresenter(resourceHost, state, retryCommand);
                var root = new Grid();
                root.Children.Add(new WpfButton { Content = "底层操作" });
                root.Children.Add(presenter);
                window = CreateWindow(root);
                window.Show();
                window.UpdateLayout();
                presenter.ApplyTemplate();
                var retryButton = (WpfButton)presenter.Template!.FindName("RetryButton", presenter)!;
                retryCommandPresent = retryButton.Command != null;
                retryVisible = retryButton.Visibility == Visibility.Visible;
                var point = retryButton.TransformToAncestor(window).Transform(
                    new Point(retryButton.ActualWidth / 2, retryButton.ActualHeight / 2));
                var hit = VisualTreeHelper.HitTest(window, point)?.VisualHit;
                hitButton = ReferenceEquals(FindAncestor<WpfButton>(hit), retryButton);
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
        Assert.True(retryVisible, $"state={state}, retryVisible={retryVisible}, hitButton={hitButton}, command={retryCommandPresent}");
        Assert.True(hitButton, $"state={state}, retryVisible={retryVisible}, hitButton={hitButton}, command={retryCommandPresent}");
        Assert.True(retryCommandPresent);
    }

    [Fact]
    public void LoadingPresenterBlocksUnderlyingInputAndHidesRetry()
    {
        Exception? exception = null;
        var retryCollapsed = false;
        var hitUnderlyingButton = false;

        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var resourceHost = new MediaCenterView();
                var presenter = CreatePresenter(resourceHost, "Loading", new CountingCommand(() => { }));
                var underlying = new WpfButton { Content = "底层操作" };
                var root = new Grid();
                root.Children.Add(underlying);
                root.Children.Add(presenter);
                window = CreateWindow(root);
                window.Show();
                window.UpdateLayout();
                presenter.ApplyTemplate();
                var retryButton = (WpfButton)presenter.Template!.FindName("RetryButton", presenter)!;
                retryCollapsed = retryButton.Visibility == Visibility.Collapsed;
                var point = presenter.TransformToAncestor(window).Transform(
                    new Point(presenter.ActualWidth / 2, presenter.ActualHeight / 2));
                hitUnderlyingButton = ReferenceEquals(FindAncestor<WpfButton>(VisualTreeHelper.HitTest(window, point)?.VisualHit), underlying);
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
        Assert.True(retryCollapsed);
        Assert.False(hitUnderlyingButton);
    }

    [Theory]
    [InlineData(Key.Enter)]
    [InlineData(Key.Space)]
    public void FailureRetryButtonKeyboardActivationExecutesOnce(Key key)
    {
        Exception? exception = null;
        var executeCount = 0;

        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var resourceHost = new MediaCenterView();
                var presenter = CreatePresenter(resourceHost, "Error", new CountingCommand(() => executeCount++));
                window = CreateWindow(presenter);
                window.Show();
                window.UpdateLayout();
                presenter.ApplyTemplate();
                var retryButton = (WpfButton)presenter.Template!.FindName("RetryButton", presenter)!;
                retryButton.Focus();
                var source = PresentationSource.FromVisual(retryButton)
                    ?? throw new InvalidOperationException("重试按钮尚未连接到 WPF PresentationSource。");
                retryButton.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key)
                {
                    RoutedEvent = Keyboard.KeyDownEvent
                });
                retryButton.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key)
                {
                    RoutedEvent = Keyboard.KeyUpEvent
                });
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
        Assert.Equal(1, executeCount);
    }

    private static WorkspaceStatePresenter CreatePresenter(MediaCenterView resourceHost, string state, ICommand command)
    {
        var presenter = new WorkspaceStatePresenter
        {
            State = state,
            Title = "状态标题",
            Message = "状态说明",
            RetryText = "重试",
            RetryCommand = command,
            Style = (Style)resourceHost.Resources["GscWorkspaceStatePresenter"]
        };
        presenter.Width = 360;
        presenter.Height = 220;
        return presenter;
    }

    private static Window CreateWindow(UIElement content)
        => new()
        {
            Content = content,
            Width = 420,
            Height = 280,
            ShowInTaskbar = false,
            ShowActivated = false,
            WindowStyle = WindowStyle.None,
            Opacity = 0.01
        };

    private static T? FindAncestor<T>(DependencyObject? child) where T : DependencyObject
    {
        var current = child;
        while (current != null)
        {
            if (current is T match)
                return match;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    private static void RunSta(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (exception != null)
            throw new Xunit.Sdk.XunitException(exception.ToString());
    }

    private sealed class CountingCommand : ICommand
    {
        private readonly Action execute;

        public CountingCommand(Action execute) => this.execute = execute;

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => execute();
    }
}
