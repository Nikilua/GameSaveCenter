using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Controls;
using GameSaveCenter.Playnite.Diagnostics;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using GameSaveCenter.Playnite.Views.Development;
using Xunit;
using WpfButton = System.Windows.Controls.Button;

namespace GameSaveCenter.Playnite.Tests;

public sealed class UiNegativeFixtureRegistryTests
{
    private static readonly IReadOnlyList<NegativeFixture> Fixtures = new[]
    {
        new NegativeFixture(
            "N01",
            "对比度",
            "AdaptiveThemePaletteContrastGuard.ValidateTextContrast",
            "黑字落在暗底上必须产生 violation",
            ProbeContrast),
        new NegativeFixture(
            "N02",
            "裁切",
            "NumericCellReadability.Measure",
            "行高仍足够但数值列宽不足必须失败",
            ProbeNumericCrop),
        new NegativeFixture(
            "N03",
            "焦点",
            "AcrylicProductionShellView.OnPickerPreviewKeyDown",
            "无可见候选的 Enter 不得确认旧选择或关闭选框",
            ProbePickerFocus),
        new NegativeFixture(
            "N04",
            "层级",
            "RealHostUiAuditService.CheckChildLayoutOverflow",
            "固定父级中的超宽子级必须写 CHILD_LAYOUT_OVERFLOW",
            ProbeHierarchyOverflow),
        new NegativeFixture(
            "N05",
            "状态",
            "WorkspaceStatePresenter",
            "Loading 状态必须隐藏重试并阻断底层命中",
            ProbeLoadingState)
    };

    [Fact]
    public void RegisteredNegativeFixturesAreRejectedByTheirDetectors()
    {
        Assert.Equal(new[] { "N01", "N02", "N03", "N04", "N05" }, Fixtures.Select(fixture => fixture.Id));
        Assert.All(Fixtures, fixture =>
        {
            Assert.False(fixture.ProductionEntry);
            var result = fixture.Probe();
            Assert.True(result.Detected, $"{fixture.Id} {fixture.Category} expected-failure 未被捕获：{fixture.ExpectedFailure}; {result.Detail}");
            Console.WriteLine($"{fixture.Id} [{fixture.Category}] detector={fixture.Detector}; expected-failure={fixture.ExpectedFailure}; detected=True; {result.Detail}");
        });
    }

    private static ProbeResult ProbeContrast()
    {
        var violations = AdaptiveThemePaletteContrastGuard.ValidateTextContrast(new[]
        {
            new AdaptiveThemePaletteContrastGuard.TextContrastSample
            {
                Check = "negative-black-on-dark",
                Foreground = Colors.Black,
                Background = Color.FromRgb(37, 42, 52),
                Minimum = 4.5
            }
        });
        return new ProbeResult(
            violations.Count > 0,
            $"violations={violations.Count}, check={violations.FirstOrDefault()?.Check ?? "none"}");
    }

    private static ProbeResult ProbeNumericCrop()
    {
        return RunSta(() =>
        {
            var grid = new DataGrid
            {
                Width = 180,
                Height = 110,
                MinColumnWidth = 0,
                AutoGenerateColumns = false,
                CanUserAddRows = false,
                HeadersVisibility = DataGridHeadersVisibility.Column,
                RowHeight = 52,
                ColumnHeaderHeight = 42,
                ItemsSource = new[] { new NumericRow { Value = "-99,999,999,999,999" } }
            };
            var style = new Style(typeof(TextBlock));
            style.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Left));
            style.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.NoWrap));
            style.Setters.Add(new Setter(TextBlock.TextTrimmingProperty, TextTrimming.None));
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = "数值",
                Binding = new Binding(nameof(NumericRow.Value)),
                Width = 56,
                MinWidth = 0,
                ElementStyle = style
            });

            using var host = new WindowHost(new Window
            {
                Content = grid,
                Width = 180,
                Height = 110,
                ShowInTaskbar = false,
                ShowActivated = false,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            });
            host.Window.Show();
            host.Window.UpdateLayout();

            var measurement = Assert.Single(NumericCellReadability.Measure(grid));
            return new ProbeResult(
                measurement.VerticalFit && !measurement.HorizontalFit && !measurement.IsReadable,
                $"text={measurement.Text}, horizontalFit={measurement.HorizontalFit}, verticalFit={measurement.VerticalFit}, isReadable={measurement.IsReadable}");
        });
    }

    private static ProbeResult ProbePickerFocus()
    {
        return RunSta(() =>
        {
            var picker = new GamePickerViewModel();
            picker.StatusFilter = "全部";
            picker.SetItems(new[] { CreateGame("旧游戏"), CreateGame("另一款") }, "旧游戏");
            var previousGame = picker.SelectedGame;
            picker.SearchText = "不存在的游戏";
            picker.RefreshNow();

            var shell = new AcrylicProductionShellView();
            using var host = new WindowHost(new Window
            {
                Content = shell,
                Width = 900,
                Height = 640,
                ShowInTaskbar = false,
                ShowActivated = false,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            });
            host.Window.Show();
            host.Window.UpdateLayout();

            var overlay = (Grid)shell.FindName("PickerOverlay")!;
            var search = (TextBox)shell.FindName("GameSearchTextBox")!;
            var list = (ListBox)shell.FindName("PickerList")!;
            list.ItemsSource = picker.ItemsView;
            list.SelectedItem = null;
            overlay.Visibility = Visibility.Visible;
            AttachPickerForInput(shell, picker);
            Keyboard.Focus(search);

            var key = new KeyEventArgs(
                Keyboard.PrimaryDevice,
                PresentationSource.FromVisual(host.Window)!,
                0,
                Key.Enter)
            {
                RoutedEvent = Keyboard.PreviewKeyDownEvent
            };
            search.RaiseEvent(key);

            var detected = !key.Handled
                && overlay.Visibility == Visibility.Visible
                && ReferenceEquals(previousGame, picker.SelectedGame);
            return new ProbeResult(
                detected,
                $"handled={key.Handled}, overlay={overlay.Visibility}, selectionPreserved={ReferenceEquals(previousGame, picker.SelectedGame)}");
        });
    }

    private static ProbeResult ProbeHierarchyOverflow()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "gsc-r01-05-negative-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        try
        {
            return RunSta(() =>
            {
                var root = new Grid { Width = 100, Height = 100, Name = "NegativeRoot" };
                root.Children.Add(new Border
                {
                    Width = 200,
                    Height = 100,
                    Name = "NegativeOverflowChild",
                    Background = Brushes.Red
                });
                root.Measure(new Size(100, 100));
                root.Arrange(new Rect(0, 0, 100, 100));
                root.UpdateLayout();
                RealHostUiAuditService.CheckChildLayoutOverflow(root, tempRoot);
                var gate = Path.Combine(tempRoot, "gates", "CHILD_LAYOUT_OVERFLOW.json");
                return new ProbeResult(File.Exists(gate), $"gateExists={File.Exists(gate)}");
            });
        }
        finally
        {
            try { Directory.Delete(tempRoot, true); } catch { }
        }
    }

    private static ProbeResult ProbeLoadingState()
    {
        return RunSta(() =>
        {
            var resourceHost = new MediaCenterView();
            var presenter = new WorkspaceStatePresenter
            {
                State = "Loading",
                Title = "正在加载",
                Message = "请稍候",
                RetryText = "重试",
                RetryCommand = new NoOpCommand(),
                Style = (Style)resourceHost.Resources["GscWorkspaceStatePresenter"],
                Width = 360,
                Height = 220
            };
            var underlying = new WpfButton { Content = "底层操作" };
            var root = new Grid();
            root.Children.Add(underlying);
            root.Children.Add(presenter);
            using var host = new WindowHost(new Window
            {
                Content = root,
                Width = 420,
                Height = 280,
                ShowInTaskbar = false,
                ShowActivated = false,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            });
            host.Window.Show();
            host.Window.UpdateLayout();
            presenter.ApplyTemplate();
            var retry = (WpfButton)presenter.Template!.FindName("RetryButton", presenter)!;
            var point = presenter.TransformToAncestor(host.Window).Transform(
                new Point(presenter.ActualWidth / 2, presenter.ActualHeight / 2));
            var hitUnderlying = ReferenceEquals(FindAncestor<WpfButton>(VisualTreeHelper.HitTest(host.Window, point)?.VisualHit), underlying);
            var detected = retry.Visibility == Visibility.Collapsed && !hitUnderlying;
            return new ProbeResult(
                detected,
                $"retry={retry.Visibility}, underlyingHit={hitUnderlying}, presenterHitTestVisible={presenter.IsHitTestVisible}");
        });
    }

    private static GameStatusDto CreateGame(string name)
        => new GameStatusDto
        {
            PlayniteId = name,
            Name = name,
            Platform = GamePlatformKind.Other,
            IsInstalled = true,
            LudusaviMatched = true
        };

    private static void AttachPickerForInput(AcrylicProductionShellView shell, GamePickerViewModel picker)
    {
        var dashboard = (DashboardViewModel)FormatterServices.GetUninitializedObject(typeof(DashboardViewModel));
        var pickerField = typeof(DashboardViewModel).GetField("gamePicker", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(pickerField);
        pickerField!.SetValue(dashboard, picker);

        var viewModelField = typeof(AcrylicProductionShellView).GetField("viewModel", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(viewModelField);
        viewModelField!.SetValue(shell, dashboard);
    }

    private static T? FindAncestor<T>(DependencyObject? child) where T : DependencyObject
    {
        var current = child;
        while (current != null)
        {
            if (current is T match)
                return match;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    private static ProbeResult RunSta(Func<ProbeResult> probe)
    {
        ProbeResult? result = null;
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                result = probe();
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
        return result ?? throw new InvalidOperationException("Negative fixture did not return a result.");
    }

    private sealed class NegativeFixture
    {
        public NegativeFixture(string id, string category, string detector, string expectedFailure, Func<ProbeResult> probe)
        {
            Id = id;
            Category = category;
            Detector = detector;
            ExpectedFailure = expectedFailure;
            Probe = probe;
        }

        public string Id { get; }
        public string Category { get; }
        public string Detector { get; }
        public string ExpectedFailure { get; }
        public Func<ProbeResult> Probe { get; }
        public bool ProductionEntry => false;
    }

    private sealed class ProbeResult
    {
        public ProbeResult(bool detected, string detail)
        {
            Detected = detected;
            Detail = detail;
        }

        public bool Detected { get; }
        public string Detail { get; }
    }

    private sealed class NumericRow
    {
        public string Value { get; set; } = string.Empty;
    }

    private sealed class NoOpCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) { }
    }

    private sealed class WindowHost : IDisposable
    {
        public WindowHost(Window window) => Window = window;

        public Window Window { get; }

        public void Dispose()
        {
            if (Window.IsVisible)
                Window.Close();
        }
    }
}
