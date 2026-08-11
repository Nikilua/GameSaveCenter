using GameSaveCenter.Worker.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class GameToolSessionTrackerTests
{
    [Fact]
    public void TrackedProcesses_AreIsolatedBySession()
    {
        var tracker = new GameToolSessionTracker();
        tracker.Track("session-a", 100, DateTime.UtcNow, true);
        tracker.Track("session-b", 200, DateTime.UtcNow, false);

        Assert.Single(tracker.GetTracked("session-a"));
        Assert.Single(tracker.GetTracked("session-b"));
        Assert.Equal(100, tracker.GetTracked("session-a")[0].ProcessId);
        Assert.Equal(200, tracker.GetTracked("session-b")[0].ProcessId);
    }

    [Fact]
    public void CloseOnExit_IsRecordedPerProcess()
    {
        var tracker = new GameToolSessionTracker();
        tracker.Track("session", 100, DateTime.UtcNow, true);
        tracker.Track("session", 200, DateTime.UtcNow, false);

        var tracked = tracker.GetTracked("session");
        Assert.Equal(2, tracked.Count);
        Assert.True(tracked[0].CloseOnExit);
        Assert.False(tracked[1].CloseOnExit);
    }

    [Fact]
    public async Task CloseSessionAsync_RemovesOnlyThatSessionAndToleratesMissingProcesses()
    {
        var tracker = new GameToolSessionTracker();
        tracker.Track("session-a", 999999, DateTime.UtcNow, true);
        tracker.Track("session-b", 100, DateTime.UtcNow, false);

        await tracker.CloseSessionAsync("session-a", TimeSpan.FromMilliseconds(50), NullLogger.Instance, CancellationToken.None);

        Assert.Empty(tracker.GetTracked("session-a"));
        Assert.Single(tracker.GetTracked("session-b"));
    }
}
