using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class WorklistExportPreviewBehaviorTests
{
    [Fact]
    public void PreviewCommandExportsOnlyUncompletedItemsAndKeepsUnresolvedLookupStates()
    {
        var list = new WorklistDefinition { Name = "周末检查" };
        var entries = new[]
        {
            Entry(list, WorklistObjectKind.BackupVersion, "backup-private-1", WorklistReferenceState.Indexed, "Bongo Cat · 10 月 20 日存档"),
            Entry(list, WorklistObjectKind.MediaItem, "media-private-2", WorklistReferenceState.NotFound, "本地媒体索引已失效"),
            Entry(list, WorklistObjectKind.BackgroundTask, "task-private-3", WorklistReferenceState.Unavailable, "Worker 暂不可用"),
            Entry(list, WorklistObjectKind.ValidationFinding, "finding-private-4", WorklistReferenceState.Resolved, "已解决问题", manuallyComplete: true)
        };
        var timestamp = new DateTime(2026, 10, 21, 4, 5, 6, DateTimeKind.Utc);
        var session = new WorklistExportPreviewSession();
        var command = new CreateWorklistExportPreviewCommand(session, () => list, () => entries, () => timestamp);

        Assert.True(command.CanExecute(null));
        command.Execute(null);

        Assert.True(session.IsOpen);
        Assert.Contains("导出时间：2026-10-21 04:05:06 UTC", session.Text, StringComparison.Ordinal);
        Assert.Contains("3 项未手动完成", session.Text, StringComparison.Ordinal);
        Assert.Contains("Bongo Cat", session.Text, StringComparison.Ordinal);
        Assert.Contains("失效 · 未找到记录", session.Text, StringComparison.Ordinal);
        Assert.Contains("无法检查", session.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("已解决问题", session.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("backup-private-1", session.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("media-private-2", session.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("task-private-3", session.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("game-private-id", session.Text, StringComparison.Ordinal);
        Assert.Contains("人工整理记录", session.Text, StringComparison.Ordinal);
        Assert.Contains("不可导入或执行", session.Text, StringComparison.Ordinal);

        session.Close();
        Assert.False(session.IsOpen);
        Assert.Empty(session.Text);
    }

    [Fact]
    public void PreviewRedactsPathsSecretsAndControlCharactersFromShareableFields()
    {
        var list = new WorklistDefinition { Name = @"C:\Users\private-user\Exports" };
        var entry = Entry(list, WorklistObjectKind.ValidationFinding, "finding-do-not-export", WorklistReferenceState.Unavailable,
            summary: @"D:\private\saves\current.sav",
            parentSummary: "Bongo Cat https://example.test/game?token=share-secret",
            note: "token=note-secret\r\n下一行保留");

        var preview = WorklistExportPreviewFormatter.Create(
            list,
            new[] { entry },
            new DateTime(2026, 10, 21, 0, 0, 0, DateTimeKind.Utc));

        Assert.Contains("[包含本地路径，已隐藏]", preview, StringComparison.Ordinal);
        Assert.Contains("token=[已隐藏]", preview, StringComparison.Ordinal);
        Assert.Contains("[参数已隐藏]", preview, StringComparison.Ordinal);
        Assert.DoesNotContain("private-user", preview, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("current.sav", preview, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("share-secret", preview, StringComparison.Ordinal);
        Assert.DoesNotContain("note-secret", preview, StringComparison.Ordinal);
        Assert.DoesNotContain("finding-do-not-export", preview, StringComparison.Ordinal);
        Assert.DoesNotContain("\r", preview.Replace(Environment.NewLine, string.Empty), StringComparison.Ordinal);
    }

    [Fact]
    public void PreviewCommandTracksMissingSelectedListAndUsesFreshEntries()
    {
        WorklistDefinition? list = null;
        var entries = new List<WorklistBrowserEntry>();
        var session = new WorklistExportPreviewSession();
        var command = new CreateWorklistExportPreviewCommand(session, () => list, () => entries, () => DateTime.UtcNow);
        var canExecuteChanged = 0;
        command.CanExecuteChanged += (_, _) => canExecuteChanged++;

        Assert.False(command.CanExecute(null));
        command.RaiseCanExecuteChanged();
        Assert.Equal(1, canExecuteChanged);

        list = new WorklistDefinition { Name = "当前清单" };
        entries.Add(Entry(list, WorklistObjectKind.BackgroundTask, "task-current", WorklistReferenceState.NotChecked, "暂无索引摘要"));
        Assert.True(command.CanExecute(null));
        command.Execute(null);

        Assert.Contains("当前清单", session.Text, StringComparison.Ordinal);
        Assert.Contains("暂无索引摘要", session.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void ProductionPreviewControlIsReadOnlyAndItsCopyButtonUsesRetryFeedback()
    {
        RunSta(() =>
        {
            var view = new TaskCenterView();
            var viewType = typeof(TaskCenterView);
            var preview = (TextBox)viewType.GetField("WorklistExportPreviewTextBox", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(view)!;
            var copyButton = (Button)viewType.GetField("WorklistExportCopyButton", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(view)!;
            var overlay = (Grid)view.FindName("WorklistExportPreviewOverlay");
            var overlayState = new PreviewOverlayState();
            view.DataContext = overlayState;
            preview.Text = "GameSaveCenter 人工待办清单记录\r\n已脱敏的摘要";
            Assert.True(preview.IsReadOnly);

            var window = Show(view);
            var copied = string.Empty;
            var attempts = 0;
            try
            {
                overlayState.IsWorklistExportPreviewOpen = true;
                window.Dispatcher.Invoke(DispatcherPriority.DataBind, new Action(() => { }));
                Assert.Equal(Visibility.Visible, overlay.Visibility);

                WorklistExportClipboard.RetryDelayForVerification = _ => Task.CompletedTask;
                WorklistExportClipboard.ClipboardSetterForVerification = text =>
                {
                    attempts++;
                    if (attempts == 1) throw new InvalidOperationException("clipboard busy");
                    copied = text;
                };
                copyButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

                Assert.Equal(2, attempts);
                Assert.Equal(preview.Text, copied);
                Assert.Contains("已复制", ClipboardFeedback.GetMessageForVerification(copyButton), StringComparison.Ordinal);

                attempts = 0;
                WorklistExportClipboard.ClipboardSetterForVerification = _ =>
                {
                    attempts++;
                    throw new InvalidOperationException("clipboard busy");
                };
                copyButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

                Assert.Equal(4, attempts);
                Assert.Contains("请稍后重试", ClipboardFeedback.GetMessageForVerification(copyButton), StringComparison.Ordinal);
                Assert.Equal("待办清单脱敏导出预览", AutomationProperties.GetName((DependencyObject)view.FindName("WorklistExportPreviewOverlay")));

                overlayState.IsWorklistExportPreviewOpen = false;
                window.Dispatcher.Invoke(DispatcherPriority.DataBind, new Action(() => { }));
                Assert.Equal(Visibility.Collapsed, overlay.Visibility);
            }
            finally
            {
                ClipboardFeedback.CloseForVerification(copyButton);
                WorklistExportClipboard.ClipboardSetterForVerification = null;
                WorklistExportClipboard.RetryDelayForVerification = null;
                window.Close();
            }
        });
    }

    private static WorklistBrowserEntry Entry(
        WorklistDefinition list,
        WorklistObjectKind kind,
        string stableId,
        WorklistReferenceState state,
        string summary,
        string parentSummary = "",
        string note = "",
        bool manuallyComplete = false)
    {
        var record = new WorklistItemRecord
        {
            Kind = kind,
            StableObjectId = stableId,
            Note = note,
            IsManuallyComplete = manuallyComplete
        };
        Assert.Equal(WorklistItemAddResult.Added, list.TryAdd(record));
        var entry = new WorklistBrowserEntry(record, list.Items.Count - 1);
        entry.SetResolution(state, "game-private-id", summary, parentSummary);
        return entry;
    }

    private static Window Show(FrameworkElement content)
    {
        var window = new Window
        {
            Width = 720,
            Height = 520,
            WindowStyle = WindowStyle.None,
            ShowInTaskbar = false,
            ShowActivated = false,
            Content = content
        };
        window.Show();
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
        return window;
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

    private sealed class PreviewOverlayState : INotifyPropertyChanged
    {
        private bool isWorklistExportPreviewOpen;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsWorklistExportPreviewOpen
        {
            get => isWorklistExportPreviewOpen;
            set
            {
                if (isWorklistExportPreviewOpen == value) return;
                isWorklistExportPreviewOpen = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsWorklistExportPreviewOpen)));
            }
        }
    }
}
