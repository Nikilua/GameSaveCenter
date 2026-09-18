using System;
using System.IO;
using System.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R10ContextGameBehaviorTests
{
    [Fact]
    public void PickerLocatesDuplicateDisplayNamesByStablePlayniteId()
    {
        using var picker = new GamePickerViewModel();
        var first = Game("playnite-a", "同名游戏");
        var second = Game("playnite-b", "同名游戏");

        picker.SetItems(new[] { first, second });
        picker.SelectGame(new GameStatusDto { PlayniteId = "playnite-b", Name = "同名游戏" });

        Assert.NotNull(picker.SelectedGame);
        Assert.Equal("playnite-b", picker.SelectedGame!.PlayniteId);
        Assert.Same(second, picker.Items.Single(item => item.PlayniteId == "playnite-b").Game);
    }

    [Fact]
    public void TaskAndMediaDetailsExposeStableGameIdentityRoutes()
    {
        var root = TestRepositoryContext.Root;
        var navigation = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.Navigation.cs"));
        var media = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.Media.cs"));
        var shell = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Views", "AcrylicProductionShellView.xaml"));

        Assert.Contains("SelectedTask.GameId", navigation);
        Assert.Contains("SelectedGame.PlayniteId", media);
        Assert.Contains("Text=\"{Binding SelectedGame.Name", shell);
        Assert.Contains("ItemsSource=\"{Binding GamePicker.ItemsView}\"", shell);
    }

    private static GameStatusDto Game(string id, string name)
        => new GameStatusDto
        {
            PlayniteId = id,
            Name = name,
            IsInstalled = true,
            LudusaviMatched = true,
            HealthState = "Ready"
        };
}
