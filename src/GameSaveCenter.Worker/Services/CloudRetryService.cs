using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>Resumes durable backup and media uploads after a Worker restart.</summary>
public sealed class CloudRetryService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);
    private readonly WorkerOptions _options;
    private readonly SqliteStateStore _store;
    private readonly BackupOrchestrator _backups;
    private readonly MediaSyncService _media;
    private readonly CloudTransferStateService _cloudState;
    private readonly RcloneClient _rclone;
    private readonly ILogger<CloudRetryService> _logger;

    public CloudRetryService(WorkerOptions options, SqliteStateStore store, BackupOrchestrator backups,
        MediaSyncService media, CloudTransferStateService cloudState, RcloneClient rclone, ILogger<CloudRetryService> logger)
    {
        _options = options;
        _store = store;
        _backups = backups;
        _media = media;
        _cloudState = cloudState;
        _rclone = rclone;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!_options.SafeModeEnabled && _options.EnableCloudUpload && _rclone.IsConfigured
                    && !_options.CloudUploadQueuePaused)
                {
                    var now=DateTime.UtcNow;
                    if (!CloudUploadWindowPolicy.IsAllowed(now,_options.CloudUploadAllowedStartMinute,_options.CloudUploadAllowedEndMinute))
                    {
                        await DeferDueEntriesAsync(CloudUploadWindowPolicy.GetNextAllowedStartUtc(now,_options.CloudUploadAllowedStartMinute,_options.CloudUploadAllowedEndMinute),stoppingToken).ConfigureAwait(false);
                    }
                    else
                    {
                        if (Directory.Exists(_options.LudusaviBackupDirectory))
                            await RetryBackupsAsync(stoppingToken).ConfigureAwait(false);
                        if (Directory.Exists(_options.MediaArchiveDirectory))
                            await RetryMediaAsync(stoppingToken).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cloud retry sweep failed; queued uploads will be retried on the next sweep");
            }

            try { await Task.Delay(PollInterval, stoppingToken).ConfigureAwait(false); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }

    private async Task DeferDueEntriesAsync(DateTime nextAttemptUtc, CancellationToken token)
    {
        var dueBackups=await _store.GetDueCloudRetriesAsync(DateTime.UtcNow,10,token).ConfigureAwait(false);
        foreach(var entry in dueBackups)
        {
            await _store.DeferCloudRetryAsync(entry.PlayniteId,nextAttemptUtc,"当前不在云端允许上传时段。",token).ConfigureAwait(false);
            await _cloudState.RecordRetryScheduledAsync(CloudTransferKind.Backup,entry.PlayniteId,entry.RetryCount,nextAttemptUtc,"CLOUD_OUTSIDE_ALLOWED_WINDOW","当前不在云端允许上传时段。",token).ConfigureAwait(false);
        }
        var dueMedia=await _store.GetDueCloudTransfersAsync(CloudTransferKind.Media,DateTime.UtcNow,10,token).ConfigureAwait(false);
        foreach(var entry in dueMedia)
            await _cloudState.DeferAsync(CloudTransferKind.Media,entry.PlayniteId,nextAttemptUtc,"当前不在云端允许上传时段。",token).ConfigureAwait(false);
    }

    private async Task RetryBackupsAsync(CancellationToken token)
    {
        var legacy=await _store.GetDueCloudRetriesAsync(DateTime.UtcNow,10,token).ConfigureAwait(false);
        var durable=await _store.GetDueCloudTransfersAsync(CloudTransferKind.Backup,DateTime.UtcNow,10,token).ConfigureAwait(false);
        var dueIds=legacy.Select(x=>x.PlayniteId)
            .Concat(durable.Select(x=>x.PlayniteId))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(10)
            .ToList();
        foreach (var playniteId in dueIds)
        {
            try
            {
                var task = await _backups.RetryCloudUploadAsync(playniteId, token).ConfigureAwait(false);
                if (task.State == TaskState.Succeeded)
                    _logger.LogInformation("Cloud backup retry completed for {GameId}", playniteId);
                else if (string.Equals(task.ErrorCode, "CLOUD_GAME_NOT_FOUND", StringComparison.Ordinal))
                {
                    await _store.RemoveCloudRetryAsync(playniteId, token).ConfigureAwait(false);
                    await _store.AppendAuditAsync("CloudRetry", "已移除不存在游戏的云端重试任务", playniteId, token).ConfigureAwait(false);
                }
                else if (string.Equals(task.ErrorCode, "GAME_OPERATION_BUSY", StringComparison.Ordinal))
                {
                    _logger.LogDebug("Cloud backup retry skipped because the game is busy: {GameId}", playniteId);
                }
                else if (task.ErrorCode.StartsWith("RCLONE_", StringComparison.OrdinalIgnoreCase)
                    && !RcloneFailureClassifier.IsRetryable(task.ErrorCode))
                {
                    await _store.RemoveCloudRetryAsync(playniteId, token).ConfigureAwait(false);
                    await _store.UpdateGameCloudStateAsync(playniteId,
                        string.Equals(task.ErrorCode, "RCLONE_AUTH_FAILED", StringComparison.OrdinalIgnoreCase)
                            ? "AuthenticationRequired" : "Failed", token).ConfigureAwait(false);
                    await _store.AppendAuditAsync("CloudRetry", $"云端重试已停止（{task.ErrorCode}）", task.ErrorMessage, token).ConfigureAwait(false);
                }
                else if (!RcloneFailureClassifier.IsRetryable(task.ErrorCode))
                {
                    await _store.DeferCloudRetryAsync(playniteId, DateTime.UtcNow.AddMinutes(5),
                        task.ErrorMessage ?? "云端上传暂不可用", token).ConfigureAwait(false);
                    await _cloudState.DeferAsync(CloudTransferKind.Backup,playniteId,DateTime.UtcNow.AddMinutes(5),
                        task.ErrorMessage ?? "云端上传暂不可用",token).ConfigureAwait(false);
                }
            }
            catch (WorkerOperationException ex) when (string.Equals(ex.Code, "CLOUD_GAME_NOT_FOUND", StringComparison.Ordinal))
            {
                await _store.RemoveCloudRetryAsync(playniteId, token).ConfigureAwait(false);
                await _store.AppendAuditAsync("CloudRetry", "已移除不存在游戏的云端重试任务", playniteId, token).ConfigureAwait(false);
            }
            catch (WorkerOperationException ex) when (string.Equals(ex.Code, "GAME_OPERATION_BUSY", StringComparison.Ordinal))
            {
                _logger.LogDebug("Cloud backup retry skipped because the game is busy: {GameId}", playniteId);
            }
            catch (WorkerOperationException ex)
            {
                var next=DateTime.UtcNow.AddMinutes(5);
                await _store.DeferCloudRetryAsync(playniteId,next,ex.Message,token).ConfigureAwait(false);
                await _cloudState.DeferAsync(CloudTransferKind.Backup,playniteId,next,ex.Message,token).ConfigureAwait(false);
            }
        }
    }

    private async Task RetryMediaAsync(CancellationToken token)
    {
        var due = await _store.GetDueCloudTransfersAsync(CloudTransferKind.Media, DateTime.UtcNow, 10, token).ConfigureAwait(false);
        foreach (var entry in due)
        {
            try
            {
                var task = await _media.RetryCloudUploadAsync(entry.PlayniteId, token).ConfigureAwait(false);
                if (task.State == TaskState.Succeeded)
                    _logger.LogInformation("Cloud media retry completed for {GameId}", entry.PlayniteId);
            }
            catch (WorkerOperationException ex) when (string.Equals(ex.Code, "GAME_OPERATION_BUSY", StringComparison.Ordinal))
            {
                _logger.LogDebug("Cloud media retry skipped because the game is busy: {GameId}", entry.PlayniteId);
            }
            catch (WorkerOperationException ex)
            {
                await _cloudState.DeferAsync(CloudTransferKind.Media, entry.PlayniteId, DateTime.UtcNow.AddMinutes(5), ex.Message, token).ConfigureAwait(false);
            }
        }
    }
}
