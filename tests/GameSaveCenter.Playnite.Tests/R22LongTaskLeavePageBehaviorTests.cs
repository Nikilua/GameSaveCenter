using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Threading;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R22LongTaskLeavePageBehaviorTests
{
    [Fact]
    public void TaskPageShowsTheNonCancellingLeaveAndResumeContract()
    {
        RunSta(() =>
        {
            var view = new TaskCenterView();
            var hint = (TextBlock)typeof(TaskCenterView)
                .GetField("TaskLeavePageHint", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(view)!;
            using var host = new WindowHost(view);

            Assert.Equal(
                "离开本页不会取消后台任务；返回任务中心后会从任务记录恢复阶段和进度。",
                hint.Text);
            Assert.Equal("后台任务离页说明", AutomationProperties.GetName(hint));
            Assert.Equal(hint.Text, AutomationProperties.GetHelpText(hint));
            Assert.Equal(Visibility.Visible, hint.Visibility);
        });
    }

    [Fact]
    public void DashboardUnloadStopsPresentationListenersWithoutCancellingWorkerTasks()
    {
        var root = TestRepositoryContext.Root;
        var dashboard = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml.cs"));
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var taskPage = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));

        var unloadStart = dashboard.IndexOf("private void OnUnloaded", StringComparison.Ordinal);
        var unloadEnd = dashboard.IndexOf("internal GameSaveCenterPlugin PluginForAudit", unloadStart, StringComparison.Ordinal);
        Assert.True(unloadStart >= 0);
        Assert.True(unloadEnd > unloadStart);
        var unloadBody = dashboard.Substring(unloadStart, unloadEnd - unloadStart);
        Assert.Contains("StopTaskEventSubscription();", unloadBody);
        Assert.DoesNotContain("CancelTask", unloadBody, StringComparison.Ordinal);

        var subscriptionStart = viewModel.IndexOf("public void StartTaskEventSubscription()", StringComparison.Ordinal);
        var subscriptionEnd = viewModel.IndexOf("public void StopTaskEventSubscription()", subscriptionStart, StringComparison.Ordinal);
        Assert.True(subscriptionStart >= 0);
        Assert.True(subscriptionEnd > subscriptionStart);
        var subscriptionBody = viewModel.Substring(subscriptionStart, subscriptionEnd - subscriptionStart);
        Assert.Contains("ListenForTaskEventsWhenReadyAsync", subscriptionBody);
        Assert.Contains("Normal task polling remains active", viewModel);

        Assert.Contains("离开本页不会取消后台任务", taskPage);
        Assert.Contains("Command=\"{Binding CancelTaskCommand}\"", taskPage);
        Assert.Contains("Binding=\"{Binding StageDisplay, Mode=OneWay}\"", taskPage);
        Assert.Contains("Text=\"{Binding ProgressDisplay, Mode=OneWay}\"", taskPage);
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

    private sealed class WindowHost : IDisposable
    {
        public WindowHost(UserControl content)
        {
            Window = new Window
            {
                Content = content,
                Width = 1200,
                Height = 760,
                ShowInTaskbar = false,
                ShowActivated = false,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };
            Window.Show();
            Window.UpdateLayout();
        }

        private Window Window { get; }

        public void Dispose()
        {
            if (Window.IsVisible)
                Window.Close();
        }
    }
}
