using System.Text;
using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Models;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>
/// Read-only global integrity check: SQLite health, schema, directories, configured
/// executables, indexed file references, orphan archives, manifests and storage free
/// space. It never deletes or repairs data.
/// </summary>
public sealed class IntegrityCheckService
{
    private static readonly string[] ExpectedTables =
    {
        "games", "game_policies", "backup_policy_templates", "sessions", "tasks", "findings",
        "backup_versions", "media", "media_sources", "save_candidates", "audit_log",
        "game_tools", "game_tool_versions", "protection_prompt_states", "trainer_catalog",
        "trainer_releases", "process_mappings", "device_conflict_decisions", "cloud_retry_queue", "cloud_transfer_queue",
        "health_inspection_state"
    };

    private const int MaxPathExamplesPerFinding = 20;
    private const long MinimumFreeBytes = 512L * 1024 * 1024;
    private readonly WorkerOptions _options;
    private readonly SqliteStateStore _store;
    private readonly ILogger<IntegrityCheckService> _logger;

    public IntegrityCheckService(WorkerOptions options, SqliteStateStore store, ILogger<IntegrityCheckService> logger)
    {
        _options = options;
        _store = store;
        _logger = logger;
    }

    public async Task<IntegrityCheckResultDto> RunAsync(CancellationToken token)
    {
        var findings = new List<IntegrityFindingDto>();
        CheckDirectory("数据目录", _options.DataDirectory, true, findings);
        CheckDirectory("存档目录", _options.LudusaviBackupDirectory, true, findings);
        CheckDirectory("媒体归档目录", _options.MediaArchiveDirectory, true, findings);
        CheckDirectory("GameTools 目录", _options.GameToolsDirectory, false, findings);
        CheckDirectory("下载目录", _options.DownloadDirectory, false, findings);
        CheckConfiguredExecutable("Ludusavi", _options.LudusaviExecutable, findings);
        CheckOptionalExecutable("Rclone", _options.RcloneExecutable, findings);

        DatabaseIntegrityProbeDto probe;
        try
        {
            probe = await _store.ProbeIntegrityAsync(ExpectedTables, token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Global integrity check could not open the SQLite database");
            findings.Add(Error("DATABASE_UNREADABLE", "SQLite 数据库无法打开",
                ex.Message, "检查数据库文件权限，或从诊断包之外的备份恢复该文件。"));
            return BuildResult(findings);
        }

        if (probe.IntegrityRows.Count == 0 || probe.IntegrityRows.Any(x => !string.Equals(x, "ok", StringComparison.OrdinalIgnoreCase)))
            findings.Add(Error("DATABASE_INTEGRITY_CHECK_FAILED", "SQLite 完整性检查失败",
                string.Join("；", probe.IntegrityRows.Take(10)), "停止自动写入并联系支持；不要直接覆盖数据库。"));

        if (probe.ForeignKeyViolations.Count > 0)
            findings.Add(Error("DATABASE_FOREIGN_KEY_VIOLATION", "数据库外键关系异常",
                string.Join("；", probe.ForeignKeyViolations.Take(10)), "数据库需要人工修复或从备份恢复。"));

        if (probe.MissingTables.Count > 0)
            findings.Add(Error("DATABASE_TABLE_MISSING", "数据库缺少必要表",
                string.Join("、", probe.MissingTables), "运行一次 Worker 初始化；若仍缺失，请恢复数据库。"));

        AddMissingFileFindings(findings, "BACKUP_ARCHIVE_MISSING", "备份归档文件缺失",
            probe.BackupArchivePaths, "Ludusavi 索引中的归档已不存在。请核对存档目录或重新备份。");
        AddMissingFileFindings(findings, "GAME_TOOL_FILE_MISSING", "游戏工具文件缺失",
            probe.GameToolEntryPaths, "工具记录指向的文件不存在。请重新定位、重新导入或解除绑定。");
        AddMissingFileFindings(findings, "MEDIA_ARCHIVE_MISSING", "媒体归档文件缺失",
            probe.MediaArchivePaths, "媒体索引指向的归档副本不存在。请检查媒体目录或重新同步。");

        CheckOrphanArchives(probe.BackupArchivePaths, findings, token);
        await CheckManifestIntegrityAsync(findings, token).ConfigureAwait(false);
        CheckStorageSpace("数据目录", _options.DataDirectory, findings);
        CheckStorageSpace("存档目录", _options.LudusaviBackupDirectory, findings);
        CheckStorageSpace("媒体归档目录", _options.MediaArchiveDirectory, findings);
        return BuildResult(findings);
    }

    private static IntegrityCheckResultDto BuildResult(List<IntegrityFindingDto> findings)
    {
        var errors = findings.Count(x => x.Severity == "Error");
        var warnings = findings.Count(x => x.Severity == "Warning");
        var skipped = findings.Count(x => x.Severity == "Skipped");
        var state = errors > 0 ? "Error" : warnings > 0 ? "Warning" : skipped > 0 ? "Skipped" : "Healthy";
        var summary = state switch
        {
            "Error" => $"完整性自检发现 {errors} 个严重问题和 {warnings} 个警告，请先处理数据库/目录问题。",
            "Warning" => $"完整性自检发现 {warnings} 个警告；核心数据库和目录正常。",
            "Skipped" => $"完整性自检通过，{skipped} 个可选依赖已跳过（未配置）。",
            _ => "完整性自检通过：数据库、表结构、目录和已索引文件均正常。"
        };
        return new IntegrityCheckResultDto
        {
            CheckedUtc = DateTime.UtcNow,
            State = state,
            ErrorCount = errors,
            WarningCount = warnings,
            SkippedCount = skipped,
            Findings = findings,
            Summary = summary
        };
    }

    private void CheckDirectory(string label, string path, bool critical, List<IntegrityFindingDto> findings)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            // Optional directories are intentionally absent on a default install.
            // They are not a health finding until the user configures a path that
            // then becomes missing or unwritable. This keeps the global check
            // healthy while preserving critical-directory failures.
            if (!critical) return;

            findings.Add(CriticalOrWarning(critical, "DIRECTORY_NOT_CONFIGURED", $"{label}未配置",
                "路径为空，完整性无法确认。", "在设置中选择有效目录。"));
            return;
        }

        if (!Directory.Exists(path))
        {
            findings.Add(CriticalOrWarning(critical, "DIRECTORY_MISSING", $"{label}不存在",
                path, "创建目录或重新选择有效路径。"));
            return;
        }

        var probePath = Path.Combine(path, ".gsc-integrity-" + Guid.NewGuid().ToString("N"));
        try
        {
            File.WriteAllText(probePath, "probe");
            File.Delete(probePath);
        }
        catch (Exception ex)
        {
            TryDelete(probePath);
            findings.Add(CriticalOrWarning(critical, "DIRECTORY_NOT_WRITABLE", $"{label}不可写",
                path + "：" + ex.Message, "检查目录权限后重试。"));
        }
    }

    private static void CheckConfiguredExecutable(string label, string path, List<IntegrityFindingDto> findings)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            findings.Add(new IntegrityFindingDto
            {
                Code = label.ToUpperInvariant() + "_NOT_CONFIGURED",
                Severity = "Warning",
                Title = label + " 未配置",
                Detail = "路径为空，完整性无法确认。",
                SuggestedAction = "在设置中选择 " + label + " 路径。"
            });
            return;
        }
        if (!File.Exists(path))
        {
            findings.Add(new IntegrityFindingDto
            {
                Code = label.ToUpperInvariant() + "_EXECUTABLE_MISSING",
                Severity = "Warning",
                Title = label + " 可执行文件缺失",
                Detail = path,
                SuggestedAction = "在设置中重新选择 " + label + " 路径。"
            });
        }
    }

    private static void CheckOptionalExecutable(string label, string path, List<IntegrityFindingDto> findings)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            findings.Add(new IntegrityFindingDto
            {
                Code = label.ToUpperInvariant() + "_SKIPPED",
                Severity = "Skipped",
                Title = label + " 未配置",
                Detail = "可选依赖未配置，本次自检跳过。",
                SuggestedAction = "如需云端功能，请在设置中配置 " + label + "。"
            });
        }
    }

    private static void AddMissingFileFindings(List<IntegrityFindingDto> findings, string code, string title,
        IReadOnlyCollection<string> paths, string suggestedAction)
    {
        if (paths.Count == 0) return;
        var missing = paths.Where(x => !File.Exists(Path.GetFullPath(x))).ToList();
        if (missing.Count == 0) return;
        var detail = new StringBuilder($"共 {missing.Count} 个文件缺失");
        foreach (var path in missing.Take(MaxPathExamplesPerFinding))
            detail.AppendLine().Append(path);
        findings.Add(new IntegrityFindingDto
        {
            Code = code,
            Severity = "Warning",
            Title = title,
            Detail = detail.ToString(),
            SuggestedAction = suggestedAction
        });
    }

    private void CheckOrphanArchives(
        IReadOnlyCollection<string> dbArchivePaths,
        List<IntegrityFindingDto> findings,
        CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(_options.LudusaviBackupDirectory) || !Directory.Exists(_options.LudusaviBackupDirectory))
            return;
        var known = new HashSet<string>(
            dbArchivePaths.Where(x => !string.IsNullOrWhiteSpace(x)).Select(Path.GetFullPath),
            StringComparer.OrdinalIgnoreCase);
        var orphans = new List<string>();
        foreach (var file in Directory.EnumerateFiles(_options.LudusaviBackupDirectory, "*.zip", SearchOption.AllDirectories))
        {
            token.ThrowIfCancellationRequested();
            if (!known.Contains(Path.GetFullPath(file)))
                orphans.Add(file);
            if (orphans.Count >= 100) break;
        }
        if (orphans.Count == 0) return;
        var detail = new StringBuilder($"发现 {orphans.Count} 个未被数据库索引的归档");
        foreach (var path in orphans.Take(MaxPathExamplesPerFinding))
            detail.AppendLine().Append(path);
        findings.Add(new IntegrityFindingDto
        {
            Code = "ORPHAN_ARCHIVE",
            Severity = "Warning",
            Title = "存在未索引的备份归档",
            Detail = detail.ToString(),
            SuggestedAction = "请确认来源后使用“重建备份索引”进行只读扫描；不要直接删除。"
        });
    }

    private async Task CheckManifestIntegrityAsync(List<IntegrityFindingDto> findings, CancellationToken token)
    {
        var keys = await _store.GetBackupManifestKeysAsync(token).ConfigureAwait(false);
        var invalidExamples = new List<string>();
        var duplicateExamples = new List<string>();
        foreach (var key in keys)
        {
            token.ThrowIfCancellationRequested();
            var json = await _store.GetBackupManifestAsync(key.PlayniteId, key.BackupId, token).ConfigureAwait(false);
            try
            {
                var entries = JsonSerializer.Deserialize<List<FileManifestEntry>>(json) ?? new List<FileManifestEntry>();
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var entry in entries)
                {
                    var path = (entry.RelativePath ?? string.Empty).Trim();
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        invalidExamples.Add($"{key.PlayniteId}/{key.BackupId}: 空路径");
                        break;
                    }
                    if (!seen.Add(path))
                    {
                        duplicateExamples.Add($"{key.PlayniteId}/{key.BackupId}: {path}");
                        break;
                    }
                }
            }
            catch
            {
                invalidExamples.Add($"{key.PlayniteId}/{key.BackupId}: 无法解析");
            }
            if (invalidExamples.Count + duplicateExamples.Count >= MaxPathExamplesPerFinding) break;
        }

        if (invalidExamples.Count > 0)
            findings.Add(new IntegrityFindingDto
            {
                Code = "MANIFEST_INVALID",
                Severity = "Warning",
                Title = "存在无效备份 Manifest",
                Detail = string.Join(Environment.NewLine, invalidExamples.Take(MaxPathExamplesPerFinding)),
                SuggestedAction = "该版本无法可靠恢复，请重新备份或删除损坏版本。"
            });
        if (duplicateExamples.Count > 0)
            findings.Add(new IntegrityFindingDto
            {
                Code = "MANIFEST_DUPLICATE_PATH",
                Severity = "Warning",
                Title = "备份 Manifest 存在重复路径",
                Detail = string.Join(Environment.NewLine, duplicateExamples.Take(MaxPathExamplesPerFinding)),
                SuggestedAction = "该版本差异与恢复校验不可靠，请重新备份。"
            });
    }

    private static void CheckStorageSpace(string label, string path, List<IntegrityFindingDto> findings)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return;
        try
        {
            var root = Path.GetPathRoot(Path.GetFullPath(path));
            if (string.IsNullOrWhiteSpace(root)) return;
            var drive = new DriveInfo(root);
            if (drive.AvailableFreeSpace >= MinimumFreeBytes) return;
            findings.Add(new IntegrityFindingDto
            {
                Code = "LOW_DISK_SPACE",
                Severity = "Warning",
                Title = $"{label}所在磁盘空间不足",
                Detail = $"{root} 剩余 {drive.AvailableFreeSpace / 1024d / 1024d:0.#} MiB",
                SuggestedAction = "清理磁盘或迁移目录，避免备份写入失败。"
            });
        }
        catch
        {
        }
    }

    private static IntegrityFindingDto CriticalOrWarning(bool critical, string code, string title, string detail, string action)
        => critical
            ? Error(code, title, detail, action)
            : new IntegrityFindingDto { Code = code, Severity = "Warning", Title = title, Detail = detail, SuggestedAction = action };

    private static IntegrityFindingDto Error(string code, string title, string detail, string action)
        => new IntegrityFindingDto { Code = code, Severity = "Error", Title = title, Detail = detail, SuggestedAction = action };

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }
}
