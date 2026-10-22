using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("R11SaveWpf")]
public sealed class R12RestoreReadinessViewBehaviorTests
{
    [Fact]
    public void SaveCenterDisplaysUnknownPartialStaleAndCorruptReadinessFromBoundBackup()
    {
        RunSta(() =>
        {
            var noHashes = Version("no-hashes", null);
            var state = new RestoreReadinessPageState(noHashes);
            var view = new SaveCenterView { DataContext = state, Width = 1040, Height = 900 };
            var window = new Window
            {
                Width = 1040,
                Height = 900,
                Content = view,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None
            };

            try
            {
                window.Show();
                PumpLayout(window);
                view.ApplyResponsiveLayout(view.ActualWidth, view.ActualHeight);
                PumpLayout(window);
                Assert.True(((Border)view.FindName("SaveHistoryInspectorFrame")!).IsVisible);

                var textBlocks = FindVisualChildren<TextBlock>(view).ToList();
                Assert.Contains("未验证", textBlocks.Select(item => item.Text));
                Assert.Contains("尚未验证该版本的可恢复性。", textBlocks.Select(item => item.Text));
                Assert.Contains("未提供哈希（不等于校验成功）", textBlocks.Select(item => item.Text));
                Assert.Contains("哈希覆盖：未提供", textBlocks.Select(item => item.Text));
                Assert.Contains("尚未检查", textBlocks.Select(item => item.Text));
                Assert.DoesNotContain("可恢复", textBlocks.Select(item => item.Text));

                var partialAndStale = Version("partial-stale", new RestoreReadinessDto
                {
                    Status = RestoreReadinessStatus.Warning,
                    CheckedUtc = DateTime.UtcNow.AddDays(-2),
                    ArchiveReadable = true,
                    ExtractSucceeded = true,
                    ExpectedFileCount = 2,
                    ActualFileCount = 2,
                    ExpectedTotalSize = 9,
                    ActualTotalSize = 9,
                    HashValidation = "Partial",
                    HashCoveredFileCount = 1,
                    HashEligibleFileCount = 2,
                    Summary = "归档可以解压，但 Manifest 只覆盖部分文件哈希（1/2），未覆盖文件不能视为已校验。"
                });
                state.SelectedBackup = partialAndStale;
                PumpLayout(window);

                textBlocks = FindVisualChildren<TextBlock>(view).ToList();
                Assert.Contains("有警告", textBlocks.Select(item => item.Text));
                Assert.Contains(partialAndStale.RestoreReadiness!.Summary, textBlocks.Select(item => item.Text));
                Assert.Contains("文件 2/2 · 大小 9 B/9 B", textBlocks.Select(item => item.Text));
                Assert.Contains("哈希部分覆盖", textBlocks.Select(item => item.Text));
                Assert.Contains("哈希覆盖：1/2 个文件", textBlocks.Select(item => item.Text));

                var checkedText = Assert.Single(
                    textBlocks,
                    item => item.Text.IndexOf("结果较旧", StringComparison.Ordinal) >= 0);
                Assert.Equal(partialAndStale.RestoreReadinessCheckedFullDisplay, checkedText.ToolTip);
                Assert.Equal(partialAndStale.RestoreReadinessCheckedFullDisplay,
                    AutomationProperties.GetHelpText(checkedText));

                var corrupt = Version("corrupt", new RestoreReadinessDto
                {
                    Status = RestoreReadinessStatus.Corrupted,
                    CheckedUtc = DateTime.UtcNow.AddMinutes(-10),
                    ArchiveReadable = false,
                    ExtractSucceeded = false,
                    ExpectedFileCount = 2,
                    ActualFileCount = 0,
                    ExpectedTotalSize = 9,
                    ActualTotalSize = 0,
                    HashValidation = "Failed",
                    HashCoveredFileCount = 2,
                    HashEligibleFileCount = 2,
                    ErrorCount = 1,
                    Summary = "ZIP 归档无法读取或已损坏。"
                });
                state.SelectedBackup = corrupt;
                PumpLayout(window);

                textBlocks = FindVisualChildren<TextBlock>(view).ToList();
                Assert.Contains("疑似损坏", textBlocks.Select(item => item.Text));
                Assert.Contains("ZIP 归档无法读取或已损坏。", textBlocks.Select(item => item.Text));
                Assert.Contains("文件 0/2 · 大小 0 B/9 B", textBlocks.Select(item => item.Text));
                Assert.Contains("哈希校验失败", textBlocks.Select(item => item.Text));
                Assert.Contains("哈希覆盖：2/2 个文件", textBlocks.Select(item => item.Text));
                Assert.DoesNotContain("可恢复", textBlocks.Select(item => item.Text));
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static BackupVersionDto Version(string id, RestoreReadinessDto? readiness)
        => new()
        {
            BackupId = id,
            FileCount = 2,
            TotalBytes = 9,
            RestoreReadiness = readiness
        };

    private static void PumpLayout(Window window)
    {
        window.UpdateLayout();
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
        window.UpdateLayout();
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
        if (failure != null) throw new Xunit.Sdk.XunitException(failure.ToString());
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match) yield return match;
            foreach (var descendant in FindVisualChildren<T>(child)) yield return descendant;
        }
    }

    private sealed class RestoreReadinessPageState : INotifyPropertyChanged
    {
        private BackupVersionDto? selectedBackup;

        public RestoreReadinessPageState(BackupVersionDto selected)
        {
            Backups.Add(selected);
            SelectedBackup = selected;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<BackupVersionDto> Backups { get; } = new();
        public int SaveTabIndex { get; set; }

        public BackupVersionDto? SelectedBackup
        {
            get => selectedBackup;
            set
            {
                selectedBackup = value;
                if (value != null && !Backups.Contains(value)) Backups.Add(value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedBackup)));
            }
        }
    }
}
