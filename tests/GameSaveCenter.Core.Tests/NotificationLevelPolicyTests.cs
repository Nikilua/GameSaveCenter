using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Services;
using Xunit;

namespace GameSaveCenter.Core.Tests;

public sealed class NotificationLevelPolicyTests
{
    [Fact]
    public void ImportantOnly_SuppressesSuccessfulSessionSummary()
    {
        var summary = new GameSessionSummaryDto { IsWarning = false, IsFailure = false };
        Assert.False(NotificationLevelPolicy.ShouldEmitSessionSummary(NotificationLevel.ImportantOnly, summary));
        Assert.True(NotificationLevelPolicy.ShouldEmitSessionSummary(NotificationLevel.Summary, summary));
        Assert.True(NotificationLevelPolicy.ShouldEmitSessionSummary(NotificationLevel.Verbose, summary));
    }

    [Fact]
    public void ImportantOnly_StillEmitsWarningAndFailureSummaries()
    {
        var warning = new GameSessionSummaryDto { IsWarning = true };
        var failure = new GameSessionSummaryDto { IsFailure = true };
        Assert.True(NotificationLevelPolicy.ShouldEmitSessionSummary(NotificationLevel.ImportantOnly, warning));
        Assert.True(NotificationLevelPolicy.ShouldEmitSessionSummary(NotificationLevel.ImportantOnly, failure));
    }

    [Fact]
    public void ImportantOnly_SuppressesSuccessfulTasksButKeepsFailuresAndCancels()
    {
        var success = new TaskStatusDto { State = TaskState.Succeeded };
        var failure = new TaskStatusDto { State = TaskState.Failed };
        var cancelled = new TaskStatusDto { State = TaskState.Cancelled };

        Assert.False(NotificationLevelPolicy.ShouldEmitTask(NotificationLevel.ImportantOnly, success));
        Assert.True(NotificationLevelPolicy.ShouldEmitTask(NotificationLevel.ImportantOnly, failure));
        Assert.True(NotificationLevelPolicy.ShouldEmitTask(NotificationLevel.ImportantOnly, cancelled));
        Assert.True(NotificationLevelPolicy.ShouldEmitTask(NotificationLevel.Summary, success));
        Assert.True(NotificationLevelPolicy.ShouldEmitTask(NotificationLevel.Verbose, success));
    }

    [Fact]
    public void UnknownLevelFallsBackToSummaryBehavior()
    {
        var success = new TaskStatusDto { State = TaskState.Succeeded };
        var summary = new GameSessionSummaryDto();
        Assert.True(NotificationLevelPolicy.ShouldEmitTask((NotificationLevel)99, success));
        Assert.True(NotificationLevelPolicy.ShouldEmitSessionSummary((NotificationLevel)99, summary));
    }
}
