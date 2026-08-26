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
        var powershell = GetPowerShellPath();
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

    [Fact]
    public async Task OutputIsBoundedPerStreamAndReturnsStableErrorCode()
    {
        var powershell = GetPowerShellPath();
        if (!File.Exists(powershell)) return;
        var runner = new ExternalProcessRunner(NullLogger<ExternalProcessRunner>.Instance);

        var result = await runner.RunAsync(
            powershell,
            new[]
            {
                "-NoProfile",
                "-Command",
                "$payload='x' * (4 * 1024 * 1024 + 128); [Console]::Out.Write($payload); [Console]::Error.Write($payload)"
            },
            null,
            TimeSpan.FromSeconds(30),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(0, result.ExitCode);
        Assert.Equal("PROCESS_OUTPUT_LIMIT_EXCEEDED", result.ErrorCode);
        Assert.True(result.StandardOutput.Length <= 4 * 1024 * 1024);
        Assert.True(result.StandardError.Length <= 4 * 1024 * 1024);
    }

    [Fact]
    public async Task FailedProcessPreservesExitCodeAndOutput()
    {
        var powershell = GetPowerShellPath();
        if (!File.Exists(powershell)) return;
        var runner = new ExternalProcessRunner(NullLogger<ExternalProcessRunner>.Instance);

        var result = await runner.RunAsync(
            powershell,
            new[] { "-NoProfile", "-Command", "[Console]::Error.Write('expected failure'); exit 7" },
            null,
            TimeSpan.FromSeconds(30),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(7, result.ExitCode);
        Assert.Equal(string.Empty, result.ErrorCode);
        Assert.Contains("expected failure", result.StandardError);
    }

    [Fact]
    public async Task TimeoutReturnsStableCodeAndKeepsTimeoutMessage()
    {
        var powershell = GetPowerShellPath();
        if (!File.Exists(powershell)) return;
        var runner = new ExternalProcessRunner(NullLogger<ExternalProcessRunner>.Instance);

        var result = await runner.RunAsync(
            powershell,
            new[] { "-NoProfile", "-Command", "Start-Sleep -Seconds 30" },
            null,
            TimeSpan.FromMilliseconds(250),
            CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(-2, result.ExitCode);
        Assert.Equal("PROCESS_TIMED_OUT", result.ErrorCode);
        Assert.Contains("Process timed out.", result.StandardError);
    }

    private static string GetPowerShellPath()
        => Path.Combine(Environment.SystemDirectory, "WindowsPowerShell", "v1.0", "powershell.exe");
}
