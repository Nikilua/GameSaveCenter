using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R10FilterSourceBehaviorTests
{
    [Fact]
    public void SourceBannerShowsOnlyForAnActiveContextAndBoundClearKeepsDraftFilters()
    {
        RunSta(() =>
        {
            var view = new TaskCenterView();
            var binding = new TestNavigationBinding
            {
                HasTaskNavigationTarget = true,
                TaskNavigationSourceSummary = "已带入游戏“合成游戏”的诊断条件。"
            };
            view.DataContext = binding;

            using var host = new WindowHost(view);
            host.Window.UpdateLayout();

            var banner = (Border)view.FindName("TaskNavigationSourceBanner")!;
            var clear = FindButton(banner, "清除带入的任务筛选");
            Assert.Equal(Visibility.Visible, banner.Visibility);
            var sourceText = (TextBlock)view.FindName("TaskNavigationSourceText")!;
            Assert.Contains("合成游戏", sourceText.Text, StringComparison.Ordinal);
            Assert.True(clear.Command!.CanExecute(null));

            clear.Command.Execute(null);

            Assert.Equal(1, binding.ClearCount);
            Assert.Equal("用户正在编辑的搜索草稿", binding.TaskSearchText);
            Assert.Equal("失败", binding.TaskStatusFilter);

            binding.HasTaskNavigationTarget = false;
            binding.NotifyNavigationVisibility();
            host.Window.UpdateLayout();
            Assert.Equal(Visibility.Collapsed, banner.Visibility);
            Assert.False(clear.Command.CanExecute(null));
        });
    }

    [Fact]
    public void SourceClearImplementationTouchesOnlyTransientNavigationFields()
    {
        var root = TestRepositoryContext.Root;
        var implementation = System.IO.File.ReadAllText(System.IO.Path.Combine(
            root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.Navigation.cs"));
        var xaml = System.IO.File.ReadAllText(System.IO.Path.Combine(
            root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));
        var start = implementation.IndexOf("private void ClearTaskNavigationContext", StringComparison.Ordinal);
        var end = implementation.IndexOf("internal void SetTaskGridScrollOffset", start, StringComparison.Ordinal);
        Assert.True(start >= 0 && end > start);

        var clearBody = implementation.Substring(start, end - start);
        Assert.Contains("taskNavigationGameId = string.Empty", clearBody, StringComparison.Ordinal);
        Assert.Contains("taskNavigationGameName = string.Empty", clearBody, StringComparison.Ordinal);
        Assert.DoesNotContain("taskSearchText =", clearBody, StringComparison.Ordinal);
        Assert.DoesNotContain("taskStatusFilter =", clearBody, StringComparison.Ordinal);
        Assert.DoesNotContain("taskGameFilter =", clearBody, StringComparison.Ordinal);
        Assert.DoesNotContain("taskTypeFilter =", clearBody, StringComparison.Ordinal);
        Assert.Contains("ClearTaskNavigationContextCommand", xaml, StringComparison.Ordinal);
        Assert.Contains("仅清除带入的游戏条件，保留其他任务筛选", xaml, StringComparison.Ordinal);
    }

    private static Button FindButton(DependencyObject root, string automationName)
    {
        for (var index = 0; index < System.Windows.Media.VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(root, index);
            if (child is Button button
                && string.Equals(AutomationProperties.GetName(button), automationName, StringComparison.Ordinal))
                return button;
            var nested = FindButtonOrNull(child, automationName);
            if (nested != null) return nested;
        }

        throw new InvalidOperationException($"未找到按钮：{automationName}");
    }

    private static Button? FindButtonOrNull(DependencyObject root, string automationName)
    {
        if (root is Button button
            && string.Equals(AutomationProperties.GetName(button), automationName, StringComparison.Ordinal))
            return button;
        for (var index = 0; index < System.Windows.Media.VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var nested = FindButtonOrNull(System.Windows.Media.VisualTreeHelper.GetChild(root, index), automationName);
            if (nested != null) return nested;
        }

        return null;
    }

    private sealed class TestNavigationBinding : INotifyPropertyChanged
    {
        public bool HasTaskNavigationTarget { get; set; }
        public string TaskNavigationSourceSummary { get; set; } = string.Empty;
        public string TaskSearchText { get; set; } = "用户正在编辑的搜索草稿";
        public string TaskStatusFilter { get; set; } = "失败";
        public string TaskGameFilter { get; set; } = "全部";
        public string TaskTypeFilter { get; set; } = "全部";
        public string TaskHistoryScope { get; set; } = "最近任务";
        public string TaskHistoryRange { get; set; } = "全部时间";
        public int ClearCount { get; private set; }
        public ICommand ClearTaskNavigationContextCommand { get; }

        public TestNavigationBinding()
        {
            ClearTaskNavigationContextCommand = new RelayCommand(_ =>
            {
                ClearCount++;
                HasTaskNavigationTarget = false;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasTaskNavigationTarget)));
            }, _ => HasTaskNavigationTarget);
        }

        public void NotifyNavigationVisibility()
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasTaskNavigationTarget)));

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private sealed class WindowHost : IDisposable
    {
        public WindowHost(FrameworkElement content)
        {
            Window = new Window
            {
                Content = content,
                Width = 1000,
                Height = 720,
                ShowInTaskbar = false,
                ShowActivated = false,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            };
            Window.Show();
            Window.UpdateLayout();
        }

        public Window Window { get; }

        public void Dispose()
        {
            Window.Close();
            Dispatcher.CurrentDispatcher.InvokeShutdown();
        }
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
        thread.Join();
        if (failure != null) throw new Xunit.Sdk.XunitException(failure.ToString());
    }
}
