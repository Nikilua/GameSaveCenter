using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using GameSaveCenter.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Ipc;

/// <summary>Current-user-only newline-delimited JSON named-pipe server.</summary>
public sealed class NamedPipeServerService : BackgroundService
{
    private const int MaximumConcurrentClients = 32;
    private readonly IpcRequestDispatcher _dispatcher;
    private readonly ILogger<NamedPipeServerService> _logger;
    private readonly JsonSerializerOptions _json=new(JsonSerializerDefaults.Web){PropertyNameCaseInsensitive=true};
    private readonly SemaphoreSlim clientSlots = new(MaximumConcurrentClients, MaximumConcurrentClients);

    public NamedPipeServerService(IpcRequestDispatcher dispatcher,ILogger<NamedPipeServerService> logger)
    { _dispatcher=dispatcher;_logger=logger; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var pipe=new NamedPipeServerStream(ProtocolConstants.PipeName,PipeDirection.InOut,NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,PipeOptions.Asynchronous|PipeOptions.CurrentUserOnly,64*1024,64*1024);
                await pipe.WaitForConnectionAsync(stoppingToken).ConfigureAwait(false);
                _=HandleClientAsync(pipe,stoppingToken);
            }
            catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested){break;}
            catch(Exception ex){_logger.LogError(ex,"Named pipe accept failed");await Task.Delay(1000,stoppingToken).ConfigureAwait(false);}
        }
    }

    private async Task HandleClientAsync(NamedPipeServerStream pipe,CancellationToken token)
    {
        if (!clientSlots.Wait(0))
        {
            _logger.LogWarning("Named pipe client limit reached; closing the excess client connection");
            await pipe.DisposeAsync().ConfigureAwait(false);
            return;
        }

        try
        {
            await HandleClientCoreAsync(pipe, token).ConfigureAwait(false);
        }
        finally
        {
            clientSlots.Release();
        }
    }

    private async Task HandleClientCoreAsync(NamedPipeServerStream pipe,CancellationToken token)
    {
        await using (pipe)
        {
            using var reader=new StreamReader(pipe,new UTF8Encoding(false),false,64*1024,true);
            var lineReader = new BoundedIpcLineReader(reader);
            await using var writer=new StreamWriter(pipe,new UTF8Encoding(false),64*1024,true){AutoFlush=true};
            try
            {
                while(pipe.IsConnected&&!token.IsCancellationRequested)
                {
                    var message = await lineReader.ReadAsync(token).ConfigureAwait(false);
                    if (message.IsTooLarge)
                    {
                        await WriteErrorAsync(writer, "MESSAGE_TOO_LARGE", "IPC message exceeded the configured limit.").ConfigureAwait(false);
                        continue;
                    }
                    if (message.Line == null) break;
                    var line = message.Line;
                    IpcEnvelope? request;
                    try{request=JsonSerializer.Deserialize<IpcEnvelope>(line,_json);}catch(JsonException ex)
                    {
                        await WriteErrorAsync(writer, "INVALID_JSON", ex.Message).ConfigureAwait(false);continue;
                    }
                    if(request==null)continue;
                    var response=await _dispatcher.DispatchAsync(request,token).ConfigureAwait(false);
                    await WriteEnvelopeAsync(writer, response).ConfigureAwait(false);
                }
            }
            catch(IOException){/* Client closed while a response was in flight. */}
            catch(OperationCanceledException) when(token.IsCancellationRequested){ }
            catch(Exception ex){_logger.LogWarning(ex,"Named pipe client failed");}
        }
    }

    private async Task WriteErrorAsync(StreamWriter writer, string code, string message)
        => await WriteEnvelopeAsync(writer, new IpcEnvelope
        {
            IsResponse = true,
            Success = false,
            ErrorCode = code,
            ErrorMessage = message
        }).ConfigureAwait(false);

    private async Task WriteEnvelopeAsync(StreamWriter writer, IpcEnvelope envelope)
    {
        var serialized = JsonSerializer.Serialize(envelope, _json);
        var responseBytes = Encoding.UTF8.GetByteCount(serialized);
        if (responseBytes > ProtocolConstants.MaximumMessageBytes)
        {
            var payloadBytes = Encoding.UTF8.GetByteCount(envelope.PayloadJson ?? string.Empty);
            _logger.LogWarning(
                "Named pipe response exceeded {MaximumMessageBytes} bytes; returning a bounded error. RequestId={RequestId} Type={Type} ResponseBytes={ResponseBytes} PayloadBytes={PayloadBytes}",
                ProtocolConstants.MaximumMessageBytes,
                envelope.RequestId,
                envelope.Type,
                responseBytes,
                payloadBytes);
            serialized = JsonSerializer.Serialize(new IpcEnvelope
            {
                RequestId = envelope.RequestId,
                Type = envelope.Type,
                IsResponse = true,
                Success = false,
                ErrorCode = "MESSAGE_TOO_LARGE",
                ErrorMessage = "IPC response exceeded the configured limit."
            }, _json);
        }
        await writer.WriteLineAsync(serialized).ConfigureAwait(false);
    }
}
