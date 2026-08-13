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

        var storage = new StorageAnalysisService(options, store, NullLogger<StorageAnalysisService>.Instance);
        var mirror = new LocalMirrorService(options, NullLogger<LocalMirrorService>.Instance);
        var integrity = new IntegrityCheckService(options, store, NullLogger<IntegrityCheckService>.Instance);
        var service = new MaintenanceReportService(
            options,
            store,
            storage,
            mirror,
            integrity,
            NullLogger<MaintenanceReportService>.Instance);

        var report = await service.GetAsync(CancellationToken.None);

        Assert.Contains("GameSaveCenter 健康报告", report.ReportText);
        Assert.Contains("数据库", report.ReportText);
        Assert.Contains("备份仓库", report.ReportText);
        Assert.Contains("恢复点", report.ReportText);
        Assert.Contains("本地镜像", report.ReportText);
        Assert.Contains("Ready", report.ReportText);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
    }
}
