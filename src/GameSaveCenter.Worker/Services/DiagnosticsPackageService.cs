using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>
/// Creates a bounded, read-only support package with structured system, worker,
/// dependency, database, task, health and sanitized settings files plus bounded logs.
/// Every string that leaves this service passes through <see cref="DiagnosticRedactor"/>.
/// </summary>
public sealed class DiagnosticsPackageService
{
    private const int MaxLogBytes = 256 * 1024;
    private const int MaxTotalBytes = 2 * 1024 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };
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
        var findings = await _store.GetOpenFindingsAsync(100, token).ConfigureAwait(false);
        var counts = await _store.GetCountsAsync(token).ConfigureAwait(false);
        var healthCounts = await _store.GetHealthStateCountsAsync(token).ConfigureAwait(false);
        var dbProbe = await ProbeDatabaseAsync(token).ConfigureAwait(false);

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
                included += AddText(archive, "system.json", JsonSerializer.Serialize(BuildSystem(request, createdUtc), JsonOptions));
                included += AddText(archive, "worker.json", JsonSerializer.Serialize(BuildWorker(createdUtc), JsonOptions));
                included += AddText(archive, "dependencies.json", JsonSerializer.Serialize(BuildDependencies(), JsonOptions));
                included += AddText(archive, "database.json", JsonSerializer.Serialize(BuildDatabase(dbProbe), JsonOptions));
                included += AddText(archive, "recent-tasks.json", JsonSerializer.Serialize(BuildTasks(tasks), JsonOptions));
                included += AddText(archive, "health.json", JsonSerializer.Serialize(BuildHealth(counts, healthCounts, findings), JsonOptions));
                included += AddText(archive, "settings.json", JsonSerializer.Serialize(_options.ToDto(), JsonOptions));
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
            await _store.AppendAuditAsync("Diagnostics", "已生成脱敏诊断包", JsonSerializer.Serialize(new
            {
                result.IncludedFileCount,
                result.PackageBytes,
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
        + "\n此包仅包含有限的运行摘要、结构化状态、任务、审计和日志尾部。\n"
        + "未包含真实存档、媒体文件、SQLite 数据库、Rclone 配置或凭据。\n";

    private static object BuildSystem(CreateDiagnosticsPackageRequestDto? request, DateTime createdUtc) => new
    {
        createdUtc,
        pluginVersion = request?.PluginVersion ?? string.Empty,
        playniteVersion = request?.PlayniteVersion ?? string.Empty,
        workerVersion = typeof(DiagnosticsPackageService).Assembly.GetName().Version?.ToString() ?? "dev",
        windowsVersion = Environment.OSVersion.ToString(),
        dotNetRuntime = Environment.Version.ToString(),
        machineArchitecture = Environment.Is64BitProcess ? "x64" : "x86",
        dpiScale = request?.DpiScale ?? 1,
        screenCount = request?.ScreenCount ?? 1,
        theme = request?.ThemeMode ?? string.Empty,
        currentWorkspace = request?.CurrentWorkspace ?? string.Empty
    };

    private static object BuildWorker(DateTime createdUtc)
    {
        using var process = Process.GetCurrentProcess();
        return new
        {
            running = true,
            pid = process.Id,
            startTimeUtc = process.StartTime.ToUniversalTime().ToString("O"),
            uptimeSeconds = Math.Max(0, (long)(createdUtc - process.StartTime.ToUniversalTime()).TotalSeconds),
            ipcProtocol = GameSaveCenter.Contracts.ProtocolConstants.ProtocolVersion,
            ipcState = "Ready",
            lastHealthProbe = "见 worker-launch.log"
        };
    }

    private object BuildDependencies() => new
    {
        ludusaviConfigured = !string.IsNullOrWhiteSpace(_options.LudusaviExecutable),
        ludusaviPath = DiagnosticRedactor.Redact(_options.LudusaviExecutable),
        ludusaviVersion = "未探测",
        rcloneConfigured = !string.IsNullOrWhiteSpace(_options.RcloneExecutable) && !string.IsNullOrWhiteSpace(_options.RcloneDestination),
        rclonePath = DiagnosticRedactor.Redact(_options.RcloneExecutable),
        rcloneVersion = "未探测",
        rcloneDestination = DiagnosticRedactor.Redact(_options.RcloneDestination),
        workerAvailable = true
    };

    private object BuildDatabase(string dbProbe) => new
    {
        schemaVersion = "current",
        databasePath = DiagnosticRedactor.Redact(_options.DatabasePath),
        databaseSizeBytes = File.Exists(_options.DatabasePath) ? new FileInfo(_options.DatabasePath).Length : 0,
        lastMigration = "由 DatabaseMigrationHarness 验证",
        integrityProbe = dbProbe
    };

    private static object BuildTasks(IEnumerable<TaskStatusDto> tasks) => tasks.Select(task => new
    {
        task.TaskId,
        task.TaskType,
        task.GameId,
        task.State,
        durationSeconds = task.StartedUtc.HasValue && task.FinishedUtc.HasValue
            ? Math.Max(0, (task.FinishedUtc.Value - task.StartedUtc.Value).TotalSeconds)
            : (double?)null,
        task.ErrorCode,
        errorMessage = DiagnosticRedactor.Redact(task.ErrorMessage)
    });

    private static object BuildHealth(
        (int Games, int Matched, int Media, int Unassigned) counts,
        IReadOnlyDictionary<string,int> healthCounts,
        IReadOnlyList<ValidationFindingDto> findings) => new
    {
        totalGames = counts.Games,
        matchedGames = counts.Matched,
        mediaCount = counts.Media,
        unassignedMedia = counts.Unassigned,
        healthStateCounts = healthCounts,
        attentionGames = healthCounts.TryGetValue("Attention", out var attention) ? attention : 0,
        riskGames = healthCounts.TryGetValue("Risk", out var risk) ? risk : 0,
        recentFindings = findings.Select(finding => new
        {
            finding.PlayniteId,
            finding.Severity,
            finding.Code,
            title = DiagnosticRedactor.Redact(finding.Title),
            detail = DiagnosticRedactor.Redact(finding.Detail),
            finding.SuggestedAction
        })
    };

    private static string BuildAudit(IEnumerable<AuditLogEntryDto> entries)
    {
        var builder = new StringBuilder("最近审计记录（最多 300 条）\n");
        foreach (var entry in entries)
        {
            builder.Append(entry.CreatedLocal.ToString("O")).Append(" | ")
                .Append(DiagnosticRedactor.Redact(entry.Category)).Append(" | ")
                .Append(DiagnosticRedactor.Redact(entry.Message)).Append(" | ")
                .AppendLine(DiagnosticRedactor.Redact(entry.DetailJson));
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
        writer.Write(DiagnosticRedactor.Redact(content));
        return 1;
    }

    private async Task<string> ProbeDatabaseAsync(CancellationToken token)
    {
        try
        {
            await _store.ProbeReadWriteAsync(token).ConfigureAwait(false);
            return "ok";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Diagnostics database probe failed");
            return "failed: " + ex.GetType().Name;
        }
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

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }
}
