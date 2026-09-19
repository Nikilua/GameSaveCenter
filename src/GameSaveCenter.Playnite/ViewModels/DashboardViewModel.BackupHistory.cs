using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed partial class DashboardViewModel
    {
        private string backupHistoryRange = BackupHistoryDateRange.All;

        public ICollectionView BackupHistoryView { get; private set; } = null!;

        public IReadOnlyList<string> BackupHistoryRangeOptions => BackupHistoryDateRange.Options;

        public string BackupHistoryRange
        {
            get => backupHistoryRange;
            set
            {
                var normalized = BackupHistoryDateRange.Options.Contains(value)
                    ? value
                    : BackupHistoryDateRange.All;
                if (string.Equals(backupHistoryRange, normalized, StringComparison.Ordinal)) return;

                backupHistoryRange = normalized;
                OnPropertyChanged(nameof(BackupHistoryRange));
                RefreshBackupHistoryView();
                OnPropertyChanged(nameof(BackupHistoryHasActiveRange));
                OnPropertyChanged(nameof(BackupHistoryRangeSummary));
                RaiseCommandStates();
            }
        }

        public bool BackupHistoryHasActiveRange
            => !string.Equals(BackupHistoryRange, BackupHistoryDateRange.All, StringComparison.Ordinal);

        public string BackupHistoryRangeSummary
        {
            get
            {
                var total = Backups.Count;
                var visible = BackupHistoryView == null
                    ? total
                    : BackupHistoryView.Cast<BackupVersionDto>().Count();
                var range = BackupHistoryDateRange.Describe(BackupHistoryRange, DateTime.Now);
                return string.Equals(range, BackupHistoryDateRange.All, StringComparison.Ordinal)
                    ? $"{range} · 共 {total} 个版本"
                    : $"{range} · 显示 {visible}/{total} 个版本";
            }
        }

        public ICommand ClearBackupHistoryRangeCommand { get; private set; } = null!;
        public ICommand JumpToRecentBackupCommand { get; private set; } = null!;
        public ICommand JumpToEarlierBackupCommand { get; private set; } = null!;

        partial void OnBackupHistoryInitialize()
        {
            BackupHistoryView = new CollectionViewSource { Source = Backups }.View;
            BackupHistoryView.Filter = FilterBackupHistory;
            Backups.CollectionChanged += OnBackupHistoryCollectionChanged;
            ClearBackupHistoryRangeCommand = new RelayCommand(
                _ => ClearBackupHistoryRange(),
                _ => !IsBusy && BackupHistoryHasActiveRange);
            JumpToRecentBackupCommand = new RelayCommand(
                _ => JumpToBackup(true),
                _ => !IsBusy && CanJumpBackupHistory());
            JumpToEarlierBackupCommand = new RelayCommand(
                _ => JumpToBackup(false),
                _ => !IsBusy && CanJumpBackupHistory());
        }

        private bool FilterBackupHistory(object item)
            => item is BackupVersionDto backup
               && BackupHistoryDateRange.Matches(backup, BackupHistoryRange, DateTime.Now);

        private void OnBackupHistoryCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(BackupHistoryRangeSummary));
            RaiseCommandStates();
        }

        private void RefreshBackupHistoryView()
        {
            BackupHistoryView?.Refresh();
        }

        private void ClearBackupHistoryRange()
        {
            BackupHistoryRange = BackupHistoryDateRange.All;
            StatusMessage = "已清空历史日期范围，恢复显示全部版本。";
        }

        private bool CanJumpBackupHistory()
            => BackupHistoryView != null && BackupHistoryView.Cast<BackupVersionDto>().Any();

        private void JumpToBackup(bool recentFirst)
        {
            var target = BackupHistoryDateRange.OrderForNavigation(
                    BackupHistoryView.Cast<BackupVersionDto>(), recentFirst)
                .FirstOrDefault();
            if (target == null)
            {
                StatusMessage = $"当前{BackupHistoryRange}没有可跳转的历史版本。";
                return;
            }

            SelectedBackup = target;
            StatusMessage = recentFirst
                ? $"已跳到最近版本：{target.CreatedLocal:yyyy-MM-dd HH:mm:ss}。"
                : $"已跳到较早版本：{target.CreatedLocal:yyyy-MM-dd HH:mm:ss}。";
        }
    }
}
