using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Views;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;
using Xunit.Abstractions;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class CommandDirectorySearchTests
    {
        private readonly ITestOutputHelper output;

        public CommandDirectorySearchTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void FilterMatchesNameAliasesAndCategoryWithoutCaseSensitivity()
        {
            var items = new[]
            {
                Item("settings.open", "打开插件设置", new[] { "Preferences" }, "设置"),
                Item("media.open", "媒体中心", new[] { "Screenshots" }, "当前游戏"),
                Item("tasks.open", "任务中心", new[] { "Jobs" }, "页面导航"),
            };

            Assert.Equal("settings.open", Assert.Single(CommandDirectorySearch.Filter(items, "插件设")).Id);
            Assert.Equal("media.open", Assert.Single(CommandDirectorySearch.Filter(items, "screenshots")).Id);
            Assert.Equal("tasks.open", Assert.Single(CommandDirectorySearch.Filter(items, "页面导航")).Id);
            Assert.Empty(CommandDirectorySearch.Filter(items, "unknown"));
        }

        [Fact]
        public void EmptyQueryShowsOnlyTheFirstEightCommonItems()
        {
            var items = Enumerable.Range(0, 12)
                .Select(index => Item($"command.{index}", $"命令 {index}", Array.Empty<string>(), "页面导航"))
                .ToArray();

            var results = CommandDirectorySearch.Filter(items, "  ");

            Assert.Equal(CommandDirectorySearch.EmptyQueryResultLimit, results.Count);
            Assert.Equal("command.0", results[0].Id);
            Assert.Equal("command.7", results[7].Id);
        }

        [Fact]
        public void TwoHundredItemFilterCompletesWithinTheControlledLatencyBudget()
        {
            var items = Enumerable.Range(0, 200)
                .Select(index => Item(
                    $"tool.{index}",
                    $"工具 {index:D3}",
                    new[] { $"trainer-{index:D3}" },
                    index % 2 == 0 ? "修改器" : "页面导航"))
                .ToArray();

            var timer = Stopwatch.StartNew();
            var results = CommandDirectorySearch.Filter(items, "trainer-");
            timer.Stop();
            output.WriteLine("200 项别名搜索：{0:F3} ms，匹配 {1} 项", timer.Elapsed.TotalMilliseconds, results.Count);

            Assert.Equal(200, results.Count);
            Assert.True(timer.Elapsed < TimeSpan.FromMilliseconds(100),
                $"200 项本地过滤超过 100 ms：{timer.Elapsed.TotalMilliseconds:F3} ms");
        }

        [Fact]
        public void AvailabilityRefreshNotifiesBoundStatusAndAccessibleName()
        {
            var available = true;
            var item = new CommandDirectoryItem(
                "game.open",
                "打开游戏中心",
                Array.Empty<string>(),
                "页面导航",
                CommandDirectoryScope.CurrentGame,
                new RelayCommand(_ => { }, _ => available),
                unavailableReason: () => "请先选择游戏。");
            var changed = new List<string>();
            item.PropertyChanged += (_, args) => changed.Add(args.PropertyName ?? string.Empty);

            available = false;
            item.RaiseCanExecuteChanged();

            Assert.False(item.IsAvailable);
            Assert.Equal("暂不可用", item.AvailabilityDisplay);
            Assert.Contains("请先选择游戏。", item.AutomationDisplayName);
            Assert.Contains(nameof(CommandDirectoryItem.IsAvailable), changed);
            Assert.Contains(nameof(CommandDirectoryItem.AvailabilityDisplay), changed);
            Assert.Contains(nameof(CommandDirectoryItem.UnavailableReason), changed);
            Assert.Contains(nameof(CommandDirectoryItem.AutomationDisplayName), changed);
        }

        [Fact]
        public void SuccessfulInvocationRaisesOneUsageEventAndUnavailableItemsRaiseNone()
        {
            var canExecute = false;
            var executionCount = 0;
            var item = new CommandDirectoryItem(
                "workspace.tasks",
                "查看任务",
                Array.Empty<string>(),
                "页面导航",
                CommandDirectoryScope.Workspace,
                new RelayCommand(_ => executionCount++, _ => canExecute));
            var usageCount = 0;
            item.Executed += (_, _) => usageCount++;

            Assert.False(item.TryExecute());
            Assert.Equal(0, usageCount);

            canExecute = true;
            Assert.True(item.TryExecute());
            Assert.Equal(1, executionCount);
            Assert.Equal(1, usageCount);
        }

        [Fact]
        public void SearchPanelUsesTheInShellOverlayAndVirtualizedResults()
        {
            var shell = File.ReadAllText(Path.Combine(
                TestRepositoryContext.Root,
                "src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml"));

            Assert.Contains("<Grid x:Name=\"CommandDirectoryOverlay\"", shell);
            Assert.DoesNotContain("<Popup x:Name=\"CommandDirectoryPopup\"", shell);
            Assert.Contains("VirtualizingPanel.IsVirtualizing=\"True\"", shell);
            Assert.Contains("AutomationProperties.Name=\"命令搜索结果\"", shell);
            Assert.Contains("PreviewKeyDown=\"OnCommandDirectoryPreviewKeyDown\"", shell);
            Assert.Contains("Click=\"OnCommandDirectoryPinClick\"", shell);
            Assert.Contains("Content=\"{Binding PinActionDisplay}\"", shell);
            Assert.Contains("AutomationProperties.Name=\"{Binding PinAutomationName}\"", shell);
        }

        private static CommandDirectoryItem Item(string id, string name, IEnumerable<string> aliases, string category)
            => new CommandDirectoryItem(
                id,
                name,
                aliases,
                category,
                CommandDirectoryScope.Workspace,
                new RelayCommand(_ => { }, _ => true));
    }
}
