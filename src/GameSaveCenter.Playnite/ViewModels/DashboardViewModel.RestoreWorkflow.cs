using System;
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
        private string restoreExecutionRequestId = string.Empty;
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
            restoreExecutionRequestId = string.Empty;
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
            if (restoreWorkflowSteps.Count != steps.Count)
            {
                restoreWorkflowSteps.Clear();
                foreach (var step in steps) restoreWorkflowSteps.Add(step);
            }
            else
            {
                for (var index = 0; index < steps.Count; index++)
                {
                    var previous = restoreWorkflowSteps[index];
                    var next = steps[index];
                    if (previous.Key != next.Key || previous.Status != next.Status || previous.Detail != next.Detail
                        || previous.DetailFullDisplay != next.DetailFullDisplay || previous.Resolution != next.Resolution)
                        restoreWorkflowSteps[index] = next;
                }
            }
            RestoreWorkflowSummary = RestoreWorkflowProgress.BuildSummary(steps);
            OnPropertyChanged(nameof(RestoreWorkflowSteps));
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
            restoreExecutionRequestId = Guid.NewGuid().ToString("N");
            restoreTask = null;
            restoreExecutionError = string.Empty;
            RefreshRestoreWorkflow();
        }

        private void CompleteRestoreExecution(TaskStatusDto? task, string? error = null)
        {
            restoreExecutionActive = false;
            restoreTask = task;
            restoreExecutionError = error ?? string.Empty;
            restoreExecutionRequestId = string.Empty;
            RefreshRestoreWorkflow();
        }

        private void ObserveRestoreExecutionTask(TaskStatusDto task)
        {
            if (string.IsNullOrEmpty(restoreExecutionRequestId)
                || !string.Equals(task.RequestId, restoreExecutionRequestId, StringComparison.Ordinal)) return;
            if (restoreTask?.State == TaskState.Succeeded || restoreTask?.State == TaskState.Failed
                || restoreTask?.State == TaskState.Cancelled) return;
            restoreTask = task;
            RefreshRestoreWorkflow();
        }
    }
}
