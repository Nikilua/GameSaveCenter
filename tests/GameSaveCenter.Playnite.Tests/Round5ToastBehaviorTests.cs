using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using GameSaveCenter.Playnite;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("NotificationInboxWpf")]
public sealed class Round5ToastBehaviorTests
{
    [Fact]
    public void LongMessageHasFullDetailAndKeyboardFocusPausesItsTimer()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                var plugin = (GameSaveCenterPlugin)FormatterServices.GetUninitializedObject(typeof(GameSaveCenterPlugin));
                var dashboard = new DashboardView(plugin, () => false, useMinimalDialogBehaviorFixture: true);
                var root = (Grid)dashboard.Content;
                var toastHost = new StackPanel();
                root.Children.Insert(0, toastHost);
                typeof(DashboardView).GetField("ToastHost", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(dashboard, toastHost);
                var production = new NotificationInboxView();
                foreach (var key in new[] { "GscRedesignFeedbackToastCard", "GscRedesignFeedbackToastTitle", "GscRedesignFeedbackToastMessage", "GscButtonBase" })
                    dashboard.Resources[key] = production.FindResource(key);
                var window = new Window { Content = dashboard, Width = 800, Height = 600, ShowInTaskbar = false, WindowStyle = WindowStyle.None, Opacity = 0.01 };
                try
                {
                    window.Show();
                    var message = string.Concat(Enumerable.Repeat("合成的长通知内容", 40));
                    typeof(DashboardView).GetMethod("ShowToast", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(dashboard, new object[] { "通知", message, UiNotificationKind.Information, message, false });
                    window.UpdateLayout();
                    var card = Assert.IsAssignableFrom<Border>(Assert.Single(toastHost.Children.OfType<Border>()));
                    var grid = Assert.IsType<Grid>(card.Child);
                    var text = Assert.IsType<StackPanel>(grid.Children.OfType<StackPanel>().Single());
                    var details = Assert.Single(text.Children.OfType<Button>());
                    var close = Assert.Single(grid.Children.OfType<Button>());
                    Assert.Equal("查看通知完整详情", AutomationProperties.GetName(details));
                    Assert.Equal("关闭通知提示", AutomationProperties.GetName(close));
                    var timers = (Dictionary<Border, DispatcherTimer>)typeof(DashboardView).GetField("toastTimers", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(dashboard)!;
                    var timer = timers[card];
                    Assert.True(timer.IsEnabled);
                    Assert.True(details.Focus());
                    Assert.Same(details, Keyboard.Focus(details));
                    Assert.True(card.IsKeyboardFocusWithin);
                    Assert.False(timer.IsEnabled);
                    details.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, details));
                    var dialogText = Assert.IsType<TextBlock>(dashboard.FindName("DialogMessageText"));
                    Assert.Equal(message, dialogText.Text);
                    close.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, close));
                    Assert.Empty(toastHost.Children.OfType<Border>());
                    Assert.False(timer.IsEnabled);
                }
                finally { window.Close(); }
            }
            catch (Exception ex) { failure = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(20)));
        if (failure != null) throw new Xunit.Sdk.XunitException(failure.ToString());
    }
}
