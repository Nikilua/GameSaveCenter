using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("R11SaveWpf")]
public sealed class R11VersionNoteBehaviorTests
{
    [Fact]
    public void SaveCenterVersionEditorCancelsDraftThroughTheRealViewCommandPath()
    {
        RunSta(() =>
        {
            var backup = new BackupVersionDto
            {
                BackupId = "stable-backup-id",
                Comment = "原始备注",
                IsLocked = true,
                CreatedUtc = new DateTime(2026, 9, 19, 10, 0, 0, DateTimeKind.Utc)
            };
            var state = new NotePageState(backup);
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

                var editor = FindVisualChildren<TextBox>(view)
                    .Single(textBox => AutomationProperties.GetName(textBox) == "版本备注");
                var cancel = FindVisualChildren<Button>(view)
                    .Single(button => AutomationProperties.GetName(button) == "取消版本备注修改");

                editor.Text = "尚未保存的草稿";
                editor.GetBindingExpression(TextBox.TextProperty)!.UpdateSource();
                Assert.Equal("尚未保存的草稿", state.BackupComment);
                Assert.True(cancel.IsEnabled);

                // The production Button resolves this exact ICommand. Execute it as a
                // deterministic WPF command probe; this avoids depending on a native mouse
                // device while still exercising the real view binding path.
                cancel.Command!.Execute(null);
                PumpLayout(window);

                Assert.Equal("原始备注", state.BackupComment);
                Assert.True(state.LockSelectedBackup);
                Assert.Equal("stable-backup-id", state.SelectedBackup!.BackupId);
                Assert.Equal("已取消版本备注修改，恢复为原值。", state.StatusMessage);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void DuplicateVersionCommentsStillRestoreSelectionByStableBackupId()
    {
        var versions = new List<BackupVersionDto>
        {
            new() { BackupId = "backup-a", Comment = "同一个版本说明" },
            new() { BackupId = "backup-b", Comment = "同一个版本说明" }
        };

        var restored = SelectionAnchorResolver.Restore(versions, "backup-b", 0, version => version.BackupId);

        Assert.Same(versions[1], restored);
        Assert.Equal("backup-b", restored!.BackupId);
        Assert.Equal("同一个版本说明", restored.Comment);
    }

    [Fact]
    public void SaveCenterMetadataSurfaceUsesCommentAndCancelWithoutArchivePathEditing()
    {
        var root = TestRepositoryContext.Root;
        var view = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
        var viewModel = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var dispatcher = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Worker", "Ipc", "IpcRequestDispatcher.cs"));
        var client = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Worker", "Infrastructure", "LudusaviClient.cs"));

        Assert.Contains("Text=\"{Binding BackupComment, UpdateSourceTrigger=PropertyChanged}\"", view);
        Assert.Contains("AutomationProperties.Name=\"取消版本备注修改\"", view);
        Assert.Contains("CancelBackupMetadataCommand", viewModel);
        Assert.Contains("SyncBackupEditor(SelectedBackup, preserveDirtyFields: false)", viewModel);
        Assert.Contains("EditBackupAsync(match.Name,update.BackupId,update.Comment,update.Locked", dispatcher);
        Assert.Contains("RefreshBackupHistoryAsync(update.PlayniteId,match.Name,token)", dispatcher);
        Assert.Contains("editBackup = new { game, backup = backupId, comment, locked }", client);
        Assert.DoesNotContain("ArchivePath", client.Substring(client.IndexOf("EditBackupAsync", StringComparison.Ordinal)));
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root) where T : DependencyObject
    {
        if (root == null) yield break;
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T typed) yield return typed;
            foreach (var descendant in FindVisualChildren<T>(child)) yield return descendant;
        }
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

    private sealed class NotePageState
    {
        private readonly BackupVersionDto original;

        public NotePageState(BackupVersionDto backup)
        {
            original = backup;
            Backups.Add(backup);
            SelectedBackup = backup;
            BackupComment = backup.Comment;
            LockSelectedBackup = backup.IsLocked;
            CancelBackupMetadataCommand = new TestCommand(() =>
            {
                BackupComment = original.Comment;
                LockSelectedBackup = original.IsLocked;
                StatusMessage = "已取消版本备注修改，恢复为原值。";
            });
        }

        public ObservableCollection<BackupVersionDto> Backups { get; } = new();
        public ObservableCollection<SavePathCandidateDto> SaveCandidates { get; } = new();
        public BackupVersionDto SelectedBackup { get; set; }
        public object? SelectedGame => null;
        public SavePathCandidateDto? SelectedCandidate { get; set; }
        public string BackupComment { get; set; }
        public bool LockSelectedBackup { get; set; }
        public string StatusMessage { get; private set; } = "准备就绪";
        public ICommand CancelBackupMetadataCommand { get; }
        public ICommand UpdateBackupMetadataCommand { get; } = new TestCommand(() => { });
        public ICommand CompareBackupCommand { get; } = new TestCommand(() => { });
        public ICommand LoadDetailsCommand { get; } = new TestCommand(() => { });
        public ICommand RestoreCommand { get; } = new TestCommand(() => { });
        public ICommand ValidateRestoreReadinessCommand { get; } = new TestCommand(() => { });
        public ICommand UndoRestoreCommand { get; } = new TestCommand(() => { });
        public ICommand CopyPathCommand { get; } = new TestCommand(() => { });
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
        public int SaveTabIndex { get; set; } = 0;
        public BackupVersionDto? CompareLeftBackup { get; set; }
        public BackupVersionDto? CompareRightBackup { get; set; }
        public string CompareSelectionSummary => string.Empty;
        public string DiffComparedSummary => string.Empty;
        public string DiffSummary => string.Empty;
        public object? LastBackupDiff => null;
        public string DiffPathSearchText { get; set; } = string.Empty;
        public string DiffPathKindFilter { get; set; } = "全部";
        public ObservableCollection<object> PolicyTemplates { get; } = new();
        public object? SelectedPolicyTemplate { get; set; }
        public int DiffAddedMatchCount => 0;
        public int DiffModifiedMatchCount => 0;
        public int DiffRemovedMatchCount => 0;
        public int DiffPathVisibleCount => 0;
        public bool DiffPathHasMore => false;
        public string DiffPathFilterSummary => string.Empty;
        public string DiffUnknownSummary => string.Empty;

        private sealed class TestCommand : ICommand
        {
            private readonly Action execute;

            public TestCommand(Action execute) => this.execute = execute;

            public event EventHandler? CanExecuteChanged
            {
                add { }
                remove { }
            }

            public bool CanExecute(object? parameter) => true;
            public void Execute(object? parameter) => execute();
        }
    }
}
