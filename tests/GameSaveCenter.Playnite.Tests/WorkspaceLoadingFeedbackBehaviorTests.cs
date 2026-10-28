using System;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Controls;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class WorkspaceLoadingFeedbackBehaviorTests
{
    [Fact]
    public void FastLoadingDoesNotFlashAndSlowLoadingKeepsItsMeasuredSlot()
    {
        RunSta(() =>
        {
            var presenter = new WorkspaceStatePresenter { State = "Loading", Title = "正在读取", Message = "请稍候" };
            presenter.Resources.MergedDictionaries.Add(new TaskCenterView().Resources);
            presenter.SetResourceReference(FrameworkElement.StyleProperty, "GscWorkspaceStatePresenter");
            var window = new Window { Content = presenter, Width = 600, Height = 320, ShowInTaskbar = false };
            AutomationProperties.SetName(window, "Loading feedback timing fixture");
            try
            {
                window.Show();
                window.UpdateLayout();
                presenter.State = "Empty";
                presenter.State = "Loading";
                var surface = (Border)presenter.Template.FindName("StateSurface", presenter);
                Assert.Equal("Loading", presenter.State);
                Assert.False(presenter.IsLoadingFeedbackReady);
                Assert.Equal(Visibility.Hidden, surface.Visibility);
                var height = presenter.ActualHeight;
                presenter.State = "Empty";
                Assert.Equal(Visibility.Visible, surface.Visibility);
                PumpFor(window, TimeSpan.FromMilliseconds(240));
                Assert.Equal("Empty", presenter.State);
                Assert.Equal(Visibility.Visible, surface.Visibility);

                presenter.State = "Loading";
                Assert.Equal(Visibility.Hidden, surface.Visibility);
                PumpFor(window, TimeSpan.FromMilliseconds(260));
                Assert.True(presenter.IsLoadingFeedbackReady);
                Assert.Equal(Visibility.Visible, surface.Visibility);
                Assert.Equal(height, presenter.ActualHeight, 2);

                presenter.State = "Loading";
                window.Content = null;
                PumpFor(window, TimeSpan.FromMilliseconds(240));
                Assert.False(presenter.IsLoaded);
                window.Content = presenter;
                window.UpdateLayout();
                Assert.False(presenter.IsLoadingFeedbackReady);
                presenter.State = "Error";
                Assert.Equal(Visibility.Visible, surface.Visibility);
                Assert.True(presenter.IsLoadingFeedbackReady);
            }
            finally { window.Close(); }
        });
    }

    private static void PumpFor(Window window, TimeSpan duration)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer(DispatcherPriority.Background, window.Dispatcher) { Interval = duration };
        timer.Tick += (_, __) => { timer.Stop(); frame.Continue = false; };
        timer.Start();
        Dispatcher.PushFrame(frame);
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() => { try { action(); } catch (Exception e) { failure = e; } });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(20)), "Loading fixture must finish within 20 seconds.");
        Assert.Null(failure);
    }
}
