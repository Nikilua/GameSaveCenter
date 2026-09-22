using System;
using GameSaveCenter.Playnite.Infrastructure;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

public sealed class WorkspaceRevisitLoadGateTests
{
    [Fact]
    public void SameContextIsSkippedOnlyDuringFreshnessWindow()
    {
        var gate = new WorkspaceRevisitLoadGate(TimeSpan.FromSeconds(15));
        var completed = new DateTime(2026, 9, 20, 12, 0, 0, DateTimeKind.Utc);

        Assert.True(gate.TryBegin("Media\u001fgame-a\u001f全部", completed));
        gate.Complete("Media\u001fgame-a\u001f全部", completed);

        Assert.False(gate.TryBegin("Media\u001fgame-a\u001f全部", completed.AddSeconds(14)));
        Assert.True(gate.TryBegin("Media\u001fgame-a\u001f全部", completed.AddSeconds(15)));
    }

    [Fact]
    public void DifferentContextDoesNotReusePreviousSuccess()
    {
        var gate = new WorkspaceRevisitLoadGate(TimeSpan.FromSeconds(15));
        var completed = new DateTime(2026, 9, 20, 12, 0, 0, DateTimeKind.Utc);

        Assert.True(gate.TryBegin("Saves\u001fgame-a", completed));
        gate.Complete("Saves\u001fgame-a", completed);

        Assert.True(gate.TryBegin("Saves\u001fgame-b", completed.AddSeconds(1)));
    }

    [Fact]
    public void FailureAndCancellationDoNotInventFreshness()
    {
        var gate = new WorkspaceRevisitLoadGate(TimeSpan.FromSeconds(15));
        var started = new DateTime(2026, 9, 20, 12, 0, 0, DateTimeKind.Utc);

        Assert.True(gate.TryBegin("Maintenance", started));
        gate.Fail("Maintenance");
        Assert.True(gate.TryBegin("Maintenance", started.AddSeconds(1)));
        gate.Cancel("Maintenance");
        Assert.True(gate.TryBegin("Maintenance", started.AddSeconds(2)));
    }

    [Fact]
    public void DuplicateInFlightReadIsRejectedUntilItFinishes()
    {
        var gate = new WorkspaceRevisitLoadGate(TimeSpan.FromSeconds(15));
        var started = new DateTime(2026, 9, 20, 12, 0, 0, DateTimeKind.Utc);

        Assert.True(gate.TryBegin("Trainers\u001fgame-a", started));
        Assert.False(gate.TryBegin("Trainers\u001fgame-a", started.AddMilliseconds(1)));
        gate.Complete("Trainers\u001fgame-a", started.AddSeconds(1));
        Assert.False(gate.TryBegin("Trainers\u001fgame-a", started.AddSeconds(2)));
    }

    [Fact]
    public void InvalidatedReadCannotPublishFreshnessWhenItReturnsLate()
    {
        var gate = new WorkspaceRevisitLoadGate(TimeSpan.FromSeconds(15));
        var started = new DateTime(2026, 9, 20, 12, 0, 0, DateTimeKind.Utc);

        Assert.True(gate.TryBegin("Media\u001fgame-a", started));
        gate.InvalidateAll();

        Assert.False(gate.Complete("Media\u001fgame-a", started.AddSeconds(1)));
        Assert.True(gate.TryBegin("Media\u001fgame-a", started.AddSeconds(2)));
    }
}
