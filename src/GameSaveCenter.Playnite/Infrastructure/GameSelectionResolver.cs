using System;
using System.Collections.Generic;
using System.Linq;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Pure game-selection priority used when the dashboard first opens: running games win
    /// (persisted id → last started event → latest activity), then the persisted last
    /// selection, then first installed game. Normal refreshes never call this; it is used
    /// during initial open/page activation and after new GameStarted events.
    /// </summary>
    public static class GameSelectionResolver
    {
        public static GameStatusDto? ResolveInitial(
            IEnumerable<GameStatusDto> games,
            string? persistedPlayniteId,
            string? lastStartedPlayniteId)
        {
            var list = games == null ? new List<GameStatusDto>() : games.ToList();
            var running = list.Where(game => game.IsRunning).ToList();
            if (running.Count > 0)
            {
                var persisted = FirstById(running, persistedPlayniteId);
                if (persisted != null) return persisted;
                var lastStarted = FirstById(running, lastStartedPlayniteId);
                if (lastStarted != null) return lastStarted;
                return running
                    .OrderByDescending(game => game.LastPlayedUtc ?? DateTime.MinValue)
                    .FirstOrDefault();
            }

            var remembered = FirstById(list, persistedPlayniteId);
            if (remembered != null) return remembered;
            return list.FirstOrDefault(game => game.IsInstalled) ?? list.FirstOrDefault();
        }

        private static GameStatusDto? FirstById(IEnumerable<GameStatusDto> games, string? playniteId)
        {
            if (string.IsNullOrWhiteSpace(playniteId)) return null;
            return games.FirstOrDefault(game =>
                string.Equals(game.PlayniteId, playniteId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
