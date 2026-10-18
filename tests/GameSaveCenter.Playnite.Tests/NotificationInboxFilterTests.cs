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

    private static bool Matches(string timeFilter, DateTime createdUtc)
        => NotificationInboxFilter.Matches(
            new NotificationRecordDto { CreatedUtc = createdUtc },
            NotificationInboxFilter.AllSeverities,
            timeFilter,
            NowUtc);
}
