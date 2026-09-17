using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class SettingsValidationNavigationBehaviorTests
{
    [Fact]
    public void ErrorLinkSelectsAutomationTabAndFocusesTheFixableField()
    {
        RunSta(() =>
        {
            var application = new Application();
            application.Resources["BaseTextBlockStyle"] = new Style(typeof(TextBlock));
            var root = Path.Combine(Path.GetTempPath(), "gsc-settings-validation-ui-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                var worker = Path.Combine(root, "GameSaveCenter.Worker.exe");
                File.WriteAllText(worker, "worker");
                var settings = new GameSaveCenterSettings
                {
                    WorkerExecutable = worker,
                    LudusaviBackupDirectory = Path.Combine(root, "Saves"),
                    MediaArchiveDirectory = Path.Combine(root, "Media"),
                    HealthInspectionEnabled = true,
                    HealthInspectionIntervalMinutes = 1
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

                var scroller = (ScrollViewer)view.FindName("SettingsScroller")!;
                scroller.Height = 180;
                window.UpdateLayout();
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));

                var details = (TextBlock)view.FindName("SettingsValidationDetailsText")!;
                var link = details.Inlines.OfType<Hyperlink>()
                    .Single(candidate => candidate.Inlines.OfType<Run>()
                        .Any(run => run.Text.IndexOf("恢复可用性巡检间隔", StringComparison.Ordinal) >= 0));
                var tabs = (ListBox)view.FindName("SettingsSectionTabs")!;
                var field = (TextBox)view.FindName("HealthInspectionIntervalMinutesTextBox")!;

                link.RaiseEvent(new RoutedEventArgs(Hyperlink.ClickEvent));
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
                window.UpdateLayout();

                Assert.Equal(3, tabs.SelectedIndex);
                Assert.Contains("定位错误：恢复可用性巡检间隔", AutomationProperties.GetName(link));
                Assert.True(field.IsVisible);
                Assert.True(field.IsKeyboardFocusWithin);
                Assert.Contains("恢复可用性巡检间隔", AutomationProperties.GetHelpText(field));
                Assert.True(scroller.VerticalOffset > 0, $"Expected the automation field to scroll into view, offset was {scroller.VerticalOffset}.");
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
