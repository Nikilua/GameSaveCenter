using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>
/// Creates a bounded, read-only support package. It deliberately excludes save/media
/// archives, database files, credentials and full configuration files.
/// </summary>
public sealed class DiagnosticsPackageService
{
    private const int MaxLogBytes = 256 * 1024;
    private const int MaxTotalBytes = 2 * 1024 * 1024;
    private static readonly Regex SecretPattern = new(
        @"(?i)(password|passwd|token|secret|api[_-]?key|access[_-]?token)[""']?\s*([=:])\s*[""']?([^""'\s,;}]+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private readonly WorkerOptions _options;
    private readonly SqliteStateStore _store;
    private readonly ILogger<DiagnosticsPackageService> _logger;

    public DiagnosticsPackageService(WorkerOptions options, SqliteStateStore store, ILogger<DiagnosticsPackageService> logger)
    {
        _options = options;
        _store = store;
        _logger = logger;
    }

    public async Task<DiagnosticsPackageResultDto> CreateAsync(CreateDiagnosticsPackageRequestDto? request, CancellationToken token)
    {
        var createdUtc = DateTime.UtcNow;
        var audit = await _store.GetAuditAsync(Math.Clamp(request?.AuditLimit ?? 300, 1, 300), token).ConfigureAwait(false);
        var tasks = await _store.GetRecentTasksAsync(Math.Clamp(request?.TaskLimit ?? 200, 1, 200), token).ConfigureAwait(false);
        var packageDirectory = Path.Combine(_options.DataDirectory, "Diagnostics");
        Directory.CreateDirectory(packageDirectory);
        var stem = "gsc-diagnostics-" + createdUtc.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N");
        var temporaryPath = Path.Combine(packageDirectory, stem + ".tmp");
        var packagePath = Path.Combine(packageDirectory, stem + ".zip");
        var included = 0;
        try
        {
            using (var archive = ZipFile.Open(temporaryPath, ZipArchiveMode.Create))
            {
                included += AddText(archive, "README.txt", BuildReadme(createdUtc));
                included += AddText(archive, "environment.txt", BuildEnvironmentSummary(createdUtc));
                included += AddText(archive, "tasks.txt", BuildTasks(tasks));
                included += AddText(archive, "audit.txt", BuildAudit(audit));
                included += AddTailLogs(archive);
            }

            var packageInfo = new FileInfo(temporaryPath);
            if (packageInfo.Length > MaxTotalBytes)
                throw new InvalidOperationException("诊断包超过安全大小上限，未保留输出文件。");
            File.Move(temporaryPath, packagePath);
            var result = new DiagnosticsPackageResultDto
            {
                PackagePath = packagePath,
                CreatedUtc = createdUtc,
                PackageBytes = new FileInfo(packagePath).Length,
                IncludedFileCount = included,
                Summary = $"已生成诊断包，包含 {included} 个脱敏诊断文件；未包含存档、媒体或凭据。"
            };
            await _store.AppendAuditAsync("Diagnostics", "已生成脱敏诊断包", System.Text.Json.JsonSerializer.Serialize(new
            {
                result.IncludedFileCount,
                result.PackageBytes,
                // Do not put the absolute package path in the audit record.
                fileName = Path.GetFileName(packagePath)
            }), token).ConfigureAwait(false);
            return result;
        }
        catch
        {
            TryDelete(temporaryPath);
            TryDelete(packagePath);
            throw;
        }
    }

    private string BuildReadme(DateTime createdUtc) =>
        "GameSaveCenter 诊断包\n"
        + "生成时间（UTC）：" + createdUtc.ToString("O") + "\n"
        + "\n此包仅包含有限的运行摘要、任务、审计和日志尾部。\n"
        + "未包含真实存档、媒体文件、SQLite 数据库、Rclone 配置或凭据。\n";

    private string BuildEnvironmentSummary(DateTime createdUtc)
    {
        var builder = new StringBuilder();
        builder.AppendLine("GameSaveCenter 环境摘要");
        builder.AppendLine("生成时间（UTC）：" + createdUtc.ToString("O"));
        builder.AppendLine("Worker 版本：" + (typeof(DiagnosticsPackageService).Assembly.GetName().Version?.ToString() ?? "dev"));
        builder.AppendLine("系统：" + Environment.OSVersion);
        builder.AppendLine("64 位进程：" + Environment.Is64BitProcess);
        builder.AppendLine("数据目录已配置：" + (!string.IsNullOrWhiteSpace(_options.DataDirectory)));
        builder.AppendLine("存档目录已配置：" + (!string.IsNullOrWhiteSpace(_options.LudusaviBackupDirectory)));
        builder.AppendLine("媒体目录已配置：" + (!string.IsNullOrWhiteSpace(_options.MediaArchiveDirectory)));
        builder.AppendLine("Ludusavi 已配置：" + (!string.IsNullOrWhiteSpace(_options.LudusaviExecutable)));
        builder.AppendLine("Rclone 已配置：" + (!string.IsNullOrWhiteSpace(_options.RcloneExecutable) && !string.IsNullOrWhiteSpace(_options.RcloneDestination)));
        builder.AppendLine("设备身份：已省略");
        return builder.ToString();
    }

    private static string BuildTasks(IEnumerable<TaskStatusDto> tasks)
    {
        var builder = new StringBuilder("最近任务（最多 200 条）\n");
        foreach (var task in tasks)
        {
            builder.Append(task.CreatedLocal.ToString("O")).Append(" | ")
                .Append(Sanitize(task.TaskType)).Append(" | ")
                .Append(Sanitize(task.GameName)).Append(" | ")
                .Append(task.State).Append(" | ")
                .Append(Sanitize(task.ErrorCode)).Append(" | ")
                .AppendLine(Sanitize(task.DetailMessage));
        }
        return builder.ToString();
    }

    private static string BuildAudit(IEnumerable<AuditLogEntryDto> entries)
    {
        var builder = new StringBuilder("最近审计记录（最多 300 条）\n");
        foreach (var entry in entries)
        {
            builder.Append(entry.CreatedLocal.ToString("O")).Append(" | ")
                .Append(Sanitize(entry.Category)).Append(" | ")
                .Append(Sanitize(entry.Message)).Append(" | ")
                .AppendLine(Sanitize(entry.DetailJson));
        }
        return builder.ToString();
    }

    private int AddTailLogs(ZipArchive archive)
    {
        var candidates = new[]
        {
            Path.Combine(_options.LogDirectory, "worker-launch.log"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GameSaveCenter", "Logs", "worker-launch.log")
        };
        var included = 0;
        foreach (var path in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!File.Exists(path)) continue;
            var text = ReadTail(path, MaxLogBytes);
            if (text.Length == 0) continue;
            AddText(archive, "logs/" + included + "-worker-launch.log", text);
            included++;
        }
        return included;
    }

    private static int AddText(ZipArchive archive, string name, string content)
    {
        var entry = archive.CreateEntry(name, CompressionLevel.Fastest);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        writer.Write(Sanitize(content));
        return 1;
    }

    private static string ReadTail(string path, int maxBytes)
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var length = (int)Math.Min(stream.Length, maxBytes);
            stream.Seek(-length, SeekOrigin.End);
            var buffer = new byte[length];
            var offset = 0;
            while (offset < buffer.Length)
            {
                var read = stream.Read(buffer, offset, buffer.Length - offset);
                if (read == 0) break;
                offset += read;
            }
            return Encoding.UTF8.GetString(buffer, 0, offset);
        }
        catch (Exception ex)
        {
            return "无法读取日志尾部：" + ex.GetType().Name;
        }
    }

    private static string Sanitize(string? value) => SecretPattern.Replace(value ?? string.Empty, "$1$2[REDACTED]");

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }
}
