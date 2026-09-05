using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameSaveCenter.Contracts
{
    /// <summary>Result of reading one bounded newline-delimited IPC message.</summary>
    public sealed class BoundedIpcLineReadResult
    {
        public BoundedIpcLineReadResult(string? line, bool isTooLarge, bool isEndOfStream)
        {
            Line = line;
            IsTooLarge = isTooLarge;
            IsEndOfStream = isEndOfStream;
        }

        public string? Line { get; }
        public bool IsTooLarge { get; }
        public bool IsEndOfStream { get; }
    }

    /// <summary>
    /// Reads a pipe line without first allocating an unbounded string. Once the
    /// shared limit is reached, the rest of that line is consumed and discarded.
    /// </summary>
    public sealed class BoundedIpcLineReader
    {
        private readonly StreamReader reader;
        private readonly Action? cancelPendingRead;
        private readonly char[] buffer = new char[4096];
        private int bufferOffset;
        private int bufferCount;

        public BoundedIpcLineReader(StreamReader reader, Action? cancelPendingRead = null)
        {
            this.reader = reader ?? throw new System.ArgumentNullException(nameof(reader));
            this.cancelPendingRead = cancelPendingRead;
        }

        public async Task<BoundedIpcLineReadResult> ReadAsync(CancellationToken token)
        {
            var builder = new StringBuilder();
            var capturedBytes = 0;
            var isTooLarge = false;
            var hasData = false;
            var pendingCarriageReturn = false;

            while (true)
            {
                var value = await ReadCharacterAsync(token).ConfigureAwait(false);
                if (value < 0)
                {
                    if (!hasData)
                        return new BoundedIpcLineReadResult(null, isTooLarge, true);
                    return new BoundedIpcLineReadResult(
                        isTooLarge ? null : builder.ToString(),
                        isTooLarge,
                        false);
                }

                var character = (char)value;
                if (character == '\n')
                {
                    return new BoundedIpcLineReadResult(
                        isTooLarge ? null : builder.ToString(),
                        isTooLarge,
                        false);
                }

                hasData = true;
                if (pendingCarriageReturn)
                {
                    pendingCarriageReturn = false;
                    if (!isTooLarge) AppendCharacter(builder, '\r', 1, ref capturedBytes, ref isTooLarge);
                }

                if (character == '\r')
                {
                    pendingCarriageReturn = true;
                    continue;
                }

                if (!isTooLarge)
                    AppendCharacter(builder, character, GetUtf8ByteCount(character), ref capturedBytes, ref isTooLarge);
            }
        }

        private async Task<int> ReadCharacterAsync(CancellationToken token)
        {
            if (bufferOffset >= bufferCount)
            {
                var read = reader.ReadAsync(buffer, 0, buffer.Length);
                bufferCount = token.CanBeCanceled
                    ? await AwaitReadAsync(read, token, cancelPendingRead).ConfigureAwait(false)
                    : await read.ConfigureAwait(false);
                bufferOffset = 0;
                if (bufferCount == 0) return -1;
            }
            return buffer[bufferOffset++];
        }

        private static async Task<int> AwaitReadAsync(Task<int> read, CancellationToken token, Action? cancelPendingRead)
        {
            if (read.IsCompleted)
                return await read.ConfigureAwait(false);

            // Do not use an infinite Task.Delay here. Every completed pipe read would leave
            // that delay and its cancellation registration attached to the long-lived event
            // listener until the whole listener token was cancelled. A disposable registration
            // keeps cancellation bounded to this one pending read.
            var cancellation = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using (token.Register(() =>
            {
                cancellation.TrySetResult(true);
                // StreamReader/NamedPipeStream disposal can block briefly while a native
                // read is being completed. Never run that close synchronously inside a
                // CancellationToken callback: Cancel() must return so the caller can
                // observe the intentional cancellation promptly.
                if (cancelPendingRead != null)
                {
                    try
                    {
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            try { cancelPendingRead.Invoke(); } catch { }
                        });
                    }
                    catch { }
                }
            }))
            {
                var completed = await Task.WhenAny(read, cancellation.Task).ConfigureAwait(false);
                if (completed != read) throw new OperationCanceledException(token);
                return await read.ConfigureAwait(false);
            }
        }

        private static int GetUtf8ByteCount(char character)
        {
            if (character <= 0x7f) return 1;
            if (character <= 0x7ff) return 2;
            return 3;
        }

        private static void AppendCharacter(
            StringBuilder builder,
            char character,
            int characterBytes,
            ref int capturedBytes,
            ref bool isTooLarge)
        {
            if (capturedBytes + characterBytes > ProtocolConstants.MaximumMessageBytes)
            {
                isTooLarge = true;
                return;
            }

            builder.Append(character);
            capturedBytes += characterBytes;
        }
    }
}
