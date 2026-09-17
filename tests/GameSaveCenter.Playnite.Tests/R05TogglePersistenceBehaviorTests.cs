using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Controls;
using GameSaveCenter.Playnite.Settings;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R05TogglePersistenceBehaviorTests
{
    [Fact]
    public void ProductionToggleKeepsValueDisplayAndDraftRollbackAligned()
    {
        RunSta(() =>
        {
            EnsureApplicationResources();
            var root = Path.Combine(Path.GetTempPath(), "gsc-r05-toggle-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                var worker = Path.Combine(root, "GameSaveCenter.Worker.exe");
                var ludusavi = Path.Combine(root, "ludusavi.exe");
                var rclone = Path.Combine(root, "rclone.exe");
                File.WriteAllText(worker, "worker");
                File.WriteAllText(ludusavi, "ludusavi");
                File.WriteAllText(rclone, "rclone");
                var saves = Path.Combine(root, "Saves");
                var media = Path.Combine(root, "Media");
                Directory.CreateDirectory(saves);
                Directory.CreateDirectory(media);

                var settings = new GameSaveCenterSettings
                {
                    WorkerExecutable = worker,
                    LudusaviExecutable = ludusavi,
                    RcloneExecutable = rclone,
                    LudusaviBackupDirectory = saves,
                    MediaArchiveDirectory = media,
                    EnableUiAnimations = true,
                    EnableGlassEffects = true,
                    EnableMediaSync = true,
                    EnableCloudUpload = true,
                    EnableTaskNotifications = true,
                    HealthInspectionEnabled = true
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
                tabs.SelectedIndex = 2;
                DrainDispatcher();
                window.UpdateLayout();

                var toggle = FindToggle(view, "启用界面动画");
                var track = (Border)toggle.Template.FindName("Track", toggle)!;
                var accent = (Brush)toggle.FindResource("GscAccentBrush");
                var controlStroke = (Brush)toggle.FindResource("GscControlStrokeBrush");
                var hint = (TextBlock)view.FindName("SettingsSaveHintText")!;
                var baseline = settings.GetEditBaselineFingerprint();

                Assert.True(toggle.IsChecked);
                AssertBrushColor(accent, track.Background);

                toggle.IsChecked = false;
                DrainDispatcher();
                window.UpdateLayout();

                Assert.False(settings.EnableUiAnimations);
                Assert.False(toggle.IsChecked);
                AssertBrushColor(controlStroke, track.Background);
                Assert.NotEqual(baseline, settings.CreateSettingsFingerprint());
                Assert.Contains("未保存更改", hint.Text);

                var persisted = new GameSaveCenterSettings();
                persisted.ImportPortableJson(settings.ExportPortableJson());
                Assert.False(persisted.EnableUiAnimations);

                settings.CancelEdit();
                DrainDispatcher();
                window.UpdateLayout();

                Assert.True(settings.EnableUiAnimations);
                Assert.True(toggle.IsChecked);
                AssertBrushColor(accent, track.Background);
                Assert.False(settings.HasPendingEdit);
                Assert.Contains("已保存", hint.Text);
                Assert.DoesNotContain("未保存更改", hint.Text);

                settings.BeginEdit();
                tabs.SelectedIndex = 3;
                DrainDispatcher();
                window.UpdateLayout();
                var mediaToggle = FindToggle(view, "同步新增截图和录像");
                var cloudToggle = FindToggleByContent(view, "完成本地任务后允许单向云端复制");
                var notificationToggle = FindToggle(view, "任务完成或失败时显示 Playnite 通知");
                var healthToggle = FindToggle(view, "启用恢复可用性巡检");
                var mediaSources = FindMediaSourceFields(view);
                var healthFields = (UniformGrid)view.FindName("HealthInspectionIntervalFields")!;

                Assert.True(mediaSources.IsEnabled);
                Assert.True(healthFields.IsEnabled);
                mediaToggle.IsChecked = false;
                cloudToggle.IsChecked = false;
                notificationToggle.IsChecked = false;
                healthToggle.IsChecked = false;
                DrainDispatcher();
                window.UpdateLayout();

                Assert.False(settings.EnableMediaSync);
                Assert.False(settings.EnableCloudUpload);
                Assert.False(settings.EnableTaskNotifications);
                Assert.False(settings.HealthInspectionEnabled);
                Assert.False(mediaSources.IsEnabled);
                Assert.False(healthFields.IsEnabled);

                settings.CancelEdit();
                DrainDispatcher();
                window.UpdateLayout();
                Assert.True(settings.EnableMediaSync);
                Assert.True(settings.EnableCloudUpload);
                Assert.True(settings.EnableTaskNotifications);
                Assert.True(settings.HealthInspectionEnabled);
                Assert.True(mediaToggle.IsChecked);
                Assert.True(cloudToggle.IsChecked);
                Assert.True(notificationToggle.IsChecked);
                Assert.True(healthToggle.IsChecked);
                Assert.True(mediaSources.IsEnabled);
                Assert.True(healthFields.IsEnabled);

                window.Close();
            }
            finally
            {
                try { Directory.Delete(root, true); } catch { }
            }
        });
    }

    private static ToggleSwitch FindToggle(FrameworkElement root, string automationName)
    {
        var toggles = FindVisualChildren<ToggleSwitch>(root);
        var match = toggles.FirstOrDefault(toggle => System.Windows.Automation.AutomationProperties.GetName(toggle) == automationName);
        if (match != null) return match;
        var names = string.Join(", ", toggles.Select(toggle => System.Windows.Automation.AutomationProperties.GetName(toggle)));
        throw new Xunit.Sdk.XunitException($"未找到开关“{automationName}”，当前 Automation Name：{names}");
    }

    private static ToggleSwitch FindToggleByContent(FrameworkElement root, string content)
        => FindVisualChildren<ToggleSwitch>(root)
            .Single(toggle => string.Equals(toggle.Content as string, content, StringComparison.Ordinal));

    private static WrapPanel FindMediaSourceFields(FrameworkElement root)
        => FindVisualChildren<WrapPanel>(root)
            .Single(panel => FindVisualChildren<ToggleSwitch>(panel)
                .Any(toggle => string.Equals(toggle.Content as string, "Steam 截图", StringComparison.Ordinal)));

    private static void EnsureApplicationResources()
    {
        var application = new System.Windows.Application();
        application.Resources = new ResourceDictionary();
        application.Resources.Add("BaseTextBlockStyle", new Style(typeof(TextBlock)));
    }

    private static void AssertBrushColor(Brush expected, Brush actual)
    {
        var expectedColor = Assert.IsType<SolidColorBrush>(expected).Color;
        var actualColor = Assert.IsType<SolidColorBrush>(actual).Color;
        Assert.Equal(expectedColor, actualColor);
    }

    private static T[] FindVisualChildren<T>(DependencyObject root) where T : DependencyObject
    {
        var results = new System.Collections.Generic.List<T>();
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match) results.Add(match);
            results.AddRange(FindVisualChildren<T>(child));
        }
        return results.ToArray();
    }

    private static void DrainDispatcher()
    {
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
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
