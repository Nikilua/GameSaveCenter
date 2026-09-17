using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class SettingsDraftLifecycleBehaviorTests
{
    [Fact]
    public void DirtyDraftSurvivesDetachedViewAndRestoresTheFocusedField()
    {
        RunSta(() =>
        {
            var application = new Application();
            application.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
            var root = Path.Combine(Path.GetTempPath(), "gsc-settings-draft-ui-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                var worker = Path.Combine(root, "GameSaveCenter.Worker.exe");
                File.WriteAllText(worker, "worker");
                var saves = Path.Combine(root, "Saves");
                var media = Path.Combine(root, "Media");
                Directory.CreateDirectory(saves);
                Directory.CreateDirectory(media);
                var settings = new GameSaveCenterSettings
                {
                    WorkerExecutable = worker,
                    LudusaviBackupDirectory = saves,
                    MediaArchiveDirectory = media,
                    HealthInspectionEnabled = false
                };
                settings.BeginEdit();

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

                var tabs = (ListBox)view.FindName("SettingsSectionTabs")!;
                var field = (TextBox)view.FindName("DefaultBackupIntervalMinutesTextBox")!;
                tabs.SelectedIndex = 3;
                window.UpdateLayout();
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));

                Assert.True(field.Focus());
                Keyboard.Focus(field);
                settings.DefaultBackupIntervalMinutes = 60;
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
                window.UpdateLayout();

                Assert.True(settings.HasPendingEdit);
                Assert.Equal(60, settings.DefaultBackupIntervalMinutes);

                // Simulate Playnite temporarily detaching and reusing its settings page while
                // retaining the same ISettings edit buffer.
                window.Content = null;
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
                Assert.True(settings.HasPendingEdit);
                Assert.Equal(60, settings.DefaultBackupIntervalMinutes);

                window.Content = view;
                window.UpdateLayout();
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
                window.UpdateLayout();

                Assert.Equal(3, tabs.SelectedIndex);
                Assert.Equal(60, settings.DefaultBackupIntervalMinutes);
                Assert.True(field.IsKeyboardFocusWithin);

                settings.CancelEdit();
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
                Assert.Equal(30, settings.DefaultBackupIntervalMinutes);
                Assert.False(settings.HasPendingEdit);
                window.Close();
            }
            finally
            {
                try { Directory.Delete(root, true); } catch { }
            }
        });
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
