using System;
using System.Collections.Generic;
using System.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests
{
    public sealed class GameSelectionResolverTests
    {
        [Fact]
        public void NoRunningGame_RestoresPersistedSelection()
        {
            var games = Games("A", "B", "C");
            Assert.Equal("B", GameSelectionResolver.ResolveInitial(games, "B", null)?.PlayniteId);
        }

        [Fact]
        public void RunningGame_WinsOverPersistedSelection()
        {
            var games = Games("A", "B", "C");
            games.Single(x => x.PlayniteId == "A").IsRunning = true;
            Assert.Equal("A", GameSelectionResolver.ResolveInitial(games, "B", null)?.PlayniteId);
        }

        [Fact]
        public void MultipleRunning_PersistedRunningWins()
        {
            var games = Games("A", "B", "C");
            games.Single(x => x.PlayniteId == "A").IsRunning = true;
            games.Single(x => x.PlayniteId == "B").IsRunning = true;
            games.Single(x => x.PlayniteId == "C").IsRunning = true;
            Assert.Equal("B", GameSelectionResolver.ResolveInitial(games, "B", "C")?.PlayniteId);
        }

        [Fact]
        public void MultipleRunning_LastStartedWinsWhenNotPersisted()
        {
            var games = Games("A", "B", "C");
            games.Single(x => x.PlayniteId == "A").IsRunning = true;
            games.Single(x => x.PlayniteId == "C").IsRunning = true;
            Assert.Equal("C", GameSelectionResolver.ResolveInitial(games, null, "C")?.PlayniteId);
        }

        [Fact]
        public void MultipleRunning_NoPersistedNoLastStarted_UsesLatestActivity()
        {
            var games = Games("A", "B", "C");
            games.Single(x => x.PlayniteId == "A").IsRunning = true;
            games.Single(x => x.PlayniteId == "B").IsRunning = true;
            games.Single(x => x.PlayniteId == "B").LastPlayedUtc = DateTime.UtcNow;
            games.Single(x => x.PlayniteId == "A").LastPlayedUtc = DateTime.UtcNow.AddHours(-1);
            Assert.Equal("B", GameSelectionResolver.ResolveInitial(games, null, null)?.PlayniteId);
        }

        [Fact]
        public void NoRunningNoPersisted_SelectsFirstInstalledGame()
        {
            var games = Games("A", "B");
            games.Single(x => x.PlayniteId == "A").IsInstalled = false;
            Assert.Equal("B", GameSelectionResolver.ResolveInitial(games, null, null)?.PlayniteId);
        }

        [Fact]
        public void EmptyLibrary_ReturnsNull()
        {
            Assert.Null(GameSelectionResolver.ResolveInitial(Enumerable.Empty<GameStatusDto>(), null, null));
        }

        private static List<GameStatusDto> Games(params string[] ids)
            => ids.Select(id => new GameStatusDto
            {
                PlayniteId = id,
                Name = id,
                IsInstalled = true,
                LudusaviMatched = true,
                HealthState = "Ready"
            }).ToList();
    }
}
