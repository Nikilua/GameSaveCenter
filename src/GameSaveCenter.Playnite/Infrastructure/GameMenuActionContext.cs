using System;
using System.Collections.Generic;
using System.Linq;
using Playnite.SDK.Models;

namespace GameSaveCenter.Playnite.Infrastructure;

/// <summary>
/// Immutable identity snapshot for a Playnite game-menu invocation.
/// The host may refresh or replace its selected object before the asynchronous Action runs;
/// actions must resolve the captured IDs again instead of following a mutable row reference.
/// </summary>
internal sealed class GameMenuActionContext
{
    private readonly Guid[] gameIds;

    private GameMenuActionContext(IEnumerable<Guid> gameIds)
    {
        this.gameIds = gameIds.ToArray();
    }

    internal IReadOnlyList<Guid> GameIds => gameIds;

    internal bool IsEmpty => gameIds.Length == 0;

    internal static GameMenuActionContext Capture(IEnumerable<Game>? games)
        => new((games ?? Enumerable.Empty<Game>())
            .Where(game => game != null)
            .Select(game => game.Id));

    internal bool TryResolve(
        IEnumerable<Game>? currentGames,
        out IReadOnlyList<Game> resolvedGames,
        out Guid missingGameId)
    {
        var byId = new Dictionary<Guid, Game>();
        foreach (var game in currentGames ?? Enumerable.Empty<Game>())
        {
            if (game != null && !byId.ContainsKey(game.Id))
                byId.Add(game.Id, game);
        }

        var resolved = new List<Game>(gameIds.Length);
        foreach (var gameId in gameIds)
        {
            if (!byId.TryGetValue(gameId, out var game))
            {
                resolvedGames = Array.Empty<Game>();
                missingGameId = gameId;
                return false;
            }

            resolved.Add(game);
        }

        resolvedGames = resolved;
        missingGameId = Guid.Empty;
        return true;
    }
}
