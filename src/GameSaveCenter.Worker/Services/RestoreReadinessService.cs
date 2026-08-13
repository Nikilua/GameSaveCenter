using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Models;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>
/// Performs a non-destructive, bounded read/extract check for one Ludusavi ZIP version.
/// The staging directory is always owned by GameSaveCenter and is never a live save path.
/// </summary>
public sealed class RestoreReadinessService
{
    private const int MaxEntries = 100_000;
    private const long MaxExpandedBytes = 4L * 1024 * 1024 * 1024;
    private readonly ILogger<RestoreReadinessService> _logger;

    public RestoreReadinessService(ILogger<RestoreReadinessService> logger) => _logger = logger;

    public async Task<RestoreReadinessDto> ValidateAsync(
        BackupVersionDto version,
        string manifestJson,
        string stagingRoot,
        CancellationToken token)
    {
        var result = new RestoreReadinessDto
        {
            BackupVersionId = version.BackupId,
            CheckedUtc = DateTime.UtcNow,
            ExpectedFileCount = version.FileCount,
            ExpectedTotalSize = version.TotalBytes,
            HashValidation = "NotAvailable"
        };

        if (string.IsNullOrWhiteSpace(version.ArchivePath)
            || !string.Equals(Path.GetExtension(version.ArchivePath), ".zip", StringComparison.OrdinalIgnoreCase))
        {
            result.Status = RestoreReadinessStatus.Unsupported;
            result.Summary = "当前版本不是可独立校验的 ZIP 归档。";
            return result;
        }

        if (!File.Exists(version.ArchivePath))
        {
            result.Status = RestoreReadinessStatus.Corrupted;
            result.ErrorCount = 1;
            result.Summary = "归档文件不存在，无法验证可恢复性。";
            return result;
        }

        var manifestState = TryReadManifest(manifestJson, result, out var expected);
        if (manifestState == ManifestReadState.Invalid)
        {
            result.Status = RestoreReadinessStatus.Failed;
            result.ErrorCount++;
            result.Summary = "Manifest 无法读取，不能确认该版本的恢复内容。";
            return result;
        }

        var stagingDirectory = string.Empty;
        try
        {
            stagingDirectory = Path.Combine(stagingRoot, "restore-readiness-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(stagingDirectory);
            if (expected.Count > 0)
            {
                result.ExpectedFileCount = expected.Count;
                result.ExpectedTotalSize = expected.Sum(x => Math.Max(0, x.SizeBytes));
            }
            else if (manifestState == ManifestReadState.Missing)
            {
                result.WarningCount++;
            }

            token.ThrowIfCancellationRequested();
            using var archive = ZipFile.OpenRead(version.ArchivePath);
            result.ArchiveReadable = true;
            var files = archive.Entries.Where(x => !string.IsNullOrEmpty(x.Name)).ToList();
            if (files.Count > MaxEntries)
            {
                result.Status = RestoreReadinessStatus.Corrupted;
                result.ErrorCount++;
                result.Summary = $"归档条目数 {files.Count} 超过安全上限。";
                return result;
            }

            long totalBytes = 0;
            var expectedByPath = expected
                .ToDictionary(x => NormalizeManifestPath(x.RelativePath), x => x, StringComparer.OrdinalIgnoreCase);
            var actualByPath = new Dictionary<string, ZipArchiveEntry>(StringComparer.OrdinalIgnoreCase);
            var hashChecked = 0;
            var hashFailed = 0;
            var sizeMismatches = 0;
            foreach (var entry in files)
            {
                token.ThrowIfCancellationRequested();
                if (!TryGetSafeTarget(stagingDirectory, entry.FullName, out var target, out var normalizedPath))
                {
                    result.ErrorCount++;
                    continue;
                }

                if (!actualByPath.TryAdd(normalizedPath, entry))
                {
                    result.ErrorCount++;
                    continue;
                }

                if (entry.Length < 0 || entry.Length > MaxExpandedBytes - totalBytes)
                {
                    result.ErrorCount++;
                    continue;
                }

                totalBytes += entry.Length;
                result.ActualFileCount++;
                result.ActualTotalSize += entry.Length;
                if (entry.Length == 0) result.WarningCount++;

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                    var extraction = await ExtractEntryAsync(entry, target, expectedByPath.TryGetValue(normalizedPath, out var hashEntry) && !string.IsNullOrWhiteSpace(hashEntry.Sha256), token).ConfigureAwait(false);
                    var extractedLength = extraction.Length;
                    if (extractedLength != entry.Length) result.ErrorCount++;
                    if (expectedByPath.TryGetValue(normalizedPath, out var expectedEntry))
                    {
                        if (expectedEntry.SizeBytes >= 0 && expectedEntry.SizeBytes != entry.Length)
                            sizeMismatches++;

                        if (!string.IsNullOrWhiteSpace(expectedEntry.Sha256))
                        {
                            hashChecked++;
                            // ExtractEntryAsync hashes while copying so cancellation remains
                            // responsive and large files are not read twice.
                            var actualHash = extraction.Sha256;
                            if (!string.Equals(actualHash, expectedEntry.Sha256, StringComparison.OrdinalIgnoreCase))
                            { hashFailed++; result.ErrorCount++; }
                        }
                    }
                }
                catch (Exception ex) when (ex is IOException || ex is InvalidDataException || ex is UnauthorizedAccessException)
                {
                    _logger.LogWarning(ex, "Could not extract restore-readiness entry {EntryName}", entry.FullName);
                    result.ErrorCount++;
                }
            }

            var missingExpected = expectedByPath.Keys
                .Except(actualByPath.Keys, StringComparer.OrdinalIgnoreCase)
                .Count();
            var unexpectedActual = manifestState == ManifestReadState.Valid
                ? actualByPath.Keys.Except(expectedByPath.Keys, StringComparer.OrdinalIgnoreCase).Count()
                : 0;
            if (missingExpected > 0) result.ErrorCount += missingExpected;
            if (sizeMismatches > 0) result.WarningCount += sizeMismatches;
            if (unexpectedActual > 0) result.WarningCount += unexpectedActual;

            result.HashValidation = hashChecked == 0
                ? "NotAvailable"
                : hashFailed == 0 ? "Validated" : "Failed";

            result.ExtractSucceeded = result.ErrorCount == 0;
            var metricMismatch = result.ExpectedFileCount > 0
                && (result.ExpectedFileCount != result.ActualFileCount || result.ExpectedTotalSize != result.ActualTotalSize);
            if (metricMismatch) result.WarningCount++;

            if (result.ErrorCount > 0)
            {
                result.Status = RestoreReadinessStatus.Corrupted;
                result.Summary = missingExpected > 0
                    ? $"归档可以打开，但缺少 Manifest 中记录的 {missingExpected} 个文件。"
                    : "归档可以打开，但有条目无法安全提取或校验失败。";
            }
            else if (metricMismatch || result.WarningCount > 0)
            {
                result.Status = RestoreReadinessStatus.Warning;
                result.Summary = sizeMismatches > 0
                    ? $"归档可以解压，但有 {sizeMismatches} 个文件与 Manifest 大小不同。"
                    : !string.Equals(manifestState, ManifestReadState.Valid)
                        ? "归档可以解压，但缺少 Manifest，无法完成逐文件恢复校验。"
                        : unexpectedActual > 0
                            ? $"归档可以解压，但包含 {unexpectedActual} 个 Manifest 未记录的额外文件。"
                            : metricMismatch
                    ? "归档可读取；条目统计与索引不完全一致，差分版本可能只包含变化项。"
                    : "归档可读取并已提取到隔离目录，但包含需要留意的条目。";
            }
            else
            {
                result.Status = RestoreReadinessStatus.Ready;
                result.Summary = "归档可读取，全部条目已在隔离目录提取成功。";
            }
        }
        catch (InvalidDataException ex)
        {
            _logger.LogWarning(ex, "Restore-readiness ZIP validation failed for {BackupId}", version.BackupId);
            result.Status = RestoreReadinessStatus.Corrupted;
            result.ErrorCount++;
            result.Summary = "ZIP 归档无法读取或已损坏。";
        }
        catch (NotSupportedException ex)
        {
            _logger.LogWarning(ex, "Restore-readiness ZIP compression is unsupported for {BackupId}", version.BackupId);
            result.Status = RestoreReadinessStatus.Unsupported;
            result.Summary = "归档可以识别，但当前读取器不支持该 ZIP 压缩方式（例如 Zstd）。";
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "Restore-readiness file validation failed for {BackupId}", version.BackupId);
            result.Status = RestoreReadinessStatus.Failed;
            result.ErrorCount++;
            result.Summary = "读取归档时发生文件系统错误。";
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Restore-readiness staging directory is not accessible for {BackupId}", version.BackupId);
            result.Status = RestoreReadinessStatus.Failed;
            result.ErrorCount++;
            result.Summary = "无法访问恢复校验隔离目录。";
        }
        finally
        {
            if (stagingDirectory.Length > 0) TryDeleteStagingDirectory(stagingDirectory);
        }

        return result;
    }

    private static async Task<(long Length, string Sha256)> ExtractEntryAsync(ZipArchiveEntry entry, string target, bool hashContent, CancellationToken token)
    {
        long total = 0;
        using var hash = hashContent ? IncrementalHash.CreateHash(HashAlgorithmName.SHA256) : null;
        using var input = entry.Open();
        await using var output = new FileStream(target, FileMode.CreateNew, FileAccess.Write, FileShare.None, 128 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var buffer = new byte[128 * 1024];
        while (true)
        {
            var read = await input.ReadAsync(buffer.AsMemory(0, buffer.Length), token).ConfigureAwait(false);
            if (read == 0) break;
            await output.WriteAsync(buffer.AsMemory(0, read), token).ConfigureAwait(false);
            hash?.AppendData(buffer, 0, read);
            total += read;
        }
        await output.FlushAsync(token).ConfigureAwait(false);
        return (total, hash == null ? string.Empty : Convert.ToHexString(hash.GetHashAndReset()));
    }

    private enum ManifestReadState
    {
        Missing,
        Valid,
        Invalid
    }

    private static ManifestReadState TryReadManifest(string json, RestoreReadinessDto result, out List<FileManifestEntry> entries)
    {
        entries = new List<FileManifestEntry>();
        if (string.IsNullOrWhiteSpace(json) || json.Trim() is "[]" or "{}") return ManifestReadState.Missing;
        try
        {
            entries = JsonSerializer.Deserialize<List<FileManifestEntry>>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))
                ?? new List<FileManifestEntry>();
            var normalized = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in entries)
            {
                var path = NormalizeManifestPath(entry.RelativePath);
                if (path.Length == 0 || entry.SizeBytes < 0 || !normalized.Add(path)) return ManifestReadState.Invalid;
                entry.RelativePath = path;
            }
            return ManifestReadState.Valid;
        }
        catch (JsonException)
        {
            return ManifestReadState.Invalid;
        }
    }

    private static string NormalizeManifestPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return string.Empty;
        var normalized = path.Replace('\\', '/').Trim();
        if (normalized.StartsWith('/') || normalized.Contains(':', StringComparison.Ordinal)) return string.Empty;
        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Any(x => x is "." or "..")) return string.Empty;
        return string.Join('/', segments);
    }

    private static bool TryGetSafeTarget(string stagingDirectory, string entryName, out string target, out string normalizedPath)
    {
        target = string.Empty;
        normalizedPath = NormalizeManifestPath(entryName);
        if (normalizedPath.Length == 0) return false;
        var normalized = normalizedPath.Replace('/', Path.DirectorySeparatorChar);
        var root = Path.GetFullPath(stagingDirectory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        target = Path.GetFullPath(Path.Combine(root, normalized));
        return target.StartsWith(root, StringComparison.OrdinalIgnoreCase);
    }

    private static void TryDeleteStagingDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
        }
        catch
        {
            // A locked validation artifact remains inside the app-owned staging root and is harmless.
        }
    }
}
