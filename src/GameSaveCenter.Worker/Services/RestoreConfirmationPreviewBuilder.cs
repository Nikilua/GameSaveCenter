using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Infrastructure;

namespace GameSaveCenter.Worker.Services;

internal static class RestoreConfirmationPreviewBuilder
{
    internal static RestoreConfirmationPreviewDto Build(LudusaviCommandResult result, string ludusaviName)
    {
        var preview = new RestoreConfirmationPreviewDto();
        if (!result.Success || !result.Json.HasValue) return preview;
        var json = result.Json.Value;
        if (json.ValueKind != JsonValueKind.Object || LudusaviResultParser.SomeGamesFailed(json)
            || !json.TryGetProperty("games", out var games) || games.ValueKind != JsonValueKind.Object
            || !games.TryGetProperty(ludusaviName, out var game) || game.ValueKind != JsonValueKind.Object
            || !game.TryGetProperty("files", out var files) || files.ValueKind != JsonValueKind.Object)
            return preview;

        if (game.TryGetProperty("decision", out var decision)
            && (decision.ValueKind != JsonValueKind.String
                || !string.Equals(decision.GetString(), "Processed", StringComparison.OrdinalIgnoreCase))) return preview;

        preview.HasFileList = true;
        var reliable = true;
        var added = 0;
        var modified = 0;
        var removed = 0;
        var directories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in files.EnumerateObject())
        {
            var fields = file.Value;
            if (fields.ValueKind != JsonValueKind.Object)
            {
                reliable = false;
                continue;
            }
            if (fields.TryGetProperty("ignored", out var ignored) && ignored.ValueKind == JsonValueKind.True) continue;
            var path = fields.TryGetProperty("redirectedPath", out var redirected) && redirected.ValueKind == JsonValueKind.String
                ? redirected.GetString() ?? file.Name : file.Name;
            if (!Path.IsPathFullyQualified(path) || !paths.Add(path))
            {
                reliable = false;
                continue;
            }
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory) && directories.Count < 80) directories.Add(directory);
            var change = fields.TryGetProperty("change", out var changeValue) && changeValue.ValueKind == JsonValueKind.String
                ? changeValue.GetString()?.ToLowerInvariant() : null;
            string label;
            switch (change)
            {
                case "new": added++; label = "新增"; break;
                case "different": modified++; label = "覆盖"; break;
                case "removed": removed++; label = "删除"; break;
                case "same": label = "内容相同"; break;
                default: reliable = false; label = "变更未知"; break;
            }
            preview.FileCount++;
            if (preview.Files.Count < 1000)
                preview.Files.Add(new RestoreConfirmationFileDto { Path = path, Change = label });
        }
        preview.TargetDirectories = directories.OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToList();
        if (reliable)
        {
            preview.AddedCount = added;
            preview.ModifiedCount = modified;
            if (removed > 0) preview.RemovedCount = removed;
        }
        return preview;
    }
}
