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
            RcloneExecutable = Path.Combine(Environment.SystemDirectory, "WindowsPowerShell", "v1.0", "powershell.exe"),
            RcloneDestination = "remote:",
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

    [Fact]
    public async Task CancelledVerificationRestoresPreviousUploadedGuarantee()
    {
        await PrepareVerificationGameAsync();
        var coordinator = new CloudTransferCoordinator(NullLogger<CloudTransferCoordinator>.Instance);
        var state = CreateState(coordinator);
        await state.StartNewAsync(CloudTransferKind.Backup, "game-verify", CancellationToken.None);
        await state.MarkUploadedAsync(CloudTransferKind.Backup, "game-verify", CancellationToken.None);
        using var lease = await coordinator.PauseForRestoreAsync(CancellationToken.None);
        using var cancellation = new CancellationTokenSource();

        var verification = state.VerifyAsync(new CloudTransferVerifyRequestDto
        {
            Kind = CloudTransferKind.Backup, PlayniteId = "game-verify"
        }, cancellation.Token);
        await WaitForStateAsync("Backup:game-verify", "Verifying");
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => verification);
        var restored = await store.GetCloudTransferAsync("Backup:game-verify", CancellationToken.None);
        Assert.NotNull(restored);
        Assert.Equal("Uploaded", restored!.State);
        Assert.Equal(CloudTransferOperationKind.Upload, restored.OperationKind);
    }

    [Fact]
    public async Task VerificationExceptionRestoresPreviousRemoteGuarantee()
    {
        await PrepareVerificationGameAsync();
        var coordinator = new CloudTransferCoordinator(NullLogger<CloudTransferCoordinator>.Instance);
        var state = CreateState(coordinator);
        await state.StartNewAsync(CloudTransferKind.Backup, "game-verify", CancellationToken.None);
        await state.MarkRemoteVerifiedAsync(CloudTransferKind.Backup, "game-verify", CancellationToken.None);
        state.VerifyCheckHook = (_, _, _) => throw new InvalidOperationException("provider unavailable");

        var error = await Assert.ThrowsAsync<WorkerOperationException>(() => state.VerifyAsync(
            new CloudTransferVerifyRequestDto { Kind = CloudTransferKind.Backup, PlayniteId = "game-verify" },
            CancellationToken.None));

        Assert.Equal("RCLONE_CHECK_EXCEPTION", error.Code);
        var restored = await store.GetCloudTransferAsync("Backup:game-verify", CancellationToken.None);
        Assert.NotNull(restored);
        Assert.Equal("RemoteVerified", restored!.State);
        Assert.Equal(CloudTransferOperationKind.Upload, restored.OperationKind);
    }

    [Fact]
    public async Task VerificationFailureIsRecordedWithoutClaimingRemoteSuccess()
    {
        await PrepareVerificationGameAsync();
        var coordinator = new CloudTransferCoordinator(NullLogger<CloudTransferCoordinator>.Instance);
        var state = CreateState(coordinator);
        state.VerifyCheckHook = (_, _, _) => Task.FromResult(ProcessResult.Failed(1, string.Empty, "checksum mismatch"));

        var error = await Assert.ThrowsAsync<WorkerOperationException>(() => state.VerifyAsync(
            new CloudTransferVerifyRequestDto { Kind = CloudTransferKind.Backup, PlayniteId = "game-verify" },
            CancellationToken.None));

        Assert.Equal("RCLONE_CHECK_FAILED", error.Code);
        var failed = await store.GetCloudTransferAsync("Backup:game-verify", CancellationToken.None);
        Assert.NotNull(failed);
        Assert.Equal("CheckFailed", failed!.State);
        Assert.Equal(CloudTransferOperationKind.Verify, failed.OperationKind);
        Assert.Equal("CheckFailed", (await store.GetCloudGameStatesAsync(CancellationToken.None)).Single(x => x.PlayniteId == "game-verify").State);
    }

    [Fact]
    public async Task LateVerificationCannotOverwriteNewerUploadGeneration()
    {
        await PrepareVerificationGameAsync();
        var coordinator = new CloudTransferCoordinator(NullLogger<CloudTransferCoordinator>.Instance);
        var state = CreateState(coordinator);
        await state.StartNewAsync(CloudTransferKind.Backup, "game-verify", CancellationToken.None);
        await state.MarkUploadedAsync(CloudTransferKind.Backup, "game-verify", CancellationToken.None);
        var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        state.VerifyCheckHook = async (_, _, _) =>
        {
            entered.TrySetResult(true);
            await release.Task;
            return ProcessResult.Failed(0, string.Empty, string.Empty);
        };

        var verification = state.VerifyAsync(new CloudTransferVerifyRequestDto
        {
            Kind = CloudTransferKind.Backup, PlayniteId = "game-verify"
        }, CancellationToken.None);
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await state.StartNewAsync(CloudTransferKind.Backup, "game-verify", CancellationToken.None);
        await state.MarkUploadedAsync(CloudTransferKind.Backup, "game-verify", CancellationToken.None);
        release.TrySetResult(true);

        var error = await Assert.ThrowsAsync<WorkerOperationException>(() => verification);
        Assert.Equal("CLOUD_CHECK_SUPERSEDED", error.Code);
        var current = await store.GetCloudTransferAsync("Backup:game-verify", CancellationToken.None);
        Assert.NotNull(current);
        Assert.Equal("Uploaded", current!.State);
        Assert.Equal(CloudTransferOperationKind.Upload, current.OperationKind);
    }

    [Fact]
    public async Task RestartedVerificationRestoresPriorStateWithoutQueueingUpload()
    {
        var priorOperationId = Guid.NewGuid().ToString("N");
        await store.UpsertCloudTransferAsync(new CloudTransferQueueEntry
        {
            TransferKey = "Backup:game-verify", Kind = CloudTransferKind.Backup, PlayniteId = "game-verify",
            State = "Verifying", OperationKind = CloudTransferOperationKind.Verify, OperationId = "verify-operation",
            PriorState = "Uploaded", PriorOperationKind = CloudTransferOperationKind.Upload, PriorOperationId = priorOperationId,
            CreatedUtc = DateTime.UtcNow.AddMinutes(-2), UpdatedUtc = DateTime.UtcNow
        }, CancellationToken.None);

        await store.RecoverInterruptedCloudTransfersAsync(DateTime.UtcNow, CancellationToken.None);

        var recovered = await store.GetCloudTransferAsync("Backup:game-verify", CancellationToken.None);
        Assert.NotNull(recovered);
        Assert.Equal("Uploaded", recovered!.State);
        Assert.Equal(CloudTransferOperationKind.Upload, recovered.OperationKind);
        Assert.Equal(priorOperationId, recovered.OperationId);
        Assert.Empty(recovered.PriorState);
    }

    private CloudTransferStateService CreateState(CloudTransferCoordinator coordinator)
        => new(store, options, new RcloneClient(options, new ExternalProcessRunner(NullLogger<ExternalProcessRunner>.Instance)),
            coordinator, NullLogger<CloudTransferStateService>.Instance);

    private async Task PrepareVerificationGameAsync()
    {
        await store.UpsertGamesAsync(new[]
        {
            new GameDescriptorDto { PlayniteId = "game-verify", Name = "Verification Game" }
        }, CancellationToken.None);
        await store.UpdateGameCloudStateAsync("game-verify", "Uploaded", CancellationToken.None);
        Directory.CreateDirectory(options.LudusaviBackupDirectory);
    }

    private async Task WaitForStateAsync(string transferKey, string expectedState)
    {
        for (var attempt = 0; attempt < 50; attempt++)
        {
            var entry = await store.GetCloudTransferAsync(transferKey, CancellationToken.None);
            if (entry?.State == expectedState) return;
            await Task.Delay(20);
        }
        var actual = await store.GetCloudTransferAsync(transferKey, CancellationToken.None);
        Assert.Equal(expectedState, actual?.State);
    }

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
