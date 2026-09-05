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
public sealed class WorkerIpcClientBehaviorTests
{
    private static readonly string TestPipeName = "GameSaveCenterTest" + Guid.NewGuid().ToString("N");
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
            await Task.Delay(450);
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
            await Task.Delay(300);
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
        var server = Task.Run(async () =>
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
    public async Task CancellationDuringLargeWriteIsReportedAsAmbiguousAndIsNotRetried()
    {
        var connected = NewSignal();
        var server = RunServerAsync(async pipe =>
        {
            connected.TrySetResult(true);
            await Task.Delay(450);
        });
        using var cancelled = new CancellationTokenSource();
        var payload = new { Value = new string('x', 3_000_000) };
        var pending = CreateClient().RequestAsync<WorkerPingDto>(
            MessageTypes.BackupGame,
            payload,
            TimeSpan.FromSeconds(5),
            cancelled.Token);
        await WaitForSignalAsync(connected.Task, "server connection", server, pending);
        await Task.Delay(40);
        cancelled.Cancel();

        var exception = await Assert.ThrowsAsync<WorkerIpcCancellationException>(() => AwaitWithTimeout(pending, "cancelled write"));
        await WaitForSignalAsync(server, "server shutdown");

        Assert.Equal(WorkerIpcCancellationReason.Caller, exception.Reason);
        Assert.True(exception.MayHaveBeenAccepted);
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

    private static WorkerIpcClient CreateClient()
        => new(TestPipeName, TestPipeName + ".Events");

    private static NamedPipeServerStream CreateServer()
        => new(TestPipeName, PipeDirection.InOut);

    private static async Task<IpcEnvelope> ReadRequestAsync(NamedPipeServerStream pipe)
    {
        using var reader = new StreamReader(pipe, new UTF8Encoding(false), false, 64 * 1024, true);
        var line = await reader.ReadLineAsync();
        return JsonConvert.DeserializeObject<IpcEnvelope>(line ?? string.Empty)
               ?? throw new InvalidOperationException("Test server received an invalid request.");
    }

    private static async Task RunServerAsync(Func<NamedPipeServerStream, Task> handle)
    {
        using var server = CreateServer();
        await WaitForConnectionAsync(server);
        try { await handle(server); }
        catch (IOException) { }
        catch (ObjectDisposedException) { }
    }

    private static Task WaitForConnectionAsync(NamedPipeServerStream server)
        => Task.Run(() => server.WaitForConnection());

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
