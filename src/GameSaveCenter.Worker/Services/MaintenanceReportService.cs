using System.Text;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>
/// Builds a concise user-readable health report from real Worker state. It is deliberately
/// not a diagnostics ZIP: no secrets, logs or raw database payloads are included.
/// </summary>
public sealed class MaintenanceReportService
{
    private readonly WorkerOptions _options;
    private readonly SqliteStateStore _store;
    private readonly StorageAnalysisService _storageAnalysis;
    private readonly LocalMirrorService _localMirror;
    private readonly IntegrityCheckService _integrityCheck;
    private readonly RetentionSimulationService _retentionSimulation;
    private readonly ILogger<MaintenanceReportService> _logger;

    public MaintenanceReportService(
        WorkerOptions options,
        SqliteStateStore store,
        StorageAnalysisService storageAnalysis,
        LocalMirrorService localMirror,
        IntegrityCheckService integrityCheck,
        RetentionSimulationService retentionSimulation,
        ILogger<MaintenanceReportService> logger)
    {
        _options = options;
        _store = store;
        _storageAnalysis = storageAnalysis;
        _localMirror = localMirror;
        _integrityCheck = integrityCheck;
        _retentionSimulation = retentionSimulation;
        _logger = logger;
    }

    public async Task<MaintenanceReportDto> GetAsync(CancellationToken token)
    {
        var counts = await _store.GetCountsAsync(token).ConfigureAwait(false);
        var rows = await _store.GetStorageAnalysisRowsAsync(token).ConfigureAwait(false);
        var games = await _store.GetDashboardGameRecordsAsync(token).ConfigureAwait(false);
        var integrity = await _integrityCheck.RunAsync(token).ConfigureAwait(false);
        var storage = await _storageAnalysis.AnalyzeAsync(token).ConfigureAwait(false);
        var mirror = await _localMirror.StatusAsync(token).ConfigureAwait(false);
        var quarantine = await _retentionSimulation.GetQuarantineStatusAsync(token).ConfigureAwait(false);

        var ready = rows.Count(x => x.RestoreReadiness?.Status == RestoreReadinessStatus.Ready);
        var warning = rows.Count(x => x.RestoreReadiness?.Status == RestoreReadinessStatus.Warning);
        var corrupted = rows.Count(x => x.RestoreReadiness?.Status is RestoreReadinessStatus.Corrupted or RestoreReadinessStatus.Failed);
        var protectedGames = games.Count(x => x.Policy.Enabled);
        var attention = integrity.Findings.Count(x => x.Severity is "Warning" or "Error");
        var missingTools = integrity.Findings.Count(x => x.Code == "GAME_TOOL_FILE_MISSING");
        var storagePercent = storage.VolumeTotalBytes > 0
            ? storage.VolumeUsedBytes * 100d / storage.VolumeTotalBytes
            : 0;

        var builder = new StringBuilder();
        builder.AppendLine("GameSaveCenter 健康报告");
        builder.AppendLine($"生成时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine();
        builder.AppendLine($"数据库：{integrity.State}（{integrity.ErrorCount} 错误 / {integrity.WarningCount} 警告）");
        builder.AppendLine($"备份仓库：{(storage.BackupDirectoryAvailable ? "可访问" : "不可访问")}，目录实测 {storage.RepositoryBytesDisplay}");
        builder.AppendLine($"恢复点：Ready {ready}，Warning {warning}，Corrupted {corrupted}");
        builder.AppendLine($"最近游戏：已保护 {protectedGames}，需关注 {attention}，已匹配存档 {counts.Matched}");
        builder.AppendLine($"云端：{CloudDisplay()}");
        builder.AppendLine($"本地镜像：{mirror.Message}");
        builder.AppendLine($"工具：{missingTools} 个外部路径失效");
        builder.AppendLine($"存储：{storagePercent:0.#}% used（{storage.VolumeFreeDisplay} 剩余）");
        builder.AppendLine($"上次完整性自检：{integrity.Summary}");
        builder.AppendLine($"保留清理隔离区：{quarantine.PendingCount} 个条目，占用 {FormatBytes(quarantine.OccupancyBytes)}，待恢复 {quarantine.RecoveryRequiredCount}");

        var summary = $"健康报告：数据库 {integrity.State}，恢复点 Ready {ready}，需关注 {attention}，存储 {storagePercent:0.#}% used，隔离区待处理 {quarantine.PendingCount} 个。";
        return new MaintenanceReportDto
        {
            GeneratedUtc = DateTime.UtcNow,
            Summary = summary,
            ReportText = builder.ToString()
        };
    }

    private string CloudDisplay()
    {
        if (!_options.EnableCloudUpload || string.IsNullOrWhiteSpace(_options.RcloneDestination)) return "未启用/未配置";
        return "已配置（仅 copy/check，不做删除）";
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes:0} B";
        if (bytes < 1024L * 1024) return $"{bytes / 1024d:0.##} KiB";
        if (bytes < 1024L * 1024 * 1024) return $"{bytes / 1024d / 1024d:0.##} MiB";
        return $"{bytes / 1024d / 1024d / 1024d:0.##} GiB";
    }
}
