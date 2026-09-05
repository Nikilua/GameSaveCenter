using System.Text;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Ipc;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class IpcMessageBoundaryTests
{
    [Fact]
    public async Task MaximumAllowedLineIsReturnedWithoutTruncation()
    {
        var content = new string('x', ProtocolConstants.MaximumMessageBytes) + "\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        using var reader = new StreamReader(stream, new UTF8Encoding(false));
        var lineReader = new BoundedIpcLineReader(reader);

        var result = await lineReader.ReadAsync(CancellationToken.None);

        Assert.False(result.IsTooLarge);
        Assert.False(result.IsEndOfStream);
        Assert.NotNull(result.Line);
        Assert.Equal(ProtocolConstants.MaximumMessageBytes, result.Line!.Length);
    }

    [Fact]
    public async Task OversizedLineIsDiscardedBeforeTheNextMessageIsRead()
    {
        var content = new string('x', ProtocolConstants.MaximumMessageBytes + 1) + "\nnext\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        using var reader = new StreamReader(stream, new UTF8Encoding(false));
        var lineReader = new BoundedIpcLineReader(reader);

        var oversized = await lineReader.ReadAsync(CancellationToken.None);
        var next = await lineReader.ReadAsync(CancellationToken.None);

        Assert.True(oversized.IsTooLarge);
        Assert.Null(oversized.Line);
        Assert.False(next.IsTooLarge);
        Assert.Equal("next", next.Line);
    }

    [Fact]
    public async Task PendingReadCanBeCancelledBeforeThePipeProducesData()
    {
        using var stream = new BlockingReadStream();
        using var reader = new StreamReader(stream, new UTF8Encoding(false));
        var lineReader = new BoundedIpcLineReader(reader);
        using var cancellation = new CancellationTokenSource();

        var pending = lineReader.ReadAsync(cancellation.Token);
        await stream.ReadStarted;
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
    }

    [Fact]
    public void PendingReadCancellationUsesAReleasableRegistration()
    {
        var root = FindRepositoryRoot();
        var source = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Contracts", "BoundedIpcLineReader.cs"));

        Assert.DoesNotContain("Task.Delay(Timeout.Infinite, token)", source);
        Assert.Contains("token.Register", source);
    }

    [Fact]
    public void PipeEndpointsUseTheSharedBoundedReaderAndFiniteClientSlots()
    {
        var root = FindRepositoryRoot();
        var server = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Ipc", "NamedPipeServerService.cs"));
        var events = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Ipc", "TaskEventPipeServerService.cs"));
        var client = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Ipc", "WorkerIpcClient.cs"));

        Assert.Contains("new BoundedIpcLineReader(reader)", server);
        Assert.Contains("new BoundedIpcLineReader(reader,", client);
        Assert.Contains("WriteEnvelopeAsync", events);
        Assert.Contains("ProtocolConstants.MaximumMessageBytes", events);
        Assert.DoesNotContain("reader.ReadLineAsync", server);
        Assert.DoesNotContain("reader.ReadLineAsync", client);
        Assert.Contains("SemaphoreSlim clientSlots", server);
        Assert.Contains("SemaphoreSlim clientSlots", events);
        Assert.Contains("MESSAGE_TOO_LARGE", server);
        Assert.Contains("MESSAGE_TOO_LARGE", events);
        Assert.Contains("MESSAGE_TOO_LARGE", client);
        Assert.Contains("PipeOptions.CurrentUserOnly", server);
        Assert.Contains("PipeOptions.CurrentUserOnly", events);
        Assert.Contains("RequestId={RequestId}", server);
        Assert.Contains("Type={Type}", server);
        Assert.Contains("ResponseBytes={ResponseBytes}", server);
        Assert.Contains("PayloadBytes={PayloadBytes}", server);
        Assert.Contains("catch(JsonException", server);
        Assert.Contains("catch(IOException)", server);
        Assert.Contains("CancellationToken cancellationToken = default(CancellationToken)", client);
        Assert.Contains("hostCancellationToken", client);
        Assert.Contains("WorkerIpcCancellationReason.HostShutdown", client);
        Assert.Contains("WorkerIpcFailureKind.PipeDisconnected", client);
        Assert.Contains("AwaitPipeOperationAsync", client);
        Assert.Contains("IpcRequestSemantics.RequiresReplayProtection", client);
    }

    [Fact]
    public void MediaInboxRequestsKeepCompatibilityAndAddCursorPages()
    {
        var root = FindRepositoryRoot();
        var dispatcher = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Ipc", "IpcRequestDispatcher.cs"));
        var store = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Persistence", "SqliteStateStore.Media.cs"));
        var viewModel = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "ViewModels", "DashboardViewModel.Media.cs"));

        Assert.Contains("MaximumMediaInboxPageSize = 500", dispatcher);
        Assert.Contains("ClampMediaPageSize(query.Limit)", dispatcher);
        Assert.Contains("query.Offset", dispatcher);
        Assert.Contains("OFFSET $offset", store);
        Assert.Contains("ORDER BY captured_utc DESC, media_id DESC", store);
        Assert.Contains("ListUnassignedMediaPage", dispatcher);
        Assert.Contains("ListIgnoredMediaPage", dispatcher);
        Assert.Contains("GetUnassignedMediaPageAsync", dispatcher);
        Assert.Contains("MediaPageSize = 200", viewModel);
        Assert.Contains("RequestMediaInboxPageAsync", viewModel);
        Assert.Contains("Cursor = reset", viewModel);
        Assert.Contains("ignored ? ignoredMediaPageCursor : unassignedMediaPageCursor", viewModel);
        Assert.DoesNotContain("LoadMediaInboxPagesAsync", viewModel);
    }

    [Theory]
    [InlineData(-10, 1)]
    [InlineData(0, 1)]
    [InlineData(1000, 1000)]
    [InlineData(5000, 1000)]
    public void CurrentGameMediaRequestsAreClampedBeforeStoreAccess(int requested, int expected)
    {
        Assert.Equal(expected, IpcRequestDispatcher.ClampMediaPageSize(requested));
        Assert.Equal(1000, IpcRequestDispatcher.MaximumMediaPageSize);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }

    private sealed class BlockingReadStream : Stream
    {
        private readonly TaskCompletionSource<bool> readStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<int> readCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task ReadStarted => readStarted.Task;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => 0;
        public override long Position { get => 0; set => throw new NotSupportedException(); }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            readStarted.TrySetResult(true);
            return readCompletion.Task;
        }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            readCompletion.TrySetResult(0);
            base.Dispose(disposing);
        }
    }
}
