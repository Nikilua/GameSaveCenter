using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("R11SaveWpf")]
public sealed class R11BackupPreviewBehaviorTests
{
    [Fact]
    public void SaveHistoryShowsNonDestructiveBackupPreviewAndIdentifiedPaths()
    {
        RunSta(() =>
        {
            var state = new SavePageState
            {
                BackupPreview = new BackupPreviewDto
                {
                    GameName = "Demo Game",
                    State = "Ready",
                    PathCount = 2,
                    TotalBytes = 384,
                    GeneratedUtc = DateTime.UtcNow,
                    Paths = new List<BackupPreviewPathDto>
                    {
                        new() { Path = "profiles/save-01.dat", SizeBytes = 128 },
                        new() { Path = "settings/user.json", SizeBytes = 256 }
                    },
                    Summary = "本次备份将扫描 2 个路径，预计纳入 384 B。"
                }
            };
            var view = new SaveCenterView { DataContext = state, Width = 1040, Height = 700 };
            var window = new Window
            {
                Width = 1040,
                Height = 700,
                Content = view,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None
            };

            try
            {
                window.Show();
                PumpLayout(window);

                var card = Assert.IsType<Border>(view.FindName("SaveBackupPreviewCard"));
                Assert.Equal(Visibility.Visible, card.Visibility);
                var text = FindVisualChildren<TextBlock>(card).Select(x => x.Text).ToArray();
                Assert.Contains(text, value => value.IndexOf("本次备份将扫描 2 个路径", StringComparison.Ordinal) >= 0);
                Assert.Contains(text, value => value.IndexOf("profiles/save-01.dat", StringComparison.Ordinal) >= 0);
                Assert.Contains(text, value => value.IndexOf("settings/user.json", StringComparison.Ordinal) >= 0);

                var xaml = System.IO.File.ReadAllText(System.IO.Path.Combine(
                    TestRepositoryContext.Root,
                    "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
                Assert.Contains("PreviewBackupCommand", xaml);
                Assert.Contains("预览不产生归档", xaml);
                Assert.Contains("BackupPreview.IdentifiedPathsDisplay", xaml);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void SaveHistoryShowsLocalSuccessAndScopedCloudRetryWhenCloudUploadIsQueued()
    {
        RunSta(() =>
        {
            var state = new SavePageState
            {
                BackupResult = new BackupResultDto
                {
                    LocalState = "Succeeded",
                    CloudState = "RetryScheduled",
                    Summary = "本地备份已成功；云端上传已排队等待重试。"
                }
            };
            var view = new SaveCenterView { DataContext = state, Width = 1040, Height = 700 };
            var window = new Window
            {
                Width = 1040,
                Height = 700,
                Content = view,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None
            };

            try
            {
                window.Show();
                PumpLayout(window);

                var card = Assert.IsType<Border>(view.FindName("SaveBackupResultCard"));
                Assert.Equal(Visibility.Visible, card.Visibility);
                var text = FindVisualChildren<TextBlock>(card).Select(x => x.Text).ToArray();
                Assert.Contains(text, value => value.IndexOf("本地备份已成功", StringComparison.Ordinal) >= 0);
                Assert.Contains(text, value => value.IndexOf("云端上传已排队", StringComparison.Ordinal) >= 0);
                Assert.Contains(text, value => value.IndexOf("不会重新创建本地备份", StringComparison.Ordinal) >= 0);

                var retry = FindVisualChildren<Button>(card).Single(button => Equals(button.Content, "单独重试云端上传"));
                Assert.Equal(Visibility.Visible, retry.Visibility);
                Assert.Same(state.RetrySelectedGameCloudUploadCommand, retry.Command);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void UploadedCloudCopyWaitsForRemoteVerificationAndDoesNotOfferUploadRetry()
    {
        RunSta(() =>
        {
            var state = new SavePageState
            {
                BackupResult = new BackupResultDto
                {
                    LocalState = "Succeeded",
                    CloudState = "Uploaded",
                    Summary = "本地备份已完成；云端上传成功，尚未进行远端校验。"
                }
            };
            var view = new SaveCenterView { DataContext = state, Width = 1040, Height = 700 };
            var window = new Window
            {
                Width = 1040,
                Height = 700,
                Content = view,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None
            };

            try
            {
                window.Show();
                PumpLayout(window);

                var card = Assert.IsType<Border>(view.FindName("SaveBackupResultCard"));
                Assert.Equal(Visibility.Visible, card.Visibility);
                var text = FindVisualChildren<TextBlock>(card).Select(x => x.Text).ToArray();
                Assert.Contains(text, value => value.IndexOf("待远端校验", StringComparison.Ordinal) >= 0);
                Assert.DoesNotContain(text, value => value.IndexOf("远端已校验", StringComparison.Ordinal) >= 0);

                var retry = FindVisualChildren<Button>(card).Single(button => Equals(button.Content, "单独重试云端上传"));
                Assert.Equal(Visibility.Collapsed, retry.Visibility);
            }
            finally
            {
                window.Close();
            }
        });
    }

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

    private sealed class SavePageState
    {
        public BackupPreviewDto BackupPreview { get; set; } = new BackupPreviewDto();
        public BackupResultDto BackupResult { get; set; } = new BackupResultDto();
        public ObservableCollection<BackupVersionDto> Backups { get; } = new();
        public ObservableCollection<SavePathCandidateDto> SaveCandidates { get; } = new();
        public object? SelectedGame => null;
        public BackupVersionDto? SelectedBackup { get; set; }
        public SavePathCandidateDto? SelectedCandidate { get; set; }
        public string SaveDetailsState => "Ready";
        public string SaveDetailsPresenterState => "Ready";
        public string SaveDetailsStateTitle => string.Empty;
        public string SaveDetailsStateMessage => string.Empty;
        public string SaveDetailsStateDetail => string.Empty;
        public bool SaveDetailsStaleVisible => false;
        public bool SaveHistoryStateOverlayVisible => false;
        public bool SaveCandidateStateOverlayVisible => false;
        public string RestoreAvailabilityHint => string.Empty;
        public bool RestoreAvailabilityNeedsMaintenance => false;
        public ICommand LoadDetailsCommand { get; } = new CommandStub();
        public ICommand RetrySelectedGameCloudUploadCommand { get; } = new CommandStub();
    }

    private sealed class CommandStub : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) { }
    }
}
