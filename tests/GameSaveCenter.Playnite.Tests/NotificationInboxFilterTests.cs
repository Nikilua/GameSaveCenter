using System;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class NotificationInboxFilterTests
{
    private static readonly DateTime NowUtc = new(2026, 10, 18, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(NotificationSeverity.Error, "错误")]
    [InlineData(NotificationSeverity.Warning, "警告")]
    [InlineData(NotificationSeverity.Success, "成功")]
    [InlineData(NotificationSeverity.Information, "信息")]
    public void SeverityFilterMatchesOnlyItsSeverity(NotificationSeverity severity, string label)
    {
        var record = new NotificationRecordDto { Severity = severity };

        Assert.True(NotificationInboxFilter.Matches(record, label, NotificationInboxFilter.AllTime, NowUtc));
        Assert.False(NotificationInboxFilter.Matches(
            record,
            label == "错误" ? "警告" : "错误",
            NotificationInboxFilter.AllTime,
            NowUtc));
        Assert.True(NotificationInboxFilter.Matches(record, NotificationInboxFilter.AllSeverities, "unknown", NowUtc));
    }

    [Fact]
    public void TimeFiltersIncludeExactBoundaryAndExcludeOlderRecords()
    {
        Assert.True(Matches(NotificationInboxFilter.Last24Hours, NowUtc.AddHours(-24)));
        Assert.False(Matches(NotificationInboxFilter.Last24Hours, NowUtc.AddHours(-24).AddTicks(-1)));
        Assert.True(Matches(NotificationInboxFilter.Last7Days, NowUtc.AddDays(-7)));
        Assert.False(Matches(NotificationInboxFilter.Last7Days, NowUtc.AddDays(-7).AddTicks(-1)));
        Assert.True(Matches(NotificationInboxFilter.Last30Days, NowUtc.AddDays(-30)));
        Assert.False(Matches(NotificationInboxFilter.Last30Days, NowUtc.AddDays(-30).AddTicks(-1)));
    }

    [Fact]
    public void LocalTimestampsAreComparedAsUtc()
    {
        var localBoundary = NowUtc.AddDays(-7).ToLocalTime();
        var record = new NotificationRecordDto { CreatedUtc = localBoundary };

        Assert.True(NotificationInboxFilter.Matches(
            record,
            NotificationInboxFilter.AllSeverities,
            NotificationInboxFilter.Last7Days,
            NowUtc));
    }

    [Fact]
    public void CurrentGameScopeUsesStableIdAndExcludesSameNameGamesAndGlobalRecords()
    {
        var selectedGameRecord = new NotificationRecordDto { GameId = "playnite-game-a", Title = "同名游戏" };
        var sameNameRecord = new NotificationRecordDto { GameId = "playnite-game-b", Title = "同名游戏" };
        var globalRecord = new NotificationRecordDto { GameId = string.Empty, Title = "全局通知" };

        Assert.True(MatchesCurrentGame(selectedGameRecord, "PLAYNITE-GAME-A"));
        Assert.False(MatchesCurrentGame(sameNameRecord, "playnite-game-a"));
        Assert.True(MatchesCurrentGame(sameNameRecord, "playnite-game-b"));
        Assert.False(MatchesCurrentGame(globalRecord, "playnite-game-a"));
        Assert.False(MatchesCurrentGame(selectedGameRecord, null));
    }

    [Fact]
    public void CurrentGameScopeComposesWithSeverityAndTimeFilters()
    {
        var recentError = new NotificationRecordDto
        {
            GameId = "playnite-game-a",
            Severity = NotificationSeverity.Error,
            CreatedUtc = NowUtc.AddHours(-1)
        };
        var oldError = new NotificationRecordDto
        {
            GameId = "playnite-game-a",
            Severity = NotificationSeverity.Error,
            CreatedUtc = NowUtc.AddDays(-8)
        };
        var recentWarning = new NotificationRecordDto
        {
            GameId = "playnite-game-a",
            Severity = NotificationSeverity.Warning,
            CreatedUtc = NowUtc.AddHours(-1)
        };

        Assert.True(NotificationInboxFilter.Matches(
            recentError,
            "错误",
            NotificationInboxFilter.Last7Days,
            NowUtc,
            filterByGame: true,
            gameId: "playnite-game-a"));
        Assert.False(NotificationInboxFilter.Matches(
            oldError,
            "错误",
            NotificationInboxFilter.Last7Days,
            NowUtc,
            filterByGame: true,
            gameId: "playnite-game-a"));
        Assert.False(NotificationInboxFilter.Matches(
            recentWarning,
            "错误",
            NotificationInboxFilter.Last7Days,
            NowUtc,
            filterByGame: true,
            gameId: "playnite-game-a"));
    }

    private static bool Matches(string timeFilter, DateTime createdUtc)
        => NotificationInboxFilter.Matches(
            new NotificationRecordDto { CreatedUtc = createdUtc },
            NotificationInboxFilter.AllSeverities,
            timeFilter,
            NowUtc);

    private static bool MatchesCurrentGame(NotificationRecordDto record, string? gameId)
        => NotificationInboxFilter.Matches(
            record,
            NotificationInboxFilter.AllSeverities,
            NotificationInboxFilter.AllTime,
            NowUtc,
            filterByGame: true,
            gameId: gameId);
}
