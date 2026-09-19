using System;
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
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("R11SaveWpf")]
public sealed class R11DiffListSearchBehaviorTests
{
    [Fact]
    public void FilterMatchesByKindAndPathWhileKeepingFiniteWindow()
    {
        var diff = new BackupDiffDto
        {
            Added = Enumerable.Range(0, 5).Select(index => $"slot/new-{index}.sav").ToList(),
            Modified = new System.Collections.Generic.List<string> { "slot/current.sav" },
            Removed = new System.Collections.Generic.List<string> { "slot/old.sav", "other/old.sav" },
            UnchangedCount = 7,
            ComparisonQuality = "Estimated"
        };

        var limited = BackupDiffPathFilter.Apply(diff, "slot", "全部", 2);
        Assert.Equal(5, limited.Added.Count);
        Assert.Single(limited.Modified);
        Assert.Single(limited.Removed);
        Assert.Equal(2, limited.VisibleAdded.Count);
        Assert.Equal(4, limited.VisibleCount);
        Assert.True(limited.HasMore);

        var removedOnly = BackupDiffPathFilter.Apply(diff, "slot/old", "删除", 10);
        Assert.Empty(removedOnly.Added);
        Assert.Empty(removedOnly.Modified);
        Assert.Equal(new[] { "slot/old.sav" }, removedOnly.Removed);
        Assert.False(removedOnly.HasMore);
    }

    [Fact]
    public void ComparePageExposesSelectableFullPathCopyForBoundedDiffRows()
    {
        RunSta(() =>
        {
            var path = @"profiles\very-long-save-slot\new-save.dat";
            var state = new DiffPageState(path);
            var view = new SaveCenterView
            {
                DataContext = state,
                Width = 1040,
                Height = 700
            };
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

                var pathBox = FindVisualChildren<TextBox>(view).Single(box => string.Equals(box.Text, path, StringComparison.Ordinal));
                Assert.True(pathBox.IsReadOnly);
                pathBox.SelectAll();
                Assert.Equal(path, pathBox.SelectedText);

                var copyButton = FindVisualChildren<Button>(view).Single(button => AutomationProperties.GetName(button) == "复制完整新增路径");
                Assert.Equal(path, copyButton.CommandParameter);
                copyButton.Command!.Execute(copyButton.CommandParameter);
                Assert.Equal(path, state.CopiedPath);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject root) where T : DependencyObject
    {
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

    private sealed class DiffPageState
    {
        public DiffPageState(string path)
        {
            DiffAddedPaths.Add(path);
            LastBackupDiff = new BackupDiffDto
            {
                Added = new System.Collections.Generic.List<string> { path },
                ComparisonQuality = "Estimated",
                UnchangedCount = 3,
                Summary = "合成差异"
            };
            CopyPathCommand = new TestCommand(value => CopiedPath = value as string ?? string.Empty);
        }

        public ObservableCollection<BackupVersionDto> Backups { get; } = new ObservableCollection<BackupVersionDto>();
        public ObservableCollection<SavePathCandidateDto> SaveCandidates { get; } = new ObservableCollection<SavePathCandidateDto>();
        public int SaveTabIndex { get; set; } = 3;
        public BackupVersionDto? SelectedBackup { get; set; }
        public SavePathCandidateDto? SelectedCandidate { get; set; }
        public BackupDiffDto LastBackupDiff { get; }
        public ObservableCollection<string> DiffAddedPaths { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> DiffModifiedPaths { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> DiffRemovedPaths { get; } = new ObservableCollection<string>();
        public string DiffPathSearchText { get; set; } = string.Empty;
        public string DiffPathKindFilter { get; set; } = "全部";
        public string[] DiffPathKindOptions { get; } = { "全部", "新增", "修改", "删除" };
        public string DiffPathFilterSummary { get; } = "匹配 1 条，当前显示 1 条。";
        public string DiffUnknownSummary { get; } = "未知差异：估算比较；零变化：3 条。";
        public int DiffAddedMatchCount { get; } = 1;
        public int DiffModifiedMatchCount { get; } = 0;
        public int DiffRemovedMatchCount { get; } = 0;
        public string CompareSelectionSummary { get; } = "A 为基准，B 为对照。";
        public string DiffComparedSummary { get; } = "A → B";
        public string DiffSummary => LastBackupDiff.Summary;
        public string? CopiedPath { get; private set; }
        public ICommand CopyPathCommand { get; }
        public ICommand CompareBackupCommand { get; } = new TestCommand();
        public ICommand SwapCompareBackupCommand { get; } = new TestCommand();
        public ICommand LoadMoreDiffPathsCommand { get; } = new TestCommand();
        public ICommand ClearDiffPathFiltersCommand { get; } = new TestCommand();
        public ICommand PreviewRetentionCommand { get; } = new TestCommand();
        public ICommand LoadDetailsCommand { get; } = new TestCommand();
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

        private sealed class TestCommand : ICommand
        {
            private readonly Action<object?> execute;

            public TestCommand(Action<object?>? execute = null) => this.execute = execute ?? (_ => { });
            public event EventHandler? CanExecuteChanged { add { } remove { } }
            public bool CanExecute(object? parameter) => true;
            public void Execute(object? parameter) => execute(parameter);
        }
    }
}
