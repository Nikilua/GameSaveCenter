using System.Diagnostics;
using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class TaskQueryPersistenceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly WorkerOptions options;
    private readonly SqliteStateStore store;

    public TaskQueryPersistenceTests()
    {
        options = new WorkerOptions
        {
            DataDirectory = Path.Combine(root, "Data"),
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            MediaArchiveDirectory = Path.Combine(root, "Media")
        };
        Directory.CreateDirectory(options.DataDirectory);
        Directory.CreateDirectory(options.LudusaviBackupDirectory);
        Directory.CreateDirectory(options.MediaArchiveDirectory);
        store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task PageUsesStableCursorWhenTasksShareCreationTime()
    {
        var created = DateTime.UtcNow.AddMinutes(-5);
        for (var i = 0; i < 5; i++)
        {
            await AddTaskAsync($"same-time-{i:00}", TaskState.Succeeded, created, created.AddSeconds(1));
        }

        var first = await store.GetTaskPageAsync(new TaskQueryDto { Limit = 2 }, CancellationToken.None);
        var second = await store.GetTaskPageAsync(new TaskQueryDto { Limit = 2, Cursor = first.NextCursor }, CancellationToken.None);

        Assert.Equal(5, first.TotalCount);
        Assert.Equal(5, first.Summary.SucceededCount);
        Assert.Equal(new[] { "same-time-04", "same-time-03" }, first.Items.Select(x => x.TaskId));
        Assert.True(first.HasMore);
        Assert.Equal(new[] { "same-time-02", "same-time-01" }, second.Items.Select(x => x.TaskId));
        Assert.True(second.HasMore);
        Assert.DoesNotContain(second.Items, x => first.Items.Any(y => y.TaskId == x.TaskId));
    }

    [Fact]
    public async Task ActiveTaskQueryIsIndependentOfRecentHistoryWindow()
    {
        var created = DateTime.UtcNow.AddHours(-3);
        for (var i = 0; i < 225; i++)
            await AddTaskAsync("active-" + i.ToString("000"), TaskState.Running, created.AddSeconds(i), null);

        var active = await store.GetActiveTasksAsync(CancellationToken.None);

        Assert.Equal(225, active.Count);
        Assert.Equal("active-224", active[0].TaskId);
        Assert.Equal("active-000", active[^1].TaskId);
        Assert.All(active, task => Assert.Equal(TaskState.Running, task.State));
    }

    [Fact]
    public async Task SummaryCountsWaitingCloudTasksAsPending()
    {
        await AddTaskAsync("waiting-cloud", TaskState.WaitingForUser, DateTime.UtcNow, null);

        var summary = await store.GetTaskSummaryAsync(new TaskQueryDto(), CancellationToken.None);

        Assert.Equal(1, summary.WaitingForUserCount);
        Assert.Equal(1, summary.PendingCloudCount);
    }

    [Fact]
    public async Task MonotonicTaskDurationRoundTripsAndIsCapturedDuringRestartRecovery()
    {
        var startedTimestamp = Stopwatch.GetTimestamp() - Stopwatch.Frequency;
        await store.AddOrUpdateTaskAsync(new TaskStatusDto
        {
            TaskId = "monotonic-recovery",
            WorkerSessionId = "old-worker",
            TaskType = "Backup",
            GameId = "game-1",
            GameName = "测试游戏",
            State = TaskState.Running,
            ProgressPercent = 20,
            Message = "正在执行",
            CreatedUtc = DateTime.UtcNow.AddMinutes(-2),
            StartedUtc = DateTime.UtcNow.AddMinutes(-1),
            ElapsedSeconds = 2.5,
            MonotonicStartedTimestamp = startedTimestamp,
            MonotonicFrequency = Stopwatch.Frequency
        }, CancellationToken.None);

        var beforeRecovery = Assert.Single(await store.GetActiveTasksAsync(CancellationToken.None));
        Assert.Equal(2.5, beforeRecovery.ElapsedSeconds);
        Assert.Equal(startedTimestamp, beforeRecovery.MonotonicStartedTimestamp);

        Assert.Equal(1, await store.MarkInterruptedTasksAsync("new-worker", CancellationToken.None));

        var recovered = Assert.Single(await store.GetRecentTasksAsync(10, CancellationToken.None));
        Assert.Equal(TaskState.Failed, recovered.State);
        Assert.True(recovered.ElapsedSeconds >= 2.5);
        Assert.Equal(0, recovered.MonotonicStartedTimestamp);
        Assert.Equal(0, recovered.MonotonicFrequency);
        Assert.Equal("old-worker", recovered.WorkerSessionId);
    }

    [Fact]
    public async Task SummaryAndPageApplyIndependentFiltersAndDateHalfOpenRange()
    {
        var start = DateTime.UtcNow.AddHours(-2);
        var end = DateTime.UtcNow.AddHours(2);
        await AddTaskAsync("outside", TaskState.Succeeded, start.AddMinutes(-1), start.AddMinutes(-1));
        await AddTaskAsync("inside-success", TaskState.Succeeded, start, start.AddMinutes(1));
        await AddTaskAsync("inside-running", TaskState.Running, start.AddMinutes(2), null);
        await AddTaskAsync("inside-failed", TaskState.Failed, start.AddMinutes(3), start.AddMinutes(4));

        var query = new TaskQueryDto { Limit = 1, StartUtc = start, EndUtc = end };
        var page = await store.GetTaskPageAsync(query, CancellationToken.None);
        var finishedSuccesses = await store.GetSucceededTaskCountAsync(start, end, CancellationToken.None);

        Assert.Equal(3, page.TotalCount);
        Assert.Single(page.Items);
        Assert.Equal(1, page.Summary.SucceededCount);
        Assert.Equal(1, page.Summary.RunningCount);
        Assert.Equal(1, page.Summary.FailedCount);
        Assert.Equal(1, finishedSuccesses);
    }

    [Fact]
    public async Task RestoreReportRoundTripsThroughRecentAndPagedTaskQueries()
    {
        await store.AddOrUpdateTaskAsync(new TaskStatusDto
        {
            TaskId = "restore-report",
            TaskType = "Restore",
            GameId = "game-1",
            GameName = "测试游戏",
            State = TaskState.Running,
            ProgressPercent = 60,
            Message = "正在安全收尾",
            StageMessage = "正在执行恢复后校验",
            CancellationState = TaskCancellationStates.Finalizing,
            CreatedUtc = DateTime.UtcNow,
            RestoreReport = new RestoreReportDto
            {
                PlayniteId = "game-1",
                GameName = "测试游戏",
                BackupId = "backup-b",
                FileCount = 4,
                TotalBytes = 4096,
                PreRestoreBackupId = "pre-1",
                PreRestoreCreated = true,
                Stage = "回滚",
                OutcomeKind = "RolledBack",
                FailureCode = "RESTORE_FAILED_ROLLED_BACK",
                WasRolledBack = true,
                TaskId = "restore-report"
            },
            SourceReferences = new List<TaskSourceReferenceDto>
            {
                new TaskSourceReferenceDto
                {
                    Kind = TaskSourceReferenceKind.BackupVersion,
                    StableId = "backup-b",
                    PlayniteId = "game-1",
                    DisplayName = "backup-b",
                    Detail = "合成测试版本"
                },
                new TaskSourceReferenceDto
                {
                    Kind = TaskSourceReferenceKind.CloudTransfer,
                    StableId = "Backup:game-1",
                    PlayniteId = "game-1",
                    DisplayName = "备份云队列",
                    Detail = "合成测试队列"
                }
            }
        }, CancellationToken.None);

        var recent = Assert.Single(await store.GetRecentTasksAsync(10, CancellationToken.None));
        var page = Assert.Single((await store.GetTaskPageAsync(new TaskQueryDto { Limit = 10 }, CancellationToken.None)).Items);

        Assert.Equal("backup-b", recent.RestoreReport?.BackupId);
        Assert.Equal("pre-1", page.RestoreReport?.PreRestoreBackupId);
        Assert.Equal("RolledBack", page.RestoreReport?.OutcomeKind);
        Assert.Equal("正在执行恢复后校验", recent.StageMessage);
        Assert.Equal("校验中", page.StageDisplay);
        Assert.Equal(TaskCancellationStates.Finalizing, recent.CancellationState);
        Assert.Equal("无法立即中断 · 正在安全收尾", page.CancellationDisplay);
        Assert.Contains(recent.SourceReferences, reference => reference.Kind == TaskSourceReferenceKind.BackupVersion
            && reference.StableId == "backup-b");
        Assert.Contains(page.SourceReferences, reference => reference.Kind == TaskSourceReferenceKind.CloudTransfer
            && reference.StableId == "Backup:game-1");
    }

    [Fact]
    public async Task ReliableProgressMetricsRoundTripAndUnknownWorkDoesNotExposeEta()
    {
        var sampled = DateTime.UtcNow.AddSeconds(-1);
        await store.AddOrUpdateTaskAsync(new TaskStatusDto
        {
            TaskId = "sampled-progress",
            TaskType = "TrainerDownload",
            GameId = "game-1",
            GameName = "测试游戏",
            State = TaskState.Running,
            ProgressPercent = 42,
            Message = "正在下载",
            CreatedUtc = sampled.AddMinutes(-1),
            StartedUtc = sampled.AddMinutes(-1),
            ProgressCompletedUnits = 420,
            ProgressTotalUnits = 1000,
            ProgressUnit = "字节",
            ProgressRatePerSecond = 21,
            ProgressEtaSeconds = 28,
            ProgressUpdatedUtc = sampled
        }, CancellationToken.None);

        var recent = Assert.Single(await store.GetRecentTasksAsync(10, CancellationToken.None));
        var page = Assert.Single((await store.GetTaskPageAsync(new TaskQueryDto { Limit = 10 }, CancellationToken.None)).Items);

        Assert.Equal(420, recent.ProgressCompletedUnits);
        Assert.Equal(1000, page.ProgressTotalUnits);
        Assert.Equal("字节", recent.ProgressUnit);
        Assert.Equal(21, page.ProgressRatePerSecond);
        Assert.Equal(28, recent.ProgressEtaSeconds);
        Assert.Equal("21 B/秒", recent.ProgressRateDisplay);
        Assert.Equal("28 秒", recent.ProgressEtaDisplay);

        var unknown = new TaskStatusDto
        {
            State = TaskState.Running,
            ProgressCompletedUnits = -1,
            ProgressTotalUnits = -1,
            ProgressRatePerSecond = 99,
            ProgressEtaSeconds = 1
        };
        var waiting = new TaskStatusDto
        {
            State = TaskState.WaitingForUser,
            ProgressCompletedUnits = 5,
            ProgressTotalUnits = 10,
            ProgressUnit = "文件",
            ProgressRatePerSecond = 1,
            ProgressEtaSeconds = 5
        };
        var stale = new TaskStatusDto
        {
            State = TaskState.Running,
            ProgressCompletedUnits = 5,
            ProgressTotalUnits = 10,
            ProgressUnit = "文件",
            ProgressRatePerSecond = 1,
            ProgressEtaSeconds = 5,
            ProgressUpdatedUtc = DateTime.UtcNow.AddSeconds(-11)
        };
        Assert.False(unknown.HasReliableProgressMetrics);
        Assert.Equal("—", unknown.ProgressRateDisplay);
        Assert.Equal("—", unknown.ProgressEtaDisplay);
        Assert.False(waiting.HasReliableProgressMetrics);
        Assert.Equal("—", waiting.ProgressEtaDisplay);
        Assert.False(stale.HasReliableProgressMetrics);
        Assert.Equal("—", stale.ProgressRateDisplay);
        Assert.Equal("—", stale.ProgressEtaDisplay);
    }

    [Fact]
    public async Task SearchFindsMatchingTaskOutsideTheDefaultRecentWindow()
    {
        var old = DateTime.UtcNow.AddDays(-30);
        for (var i = 0; i < 210; i++)
        {
            await AddTaskAsync("history-" + i.ToString("000"), TaskState.Failed, old.AddSeconds(i), old.AddSeconds(i + 1),
                i == 17 ? "needle in the old error" : string.Empty);
        }

        var page = await store.GetTaskPageAsync(new TaskQueryDto { Limit = 10, Search = "needle" }, CancellationToken.None);

        var match = Assert.Single(page.Items);
        Assert.Equal("history-017", match.TaskId);
        Assert.Equal(1, page.TotalCount);
        Assert.Equal(1, page.Summary.FailedCount);
    }

    [Fact]
    public async Task FinishedTaskCountUsesTheFinishedStateIndex()
    {
        for (var i = 0; i < 1000; i++)
        {
            await AddTaskAsync("indexed-" + i.ToString("0000"), TaskState.Succeeded,
                DateTime.UtcNow.AddDays(-i), DateTime.UtcNow.AddDays(-i));
        }

        await using var connection = new SqliteConnection($"Data Source={options.DatabasePath};Mode=ReadOnly");
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = @"
EXPLAIN QUERY PLAN
SELECT COUNT(*) FROM tasks
WHERE state=$state AND finished_utc IS NOT NULL
  AND finished_utc >= $start AND finished_utc < $end;";
        command.Parameters.AddWithValue("$state", (int)TaskState.Succeeded);
        command.Parameters.AddWithValue("$start", DateTime.UtcNow.AddDays(-7).ToString("O"));
        command.Parameters.AddWithValue("$end", DateTime.UtcNow.ToString("O"));

        var plan = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) plan.Add(reader.GetString(3));

        Assert.Contains(plan, detail => detail.Contains("ix_tasks_finished_state", StringComparison.Ordinal));
    }

    private async Task AddTaskAsync(string taskId, TaskState state, DateTime createdUtc, DateTime? finishedUtc, string message = "")
    {
        await store.AddOrUpdateTaskAsync(new TaskStatusDto
        {
            TaskId = taskId,
            TaskType = state == TaskState.Failed ? "Backup" : "CloudUpload",
            GameId = "game-1",
            GameName = "测试游戏",
            State = state,
            ProgressPercent = state == TaskState.Succeeded ? 100 : 20,
            Message = message,
            CreatedUtc = createdUtc,
            StartedUtc = createdUtc,
            FinishedUtc = finishedUtc,
            ErrorMessage = message
        }, CancellationToken.None);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
    }
}
