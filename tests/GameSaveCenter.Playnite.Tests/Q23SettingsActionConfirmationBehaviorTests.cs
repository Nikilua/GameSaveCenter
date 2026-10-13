using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("Q23SettingsActionsWpf")]
public sealed class Q23SettingsActionConfirmationBehaviorTests
{
    [Fact]
    public void ResetAndImportConfirmationPreserveCancelAndDeclineSemantics()
    {
        RunSta(() =>
        {
            EnsureApplicationResources();
            var root = Path.Combine(Path.GetTempPath(), "gsc-q23-reset-confirm-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                var worker = Path.Combine(root, "GameSaveCenter.Worker.exe");
                var saves = Path.Combine(root, "Saves");
                var media = Path.Combine(root, "Media");
                File.WriteAllText(worker, "synthetic worker placeholder; never executed");
                Directory.CreateDirectory(saves);
                Directory.CreateDirectory(media);

                var settings = new GameSaveCenterSettings
                {
                    WorkerExecutable = worker,
                    LudusaviBackupDirectory = saves,
                    MediaArchiveDirectory = media,
                    RcloneDestination = "synthetic:profile",
                    Compression = "none",
                    EnableCloudUpload = true,
                    DefaultBackupIntervalMinutes = 45
                };
                settings.BeginEdit();
                settings.Compression = "bzip2";
                settings.EnableCloudUpload = false;
                settings.DefaultBackupIntervalMinutes = 60;
                settings.RcloneDestination = "synthetic:edited";

                var view = new GameSaveCenterSettingsView { DataContext = settings };
                var window = new Window
                {
                    Content = view,
                    Width = 720,
                    Height = 640,
                    ShowInTaskbar = false,
                    ShowActivated = false,
                    WindowStyle = WindowStyle.None,
                    Opacity = 0.01
                };
                var promptResult = MessageBoxResult.No;
                var promptCalls = 0;
                string? promptMessage = null;
                Window? promptOwner = null;
                view.SettingsActionConfirmationPromptForTesting = (owner, message, title, buttons, image) =>
                {
                    promptCalls++;
                    promptOwner = owner;
                    promptMessage = message;
                    Assert.Equal("确认恢复全部默认", title);
                    Assert.Equal(MessageBoxButton.YesNo, buttons);
                    Assert.Equal(MessageBoxImage.Warning, image);
                    return promptResult;
                };

                window.Show();
                window.UpdateLayout();
                DrainDispatcher();
                // The settings page's first-open policy may enlarge a small owner window.
                // Resize it after load to exercise the actual compact interaction state.
                window.Width = 720;
                window.Height = 640;
                window.UpdateLayout();
                DrainDispatcher();
                var expander = (Expander)view.FindName("SettingsResetDefaultsExpander")!;
                Assert.False(expander.IsExpanded, $"short owner height={window.ActualHeight:0.##}, view height={view.ActualHeight:0.##}");
                expander.IsExpanded = true;
                window.UpdateLayout();
                DrainDispatcher();
                var resetButton = (Button)view.FindName("SettingsResetAllButton")!;
                Assert.True(resetButton.IsVisible);
                Assert.True(resetButton.IsEnabled);

                var beforeDecline = settings.CreateSettingsFingerprint();
                resetButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

                Assert.Equal(1, promptCalls);
                Assert.Same(window, promptOwner);
                Assert.Contains("不会清除", promptMessage!);
                Assert.Equal(beforeDecline, settings.CreateSettingsFingerprint());
                Assert.Equal("bzip2", settings.Compression);
                Assert.Equal("synthetic:edited", settings.RcloneDestination);

                promptResult = MessageBoxResult.Yes;
                resetButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.Equal(2, promptCalls);
                Assert.Equal("zstd", settings.Compression);
                Assert.False(settings.EnableCloudUpload);
                Assert.Equal(30, settings.DefaultBackupIntervalMinutes);
                Assert.Equal(worker, settings.WorkerExecutable);
                Assert.Equal(saves, settings.LudusaviBackupDirectory);
                Assert.Equal(media, settings.MediaArchiveDirectory);
                Assert.Equal("synthetic:edited", settings.RcloneDestination);
                Assert.True(settings.HasPendingEdit);

                settings.CancelEdit();
                Assert.Equal("none", settings.Compression);
                Assert.True(settings.EnableCloudUpload);
                Assert.Equal(45, settings.DefaultBackupIntervalMinutes);
                Assert.Equal("synthetic:profile", settings.RcloneDestination);
                Assert.False(settings.HasPendingEdit);

                window.Close();
            }
            finally
            {
                try { Directory.Delete(root, true); } catch { }
            }

            AssertDecliningImportOnCurrentSta();
        });
    }

    private static void AssertDecliningImportOnCurrentSta()
    {
            var original = new GameSaveCenterSettings
            {
                WorkerExecutable = @"C:\Synthetic\GameSaveCenter.Worker.exe",
                ThemeMode = GameSaveCenterThemeMode.Light
            };
            var incoming = new GameSaveCenterSettings
            {
                WorkerExecutable = @"D:\Synthetic\GameSaveCenter.Worker.exe",
                ThemeMode = GameSaveCenterThemeMode.Dark
            };
            var preview = original.PreviewPortableJson(incoming.ExportPortableJson());
            Assert.True(preview.IsCompatible);
            var before = original.CreateSettingsFingerprint();

            var view = new GameSaveCenterSettingsView { DataContext = original };
            var window = new Window
            {
                Content = view,
                Width = 720,
                Height = 640,
                ShowInTaskbar = false,
                ShowActivated = false,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };
            var promptResult = MessageBoxResult.No;
            var promptCalls = 0;
            string? promptMessage = null;
            view.SettingsActionConfirmationPromptForTesting = (owner, message, title, buttons, image) =>
            {
                promptCalls++;
                Assert.Same(window, owner);
                Assert.Equal("确认导入 GameSaveCenter 设置", title);
                Assert.Equal(MessageBoxButton.YesNo, buttons);
                Assert.Equal(MessageBoxImage.Warning, image);
                promptMessage = message;
                return promptResult;
            };

            window.Show();
            window.UpdateLayout();
            DrainDispatcher();

            Assert.False(view.ConfirmSettingsImportForTesting(preview));
            Assert.Equal(1, promptCalls);
            Assert.Contains("将覆盖字段", promptMessage!);
            Assert.Contains("Worker", promptMessage!);
            Assert.Contains("选择“否”保持原配置", promptMessage!);
            Assert.Equal(before, original.CreateSettingsFingerprint());

            promptResult = MessageBoxResult.Yes;
            Assert.True(view.ConfirmSettingsImportForTesting(preview));
            Assert.Equal(2, promptCalls);
            Assert.Equal(before, original.CreateSettingsFingerprint());
            original.ApplyPortableJson(preview);
            Assert.Equal(incoming.WorkerExecutable, original.WorkerExecutable);
            Assert.Equal(incoming.ThemeMode, original.ThemeMode);

            window.Close();
    }

    private static void EnsureApplicationResources()
    {
        var application = Application.Current ?? new Application();
        application.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        if (!application.Resources.Contains("BaseTextBlockStyle"))
            application.Resources.Add("BaseTextBlockStyle", new Style(typeof(TextBlock)));
    }

    private static void DrainDispatcher()
    {
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
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
            catch (Exception exception)
            {
                failure = exception;
            }
            finally
            {
                try { Application.Current?.Shutdown(); } catch { }
                try { Dispatcher.CurrentDispatcher.InvokeShutdown(); } catch { }
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure != null)
            throw new Xunit.Sdk.XunitException(failure.ToString());
    }
}

[CollectionDefinition("Q23SettingsActionsWpf", DisableParallelization = true)]
public sealed class Q23SettingsActionsWpfCollection
{
}
