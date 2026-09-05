using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Persistence;
using GameSaveCenter.Worker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GameSaveCenter.Worker.Tests;

public sealed class CloudTransferStateTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "GameSaveCenter.Tests", Guid.NewGuid().ToString("N"));
    private readonly WorkerOptions options;
    private readonly SqliteStateStore store;

    public CloudTransferStateTests()
    {
        options = new WorkerOptions
        {
            DataDirectory = root,
            EnableCloudUpload = true,
            LudusaviBackupDirectory = Path.Combine(root, "Saves"),
            MediaArchiveDirectory = Path.Combine(root, "Media")
        };
        store = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        store.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task BackupAndMediaForTheSameGameRemainSeparateAndDeduplicated()
    {
        var coordinator = new CloudTransferCoordinator(NullLogger<CloudTransferCoordinator>.Instance);
        var state = CreateState(coordinator);
        var next = DateTime.UtcNow.AddMinutes(5);

        await state.StartNewAsync(CloudTransferKind.Backup, "game-1", CancellationToken.None);
        await state.RecordRetryScheduledAsync(CloudTransferKind.Backup, "game-1", 2, next, "RCLONE_NETWORK_FAILED", "断网", CancellationToken.None);
        await state.StartNewAsync(CloudTransferKind.Media, "game-1", CancellationToken.None);
        await state.MarkAuthenticationRequiredAsync(CloudTransferKind.Media, "game-1", "RCLONE_AUTH_FAILED", "凭据过期", CancellationToken.None);

        var summary = await state.GetStatusAsync(CancellationToken.None);
        Assert.Equal(2, summary.TotalCount);
        Assert.Equal(1, summary.RetryScheduledCount);
        Assert.Equal(1, summary.AuthenticationRequiredCount);
        Assert.Contains(summary.Items, x => x.Kind == CloudTransferKind.Backup && x.AttemptCount == 2 && x.State == "RetryScheduled");
        Assert.Contains(summary.Items, x => x.Kind == CloudTransferKind.Media && x.State == "AuthenticationRequired");
    }

    [Fact]
    public async Task AuthenticationFailureIsVisibleButNeverDueForAutomaticRetry()
    {
        var coordinator = new CloudTransferCoordinator(NullLogger<CloudTransferCoordinator>.Instance);
        var state = CreateState(coordinator);
        await state.StartNewAsync(CloudTransferKind.Media, "game-auth", CancellationToken.None);
        await state.ScheduleAutomaticRetryAsync(CloudTransferKind.Media, "game-auth", "RCLONE_AUTH_FAILED", "expired token", CancellationToken.None);

        Assert.Empty(await store.GetDueCloudTransfersAsync(CloudTransferKind.Media, DateTime.UtcNow.AddDays(1), 10, CancellationToken.None));
        var item = Assert.Single((await state.GetStatusAsync(CancellationToken.None)).Items);
        Assert.Equal("AuthenticationRequired", item.State);
        Assert.Contains("认证", item.StateDisplay);
    }

    [Fact]
    public async Task QueueStateSurvivesStoreRecreation()
    {
        var coordinator = new CloudTransferCoordinator(NullLogger<CloudTransferCoordinator>.Instance);
        var state = CreateState(coordinator);
        await state.RecordRetryScheduledAsync(CloudTransferKind.Media, "game-restart", 1,
            DateTime.UtcNow.AddMinutes(1), "RCLONE_NETWORK_FAILED", "offline", CancellationToken.None);

        var restarted = new SqliteStateStore(options, NullLogger<SqliteStateStore>.Instance);
        await restarted.InitializeAsync(CancellationToken.None);
        var loaded = await restarted.GetCloudTransferAsync(
            CloudTransferStateService.GetTransferKey(CloudTransferKind.Media, "game-restart"), CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Equal(1, loaded!.AttemptCount);
        Assert.Equal("RetryScheduled", loaded.State);
        Assert.Equal("offline", loaded.LastError);
    }

    private CloudTransferStateService CreateState(CloudTransferCoordinator coordinator)
        => new(store, options, new RcloneClient(options, new ExternalProcessRunner(NullLogger<ExternalProcessRunner>.Instance)),
            coordinator, NullLogger<CloudTransferStateService>.Instance);

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(root)) Directory.Delete(root, true);
    }
}

public sealed class CloudUploadWindowPolicyTests
{
    [Fact]
    public void CrossMidnightWindowAllowsBothSidesAndFindsNextStart()
    {
        var date = DateTime.Today;
        var late = date.AddHours(23.5).ToUniversalTime();
        var early = date.AddHours(1).ToUniversalTime();
        var daytime = date.AddHours(12).ToUniversalTime();

        Assert.True(CloudUploadWindowPolicy.IsAllowed(late, 1320, 120));
        Assert.True(CloudUploadWindowPolicy.IsAllowed(early, 1320, 120));
        Assert.False(CloudUploadWindowPolicy.IsAllowed(daytime, 1320, 120));
        Assert.Equal(date.AddHours(22), CloudUploadWindowPolicy.GetNextAllowedStartUtc(daytime, 1320, 120).ToLocalTime());
    }

    [Fact]
    public void FullDayWindowKeepsExistingBehavior()
    {
        var now = DateTime.UtcNow;
        Assert.True(CloudUploadWindowPolicy.IsAllowed(now, 0, 1440));
        Assert.Equal(now, CloudUploadWindowPolicy.GetNextAllowedStartUtc(now, 0, 1440));
    }
}
