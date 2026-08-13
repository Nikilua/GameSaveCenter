using GameSaveCenter.Worker.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class ExternalProcessRunnerTests
{
    [Fact]
    public async Task CancellationStopsLongRunningExternalTransferPromptly()
    {
        var executable = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "ping.exe");
        if (!File.Exists(executable)) return;
        var runner = new ExternalProcessRunner(NullLogger<ExternalProcessRunner>.Instance);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(250));
        var started = DateTime.UtcNow;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => runner.RunAsync(
            executable, new[] { "127.0.0.1", "-n", "30", "-w", "1000" }, null,
            TimeSpan.FromMinutes(1), cancellation.Token));

        Assert.True(DateTime.UtcNow - started < TimeSpan.FromSeconds(5));
    }
}
