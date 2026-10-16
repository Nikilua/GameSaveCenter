using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Ipc;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class ManualCloudRetryUsesExistingBackupTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly WorkerOptions options;
    private readonly SqliteStateStore store;

    public ManualCloudRetryUsesExistingBackupTests()
    {
        options = new WorkerOptions
        {
            DataDirectory = root,
            LudusaviExecutable = Path.Combine(root, "missing-ludusavi.exe"),
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            RcloneExecutable = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "where.exe"),
            RcloneDestination = "synthetic-remote:",
            EnableCloudUpload = true
        };
        store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task RetryUploadsExistingLocalBackupWithoutRequiringAnotherLudusaviBackup()
    {
        Assert.True(File.Exists(options.RcloneExecutable));
        Directory.CreateDirectory(options.LudusaviBackupDirectory);
        var retainedBackup = Path.Combine(options.LudusaviBackupDirectory, "synthetic-existing-backup.zip");
        var originalBytes = new byte[] { 9, 2, 6, 5, 3, 5 };
        await File.WriteAllBytesAsync(retainedBackup, originalBytes);
        await store.UpsertGamesAsync(new[]
        {
            new GameDescriptorDto
            {
                PlayniteId = "retry-existing-backup",
                Name = "Retry Existing Backup",
                Platform = GamePlatformKind.Steam
            }
        }, CancellationToken.None);

        var runner = new ExternalProcessRunner(NullLogger<ExternalProcessRunner>.Instance);
        var rclone = new RcloneClient(options, runner);
        var cloudCoordinator = new CloudTransferCoordinator(NullLogger<CloudTransferCoordinator>.Instance);
        var cloudState = new CloudTransferStateService(
            store,
            options,
            rclone,
            cloudCoordinator,
            NullLogger<CloudTransferStateService>.Instance);
        var tasks = new TaskCoordinator(store, new TaskEventBroadcaster(), NullLogger<TaskCoordinator>.Instance, options);
        var catalog = new GameCatalogService(
            store,
            new LudusaviClient(options, runner, NullLogger<LudusaviClient>.Instance),
            NullLogger<GameCatalogService>.Instance);
        var orchestrator = new BackupOrchestrator(
            catalog,
            store,
            new LudusaviClient(options, runner, NullLogger<LudusaviClient>.Instance),
            rclone,
            cloudCoordinator,
            cloudState,
            tasks,
            options,
            new GameOperationLock(),
            NullLogger<BackupOrchestrator>.Instance);

        var result = await orchestrator.RetryCloudUploadAsync("retry-existing-backup", CancellationToken.None);

        Assert.Equal(TaskState.Failed, result.State);
        Assert.StartsWith("RCLONE_", result.ErrorCode, StringComparison.Ordinal);
        Assert.True(result.BackupResult?.LocalBackupSucceeded);
        Assert.Equal(originalBytes, await File.ReadAllBytesAsync(retainedBackup));
        Assert.False(File.Exists(options.LudusaviExecutable));
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(root)) Directory.Delete(root, true);
    }
}
