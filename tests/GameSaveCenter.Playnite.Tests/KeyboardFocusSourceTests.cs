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
