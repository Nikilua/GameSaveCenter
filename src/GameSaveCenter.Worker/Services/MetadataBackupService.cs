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

    public async Task<MetadataBackupResultDto> CreateAsync(CancellationToken token)
    {
        if (!File.Exists(_options.DatabasePath))
            throw new WorkerOperationException("METADATA_DATABASE_MISSING", "GameSaveCenter 数据库不存在，无法生成元数据灾备包。", _options.DatabasePath);

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

            var manifest = new
            {
                schemaVersion = 1,
                createdUtc,
                workerVersion = typeof(MetadataBackupService).Assembly.GetName().Version?.ToString() ?? "dev",
                databaseFile = "database/gamesavecenter.db",
                databaseSha256,
                settingsFile = hasSettings ? "settings/worker-settings.json" : null,
                settingsSha256,
                excludes = new[] { "存档", "媒体", "Rclone 凭据" }
            };

            using (var archive = ZipFile.Open(temporaryZip, ZipArchiveMode.Create))
            {
                AddText(archive, "manifest.json", JsonSerializer.Serialize(manifest, JsonOptions));
                AddText(archive, "README.txt", BuildReadme(createdUtc));
                archive.CreateEntryFromFile(databaseSnapshot, "database/gamesavecenter.db", CompressionLevel.Fastest);
                if (hasSettings)
                    archive.CreateEntryFromFile(settingsPath, "settings/worker-settings.json", CompressionLevel.Fastest);
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
                IncludedFileCount = hasSettings ? 4 : 3,
                Summary = $"元数据灾备包已生成：SQLite 快照、Worker 设置和版本清单；未包含存档、媒体或凭据。"
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

    private static void TryDeleteDirectory(string path)
    {
        try { if (Directory.Exists(path)) Directory.Delete(path, true); } catch { }
    }
}
