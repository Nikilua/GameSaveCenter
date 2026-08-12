using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>Process launched by GameSaveCenter for one game session.</summary>
public sealed record TrackedToolProcess(int ProcessId, DateTime StartedUtc, bool CloseOnExit);

/// <summary>
/// Identity guard for PID reuse: a process may only be closed when both the PID and the
/// actual StartTime match the session record within a small tolerance.
/// </summary>
public static class ProcessIdentityGuard
{
    public static bool IsSameProcess(DateTime trackedStartUtc, DateTime actualStartUtc, TimeSpan tolerance)
        => Math.Abs((actualStartUtc - trackedStartUtc).TotalMilliseconds) <= tolerance.TotalMilliseconds;
}

/// <summary>
/// Tracks only processes started by GameSaveCenter for a specific game session. Closing a
/// session never touches processes by name and never crosses into another session.
/// </summary>
public sealed class GameToolSessionTracker
{
    private readonly ConcurrentDictionary<string, List<TrackedToolProcess>> sessionProcesses = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, CancellationTokenSource> sessionDelays = new(StringComparer.OrdinalIgnoreCase);

    public void RegisterDelay(string sessionId, CancellationTokenSource source)
        => sessionDelays[sessionId] = source;

    public void Track(string sessionId, int processId, DateTime startedUtc, bool closeOnExit)
    {
        var list = sessionProcesses.GetOrAdd(sessionId, _ => new List<TrackedToolProcess>());
        lock (list)
        {
            list.Add(new TrackedToolProcess(processId, startedUtc, closeOnExit));
        }
    }

    public IReadOnlyList<TrackedToolProcess> GetTracked(string sessionId)
    {
        if (!sessionProcesses.TryGetValue(sessionId, out var list)) return Array.Empty<TrackedToolProcess>();
        lock (list)
        {
            return list.ToArray();
        }
    }

    public Task CloseSessionAsync(string sessionId, TimeSpan gracefulCloseTimeout, ILogger logger, CancellationToken token)
    {
        if (sessionDelays.TryRemove(sessionId, out var delay))
        {
            delay.Cancel();
            delay.Dispose();
        }
        if (!sessionProcesses.TryRemove(sessionId, out var launched)) return Task.CompletedTask;

        TrackedToolProcess[] candidates;
        lock (launched)
        {
            candidates = launched.Where(x => x.CloseOnExit).ToArray();
        }

        foreach (var item in candidates)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                using var process = Process.GetProcessById(item.ProcessId);
                if (!ProcessIdentityGuard.IsSameProcess(item.StartedUtc, process.StartTime.ToUniversalTime(), TimeSpan.FromSeconds(5))) continue;
                process.CloseMainWindow();
                if (!process.WaitForExit((int)gracefulCloseTimeout.TotalMilliseconds)) process.Kill();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not close game tool PID {Pid}", item.ProcessId);
            }
        }
        return Task.CompletedTask;
    }
}
