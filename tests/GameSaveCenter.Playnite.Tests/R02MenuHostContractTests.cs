using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R02MenuHostContractTests
{
    [Fact]
    public void EmptyGameSelectionProducesNoHostMenuItems()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var plugin = CreateUninitializedPlugin();
        var items = plugin.GetGameMenuItems(new GetGameMenuItemsArgs
        {
            Games = new List<Game>()
        }).ToArray();

        Assert.Empty(items);
    }

    [Fact]
    public void SelectedGamesProduceOrderedHostOwnedQuickActionsWithoutExecutingThem()
    {
        TestRepositoryContext.AssertAssemblyMatchesSource();
        var plugin = CreateUninitializedPlugin();
        var items = plugin.GetGameMenuItems(new GetGameMenuItemsArgs
        {
            Games = new List<Game>
            {
                new() { Id = Guid.NewGuid(), Name = "Synthetic game" },
                new() { Id = Guid.NewGuid(), Name = "Second synthetic game" }
            }
        }).ToArray();

        Assert.Equal(
            new[] { "立即备份", "同步媒体", "查看备份历史", "验证最新恢复点", "游戏工具", "打开设置" },
            items.Select(item => item.Description).ToArray());
        Assert.All(items, item =>
        {
            Assert.Equal("GameSaveCenter", item.MenuSection);
            Assert.NotNull(item.Action);
        });
    }

    private static GameSaveCenterPlugin CreateUninitializedPlugin()
        => (GameSaveCenterPlugin)FormatterServices.GetUninitializedObject(typeof(GameSaveCenterPlugin));
}
