using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>
/// Exports GameSaveCenter's own metadata (SQLite snapshot and Worker settings) into a
/// bounded, self-describing ZIP for disaster recovery. It never includes save archives,
/// media files, Rclone configuration or credentials.
/// </summary>
public sealed class MetadataBackupService
{
    private const long MaxPackageBytes = 512L * 1024 * 1024;
    private static readonly Regex SecretPattern = new(
        @"(?i)(password|passwd|token|secret|api[_-]?key|access[_-]?token)[""']?\s*([=:])\s*[""']?([^""'\s,;}]+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private readonly WorkerOptions _options;
    private readonly SqliteStateStore _store;
    private readonly ILogger<MetadataBackupService> _logger;

    public MetadataBackupService(WorkerOptions options, SqliteStateStore store, ILogger<MetadataBackupService> logger)
    {
        _options = options;
        _store = store;
        _logger = logger;
    }

    public async Task<MetadataBackupResultDto> CreateAsync(MetadataBackupCreateRequestDto request, CancellationToken token)
    {
        if (!File.Exists(_options.DatabasePath))
            throw new WorkerOperationException("METADATA_DATABASE_MISSING", "GameSaveCenter 数据库不存在，无法生成元数据灾备包。", _options.DatabasePath);
        if (request == null)
            throw new WorkerOperationException("METADATA_REQUEST_INVALID", "元数据灾备请求无效。");

        var createdUtc = DateTime.UtcNow;
        var backupRoot = Path.Combine(_options.DataDirectory, "MetadataBackups");
        Directory.CreateDirectory(backupRoot);
        var stem = "gsc-metadata-" + createdUtc.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N");
        var temporaryDirectory = Path.Combine(backupRoot, stem + ".tmp");
        var temporaryZip = temporaryDirectory + ".zip";
        var packagePath = Path.Combine(backupRoot, stem + ".zip");
        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            var databaseSnapshot = Path.Combine(temporaryDirectory, "gamesavecenter.db");
            await CreateDatabaseSnapshotAsync(databaseSnapshot, token).ConfigureAwait(false);
            var databaseSha256 = await ComputeSha256Async(databaseSnapshot, token).ConfigureAwait(false);

            var settingsPath = Path.Combine(temporaryDirectory, "worker-settings.json");
            var hasSettings = File.Exists(_options.RuntimeSettingsPath);
            string? settingsSha256 = null;
            if (hasSettings)
            {
                var raw = await File.ReadAllTextAsync(_options.RuntimeSettingsPath, token).ConfigureAwait(false);
                await File.WriteAllTextAsync(settingsPath, Sanitize(raw), new UTF8Encoding(false), token).ConfigureAwait(false);
                settingsSha256 = await ComputeSha256Async(settingsPath, token).ConfigureAwait(false);
            }

            var hasPluginSettings = !string.IsNullOrWhiteSpace(request.PluginSettingsJson);
            string? pluginSettingsSha256 = null;
            string? pluginSettingsPath = null;
            if (hasPluginSettings)
            {
                if (request.PluginSettingsJson.Length > 1024 * 1024)
                    throw new WorkerOperationException("METADATA_PLUGIN_SETTINGS_TOO_LARGE", "Playnite 插件设置超过 1 MiB 安全上限，已拒绝生成灾备包。");
                try
                {
                    using var document = JsonDocument.Parse(request.PluginSettingsJson);
                    if (document.RootElement.ValueKind != JsonValueKind.Object)
                        throw new JsonException("settings root is not an object");
                }
                catch (JsonException ex)
                {
                    throw new WorkerOperationException("METADATA_PLUGIN_SETTINGS_INVALID", "Playnite 插件设置不是有效 JSON，已拒绝生成灾备包。", ex.Message);
                }
                pluginSettingsPath = Path.Combine(temporaryDirectory, "plugin-settings.json");
                await File.WriteAllTextAsync(pluginSettingsPath, Sanitize(request.PluginSettingsJson), new UTF8Encoding(false), token).ConfigureAwait(false);
                pluginSettingsSha256 = await ComputeSha256Async(pluginSettingsPath, token).ConfigureAwait(false);
            }

            var manifest = new
            {
                schemaVersion = 1,
                createdUtc,
                workerVersion = typeof(MetadataBackupService).Assembly.GetName().Version?.ToString() ?? "dev",
                databaseFile = "database/gamesavecenter.db",
                databaseSha256,
                settingsFile = hasSettings ? "settings/worker-settings.json" : null,
                settingsSha256,
                pluginSettingsFile = hasPluginSettings ? "settings/plugin-settings.json" : null,
                pluginSettingsSha256,
                excludes = new[] { "存档", "媒体", "Rclone 凭据" }
            };

            using (var archive = ZipFile.Open(temporaryZip, ZipArchiveMode.Create))
            {
                AddText(archive, "manifest.json", JsonSerializer.Serialize(manifest, JsonOptions));
                AddText(archive, "README.txt", BuildReadme(createdUtc));
                archive.CreateEntryFromFile(databaseSnapshot, "database/gamesavecenter.db", CompressionLevel.Fastest);
                if (hasSettings)
                    archive.CreateEntryFromFile(settingsPath, "settings/worker-settings.json", CompressionLevel.Fastest);
                if (hasPluginSettings && pluginSettingsPath != null)
                    archive.CreateEntryFromFile(pluginSettingsPath, "settings/plugin-settings.json", CompressionLevel.Fastest);
            }

            var packageInfo = new FileInfo(temporaryZip);
            if (packageInfo.Length > MaxPackageBytes)
                throw new WorkerOperationException("METADATA_BACKUP_TOO_LARGE", "元数据灾备包超过 512 MiB 安全上限，已丢弃输出。", packageInfo.Length.ToString());

            File.Move(temporaryZip, packagePath);
            var result = new MetadataBackupResultDto
            {
                PackagePath = packagePath,
                CreatedUtc = createdUtc,
                PackageBytes = new FileInfo(packagePath).Length,
                IncludedFileCount = 3 + (hasSettings ? 1 : 0) + (hasPluginSettings ? 1 : 0),
                PluginSettingsIncluded = hasPluginSettings,
                Summary = $"元数据灾备包已生成：SQLite 快照、Worker 设置{(hasPluginSettings ? "、Playnite 插件设置" : string.Empty)}和版本清单；未包含存档、媒体或凭据。"
            };
            await _store.AppendAuditAsync("MetadataBackup", "已生成 GameSaveCenter 元数据灾备包",
                JsonSerializer.Serialize(new
                {
                    result.IncludedFileCount,
                    result.PackageBytes,
                    fileName = Path.GetFileName(packagePath)
                }), token).ConfigureAwait(false);
            return result;
        }
        catch
        {
            TryDeleteFile(temporaryZip);
            TryDeleteFile(packagePath);
            throw;
        }
        finally
        {
            TryDeleteDirectory(temporaryDirectory);
        }
    }

    public async Task<MetadataRestorePreviewDto> PreviewAsync(string packagePath, CancellationToken token)
    {
        var temporary = Path.Combine(Path.GetTempPath(), "GscMetadataPreview", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporary);
        try
        {
            var extracted = await ValidateAndExtractAsync(packagePath, temporary, token).ConfigureAwait(false);
            return new MetadataRestorePreviewDto
            {
                Valid = true,
                PackagePath = packagePath,
                SchemaVersion = extracted.SchemaVersion,
                DatabaseSha256 = extracted.DatabaseSha256,
                SettingsSha256 = extracted.SettingsSha256,
                PluginSettingsSha256 = extracted.PluginSettingsSha256,
                PluginSettingsJson = extracted.PluginSettingsJson,
                Entries = extracted.Entries,
                Summary = $"元数据包有效：schema {extracted.SchemaVersion}，包含 {extracted.Entries.Count} 个文件，数据库与设置哈希校验通过{(extracted.PluginSettingsSha256.Length > 0 ? "，包含 settings/plugin-settings.json" : string.Empty)}。"
            };
        }
        catch (Exception ex)
        {
            return new MetadataRestorePreviewDto
            {
                Valid = false,
                PackagePath = packagePath,
                Summary = "元数据包校验失败：" + ex.Message
            };
        }
        finally
        {
            TryDeleteDirectory(temporary);
        }
    }

    public async Task<MetadataRestoreResultDto> RestoreAsync(MetadataRestoreRequestDto request, CancellationToken token)
    {
        if (!request.Confirmed)
            throw new WorkerOperationException("METADATA_RESTORE_NOT_CONFIRMED", "请先确认元数据恢复操作。");

        var temporary = Path.Combine(Path.GetTempPath(), "GscMetadataRestore", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temporary);
        try
        {
            var extracted = await ValidateAndExtractAsync(request.PackagePath, temporary, token).ConfigureAwait(false);
            var backupRoot = Path.Combine(_options.DataDirectory, "MetadataBackups");
            Directory.CreateDirectory(backupRoot);
            var preRestore = Path.Combine(backupRoot, "PreRestore-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(preRestore);
            await CreateDatabaseSnapshotAsync(Path.Combine(preRestore, "gamesavecenter.db"), token).ConfigureAwait(false);
            if (File.Exists(_options.RuntimeSettingsPath))
                File.Copy(_options.RuntimeSettingsPath, Path.Combine(preRestore, "worker-settings.json"), true);

            _options.SafeModeEnabled = true;
            _options.PersistNow();
            try
            {
                SqliteConnection.ClearAllPools();
                DeleteSidecars(_options.DatabasePath);
                var extractedDatabase = Path.Combine(temporary, "database", "gamesavecenter.db");
                File.SetAttributes(extractedDatabase, FileAttributes.Normal);
                await AtomicFileWriter.ReplaceFileAsync(
                    extractedDatabase,
                    _options.DatabasePath,
                    token).ConfigureAwait(false);
                File.SetAttributes(_options.DatabasePath, FileAttributes.Normal);
                DeleteSidecars(_options.DatabasePath);
                var settingsExtracted = Path.Combine(temporary, "settings", "worker-settings.json");
                if (File.Exists(settingsExtracted))
                {
                    File.SetAttributes(settingsExtracted, FileAttributes.Normal);
                    await AtomicFileWriter.ReplaceFileAsync(settingsExtracted, _options.RuntimeSettingsPath, token).ConfigureAwait(false);
                    File.SetAttributes(_options.RuntimeSettingsPath, FileAttributes.Normal);
                }
                else
                {
                    var preSettings = Path.Combine(preRestore, "worker-settings.json");
                    if (File.Exists(preSettings))
                        await AtomicFileWriter.ReplaceFileAsync(preSettings, _options.RuntimeSettingsPath, token).ConfigureAwait(false);
                }
                _options.ReloadPersistedSettings();

                await ValidateDatabaseAsync(_options.DatabasePath, token).ConfigureAwait(false);
                SqliteConnection.ClearAllPools();
                await _store.AppendAuditAsync("MetadataRestore", "元数据灾备已恢复",
                    JsonSerializer.Serialize(new
                    {
                        preRestore,
                        schemaVersion = extracted.SchemaVersion,
                        databaseSha256 = extracted.DatabaseSha256
                    }), token).ConfigureAwait(false);
                return new MetadataRestoreResultDto
                {
                    Restored = true,
                    PreRestorePath = preRestore,
                    PluginSettingsJson = extracted.PluginSettingsJson,
                    Summary = "元数据恢复完成：数据库与设置已替换并通过完整性校验" +
                        (extracted.PluginSettingsJson.Length > 0 ? "，Playnite 插件设置将由插件侧导入" : string.Empty) +
                        "；恢复前副本保留在 " + preRestore + "。"
                };
            }
            catch
            {
                try
                {
                    SqliteConnection.ClearAllPools();
                    DeleteSidecars(_options.DatabasePath);
                    var preDatabase = Path.Combine(preRestore, "gamesavecenter.db");
                    var preSettings = Path.Combine(preRestore, "worker-settings.json");
                    if (File.Exists(preDatabase))
                        await AtomicFileWriter.ReplaceFileAsync(preDatabase, _options.DatabasePath, token).ConfigureAwait(false);
                    DeleteSidecars(_options.DatabasePath);
                    if (File.Exists(preSettings))
                        await AtomicFileWriter.ReplaceFileAsync(preSettings, _options.RuntimeSettingsPath, token).ConfigureAwait(false);
                    _options.ReloadPersistedSettings();
                }
                catch (Exception rollbackEx)
                {
                    throw new WorkerOperationException(
                        "METADATA_RESTORE_ROLLBACK_FAILED",
                        "元数据恢复失败，且回滚恢复前副本失败，需要人工介入。",
                        rollbackEx.Message);
                }
                throw;
            }
        }
        finally
        {
            TryDeleteDirectory(temporary);
        }
    }

    public async Task<MetadataRestoreRollbackResultDto> RollbackAsync(MetadataRestoreRollbackRequestDto request, CancellationToken token)
    {
        if (!request.Confirmed)
            throw new WorkerOperationException("METADATA_ROLLBACK_NOT_CONFIRMED", "元数据整体回滚需要确认。");
        if (string.IsNullOrWhiteSpace(request.PreRestorePath))
            throw new WorkerOperationException("METADATA_ROLLBACK_PATH_MISSING", "缺少恢复前副本路径。");

        var backupRoot = Path.GetFullPath(Path.Combine(_options.DataDirectory, "MetadataBackups"));
        var preRestore = Path.GetFullPath(request.PreRestorePath);
        if (!preRestore.StartsWith(backupRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new WorkerOperationException("METADATA_ROLLBACK_PATH_INVALID", "回滚目录不在元数据灾备目录内，已拒绝。", request.PreRestorePath);
        var preDatabase = Path.Combine(preRestore, "gamesavecenter.db");
        if (!File.Exists(preDatabase))
            throw new WorkerOperationException("METADATA_ROLLBACK_PRE_RESTORE_MISSING", "恢复前数据库副本不存在，无法整体回滚。", preDatabase);

        _options.SafeModeEnabled = true;
        _options.PersistNow();
        try
        {
            SqliteConnection.ClearAllPools();
            DeleteSidecars(_options.DatabasePath);
            await AtomicFileWriter.ReplaceFileAsync(preDatabase, _options.DatabasePath, token).ConfigureAwait(false);
            File.SetAttributes(_options.DatabasePath, FileAttributes.Normal);
            DeleteSidecars(_options.DatabasePath);

            var preSettings = Path.Combine(preRestore, "worker-settings.json");
            if (File.Exists(preSettings))
            {
                await AtomicFileWriter.ReplaceFileAsync(preSettings, _options.RuntimeSettingsPath, token).ConfigureAwait(false);
                File.SetAttributes(_options.RuntimeSettingsPath, FileAttributes.Normal);
            }
            _options.ReloadPersistedSettings();

            await ValidateDatabaseAsync(_options.DatabasePath, token).ConfigureAwait(false);
            SqliteConnection.ClearAllPools();
            _logger.LogInformation("Metadata restore rolled back to pre-restore state: {PreRestore}", preRestore);
            return new MetadataRestoreRollbackResultDto
            {
                RolledBack = true,
                Summary = "元数据已整体回滚到恢复前状态；数据库与 Worker 设置已恢复并通过完整性校验。"
            };
        }
        catch (Exception ex)
        {
            throw new WorkerOperationException(
                "METADATA_ROLLBACK_MANUAL_INTERVENTION_REQUIRED",
                "元数据整体回滚失败，需要人工介入。",
                ex.Message);
        }
    }

    private async Task<ExtractedMetadata> ValidateAndExtractAsync(string packagePath, string temporary, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(packagePath) || !File.Exists(packagePath))
            throw new WorkerOperationException("METADATA_PACKAGE_MISSING", "元数据灾备包不存在。", packagePath);

        using var archive = ZipFile.OpenRead(packagePath);
        var manifestEntry = archive.GetEntry("manifest.json")
            ?? throw new WorkerOperationException("METADATA_PACKAGE_INVALID", "灾备包缺少 manifest.json。");
        var manifest = JsonSerializer.Deserialize<MetadataManifestInfo>(
                await ReadEntryTextAsync(manifestEntry, token).ConfigureAwait(false),
                JsonOptions)
            ?? throw new WorkerOperationException("METADATA_PACKAGE_INVALID", "manifest.json 无法解析。");
        if (manifest.SchemaVersion != 1)
            throw new WorkerOperationException("METADATA_PACKAGE_UNSUPPORTED", $"不支持的元数据包 schema：{manifest.SchemaVersion}。");
        if (archive.GetEntry("database/gamesavecenter.db") == null)
            throw new WorkerOperationException("METADATA_PACKAGE_INVALID", "灾备包缺少数据库快照。");

        var entries = new List<string>();
        foreach (var entry in archive.Entries)
        {
            token.ThrowIfCancellationRequested();
            var target = Path.GetFullPath(Path.Combine(temporary, entry.FullName));
            if (!target.StartsWith(Path.GetFullPath(temporary) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new WorkerOperationException("METADATA_PACKAGE_TRAVERSAL", "灾备包包含越界路径，已拒绝。", entry.FullName);
            var directory = Path.GetDirectoryName(target);
            if (!string.IsNullOrWhiteSpace(directory)) Directory.CreateDirectory(directory);
            entry.ExtractToFile(target, true);
            entries.Add(entry.FullName);
        }

        var databasePath = Path.Combine(temporary, "database", "gamesavecenter.db");
        var databaseSha256 = await ComputeSha256Async(databasePath, token).ConfigureAwait(false);
        if (!string.Equals(databaseSha256, manifest.DatabaseSha256, StringComparison.OrdinalIgnoreCase))
            throw new WorkerOperationException("METADATA_PACKAGE_HASH_MISMATCH", "数据库快照哈希与清单不一致，已拒绝恢复。");

        var settingsSha256 = string.Empty;
        var settingsPath = Path.Combine(temporary, "settings", "worker-settings.json");
        if (File.Exists(settingsPath))
        {
            settingsSha256 = await ComputeSha256Async(settingsPath, token).ConfigureAwait(false);
            if (!string.Equals(settingsSha256, manifest.SettingsSha256, StringComparison.OrdinalIgnoreCase))
                throw new WorkerOperationException("METADATA_PACKAGE_HASH_MISMATCH", "设置文件哈希与清单不一致，已拒绝恢复。");
        }

        var pluginSettingsSha256 = string.Empty;
        var pluginSettingsJson = string.Empty;
        var pluginSettingsPath = Path.Combine(temporary, "settings", "plugin-settings.json");
        if (File.Exists(pluginSettingsPath))
        {
            pluginSettingsJson = await File.ReadAllTextAsync(pluginSettingsPath, token).ConfigureAwait(false);
            pluginSettingsSha256 = await ComputeSha256Async(pluginSettingsPath, token).ConfigureAwait(false);
            if (!string.Equals(pluginSettingsSha256, manifest.PluginSettingsSha256, StringComparison.OrdinalIgnoreCase))
                throw new WorkerOperationException("METADATA_PACKAGE_HASH_MISMATCH", "Playnite 插件设置哈希与清单不一致，已拒绝恢复。");
        }

        return new ExtractedMetadata(manifest.SchemaVersion, databaseSha256, settingsSha256, pluginSettingsSha256, pluginSettingsJson, entries);
    }

    private static async Task<string> ReadEntryTextAsync(ZipArchiveEntry entry, CancellationToken token)
    {
        using var reader = new StreamReader(entry.Open());
        return await reader.ReadToEndAsync(token).ConfigureAwait(false);
    }

    private static async Task ValidateDatabaseAsync(string path, CancellationToken token)
    {
        await using var connection = new SqliteConnection($"Data Source={path};Mode=ReadOnly;Cache=Shared;Foreign Keys=True");
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = "PRAGMA integrity_check;";
        var value = Convert.ToString(await command.ExecuteScalarAsync(token).ConfigureAwait(false));
        if (!string.Equals(value, "ok", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("恢复后的数据库完整性检查失败：" + value);
    }

    private async Task CreateDatabaseSnapshotAsync(string destination, CancellationToken token)
    {
        await using var connection = new SqliteConnection($"Data Source={_options.DatabasePath};Mode=ReadWriteCreate;Cache=Shared;Foreign Keys=True");
        await connection.OpenAsync(token).ConfigureAwait(false);
        var command = connection.CreateCommand();
        command.CommandText = "VACUUM INTO " + Quote(destination) + ";";
        await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
    }

    private static string BuildReadme(DateTime createdUtc) =>
        "GameSaveCenter 元数据灾备包\n"
        + "生成时间（UTC）：" + createdUtc.ToString("O") + "\n"
        + "\n包含：\n"
        + "- SQLite 数据库一致性快照（database/gamesavecenter.db）\n"
        + "- Worker 设置（settings/worker-settings.json，敏感字段已脱敏）\n"
        + "- Playnite 插件设置（settings/plugin-settings.json，设备身份不包含）\n"
        + "- 版本与校验清单（manifest.json）\n"
        + "\n不包含真实存档、媒体文件、Rclone 配置或凭据。\n";

    private static int AddText(ZipArchive archive, string name, string content)
    {
        var entry = archive.CreateEntry(name, CompressionLevel.Fastest);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        writer.Write(Sanitize(content));
        return 1;
    }

    private static async Task<string> ComputeSha256Async(string path, CancellationToken token)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 128, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var hash = await SHA256.HashDataAsync(stream, token).ConfigureAwait(false);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string Quote(string path) => "'" + path.Replace("'", "''") + "'";

    private static string Sanitize(string? value) => SecretPattern.Replace(value ?? string.Empty, "$1$2[REDACTED]");

    private static void TryDeleteFile(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }

    private static void DeleteSidecars(string path)
    {
        TryDeleteFile(path + "-wal");
        TryDeleteFile(path + "-shm");
    }

    private static void TryDeleteDirectory(string path)
    {
        try { if (Directory.Exists(path)) Directory.Delete(path, true); } catch { }
    }

    private sealed record ExtractedMetadata(
        int SchemaVersion,
        string DatabaseSha256,
        string SettingsSha256,
        string PluginSettingsSha256,
        string PluginSettingsJson,
        List<string> Entries);

    private sealed class MetadataManifestInfo
    {
        public int SchemaVersion { get; set; }
        public string DatabaseSha256 { get; set; } = string.Empty;
        public string SettingsSha256 { get; set; } = string.Empty;
        public string PluginSettingsSha256 { get; set; } = string.Empty;
    }
}
