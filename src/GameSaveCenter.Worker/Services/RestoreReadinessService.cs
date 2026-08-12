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

    public Task<RestoreReadinessDto> ValidateAsync(
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
            return Task.FromResult(result);
        }

        if (!File.Exists(version.ArchivePath))
        {
            result.Status = RestoreReadinessStatus.Corrupted;
            result.ErrorCount = 1;
            result.Summary = "归档文件不存在，无法验证可恢复性。";
            return Task.FromResult(result);
        }

        var stagingDirectory = Path.Combine(stagingRoot, "restore-readiness-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(stagingDirectory);
        try
        {
            var expected = TryReadManifest(manifestJson, result);
            if (expected.Count > 0)
            {
                result.ExpectedFileCount = expected.Count;
                result.ExpectedTotalSize = expected.Sum(x => Math.Max(0, x.SizeBytes));
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
                return Task.FromResult(result);
            }

            long totalBytes = 0;
            var expectedByPath = expected
                .Where(x => !string.IsNullOrWhiteSpace(x.RelativePath))
                .GroupBy(x => x.RelativePath, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);
            var hashChecked = 0;
            var hashFailed = 0;
            foreach (var entry in files)
            {
                token.ThrowIfCancellationRequested();
                if (!TryGetSafeTarget(stagingDirectory, entry.FullName, out var target))
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
                    entry.ExtractToFile(target, overwrite: false);
                    var extractedLength = new FileInfo(target).Length;
                    if (extractedLength != entry.Length) result.ErrorCount++;
                    if (expectedByPath.TryGetValue(entry.FullName, out var expectedEntry)
                        && !string.IsNullOrWhiteSpace(expectedEntry.Sha256))
                    {
                        hashChecked++;
                        using var stream = File.OpenRead(target);
                        var actualHash = Convert.ToHexString(SHA256.HashData(stream));
                        if (!string.Equals(actualHash, expectedEntry.Sha256, StringComparison.OrdinalIgnoreCase))
                        {
                            hashFailed++;
                            result.ErrorCount++;
                        }
                    }
                }
                catch (Exception ex) when (ex is IOException || ex is InvalidDataException || ex is UnauthorizedAccessException)
                {
                    _logger.LogWarning(ex, "Could not extract restore-readiness entry {EntryName}", entry.FullName);
                    result.ErrorCount++;
                }
            }

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
                result.Summary = "归档可以打开，但有条目无法安全提取。";
            }
            else if (metricMismatch || result.WarningCount > 0)
            {
                result.Status = RestoreReadinessStatus.Warning;
                result.Summary = metricMismatch
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
        finally
        {
            TryDeleteStagingDirectory(stagingDirectory);
        }

        return Task.FromResult(result);
    }

    private static List<FileManifestEntry> TryReadManifest(string json, RestoreReadinessDto result)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "[]" || json == "{}") return new List<FileManifestEntry>();
        try
        {
            return JsonSerializer.Deserialize<List<FileManifestEntry>>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))
                   ?? new List<FileManifestEntry>();
        }
        catch (JsonException)
        {
            result.WarningCount++;
            return new List<FileManifestEntry>();
        }
    }

    private static bool TryGetSafeTarget(string stagingDirectory, string entryName, out string target)
    {
        target = string.Empty;
        if (string.IsNullOrWhiteSpace(entryName)) return false;
        var normalized = entryName.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        if (Path.IsPathRooted(normalized) || normalized.Contains(":", StringComparison.Ordinal)) return false;
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
