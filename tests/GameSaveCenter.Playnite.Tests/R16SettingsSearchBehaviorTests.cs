using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R16SettingsSearchBehaviorTests
{
    [Fact]
    public void SearchShowsMatchingEditableFieldAndClearingRestoresOriginalCategory()
    {
        RunSta(() =>
        {
            var application = new Application();
            application.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
            var root = Path.Combine(Path.GetTempPath(), "gsc-settings-search-ui-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                var worker = Path.Combine(root, "GameSaveCenter.Worker.exe");
                var saves = Path.Combine(root, "Saves");
                var media = Path.Combine(root, "Media");
                File.WriteAllText(worker, "worker");
                Directory.CreateDirectory(saves);
                Directory.CreateDirectory(media);
                var settings = new GameSaveCenterSettings
                {
                    WorkerExecutable = worker,
                    LudusaviBackupDirectory = saves,
                    MediaArchiveDirectory = media,
                    HealthInspectionEnabled = true
                };
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
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));

                var tabs = (ListBox)view.FindName("SettingsSectionTabs")!;
                var search = (TextBox)view.FindName("SettingsSearchTextBox")!;
                var workerField = (TextBox)view.FindName("WorkerExecutableTextBox")!;
                var healthField = (TextBox)view.FindName("HealthInspectionIntervalMinutesTextBox")!;

                Assert.Equal(0, tabs.SelectedIndex);
                search.Text = "恢复巡检间隔";
                window.UpdateLayout();
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));

                Assert.Equal(3, tabs.SelectedIndex);
                Assert.True(healthField.IsVisible);
                Assert.True(healthField.IsEnabled);
                Assert.False(workerField.IsVisible);
                Assert.Contains("匹配设置", ((TextBlock)view.FindName("SettingsSearchSummary")!).Text);
                Assert.Equal(saves, settings.LudusaviBackupDirectory);

                search.Clear();
                window.UpdateLayout();
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));

                Assert.Equal(0, tabs.SelectedIndex);
                Assert.True(workerField.IsVisible);
                Assert.False(settings.HasPendingEdit);
                window.Close();
            }
            finally
            {
                try { Directory.Delete(root, true); } catch { }
                application.Shutdown();
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
