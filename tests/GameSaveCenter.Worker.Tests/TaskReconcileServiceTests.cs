using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Cryptography;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class TaskReconcileServiceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly SqliteStateStore store;
    private readonly WorkerOptions options;

    public TaskReconcileServiceTests()
    {
        options = new WorkerOptions
        {
            DataDirectory = Path.Combine(root, "Data"),
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            MediaArchiveDirectory = Path.Combine(root, "Media")
        };
        store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task ReconcileMarksInterruptedTasksAndIsIdempotent()
    {
        var now = DateTime.UtcNow;
        await store.AddOrUpdateTaskAsync(new TaskStatusDto
        {
            TaskId = "queued",
            TaskType = "Backup",
            WorkerSessionId = "old-session",
            State = TaskState.Queued,
            CreatedUtc = now
        }, CancellationToken.None);
        await store.AddOrUpdateTaskAsync(new TaskStatusDto
        {
            TaskId = "running",
            TaskType = "CloudUpload",
            WorkerSessionId = "old-session",
            State = TaskState.Running,
            CreatedUtc = now,
            StartedUtc = now
        }, CancellationToken.None);
        await store.AddOrUpdateTaskAsync(new TaskStatusDto
        {
            TaskId = "restore",
            TaskType = "Restore",
            WorkerSessionId = "old-session",
            State = TaskState.Running,
            CreatedUtc = now,
            StartedUtc = now
        }, CancellationToken.None);
        await store.AddOrUpdateTaskAsync(new TaskStatusDto
        {
            TaskId = "integrity",
            TaskType = "Integrity",
            WorkerSessionId = "old-session",
            State = TaskState.Running,
            CreatedUtc = now,
            StartedUtc = now
        }, CancellationToken.None);
        await store.AddOrUpdateTaskAsync(new TaskStatusDto
        {
            TaskId = "done",
            TaskType = "Backup",
            State = TaskState.Succeeded,
            CreatedUtc = now,
            FinishedUtc = now
        }, CancellationToken.None);

        var service = new TaskReconcileService(store, options, NullLogger<TaskReconcileService>.Instance);
        var first = await service.ReconcileAsync(CancellationToken.None);
        var second = await service.ReconcileAsync(CancellationToken.None);

        Assert.Equal(4, first.InterruptedTaskCount);
        Assert.Equal(0, second.InterruptedTaskCount);
        var tasks = await store.GetRecentTasksAsync(10, CancellationToken.None);
        Assert.Equal(TaskState.Failed, tasks.Single(x => x.TaskId == "queued").State);
        Assert.Equal("WORKER_RESTARTED_RETRYABLE", tasks.Single(x => x.TaskId == "queued").ErrorCode);
        Assert.Equal(TaskState.Failed, tasks.Single(x => x.TaskId == "running").State);
        Assert.Equal("WORKER_RESTARTED_RETRYABLE", tasks.Single(x => x.TaskId == "running").ErrorCode);
        Assert.Equal("MANUAL_INTERVENTION_REQUIRED", tasks.Single(x => x.TaskId == "restore").ErrorCode);
        Assert.Equal("WORKER_RESTARTED", tasks.Single(x => x.TaskId == "integrity").ErrorCode);
        Assert.Equal(TaskState.Succeeded, tasks.Single(x => x.TaskId == "done").State);
    }

    [Fact]
    public async Task ReconcileAfterRestartPreservesRestoreReportAndLockedPreRestoreSnapshot()
    {
        var now = DateTime.UtcNow;
        options.WorkerSessionId = "new-worker-session";

        var saveDirectory = Path.Combine(root, "SyntheticSaveTarget");
        Directory.CreateDirectory(saveDirectory);
        var saveFile = Path.Combine(saveDirectory, "profile.dat");
        var partiallyWrittenSave = new byte[] { 0x47, 0x53, 0x43, 0x01, 0xAA };
        await File.WriteAllBytesAsync(saveFile, partiallyWrittenSave);

        var preRestoreDirectory = Path.Combine(options.LudusaviBackupDirectory, "SyntheticGame");
        Directory.CreateDirectory(preRestoreDirectory);
        var preRestoreArchive = Path.Combine(preRestoreDirectory, "pre-restore.zip");
        var preRestoreBytes = new byte[] { 0x50, 0x52, 0x45, 0x01, 0x02, 0x03 };
        await File.WriteAllBytesAsync(preRestoreArchive, preRestoreBytes);
        var expectedSnapshotHash = Convert.ToHexString(SHA256.HashData(preRestoreBytes));

        await store.AddBackupVersionAsync(new BackupVersionDto
        {
            BackupId = "pre-restore-1",
            PlayniteId = "synthetic-game",
            LudusaviName = "SyntheticGame",
            CreatedUtc = now,
            TotalBytes = preRestoreBytes.Length,
            FileCount = 1,
            IsLocked = true,
            IsPreRestore = true,
            Comment = "PreRestore synthetic crash fixture",
            SourceDevice = "test-device",
            OperatingSystem = "Windows",
            ArchivePath = preRestoreArchive
        }, "{\"files\":[\"profile.dat\"]}", CancellationToken.None);

        var report = new RestoreReportDto
        {
            PlayniteId = "synthetic-game",
            GameName = "Synthetic Game",
            BackupId = "target-backup-1",
            FileCount = 1,
            TotalBytes = partiallyWrittenSave.Length,
            PreRestoreBackupId = "pre-restore-1",
            PreRestoreCreated = true,
            Stage = "写入目标",
            OutcomeKind = "Running",
            TaskId = "restore-crashed"
        };
        await store.AddOrUpdateTaskAsync(new TaskStatusDto
        {
            TaskId = "restore-crashed",
            TaskType = "Restore",
            GameId = "synthetic-game",
            GameName = "Synthetic Game",
            WorkerSessionId = "crashed-worker-session",
            State = TaskState.Running,
            ProgressPercent = 52,
            Message = "正在写入目标文件",
            StageMessage = "写入目标",
            CreatedUtc = now,
            StartedUtc = now,
            RestoreReport = report
        }, CancellationToken.None);
        await store.AppendAuditAsync(
            "Restore",
            "写入目标",
            "{\"taskId\":\"restore-crashed\",\"state\":\"writing-target\"}",
            CancellationToken.None);

        SqliteConnection.ClearAllPools();
        var restartedStore = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        await restartedStore.InitializeAsync(CancellationToken.None);
        var persistedTask = (await restartedStore.GetRecentTasksAsync(10, CancellationToken.None))
            .Single(x => x.TaskId == "restore-crashed");
        Assert.Equal("写入目标", persistedTask.RestoreReport?.Stage);
        Assert.Equal("pre-restore-1", persistedTask.RestoreReport?.PreRestoreBackupId);

        var service = new TaskReconcileService(restartedStore, options, NullLogger<TaskReconcileService>.Instance);
        var result = await service.ReconcileAsync(CancellationToken.None);

        Assert.Equal(1, result.InterruptedTaskCount);
        var reconciled = (await restartedStore.GetRecentTasksAsync(10, CancellationToken.None))
            .Single(x => x.TaskId == "restore-crashed");
        Assert.Equal(TaskState.Failed, reconciled.State);
        Assert.Equal("MANUAL_INTERVENTION_REQUIRED", reconciled.ErrorCode);
        Assert.Contains("PreRestore", reconciled.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("写入目标", reconciled.RestoreReport?.Stage);
        Assert.Equal("pre-restore-1", reconciled.RestoreReport?.PreRestoreBackupId);
        Assert.Equal("MANUAL_INTERVENTION_REQUIRED", reconciled.RestoreReport?.FailureCode);
        Assert.True(reconciled.RestoreReport?.PreRestoreCreated);
        Assert.Equal("ManualIntervention", reconciled.RestoreReport?.OutcomeKind);
        Assert.True(reconciled.RestoreReport?.RequiresManualIntervention);

        var indexedSnapshot = (await restartedStore.GetBackupVersionsAsync("synthetic-game", CancellationToken.None))
            .Single(x => x.BackupId == "pre-restore-1");
        Assert.True(indexedSnapshot.IsLocked);
        Assert.True(indexedSnapshot.IsPreRestore);
        Assert.Equal(preRestoreArchive, indexedSnapshot.ArchivePath);
        Assert.True(File.Exists(indexedSnapshot.ArchivePath));
        Assert.Equal(expectedSnapshotHash, Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(indexedSnapshot.ArchivePath))));

        Assert.Equal(partiallyWrittenSave, await File.ReadAllBytesAsync(saveFile));
        var audit = await restartedStore.GetAuditAsync(20, CancellationToken.None);
        Assert.Contains(audit, x => x.Category == "Restore" && x.Message == "写入目标");
        Assert.Contains(audit, x => x.Category == "TaskReconcile");
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try { if (Directory.Exists(root)) Directory.Delete(root, true); } catch { }
    }
}
