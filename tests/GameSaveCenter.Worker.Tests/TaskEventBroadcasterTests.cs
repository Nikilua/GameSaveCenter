using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Ipc;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class TaskEventBroadcasterTests
{
    [Fact]
    public async Task Publish_FansOutIndependentTaskSnapshots()
    {
        var broadcaster = new TaskEventBroadcaster();
        using var first = broadcaster.Subscribe();
        using var second = broadcaster.Subscribe();
        var change = new TaskChangeEventDto
        {
            Sequence = 42,
            Task = new TaskStatusDto { TaskId = "task", TaskType = "Backup", State = TaskState.Running, ProgressPercent = 30 }
        };

        broadcaster.Publish(change);
        change.Task.ProgressPercent = 99;

        var firstEvent = await first.Reader.ReadAsync();
        var secondEvent = await second.Reader.ReadAsync();
        Assert.Equal(42, firstEvent.Sequence);
        Assert.Equal(30, firstEvent.Task.ProgressPercent);
        Assert.Equal(30, secondEvent.Task.ProgressPercent);
        Assert.NotSame(firstEvent.Task, secondEvent.Task);
    }

    [Fact]
    public async Task Dispose_CompletesOnlyThatSubscriber()
    {
        var broadcaster = new TaskEventBroadcaster();
        var closed = broadcaster.Subscribe();
        using var active = broadcaster.Subscribe();
        closed.Dispose();

        Assert.False(await closed.Reader.WaitToReadAsync());
        broadcaster.Publish(new TaskChangeEventDto { Sequence = 1, Task = new TaskStatusDto { TaskId = "still-active" } });
        Assert.Equal("still-active", (await active.Reader.ReadAsync()).Task.TaskId);
    }

    [Fact]
    public void RepeatedSubscribeDisposeLeavesNoSubscriberResidue()
    {
        var broadcaster = new TaskEventBroadcaster();
        for (var i = 0; i < 200; i++)
        {
            using (var subscription = broadcaster.Subscribe())
            {
                Assert.Equal(1, broadcaster.SubscriberCount);
                broadcaster.Publish(new TaskChangeEventDto
                {
                    Sequence = i,
                    Task = new TaskStatusDto { TaskId = $"subscriber-{i}" }
                });
            }

            Assert.Equal(0, broadcaster.SubscriberCount);
        }
    }

    [Fact]
    public void SlowSubscriberSeesOnlyBoundedDropOldestWindow()
    {
        var broadcaster = new TaskEventBroadcaster();
        List<TaskChangeEventDto> received;
        using (var subscription = broadcaster.Subscribe())
        {
            for (var i = 0; i < 200; i++)
            {
                broadcaster.Publish(new TaskChangeEventDto
                {
                    Sequence = i,
                    Task = new TaskStatusDto { TaskId = $"event-{i}" }
                });
            }

            received = new List<TaskChangeEventDto>();
            while (subscription.Reader.TryRead(out var change))
                received.Add(change);
        }

        Assert.Equal(128, received.Count);
        Assert.Equal(72, received[0].Sequence);
        Assert.Equal(199, received[^1].Sequence);
        Assert.Equal(0, broadcaster.SubscriberCount);
    }
}
