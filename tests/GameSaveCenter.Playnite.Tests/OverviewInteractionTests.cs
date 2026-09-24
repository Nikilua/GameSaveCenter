using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using PlayniteButton = GameSaveCenter.Playnite.Controls.Button;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class OverviewInteractionTests
{
    [Theory]
    [InlineData(false, "Unmatched", "未匹配")]
    [InlineData(true, "Backupable", "可备份")]
    public void PriorityHeroClickOpensTheResolvedGamePickerWithoutStartingBulkBackup(
        bool matched,
        string expectedKind,
        string expectedFilter)
    {
        Exception? exception = null;
        var bulkBackupCalls = 0;
        var pickerRequests = 0;

        var thread = new System.Threading.Thread(() =>
        {
            try
            {
                var snapshot = new DashboardSnapshotDto
                {
                    WorkerHealthy = true,
                    ManagedGames = 1,
                    LudusaviAvailable = matched,
                    Games = new List<GameStatusDto>
                    {
                        new GameStatusDto { LudusaviMatched = matched }
                    }
                };
                var plugin = (GameSaveCenterPlugin)FormatterServices.GetUninitializedObject(typeof(GameSaveCenterPlugin));
                SetBackingField(plugin, "Settings", new GameSaveCenterSettings { OnboardingCompleted = true });

                var viewModel = (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
                var gamePicker = new GamePickerViewModel();
                SetPrivateField(viewModel, "plugin", plugin);
                SetPrivateField(viewModel, "gamePicker", gamePicker);
                SetPrivateField(viewModel, "snapshot", snapshot);
                SetPrivateField(viewModel, "dashboardSnapshotLoaded", true);
                SetPrivateField(viewModel, "currentWorkspace", WorkspaceKind.Tasks);
                SetBackingField(viewModel, "OpenOverviewGamePickerCommand", new RelayCommand(_ =>
                    typeof(DashboardViewModel).GetMethod("OpenOverviewGamePicker", BindingFlags.Instance | BindingFlags.NonPublic)!
                        .Invoke(viewModel, Array.Empty<object>())));
                SetBackingField(viewModel, "BackupAllCommand", new CountingCommand(() => bulkBackupCalls++));
                viewModel.GamePickerRequested += (_, _) => pickerRequests++;

                var overview = new OverviewView
                {
                    DataContext = new OverviewPriorityInteractionData(viewModel),
                    Width = 1280,
                    Height = 820
                };
                var host = new Grid { Width = 1280, Height = 820 };
                host.Children.Add(overview);
                host.Measure(new Size(1280, 820));
                host.Arrange(new Rect(0, 0, 1280, 820));
                overview.UpdateLayout();

                Assert.Equal(expectedKind, viewModel.OverviewPriorityKind);
                Assert.Equal(expectedFilter == "未匹配" ? "查看未匹配游戏" : "查看可备份游戏", viewModel.OverviewPriorityActionText);
                var heroAction = FindVisualDescendants<PlayniteButton>(overview).Single(button =>
                    AutomationProperties.GetName(button) == viewModel.OverviewPriorityActionText);
                Assert.Same(viewModel.OverviewPriorityActionCommand, heroAction.Command);

                // Exercise ButtonBase's normal command dispatch, then verify the real
                // DashboardViewModel route updates the shared picker and asks the shell
                // to open it. A matched game must never turn this action into bulk backup.
                typeof(ButtonBase).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(heroAction, Array.Empty<object>());

                Assert.Equal(expectedFilter, gamePicker.StatusFilter);
                Assert.Equal(WorkspaceKind.Overview, viewModel.CurrentWorkspace);
                Assert.Equal(1, pickerRequests);
                Assert.Equal(0, bulkBackupCalls);
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });

        thread.SetApartmentState(System.Threading.ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
    }

    [Fact]
    public void OverviewActivityRowsKeepTheirVisualTreeAndCloudQueueCardExecutesOneClickCommand()
    {
        Exception? exception = null;
        var cloudQueueClicks = 0;

        var thread = new System.Threading.Thread(() =>
        {
            try
            {
                var data = new OverviewInteractionData(() => cloudQueueClicks++);
                data.Snapshot.CloudTransfers = new CloudTransferSummaryDto
                {
                    PendingCount = 1,
                    UploadedCount = 2,
                    VerifiedCount = 3
                };
                data.Activities.Add(new ActivityEntryDto
                {
                    GameName = "全局",
                    Summary = "首页活动测试",
                    CreatedUtc = DateTime.UtcNow
                });

                var overview = new OverviewView
                {
                    DataContext = data,
                    Width = 1280,
                    Height = 820
                };
                var host = new Grid
                {
                    Width = 1280,
                    Height = 820
                };
                host.Children.Add(overview);

                host.Measure(new Size(1280, 820));
                host.Arrange(new Rect(0, 0, 1280, 820));
                overview.UpdateLayout();

                var activityButton = FindVisualDescendants<PlayniteButton>(overview).Find(button =>
                    AutomationProperties.GetName(button) == "打开活动对应工作区");
                Assert.NotNull(activityButton);
                Assert.IsType<Border>(activityButton!.Content);

                var cloudQueueCard = FindVisualDescendants<PlayniteButton>(overview).Find(button =>
                    AutomationProperties.GetName(button) == "打开云端队列");
                Assert.NotNull(cloudQueueCard);
                Assert.IsType<StackPanel>(cloudQueueCard!.Content);
                Assert.Null(cloudQueueCard.ContentTemplate);
                Assert.Same(data.OpenCloudQueueCommand, cloudQueueCard.Command);

                var cloudStatusLine = ((StackPanel)cloudQueueCard.Content).Children
                    .OfType<TextBlock>()
                    .Last();
                var cloudStatusText = string.Concat(cloudStatusLine.Inlines
                    .OfType<System.Windows.Documents.Run>()
                    .Select(run => run.Text));
                Assert.Contains("已上传 2 · 已校验 3", cloudStatusText);

                // Invoke the framework's protected click path so ButtonBase performs its
                // normal CanExecute/Execute handling rather than calling the command directly.
                typeof(ButtonBase).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(cloudQueueCard, Array.Empty<object>());
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });

        thread.SetApartmentState(System.Threading.ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
        Assert.Equal(1, cloudQueueClicks);
    }

    [Fact]
    public void OverviewLatestBackupRendersRelativeTextAndFullAutomationEvidence()
    {
        Exception? exception = null;
        var thread = new System.Threading.Thread(() =>
        {
            try
            {
                var data = new OverviewTimeData();
                var overview = new OverviewView
                {
                    DataContext = data,
                    Width = 1280,
                    Height = 820
                };
                var host = new Grid
                {
                    Width = 1280,
                    Height = 820
                };
                host.Children.Add(overview);

                host.Measure(new Size(1280, 820));
                host.Arrange(new Rect(0, 0, 1280, 820));
                overview.UpdateLayout();

                var latestBackup = FindVisualDescendants<TextBlock>(overview)
                    .Single(textBlock => textBlock.Text == data.SelectedGameLastBackupRelativeDisplay);

                Assert.Equal(data.SelectedGameLastBackupFullDisplay, latestBackup.ToolTip);
                Assert.Equal(data.SelectedGameLastBackupFullDisplay, AutomationProperties.GetHelpText(latestBackup));

                var snapshotScope = FindVisualDescendants<TextBlock>(overview)
                    .Single(textBlock => textBlock.Text == data.OverviewSnapshotScopeDisplay);

                Assert.Equal(data.OverviewSnapshotUpdatedDisplay, snapshotScope.ToolTip);
                Assert.Equal(data.OverviewSnapshotUpdatedDisplay, AutomationProperties.GetHelpText(snapshotScope));
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });

        thread.SetApartmentState(System.Threading.ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
    }

    private static List<T> FindVisualDescendants<T>(DependencyObject root)
        where T : DependencyObject
    {
        var matches = new List<T>();
        Visit(root, matches);
        return matches;
    }

    private static void Visit<T>(DependencyObject? current, List<T> matches)
        where T : DependencyObject
    {
        if (current == null)
        {
            return;
        }

        if (current is T match)
        {
            matches.Add(match);
        }

        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(current); index++)
        {
            Visit(VisualTreeHelper.GetChild(current, index), matches);
        }
    }

    private sealed class OverviewInteractionData
    {
        public OverviewInteractionData(Action onCloudQueueClick)
        {
            OpenCloudQueueCommand = new CountingCommand(onCloudQueueClick);
            OpenActivityCommand = new CountingCommand(() => { });
            RefreshCommand = new CountingCommand(() => { });
        }

        public DashboardSnapshotDto Snapshot { get; } = new();
        public ObservableCollection<ActivityEntryDto> Activities { get; } = new();
        public ICommand OpenCloudQueueCommand { get; }
        public ICommand OpenActivityCommand { get; }
        public ICommand RefreshCommand { get; }
    }

    private sealed class OverviewTimeData
    {
        public DashboardSnapshotDto Snapshot { get; } = new()
        {
            ManagedGames = 1,
            MatchedGames = 1,
            CloudTransfers = new CloudTransferSummaryDto()
        };

        public bool IsDashboardSnapshotLoaded => true;
        public string OverviewSnapshotScopeDisplay => "全库 · Playnite 游戏库 · 更新于 刚刚";
        public string OverviewSnapshotUpdatedDisplay => "更新于 2026-09-21 12:34:56 (UTC+08:00) · 2026-09-21T04:34:56.0000000Z";
        public GameStatusDto SelectedGame { get; } = new()
        {
            Name = "时间证据游戏",
            Platform = GamePlatformKind.Steam,
            HealthState = "Healthy",
            LastBackupUtc = DateTime.UtcNow.AddHours(-3)
        };
        public string OverviewCurrentGameScopeDisplay => "当前游戏 · 与全库快照同步";
        public string SelectedGameBackupVersionDisplay => "1";
        public string SelectedGameMediaCountDisplay => "0 项";
        public string SelectedGameCloudStateDisplay => "未启用";
        public string SelectedGameLastBackupRelativeDisplay => "3 小时前";
        public string SelectedGameLastBackupFullDisplay => "2026-09-21 01:00:00 (UTC+08:00) · 2026-09-20T17:00:00.0000000Z";
        public ObservableCollection<ActivityEntryDto> Activities { get; } = new();
        public ObservableCollection<TaskStatusDto> OverviewTasks { get; } = new();
        public ICommand RefreshCommand { get; } = new CountingCommand(() => { });
        public ICommand OpenCloudQueueCommand { get; } = new CountingCommand(() => { });
    }

    private sealed class OverviewPriorityInteractionData
    {
        private readonly DashboardViewModel viewModel;

        public OverviewPriorityInteractionData(DashboardViewModel viewModel) => this.viewModel = viewModel;

        public DashboardSnapshotDto Snapshot => viewModel.Snapshot;
        public string OverviewPriorityKind => viewModel.OverviewPriorityKind;
        public string OverviewPriorityTitle => viewModel.OverviewPriorityTitle;
        public string OverviewPriorityDescription => viewModel.OverviewPriorityDescription;
        public string OverviewPriorityActionText => viewModel.OverviewPriorityActionText;
        public string OverviewPriorityActionToolTip => viewModel.OverviewPriorityActionToolTip;
        public ICommand OverviewPriorityActionCommand => viewModel.OverviewPriorityActionCommand;
        public ObservableCollection<ActivityEntryDto> Activities { get; } = new();
        public ObservableCollection<TaskStatusDto> OverviewTasks { get; } = new();
        public ICommand RefreshCommand { get; } = new CountingCommand(() => { });
        public ICommand OpenCloudQueueCommand { get; } = new CountingCommand(() => { });
        public ICommand OpenActivityCommand { get; } = new CountingCommand(() => { });
    }

    private static void SetPrivateField(object target, string fieldName, object? value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);
        field!.SetValue(target, value);
    }

    private static void SetBackingField(object target, string propertyName, object? value)
        => SetPrivateField(target, $"<{propertyName}>k__BackingField", value);

    private sealed class CountingCommand : ICommand
    {
        private readonly Action execute;

        public CountingCommand(Action execute) => this.execute = execute;

        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => execute();
    }
}
