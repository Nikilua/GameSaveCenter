using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Ipc;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;

namespace GameSaveCenter.Worker.Tests.Infrastructure;

/// <summary>
/// Injects deterministic boundary faults into Worker surfaces and verifies each failure
/// produces a stable terminal state without leaving partial files, locks or subscriptions
/// behind. All state lives below a temporary directory and no real user data is touched.
/// </summary>
public sealed class FaultInjectionHarness : IDisposable
{
    private readonly string root;
    private readonly string probeDirectory;
    private readonly WorkerOptions options;
    private readonly SqliteStateStore store;
    private readonly TaskEventBroadcaster broadcaster;
    private readonly TaskCoordinator coordinator;
    private readonly GameOperationLock operationLock;
    private readonly ExternalProcessRunner runner;
    private readonly List<string> errors = new();

    public FaultInjectionHarness()
    {
        root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Faults", Guid.NewGuid().ToString("N"));
        options = new WorkerOptions
        {
            DataDirectory = Path.Combine(root, "Data"),
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            MediaArchiveDirectory = Path.Combine(root, "Media")
        };
        Directory.CreateDirectory(options.DataDirectory);
        Directory.CreateDirectory(options.LudusaviBackupDirectory);
        Directory.CreateDirectory(options.MediaArchiveDirectory);
        probeDirectory = Path.Combine(root, "Probe");
        Directory.CreateDirectory(probeDirectory);

        store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
        broadcaster = new TaskEventBroadcaster();
        coordinator = new TaskCoordinator(store, broadcaster, NullLogger<TaskCoordinator>.Instance);
        operationLock = new GameOperationLock();
        runner = new ExternalProcessRunner(NullLogger<ExternalProcessRunner>.Instance);
    }

    public int Attempted { get; private set; }
    public int Recovered { get; private set; }
    public IReadOnlyList<string> Errors => errors;

    public async Task RunAsync(CancellationToken token)
    {
        await RunStepAsync("atomic-missing-source", () => InjectMissingSourceCopyAsync(token)).ConfigureAwait(false);
        await RunStepAsync("atomic-occupied-destination", () => InjectOccupiedDestinationCopyAsync(token)).ConfigureAwait(false);
        await RunStepAsync("atomic-canceled-write", () => InjectCanceledWriteAsync(token)).ConfigureAwait(false);
        await RunStepAsync("process-missing-executable", () => InjectMissingExecutableAsync(token)).ConfigureAwait(false);
        await RunStepAsync("process-timeout", () => InjectProcessTimeoutAsync(token)).ConfigureAwait(false);
        await RunStepAsync("process-cancellation", () => InjectProcessCancellationAsync(token)).ConfigureAwait(false);
        await RunStepAsync("process-nonzero-exit", () => InjectNonZeroExitAsync(token)).ConfigureAwait(false);
        await RunStepAsync("task-worker-operation-failure", () => InjectTaskWorkerOperationFailureAsync(token)).ConfigureAwait(false);
        await RunStepAsync("task-generic-failure", () => InjectTaskGenericFailureAsync(token)).ConfigureAwait(false);
        await RunStepAsync("task-cancellation", () => InjectTaskCancellationAsync(token)).ConfigureAwait(false);
        await RunStepAsync("broadcaster-disposed-publish", () => InjectBroadcasterDisposedPublishAsync(token)).ConfigureAwait(false);
        await RunStepAsync("lock-contention-timeout", () => InjectLockContentionTimeoutAsync(token)).ConfigureAwait(false);
        await RunStepAsync("lock-double-dispose", () => InjectLockDoubleDisposeAsync(token)).ConfigureAwait(false);
    }

    private async Task RunStepAsync(string name, Func<Task> step)
    {
        Attempted++;
        try
        {
            await step().ConfigureAwait(false);
            Recovered++;
        }
        catch (Exception ex)
        {
            errors.Add($"{name}: unexpected {ex.GetType().Name}: {ex.Message}");
        }
    }

    private async Task InjectMissingSourceCopyAsync(CancellationToken token)
    {
        var source = Path.Combine(probeDirectory, "missing-source.bin");
        var destination = Path.Combine(probeDirectory, "missing-copy.bin");
        try
        {
            await AtomicFileWriter.CopyAtomicallyAsync(source, destination, token).ConfigureAwait(false);
            errors.Add("missing source copy did not throw");
        }
        catch (FileNotFoundException)
        {
        }
        catch (Exception ex)
        {
            errors.Add($"missing source copy threw {ex.GetType().Name}: {ex.Message}");
        }
        VerifyNoPartialFiles();
    }

    private async Task InjectOccupiedDestinationCopyAsync(CancellationToken token)
    {
        var source = Path.Combine(probeDirectory, "source.bin");
        var destination = Path.Combine(probeDirectory, "occupied.bin");
        await File.WriteAllBytesAsync(source, new byte[] { 1, 2, 3 }, token).ConfigureAwait(false);
        await File.WriteAllBytesAsync(destination, new byte[] { 9, 9 }, token).ConfigureAwait(false);
        try
        {
            await AtomicFileWriter.CopyAtomicallyAsync(source, destination, token).ConfigureAwait(false);
            errors.Add("occupied destination copy did not throw");
        }
        catch (IOException)
        {
        }
        catch (Exception ex)
        {
            errors.Add($"occupied destination copy threw {ex.GetType().Name}: {ex.Message}");
        }
        var content = await File.ReadAllBytesAsync(destination, token).ConfigureAwait(false);
        if (!content.SequenceEqual(new byte[] { 9, 9 }))
            errors.Add("occupied destination was overwritten by failed copy");
        VerifyNoPartialFiles();
    }

    private async Task InjectCanceledWriteAsync(CancellationToken token)
    {
        using var canceled = new CancellationTokenSource();
        canceled.Cancel();
        var path = Path.Combine(probeDirectory, "canceled-settings.json");
        try
        {
            await AtomicFileWriter.WriteAllTextAsync(path, "never", canceled.Token).ConfigureAwait(false);
            errors.Add("canceled write did not throw");
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            errors.Add($"canceled write threw {ex.GetType().Name}: {ex.Message}");
        }
        VerifyNoTemporaryFiles();
    }

    private async Task InjectMissingExecutableAsync(CancellationToken token)
    {
        var missing = Path.Combine(root, "missing-tool.exe");
        var result = await runner.RunAsync(missing, new[] { "x" }, null, TimeSpan.FromSeconds(2), token).ConfigureAwait(false);
        if (result.Success || result.ExitCode != -1 ||
            !result.StandardError.Contains("Executable not found", StringComparison.OrdinalIgnoreCase))
            errors.Add($"missing executable returned unexpected result: {result.ExitCode}");
    }

    private async Task InjectProcessTimeoutAsync(CancellationToken token)
    {
        var ping = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "ping.exe");
        if (!File.Exists(ping))
        {
            errors.Add("ping.exe is not available for timeout fault");
            return;
        }
        var result = await runner.RunAsync(
            ping,
            new[] { "127.0.0.1", "-n", "30", "-w", "1000" },
            null,
            TimeSpan.FromMilliseconds(250),
            token).ConfigureAwait(false);
        if (result.Success || result.ExitCode != -2 ||
            !result.StandardError.Contains("Process timed out", StringComparison.OrdinalIgnoreCase))
            errors.Add($"process timeout returned unexpected result: {result.ExitCode}");
    }

    private async Task InjectProcessCancellationAsync(CancellationToken token)
    {
        var ping = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "ping.exe");
        if (!File.Exists(ping))
        {
            errors.Add("ping.exe is not available for cancellation fault");
            return;
        }
        using var canceled = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
        var started = DateTime.UtcNow;
        try
        {
            await runner.RunAsync(
                ping,
                new[] { "127.0.0.1", "-n", "30", "-w", "1000" },
                null,
                TimeSpan.FromMinutes(1),
                canceled.Token).ConfigureAwait(false);
            errors.Add("canceled external process did not throw");
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            errors.Add($"canceled external process threw {ex.GetType().Name}: {ex.Message}");
        }
        if (DateTime.UtcNow - started > TimeSpan.FromSeconds(5))
            errors.Add("external process cancellation was not prompt");
    }

    private async Task InjectNonZeroExitAsync(CancellationToken token)
    {
        var cmd = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe");
        var result = await runner.RunAsync(cmd, new[] { "/c", "exit 7" }, null, TimeSpan.FromSeconds(5), token).ConfigureAwait(false);
        if (result.Success || result.ExitCode != 7)
            errors.Add($"nonzero exit returned unexpected result: {result.ExitCode}");
    }

    private async Task InjectTaskWorkerOperationFailureAsync(CancellationToken token)
    {
        var task = await coordinator.RunAsync(
            "FaultBackup",
            "game-fault",
            "Fault Game",
            (_, _) => Task.FromException(new WorkerOperationException("FAULT_INJECTED", "injected operational failure")),
            token).ConfigureAwait(false);
        if (task.State != TaskState.Failed || !string.Equals(task.ErrorCode, "FAULT_INJECTED", StringComparison.Ordinal))
            errors.Add($"worker operation failure ended as {task.State}/{task.ErrorCode}");
        if (coordinator.Cancel(task.TaskId))
            errors.Add("cancel token remained after worker operation failure");
    }

    private async Task InjectTaskGenericFailureAsync(CancellationToken token)
    {
        var task = await coordinator.RunAsync(
            "FaultBackup",
            "game-generic",
            "Generic Fault Game",
            (_, _) => Task.FromException(new InvalidOperationException("injected generic failure")),
            token).ConfigureAwait(false);
        if (task.State != TaskState.Failed || !string.Equals(task.ErrorCode, "InvalidOperationException", StringComparison.Ordinal))
            errors.Add($"generic failure ended as {task.State}/{task.ErrorCode}");
        if (coordinator.Cancel(task.TaskId))
            errors.Add("cancel token remained after generic failure");
    }

    private async Task InjectTaskCancellationAsync(CancellationToken token)
    {
        var task = await coordinator.RunAsync(
            "FaultBackup",
            "game-cancel",
            "Cancel Fault Game",
            async (_, ct) =>
            {
                await Task.Yield();
                throw new OperationCanceledException(ct);
            },
            token).ConfigureAwait(false);
        if (task.State != TaskState.Cancelled)
            errors.Add($"task cancellation ended as {task.State}");
        if (coordinator.Cancel(task.TaskId))
            errors.Add("cancel token remained after task cancellation");
    }

    private async Task InjectBroadcasterDisposedPublishAsync(CancellationToken token)
    {
        var subscription = broadcaster.Subscribe();
        subscription.Dispose();
        try
        {
            broadcaster.Publish(new TaskChangeEventDto
            {
                Sequence = 1,
                Task = new TaskStatusDto { TaskId = "after-dispose", TaskType = "Fault" }
            });
        }
        catch (Exception ex)
        {
            errors.Add($"publish after dispose threw {ex.GetType().Name}: {ex.Message}");
        }
        if (broadcaster.SubscriberCount != 0)
            errors.Add("disposed subscriber remained registered");
        await Task.Yield();
    }

    private async Task InjectLockContentionTimeoutAsync(CancellationToken token)
    {
        using (var held = await operationLock.AcquireAsync("fault-lock", TimeSpan.FromSeconds(1), token).ConfigureAwait(false))
        {
            if (held == null)
            {
                errors.Add("could not acquire fault lock");
                return;
            }
            var denied = await operationLock.AcquireAsync("fault-lock", TimeSpan.FromMilliseconds(50), token).ConfigureAwait(false);
            if (denied != null)
            {
                denied.Dispose();
                errors.Add("contended fault lock did not time out");
            }
        }
        using (var afterRelease = await operationLock.AcquireAsync("fault-lock", TimeSpan.FromSeconds(1), token).ConfigureAwait(false))
        {
            if (afterRelease == null)
                errors.Add("fault lock was not released after dispose");
        }
    }

    private async Task InjectLockDoubleDisposeAsync(CancellationToken token)
    {
        var lease = await operationLock.AcquireAsync("fault-double", TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
        if (lease == null)
        {
            errors.Add("could not acquire double-dispose lock");
            return;
        }
        lease.Dispose();
        lease.Dispose();
        using var reacquired = await operationLock.AcquireAsync("fault-double", TimeSpan.FromMilliseconds(200), token).ConfigureAwait(false);
        if (reacquired == null)
            errors.Add("double dispose over-released the lock");
    }

    private void VerifyNoPartialFiles()
    {
        if (Directory.EnumerateFiles(probeDirectory, "*.partial").Any())
            errors.Add("atomic copy left a .partial file after failure");
    }

    private void VerifyNoTemporaryFiles()
    {
        if (Directory.EnumerateFiles(probeDirectory, "*.tmp").Any())
            errors.Add("atomic write left a .tmp file after failure");
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
    }
}
