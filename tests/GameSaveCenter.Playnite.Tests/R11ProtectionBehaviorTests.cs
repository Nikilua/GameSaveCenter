using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
public sealed class R11ProtectionBehaviorTests
{
    [Fact]
    public void ProtectionDisplayMatchesRetentionRulesAndExplainsUnlockBoundary()
    {
        var locked = Version("locked", isLocked: true, fileCount: 4, totalBytes: 4096);
        Assert.Equal("✓", locked.RetentionProtectionGlyphDisplay);
        Assert.Equal("已锁定保护", locked.RetentionProtectionDisplay);
        Assert.Contains("取消锁定并保存", locked.RetentionProtectionExplanationDisplay);

        var preRestore = Version("pre", isPreRestore: true, fileCount: 4, totalBytes: 4096);
        Assert.Equal("✓", preRestore.RetentionProtectionGlyphDisplay);
        Assert.Equal("PreRestore 保护", preRestore.RetentionProtectionDisplay);
        Assert.Contains("恢复保护流程", preRestore.RetentionProtectionExplanationDisplay);

        var healthy = Version("healthy", fileCount: 4, totalBytes: 4096, readiness: RestoreReadinessStatus.Ready);
        Assert.Equal("✓", healthy.RetentionProtectionGlyphDisplay);
        Assert.Equal("健康恢复点保护", healthy.RetentionProtectionDisplay);
        Assert.Contains("安全底线", healthy.RetentionProtectionExplanationDisplay);

        var ordinary = Version("ordinary", fileCount: 4, totalBytes: 4096);
        Assert.Equal("⚠", ordinary.RetentionProtectionGlyphDisplay);
        Assert.Equal("未受保护", ordinary.RetentionProtectionDisplay);
        Assert.Contains("按当前策略评估", ordinary.RetentionProtectionExplanationDisplay);

        // A Ready status with no usable content is not a retention safety floor.
        var invalidReady = Version("invalid-ready", readiness: RestoreReadinessStatus.Ready);
        Assert.False(invalidReady.IsHealthProtected);
        Assert.Equal("⚠", invalidReady.RetentionProtectionGlyphDisplay);
        Assert.Equal("未受保护", invalidReady.RetentionProtectionDisplay);
    }

    [Fact]
    public void SaveHistoryRowsExposeBoundProtectionStateAndExplanationContract()
    {
        RunSta(() =>
        {
            var state = new SavePageState();
            state.Backups.Add(Version("locked", isLocked: true, fileCount: 4, totalBytes: 4096));
            state.Backups.Add(Version("ordinary", fileCount: 4, totalBytes: 4096));

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
                var history = (DataGrid)view.GetType()
                    .GetField("SaveHistoryGrid", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                    .GetValue(view)!;
                history.EnableColumnVirtualization = false;
                history.EnableRowVirtualization = false;
                foreach (var backup in state.Backups) history.ScrollIntoView(backup);
                PumpLayout(window);

                var rows = FindVisualChildren<DataGridRow>(history)
                    .Where(row => row.Item is BackupVersionDto)
                    .ToList();
                Assert.Equal(2, rows.Count);
                var rowStates = rows
                    .Select(row => Assert.IsType<BackupVersionDto>(row.Item).RetentionProtectionGlyphDisplay)
                    .ToArray();
                Assert.Equal(new[] { "✓", "⚠" }, rowStates.OrderBy(value => value).ToArray());

                var xaml = File.ReadAllText(Path.Combine(TestRepositoryContext.Root,
                    "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
                Assert.Contains("RetentionProtectionGlyphDisplay", xaml);
                Assert.Contains("RetentionProtectionExplanationDisplay", xaml);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static BackupVersionDto Version(
        string id,
        bool isLocked = false,
        bool isPreRestore = false,
        int fileCount = 0,
        long totalBytes = 0,
        RestoreReadinessStatus? readiness = null)
        => new()
        {
            BackupId = id,
            IsLocked = isLocked,
            IsPreRestore = isPreRestore,
            FileCount = fileCount,
            TotalBytes = totalBytes,
            RestoreReadiness = readiness == null ? null : new RestoreReadinessDto { Status = readiness.Value }
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

    private sealed class SavePageState
    {
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
    }

    private sealed class CommandStub : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) { }
    }
}
