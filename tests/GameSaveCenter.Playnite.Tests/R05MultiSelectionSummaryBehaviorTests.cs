using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R05MultiSelectionSummaryBehaviorTests
{
    [Fact]
    public void EmptySingleAndMultipleSelectionsUseCompactAccurateSummaries()
    {
        RunSta(() =>
        {
            using var host = new WindowHost(CreateHost(out var grid, out var summary, out var clearButton, out _));
            var items = MediaItems(4).ToArray();
            grid.ItemsSource = items;
            host.UpdateLayout();

            Assert.Equal("未选择媒体 · Ctrl / Shift 多选", summary.Text);
            Assert.Equal(Visibility.Collapsed, clearButton.Visibility);

            grid.SelectedItems.Add(items[0]);
            host.UpdateLayout();
            Assert.Equal("已选 1 项 · 可批量处理", summary.Text);
            Assert.Equal(Visibility.Visible, clearButton.Visibility);

            grid.SelectedItems.Add(items[1]);
            host.UpdateLayout();
            Assert.Equal("已选 2 项 · 可批量处理", summary.Text);
        });
    }

    [Fact]
    public void CrossWindowSelectionSeparatesTotalSelectionFromCurrentWindowScope()
    {
        RunSta(() =>
        {
            using var host = new WindowHost(CreateHost(out var grid, out var summary, out _, out _));
            var items = MediaItems(4).ToArray();
            grid.ItemsSource = items;
            host.UpdateLayout();

            grid.SelectedItems.Add(items[0]);
            var selections = (Dictionary<string, HashSet<string>>)typeof(MediaCenterView)
                .GetField("selectedInboxIdsByMode", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .GetValue(host.Content)!;
            selections["待归类"].Add("media-hidden");
            grid.SelectedItems.Add(items[1]);
            host.UpdateLayout();

            Assert.Equal("已选 3 项 · 当前窗口 2 项可操作 · 另 1 项暂不可见", summary.Text);
        });
    }

    [Fact]
    public void ClearingSelectionDoesNotChangeInboxScope()
    {
        RunSta(() =>
        {
            using var host = new WindowHost(CreateHost(out var grid, out var summary, out var clearButton, out var modeCombo));
            var view = (MediaCenterView)host.Content!;
            var items = MediaItems(2).ToArray();
            grid.ItemsSource = items;
            modeCombo.ItemsSource = new[] { "待归类", "已忽略" };
            modeCombo.SelectedItem = "已忽略";
            host.UpdateLayout();

            var viewModel = CreateDetachedViewModel("已忽略");
            typeof(MediaCenterView)
                .GetField("attachedViewModel", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(view, viewModel);
            var selections = (Dictionary<string, HashSet<string>>)typeof(MediaCenterView)
                .GetField("selectedInboxIdsByMode", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(view)!;
            selections["待归类"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            selections["已忽略"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            selections["待归类"].Add("pending-retained");
            selections["已忽略"].Add("ignored-hidden");
            grid.SelectedItems.Add(items[0]);
            host.UpdateLayout();

            Assert.Equal("已选 2 项 · 当前窗口 1 项可操作 · 另 1 项暂不可见", summary.Text);

            clearButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            host.UpdateLayout();

            Assert.Equal("已忽略", modeCombo.SelectedItem);
            Assert.Equal("已忽略", viewModel.MediaInboxMode);
            Assert.Equal("未选择媒体 · Ctrl / Shift 多选", summary.Text);
            Assert.Empty(grid.SelectedItems);
            Assert.Empty(selections["已忽略"]);
            Assert.Equal("pending-retained", Assert.Single(selections["待归类"]));
            Assert.Equal(Visibility.Collapsed, clearButton.Visibility);
        });
    }

    private static DashboardViewModel CreateDetachedViewModel(string inboxMode)
    {
        // The view only reads MediaInboxMode in this fixture. Construct an uninitialized
        // VM and diagnostic identity so the test does not start Playnite services or
        // background work while the production grid diagnostic observes selection.
        var formatterServices = Type.GetType("System.Runtime.Serialization.FormatterServices, mscorlib", throwOnError: true)!;
        var createUninitialized = formatterServices.GetMethod("GetUninitializedObject", BindingFlags.Public | BindingFlags.Static)!;
        var createObject = new Func<Type, object>(type => createUninitialized.Invoke(null, new object[] { type })!);
        var viewModel = (DashboardViewModel)createUninitialized.Invoke(null, new object[] { typeof(DashboardViewModel) })!;
        var plugin = (GameSaveCenterPlugin)createObject(typeof(GameSaveCenterPlugin));
        var settings = (GameSaveCenterSettings)createObject(typeof(GameSaveCenterSettings));
        typeof(GameSaveCenterPlugin)
            .GetField("<Settings>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(plugin, settings);
        typeof(DashboardViewModel)
            .GetField("plugin", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(viewModel, plugin);
        typeof(DashboardViewModel)
            .GetField("mediaInboxMode", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(viewModel, inboxMode);
        return viewModel;
    }

    private static Window CreateHost(
        out DataGrid grid,
        out TextBlock summary,
        out Button clearButton,
        out ComboBox modeCombo)
    {
        var view = new MediaCenterView();
        var viewType = typeof(MediaCenterView);
        grid = (DataGrid)viewType.GetField("MediaInboxGrid", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(view)!;
        summary = (TextBlock)viewType.GetField("MediaInboxBatchSelectionSummary", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(view)!;
        clearButton = (Button)viewType.GetField("MediaInboxClearSelectionButton", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(view)!;
        modeCombo = (ComboBox)viewType.GetField("MediaInboxModeCombo", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(view)!;

        var window = new Window
        {
            Content = view,
            Width = 1100,
            Height = 720,
            ShowInTaskbar = false,
            ShowActivated = true,
            WindowStyle = WindowStyle.None,
            Opacity = 0.01
        };
        window.Show();
        window.UpdateLayout();
        return window;
    }

    private static IEnumerable<MediaItemDto> MediaItems(int count)
        => Enumerable.Range(0, count).Select(index => new MediaItemDto
        {
            MediaId = "media-" + index,
            PlayniteId = "game-1",
            Kind = MediaKind.Screenshot,
            Source = MediaSourceKind.WindowsScreenshot,
            ArchivePath = "C:\\isolated\\archive\\media-" + index + ".png",
            OriginalPath = "C:\\isolated\\source\\media-" + index + ".png",
            CapturedUtc = DateTime.UtcNow.AddMinutes(-index),
            ClassificationState = "Inbox"
        });

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
        public WindowHost(Window window) => Window = window;

        private Window Window { get; }
        public object? Content => Window.Content;

        public void UpdateLayout() => Window.UpdateLayout();

        public void Dispose()
        {
            if (Window.IsVisible)
                Window.Close();
        }
    }
}
