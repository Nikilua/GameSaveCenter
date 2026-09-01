using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed partial class DashboardViewModel
    {
        private const int MediaInboxPageSize = 500;
        private const int MaximumMediaInboxItems = 5000;

        private async Task LoadMediaWorkspaceAsync()
        {
            await LoadInboxAsync();
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
            var selectedId = SelectedInboxMedia?.MediaId;
            var targetId = InboxTargetGame?.PlayniteId;
            var requestMode = MediaInboxMode;
            var inbox = await LoadMediaInboxPagesAsync(MessageTypes.ListUnassignedMedia);
            ApplyOnUi(() =>
            {
                if (!InboxEquals(UnassignedMedia, inbox))
                    UnassignedMedia = new GameSaveCenter.Playnite.Infrastructure.BatchObservableCollection<MediaItemDto>(inbox);

                if (string.Equals(MediaInboxMode, requestMode, StringComparison.Ordinal)
                    && requestGeneration == Interlocked.Read(ref mediaInboxLoadGeneration))
                {
                    var currentSelectedId = SelectedInboxMedia?.MediaId;
                    var keepSelectedId = string.Equals(currentSelectedId, selectedId, StringComparison.OrdinalIgnoreCase)
                        ? selectedId
                        : currentSelectedId;
                    ApplyMediaInboxMode(keepSelectedId);
                }

                var currentTargetId = InboxTargetGame?.PlayniteId;
                var keepTargetId = string.Equals(currentTargetId, targetId, StringComparison.OrdinalIgnoreCase)
                    ? targetId
                    : currentTargetId;
                InboxTargetGame = Games.FirstOrDefault(x => string.Equals(x.PlayniteId, keepTargetId, StringComparison.OrdinalIgnoreCase))
                                  ?? SelectedGame
                                  ?? Games.FirstOrDefault();
                RaiseCommandStates();
            });
        }

        private Task LoadIgnoredMediaAsync()
            => LoadIgnoredMediaAsync(Interlocked.Read(ref mediaInboxLoadGeneration));

        private async Task LoadIgnoredMediaAsync(long requestGeneration)
        {
            var selectedId = SelectedInboxMedia?.MediaId;
            var requestMode = MediaInboxMode;
            var ignored = await LoadMediaInboxPagesAsync(MessageTypes.ListIgnoredMedia);
            ApplyOnUi(() =>
            {
                if (!InboxEquals(IgnoredMedia, ignored))
                    IgnoredMedia = new GameSaveCenter.Playnite.Infrastructure.BatchObservableCollection<MediaItemDto>(ignored);

                if (string.Equals(MediaInboxMode, requestMode, StringComparison.Ordinal)
                    && requestGeneration == Interlocked.Read(ref mediaInboxLoadGeneration))
                {
                    var currentSelectedId = SelectedInboxMedia?.MediaId;
                    var keepSelectedId = string.Equals(currentSelectedId, selectedId, StringComparison.OrdinalIgnoreCase)
                        ? selectedId
                        : currentSelectedId;
                    ApplyMediaInboxMode(keepSelectedId);
                }
                RaiseCommandStates();
            });
        }

        private async Task<MediaItemDto[]> LoadMediaInboxPagesAsync(string messageType)
        {
            var result = new List<MediaItemDto>();
            for (var offset = 0; offset < MaximumMediaInboxItems; offset += MediaInboxPageSize)
            {
                var limit = Math.Min(MediaInboxPageSize, MaximumMediaInboxItems - offset);
                var page = await plugin.RequestAsync<MediaItemDto[]>(messageType, new GameQueryDto
                {
                    Limit = limit,
                    Offset = offset
                }).ConfigureAwait(false) ?? Array.Empty<MediaItemDto>();
                if (page.Length == 0) break;

                result.AddRange(page);
                if (page.Length < limit) break;
            }
            return result.ToArray();
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
            var selected=GetSelectedInboxMedia(value);
            if(selected.Count==0)throw new InvalidOperationException("请先在收件箱中选择一个或多个媒体。");

            var target=InboxTargetGame??throw new InvalidOperationException("请选择目标游戏。");
            var result=await ProcessInboxBatchAsync(MessageTypes.ReassignMediaBatch,selected.Select(x=>x.MediaId).ToList(),target.PlayniteId);
            ReportInboxBatchResult("归类",result,target.Name);
            await RefreshDashboardAsync(false,false);
            await LoadInboxAsync();
            if(SelectedGame!=null&&string.Equals(SelectedGame.PlayniteId,target.PlayniteId,StringComparison.OrdinalIgnoreCase))
                await LoadDetailsAsync();
        }

        private async Task IgnoreInboxMediaBatchAsync(object? value)
        {
            var selected=GetSelectedInboxMedia(value);
            if(selected.Count==0)throw new InvalidOperationException("请先在收件箱中选择一个或多个媒体。");

            if(!await plugin.ConfirmAsync(
                    "忽略所选待归类媒体",
                    $"确认忽略所选 {selected.Count} 项媒体？\n\n所有归档副本仍会保留在媒体目录中。",
                    "忽略并保留副本",
                    "取消")) return;

            var result=await ProcessInboxBatchAsync(MessageTypes.IgnoreMediaBatch,selected.Select(x=>x.MediaId).ToList());
            ReportInboxBatchResult("忽略",result);
            await RefreshDashboardAsync(false,false);
            await LoadInboxAsync();
        }

        private async Task RestoreIgnoredMediaBatchAsync(object? value)
        {
            var selected = GetSelectedInboxMedia(value);
            if (selected.Count == 0) throw new InvalidOperationException("请先在已忽略列表中选择一个或多个媒体。");

            if (!await plugin.ConfirmAsync(
                    "恢复已忽略媒体",
                    $"确认将所选 {selected.Count} 项媒体恢复到待归类收件箱？\n\n文件会移动回待归类归档目录，原始截图/录像不会被删除。",
                    "恢复到待归类",
                    "取消")) return;

            var result = await ProcessInboxBatchAsync(MessageTypes.RestoreIgnoredMediaBatch, selected.Select(x => x.MediaId).ToList());
            ReportInboxBatchResult("恢复到待归类", result);
            await RefreshDashboardAsync(false, false);
            await LoadInboxAsync();
            await LoadIgnoredMediaAsync();
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

        private void ReportInboxBatchResult(string operation,MediaInboxBatchResultDto result,string targetName="")
        {
            var succeeded=result.UpdatedItems?.Count??0;
            var failed=result.Failures?.Count??0;
            var targetSuffix=string.IsNullOrWhiteSpace(targetName)?string.Empty:$"到 {targetName}";
            if(failed==0)
            {
                ConfirmSuccess($"已{operation} {succeeded} 项媒体{targetSuffix}");
                return;
            }

            var firstFailure=result.Failures?.FirstOrDefault()?.ErrorMessage??"未知错误";
            StatusMessage=$"批量{operation}完成：成功 {succeeded} 项，失败 {failed} 项。{firstFailure}";
            plugin.ShowError(StatusMessage);
        }

        private static List<MediaItemDto> GetSelectedInboxMedia(object? value)
            =>(value as IList)?.Cast<object>()
                .OfType<MediaItemDto>()
                .GroupBy(x=>x.MediaId,StringComparer.OrdinalIgnoreCase)
                .Select(x=>x.First())
                .ToList()??new List<MediaItemDto>();

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
