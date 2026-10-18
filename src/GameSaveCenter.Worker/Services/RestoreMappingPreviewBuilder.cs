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
        DateTime previewedUtc)
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
        foreach (var file in files.EnumerateObject())
        {
            if (fileCount < int.MaxValue) fileCount++;
            if (file.Value.ValueKind == JsonValueKind.Object
                && file.Value.TryGetProperty("bytes", out var bytes)
                && bytes.TryGetInt64(out var parsed)
                && parsed > 0)
            {
                totalBytes = totalBytes > long.MaxValue - parsed ? long.MaxValue : totalBytes + parsed;
            }
        }

        output.Success = true;
        output.FileCount = fileCount;
        output.TotalBytes = totalBytes;
        output.Summary = $"映射预览：{fileCount} 个文件 · {ByteSizeFormatter.Format(totalBytes)}；只读核对，不写入当前存档。";
        return output;
    }

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
}
