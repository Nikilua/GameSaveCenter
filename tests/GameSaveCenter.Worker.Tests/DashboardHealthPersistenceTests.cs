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
            ParentBackupId = "older-health-backup",
            RestoreReadiness = new RestoreReadinessDto
            {
                Status = RestoreReadinessStatus.Ready,
                BackupVersionId = "health-backup.zip",
                CheckedUtc = now.AddHours(-1),
                ActualFileCount = 1,
                ExpectedFileCount = 1
            }
        }, "[]", CancellationToken.None);
        var storedVersion = Assert.Single(await store.GetBackupVersionsAsync("health-game", CancellationToken.None));
        Assert.Equal("older-health-backup", storedVersion.ParentBackupId);
        await store.AddOrUpdateTaskAsync(new TaskStatusDto
        {
            TaskId = "health-task",
            SessionId = "health-session",
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
        Assert.Equal("health-session", Assert.Single(await store.GetRecentTasksAsync(10, CancellationToken.None)).SessionId);
    }

    [Fact]
    public async Task DashboardAggregateCombinesSaveAndAssignedMediaCloudState()
    {
        await store.UpsertGamesAsync(new[]
        {
            new GameDescriptorDto
            {
                PlayniteId = "cloud-failed-game",
                Name = "Cloud Failed Game",
                Platform = GamePlatformKind.Steam
            },
            new GameDescriptorDto
            {
                PlayniteId = "cloud-synced-media-game",
                Name = "Cloud Synced Media Game",
                Platform = GamePlatformKind.Epic
            }
        }, CancellationToken.None);
        await store.UpdateGameCloudStateAsync("cloud-failed-game", "Uploaded", CancellationToken.None);
        await store.AddMediaAsync(new MediaItemDto
        {
            MediaId = "cloud-failed-media",
            PlayniteId = "cloud-failed-game",
            Kind = MediaKind.Screenshot,
            Source = MediaSourceKind.Steam,
            ArchivePath = "archive/cloud-failed-media.png",
            OriginalPath = "C:/Screens/cloud-failed-media.png",
            CapturedUtc = DateTime.UtcNow,
            SizeBytes = 12,
            Sha256 = "cloud-failed-media-hash",
            CloudState = "Failed",
            ClassificationState = "Assigned"
        }, CancellationToken.None);
        await store.AddMediaAsync(new MediaItemDto
        {
            MediaId = "cloud-synced-media",
            PlayniteId = "cloud-synced-media-game",
            Kind = MediaKind.Screenshot,
            Source = MediaSourceKind.Epic,
            ArchivePath = "archive/cloud-synced-media.png",
            OriginalPath = "C:/Screens/cloud-synced-media.png",
            CapturedUtc = DateTime.UtcNow,
            SizeBytes = 12,
            Sha256 = "cloud-synced-media-hash",
            CloudState = "Synced",
            ClassificationState = "Assigned"
        }, CancellationToken.None);

        var records = await store.GetDashboardGameRecordsAsync(CancellationToken.None);

        Assert.Equal("Failed", Assert.Single(records, x => x.Descriptor.PlayniteId == "cloud-failed-game").CloudState);
        Assert.Equal("Uploaded", Assert.Single(records, x => x.Descriptor.PlayniteId == "cloud-synced-media-game").CloudState);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }
}
