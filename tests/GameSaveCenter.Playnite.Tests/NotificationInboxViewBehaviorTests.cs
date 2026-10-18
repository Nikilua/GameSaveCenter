using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Automation;
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
                markSelected.GetBindingExpression(Button.CommandProperty)?.UpdateTarget();
                markVisible.GetBindingExpression(Button.CommandProperty)?.UpdateTarget();
                clearRead.GetBindingExpression(Button.CommandProperty)?.UpdateTarget();

                Assert.Equal(2, Grid.GetRow(actionBar));
                Assert.True(ReferenceEquals(data.MarkSelectedNotificationReadCommand, markSelected.Command), "Selected-read command binding");
                Assert.True(ReferenceEquals(data.MarkVisibleNotificationsReadCommand, markVisible.Command), "Visible-read command binding");
                Assert.True(ReferenceEquals(data.ClearReadNotificationRecordsCommand, clearRead.Command), "Clear-read command binding");
                Assert.Equal("标记所选通知为已读", AutomationProperties.GetName(markSelected));
                Assert.Contains("被筛除的通知保持原状态", AutomationProperties.GetHelpText(markVisible));
                Assert.Contains("不会删除任务、审计日志或恢复证据", AutomationProperties.GetHelpText(clearRead));

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
        public bool NotificationInboxHasNoFilterResults => false;
        public string NotificationInboxErrorMessage => string.Empty;
        public bool HasNotificationInboxError => false;
        public string NotificationInboxLoadStatus => string.Empty;
        public System.Windows.Input.ICommand RefreshNotificationInboxCommand { get; } = new RelayCommand(_ => { });
        public System.Windows.Input.ICommand MarkSelectedNotificationReadCommand { get; }
        public System.Windows.Input.ICommand MarkVisibleNotificationsReadCommand { get; }
        public System.Windows.Input.ICommand ClearReadNotificationRecordsCommand { get; }

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
