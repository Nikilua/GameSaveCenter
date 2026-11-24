using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed partial class DashboardViewModel
    {
        private const int PlaySessionHistoryTabIndex = 5;
        private readonly LatestRequestCoordinator playSessionReads = new LatestRequestCoordinator();
        private PlaySessionHistoryPageDto? playSessionPage;
        private string? playSessionGameId;
        private bool isPlaySessionLoading;
        private string playSessionStatus = string.Empty;

        public BatchObservableCollection<PlaySessionHistoryCard> PlaySessionHistory { get; } = new BatchObservableCollection<PlaySessionHistoryCard>();
        public string PlaySessionHistoryStatus => playSessionStatus;
        public ICommand RefreshPlaySessionHistoryCommand { get; private set; } = null!;
        public ICommand LoadMorePlaySessionHistoryCommand { get; private set; } = null!;
        public ICommand OpenPlaySessionTaskCommand { get; private set; } = null!;
        public ICommand OpenPlaySessionResultCommand { get; private set; } = null!;

        private bool CanReadPlaySessionHistory => CurrentWorkspace == WorkspaceKind.Saves
            && SaveTabIndex == PlaySessionHistoryTabIndex && SelectedGame != null;

        private void InitializePlaySessionHistory()
        {
            RefreshPlaySessionHistoryCommand = new RelayCommand(_ => Observe(LoadPlaySessionHistoryAsync(false)), _ => CanReadPlaySessionHistory);
            LoadMorePlaySessionHistoryCommand = new RelayCommand(_ => Observe(LoadPlaySessionHistoryAsync(true)),
                _ => CanReadPlaySessionHistory && !isPlaySessionLoading && playSessionPage?.HasMore == true);
            OpenPlaySessionTaskCommand = new RelayCommand(value => Run(() => OpenPlaySessionTaskAsync((PlaySessionTaskResult)value!)),
                value => CanReadPlaySessionHistory && !IsBusy && value is PlaySessionTaskResult);
            OpenPlaySessionResultCommand = new RelayCommand(value => Run(() => OpenPlaySessionResultAsync((TaskSourceReferenceDto)value!)),
                value => CanReadPlaySessionHistory && !IsBusy && value is TaskSourceReferenceDto source && IsSelectedGame(source.PlayniteId));
        }

        private void EnsurePlaySessionHistoryLoaded()
        {
            if (CanReadPlaySessionHistory && !string.Equals(playSessionGameId, SelectedGame.PlayniteId, StringComparison.Ordinal))
                Observe(LoadPlaySessionHistoryAsync(false));
            else if (CurrentWorkspace == WorkspaceKind.Saves && SaveTabIndex == PlaySessionHistoryTabIndex && SelectedGame == null)
            {
                playSessionStatus = "选择游戏后可以查看最近的游玩记录";
                OnPropertyChanged(nameof(PlaySessionHistoryStatus));
            }
        }

        private void ResetPlaySessionHistory()
        {
            playSessionReads.Cancel();
            playSessionGameId = null;
            playSessionPage = null;
            isPlaySessionLoading = false;
            PlaySessionHistory.Clear();
            playSessionStatus = string.Empty;
            OnPropertyChanged(nameof(PlaySessionHistoryStatus));
        }

        private async Task LoadPlaySessionHistoryAsync(bool append)
        {
            if (!CanReadPlaySessionHistory) return;
            var gameId = SelectedGame.PlayniteId;
            var cursor = append ? playSessionPage : null;
            if (!append) ResetPlaySessionHistory();
            var request = playSessionReads.Begin();
            playSessionGameId = gameId;
            isPlaySessionLoading = true;
            playSessionStatus = "正在读取游玩记录…";
            OnPropertyChanged(nameof(PlaySessionHistoryStatus));
            RaiseCommandStates();
            try
            {
                var page = await plugin.RequestAsync<PlaySessionHistoryPageDto>(MessageTypes.ListPlaySessionHistory,
                    new PlaySessionHistoryQueryDto
                    {
                        PlayniteId = gameId,
                        BeforeStoppedUtcKey = cursor?.NextStoppedUtcKey ?? string.Empty,
                        BeforeSessionId = cursor?.NextSessionId ?? string.Empty
                    }, cancellationToken: request.Token).ConfigureAwait(false);
                ApplyOnUi(() =>
                {
                    if (!playSessionReads.IsCurrent(request) || !IsSelectedGame(gameId)) return;
                    var known = new HashSet<string>(PlaySessionHistory.Select(item => item.SessionId), StringComparer.Ordinal);
                    foreach (var session in page.Items)
                        if (known.Add(session.SessionId)) PlaySessionHistory.Add(new PlaySessionHistoryCard(session));
                    playSessionPage = page;
                    playSessionStatus = PlaySessionHistory.Count == 0 ? "当前游戏还没有已结束的游玩记录" : $"已载入 {PlaySessionHistory.Count} 次游玩";
                });
            }
            catch (OperationCanceledException) when (request.Token.IsCancellationRequested) { }
            catch (Exception)
            {
                ApplyOnUi(() =>
                {
                    if (!playSessionReads.IsCurrent(request)) return;
                    playSessionStatus = "无法读取游玩记录。可以刷新重试。";
                });
            }
            finally
            {
                ApplyOnUi(() =>
                {
                    if (!playSessionReads.IsCurrent(request)) return;
                    isPlaySessionLoading = false;
                    OnPropertyChanged(nameof(PlaySessionHistoryStatus));
                    RaiseCommandStates();
                });
                playSessionReads.End(request);
            }
        }

        private async Task OpenPlaySessionTaskAsync(PlaySessionTaskResult result)
        {
            var gameId = SelectedGame.PlayniteId;
            var page = await plugin.RequestAsync<TaskPageDto>(MessageTypes.GetTaskPage,
                new TaskQueryDto { Limit = 1, ExactTaskId = result.TaskId }).ConfigureAwait(false);
            var task = TaskSourceNavigationResolver.ResolveExactTask(result.TaskId, page?.Items);
            ApplyOnUi(() =>
            {
                if (!CanReadPlaySessionHistory || !IsSelectedGame(gameId)) return;
                if (task == null || !string.Equals(task.SessionId, result.SessionId, StringComparison.Ordinal))
                {
                    StatusMessage = "这次游玩的任务已清理或无法读取。";
                    return;
                }

                PushNavigationReturnTarget("返回游玩记录", "游玩记录");
                FocusExactTask(task.TaskId);
                StatusMessage = $"已打开这次游玩的{task.TaskTypeDisplay}任务。";
            });
        }

        private async Task OpenPlaySessionResultAsync(TaskSourceReferenceDto source)
        {
            if (source.Kind == TaskSourceReferenceKind.MediaItem)
            {
                var media = await plugin.RequestAsync<MediaItemDto>(MessageTypes.GetSessionMedia,
                    new GameMediaIdentityDto { PlayniteId = source.PlayniteId, MediaId = source.StableId }).ConfigureAwait(false);
                ApplyOnUi(() =>
                {
                    if (!CanReadPlaySessionHistory || !IsSelectedGame(source.PlayniteId)) return;
                    if (media.MediaId != source.StableId || !File.Exists(media.ArchivePath))
                    {
                        StatusMessage = "这次游玩的归档媒体已删除、重新归类或不可用。";
                        return;
                    }
                    Process.Start(new ProcessStartInfo { FileName = media.ArchivePath, UseShellExecute = true });
                });
                return;
            }
            var page = await ReadExactHistoryVersionAsync(source.PlayniteId, source.StableId).ConfigureAwait(false);
            ApplyOnUi(() =>
            {
                if (!CanReadPlaySessionHistory || !IsSelectedGame(source.PlayniteId)) return;
                if (page.Items.Count == 0)
                {
                    StatusMessage = "这次游玩生成的存档版本已清理或不可用。";
                    return;
                }
                PushNavigationReturnTarget("返回游玩记录", "游玩记录");
                FocusHistoryVersion(page, "这次游玩生成的存档版本");
            });
        }
    }

    public sealed class PlaySessionHistoryCard
    {
        internal PlaySessionHistoryCard(PlaySessionHistoryEntryDto session)
        {
            SessionId = session.SessionId;
            Exited = $"退出于 {session.StoppedUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss}";
            Period = $"开始于 {session.StartedUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss} · 游玩 {TimeSpan.FromSeconds(session.ElapsedSeconds):g}";
            Tasks = session.Tasks.Select(task => new PlaySessionTaskResult(task)).ToList();
            Results = session.Tasks.SelectMany(task => task.SourceReferences)
                .Where(source => source.HasStableIdentity && string.Equals(source.PlayniteId, session.PlayniteId, StringComparison.Ordinal)
                    && (source.Kind == TaskSourceReferenceKind.BackupVersion || source.Kind == TaskSourceReferenceKind.MediaItem))
                .GroupBy(source => (source.Kind, source.StableId)).Select(group => group.First()).ToList();
        }

        public string SessionId { get; }
        public string Exited { get; }
        public string Period { get; }
        public IReadOnlyList<PlaySessionTaskResult> Tasks { get; }
        public IReadOnlyList<TaskSourceReferenceDto> Results { get; }
        public string EvidenceNote => Tasks.Count == 0 ? "这次游玩没有保留关联任务。"
            : Results.Count == 0 ? "这次游玩没有可确认的关联存档或媒体。" : string.Empty;
    }

    public sealed class PlaySessionTaskResult
    {
        internal PlaySessionTaskResult(TaskStatusDto task)
        {
            TaskId = task.TaskId;
            SessionId = task.SessionId;
            Title = $"{task.TaskTypeDisplay} · {task.StateDisplay}";
            Outcome = task.BackupResult?.HasResult == true ? task.BackupResult.StateDisplay : task.Message;
        }

        internal string TaskId { get; }
        internal string SessionId { get; }
        public string Title { get; }
        public string Outcome { get; }
    }
}
