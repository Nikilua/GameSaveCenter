using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Settings;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class WorklistContinuationResolverTests
{
    [Fact]
    public void BackupRouteParsesLengthPrefixedParentAndVersionIdsExactly()
    {
        const string gameId = "game|one:steam";
        const string backupId = "backup:two|stable";
        var entry = Entry(WorklistObjectKind.BackupVersion, $"v1|{gameId.Length}:{gameId}|{backupId.Length}:{backupId}", WorklistReferenceState.Indexed, gameId);

        Assert.True(WorklistContinuationResolver.TryResolve(entry, out var target, out var message), message);
        Assert.Equal(gameId, target!.PlayniteId);
        Assert.Equal(backupId, target.ObjectId);
    }

    [Fact]
    public void MediaRouteRequiresTheResolvedParentGameAndNeverGuesses()
    {
        var entry = Entry(WorklistObjectKind.MediaItem, "media-42", WorklistReferenceState.Indexed, string.Empty);

        Assert.False(WorklistContinuationResolver.TryResolve(entry, out var target, out var message));
        Assert.Null(target);
        Assert.Contains("没有可验证的所属游戏", message);
        Assert.Equal(WorklistReferenceState.Indexed, entry.State);

        entry.SetResolution(WorklistReferenceState.Indexed, "game-9");
        Assert.True(WorklistContinuationResolver.TryResolve(entry, out target, out message), message);
        Assert.Equal("game-9", target!.PlayniteId);
        Assert.Equal("media-42", target.ObjectId);
    }

    [Theory]
    [InlineData(WorklistObjectKind.BackgroundTask, WorklistReferenceState.Indexed, "task-exact")]
    [InlineData(WorklistObjectKind.ValidationFinding, WorklistReferenceState.Resolved, "finding-exact")]
    public void TaskAndFindingRoutesKeepTheExactStableIdentity(
        WorklistObjectKind kind,
        WorklistReferenceState state,
        string stableId)
    {
        var entry = Entry(kind, stableId, state, "game-1", isManuallyComplete: true);

        Assert.True(WorklistContinuationResolver.TryResolve(entry, out var target, out var message), message);
        Assert.Equal(kind, target!.Kind);
        Assert.Equal(stableId, target.ObjectId);
        Assert.Equal("game-1", target.PlayniteId);
        Assert.True(entry.IsManuallyComplete);
    }

    [Theory]
    [InlineData(WorklistReferenceState.NotFound)]
    [InlineData(WorklistReferenceState.InvalidReference)]
    [InlineData(WorklistReferenceState.Unavailable)]
    [InlineData(WorklistReferenceState.NotChecked)]
    public void StaleInvalidAndUnknownReferencesCannotBeContinued(WorklistReferenceState state)
    {
        var entry = Entry(WorklistObjectKind.BackgroundTask, "task-42", state, string.Empty);

        Assert.False(WorklistContinuationResolver.TryResolve(entry, out var target, out var message));
        Assert.Null(target);
        Assert.NotEmpty(message);
    }

    private static WorklistBrowserEntry Entry(
        WorklistObjectKind kind,
        string stableId,
        WorklistReferenceState state,
        string playniteId,
        bool isManuallyComplete = false)
    {
        var entry = new WorklistBrowserEntry(new WorklistItemRecord { Kind = kind, StableObjectId = stableId, IsManuallyComplete = isManuallyComplete }, 0);
        entry.SetResolution(state, playniteId);
        return entry;
    }
}
