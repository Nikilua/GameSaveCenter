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
    public void ToastStackKeepsNewestFirstAndRetainsOrderWhileOneToastExits()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            var window = default(Window);
            DashboardView? dashboard = null;
            try
            {
                var plugin = (GameSaveCenterPlugin)FormatterServices.GetUninitializedObject(typeof(GameSaveCenterPlugin));
                dashboard = new DashboardView(plugin, () => true, useMinimalDialogBehaviorFixture: true);
                var root = (Grid)dashboard.Content;
                var toastHost = new StackPanel
                {
                    Width = 360,
                    MaxWidth = 360,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 78, 22, 0)
                };
                root.Children.Insert(0, toastHost);
                typeof(DashboardView).GetField("ToastHost", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(dashboard, toastHost);
                var production = new NotificationInboxView();
                foreach (var key in new[] { "GscRedesignFeedbackToastCard", "GscRedesignFeedbackToastTitle", "GscRedesignFeedbackToastMessage", "GscButtonBase" })
                    dashboard.Resources[key] = production.FindResource(key);
                window = new Window { Content = dashboard, Width = 800, Height = 600, ShowInTaskbar = false, WindowStyle = WindowStyle.None, Opacity = 0.01 };
                window.Show();

                var showToast = typeof(DashboardView).GetMethod("ShowToast", BindingFlags.Instance | BindingFlags.NonPublic)!;
                for (var index = 1; index <= 5; index++)
                    showToast.Invoke(dashboard, new object[] { $"通知 {index}", $"内容 {index}", UiNotificationKind.Information, $"详情 {index}", false });

                PumpDispatcherFor(TimeSpan.FromMilliseconds(400));
                window.UpdateLayout();
                var cards = toastHost.Children.OfType<Border>().ToArray();
                Assert.Equal(4, cards.Length);
                Assert.Equal(new[] { "通知 5", "通知 4", "通知 3", "通知 2" }, cards.Select(GetToastTitle));
                var timers = (Dictionary<Border, DispatcherTimer>)typeof(DashboardView).GetField("toastTimers", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(dashboard)!;
                Assert.Equal(4, timers.Count);
                Assert.DoesNotContain(cards, card => !timers.ContainsKey(card));

                var exiting = cards[1];
                var close = Assert.Single(((Grid)exiting.Child).Children.OfType<Button>());
                close.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, close));

                Assert.Equal(cards, toastHost.Children.OfType<Border>());
                Assert.False(timers.ContainsKey(exiting));
                PumpDispatcherFor(TimeSpan.FromMilliseconds(450));

                var remaining = toastHost.Children.OfType<Border>().ToArray();
                Assert.Equal(new[] { "通知 5", "通知 3", "通知 2" }, remaining.Select(GetToastTitle));
                Assert.Equal(3, timers.Count);
            }
            catch (Exception ex)
            {
                failure = ex;
            }
            finally
            {
                if (dashboard != null)
                    typeof(DashboardView).GetMethod("ClearToasts", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(dashboard, null);
                window?.Close();
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(20)));
        if (failure != null) throw new Xunit.Sdk.XunitException(failure.ToString());
    }

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
                    var title = string.Concat(Enumerable.Repeat("较长通知标题", 12));
                    typeof(DashboardView).GetMethod("ShowToast", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(dashboard, new object[] { title, message, UiNotificationKind.Information, message, false });
                    window.UpdateLayout();
                    var card = Assert.IsAssignableFrom<Border>(Assert.Single(toastHost.Children.OfType<Border>()));
                    var grid = Assert.IsType<Grid>(card.Child);
                    var text = Assert.IsType<StackPanel>(grid.Children.OfType<StackPanel>().Single());
                    var titleText = Assert.IsType<TextBlock>(text.Children.OfType<TextBlock>().First());
                    var details = Assert.Single(text.Children.OfType<Button>());
                    var close = Assert.Single(grid.Children.OfType<Button>());
                    Assert.Equal(TextWrapping.NoWrap, titleText.TextWrapping);
                    Assert.Equal(TextTrimming.CharacterEllipsis, titleText.TextTrimming);
                    Assert.Equal(title, titleText.ToolTip);
                    Assert.Contains(title, AutomationProperties.GetName(card));
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
                    Assert.Equal(title, Assert.IsType<TextBlock>(dashboard.FindName("DialogTitleText")).Text);
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

    private static string GetToastTitle(Border card)
        => Assert.IsType<TextBlock>(Assert.IsType<StackPanel>(Assert.IsType<Grid>(card.Child)
            .Children.OfType<StackPanel>().Single()).Children.OfType<TextBlock>().First()).Text;

    private static void PumpDispatcherFor(TimeSpan duration)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = duration
        };
        timer.Tick += (_, __) =>
        {
            timer.Stop();
            frame.Continue = false;
        };
        timer.Start();
        Dispatcher.PushFrame(frame);
    }
}
