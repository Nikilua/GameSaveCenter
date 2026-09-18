using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GameSaveCenter.Playnite.Infrastructure;
using GameSaveCenter.Playnite.Settings;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed partial class DashboardViewModel
    {
        private bool restoringRecentAccess;

        public BatchObservableCollection<RecentAccessItem> RecentAccessItems { get; }
            = new BatchObservableCollection<RecentAccessItem>();

        public ICommand OpenRecentAccessCommand { get; private set; } = null!;

        partial void OnRecentAccessInitialize()
        {
            OpenRecentAccessCommand = new RelayCommand(
                value => OpenRecentAccess(value as RecentAccessItem),
                value => !IsBusy && value is RecentAccessItem);
        }

        private void RecordRecentAccess()
        {
            if (restoringRecentAccess) return;
            var selected = gamePicker.SelectedGame;
            if (selected == null || string.IsNullOrWhiteSpace(selected.PlayniteId)) return;
            if (!Games.Any(game => string.Equals(game.PlayniteId, selected.PlayniteId, StringComparison.OrdinalIgnoreCase))) return;

            var next = RecentAccessRecord.Upsert(
                plugin.Settings.RecentAccess,
                selected.PlayniteId,
                CurrentWorkspace.ToString(),
                GetCurrentWorkspaceTabIndex(),
                DateTime.UtcNow);
            plugin.Settings.RecentAccess = next;
            uiStateSave?.Schedule();
            RefreshRecentAccessItems(pruneMissing: false);
        }

        private void RefreshRecentAccessItems(bool pruneMissing)
        {
            var records = RecentAccessRecord.NormalizeMany(plugin.Settings.RecentAccess);
            if (pruneMissing)
            {
                var pruned = RecentAccessRecord.KeepExistingGames(
                    plugin.Settings.RecentAccess,
                    Games.Select(game => game.PlayniteId));
                if (!HaveSameRecentAccess(records, pruned))
                {
                    plugin.Settings.RecentAccess = pruned;
                    uiStateSave?.Schedule();
                }
                records = pruned;
            }

            var items = records
                .Select(record =>
                {
                    var game = Games.FirstOrDefault(candidate =>
                        string.Equals(candidate.PlayniteId, record.PlayniteId, StringComparison.OrdinalIgnoreCase));
                    return game == null ? null : new RecentAccessItem(record, game.Name);
                })
                .Where(item => item != null)
                .Cast<RecentAccessItem>()
                .ToList();
            Replace(RecentAccessItems, items, AreSameRecentAccessItem);
        }

        private void OpenRecentAccess(RecentAccessItem? item)
        {
            if (item == null) return;
            var game = Games.FirstOrDefault(candidate =>
                string.Equals(candidate.PlayniteId, item.PlayniteId, StringComparison.OrdinalIgnoreCase));
            if (game == null)
            {
                plugin.Settings.RecentAccess = RecentAccessRecord.KeepExistingGames(
                    plugin.Settings.RecentAccess,
                    Games.Select(candidate => candidate.PlayniteId));
                RefreshRecentAccessItems(pruneMissing: false);
                uiStateSave?.Schedule();
                StatusMessage = "该对象已从当前库移除，已清理最近访问记录。";
                return;
            }

            restoringRecentAccess = true;
            try
            {
                gamePicker.SelectGame(game);
                RestoreWorkspaceTabIndex(item.Workspace, item.TabIndex);
                CurrentWorkspace = ParseWorkspace(item.Workspace);
            }
            finally
            {
                restoringRecentAccess = false;
            }

            RecordRecentAccess();
            RequestWorkspaceLoad();
            StatusMessage = $"已打开最近访问的“{game.Name}”，进入{RecentAccessItem.ToWorkspaceDisplay(item.Workspace)}。";
        }

        private int GetCurrentWorkspaceTabIndex()
            => CurrentWorkspace switch
            {
                WorkspaceKind.Saves => SaveTabIndex,
                WorkspaceKind.Media => MediaTabIndex,
                WorkspaceKind.Maintenance => MaintenanceTabIndex,
                _ => 0
            };

        private void RestoreWorkspaceTabIndex(string workspace, int tabIndex)
        {
            switch (workspace)
            {
                case RecentAccessRecord.SavesWorkspace:
                    SaveTabIndex = tabIndex;
                    break;
                case RecentAccessRecord.MediaWorkspace:
                    MediaTabIndex = tabIndex;
                    break;
                case RecentAccessRecord.MaintenanceWorkspace:
                    MaintenanceTabIndex = tabIndex;
                    break;
            }
        }

        private static WorkspaceKind ParseWorkspace(string workspace)
            => Enum.TryParse(workspace, ignoreCase: false, out WorkspaceKind parsed)
                ? parsed
                : WorkspaceKind.Overview;

        private static bool HaveSameRecentAccess(
            IReadOnlyList<RecentAccessRecord> left,
            IReadOnlyList<RecentAccessRecord> right)
        {
            if (left.Count != right.Count) return false;
            for (var index = 0; index < left.Count; index++)
            {
                if (!string.Equals(left[index].PlayniteId, right[index].PlayniteId, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(left[index].Workspace, right[index].Workspace, StringComparison.Ordinal)
                    || left[index].TabIndex != right[index].TabIndex
                    || left[index].LastAccessUtc != right[index].LastAccessUtc)
                    return false;
            }
            return true;
        }

        private static bool AreSameRecentAccessItem(RecentAccessItem left, RecentAccessItem right)
            => string.Equals(left.PlayniteId, right.PlayniteId, StringComparison.OrdinalIgnoreCase)
               && string.Equals(left.Workspace, right.Workspace, StringComparison.Ordinal)
               && left.TabIndex == right.TabIndex
               && left.LastAccessUtc == right.LastAccessUtc
               && string.Equals(left.GameName, right.GameName, StringComparison.Ordinal);
    }
}
