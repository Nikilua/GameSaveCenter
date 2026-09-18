using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Views;
using Xunit;
using WorkspaceStatePresenter = GameSaveCenter.Playnite.Controls.WorkspaceStatePresenter;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R06EmptyStateBehaviorTests
{
    [Fact]
    public void SaveTablesKeepHeadersAndSeparateLoadingEmptyCompletedAndFailureStates()
    {
        RunSta(() =>
        {
            var state = new SavePageState();
            var view = new SaveCenterView { DataContext = state };
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

                var history = (DataGrid)view.FindName("SaveHistoryGrid")!;
                var candidates = (DataGrid)view.FindName("SaveCandidateGrid")!;
                var historyEmpty = (TextBlock)view.FindName("SaveHistoryEmptyStateText")!;
                var candidateEmpty = (TextBlock)view.FindName("SaveCandidateEmptyStateText")!;
                var historyPresenter = (WorkspaceStatePresenter)view.FindName("SaveHistoryStatePresenter")!;
                var candidatePresenter = (WorkspaceStatePresenter)view.FindName("SaveCandidateStatePresenter")!;

                Assert.Equal(7, history.Columns.Count);
                Assert.Equal(4, candidates.Columns.Count);
                Assert.Equal(new[] { "时间", "类型", "文件数", "大小", "设备", "备注", "状态" }, history.Columns.Select(column => column.Header).ToArray());
                Assert.Equal(new[] { "可信度", "状态", "路径", "依据" }, candidates.Columns.Select(column => column.Header).ToArray());

                state.SetState("Loading", historyOverlay: true, candidateOverlay: true);
                PumpLayout(window);
                Assert.Equal(Visibility.Visible, historyPresenter.Visibility);
                Assert.Equal(Visibility.Visible, candidatePresenter.Visibility);
                Assert.Equal(Visibility.Collapsed, historyEmpty.Visibility);
                Assert.Equal(Visibility.Collapsed, candidateEmpty.Visibility);

                state.SetState("Empty", historyOverlay: false, candidateOverlay: false);
                PumpLayout(window);
                Assert.Equal(Visibility.Collapsed, historyPresenter.Visibility);
                Assert.Equal(Visibility.Collapsed, candidatePresenter.Visibility);
                Assert.Equal(Visibility.Visible, historyEmpty.Visibility);
                Assert.Equal(Visibility.Visible, candidateEmpty.Visibility);
                Assert.Contains("首次读取为空", candidateEmpty.Text);

                state.SetState("Ready", historyOverlay: false, candidateOverlay: false);
                PumpLayout(window);
                Assert.Equal(Visibility.Visible, candidateEmpty.Visibility);
                Assert.Contains("候选处理完成", candidateEmpty.Text);

                state.SetState("Error", historyOverlay: true, candidateOverlay: true);
                PumpLayout(window);
                Assert.Equal(Visibility.Visible, historyPresenter.Visibility);
                Assert.Equal(Visibility.Visible, candidatePresenter.Visibility);
                Assert.Equal(Visibility.Collapsed, historyEmpty.Visibility);
                Assert.Equal(Visibility.Collapsed, candidateEmpty.Visibility);

                Assert.Same(state.LoadDetailsCommand, historyPresenter.RetryCommand);
                Assert.Same(state.LoadDetailsCommand, candidatePresenter.RetryCommand);
                historyPresenter.RetryCommand!.Execute(null);
                Assert.Equal(1, state.RetryCount);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void SaveSourceUsesStateAwareRetryAndDoesNotUseBusyAsAnEmptySignal()
    {
        var root = FindRepositoryRoot();
        var view = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
        var state = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.WorkspaceStates.cs"));
        var implementation = System.IO.File.ReadAllText(System.IO.Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));

        Assert.Contains("SaveDetailsState", view);
        Assert.Contains("SaveHistoryStateOverlayVisible", view);
        Assert.Contains("SaveCandidateStateOverlayVisible", view);
        Assert.Contains("RetryCommand=\"{Binding LoadDetailsCommand}\"", view);
        Assert.Contains("CompleteSaveDetailsLoad();", implementation);
        Assert.Contains("FailSaveDetailsLoad(ex)", implementation);
        Assert.Contains("WorkspaceDataState.Error", state);
        Assert.Contains("WorkspaceDataState.Stale", state);
        Assert.DoesNotContain("DataTrigger Binding=\"{Binding IsBusy}\" Value=\"False\"", view);
    }

    private sealed class SavePageState : INotifyPropertyChanged
    {
        public ObservableCollection<BackupVersionDto> Backups { get; } = new ObservableCollection<BackupVersionDto>();
        public ObservableCollection<SavePathCandidateDto> SaveCandidates { get; } = new ObservableCollection<SavePathCandidateDto>();
        public object? SelectedGame => null;
        public object? SelectedBackup { get; set; }
        public object? SelectedCandidate { get; set; }
        public ICommand LoadDetailsCommand { get; }
        public string SaveDetailsState { get; private set; } = "Empty";
        public string SaveDetailsPresenterState => SaveDetailsState == "Stale" ? "Degraded" : SaveDetailsState;
        public string SaveDetailsStateTitle => SaveDetailsState == "Error" ? "存档列表读取失败" : "状态";
        public string SaveDetailsStateMessage => SaveDetailsState == "Error" ? "读取失败，请重试。" : string.Empty;
        public string SaveDetailsStateDetail => SaveDetailsState == "Error" ? "合成错误" : string.Empty;
        public bool SaveHistoryStateOverlayVisible { get; private set; }
        public bool SaveCandidateStateOverlayVisible { get; private set; }
        public bool SaveDetailsStaleVisible => SaveDetailsState == "Stale";
        public string SaveCandidateEmptyText { get; private set; } = "暂无待处理的存档路径候选\n首次读取为空；可以点击“立即扫描”重新检测候选目录。";
        public int RetryCount { get; private set; }

        public SavePageState()
        {
            LoadDetailsCommand = new TestCommand(() => RetryCount++);
        }

        public void SetState(string state, bool historyOverlay, bool candidateOverlay)
        {
            SaveDetailsState = state;
            SaveHistoryStateOverlayVisible = historyOverlay;
            SaveCandidateStateOverlayVisible = candidateOverlay;
            SaveCandidateEmptyText = state == "Empty"
                ? "暂无待处理的存档路径候选\n首次读取为空；可以点击“立即扫描”重新检测候选目录。"
                : "当前没有新的待处理存档路径候选\n候选处理完成或本次扫描没有新结果；可以点击“立即扫描”重新检测。";
            foreach (var name in new[]
            {
                nameof(SaveDetailsState), nameof(SaveDetailsPresenterState), nameof(SaveDetailsStateTitle),
                nameof(SaveDetailsStateMessage), nameof(SaveDetailsStateDetail), nameof(SaveHistoryStateOverlayVisible),
                nameof(SaveCandidateStateOverlayVisible), nameof(SaveDetailsStaleVisible), nameof(SaveCandidateEmptyText)
            })
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private sealed class TestCommand : ICommand
    {
        private readonly Action execute;

        public TestCommand(Action execute) => this.execute = execute;

        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => execute();
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
    }

    private static string FindRepositoryRoot()
        => TestRepositoryContext.Root;

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
}
