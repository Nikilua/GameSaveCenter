using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Automation;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Models;
using GameSaveCenter.Core.Services;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("R11SaveWpf")]
public sealed class R11VersionComparisonBehaviorTests
{
    [Fact]
    public void ReversingComparisonDirectionReversesAddedRemovedAndDelta()
    {
        var before = new[]
        {
            new FileManifestEntry { RelativePath = "old.sav", SizeBytes = 2 },
            new FileManifestEntry { RelativePath = "slot.dat", SizeBytes = 1 }
        };
        var after = new[]
        {
            new FileManifestEntry { RelativePath = "new.sav", SizeBytes = 5 },
            new FileManifestEntry { RelativePath = "slot.dat", SizeBytes = 3 }
        };

        var forward = new FileManifestDiffService().Compare(before, after);
        var reverse = new FileManifestDiffService().Compare(after, before);

        Assert.Equal(new[] { "new.sav" }, forward.Added.Select(x => x.RelativePath));
        Assert.Equal(new[] { "old.sav" }, forward.Removed.Select(x => x.RelativePath));
        Assert.Single(forward.Modified);
        Assert.Equal(5, forward.AfterTotalBytes - forward.BeforeTotalBytes);

        Assert.Equal(new[] { "old.sav" }, reverse.Added.Select(x => x.RelativePath));
        Assert.Equal(new[] { "new.sav" }, reverse.Removed.Select(x => x.RelativePath));
        Assert.Single(reverse.Modified);
        Assert.Equal(-5, reverse.AfterTotalBytes - reverse.BeforeTotalBytes);
    }

    [Fact]
    public void ComparePageBindsDistinctABChoicesAndDisablesSameVersionCommand()
    {
        RunSta(() =>
        {
            var a = new BackupVersionDto
            {
                BackupId = "backup-a",
                CreatedUtc = new DateTime(2026, 9, 18, 10, 0, 0, DateTimeKind.Utc)
            };
            var b = new BackupVersionDto
            {
                BackupId = "backup-b",
                CreatedUtc = new DateTime(2026, 9, 19, 10, 0, 0, DateTimeKind.Utc)
            };
            var state = new ComparePageState(a, b);
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

                var combos = FindVisualChildren<ComboBox>(view)
                    .Where(combo => !string.IsNullOrWhiteSpace(AutomationProperties.GetName(combo)))
                    .ToArray();
                Assert.Contains(combos, combo => AutomationProperties.GetName(combo) == "选择 A 基准版本");
                Assert.Contains(combos, combo => AutomationProperties.GetName(combo) == "选择 B 对照版本");
                var leftCombo = combos.Single(combo => AutomationProperties.GetName(combo) == "选择 A 基准版本");
                var rightCombo = combos.Single(combo => AutomationProperties.GetName(combo) == "选择 B 对照版本");
                Assert.Same(a, leftCombo.SelectedItem);
                Assert.Same(b, rightCombo.SelectedItem);
                var leftText = FindVisualChildren<TextBlock>(leftCombo).Single(text => text.Text == a.ComparisonRelativeDisplay);
                var rightText = FindVisualChildren<TextBlock>(rightCombo).Single(text => text.Text == b.ComparisonRelativeDisplay);
                Assert.Equal(a.ComparisonFullDisplay, leftText.ToolTip);
                Assert.Equal(b.ComparisonFullDisplay, rightText.ToolTip);
                Assert.Equal(a.ComparisonFullDisplay, AutomationProperties.GetHelpText(leftText));
                Assert.Equal(b.ComparisonFullDisplay, AutomationProperties.GetHelpText(rightText));

                var compareButton = FindVisualChildren<Button>(view).Single(button => AutomationProperties.GetName(button) == "比较 A 与 B 版本");
                var swapButton = FindVisualChildren<Button>(view).Single(button => AutomationProperties.GetName(button) == "交换 A 和 B 版本");
                Assert.True(compareButton.IsEnabled);
                Assert.False(swapButton.IsEnabled);
                Assert.Contains("新增属于 B", state.CompareSelectionSummary);
                var selectionSummary = FindVisualChildren<TextBlock>(view).Single(text => text.Text == state.CompareSelectionSummary);
                Assert.Equal(state.CompareSelectionSummaryFullDisplay, selectionSummary.ToolTip);
                Assert.Equal(state.CompareSelectionSummaryFullDisplay, AutomationProperties.GetHelpText(selectionSummary));
                var emptyResultSummary = FindVisualChildren<TextBlock>(view).Single(text => text.Text == state.DiffComparedSummary);
                Assert.Equal(state.DiffComparedSummaryFullDisplay, emptyResultSummary.ToolTip);
                Assert.Equal(state.DiffComparedSummaryFullDisplay, AutomationProperties.GetHelpText(emptyResultSummary));

                compareButton.Command!.Execute(null);
                Assert.Equal(1, state.CompareRequestCount);
                state.HasComparisonResult = true;
                swapButton.Command!.Execute(null);
                Assert.Same(b, state.CompareLeftBackup);
                Assert.Same(a, state.CompareRightBackup);

                var sameVersionState = new ComparePageState(a, a);
                view.DataContext = sameVersionState;
                PumpLayout(window);
                var sameVersionButton = FindVisualChildren<Button>(view).Single(button => AutomationProperties.GetName(button) == "比较 A 与 B 版本");
                Assert.False(sameVersionButton.IsEnabled);
                Assert.Contains("同一版本", sameVersionState.CompareSelectionSummary);
                Assert.False(sameVersionButton.Command!.CanExecute(null));
                Assert.Equal(1, state.CompareRequestCount);
            }
            finally
            {
                window.Close();
            }
        });
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

    private sealed class ComparePageState
    {
        public ComparePageState(BackupVersionDto left, BackupVersionDto right)
        {
            Backups.Add(left);
            Backups.Add(right);
            CompareLeftBackup = left;
            CompareRightBackup = right;
            CompareBackupCommand = new TestCommand(() => CompareRequestCount++, () => !string.Equals(CompareLeftBackup?.BackupId, CompareRightBackup?.BackupId, StringComparison.OrdinalIgnoreCase));
            SwapCompareBackupCommand = new TestCommand(() =>
            {
                var oldLeft = CompareLeftBackup;
                CompareLeftBackup = CompareRightBackup;
                CompareRightBackup = oldLeft;
            }, () => HasComparisonResult);
        }

        public ObservableCollection<BackupVersionDto> Backups { get; } = new ObservableCollection<BackupVersionDto>();
        public ObservableCollection<SavePathCandidateDto> SaveCandidates { get; } = new ObservableCollection<SavePathCandidateDto>();
        public int SaveTabIndex { get; set; } = 3;
        public BackupVersionDto? SelectedBackup { get; set; }
        public SavePathCandidateDto? SelectedCandidate { get; set; }
        public BackupVersionDto? CompareLeftBackup { get; set; }
        public BackupVersionDto? CompareRightBackup { get; set; }
        public string CompareSelectionSummary => CompareLeftBackup != null && CompareRightBackup != null
            ? string.Equals(CompareLeftBackup.BackupId, CompareRightBackup.BackupId, StringComparison.OrdinalIgnoreCase)
                ? "A、B 当前是同一版本；请选择不同版本，不会发起比较或恢复。"
                : $"A：{CompareLeftBackup.ComparisonRelativeDisplay} → B：{CompareRightBackup.ComparisonRelativeDisplay}；新增属于 B，删除属于 A。"
            : "请选择两个不同版本。A 为基准版本，B 为对照版本；新增属于 B，删除属于 A。";
        public string CompareSelectionSummaryFullDisplay => CompareLeftBackup != null && CompareRightBackup != null
            ? string.Equals(CompareLeftBackup.BackupId, CompareRightBackup.BackupId, StringComparison.OrdinalIgnoreCase)
                ? "A、B 当前是同一版本；请选择不同版本，不会发起比较或恢复。"
                : $"A：{CompareLeftBackup.ComparisonFullDisplay} → B：{CompareRightBackup.ComparisonFullDisplay}；新增属于 B，删除属于 A。"
            : "请选择两个不同版本。A 为基准版本，B 为对照版本；新增属于 B，删除属于 A。";
        public string DiffComparedSummary { get; } = "尚未选择可比较的版本。";
        public string DiffComparedSummaryFullDisplay => "尚未选择可比较的版本。";
        public BackupDiffDto? LastBackupDiff { get; } = null;
        public string DiffSummary { get; } = "选择两个版本后，比较结果会显示在这里。";
        public int CompareRequestCount { get; private set; }
        public bool HasComparisonResult { get; set; }
        public ICommand CompareBackupCommand { get; }
        public ICommand SwapCompareBackupCommand { get; }
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
        public ICommand LoadDetailsCommand { get; } = new TestCommand(() => { });

        private sealed class TestCommand : ICommand
        {
            private readonly Action execute;
            private readonly Func<bool> canExecute;

            public TestCommand(Action execute, Func<bool>? canExecute = null)
            {
                this.execute = execute;
                this.canExecute = canExecute ?? (() => true);
            }

            public event EventHandler? CanExecuteChanged
            {
                add { }
                remove { }
            }
            public bool CanExecute(object? parameter) => canExecute();
            public void Execute(object? parameter) => execute();
        }
    }
}
