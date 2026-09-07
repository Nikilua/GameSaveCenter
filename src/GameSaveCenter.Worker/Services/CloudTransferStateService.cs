using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>
/// Owns the durable, user-visible state shared by backup and media cloud copies.
/// It records only one row per game and transfer kind; remote content is never deleted
/// or overwritten through this service. Uploads and read-only verification use separate
/// durable operation generations so a late check cannot overwrite a newer upload.
/// </summary>
public sealed class CloudTransferStateService
{
    private readonly SqliteStateStore _store;
    private readonly WorkerOptions _options;
    private readonly RcloneClient _rclone;
    private readonly CloudTransferCoordinator _coordinator;
    private readonly ILogger<CloudTransferStateService> _logger;

    // Test-only injection keeps cancellation and process-failure windows deterministic
    // without requiring a real remote provider or mutating user data.
    internal Func<string, string, CancellationToken, Task<ProcessResult>>? VerifyCheckHook { get; set; }

    public CloudTransferStateService(SqliteStateStore store, WorkerOptions options, RcloneClient rclone,
        CloudTransferCoordinator coordinator, ILogger<CloudTransferStateService> logger)
    {
        _store = store;
        _options = options;
        _rclone = rclone;
        _coordinator = coordinator;
        _logger = logger;
    }

    public static string GetTransferKey(CloudTransferKind kind, string playniteId)
        => $"{kind}:{playniteId}";

    public async Task StartNewAsync(CloudTransferKind kind, string playniteId, CancellationToken token)
    {
        var now = DateTime.UtcNow;
        var existing = await _store.GetCloudTransferAsync(GetTransferKey(kind, playniteId), token).ConfigureAwait(false);
        await SaveAsync(new CloudTransferQueueEntry
        {
            TransferKey = GetTransferKey(kind, playniteId), Kind = kind, PlayniteId = playniteId,
            State = "Pending", OperationKind = CloudTransferOperationKind.Upload, OperationId = NewOperationId(),
            AttemptCount = 0, CreatedUtc = existing?.CreatedUtc ?? now, UpdatedUtc = now
        }, token).ConfigureAwait(false);
    }

    public Task MarkTransferringAsync(CloudTransferKind kind, string playniteId, CancellationToken token)
        => UpdateStateAsync(kind, playniteId, "Transferring", token, beginOperation: true);

    public Task MarkUploadedAsync(CloudTransferKind kind, string playniteId, CancellationToken token)
        => UpdateStateAsync(kind, playniteId, "Uploaded", token, clearError: true);

    public Task MarkRemoteVerifiedAsync(CloudTransferKind kind, string playniteId, CancellationToken token)
        => UpdateStateAsync(kind, playniteId, "RemoteVerified", token, clearError: true);

    public Task MarkCheckFailedAsync(CloudTransferKind kind, string playniteId, string errorCode, string error, CancellationToken token)
        => UpdateStateAsync(kind, playniteId, "CheckFailed", token, errorCode, error);

    public Task MarkPausedAsync(CloudTransferKind kind, string playniteId, string reason, CancellationToken token)
        => UpdateStateAsync(kind, playniteId, "Paused", token, "CLOUD_POLICY_PAUSED", reason);

    public Task MarkFailedAsync(CloudTransferKind kind, string playniteId, string errorCode, string error, CancellationToken token)
        => UpdateStateAsync(kind, playniteId, "Failed", token, errorCode, error);

    public Task MarkAuthenticationRequiredAsync(CloudTransferKind kind, string playniteId, string errorCode, string error, CancellationToken token)
        => UpdateStateAsync(kind, playniteId, "AuthenticationRequired", token, errorCode, error);

    public async Task DeferAsync(CloudTransferKind kind, string playniteId, DateTime nextAttemptUtc, string error, CancellationToken token)
    {
        var existing = await _store.GetCloudTransferAsync(GetTransferKey(kind, playniteId), token).ConfigureAwait(false);
        if (existing == null) return;
        await _store.UpsertCloudTransferAsync(new CloudTransferQueueEntry
        {
            TransferKey = existing.TransferKey, Kind = existing.Kind, PlayniteId = existing.PlayniteId,
            State = "RetryScheduled", AttemptCount = existing.AttemptCount, NextAttemptUtc = nextAttemptUtc,
            LastAttemptUtc = existing.LastAttemptUtc, LastErrorCode = existing.LastErrorCode, LastError = error,
            OperationKind = existing.OperationKind, OperationId = existing.OperationId,
            CreatedUtc = existing.CreatedUtc, UpdatedUtc = DateTime.UtcNow
        }, token).ConfigureAwait(false);
    }

    public Task RecoverInterruptedAsync(CancellationToken token)
        => _store.RecoverInterruptedCloudTransfersAsync(DateTime.UtcNow, token);

    /// <summary>Persists one bounded automatic retry schedule and stops for non-retryable errors.</summary>
    public async Task ScheduleAutomaticRetryAsync(CloudTransferKind kind, string playniteId, string errorCode, string error, CancellationToken token)
    {
        var existing = await _store.GetCloudTransferAsync(GetTransferKey(kind, playniteId), token).ConfigureAwait(false);
        if (string.Equals(errorCode, "RCLONE_AUTH_FAILED", StringComparison.OrdinalIgnoreCase))
        {
            await MarkAuthenticationRequiredAsync(kind, playniteId, errorCode, error, token).ConfigureAwait(false);
            return;
        }

        if (!RcloneFailureClassifier.IsRetryable(errorCode))
        {
            await MarkFailedAsync(kind, playniteId, errorCode, error, token).ConfigureAwait(false);
            return;
        }

        var completedRetries = existing?.AttemptCount ?? 0;
        if (CloudRetryPolicy.IsAutomaticRetryLimitReached(completedRetries))
        {
            await MarkFailedAsync(kind, playniteId, errorCode, error, token).ConfigureAwait(false);
            return;
        }

        var now = DateTime.UtcNow;
        var retryCount = completedRetries + 1;
        await RecordRetryScheduledAsync(kind, playniteId, retryCount,
            CloudRetryPolicy.GetNextAttemptUtc(retryCount, now), errorCode, error, token).ConfigureAwait(false);
    }

    public async Task RecordRetryScheduledAsync(CloudTransferKind kind, string playniteId, int retryCount,
        DateTime nextAttemptUtc, string errorCode, string error, CancellationToken token)
    {
        var now = DateTime.UtcNow;
        var existing = await _store.GetCloudTransferAsync(GetTransferKey(kind, playniteId), token).ConfigureAwait(false);
        await SaveAsync(new CloudTransferQueueEntry
        {
            TransferKey = GetTransferKey(kind, playniteId), Kind = kind, PlayniteId = playniteId,
            State = "RetryScheduled", AttemptCount = Math.Max(0, retryCount), NextAttemptUtc = nextAttemptUtc,
            LastAttemptUtc = now, LastErrorCode = errorCode, LastError = error,
            OperationKind = existing?.OperationKind ?? CloudTransferOperationKind.Upload,
            OperationId = existing?.OperationId ?? NewOperationId(),
            CreatedUtc = existing?.CreatedUtc ?? now, UpdatedUtc = now
        }, token).ConfigureAwait(false);
    }

    public async Task<CloudTransferStatusDto> VerifyAsync(CloudTransferVerifyRequestDto request, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(request.PlayniteId)) throw new ArgumentException("必须提供游戏标识。");
        if (!_options.EnableCloudUpload || !_rclone.IsConfigured)
            throw new WorkerOperationException("RCLONE_NOT_CONFIGURED", "云端复制尚未启用或 Rclone 配置不可用。", _options.RcloneDestination);

        var game = (await _store.GetCloudGameStatesAsync(token).ConfigureAwait(false))
            .FirstOrDefault(x => string.Equals(x.PlayniteId, request.PlayniteId, StringComparison.OrdinalIgnoreCase));
        var gameName = game?.GameName;
        if (string.IsNullOrWhiteSpace(gameName))
        {
            var media = (await _store.GetCloudMediaStatesAsync(token).ConfigureAwait(false))
                .FirstOrDefault(x => string.Equals(x.PlayniteId, request.PlayniteId, StringComparison.OrdinalIgnoreCase));
            gameName = media?.GameName;
        }
        if (string.IsNullOrWhiteSpace(gameName)) throw new WorkerOperationException("CLOUD_GAME_NOT_FOUND", "找不到需要校验云端内容的游戏。", request.PlayniteId);

        var local = request.Kind == CloudTransferKind.Backup
            ? _options.LudusaviBackupDirectory
            : Path.Combine(_options.MediaArchiveDirectory, Sanitize(gameName));
        var remote = request.Kind == CloudTransferKind.Backup
            ? Path.Combine(_options.DeviceStorageKey, "Saves")
            : Path.Combine(Environment.MachineName, "Media", Sanitize(gameName));
        if (!Directory.Exists(local)) throw new WorkerOperationException("CLOUD_LOCAL_SOURCE_MISSING", "本地云端复制源不存在，已阻止校验。", local);

        var operation = await BeginVerificationAsync(request.Kind, request.PlayniteId, token).ConfigureAwait(false);
        var finalized = false;
        try
        {
            var result = await _coordinator.RunUploadAsync($"{request.Kind} remote check",
                ct => VerifyCheckHook?.Invoke(local, remote, ct) ?? _rclone.CheckAsync(local, remote, ct), token,
                operation.TransferKey, CloudTransferOperationKind.Verify).ConfigureAwait(false);
            if (!result.Success)
            {
                var failure = RcloneFailureClassifier.Classify(result.StandardError);
                var code = failure == RcloneFailureKind.Authentication ? "RCLONE_AUTH_FAILED" : "RCLONE_CHECK_FAILED";
                var state = code == "RCLONE_AUTH_FAILED" ? "AuthenticationRequired" : "CheckFailed";
                finalized = await TryFinalizeVerificationAsync(operation, state, code, result.StandardError).ConfigureAwait(false);
                if (!finalized) throw CreateSupersededException(operation);
                await PersistGameCloudStateBestEffortAsync(request.Kind, request.PlayniteId, state).ConfigureAwait(false);
                throw new WorkerOperationException(code, "远端 check 未通过；本地副本保持不变。", result.StandardError);
            }

            finalized = await TryFinalizeVerificationAsync(operation, "RemoteVerified", string.Empty, string.Empty).ConfigureAwait(false);
            if (!finalized) throw CreateSupersededException(operation);
            await PersistGameCloudStateBestEffortAsync(request.Kind, request.PlayniteId, "RemoteVerified").ConfigureAwait(false);
            return await GetOneAsync(request.Kind, request.PlayniteId, CancellationToken.None).ConfigureAwait(false)
            ?? throw new InvalidOperationException("云端校验状态写入后无法读取。");
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            if (!finalized) await RestoreVerificationBestEffortAsync(operation, "CLOUD_CHECK_CANCELLED").ConfigureAwait(false);
            throw;
        }
        catch (WorkerOperationException)
        {
            if (!finalized) await RestoreVerificationBestEffortAsync(operation, "CLOUD_CHECK_INTERRUPTED").ConfigureAwait(false);
            throw;
        }
        catch (Exception ex)
        {
            if (!finalized) await RestoreVerificationBestEffortAsync(operation, "CLOUD_CHECK_EXCEPTION").ConfigureAwait(false);
            throw new WorkerOperationException("RCLONE_CHECK_EXCEPTION", "远端 check 执行失败；此前云端保证未被提升。", ex.Message);
        }
    }

    private async Task<CloudTransferQueueEntry> BeginVerificationAsync(CloudTransferKind kind, string playniteId, CancellationToken token)
    {
        var now = DateTime.UtcNow;
        var key = GetTransferKey(kind, playniteId);
        var existing = await _store.GetCloudTransferAsync(key, token).ConfigureAwait(false);
        var operation = new CloudTransferQueueEntry
        {
            TransferKey = key,
            Kind = kind,
            PlayniteId = playniteId,
            State = "Verifying",
            OperationKind = CloudTransferOperationKind.Verify,
            OperationId = NewOperationId(),
            AttemptCount = existing?.AttemptCount ?? 0,
            LastAttemptUtc = existing?.LastAttemptUtc,
            LastErrorCode = existing?.LastErrorCode ?? string.Empty,
            LastError = existing?.LastError ?? string.Empty,
            CreatedUtc = existing?.CreatedUtc ?? now,
            UpdatedUtc = now,
            PriorState = existing?.State ?? string.Empty,
            PriorOperationKind = existing?.OperationKind ?? CloudTransferOperationKind.Upload,
            PriorOperationId = existing?.OperationId ?? string.Empty,
            PriorNextAttemptUtc = existing?.NextAttemptUtc,
            PriorLastAttemptUtc = existing?.LastAttemptUtc,
            PriorErrorCode = existing?.LastErrorCode ?? string.Empty,
            PriorError = existing?.LastError ?? string.Empty
        };
        await SaveAsync(operation, token).ConfigureAwait(false);
        return operation;
    }

    private async Task<bool> TryFinalizeVerificationAsync(CloudTransferQueueEntry operation, string state,
        string errorCode, string error)
    {
        var now = DateTime.UtcNow;
        return await _store.TryUpdateCloudTransferAsync(new CloudTransferQueueEntry
        {
            TransferKey = operation.TransferKey,
            Kind = operation.Kind,
            PlayniteId = operation.PlayniteId,
            State = state,
            OperationKind = CloudTransferOperationKind.Verify,
            OperationId = operation.OperationId,
            AttemptCount = operation.AttemptCount,
            LastAttemptUtc = now,
            LastErrorCode = errorCode,
            LastError = error,
            CreatedUtc = operation.CreatedUtc,
            UpdatedUtc = now
        }, operation.OperationId, CancellationToken.None).ConfigureAwait(false);
    }

    private async Task RestoreVerificationBestEffortAsync(CloudTransferQueueEntry operation, string cancellationCode)
    {
        var hasPriorState = !string.IsNullOrWhiteSpace(operation.PriorState);
        var now = DateTime.UtcNow;
        var restored = new CloudTransferQueueEntry
        {
            TransferKey = operation.TransferKey,
            Kind = operation.Kind,
            PlayniteId = operation.PlayniteId,
            State = hasPriorState ? operation.PriorState : "CheckCancelled",
            OperationKind = hasPriorState ? operation.PriorOperationKind : CloudTransferOperationKind.Verify,
            OperationId = hasPriorState && !string.IsNullOrWhiteSpace(operation.PriorOperationId)
                ? operation.PriorOperationId : operation.OperationId,
            AttemptCount = operation.AttemptCount,
            NextAttemptUtc = hasPriorState ? operation.PriorNextAttemptUtc : null,
            LastAttemptUtc = hasPriorState ? operation.PriorLastAttemptUtc : now,
            LastErrorCode = hasPriorState ? operation.PriorErrorCode : cancellationCode,
            LastError = hasPriorState ? operation.PriorError : "远端校验已取消或中断，未发起上传。",
            CreatedUtc = operation.CreatedUtc,
            UpdatedUtc = now
        };
        try
        {
            var restoredByThisOperation = await _store.TryUpdateCloudTransferAsync(restored, operation.OperationId, CancellationToken.None).ConfigureAwait(false);
            if (!restoredByThisOperation)
                _logger.LogInformation("Cloud verification cleanup skipped because a newer operation owns {TransferKey}", operation.TransferKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not finalize cancelled cloud verification for {TransferKey}", operation.TransferKey);
        }
    }

    private async Task PersistGameCloudStateBestEffortAsync(CloudTransferKind kind, string playniteId, string state)
    {
        try
        {
            await PersistGameCloudStateAsync(kind, playniteId, state, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not project cloud verification state for {Kind}/{PlayniteId}", kind, playniteId);
        }
    }

    private static WorkerOperationException CreateSupersededException(CloudTransferQueueEntry operation)
        => new("CLOUD_CHECK_SUPERSEDED", "远端校验结果已被更新的云端操作取代，未覆盖新状态。", operation.TransferKey);

    private static string NewOperationId() => Guid.NewGuid().ToString("N");

    public Task<CloudTransferSummaryDto> GetStatusAsync(CancellationToken token)
        => GetStatusAsync(new CloudTransferStatusRequestDto(), token);

    public async Task<CloudTransferSummaryDto> GetStatusAsync(CloudTransferStatusRequestDto request, CancellationToken token)
    {
        var stateFilter = request.State?.Trim() ?? string.Empty;
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var page = Math.Clamp(request.Page, 0, int.MaxValue / pageSize);
        var offset = page * pageSize;
        var kindFilter = request.Kind;
        var consistencyToken = await _store.GetQueryRevisionAsync(
            SqliteStateStore.CloudTransferQueryRevision, token).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(request.ConsistencyToken)
            && !string.Equals(request.ConsistencyToken, consistencyToken, StringComparison.Ordinal))
        {
            return CreatePageResetSummary(page, pageSize, stateFilter, kindFilter, consistencyToken,
                "云端队列已发生变化，请从第一页刷新后继续。");
        }

        var aggregate = await _store.GetCloudTransferSummaryAsync(stateFilter, kindFilter, token).ConfigureAwait(false);
        var entries = await _store.GetCloudTransferPageAsync(offset, pageSize, stateFilter, kindFilter, token).ConfigureAwait(false);
        var games = await _store.GetCloudGameStatesAsync(token).ConfigureAwait(false);
        var media = await _store.GetCloudMediaStatesAsync(token).ConfigureAwait(false);
        var names = games.Concat<CloudGameStateRecord>(media)
            .GroupBy(x => x.PlayniteId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.Select(y => y.GameName).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? string.Empty, StringComparer.OrdinalIgnoreCase);
        var items = entries.Select(entry =>
        {
            var gameName = names.TryGetValue(entry.PlayniteId, out var name) ? name : string.Empty;
            return ToDto(entry, gameName);
        }).ToList();

        var activeByKey = _coordinator.GetActiveTransfers()
            .ToDictionary(x => x.TransferKey, StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            if (!activeByKey.TryGetValue(item.TransferKey, out var active)) continue;
            item.OperationKind = active.OperationKind;
            item.State = active.OperationKind == CloudTransferOperationKind.Verify ? "Verifying" : "Transferring";
            item.UpdatedUtc = DateTime.UtcNow;
        }

        var summary = new CloudTransferSummaryDto
        {
            TotalCount = aggregate.TotalCount,
            PendingCount = aggregate.PendingCount,
            TransferringCount = aggregate.TransferringCount,
            VerifyingCount = aggregate.VerifyingCount,
            RetryScheduledCount = aggregate.RetryScheduledCount,
            AuthenticationRequiredCount = aggregate.AuthenticationRequiredCount,
            UploadedCount = aggregate.UploadedCount,
            VerifiedCount = aggregate.VerifiedCount,
            CheckFailedCount = aggregate.CheckFailedCount,
            FailedCount = aggregate.FailedCount,
            PausedCount = aggregate.PausedCount,
            NextAttemptUtc = aggregate.NextAttemptUtc,
            Items = items,
            Page = page,
            PageSize = pageSize,
            LoadedCount = items.Count,
            HasMore = offset + items.Count < aggregate.TotalCount,
            StateFilter = stateFilter,
            KindFilter = kindFilter,
            QueuePaused = _options.CloudUploadQueuePaused,
            OutsideAllowedWindow = _options.EnableCloudUpload
                && !_options.CloudUploadQueuePaused
                && !CloudUploadWindowPolicy.IsAllowed(DateTime.UtcNow, _options.CloudUploadAllowedStartMinute, _options.CloudUploadAllowedEndMinute)
        };
        var completedToken = await _store.GetQueryRevisionAsync(
            SqliteStateStore.CloudTransferQueryRevision, token).ConfigureAwait(false);
        if (!string.Equals(consistencyToken, completedToken, StringComparison.Ordinal))
        {
            return CreatePageResetSummary(page, pageSize, stateFilter, kindFilter, completedToken,
                "云端队列在加载期间发生变化，请从第一页刷新后继续。");
        }

        summary.ConsistencyToken = consistencyToken;
        return summary;
    }

    private static CloudTransferSummaryDto CreatePageResetSummary(
        int page, int pageSize, string stateFilter, CloudTransferKind? kindFilter,
        string consistencyToken, string reason)
        => new()
        {
            Page = page,
            PageSize = pageSize,
            StateFilter = stateFilter,
            KindFilter = kindFilter,
            ConsistencyToken = consistencyToken,
            PageResetRequired = true,
            PageResetReason = reason
        };

    public async Task<CloudTransferStatusDto?> GetOneAsync(CloudTransferKind kind, string playniteId, CancellationToken token)
    {
        var entry = await _store.GetCloudTransferAsync(GetTransferKey(kind, playniteId), token).ConfigureAwait(false);
        if (entry == null) return null;
        var gameName = (await _store.GetCloudGameStatesAsync(token).ConfigureAwait(false))
            .FirstOrDefault(x => string.Equals(x.PlayniteId, playniteId, StringComparison.OrdinalIgnoreCase))?.GameName;
        if (string.IsNullOrWhiteSpace(gameName))
            gameName = (await _store.GetCloudMediaStatesAsync(token).ConfigureAwait(false))
                .FirstOrDefault(x => string.Equals(x.PlayniteId, playniteId, StringComparison.OrdinalIgnoreCase))?.GameName;
        return ToDto(entry, gameName ?? string.Empty);
    }

    private async Task UpdateStateAsync(CloudTransferKind kind, string playniteId, string state, CancellationToken token,
        string errorCode = "", string error = "", bool clearError = false, bool beginOperation = false)
    {
        var now = DateTime.UtcNow;
        var existing = await _store.GetCloudTransferAsync(GetTransferKey(kind, playniteId), token).ConfigureAwait(false);
        var operationId = beginOperation || string.IsNullOrWhiteSpace(existing?.OperationId)
            ? NewOperationId() : existing!.OperationId;
        await SaveAsync(new CloudTransferQueueEntry
        {
            TransferKey = GetTransferKey(kind, playniteId), Kind = kind, PlayniteId = playniteId, State = state,
            OperationKind = beginOperation ? CloudTransferOperationKind.Upload : existing?.OperationKind ?? CloudTransferOperationKind.Upload,
            OperationId = operationId,
            AttemptCount = existing?.AttemptCount ?? 0,
            NextAttemptUtc = state == "RetryScheduled" ? existing?.NextAttemptUtc : null,
            LastAttemptUtc = state == "Transferring" || state == "Uploaded" || state == "RemoteVerified" ? now : existing?.LastAttemptUtc,
            LastErrorCode = clearError ? string.Empty : string.IsNullOrWhiteSpace(errorCode) ? existing?.LastErrorCode ?? string.Empty : errorCode,
            LastError = clearError ? string.Empty : string.IsNullOrWhiteSpace(error) ? existing?.LastError ?? string.Empty : error,
            CreatedUtc = existing?.CreatedUtc ?? now, UpdatedUtc = now
        }, token).ConfigureAwait(false);
    }

    private Task SaveAsync(CloudTransferQueueEntry entry, CancellationToken token)
        => _store.UpsertCloudTransferAsync(entry, token);

    private Task PersistGameCloudStateAsync(CloudTransferKind kind, string playniteId, string state, CancellationToken token)
        => kind == CloudTransferKind.Backup
            ? _store.UpdateGameCloudStateAsync(playniteId, state, token)
            : _store.UpdateMediaCloudStateAsync(playniteId, state, token);

    private static CloudTransferStatusDto ToDto(CloudTransferQueueEntry entry, string gameName)
        => new()
        {
            TransferKey = entry.TransferKey, Kind = entry.Kind, OperationKind = entry.OperationKind, PlayniteId = entry.PlayniteId, GameName = gameName,
            State = entry.State, AttemptCount = entry.AttemptCount, NextAttemptUtc = entry.NextAttemptUtc,
            LastAttemptUtc = entry.LastAttemptUtc, LastErrorCode = entry.LastErrorCode, LastError = entry.LastError,
            UpdatedUtc = entry.UpdatedUtc
        };

    private static string Sanitize(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = value.Select(c => invalid.Contains(c) ? '_' : c).ToArray();
        return new string(chars).Trim().TrimEnd('.');
    }
}
