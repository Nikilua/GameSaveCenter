using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>
/// Batch path migration: rewrites stored absolute path prefixes in SQLite and Worker
/// settings when backup/media/game-tool roots move. It never moves or deletes files.
/// </summary>
public sealed class PathRemapService
{
    private readonly WorkerOptions _options;
    private readonly SqliteStateStore _store;
    private readonly MetadataBackupService _metadataBackup;
    private readonly ILogger<PathRemapService> _logger;

    public PathRemapService(WorkerOptions options, SqliteStateStore store, MetadataBackupService metadataBackup, ILogger<PathRemapService> logger)
    {
        _options = options;
        _store = store;
        _metadataBackup = metadataBackup;
        _logger = logger;
    }

    public async Task<PathRemapPreviewDto> PreviewAsync(PathRemapRequestDto request, CancellationToken token)
    {
        var oldRoot = NormalizeRoot(request.OldRoot);
        var newRoot = NormalizeRoot(request.NewRoot);
        ValidateRoots(oldRoot, newRoot);
        var rows = await _store.PreviewRemapPathsAsync(oldRoot, newRoot, token).ConfigureAwait(false);
        var items = rows.Select(row => new PathRemapPreviewItemDto
        {
            Category = row.Category,
            OldPath = row.OldPath,
            NewPath = row.NewPath,
            TargetExists = File.Exists(row.NewPath) || Directory.Exists(row.NewPath)
        }).ToList();
        var missing = items.Count(x => !x.TargetExists);
        return new PathRemapPreviewDto
        {
            Items = items,
            AffectedRowCount = items.Count,
            MissingTargetCount = missing,
            Summary = $"预览到 {items.Count} 条路径需要迁移，其中 {missing} 条目标路径当前不存在。"
        };
    }

    public async Task<PathRemapResultDto> RemapAsync(PathRemapRequestDto request, CancellationToken token)
    {
        if (!request.Confirmed)
            throw new WorkerOperationException("PATH_REMAP_NOT_CONFIRMED", "路径迁移需要用户明确确认。", "Confirmed=false");
        var oldRoot = NormalizeRoot(request.OldRoot);
        var newRoot = NormalizeRoot(request.NewRoot);
        ValidateRoots(oldRoot, newRoot);
        var preview = await PreviewAsync(request, token).ConfigureAwait(false);
        if (preview.MissingTargetCount > 0 && !request.ApplyMissingTargets)
            throw new WorkerOperationException(
                "PATH_REMAP_TARGET_MISSING",
                $"存在 {preview.MissingTargetCount} 条目标路径不存在，已按默认策略跳过本次迁移。",
                string.Join("；", preview.Items.Where(x => !x.TargetExists).Take(20).Select(x => x.NewPath)));

        var metadataBackup = await _metadataBackup.CreateAsync(token).ConfigureAwait(false);
        _logger.LogInformation("Path remap created metadata backup before apply: {PackagePath}", metadataBackup.PackagePath);

        var affectedRows = await _store.RemapStoredPathsAsync(oldRoot, newRoot, token).ConfigureAwait(false);
        var updatedSettings = new List<string>();
        if (StartsWithPath(_options.LudusaviBackupDirectory, oldRoot))
        {
            _options.LudusaviBackupDirectory = ReplaceRoot(_options.LudusaviBackupDirectory, oldRoot, newRoot);
            updatedSettings.Add("存档目录");
        }
        if (StartsWithPath(_options.MediaArchiveDirectory, oldRoot))
        {
            _options.MediaArchiveDirectory = ReplaceRoot(_options.MediaArchiveDirectory, oldRoot, newRoot);
            updatedSettings.Add("媒体目录");
        }
        if (updatedSettings.Count > 0)
            _options.Apply(_options.ToDto(), persist: true);

        var summary = $"路径迁移完成：{oldRoot} -> {newRoot}，更新 {affectedRows} 条数据库路径" +
                      (updatedSettings.Count > 0 ? "，同步 " + string.Join("、", updatedSettings) : "") + "。";
        await _store.AppendAuditAsync("PathRemap", summary,
            JsonSerializer.Serialize(new { oldRoot, newRoot, affectedRows, updatedSettings }),
            token).ConfigureAwait(false);
        _logger.LogInformation("Path remap completed: {AffectedRows} rows, settings {Settings}", affectedRows, string.Join(",", updatedSettings));
        return new PathRemapResultDto
        {
            OldRoot = oldRoot,
            NewRoot = newRoot,
            AffectedRows = affectedRows,
            UpdatedSettings = updatedSettings,
            Summary = summary
        };
    }

    private static void ValidateRoots(string oldRoot, string newRoot)
    {
        if (string.IsNullOrWhiteSpace(oldRoot) || string.IsNullOrWhiteSpace(newRoot))
            throw new WorkerOperationException("PATH_REMAP_EMPTY", "旧路径和新路径都不能为空。", oldRoot + " -> " + newRoot);
        if (string.Equals(oldRoot, newRoot, StringComparison.OrdinalIgnoreCase))
            throw new WorkerOperationException("PATH_REMAP_SAME", "旧路径和新路径相同，无需迁移。", oldRoot);
        if (!Path.IsPathRooted(oldRoot) || !Path.IsPathRooted(newRoot))
            throw new WorkerOperationException("PATH_REMAP_NOT_ABSOLUTE", "路径迁移只接受绝对路径。", oldRoot + " -> " + newRoot);
    }

    private static string NormalizeRoot(string value)
    {
        var expanded = Environment.ExpandEnvironmentVariables(value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(expanded)) return string.Empty;
        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(expanded));
    }

    private static bool StartsWithPath(string path, string root)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        var normalized = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
        if (!normalized.StartsWith(root, StringComparison.OrdinalIgnoreCase)) return false;
        return normalized.Length == root.Length || normalized[root.Length] is '\\' or '/';
    }

    private static string ReplaceRoot(string path, string oldRoot, string newRoot)
    {
        var normalized = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
        return newRoot + normalized.Substring(oldRoot.Length);
    }
}
