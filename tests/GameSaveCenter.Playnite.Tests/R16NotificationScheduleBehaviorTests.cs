using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Controls;
using GameSaveCenter.Playnite.Settings;
using Xunit;
using GscButton = GameSaveCenter.Playnite.Controls.Button;
using WpfButton = System.Windows.Controls.Button;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R16NotificationScheduleBehaviorTests
{
    [Fact]
    public void MutePresetsStoreUtcExpiryAndReleaseClearsIt()
    {
        RunSta(() =>
        {
            using var host = CreateHost(new GameSaveCenterSettings());
            var settings = (GameSaveCenterSettings)host.View.DataContext;
            var oneHourButton = (GscButton)host.View.FindName("NotificationMuteOneHourButton")!;
            var releaseButton = (GscButton)host.View.FindName("NotificationMuteReleaseButton")!;

            Assert.False(releaseButton.IsEnabled);
            var beforeClickUtc = DateTime.UtcNow;
            oneHourButton.RaiseEvent(new RoutedEventArgs(WpfButton.ClickEvent, oneHourButton));

            Assert.NotNull(settings.NotificationMutedUntilUtc);
            Assert.Equal(DateTimeKind.Utc, settings.NotificationMutedUntilUtc!.Value.Kind);
            Assert.InRange(settings.NotificationMutedUntilUtc.Value, beforeClickUtc.AddMinutes(59), DateTime.UtcNow.AddMinutes(61));
            Assert.True(releaseButton.IsEnabled);
            Assert.Contains("临时静音中", ((TextBlock)host.View.FindName("NotificationScheduleStatusTextBlock")!).Text);

            releaseButton.RaiseEvent(new RoutedEventArgs(WpfButton.ClickEvent, releaseButton));
            Assert.Null(settings.NotificationMutedUntilUtc);
            Assert.False(releaseButton.IsEnabled);

            var toggle = (ToggleSwitch)host.View.FindName("NotificationQuietHoursToggleSwitch")!;
            var fields = (UniformGrid)host.View.FindName("NotificationQuietHoursFields")!;
            var start = (TextBox)host.View.FindName("NotificationQuietHoursStartMinuteTextBox")!;
            var end = (TextBox)host.View.FindName("NotificationQuietHoursEndMinuteTextBox")!;

            Assert.False(fields.IsEnabled);
            toggle.IsChecked = true;
            host.Window.UpdateLayout();
            FlushDispatcher();

            Assert.True(settings.NotificationQuietHoursEnabled);
            Assert.True(fields.IsEnabled);
            start.Text = "1380";
            end.Text = "360";
            BindingOperations.GetBindingExpression(start, TextBox.TextProperty)!.UpdateSource();
            BindingOperations.GetBindingExpression(end, TextBox.TextProperty)!.UpdateSource();

            Assert.Equal(1380, settings.NotificationQuietHoursStartMinute);
            Assert.Equal(360, settings.NotificationQuietHoursEndMinute);
        });
    }

    private static SettingsHost CreateHost(GameSaveCenterSettings settings)
    {
        var application = new Application();
        application.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
        var view = new GameSaveCenterSettingsView { DataContext = settings };
        var window = new Window
        {
            Content = view,
            Width = 1040,
            Height = 720,
            ShowInTaskbar = false,
            ShowActivated = false,
            WindowStyle = WindowStyle.None,
            Opacity = 0.01
        };
        window.Show();
        window.UpdateLayout();
        FlushDispatcher();
        return new SettingsHost(application, window, view);
    }

    private static void FlushDispatcher()
        => Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));

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

    private sealed class SettingsHost : IDisposable
    {
        private readonly Application application;

        public SettingsHost(Application application, Window window, GameSaveCenterSettingsView view)
        {
            this.application = application;
            Window = window;
            View = view;
        }

        public Window Window { get; }
        public GameSaveCenterSettingsView View { get; }

        public void Dispose()
        {
            Window.Close();
            application.Shutdown();
        }
    }
}
