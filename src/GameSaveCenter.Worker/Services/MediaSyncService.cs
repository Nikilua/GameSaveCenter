using System;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Services;
using GameSaveCenter.Worker.Configuration;
using GameSaveCenter.Worker.Infrastructure;
using GameSaveCenter.Worker.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace GameSaveCenter.Worker.Services;

/// <summary>
/// Incrementally copies screenshots and clips into a stable archive. Files are deduplicated
/// by SHA-256 and source deletion never removes the archive copy. Shared capture folders are
/// scanned once and ambiguous items are preserved in a manual classification inbox.
/// </summary>
public sealed class MediaSyncService
{
    private const int MediaSignatureSampleBytes = 4096;
    private static readonly TimeSpan MediaSignatureRetention = TimeSpan.FromDays(30);
    private const int MaximumMediaSignatures = 100_000;
    private static readonly HashSet<string> ImageExtensions=new(StringComparer.OrdinalIgnoreCase){".png",".jpg",".jpeg",".webp",".bmp"};
    private static readonly HashSet<string> VideoExtensions=new(StringComparer.OrdinalIgnoreCase){".mp4",".mkv",".mov",".webm",".avi"};
    private readonly WorkerOptions _options;
    private readonly GameCatalogService _catalog;
    private readonly SqliteStateStore _store;
    private readonly RcloneClient _rclone;
    private readonly CloudTransferCoordinator _cloudTransfers;
    private readonly CloudTransferStateService _cloudState;
    private readonly TaskCoordinator _tasks;
    private readonly GameOperationLock _gameLock;
    private readonly ILogger<MediaSyncService> _logger;

    public MediaSyncService(WorkerOptions options,GameCatalogService catalog,SqliteStateStore store,RcloneClient rclone,CloudTransferCoordinator cloudTransfers,TaskCoordinator tasks,GameOperationLock gameLock,ILogger<MediaSyncService> logger)
        : this(options,catalog,store,rclone,cloudTransfers,new CloudTransferStateService(store,options,rclone,cloudTransfers,NullLogger<CloudTransferStateService>.Instance),tasks,gameLock,logger)
    {
    }

    public MediaSyncService(WorkerOptions options,GameCatalogService catalog,SqliteStateStore store,RcloneClient rclone,CloudTransferCoordinator cloudTransfers,CloudTransferStateService cloudState,TaskCoordinator tasks,GameOperationLock gameLock,ILogger<MediaSyncService> logger)
    { _options=options;_catalog=catalog;_store=store;_rclone=rclone;_cloudTransfers=cloudTransfers;_cloudState=cloudState;_tasks=tasks;_gameLock=gameLock;_logger=logger; }

    public async Task<List<TaskStatusDto>> SyncAsync(MediaSyncRequestDto request,CancellationToken token)
    {
        if(!_options.EnableMediaSync) return new List<TaskStatusDto>();
        await _store.PruneMediaFileSignaturesAsync(DateTime.UtcNow.Subtract(MediaSignatureRetention), MaximumMediaSignatures, token).ConfigureAwait(false);
        var allGames=await _catalog.GetGamesAsync(token).ConfigureAwait(false);
        var selectedGames=allGames;
        if(request.PlayniteIds.Count>0)
            selectedGames=allGames.Where(x=>request.PlayniteIds.Contains(x.PlayniteId,StringComparer.OrdinalIgnoreCase)).ToList();

        var output=new List<TaskStatusDto>();
        if(!request.SharedOnly)
            foreach(var game in selectedGames)
            {
                using var lease = await _gameLock.AcquireAsync(game.PlayniteId, GameOperationKind.Media, TimeSpan.FromSeconds(10), token).ConfigureAwait(false);
                if (lease == null)
                {
                    output.Add(await _tasks.RunAsync("MediaSync",game.PlayniteId,game.Name,
                        (_, _) => Task.FromException(new WorkerOperationException("GAME_OPERATION_BUSY","该游戏已有备份、恢复或媒体操作正在执行，已跳过本次媒体同步。",game.PlayniteId)),
                        token,request.NotificationSessionId).ConfigureAwait(false));
                    continue;
                }
                output.Add(await SyncGameSourcesAsync(game,request,token).ConfigureAwait(false));
            }

        if(request.IncludeUnassignedInbox)
            output.Add(await SyncSharedSourcesAsync(allGames,request,token).ConfigureAwait(false));
        return output;
    }

    /// <summary>Moves either an inbox item or an already classified item to the selected game archive.</summary>
    public async Task<MediaItemDto> ReassignAsync(ReassignMediaRequestDto request,CancellationToken token)
    {
        if(string.IsNullOrWhiteSpace(request.MediaId)||string.IsNullOrWhiteSpace(request.TargetPlayniteId))
            throw new InvalidOperationException("Media and target game are required.");
        var item=await _store.GetMediaByIdAsync(request.MediaId,token).ConfigureAwait(false)
                 ??throw new InvalidOperationException("媒体记录不存在或已经被清理。");
        var game=await _store.GetGameAsync(request.TargetPlayniteId,token).ConfigureAwait(false)
                 ??throw new InvalidOperationException("目标游戏不存在于当前 Playnite 游戏库。");
        var extension=Path.GetExtension(File.Exists(item.ArchivePath)?item.ArchivePath:item.OriginalPath);
        var destination=BuildArchivePath(game,item.Source,item.Kind,item.CapturedUtc,item.Sha256,extension);
        await RelocateArchivedCopyAsync(item,destination,token).ConfigureAwait(false);
        await _store.AssignMediaAsync(item.MediaId,game.PlayniteId,destination,token).ConfigureAwait(false);
        await _store.AppendAuditAsync("Media","媒体已人工归类",JsonSerializer.Serialize(new{item.MediaId,game.PlayniteId,game.Name,item.OriginalPath,destination}),token).ConfigureAwait(false);
        item.PlayniteId=game.PlayniteId;
        item.ArchivePath=destination;
        item.ClassificationState="Assigned";
        item.ClassificationReason=string.Empty;
        item.CloudState="Pending";
        return item;
    }

    /// <summary>Removes an item from the inbox while retaining a recoverable local copy.</summary>
    public async Task<MediaItemDto> IgnoreAsync(IgnoreMediaRequestDto request,CancellationToken token)
    {
        if(string.IsNullOrWhiteSpace(request.MediaId))throw new InvalidOperationException("Media is required.");
        var item=await _store.GetMediaByIdAsync(request.MediaId,token).ConfigureAwait(false)
                 ??throw new InvalidOperationException("媒体记录不存在或已经被清理。");
        if(!string.Equals(item.ClassificationState,"Inbox",StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("只有待归类收件箱中的媒体可以忽略。");
        var extension=Path.GetExtension(File.Exists(item.ArchivePath)?item.ArchivePath:item.OriginalPath);
        var destination=BuildIgnoredArchivePath(item,extension);
        await RelocateArchivedCopyAsync(item,destination,token).ConfigureAwait(false);
        await _store.IgnoreMediaAsync(item.MediaId,destination,token).ConfigureAwait(false);
        await _store.AppendAuditAsync("Media","媒体收件箱项目已忽略",JsonSerializer.Serialize(new{item.MediaId,item.OriginalPath,destination}),token).ConfigureAwait(false);
        item.ArchivePath=destination;
        item.ClassificationState="Ignored";
        item.ClassificationReason="用户已忽略";
        item.CloudState="NotApplicable";
        return item;
    }

    /// <summary>
    /// Processes a bounded inbox batch without turning each selected row into a separate
    /// Playnite-to-Worker round trip. Individual items remain best-effort because moving a
    /// file and updating its index cannot be made one SQLite transaction.
    /// </summary>
    public async Task<MediaInboxBatchResultDto> ReassignBatchAsync(MediaInboxBatchRequestDto request,CancellationToken token)
    {
        var mediaIds=NormalizeInboxBatchIds(request.MediaIds);
        if(mediaIds.Count==0)throw new InvalidOperationException("必须选择至少一个待归类媒体。");
        if(mediaIds.Count>500)throw new InvalidOperationException("单次最多批量处理 500 个媒体文件。");
        if(string.IsNullOrWhiteSpace(request.TargetPlayniteId))throw new InvalidOperationException("必须选择目标游戏。");

        var result=new MediaInboxBatchResultDto();
        foreach(var mediaId in mediaIds)
        {
            try
            {
                result.UpdatedItems.Add(await ReassignAsync(new ReassignMediaRequestDto
                {
                    MediaId=mediaId,
                    TargetPlayniteId=request.TargetPlayniteId
                },token).ConfigureAwait(false));
            }
            catch(OperationCanceledException)
            {
                throw;
            }
            catch(Exception ex)
            {
                result.Failures.Add(new MediaInboxBatchFailureDto { MediaId=mediaId, ErrorMessage=ex.Message });
            }
        }
        return result;
    }

    /// <summary>Ignores a bounded inbox batch while retaining each archive copy.</summary>
    public async Task<MediaInboxBatchResultDto> IgnoreBatchAsync(MediaInboxBatchRequestDto request,CancellationToken token)
    {
        var mediaIds=NormalizeInboxBatchIds(request.MediaIds);
        if(mediaIds.Count==0)throw new InvalidOperationException("必须选择至少一个待归类媒体。");
        if(mediaIds.Count>500)throw new InvalidOperationException("单次最多批量处理 500 个媒体文件。");

        var result=new MediaInboxBatchResultDto();
        foreach(var mediaId in mediaIds)
        {
            try
            {
                result.UpdatedItems.Add(await IgnoreAsync(new IgnoreMediaRequestDto { MediaId=mediaId },token).ConfigureAwait(false));
            }
            catch(OperationCanceledException)
            {
                throw;
            }
            catch(Exception ex)
            {
                result.Failures.Add(new MediaInboxBatchFailureDto { MediaId=mediaId, ErrorMessage=ex.Message });
            }
        }
        return result;
    }

    /// <summary>Restores a bounded ignored-media batch to the manual classification inbox.</summary>
    public async Task<MediaInboxBatchResultDto> RestoreIgnoredBatchAsync(MediaInboxBatchRequestDto request,CancellationToken token)
    {
        var mediaIds=NormalizeInboxBatchIds(request.MediaIds);
        if(mediaIds.Count==0)throw new InvalidOperationException("必须选择至少一个已忽略媒体。");
        if(mediaIds.Count>500)throw new InvalidOperationException("单次最多批量处理 500 个媒体文件。");

        var result=new MediaInboxBatchResultDto();
        foreach(var mediaId in mediaIds)
        {
            try
            {
                result.UpdatedItems.Add(await RestoreIgnoredAsync(mediaId,token).ConfigureAwait(false));
            }
            catch(OperationCanceledException)
            {
                throw;
            }
            catch(Exception ex)
            {
                result.Failures.Add(new MediaInboxBatchFailureDto { MediaId=mediaId, ErrorMessage=ex.Message });
            }
        }
        return result;
    }

    /// <summary>
    /// Creates an expiring, Worker-owned classification preview. Suggestions are conservative:
    /// only a unique source/process signal is high confidence; ambiguous candidates stay in
    /// the inbox and are never applied by the batch command.
    /// </summary>
    public async Task<MediaClassificationPreviewDto> CreateClassificationPreviewAsync(MediaClassificationPreviewRequestDto request, CancellationToken token)
    {
        var mediaIds = NormalizeInboxBatchIds(request.MediaIds);
        var limit = Math.Clamp(request.Limit, 1, 200);
        var media = new List<MediaItemDto>();
        if (mediaIds.Count > 0)
        {
            foreach (var mediaId in mediaIds.Take(500))
            {
                var item = await _store.GetMediaByIdAsync(mediaId, token).ConfigureAwait(false);
                if (item != null && string.Equals(item.ClassificationState, "Inbox", StringComparison.OrdinalIgnoreCase)) media.Add(item);
            }
        }
        else
        {
            media.AddRange(await _store.GetUnassignedMediaAsync(limit, token).ConfigureAwait(false));
        }
        media = media.Take(limit).ToList();

        var batchId = Guid.NewGuid().ToString("N");
        var now = DateTime.UtcNow;
        var preview = new MediaClassificationPreviewDto { BatchId = batchId, CreatedUtc = now, ExpiresUtc = now.AddMinutes(30) };
        if (media.Count == 0)
        {
            await _store.CreateMediaClassificationBatchAsync(batchId, now, preview.ExpiresUtc,
                Array.Empty<MediaClassificationBatchItemRecord>(), token).ConfigureAwait(false);
            return preview;
        }

        var games = await _catalog.GetGamesAsync(token).ConfigureAwait(false);
        var sources = await _store.GetEnabledMediaSourcesForClassificationAsync(token).ConfigureAwait(false);
        var knownCaptures = media.Where(x => x.CapturedUtc != default).Select(x => x.CapturedUtc).ToList();
        var from = knownCaptures.Count == 0 ? DateTime.UtcNow : knownCaptures.Min().AddMinutes(-15);
        var to = knownCaptures.Count == 0 ? DateTime.UtcNow : knownCaptures.Max().AddMinutes(15);
        var sessions = await _store.GetSessionsForMediaClassificationAsync(from, to, token).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(request.SessionId))
        {
            var requestedSession = await _store.GetSessionAsync(request.SessionId, token).ConfigureAwait(false);
            sessions = requestedSession == null ? new List<GameSessionEventDto>() : new List<GameSessionEventDto> { requestedSession };
        }
        var mappings = await _store.GetProcessMappingsAsync(token).ConfigureAwait(false);
        var gameById = games.ToDictionary(x => x.PlayniteId, StringComparer.OrdinalIgnoreCase);
        var suggestions = media.Select(item => BuildClassificationSuggestion(item, games, gameById, sources, sessions, mappings)).ToList();
        preview.Items = suggestions;
        preview.HighConfidenceCount = suggestions.Count(x => x.Confidence == "High");
        preview.MediumConfidenceCount = suggestions.Count(x => x.Confidence == "Medium");
        preview.LowConfidenceCount = suggestions.Count(x => x.Confidence == "Low");

        var records = media.Zip(suggestions, (item, suggestion) => new MediaClassificationBatchItemRecord
        {
            BatchId = batchId, MediaId = item.MediaId, OriginalPlayniteId = item.PlayniteId ?? string.Empty,
            OriginalClassificationState = item.ClassificationState ?? "Inbox", OriginalClassificationReason = item.ClassificationReason ?? string.Empty,
            OriginalArchivePath = item.ArchivePath, OriginalPath = item.OriginalPath, OriginalCapturedUtc = item.CapturedUtc,
            OriginalSizeBytes = item.SizeBytes, OriginalSha256 = item.Sha256, OriginalIsFavorite = item.IsFavorite,
            OriginalComment = item.Comment ?? string.Empty, OriginalCloudState = item.CloudState ?? "NotApplicable",
            TargetPlayniteId = suggestion.SuggestedPlayniteId, TargetReason = suggestion.Reason, Confidence = suggestion.Confidence,
            UpdatedUtc = now
        }).ToList();
        await _store.CreateMediaClassificationBatchAsync(batchId, now, preview.ExpiresUtc, records, token).ConfigureAwait(false);
        return preview;
    }

    /// <summary>Applies only explicitly confirmable high-confidence suggestions and keeps per-item conflicts.</summary>
    public async Task<MediaClassificationBatchResultDto> ApplyClassificationPreviewAsync(MediaClassificationApplyRequestDto request, CancellationToken token)
    {
        var batch = await GetLiveClassificationBatchAsync(request.BatchId, token).ConfigureAwait(false);
        var records = await _store.GetMediaClassificationBatchItemsAsync(batch.BatchId, token).ConfigureAwait(false);
        var selected = NormalizeInboxBatchIds(request.MediaIds);
        var games = (await _catalog.GetGamesAsync(token).ConfigureAwait(false)).ToDictionary(x => x.PlayniteId, StringComparer.OrdinalIgnoreCase);
        var result = new MediaClassificationBatchResultDto { BatchId = batch.BatchId };

        foreach (var record in records)
        {
            if (selected.Count > 0 && !selected.Contains(record.MediaId, StringComparer.OrdinalIgnoreCase)) continue;
            if (record.ItemState == "Applied")
            {
                AddClassificationResult(result, record.MediaId, "Skipped", "该建议批次项目已经应用。", skipped: true);
                continue;
            }
            if (request.HighConfidenceOnly && record.Confidence != "High")
            {
                AddClassificationResult(result, record.MediaId, "Skipped", "低于高置信门槛，保持未归类。", skipped: true);
                await _store.UpdateMediaClassificationBatchItemAsync(batch.BatchId, record.MediaId, "Skipped", string.Empty, token).ConfigureAwait(false);
                continue;
            }
            if (string.IsNullOrWhiteSpace(record.TargetPlayniteId) || !games.TryGetValue(record.TargetPlayniteId, out var game))
            {
                AddClassificationResult(result, record.MediaId, "Skipped", "没有可确认的唯一目标游戏。", skipped: true);
                await _store.UpdateMediaClassificationBatchItemAsync(batch.BatchId, record.MediaId, "Skipped", string.Empty, token).ConfigureAwait(false);
                continue;
            }

            var current = await _store.GetMediaByIdAsync(record.MediaId, token).ConfigureAwait(false);
            if (current == null || !MatchesOriginalClassification(current, record))
            {
                AddClassificationResult(result, record.MediaId, "Conflict", "预览后媒体记录已变化，未移动或覆盖。", conflict: true);
                await _store.UpdateMediaClassificationBatchItemAsync(batch.BatchId, record.MediaId, "Conflict", string.Empty, token).ConfigureAwait(false);
                continue;
            }

            var extension = Path.GetExtension(File.Exists(current.ArchivePath) ? current.ArchivePath : current.OriginalPath);
            var destination = BuildArchivePath(game, current.Source, current.Kind, current.CapturedUtc, current.Sha256, extension);
            var moved = false;
            try
            {
                await RelocateArchivedCopyAsync(current, destination, token).ConfigureAwait(false);
                moved = true;
                if (!await _store.TryApplyMediaClassificationAsync(record, destination, token).ConfigureAwait(false))
                {
                    await RestoreMovedClassificationCopyAsync(current, destination, token).ConfigureAwait(false);
                    AddClassificationResult(result, record.MediaId, "Conflict", "媒体在应用前后发生变化，已恢复原归档路径。", conflict: true);
                    await _store.UpdateMediaClassificationBatchItemAsync(batch.BatchId, record.MediaId, "Conflict", string.Empty, token).ConfigureAwait(false);
                    continue;
                }
                await _store.UpdateMediaClassificationBatchItemAsync(batch.BatchId, record.MediaId, "Applied", destination, token).ConfigureAwait(false);
                await _store.AppendAuditAsync("Media", "已应用媒体归类建议", JsonSerializer.Serialize(new { record.MediaId, record.TargetPlayniteId, destination, record.TargetReason }), token).ConfigureAwait(false);
                AddClassificationResult(result, record.MediaId, "Applied", $"已归类到 {game.Name}。", applied: true);
            }
            catch (OperationCanceledException)
            {
                if (moved) await RestoreMovedClassificationCopyAsync(current, destination, token).ConfigureAwait(false);
                throw;
            }
            catch (Exception ex)
            {
                if (moved)
                {
                    try { await RestoreMovedClassificationCopyAsync(current, destination, token).ConfigureAwait(false); }
                    catch (Exception rollback) { _logger.LogError(rollback, "Could not roll back classification suggestion move for {MediaId}", record.MediaId); }
                }
                AddClassificationResult(result, record.MediaId, "Conflict", $"归类未完成，原副本保持不变：{ex.Message}", conflict: true);
                await _store.UpdateMediaClassificationBatchItemAsync(batch.BatchId, record.MediaId, "Conflict", string.Empty, token).ConfigureAwait(false);
            }
        }

        result.State = result.AppliedCount == 0
            ? result.ConflictCount > 0 ? "Conflict" : "Preview"
            : result.ConflictCount > 0 ? "AppliedWithConflicts" : "Applied";
        await _store.UpdateMediaClassificationBatchStateAsync(batch.BatchId, result.State, string.Empty, token).ConfigureAwait(false);
        return result;
    }

    /// <summary>Undoes only items that still match the applied snapshot; changed items become conflicts.</summary>
    public async Task<MediaClassificationBatchResultDto> UndoClassificationBatchAsync(MediaClassificationUndoRequestDto request, CancellationToken token)
    {
        var batch = await GetLiveClassificationBatchAsync(request.BatchId, token, forUndo: true).ConfigureAwait(false);
        var records = await _store.GetMediaClassificationBatchItemsAsync(batch.BatchId, token).ConfigureAwait(false);
        var result = new MediaClassificationBatchResultDto { BatchId = batch.BatchId };
        foreach (var record in records.Where(x => x.ItemState == "Applied"))
        {
            var current = await _store.GetMediaByIdAsync(record.MediaId, token).ConfigureAwait(false);
            if (current == null || !MatchesAppliedClassification(current, record))
            {
                AddClassificationResult(result, record.MediaId, "Conflict", "应用后媒体已被再次修改，未撤销。", conflict: true);
                await _store.UpdateMediaClassificationBatchItemAsync(batch.BatchId, record.MediaId, "Conflict", record.AppliedArchivePath, token).ConfigureAwait(false);
                continue;
            }

            var moved = false;
            try
            {
                await RelocateArchivedCopyAsync(current, record.OriginalArchivePath, token).ConfigureAwait(false);
                moved = true;
                if (!await _store.TryUndoMediaClassificationAsync(record, token).ConfigureAwait(false))
                {
                    await RestoreMovedClassificationCopyAsync(current, record.OriginalArchivePath, token, record.AppliedArchivePath).ConfigureAwait(false);
                    AddClassificationResult(result, record.MediaId, "Conflict", "撤销时媒体状态已变化，已恢复应用后的归档路径。", conflict: true);
                    await _store.UpdateMediaClassificationBatchItemAsync(batch.BatchId, record.MediaId, "Conflict", record.AppliedArchivePath, token).ConfigureAwait(false);
                    continue;
                }
                await _store.UpdateMediaClassificationBatchItemAsync(batch.BatchId, record.MediaId, "Undone", string.Empty, token).ConfigureAwait(false);
                await _store.AppendAuditAsync("Media", "已撤销媒体归类建议", JsonSerializer.Serialize(new { record.MediaId, record.BatchId }), token).ConfigureAwait(false);
                AddClassificationResult(result, record.MediaId, "Undone", "已恢复到待归类状态。", undone: true);
            }
            catch (OperationCanceledException)
            {
                if (moved) await RestoreMovedClassificationCopyAsync(current, record.OriginalArchivePath, token, record.AppliedArchivePath).ConfigureAwait(false);
                throw;
            }
            catch (Exception ex)
            {
                if (moved)
                {
                    try { await RestoreMovedClassificationCopyAsync(current, record.OriginalArchivePath, token, record.AppliedArchivePath).ConfigureAwait(false); }
                    catch (Exception rollback) { _logger.LogError(rollback, "Could not restore applied classification copy for {MediaId}", record.MediaId); }
                }
                AddClassificationResult(result, record.MediaId, "Conflict", $"撤销未完成，未覆盖当前文件：{ex.Message}", conflict: true);
                await _store.UpdateMediaClassificationBatchItemAsync(batch.BatchId, record.MediaId, "Conflict", record.AppliedArchivePath, token).ConfigureAwait(false);
            }
        }

        result.State = result.ConflictCount > 0 ? "UndoneWithConflicts" : "Undone";
        await _store.UpdateMediaClassificationBatchStateAsync(batch.BatchId, result.State, string.Empty, token).ConfigureAwait(false);
        return result;
    }

    private async Task<MediaItemDto> RestoreIgnoredAsync(string mediaId,CancellationToken token)
    {
        var item=await _store.GetMediaByIdAsync(mediaId,token).ConfigureAwait(false)
                 ??throw new InvalidOperationException("媒体记录不存在或已经被清理。");
        if(!string.Equals(item.ClassificationState,"Ignored",StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("只有已忽略媒体可以恢复到待归类收件箱。");

        var extension=Path.GetExtension(File.Exists(item.ArchivePath)?item.ArchivePath:item.OriginalPath);
        var destination=BuildInboxArchivePath(item.Source,item.Kind,item.CapturedUtc,item.Sha256,extension);
        await RelocateArchivedCopyAsync(item,destination,token).ConfigureAwait(false);
        await _store.RestoreMediaToInboxAsync(item.MediaId,destination,token).ConfigureAwait(false);
        await _store.AppendAuditAsync("Media","已忽略媒体恢复到待归类收件箱",JsonSerializer.Serialize(new{item.MediaId,item.OriginalPath,destination}),token).ConfigureAwait(false);
        item.PlayniteId=string.Empty;
        item.ArchivePath=destination;
        item.ClassificationState="Inbox";
        item.ClassificationReason="用户撤销忽略，待重新归类";
        item.CloudState="NotApplicable";
        return item;
    }

    private static List<string> NormalizeInboxBatchIds(IEnumerable<string>? mediaIds)
        =>(mediaIds??Array.Empty<string>())
            .Where(x=>!string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static MediaClassificationSuggestionDto BuildClassificationSuggestion(MediaItemDto item,
        IReadOnlyList<GameDescriptorDto> games,
        IReadOnlyDictionary<string, GameDescriptorDto> gameById,
        IReadOnlyList<MediaSourceRuleDto> sources,
        IReadOnlyList<GameSessionEventDto> sessions,
        IReadOnlyList<ProcessMappingDto> mappings)
    {
        var candidates = new Dictionary<string, (int Rank, List<string> Reasons)>(StringComparer.OrdinalIgnoreCase);

        void AddCandidate(string playniteId, int rank, string reason)
        {
            if (!gameById.ContainsKey(playniteId)) return;
            if (candidates.TryGetValue(playniteId, out var candidate))
            {
                if (!candidate.Reasons.Contains(reason, StringComparer.Ordinal)) candidate.Reasons.Add(reason);
                candidates[playniteId] = (Math.Max(candidate.Rank, rank), candidate.Reasons);
                return;
            }
            candidates[playniteId] = (rank, new List<string> { reason });
        }

        foreach (var source in sources.Where(x => x.Enabled && !string.IsNullOrWhiteSpace(x.PlayniteId)))
        {
            if (IsPathWithin(item.OriginalPath, source.RootPath)
                && MatchesIncludePattern(item.OriginalPath, source.IncludePattern))
            {
                AddCandidate(source.PlayniteId, 3, "命中游戏媒体来源规则");
            }
        }

        var captureKnown = item.CapturedUtc != default;
        var matchingSessions = captureKnown
            ? sessions.Where(x => IsCapturedWithinSession(item.CapturedUtc, x)).ToList()
            : new List<GameSessionEventDto>();
        foreach (var sessionGame in matchingSessions.Select(x => x.PlayniteId)
                     .Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            AddCandidate(sessionGame, 2, matchingSessions.Count(x => string.Equals(x.PlayniteId, sessionGame, StringComparison.OrdinalIgnoreCase)) > 1
                ? "媒体时间命中重叠的同一游戏会话"
                : "媒体时间位于游戏会话窗口");
        }

        foreach (var session in matchingSessions)
        {
            var processName = NormalizeProcessName(session.ProcessName);
            if (string.IsNullOrWhiteSpace(processName)) continue;
            foreach (var mapping in mappings.Where(x => x.Enabled && !string.IsNullOrWhiteSpace(x.PlayniteId)
                                                         && string.Equals(NormalizeProcessName(x.ExecutableName), processName, StringComparison.OrdinalIgnoreCase)))
            {
                AddCandidate(mapping.PlayniteId, 3, "会话进程映射与媒体时间一致");
            }
        }

        var nameMatches = games.Where(x => !string.IsNullOrWhiteSpace(x.Name)
                                           && SharedFileMatchesGame(item.OriginalPath, x.Name)).ToList();
        foreach (var game in nameMatches)
        {
            AddCandidate(game.PlayniteId, 2, nameMatches.Count == 1 ? "文件名唯一匹配游戏" : "文件名匹配多个候选游戏");
        }

        var suggestion = new MediaClassificationSuggestionDto
        {
            MediaId = item.MediaId,
            FileName = item.FileName,
            CapturedUtc = item.CapturedUtc,
            Confidence = "Low",
            State = "Suggested"
        };

        if (candidates.Count == 1)
        {
            var candidate = candidates.Single();
            var game = gameById[candidate.Key];
            suggestion.SuggestedPlayniteId = game.PlayniteId;
            suggestion.SuggestedGameName = game.Name;
            suggestion.Confidence = candidate.Value.Rank >= 3 ? "High" : "Medium";
            suggestion.Reason = string.Join("；", candidate.Value.Reasons);
        }
        else if (candidates.Count > 1)
        {
            suggestion.Reason = "来源规则、会话或文件名产生多个候选，保持未归类";
        }
        else
        {
            suggestion.Reason = captureKnown ? "没有足够的来源、会话或文件名证据，保持未归类" : "媒体时间未知，无法安全匹配会话，保持未归类";
        }

        return suggestion;
    }

    private async Task<MediaClassificationBatchRecord> GetLiveClassificationBatchAsync(string batchId, CancellationToken token, bool forUndo = false)
    {
        if (string.IsNullOrWhiteSpace(batchId)) throw new InvalidOperationException("缺少媒体归类建议批次号。");
        var batch = await _store.GetMediaClassificationBatchAsync(batchId, token).ConfigureAwait(false)
                    ?? throw new InvalidOperationException("媒体归类建议批次不存在，可能已被清理。");
        if (forUndo)
        {
            if (batch.State is not ("Applied" or "AppliedWithConflicts"))
                throw new InvalidOperationException("只有已应用的媒体归类建议批次可以撤销。");
            return batch;
        }

        if (!string.Equals(batch.State, "Preview", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("该媒体归类建议批次已经处理，不能重复应用。");
        if (DateTime.UtcNow > batch.ExpiresUtc)
        {
            await _store.UpdateMediaClassificationBatchStateAsync(batch.BatchId, "Expired", "预览已过期，请重新生成建议。", token).ConfigureAwait(false);
            throw new InvalidOperationException("媒体归类预览已过期，请重新生成建议。");
        }
        return batch;
    }

    private static bool MatchesOriginalClassification(MediaItemDto current, MediaClassificationBatchItemRecord record)
        => string.Equals(current.PlayniteId ?? string.Empty, record.OriginalPlayniteId, StringComparison.Ordinal)
           && string.Equals(current.ClassificationState ?? string.Empty, record.OriginalClassificationState, StringComparison.Ordinal)
           && string.Equals(current.ClassificationReason ?? string.Empty, record.OriginalClassificationReason, StringComparison.Ordinal)
           && string.Equals(current.ArchivePath ?? string.Empty, record.OriginalArchivePath ?? string.Empty, StringComparison.OrdinalIgnoreCase)
           && string.Equals(current.OriginalPath ?? string.Empty, record.OriginalPath ?? string.Empty, StringComparison.OrdinalIgnoreCase)
           && current.CapturedUtc.ToUniversalTime() == record.OriginalCapturedUtc.ToUniversalTime()
           && current.SizeBytes == record.OriginalSizeBytes
           && string.Equals(current.Sha256 ?? string.Empty, record.OriginalSha256, StringComparison.OrdinalIgnoreCase)
           && current.IsFavorite == record.OriginalIsFavorite
           && string.Equals(current.Comment ?? string.Empty, record.OriginalComment, StringComparison.Ordinal)
           && string.Equals(current.CloudState ?? string.Empty, record.OriginalCloudState, StringComparison.OrdinalIgnoreCase);

    private static bool MatchesAppliedClassification(MediaItemDto current, MediaClassificationBatchItemRecord record)
        => string.Equals(current.PlayniteId ?? string.Empty, record.TargetPlayniteId, StringComparison.Ordinal)
           && string.Equals(current.ClassificationState ?? string.Empty, "Assigned", StringComparison.OrdinalIgnoreCase)
           && string.Equals(current.ClassificationReason ?? string.Empty, record.TargetReason, StringComparison.Ordinal)
           && string.Equals(current.ArchivePath ?? string.Empty, record.AppliedArchivePath ?? string.Empty, StringComparison.OrdinalIgnoreCase)
           && string.Equals(current.OriginalPath ?? string.Empty, record.OriginalPath ?? string.Empty, StringComparison.OrdinalIgnoreCase)
           && current.CapturedUtc.ToUniversalTime() == record.OriginalCapturedUtc.ToUniversalTime()
           && current.SizeBytes == record.OriginalSizeBytes
           && string.Equals(current.Sha256 ?? string.Empty, record.OriginalSha256, StringComparison.OrdinalIgnoreCase)
           && current.IsFavorite == record.OriginalIsFavorite
           && string.Equals(current.Comment ?? string.Empty, record.OriginalComment, StringComparison.Ordinal)
           && string.Equals(current.CloudState ?? string.Empty, "Pending", StringComparison.OrdinalIgnoreCase);

    private static Task RestoreMovedClassificationCopyAsync(MediaItemDto item, string movedPath, CancellationToken token, string? destination = null)
    {
        var restoreSource = new MediaItemDto
        {
            ArchivePath = movedPath,
            OriginalPath = item.OriginalPath,
            Sha256 = item.Sha256
        };
        return RelocateArchivedCopyAsync(restoreSource, destination ?? item.ArchivePath, token);
    }

    private static void AddClassificationResult(MediaClassificationBatchResultDto result, string mediaId, string state,
        string message, bool applied = false, bool undone = false, bool conflict = false, bool skipped = false)
    {
        result.Items.Add(new MediaClassificationBatchItemResultDto { MediaId = mediaId, State = state, Message = message });
        if (applied) result.AppliedCount++;
        if (undone) result.UndoneCount++;
        if (conflict) result.ConflictCount++;
        if (skipped) result.SkippedCount++;
    }

    private static bool IsCapturedWithinSession(DateTime capturedUtc, GameSessionEventDto session)
    {
        if (capturedUtc == default || session.StartedUtc == default) return false;
        var start = session.StartedUtc.ToUniversalTime().AddMinutes(-2);
        var stop = (session.StoppedUtc ?? DateTime.UtcNow).ToUniversalTime().AddMinutes(10);
        return capturedUtc.ToUniversalTime() >= start && capturedUtc.ToUniversalTime() <= stop;
    }

    private static bool IsPathWithin(string path, string root)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(root)) return false;
        try
        {
            var fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return string.Equals(fullPath, fullRoot, StringComparison.OrdinalIgnoreCase)
                   || fullPath.StartsWith(fullRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                   || fullPath.StartsWith(fullRoot + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception) { return false; }
    }

    private static bool MatchesIncludePattern(string path, string pattern)
    {
        var fileName = Path.GetFileName(path);
        var patterns = (pattern ?? "*").Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (patterns.Length == 0) return true;
        return patterns.Any(value => string.Equals(value, "*", StringComparison.Ordinal)
                                     || Regex.IsMatch(fileName, "^" + Regex.Escape(value).Replace("\\*", ".*").Replace("\\?", ".") + "$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
    }

    private static string NormalizeProcessName(string value)
        => Path.GetFileName(value ?? string.Empty).Trim();

    private Task<TaskStatusDto> SyncGameSourcesAsync(GameDescriptorDto game,MediaSyncRequestDto request,CancellationToken token)=>
        _tasks.RunAsync("MediaSync",game.PlayniteId,game.Name,async(progress,ct)=>
        {
            await progress.ReportAsync(5,"正在查找游戏专属媒体来源").ConfigureAwait(false);
            var sources=(await DiscoverGameSourcesAsync(game,ct).ConfigureAwait(false))
                .DistinctBy(x=>$"{x.Path}|{x.IncludePattern}",StringComparer.OrdinalIgnoreCase)
                .Where(x=>Directory.Exists(x.Path)).ToList();
            var candidates=EnumerateCandidates(sources);
            var copied=0;var index=0;
            foreach(var candidate in candidates.OrderBy(x=>x.Path,StringComparer.OrdinalIgnoreCase))
            {
                ct.ThrowIfCancellationRequested();index++;
                if(await ArchiveCandidateAsync(candidate.Path,candidate.Source,game,"游戏专属来源",ct).ConfigureAwait(false))copied++;
                if(index%20==0)await progress.ReportAsync(Math.Min(85,5+(int)(80d*index/Math.Max(1,candidates.Count))),$"已检查 {index}/{candidates.Count}").ConfigureAwait(false);
            }

            var policy=await _store.GetPolicyAsync(game.PlayniteId,ct).ConfigureAwait(false);
            var pendingCloudGames = await _store.GetMediaGamesNeedingCloudUploadAsync(ct).ConfigureAwait(false);
            var shouldUpload = copied > 0 || pendingCloudGames.Contains(game.PlayniteId, StringComparer.OrdinalIgnoreCase);
            if(!_options.SafeModeEnabled&&_options.EnableCloudUpload&&(request.UploadAfterSync||policy.UploadAfterBackup)&&shouldUpload&&_rclone.IsConfigured)
            {
                if(copied>0) await _cloudState.StartNewAsync(CloudTransferKind.Media,game.PlayniteId,ct).ConfigureAwait(false);
                await _store.UpdateMediaCloudStateAsync(game.PlayniteId,"Pending",ct).ConfigureAwait(false);
                await _cloudState.MarkTransferringAsync(CloudTransferKind.Media,game.PlayniteId,ct).ConfigureAwait(false);
                await progress.ReportAsync(90,"正在复制媒体到云端").ConfigureAwait(false);
                var gameDirectory=Path.Combine(_options.MediaArchiveDirectory,Sanitize(game.Name));
                var remote=Path.Combine(Environment.MachineName,"Media",Sanitize(game.Name));
                var cloud=await _cloudTransfers.RunUploadAsync("media",transferToken=>_rclone.CopyAsync(gameDirectory,remote,transferToken),ct,
                    CloudTransferStateService.GetTransferKey(CloudTransferKind.Media,game.PlayniteId)).ConfigureAwait(false);
                if(!cloud.Success)
                {
                    var failure=RcloneFailureClassifier.Classify(cloud.StandardError);
                    var errorCode=RcloneFailureClassifier.GetErrorCode(failure);
                    await _cloudState.ScheduleAutomaticRetryAsync(CloudTransferKind.Media,game.PlayniteId,errorCode,cloud.StandardError,ct).ConfigureAwait(false);
                    await _store.UpdateMediaCloudStateAsync(game.PlayniteId,
                        errorCode == "RCLONE_AUTH_FAILED" ? "AuthenticationRequired" : RcloneFailureClassifier.IsRetryable(errorCode) ? "RetryScheduled" : "Failed",ct).ConfigureAwait(false);
                    throw new InvalidOperationException("媒体已在本地归档，但云端复制失败："+cloud.StandardError);
                }
                await _store.UpdateMediaCloudStateAsync(game.PlayniteId,"Synced",ct).ConfigureAwait(false);
                await _cloudState.MarkUploadedAsync(CloudTransferKind.Media,game.PlayniteId,ct).ConfigureAwait(false);
            }
            await progress.ReportAsync(100,$"媒体同步完成，新增 {copied} 个文件").ConfigureAwait(false);
        },token,request.NotificationSessionId);

    private Task<TaskStatusDto> SyncSharedSourcesAsync(IReadOnlyList<GameDescriptorDto> games,MediaSyncRequestDto request,CancellationToken token)=>
        _tasks.RunAsync("MediaInbox",string.Empty,"公共媒体收件箱",async(progress,ct)=>
        {
            await progress.ReportAsync(5,"正在扫描公共截图与录像目录").ConfigureAwait(false);
            var sources=(await DiscoverSharedSourcesAsync(ct).ConfigureAwait(false))
                .DistinctBy(x=>$"{x.Path}|{x.IncludePattern}",StringComparer.OrdinalIgnoreCase)
                .Where(x=>Directory.Exists(x.Path)).ToList();
            var candidates=EnumerateCandidates(sources)
                .OrderByDescending(x=>SafeCapturedUtc(x.Path))
                .ToList();
            var session=await ResolveSharedSessionAsync(request.SessionId,ct).ConfigureAwait(false);
            var assigned=0;var inbox=0;var index=0;
            var assignedGameIds=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var newlyAssignedGameIds=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach(var candidate in candidates)
            {
                ct.ThrowIfCancellationRequested();index++;
                var resolution=ResolveSharedTarget(candidate.Path,games,session);
                var target=resolution.Game;
                if(await ArchiveCandidateAsync(candidate.Path,candidate.Source,target,resolution.Reason,ct).ConfigureAwait(false))
                {
                    if(target==null)inbox++;
                    else
                    {
                        assigned++;
                        assignedGameIds.Add(target.PlayniteId);
                        newlyAssignedGameIds.Add(target.PlayniteId);
                    }
                }
                if(index%25==0)await progress.ReportAsync(Math.Min(90,5+(int)(85d*index/Math.Max(1,candidates.Count))),$"已检查 {index}/{candidates.Count}，待归类 {inbox}").ConfigureAwait(false);
                // Prevent one first run from filling the interface with thousands of ambiguous legacy captures.
                if(inbox>=200)
                {
                    _logger.LogInformation("Stopped shared media inbox import after the safety limit of 200 new unassigned items");
                    break;
                }
            }
            if(!_options.SafeModeEnabled&&_options.EnableCloudUpload&&_rclone.IsConfigured)
            {
                foreach(var gameId in await _store.GetMediaGamesNeedingCloudUploadAsync(ct).ConfigureAwait(false))
                    assignedGameIds.Add(gameId);
                foreach(var gameId in assignedGameIds)
                {
                    var game=games.First(x=>string.Equals(x.PlayniteId,gameId,StringComparison.OrdinalIgnoreCase));
                    var policy=await _store.GetPolicyAsync(gameId,ct).ConfigureAwait(false);
                    if(!request.UploadAfterSync&&!policy.UploadAfterBackup)continue;
                    await progress.ReportAsync(94,$"正在复制 {game.Name} 的公共媒体到云端").ConfigureAwait(false);
                    var gameDirectory=Path.Combine(_options.MediaArchiveDirectory,Sanitize(game.Name));
                    var remote=Path.Combine(Environment.MachineName,"Media",Sanitize(game.Name));
                    if(newlyAssignedGameIds.Contains(gameId))
                        await _cloudState.StartNewAsync(CloudTransferKind.Media,gameId,ct).ConfigureAwait(false);
                    await _store.UpdateMediaCloudStateAsync(gameId,"Pending",ct).ConfigureAwait(false);
                    await _cloudState.MarkTransferringAsync(CloudTransferKind.Media,gameId,ct).ConfigureAwait(false);
                    var cloud=await _cloudTransfers.RunUploadAsync("media inbox",transferToken=>_rclone.CopyAsync(gameDirectory,remote,transferToken),ct,
                        CloudTransferStateService.GetTransferKey(CloudTransferKind.Media,gameId)).ConfigureAwait(false);
                    if(!cloud.Success)
                    {
                        var failure=RcloneFailureClassifier.Classify(cloud.StandardError);
                        var errorCode=RcloneFailureClassifier.GetErrorCode(failure);
                        await _cloudState.ScheduleAutomaticRetryAsync(CloudTransferKind.Media,gameId,errorCode,cloud.StandardError,ct).ConfigureAwait(false);
                        await _store.UpdateMediaCloudStateAsync(gameId,
                            errorCode == "RCLONE_AUTH_FAILED" ? "AuthenticationRequired" : RcloneFailureClassifier.IsRetryable(errorCode) ? "RetryScheduled" : "Failed",ct).ConfigureAwait(false);
                        throw new InvalidOperationException($"{game.Name} 的公共媒体已在本地归档，但云端复制失败：{cloud.StandardError}");
                    }
                    await _store.UpdateMediaCloudStateAsync(gameId,"Synced",ct).ConfigureAwait(false);
                    await _cloudState.MarkUploadedAsync(CloudTransferKind.Media,gameId,ct).ConfigureAwait(false);
                }
            }
            await progress.ReportAsync(100,$"公共媒体扫描完成，自动归类 {assigned} 个，待人工归类 {inbox} 个").ConfigureAwait(false);
        },token,request.NotificationSessionId);

    /// <summary>Retries only the safe one-way media copy; it never rescans or deletes captures.</summary>
    public async Task<TaskStatusDto> RetryCloudUploadAsync(string playniteId, CancellationToken token)
    {
        var game=await _catalog.GetGameAsync(playniteId,token).ConfigureAwait(false)
            ??throw new WorkerOperationException("CLOUD_GAME_NOT_FOUND","找不到需要重试媒体云端上传的游戏。",playniteId);
        var policy=await _store.GetPolicyAsync(playniteId,token).ConfigureAwait(false);
        if(!policy.UploadAfterBackup)
        {
            await _cloudState.MarkPausedAsync(CloudTransferKind.Media,playniteId,"该游戏策略未允许媒体上传，已暂停自动重试。",token).ConfigureAwait(false);
            return new TaskStatusDto{TaskType="CloudUpload",GameId=playniteId,GameName=game.Name,State=TaskState.Cancelled,Message="媒体云端上传已按策略暂停",CreatedUtc=DateTime.UtcNow,FinishedUtc=DateTime.UtcNow};
        }
        using var lease=await _gameLock.AcquireAsync(playniteId,GameOperationKind.CloudUpload,TimeSpan.FromSeconds(10),token).ConfigureAwait(false);
        if(lease==null)throw new WorkerOperationException("GAME_OPERATION_BUSY","该游戏已有操作正在执行，已跳过媒体云端上传重试。",playniteId);
        return await _tasks.RunAsync("CloudUpload",game.PlayniteId,game.Name,async(progress,ct)=>
        {
            if(_options.SafeModeEnabled)throw new WorkerOperationException("SAFE_MODE_ENABLED","安全模式已开启，云端上传已暂停。请先关闭安全模式。","SafeMode");
            if(!_options.EnableCloudUpload||!_rclone.IsConfigured)throw new WorkerOperationException("RCLONE_NOT_CONFIGURED","云端复制尚未启用或 Rclone 配置不可用。",_options.RcloneDestination);
            var local=Path.Combine(_options.MediaArchiveDirectory,Sanitize(game.Name));
            if(!Directory.Exists(local))throw new WorkerOperationException("MEDIA_DIRECTORY_MISSING","本地媒体归档目录不存在，无法重试上传。",local);
            await _store.UpdateMediaCloudStateAsync(playniteId,"Pending",ct).ConfigureAwait(false);
            await _cloudState.MarkTransferringAsync(CloudTransferKind.Media,playniteId,ct).ConfigureAwait(false);
            await progress.ReportAsync(10,"正在重新复制媒体到云端").ConfigureAwait(false);
            var remote=Path.Combine(Environment.MachineName,"Media",Sanitize(game.Name));
            var cloud=await _cloudTransfers.RunUploadAsync("media retry",transferToken=>_rclone.CopyAsync(local,remote,transferToken),ct,
                CloudTransferStateService.GetTransferKey(CloudTransferKind.Media,playniteId)).ConfigureAwait(false);
            if(!cloud.Success)
            {
                var failure=RcloneFailureClassifier.Classify(cloud.StandardError);
                var errorCode=RcloneFailureClassifier.GetErrorCode(failure);
                await _cloudState.ScheduleAutomaticRetryAsync(CloudTransferKind.Media,playniteId,errorCode,cloud.StandardError,ct).ConfigureAwait(false);
                await _store.UpdateMediaCloudStateAsync(playniteId,
                    errorCode == "RCLONE_AUTH_FAILED" ? "AuthenticationRequired" : RcloneFailureClassifier.IsRetryable(errorCode) ? "RetryScheduled" : "Failed",ct).ConfigureAwait(false);
                throw new WorkerOperationException(errorCode,$"媒体云端复制重试失败：{RcloneFailureClassifier.GetUserMessage(failure)}",cloud.StandardError);
            }
            await _store.UpdateMediaCloudStateAsync(playniteId,"Synced",ct).ConfigureAwait(false);
            await _cloudState.MarkUploadedAsync(CloudTransferKind.Media,playniteId,ct).ConfigureAwait(false);
            await progress.ReportAsync(100,"媒体云端复制重试完成").ConfigureAwait(false);
        },token).ConfigureAwait(false);
    }

    private async Task<bool> ArchiveCandidateAsync(string path,MediaSourceKind source,GameDescriptorDto? game,string classificationReason,CancellationToken token)
    {
        try
        {
            // Fast path: a size+last-write match plus three small content samples against a
            // previously hashed version lets us reuse the stored SHA-256 without the 350 ms
            // stability wait or a full-file read on every rescan. Old rows without a sample
            // deliberately fall through to a full hash. The sample is only a change detector;
            // the archive hash still remains the deduplication source of truth.
            var current=new FileInfo(path);
            if(!current.Exists)return false;
            var signature=await _store.TryGetMediaFileSignatureAsync(path,token).ConfigureAwait(false);
            if(signature!=null&&signature.Length==current.Length&&signature.LastWriteTimeUtc==current.LastWriteTimeUtc&&!string.IsNullOrWhiteSpace(signature.SampleHash))
            {
                var sampleHash=await ComputeSampleHashAsync(path,current.Length,token).ConfigureAwait(false);
                if(string.Equals(sampleHash,signature.SampleHash,StringComparison.OrdinalIgnoreCase)
                    && await _store.MediaHashExistsAsync(signature.Sha256,token).ConfigureAwait(false))
                {
                    // Refresh the retention timestamp whenever a cached source is observed.
                    await _store.UpsertMediaFileSignatureAsync(path,current.Length,current.LastWriteTimeUtc,signature.Sha256,sampleHash,token).ConfigureAwait(false);
                    return false;
                }
            }

            if(!await IsStableAsync(path,token).ConfigureAwait(false))return false;
            current.Refresh();
            var currentSampleHash=await ComputeSampleHashAsync(path,current.Length,token).ConfigureAwait(false);
            var hash=await ComputeSha256Async(path,token).ConfigureAwait(false);
            await _store.UpsertMediaFileSignatureAsync(path,current.Length,current.LastWriteTimeUtc,hash,currentSampleHash,token).ConfigureAwait(false);
            if(await _store.MediaHashExistsAsync(hash,token).ConfigureAwait(false))return false;
            return await ArchiveWithHashAsync(path,current,hash,source,game,classificationReason,token).ConfigureAwait(false);
        }
        catch(OperationCanceledException){throw;}
        catch(Exception ex)
        {
            _logger.LogWarning(ex,"Could not archive media candidate {Path}",path);
            return false;
        }
    }

    private async Task<bool> ArchiveWithHashAsync(string path,FileInfo info,string hash,MediaSourceKind source,GameDescriptorDto? game,string classificationReason,CancellationToken token)
    {
        if(!info.Exists)return false;
        var captured=info.CreationTimeUtc==DateTime.MinValue?info.LastWriteTimeUtc:info.CreationTimeUtc;
        var kind=ImageExtensions.Contains(info.Extension)?MediaKind.Screenshot:MediaKind.VideoClip;
        var archive=game==null
            ?BuildInboxArchivePath(source,kind,captured,hash,info.Extension)
            :BuildArchivePath(game,source,kind,captured,hash,info.Extension);
        Directory.CreateDirectory(Path.GetDirectoryName(archive)!);
        await CopyAtomicallyAsync(path,archive,token).ConfigureAwait(false);
        await _store.AddMediaAsync(new MediaItemDto
        {
            MediaId=Guid.NewGuid().ToString("N"),PlayniteId=game?.PlayniteId??string.Empty,Kind=kind,Source=source,
            ArchivePath=archive,OriginalPath=path,CapturedUtc=captured,SizeBytes=info.Length,Sha256=hash,
            CloudState=game==null?"NotApplicable":"Pending",ClassificationState=game==null?"Inbox":"Assigned",ClassificationReason=classificationReason
        },token).ConfigureAwait(false);
        return true;
    }

    private async Task<GameSessionEventDto?> ResolveSharedSessionAsync(string sessionId,CancellationToken token)
    {
        if(string.IsNullOrWhiteSpace(sessionId))return null;
        var session=await _store.GetSessionAsync(sessionId,token).ConfigureAwait(false);
        if(session==null)return null;
        var stop=session.StoppedUtc??DateTime.UtcNow;
        if(await _store.HasOverlappingGameSessionAsync(session.PlayniteId,session.StartedUtc.AddMinutes(-2),stop.AddMinutes(10),token).ConfigureAwait(false))
        {
            _logger.LogInformation("Skipped time-only shared media attribution for {Game} because another game session overlaps",session.PlayniteId);
            return null;
        }
        return session;
    }

    private static SharedMediaResolution ResolveSharedTarget(string path,IReadOnlyList<GameDescriptorDto> games,GameSessionEventDto? session)
    {
        var nameMatches=games.Where(x=>SharedFileMatchesGame(path,x.Name)).Take(3).ToList();
        if(nameMatches.Count==1)
            return new SharedMediaResolution(nameMatches[0],"文件名唯一匹配游戏");

        if(session!=null&&SharedFileWithinSession(path,session))
        {
            var sessionGame=games.FirstOrDefault(x=>string.Equals(x.PlayniteId,session.PlayniteId,StringComparison.OrdinalIgnoreCase));
            if(sessionGame!=null&&nameMatches.Count==0)
                return new SharedMediaResolution(sessionGame,"文件时间位于无重叠游戏会话窗口");
            if(sessionGame!=null&&nameMatches.Any(x=>string.Equals(x.PlayniteId,sessionGame.PlayniteId,StringComparison.OrdinalIgnoreCase)))
                return new SharedMediaResolution(sessionGame,"会话窗口消解了文件名的多游戏歧义");
        }

        var reason=nameMatches.Count>1
            ? "文件名同时匹配多个游戏，未自动归类"
            : session==null
                ? "缺少可安全使用的无重叠游戏会话，且文件名不能唯一匹配"
                : "文件时间不在当前无重叠游戏会话窗口内，且文件名不能唯一匹配";
        return new SharedMediaResolution(null,reason);
    }

    private async Task<List<MediaSource>> DiscoverGameSourcesAsync(GameDescriptorDto game,CancellationToken token)
    {
        var output=new List<MediaSource>();
        if(_options.EnableSteamMedia&&game.Platform==GamePlatformKind.Steam&&!string.IsNullOrWhiteSpace(game.PlatformGameId))
        {
            foreach(var steamRoot in SteamRoots())
            {
                var userdata=Path.Combine(steamRoot,"userdata");
                if(!Directory.Exists(userdata))continue;
                foreach(var user in Directory.EnumerateDirectories(userdata))
                    output.Add(new MediaSource(Path.Combine(user,"760","remote",game.PlatformGameId,"screenshots"),MediaSourceKind.Steam));
            }
        }
        if(_options.EnablePlatformAdjacentMedia&&!string.IsNullOrWhiteSpace(game.InstallDirectory))
            foreach(var child in new[]{"Screenshots","Screenshot","Captures","Capture","Media"})
                output.Add(new MediaSource(Path.Combine(game.InstallDirectory,child),PlatformSource(game.Platform)));
        if(_options.EnablePlatformAdjacentMedia)
        foreach(var action in game.Actions)
        {
            var basePath=string.IsNullOrWhiteSpace(action.WorkingDirectory)?Path.GetDirectoryName(action.Path):action.WorkingDirectory;
            if(string.IsNullOrWhiteSpace(basePath))continue;
            foreach(var child in new[]{"Screenshots","Captures"})
                output.Add(new MediaSource(Path.Combine(basePath,child),action.IsModLoader?MediaSourceKind.Custom:PlatformSource(game.Platform)));
        }
        if(_options.EnableCustomMedia)
        foreach(var custom in await _store.GetMediaSourcesAsync(game.PlayniteId,token).ConfigureAwait(false))
            if(custom.Enabled&&!custom.SharedDirectory&&!string.IsNullOrWhiteSpace(custom.RootPath))
                output.Add(new MediaSource(custom.RootPath,custom.SourceKind,string.IsNullOrWhiteSpace(custom.IncludePattern)?"*":custom.IncludePattern));
        return output;
    }

    private async Task<List<MediaSource>> DiscoverSharedSourcesAsync(CancellationToken token)
    {
        var output=new List<MediaSource>();
        var captures=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),"Captures");
        if(_options.EnableXboxGameBarMedia&&Directory.Exists(captures))output.Add(new MediaSource(captures,MediaSourceKind.XboxGameBar));
        var windowsScreens=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),"Screenshots");
        if(_options.EnableWindowsScreenshotMedia&&Directory.Exists(windowsScreens))output.Add(new MediaSource(windowsScreens,MediaSourceKind.WindowsScreenshot));
        if(_options.EnableCustomMedia)
        foreach(var custom in await _store.GetSharedMediaSourcesAsync(token).ConfigureAwait(false))
            if(!string.IsNullOrWhiteSpace(custom.RootPath))
                output.Add(new MediaSource(custom.RootPath,custom.SourceKind,string.IsNullOrWhiteSpace(custom.IncludePattern)?"*":custom.IncludePattern));
        return output;
    }

    private List<(string Path,MediaSourceKind Source)> EnumerateCandidates(IEnumerable<MediaSource> sources)
    {
        var output=new List<(string Path,MediaSourceKind Source)>();
        foreach(var source in sources)
        {
            try
            {
                output.AddRange(Directory.EnumerateFiles(source.Path,string.IsNullOrWhiteSpace(source.IncludePattern)?"*":source.IncludePattern,SearchOption.AllDirectories)
                    .Where(IsMedia).Select(x=>(x,source.Source)));
            }
            catch(Exception ex){_logger.LogWarning(ex,"Could not scan media source {Path}",source.Path);}
        }
        return output.DistinctBy(x=>x.Path,StringComparer.OrdinalIgnoreCase).ToList();
    }

    private IEnumerable<string> SteamRoots()
    {
        var values=new[]{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),"Steam"),Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),"Steam"),@"C:\Steam",@"D:\Steam",@"E:\Steam"};
        return values.Where(Directory.Exists);
    }

    private static MediaSourceKind PlatformSource(GamePlatformKind platform)=>platform switch
    {GamePlatformKind.Steam=>MediaSourceKind.Steam,GamePlatformKind.Xbox=>MediaSourceKind.XboxGameBar,GamePlatformKind.Epic=>MediaSourceKind.Epic,GamePlatformKind.Ubisoft=>MediaSourceKind.Ubisoft,GamePlatformKind.Ea=>MediaSourceKind.Ea,GamePlatformKind.Gog=>MediaSourceKind.Gog,_=>MediaSourceKind.GameNative};

    private static bool SharedFileMatchesGame(string path,string gameName)
    {
        var file=NameNormalizer.Normalize(Path.GetFileNameWithoutExtension(path));
        var game=NameNormalizer.Normalize(gameName);
        if(string.IsNullOrWhiteSpace(file)||string.IsNullOrWhiteSpace(game))return false;
        if(file.Contains(game,StringComparison.OrdinalIgnoreCase))return true;
        var meaningful=game.Split(' ',StringSplitOptions.RemoveEmptyEntries).Where(x=>x.Length>=4).ToArray();
        return meaningful.Length>0&&meaningful.Count(x=>file.Contains(x,StringComparison.OrdinalIgnoreCase))>=Math.Min(2,meaningful.Length);
    }

    private static bool SharedFileWithinSession(string path,GameSessionEventDto? session)
    {
        if(session==null||session.StartedUtc==default)return false;
        try
        {
            var captured=SafeCapturedUtc(path);
            var start=session.StartedUtc.ToUniversalTime().AddMinutes(-2);
            var stop=(session.StoppedUtc??DateTime.UtcNow).ToUniversalTime().AddMinutes(10);
            return captured>=start&&captured<=stop;
        }
        catch{return false;}
    }

    private static DateTime SafeCapturedUtc(string path)
    {
        try
        {
            var info=new FileInfo(path);
            return info.CreationTimeUtc==DateTime.MinValue?info.LastWriteTimeUtc:info.CreationTimeUtc;
        }
        catch{return DateTime.MinValue;}
    }

    private static bool IsMedia(string path){var ext=Path.GetExtension(path);return ImageExtensions.Contains(ext)||VideoExtensions.Contains(ext);}

    private string BuildArchivePath(GameDescriptorDto game,MediaSourceKind source,MediaKind kind,DateTime captured,string hash,string extension)
    {
        var category=kind==MediaKind.Screenshot?"Screenshots":"Clips";
        var file=$"{captured:yyyy-MM-dd_HH-mm-ss}_{source}_{hash[..8]}{extension.ToLowerInvariant()}";
        return Path.Combine(_options.MediaArchiveDirectory,Sanitize(game.Name),category,captured.ToString("yyyy"),captured.ToString("MM"),file);
    }

    private string BuildInboxArchivePath(MediaSourceKind source,MediaKind kind,DateTime captured,string hash,string extension)
    {
        var category=kind==MediaKind.Screenshot?"Screenshots":"Clips";
        var file=$"{captured:yyyy-MM-dd_HH-mm-ss}_{source}_{hash[..8]}{extension.ToLowerInvariant()}";
        return Path.Combine(_options.MediaArchiveDirectory,"_Inbox","Pending",category,captured.ToString("yyyy"),captured.ToString("MM"),file);
    }

    private string BuildIgnoredArchivePath(MediaItemDto item,string extension)
    {
        var category=item.Kind==MediaKind.Screenshot?"Screenshots":"Clips";
        var file=$"{item.CapturedUtc:yyyy-MM-dd_HH-mm-ss}_{item.Source}_{item.Sha256[..8]}{extension.ToLowerInvariant()}";
        return Path.Combine(_options.MediaArchiveDirectory,"_Inbox","Ignored",category,item.CapturedUtc.ToString("yyyy"),item.CapturedUtc.ToString("MM"),file);
    }

    private static async Task RelocateArchivedCopyAsync(MediaItemDto item,string destination,CancellationToken token)
    {
        if(File.Exists(item.ArchivePath))
        {
            await MoveArchivedFileAsync(item.ArchivePath,destination,item.Sha256,token).ConfigureAwait(false);
            return;
        }
        if(File.Exists(item.OriginalPath))
        {
            // The original capture belongs to the source application/user. Rebuild the missing archive
            // copy without deleting or moving that original file.
            await EnsureArchivedCopyAsync(item.OriginalPath,destination,item.Sha256,token).ConfigureAwait(false);
            return;
        }
        throw new FileNotFoundException("归档文件和原始媒体文件都不存在，无法移动该记录。",item.ArchivePath);
    }

    private static async Task MoveArchivedFileAsync(string source,string destination,string expectedHash,CancellationToken token)
    {
        if(string.Equals(Path.GetFullPath(source),Path.GetFullPath(destination),StringComparison.OrdinalIgnoreCase))return;
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        if(File.Exists(destination))
        {
            var destinationHash=await ComputeSha256Async(destination,token).ConfigureAwait(false);
            if(!string.Equals(destinationHash,expectedHash,StringComparison.OrdinalIgnoreCase))
                throw new IOException("目标媒体路径已存在不同内容，已停止归类以避免覆盖。");
            File.Delete(source);
            return;
        }
        try{File.Move(source,destination);}
        catch(IOException)
        {
            await CopyAtomicallyAsync(source,destination,token).ConfigureAwait(false);
            File.Delete(source);
        }
    }

    private static async Task EnsureArchivedCopyAsync(string source,string destination,string expectedHash,CancellationToken token)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        if(File.Exists(destination))
        {
            var destinationHash=await ComputeSha256Async(destination,token).ConfigureAwait(false);
            if(!string.Equals(destinationHash,expectedHash,StringComparison.OrdinalIgnoreCase))
                throw new IOException("目标媒体路径已存在不同内容，已停止归类以避免覆盖。");
            return;
        }
        await CopyAtomicallyAsync(source,destination,token).ConfigureAwait(false);
    }

    private static async Task<bool> IsStableAsync(string path,CancellationToken token)
    {
        try
        {
            var first=new FileInfo(path).Length;await Task.Delay(350,token).ConfigureAwait(false);var second=new FileInfo(path).Length;
            return first==second&&second>0;
        }
        catch{return false;}
    }

    private static async Task<string> ComputeSha256Async(string path,CancellationToken token)
    {
        await using var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite,1024*128,FileOptions.Asynchronous|FileOptions.SequentialScan);
        var hash=await SHA256.HashDataAsync(stream,token).ConfigureAwait(false);return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static async Task<string> ComputeSampleHashAsync(string path,long length,CancellationToken token)
    {
        var offsets=new[]{0L,Math.Max(0,length/2-MediaSignatureSampleBytes/2),Math.Max(0,length-MediaSignatureSampleBytes)}
            .Distinct()
            .ToArray();
        using var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        await using var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete,64*1024,FileOptions.Asynchronous);
        var buffer=new byte[MediaSignatureSampleBytes];
        foreach(var offset in offsets)
        {
            stream.Position=offset;
            hash.AppendData(BitConverter.GetBytes(offset));
            var remaining=(int)Math.Min(MediaSignatureSampleBytes,Math.Max(0,length-offset));
            while(remaining>0)
            {
                var read=await stream.ReadAsync(buffer,0,remaining,token).ConfigureAwait(false);
                if(read==0)break;
                hash.AppendData(buffer,0,read);
                remaining-=read;
            }
        }
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static async Task CopyAtomicallyAsync(string source,string destination,CancellationToken token)
        => await AtomicFileWriter.CopyAtomicallyAsync(source,destination,token).ConfigureAwait(false);

    private static string Sanitize(string value)
    {
        var invalid=Path.GetInvalidFileNameChars();var normalized=new string(value.Select(c=>invalid.Contains(c)?'_':c).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(normalized)?"Unknown Game":normalized;
    }

    private sealed record MediaSource(string Path,MediaSourceKind Source,string IncludePattern="*");
    private sealed record SharedMediaResolution(GameDescriptorDto? Game,string Reason);
}
