using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class Round5OverviewSaveContextTests
{
    [Fact]
    public void OverviewHeroKeepsNeutralStatusAndNextStepInStandardWindow()
    {
        RunSta(() =>
        {
            var data = new OverviewFixture();
            var overview = new OverviewView { DataContext = data };
            var window = Show(overview, 1040, 700);
            try
            {
                overview.ApplyResponsiveColumns(stack: true);
                overview.ApplyResponsiveWidth(1040);
                overview.ApplyResponsiveHeight(700, stack: true);
                PumpLayout(window);

                var card = Assert.IsType<Border>(overview.FindName("OverviewTodayHeroCard"));
                var eyebrow = Assert.IsType<TextBlock>(overview.FindName("OverviewTodayHeroEyebrow"));
                var title = Assert.IsType<TextBlock>(overview.FindName("OverviewPriorityTitleText"));
                var action = FindVisualDescendants<ButtonBase>(card)
                    .Single(button => AutomationProperties.GetName(button) == data.OverviewPriorityActionText);

                Assert.Equal("当前状态", eyebrow.Text);
                Assert.Equal(Visibility.Visible, eyebrow.Visibility);
                Assert.Equal("查看未匹配游戏", title.Text);
                Assert.True(card.IsVisible);
                Assert.True(card.ActualWidth > 0 && card.ActualHeight > 0,
                    $"Overview hero is not arranged in the standard window: {card.ActualWidth:0.##}x{card.ActualHeight:0.##}.");
                Assert.True(action.IsVisible && action.ActualWidth > 0 && action.ActualHeight > 0,
                    $"Overview next step is not visible: visible={action.IsVisible}, bounds={action.ActualWidth:0.##}x{action.ActualHeight:0.##}.");
                Assert.Same(data.OverviewPriorityActionCommand, action.Command);
                Assert.Equal(data.OverviewPriorityActionText, AutomationProperties.GetName(action));
                Assert.Equal(data.OverviewPriorityActionToolTip, action.ToolTip);

                var pageScroll = Assert.IsType<ScrollViewer>(overview.FindName("OverviewStackScrollSurface"));
                Assert.Equal(ScrollBarVisibility.Auto, pageScroll.VerticalScrollBarVisibility);
                Assert.Equal(ScrollBarVisibility.Disabled, pageScroll.HorizontalScrollBarVisibility);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void SaveHistoryShowsAndUpdatesSelectedGameIdentityInStandardWindow()
    {
        RunSta(() =>
        {
            var data = new SaveFixture
            {
                SelectedGame = new GameFixture("合成游戏", "合成游戏 · 合成平台")
            };
            var save = new SaveCenterView { DataContext = data };
            var window = Show(save, 1040, 700);
            try
            {
                save.ApplyResponsiveLayout(1040, 700);
                PumpLayout(window);

                var target = Assert.IsType<TextBlock>(save.FindName("SaveHistoryTargetGame"));
                var summary = Assert.IsType<Border>(save.FindName("SaveHistorySummaryCard"));
                var history = Assert.IsType<DataGrid>(save.FindName("SaveHistoryGrid"));
                var pageScroll = Assert.IsType<ScrollViewer>(save.FindName("SaveHistoryPageScrollViewer"));

                Assert.True(summary.IsVisible && summary.ActualHeight > 0);
                Assert.True(target.IsVisible && target.ActualWidth > 0 && target.ActualHeight > 0,
                    $"Selected game identity is not visible: visible={target.IsVisible}, bounds={target.ActualWidth:0.##}x{target.ActualHeight:0.##}.");
                Assert.Equal("合成游戏", target.Text);
                Assert.Equal("存档历史当前游戏", AutomationProperties.GetName(target));
                Assert.Equal("合成游戏 · 合成平台", AutomationProperties.GetHelpText(target));
                Assert.Equal("合成游戏 · 合成平台", target.ToolTip);
                var retry = FindVisualDescendants<ButtonBase>(save)
                    .Single(button => AutomationProperties.GetName(button) == "重试读取存档列表");
                Assert.Same(data.LoadDetailsCommand, retry.Command);
                Assert.True(history.MinHeight >= 236);
                Assert.Equal(ScrollBarVisibility.Auto, pageScroll.VerticalScrollBarVisibility);
                Assert.Equal(ScrollBarVisibility.Disabled, pageScroll.HorizontalScrollBarVisibility);

                data.SelectedGame = null;
                PumpLayout(window);

                Assert.Equal("未选择游戏", target.Text);
                Assert.Equal("未选择游戏", AutomationProperties.GetHelpText(target));
                Assert.Equal("未选择游戏", target.ToolTip);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static Window Show(UserControl view, double width, double height)
    {
        var window = new Window
        {
            Width = width,
            Height = height,
            Content = view,
            ShowInTaskbar = false,
            ShowActivated = false,
            WindowStyle = WindowStyle.None,
            Opacity = 0.01
        };
        window.Show();
        return window;
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception exception) { failure = exception; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(20)), "WPF layout test did not finish within 20 seconds.");
        if (failure is not null) throw new Xunit.Sdk.XunitException(failure.ToString());
    }

    private static void PumpLayout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
        window.UpdateLayout();
    }

    private static T[] FindVisualDescendants<T>(DependencyObject root)
        where T : DependencyObject
    {
        var matches = new System.Collections.Generic.List<T>();
        Visit(root, matches);
        return matches.ToArray();
    }

    private static void Visit<T>(DependencyObject? current, System.Collections.Generic.List<T> matches)
        where T : DependencyObject
    {
        if (current is null) return;
        if (current is T match) matches.Add(match);
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(current); index++)
            Visit(VisualTreeHelper.GetChild(current, index), matches);
    }

    private sealed class OverviewFixture
    {
        public DashboardSnapshotDto Snapshot { get; } = new()
        {
            ManagedGames = 3,
            MatchedGames = 2,
            WarningGames = 1,
            CloudTransfers = new CloudTransferSummaryDto()
        };
        public bool IsDashboardSnapshotLoaded => true;
        public string OverviewPriorityKind => "Unmatched";
        public string OverviewPriorityTitle => "查看未匹配游戏";
        public string OverviewPriorityDescription => "检查当前游戏库中的规则覆盖情况。";
        public string OverviewPriorityActionText => "查看未匹配游戏";
        public string OverviewPriorityActionToolTip => "打开未匹配游戏筛选。";
        public ICommand OverviewPriorityActionCommand { get; } = new TestCommand();
        public string OverviewManagedGamesDisplay => "3";
        public string OverviewMatchedGamesDisplay => "2";
        public string OverviewWarningGamesDisplay => "1";
        public string OverviewCloudQueueDisplay => "0";
        public string OverviewUnassignedMediaDisplay => "0";
        public string OverviewRunningGamesDisplay => "0";
        public string OverviewSnapshotScopeDisplay => "全库 · 合成数据";
        public string OverviewSnapshotUpdatedDisplay => "合成更新时间";
        public string OverviewCurrentGameScopeDisplay => "当前游戏";
        public string SelectedGameBackupVersionDisplay => "0";
        public string SelectedGameMediaCountDisplay => "0 项";
        public string SelectedGameCloudStateDisplay => "未启用";
        public string SelectedGameLastBackupRelativeDisplay => "尚无备份";
        public string SelectedGameLastBackupFullDisplay => "尚无备份记录";
        public ObservableCollection<ActivityEntryDto> Activities { get; } = new();
        public ObservableCollection<TaskStatusDto> OverviewTasks { get; } = new();
        public ICommand RefreshCommand { get; } = new TestCommand();
        public ICommand OpenCloudQueueCommand { get; } = new TestCommand();
        public ICommand OpenActivityCommand { get; } = new TestCommand();
    }

    private sealed class SaveFixture : INotifyPropertyChanged
    {
        private GameFixture? selectedGame;

        public GameFixture? SelectedGame
        {
            get => selectedGame;
            set
            {
                selectedGame = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedGame)));
            }
        }

        public ObservableCollection<BackupVersionDto> Backups { get; } = new();
        public ObservableCollection<SavePathCandidateDto> SaveCandidates { get; } = new();
        public int SaveTabIndex { get; set; }
        public object? SelectedBackup { get; set; }
        public object? SelectedCandidate { get; set; }
        public ICommand LoadDetailsCommand { get; } = new TestCommand();
        public bool SaveDetailsStaleVisible => false;
        public string BackupHistoryRangeSummary => "尚无历史版本";
        public string RestoreAvailabilityHint => "请选择历史版本并完成安全检查。";
        public bool SaveHistoryStateOverlayVisible => false;
        public bool SaveCandidateStateOverlayVisible => false;
        public string SaveDetailsPresenterState => "Ready";
        public string SaveDetailsState => "Ready";
        public string SaveDetailsStateTitle => "已读取存档信息";
        public string SaveDetailsStateMessage => string.Empty;
        public string SaveDetailsStateDetail => string.Empty;
        public string SaveCandidateEmptyText => "暂无待处理候选。";

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private sealed class GameFixture
    {
        public GameFixture(string name, string identityDisplay)
        {
            Name = name;
            IdentityDisplay = identityDisplay;
        }

        public string Name { get; }
        public string IdentityDisplay { get; }
    }

    private sealed class TestCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) { }
    }
}
