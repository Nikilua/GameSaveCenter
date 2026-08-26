using GameSaveCenter.Worker.Configuration;

namespace GameSaveCenter.Worker.Infrastructure;

/// <summary>Safe, one-way Rclone adapter. It never calls sync, move, delete or purge.</summary>
public sealed class RcloneClient
{
    private static readonly HashSet<string> AllowedCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        "copy", "check", "lsf", "cat", "version"
    };
    private readonly WorkerOptions _options;
    private readonly ExternalProcessRunner _runner;

    public RcloneClient(WorkerOptions options, ExternalProcessRunner runner)
    {
        _options = options;
        _runner = runner;
    }

    public bool IsAvailable => !string.IsNullOrWhiteSpace(_options.RcloneExecutable) && File.Exists(_options.RcloneExecutable);
    public bool IsConfigured => IsAvailable && !string.IsNullOrWhiteSpace(_options.RcloneDestination);

    public Task<ProcessResult> CopyAsync(string localDirectory, string remoteSubPath, CancellationToken token)
    {
        if (!IsConfigured) return Task.FromResult(ProcessResult.Failed(-1, string.Empty, "Rclone is not configured."));
        var destination = CombineRemote(_options.RcloneDestination, remoteSubPath);
        return RunSafeAsync(
            new[] { "copy", localDirectory, destination, "--checksum", "--check-first", "--create-empty-src-dirs", "--stats-one-line" },
            TimeSpan.FromHours(2), token);
    }

    public Task<ProcessResult> CheckAsync(string localDirectory, string remoteSubPath, CancellationToken token)
    {
        if (!IsConfigured) return Task.FromResult(ProcessResult.Failed(-1, string.Empty, "Rclone is not configured."));
        var destination = CombineRemote(_options.RcloneDestination, remoteSubPath);
        return RunSafeAsync(
            new[] { "check", localDirectory, destination, "--one-way", "--size-only" },
            TimeSpan.FromHours(1), token);
    }

    /// <summary>Verifies a staged download using provider hashes whenever Rclone can obtain them.</summary>
    public Task<ProcessResult> ChecksumCheckAsync(string localDirectory, string remoteSubPath, CancellationToken token)
    {
        if (!IsConfigured) return Task.FromResult(ProcessResult.Failed(-1, string.Empty, "Rclone is not configured."));
        return RunSafeAsync(
            BuildChecksumCheckArguments(_options.RcloneDestination, remoteSubPath, localDirectory),
            TimeSpan.FromHours(1), token);
    }

    /// <summary>Copies a remote directory into a new local staging directory. It never writes to the remote.</summary>
    public Task<ProcessResult> DownloadAsync(string remoteSubPath, string localDirectory, CancellationToken token)
    {
        if (!IsConfigured) return Task.FromResult(ProcessResult.Failed(-1, string.Empty, "Rclone is not configured."));
        var source = CombineRemote(_options.RcloneDestination, remoteSubPath);
        return RunSafeAsync(
            new[] { "copy", source, localDirectory, "--checksum", "--check-first", "--create-empty-src-dirs", "--stats-one-line" },
            TimeSpan.FromHours(2), token);
    }

    /// <summary>Lists remote sidecar paths. This is read-only and never modifies the remote.</summary>
    public async Task<IReadOnlyList<string>> ListDeviceStateFilesAsync(CancellationToken token)
    {
        if (!IsConfigured) return Array.Empty<string>();
        var result = await RunSafeAsync(
            new[] { "lsf", _options.RcloneDestination, "--recursive", "--files-only", "--include", "*/DeviceState/*.json" },
            TimeSpan.FromMinutes(2), token).ConfigureAwait(false);
        return !result.Success ? Array.Empty<string>() : result.StandardOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x=>x.Trim()).Where(x=>x.Length>0).Take(64).ToList();
    }

    /// <summary>Performs a bounded read-only remote probe without uploading or changing files.</summary>
    public Task<ProcessResult> ProbeRemoteAsync(CancellationToken token)
    {
        if (!IsConfigured) return Task.FromResult(ProcessResult.Failed(-1, string.Empty, "Rclone is not configured."));
        return RunSafeAsync(
            new[] { "lsf", _options.RcloneDestination, "--max-depth", "1", "--files-only" },
            TimeSpan.FromMinutes(2), token);
    }

    /// <summary>Reads a small JSON sidecar; callers enforce their own schema and size limits.</summary>
    public async Task<string> ReadRemoteTextAsync(string remoteRelativePath,CancellationToken token)
    {
        if (!IsConfigured) return string.Empty;
        var result = await RunSafeAsync(new[] { "cat", CombineRemote(_options.RcloneDestination,remoteRelativePath) },
            TimeSpan.FromMinutes(1),token).ConfigureAwait(false);
        return result.Success&&result.StandardOutput.Length<=1024*1024 ? result.StandardOutput : string.Empty;
    }

    public async Task<string> GetVersionAsync(CancellationToken token)
    {
        if (!IsAvailable) return string.Empty;
        var result = await RunSafeAsync(new[] { "version" }, TimeSpan.FromSeconds(15), token).ConfigureAwait(false);
        return result.Success ? result.StandardOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty : string.Empty;
    }

    private static string CombineRemote(string root, string child)
    {
        var separator = root.EndsWith(":", StringComparison.Ordinal) || root.EndsWith("/", StringComparison.Ordinal) ? string.Empty : "/";
        return root + separator + child.Replace('\\', '/').TrimStart('/');
    }

    internal static string[] BuildChecksumCheckArguments(string remoteRoot,string remoteSubPath,string localDirectory)
        =>new[] { "check", CombineRemote(remoteRoot,remoteSubPath), localDirectory, "--one-way" };

    internal static bool IsAllowedCommand(IReadOnlyList<string>? arguments)
        => arguments is { Count: > 0 } && AllowedCommands.Contains(arguments[0]);

    private Task<ProcessResult> RunSafeAsync(IReadOnlyList<string> arguments, TimeSpan timeout, CancellationToken token)
    {
        if (!IsAllowedCommand(arguments))
            return Task.FromResult(ProcessResult.Failed(-1, string.Empty, "Rclone command is outside the one-way safety allowlist."));
        return _runner.RunAsync(_options.RcloneExecutable, arguments, null, timeout, token);
    }
}
