using System;
using System.Threading;

namespace GameSaveCenter.Playnite.ViewModels
{
    public enum WorkspaceDataState
    {
        Loading,
        Ready,
        Empty,
        Stale,
        Error,
        Offline
    }

    /// <summary>
    /// Keeps a workspace's cache timestamp and error scoped to the request context that
    /// produced them. A successful load for game A must not make a failed first load for
    /// game B look stale, even when the old collection is still present while the request
    /// is in flight.
    /// </summary>
    internal sealed class MediaWorkspaceStateCache
    {
        public WorkspaceDataState State { get; private set; } = WorkspaceDataState.Empty;
        public DateTime? LastSuccessUtc { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;
        public string ContextKey { get; private set; } = string.Empty;
        public bool HasCurrentContextSuccess => LastSuccessUtc.HasValue;

        public bool SwitchContext(string contextKey)
        {
            contextKey = contextKey ?? string.Empty;
            if (string.Equals(ContextKey, contextKey, StringComparison.Ordinal)) return false;

            ContextKey = contextKey;
            State = WorkspaceDataState.Empty;
            LastSuccessUtc = null;
            ErrorMessage = string.Empty;
            return true;
        }

        public void Begin(string contextKey)
        {
            SwitchContext(contextKey);
            State = WorkspaceDataState.Loading;
            ErrorMessage = string.Empty;
        }

        public void Complete(string contextKey, bool hasItems)
        {
            if (!IsCurrent(contextKey)) return;
            LastSuccessUtc = DateTime.UtcNow;
            ErrorMessage = string.Empty;
            State = hasItems ? WorkspaceDataState.Ready : WorkspaceDataState.Empty;
        }

        public void Fail(string contextKey, string errorMessage)
        {
            if (!IsCurrent(contextKey)) return;
            ErrorMessage = errorMessage ?? "未知错误";
            State = HasCurrentContextSuccess ? WorkspaceDataState.Stale : WorkspaceDataState.Error;
        }

        public void Cancel(string contextKey, bool hasItems)
        {
            if (!IsCurrent(contextKey)) return;
            State = HasCurrentContextSuccess
                ? (hasItems ? WorkspaceDataState.Ready : WorkspaceDataState.Empty)
                : WorkspaceDataState.Empty;
        }

        private bool IsCurrent(string contextKey)
            => string.Equals(ContextKey, contextKey ?? string.Empty, StringComparison.Ordinal);
    }

    /// <summary>
    /// Real workspace state signals consumed by the shared WorkspaceStatePresenter.
    /// These values are derived from live Worker snapshots and collection state, not
    /// simulated by the UI.
    /// </summary>
    public sealed partial class DashboardViewModel
    {
        private readonly MediaWorkspaceStateCache mediaDetailsStateCache = new MediaWorkspaceStateCache();
        private readonly MediaWorkspaceStateCache mediaInboxStateCache = new MediaWorkspaceStateCache();
        private WorkspaceDataState maintenanceState = WorkspaceDataState.Empty;
        private DateTime? maintenanceLastSuccessUtc;
        private string maintenanceErrorMessage = string.Empty;

        public bool IsWorkerOffline => !Snapshot.WorkerHealthy;
        public bool IsCloudDegraded => Snapshot.WorkerHealthy && EffectiveSettings.EnableCloudUpload && !Snapshot.RcloneAvailable;
        public bool IsSaveHistoryLoading => IsBusy && Backups.Count == 0;
        public bool IsTrainerToolsLoading => IsBusy && GameTools.Count == 0;
        public bool IsTrainerCatalogLoading { get => isTrainerCatalogLoading; private set => SetValue(ref isTrainerCatalogLoading, value); }
        public bool IsTrainerReleasesLoading { get => isTrainerReleasesLoading; private set => SetValue(ref isTrainerReleasesLoading, value); }

        public string MediaDetailsState => mediaDetailsStateCache.State.ToString();
        public string MediaDetailsPresenterState => IsWorkerOffline
            ? WorkspaceDataState.Offline.ToString()
            : mediaDetailsStateCache.State == WorkspaceDataState.Stale
                ? "Degraded"
                : mediaDetailsStateCache.State.ToString();
        public string MediaDetailsStateTitle => EffectiveMediaDetailsState switch
        {
            WorkspaceDataState.Loading => "正在读取当前游戏媒体",
            WorkspaceDataState.Empty => "当前筛选条件没有媒体",
            WorkspaceDataState.Stale => "媒体显示已过期",
            WorkspaceDataState.Error => "媒体读取失败",
            WorkspaceDataState.Offline => "Worker 当前离线",
            _ => string.Empty
        };
        public string MediaDetailsStateMessage => EffectiveMediaDetailsState switch
        {
            WorkspaceDataState.Loading => "正在读取媒体、来源规则和归档摘要。",
            WorkspaceDataState.Empty => "导入截图或录像后，它们会显示在这里。",
            WorkspaceDataState.Stale => "仍保留上次成功读取的内容；本次刷新没有覆盖它。",
            WorkspaceDataState.Error => "当前游戏媒体暂时无法读取，请稍后重试。",
            WorkspaceDataState.Offline => "媒体列表和归类操作暂时不可用，Worker 恢复后可重新读取。",
            _ => string.Empty
        };
        public string MediaDetailsStateDetail => FormatStateDetail(mediaDetailsStateCache.LastSuccessUtc, mediaDetailsStateCache.ErrorMessage);
        public bool MediaDetailsStateOverlayVisible => IsWorkerOffline
            || mediaDetailsStateCache.State == WorkspaceDataState.Loading
            || (mediaDetailsStateCache.State == WorkspaceDataState.Error && !mediaDetailsStateCache.HasCurrentContextSuccess);
        public bool MediaDetailsStaleVisible => !IsWorkerOffline && mediaDetailsStateCache.State == WorkspaceDataState.Stale;

        public string MediaInboxState => mediaInboxStateCache.State.ToString();
        public string MediaInboxCountDisplay
            => IsWorkerOffline
                ? (mediaInboxStateCache.HasCurrentContextSuccess ? "缓存" : "—")
                : mediaInboxStateCache.State == WorkspaceDataState.Loading || mediaInboxStateCache.State == WorkspaceDataState.Error || !mediaInboxStateCache.HasCurrentContextSuccess
                    ? "—"
                    : MediaInboxItems.Count.ToString();
        public string MediaInboxCountCaption
            => IsWorkerOffline
                ? (mediaInboxStateCache.HasCurrentContextSuccess ? $"离线 · 缓存于 {mediaInboxStateCache.LastSuccessUtc.GetValueOrDefault().ToLocalTime():MM-dd HH:mm}" : "离线 · 无法读取")
                : mediaInboxStateCache.State == WorkspaceDataState.Loading
                    ? "正在读取 · 来源文件始终保留"
                    : mediaInboxStateCache.State == WorkspaceDataState.Error || !mediaInboxStateCache.HasCurrentContextSuccess
                        ? "无法读取 · 尚未确认数量"
                        : mediaInboxStateCache.State == WorkspaceDataState.Stale
                            ? $"缓存 · 上次成功 {mediaInboxStateCache.LastSuccessUtc.GetValueOrDefault().ToLocalTime():MM-dd HH:mm}"
                            : "待归类 · 来源文件始终保留";
        public string MediaInboxPresenterState => IsWorkerOffline
            ? WorkspaceDataState.Offline.ToString()
            : mediaInboxStateCache.State == WorkspaceDataState.Stale
                ? "Degraded"
                : mediaInboxStateCache.State.ToString();
        public string MediaInboxStateTitle => IsWorkerOffline
            ? "无法读取待归类媒体"
            : mediaInboxStateCache.State switch
        {
            WorkspaceDataState.Loading => "正在读取媒体收件箱",
            WorkspaceDataState.Empty => MediaInboxTitle,
            WorkspaceDataState.Stale => "媒体收件箱显示已过期",
            WorkspaceDataState.Error => "媒体收件箱读取失败",
            WorkspaceDataState.Offline => "Worker 当前离线",
            _ => string.Empty
        };
        public string MediaInboxStateMessage => IsWorkerOffline
            ? (mediaInboxStateCache.HasCurrentContextSuccess
                ? "Worker 当前离线；列表保留上次成功读取的缓存，恢复连接后才能确认最新数量。"
                : "Worker 当前离线；恢复连接后才能确认列表是否为空。")
            : mediaInboxStateCache.State switch
        {
            WorkspaceDataState.Loading => "正在读取待归类和已忽略媒体；已有内容会保留到新结果确认后。",
            WorkspaceDataState.Empty => MediaInboxEmptyText,
            WorkspaceDataState.Stale => "仍保留上次成功读取的列表；本次刷新没有覆盖它。",
            WorkspaceDataState.Error => "当前收件箱暂时无法读取，请稍后重试。",
            WorkspaceDataState.Offline => "媒体收件箱暂时不可用；Worker 恢复后可重新读取列表。",
            _ => string.Empty
        };
        public string MediaInboxStateDetail => FormatStateDetail(mediaInboxStateCache.LastSuccessUtc, mediaInboxStateCache.ErrorMessage);
        public bool MediaInboxStateOverlayVisible => IsWorkerOffline
            || mediaInboxStateCache.State == WorkspaceDataState.Loading
            || (mediaInboxStateCache.State == WorkspaceDataState.Error && !mediaInboxStateCache.HasCurrentContextSuccess);
        public bool MediaInboxStaleVisible => !IsWorkerOffline && mediaInboxStateCache.State == WorkspaceDataState.Stale;

        private WorkspaceDataState EffectiveMediaDetailsState => IsWorkerOffline
            ? WorkspaceDataState.Offline
            : mediaDetailsStateCache.State;

        private string CurrentMediaDetailsContextKey => string.Join("\u001f", new[]
        {
            SelectedGame?.PlayniteId ?? string.Empty,
            MediaFilter ?? string.Empty,
            MediaSearchText ?? string.Empty
        });

        private string CurrentMediaInboxContextKey => MediaInboxMode ?? string.Empty;

        public string MaintenanceState => maintenanceState.ToString();
        public string MaintenancePresenterState => IsWorkerOffline
            ? WorkspaceDataState.Offline.ToString()
            : maintenanceState == WorkspaceDataState.Stale
                ? "Degraded"
                : maintenanceState.ToString();
        public string MaintenanceStateTitle => maintenanceState switch
        {
            WorkspaceDataState.Loading => "正在读取维护信息",
            WorkspaceDataState.Empty => "暂无需要处理的诊断项",
            WorkspaceDataState.Stale => "维护信息显示已过期",
            WorkspaceDataState.Error => "维护信息读取失败",
            WorkspaceDataState.Offline => "Worker 当前离线",
            _ => string.Empty
        };
        public string MaintenanceStateMessage => maintenanceState switch
        {
            WorkspaceDataState.Loading => "正在读取 Worker 设置、进程映射和诊断结果。",
            WorkspaceDataState.Empty => "Worker、备份和媒体状态正常时，这里会保持为空。",
            WorkspaceDataState.Stale => "仍保留上次成功读取的诊断信息；本次刷新没有覆盖它。",
            WorkspaceDataState.Error => "维护信息暂时无法读取，请稍后重试。",
            WorkspaceDataState.Offline => "维护信息暂时不可用；Worker 恢复后可重新读取。",
            _ => string.Empty
        };
        public string MaintenanceStateDetail => FormatStateDetail(maintenanceLastSuccessUtc, maintenanceErrorMessage);
        public bool MaintenanceStateOverlayVisible => IsWorkerOffline
            || maintenanceState == WorkspaceDataState.Loading
            || (maintenanceState == WorkspaceDataState.Error && Findings.Count == 0);
        public bool MaintenanceStaleVisible => !IsWorkerOffline && maintenanceState == WorkspaceDataState.Stale;

        partial void OnWorkspaceStateInitialize()
        {
            Backups.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsSaveHistoryLoading));
            GameTools.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsTrainerToolsLoading));
            Media.CollectionChanged += (_, _) => NotifyMediaDetailsStateChanged();
        }

        partial void OnWorkspaceStateInputsChanged()
        {
            OnPropertyChanged(nameof(IsWorkerOffline));
            OnPropertyChanged(nameof(IsCloudDegraded));
            OnPropertyChanged(nameof(IsSaveHistoryLoading));
            OnPropertyChanged(nameof(IsTrainerToolsLoading));
            NotifyMediaDetailsStateChanged();
            NotifyMediaInboxStateChanged();
            NotifyMaintenanceStateChanged();
            RebuildMaintenanceActionItems();
        }

        private static string FormatStateDetail(DateTime? lastSuccessUtc, string errorMessage)
        {
            var lastSuccess = lastSuccessUtc.HasValue
                ? $"上次成功读取：{lastSuccessUtc.Value.ToLocalTime():yyyy-MM-dd HH:mm}。"
                : string.Empty;
            return string.IsNullOrWhiteSpace(errorMessage)
                ? lastSuccess
                : lastSuccess + (lastSuccess.Length == 0 ? string.Empty : " ") + "本次刷新失败：" + errorMessage;
        }

        private void NotifyMediaDetailsStateChanged()
        {
            OnPropertyChanged(nameof(MediaDetailsState));
            OnPropertyChanged(nameof(MediaDetailsPresenterState));
            OnPropertyChanged(nameof(MediaDetailsStateTitle));
            OnPropertyChanged(nameof(MediaDetailsStateMessage));
            OnPropertyChanged(nameof(MediaDetailsStateDetail));
            OnPropertyChanged(nameof(MediaDetailsStateOverlayVisible));
            OnPropertyChanged(nameof(MediaDetailsStaleVisible));
        }

        /// <summary>
        /// Invalidates the media-detail request context as soon as its game/filter/search
        /// identity changes. This closes the window where an older same-game request could
        /// complete before the debounced replacement request increments the generation.
        /// </summary>
        private void InvalidateMediaDetailsContext()
        {
            Interlocked.Increment(ref mediaPageGeneration);
            CancelMediaPageRequest();
            mediaDetailsStateCache.SwitchContext(CurrentMediaDetailsContextKey);
            ResetMediaPageState();
            NotifyMediaDetailsStateChanged();
        }

        private void NotifyMediaInboxStateChanged()
        {
            OnPropertyChanged(nameof(MediaInboxState));
            OnPropertyChanged(nameof(MediaInboxCountDisplay));
            OnPropertyChanged(nameof(MediaInboxCountCaption));
            OnPropertyChanged(nameof(MediaInboxPresenterState));
            OnPropertyChanged(nameof(MediaInboxStateTitle));
            OnPropertyChanged(nameof(MediaInboxStateMessage));
            OnPropertyChanged(nameof(MediaInboxStateDetail));
            OnPropertyChanged(nameof(MediaInboxStateOverlayVisible));
            OnPropertyChanged(nameof(MediaInboxStaleVisible));
        }

        private void NotifyMaintenanceStateChanged()
        {
            OnPropertyChanged(nameof(MaintenanceState));
            OnPropertyChanged(nameof(MaintenancePresenterState));
            OnPropertyChanged(nameof(MaintenanceStateTitle));
            OnPropertyChanged(nameof(MaintenanceStateMessage));
            OnPropertyChanged(nameof(MaintenanceStateDetail));
            OnPropertyChanged(nameof(MaintenanceStateOverlayVisible));
            OnPropertyChanged(nameof(MaintenanceStaleVisible));
        }

        private void BeginMediaDetailsLoad(long generation = 0)
        {
            if (generation != 0 && generation != Interlocked.Read(ref mediaPageGeneration)) return;
            mediaDetailsStateCache.Begin(CurrentMediaDetailsContextKey);
            NotifyMediaDetailsStateChanged();
        }

        private void CompleteMediaDetailsLoad(long generation = 0)
        {
            if (generation != 0 && generation != Interlocked.Read(ref mediaPageGeneration)) return;
            mediaDetailsStateCache.Complete(CurrentMediaDetailsContextKey, Media.Count > 0);
            NotifyMediaDetailsStateChanged();
        }

        private void FailMediaDetailsLoad(Exception error, long generation = 0)
        {
            if (generation != 0 && generation != Interlocked.Read(ref mediaPageGeneration)) return;
            mediaDetailsStateCache.Fail(CurrentMediaDetailsContextKey, error?.Message ?? "未知错误");
            NotifyMediaDetailsStateChanged();
        }

        private void CancelMediaDetailsLoad(long generation = 0)
        {
            if (generation != 0 && generation != Interlocked.Read(ref mediaPageGeneration)) return;
            mediaDetailsStateCache.Cancel(CurrentMediaDetailsContextKey, Media.Count > 0);
            NotifyMediaDetailsStateChanged();
        }

        private void BeginMediaInboxLoad(string mode, long generation = 0)
        {
            if (!string.Equals(MediaInboxMode, mode, StringComparison.Ordinal)
                || (generation != 0 && generation != Interlocked.Read(ref mediaInboxLoadGeneration))) return;
            mediaInboxStateCache.Begin(CurrentMediaInboxContextKey);
            NotifyMediaInboxStateChanged();
        }

        private void CompleteMediaInboxLoad(string mode, long generation = 0)
        {
            if (!string.Equals(MediaInboxMode, mode, StringComparison.Ordinal)
                || (generation != 0 && generation != Interlocked.Read(ref mediaInboxLoadGeneration))) return;
            mediaInboxStateCache.Complete(CurrentMediaInboxContextKey, MediaInboxItems.Count > 0);
            NotifyMediaInboxStateChanged();
        }

        private void FailMediaInboxLoad(string mode, Exception error, long generation = 0)
        {
            if (!string.Equals(MediaInboxMode, mode, StringComparison.Ordinal)
                || (generation != 0 && generation != Interlocked.Read(ref mediaInboxLoadGeneration))) return;
            mediaInboxStateCache.Fail(CurrentMediaInboxContextKey, error?.Message ?? "未知错误");
            NotifyMediaInboxStateChanged();
        }

        private void CancelMediaInboxLoad(string mode, long generation = 0)
        {
            if (!string.Equals(MediaInboxMode, mode, StringComparison.Ordinal)
                || (generation != 0 && generation != Interlocked.Read(ref mediaInboxLoadGeneration))) return;
            mediaInboxStateCache.Cancel(CurrentMediaInboxContextKey, MediaInboxItems.Count > 0);
            NotifyMediaInboxStateChanged();
        }

        private void BeginMaintenanceLoad()
        {
            maintenanceState = WorkspaceDataState.Loading;
            maintenanceErrorMessage = string.Empty;
            NotifyMaintenanceStateChanged();
        }

        private void CompleteMaintenanceLoad()
        {
            maintenanceLastSuccessUtc = DateTime.UtcNow;
            maintenanceErrorMessage = string.Empty;
            maintenanceState = Findings.Count == 0 ? WorkspaceDataState.Empty : WorkspaceDataState.Ready;
            NotifyMaintenanceStateChanged();
        }

        private void FailMaintenanceLoad(Exception error)
        {
            maintenanceErrorMessage = error?.Message ?? "未知错误";
            maintenanceState = maintenanceLastSuccessUtc.HasValue || Findings.Count > 0
                ? WorkspaceDataState.Stale
                : WorkspaceDataState.Error;
            NotifyMaintenanceStateChanged();
        }

        private void CancelMaintenanceLoad()
        {
            maintenanceState = maintenanceLastSuccessUtc.HasValue
                ? (Findings.Count == 0 ? WorkspaceDataState.Empty : WorkspaceDataState.Ready)
                : WorkspaceDataState.Empty;
            NotifyMaintenanceStateChanged();
        }
    }
}
