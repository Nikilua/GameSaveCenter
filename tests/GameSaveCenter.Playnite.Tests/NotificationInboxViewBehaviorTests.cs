using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
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

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                _ = new Application();
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
    }
}

[CollectionDefinition("NotificationInboxWpf", DisableParallelization = true)]
public sealed class NotificationInboxWpfCollection
{
}
