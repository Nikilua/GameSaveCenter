using System;
using System.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class GamePickerViewModelTests
    {
        [Fact]
        public void NewPickerShowsAllGamesByDefault()
        {
            using var picker = new GamePickerViewModel();

            Assert.Equal("已安装", picker.StatusFilter);
            Assert.Equal("全部", picker.PlatformFilter);
            Assert.Equal("名称", picker.SortMode);
            Assert.Equal("全部", picker.StatusFilterOptions[0]);
        }

        [Fact]
        public void FiltersInstalledMatchedBackupAttentionAndUnmatchedLocally()
        {
            using var picker = new GamePickerViewModel();
            picker.SetItems(new[]
            {
                Game("installed", installed: true, matched: true, backups: 2),
                Game("uninstalled", installed: false, matched: true, backups: 1),
                Game("unmatched", installed: true, matched: false, backups: 0),
                Game("attention", installed: true, matched: true, backups: 0, health: "Attention")
            });

            picker.StatusFilter = "已安装";
            Assert.Equal(3, picker.FilteredCount);
            picker.StatusFilter = "已匹配";
            Assert.Equal(3, picker.FilteredCount);
            picker.StatusFilter = "有备份";
            Assert.Equal(2, picker.FilteredCount);
            picker.StatusFilter = "需处理";
            Assert.Equal(1, picker.FilteredCount);
            picker.StatusFilter = "未匹配";
            Assert.Equal(1, picker.FilteredCount);
        }

        [Fact]
        public void SearchAndPlatformFilterUseLocalCacheWithoutWorkerDependency()
        {
            using var picker = new GamePickerViewModel();
            picker.StatusFilter = "全部";
            picker.SetItems(new[]
            {
                Game("Steam Adventure", platform: GamePlatformKind.Steam),
                Game("Xbox Adventure", platform: GamePlatformKind.Xbox)
            });

            picker.PlatformFilter = "Steam";
            picker.SearchText = "Adventure";
            picker.RefreshNow();

            Assert.Equal(1, picker.FilteredCount);
            Assert.Equal("Steam Adventure", picker.ItemsView.Cast<GamePickerItem>().Single().Name);
        }

        [Fact]
        public void SelectionFallsBackWhenSelectedGameIsRemovedAndPreservesHiddenSelection()
        {
            using var picker = new GamePickerViewModel();
            picker.StatusFilter = "全部";
            picker.SetItems(new[] { Game("A"), Game("B") }, "B");
            Assert.Equal("B", picker.SelectedGame!.Name);

            picker.SetItems(new[] { Game("A") }, "B");
            Assert.Equal("A", picker.SelectedGame!.Name);

            picker.StatusFilter = "需处理";
            Assert.Equal("A", picker.SelectedGame!.Name);
            Assert.True(picker.SelectedGameHiddenByFilter);
        }

        [Fact]
        public void SelectionIsPreservedWhenCurrentFiltersHideIt()
        {
            using var picker = new GamePickerViewModel();
            picker.StatusFilter = "全部";
            picker.SetItems(new[] { Game("A"), Game("B", installed: false) }, "B");

            picker.StatusFilter = "已安装";

            Assert.Equal("B", picker.SelectedGame!.Name);
            Assert.True(picker.SelectedGameHiddenByFilter);
            picker.ShowSelectedGameCommand.Execute(null);
            Assert.Equal("B", picker.SelectedGame!.Name);
            Assert.False(picker.SelectedGameHiddenByFilter);
            Assert.Equal("全部", picker.StatusFilter);
        }

        [Fact]
        public void SortModesHaveStableNameTieBreaker()
        {
            using var picker = new GamePickerViewModel();
            picker.StatusFilter = "全部";
            picker.SortMode = "最近游玩";
            picker.SetItems(new[]
            {
                Game("Older", played: DateTime.UtcNow.AddDays(-2)),
                Game("Newer", played: DateTime.UtcNow),
                Game("No play")
            });

            Assert.Equal(new[] { "Newer", "Older", "No play" }, picker.ItemsView.Cast<GamePickerItem>().Select(x => x.Name).ToArray());
        }

        [Fact]
        public void PersistedStateIsRestoredWithoutAnyWorkerRequest()
        {
            using var picker = new GamePickerViewModel();
            picker.ApplyPersistedState("ring", "全部", "Steam", "最近游玩");

            Assert.Equal("ring", picker.SearchText);
            Assert.Equal("全部", picker.StatusFilter);
            Assert.Equal("Steam", picker.PlatformFilter);
            Assert.Equal("最近游玩", picker.SortMode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("未知值")]
        public void FreshOrUnknownStatusDefaultsToInstalled(string? persistedStatus)
        {
            using var picker = new GamePickerViewModel();

            picker.ApplyPersistedState(null, persistedStatus, null, null);

            Assert.Equal("已安装", picker.StatusFilter);
        }

        [Fact]
        public void ExplicitPersistedStatusIsPreserved()
        {
            using var picker = new GamePickerViewModel();

            picker.ApplyPersistedState(null, "有备份", null, null);

            Assert.Equal("有备份", picker.StatusFilter);
        }

        [Fact]
        public void PickerItemsExposeDemoStylePresentationFieldsLocally()
        {
            var latin = new GamePickerItem(Game("Bongo Cat", installed: false, matched: false, health: "Attention", platform: GamePlatformKind.Steam));
            Assert.Equal("BC", latin.Initials);
            Assert.Equal("Attention", latin.HealthState);
            Assert.Contains(latin.PlatformDisplay, latin.MetaDisplay);
            Assert.Contains(latin.InstallStateDisplay, latin.MetaDisplay);
            Assert.Contains(latin.MatchStateDisplay, latin.MetaDisplay);

            var cjk = new GamePickerItem(Game("黑神话悟空"));
            Assert.Equal("黑神", cjk.Initials);
        }

        [Fact]
        public void LargeSetReplacementEmitsOneResetNotification()
        {
            using var picker = new GamePickerViewModel();
            var resetCount = 0;
            var addCount = 0;
            picker.Items.CollectionChanged += (_, args) =>
            {
                if (args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset) resetCount++;
                if (args.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add) addCount++;
            };

            picker.SetItems(Enumerable.Range(0, 250).Select(i => Game("Game " + i)));

            Assert.Equal(1, resetCount);
            Assert.Equal(0, addCount);
            Assert.Equal(250, picker.Items.Count);
        }

        [Fact]
        public void UnchangedSetEmitsNoCollectionNotification()
        {
            using var picker = new GamePickerViewModel();
            var notificationCount = 0;
            picker.Items.CollectionChanged += (_, _) => notificationCount++;

            var games = Enumerable.Range(0, 50).Select(i => Game("Game " + i)).ToArray();
            picker.SetItems(games);
            Assert.Equal(1, notificationCount);

            // Fresh DTO instances with identical visible content must not reset the picker.
            picker.SetItems(Enumerable.Range(0, 50).Select(i => Game("Game " + i)).ToArray());
            Assert.Equal(1, notificationCount);

            picker.SetItems(Enumerable.Range(0, 50).Select(i => Game("Game " + i, health: i == 7 ? "Attention" : "Ready")).ToArray());
            Assert.Equal(2, notificationCount);
        }

        private static GameStatusDto Game(string name, bool installed = true, bool matched = true,
            int backups = 0, string health = "Ready", GamePlatformKind platform = GamePlatformKind.Other,
            DateTime? backup = null, DateTime? played = null)
            => new GameStatusDto
            {
                PlayniteId = name,
                Name = name,
                Platform = platform,
                IsInstalled = installed,
                LudusaviMatched = matched,
                BackupVersionCount = backups,
                LastBackupUtc = backup,
                LastPlayedUtc = played,
                HealthState = health
            };
    }
}
