namespace GameSaveCenter.Playnite.ViewModels
{
    /// <summary>
    /// Real workspace state signals consumed by the shared WorkspaceStatePresenter.
    /// These values are derived from live Worker snapshots and collection state, not
    /// simulated by the UI.
    /// </summary>
    public sealed partial class DashboardViewModel
    {
        public bool IsWorkerOffline => !Snapshot.WorkerHealthy;
        public bool IsCloudDegraded => Snapshot.WorkerHealthy && EffectiveSettings.EnableCloudUpload && !Snapshot.RcloneAvailable;
        public bool IsSaveHistoryLoading => IsBusy && Backups.Count == 0;
        public bool IsTrainerToolsLoading => IsBusy && GameTools.Count == 0;

        partial void OnWorkspaceStateInitialize()
        {
            Backups.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsSaveHistoryLoading));
            GameTools.CollectionChanged += (_, _) => OnPropertyChanged(nameof(IsTrainerToolsLoading));
        }

        partial void OnWorkspaceStateInputsChanged()
        {
            OnPropertyChanged(nameof(IsWorkerOffline));
            OnPropertyChanged(nameof(IsCloudDegraded));
            OnPropertyChanged(nameof(IsSaveHistoryLoading));
            OnPropertyChanged(nameof(IsTrainerToolsLoading));
        }
    }
}
