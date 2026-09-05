using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GameSaveCenter.Contracts;
using Newtonsoft.Json;

namespace GameSaveCenter.Playnite.Ipc
{
    public enum WorkerIpcFailureKind
    {
        Timeout,
        PipeDisconnected,
        ConnectionFailed,
        RequestInProgress,
        RequestInterrupted,
        ServerRejected
    }

    public enum WorkerIpcCancellationReason
    {
        Caller,
        HostShutdown
    }

    /// <summary>Metadata returned with a response so write submissions can be reconciled.</summary>
    public sealed class WorkerRequestResult<TResponse>
    {
        public WorkerRequestResult(TResponse response, string requestId, string requestType, bool replayed, IReadOnlyList<string> taskIds)
        {
            Response = response;
            RequestId = requestId;
            RequestType = requestType;
            Replayed = replayed;
            TaskIds = taskIds;
        }

        public TResponse Response { get; }
        public string RequestId { get; }
        public string RequestType { get; }
        public bool Replayed { get; }
        public IReadOnlyList<string> TaskIds { get; }
    }

    /// <summary>Cancellation that identifies whether the caller or the host ended the wait.</summary>
    public sealed class WorkerIpcCancellationException : OperationCanceledException
    {
        public WorkerIpcCancellationException(
            WorkerIpcCancellationReason reason,
            string requestId,
            string requestType,
            bool mayHaveBeenAccepted)
            : base(reason == WorkerIpcCancellationReason.HostShutdown ? "Playnite 正在退出，已停止等待 Worker 响应。" : "操作已取消。")
        {
            Reason = reason;
            RequestId = requestId;
            RequestType = requestType;
            MayHaveBeenAccepted = mayHaveBeenAccepted;
        }

        public WorkerIpcCancellationReason Reason { get; }
        public string RequestId { get; }
        public string RequestType { get; }
        public bool MayHaveBeenAccepted { get; }
    }

    /// <summary>Typed Worker error surfaced to the UI or a caller's recovery policy.</summary>
    public sealed class WorkerRequestException : Exception
    {
        public WorkerRequestException(string code, string message)
            : this(code, message, WorkerIpcFailureKind.ServerRejected, string.Empty, string.Empty, false, null) { }

        internal WorkerRequestException(
            string code,
            string message,
            WorkerIpcFailureKind failureKind,
            string requestId,
            string requestType,
            bool mayHaveBeenAccepted,
            Exception? innerException)
            : base(message, innerException)
        {
            Code = code ?? string.Empty;
            FailureKind = failureKind;
            RequestId = requestId ?? string.Empty;
            RequestType = requestType ?? string.Empty;
            MayHaveBeenAccepted = mayHaveBeenAccepted;
        }

        public string Code { get; private set; }
        public WorkerIpcFailureKind FailureKind { get; }
        public string RequestId { get; }
        public string RequestType { get; }
        public bool MayHaveBeenAccepted { get; }
    }

    /// <summary>Short-lived request/response client for the local Worker named pipe.</summary>
    public sealed class WorkerIpcClient
    {
        private readonly string pipeName;
        private readonly string eventPipeName;
        private readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            NullValueHandling = NullValueHandling.Include
        };

        public WorkerIpcClient()
            : this(ProtocolConstants.PipeName, ProtocolConstants.EventPipeName) { }

        internal WorkerIpcClient(string pipeName, string eventPipeName)
        {
            this.pipeName = string.IsNullOrWhiteSpace(pipeName) ? ProtocolConstants.PipeName : pipeName;
            this.eventPipeName = string.IsNullOrWhiteSpace(eventPipeName) ? ProtocolConstants.EventPipeName : eventPipeName;
        }

        public async Task<WorkerHandshakeDto> HandshakeAsync(
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default(CancellationToken),
            CancellationToken hostCancellationToken = default(CancellationToken))
        {
            var handshake = await RequestAsync<WorkerHandshakeDto>(
                MessageTypes.Handshake,
                new { },
                timeout,
                cancellationToken,
                hostCancellationToken).ConfigureAwait(false);
            if (!ProtocolCompatibility.IsCompatible(
                    ProtocolConstants.ProtocolVersion,
                    handshake.ProtocolVersion,
                    handshake.MinimumSupportedProtocolVersion))
            {
                throw new WorkerRequestException(
                    "PROTOCOL_MISMATCH",
                    $"Worker 协议不兼容：客户端 {ProtocolConstants.ProtocolVersion}，服务端 {handshake.ProtocolVersion}（最低支持 {handshake.MinimumSupportedProtocolVersion}）。");
            }
            return handshake;
        }

        public async Task<TResponse> RequestAsync<TResponse>(
            string type,
            object payload,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default(CancellationToken),
            CancellationToken hostCancellationToken = default(CancellationToken))
        {
            var result = await RequestWithTrackingAsync<TResponse>(
                type, payload, timeout, cancellationToken, hostCancellationToken).ConfigureAwait(false);
            return result.Response;
        }

        /// <summary>
        /// Sends one request and returns the envelope ID plus any task IDs in its response.
        /// Destructive requests are replayed once with the same envelope ID after a timeout
        /// or pipe loss. A completed ledger entry returns the original response; an in-flight
        /// or interrupted entry is reported without executing the request again.
        /// </summary>
        public async Task<WorkerRequestResult<TResponse>> RequestWithTrackingAsync<TResponse>(
            string type,
            object payload,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default(CancellationToken),
            CancellationToken hostCancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("IPC request type is required.", nameof(type));
            if (timeout.HasValue && timeout.Value <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));

            var request = new IpcEnvelope
            {
                Type = type,
                PayloadJson = JsonConvert.SerializeObject(payload, jsonSettings)
            };
            var timeoutValue = timeout ?? ProtocolConstants.DefaultRequestTimeout;
            try
            {
                return await ExecuteRequestAsync<TResponse>(
                    request, timeoutValue, cancellationToken, hostCancellationToken, replayed: false).ConfigureAwait(false);
            }
            catch (WorkerRequestException ex)
                when (IpcRequestSemantics.RequiresReplayProtection(type)
                      && ex.MayHaveBeenAccepted
                      && (ex.FailureKind == WorkerIpcFailureKind.Timeout
                          || ex.FailureKind == WorkerIpcFailureKind.PipeDisconnected)
                      && !cancellationToken.IsCancellationRequested
                      && !hostCancellationToken.IsCancellationRequested)
            {
                // The first request may still be running. Reusing the same ID is safe and
                // lets a completed response be recovered without submitting a second backup.
                WorkerRequestException? lastReplayError = null;
                for (var attempt = 0; attempt < 4; attempt++)
                {
                    try
                    {
                        return await ExecuteRequestAsync<TResponse>(
                            request,
                            GetReplayTimeout(timeoutValue),
                            CancellationToken.None,
                            hostCancellationToken,
                            replayed: true).ConfigureAwait(false);
                    }
                    catch (WorkerRequestException replayError)
                    {
                        lastReplayError = replayError;
                        if (replayError.FailureKind != WorkerIpcFailureKind.RequestInProgress || attempt == 3)
                            break;
                        await Task.Delay(TimeSpan.FromMilliseconds(150 * (attempt + 1)), hostCancellationToken)
                            .ConfigureAwait(false);
                    }
                }

                if (lastReplayError != null)
                {
                    throw new WorkerRequestException(
                        lastReplayError.Code,
                        lastReplayError.Message,
                        lastReplayError.FailureKind,
                        request.RequestId,
                        type,
                        mayHaveBeenAccepted: true,
                        lastReplayError);
                }

                throw new InvalidOperationException("IPC replay ended without a response or error.");
            }
        }

        private static TimeSpan GetReplayTimeout(TimeSpan originalTimeout)
        {
            // A replay of a completed ledger entry is fast. Keep reconciliation bounded
            // when the original request used an hours-long business timeout, while still
            // allowing a Worker restart or a short in-flight operation to settle.
            var milliseconds = Math.Max(1000, Math.Min(originalTimeout.TotalMilliseconds, 5000));
            return TimeSpan.FromMilliseconds(milliseconds);
        }

        private async Task<WorkerRequestResult<TResponse>> ExecuteRequestAsync<TResponse>(
            IpcEnvelope request,
            TimeSpan timeoutValue,
            CancellationToken callerToken,
            CancellationToken hostToken,
            bool replayed)
        {
            ThrowIfCancellationRequested(request, callerToken, hostToken, false);

            using (var timeoutCancellation = new CancellationTokenSource(timeoutValue))
            using (var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(callerToken, hostToken, timeoutCancellation.Token))
            using (var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous))
            {
                var mayHaveBeenAccepted = false;
                try
                {
                    await AwaitPipeOperationAsync(
                        ConnectAsync(pipe, (int)Math.Min(int.MaxValue, timeoutValue.TotalMilliseconds)),
                        linkedCancellation.Token,
                        pipe).ConfigureAwait(false);

                    var reader = new StreamReader(pipe, new UTF8Encoding(false), false, 64 * 1024, true);
                    var writer = new StreamWriter(pipe, new UTF8Encoding(false), 64 * 1024, true) { AutoFlush = true };
                    try
                    {
                        var lineReader = new BoundedIpcLineReader(reader, () => QueueDispose(pipe));
                        var line = JsonConvert.SerializeObject(request, jsonSettings);
                        if (Encoding.UTF8.GetByteCount(line) > ProtocolConstants.MaximumMessageBytes)
                            throw new WorkerRequestException(
                                "MESSAGE_TOO_LARGE",
                                "IPC request exceeded the configured limit.",
                                WorkerIpcFailureKind.ServerRejected,
                                request.RequestId,
                                request.Type,
                                false,
                                null);

                        // Mark only destructive requests before starting the write: cancellation
                        // during their write has an ambiguous acceptance boundary and must never
                        // be retried with a new ID. Read-only requests cannot commit business
                        // state merely because their response read was interrupted.
                        mayHaveBeenAccepted = IpcRequestSemantics.RequiresReplayProtection(request.Type);
                        await AwaitPipeOperationAsync(writer.WriteLineAsync(line), linkedCancellation.Token, pipe).ConfigureAwait(false);

                        var responseMessage = await AwaitPipeOperationAsync(
                            lineReader.ReadAsync(linkedCancellation.Token),
                            linkedCancellation.Token,
                            pipe).ConfigureAwait(false);
                        if (responseMessage.IsTooLarge)
                            throw new WorkerRequestException(
                                "MESSAGE_TOO_LARGE",
                                "IPC response exceeded the configured limit.",
                                WorkerIpcFailureKind.ServerRejected,
                                request.RequestId,
                                request.Type,
                                mayHaveBeenAccepted,
                                null);
                        var responseLine = responseMessage.Line;
                        if (string.IsNullOrWhiteSpace(responseLine))
                            throw CreatePipeException(request, mayHaveBeenAccepted, null);
                        var response = JsonConvert.DeserializeObject<IpcEnvelope>(responseLine!, jsonSettings);
                        if (response == null)
                            throw CreatePipeException(request, mayHaveBeenAccepted, null);
                        if (!response.Success)
                        {
                            var kind = response.ErrorCode == "REQUEST_IN_PROGRESS"
                                ? WorkerIpcFailureKind.RequestInProgress
                                : response.ErrorCode == "REQUEST_INTERRUPTED"
                                    ? WorkerIpcFailureKind.RequestInterrupted
                                    : WorkerIpcFailureKind.ServerRejected;
                            throw new WorkerRequestException(
                                response.ErrorCode,
                                response.ErrorMessage,
                                kind,
                                request.RequestId,
                                request.Type,
                                mayHaveBeenAccepted,
                                null);
                        }
                        var payloadResult = JsonConvert.DeserializeObject<TResponse>(response.PayloadJson, jsonSettings);
                        if (payloadResult == null)
                            throw CreatePipeException(request, mayHaveBeenAccepted, null);
                        return new WorkerRequestResult<TResponse>(
                            payloadResult,
                            request.RequestId,
                            request.Type,
                            replayed,
                            ExtractTaskIds(payloadResult));
                    }
                    finally
                    {
                        // Cancellation closes the pipe first to unblock a pending read/write.
                        // StreamReader/StreamWriter disposal may then observe that close; it is
                        // transport cleanup, not a new business failure.
                        try { writer.Dispose(); } catch { }
                        try { reader.Dispose(); } catch { }
                    }
                }
                catch (WorkerIpcCancellationException)
                {
                    throw;
                }
                catch (OperationCanceledException) when (linkedCancellation.IsCancellationRequested)
                {
                    ThrowIfCancellationRequested(request, callerToken, hostToken, mayHaveBeenAccepted);
                    throw CreateTimeoutException(request, mayHaveBeenAccepted, null);
                }
                catch (TimeoutException ex)
                {
                    throw CreateTimeoutException(request, mayHaveBeenAccepted, ex);
                }
                catch (WorkerRequestException)
                {
                    throw;
                }
                catch (IOException ex)
                {
                    throw CreatePipeException(request, mayHaveBeenAccepted, ex);
                }
                catch (UnauthorizedAccessException ex)
                {
                    throw CreateConnectionException(request, mayHaveBeenAccepted, ex);
                }
                catch (ObjectDisposedException ex)
                {
                    throw CreatePipeException(request, mayHaveBeenAccepted, ex);
                }
            }
        }

        private static void ThrowIfCancellationRequested(IpcEnvelope request, CancellationToken callerToken, CancellationToken hostToken, bool mayHaveBeenAccepted)
        {
            if (hostToken.IsCancellationRequested)
                throw new WorkerIpcCancellationException(WorkerIpcCancellationReason.HostShutdown, request.RequestId, request.Type, mayHaveBeenAccepted);
            if (callerToken.IsCancellationRequested)
                throw new WorkerIpcCancellationException(WorkerIpcCancellationReason.Caller, request.RequestId, request.Type, mayHaveBeenAccepted);
        }

        private static WorkerRequestException CreateTimeoutException(IpcEnvelope request, bool mayHaveBeenAccepted, Exception? inner)
            => new(
                "IPC_TIMEOUT",
                mayHaveBeenAccepted ? "Worker 响应超时；请求可能已提交，正在使用同一请求 ID 复核。" : "Worker 连接或响应超时。",
                WorkerIpcFailureKind.Timeout,
                request.RequestId,
                request.Type,
                mayHaveBeenAccepted,
                inner);

        private static WorkerRequestException CreatePipeException(IpcEnvelope request, bool mayHaveBeenAccepted, Exception? inner)
            => new(
                "PIPE_DISCONNECTED",
                mayHaveBeenAccepted ? "Worker 管道已断开；请求可能已提交，不能据此判断业务回滚。" : "Worker 管道已断开。",
                WorkerIpcFailureKind.PipeDisconnected,
                request.RequestId,
                request.Type,
                mayHaveBeenAccepted,
                inner);

        private static WorkerRequestException CreateConnectionException(IpcEnvelope request, bool mayHaveBeenAccepted, Exception? inner)
            => new(
                "PIPE_CONNECTION_FAILED",
                "无法连接到 Worker 管道。",
                WorkerIpcFailureKind.ConnectionFailed,
                request.RequestId,
                request.Type,
                mayHaveBeenAccepted,
                inner);

        private static IReadOnlyList<string> ExtractTaskIds<TResponse>(TResponse response)
        {
            if (response is TaskStatusDto task)
                return string.IsNullOrWhiteSpace(task.TaskId) ? Array.Empty<string>() : new[] { task.TaskId };
            var tasks = response as IEnumerable<TaskStatusDto>;
            return tasks == null
                ? Array.Empty<string>()
                : tasks.Where(task => !string.IsNullOrWhiteSpace(task.TaskId)).Select(task => task.TaskId).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        }

        private static Task ConnectAsync(NamedPipeClientStream pipe, int timeoutMilliseconds)
        {
            // ConnectAsync overloads differ across .NET Framework versions. Running the
            // bounded synchronous call on the pool keeps Playnite's UI responsive.
            return Task.Run(() => pipe.Connect(Math.Max(1, timeoutMilliseconds)));
        }

        private static async Task AwaitPipeOperationAsync(Task operation, CancellationToken token, NamedPipeClientStream pipe)
        {
            if (operation.IsCompleted)
            {
                await operation.ConfigureAwait(false);
                return;
            }

            var cancellation = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using (token.Register(() =>
            {
                cancellation.TrySetResult(true);
                QueueDispose(pipe);
            }))
            {
                if (token.IsCancellationRequested)
                {
                    TryDispose(pipe);
                    ObserveLateOperation(operation);
                    throw new OperationCanceledException(token);
                }
                var completed = await Task.WhenAny(operation, cancellation.Task).ConfigureAwait(false);
                if (completed != operation)
                {
                    TryDispose(pipe);
                    ObserveLateOperation(operation);
                    throw new OperationCanceledException(token);
                }
                await operation.ConfigureAwait(false);
            }
        }

        private static async Task<T> AwaitPipeOperationAsync<T>(Task<T> operation, CancellationToken token, NamedPipeClientStream pipe)
        {
            if (operation.IsCompleted)
                return await operation.ConfigureAwait(false);

            var cancellation = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using (token.Register(() =>
            {
                cancellation.TrySetResult(true);
                QueueDispose(pipe);
            }))
            {
                if (token.IsCancellationRequested)
                {
                    TryDispose(pipe);
                    ObserveLateOperation(operation);
                    throw new OperationCanceledException(token);
                }
                var completed = await Task.WhenAny(operation, cancellation.Task).ConfigureAwait(false);
                if (completed != operation)
                {
                    TryDispose(pipe);
                    ObserveLateOperation(operation);
                    throw new OperationCanceledException(token);
                }
                return await operation.ConfigureAwait(false);
            }
        }

        private static void ObserveLateOperation(Task operation)
        {
            _ = operation.ContinueWith(
                completed => { var ignored = completed.Exception; },
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        private static void TryDispose(IDisposable disposable)
        {
            try { disposable.Dispose(); } catch { }
        }

        private static void QueueDispose(IDisposable disposable)
        {
            try
            {
                if (!ThreadPool.QueueUserWorkItem(_ => TryDispose(disposable)))
                    TryDispose(disposable);
            }
            catch
            {
                TryDispose(disposable);
            }
        }

        /// <summary>
        /// Reads best-effort task events from the Worker while a dashboard is open.
        /// This never replaces normal request/response calls: an unavailable event pipe
        /// simply reconnects in the background and the caller can still use SQLite-backed
        /// snapshots and the bounded change feed.
        /// </summary>
        public async Task ListenForTaskEventsAsync(Func<TaskChangeEventDto, Task> onEvent, CancellationToken token)
        {
            if (onEvent == null) throw new ArgumentNullException(nameof(onEvent));
            var retryDelay = TimeSpan.FromMilliseconds(300);
            while (!token.IsCancellationRequested)
            {
                try
                {
                    using (var pipe = new NamedPipeClientStream(".", eventPipeName, PipeDirection.In, PipeOptions.Asynchronous))
                    {
                        await AwaitPipeOperationAsync(ConnectAsync(pipe, 3000), token, pipe).ConfigureAwait(false);
                        using (var reader = new StreamReader(pipe, new UTF8Encoding(false), false, 64 * 1024, true))
                        {
                            var lineReader = new BoundedIpcLineReader(reader, () => QueueDispose(pipe));
                            retryDelay = TimeSpan.FromMilliseconds(300);
                            while (pipe.IsConnected && !token.IsCancellationRequested)
                            {
                                var message = await lineReader.ReadAsync(token).ConfigureAwait(false);
                                if (message.IsTooLarge) continue;
                                var line = message.Line;
                                if (string.IsNullOrWhiteSpace(line)) break;
                                var envelope = JsonConvert.DeserializeObject<IpcEnvelope>(line!, jsonSettings);
                                if (envelope == null || !envelope.Success || !string.Equals(envelope.Type, MessageTypes.TaskEvent, StringComparison.Ordinal)) continue;
                                var change = JsonConvert.DeserializeObject<TaskChangeEventDto>(envelope.PayloadJson, jsonSettings);
                                if (change != null && change.Task != null) await onEvent(change).ConfigureAwait(false);
                            }
                        }
                    }
                }
                catch (OperationCanceledException) when (token.IsCancellationRequested)
                {
                    break;
                }
                catch (IOException)
                {
                    // The Worker may be starting, restarting, or the dashboard may be closing.
                }
                catch (TimeoutException)
                {
                    // The event endpoint is optional and reconnects without surfacing UI errors.
                }
                catch (Exception)
                {
                    // Keep the event channel isolated from normal plugin operations. The durable
                    // request path will continue to provide task state and diagnostics.
                }

                try { await Task.Delay(retryDelay, token).ConfigureAwait(false); }
                catch (OperationCanceledException) { break; }
                retryDelay = TimeSpan.FromMilliseconds(Math.Min(retryDelay.TotalMilliseconds * 2, 5000));
            }
        }
    }
}
