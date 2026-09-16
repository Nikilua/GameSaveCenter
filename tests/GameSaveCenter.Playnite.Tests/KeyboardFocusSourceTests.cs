using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class KeyboardFocusSourceTests
{
    [Fact]
    public void CompactInspectorsDeclareEscapeAndFocusReturnContract()
    {
        var root = FindRepositoryRoot();
        var pages = new[]
        {
            ("MediaCenterView", "MediaInspectorScrollViewer", "OnMediaInspectorPreviewKeyDown", "MediaCompactDetailsButton"),
            ("TaskCenterView", "TaskDetailScrollViewer", "OnTaskDetailPreviewKeyDown", "TaskCompactDetailsButton"),
            ("SaveCenterView", "SaveHistoryActionsScrollViewer", "OnSaveInspectorPreviewKeyDown", "SaveHistoryCompactDetailsButton"),
            ("MaintenanceView", "MaintenanceDeviceInspectorScrollViewer", "OnCompactInspectorPreviewKeyDown", "MaintenanceDeviceCompactDetailsButton"),
            ("TrainerCenterView", "TrainerToolsSettingsScrollViewer", "OnTrainerInspectorPreviewKeyDown", "TrainerToolsCompactDetailsButton")
        };

        foreach (var (page, inspector, handler, button) in pages)
        {
            var xaml = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", page + ".xaml"));
            var code = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", page + ".xaml.cs"));

            Assert.Contains("x:Name=\"" + inspector + "\"", xaml);
            Assert.Contains("Focusable=\"True\"", xaml);
            Assert.Contains("KeyboardNavigation.IsTabStop=\"False\"", xaml);
            Assert.Contains("PreviewKeyDown=\"" + handler + "\"", xaml);
            Assert.Contains("AutomationProperties.Name=", xaml);
            Assert.Contains("Key.Escape", code);
            Assert.Contains("Keyboard.Focus", code);
            Assert.Contains("FocusElement(", code);
            Assert.Contains(inspector, code);
            Assert.Contains(button, code);
            Assert.Contains("e.Handled = true", code);
        }
    }

    [Fact]
    public void TaskInspectorIsFocusableWithoutEnteringTabSequence()
    {
        Exception? exception = null;

        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var view = new TaskCenterView();
                window = CreateWindow(view);
                window.Show();
                window.UpdateLayout();

                var inspector = view.TaskDetailScrollViewerElement;
                var detailsButton = view.FindName("TaskCompactDetailsButton") as Button;

                Assert.True(inspector.Focusable);
                Assert.False(KeyboardNavigation.GetIsTabStop(inspector));
                Assert.NotNull(detailsButton);
                Assert.Equal("查看或收起所选任务详情", AutomationProperties.GetName(detailsButton));
            }
            catch (Exception caught)
            {
                exception = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        Assert.Null(exception);
    }

    [Fact]
    public void ProductionGamePickerClosesWithEscapeOrEnterAndReturnsFocus()
    {
        var root = FindRepositoryRoot();
        var xaml = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml"));
        var code = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml.cs"));

        Assert.Contains("x:Name=\"PickerOverlay\"", xaml);
        Assert.Contains("PreviewKeyDown=\"OnPickerPreviewKeyDown\"", xaml);
        Assert.Contains("AutomationProperties.Name=\"游戏选择器\"", xaml);
        Assert.Contains("private void OnPickerPreviewKeyDown", code);
        Assert.Contains("e.Key == Key.ImeProcessed", code);
        Assert.Contains("e.Key == Key.Escape", code);
        Assert.Contains("PickerList.SelectedItem as GamePickerItem", code);
        Assert.Contains("viewModel.GamePicker.ItemsView.Contains(candidate)", code);
        Assert.DoesNotContain("e.Key == Key.Enter && viewModel?.SelectedGame != null", code);
        Assert.Contains("ClosePickerAndRestoreFocus();", code);
        Assert.Contains("Keyboard.Focus(GameContextButton);", code);
        Assert.Contains("OnPickerScrimMouseDown", code);
        Assert.Contains("OnPickerSelectionChanged", code);
    }

    [Fact]
    public void ProductionGamePickerEscapeActuallyReturnsFocusToContextButton()
    {
        Exception? exception = null;
        var pickerCollapsed = false;
        var handled = false;
        var focusReturned = false;

        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var shell = new AcrylicProductionShellView();
                window = CreateWindow(shell);
                window.Show();
                window.UpdateLayout();

                var picker = (Grid)shell.FindName("PickerOverlay")!;
                var contextButton = (Button)shell.FindName("GameContextButton")!;
                picker.Visibility = Visibility.Visible;
                contextButton.Focus();

                var key = new KeyEventArgs(
                    Keyboard.PrimaryDevice,
                    PresentationSource.FromVisual(window)!,
                    0,
                    Key.Escape)
                {
                    RoutedEvent = Keyboard.PreviewKeyDownEvent
                };
                picker.RaiseEvent(key);

                pickerCollapsed = picker.Visibility == Visibility.Collapsed;
                handled = key.Handled;
                focusReturned = ReferenceEquals(Keyboard.FocusedElement, contextButton);
            }
            catch (Exception caught)
            {
                exception = caught;
            }
            finally
            {
                window?.Close();
            }
        });

        Assert.Null(exception);
        Assert.True(pickerCollapsed);
        Assert.True(handled);
        Assert.True(focusReturned);
    }

    [Fact]
    public void ProductionHeaderAndPickerActionsHaveStableAutomationNames()
    {
        var root = FindRepositoryRoot();
        var shell = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml"));
        var overview = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "OverviewView.xaml"));

        foreach (var name in new[]
        {
            "同步媒体", "立即备份当前游戏", "备份全部游戏", "按状态筛选游戏", "按平台筛选游戏", "排序游戏", "游戏列表",
            "工作区导航", "Worker 与 Ludusavi 运行状态"
        })
            Assert.Contains("AutomationProperties.Name=\"" + name + "\"", shell);

        Assert.Contains("AutomationProperties.Name=\"{Binding OverviewPriorityTitle}\"", overview);
        Assert.Contains("AutomationProperties.Name=\"刷新概览\"", overview);
        Assert.Contains("AutomationProperties.Name=\"刷新当前游戏详情\"", overview);
    }

    private static Window CreateWindow(UIElement content)
        => new()
        {
            Content = content,
            Width = 900,
            Height = 640,
            ShowInTaskbar = false,
            ShowActivated = false,
            WindowStyle = WindowStyle.None,
            Opacity = 0.01
        };

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

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
