using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Infrastructure;

/// <summary>Runs trusted local executables without invoking a shell.</summary>
public sealed class ExternalProcessRunner
{
    private readonly ILogger<ExternalProcessRunner> _logger;

    public ExternalProcessRunner(ILogger<ExternalProcessRunner> logger) => _logger = logger;

    public async Task<ProcessResult> RunAsync(
        string executable,
        IEnumerable<string> arguments,
        string? standardInput,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(executable) || !File.Exists(executable))
        {
            return ProcessResult.Failed(
                -1,
                string.Empty,
                $"Executable not found: {executable}",
                ProcessExecutionLimits.ExecutableNotFoundCode);
        }

        var start = new ProcessStartInfo
        {
            FileName = executable,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = standardInput != null,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(executable) ?? Environment.CurrentDirectory
        };
        foreach (var argument in arguments) start.ArgumentList.Add(argument);

        using var process = new Process { StartInfo = start, EnableRaisingEvents = true };
        // Arguments can contain local profile paths, remote names or provider-specific
        // options. Keep operational logs useful without persisting the full command line.
        _logger.LogInformation("Starting {Executable} with {ArgumentCount} argument(s)", Path.GetFileName(executable), start.ArgumentList.Count);
        process.Start();

        if (standardInput != null)
        {
            await process.StandardInput.WriteAsync(standardInput).ConfigureAwait(false);
            process.StandardInput.Close();
        }

        var stdoutTask = ReadBoundedAsync(process.StandardOutput, cancellationToken);
        var stderrTask = ReadBoundedAsync(process.StandardError, cancellationToken);
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);

        try
        {
            await process.WaitForExitAsync(timeoutCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Cancelling external process {Executable}", Path.GetFileName(executable));
            TryKill(process);
            await ObserveOutputAsync(stdoutTask).ConfigureAwait(false);
            await ObserveOutputAsync(stderrTask).ConfigureAwait(false);
            throw;
        }
        catch (OperationCanceledException)
        {
            TryKill(process);
            var timedOutOutput = await CaptureOutputAfterTerminationAsync(process, stdoutTask, stderrTask).ConfigureAwait(false);
            return ProcessResult.Failed(
                -2,
                timedOutOutput.Text,
                "Process timed out.",
                ProcessExecutionLimits.TimeoutCode);
        }

        var stdout = await stdoutTask.ConfigureAwait(false);
        var stderr = await stderrTask.ConfigureAwait(false);
        var errorCode = stdout.WasTruncated || stderr.WasTruncated
            ? ProcessExecutionLimits.OutputLimitExceededCode
            : string.Empty;
        if (!string.IsNullOrEmpty(errorCode))
        {
            _logger.LogWarning(
                "External process output was limited to {MaximumOutputBytes} bytes per stream; stdoutLimited={StdoutLimited}, stderrLimited={StderrLimited}",
                ProcessExecutionLimits.MaximumOutputBytes,
                stdout.WasTruncated,
                stderr.WasTruncated);
        }

        return new ProcessResult(process.ExitCode, stdout.Text, stderr.Text, errorCode);
    }

    private static async Task<ProcessStreamCapture> ReadBoundedAsync(
        StreamReader reader,
        CancellationToken cancellationToken)
    {
        var builder = new StringBuilder();
        var buffer = new char[8192];
        var capturedBytes = 0;
        var wasTruncated = false;

        while (true)
        {
            var count = await reader.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false);
            if (count == 0) break;
            if (wasTruncated) continue;

            var remainingBytes = ProcessExecutionLimits.MaximumOutputBytes - capturedBytes;
            var take = GetUtf8PrefixLength(buffer, count, remainingBytes);
            if (take > 0)
            {
                builder.Append(buffer, 0, take);
                capturedBytes += Encoding.UTF8.GetByteCount(buffer, 0, take);
            }

            if (take < count) wasTruncated = true;
        }

        return new ProcessStreamCapture(builder.ToString(), wasTruncated);
    }

    private static int GetUtf8PrefixLength(char[] buffer, int count, int remainingBytes)
    {
        if (remainingBytes <= 0) return 0;
        var bytes = 0;
        var take = 0;
        while (take < count)
        {
            var charBytes = Encoding.UTF8.GetByteCount(buffer, take, 1);
            if (bytes + charBytes > remainingBytes) break;
            bytes += charBytes;
            take++;
        }
        return take;
    }

    private static async Task<ProcessStreamCapture> CaptureOutputAfterTerminationAsync(
        Process process,
        Task<ProcessStreamCapture> stdoutTask,
        Task<ProcessStreamCapture> stderrTask)
    {
        try
        {
            await process.WaitForExitAsync().ConfigureAwait(false);
        }
        catch
        {
            // The timeout result remains authoritative if the process exits between kill and wait.
        }

        var stdout = await ObserveOutputAsync(stdoutTask).ConfigureAwait(false);
        await ObserveOutputAsync(stderrTask).ConfigureAwait(false);
        return stdout;
    }

    private static async Task<ProcessStreamCapture> ObserveOutputAsync(Task<ProcessStreamCapture> outputTask)
    {
        try
        {
            return await outputTask.ConfigureAwait(false);
        }
        catch
        {
            return new ProcessStreamCapture(string.Empty, false);
        }
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited) process.Kill(entireProcessTree: true);
        }
        catch
        {
            // The process may have exited between checks. The original timeout result is retained.
        }
    }
}

/// <summary>External process execution result.</summary>
public sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError, string ErrorCode = "")
{
    public bool Success => ExitCode == 0 && string.IsNullOrEmpty(ErrorCode);
    public static ProcessResult Failed(int exitCode, string stdout, string stderr, string errorCode = "")
        => new(exitCode, stdout, stderr, errorCode);
}

internal readonly record struct ProcessStreamCapture(string Text, bool WasTruncated);
