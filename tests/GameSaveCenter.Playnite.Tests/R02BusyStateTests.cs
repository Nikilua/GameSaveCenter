using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GameSaveCenter.Playnite.Controls;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views.Development;
using Xunit;
using ProductionButton = GameSaveCenter.Playnite.Controls.Button;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R02BusyStateTests
{
    [Fact]
    public async Task BusyCoordinatorRejectsDuplicateAndRestoresAfterFailureAndCancellation()
    {
        var coordinator = new BusyOperationCoordinator();
        var busy = false;
        var executeCount = 0;
        var finishedCount = 0;
        var transitions = new List<bool>();
        Exception? failure = null;
        string? cancellation = null;

        var failedOperation = new TaskCompletionSource<object?>();
        var first = coordinator.TryRunAsync(
            () => Task.CompletedTask,
            () =>
            {
                executeCount++;
                return failedOperation.Task;
            },
            value =>
            {
                busy = value;
                transitions.Add(value);
            },
            message => cancellation = message,
            error => failure = error,
            () => finishedCount++);

        Assert.True(busy);
        Assert.Equal(1, executeCount);

        var duplicate = await coordinator.TryRunAsync(
            () => Task.CompletedTask,
            () =>
            {
                executeCount++;
                return Task.CompletedTask;
            },
            value => busy = value,
            message => cancellation = message,
            error => failure = error,
            () => finishedCount++);

        Assert.False(duplicate);
        Assert.Equal(1, executeCount);
        Assert.True(busy);

        var expectedFailure = new InvalidOperationException("合成失败");
        failedOperation.SetException(expectedFailure);
        Assert.True(await first);
        Assert.False(busy);
        Assert.Same(expectedFailure, failure);
        Assert.Equal(1, finishedCount);
        Assert.Equal(new[] { true, false }, transitions);

        var cancelledOperation = new TaskCompletionSource<object?>();
        var second = coordinator.TryRunAsync(
            () => Task.CompletedTask,
            () => cancelledOperation.Task,
            value =>
            {
                busy = value;
                transitions.Add(value);
            },
            message => cancellation = message,
            error => failure = error,
            () => finishedCount++);

        Assert.True(busy);
        cancelledOperation.SetCanceled();
        Assert.True(await second);
        Assert.False(busy);
        Assert.Equal("操作已取消", cancellation);
        Assert.Equal(2, finishedCount);
        Assert.Equal(new[] { true, false, true, false }, transitions);
    }

    [Fact]
    public void SharedProductionButtonKeepsWidthContentAndFocusAcrossBusyCycle()
    {
        Exception? exception = null;
        var normalWidth = 0d;
        var busyWidth = 0d;
        var restoredWidth = 0d;
        var indicatorVisible = false;
        var indicatorCollapsed = false;
        var indicatorIsHitTestVisible = true;
        var indicatorIsIndeterminate = false;
        var contentText = string.Empty;
        var busyFocusRetained = false;
        var restoredFocusRetained = false;

        RunSta(() =>
        {
            Window? window = null;
            try
            {
                var resourceHost = new UiFrameworkProbeView();
                var state = new BusyState();
                var root = new UserControl { DataContext = state };
                root.Resources.MergedDictionaries.Add(resourceHost.Resources);
                var button = new ProductionButton
                {
                    Width = 180,
                    Height = 44,
                    Content = "全部备份",
                    Style = (Style)resourceHost.Resources["GscWpfUiPrimaryButton"]
                };
                root.Content = button;
                window = CreateWindow(root);
                window.Show();
                window.UpdateLayout();
                button.ApplyTemplate();

                normalWidth = button.ActualWidth;
                contentText = FindVisualChild<TextBlock>(button)?.Text ?? string.Empty;
                Assert.False(button.IsBusy);
                Assert.True(Keyboard.Focus(button) != null);
                Assert.Same(button, Keyboard.FocusedElement);

                state.IsBusy = true;
                root.UpdateLayout();
                button.ApplyTemplate();
                var busyHost = (Border)button.Template!.FindName("BusyIndicatorHost", button)!;
                var progress = FindVisualChild<ProgressBar>(busyHost);
                busyWidth = button.ActualWidth;
                indicatorVisible = busyHost.Visibility == Visibility.Visible;
                indicatorIsHitTestVisible = busyHost.IsHitTestVisible;
                indicatorIsIndeterminate = progress?.IsIndeterminate == true;
                busyFocusRetained = ReferenceEquals(button, Keyboard.FocusedElement)
                                     && button.IsKeyboardFocusWithin;

                state.IsBusy = false;
                root.UpdateLayout();
                restoredWidth = button.ActualWidth;
                indicatorCollapsed = busyHost.Visibility == Visibility.Collapsed;
                restoredFocusRetained = ReferenceEquals(button, Keyboard.FocusedElement)
                                        && button.IsKeyboardFocusWithin;
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
        Assert.Equal(180, normalWidth, 3);
        Assert.Equal(normalWidth, busyWidth, 3);
        Assert.Equal(normalWidth, restoredWidth, 3);
        Assert.Equal("全部备份", contentText);
        Assert.True(indicatorVisible);
        Assert.True(indicatorCollapsed);
        Assert.False(indicatorIsHitTestVisible);
        Assert.True(indicatorIsIndeterminate);
        Assert.True(busyFocusRetained);
        Assert.True(restoredFocusRetained);
    }

    [Fact]
    public void CurrentProductionShellActionsUseTheSharedBusyPrimitive()
    {
        var production = Read("src", "GameSaveCenter.Playnite", "Themes", "WpfUiProduction.xaml");
        var shell = Read("src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml");

        Assert.Contains("<Setter Property=\"IsBusy\"", production);
        Assert.Contains("DataContext.IsBusy", production);
        Assert.Contains("Style=\"{DynamicResource GscIconOnlyToolbarButton}\"", shell);
        Assert.Contains("Style=\"{DynamicResource GscRedesignHeaderButton}\"", shell);
        Assert.Equal(2, shell.Split(new[] { "Style=\"{DynamicResource GscRedesignPrimaryHeaderButton}\"" }, StringSplitOptions.None).Length - 1);
    }

    private static Window CreateWindow(UIElement content)
        => new()
        {
            Content = content,
            Width = 420,
            Height = 220,
            ShowInTaskbar = false,
            ShowActivated = false,
            WindowStyle = WindowStyle.None,
            Opacity = 0.01
        };

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is T match)
                return match;

            var nested = FindVisualChild<T>(child);
            if (nested != null)
                return nested;
        }

        return null;
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

    private static string Read(params string[] segments)
        => File.ReadAllText(Path.Combine(TestRepositoryContext.Root, Path.Combine(segments)));

    private sealed class BusyState : INotifyPropertyChanged
    {
        private bool isBusy;

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                if (isBusy == value) return;
                isBusy = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBusy)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
