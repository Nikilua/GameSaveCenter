using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class DashboardHealthPersistenceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly SqliteStateStore store;

    public DashboardHealthPersistenceTests()
    {
        var options = new WorkerOptions
        {
            DataDirectory = root,
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            MediaArchiveDirectory = Path.Combine(root, "Media")
        };
        store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task DashboardAggregateLoadsLatestBackupTaskReadinessAndFindings()
    {
        var now = DateTime.UtcNow;
        await store.UpsertGamesAsync(new[]
        {
            new GameDescriptorDto
            {
                PlayniteId = "health-game",
                Name = "Health Game",
                Platform = GamePlatformKind.Steam,
                LastPlayedUtc = now.AddDays(-1)
            }
        }, CancellationToken.None);
        await store.SetGameMatchAsync("health-game", "Health Game", 0.99, "health-input", CancellationToken.None);
        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            BackupId = "health-backup.zip",
            PlayniteId = "health-game",
            LudusaviName = "Health Game",
            CreatedUtc = now.AddDays(-1),
            TotalBytes = 12,
            FileCount = 1,
            RestoreReadiness = new RestoreReadinessDto
            {
                Status = RestoreReadinessStatus.Ready,
                BackupVersionId = "health-backup.zip",
                CheckedUtc = now.AddHours(-1),
                ActualFileCount = 1,
                ExpectedFileCount = 1
            }
        }, "[]", CancellationToken.None);
        await store.AddOrUpdateTaskAsync(new TaskStatusDto
        {
            TaskId = "health-task",
            TaskType = "Backup",
            GameId = "health-game",
            GameName = "Health Game",
            State = TaskState.Failed,
            CreatedUtc = now.AddHours(-2),
            ErrorCode = "BACKUP_FAILED",
            ErrorMessage = "test failure"
        }, CancellationToken.None);
        await store.AddFindingAsync("health-game", new ValidationFindingDto
        {
            Severity = FindingSeverity.Warning,
            Code = "TEST_WARNING",
            Title = "测试警告",
            Detail = "test detail"
        }, CancellationToken.None);

        var records = await store.GetDashboardGameRecordsAsync(CancellationToken.None);
        var record = Assert.Single(records);

        Assert.Equal(1, record.BackupVersionCount);
        Assert.Equal(RestoreReadinessStatus.Ready, record.LatestRestoreReadiness?.Status);
        Assert.Equal(1, record.RecentBackupFailureCount);
        Assert.Equal(TaskState.Failed, record.LastBackupTaskState);
        Assert.Equal(1, record.OpenFindingWarningCount);
        Assert.Equal(0, record.OpenFindingErrorCount);
        Assert.Equal("测试警告", record.LatestFindingTitle);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }
}
