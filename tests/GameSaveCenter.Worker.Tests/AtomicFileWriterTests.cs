using GameSaveCenter.Worker.Infrastructure;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class AtomicFileWriterTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));

    public AtomicFileWriterTests()
    {
        Directory.CreateDirectory(root);
    }

    [Fact]
    public async Task WriteAllTextUsesTemporaryFileAndLeavesNoPartial()
    {
        var path = Path.Combine(root, "settings.json");

        await AtomicFileWriter.WriteAllTextAsync(path, "{\"value\":1}", CancellationToken.None);

        Assert.Equal("{\"value\":1}", await File.ReadAllTextAsync(path));
        Assert.DoesNotContain(Directory.EnumerateFiles(Path.GetDirectoryName(path)!), x => x.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task OverwriteReplacesExistingFileAtomically()
    {
        var path = Path.Combine(root, "settings.json");
        await AtomicFileWriter.WriteAllTextAsync(path, "old", CancellationToken.None);

        await AtomicFileWriter.WriteAllTextAsync(path, "new", CancellationToken.None);

        Assert.Equal("new", await File.ReadAllTextAsync(path));
        Assert.DoesNotContain(Directory.EnumerateFiles(Path.GetDirectoryName(path)!), x => x.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CopyAtomicallyWritesCompleteDestinationAndNoPartial()
    {
        var source = Path.Combine(root, "source.bin");
        var destination = Path.Combine(root, "archive.bin");
        await File.WriteAllBytesAsync(source, new byte[] { 1, 2, 3, 4 });

        await AtomicFileWriter.CopyAtomicallyAsync(source, destination, CancellationToken.None);

        Assert.Equal(new byte[] { 1, 2, 3, 4 }, await File.ReadAllBytesAsync(destination));
        Assert.DoesNotContain(Directory.EnumerateFiles(root), x => x.EndsWith(".partial", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task FailedCopyCleansTemporaryPartial()
    {
        var source = Path.Combine(root, "source.bin");
        var destination = Path.Combine(root, "occupied");
        await File.WriteAllBytesAsync(source, new byte[] { 9 });
        Directory.CreateDirectory(destination);

        await Assert.ThrowsAnyAsync<Exception>(() => AtomicFileWriter.CopyAtomicallyAsync(source, destination, CancellationToken.None));

        Assert.DoesNotContain(Directory.EnumerateFiles(root), x => x.EndsWith(".partial", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CanceledWriteLeavesExistingFileIntact()
    {
        var path = Path.Combine(root, "settings.json");
        await File.WriteAllTextAsync(path, "old");
        using var canceled = new CancellationTokenSource();
        canceled.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            AtomicFileWriter.WriteAllTextAsync(path, "new", canceled.Token));

        Assert.Equal("old", await File.ReadAllTextAsync(path));
        Assert.DoesNotContain(Directory.EnumerateFiles(Path.GetDirectoryName(path)!), x => x.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task LockedDestinationFailurePreservesExistingFileAndCleansTemporary()
    {
        var path = Path.Combine(root, "locked.json");
        await File.WriteAllTextAsync(path, "old");
        Exception? writeError;

        await using (var locked = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            writeError = await Record.ExceptionAsync(() =>
                AtomicFileWriter.WriteAllTextAsync(path, "new", CancellationToken.None));
        }

        Assert.NotNull(writeError);
        Assert.True(writeError is IOException or UnauthorizedAccessException, writeError.ToString());
        Assert.Equal("old", await File.ReadAllTextAsync(path));
        Assert.DoesNotContain(Directory.EnumerateFiles(root), x => x.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ReadOnlyDestinationFailurePreservesExistingFileAndCleansTemporary()
    {
        var path = Path.Combine(root, "readonly.json");
        await File.WriteAllTextAsync(path, "old");
        File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.ReadOnly);

        Exception? writeError;
        try
        {
            writeError = await Record.ExceptionAsync(() =>
                AtomicFileWriter.WriteAllTextAsync(path, "new", CancellationToken.None));
        }
        finally
        {
            File.SetAttributes(path, FileAttributes.Normal);
        }

        Assert.NotNull(writeError);
        Assert.True(writeError is IOException or UnauthorizedAccessException, writeError.ToString());
        Assert.Equal("old", await File.ReadAllTextAsync(path));
        Assert.DoesNotContain(Directory.EnumerateFiles(root), x => x.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ConcurrentWritesLeaveOneCompletePayloadAndNoTemporaryFiles()
    {
        var path = Path.Combine(root, "concurrent.json");
        var payloads = Enumerable.Range(0, 12)
            .Select(index => $"writer:{index:D2}|" + new string((char)('a' + index), 256 * 1024))
            .ToArray();

        await Task.WhenAll(payloads.Select(payload =>
            AtomicFileWriter.WriteAllTextAsync(path, payload, CancellationToken.None)));

        var written = await File.ReadAllTextAsync(path);
        Assert.Contains(written, payloads);
        Assert.DoesNotContain(Directory.EnumerateFiles(root), x => x.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task CancellationAfterTemporaryFileCreationPreservesExistingFileAndCleansTemporary()
    {
        var path = Path.Combine(root, "interrupted.json");
        await File.WriteAllTextAsync(path, "old");
        using var canceled = new CancellationTokenSource();
        using var watcher = new FileSystemWatcher(root, "*.tmp")
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size
        };
        var temporaryCreated = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        watcher.Created += (_, args) =>
        {
            if (Path.GetFileName(args.FullPath).StartsWith(".interrupted.json.", StringComparison.OrdinalIgnoreCase))
            {
                canceled.Cancel();
                temporaryCreated.TrySetResult();
            }
        };
        watcher.EnableRaisingEvents = true;

        var write = AtomicFileWriter.WriteAllTextAsync(
            path,
            new string('x', 16 * 1024 * 1024),
            canceled.Token);
        var observed = await Task.WhenAny(temporaryCreated.Task, Task.Delay(TimeSpan.FromSeconds(10)));
        if (observed != temporaryCreated.Task) canceled.Cancel();

        Assert.Same(temporaryCreated.Task, observed);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => write);
        Assert.Equal("old", await File.ReadAllTextAsync(path));
        Assert.DoesNotContain(Directory.EnumerateFiles(root), x => x.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task FailedReplacePreservesExistingDestination()
    {
        var missingSource = Path.Combine(root, "missing-source.bin");
        var destination = Path.Combine(root, "settings.json");
        await File.WriteAllTextAsync(destination, "old");

        await Assert.ThrowsAnyAsync<FileNotFoundException>(() =>
            AtomicFileWriter.ReplaceFileAsync(missingSource, destination, CancellationToken.None));

        Assert.Equal("old", await File.ReadAllTextAsync(destination));
        Assert.DoesNotContain(Directory.EnumerateFiles(root), x => x.EndsWith(".replace", StringComparison.OrdinalIgnoreCase));
    }

    public void Dispose()
    {
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
    }
}
