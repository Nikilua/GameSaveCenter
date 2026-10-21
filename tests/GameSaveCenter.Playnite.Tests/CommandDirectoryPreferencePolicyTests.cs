using System.Linq;
using GameSaveCenter.Playnite.Settings;
using Newtonsoft.Json;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class CommandDirectoryPreferencePolicyTests
{
    [Fact]
    public void SettingsRoundTripKeepsCommandHistorySeparateAndBounded()
    {
        var settings = new GameSaveCenterSettings
        {
            PinnedCommandIds = Enumerable.Range(0, 8).Select(index => $"workspace.{index}").ToList(),
            RecentCommandIds = Enumerable.Range(0, 24).Select(index => $"workspace.{index}").ToList(),
            RecentAccess = new()
            {
                new RecentAccessRecord { PlayniteId = "playnite-game-id", Workspace = RecentAccessRecord.TasksWorkspace }
            }
        };

        var restored = JsonConvert.DeserializeObject<GameSaveCenterSettings>(JsonConvert.SerializeObject(settings))!;

        Assert.Equal(5, restored.PinnedCommandIds.Count);
        Assert.Equal(20, restored.RecentCommandIds.Count);
        Assert.Equal("workspace.0", restored.PinnedCommandIds[0]);
        Assert.Equal("workspace.0", restored.RecentCommandIds[0]);
        Assert.Equal("playnite-game-id", Assert.Single(restored.RecentAccess).PlayniteId);
    }

    [Fact]
    public void OldSettingsWithoutCommandPreferencesLoadWithEmptyLists()
    {
        var restored = JsonConvert.DeserializeObject<GameSaveCenterSettings>("{\"LastWorkspace\":\"Tasks\"}")!;

        Assert.Empty(restored.PinnedCommandIds);
        Assert.Empty(restored.RecentCommandIds);
    }

    [Fact]
    public void CatalogNormalizationDropsRemovedIdsAndUsesCanonicalStableIds()
    {
        var normalized = CommandDirectoryPreferencePolicy.NormalizeForCatalog(
            new[] { " workspace.tasks ", "removed.command", "SETTINGS.OPEN", "workspace.tasks" },
            new[] { "settings.open", "removed.command", "workspace.tasks", "SETTINGS.OPEN" },
            new[] { "workspace.tasks", "settings.open", "workspace.media" });

        Assert.Equal(new[] { "workspace.tasks", "settings.open" }, normalized.PinnedCommandIds);
        Assert.Equal(new[] { "settings.open", "workspace.tasks" }, normalized.RecentCommandIds);
    }

    [Fact]
    public void RecentUseMovesCommandToFrontAndEvictsTheOldestAfterTwentyEntries()
    {
        var known = Enumerable.Range(0, 21).Select(index => $"command.{index:D2}").ToArray();
        var current = known.Take(20).Reverse().ToArray();

        var updated = CommandDirectoryPreferencePolicy.RecordRecent(current, "command.20", known);

        Assert.Equal(20, updated.Count);
        Assert.Equal("command.20", updated[0]);
        Assert.Equal("command.19", updated[1]);
        Assert.DoesNotContain("command.00", updated);
    }

    [Fact]
    public void PinOrderIsStableAndTheSixthPinRequiresRemovingOneFirst()
    {
        var known = Enumerable.Range(1, 6).Select(index => $"command.{index}").ToArray();
        var pinned = new System.Collections.Generic.List<string>();
        for (var index = 1; index <= 5; index++)
        {
            var next = new System.Collections.Generic.List<string>();
            Assert.True(CommandDirectoryPreferencePolicy.TryTogglePinned(
                pinned, $"command.{index}", known, out next, out var isPinned));
            Assert.True(isPinned);
            pinned = next;
        }

        Assert.False(CommandDirectoryPreferencePolicy.TryTogglePinned(
            pinned, "command.6", known, out var unchanged, out _));
        Assert.Equal(new[] { "command.1", "command.2", "command.3", "command.4", "command.5" }, unchanged);

        Assert.True(CommandDirectoryPreferencePolicy.TryTogglePinned(
            pinned, "command.3", known, out var afterUnpin, out var wasPinned));
        Assert.False(wasPinned);
        Assert.True(CommandDirectoryPreferencePolicy.TryTogglePinned(
            afterUnpin, "command.6", known, out var afterPin, out var isNowPinned));

        Assert.True(isNowPinned);
        Assert.Equal(new[] { "command.1", "command.2", "command.4", "command.5", "command.6" }, afterPin);
    }
}
