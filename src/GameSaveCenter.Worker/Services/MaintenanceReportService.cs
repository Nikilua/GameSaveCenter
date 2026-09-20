using System.Collections.Generic;
using System.Text;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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
    private readonly CloudTransferStateService _cloudTransfers;
    private readonly ILogger<MaintenanceReportService> _logger;

    public MaintenanceReportService(
        WorkerOptions options,
        SqliteStateStore store,
        StorageAnalysisService storageAnalysis,
        LocalMirrorService localMirror,
        IntegrityCheckService integrityCheck,
        RetentionSimulationService retentionSimulation,
        ILogger<MaintenanceReportService> logger)
        : this(options,store,storageAnalysis,localMirror,integrityCheck,retentionSimulation,
            new CloudTransferStateService(
                store,options,
                new RcloneClient(options,new ExternalProcessRunner(NullLogger<ExternalProcessRunner>.Instance)),
                new CloudTransferCoordinator(NullLogger<CloudTransferCoordinator>.Instance),
                NullLogger<CloudTransferStateService>.Instance),
            logger)
    {
    }

    public MaintenanceReportService(
        WorkerOptions options,
        SqliteStateStore store,
        StorageAnalysisService storageAnalysis,
        LocalMirrorService localMirror,
        IntegrityCheckService integrityCheck,
        RetentionSimulationService retentionSimulation,
        CloudTransferStateService cloudTransfers,
        ILogger<MaintenanceReportService> logger)
    {
        _options = options;
        _store = store;
        _storageAnalysis = storageAnalysis;
        _localMirror = localMirror;
        _integrityCheck = integrityCheck;
        _retentionSimulation = retentionSimulation;
        _cloudTransfers = cloudTransfers;
        _logger = logger;
    }

    public Task<MaintenanceReportDto> GetAsync(CancellationToken token)
        => GetAsync(new MaintenanceReportRequestDto(), token);

    public async Task<MaintenanceReportDto> GetAsync(MaintenanceReportRequestDto? request, CancellationToken token)
    {
        request ??= new MaintenanceReportRequestDto();
        var generatedUtc = DateTime.UtcNow;
        var generatedLocal = generatedUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
        var counts = await _store.GetCountsAsync(token).ConfigureAwait(false);
        var rows = await _store.GetStorageAnalysisRowsAsync(token).ConfigureAwait(false);
        var games = await _store.GetDashboardGameRecordsAsync(token).ConfigureAwait(false);
        var integrity = await _integrityCheck.RunAsync(token).ConfigureAwait(false);
        var storage = await _storageAnalysis.AnalyzeAsync(token).ConfigureAwait(false);
        var mirror = await _localMirror.StatusAsync(token).ConfigureAwait(false);
        var quarantine = await _retentionSimulation.GetQuarantineStatusAsync(token).ConfigureAwait(false);
        var inspection = await _store.GetHealthInspectionStateAsync(token).ConfigureAwait(false);
        var cloudTransfers = await _cloudTransfers.GetStatusAsync(token).ConfigureAwait(false);

        var ready = rows.Count(x => x.RestoreReadiness?.Status == RestoreReadinessStatus.Ready);
        var warning = rows.Count(x => x.RestoreReadiness?.Status == RestoreReadinessStatus.Warning);
        var corrupted = rows.Count(x => x.RestoreReadiness?.Status is RestoreReadinessStatus.Corrupted or RestoreReadinessStatus.Failed);
        var protectedGames = games.Count(x => x.Policy.Enabled);
        var attention = integrity.Findings.Count(x => x.Severity is "Warning" or "Error");
        var missingTools = integrity.Findings.Count(x => x.Code == "GAME_TOOL_FILE_MISSING");
        var storagePercent = storage.VolumeTotalBytes > 0
            ? storage.VolumeUsedBytes * 100d / storage.VolumeTotalBytes
            : 0;

        var pending = new List<string>();
        foreach (var finding in integrity.Findings.Where(x => x.Severity is "Warning" or "Error").Take(20))
        {
            var detail = string.IsNullOrWhiteSpace(finding.Detail) ? string.Empty : $"：{finding.Detail}";
            var action = string.IsNullOrWhiteSpace(finding.SuggestedAction) ? string.Empty : $"；建议：{finding.SuggestedAction}";
            pending.Add($"{finding.Title}{detail}{action}");
        }

        if (corrupted > 0)
            pending.Add($"恢复点中有 {corrupted} 个版本状态为 Corrupted/Failed。");
        if (missingTools > 0)
            pending.Add($"有 {missingTools} 个外部工具路径失效。");
        foreach (var item in cloudTransfers.Items.Where(x => x.State is "RetryScheduled" or "AuthenticationRequired" or "CheckFailed" or "Failed").Take(8))
            pending.Add($"云端：{item.KindDisplay} · {item.GameName} · {item.DetailDisplay}");
        if (quarantine.PendingCount > 0)
            pending.Add($"保留清理隔离区有 {quarantine.PendingCount} 个待处理条目，其中 {quarantine.RecoveryRequiredCount} 个需要恢复。");
        if (storage.MissingIndexedPathCount > 0)
            pending.Add($"有 {storage.MissingIndexedPathCount} 个索引版本的归档路径失联。");

        var verified = new List<string>
        {
            $"数据库完整性结果：{integrity.StateDisplay}（{integrity.ErrorCount} 错误 / {integrity.WarningCount} 警告 / {integrity.SkippedCount} 跳过）。",
            $"恢复点统计：Ready {ready}，Warning {warning}，Corrupted {corrupted}。",
            $"最近游戏：已保护 {protectedGames}，需关注 {attention}，已匹配存档 {counts.Matched}。",
            $"云端快照：{CloudDisplay()}；{cloudTransfers.SummaryDisplay}；{cloudTransfers.QueueControlDisplay}。",
            $"本地镜像：{mirror.Message}",
            $"存储卷：{storagePercent:0.#}% used（{storage.VolumeFreeDisplay} 剩余）。",
            $"上次完整性自检：{integrity.Summary}",
            $"恢复可用性巡检：{inspection.LastStatusDisplay}；最近成功验证：{inspection.LastSuccessfulLocalDisplay}；下次计划：{inspection.NextDueLocalDisplay}",
            $"保留清理隔离区：{quarantine.PendingCount} 个条目，占用 {FormatBytes(quarantine.OccupancyBytes)}，待恢复 {quarantine.RecoveryRequiredCount}。"
        };

        var unknown = new List<string>();
        if (!storage.BackupDirectoryAvailable)
            unknown.Add("备份仓库目录不可访问，路径状态未知，不能按 0 解释。");
        if (storage.MissingIndexedPathCount > 0)
            unknown.Add(storage.MissingIndexedPathSummary);
        if (inspection.LastSuccessfulLocalDisplay == "未知")
            unknown.Add("健康巡检尚无可证明的最近成功验证时间。");
        if (integrity.SkippedCount > 0)
            unknown.Add($"完整性自检有 {integrity.SkippedCount} 项跳过，未将其当作健康。");

        var summary = $"摘要：待处理 {pending.Count} 项，已验证 {verified.Count} 项，未知 {unknown.Count} 项。生成时间 {generatedLocal}。";
        var builder = new StringBuilder();
        builder.AppendLine("GameSaveCenter 健康报告");
        builder.AppendLine($"生成时间：{generatedLocal}");
        builder.AppendLine();
        builder.AppendLine("## 软件身份");
        builder.AppendLine($"GameSaveCenter 插件：{DisplayIdentity(request.PluginVersion)} / 构建 {DisplayIdentity(request.PluginBuildIdentity)}");
        builder.AppendLine($"GameSaveCenter Worker：{typeof(MaintenanceReportService).Assembly.GetName().Version?.ToString() ?? "dev"} / 构建 {BuildIdentity.ForAssembly(typeof(MaintenanceReportService).Assembly)}");
        builder.AppendLine($"Playnite：{DisplayIdentity(request.PlayniteVersion)}");
        builder.AppendLine($"IPC 协议：{ProtocolConstants.ProtocolVersion}；Windows：{Environment.OSVersion.VersionString}；.NET：{Environment.Version}");
        builder.AppendLine();
        builder.AppendLine("## 摘要");
        builder.AppendLine(summary);
        AppendSection(builder, "待处理", pending);
        AppendSection(builder, "已验证", verified);
        AppendSection(builder, "未知", unknown);

        var reportText = MaintenanceReportRedactor.Redact(builder.ToString());
        return new MaintenanceReportDto
        {
            GeneratedUtc = generatedUtc,
            Summary = MaintenanceReportRedactor.Redact(summary),
            ReportText = reportText
        };
    }

    private static void AppendSection(StringBuilder builder, string title, IReadOnlyList<string> items)
    {
        builder.AppendLine();
        builder.AppendLine($"## {title}（{items.Count}）");
        if (items.Count == 0)
        {
            builder.AppendLine("无项目（当前采集范围内）。");
            return;
        }

        foreach (var item in items)
            builder.AppendLine("- " + item);
    }

    private static string DisplayIdentity(string value)
        => string.IsNullOrWhiteSpace(value) ? "未知" : value.Trim();

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
