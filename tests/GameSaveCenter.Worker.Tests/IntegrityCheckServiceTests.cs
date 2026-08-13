using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class IntegrityCheckServiceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly WorkerOptions options;
    private readonly SqliteStateStore store;
    private readonly IntegrityCheckService service;

    public IntegrityCheckServiceTests()
    {
        options = new WorkerOptions
        {
            DataDirectory = Path.Combine(root, "Data"),
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            MediaArchiveDirectory = Path.Combine(root, "Media")
        };
        store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
        service = new IntegrityCheckService(options, store, NullLogger<IntegrityCheckService>.Instance);
    }

    [Fact]
    public async Task HealthyDatabaseAndDirectoriesReportHealthy()
    {
        var result = await service.RunAsync(CancellationToken.None);

        Assert.Equal("Healthy", result.State);
        Assert.Equal(0, result.ErrorCount);
        Assert.Equal(0, result.WarningCount);
        Assert.Empty(result.Findings);
    }

    [Fact]
    public async Task MissingBackupArchiveProducesWarning()
    {
        var missing = Path.Combine(root, "missing.zip");
        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            PlayniteId = "game",
            BackupId = "missing",
            LudusaviName = "game",
            CreatedUtc = DateTime.UtcNow,
            ArchivePath = missing
        }, "{}", CancellationToken.None);

        var result = await service.RunAsync(CancellationToken.None);

        Assert.Equal("Warning", result.State);
        Assert.Equal(0, result.ErrorCount);
        Assert.Contains(result.Findings, x => x.Code == "BACKUP_ARCHIVE_MISSING");
    }

    [Fact]
    public async Task MissingTableProducesCriticalFinding()
    {
        await using (var connection = new SqliteConnection($"Data Source={options.DatabasePath};Mode=ReadWriteCreate;Cache=Shared;Foreign Keys=True"))
        {
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "DROP TABLE cloud_retry_queue;";
            await command.ExecuteNonQueryAsync();
        }

        var result = await service.RunAsync(CancellationToken.None);

        Assert.Equal("Critical", result.State);
        Assert.Contains(result.Findings, x => x.Code == "DATABASE_TABLE_MISSING");
        Assert.Contains(result.Findings, x => x.Detail.Contains("cloud_retry_queue", StringComparison.Ordinal));
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
    }
}
