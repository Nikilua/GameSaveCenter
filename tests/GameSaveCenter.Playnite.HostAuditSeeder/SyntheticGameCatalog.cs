using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Playnite.SDK.Models;

namespace GameSaveCenter.Playnite.HostAuditSeeder;

public static class SyntheticGameCatalog
{
    public const string GameIdPrefix = "gsc-ui-audit-r23-04-";
    public const string NamePrefix = "GSC Audit Synthetic ";
    public const int MaximumCount = 512;

    public static IReadOnlyList<GameMetadata> Create(int count)
    {
        if (count < 1 || count > MaximumCount)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count,
                $"Synthetic host audit count must be between 1 and {MaximumCount}.");
        }

        return Enumerable.Range(1, count)
            .Select(index => new GameMetadata
            {
                Name = NamePrefix + index.ToString("D3", CultureInfo.InvariantCulture),
                GameId = GameIdPrefix + index.ToString("D4", CultureInfo.InvariantCulture),
                IsInstalled = false
            })
            .ToArray();
    }
}
