using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using GameSaveCenter.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Ipc;

/// <summary>
/// Streams best-effort task events to any currently open Playnite dashboard.
/// It intentionally uses a separate, current-user-only pipe so a long-lived event
/// reader can never delay normal request/response IPC.
/// </summary>
public sealed class TaskEventPipeServerService : BackgroundService
{
    private const int MaximumConcurrentClients = 8;
    private readonly TaskEventBroadcaster broadcaster;
    private readonly ILogger<TaskEventPipeServerService> logger;
    private readonly JsonSerializerOptions json = new(JsonSerializerDefaults.Web);
    private readonly SemaphoreSlim clientSlots = new(MaximumConcurrentClients, MaximumConcurrentClients);

    public TaskEventPipeServerService(TaskEventBroadcaster broadcaster, ILogger<TaskEventPipeServerService> logger)
    {
        this.broadcaster = broadcaster;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var pipe = new NamedPipeServerStream(
                    ProtocolConstants.EventPipeName,
                    PipeDirection.Out,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly,
                    64 * 1024,
                    64 * 1024);
                await pipe.WaitForConnectionAsync(stoppingToken).ConfigureAwait(false);
                _ = StreamEventsAsync(pipe, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Task event pipe accept failed");
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken).ConfigureAwait(false);
            }
        }
    }

    private async Task StreamEventsAsync(NamedPipeServerStream pipe, CancellationToken token)
    {
        if (!clientSlots.Wait(0))
        {
            logger.LogWarning("Task event pipe client limit reached; closing the excess client connection");
            await pipe.DisposeAsync().ConfigureAwait(false);
            return;
        }

        try
        {
            await StreamEventsCoreAsync(pipe, token).ConfigureAwait(false);
        }
        finally
        {
            clientSlots.Release();
        }
    }

    private async Task StreamEventsCoreAsync(NamedPipeServerStream pipe, CancellationToken token)
    {
        await using (pipe)
        using (var subscription = broadcaster.Subscribe())
        await using (var writer = new StreamWriter(pipe, new UTF8Encoding(false), 64 * 1024, true) { AutoFlush = true })
        {
            try
            {
                await foreach (var change in subscription.Reader.ReadAllAsync(token).ConfigureAwait(false))
                {
                    var envelope = new IpcEnvelope
                    {
                        Type = MessageTypes.TaskEvent,
                        PayloadJson = JsonSerializer.Serialize(change, json)
                    };
                    await WriteEnvelopeAsync(writer, envelope).ConfigureAwait(false);
                }
            }
            catch (IOException)
            {
                // The dashboard can close while a progress update is being written.
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Task event client disconnected unexpectedly");
            }
        }
    }

    private async Task WriteEnvelopeAsync(StreamWriter writer, IpcEnvelope envelope)
    {
        var serialized = JsonSerializer.Serialize(envelope, json);
        if (Encoding.UTF8.GetByteCount(serialized) > ProtocolConstants.MaximumMessageBytes)
        {
            logger.LogWarning("Task event exceeded {MaximumMessageBytes} bytes; sending a bounded error", ProtocolConstants.MaximumMessageBytes);
            serialized = JsonSerializer.Serialize(new IpcEnvelope
            {
                RequestId = envelope.RequestId,
                Type = MessageTypes.TaskEvent,
                IsResponse = true,
                Success = false,
                ErrorCode = "MESSAGE_TOO_LARGE",
                ErrorMessage = "IPC event exceeded the configured limit."
            }, json);
        }
        await writer.WriteLineAsync(serialized).ConfigureAwait(false);
    }
}
