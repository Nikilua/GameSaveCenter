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
