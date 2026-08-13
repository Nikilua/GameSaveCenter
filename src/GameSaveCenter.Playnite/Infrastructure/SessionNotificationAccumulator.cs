using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.Infrastructure;

/// <summary>
/// Accumulates one session's terminal task states and emits the final exit summary exactly
/// once, even when the Worker change feed or Playnite stop event delivers the same tasks
/// through overlapping paths.
/// </summary>
internal sealed class SessionNotificationAccumulator
{
    private readonly object sync = new object();
    private int expectedTaskCount;
    private int emitted;
    private readonly Dictionary<string, TaskStatusDto> tasks = new Dictionary<string, TaskStatusDto>(StringComparer.OrdinalIgnoreCase);

    public SessionNotificationAccumulator(string gameName)
    {
        GameName = gameName ?? string.Empty;
    }

    public string GameName { get; }

    public IReadOnlyCollection<TaskStatusDto> Tasks
    {
        get { lock (sync) return tasks.Values.ToList(); }
    }

    public bool IsComplete
    {
        get { lock (sync) return expectedTaskCount > 0 && tasks.Count >= expectedTaskCount; }
    }

    public bool HasExpectedTaskCount => expectedTaskCount > 0;

    public void SetExpectedTaskCount(int count)
    {
        lock (sync) expectedTaskCount = Math.Max(1, count);
    }

    public bool TryMarkEmitted() => Interlocked.Exchange(ref emitted, 1) == 0;

    public void Add(TaskStatusDto task)
    {
        lock (sync) tasks[task.TaskId] = task;
    }
}
