using System;
using System.Collections.Generic;
using System.Diagnostics;
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
