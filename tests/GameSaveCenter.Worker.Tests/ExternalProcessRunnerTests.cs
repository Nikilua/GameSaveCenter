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

    [Fact]
    public async Task Utf8OutputIsDecodedAsUtf8()
    {
        var powershell = Path.Combine(
            Environment.SystemDirectory,
            "WindowsPowerShell",
            "v1.0",
            "powershell.exe");
        if (!File.Exists(powershell)) return;
        var runner = new ExternalProcessRunner(NullLogger<ExternalProcessRunner>.Instance);

        var result = await runner.RunAsync(
            powershell,
            new[]
            {
                "-NoProfile",
                "-Command",
                "[Console]::OutputEncoding=[Text.Encoding]::UTF8; Write-Output '中文错误'"
            },
            null,
            TimeSpan.FromSeconds(30),
            CancellationToken.None);

        Assert.Contains("中文错误", result.StandardOutput);
    }
}
