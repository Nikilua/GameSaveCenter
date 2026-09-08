using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed partial class DashboardViewModel
    {
        private const int MediaPageSize = 200;

        private sealed class MediaInboxBatchSelection
        {
            public MediaInboxBatchSelection(IReadOnlyList<MediaItemDto> items, int rawCount, int duplicateCount, int invalidCount)
            {
                Items = items;
                MediaIds = items.Select(x => x.MediaId).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                RawCount = rawCount;
                DuplicateCount = duplicateCount;
                InvalidCount = invalidCount;
            }

            public IReadOnlyList<MediaItemDto> Items { get; }
            public IReadOnlyList<string> MediaIds { get; }
            public int RawCount { get; }
            public int DuplicateCount { get; }
            public int InvalidCount { get; }
            public int UniqueCount => MediaIds.Count;
            public int PreSubmitSkippedCount => DuplicateCount + InvalidCount;

            public string SelectionSummary(string action)
                => $"操作：{action}\n选择记录 {RawCount} 项，实际提交 {UniqueCount} 项；提交前跳过 {PreSubmitSkippedCount} 项（重复 {DuplicateCount}，无稳定 ID {InvalidCount}）。\n确认后将按本次捕获的媒体 ID 执行，即使列表选择随后变化也不会改用新选择。";
        }

        private MediaPageAccumulator CurrentMediaInboxAccumulator => MediaInboxMode == "已忽略"
            ? ignoredMediaPageAccumulator
            : unassignedMediaPageAccumulator;

        public MediaClassificationPreviewDto? MediaClassificationPreview
        {
            get => mediaClassificationPreview;
            private set
            {
                SetValue(ref mediaClassificationPreview, value);
                OnPropertyChanged(nameof(MediaClassificationPreviewSummary));
                RaiseCommandStates();
            }
        }

        public string MediaClassificationPreviewSummary => MediaClassificationPreview == null
            ? mediaClassificationStatus
            : $"{MediaClassificationPreview.SummaryDisplay} 预览有效期至 {MediaClassificationPreview.ExpiresUtc.ToLocalTime():MM-dd HH:mm}。低/中置信项目保持未归类。";

        public string LastMediaClassificationBatchId
        {
            get => lastMediaClassificationBatchId;
            private set
            {
                SetValue(ref lastMediaClassificationBatchId, value ?? string.Empty);
                RaiseCommandStates();
            }
        }

        private async Task LoadMediaWorkspaceAsync()
        {
            await LoadInboxAsync();
            await LoadMediaClassificationHistoryAsync(true);
            if (MediaInboxMode == "已忽略") await LoadIgnoredMediaAsync();
            if (SelectedGame != null) await LoadDetailsAsync();
        }

        private void StartQueuedMediaInboxLoad()
        {
            if (IsBusy || string.IsNullOrWhiteSpace(pendingMediaInboxLoadMode)) return;
            if (!string.Equals(MediaInboxMode, pendingMediaInboxLoadMode, StringComparison.Ordinal))
            {
                pendingMediaInboxLoadMode = null;
                return;
            }

            var requestedMode = pendingMediaInboxLoadMode;
            var requestGeneration = Interlocked.Read(ref mediaInboxLoadGeneration);
            pendingMediaInboxLoadMode = null;
            Run(() => LoadMediaInboxModeAsync(requestedMode!, requestGeneration));
        }

        private Task LoadMediaInboxModeAsync(string mode, long requestGeneration)
            => string.Equals(mode, "已忽略", StringComparison.Ordinal)
                ? LoadIgnoredMediaAsync(requestGeneration)
                : LoadInboxAsync(requestGeneration);

        private Task LoadInboxAsync()
            => LoadInboxAsync(Interlocked.Read(ref mediaInboxLoadGeneration));

        private async Task LoadInboxAsync(long requestGeneration)
        {
            BeginMediaInboxLoad("待归类", requestGeneration);
            try
            {
                var selectedId = SelectedInboxMedia?.MediaId;
                var targetId = InboxTargetGame?.PlayniteId;
                var inbox = await RequestMediaInboxPageAsync(true, ignored: false, requestGeneration: requestGeneration);
                if (inbox == null) return;
                ApplyOnUi(() =>
                {
                    if (requestGeneration != Interlocked.Read(ref mediaInboxLoadGeneration))
                        return;

                    ApplyMediaInboxPage(inbox, reset: true, collectionMode: "待归类", selectedId: selectedId, targetId: targetId);
                    CompleteMediaInboxLoad("待归类", requestGeneration);
                });
            }
            catch (OperationCanceledException)
            {
                ApplyOnUi(() => CancelMediaInboxLoad("待归类", requestGeneration));
                throw;
            }
            catch (Exception ex)
            {
                ApplyOnUi(() => FailMediaInboxLoad("待归类", ex, requestGeneration));
                throw;
            }
        }

        private Task LoadIgnoredMediaAsync()
            => LoadIgnoredMediaAsync(Interlocked.Read(ref mediaInboxLoadGeneration));

        private async Task LoadIgnoredMediaAsync(long requestGeneration)
        {
            BeginMediaInboxLoad("已忽略", requestGeneration);
            try
            {
                var selectedId = SelectedInboxMedia?.MediaId;
                var ignored = await RequestMediaInboxPageAsync(true, ignored: true, requestGeneration: requestGeneration);
                if (ignored == null) return;
                ApplyOnUi(() =>
                {
                    if (requestGeneration != Interlocked.Read(ref mediaInboxLoadGeneration))
                        return;

                    ApplyMediaInboxPage(ignored, reset: true, collectionMode: "已忽略", selectedId: selectedId, targetId: null);
                    CompleteMediaInboxLoad("已忽略", requestGeneration);
                });
            }
            catch (OperationCanceledException)
            {
                ApplyOnUi(() => CancelMediaInboxLoad("已忽略", requestGeneration));
                throw;
            }
            catch (Exception ex)
            {
                ApplyOnUi(() => FailMediaInboxLoad("已忽略", ex, requestGeneration));
                throw;
            }
        }

        private async Task LoadMoreMediaInboxPageAsync()
        {
            var requestGeneration = Interlocked.Read(ref mediaInboxLoadGeneration);
            var requestMode = MediaInboxMode;
            try
            {
                var selectedId = SelectedInboxMedia?.MediaId;
                var targetId = InboxTargetGame?.PlayniteId;
                var page = await RequestMediaInboxPageAsync(false, ignored: requestMode == "已忽略", requestGeneration: requestGeneration);
                if (page == null) return;
                ApplyOnUi(() =>
                {
                    if (!string.Equals(MediaInboxMode, requestMode, StringComparison.Ordinal)
                        || requestGeneration != Interlocked.Read(ref mediaInboxLoadGeneration))
                        return;
                    ApplyMediaInboxPage(page, reset: false, collectionMode: requestMode, selectedId: selectedId, targetId: targetId);
                    CompleteMediaInboxLoad(requestMode, requestGeneration);
                });
            }
            catch (OperationCanceledException)
            {
                ApplyOnUi(() => CancelMediaInboxLoad(requestMode, requestGeneration));
                throw;
            }
            catch (Exception ex)
            {
                ApplyOnUi(() => FailMediaInboxLoad(requestMode, ex, requestGeneration));
                throw;
            }
        }

        private async Task<MediaPageDto?> RequestMediaInboxPageAsync(bool reset, bool ignored, long requestGeneration)
        {
            if (requestGeneration != Interlocked.Read(ref mediaInboxLoadGeneration)) return null;
            var messageType = ignored
                ? MessageTypes.ListIgnoredMediaPage
                : MessageTypes.ListUnassignedMediaPage;
            var requestCancellation = BeginMediaInboxRequest();
            try
            {
                if (requestGeneration != Interlocked.Read(ref mediaInboxLoadGeneration)) return null;
                return await plugin.RequestAsync<MediaPageDto>(messageType, new MediaQueryDto
                {
                    Limit = MediaPageSize,
                    Cursor = reset
                        ? string.Empty
                        : ignored ? ignoredMediaPageCursor : unassignedMediaPageCursor
                }, cancellationToken: requestCancellation.Token).ConfigureAwait(false);
            }
            finally
            {
                EndMediaInboxRequest(requestCancellation);
            }
        }

        private void ApplyMediaInboxPage(MediaPageDto page, bool reset, string collectionMode, string? selectedId, string? targetId)
        {
            var ignored = string.Equals(collectionMode, "已忽略", StringComparison.Ordinal);
            var incoming = page.Items ?? new List<MediaItemDto>();
            var accumulator = ignored ? ignoredMediaPageAccumulator : unassignedMediaPageAccumulator;
            if (reset)
                accumulator.ReplaceFirstPage(incoming, selectedId);
            else
                accumulator.AppendPage(incoming, selectedId);
            if (ignored)
            {
                ignoredMediaPageCursor = page.NextCursor ?? string.Empty;
                ignoredMediaPageTotalCount = Math.Max(0, page.TotalCount);
                ignoredMediaPageHasMore = page.HasMore;
            }
            else
            {
                unassignedMediaPageCursor = page.NextCursor ?? string.Empty;
                unassignedMediaPageTotalCount = Math.Max(0, page.TotalCount);
                unassignedMediaPageHasMore = page.HasMore;
            }

            if (!string.Equals(MediaInboxMode, collectionMode, StringComparison.Ordinal))
                return;

            OnPropertyChanged(nameof(MediaInboxPageHasMore));
            OnPropertyChanged(nameof(MediaInboxLoadedSummary));

            var currentSelectedId = SelectedInboxMedia?.MediaId;
            var keepSelectedId = string.Equals(currentSelectedId, selectedId, StringComparison.OrdinalIgnoreCase)
                ? selectedId
                : currentSelectedId;
            ApplyMediaInboxMode(keepSelectedId);

            var currentTargetId = InboxTargetGame?.PlayniteId;
            var keepTargetId = string.Equals(currentTargetId, targetId, StringComparison.OrdinalIgnoreCase)
                ? targetId
                : currentTargetId;
            InboxTargetGame = Games.FirstOrDefault(x => string.Equals(x.PlayniteId, keepTargetId, StringComparison.OrdinalIgnoreCase))
                              ?? SelectedGame
                              ?? Games.FirstOrDefault();
            OnPropertyChanged(nameof(MediaInboxLoadedSummary));
            RaiseCommandStates();
        }

        private async Task LoadFilteredMediaPageAsync()
        {
            if (SelectedGame == null || CurrentWorkspace != WorkspaceKind.Media) return;
            await LoadMediaPageAsync(true, SelectedGame.PlayniteId);
        }

        private async Task LoadMoreMediaPageAsync()
        {
            if (SelectedGame == null || CurrentWorkspace != WorkspaceKind.Media) return;
            await LoadMediaPageAsync(false, SelectedGame.PlayniteId);
        }

        private async Task ReloadMediaWindowAsync()
        {
            if (SelectedGame == null || CurrentWorkspace != WorkspaceKind.Media) return;
            await LoadMediaPageAsync(true, SelectedGame.PlayniteId);
        }

        private async Task ReloadMediaInboxWindowAsync()
        {
            if (CurrentWorkspace != WorkspaceKind.Media) return;

            var requestGeneration = Interlocked.Increment(ref mediaInboxLoadGeneration);
            var mode = MediaInboxMode;
            await LoadMediaInboxModeAsync(mode, requestGeneration);
        }

        private async Task LoadMediaPageAsync(bool reset, string playniteId)
        {
            var requestGeneration = reset
                ? Interlocked.Increment(ref mediaPageGeneration)
                : Interlocked.Read(ref mediaPageGeneration);
            if (reset) BeginMediaDetailsLoad(requestGeneration);
            var requestCancellation = BeginMediaPageRequest();
            MediaPageDto? page;
            try
            {
                if (requestGeneration != Interlocked.Read(ref mediaPageGeneration)) return;
                page = await plugin.RequestAsync<MediaPageDto>(MessageTypes.ListMediaPage, BuildMediaQuery(
                    playniteId,
                    reset ? string.Empty : mediaPageCursor), cancellationToken: requestCancellation.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                if (reset) ApplyOnUi(() => CancelMediaDetailsLoad(requestGeneration));
                throw;
            }
            catch (Exception ex)
            {
                if (reset) ApplyOnUi(() => FailMediaDetailsLoad(ex, requestGeneration));
                throw;
            }
            finally
            {
                EndMediaPageRequest(requestCancellation);
            }
            if (requestGeneration != Interlocked.Read(ref mediaPageGeneration)
                || CurrentWorkspace != WorkspaceKind.Media
                || !IsSelectedGame(playniteId)) return;

            var selectedId = SelectedMedia?.MediaId;
            ApplyOnUi(() =>
            {
                if (requestGeneration != Interlocked.Read(ref mediaPageGeneration)
                    || CurrentWorkspace != WorkspaceKind.Media
                    || !IsSelectedGame(playniteId)) return;
                ApplyMediaPage(page ?? new MediaPageDto(), reset, selectedId);
                if (reset) CompleteMediaDetailsLoad(requestGeneration);
            });
        }

        private void ApplyMediaPage(MediaPageDto page, bool reset, string? selectedId)
        {
            var incoming = page.Items ?? new List<MediaItemDto>();
            if (reset)
                mediaPageAccumulator.ReplaceFirstPage(incoming, selectedId);
            else
                mediaPageAccumulator.AppendPage(incoming, selectedId);
            mediaPageCursor = page.NextCursor ?? string.Empty;
            mediaPageTotalCount = Math.Max(0, page.TotalCount);
            mediaPageHasMore = page.HasMore;
            OnPropertyChanged(nameof(MediaPageHasMore));
            OnPropertyChanged(nameof(MediaLoadedSummary));
            SelectedMedia = Media.FirstOrDefault(x => string.Equals(x.MediaId, selectedId, StringComparison.OrdinalIgnoreCase))
                            ?? Media.FirstOrDefault();
            MediaView.Refresh();
            RaiseCommandStates();
        }

        private MediaQueryDto BuildMediaQuery(string playniteId, string cursor)
        {
            var query = new MediaQueryDto
            {
                PlayniteId = playniteId,
                Search = MediaSearchText,
                Limit = MediaPageSize,
                Cursor = cursor ?? string.Empty
            };
            if (string.Equals(MediaFilter, "截图", StringComparison.Ordinal))
                query.Kind = MediaKind.Screenshot;
            else if (string.Equals(MediaFilter, "录像", StringComparison.Ordinal))
                query.Kind = MediaKind.VideoClip;
            else if (string.Equals(MediaFilter, "收藏", StringComparison.Ordinal))
                query.FavoriteOnly = true;
            return query;
        }

        private void ScheduleMediaPageQuery()
        {
            if (CurrentWorkspace == WorkspaceKind.Media && SelectedGame != null)
                mediaPageQueryRefresh.Schedule();
        }

        private void ClearMediaFilters()
        {
            mediaSearchRefresh.Cancel();
            mediaPageQueryRefresh.Cancel();
            var changed = !string.IsNullOrWhiteSpace(mediaSearchText)
                          || !string.Equals(mediaFilter, "全部", StringComparison.Ordinal);
            mediaSearchText = string.Empty;
            mediaFilter = "全部";
            if (changed) InvalidateMediaDetailsContext();
            OnPropertyChanged(nameof(MediaSearchText));
            OnPropertyChanged(nameof(MediaFilter));
            OnPropertyChanged(nameof(MediaHasActiveFilters));
            MediaView.Refresh();
            uiStateSave?.Schedule();
            if (changed) ScheduleMediaPageQuery();
        }

        private void ResetMediaPageState()
        {
            mediaPageCursor = string.Empty;
            mediaPageTotalCount = 0;
            mediaPageHasMore = false;
            OnPropertyChanged(nameof(MediaPageHasMore));
            OnPropertyChanged(nameof(MediaLoadedSummary));
            RaiseCommandStates();
        }

        private void ResetMediaInboxPageState()
        {
            if (MediaInboxMode == "已忽略")
            {
                ignoredMediaPageCursor = string.Empty;
                ignoredMediaPageTotalCount = 0;
                ignoredMediaPageHasMore = false;
            }
            else
            {
                unassignedMediaPageCursor = string.Empty;
                unassignedMediaPageTotalCount = 0;
                unassignedMediaPageHasMore = false;
            }
            OnPropertyChanged(nameof(MediaInboxPageHasMore));
            OnPropertyChanged(nameof(MediaInboxLoadedSummary));
            RaiseCommandStates();
        }

        private void ApplyMediaInboxMode(string? selectedId = null)
        {
            var keepId = selectedId ?? SelectedInboxMedia?.MediaId;
            MediaInboxItems = MediaInboxMode == "已忽略" ? IgnoredMedia : UnassignedMedia;
            SelectedInboxMedia = MediaInboxItems.FirstOrDefault(x => string.Equals(x.MediaId, keepId, StringComparison.OrdinalIgnoreCase))
                                 ?? MediaInboxItems.FirstOrDefault();
            RaiseCommandStates();
        }

        private static bool InboxEquals(IReadOnlyList<MediaItemDto> current, IReadOnlyList<MediaItemDto> incoming)
        {
            if (current.Count != incoming.Count)
                return false;

            for (var index = 0; index < current.Count; index++)
            {
                var left = current[index];
                var right = incoming[index];
                if (!string.Equals(left.MediaId, right.MediaId, StringComparison.OrdinalIgnoreCase)
                    || left.CapturedUtc != right.CapturedUtc
                    || !string.Equals(left.ClassificationReason, right.ClassificationReason, StringComparison.Ordinal)
                    || !string.Equals(left.OriginalPath, right.OriginalPath, StringComparison.Ordinal)
                    || left.SizeBytes != right.SizeBytes)
                    return false;
            }

            return true;
        }

        private async Task SyncMediaAsync()
        {
            if (!plugin.Settings.EnableMediaSync)
            {
                StatusMessage = "全局媒体归档已关闭；请在插件设置中启用后再同步。";
                return;
            }

            var ids = SelectedGame == null ? new string[0] : new[] { SelectedGame.PlayniteId };
            var request = new MediaSyncRequestDto { UploadAfterSync = plugin.Settings.EnableCloudUpload };
            foreach (var id in ids) request.PlayniteIds.Add(id);
            var tasks = await plugin.RequestAsync<TaskStatusDto[]>(MessageTypes.SyncMedia, request, TimeSpan.FromMinutes(60));
            await RefreshCoreAsync(false);
            NotifyTaskResults(tasks);
        }

        private async Task ReassignMediaAsync()
        {
            var media = SelectedMedia ?? throw new InvalidOperationException("请先选择媒体。");
            var sourceGameId = SelectedGame?.PlayniteId ?? throw new InvalidOperationException("请先选择游戏。");
            var target = MediaTargetGame ?? throw new InvalidOperationException("请选择目标游戏。");
            var mediaId = media.MediaId;
            var targetName = target.Name;
            await plugin.RequestAsync<MediaItemDto>(MessageTypes.ReassignMedia, new ReassignMediaRequestDto { MediaId = mediaId, TargetPlayniteId = target.PlayniteId });
            ConfirmSuccess($"媒体已重新归类到 {targetName}");
            if (CurrentWorkspace == WorkspaceKind.Media && IsSelectedGame(sourceGameId))
                await LoadDetailsAsync();
            await LoadInboxAsync();
        }

        private async Task UpdateMediaMetadataAsync()
        {
            var selected=SelectedMedia??throw new InvalidOperationException("请先选择媒体。");
            var gameId = SelectedGame?.PlayniteId ?? throw new InvalidOperationException("请先选择游戏。");
            var mediaId = selected.MediaId;
            var favorite = MediaFavorite;
            var comment = MediaComment;
            var updated=await plugin.RequestAsync<MediaItemDto>(MessageTypes.UpdateMediaMetadata,new MediaMetadataUpdateDto
            {
                MediaId=mediaId,
                IsFavorite=favorite,
                Comment=comment
            });
            if (CurrentWorkspace == WorkspaceKind.Media && IsSelectedGame(gameId))
            {
                var index = Media.ToList().FindIndex(x => string.Equals(x.MediaId, mediaId, StringComparison.OrdinalIgnoreCase));
                if (index >= 0) Media[index] = updated;
                if (string.Equals(SelectedMedia?.MediaId, mediaId, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(MediaComment, comment, StringComparison.Ordinal)) mediaCommentDirty = false;
                    if (MediaFavorite == favorite) mediaFavoriteDirty = false;
                    SelectedMedia = updated;
                }
                MediaView.Refresh();
                var summary = await plugin.RequestAsync<MediaStorageSummaryDto>(MessageTypes.GetMediaSummary,new GameQueryDto{PlayniteId=gameId});
                if (CurrentWorkspace == WorkspaceKind.Media && IsSelectedGame(gameId))
                    MediaSummary = summary;
            }
            ConfirmSuccess("媒体备注与收藏状态已保存");
        }

        private async Task UpdateMediaMetadataBatchAsync(object? value,bool? favorite,bool updateComment)
        {
            var selected=(value as IList)?.Cast<object>().OfType<MediaItemDto>()
                .GroupBy(x=>x.MediaId,StringComparer.OrdinalIgnoreCase)
                .Select(x=>x.First())
                .ToList()??new List<MediaItemDto>();
            if(selected.Count==0)throw new InvalidOperationException("请先在媒体列表中选择一个或多个项目。");
            var gameId = SelectedGame?.PlayniteId ?? throw new InvalidOperationException("请先选择游戏。");
            var comment = MediaComment;
            var mediaIds = selected.Select(x=>x.MediaId).ToList();
            var updated=await plugin.RequestAsync<MediaItemDto[]>(MessageTypes.UpdateMediaMetadataBatch,new MediaMetadataBatchUpdateDto
            {
                MediaIds=mediaIds,
                IsFavorite=favorite,
                UpdateComment=updateComment,
                Comment=comment
            });
            var byId=updated.ToDictionary(x=>x.MediaId,StringComparer.OrdinalIgnoreCase);
            if (CurrentWorkspace == WorkspaceKind.Media && IsSelectedGame(gameId))
            {
                for(var index=0;index<Media.Count;index++)
                    if(byId.TryGetValue(Media[index].MediaId,out var replacement))Media[index]=replacement;
                MediaView.Refresh();
                if(SelectedMedia!=null&&byId.TryGetValue(SelectedMedia.MediaId,out var selectedReplacement))
                {
                    if (updateComment && string.Equals(MediaComment, comment, StringComparison.Ordinal)) mediaCommentDirty = false;
                    if (favorite.HasValue && MediaFavorite == favorite.Value) mediaFavoriteDirty = false;
                    SelectedMedia=selectedReplacement;
                }
                var summary = await plugin.RequestAsync<MediaStorageSummaryDto>(MessageTypes.GetMediaSummary,new GameQueryDto{PlayniteId=gameId});
                if (CurrentWorkspace == WorkspaceKind.Media && IsSelectedGame(gameId))
                    MediaSummary = summary;
            }
            ConfirmSuccess(updateComment?$"已为 {updated.Length} 个媒体文件更新备注":favorite==true?$"已收藏 {updated.Length} 个媒体文件":$"已取消收藏 {updated.Length} 个媒体文件");
        }

        private void OpenSelectedMedia()
        {
            var path=SelectedMedia?.ArchivePath;
            if(string.IsNullOrWhiteSpace(path)||!File.Exists(path))throw new FileNotFoundException("归档媒体文件不存在。",path);
            Process.Start(new ProcessStartInfo{FileName=path,UseShellExecute=true});
        }

        private async Task AssignInboxMediaAsync()
        {
            var media = SelectedInboxMedia ?? throw new InvalidOperationException("请先选择待归类媒体。");
            var target = InboxTargetGame ?? throw new InvalidOperationException("请选择目标游戏。");
            await plugin.RequestAsync<MediaItemDto>(MessageTypes.ReassignMedia, new ReassignMediaRequestDto { MediaId = media.MediaId, TargetPlayniteId = target.PlayniteId });
            ConfirmSuccess($"已将 {media.FileName} 归类到 {target.Name}");
            await RefreshDashboardAsync(false, false);
            await LoadInboxAsync();
            if (SelectedGame != null && string.Equals(SelectedGame.PlayniteId, target.PlayniteId, StringComparison.OrdinalIgnoreCase))
                await LoadDetailsAsync();
        }

        private async Task IgnoreInboxMediaAsync()
        {
            var media = SelectedInboxMedia ?? throw new InvalidOperationException("请先选择待归类媒体。");
            if (!await plugin.ConfirmAsync(
                    "忽略待归类媒体",
                    $"确认忽略“{media.FileName}”？\n\n归档副本仍会保留在媒体目录中。",
                    "忽略并保留副本",
                    "取消")) return;
            await plugin.RequestAsync<MediaItemDto>(MessageTypes.IgnoreMedia, new IgnoreMediaRequestDto { MediaId = media.MediaId });
            ConfirmSuccess($"已忽略 {media.FileName}；归档副本仍保留在媒体目录");
            await RefreshDashboardAsync(false, false);
            await LoadInboxAsync();
        }

        private async Task AssignInboxMediaBatchAsync(object? value)
        {
            var selection = CaptureInboxMediaSelection(value);
            if (selection.UniqueCount == 0) throw new InvalidOperationException("请先在收件箱中选择一个或多个有效媒体。");

            var target=InboxTargetGame??throw new InvalidOperationException("请选择目标游戏。");
            var targetId = target.PlayniteId;
            var targetName = target.Name;
            if (!await plugin.ConfirmAsync(
                    "批量归类媒体",
                    $"{selection.SelectionSummary("归类")}\n\n目标游戏：{targetName}\n归档副本仍会保留。",
                    "归类所选",
                    "取消"))
            {
                StatusMessage = "已取消批量归类。";
                return;
            }
            var result=await ProcessInboxBatchAsync(MessageTypes.ReassignMediaBatch,selection.MediaIds.ToList(),targetId);
            ReportInboxBatchResult("归类",result,targetName,selection);
            await RefreshDashboardAsync(false,false);
            await LoadInboxAsync();
            if(SelectedGame!=null&&string.Equals(SelectedGame.PlayniteId,targetId,StringComparison.OrdinalIgnoreCase))
                await LoadDetailsAsync();
        }

        private async Task IgnoreInboxMediaBatchAsync(object? value)
        {
            var selection = CaptureInboxMediaSelection(value);
            if (selection.UniqueCount == 0) throw new InvalidOperationException("请先在收件箱中选择一个或多个有效媒体。");

            if(!await plugin.ConfirmAsync(
                    "忽略所选待归类媒体",
                    $"{selection.SelectionSummary("忽略")}\n\n所有归档副本仍会保留在媒体目录中。",
                    "忽略并保留副本",
                    "取消")) return;

            var result=await ProcessInboxBatchAsync(MessageTypes.IgnoreMediaBatch,selection.MediaIds.ToList());
            ReportInboxBatchResult("忽略",result,selection: selection);
            await RefreshDashboardAsync(false,false);
            await LoadInboxAsync();
        }

        private async Task RestoreIgnoredMediaBatchAsync(object? value)
        {
            var selection = CaptureInboxMediaSelection(value);
            if (selection.UniqueCount == 0) throw new InvalidOperationException("请先在已忽略列表中选择一个或多个有效媒体。");

            if (!await plugin.ConfirmAsync(
                    "恢复已忽略媒体",
                    $"{selection.SelectionSummary("恢复到待归类")}\n\n文件会移动回待归类归档目录，原始截图/录像不会被删除。",
                    "恢复到待归类",
                    "取消")) return;

            var result = await ProcessInboxBatchAsync(MessageTypes.RestoreIgnoredMediaBatch, selection.MediaIds.ToList());
            ReportInboxBatchResult("恢复到待归类", result, selection: selection);
            await RefreshDashboardAsync(false, false);
            await LoadInboxAsync();
            await LoadIgnoredMediaAsync();
        }

        private async Task PreviewMediaClassificationAsync(object? value)
        {
            var selection = CaptureInboxMediaSelection(value);
            if (selection.UniqueCount == 0) throw new InvalidOperationException("请先在待归类收件箱中选择一个或多个有效媒体。");

            var preview = await plugin.RequestAsync<MediaClassificationPreviewDto>(MessageTypes.PreviewMediaClassification,
                new MediaClassificationPreviewRequestDto
                {
                    MediaIds = selection.MediaIds.ToList(),
                    Limit = Math.Min(200, selection.UniqueCount)
            }, TimeSpan.FromMinutes(3));
            MediaClassificationPreview = preview;
            mediaClassificationStatus = preview.SummaryDisplay;
            OnPropertyChanged(nameof(MediaClassificationPreviewSummary));
            await LoadMediaClassificationHistoryAsync(true);
            ApplyOnUi(() => SelectedMediaClassificationBatch = MediaClassificationHistoryItems.FirstOrDefault(x => x.BatchId == preview.BatchId));
            ConfirmSuccess($"已生成媒体归类预览：{preview.SummaryDisplay}");
        }

        private async Task ApplyMediaClassificationAsync()
        {
            var preview = MediaClassificationPreview ?? throw new InvalidOperationException("请先生成媒体归类预览。");
            var previewBatchId = preview.BatchId;
            var previewIds = preview.Items
                .Where(x => x.CanApply && !string.IsNullOrWhiteSpace(x.MediaId))
                .Select(x => x.MediaId)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (preview.HighConfidenceCount <= 0 || previewIds.Count == 0) throw new InvalidOperationException("当前预览没有可安全确认的高置信建议。");
            if (!await plugin.ConfirmAsync(
                    "应用媒体归类建议",
                    $"操作：应用归类建议\n预览批次 {previewBatchId}，高置信建议 {preview.HighConfidenceCount} 项，实际提交 {previewIds.Count} 项；中/低置信 {preview.MediumConfidenceCount + preview.LowConfidenceCount} 项跳过。\n确认后将按本次捕获的批次 ID 和媒体 ID 执行，即使收件箱选择随后变化也不会改用新选择。\n\n只会处理预览后未发生变化的项目；冲突项目会继续留在待归类收件箱。原始截图/录像不会被删除。",
                    "应用高置信建议",
                    "取消")) return;

            var result = await plugin.RequestAsync<MediaClassificationBatchResultDto>(MessageTypes.ApplyMediaClassification,
                new MediaClassificationApplyRequestDto
                {
                    BatchId = previewBatchId,
                    MediaIds = previewIds,
                    HighConfidenceOnly = true
            }, TimeSpan.FromMinutes(10));
            MediaClassificationPreview = null;
            LastMediaClassificationBatchId = result.AppliedCount > 0 ? result.BatchId : string.Empty;
            mediaClassificationStatus = $"{result.State}：{result.SummaryDisplay}。如需回退，可撤销本次建议批次。";
            OnPropertyChanged(nameof(MediaClassificationPreviewSummary));
            await RefreshDashboardAsync(false, false);
            await LoadInboxAsync();
            await LoadMediaClassificationHistoryAsync(true);
            ApplyOnUi(() => SelectedMediaClassificationBatch = MediaClassificationHistoryItems.FirstOrDefault(x => x.BatchId == result.BatchId));
            if (SelectedGame != null) await LoadDetailsAsync();
            ReportMediaClassificationResult("应用媒体归类建议", result, previewIds.Count);
        }

        private async Task UndoMediaClassificationAsync()
        {
            var batchId = GetMediaClassificationUndoBatchId();
            if (string.IsNullOrWhiteSpace(batchId)) throw new InvalidOperationException("没有可撤销的媒体归类建议批次。");
            if (!await plugin.ConfirmAsync(
                    "撤销媒体归类建议",
                    "确认撤销最近一批已应用的媒体归类建议？\n\n只有应用后未被再次修改的项目会被恢复；发生冲突的项目会保留当前状态，不会覆盖用户后续修改。",
                    "撤销建议批次",
                    "取消")) return;

            var result = await plugin.RequestAsync<MediaClassificationBatchResultDto>(MessageTypes.UndoMediaClassification,
                new MediaClassificationUndoRequestDto { BatchId = batchId }, TimeSpan.FromMinutes(10));
            LastMediaClassificationBatchId = result.ConflictCount > 0 ? result.BatchId : string.Empty;
            mediaClassificationStatus = $"{result.State}：{result.SummaryDisplay}。冲突项目已保留当前状态。";
            OnPropertyChanged(nameof(MediaClassificationPreviewSummary));
            await RefreshDashboardAsync(false, false);
            await LoadInboxAsync();
            await LoadMediaClassificationHistoryAsync(true);
            ApplyOnUi(() => SelectedMediaClassificationBatch = MediaClassificationHistoryItems.FirstOrDefault(x => x.BatchId == result.BatchId));
            if (SelectedGame != null) await LoadDetailsAsync();
            ReportMediaClassificationResult("撤销媒体归类建议", result, result.Items?.Count ?? 0);
        }

        private async Task<MediaInboxBatchResultDto> ProcessInboxBatchAsync(string messageType,List<string> mediaIds,string targetPlayniteId="")
        {
            if(mediaIds.Count==0)throw new InvalidOperationException("请先在收件箱中选择一个或多个媒体。");

            var result=new MediaInboxBatchResultDto();
            for(var offset=0;offset<mediaIds.Count;offset+=MediaInboxBatchSize)
            {
                var request=new MediaInboxBatchRequestDto
                {
                    MediaIds=mediaIds.Skip(offset).Take(MediaInboxBatchSize).ToList(),
                    TargetPlayniteId=targetPlayniteId
                };
                var chunk=await plugin.RequestAsync<MediaInboxBatchResultDto>(messageType,request,TimeSpan.FromMinutes(30));
                if(chunk==null)continue;
                if(chunk.UpdatedItems!=null)result.UpdatedItems.AddRange(chunk.UpdatedItems);
                if(chunk.Failures!=null)result.Failures.AddRange(chunk.Failures);
            }
            return result;
        }

        private void ReportInboxBatchResult(string operation,MediaInboxBatchResultDto result,string targetName="",MediaInboxBatchSelection? selection=null)
        {
            var succeeded = result.UpdatedItems?
                .Select(x => x.MediaId)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() ?? 0;
            var failed = result.Failures?
                .Select(x => x.MediaId)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() ?? 0;
            var submitted = selection?.UniqueCount ?? succeeded + failed;
            var unreported = Math.Max(0, submitted - succeeded - failed);
            var targetSuffix=string.IsNullOrWhiteSpace(targetName)?string.Empty:$"到 {targetName}";
            if(failed==0 && unreported==0 && succeeded==submitted)
            {
                ConfirmSuccess($"已{operation} {succeeded} 项媒体{targetSuffix}。提交前重复/无效项：{selection?.PreSubmitSkippedCount ?? 0} 项。");
                return;
            }

            var firstFailure=result.Failures?.FirstOrDefault()?.ErrorMessage??"未知错误";
            var skipped = (selection?.PreSubmitSkippedCount ?? 0) + unreported;
            StatusMessage=$"批量{operation}完成：计划 {submitted} 项，成功 {succeeded} 项，失败 {failed} 项，跳过/未返回 {skipped} 项。{firstFailure}";
            plugin.ShowError(StatusMessage);
        }

        private void ReportMediaClassificationResult(string operation, MediaClassificationBatchResultDto result, int requestedCount)
        {
            var conflict = Math.Max(0, result.ConflictCount);
            var skipped = Math.Max(0, result.SkippedCount);
            var completed = result.Items?.Count(x => string.Equals(x.State, "Applied", StringComparison.OrdinalIgnoreCase)
                || string.Equals(x.State, "Undone", StringComparison.OrdinalIgnoreCase)) ?? 0;
            var unreported = Math.Max(0, requestedCount - completed - conflict - skipped);
            if (conflict == 0 && skipped == 0 && unreported == 0)
            {
                ConfirmSuccess($"{operation}完成：{result.SummaryDisplay}");
                return;
            }

            StatusMessage = $"{operation}完成但存在部分结果：已处理 {completed} 项，冲突 {conflict} 项，跳过 {skipped + unreported} 项。";
            var detail = result.Items?.FirstOrDefault(x => string.Equals(x.State, "Conflict", StringComparison.OrdinalIgnoreCase)
                || string.Equals(x.State, "Skipped", StringComparison.OrdinalIgnoreCase))?.Message;
            plugin.ShowWarning(StatusMessage + (string.IsNullOrWhiteSpace(detail) ? string.Empty : $"\n{detail}"));
        }

        private static MediaInboxBatchSelection CaptureInboxMediaSelection(object? value)
        {
            var raw = (value as IList)?.Cast<object>().OfType<MediaItemDto>().ToList() ?? new List<MediaItemDto>();
            var invalidCount = raw.Count(x => string.IsNullOrWhiteSpace(x.MediaId));
            var items = raw
                .Where(x => !string.IsNullOrWhiteSpace(x.MediaId))
                .GroupBy(x => x.MediaId, StringComparer.OrdinalIgnoreCase)
                .Select(x => x.First())
                .ToList();
            return new MediaInboxBatchSelection(items, raw.Count, raw.Count - invalidCount - items.Count, invalidCount);
        }

        private static List<MediaItemDto> GetSelectedInboxMedia(object? value)
            => CaptureInboxMediaSelection(value).Items.ToList();

        private bool FilterMedia(object value)
        {
            if(value is not MediaItemDto item)return false;
            if(string.Equals(MediaFilter,"截图",StringComparison.Ordinal)&&item.Kind!=MediaKind.Screenshot)return false;
            if(string.Equals(MediaFilter,"录像",StringComparison.Ordinal)&&item.Kind!=MediaKind.VideoClip)return false;
            if(string.Equals(MediaFilter,"收藏",StringComparison.Ordinal)&&!item.IsFavorite)return false;
            if(string.IsNullOrWhiteSpace(MediaSearchText))return true;
            return (item.FileName??string.Empty).IndexOf(MediaSearchText,StringComparison.OrdinalIgnoreCase)>=0
                   || (item.Comment??string.Empty).IndexOf(MediaSearchText,StringComparison.OrdinalIgnoreCase)>=0
                   || (item.SourceDisplay??string.Empty).IndexOf(MediaSearchText,StringComparison.OrdinalIgnoreCase)>=0;
        }
    }
}
