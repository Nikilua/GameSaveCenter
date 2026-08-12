using System.Diagnostics;
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

    [Fact]
    public void ProcessIdentityGuard_RejectsPidReuseWithLaterStartTime()
    {
        var tracked = DateTime.UtcNow;
        var reused = tracked.AddSeconds(30);

        Assert.True(ProcessIdentityGuard.IsSameProcess(tracked, tracked.AddMilliseconds(200), TimeSpan.FromSeconds(5)));
        Assert.False(ProcessIdentityGuard.IsSameProcess(tracked, reused, TimeSpan.FromSeconds(5)));
        Assert.False(ProcessIdentityGuard.IsSameProcess(tracked, tracked.AddSeconds(-10), TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public async Task CloseSessionAsync_ClosesOnlyTheMatchingLiveProcess()
    {
        var tracker = new GameToolSessionTracker();
        using var first = Process.Start(new ProcessStartInfo("cmd.exe", "/c ping -n 8 127.0.0.1")
        {
            UseShellExecute = false,
            CreateNoWindow = true
        });
        using var second = Process.Start(new ProcessStartInfo("cmd.exe", "/c ping -n 8 127.0.0.1")
        {
            UseShellExecute = false,
            CreateNoWindow = true
        });
        Assert.NotNull(first);
        Assert.NotNull(second);

        try
        {
            tracker.Track("session-a", first!.Id, first.StartTime.ToUniversalTime(), true);
            tracker.Track("session-b", second!.Id, second.StartTime.ToUniversalTime(), false);

            await tracker.CloseSessionAsync("session-a", TimeSpan.FromMilliseconds(1500), NullLogger.Instance, CancellationToken.None);

            first.WaitForExit(3000);
            Assert.True(first.HasExited, "Session A process should be closed.");
            Assert.False(second.HasExited, "Session B process must not be touched.");
        }
        finally
        {
            if (!first!.HasExited) first.Kill();
            if (!second!.HasExited) second.Kill();
        }
    }
}
