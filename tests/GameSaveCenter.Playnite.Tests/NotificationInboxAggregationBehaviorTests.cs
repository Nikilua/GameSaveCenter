using System;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class NotificationInboxAggregationBehaviorTests
{
    [Fact]
    public void AggregatedIssueShowsAttemptWindowLastErrorAndBothTaskIds()
    {
        var first = DateTime.UtcNow.AddHours(-2);
        var last = DateTime.UtcNow;
        var item = new NotificationInboxItem(new NotificationRecordDto
        {
            NotificationId = "notice-1",
            DeduplicationKey = "issue:1",
            IssueKey = "issue:1",
            TaskId = "retry-task",
            FirstTaskId = "first-task",
            GameId = "game-1",
            Severity = NotificationSeverity.Error,
            Source = "存档校验",
            Title = "后台任务失败",
            Summary = "最近一次失败",
            CreatedUtc = first,
            FirstOccurredUtc = first,
            LastOccurredUtc = last,
            OccurrenceCount = 3,
            LastError = "ACCESS_DENIED: 权限不足"
        });

        Assert.True(item.HasIssue);
        Assert.Equal("3 次", item.OccurrenceDisplay);
        Assert.Equal("待恢复", item.ResolvedDisplay);
        Assert.Equal("first-task", item.FirstTaskId);
        Assert.Equal("retry-task", item.TaskId);
        Assert.Equal("ACCESS_DENIED: 权限不足", item.LastError);
        Assert.Equal("存档校验", item.SourceDisplay);
        Assert.Equal("来源：存档校验", item.SourceLineDisplay);
        Assert.Equal(last, item.CreatedUtc);
        Assert.Equal(first, item.FirstOccurredUtc);
        Assert.Equal(last, item.LastOccurredUtc);
    }

    [Fact]
    public void StandaloneNotificationKeepsExistingSingleTaskDisplay()
    {
        var item = new NotificationInboxItem(new NotificationRecordDto
        {
            NotificationId = "notice-2",
            DeduplicationKey = "event:2",
            TaskId = "task-2",
            CreatedUtc = DateTime.UtcNow
        });

        Assert.False(item.HasIssue);
        Assert.Equal("—", item.OccurrenceDisplay);
        Assert.Equal("—", item.ResolvedDisplay);
        Assert.Equal("task-2", item.FirstTaskId);
        Assert.Equal("来源未记录", item.SourceDisplay);
        Assert.Equal("来源：来源未记录", item.SourceLineDisplay);
    }
}
