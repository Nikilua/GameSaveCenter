using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class TaskRetryGuidanceViewTests
{
    [Fact]
    public void RetryGuidanceCardTracksTerminalTaskAndAutomationHelpText()
    {
        Exception? exception = null;
        var initialVisibility = Visibility.Visible;
        var failedVisibility = Visibility.Collapsed;
        var failedText = string.Empty;
        var retryHelpText = string.Empty;
        var cancelledVisibility = Visibility.Collapsed;
        var restoredVisibility = Visibility.Visible;

        var thread = new Thread(() =>
        {
            try
            {
                var context = new RetryGuidanceDataContext
                {
                    SelectedTask = new TaskStatusDto { TaskType = "CloudUpload", State = TaskState.Running }
                };
                var view = new TaskCenterView { DataContext = context };
                var window = new Window { Width = 1040, Height = 700, Content = view };
                window.Show();
                FlushLayout(window);

                var card = (Border)typeof(TaskCenterView)
                    .GetField("TaskRetryGuidanceCard", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(view)!;
                var actions = (WrapPanel)typeof(TaskCenterView)
                    .GetField("TaskDetailActions", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(view)!;
                var retry = Array.Find(actions.Children.OfType<Button>().ToArray(), button =>
                    AutomationProperties.GetName(button) == "安全重试任务")!;
                var guidanceText = (TextBlock)typeof(TaskCenterView)
                    .GetField("TaskRetryGuidanceText", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(view)!;
                initialVisibility = card.Visibility;

                const string permissionGuidance = "权限不足：检查目标目录授权；结果未确认前不要盲目重试。";
                context.SelectedTask = new TaskStatusDto { TaskType = "CloudUpload", State = TaskState.Failed };
                context.SelectedTaskRetryGuidance = permissionGuidance;
                FlushLayout(window);
                failedVisibility = card.Visibility;
                failedText = guidanceText.Text;
                retryHelpText = AutomationProperties.GetHelpText(retry);

                context.SelectedTask = new TaskStatusDto { TaskType = "CloudUpload", State = TaskState.Cancelled };
                context.SelectedTaskRetryGuidance = "云端状态尚未确认。请核对远端后再决定是否重试。";
                FlushLayout(window);
                cancelledVisibility = card.Visibility;

                context.SelectedTask = new TaskStatusDto { TaskType = "CloudUpload", State = TaskState.Succeeded };
                FlushLayout(window);
                restoredVisibility = card.Visibility;

                window.Close();
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        Assert.Null(exception);
        Assert.Equal(Visibility.Collapsed, initialVisibility);
        Assert.Equal(Visibility.Visible, failedVisibility);
        Assert.Contains("权限不足", failedText);
        Assert.Contains("不要盲目重试", retryHelpText);
        Assert.Equal(Visibility.Visible, cancelledVisibility);
        Assert.Equal(Visibility.Collapsed, restoredVisibility);
    }

    private static void FlushLayout(Window window)
    {
        window.UpdateLayout();
        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
        window.UpdateLayout();
    }

    private sealed class RetryGuidanceDataContext : INotifyPropertyChanged
    {
        private TaskStatusDto selectedTask = new();
        private string selectedTaskRetryGuidance = string.Empty;

        public TaskStatusDto SelectedTask
        {
            get => selectedTask;
            set
            {
                selectedTask = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedTask)));
            }
        }

        public string SelectedTaskRetryGuidance
        {
            get => selectedTaskRetryGuidance;
            set
            {
                selectedTaskRetryGuidance = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedTaskRetryGuidance)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
