using System.Collections.Concurrent;

namespace GameSaveCenter.Worker.Services;

/// <summary>
/// Per-game operation lock that prevents overlapping backup, media sync and restore work
/// on the same Playnite game while allowing different games to proceed in parallel.
/// </summary>
public sealed class GameOperationLock
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Number of distinct games currently tracked by the lock table.</summary>
    public int TrackedGameCount => _locks.Count;

    public async Task<GameOperationLease?> AcquireAsync(string playniteId, TimeSpan timeout, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(playniteId)) return null;
        var semaphore = _locks.GetOrAdd(playniteId, _ => new SemaphoreSlim(1, 1));
        if (!await semaphore.WaitAsync(timeout, token).ConfigureAwait(false))
            return null;
        return new GameOperationLease(playniteId, semaphore);
    }
}

public sealed class GameOperationLease : IDisposable
{
    private readonly string playniteId;
    private readonly SemaphoreSlim semaphore;
    private int disposed;

    public GameOperationLease(string playniteId, SemaphoreSlim semaphore)
    {
        this.playniteId = playniteId;
        this.semaphore = semaphore;
    }

    public string PlayniteId => playniteId;

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) == 0)
            semaphore.Release();
    }
}
