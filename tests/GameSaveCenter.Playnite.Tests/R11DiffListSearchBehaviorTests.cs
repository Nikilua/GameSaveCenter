using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
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
    public void ProductionDashboardFiltersLoadsMoreAndKeepsUnknownSeparateFromZeroChanges()
    {
        var diff = new BackupDiffDto
        {
            Added = Enumerable.Range(0, 123).Select(index => $"slot/new-{index:D3}.sav").ToList(),
            Modified = new System.Collections.Generic.List<string> { "slot/current.sav" },
            Removed = new System.Collections.Generic.List<string> { "slot/old.sav" },
            UnchangedCount = 0,
            ComparisonQuality = "Estimated"
        };
        var dashboard = CreateDetachedDiffDashboard(diff, visibleLimit: 2, searchText: "slot");

        Assert.Equal(123, dashboard.DiffAddedMatchCount);
        Assert.Equal(1, dashboard.DiffModifiedMatchCount);
        Assert.Equal(1, dashboard.DiffRemovedMatchCount);
        Assert.Equal(4, dashboard.DiffPathVisibleCount);
        Assert.Equal(2, dashboard.DiffAddedPaths.Count);
        Assert.True(dashboard.DiffPathHasMore);
        Assert.Contains("匹配 125 条，当前显示 4 条", dashboard.DiffPathFilterSummary);
        Assert.Contains("未知差异：", dashboard.DiffUnknownSummary);
        Assert.Contains("零变化：0 条", dashboard.DiffUnknownSummary);

        InvokeDashboardMethod(dashboard, "LoadMoreDiffPaths");
        Assert.Equal(124, dashboard.DiffPathVisibleCount);
        Assert.Equal(122, dashboard.DiffAddedPaths.Count);
        Assert.True(dashboard.DiffPathHasMore);
        InvokeDashboardMethod(dashboard, "LoadMoreDiffPaths");
        Assert.Equal(125, dashboard.DiffPathVisibleCount);
        Assert.False(dashboard.DiffPathHasMore);

        dashboard.DiffPathSearchText = "slot/old";
        dashboard.DiffPathKindFilter = "删除";
        Assert.Empty(dashboard.DiffAddedPaths);
        Assert.Empty(dashboard.DiffModifiedPaths);
        Assert.Equal(new[] { "slot/old.sav" }, dashboard.DiffRemovedPaths);
        Assert.Equal(0, dashboard.DiffAddedMatchCount);
        Assert.Equal(0, dashboard.DiffModifiedMatchCount);
        Assert.Equal(1, dashboard.DiffRemovedMatchCount);
        Assert.Equal(1, dashboard.DiffPathVisibleCount);
        Assert.False(dashboard.DiffPathHasMore);

        InvokeDashboardMethod(dashboard, "LoadMoreDiffPaths");
        Assert.Equal(1, dashboard.DiffPathVisibleCount);
        Assert.Equal(new[] { "slot/old.sav" }, dashboard.DiffRemovedPaths);
        InvokeDashboardMethod(dashboard, "ClearDiffPathFilters");
        Assert.Equal(string.Empty, dashboard.DiffPathSearchText);
        Assert.Equal("全部", dashboard.DiffPathKindFilter);
        Assert.Equal(125, dashboard.DiffAddedMatchCount + dashboard.DiffModifiedMatchCount + dashboard.DiffRemovedMatchCount);
        Assert.Equal(122, dashboard.DiffPathVisibleCount);
        Assert.True(dashboard.DiffPathHasMore);

        SetDashboardField(dashboard, "lastBackupDiff", new BackupDiffDto
        {
            ComparisonQuality = "Exact",
            UnchangedCount = 0
        });
        InvokeDashboardMethod(dashboard, "RefreshDiffPathResults");
        Assert.Equal(0, dashboard.DiffPathVisibleCount);
        Assert.False(dashboard.DiffPathHasMore);
        Assert.DoesNotContain("未知差异", dashboard.DiffUnknownSummary);
        Assert.Contains("差异质量：精确比较", dashboard.DiffUnknownSummary);
        Assert.Contains("零变化：0 条", dashboard.DiffUnknownSummary);

        SetDashboardField(dashboard, "lastBackupDiff", null);
        InvokeDashboardMethod(dashboard, "RefreshDiffPathResults");
        Assert.Equal("比较两个版本后，可按类型和路径筛选。", dashboard.DiffPathFilterSummary);
        Assert.Equal("差异质量和未变化数量会在比较后单独显示。", dashboard.DiffUnknownSummary);
        Assert.Equal(0, dashboard.DiffAddedMatchCount + dashboard.DiffModifiedMatchCount + dashboard.DiffRemovedMatchCount);
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

    private static DashboardViewModel CreateDetachedDiffDashboard(BackupDiffDto diff, int visibleLimit, string searchText)
    {
        // Exercise the production projection and command methods without constructing a
        // Playnite plugin, starting Worker IPC, or touching the real clipboard.
        var dashboard = (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
        SetDashboardField(dashboard, "<DiffAddedPaths>k__BackingField", new BatchObservableCollection<string>());
        SetDashboardField(dashboard, "<DiffModifiedPaths>k__BackingField", new BatchObservableCollection<string>());
        SetDashboardField(dashboard, "<DiffRemovedPaths>k__BackingField", new BatchObservableCollection<string>());
        SetDashboardField(dashboard, "<DiffPathKindOptions>k__BackingField", new[] { "全部", "新增", "修改", "删除" });
        SetDashboardField(dashboard, "lastBackupDiff", diff);
        SetDashboardField(dashboard, "diffPathVisibleLimit", visibleLimit);
        SetDashboardField(dashboard, "diffPathSearchText", searchText);
        SetDashboardField(dashboard, "diffPathKindFilter", "全部");
        SetDashboardField(dashboard, "commandRefreshScheduled", true);
        InvokeDashboardMethod(dashboard, "RefreshDiffPathResults");
        return dashboard;
    }

    private static void SetDashboardField(DashboardViewModel dashboard, string name, object? value)
    {
        var field = typeof(DashboardViewModel).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Missing DashboardViewModel field {name}.");
        field.SetValue(dashboard, value);
    }

    private static void InvokeDashboardMethod(DashboardViewModel dashboard, string name)
    {
        var method = typeof(DashboardViewModel).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Missing DashboardViewModel method {name}.");
        method.Invoke(dashboard, null);
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
