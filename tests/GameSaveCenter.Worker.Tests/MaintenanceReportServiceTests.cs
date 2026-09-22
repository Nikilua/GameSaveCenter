using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class MaintenanceReportServiceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly WorkerOptions options;
    private readonly SqliteStateStore store;

    public MaintenanceReportServiceTests()
    {
        options = new WorkerOptions
        {
            DataDirectory = Path.Combine(root, "Data"),
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            MediaArchiveDirectory = Path.Combine(root, "Media"),
            LocalMirrorPath = Path.Combine(root, "Mirror")
        };
        Directory.CreateDirectory(options.DataDirectory);
        Directory.CreateDirectory(options.LudusaviBackupDirectory);
        Directory.CreateDirectory(options.MediaArchiveDirectory);
        store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task ReportContainsUserReadableHealthSections()
    {
        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            PlayniteId = "g1",
            BackupId = "b1",
            LudusaviName = "Game One",
            CreatedUtc = DateTime.UtcNow,
            TotalBytes = 1024,
            FileCount = 1,
            RestoreReadiness = new RestoreReadinessDto { Status = RestoreReadinessStatus.Ready }
        }, "{}", CancellationToken.None);

        var service = CreateReportService();

        var report = await service.GetAsync(new MaintenanceReportRequestDto
        {
            PluginVersion = "0.6.73",
            PluginBuildIdentity = "plugin-build-123",
            PlayniteVersion = "10.35"
        }, CancellationToken.None);

        Assert.Contains("GameSaveCenter 健康报告", report.ReportText);
        Assert.Contains("## 软件身份", report.ReportText);
        Assert.Contains("GameSaveCenter 插件：0.6.73 / 构建 plugin-build-123", report.ReportText);
        Assert.Contains("Playnite：10.35", report.ReportText);
        Assert.Contains("## 摘要", report.ReportText);
        Assert.Contains("## 待处理（", report.ReportText);
        Assert.Contains("## 已验证（", report.ReportText);
        Assert.Contains("## 未知（", report.ReportText);
        Assert.Contains(report.Summary, report.ReportText);
        Assert.Contains(report.GeneratedUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"), report.ReportText);
        Assert.Contains("数据库完整性结果", report.ReportText);
        Assert.Contains("恢复点统计", report.ReportText);
        Assert.Contains("本地镜像", report.ReportText);
        Assert.Contains("Ready", report.ReportText);
        Assert.Contains("保留清理隔离区", report.ReportText);
    }

    [Fact]
    public async Task ReportUsesCanonicalUnitForSmallMirrorFile()
    {
        options.EnableLocalMirror = true;
        Directory.CreateDirectory(options.LocalMirrorPath);
        await File.WriteAllBytesAsync(Path.Combine(options.LocalMirrorPath, "one-byte.bin"), new byte[] { 0x2A });

        var report = await CreateReportService().GetAsync(new MaintenanceReportRequestDto(), CancellationToken.None);

        Assert.Contains("本地镜像：镜像可用：1 个文件，共 1 B", report.ReportText);
        Assert.DoesNotContain("0 KiB", report.ReportText);
    }

    [Fact]
    public void ReportRedactorRemovesUrlParametersAndWindowsUserNames()
    {
        var redacted = MaintenanceReportRedactor.Redact(
            "远端 https://example.test/save?token=secret-value&user=alice#fragment；路径 C:\\Users\\Alice\\GameSaveCenter\\save.db");

        Assert.DoesNotContain("secret-value", redacted);
        Assert.DoesNotContain("Alice", redacted);
        Assert.Contains("https://example.test/save?[参数已隐藏]", redacted);
        Assert.Contains("C:\\Users\\[用户]\\GameSaveCenter", redacted);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
    }

    private MaintenanceReportService CreateReportService()
    {
        var storage = new StorageAnalysisService(options, store, NullLogger<StorageAnalysisService>.Instance);
        var mirror = new LocalMirrorService(options, NullLogger<LocalMirrorService>.Instance);
        var integrity = new IntegrityCheckService(options, store, NullLogger<IntegrityCheckService>.Instance);
        var retention = new RetentionSimulationService(options, store, NullLogger<RetentionSimulationService>.Instance);
        return new MaintenanceReportService(
            options,
            store,
            storage,
            mirror,
            integrity,
            retention,
            NullLogger<MaintenanceReportService>.Instance);
    }
}
