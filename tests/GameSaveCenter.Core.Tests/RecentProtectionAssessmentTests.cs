using System;
using System.Collections.Generic;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Services;
using Xunit;

namespace GameSaveCenter.Core.Tests;

public sealed class RecentProtectionAssessmentTests
{
    private static readonly DateTime Now = new(2026, 8, 12, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void RecentGames_AreCountedAndOlderGamesAreExcluded()
    {
        var result = Assess(
            Game(Now.AddDays(-2), "Protected", ready: true),
            Game(Now.AddDays(-8), "Outside window", ready: true));

        Assert.Equal(1, result.RecentlyPlayedGames);
        Assert.Equal(1, result.ProtectedGames);
        Assert.Empty(result.AttentionItems);
        var item = Assert.Single(result.Items);
        Assert.Equal(RecentProtectionIssueKind.Protected, item.IssueKind);
        Assert.Equal("已保护", item.StatusDisplay);
        Assert.False(item.IsSelectable);
    }

    [Fact]
    public void UnmatchedRecentGame_IsUnrecognizedSave()
    {
        var result = Assess(Game(Now.AddDays(-1), "Unknown", matched: false));

        var item = Assert.Single(result.Items);
        Assert.Equal(RecentProtectionIssueKind.UnrecognizedSave, item.IssueKind);
        Assert.Equal(1, result.UnrecognizedSaveGames);
        Assert.Equal("未识别存档", item.Title);
    }

    [Fact]
    public void MatchedRecentGameWithoutBackup_IsNeverBackedUp()
    {
        var result = Assess(Game(Now.AddDays(-1), "No backup"));

        var item = Assert.Single(result.Items);
        Assert.Equal(RecentProtectionIssueKind.NeverBackedUp, item.IssueKind);
    }

    [Fact]
    public void ExistingBackupWithDisabledPolicy_IsAutomaticProtectionDisabled()
    {
        var game = Game(Now.AddDays(-1), "Manual only", ready: true);
        game.Policy.Enabled = false;

        var item = Assert.Single(Assess(game).Items);

        Assert.Equal(RecentProtectionIssueKind.AutomaticProtectionDisabled, item.IssueKind);
    }

    [Fact]
    public void BackupCreatedBeforeLatestPlay_IsOutdated()
    {
        var game = Game(Now.AddDays(-1), "Played after backup", ready: true);
        game.LastBackupUtc = Now.AddDays(-2);

        var item = Assert.Single(Assess(game).Items);

        Assert.Equal(RecentProtectionIssueKind.BackupOutdated, item.IssueKind);
    }

    [Fact]
    public void CloudFailureRequiresUploadPolicy()
    {
        var game = Game(Now.AddDays(-1), "Cloud issue", ready: true);
        game.Policy.UploadAfterBackup = true;
        game.CloudState = "Failed";

        var item = Assert.Single(Assess(game).Items);

        Assert.Equal(RecentProtectionIssueKind.CloudFailure, item.IssueKind);
    }

    [Fact]
    public void CorruptedReadiness_IsLatestRestorePointIssue()
    {
        var game = Game(Now.AddDays(-1), "Corrupted", ready: true);
        game.LatestRestoreReadinessStatus = RestoreReadinessStatus.Corrupted;

        var item = Assert.Single(Assess(game).Items);

        Assert.Equal(RecentProtectionIssueKind.RestorePointUnavailable, item.IssueKind);
        Assert.Equal("最新版本不可恢复", item.Title);
    }

    [Fact]
    public void InvalidWindowFallsBackToThirtyDays()
    {
        var result = new RecentProtectionAssessmentService().Assess(
            new[] { Game(Now.AddDays(-20), "Within default") }, 21, Now);

        Assert.Equal(30, result.WindowDays);
        Assert.Equal(1, result.RecentlyPlayedGames);
    }

    [Fact]
    public void BatchProtectionPreviewListsOnlySelectedGamesAndPreservesOtherSettings()
    {
        var selected = Game(Now.AddDays(-1), "Selected", ready: false);
        selected.Policy.UploadAfterBackup = true;
        var protectedGame = Game(Now.AddDays(-1), "Already protected", ready: true);

        var summary = Assess(selected, protectedGame);
        var item = Assert.Single(summary.Items, x => x.GameName == "Selected");
        item.IsSelected = true;

        var preview = ProtectionRecommendationPreview.Build(summary.Items);

        Assert.Contains("将修改 1 个游戏", preview);
        Assert.Contains("Selected → 推荐自动保护（游戏中 + 游戏退出后）", preview);
        Assert.DoesNotContain("Already protected", preview);
        Assert.Contains("不会执行备份、恢复或覆盖现有其他策略设置", preview);
    }

    private static RecentProtectionSummary Assess(params GameStatusDto[] games)
        => new RecentProtectionAssessmentService().Assess(games, 7, Now);

    private static GameStatusDto Game(DateTime lastPlayed, string name, bool matched = true, bool ready = false, bool hasBackup = false)
        => new GameStatusDto
        {
            PlayniteId = name,
            Name = name,
            LastPlayedUtc = lastPlayed,
            LudusaviMatched = matched,
            LastBackupUtc = matched && (ready || hasBackup) ? lastPlayed : null,
            BackupVersionCount = matched && (ready || hasBackup) ? 1 : 0,
            LatestRestoreReadinessStatus = ready ? RestoreReadinessStatus.Ready : null,
            Policy = new BackupPolicyDto()
        };
}
