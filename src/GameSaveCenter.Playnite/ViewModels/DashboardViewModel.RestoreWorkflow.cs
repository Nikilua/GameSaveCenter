using System.Collections.ObjectModel;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.ViewModels
{
    public sealed partial class DashboardViewModel
    {
        private readonly ObservableCollection<RestoreWorkflowStepState> restoreWorkflowSteps = new ObservableCollection<RestoreWorkflowStepState>();
        private bool restoreReadinessChecking;
        private bool restoreTargetConfirmed;
        private bool restoreExecutionActive;
        private TaskStatusDto? restoreTask;
        private string restoreReadinessError = string.Empty;
        private string restoreExecutionError = string.Empty;
        private string restoreWorkflowSummary = "选择一个版本后，按顺序完成四个阶段。";

        public ObservableCollection<RestoreWorkflowStepState> RestoreWorkflowSteps => restoreWorkflowSteps;
        public string RestoreWorkflowSummary
        {
            get => restoreWorkflowSummary;
            private set => SetValue(ref restoreWorkflowSummary, value);
        }
        public bool RestoreWorkflowHasFailure
        {
            get
            {
                foreach (var step in restoreWorkflowSteps)
                    if (step.IsFailed) return true;
                return false;
            }
        }

        private void ResetRestoreWorkflow()
        {
            restoreReadinessChecking = false;
            restoreTargetConfirmed = false;
            restoreExecutionActive = false;
            restoreTask = null;
            restoreReadinessError = string.Empty;
            restoreExecutionError = string.Empty;
            RefreshRestoreWorkflow();
        }

        private void RefreshRestoreWorkflow()
        {
            var steps = RestoreWorkflowProgress.Build(
                SelectedBackup,
                restoreReadinessChecking,
                restoreTargetConfirmed,
                restoreExecutionActive,
                restoreTask,
                restoreReadinessError,
                restoreExecutionError);
            restoreWorkflowSteps.Clear();
            foreach (var step in steps) restoreWorkflowSteps.Add(step);
            RestoreWorkflowSummary = RestoreWorkflowProgress.BuildSummary(steps);
            OnPropertyChanged(nameof(RestoreWorkflowHasFailure));
        }

        private void BeginRestoreReadinessCheck()
        {
            restoreReadinessChecking = true;
            restoreReadinessError = string.Empty;
            RefreshRestoreWorkflow();
        }

        private void CompleteRestoreReadinessCheck(string? error = null)
        {
            restoreReadinessChecking = false;
            restoreReadinessError = error ?? string.Empty;
            RefreshRestoreWorkflow();
        }

        private void BeginRestoreExecution()
        {
            restoreTargetConfirmed = true;
            restoreExecutionActive = true;
            restoreTask = null;
            restoreExecutionError = string.Empty;
            RefreshRestoreWorkflow();
        }

        private void CompleteRestoreExecution(TaskStatusDto? task, string? error = null)
        {
            restoreExecutionActive = false;
            restoreTask = task;
            restoreExecutionError = error ?? string.Empty;
            RefreshRestoreWorkflow();
        }
    }
}
