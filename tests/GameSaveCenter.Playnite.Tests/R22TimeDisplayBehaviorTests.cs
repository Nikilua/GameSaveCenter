using System;
using System.Linq;
using GameSaveCenter.Contracts;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class R22TimeDisplayBehaviorTests
{
    [Fact]
    public void RelativeDisplayHasStableJustNowAndMinuteBoundaries()
    {
        var now = new DateTime(2026, 9, 21, 4, 0, 0, DateTimeKind.Utc);

        Assert.Equal("刚刚", TimeDisplayFormatter.Relative(now.AddSeconds(-59), now));
        Assert.Equal("1 分钟前", TimeDisplayFormatter.Relative(now.AddMinutes(-1), now));
        Assert.Equal("即将", TimeDisplayFormatter.Relative(now.AddSeconds(30), now));
    }

    [Fact]
    public void RelativeDisplayUsesLocalYesterdayBoundary()
    {
        var nowUtc = new DateTime(2026, 9, 21, 4, 0, 0, DateTimeKind.Utc);
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, TimeZoneInfo.Local);
        var yesterdayLocal = DateTime.SpecifyKind(
            localNow.Date.AddDays(-1).AddHours(12),
            DateTimeKind.Unspecified);
        var yesterdayUtc = TimeZoneInfo.ConvertTimeToUtc(yesterdayLocal, TimeZoneInfo.Local);

        Assert.Equal($"昨天 {yesterdayLocal:HH:mm}", TimeDisplayFormatter.Relative(yesterdayUtc, nowUtc));
    }

    [Fact]
    public void FullDisplayKeepsTimezoneHintAndCopyableUtcValue()
    {
        var timestamp = new DateTime(2026, 9, 21, 4, 5, 6, 123, DateTimeKind.Utc).AddTicks(4567);

        var raw = TimeDisplayFormatter.RawUtc(timestamp);
        var full = TimeDisplayFormatter.Full(timestamp);

        Assert.Equal(timestamp.ToString("O"), raw);
        Assert.Contains("UTC", full, StringComparison.Ordinal);
        Assert.Contains(raw, full, StringComparison.Ordinal);
    }

    [Fact]
    public void TaskAndActivityModelsExposeTheSameFullAndRawTimeContract()
    {
        var timestamp = new DateTime(2026, 9, 20, 12, 34, 56, DateTimeKind.Utc);
        var task = new TaskStatusDto
        {
            TaskId = "time-contract-task",
            TaskType = "Backup",
            State = TaskState.Succeeded,
            CreatedUtc = timestamp,
            StartedUtc = timestamp.AddSeconds(2)
        };
        var activity = new ActivityEntryDto { CreatedUtc = timestamp };

        Assert.Equal(TimeDisplayFormatter.RawUtc(timestamp), task.CreatedRawUtcDisplay);
        Assert.Equal(TimeDisplayFormatter.Full(timestamp), task.CreatedFullDisplay);
        Assert.Equal(TimeDisplayFormatter.Full(timestamp), activity.CreatedFullDisplay);
        Assert.Equal(TimeDisplayFormatter.RawUtc(timestamp), activity.CreatedRawUtcDisplay);
        Assert.NotEqual("时间未知", task.CreatedRelativeDisplay);
        Assert.Equal(TimeDisplayFormatter.Full(task.StartedUtc.Value), task.StartedFullDisplay);

        task.StartedUtc = null;
        Assert.Equal("未开始", task.StartedRelativeDisplay);
        Assert.Equal("未开始", task.StartedFullDisplay);
    }

    [Fact]
    public void TimelineOrderingRemainsUtcBasedWhileRelativeTextIsComputedSeparately()
    {
        var created = new DateTime(2026, 9, 20, 1, 0, 0, DateTimeKind.Utc);
        var started = created.AddMinutes(1);
        var task = new TaskStatusDto
        {
            TaskId = "relative-order",
            TaskType = "Backup",
            State = TaskState.Running,
            CreatedUtc = created,
            StartedUtc = started
        };

        var timeline = TaskTimelineBuilder.Build(task, new[]
        {
            new TaskChangeEventDto
            {
                Sequence = 2,
                OccurredUtc = started,
                Task = new TaskStatusDto
                {
                    TaskId = task.TaskId,
                    State = TaskState.Running,
                    StartedUtc = started,
                    CreatedUtc = created
                }
            },
            new TaskChangeEventDto
            {
                Sequence = 1,
                OccurredUtc = created.AddSeconds(10),
                Task = new TaskStatusDto
                {
                    TaskId = task.TaskId,
                    State = TaskState.Queued,
                    CreatedUtc = created
                }
            }
        });

        Assert.Equal(new[] { "Created", "Started" }, timeline.Select(entry => entry.Kind).ToArray());
        Assert.All(timeline.Where(entry => entry.HasKnownTime), entry =>
        {
            Assert.NotEqual("时间未知", entry.RelativeTimeDisplay);
            Assert.Contains(entry.RawUtcTimeDisplay, entry.FullTimeDisplay, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void UnknownTimelineTimeRemainsExplicit()
    {
        var task = new TaskStatusDto
        {
            TaskId = "unknown-relative",
            TaskType = "Restore",
            State = TaskState.Running,
            CreatedUtc = new DateTime(2026, 9, 20, 2, 0, 0, DateTimeKind.Utc)
        };

        var unknown = TaskTimelineBuilder.Build(task, new[]
        {
            new TaskChangeEventDto
            {
                Sequence = 1,
                OccurredUtc = DateTime.MinValue,
                Task = task
            }
        }).Single(entry => entry.Kind == "Started");

        Assert.False(unknown.HasKnownTime);
        Assert.Equal("时间未知", unknown.RelativeTimeDisplay);
        Assert.Equal("未记录 UTC 时间", unknown.RawUtcTimeDisplay);
    }
}
