using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class DeviceConflictReadinessSummaryTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.DeviceConflictEvidence.Tests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task DeviceSummaryContainsExactVersionReadinessTimeWithoutManifestPaths()
    {
        Directory.CreateDirectory(root);
        var options = new WorkerOptions
        {
            DataDirectory = Path.Combine(root, "state"),
            LudusaviBackupDirectory = Path.Combine(root, "saves"),
            MediaArchiveDirectory = Path.Combine(root, "media")
        };
        var store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        await store.InitializeAsync(CancellationToken.None);
        await store.UpsertGamesAsync(new[] { new GameDescriptorDto { PlayniteId = "game-1", Name = "Demo Game" } }, CancellationToken.None);
        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            PlayniteId = "game-1",
            BackupId = "backup-1",
            CreatedUtc = new DateTime(2026, 10, 19, 7, 0, 0, DateTimeKind.Utc),
            ParentBackupId = "base-1",
            FileCount = 1,
            TotalBytes = 24
        }, "[{\"relativePath\":\"profile.sav\",\"sizeBytes\":24}]", CancellationToken.None);
        var checkedUtc = new DateTime(2026, 10, 18, 7, 0, 0, DateTimeKind.Utc);
        await store.SaveRestoreReadinessAsync("game-1", "backup-1", new RestoreReadinessDto
        {
            Status = RestoreReadinessStatus.Warning,
            CheckedUtc = checkedUtc,
            BackupVersionId = "backup-1",
            Summary = "警告摘要不进入设备状态侧车。"
        }, CancellationToken.None);

        var summary = Assert.Single(await store.GetLatestBackupSummariesAsync(CancellationToken.None));

        Assert.Equal("backup-1", summary.BackupId);
        Assert.Equal("base-1", summary.ParentBackupId);
        Assert.Equal("Warning", summary.RestoreReadinessStatus);
        Assert.Equal(checkedUtc, summary.RestoreReadinessCheckedUtc);
        Assert.DoesNotContain("profile.sav", System.Text.Json.JsonSerializer.Serialize(summary));
        Assert.DoesNotContain("警告摘要", System.Text.Json.JsonSerializer.Serialize(summary));
    }

    [Fact]
    public void NewSidecarSchemaDefaultsToFourAndOlderEvidenceIsOptional()
    {
        var current = new DeviceStateSidecarDto();
        var older = System.Text.Json.JsonSerializer.Deserialize<DeviceBackupSummaryDto>("{}");

        Assert.Equal(4, current.SchemaVersion);
        Assert.NotNull(older);
        Assert.Null(older!.RestoreReadinessCheckedUtc);
        Assert.Empty(older.RestoreReadinessStatus);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(root))
        {
            try { Directory.Delete(root, recursive: true); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
