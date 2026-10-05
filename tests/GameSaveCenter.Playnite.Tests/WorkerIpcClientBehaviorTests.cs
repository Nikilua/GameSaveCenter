using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Ipc;
using Newtonsoft.Json;
using Xunit;

namespace GameSaveCenter.Playnite.Tests;

[Collection("WorkerPipe")]
public sealed class WorkerIpcClientBehaviorTests : IAsyncLifetime
{
    // xUnit creates one instance per test. A failed scenario cannot occupy the next
    // scenario's pipe, and DisposeAsync also closes servers when an assertion fails.
    private readonly string TestPipeName = "GameSaveCenterTest" + Guid.NewGuid().ToString("N");
    private readonly List<NamedPipeServerStream> servers = new();
    private readonly List<Task> serverTasks = new();
    private readonly TaskCompletionSource<bool> releaseServer = NewSignal();
    private bool disposing;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        lock (servers)
        {
            disposing = true;
            releaseServer.TrySetResult(true);
            foreach (var server in servers) server.Dispose();
        }
        var cleanup = Task.WhenAll(serverTasks);
        if (await Task.WhenAny(cleanup, Task.Delay(TimeSpan.FromSeconds(3))) != cleanup)
            throw new TimeoutException("Test pipe server did not stop during cleanup.");
        await cleanup;
    }

    private Task StartServer(Func<Task> handle)
    {
        var task = Task.Run(async () =>
        {
            try { await handle(); }
            catch (IOException) when (disposing) { }
            catch (ObjectDisposedException) when (disposing) { }
        });
        serverTasks.Add(task);
        return task;
    }

    [Fact]
    public void IsolatedAuditEnvironmentOverridesTheProductionPipePair()
    {
        const string pipeVariable = "GSC_UI_AUDIT_PIPE_NAME";
        const string eventPipeVariable = "GSC_UI_AUDIT_EVENT_PIPE_NAME";
        var originalPipe = Environment.GetEnvironmentVariable(pipeVariable);
        var originalEventPipe = Environment.GetEnvironmentVariable(eventPipeVariable);
        var pipeName = "GameSaveCenterAuditTest" + Guid.NewGuid().ToString("N");
        var eventPipeName = pipeName + ".Events";
        try
        {
            Environment.SetEnvironmentVariable(pipeVariable, pipeName);
            Environment.SetEnvironmentVariable(eventPipeVariable, eventPipeName);
            var client = new WorkerIpcClient();

            Assert.Equal(pipeName, client.PipeNameForDiagnostics);
            Assert.Equal(eventPipeName, client.EventPipeNameForDiagnostics);
        }
        finally
        {
            Environment.SetEnvironmentVariable(pipeVariable, originalPipe);
            Environment.SetEnvironmentVariable(eventPipeVariable, originalEventPipe);
        }
    }

    [NamedPipeFact]
    public async Task CallerCancellationBeforeConnectDoesNotOpenARequest()
    {
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();

        var exception = await Assert.ThrowsAsync<WorkerIpcCancellationException>(() =>
            CreateClient().RequestAsync<WorkerPingDto>(
                MessageTypes.GetDashboard,
                new { },
                TimeSpan.FromSeconds(2),
                cancelled.Token));

        Assert.Equal(WorkerIpcCancellationReason.Caller, exception.Reason);
        Assert.False(exception.MayHaveBeenAccepted);
    }

    [NamedPipeFact]
    public async Task CallerCancellationDuringReadClosesThePipeAndReturnsPromptly()
    {
        var connected = NewSignal();
        var received = NewSignal();
        var server = RunServerAsync(async pipe =>
        {
            connected.TrySetResult(true);
            await ReadRequestAsync(pipe);
            received.TrySetResult(true);
            await releaseServer.Task;
        });

        using var cancelled = new CancellationTokenSource();
        var client = CreateClient();
        var pending = client.RequestAsync<WorkerPingDto>(
            MessageTypes.GetDashboard,
            new { },
            TimeSpan.FromSeconds(5),
            cancelled.Token);
        await WaitForSignalAsync(connected.Task, "server connection", server, pending);
        await WaitForSignalAsync(received.Task, "request receipt", server, pending);
        var stopwatch = Stopwatch.StartNew();
        cancelled.Cancel();

        var exception = await Assert.ThrowsAsync<WorkerIpcCancellationException>(() => AwaitWithTimeout(pending, "cancelled read"));
        stopwatch.Stop();
        releaseServer.TrySetResult(true);
        await WaitForSignalAsync(server, "server shutdown");

        Assert.Equal(WorkerIpcCancellationReason.Caller, exception.Reason);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(1), $"Cancellation took {stopwatch.Elapsed}.");
        Assert.True(exception.MayHaveBeenAccepted == false, "Read-only request cancellation must not be presented as a committed write.");
    }

    [NamedPipeFact]
    public async Task HostShutdownDuringReadIsDistinctFromCallerCancellation()
    {
        var connected = NewSignal();
        var server = RunServerAsync(async pipe =>
        {
            connected.TrySetResult(true);
            await ReadRequestAsync(pipe);
            await releaseServer.Task;
        });

        using var hostStopping = new CancellationTokenSource();
        var pending = CreateClient().RequestAsync<WorkerPingDto>(
            MessageTypes.GetDashboard,
            new { },
            TimeSpan.FromSeconds(5),
            CancellationToken.None,
            hostStopping.Token);
        await WaitForSignalAsync(connected.Task, "server connection", server, pending);
        hostStopping.Cancel();

        var exception = await Assert.ThrowsAsync<WorkerIpcCancellationException>(() => AwaitWithTimeout(pending, "host-cancelled read"));
        releaseServer.TrySetResult(true);
        await WaitForSignalAsync(server, "server shutdown");

        Assert.Equal(WorkerIpcCancellationReason.HostShutdown, exception.Reason);
        Assert.False(exception.MayHaveBeenAccepted);
    }

    [NamedPipeFact]
    public async Task LostWriteResponseIsRecoveredWithTheSameRequestId()
    {
        var firstConnected = NewSignal();
        var secondConnected = NewSignal();
        var seenRequestIds = new List<string>();
        var server = StartServer(async () =>
        {
            using (var first = CreateServer())
            {
                await WaitForConnectionAsync(first);
                firstConnected.TrySetResult(true);
                var request = await ReadRequestAsync(first);
                seenRequestIds.Add(request.RequestId);
            }

            using (var second = CreateServer())
            {
                await WaitForConnectionAsync(second);
                secondConnected.TrySetResult(true);
                var request = await ReadRequestAsync(second);
                seenRequestIds.Add(request.RequestId);
                using var writer = new StreamWriter(second, new UTF8Encoding(false), 64 * 1024, true) { AutoFlush = true };
                await writer.WriteLineAsync(JsonConvert.SerializeObject(new IpcEnvelope
                {
                    RequestId = request.RequestId,
                    Type = request.Type,
                    IsResponse = true,
                    Success = true,
                    PayloadJson = JsonConvert.SerializeObject(new WorkerPingDto { Version = "test" })
                }));
            }
        });

        var resultTask = CreateClient().RequestWithTrackingAsync<WorkerPingDto>(
            MessageTypes.BackupGame,
            new { },
            TimeSpan.FromSeconds(3));
        await WaitForSignalAsync(firstConnected.Task, "first server connection", server, resultTask);
        await WaitForSignalAsync(secondConnected.Task, "replay server connection", server, resultTask);
        var result = await AwaitWithTimeout(resultTask, "recovered request");
        await WaitForSignalAsync(server, "server shutdown");

        Assert.True(result.Replayed);
        Assert.Equal("test", result.Response.Version);
        Assert.Equal(2, seenRequestIds.Count);
        Assert.NotEqual(string.Empty, seenRequestIds[0]);
        Assert.Equal(seenRequestIds[0], seenRequestIds[1]);
        Assert.Empty(result.TaskIds);
    }

    [NamedPipeFact]
    public async Task CallerCancellationDuringReplayWaitStopsWithAmbiguousOutcome()
    {
        var replayConnected = NewSignal();
        var seenRequestIds = new List<string>();
        var server = StartServer(async () =>
        {
            using (var first = CreateServer())
            {
                await WaitForConnectionAsync(first);
                seenRequestIds.Add((await ReadRequestAsync(first)).RequestId);
            }

            using (var replay = CreateServer())
            {
                await WaitForConnectionAsync(replay);
                seenRequestIds.Add((await ReadRequestAsync(replay)).RequestId);
                replayConnected.TrySetResult(true);
                await releaseServer.Task;
            }
        });

        using var cancelled = new CancellationTokenSource();
        var pending = CreateClient().RequestAsync<WorkerPingDto>(
            MessageTypes.BackupGame,
            new { },
            TimeSpan.FromSeconds(2),
            cancelled.Token);
        await WaitForSignalAsync(replayConnected.Task, "replay server connection", server, pending);
        cancelled.Cancel();

        var exception = await Assert.ThrowsAsync<WorkerIpcCancellationException>(() => AwaitWithTimeout(pending, "cancelled replay"));
        releaseServer.TrySetResult(true);
        await WaitForSignalAsync(server, "server shutdown");

        Assert.Equal(WorkerIpcCancellationReason.Caller, exception.Reason);
        Assert.True(exception.MayHaveBeenAccepted);
        Assert.False(string.IsNullOrWhiteSpace(exception.RequestId));
        Assert.Equal(2, seenRequestIds.Count);
        Assert.All(seenRequestIds, id => Assert.Equal(exception.RequestId, id));
    }

    [NamedPipeFact]
    public async Task CancellationDuringLargeWriteIsReportedAsAmbiguousAndIsNotRetried()
    {
        var connected = NewSignal();
        var writing = NewSignal();
        var server = RunServerAsync(async pipe =>
        {
            connected.TrySetResult(true);
            // Consume one byte to prove the write started, then keep the small
            // pipe buffer blocked until cancellation has returned to the caller.
            var firstByte = new byte[1];
            Assert.Equal(1, await pipe.ReadAsync(firstByte, 0, firstByte.Length));
            writing.TrySetResult(true);
            await releaseServer.Task;
        });
        using var cancelled = new CancellationTokenSource();
        var payload = new { Value = new string('x', 3_000_000) };
        var pending = CreateClient().RequestAsync<WorkerPingDto>(
            MessageTypes.BackupGame,
            payload,
            TimeSpan.FromSeconds(5),
            cancelled.Token);
        await WaitForSignalAsync(connected.Task, "server connection", server, pending);
        await WaitForSignalAsync(writing.Task, "write started", server, pending);
        cancelled.Cancel();

        var exception = await Assert.ThrowsAsync<WorkerIpcCancellationException>(() => AwaitWithTimeout(pending, "cancelled write"));
        releaseServer.TrySetResult(true);
        await WaitForSignalAsync(server, "server shutdown");

        Assert.Equal(WorkerIpcCancellationReason.Caller, exception.Reason);
        Assert.True(exception.MayHaveBeenAccepted);
        Assert.False(string.IsNullOrWhiteSpace(exception.RequestId));
    }

    [NamedPipeFact]
    public async Task ReplayDisconnectBeforeCallerCancellationRemainsAnAmbiguousTransportFailure()
    {
        var seenRequestIds = new List<string>();
        var server = StartServer(async () =>
        {
            // A server that closes before the caller cancels reproduces the old
            // fixed-delay fixture's alternative outcome without a scheduler race.
            for (var index = 0; index < 2; index++)
            {
                using var pipe = CreateServer();
                await WaitForConnectionAsync(pipe);
                seenRequestIds.Add((await ReadRequestAsync(pipe)).RequestId);
            }
        });
        using var cancelled = new CancellationTokenSource();
        var pending = CreateClient().RequestAsync<WorkerPingDto>(
            MessageTypes.BackupGame, new { }, TimeSpan.FromSeconds(2), cancelled.Token);
        var exception = await Assert.ThrowsAsync<WorkerRequestException>(() => AwaitWithTimeout(pending, "disconnected replay"));
        cancelled.Cancel();
        await WaitForSignalAsync(server, "server shutdown");

        Assert.Equal(WorkerIpcFailureKind.PipeDisconnected, exception.FailureKind);
        Assert.True(exception.MayHaveBeenAccepted);
        Assert.Equal(2, seenRequestIds.Count);
        Assert.All(seenRequestIds, id => Assert.Equal(exception.RequestId, id));
    }

    [NamedPipeFact]
    public async Task FixtureCleanupReleasesAnUnconnectedServerAfterScenarioFailure()
    {
        var scenario = new WorkerIpcClientBehaviorTests();
        var ready = NewSignal();
        var server = scenario.StartServer(async () =>
        {
            using var pipe = scenario.CreateServer();
            ready.TrySetResult(true);
            await WaitForConnectionAsync(pipe);
        });
        try
        {
            await WaitForSignalAsync(ready.Task, "fixture server ready", server);
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await Task.Yield();
                throw new InvalidOperationException("Controlled scenario failure before connecting.");
            });
        }
        finally
        {
            await scenario.DisposeAsync();
        }

        Assert.True(server.IsCompleted);
        // The same name and single-instance limit must be available immediately.
        using var replacement = new NamedPipeServerStream(scenario.TestPipeName, PipeDirection.InOut);
    }

    [NamedPipeFact]
    public async Task FixtureCleanupReleasesAConnectedServerAfterScenarioFailure()
    {
        var scenario = new WorkerIpcClientBehaviorTests();
        var received = NewSignal();
        var server = scenario.RunServerAsync(async pipe =>
        {
            await ReadRequestAsync(pipe);
            received.TrySetResult(true);
            await scenario.releaseServer.Task;
        });
        using var cancelled = new CancellationTokenSource();
        var pending = scenario.CreateClient().RequestAsync<WorkerPingDto>(
            MessageTypes.GetDashboard, new { }, TimeSpan.FromSeconds(2), cancelled.Token);
        try
        {
            await WaitForSignalAsync(received.Task, "fixture request receipt", server, pending);
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await Task.Yield();
                throw new InvalidOperationException("Controlled scenario failure after connecting.");
            });
        }
        finally
        {
            cancelled.Cancel();
            await scenario.DisposeAsync();
        }
        await Assert.ThrowsAsync<WorkerIpcCancellationException>(() => AwaitWithTimeout(pending, "fixture request cleanup"));
        Assert.True(server.IsCompleted);
        using var replacement = new NamedPipeServerStream(scenario.TestPipeName, PipeDirection.InOut);
    }

    private static TaskCompletionSource<bool> NewSignal()
        => new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static async Task WaitForSignalAsync(Task signal, string name, params Task[] failures)
    {
        var timeout = Task.Delay(TimeSpan.FromSeconds(3));
        var candidates = new Task[failures.Length + 2];
        candidates[0] = signal;
        Array.Copy(failures, 0, candidates, 1, failures.Length);
        candidates[candidates.Length - 1] = timeout;
        var completed = await Task.WhenAny(candidates);
        if (completed != signal && completed != timeout)
            await completed;
        if (completed != signal) throw new TimeoutException($"Timed out waiting for {name}.");
        await signal;
    }

    private static async Task<T> AwaitWithTimeout<T>(Task<T> operation, string name)
    {
        var completed = await Task.WhenAny(operation, Task.Delay(TimeSpan.FromSeconds(3)));
        if (completed != operation) throw new TimeoutException($"Timed out waiting for {name}.");
        return await operation;
    }

    private WorkerIpcClient CreateClient()
        => new(TestPipeName, TestPipeName + ".Events");

    private NamedPipeServerStream CreateServer()
    {
        lock (servers)
        {
            if (disposing) throw new ObjectDisposedException(nameof(WorkerIpcClientBehaviorTests));
            var server = new NamedPipeServerStream(TestPipeName, PipeDirection.InOut, 1,
                PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 4096, 4096);
            servers.Add(server);
            return server;
        }
    }

    private static async Task<IpcEnvelope> ReadRequestAsync(NamedPipeServerStream pipe)
    {
        using var reader = new StreamReader(pipe, new UTF8Encoding(false), false, 64 * 1024, true);
        var line = await reader.ReadLineAsync();
        return JsonConvert.DeserializeObject<IpcEnvelope>(line ?? string.Empty)
               ?? throw new InvalidOperationException("Test server received an invalid request.");
    }

    private Task RunServerAsync(Func<NamedPipeServerStream, Task> handle)
        => StartServer(async () =>
    {
        using var server = CreateServer();
        await WaitForConnectionAsync(server);
        try { await handle(server); }
        catch (IOException) { }
        catch (ObjectDisposedException) { }
    });

    private static Task WaitForConnectionAsync(NamedPipeServerStream server)
        => server.WaitForConnectionAsync();

}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class NamedPipeFactAttribute : FactAttribute
{
    public NamedPipeFactAttribute()
    {
        if (!NamedPipeTestSupport.IsAvailable)
            Skip = "当前执行环境禁止创建本地 Named Pipe 客户端；在完整 Windows/Playnite 环境执行该行为套件。";
    }
}

internal static class NamedPipeTestSupport
{
    public static readonly bool IsAvailable = ProbeNamedPipeAccess();

    private static bool ProbeNamedPipeAccess()
    {
        var name = "GameSaveCenterProbe" + Guid.NewGuid().ToString("N");
        using (var server = new NamedPipeServerStream(name, PipeDirection.InOut))
        using (var client = new NamedPipeClientStream(".", name, PipeDirection.InOut))
        {
            var serverTask = Task.Run(() =>
            {
                try { server.WaitForConnection(); return true; }
                catch { return false; }
            });
            var clientTask = Task.Run(() =>
            {
                try { client.Connect(1000); return true; }
                catch { return false; }
            });
            clientTask.Wait(2000);
            var available = clientTask.IsCompleted && clientTask.Result;
            server.Dispose();
            serverTask.Wait(2000);
            return available;
        }
    }
}

[CollectionDefinition("WorkerPipe", DisableParallelization = true)]
public sealed class WorkerPipeCollection { }
