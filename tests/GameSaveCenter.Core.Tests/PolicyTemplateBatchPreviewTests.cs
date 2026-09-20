using System;
using System.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Services;
using Xunit;

public sealed class PolicyTemplateBatchPreviewTests
{
    [Fact]
    public void BatchPreviewUsesOnlyExplicitStableIdsAndKeepsExcludedTargets()
    {
        var games = new[]
        {
            Game("a", "Alpha", 30),
            Game("b", "Beta", 30),
            Game("c", "Gamma", 30)
        };
        var targets = PolicyTemplateBatchPreview.Build(games, new BackupPolicyDto { DuringPlayIntervalMinutes = 15 }, new[] { "b", "missing" });

        Assert.True(targets.Count == 3);
        Assert.Equal("b", Assert.Single(targets, target => target.IsSelected).PlayniteId);
        Assert.True(targets.Count(target => target.IsExcluded) == 2);
        Assert.True(PolicyTemplateBatchPreview.Select(targets).Count == 1);
    }

    [Fact]
    public void BatchPreviewReportsChangeCountAndRefusesEmptySelectionInsteadOfSelectingAll()
    {
        var targets = PolicyTemplateBatchPreview.Build(
            new[] { Game("a", "Alpha", 30), Game("b", "Beta", 30) },
            new BackupPolicyDto { DuringPlayIntervalMinutes = 15 });

        Assert.All(targets, target => Assert.Equal(1, target.ChangeCount));
        Assert.Empty(PolicyTemplateBatchPreview.Select(targets));
        Assert.Contains("明确勾选", PolicyTemplateBatchPreview.BuildConfirmation("模板", targets));
    }

    [Fact]
    public void BatchPreviewCapsConfirmationWithoutSilentlyTruncatingSelection()
    {
        var targets = Enumerable.Range(0, PolicyTemplateBatchPreview.MaxTargetCount + 1)
            .Select(index => Game($"game-{index}", $"Game {index}", 30))
            .ToArray();
        var preview = PolicyTemplateBatchPreview.Build(targets, new BackupPolicyDto(), targets.Select(game => game.PlayniteId));

        Assert.True(PolicyTemplateBatchPreview.Select(preview).Count == PolicyTemplateBatchPreview.MaxTargetCount + 1);
        Assert.Contains("超过单次最多", PolicyTemplateBatchPreview.BuildConfirmation("模板", preview));
    }

    private static GameStatusDto Game(string id, string name, int interval)
        => new GameStatusDto
        {
            PlayniteId = id,
            Name = name,
            Policy = new BackupPolicyDto { DuringPlayIntervalMinutes = interval }
        };
}
