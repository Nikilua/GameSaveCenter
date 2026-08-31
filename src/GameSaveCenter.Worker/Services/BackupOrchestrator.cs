using System.Text.Json;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Services;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameSaveCenter.Worker.Services;

/// <summary>Coordinates safe Ludusavi backups, validation, history indexing and optional upload.</summary>
public sealed class BackupOrchestrator : IBackupHistoryRebuilder
{
    private readonly GameCatalogService _catalog;
    private readonly SqliteStateStore _store;
    private readonly LudusaviClient _ludusavi;
    private readonly RcloneClient _rclone;
    private readonly CloudTransferCoordinator _cloudTransfers;
    private readonly TaskCoordinator _tasks;
    private readonly WorkerOptions _options;
    private readonly GameOperationLock _gameLock;
    private readonly BackupValidationService _validator = new();
    private readonly ILogger<BackupOrchestrator> _logger;
    private readonly IHostApplicationLifetime? _lifetime;
    private int _backupAllInFlight;

    public BackupOrchestrator(
        GameCatalogService catalog,
        SqliteStateStore store,
        LudusaviClient ludusavi,
        RcloneClient rclone,
        CloudTransferCoordinator cloudTransfers,
        TaskCoordinator tasks,
        WorkerOptions options,
        GameOperationLock gameLock,
        ILogger<BackupOrchestrator> logger,
        IHostApplicationLifetime? lifetime = null)
    {
        _catalog = catalog;
        _store = store;
        _ludusavi = ludusavi;
        _rclone = rclone;
        _cloudTransfers = cloudTransfers;
        _tasks = tasks;
        _options = options;
        _gameLock = gameLock;
        _logger = logger;
        _lifetime = lifetime;
    }

    public Task<List<TaskStatusDto>> BackupAsync(BackupRequestDto request, CancellationToken token)
        => BackupAsync(request, token, null, null);

    private async Task<List<TaskStatusDto>> BackupAsync(
        BackupRequestDto request,
        CancellationToken token,
        ISet<string>? completedGameIds,
        Func<int, int, GameDescriptorDto, bool, Task>? progressCallback)
    {
        var games = await _catalog.GetGamesAsync(token).ConfigureAwait(false);
        var matches = await _catalog.GetMatchesAsync(token).ConfigureAwait(false);
        if (request.PlayniteIds.Count > 0)
        {
            games = games
                .Where(x => request.PlayniteIds.Contains(x.PlayniteId, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }

        var totalGames = games.Count;
        var processedGames = completedGameIds == null
            ? 0
            : games.Count(x => completedGameIds.Contains(x.PlayniteId));
        async Task ReportProcessedAsync(GameDescriptorDto game, bool completed)
        {
            if (completed && completedGameIds != null)
                completedGameIds.Add(game.PlayniteId);
            processedGames++;
            if (progressCallback != null)
                await progressCallback(processedGames, totalGames, game, completed).ConfigureAwait(false);
        }

        var results = new List<TaskStatusDto>();
        var requestLabel = GetRequestLabel(request.Reason);
        foreach (var game in games)
        {
            if (completedGameIds?.Contains(game.PlayniteId) == true)
                continue;

            if (!_ludusavi.IsAvailable)
            {
                var task = await _tasks.RunAsync(
                    "Backup",
                    game.PlayniteId,
                    game.Name,
                    (_, _) => Task.FromException(new WorkerOperationException(
                        "LUDUSAVI_NOT_CONFIGURED",
                        "Ludusavi 尚未配置或可执行文件不存在。",
                        _options.LudusaviExecutable)),
                    token, request.NotificationSessionId).ConfigureAwait(false);
                results.Add(task);
                await ReportProcessedAsync(game, task.State == TaskState.Succeeded).ConfigureAwait(false);
                continue;
            }

            if (!matches.TryGetValue(game.PlayniteId, out var match) || string.IsNullOrWhiteSpace(match.Name))
            {
                if (request.PlayniteIds.Count > 0)
                {
                    var task = await _tasks.RunAsync(
                        "Backup",
                        game.PlayniteId,
                        game.Name,
                        (_, _) => Task.FromException(new WorkerOperationException(
                            "LUDUSAVI_GAME_UNMATCHED",
                            "该游戏尚未匹配到 Ludusavi 存档规则。",
                            game.Name)),
                        token, request.NotificationSessionId).ConfigureAwait(false);
                    results.Add(task);
                    await ReportProcessedAsync(game, task.State == TaskState.Succeeded).ConfigureAwait(false);
                }
                else
                    await ReportProcessedAsync(game, false).ConfigureAwait(false);
                continue;
            }

            using var lease = await _gameLock.AcquireAsync(game.PlayniteId, GameOperationKind.Backup, TimeSpan.FromSeconds(10), token).ConfigureAwait(false);
            if (lease == null)
            {
                var task = await _tasks.RunAsync(
                    "Backup",
                    game.PlayniteId,
                    game.Name,
                    (_, _) => Task.FromException(new WorkerOperationException(
                        "GAME_OPERATION_BUSY",
                        "该游戏已有备份、恢复或媒体操作正在执行，已跳过本次备份。",
                        game.PlayniteId)),
                    token, request.NotificationSessionId).ConfigureAwait(false);
                results.Add(task);
                await ReportProcessedAsync(game, task.State == TaskState.Succeeded).ConfigureAwait(false);
                continue;
            }

            var completedTask = await _tasks.RunAsync(
                "Backup",
                game.PlayniteId,
                game.Name,
                async (progress, ct) =>
                {
                    await progress.ReportAsync(10, $"{requestLabel}：正在扫描存档").ConfigureAwait(false);
                    var operation = await _ludusavi
                        .BackupAsync(new[] { match.Name }, request.Force, false, ct)
                        .ConfigureAwait(false);

                    if (!operation.Success)
                    {
                        throw new WorkerOperationException(
                            operation.ErrorCode,
                            operation.ErrorMessage,
                            BuildDiagnostic(operation));
                    }

                    if (!operation.Json.HasValue)
                    {
                        throw new WorkerOperationException(
                            "LUDUSAVI_EMPTY_RESULT",
                            "Ludusavi 没有返回可解析的备份结果。",
                            operation.RawOutput);
                    }

                    if (LudusaviResultParser.SomeGamesFailed(operation.Json.Value))
                    {
                        throw new WorkerOperationException(
                            "LUDUSAVI_GAME_FAILED",
                            "Ludusavi 报告该游戏备份失败。",
                            operation.RawOutput);
                    }

                    var change = LudusaviResultParser.ParseGameChange(operation.Json.Value, match.Name);
                    await _store.AppendAuditAsync(
                        "Backup",
                        $"Ludusavi 备份结果：{game.Name} / {change}",
                        JsonSerializer.Serialize(new
                        {
                            game.PlayniteId,
                            ludusaviName = match.Name,
                            change,
                            operation.ExitCode,
                            operation.WarningText,
                            output = operation.RawOutput
                        }),
                        ct).ConfigureAwait(false);

                    await progress.ReportAsync(55, $"{requestLabel}：正在校验备份摘要").ConfigureAwait(false);
                    var now = DateTime.UtcNow;
                    var snapshot = LudusaviResultParser.ParseOperationSnapshot(
                        operation.Json.Value,
                        match.Name,
                        $"pending-{now:yyyyMMddHHmmss}",
                        now);

                    var previous = (await _store.GetBackupVersionsAsync(game.PlayniteId, ct).ConfigureAwait(false))
                        .FirstOrDefault();
                    var policy = await _store.GetPolicyAsync(game.PlayniteId, ct).ConfigureAwait(false);
                    var previousDomain = previous == null
                        ? null
                        : new GameSaveCenter.Core.Models.BackupSnapshot
                        {
                            BackupId = previous.BackupId,
                            CreatedUtc = previous.CreatedUtc,
                            FileCount = previous.FileCount,
                            TotalBytes = previous.TotalBytes,
                            Files = DeserializeManifest(await _store.GetBackupManifestAsync(game.PlayniteId, previous.BackupId, ct).ConfigureAwait(false))
                        };

                    foreach (var finding in _validator.Validate(snapshot, previousDomain, null, true, policy.AnomalyProtectionLevel))
                    {
                        await _store.AddFindingAsync(
                            game.PlayniteId,
                            new ValidationFindingDto
                            {
                                Severity = finding.Severity,
                                Code = finding.Code,
                                Title = finding.Title,
                                Detail = finding.Detail,
                                SuggestedAction = finding.SuggestedAction
                            },
                            ct).ConfigureAwait(false);
                    }

                    await progress.ReportAsync(70, $"{requestLabel}：正在索引历史版本").ConfigureAwait(false);
                    await RefreshBackupHistoryAsync(game.PlayniteId, match.Name, ct).ConfigureAwait(false);

                    var indexed = (await _store.GetBackupVersionsAsync(game.PlayniteId, ct).ConfigureAwait(false))
                        .OrderByDescending(x => x.CreatedUtc)
                        .FirstOrDefault();
                    if (indexed == null)
                    {
                        throw new WorkerOperationException(
                            "BACKUP_HISTORY_EMPTY",
                            "本地备份已执行，但没有从 Ludusavi 读取到历史版本。",
                            operation.RawOutput);
                    }

                    indexed.TotalBytes = snapshot.TotalBytes;
                    indexed.FileCount = snapshot.FileCount;
                    indexed.ParentBackupId = previous != null
                        && !string.Equals(indexed.BackupId, previous.BackupId, StringComparison.OrdinalIgnoreCase)
                        ? previous.BackupId
                        : indexed.ParentBackupId;
                    await _store.AddBackupVersionAsync(indexed, JsonSerializer.Serialize(snapshot.Files), ct)
                        .ConfigureAwait(false);

                    if (!_options.SafeModeEnabled && _options.EnableCloudUpload && policy.UploadAfterBackup && _rclone.IsConfigured)
                    {
                        await _store.UpdateGameCloudStateAsync(game.PlayniteId,"Pending",ct).ConfigureAwait(false);
                        await progress.ReportAsync(82, $"{requestLabel}：正在复制到云端").ConfigureAwait(false);
                        var cloud = await _cloudTransfers.RunUploadAsync("backup", transferToken => _rclone
                            .CopyAsync(_options.LudusaviBackupDirectory, Path.Combine(_options.DeviceStorageKey, "Saves"), transferToken), ct)
                            .ConfigureAwait(false);
                        if (!cloud.Success)
                        {
                            var failure = RcloneFailureClassifier.Classify(cloud.StandardError);
                            var errorCode = RcloneFailureClassifier.GetErrorCode(failure);
                            await ScheduleCloudRetryAsync(game.PlayniteId, errorCode, cloud.StandardError, ct).ConfigureAwait(false);
                            throw new WorkerOperationException(
                                errorCode,
                                $"本地备份成功，但云端复制失败：{RcloneFailureClassifier.GetUserMessage(failure)}",
                                cloud.StandardError);
                        }
                        await _store.RemoveCloudRetryAsync(game.PlayniteId,ct).ConfigureAwait(false);
                        await _store.UpdateGameCloudStateAsync(game.PlayniteId,"Uploaded",ct).ConfigureAwait(false);
                    }

                    var completion = change switch
                    {
                        "New" => "已创建首个备份版本",
                        "Different" => _options.BackupFormat == BackupStorageFormat.Zip
                            ? "已创建新的历史版本"
                            : "已更新 Simple 当前副本",
                        "Same" => "存档无变化，历史未新增",
                        _ => "备份完成"
                    };
                    await progress.ReportAsync(100, $"{requestLabel}：{completion}").ConfigureAwait(false);
                },
                token, request.NotificationSessionId).ConfigureAwait(false);
            results.Add(completedTask);
            await ReportProcessedAsync(game, completedTask.State == TaskState.Succeeded).ConfigureAwait(false);
        }

        if (request.PlayniteIds.Count > 0 && results.Count == 0)
        {
            throw new WorkerOperationException(
                "BACKUP_GAME_NOT_FOUND",
                "没有找到需要备份的游戏。",
                string.Join(",", request.PlayniteIds));
        }

        return results;
    }

    /// <summary>Creates a durable full-library backup job and returns its queued status immediately.
    /// The request is stored before the background operation starts, so an unexpected Worker exit
    /// leaves enough information for the next Worker instance to resume the unfinished job.</summary>
    public async Task<List<TaskStatusDto>> SubmitAllAsync(BackupRequestDto request, CancellationToken token)
    {
        if (Interlocked.CompareExchange(ref _backupAllInFlight, 1, 0) != 0)
        {
            _logger.LogInformation("Backup-all submission ignored: a full-library backup is already running.");
            var current = await _store.GetActiveBackupAllJobAsync(token).ConfigureAwait(false);
            return current == null ? new List<TaskStatusDto>() : new List<TaskStatusDto> { current.ToTaskStatus() };
        }

        try
        {
            var current = await _store.GetActiveBackupAllJobAsync(token).ConfigureAwait(false);
            if (current != null)
            {
                _logger.LogInformation("Resuming the existing durable backup-all job {JobId}.", current.JobId);
                _ = RunAllBackgroundAsync(current, _lifetime?.ApplicationStopping ?? token);
                return new List<TaskStatusDto> { current.ToTaskStatus() };
            }

            var job = new BackupAllJobRecord
            {
                JobId = Guid.NewGuid().ToString("N"),
                RequestJson = JsonSerializer.Serialize(request),
                CreatedUtc = DateTime.UtcNow,
                WorkerSessionId = _options.WorkerSessionId,
                Message = "等待执行"
            };
            await _store.CreateBackupAllJobAsync(job, token).ConfigureAwait(false);
            _ = RunAllBackgroundAsync(job, _lifetime?.ApplicationStopping ?? token);
            return new List<TaskStatusDto> { job.ToTaskStatus() };
        }
        catch
        {
            Interlocked.Exchange(ref _backupAllInFlight, 0);
            throw;
        }
    }

    /// <summary>Starts an unfinished durable full-library job after storage reconciliation.</summary>
    public async Task ResumePendingAsync(CancellationToken token)
    {
        var job = await _store.GetActiveBackupAllJobAsync(token).ConfigureAwait(false);
        if (job == null || Interlocked.CompareExchange(ref _backupAllInFlight, 1, 0) != 0)
            return;

        _logger.LogInformation("Resuming unfinished backup-all job {JobId} from durable state.", job.JobId);
        _ = RunAllBackgroundAsync(job, token);
    }

    private async Task RunAllBackgroundAsync(BackupAllJobRecord job, CancellationToken token)
    {
        try
        {
            var request = JsonSerializer.Deserialize<BackupRequestDto>(job.RequestJson)
                ?? throw new WorkerOperationException("BACKUP_ALL_REQUEST_INVALID", "整库备份任务请求已损坏，无法恢复。", job.JobId);
            request.PlayniteIds ??= new List<string>();
            request.PlayniteIds.Clear();

            job.State = TaskState.Running;
            job.StartedUtc ??= DateTime.UtcNow;
            job.WorkerSessionId = _options.WorkerSessionId;
            job.Message = "正在准备整库备份";
            await _store.UpdateBackupAllJobAsync(job, CancellationToken.None).ConfigureAwait(false);

            var completedGameIds = DeserializeGameIds(job.CompletedGameIdsJson);
            var result = await _tasks.RunAsync(
                "BackupAll",
                string.Empty,
                "全部游戏",
                async (progress, operationToken) =>
                {
                    var results = await BackupAsync(
                        request,
                        operationToken,
                        completedGameIds,
                        async (processed, total, game, completed) =>
                        {
                            job.ProgressPercent = total <= 0 ? 100 : Math.Clamp(processed * 100 / total, 0, 99);
                            job.CurrentGameId = game.PlayniteId;
                            job.Message = completed
                                ? $"正在处理整库备份：{processed}/{total} · {game.Name}"
                                : $"已记录结果：{processed}/{total} · {game.Name}";
                            job.CompletedGameIdsJson = JsonSerializer.Serialize(completedGameIds.OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
                            await _store.UpdateBackupAllJobAsync(job, CancellationToken.None).ConfigureAwait(false);
                            await progress.ReportAsync(job.ProgressPercent, job.Message).ConfigureAwait(false);
                        }).ConfigureAwait(false);

                    operationToken.ThrowIfCancellationRequested();
                    var failed = results.Count(x => x.State == TaskState.Failed);
                    var cancelled = results.Count(x => x.State == TaskState.Cancelled);
                    if (failed > 0 || cancelled > 0)
                    {
                        throw new WorkerOperationException(
                            "BACKUP_ALL_PARTIAL_FAILURE",
                            $"整库备份已处理，但有 {failed} 个游戏失败、{cancelled} 个游戏取消。",
                            string.Join("；", results.Where(x => x.State is TaskState.Failed or TaskState.Cancelled)
                                .Take(8)
                                .Select(x => $"{x.GameName}: {x.DetailMessage}")));
                    }

                    await progress.ReportAsync(100, results.Count == 0 ? "整库备份没有发现可执行的匹配游戏" : "整库备份已完成").ConfigureAwait(false);
                },
                token,
                request.NotificationSessionId,
                job.JobId,
                job.CreatedUtc).ConfigureAwait(false);

            job.State = result.State;
            job.ProgressPercent = result.ProgressPercent;
            job.Message = result.Message;
            job.FinishedUtc = result.FinishedUtc ?? DateTime.UtcNow;
            job.ErrorCode = result.ErrorCode;
            job.ErrorMessage = result.ErrorMessage;
            job.CompletedGameIdsJson = JsonSerializer.Serialize(completedGameIds.OrderBy(x => x, StringComparer.OrdinalIgnoreCase));
            await _store.UpdateBackupAllJobAsync(job, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Background backup-all failed for job {JobId}.", job.JobId);
            job.State = ex is OperationCanceledException ? TaskState.Cancelled : TaskState.Failed;
            job.ProgressPercent = Math.Clamp(job.ProgressPercent, 0, 99);
            job.Message = job.State == TaskState.Cancelled ? "已取消" : "整库备份失败";
            job.FinishedUtc = DateTime.UtcNow;
            if (ex is WorkerOperationException workerError)
            {
                job.ErrorCode = workerError.Code;
                job.ErrorMessage = string.IsNullOrWhiteSpace(workerError.DiagnosticDetail)
                    ? workerError.Message
                    : $"{workerError.Message} | {workerError.DiagnosticDetail}";
            }
            else
            {
                job.ErrorCode = ex.GetType().Name;
                job.ErrorMessage = ex.Message;
            }
            try { await _store.UpdateBackupAllJobAsync(job, CancellationToken.None).ConfigureAwait(false); }
            catch (Exception persistError) { _logger.LogError(persistError, "Could not persist backup-all failure for job {JobId}.", job.JobId); }
        }
        finally
        {
            Interlocked.Exchange(ref _backupAllInFlight, 0);
        }
    }

    private static HashSet<string> DeserializeGameIds(string json)
    {
        try
        {
            var ids = JsonSerializer.Deserialize<List<string>>(json);
            return new HashSet<string>(ids?.Where(x => !string.IsNullOrWhiteSpace(x)) ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static List<GameSaveCenter.Core.Models.FileManifestEntry> DeserializeManifest(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<GameSaveCenter.Core.Models.FileManifestEntry>>(json)
                ?? new List<GameSaveCenter.Core.Models.FileManifestEntry>();
        }
        catch (JsonException)
        {
            return new List<GameSaveCenter.Core.Models.FileManifestEntry>();
        }
    }

    /// <summary>
    /// Repeats only the safe one-way cloud copy after a local backup already succeeded.
    /// It deliberately does not create another Ludusavi version.
    /// </summary>
    public async Task<TaskStatusDto> RetryCloudUploadAsync(string playniteId,CancellationToken token)
    {
        if(_options.SafeModeEnabled)
            throw new WorkerOperationException("SAFE_MODE_ENABLED","安全模式已开启，云端上传已暂停。请先关闭安全模式。","SafeMode");
        var game=await _catalog.GetGameAsync(playniteId,token).ConfigureAwait(false)
                 ??throw new WorkerOperationException("CLOUD_GAME_NOT_FOUND","找不到需要重试云端上传的游戏。",playniteId);
        using var lease = await _gameLock.AcquireAsync(playniteId, GameOperationKind.CloudUpload, TimeSpan.FromSeconds(10), token).ConfigureAwait(false);
        if (lease == null)
            throw new WorkerOperationException("GAME_OPERATION_BUSY","该游戏已有操作正在执行，已跳过云端上传重试。",playniteId);
        var result = await _tasks.RunAsync("CloudUpload",game.PlayniteId,game.Name,async(progress,ct)=>
        {
            if(!_options.EnableCloudUpload||!_rclone.IsConfigured)
                throw new WorkerOperationException("RCLONE_NOT_CONFIGURED","云端复制尚未启用或 Rclone 配置不可用。",_options.RcloneDestination);
            if(!Directory.Exists(_options.LudusaviBackupDirectory))
                throw new WorkerOperationException("BACKUP_DIRECTORY_MISSING","本地 Ludusavi 备份目录不存在，无法重试上传。",_options.LudusaviBackupDirectory);

            await _store.UpdateGameCloudStateAsync(game.PlayniteId,"Pending",ct).ConfigureAwait(false);
            await progress.ReportAsync(10,"正在重新复制本地备份到云端").ConfigureAwait(false);
            var cloud=await _cloudTransfers.RunUploadAsync("backup retry",transferToken=>_rclone.CopyAsync(
                _options.LudusaviBackupDirectory,Path.Combine(_options.DeviceStorageKey,"Saves"),transferToken),ct).ConfigureAwait(false);
            if(!cloud.Success)
            {
                var failure = RcloneFailureClassifier.Classify(cloud.StandardError);
                var errorCode = RcloneFailureClassifier.GetErrorCode(failure);
                await ScheduleCloudRetryAsync(game.PlayniteId,errorCode,cloud.StandardError,ct).ConfigureAwait(false);
                throw new WorkerOperationException(errorCode,$"云端复制重试失败：{RcloneFailureClassifier.GetUserMessage(failure)}",cloud.StandardError);
            }
            await _store.RemoveCloudRetryAsync(game.PlayniteId,ct).ConfigureAwait(false);
            await _store.UpdateGameCloudStateAsync(game.PlayniteId,"Uploaded",ct).ConfigureAwait(false);
            await progress.ReportAsync(100,"云端复制重试完成").ConfigureAwait(false);
        },token).ConfigureAwait(false);
        return result;
    }

    private async Task ScheduleCloudRetryAsync(string playniteId, string errorCode, string error, CancellationToken token)
    {
        var now = DateTime.UtcNow;
        if (!RcloneFailureClassifier.IsRetryable(errorCode))
        {
            await _store.RemoveCloudRetryAsync(playniteId, token).ConfigureAwait(false);
            await _store.UpdateGameCloudStateAsync(playniteId, "Failed", token).ConfigureAwait(false);
            await _store.AppendAuditAsync("CloudRetry", $"云端复制失败（{errorCode}），未安排自动重试", error, token).ConfigureAwait(false);
            return;
        }
        var existing = await _store.GetCloudRetryAsync(playniteId, token).ConfigureAwait(false);
        var completedAutomaticRetries = existing?.RetryCount ?? 0;
        if (CloudRetryPolicy.IsAutomaticRetryLimitReached(completedAutomaticRetries))
        {
            await _store.RemoveCloudRetryAsync(playniteId, token).ConfigureAwait(false);
            await _store.UpdateGameCloudStateAsync(playniteId, "Failed", token).ConfigureAwait(false);
            await _store.AppendAuditAsync("CloudRetry", $"云端复制的 {CloudRetryPolicy.MaximumAutomaticRetries} 次自动重试均失败，已停止自动重试", error, token).ConfigureAwait(false);
            return;
        }

        var retryCount = completedAutomaticRetries + 1;
        var entry = new CloudRetryQueueEntry
        {
            PlayniteId = playniteId, RetryCount = retryCount,
            NextAttemptUtc = CloudRetryPolicy.GetNextAttemptUtc(retryCount, now),
            LastError = error, CreatedUtc = existing?.CreatedUtc ?? now, UpdatedUtc = now
        };
        await _store.UpsertCloudRetryAsync(entry, token).ConfigureAwait(false);
        await _store.UpdateGameCloudStateAsync(playniteId, "RetryScheduled", token).ConfigureAwait(false);
        await _store.AppendAuditAsync("CloudRetry", $"云端复制失败，已排程第 {retryCount} 次自动重试", error, token).ConfigureAwait(false);
    }

    public async Task RefreshBackupHistoryAsync(string playniteId, string ludusaviName, CancellationToken token)
    {
        var listed = await _ludusavi.ListBackupsAsync(new[] { ludusaviName }, token).ConfigureAwait(false);
        if (!listed.Success)
        {
            throw new WorkerOperationException(
                listed.ErrorCode,
                "读取 Ludusavi 历史版本失败：" + listed.ErrorMessage,
                BuildDiagnostic(listed));
        }
        if (!listed.Json.HasValue)
        {
            throw new WorkerOperationException(
                "LUDUSAVI_BACKUPS_EMPTY_RESULT",
                "Ludusavi 没有返回历史版本 JSON。",
                listed.RawOutput);
        }

        var versions = LudusaviResultParser
            .ParseBackupList(listed.Json.Value, playniteId, ludusaviName)
            .ToList();
        var reportedCount = LudusaviResultParser.GetReportedBackupCount(listed.Json.Value, ludusaviName);
        if (reportedCount.GetValueOrDefault() > 0 && versions.Count == 0)
        {
            throw new WorkerOperationException(
                "LUDUSAVI_BACKUP_LIST_PARSE_FAILED",
                "Ludusavi 报告存在历史版本，但 GameSaveCenter 未能解析版本列表。",
                listed.RawOutput);
        }

        await _store.AppendAuditAsync(
            "BackupHistory",
            $"已同步 {ludusaviName} 的 {versions.Count} 个历史版本",
            listed.RawOutput,
            token).ConfigureAwait(false);

        await _store.RemoveMissingBackupVersionsAsync(
            playniteId,
            versions.Select(x => x.BackupId).ToArray(),
            token).ConfigureAwait(false);
        foreach (var version in versions)
        {
            await _store.AddBackupVersionAsync(version, "{}", token).ConfigureAwait(false);
        }
    }

    private static string BuildDiagnostic(LudusaviCommandResult result)
    {
        return JsonSerializer.Serialize(new
        {
            result.ExitCode,
            result.WarningText,
            result.RawOutput
        });
    }

    private static string GetRequestLabel(string? reason) => reason switch
    {
        "DuringPlay" => "游玩中定时备份",
        "GameStopped" => "退出后自动备份",
        "ManualAll" => "全部手动备份",
        "Manual" => "手动备份",
        _ => "备份"
    };
}
