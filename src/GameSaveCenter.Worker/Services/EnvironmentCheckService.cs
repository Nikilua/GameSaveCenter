using System.Diagnostics;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>Runs bounded, non-destructive checks needed before a first backup.</summary>
public sealed class EnvironmentCheckService
{
    private readonly WorkerOptions _options;
    private readonly SqliteStateStore _store;
    private readonly GameCatalogService _catalog;
    private readonly LudusaviClient _ludusavi;
    private readonly RcloneClient _rclone;
    private readonly ILogger<EnvironmentCheckService> _logger;

    public EnvironmentCheckService(WorkerOptions options, SqliteStateStore store, GameCatalogService catalog,
        LudusaviClient ludusavi, RcloneClient rclone, ILogger<EnvironmentCheckService> logger)
    {
        _options = options;
        _store = store;
        _catalog = catalog;
        _ludusavi = ludusavi;
        _rclone = rclone;
        _logger = logger;
    }

    public async Task<EnvironmentCheckReportDto> RunAsync(EnvironmentCheckRequestDto request, CancellationToken token)
    {
        var report = new EnvironmentCheckReportDto { CheckedUtc = DateTime.UtcNow };
        Add(report, await CheckWorkerAsync(token).ConfigureAwait(false));
        Add(report, CheckDirectory("data", "数据目录", _options.DataDirectory, false));
        Add(report, CheckDirectory("backup", "存档目录", _options.LudusaviBackupDirectory, false));
        Add(report, CheckDirectory("media", "媒体目录", _options.MediaArchiveDirectory, true));
        Add(report, await CheckDatabaseAsync(token).ConfigureAwait(false));
        Add(report, await CheckLibraryAsync(token).ConfigureAwait(false));
        Add(report, await CheckLudusaviAsync(token).ConfigureAwait(false));
        Add(report, await CheckRcloneAsync(request.IncludeRemoteProbe, token).ConfigureAwait(false));
        Add(report, CheckDiskSpace());

        report.PassedCount = report.Items.Count(x => x.State == EnvironmentCheckState.Passed);
        report.WarningCount = report.Items.Count(x => x.State == EnvironmentCheckState.Warning);
        report.FailedCount = report.Items.Count(x => x.State == EnvironmentCheckState.Failed);
        report.SkippedCount = report.Items.Count(x => x.State == EnvironmentCheckState.Skipped);
        report.Summary = report.FailedCount > 0
            ? $"检查完成：{report.FailedCount} 项失败，{report.WarningCount} 项需注意。"
            : report.WarningCount > 0
                ? $"检查完成：基础环境可用，{report.WarningCount} 项需注意。"
                : "检查完成：环境已准备好，可以手动执行一次测试备份。";
        return report;
    }

    private static void Add(EnvironmentCheckReportDto report, EnvironmentCheckItemDto item) => report.Items.Add(item);

    private static Task<EnvironmentCheckItemDto> CheckWorkerAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var version = typeof(EnvironmentCheckService).Assembly.GetName().Version?.ToString() ?? "dev";
        return Task.FromResult(Passed("worker", "Worker 服务", "IPC 请求已成功到达 Worker。", version));
    }

    private async Task<EnvironmentCheckItemDto> CheckDatabaseAsync(CancellationToken token)
    {
        try
        {
            await _store.ProbeReadWriteAsync(token).ConfigureAwait(false);
            return Passed("database", "SQLite 数据库", "数据库可读取和写入临时探针。", _options.DatabasePath);
        }
        catch (Exception ex)
        {
            return Failed("database", "SQLite 数据库", "数据库读写检查失败。", ex.Message);
        }
    }

    private async Task<EnvironmentCheckItemDto> CheckLibraryAsync(CancellationToken token)
    {
        try
        {
            var games = await _catalog.GetGamesAsync(token).ConfigureAwait(false);
            return games.Count == 0
                ? Warning("library", "Playnite 游戏库", "Worker 可读取游戏库，但还没有同步到本地缓存。", "请回到概览页刷新游戏库。")
                : Passed("library", "Playnite 游戏库", $"已读取 {games.Count} 个游戏。", string.Empty);
        }
        catch (Exception ex)
        {
            return Failed("library", "Playnite 游戏库", "无法读取本地游戏库缓存。", ex.Message);
        }
    }

    private async Task<EnvironmentCheckItemDto> CheckLudusaviAsync(CancellationToken token)
    {
        if (!_ludusavi.IsAvailable)
            return Failed("ludusavi", "Ludusavi", "未配置有效的 Ludusavi 可执行文件。", _options.LudusaviExecutable);

        try
        {
            var version = await _ludusavi.GetVersionAsync(token).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(version)) return Failed("ludusavi", "Ludusavi", "Ludusavi 无法返回版本信息。", _options.LudusaviExecutable);
            var result = await _ludusavi.ListBackupsAsync(Array.Empty<string>(), token).ConfigureAwait(false);
            var item = result.Success
                ? Passed("ludusavi", "Ludusavi", "版本检查和只读备份列表调用均成功。", _options.LudusaviExecutable)
                : Failed("ludusavi", "Ludusavi", "版本可读取，但只读备份列表调用失败。", result.ErrorMessage);
            item.Version = version;
            return item;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Ludusavi environment check failed");
            return Failed("ludusavi", "Ludusavi", "Ludusavi 检查失败。", ex.Message);
        }
    }

    private async Task<EnvironmentCheckItemDto> CheckRcloneAsync(bool includeRemoteProbe, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(_options.RcloneExecutable) || string.IsNullOrWhiteSpace(_options.RcloneDestination))
            return Skipped("rclone", "Rclone 与云端", "未配置可选的 Rclone 远端。", "可在设置中配置，之后重新运行检查。", true);
        if (!_rclone.IsAvailable)
            return Failed("rclone", "Rclone 与云端", "已填写 Rclone 配置，但可执行文件不存在。", _options.RcloneExecutable);

        try
        {
            var version = await _rclone.GetVersionAsync(token).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(version)) return Failed("rclone", "Rclone 与云端", "Rclone 无法返回版本信息。", _options.RcloneExecutable);
            if (!includeRemoteProbe)
            {
                var skippedProbe = Passed("rclone", "Rclone 与云端", "Rclone 版本可用；本次跳过远端探测。", _options.RcloneDestination);
                skippedProbe.Version = version;
                return skippedProbe;
            }
            var probe = await _rclone.ProbeRemoteAsync(token).ConfigureAwait(false);
            var item = probe.Success
                ? Passed("rclone", "Rclone 与云端", "版本检查和远端只读探测均成功。", _options.RcloneDestination)
                : Failed("rclone", "Rclone 与云端", "Rclone 可启动，但远端只读探测失败。", probe.StandardError);
            item.Version = version;
            return item;
        }
        catch (Exception ex)
        {
            return Failed("rclone", "Rclone 与云端", "Rclone 检查失败。", ex.Message);
        }
    }

    private EnvironmentCheckItemDto CheckDiskSpace()
    {
        try
        {
            var root = Path.GetPathRoot(_options.DataDirectory);
            if (string.IsNullOrWhiteSpace(root)) return Warning("disk", "磁盘空间", "无法识别数据目录所在磁盘。", _options.DataDirectory);
            var drive = new DriveInfo(root);
            var free = FormatBytes(drive.AvailableFreeSpace);
            return drive.AvailableFreeSpace < 512L * 1024 * 1024
                ? Warning("disk", "磁盘空间", $"可用空间约 {free}，低于建议阈值 512 MiB。", drive.Name)
                : Passed("disk", "磁盘空间", $"可用空间约 {free}。", drive.Name);
        }
        catch (Exception ex)
        {
            return Warning("disk", "磁盘空间", "无法读取可用空间。", ex.Message);
        }
    }

    private static EnvironmentCheckItemDto CheckDirectory(string key, string title, string path, bool optional)
    {
        if (string.IsNullOrWhiteSpace(path)) return optional
            ? Skipped(key, title, "未配置可选目录。", string.Empty, true)
            : Failed(key, title, "目录路径为空。", string.Empty);
        try
        {
            Directory.CreateDirectory(path);
            var probePath = Path.Combine(path, ".gsc-environment-check-" + Guid.NewGuid().ToString("N") + ".tmp");
            File.WriteAllText(probePath, "ok");
            File.Delete(probePath);
            return Passed(key, title, "目录可创建、写入和删除临时探针。", path);
        }
        catch (Exception ex)
        {
            return optional
                ? Warning(key, title, "目录不可写，媒体归档可能不可用。", ex.Message)
                : Failed(key, title, "目录不可写。", ex.Message);
        }
    }

    private static EnvironmentCheckItemDto Passed(string key, string title, string summary, string detail) => Item(key, title, EnvironmentCheckState.Passed, summary, detail, false);
    private static EnvironmentCheckItemDto Warning(string key, string title, string summary, string detail) => Item(key, title, EnvironmentCheckState.Warning, summary, detail, false);
    private static EnvironmentCheckItemDto Failed(string key, string title, string summary, string detail) => Item(key, title, EnvironmentCheckState.Failed, summary, detail, false);
    private static EnvironmentCheckItemDto Skipped(string key, string title, string summary, string detail, bool optional) => Item(key, title, EnvironmentCheckState.Skipped, summary, detail, optional);
    private static EnvironmentCheckItemDto Item(string key, string title, EnvironmentCheckState state, string summary, string detail, bool optional) => new()
    { Key = key, Title = title, State = state, Summary = summary, Detail = detail, Version = string.Empty, IsOptional = optional };

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return bytes + " B";
        if (bytes < 1024L * 1024) return $"{bytes / 1024d:0.##} KiB";
        if (bytes < 1024L * 1024 * 1024) return $"{bytes / 1024d / 1024d:0.##} MiB";
        return $"{bytes / 1024d / 1024d / 1024d:0.##} GiB";
    }
}
