using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R21AsyncCompletionAnnouncementBehaviorTests
{
    [Fact]
    public void TerminalToastIsReadableWithoutTakingFocus()
    {
        Exception? exception = null;
        string? name = null;
        string? helpText = null;
        IInputElement? focusedBefore = null;
        IInputElement? focusedAfter = null;
        var focusable = true;

        RunSta(() =>
        {
            try
            {
                var input = new TextBox { Text = "保持输入" };
                var toast = new GameSaveCenter.Playnite.Controls.FeedbackToast();
                var root = new StackPanel();
                root.Children.Add(input);
                root.Children.Add(toast);
                var window = Show(root, 520, 220);
                try
                {
                    input.Focus();
                    Keyboard.Focus(input);
                    PumpLayout(window);
                    focusedBefore = Keyboard.FocusedElement;

                    DashboardView.ApplyToastAutomation(toast, "后台任务完成", "合成游戏 · 存档备份已完成");
                    PumpLayout(window);

                    focusedAfter = Keyboard.FocusedElement;
                    name = AutomationProperties.GetName(toast);
                    helpText = AutomationProperties.GetHelpText(toast);
                    focusable = toast.Focusable;
                }
                finally
                {
                    window.Close();
                }
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });

        Assert.Null(exception);
        Assert.Same(focusedBefore, focusedAfter);
        Assert.False(focusable);
        Assert.Equal("后台任务完成：合成游戏 · 存档备份已完成", name);
        Assert.Contains("最终状态以任务中心记录为准", helpText);
    }

    [Fact]
    public void TaskPageLoadingCompletionReplacesTheReadableStatusSurface()
    {
        Exception? exception = null;
        string? loadingText = null;
        string? completedText = null;

        RunSta(() =>
        {
            try
            {
                var state = new TaskPageStatusContext();
                var view = new TaskCenterView { DataContext = state };
                var window = Show(view, 1000, 700);
                try
                {
                    view.ApplyResponsiveLayout(1000, 700);
                    PumpLayout(window);
                    var summary = (TextBlock)view.FindName("TaskQueueLastUpdatedSummary")!;
                    loadingText = summary.Text;

                    state.TaskPageStatusSummary = "最近更新：2026-09-21 12:34:56";
                    state.Raise(nameof(TaskPageStatusContext.TaskPageStatusSummary));
                    PumpLayout(window);
                    completedText = summary.Text;
                }
                finally
                {
                    window.Close();
                }
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });

        Assert.Null(exception);
        Assert.Equal("正在加载任务记录…", loadingText);
        Assert.Equal("最近更新：2026-09-21 12:34:56", completedText);
    }

    private static Window Show(FrameworkElement content, double width, double height)
    {
        var window = new Window
        {
            Width = width,
            Height = height,
            WindowStyle = WindowStyle.None,
            ShowInTaskbar = false,
            ShowActivated = false,
            Content = content
        };
        window.Show();
        return window;
    }

    private static void PumpLayout(Window window)
    {
        window.UpdateLayout();
        window.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.Background,
            new Action(() => { }));
        window.UpdateLayout();
    }

    private static void RunSta(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception caught)
            {
                exception = caught;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (exception != null)
            throw new Xunit.Sdk.XunitException(exception.ToString());
    }

    private sealed class TaskPageStatusContext : INotifyPropertyChanged
    {
        public ObservableCollection<object> Items { get; } = new ObservableCollection<object>();

        public ICollectionView TasksView { get; }

        public string TaskPageStatusSummary { get; set; } = "正在加载任务记录…";

        public bool IsTaskPageLoading => true;
        public bool TaskPageLoadFailed => false;
        public bool TaskPageHasItems => Items.Count > 0;
        public string TaskPageState => "Loading";
        public string TaskPageErrorMessage => string.Empty;
        public string TaskPageLastUpdatedDisplay => "未知";
        public bool TaskHasActiveFilters => false;
        public string TaskActiveFiltersSummary => string.Empty;
        public ICommand RefreshCommand { get; } = new TestCommand();

        public TaskPageStatusContext()
        {
            TasksView = new ListCollectionView((IList)Items);
        }

        public void Raise(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private sealed class TestCommand : ICommand
    {
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) { }
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
