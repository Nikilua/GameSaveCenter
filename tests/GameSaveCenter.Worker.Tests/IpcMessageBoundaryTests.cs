using System.Text;
using GameSaveCenter.Contracts;
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
    public void PipeEndpointsUseTheSharedBoundedReaderAndFiniteClientSlots()
    {
        var root = FindRepositoryRoot();
        var server = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Ipc", "NamedPipeServerService.cs"));
        var events = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Worker", "Ipc", "TaskEventPipeServerService.cs"));
        var client = File.ReadAllText(Path.Combine(root, "src", "GameSaveCenter.Playnite", "Ipc", "WorkerIpcClient.cs"));

        Assert.Contains("new BoundedIpcLineReader(reader)", server);
        Assert.Contains("new BoundedIpcLineReader(reader)", client);
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
        Assert.Contains("catch(JsonException", server);
        Assert.Contains("catch(IOException)", server);
        Assert.Contains("catch (OperationCanceledException) when (cancellation.IsCancellationRequested)", client);
        Assert.Contains("throw new TimeoutException(\"Worker response timed out.\")", client);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "GameSaveCenter.sln")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
