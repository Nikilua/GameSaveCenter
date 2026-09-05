using GameSaveCenter.Contracts;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>
/// Owns the durable, user-visible state shared by backup and media cloud copies.
/// It records only one row per game and transfer kind; remote content is never deleted
/// or overwritten through this service.
/// </summary>
public sealed class CloudTransferStateService
{
    private readonly SqliteStateStore _store;
    private readonly WorkerOptions _options;
    private readonly RcloneClient _rclone;
    private readonly CloudTransferCoordinator _coordinator;

    public CloudTransferStateService(SqliteStateStore store, WorkerOptions options, RcloneClient rclone,
        CloudTransferCoordinator coordinator, ILogger<CloudTransferStateService> logger)
    {
        _store = store;
        _options = options;
        _rclone = rclone;
        _coordinator = coordinator;
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
            State = "Pending", AttemptCount = 0, CreatedUtc = existing?.CreatedUtc ?? now, UpdatedUtc = now
        }, token).ConfigureAwait(false);
    }

    public Task MarkTransferringAsync(CloudTransferKind kind, string playniteId, CancellationToken token)
        => UpdateStateAsync(kind, playniteId, "Transferring", token);

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

        await MarkTransferringAsync(request.Kind, request.PlayniteId, token).ConfigureAwait(false);
        var result = await _coordinator.RunUploadAsync($"{request.Kind} remote check",
            ct => _rclone.CheckAsync(local, remote, ct), token,
            GetTransferKey(request.Kind, request.PlayniteId)).ConfigureAwait(false);
        if (!result.Success)
        {
            var failure = RcloneFailureClassifier.Classify(result.StandardError);
            var code = "RCLONE_CHECK_FAILED";
            if (failure == RcloneFailureKind.Authentication) code = "RCLONE_AUTH_FAILED";
            if (code == "RCLONE_AUTH_FAILED")
            {
                await MarkAuthenticationRequiredAsync(request.Kind, request.PlayniteId, code, result.StandardError, token).ConfigureAwait(false);
                await PersistGameCloudStateAsync(request.Kind, request.PlayniteId, "AuthenticationRequired", token).ConfigureAwait(false);
            }
            else
            {
                await MarkCheckFailedAsync(request.Kind, request.PlayniteId, code, result.StandardError, token).ConfigureAwait(false);
                await PersistGameCloudStateAsync(request.Kind, request.PlayniteId, "CheckFailed", token).ConfigureAwait(false);
            }
            throw new WorkerOperationException(code, "远端 check 未通过；本地副本保持不变。", result.StandardError);
        }

        await MarkRemoteVerifiedAsync(request.Kind, request.PlayniteId, token).ConfigureAwait(false);
        await PersistGameCloudStateAsync(request.Kind, request.PlayniteId, "RemoteVerified", token).ConfigureAwait(false);
        return await GetOneAsync(request.Kind, request.PlayniteId, token).ConfigureAwait(false)
            ?? throw new InvalidOperationException("云端校验状态写入后无法读取。");
    }

    public async Task<CloudTransferSummaryDto> GetStatusAsync(CancellationToken token)
    {
        var entries = await _store.GetCloudTransfersAsync(1000, token).ConfigureAwait(false);
        var legacy = await _store.GetCloudRetriesAsync(1000, token).ConfigureAwait(false);
        var games = await _store.GetCloudGameStatesAsync(token).ConfigureAwait(false);
        var media = await _store.GetCloudMediaStatesAsync(token).ConfigureAwait(false);
        var byKey = new Dictionary<string, CloudTransferStatusDto>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in entries)
            byKey[entry.TransferKey] = ToDto(entry, string.Empty);

        foreach (var old in legacy)
        {
            var key = GetTransferKey(CloudTransferKind.Backup, old.PlayniteId);
            if (byKey.ContainsKey(key)) continue;
            var code = RcloneFailureClassifier.GetErrorCode(RcloneFailureClassifier.Classify(old.LastError));
            byKey[key] = new CloudTransferStatusDto
            {
                TransferKey = key, Kind = CloudTransferKind.Backup, PlayniteId = old.PlayniteId,
                State = code == "RCLONE_AUTH_FAILED" ? "AuthenticationRequired" : "RetryScheduled",
                AttemptCount = old.RetryCount, NextAttemptUtc = old.NextAttemptUtc, LastAttemptUtc = old.UpdatedUtc,
                LastErrorCode = code, LastError = old.LastError, UpdatedUtc = old.UpdatedUtc
            };
        }

        foreach (var game in games)
            AddBaseState(byKey, CloudTransferKind.Backup, game.PlayniteId, game.GameName, game.State);
        foreach (var item in media)
            AddBaseState(byKey, CloudTransferKind.Media, item.PlayniteId, item.GameName, item.State);

        foreach (var active in _coordinator.GetActiveTransfers())
        {
            if (!byKey.TryGetValue(active.TransferKey, out var status))
            {
                if (!TryParseKey(active.TransferKey, out var kind, out var gameId)) continue;
                status = new CloudTransferStatusDto { TransferKey = active.TransferKey, Kind = kind, PlayniteId = gameId, UpdatedUtc = DateTime.UtcNow };
                byKey[active.TransferKey] = status;
            }
            status.State = "Transferring";
            status.UpdatedUtc = DateTime.UtcNow;
        }

        var allItems = byKey.Values
            .Where(x => !string.Equals(x.State, "NotApplicable", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.State switch
            {
                "AuthenticationRequired" => 7,
                "CheckFailed" => 6,
                "Failed" => 5,
                "RetryScheduled" => 4,
                "Transferring" => 3,
                "Pending" => 2,
                "Paused" => 1,
                _ => 0
            })
            .ThenBy(x => x.NextAttemptUtc ?? DateTime.MaxValue)
            .ThenByDescending(x => x.UpdatedUtc)
            .ToList();
        var names = games.Concat<CloudGameStateRecord>(media)
            .GroupBy(x => x.PlayniteId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.Select(y => y.GameName).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n)) ?? string.Empty, StringComparer.OrdinalIgnoreCase);
        foreach (var item in allItems)
            if (string.IsNullOrWhiteSpace(item.GameName) && names.TryGetValue(item.PlayniteId, out var gameName)) item.GameName = gameName;
        var items = allItems.Take(100).ToList();

        var summary = new CloudTransferSummaryDto
        {
            TotalCount = allItems.Count,
            Items = items,
            QueuePaused = _options.CloudUploadQueuePaused,
            OutsideAllowedWindow = _options.EnableCloudUpload
                && !_options.CloudUploadQueuePaused
                && !CloudUploadWindowPolicy.IsAllowed(DateTime.UtcNow, _options.CloudUploadAllowedStartMinute, _options.CloudUploadAllowedEndMinute)
        };
        summary.PendingCount = allItems.Count(x => x.State == "Pending");
        summary.TransferringCount = allItems.Count(x => x.State == "Transferring");
        summary.RetryScheduledCount = allItems.Count(x => x.State == "RetryScheduled");
        summary.AuthenticationRequiredCount = allItems.Count(x => x.State == "AuthenticationRequired");
        summary.UploadedCount = allItems.Count(x => x.State == "Uploaded");
        summary.VerifiedCount = allItems.Count(x => x.State == "RemoteVerified");
        summary.CheckFailedCount = allItems.Count(x => x.State == "CheckFailed");
        summary.FailedCount = allItems.Count(x => x.State == "Failed");
        summary.PausedCount = allItems.Count(x => x.State == "Paused");
        summary.NextAttemptUtc = allItems.Where(x => x.State == "RetryScheduled" && x.NextAttemptUtc.HasValue).Select(x => x.NextAttemptUtc).Min();
        return summary;
    }

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
        string errorCode = "", string error = "", bool clearError = false)
    {
        var now = DateTime.UtcNow;
        var existing = await _store.GetCloudTransferAsync(GetTransferKey(kind, playniteId), token).ConfigureAwait(false);
        await SaveAsync(new CloudTransferQueueEntry
        {
            TransferKey = GetTransferKey(kind, playniteId), Kind = kind, PlayniteId = playniteId, State = state,
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
            TransferKey = entry.TransferKey, Kind = entry.Kind, PlayniteId = entry.PlayniteId, GameName = gameName,
            State = entry.State, AttemptCount = entry.AttemptCount, NextAttemptUtc = entry.NextAttemptUtc,
            LastAttemptUtc = entry.LastAttemptUtc, LastErrorCode = entry.LastErrorCode, LastError = entry.LastError,
            UpdatedUtc = entry.UpdatedUtc
        };

    private static void AddBaseState(Dictionary<string, CloudTransferStatusDto> byKey, CloudTransferKind kind, string playniteId, string gameName, string state)
    {
        if (string.IsNullOrWhiteSpace(playniteId) || string.Equals(state, "Disabled", StringComparison.OrdinalIgnoreCase)) return;
        var key = GetTransferKey(kind, playniteId);
        if (byKey.TryGetValue(key, out var existing))
        {
            if (string.IsNullOrWhiteSpace(existing.GameName)) existing.GameName = gameName;
            return;
        }
        byKey[key] = new CloudTransferStatusDto
        {
            TransferKey = key, Kind = kind, PlayniteId = playniteId, GameName = gameName,
            State = state == "Synced" ? "Uploaded" : state, UpdatedUtc = DateTime.UtcNow
        };
    }

    private static bool TryParseKey(string key, out CloudTransferKind kind, out string playniteId)
    {
        kind = CloudTransferKind.Backup;
        playniteId = string.Empty;
        var separator = key.IndexOf(':');
        if (separator <= 0 || separator == key.Length - 1) return false;
        if (!Enum.TryParse(key[..separator], true, out kind)) return false;
        playniteId = key[(separator + 1)..];
        return true;
    }

    private static string Sanitize(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = value.Select(c => invalid.Contains(c) ? '_' : c).ToArray();
        return new string(chars).Trim().TrimEnd('.');
    }
}
