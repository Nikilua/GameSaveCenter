using System;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Services;
using Xunit;

namespace GameSaveCenter.Core.Tests;

public sealed class NotificationPopupPolicyTests
{
    [Theory]
    [InlineData(NotificationSeverity.Information)]
    [InlineData(NotificationSeverity.Success)]
    [InlineData((NotificationSeverity)99)]
    public void TemporaryMuteHidesRoutinePopupsUntilTheUtcExpiry(NotificationSeverity severity)
    {
        var now = new DateTime(2026, 10, 18, 8, 0, 0, DateTimeKind.Utc);
        var expiry = now.AddHours(1);

        Assert.False(NotificationPopupPolicy.ShouldShowPopup(severity, expiry, false, 1320, 420, now, now.ToLocalTime()));
        Assert.True(NotificationPopupPolicy.ShouldShowPopup(severity, expiry, false, 1320, 420, expiry, expiry.ToLocalTime()));
    }

    [Fact]
    public void UnknownSeverityDoesNotBypassQuietHours()
    {
        var now = new DateTime(2026, 10, 18, 23, 30, 0, DateTimeKind.Utc);

        Assert.False(NotificationPopupPolicy.ShouldShowPopup(
            (NotificationSeverity)99,
            null,
            true,
            22 * 60,
            7 * 60,
            now,
            new DateTime(2026, 10, 18, 23, 30, 0, DateTimeKind.Local)));
    }

    [Theory]
    [InlineData(NotificationSeverity.Warning)]
    [InlineData(NotificationSeverity.Error)]
    public void WarningAndErrorPopupsRemainVisibleDuringMuteAndQuietHours(NotificationSeverity severity)
    {
        var now = new DateTime(2026, 10, 18, 23, 30, 0, DateTimeKind.Utc);

        Assert.True(NotificationPopupPolicy.ShouldShowPopup(
            severity,
            now.AddHours(1),
            true,
            22 * 60,
            7 * 60,
            now,
            new DateTime(2026, 10, 18, 23, 30, 0, DateTimeKind.Local)));
    }

    [Theory]
    [InlineData(21, 59, false)]
    [InlineData(22, 0, true)]
    [InlineData(23, 59, true)]
    [InlineData(0, 0, true)]
    [InlineData(6, 59, true)]
    [InlineData(7, 0, false)]
    [InlineData(12, 0, false)]
    public void CrossMidnightQuietHoursUseAnInclusiveStartAndExclusiveEnd(int hour, int minute, bool expected)
    {
        var localNow = new DateTime(2026, 10, 18, hour, minute, 0, DateTimeKind.Local);

        Assert.Equal(expected, NotificationPopupPolicy.IsWithinQuietHours(true, 22 * 60, 7 * 60, localNow));
    }

    [Theory]
    [InlineData(8, 59, false)]
    [InlineData(9, 0, true)]
    [InlineData(16, 59, true)]
    [InlineData(17, 0, false)]
    public void SameDayQuietHoursUseAnInclusiveStartAndExclusiveEnd(int hour, int minute, bool expected)
    {
        var localNow = new DateTime(2026, 10, 18, hour, minute, 0, DateTimeKind.Local);

        Assert.Equal(expected, NotificationPopupPolicy.IsWithinQuietHours(true, 9 * 60, 17 * 60, localNow));
    }

    [Fact]
    public void RechecksUseCurrentLocalTimeAfterAClockOrTimezoneChange()
    {
        var beforeJump = new DateTime(2026, 10, 18, 21, 0, 0, DateTimeKind.Local);
        var afterJump = new DateTime(2026, 10, 18, 23, 0, 0, DateTimeKind.Local);

        Assert.False(NotificationPopupPolicy.IsWithinQuietHours(true, 22 * 60, 7 * 60, beforeJump));
        Assert.True(NotificationPopupPolicy.IsWithinQuietHours(true, 22 * 60, 7 * 60, afterJump));
    }

    [Theory]
    [InlineData(-1, 420)]
    [InlineData(1320, 1440)]
    [InlineData(1320, 1320)]
    public void InvalidOrEmptySchedulesDoNotSilenceTheWholeDay(int startMinute, int endMinute)
    {
        Assert.False(NotificationPopupPolicy.IsWithinQuietHours(
            true,
            startMinute,
            endMinute,
            new DateTime(2026, 10, 18, 23, 0, 0, DateTimeKind.Local)));
    }

    [Fact]
    public void UnspecifiedMuteTimestampIsTreatedAsUtcAndFormattingIsStable()
    {
        var unspecified = DateTime.SpecifyKind(new DateTime(2026, 10, 18, 9, 30, 0), DateTimeKind.Unspecified);

        Assert.Equal(DateTimeKind.Utc, NotificationPopupPolicy.NormalizeUtc(unspecified).Kind);
        Assert.Equal(new DateTime(2026, 10, 18, 9, 30, 0, DateTimeKind.Utc), NotificationPopupPolicy.NormalizeUtc(unspecified));
        Assert.Equal("22:00", NotificationPopupPolicy.FormatMinuteOfDay(1320));
        Assert.Equal("--:--", NotificationPopupPolicy.FormatMinuteOfDay(1440));
    }
}
