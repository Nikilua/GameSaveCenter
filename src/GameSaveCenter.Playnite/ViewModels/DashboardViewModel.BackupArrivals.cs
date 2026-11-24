using System;
using System.Collections.Generic;
using System.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed partial class DashboardViewModel
    {
        private IReadOnlyList<BackupVersionDto> pendingBackupArrivals = Array.Empty<BackupVersionDto>();
        private IReadOnlyCollection<BackupVersionDto> recentBackupArrivals = Array.Empty<BackupVersionDto>();

        internal bool IsReadingOlderBackups { get; set; }
        public bool HasPendingBackupArrivals => pendingBackupArrivals.Count > 0;
        public string PendingBackupArrivalText => $"有 {pendingBackupArrivals.Count} 个新版本";
        public IReadOnlyCollection<BackupVersionDto> RecentBackupArrivals => recentBackupArrivals;

        private bool ApplyBackupArrivalPage(BackupVersionPageDto page)
        {
            if (Backups.Count == 0 || page.Items.Count == 0) return false;
            var prefixLength = page.Items.FindIndex(item =>
                string.Equals(item.BackupId, Backups[0].BackupId, StringComparison.Ordinal));
            if (prefixLength < 0 || page.TotalCount != BackupHistoryTotalCount - pendingBackupArrivals.Count + prefixLength
                || page.Items.Count - prefixLength > Backups.Count)
                return false;

            // Keep the loaded tail and its cursor only when the incoming page agrees with
            // the visible history. Reconciliations that edit older rows use the normal path.
            var overlapCount = Math.Min(Backups.Count, page.Items.Count - prefixLength);
            for (var index = 0; index < overlapCount; index++)
                if (!SnapshotComparers.Backup(Backups[index], page.Items[prefixLength + index])) return false;

            if (prefixLength == 0)
            {
                SetPendingBackupArrivals(Array.Empty<BackupVersionDto>());
                backupHistoryTotalCount = page.TotalCount;
                OnPropertyChanged(nameof(BackupHistoryTotalCount));
                OnPropertyChanged(nameof(BackupHistoryRangeSummary));
                return true;
            }

            var arrivals = page.Items.Take(prefixLength).ToArray();
            backupHistoryTotalCount = page.TotalCount;
            if (HasPendingBackupArrivals || IsReadingOlderBackups || backupSearchPage != null
                || arrivals.Any(item => !FilterBackupHistory(item)))
                SetPendingBackupArrivals(arrivals);
            else
                InsertBackupArrivals(arrivals);
            OnPropertyChanged(nameof(BackupHistoryTotalCount));
            OnPropertyChanged(nameof(BackupHistoryRangeSummary));
            return true;
        }

        internal BackupVersionDto? RevealBackupArrivals()
        {
            if (!HasPendingBackupArrivals) return null;
            var arrivals = pendingBackupArrivals;
            SetPendingBackupArrivals(Array.Empty<BackupVersionDto>());
            BackupHistorySearchText = string.Empty;
            BackupHistoryRange = BackupHistoryDateRange.All;
            InsertBackupArrivals(arrivals);
            SelectedBackup = arrivals[0];
            return arrivals[0];
        }

        private void InsertBackupArrivals(IReadOnlyList<BackupVersionDto> arrivals)
        {
            for (var index = 0; index < arrivals.Count; index++) Backups.Insert(index, arrivals[index]);
            recentBackupArrivals = new HashSet<BackupVersionDto>(arrivals);
            OnPropertyChanged(nameof(RecentBackupArrivals));
        }

        private void SetPendingBackupArrivals(IReadOnlyList<BackupVersionDto> arrivals)
        {
            if (pendingBackupArrivals.Count == arrivals.Count
                && pendingBackupArrivals.Zip(arrivals, SnapshotComparers.Backup).All(same => same)) return;
            pendingBackupArrivals = arrivals;
            OnPropertyChanged(nameof(HasPendingBackupArrivals));
            OnPropertyChanged(nameof(PendingBackupArrivalText));
        }

        private void ResetBackupArrivals()
        {
            SetPendingBackupArrivals(Array.Empty<BackupVersionDto>());
            if (recentBackupArrivals.Count == 0) return;
            recentBackupArrivals = Array.Empty<BackupVersionDto>();
            OnPropertyChanged(nameof(RecentBackupArrivals));
        }
    }
}
