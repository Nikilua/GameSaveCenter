using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Services;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class NotificationRecordPersistenceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly WorkerOptions options;
    private readonly SqliteStateStore store;

    public NotificationRecordPersistenceTests()
    {
        options = new WorkerOptions
        {
            DataDirectory = root,
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            MediaArchiveDirectory = Path.Combine(root, "Media")
        };
        store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task ReplayUsesStableDeduplicationKeyAndRecordSurvivesWorkerRestart()
    {
        var notification = CreateRecord("event:task-1:completed", "notification-1");
        notification.TaskId = "task-1";
        notification.GameId = "game-1";
        notification.Severity = NotificationSeverity.Success;
        notification.Title = "备份完成";
        notification.Summary = "游戏存档备份已完成";

        Assert.True(await store.TryAddNotificationAsync(notification, CancellationToken.None));

        var replay = CreateRecord("event:task-1:completed", "notification-replay");
        replay.Summary = "重复事件不得覆盖首次快照";
        Assert.False(await store.TryAddNotificationAsync(replay, CancellationToken.None));

        var persisted = Assert.Single(await store.GetRecentNotificationRecordsAsync(10, CancellationToken.None));
        Assert.Equal("notification-1", persisted.NotificationId);
        Assert.Equal("task-1", persisted.TaskId);
        Assert.Equal("game-1", persisted.GameId);
        Assert.Equal(NotificationSeverity.Success, persisted.Severity);
        Assert.Equal("备份完成", persisted.Title);
        Assert.Equal("游戏存档备份已完成", persisted.Summary);
        Assert.False(persisted.IsRead);

        var restartedStore = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        await restartedStore.InitializeAsync(CancellationToken.None);
        Assert.Equal("notification-1", Assert.Single(
            await restartedStore.GetRecentNotificationRecordsAsync(10, CancellationToken.None)).NotificationId);
    }

    [Fact]
    public async Task RetentionAndQuantityCapsAreAppliedAndExpiredRecordsAreRejected()
    {
        var expired = CreateRecord("expired:event", "expired-record");
        expired.CreatedUtc = DateTime.UtcNow.AddDays(-(NotificationRecordPolicy.RetentionDays + 1));
        Assert.False(await store.TryAddNotificationAsync(expired, CancellationToken.None));

        var firstCreatedUtc = DateTime.UtcNow;
        for (var index = 0; index <= NotificationRecordPolicy.MaximumRecords; index++)
        {
            var notification = CreateRecord($"event:{index}", $"notification-{index:D3}");
            notification.CreatedUtc = firstCreatedUtc.AddTicks(index);
            Assert.True(await store.TryAddNotificationAsync(notification, CancellationToken.None));
        }

        var retained = await store.GetRecentNotificationRecordsAsync(
            NotificationRecordPolicy.MaximumRecords + 20,
            CancellationToken.None);
        Assert.Equal(NotificationRecordPolicy.MaximumRecords, retained.Count);
        Assert.DoesNotContain(retained, item => item.NotificationId == "notification-000");
        Assert.Contains(retained, item => item.NotificationId == $"notification-{NotificationRecordPolicy.MaximumRecords:D3}");
    }

    [Fact]
    public void RecordNormalizationBoundsTextAndDoesNotEscalateUnknownSeverity()
    {
        var normalized = NotificationRecordPolicy.Normalize(new NotificationRecordDto
        {
            NotificationId = "notification-1",
            DeduplicationKey = "  stable  event\r\nkey ",
            TaskId = " task-1 ",
            GameId = " ",
            Severity = (NotificationSeverity)99,
            Title = "  title\r\nwith   spacing  ",
            Summary = new string('x', NotificationRecordPolicy.MaximumSummaryLength + 50)
        });

        Assert.NotNull(normalized);
        Assert.Equal("stable event key", normalized!.DeduplicationKey);
        Assert.Equal("task-1", normalized.TaskId);
        Assert.Equal(string.Empty, normalized.GameId);
        Assert.Equal(NotificationSeverity.Information, normalized.Severity);
        Assert.Equal("title with spacing", normalized.Title);
        Assert.Equal(NotificationRecordPolicy.MaximumSummaryLength, normalized.Summary.Length);
        Assert.Null(NotificationRecordPolicy.Normalize(new NotificationRecordDto { DeduplicationKey = new string('x', 129) }));
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }

    private static NotificationRecordDto CreateRecord(string deduplicationKey, string id)
        => new()
        {
            NotificationId = id,
            DeduplicationKey = deduplicationKey,
            Title = "任务通知",
            Summary = "测试摘要",
            CreatedUtc = DateTime.UtcNow
        };
}
