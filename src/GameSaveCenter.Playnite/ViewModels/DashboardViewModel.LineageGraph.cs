using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GameSaveCenter.Playnite.Infrastructure;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed partial class DashboardViewModel
    {
        private bool showBackupLineageGraph;
        private BackupLineageGraph? backupLineageGraph;
        private BackupLineageGraphNode? selectedLineageGraphNode;

        public bool ShowBackupLineageGraph => showBackupLineageGraph;
        public IReadOnlyList<BackupLineageGraphNode> BackupLineageGraphRoots
            => backupLineageGraph?.Roots ?? Array.Empty<BackupLineageGraphNode>();
        public string BackupLineageGraphSummary => backupLineageGraph?.Summary ?? string.Empty;
        public BackupLineageGraphNode? SelectedLineageGraphNode
        {
            get => selectedLineageGraphNode;
            set
            {
                SetValue(ref selectedLineageGraphNode, value);
                RaiseCommandStates();
            }
        }
        public ICommand ToggleBackupLineageGraphCommand { get; private set; } = null!;
        public ICommand CompareLineageGraphNodeCommand { get; private set; } = null!;

        private void InitializeLineageGraphCommands()
        {
            ToggleBackupLineageGraphCommand = new RelayCommand(_ =>
            {
                showBackupLineageGraph = !showBackupLineageGraph;
                OnPropertyChanged(nameof(ShowBackupLineageGraph));
                RefreshBackupLineageGraph();
            });
            CompareLineageGraphNodeCommand = new RelayCommand(_ => CompareLineageGraphNode(),
                _ => !IsBusy && IsValidComparisonSelection(SelectedBackup, SelectedLineageGraphNode?.Backup));
        }

        private void RefreshBackupLineageGraph()
        {
            if (!showBackupLineageGraph) return;
            backupLineageGraph = BackupLineageGraphBuilder.Build(SelectedBackup,
                Backups.Concat(backupSearchResults), BackupHistoryTotalCount);
            OnPropertyChanged(nameof(BackupLineageGraphRoots));
            OnPropertyChanged(nameof(BackupLineageGraphSummary));
            SelectedLineageGraphNode = backupLineageGraph.Selected;
        }

        private void CompareLineageGraphNode()
        {
            var anchor = SelectedBackup;
            var node = SelectedLineageGraphNode?.Backup;
            var gameId = SelectedGame?.PlayniteId;
            if (node == null || anchor == null || string.IsNullOrEmpty(gameId)
                || !string.Equals(anchor.PlayniteId, gameId, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(node.PlayniteId, gameId, StringComparison.OrdinalIgnoreCase)) return;
            SetPinnedHistoryComparison(gameId!, anchor, node);
            Run(OpenPinnedHistoryComparisonAsync);
        }
    }
}
