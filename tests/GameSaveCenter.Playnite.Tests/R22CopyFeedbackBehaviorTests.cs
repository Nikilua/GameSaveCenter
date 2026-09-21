using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Controls;
using GameSaveCenter.Playnite.Infrastructure;

using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R22CopyFeedbackBehaviorTests
{
    [Fact]
    public void CopyFeedbackStaysNearTheGridReusesOnePopupAndAnnouncesFailureRetry()
    {
        RunSta(() =>
        {
            var row = new TaskStatusDto
            {
                TaskId = "copy-feedback-task",
                TaskType = "Backup",
                GameName = "合成游戏",
                State = TaskState.Succeeded,
                CreatedUtc = new DateTime(2026, 9, 21, 4, 0, 0, DateTimeKind.Utc),
                ProgressPercent = 100
            };
            var grid = new DataGrid
            {
                AutoGenerateColumns = false,
                CanUserAddRows = false,
                SelectionMode = DataGridSelectionMode.Extended
            };
            grid.Columns.Add(new DataGridTextColumn { Header = "游戏", Binding = new Binding("GameName") });
            grid.ItemsSource = new ObservableCollection<TaskStatusDto> { row };
            grid.SelectedItems.Add(row);
            DataGridClipboardBehavior.SetProfile(grid, "Task");

            var window = Show(grid);
            try
            {
                var widthBefore = grid.ActualWidth;
                var heightBefore = grid.ActualHeight;
                string? copied = null;
                DataGridClipboardBehavior.ClipboardSetterForVerification = text => copied = text;

                Assert.True(DataGridClipboardBehavior.TryCopyForVerification(grid, copyCell: false));
                var popup = ClipboardFeedback.GetPopupForVerification(grid);
                Assert.NotNull(popup);
                Assert.True(popup!.IsOpen);
                Assert.Same(grid, popup.PlacementTarget);
                Assert.Equal(widthBefore, grid.ActualWidth);
                Assert.Equal(heightBefore, grid.ActualHeight);
                Assert.Contains("所选行已复制", ClipboardFeedback.GetMessageForVerification(grid), StringComparison.Ordinal);
                Assert.NotNull(copied);
                Assert.Contains("合成游戏", copied!, StringComparison.Ordinal);

                Assert.True(DataGridClipboardBehavior.TryCopyForVerification(grid, copyCell: false));
                Assert.Same(popup, ClipboardFeedback.GetPopupForVerification(grid));
                Assert.True(popup.IsOpen);

                DataGridClipboardBehavior.ClipboardSetterForVerification = _ => throw new InvalidOperationException("clipboard busy");
                Assert.False(DataGridClipboardBehavior.TryCopyForVerification(grid, copyCell: false));
                Assert.Same(popup, ClipboardFeedback.GetPopupForVerification(grid));
                Assert.Contains("请稍后重试", ClipboardFeedback.GetMessageForVerification(grid), StringComparison.Ordinal);

                var card = Assert.IsType<FeedbackToast>(popup.Child);
                Assert.False(card.Focusable);
                Assert.True(popup.StaysOpen);
                Assert.Contains("复制结果：复制失败", AutomationProperties.GetName(card), StringComparison.Ordinal);
                Assert.Contains("稍后重试", AutomationProperties.GetHelpText(card), StringComparison.Ordinal);
            }
            finally
            {
                ClipboardFeedback.CloseForVerification(grid);
                DataGridClipboardBehavior.ClipboardSetterForVerification = null;
                window.Close();
            }
        });
    }

    [Fact]
    public void CopyFeedbackContractCoversLocalSettingsAndDashboardSources()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var root = TestRepositoryContext.Root;
        var feedback = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Infrastructure", "ClipboardFeedback.cs"));
        var behavior = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Infrastructure", "DataGridClipboardBehavior.cs"));
        var settings = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Settings", "GameSaveCenterSettingsView.xaml.cs"));
        var dashboard = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "DashboardView.xaml.cs"));
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));

        Assert.Contains("PlacementTarget = target", feedback, StringComparison.Ordinal);
        Assert.Contains("activeState", feedback, StringComparison.Ordinal);
        Assert.Contains("请稍后重试", behavior, StringComparison.Ordinal);
        Assert.Contains("ClipboardFeedback.Show", behavior, StringComparison.Ordinal);
        Assert.Contains("ClipboardFeedback.Show", settings, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.SetHelpText(SettingsPathEditorStatus", settings, StringComparison.Ordinal);
        Assert.Contains("e.IsCopyFeedback", dashboard, StringComparison.Ordinal);
        Assert.Contains("ShowCopySuccess", viewModel, StringComparison.Ordinal);
        Assert.Contains("ShowCopyError", viewModel, StringComparison.Ordinal);
    }

    private static Window Show(FrameworkElement content)
    {
        var window = new Window
        {
            Width = 640,
            Height = 260,
            WindowStyle = WindowStyle.None,
            ShowInTaskbar = false,
            ShowActivated = false,
            Content = content
        };
        window.Show();
        window.UpdateLayout();
        window.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() => { }));
        return window;
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
}
