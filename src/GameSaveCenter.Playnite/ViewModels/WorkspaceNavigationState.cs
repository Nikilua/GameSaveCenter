using System;
using System.Collections.Generic;

namespace GameSaveCenter.Playnite.ViewModels
{
    /// <summary>
    /// A short-lived in-memory route snapshot. It deliberately contains only UI state;
    /// it is never persisted and never carries a file path or a write-operation token.
    /// </summary>
    public sealed class WorkspaceNavigationSnapshot
    {
        public WorkspaceNavigationSnapshot(
            WorkspaceKind workspace,
            string selectedGameId,
            int saveTabIndex,
            int mediaTabIndex,
            int maintenanceTabIndex,
            string taskSearchText,
            string taskStatusFilter,
            string taskGameFilter,
            string taskTypeFilter,
            string taskHistoryScope,
            string taskHistoryRange,
            string taskNavigationGameId,
            string taskNavigationGameName,
            string selectedTaskId,
            int selectedTaskIndex,
            string selectedFindingKey,
            double taskScrollOffset,
            double maintenanceFindingsScrollOffset,
            string returnLabel,
            string sourceSummary)
        {
            Workspace = workspace;
            SelectedGameId = selectedGameId ?? string.Empty;
            SaveTabIndex = saveTabIndex;
            MediaTabIndex = mediaTabIndex;
            MaintenanceTabIndex = maintenanceTabIndex;
            TaskSearchText = taskSearchText ?? string.Empty;
            TaskStatusFilter = taskStatusFilter ?? "全部";
            TaskGameFilter = taskGameFilter ?? "全部";
            TaskTypeFilter = taskTypeFilter ?? "全部";
            TaskHistoryScope = taskHistoryScope ?? "最近任务";
            TaskHistoryRange = taskHistoryRange ?? "全部时间";
            TaskNavigationGameId = taskNavigationGameId ?? string.Empty;
            TaskNavigationGameName = taskNavigationGameName ?? string.Empty;
            SelectedTaskId = selectedTaskId ?? string.Empty;
            SelectedTaskIndex = selectedTaskIndex;
            SelectedFindingKey = selectedFindingKey ?? string.Empty;
            TaskScrollOffset = NormalizeOffset(taskScrollOffset);
            MaintenanceFindingsScrollOffset = NormalizeOffset(maintenanceFindingsScrollOffset);
            ReturnLabel = string.IsNullOrWhiteSpace(returnLabel) ? "返回来源" : returnLabel;
            SourceSummary = sourceSummary ?? string.Empty;
        }

        public WorkspaceKind Workspace { get; }
        public string SelectedGameId { get; }
        public int SaveTabIndex { get; }
        public int MediaTabIndex { get; }
        public int MaintenanceTabIndex { get; }
        public string TaskSearchText { get; }
        public string TaskStatusFilter { get; }
        public string TaskGameFilter { get; }
        public string TaskTypeFilter { get; }
        public string TaskHistoryScope { get; }
        public string TaskHistoryRange { get; }
        public string TaskNavigationGameId { get; }
        public string TaskNavigationGameName { get; }
        public string SelectedTaskId { get; }
        public int SelectedTaskIndex { get; }
        public string SelectedFindingKey { get; }
        public double TaskScrollOffset { get; }
        public double MaintenanceFindingsScrollOffset { get; }
        public string ReturnLabel { get; }
        public string SourceSummary { get; }

        private static double NormalizeOffset(double value)
            => double.IsNaN(value) || double.IsInfinity(value) || value < 0 ? 0 : value;
    }

    /// <summary>Small LIFO route history for purpose navigation inside one dashboard session.</summary>
    public sealed class WorkspaceNavigationStack
    {
        private readonly Stack<WorkspaceNavigationSnapshot> entries = new Stack<WorkspaceNavigationSnapshot>();

        public int Count => entries.Count;
        public bool CanReturn => entries.Count > 0;

        public void Push(WorkspaceNavigationSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            entries.Push(snapshot);
        }

        public bool TryPeek(out WorkspaceNavigationSnapshot snapshot)
        {
            if (entries.Count == 0)
            {
                snapshot = null!;
                return false;
            }

            snapshot = entries.Peek();
            return true;
        }

        public bool TryPop(out WorkspaceNavigationSnapshot snapshot)
        {
            if (entries.Count == 0)
            {
                snapshot = null!;
                return false;
            }

            snapshot = entries.Pop();
            return true;
        }
    }
}
