using System.Text.Json;
using System.Security.Cryptography;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Infrastructure;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>
/// One-way second local mirror: copy and size-verify backup files into a separate volume.
/// It never deletes mirror-only files and reports an unavailable external drive as a status,
/// not a system error.
/// </summary>
public sealed class LocalMirrorService
{
    private const string MarkerName = ".gsc-mirror-sync.json";
    private readonly WorkerOptions _options;
    private readonly ILogger<LocalMirrorService> _logger;

    public LocalMirrorService(WorkerOptions options, ILogger<LocalMirrorService> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task<LocalMirrorStatusDto> StatusAsync(CancellationToken token)
    {
        var mirror = ResolveMirrorRoot();
        var status = new LocalMirrorStatusDto
        {
            Enabled = _options.EnableLocalMirror,
            MirrorPath = mirror ?? string.Empty
        };
        if (!_options.EnableLocalMirror || string.IsNullOrWhiteSpace(mirror))
        {
            status.Message = "本地镜像未启用。";
            return status;
        }
        if (!Directory.Exists(mirror))
        {
            status.Message = "镜像目录不可用（外置硬盘可能未连接）。";
            return status;
        }

        status.Available = true;
        var markerPath = Path.Combine(mirror, MarkerName);
        if (File.Exists(markerPath))
        {
            try
            {
                var marker = JsonSerializer.Deserialize<MirrorMarker>(await File.ReadAllTextAsync(markerPath, token).ConfigureAwait(false));
                if (marker != null)
                {
                    status.LastSyncUtc = marker.LastSyncUtc;
                    status.CopiedCount = marker.CopiedCount;
                    status.VerifiedCount = marker.VerifiedCount;
                    status.TotalBytes = marker.TotalBytes;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Local mirror marker could not be read; status will be recalculated");
            }
        }
        if (status.TotalBytes <= 0)
        {
            try
            {
                var files = Directory.EnumerateFiles(mirror, "*", SearchOption.AllDirectories)
                    .Where(x => !string.Equals(Path.GetFileName(x), MarkerName, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                status.CopiedCount = files.Count;
                status.VerifiedCount = 0;
                status.TotalBytes = files.Sum(x => SafeLength(x));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Local mirror could not enumerate files for status");
            }
        }
        status.Message = $"镜像可用：{status.CopiedCount} 个文件，共 {status.TotalBytesDisplay}；最近同步 {status.LastSyncDisplay}。";
        return status;
    }

    public async Task<LocalMirrorSyncResultDto> SyncAsync(CancellationToken token)
    {
        var mirror = ResolveMirrorRoot();
        if (!_options.EnableLocalMirror || string.IsNullOrWhiteSpace(mirror))
            throw new WorkerOperationException("LOCAL_MIRROR_NOT_CONFIGURED", "本地镜像未启用或未填写镜像目录。");
        if (!Directory.Exists(mirror))
            throw new WorkerOperationException("LOCAL_MIRROR_UNAVAILABLE", "镜像目录不可用（外置硬盘可能未连接）。");

        var source = _options.LudusaviBackupDirectory;
        if (string.IsNullOrWhiteSpace(source) || !Directory.Exists(source))
            throw new WorkerOperationException("LOCAL_MIRROR_SOURCE_UNAVAILABLE", "存档目录不可用，无法同步本地镜像。");

        var sourceRoot = Path.GetFullPath(source).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var mirrorRoot = Path.GetFullPath(mirror).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (sourceRoot.Equals(mirrorRoot, StringComparison.OrdinalIgnoreCase))
            throw new WorkerOperationException("LOCAL_MIRROR_SAME_PATH", "镜像目录不能与存档目录相同。");

        var result = new LocalMirrorSyncResultDto();
        long totalBytes = 0;
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            token.ThrowIfCancellationRequested();
            var relative = Path.GetRelativePath(sourceRoot, file);
            var destination = Path.Combine(mirrorRoot, relative);
            long sourceBytes;
            try
            {
                sourceBytes = new FileInfo(file).Length;
            }
            catch (Exception ex)
            {
                throw new WorkerOperationException("LOCAL_MIRROR_SOURCE_UNREADABLE", "镜像源文件无法读取。", ex.Message);
            }
            totalBytes += sourceBytes;

            if (File.Exists(destination) && SafeLength(destination) == sourceBytes &&
                await HashMatchesAsync(file, destination, token).ConfigureAwait(false))
            {
                result.SkippedCount++;
                result.VerifiedCount++;
                continue;
            }

            var directory = Path.GetDirectoryName(destination);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
            try
            {
                await using (var input = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 128 * 1024, useAsync: true))
                await using (var output = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, 128 * 1024, useAsync: true))
                {
                    await input.CopyToAsync(output, token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                TryDelete(destination);
                throw;
            }
            catch (Exception ex)
            {
                TryDelete(destination);
                throw new WorkerOperationException("LOCAL_MIRROR_COPY_FAILED", "镜像复制失败。", ex.Message);
            }

            var sourceHash = await ComputeSha256Async(file, token).ConfigureAwait(false);
            var destinationHash = await ComputeSha256Async(destination, token).ConfigureAwait(false);
            if (SafeLength(destination) != sourceBytes ||
                !string.Equals(sourceHash, destinationHash, StringComparison.OrdinalIgnoreCase))
            {
                TryDelete(destination);
                throw new WorkerOperationException("LOCAL_MIRROR_VERIFY_FAILED", "镜像文件内容校验失败，已清理不完整副本。");
            }
            result.CopiedCount++;
            result.VerifiedCount++;
        }

        result.TotalBytes = totalBytes;
        var marker = new MirrorMarker
        {
            LastSyncUtc = DateTime.UtcNow,
            CopiedCount = result.CopiedCount,
            VerifiedCount = result.VerifiedCount,
            TotalBytes = result.TotalBytes
        };
        await AtomicFileWriter.WriteAllTextAsync(
            Path.Combine(mirrorRoot.TrimEnd(Path.DirectorySeparatorChar), MarkerName),
            JsonSerializer.Serialize(marker),
            token).ConfigureAwait(false);

        result.Message = $"镜像同步完成：复制 {result.CopiedCount} 个、校验 {result.VerifiedCount} 个、跳过 {result.SkippedCount} 个；共 {FormatBytes(result.TotalBytes)}。镜像中多余文件不会被删除。";
        return result;
    }

    private string? ResolveMirrorRoot()
    {
        if (string.IsNullOrWhiteSpace(_options.LocalMirrorPath)) return string.Empty;
        try
        {
            return Path.GetFullPath(Environment.ExpandEnvironmentVariables(_options.LocalMirrorPath));
        }
        catch
        {
            return string.Empty;
        }
    }

    private static long SafeLength(string path)
    {
        try
        {
            return new FileInfo(path).Length;
        }
        catch
        {
            return 0;
        }
    }

    private static async Task<bool> HashMatchesAsync(string source, string destination, CancellationToken token)
    {
        try
        {
            var sourceHash = await ComputeSha256Async(source, token).ConfigureAwait(false);
            var destinationHash = await ComputeSha256Async(destination, token).ConfigureAwait(false);
            return string.Equals(sourceHash, destinationHash, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static async Task<string> ComputeSha256Async(string path, CancellationToken token)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            128 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        var hash = await SHA256.HashDataAsync(stream, token).ConfigureAwait(false);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes:0} B";
        if (bytes < 1024L * 1024) return $"{bytes / 1024d:0.##} KiB";
        if (bytes < 1024L * 1024 * 1024) return $"{bytes / 1024d / 1024d:0.##} MiB";
        return $"{bytes / 1024d / 1024d / 1024d:0.##} GiB";
    }

    private sealed class MirrorMarker
    {
        public DateTime LastSyncUtc { get; set; }
        public int CopiedCount { get; set; }
        public int VerifiedCount { get; set; }
        public long TotalBytes { get; set; }
    }
}
