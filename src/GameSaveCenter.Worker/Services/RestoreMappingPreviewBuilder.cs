using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Services;
using GameSaveCenter.Worker.Infrastructure;

namespace GameSaveCenter.Worker.Services;

/// <summary>Reduces Ludusavi restore-preview JSON to path-free planning evidence.</summary>
public static class RestoreMappingPreviewBuilder
{
    public static RestoreMappingPreviewDto Build(
        LudusaviCommandResult result,
        string playniteId,
        string backupId,
        string gameName,
        DateTime previewedUtc,
        string? fingerprintKeyBase64 = null,
        string? fingerprintKeyId = null)
    {
        var output = new RestoreMappingPreviewDto
        {
            PlayniteId = playniteId ?? string.Empty,
            BackupId = backupId ?? string.Empty,
            PreviewId = Guid.NewGuid().ToString("N"),
            PreviewedUtc = previewedUtc.Kind == DateTimeKind.Utc ? previewedUtc : previewedUtc.ToUniversalTime()
        };

        if (!result.Success || result.Json is not JsonElement root || !TryGetFiles(root, gameName, out var files))
        {
            output.ErrorCode = SafeErrorCode(result.ErrorCode);
            output.Summary = "映射预览未能生成；未创建草稿，也未写入当前存档。";
            return output;
        }

        long totalBytes = 0;
        var fileCount = 0;
        var unknownSizeCount = 0;
        var fingerprintItems = new List<MappingFingerprintItem>();
        foreach (var file in files.EnumerateObject())
        {
            if (fileCount < int.MaxValue) fileCount++;
            var hasSize = false;
            long fileBytes = 0;
            if (file.Value.ValueKind == JsonValueKind.Object
                && file.Value.TryGetProperty("bytes", out var bytes)
                && bytes.TryGetInt64(out var parsed)
                && parsed >= 0)
            {
                hasSize = true;
                fileBytes = parsed;
            }
            if (!hasSize && unknownSizeCount < int.MaxValue) unknownSizeCount++;
            fingerprintItems.Add(new MappingFingerprintItem(CanonicalizePath(file.Name), hasSize, fileBytes));

            if (hasSize)
            {
                totalBytes = totalBytes > long.MaxValue - fileBytes ? long.MaxValue : totalBytes + fileBytes;
            }
        }

        output.Success = true;
        output.FileCount = fileCount;
        output.TotalBytes = totalBytes;
        output.UnknownSizeCount = unknownSizeCount;
        if (TryParseFingerprintKey(fingerprintKeyBase64, fingerprintKeyId, out var key, out var normalizedKeyId))
        {
            try
            {
                output.MappingFingerprintVersion = 1;
                output.MappingFingerprint = ComputeFingerprint(fingerprintItems, key);
                output.MappingFingerprintKeyId = normalizedKeyId;
            }
            finally
            {
                CryptographicOperations.ZeroMemory(key);
            }
        }

        var sizeSummary = unknownSizeCount == 0
            ? ByteSizeFormatter.Format(totalBytes)
            : $"已知大小小计 {ByteSizeFormatter.Format(totalBytes)}；{unknownSizeCount} 个文件大小未知";
        output.Summary = $"映射预览：{fileCount} 个文件 · {sizeSummary}；只读核对，不写入当前存档。";
        return output;
    }

    private static bool TryParseFingerprintKey(string? keyBase64, string? keyId, out byte[] key, out string normalizedKeyId)
    {
        key = Array.Empty<byte>();
        normalizedKeyId = string.Empty;
        if (!Guid.TryParseExact(keyId, "N", out var parsedId) || string.IsNullOrWhiteSpace(keyBase64)) return false;

        try
        {
            var candidate = Convert.FromBase64String(keyBase64);
            if (candidate.Length != 32)
            {
                CryptographicOperations.ZeroMemory(candidate);
                return false;
            }

            key = candidate;
            normalizedKeyId = parsedId.ToString("N");
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static string ComputeFingerprint(IReadOnlyList<MappingFingerprintItem> items, byte[] key)
    {
        var ordered = items.OrderBy(item => item.Path, StringComparer.Ordinal).ToArray();
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(1); // Canonical fingerprint format version.
            writer.Write(ordered.Length);
            foreach (var item in ordered)
            {
                writer.Write(item.Path);
                writer.Write(item.HasSize);
                if (item.HasSize) writer.Write(item.Size);
            }
        }

        using var hmac = new HMACSHA256(key);
        return Convert.ToHexString(hmac.ComputeHash(stream.ToArray()));
    }

    private static string CanonicalizePath(string path)
        => (path ?? string.Empty).Normalize(NormalizationForm.FormC).Replace('/', '\\').ToUpperInvariant();

    private static bool TryGetFiles(JsonElement root, string gameName, out JsonElement files)
    {
        files = default;
        if (root.ValueKind != JsonValueKind.Object
            || !root.TryGetProperty("games", out var games)
            || games.ValueKind != JsonValueKind.Object)
            return false;

        if (!games.TryGetProperty(gameName ?? string.Empty, out var game))
        {
            var match = games.EnumerateObject().FirstOrDefault(item =>
                string.Equals(item.Name, gameName, StringComparison.OrdinalIgnoreCase));
            game = match.Value;
        }
        return game.ValueKind == JsonValueKind.Object
            && game.TryGetProperty("files", out files)
            && files.ValueKind == JsonValueKind.Object;
    }

    private static string SafeErrorCode(string? value)
    {
        var normalized = new string((value ?? string.Empty)
            .Where(character => char.IsAsciiLetterOrDigit(character) || character == '_')
            .Take(64)
            .ToArray());
        return normalized.Length == 0 ? "RESTORE_PREVIEW_FAILED" : normalized;
    }

    private readonly record struct MappingFingerprintItem(string Path, bool HasSize, long Size);
}
