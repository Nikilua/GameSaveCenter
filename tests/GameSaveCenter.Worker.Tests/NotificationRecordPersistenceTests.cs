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
    public async Task MarkReadChangesOnlyRequestedUnreadIdsAndPersistsAcrossRestart()
    {
        Assert.True(await store.TryAddNotificationAsync(CreateRecord("event:read-1", "notification-1"), CancellationToken.None));
        Assert.True(await store.TryAddNotificationAsync(CreateRecord("event:read-2", "notification-2"), CancellationToken.None));
        Assert.True(await store.TryAddNotificationAsync(CreateRecord("event:read-3", "notification-3"), CancellationToken.None));

        var updated = await store.MarkNotificationRecordsReadAsync(
            new[] { " notification-1 ", "", "notification-1", "missing", "notification-2" },
            CancellationToken.None);

        Assert.Equal(2, updated);
        Assert.Equal(0, await store.MarkNotificationRecordsReadAsync(new[] { "notification-1", "notification-2" }, CancellationToken.None));
        var restartedStore = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        await restartedStore.InitializeAsync(CancellationToken.None);
        var records = await restartedStore.GetRecentNotificationRecordsAsync(10, CancellationToken.None);
        Assert.True(Assert.Single(records, item => item.NotificationId == "notification-1").IsRead);
        Assert.True(Assert.Single(records, item => item.NotificationId == "notification-2").IsRead);
        Assert.False(Assert.Single(records, item => item.NotificationId == "notification-3").IsRead);
    }

    [Fact]
    public async Task ExplicitRetryLineageAggregatesDistinctAttemptsAndClosesOnRecovery()
    {
        var firstAt = DateTime.UtcNow.AddMinutes(-12);
        var secondAt = DateTime.UtcNow.AddMinutes(-4);
        var recoveredAt = DateTime.UtcNow;
        Assert.True(await store.TryAddNotificationAsync(
            CreateIssueRecord("issue:backup:game-1:root-1", "task-1", "attempt:task-1", firstAt, "DISK_FULL: 目标盘空间不足"),
            CancellationToken.None));

        Assert.False(await store.TryAddNotificationAsync(
            CreateIssueRecord("issue:backup:game-1:root-1", "task-1", "attempt:task-1", firstAt, "重放不应重复计数"),
            CancellationToken.None));
        Assert.True(await store.TryAddNotificationAsync(
            CreateIssueRecord("issue:backup:game-1:root-1", "task-2", "attempt:task-2", secondAt, "DISK_FULL: 重试后仍然空间不足"),
            CancellationToken.None));

        var restartedStore = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        await restartedStore.InitializeAsync(CancellationToken.None);
        var aggregate = Assert.Single(await restartedStore.GetRecentNotificationRecordsAsync(10, CancellationToken.None));
        Assert.Equal(2, aggregate.OccurrenceCount);
        Assert.Equal("task-1", aggregate.FirstTaskId);
        Assert.Equal("task-2", aggregate.TaskId);
        Assert.Equal("DISK_FULL: 重试后仍然空间不足", aggregate.LastError);
        Assert.Equal(firstAt, aggregate.FirstOccurredUtc, TimeSpan.FromMilliseconds(1));
        Assert.Equal(secondAt, aggregate.LastOccurredUtc, TimeSpan.FromMilliseconds(1));
        Assert.False(aggregate.IsResolved);

        var recovery = CreateIssueRecord(
            "issue:backup:game-1:root-1", "task-3", "recovery:task-3", recoveredAt, string.Empty,
            severity: NotificationSeverity.Success, isResolved: true);
        recovery.Title = "后台任务已恢复";
        recovery.Summary = "重试成功，关联故障已恢复。";
        Assert.True(await restartedStore.TryAddNotificationAsync(recovery, CancellationToken.None));
        Assert.False(await restartedStore.TryAddNotificationAsync(recovery, CancellationToken.None));

        aggregate = Assert.Single(await restartedStore.GetRecentNotificationRecordsAsync(10, CancellationToken.None));
        Assert.Equal(2, aggregate.OccurrenceCount);
        Assert.Equal("task-1", aggregate.FirstTaskId);
        Assert.Equal("task-3", aggregate.TaskId);
        Assert.Equal("DISK_FULL: 重试后仍然空间不足", aggregate.LastError);
        Assert.Equal(NotificationSeverity.Success, aggregate.Severity);
        Assert.True(aggregate.IsResolved);
        Assert.False(aggregate.IsRead);

        Assert.True(await restartedStore.TryAddNotificationAsync(
            CreateIssueRecord("issue:backup:game-1:root-1", "task-4", "attempt:task-4", recoveredAt.AddMinutes(1), "ACCESS_DENIED: 权限仍不足"),
            CancellationToken.None));
        aggregate = Assert.Single(await restartedStore.GetRecentNotificationRecordsAsync(10, CancellationToken.None));
        Assert.Equal(3, aggregate.OccurrenceCount);
        Assert.Equal("task-4", aggregate.TaskId);
        Assert.Equal("ACCESS_DENIED: 权限仍不足", aggregate.LastError);
        Assert.False(aggregate.IsResolved);
        Assert.Equal(NotificationSeverity.Error, aggregate.Severity);
    }

    [Fact]
    public async Task DifferentIssueRootsRemainSeparateEvenForSameGameAndError()
    {
        var created = DateTime.UtcNow;
        Assert.True(await store.TryAddNotificationAsync(
            CreateIssueRecord("issue:backup:game-1:root-1", "task-1", "attempt:task-1", created, "ACCESS_DENIED"),
            CancellationToken.None));
        Assert.True(await store.TryAddNotificationAsync(
            CreateIssueRecord("issue:backup:game-1:root-2", "task-2", "attempt:task-2", created.AddSeconds(1), "ACCESS_DENIED"),
            CancellationToken.None));

        var records = await store.GetRecentNotificationRecordsAsync(10, CancellationToken.None);
        Assert.Equal(2, records.Count);
        Assert.Equal(1, Assert.Single(records, item => item.FirstTaskId == "task-1").OccurrenceCount);
        Assert.Equal(1, Assert.Single(records, item => item.FirstTaskId == "task-2").OccurrenceCount);
    }

    [Fact]
    public async Task RetryIssueIdentitySurvivesTaskStoreRestart()
    {
        await store.AddOrUpdateTaskAsync(new TaskStatusDto
        {
            TaskId = "retry-task",
            NotificationIssueId = "root-task",
            TaskType = "Backup",
            GameId = "game-1",
            GameName = "测试游戏",
            State = TaskState.Failed,
            CreatedUtc = DateTime.UtcNow
        }, CancellationToken.None);

        var restartedStore = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        await restartedStore.InitializeAsync(CancellationToken.None);
        var task = Assert.Single(await restartedStore.GetRecentTasksAsync(10, CancellationToken.None));
        Assert.Equal("root-task", task.NotificationIssueId);
        var page = await restartedStore.GetTaskPageAsync(new TaskQueryDto { Limit = 10, ExactTaskId = "retry-task" }, CancellationToken.None);
        Assert.Equal("root-task", Assert.Single(page.Items).NotificationIssueId);
    }

    [Fact]
    public async Task ExistingStandaloneTaskFailureIsMigratedIntoItsRetryIssue()
    {
        var created = DateTime.UtcNow.AddMinutes(-5);
        await using (var connection = new SqliteConnection($"Data Source={options.DatabasePath};Mode=ReadWriteCreate;Cache=Shared;Foreign Keys=True"))
        {
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = @"
INSERT INTO tasks(task_id,task_type,game_id,game_name,state,progress,message,created_utc)
VALUES('legacy-task','Backup','game-1','测试游戏',$failed,100,'备份失败',$created);
INSERT INTO notification_records(notification_id,deduplication_key,task_id,game_id,severity,title,summary,created_utc,is_read)
VALUES('legacy-notice','legacy-event','legacy-task','game-1',$error,'后台任务失败','DISK_FULL: 空间不足',$created,0);";
            command.Parameters.AddWithValue("$failed", (int)TaskState.Failed);
            command.Parameters.AddWithValue("$error", (int)NotificationSeverity.Error);
            command.Parameters.AddWithValue("$created", created.ToString("O", System.Globalization.CultureInfo.InvariantCulture));
            await command.ExecuteNonQueryAsync();
        }

        var restartedStore = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        await restartedStore.InitializeAsync(CancellationToken.None);
        var migrated = Assert.Single(await restartedStore.GetRecentNotificationRecordsAsync(10, CancellationToken.None));
        var expectedIssueKey = NotificationRecordPolicy.CreateTaskIssueKey("Backup", "game-1", "legacy-task");
        Assert.Equal("legacy-notice", migrated.NotificationId);
        Assert.Equal(expectedIssueKey, migrated.IssueKey);
        Assert.Equal(1, migrated.OccurrenceCount);
        Assert.Equal("legacy-task", migrated.FirstTaskId);
        Assert.Equal("DISK_FULL: 空间不足", migrated.LastError);

        Assert.False(await restartedStore.TryAddNotificationAsync(new NotificationRecordDto
        {
            NotificationId = "replayed-legacy-task",
            DeduplicationKey = expectedIssueKey,
            IssueKey = expectedIssueKey,
            OccurrenceKey = NotificationRecordPolicy.CreateTaskOccurrenceKey("legacy-task", failure: true),
            TaskId = "legacy-task",
            FirstTaskId = "legacy-task",
            GameId = "game-1",
            Severity = NotificationSeverity.Error,
            Title = "后台任务失败",
            Summary = "重启后重复投递",
            LastError = "重启后重复投递"
        }, CancellationToken.None));
    }

    [Fact]
    public async Task ClearReadDeletesOnlyReadNotificationsAndPreservesTasksAuditAndRestoreEvidence()
    {
        var read = CreateRecord("event:read", "notification-read");
        read.IsRead = true;
        Assert.True(await store.TryAddNotificationAsync(read, CancellationToken.None));
        Assert.True(await store.TryAddNotificationAsync(CreateRecord("event:unread", "notification-unread"), CancellationToken.None));

        await InsertRelatedStateAsync();

        Assert.Equal(1, await store.ClearReadNotificationRecordsAsync(CancellationToken.None));
        var retained = Assert.Single(await store.GetRecentNotificationRecordsAsync(10, CancellationToken.None));
        Assert.Equal("notification-unread", retained.NotificationId);
        Assert.False(retained.IsRead);

        await using var connection = new SqliteConnection($"Data Source={options.DatabasePath};Mode=ReadWriteCreate;Cache=Shared;Foreign Keys=True");
        await connection.OpenAsync();
        Assert.Equal(1L, await ReadScalarAsync<long>(connection, "SELECT COUNT(*) FROM tasks WHERE task_id='task-preserved';"));
        Assert.Equal("restore-evidence", await ReadScalarAsync<string>(connection, "SELECT restore_report_json FROM tasks WHERE task_id='task-preserved';"));
        Assert.Equal(1L, await ReadScalarAsync<long>(connection, "SELECT COUNT(*) FROM audit_log WHERE audit_id='audit-preserved';"));
        Assert.Equal(1L, await ReadScalarAsync<long>(connection, "SELECT COUNT(*) FROM backup_versions WHERE backup_id='backup-preserved';"));
        Assert.Equal(0, await store.ClearReadNotificationRecordsAsync(CancellationToken.None));
    }

    [Theory]
    [InlineData(MessageTypes.MarkNotificationRecordsRead)]
    [InlineData(MessageTypes.ClearReadNotificationRecords)]
    public void NotificationReadMutationsRequireReplayProtection(string messageType)
        => Assert.True(IpcRequestSemantics.RequiresReplayProtection(messageType));

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

    private static NotificationRecordDto CreateIssueRecord(
        string issueKey,
        string taskId,
        string occurrenceKey,
        DateTime occurredUtc,
        string lastError,
        NotificationSeverity severity = NotificationSeverity.Error,
        bool isResolved = false)
        => new()
        {
            NotificationId = "notification-" + taskId,
            DeduplicationKey = issueKey,
            IssueKey = issueKey,
            OccurrenceKey = occurrenceKey,
            TaskId = taskId,
            FirstTaskId = taskId,
            GameId = "game-1",
            Severity = severity,
            Title = "后台任务失败",
            Summary = lastError,
            CreatedUtc = occurredUtc,
            FirstOccurredUtc = occurredUtc,
            LastOccurredUtc = occurredUtc,
            LastError = lastError,
            IsResolved = isResolved
        };

    private async Task InsertRelatedStateAsync()
    {
        await using var connection = new SqliteConnection($"Data Source={options.DatabasePath};Mode=ReadWriteCreate;Cache=Shared;Foreign Keys=True");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO tasks(task_id,task_type,state,progress,created_utc,restore_report_json)
VALUES('task-preserved','restore',0,0,$created,'restore-evidence');
INSERT INTO audit_log(audit_id,category,message,detail_json,created_utc)
VALUES('audit-preserved','restore','恢复记录保留','{}',$created);
INSERT INTO backup_versions(backup_id,playnite_id,ludusavi_name,created_utc,total_bytes,file_count)
VALUES('backup-preserved','game-preserved','game-preserved',$created,1,1);";
        command.Parameters.AddWithValue("$created", DateTime.UtcNow.ToString("O", System.Globalization.CultureInfo.InvariantCulture));
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<T> ReadScalarAsync<T>(SqliteConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        var result = await command.ExecuteScalarAsync();
        return (T)Convert.ChangeType(result!, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
    }
}
