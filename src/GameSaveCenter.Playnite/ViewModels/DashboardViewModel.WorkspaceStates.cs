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
    /// Real workspace state signals consumed by the shared WorkspaceStatePresenter.
    /// These values are derived from live Worker snapshots and collection state, not
    /// simulated by the UI.
    /// </summary>
    public sealed partial class DashboardViewModel
    {
        private WorkspaceDataState mediaDetailsState = WorkspaceDataState.Empty;
        private DateTime? mediaDetailsLastSuccessUtc;
        private string mediaDetailsErrorMessage = string.Empty;
        private WorkspaceDataState mediaInboxState = WorkspaceDataState.Empty;
        private DateTime? mediaInboxLastSuccessUtc;
        private string mediaInboxErrorMessage = string.Empty;
        private WorkspaceDataState maintenanceState = WorkspaceDataState.Empty;
        private DateTime? maintenanceLastSuccessUtc;
        private string maintenanceErrorMessage = string.Empty;

        public bool IsWorkerOffline => !Snapshot.WorkerHealthy;
        public bool IsCloudDegraded => Snapshot.WorkerHealthy && EffectiveSettings.EnableCloudUpload && !Snapshot.RcloneAvailable;
        public bool IsSaveHistoryLoading => IsBusy && Backups.Count == 0;
        public bool IsTrainerToolsLoading => IsBusy && GameTools.Count == 0;
        public bool IsTrainerCatalogLoading { get => isTrainerCatalogLoading; private set => SetValue(ref isTrainerCatalogLoading, value); }
        public bool IsTrainerReleasesLoading { get => isTrainerReleasesLoading; private set => SetValue(ref isTrainerReleasesLoading, value); }

        public string MediaDetailsState => mediaDetailsState.ToString();
        public string MediaDetailsPresenterState => IsWorkerOffline
            ? WorkspaceDataState.Offline.ToString()
            : mediaDetailsState == WorkspaceDataState.Stale
                ? "Degraded"
                : mediaDetailsState.ToString();
        public string MediaDetailsStateTitle => mediaDetailsState switch
        {
            WorkspaceDataState.Loading => "正在读取当前游戏媒体",
            WorkspaceDataState.Empty => "当前筛选条件没有媒体",
            WorkspaceDataState.Stale => "媒体显示已过期",
            WorkspaceDataState.Error => "媒体读取失败",
            WorkspaceDataState.Offline => "Worker 当前离线",
            _ => string.Empty
        };
        public string MediaDetailsStateMessage => mediaDetailsState switch
        {
            WorkspaceDataState.Loading => "正在读取媒体、来源规则和归档摘要。",
            WorkspaceDataState.Empty => "导入截图或录像后，它们会显示在这里。",
            WorkspaceDataState.Stale => "仍保留上次成功读取的内容；本次刷新没有覆盖它。",
            WorkspaceDataState.Error => "当前游戏媒体暂时无法读取，请稍后重试。",
            WorkspaceDataState.Offline => "媒体列表和归类操作暂时不可用，Worker 恢复后可重新读取。",
            _ => string.Empty
        };
        public string MediaDetailsStateDetail => FormatStateDetail(mediaDetailsLastSuccessUtc, mediaDetailsErrorMessage);
        public bool MediaDetailsStateOverlayVisible => IsWorkerOffline
            || mediaDetailsState == WorkspaceDataState.Loading
            || (mediaDetailsState == WorkspaceDataState.Error && Media.Count == 0);
        public bool MediaDetailsStaleVisible => !IsWorkerOffline && mediaDetailsState == WorkspaceDataState.Stale;

        public string MediaInboxState => mediaInboxState.ToString();
        public string MediaInboxCountDisplay
            => IsWorkerOffline
                ? (mediaInboxLastSuccessUtc.HasValue ? "缓存" : "—")
                : mediaInboxState == WorkspaceDataState.Loading || mediaInboxState == WorkspaceDataState.Error || !mediaInboxLastSuccessUtc.HasValue
                    ? "—"
                    : MediaInboxItems.Count.ToString();
        public string MediaInboxCountCaption
            => IsWorkerOffline
                ? (mediaInboxLastSuccessUtc.HasValue ? $"离线 · 缓存于 {mediaInboxLastSuccessUtc.Value.ToLocalTime():MM-dd HH:mm}" : "离线 · 无法读取")
                : mediaInboxState == WorkspaceDataState.Loading
                    ? "正在读取 · 来源文件始终保留"
                    : mediaInboxState == WorkspaceDataState.Error || !mediaInboxLastSuccessUtc.HasValue
                        ? "无法读取 · 尚未确认数量"
                        : mediaInboxState == WorkspaceDataState.Stale
                            ? $"缓存 · 上次成功 {mediaInboxLastSuccessUtc.Value.ToLocalTime():MM-dd HH:mm}"
                            : "待归类 · 来源文件始终保留";
        public string MediaInboxPresenterState => IsWorkerOffline
            ? WorkspaceDataState.Offline.ToString()
            : mediaInboxState == WorkspaceDataState.Stale
                ? "Degraded"
                : mediaInboxState.ToString();
        public string MediaInboxStateTitle => IsWorkerOffline
            ? "无法读取待归类媒体"
            : mediaInboxState switch
        {
            WorkspaceDataState.Loading => "正在读取媒体收件箱",
            WorkspaceDataState.Empty => MediaInboxTitle,
            WorkspaceDataState.Stale => "媒体收件箱显示已过期",
            WorkspaceDataState.Error => "媒体收件箱读取失败",
            WorkspaceDataState.Offline => "Worker 当前离线",
            _ => string.Empty
        };
        public string MediaInboxStateMessage => IsWorkerOffline
            ? (mediaInboxLastSuccessUtc.HasValue
                ? "Worker 当前离线；列表保留上次成功读取的缓存，恢复连接后才能确认最新数量。"
                : "Worker 当前离线；恢复连接后才能确认列表是否为空。")
            : mediaInboxState switch
        {
            WorkspaceDataState.Loading => "正在读取待归类和已忽略媒体；已有内容会保留到新结果确认后。",
            WorkspaceDataState.Empty => MediaInboxEmptyText,
            WorkspaceDataState.Stale => "仍保留上次成功读取的列表；本次刷新没有覆盖它。",
            WorkspaceDataState.Error => "当前收件箱暂时无法读取，请稍后重试。",
            WorkspaceDataState.Offline => "媒体收件箱暂时不可用；Worker 恢复后可重新读取列表。",
            _ => string.Empty
        };
        public string MediaInboxStateDetail => FormatStateDetail(mediaInboxLastSuccessUtc, mediaInboxErrorMessage);
        public bool MediaInboxStateOverlayVisible => IsWorkerOffline
            || mediaInboxState == WorkspaceDataState.Loading
            || (mediaInboxState == WorkspaceDataState.Error && MediaInboxItems.Count == 0);
        public bool MediaInboxStaleVisible => !IsWorkerOffline && mediaInboxState == WorkspaceDataState.Stale;

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
            mediaDetailsState = WorkspaceDataState.Loading;
            mediaDetailsErrorMessage = string.Empty;
            NotifyMediaDetailsStateChanged();
        }

        private void CompleteMediaDetailsLoad(long generation = 0)
        {
            if (generation != 0 && generation != Interlocked.Read(ref mediaPageGeneration)) return;
            mediaDetailsLastSuccessUtc = DateTime.UtcNow;
            mediaDetailsErrorMessage = string.Empty;
            mediaDetailsState = Media.Count == 0 ? WorkspaceDataState.Empty : WorkspaceDataState.Ready;
            NotifyMediaDetailsStateChanged();
        }

        private void FailMediaDetailsLoad(Exception error, long generation = 0)
        {
            if (generation != 0 && generation != Interlocked.Read(ref mediaPageGeneration)) return;
            mediaDetailsErrorMessage = error?.Message ?? "未知错误";
            mediaDetailsState = mediaDetailsLastSuccessUtc.HasValue || Media.Count > 0
                ? WorkspaceDataState.Stale
                : WorkspaceDataState.Error;
            NotifyMediaDetailsStateChanged();
        }

        private void CancelMediaDetailsLoad(long generation = 0)
        {
            if (generation != 0 && generation != Interlocked.Read(ref mediaPageGeneration)) return;
            mediaDetailsState = mediaDetailsLastSuccessUtc.HasValue
                ? (Media.Count == 0 ? WorkspaceDataState.Empty : WorkspaceDataState.Ready)
                : WorkspaceDataState.Empty;
            NotifyMediaDetailsStateChanged();
        }

        private void BeginMediaInboxLoad(string mode, long generation = 0)
        {
            if (!string.Equals(MediaInboxMode, mode, StringComparison.Ordinal)
                || (generation != 0 && generation != Interlocked.Read(ref mediaInboxLoadGeneration))) return;
            mediaInboxState = WorkspaceDataState.Loading;
            mediaInboxErrorMessage = string.Empty;
            NotifyMediaInboxStateChanged();
        }

        private void CompleteMediaInboxLoad(string mode, long generation = 0)
        {
            if (!string.Equals(MediaInboxMode, mode, StringComparison.Ordinal)
                || (generation != 0 && generation != Interlocked.Read(ref mediaInboxLoadGeneration))) return;
            mediaInboxLastSuccessUtc = DateTime.UtcNow;
            mediaInboxErrorMessage = string.Empty;
            mediaInboxState = MediaInboxItems.Count == 0 ? WorkspaceDataState.Empty : WorkspaceDataState.Ready;
            NotifyMediaInboxStateChanged();
        }

        private void FailMediaInboxLoad(string mode, Exception error, long generation = 0)
        {
            if (!string.Equals(MediaInboxMode, mode, StringComparison.Ordinal)
                || (generation != 0 && generation != Interlocked.Read(ref mediaInboxLoadGeneration))) return;
            mediaInboxErrorMessage = error?.Message ?? "未知错误";
            mediaInboxState = mediaInboxLastSuccessUtc.HasValue || MediaInboxItems.Count > 0
                ? WorkspaceDataState.Stale
                : WorkspaceDataState.Error;
            NotifyMediaInboxStateChanged();
        }

        private void CancelMediaInboxLoad(string mode, long generation = 0)
        {
            if (!string.Equals(MediaInboxMode, mode, StringComparison.Ordinal)
                || (generation != 0 && generation != Interlocked.Read(ref mediaInboxLoadGeneration))) return;
            mediaInboxState = mediaInboxLastSuccessUtc.HasValue
                ? (MediaInboxItems.Count == 0 ? WorkspaceDataState.Empty : WorkspaceDataState.Ready)
                : WorkspaceDataState.Empty;
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
