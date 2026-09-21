using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Services;
using GameSaveCenter.Playnite.ViewModels;
using GameSaveCenter.Playnite.Views;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R22BatchCountBehaviorTests
{
    [Fact]
    public void PolicyTemplateFilterCountsOnlySelectedTargetsHiddenByTheCurrentQuery()
    {
        var targets = new[]
        {
            new PolicyTemplateBatchTarget("game-alpha", "Alpha", 2, isSelected: true),
            new PolicyTemplateBatchTarget("game-beta", "Beta", 1, isSelected: true),
            new PolicyTemplateBatchTarget("game-gamma", "Gamma", 3, isSelected: false)
        };

        Assert.True(DashboardViewModel.MatchesPolicyTemplateBatchTarget(targets[0], "alpha"));
        Assert.False(DashboardViewModel.MatchesPolicyTemplateBatchTarget(targets[2], "alpha"));
        Assert.Equal(1, DashboardViewModel.CountPolicyTemplateBatchHiddenSelected(targets, "alpha"));
        Assert.Equal(0, DashboardViewModel.CountPolicyTemplateBatchHiddenSelected(targets, string.Empty));

        var summary = DashboardViewModel.BuildPolicyTemplateBatchSummary(
            hasTemplate: true,
            totalCount: targets.Length,
            visibleCount: 1,
            selectedCount: 2,
            hiddenSelectedCount: 1,
            excludedCount: 1,
            changeCount: 3);
        Assert.Equal(
            "已选择 2 个目标；当前筛选结果 1/3 个游戏；隐藏选择 1 个；排除 1 个；预计覆盖 3 项字段；筛选隐藏项仍按稳定 ID 保留选择。",
            summary);

        var emptySelectionSummary = DashboardViewModel.BuildPolicyTemplateBatchSummary(
            hasTemplate: true,
            totalCount: targets.Length,
            visibleCount: 1,
            selectedCount: 0,
            hiddenSelectedCount: 0,
            excludedCount: 3,
            changeCount: 0);
        Assert.Equal(
            "尚未选择目标；当前筛选结果 1/3 个游戏；隐藏选择 0 个；排除 3 个。筛选不会自动选择全部游戏。",
            emptySelectionSummary);
    }

    [Fact]
    public void CurrentMediaBatchSummarySeparatesVisibleAndHiddenSelections()
    {
        RunSta(() =>
        {
            var view = new MediaCenterView();
            var viewType = typeof(MediaCenterView);
            var grid = (ListBox)viewType.GetField("MediaGrid", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
            var summary = (TextBlock)viewType.GetField("MediaCurrentBatchSelectionSummary", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
            var selectedIds = (HashSet<string>)viewType.GetField("selectedMediaIds", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(view)!;
            var items = MediaItems(3).ToArray();
            grid.ItemsSource = items;
            using var host = new WindowHost(new Window
            {
                Content = view,
                Width = 1100,
                Height = 720,
                ShowInTaskbar = false,
                ShowActivated = true,
                WindowStyle = WindowStyle.None,
                Opacity = 0.01
            });

            host.UpdateLayout();
            Assert.Equal("当前结果 3 项 · 未选择媒体 · Ctrl / Shift 多选", summary.Text);

            selectedIds.Add("hidden-media");
            grid.SelectedItems.Add(items[0]);
            host.UpdateLayout();

            Assert.Equal("已选 2 项 · 当前结果 3 项 · 当前窗口 1 项可操作 · 另 1 项暂不可见", summary.Text);
        });
    }

    [Fact]
    public void BatchSurfacesKeepCurrentResultAndExplicitSelectionContracts()
    {
        var root = TestRepositoryContext.Root;
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.cs"));
        var saveCenter = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "SaveCenterView.xaml"));
        var media = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "MediaCenterView.xaml"));
        var taskCenter = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "TaskCenterView.xaml"));

        Assert.Contains("PolicyTemplateBatchHiddenSelectedCount", viewModel);
        Assert.Contains("StringFormat=当前结果：{0}", saveCenter);
        Assert.Contains("StringFormat=隐藏选择：{0}", saveCenter);
        Assert.Contains("MediaCurrentBatchSelectionSummary", media);
        Assert.Contains("TaskLoadedSummary", taskCenter);
        Assert.Contains("TaskActiveFiltersSummary", taskCenter);
        Assert.Contains("当前筛选", viewModel);
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
        public WindowHost(Window window)
        {
            Window = window;
            Window.Show();
            Window.UpdateLayout();
        }

        private Window Window { get; }

        public void UpdateLayout() => Window.UpdateLayout();

        public void Dispose()
        {
            if (Window.IsVisible)
                Window.Close();
        }
    }
}
