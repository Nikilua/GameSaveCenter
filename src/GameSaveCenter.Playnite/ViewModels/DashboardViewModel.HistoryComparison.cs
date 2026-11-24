using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Infrastructure;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed partial class DashboardViewModel
    {
        private string? pinnedHistoryGameId;
        private IReadOnlyList<BackupVersionDto>? pinnedHistoryChoices;

        public bool HasPinnedHistoryComparison => pinnedHistoryGameId != null;
        public string PinnedHistoryComparisonSummary
            => $"A：{CompareLeftBackup?.ComparisonRelativeDisplay ?? "待选择"} · B：{CompareRightBackup?.ComparisonRelativeDisplay ?? "待选择"}";
        public IEnumerable<BackupVersionDto> HistoryComparisonChoices => pinnedHistoryChoices ?? (IEnumerable<BackupVersionDto>)Backups;
        public ICommand PinHistoryComparisonACommand { get; private set; } = null!;
        public ICommand PinHistoryComparisonBCommand { get; private set; } = null!;
        public ICommand ClearHistoryComparisonPinsCommand { get; private set; } = null!;
        public ICommand OpenPinnedHistoryComparisonCommand { get; private set; } = null!;

        private void InitializeHistoryComparisonCommands()
        {
            PinHistoryComparisonACommand = new RelayCommand(_ => PinHistoryComparison(true), _ => !IsBusy && SelectedBackup != null);
            PinHistoryComparisonBCommand = new RelayCommand(_ => PinHistoryComparison(false), _ => !IsBusy && SelectedBackup != null);
            ClearHistoryComparisonPinsCommand = new RelayCommand(_ => ClearHistoryComparisonPins(), _ => !IsBusy && HasPinnedHistoryComparison);
            OpenPinnedHistoryComparisonCommand = new RelayCommand(
                _ => Run(OpenPinnedHistoryComparisonAsync), _ => !IsBusy && HasPinnedHistoryComparison && CanCompareSelectedBackups);
        }

        private void PinHistoryComparison(bool baseline)
        {
            var selected = SelectedBackup;
            var gameId = SelectedGame?.PlayniteId;
            if (selected == null || string.IsNullOrWhiteSpace(gameId)
                || !string.Equals(selected.PlayniteId, gameId, StringComparison.OrdinalIgnoreCase)) return;
            var existing = string.Equals(pinnedHistoryGameId, gameId, StringComparison.OrdinalIgnoreCase);
            var left = baseline ? selected : existing ? CompareLeftBackup : null;
            var right = baseline ? existing ? CompareRightBackup : null : selected;
            if (baseline && IsSameBackup(right, selected.BackupId)) right = null;
            if (!baseline && IsSameBackup(left, selected.BackupId)) left = null;
            SetPinnedHistoryComparison(gameId!, left, right);
        }

        private void SetPinnedHistoryComparison(string gameId, BackupVersionDto? left, BackupVersionDto? right)
        {
            pinnedHistoryGameId = gameId;
            var choices = new[] { left, right }.OfType<BackupVersionDto>().ToList();
            var seen = new HashSet<string>(choices.Select(item => item.BackupId), StringComparer.OrdinalIgnoreCase);
            choices.AddRange(Backups.Where(item => seen.Add(item.BackupId)));
            pinnedHistoryChoices = choices;
            OnPropertyChanged(nameof(HistoryComparisonChoices));
            OnPropertyChanged(nameof(HasPinnedHistoryComparison));
            SetComparisonPair(left, right);
        }

        private void ClearHistoryComparisonPins(bool restoreDefault = true)
        {
            if (!HasPinnedHistoryComparison) return;
            pinnedHistoryGameId = null;
            pinnedHistoryChoices = null;
            OnPropertyChanged(nameof(HistoryComparisonChoices));
            OnPropertyChanged(nameof(HasPinnedHistoryComparison));
            SetDefaultComparisonSelection(restoreDefault ? SelectedBackup : null);
        }

        private async Task OpenPinnedHistoryComparisonAsync()
        {
            await CompareBackupAsync();
            if (LastBackupDiff != null && CurrentWorkspace == WorkspaceKind.Saves
                && pinnedHistoryGameId != null && IsSelectedGame(pinnedHistoryGameId))
                SaveTabIndex = 3;
        }

        private async Task<bool> VerifyPinnedHistoryComparisonAsync(string gameId, string leftId, string rightId)
        {
            if (!HasPinnedHistoryComparison) return true;
            if (!string.Equals(pinnedHistoryGameId, gameId, StringComparison.OrdinalIgnoreCase)) return false;
            var left = ReadExactHistoryVersionAsync(gameId, leftId);
            var right = ReadExactHistoryVersionAsync(gameId, rightId);
            await Task.WhenAll(left, right);
            if (CurrentWorkspace != WorkspaceKind.Saves || !IsSelectedGame(gameId)
                || !IsSameBackup(CompareLeftBackup, leftId) || !IsSameBackup(CompareRightBackup, rightId))
                return false;
            var leftMissing = left.Result.Items.Count == 0;
            var rightMissing = right.Result.Items.Count == 0;
            if (!leftMissing && !rightMissing) return true;
            var nextLeft = leftMissing ? null : CompareLeftBackup;
            var nextRight = rightMissing ? null : CompareRightBackup;
            pinnedHistoryChoices = HistoryComparisonChoices.Where(item =>
                !(leftMissing && IsSameBackup(item, leftId)) && !(rightMissing && IsSameBackup(item, rightId))).ToList();
            OnPropertyChanged(nameof(HistoryComparisonChoices));
            SetComparisonPair(nextLeft, nextRight);
            StatusMessage = "所选 A/B 版本已不存在，请重新选择。";
            return false;
        }

        private Task<BackupVersionPageDto> ReadExactHistoryVersionAsync(string gameId, string backupId)
            => plugin.RequestAsync<BackupVersionPageDto>(MessageTypes.ListBackupHistoryPage,
                new BackupHistoryPageQueryDto { PlayniteId = gameId, ExactBackupId = backupId, Limit = 1 });

        private void SetComparisonPair(BackupVersionDto? left, BackupVersionDto? right)
        {
            suppressComparisonSelectionRefresh = true;
            try
            {
                CompareLeftBackup = left;
                CompareRightBackup = right;
            }
            finally { suppressComparisonSelectionRefresh = false; }
            OnComparisonSelectionChanged();
        }
    }
}
