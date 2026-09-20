using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class R17FindingPersistenceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly SqliteStateStore store;

    public R17FindingPersistenceTests()
    {
        store = new SqliteStateStore(new WorkerOptions
        {
            DataDirectory = root,
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            MediaArchiveDirectory = Path.Combine(root, "Media")
        }, NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task OpenFindingCarriesRecordedEvidenceTime()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        await store.AddFindingAsync("game-1", new ValidationFindingDto
        {
            Code = "R17_TEST",
            Title = "测试证据",
            Severity = FindingSeverity.Warning
        }, CancellationToken.None);

        var finding = Assert.Single(await store.GetOpenFindingsAsync(20, CancellationToken.None));
        Assert.InRange(finding.CreatedUtc, before, DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public async Task ResolvedHealthFindingLeavesTheOpenQueue()
    {
        await store.UpsertHealthInspectionFindingAsync("game-1", "backup-1", new RestoreReadinessDto
        {
            Status = RestoreReadinessStatus.Corrupted,
            Summary = "测试损坏"
        }, CancellationToken.None);
        var finding = Assert.Single(await store.GetOpenFindingsAsync(20, CancellationToken.None));
        Assert.Equal("backup-1", finding.BackupId);

        await store.ResolveHealthInspectionFindingAsync("game-1", "backup-1", CancellationToken.None);

        Assert.Empty(await store.GetOpenFindingsAsync(20, CancellationToken.None));
    }

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }
}
