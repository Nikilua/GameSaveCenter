using System.Text;

namespace GameSaveCenter.Worker.Infrastructure;

/// <summary>
/// Shared temp-file + atomic move helpers. A reader only ever sees the complete new file,
/// and failed writes clean up their temporary partial file before rethrowing.
/// </summary>
public static class AtomicFileWriter
{
    private static readonly object DestinationLocksGate = new();
    private static readonly Dictionary<string, DestinationLockEntry> DestinationLocks = new(StringComparer.OrdinalIgnoreCase);

    public static async Task WriteAllTextAsync(string path, string content, CancellationToken token)
    {
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(directory);
        using var destinationLock = await AcquireDestinationLockAsync(fullPath, token).ConfigureAwait(false);
        var temporary = TemporaryPath(directory, Path.GetFileName(fullPath), ".tmp");
        try
        {
            await File.WriteAllTextAsync(temporary, content ?? string.Empty, new UTF8Encoding(false), token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            File.Move(temporary, fullPath, true);
        }
        catch
        {
            TryDelete(temporary);
            throw;
        }
    }

    public static void WriteAllText(string path, string content)
        => WriteAllTextAsync(path, content, CancellationToken.None).GetAwaiter().GetResult();

    public static async Task CopyAtomicallyAsync(string source, string destination, CancellationToken token)
    {
        var fullSource = Path.GetFullPath(source);
        var fullDestination = Path.GetFullPath(destination);
        var directory = Path.GetDirectoryName(fullDestination) ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(directory);
        using var destinationLock = await AcquireDestinationLockAsync(fullDestination, token).ConfigureAwait(false);
        var temporary = TemporaryPath(directory, Path.GetFileName(fullDestination), ".partial");
        try
        {
            await using (var input = new FileStream(fullSource, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1024 * 128, true))
            await using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1024 * 128, true))
                await input.CopyToAsync(output, token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            File.Move(temporary, fullDestination, false);
        }
        catch
        {
            TryDelete(temporary);
            throw;
        }
    }

    public static async Task ReplaceFileAsync(string source, string destination, CancellationToken token)
    {
        var fullSource = Path.GetFullPath(source);
        var fullDestination = Path.GetFullPath(destination);
        var directory = Path.GetDirectoryName(fullDestination) ?? Directory.GetCurrentDirectory();
        Directory.CreateDirectory(directory);
        using var destinationLock = await AcquireDestinationLockAsync(fullDestination, token).ConfigureAwait(false);
        var temporary = TemporaryPath(directory, Path.GetFileName(fullDestination), ".replace");
        try
        {
            await using (var input = new FileStream(fullSource, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1024 * 128, true))
            await using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1024 * 128, true))
                await input.CopyToAsync(output, token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            File.Move(temporary, fullDestination, true);
        }
        catch
        {
            TryDelete(temporary);
            throw;
        }
    }

    private static string TemporaryPath(string directory, string fileName, string suffix)
        => Path.Combine(directory, "." + fileName + "." + Guid.NewGuid().ToString("N") + suffix);

    private static async Task<IDisposable> AcquireDestinationLockAsync(string fullPath, CancellationToken token)
    {
        DestinationLockEntry entry;
        lock (DestinationLocksGate)
        {
            if (!DestinationLocks.TryGetValue(fullPath, out entry!))
            {
                entry = new DestinationLockEntry();
                DestinationLocks.Add(fullPath, entry);
            }
            entry.ReferenceCount++;
        }

        try
        {
            await entry.Semaphore.WaitAsync(token).ConfigureAwait(false);
            return new DestinationLockLease(fullPath, entry);
        }
        catch
        {
            ReleaseDestinationLockReference(fullPath, entry, releaseSemaphore: false);
            throw;
        }
    }

    private static void ReleaseDestinationLockReference(string fullPath, DestinationLockEntry entry, bool releaseSemaphore)
    {
        lock (DestinationLocksGate)
        {
            if (releaseSemaphore) entry.Semaphore.Release();
            entry.ReferenceCount--;
            if (entry.ReferenceCount == 0
                && DestinationLocks.TryGetValue(fullPath, out var current)
                && ReferenceEquals(current, entry))
            {
                DestinationLocks.Remove(fullPath);
            }
        }
    }

    private sealed class DestinationLockEntry
    {
        public SemaphoreSlim Semaphore { get; } = new(1, 1);
        public int ReferenceCount { get; set; }
    }

    private sealed class DestinationLockLease(string fullPath, DestinationLockEntry entry) : IDisposable
    {
        private int disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) == 0)
                ReleaseDestinationLockReference(fullPath, entry, releaseSemaphore: true);
        }
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }
}
