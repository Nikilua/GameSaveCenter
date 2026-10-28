using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Automation;
using System.Windows.Input;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("NotificationInboxWpf")]
public sealed class NotificationInboxViewBehaviorTests
{
    [Fact]
    public void ClearFiltersResetsOnlySearchAndFilterSelections()
    {
        RunSta(() =>
        {
            var data = new NotificationInboxData { NotificationInboxHasNoFilterResults = true };
            var view = new NotificationInboxView { DataContext = data };
            var window = new Window { Content = view, Width = 900, Height = 600, ShowInTaskbar = false, WindowStyle = WindowStyle.None, Opacity = 0.01 };
            try
            {
                window.Show();
                window.UpdateLayout();
                var search = Assert.IsType<TextBox>(view.FindName("NotificationInboxSearchTextBox"));
                var severity = Assert.IsType<ComboBox>(view.FindName("NotificationSeverityFilterComboBox"));
                var time = Assert.IsType<ComboBox>(view.FindName("NotificationTimeFilterComboBox"));
                var clear = Assert.IsAssignableFrom<Button>(view.FindName("NotificationClearFiltersButton"));
                search.Text = "合成查询";
                severity.SelectedIndex = 1;
                time.SelectedIndex = 1;
                Assert.True(clear.Focus());
                Keyboard.Focus(clear);
                Assert.Same(clear, Keyboard.FocusedElement);
                clear.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, clear));
                Assert.Equal(string.Empty, search.Text);
                Assert.Equal(0, severity.SelectedIndex);
                Assert.Equal(0, time.SelectedIndex);
                Assert.Same(search, Keyboard.FocusedElement);
                Assert.Equal(string.Empty, data.NotificationInboxSearchText);
                Assert.Equal("全部级别", data.NotificationSeverityFilter);
                Assert.Equal("全部时间", data.NotificationTimeFilter);
                Assert.Contains("不会修改通知记录或已读状态", AutomationProperties.GetHelpText(clear));
                Assert.Equal(0, data.MarkSelectedExecutions + data.MarkVisibleExecutions + data.ClearReadExecutions);
            }
            finally { window.Close(); }
        });
    }

    [Fact]
    public void NewNotificationDoesNotTakeKeyboardFocusFromSearch()
    {
        RunSta(() =>
        {
            var data = new NotificationInboxData();
            var selected = new NotificationInboxItem(new NotificationRecordDto
            {
                NotificationId = "notice-existing",
                Title = "当前通知",
                Summary = "已有通知摘要"
            });
            data.NotificationInboxItems.Add(selected);
            data.SelectedNotification = selected;

            var view = new NotificationInboxView { DataContext = data };
            var window = new Window
            {
                Content = view,
                Width = 900,
                Height = 600,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };
            try
            {
                window.Show();
                window.UpdateLayout();
                var search = Assert.IsType<TextBox>(view.FindName("NotificationInboxSearchTextBox"));
                var grid = Assert.IsType<DataGrid>(view.FindName("NotificationGrid"));
                grid.UpdateLayout();
                Assert.True(search.Focus());
                Keyboard.Focus(search);
                Assert.Same(search, Keyboard.FocusedElement);
                Assert.Same(selected, grid.SelectedItem);

                data.NotificationInboxItems.Insert(0, new NotificationInboxItem(new NotificationRecordDto
                {
                    NotificationId = "notice-new",
                    Title = "新到通知",
                    Summary = "后台增加的通知摘要"
                }));
                window.Dispatcher.Invoke(DispatcherPriority.DataBind, new Action(() => { }));
                window.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
                grid.UpdateLayout();

                Assert.Same(search, Keyboard.FocusedElement);
                Assert.Equal("notice-new", data.NotificationInboxItems[0].NotificationId);
                Assert.Same(selected, grid.SelectedItem);
                Assert.Equal("notice-existing", ((NotificationInboxItem)grid.SelectedItem!).NotificationId);
            }
            finally { window.Close(); }
        });
    }

    [Fact]
    public void CompactLayoutKeepsListOpenAndMakesDetailsAnExplicitToggle()
    {
        RunSta(() =>
        {
            var view = new NotificationInboxView { DataContext = new NotificationInboxData() };
            view.ApplyResponsiveLayout(744, 460);
            var details = Assert.IsType<Border>(view.FindName("NotificationInspectorCard"));
            var button = Assert.IsAssignableFrom<Button>(view.FindName("NotificationCompactDetailsButton"));
            var list = Assert.IsType<Border>(view.FindName("NotificationTableFrame"));
            var detailsRow = Assert.IsType<RowDefinition>(view.FindName("NotificationCompactInspectorRow"));

            Assert.Equal(Visibility.Collapsed, details.Visibility);
            Assert.Equal(Visibility.Visible, button.Visibility);
            Assert.Equal(1, Grid.GetRowSpan(list));

            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, button));
            Assert.Equal(Visibility.Visible, details.Visibility);
            Assert.Equal(150, detailsRow.Height.Value);
            Assert.Equal("收起详情", button.Content);

            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, button));
            Assert.Equal(Visibility.Collapsed, details.Visibility);
            Assert.Equal("查看详情", button.Content);
        });
    }

    [Fact]
    public void ReadActionsUseBoundCommandsAndExposeTheirSafetyScopeToAutomation()
    {
        RunSta(() =>
        {
            var data = new NotificationInboxData();
            data.SelectedNotification = new NotificationInboxItem(new NotificationRecordDto
            {
                NotificationId = "notice-1",
                IssueKey = "issue-1",
                OccurrenceCount = 2,
                GameId = "playnite-game-1",
                TaskId = "task-1",
                FirstTaskId = "task-original",
                Title = "任务失败",
                Summary = "保留的通知摘要"
            });
            var view = new NotificationInboxView { DataContext = data };
            var window = new Window
            {
                Content = view,
                Width = 744,
                Height = 460,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                Opacity = 0.01
            };

            try
            {
                window.Show();
                window.UpdateLayout();
                window.Dispatcher.Invoke(DispatcherPriority.DataBind, new Action(() => { }));
                view.ApplyResponsiveLayout(744, 460);

                var actionBar = Assert.IsType<WrapPanel>(view.FindName("NotificationReadActions"));
                var markSelected = Assert.IsAssignableFrom<Button>(view.FindName("NotificationMarkSelectedReadButton"));
                var markVisible = Assert.IsAssignableFrom<Button>(view.FindName("NotificationMarkVisibleReadButton"));
                var clearRead = Assert.IsAssignableFrom<Button>(view.FindName("NotificationClearReadButton"));
                var noRecords = Assert.IsType<Border>(view.FindName("NotificationNoRecordsEmptyState"));
                var noResults = Assert.IsType<Border>(view.FindName("NotificationNoResultsEmptyState"));
                var openGame = Assert.IsAssignableFrom<Button>(view.FindName("NotificationOpenGameButton"));
                var openTask = Assert.IsAssignableFrom<Button>(view.FindName("NotificationOpenTaskButton"));
                var openFirstTask = Assert.IsAssignableFrom<Button>(view.FindName("NotificationOpenFirstTaskButton"));
                var detailTime = Assert.IsType<TextBlock>(view.FindName("NotificationDetailTime"));
                detailTime.GetBindingExpression(TextBlock.TextProperty)?.UpdateTarget();
                BindingOperations.GetBindingExpression(detailTime, AutomationProperties.HelpTextProperty)?.UpdateTarget();
                Assert.Equal(data.SelectedNotification!.CreatedRelativeDisplay, detailTime.Text);
                Assert.Equal(data.SelectedNotification.CreatedFullDisplay, AutomationProperties.GetHelpText(detailTime));
                markSelected.GetBindingExpression(Button.CommandProperty)?.UpdateTarget();
                markVisible.GetBindingExpression(Button.CommandProperty)?.UpdateTarget();
                clearRead.GetBindingExpression(Button.CommandProperty)?.UpdateTarget();
                openGame.GetBindingExpression(Button.CommandProperty)?.UpdateTarget();
                openTask.GetBindingExpression(Button.CommandProperty)?.UpdateTarget();
                openFirstTask.GetBindingExpression(Button.CommandProperty)?.UpdateTarget();

                Assert.Equal(2, Grid.GetRow(actionBar));
                Assert.Equal(markSelected.Margin, markVisible.Margin);
                Assert.Equal(markVisible.Margin, clearRead.Margin);
                Assert.Equal(markSelected.MinHeight, markVisible.MinHeight);
                Assert.Equal(markVisible.MinHeight, clearRead.MinHeight);
                Assert.Equal(VerticalAlignment.Center, markSelected.VerticalAlignment);
                Assert.Equal(VerticalAlignment.Center, markVisible.VerticalAlignment);
                Assert.Equal(VerticalAlignment.Center, clearRead.VerticalAlignment);
                Assert.Equal(1, Grid.GetColumnSpan(noRecords));
                Assert.Equal(1, Grid.GetColumnSpan(noResults));
                Assert.Same(view.FindResource("GscRedesignEmptyStateSurface"), noRecords.Style);
                Assert.Same(view.FindResource("GscRedesignEmptyStateSurface"), noResults.Style);
                Assert.True(ReferenceEquals(data.MarkSelectedNotificationReadCommand, markSelected.Command), "Selected-read command binding");
                Assert.True(ReferenceEquals(data.MarkVisibleNotificationsReadCommand, markVisible.Command), "Visible-read command binding");
                Assert.True(ReferenceEquals(data.ClearReadNotificationRecordsCommand, clearRead.Command), "Clear-read command binding");
                Assert.True(ReferenceEquals(data.OpenSelectedNotificationGameCommand, openGame.Command), "Open-game command binding");
                Assert.True(ReferenceEquals(data.OpenSelectedNotificationTaskCommand, openTask.Command), "Open-task command binding");
                Assert.True(ReferenceEquals(data.OpenSelectedNotificationFirstTaskCommand, openFirstTask.Command), "Open-first-task command binding");
                Assert.Equal("标记所选通知为已读", AutomationProperties.GetName(markSelected));
                Assert.Contains("被筛除的通知保持原状态", AutomationProperties.GetHelpText(markVisible));
                Assert.Contains("不会删除任务、审计日志或恢复证据", AutomationProperties.GetHelpText(clearRead));
                Assert.Contains("稳定游戏 ID", AutomationProperties.GetHelpText(openGame));
                Assert.Contains("任务稳定 ID", AutomationProperties.GetHelpText(openTask));
                Assert.Equal(Visibility.Visible, openGame.Visibility);
                Assert.Equal(Visibility.Visible, openTask.Visibility);
                Assert.Equal(Visibility.Visible, openFirstTask.Visibility);

                markSelected.Command.Execute(null);
                markVisible.Command.Execute(null);
                clearRead.Command.Execute(null);
                Assert.Equal(1, data.MarkSelectedExecutions);
                Assert.Equal(1, data.MarkVisibleExecutions);
                Assert.Equal(1, data.ClearReadExecutions);
            }
            finally
            {
                window.Close();
            }
        });
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
        if (failure != null)
            throw new Xunit.Sdk.XunitException(failure.ToString());
    }

    private sealed class NotificationInboxData
    {
        public int MarkSelectedExecutions { get; private set; }
        public int MarkVisibleExecutions { get; private set; }
        public int ClearReadExecutions { get; private set; }
        public ObservableCollection<NotificationInboxItem> NotificationInboxItems { get; } = new ObservableCollection<NotificationInboxItem>();
        public string[] NotificationSeverityOptions { get; } = new[] { "全部级别", "错误", "警告", "成功", "信息" };
        public string[] NotificationTimeOptions { get; } = new[] { "全部时间", "最近24小时", "最近7天", "最近30天" };
        public NotificationInboxItem? SelectedNotification { get; set; }
        public string NotificationSeverityFilter { get; set; } = "全部级别";
        public string NotificationTimeFilter { get; set; } = "全部时间";
        public string NotificationInboxSearchText { get; set; } = string.Empty;
        public int NotificationUnreadCount => 0;
        public string NotificationInboxCountSummary => "最近 30 天 · 暂无记录";
        public bool NotificationInboxHasNoRecords => true;
        public bool NotificationInboxHasNoFilterResults { get; set; }
        public string NotificationInboxErrorMessage => string.Empty;
        public bool HasNotificationInboxError => false;
        public string NotificationInboxLoadStatus => string.Empty;
        public bool HasSelectedNotificationGameId => !string.IsNullOrWhiteSpace(SelectedNotification?.GameId);
        public bool HasSelectedNotificationTaskId => !string.IsNullOrWhiteSpace(SelectedNotification?.TaskId);
        public bool HasSelectedNotificationFirstTaskId => SelectedNotification?.HasIssue == true
            && !string.IsNullOrWhiteSpace(SelectedNotification.FirstTaskId)
            && !string.Equals(SelectedNotification.FirstTaskId, SelectedNotification.TaskId, StringComparison.Ordinal);
        public string NotificationContextMessage => "这条通知保留了摘要。";
        public System.Windows.Input.ICommand RefreshNotificationInboxCommand { get; } = new RelayCommand(_ => { });
        public System.Windows.Input.ICommand MarkSelectedNotificationReadCommand { get; }
        public System.Windows.Input.ICommand MarkVisibleNotificationsReadCommand { get; }
        public System.Windows.Input.ICommand ClearReadNotificationRecordsCommand { get; }
        public System.Windows.Input.ICommand OpenSelectedNotificationGameCommand { get; } = new RelayCommand(_ => { });
        public System.Windows.Input.ICommand OpenSelectedNotificationTaskCommand { get; } = new RelayCommand(_ => { });
        public System.Windows.Input.ICommand OpenSelectedNotificationFirstTaskCommand { get; } = new RelayCommand(_ => { });

        public NotificationInboxData()
        {
            MarkSelectedNotificationReadCommand = new RelayCommand(_ => MarkSelectedExecutions++);
            MarkVisibleNotificationsReadCommand = new RelayCommand(_ => MarkVisibleExecutions++);
            ClearReadNotificationRecordsCommand = new RelayCommand(_ => ClearReadExecutions++);
        }
    }
}

[CollectionDefinition("NotificationInboxWpf", DisableParallelization = true)]
public sealed class NotificationInboxWpfCollection
{
}
