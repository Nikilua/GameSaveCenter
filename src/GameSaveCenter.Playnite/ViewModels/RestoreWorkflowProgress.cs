using System;
using System.Collections.Generic;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.ViewModels
{
    public enum RestoreWorkflowStepStatus
    {
        Pending,
        Active,
        Complete,
        Warning,
        Failed,
        Cancelled
    }

    public sealed class RestoreWorkflowStepState
    {
        public RestoreWorkflowStepState(string key, string title, RestoreWorkflowStepStatus status, string detail)
        {
            Key = key;
            Title = title;
            Status = status;
            Detail = detail ?? string.Empty;
        }

        public string Key { get; }
        public string Title { get; }
        public RestoreWorkflowStepStatus Status { get; }
        public string Detail { get; }
        public bool IsCompleted => Status == RestoreWorkflowStepStatus.Complete || Status == RestoreWorkflowStepStatus.Warning;
        public bool IsCurrent => Status == RestoreWorkflowStepStatus.Active
            || Status == RestoreWorkflowStepStatus.Failed
            || Status == RestoreWorkflowStepStatus.Cancelled;
        public bool IsFailed => Status == RestoreWorkflowStepStatus.Failed;
        public string StatusDisplay => Status switch
        {
            RestoreWorkflowStepStatus.Pending => "待处理",
            RestoreWorkflowStepStatus.Active => "进行中",
            RestoreWorkflowStepStatus.Complete => "已完成",
            RestoreWorkflowStepStatus.Warning => "需注意",
            RestoreWorkflowStepStatus.Failed => "失败",
            RestoreWorkflowStepStatus.Cancelled => "已取消",
            _ => "待处理"
        };
        public string Glyph => Status switch
        {
            RestoreWorkflowStepStatus.Pending => "○",
            RestoreWorkflowStepStatus.Active => "…",
            RestoreWorkflowStepStatus.Complete => "✓",
            RestoreWorkflowStepStatus.Warning => "!",
            RestoreWorkflowStepStatus.Failed => "×",
            RestoreWorkflowStepStatus.Cancelled => "—",
            _ => "○"
        };
    }

    /// <summary>
    /// Projects the real restore/readiness/task results into a small, inspectable workflow.
    /// This is presentation state only; the Worker remains the source of truth for writes,
    /// PreRestore creation and rollback.
    /// </summary>
    internal static class RestoreWorkflowProgress
    {
        public static IReadOnlyList<RestoreWorkflowStepState> Build(
            BackupVersionDto? backup,
            bool readinessChecking,
            bool targetConfirmed,
            bool executionActive,
            TaskStatusDto? task,
            string readinessError,
            string executionError)
        {
            var selected = backup != null && !string.IsNullOrWhiteSpace(backup.BackupId);
            var steps = new List<RestoreWorkflowStepState>(4);
            steps.Add(BuildSelectionStep(backup, selected));

            var readiness = BuildReadinessStep(backup, selected, readinessChecking, readinessError);
            steps.Add(readiness);

            var targetFailure = IsTargetFailure(task?.ErrorCode);
            var target = BuildTargetStep(selected, targetConfirmed, executionActive, task, targetFailure);
            steps.Add(target);
            steps.Add(BuildExecutionStep(selected, executionActive, task, readiness, target, executionError));
            return steps;
        }

        public static string BuildSummary(IReadOnlyList<RestoreWorkflowStepState> steps)
        {
            if (steps == null || steps.Count == 0 || !steps[0].IsCompleted)
                return "选择一个版本后，按顺序完成四个阶段。";

            var failed = First(steps, RestoreWorkflowStepStatus.Failed);
            if (failed != null)
                return $"恢复流程停在“{failed.Title}”：{failed.StatusDisplay}。当前步骤和失败详情会保留。";

            var cancelled = First(steps, RestoreWorkflowStepStatus.Cancelled);
            if (cancelled != null)
                return $"恢复流程在“{cancelled.Title}”已取消；当前步骤和任务结果会保留。";

            var active = First(steps, RestoreWorkflowStepStatus.Active);
            if (active != null)
                return $"恢复流程正在“{active.Title}”；执行阶段先完成保护备份并锁定 PreRestore，再写入当前存档目录。";

            var pending = steps[0].IsCompleted ? FirstPending(steps) : null;
            return pending == null
                ? "四个恢复阶段已完成；任务结果仍可在任务中心追溯。"
                : $"下一步：{pending.Title}。执行阶段才会写入当前存档目录。";
        }

        private static RestoreWorkflowStepState BuildSelectionStep(BackupVersionDto? backup, bool selected)
            => !selected
                ? new RestoreWorkflowStepState("selection", "选择版本", RestoreWorkflowStepStatus.Pending, "请先从历史列表选择一个稳定 ID 的版本。")
                : new RestoreWorkflowStepState(
                    "selection",
                    "选择版本",
                    RestoreWorkflowStepStatus.Complete,
                    $"已选择 {backup!.CreatedLocal:yyyy-MM-dd HH:mm:ss} · {backup.BackupTypeDisplay} · {backup.BackupId}");

        private static RestoreWorkflowStepState BuildReadinessStep(
            BackupVersionDto? backup,
            bool selected,
            bool checking,
            string error)
        {
            if (!selected)
                return new RestoreWorkflowStepState("readiness", "可恢复性检查", RestoreWorkflowStepStatus.Pending, "选择版本后才能读取归档并检查文件清单。此检查只使用隔离目录，不写入当前存档。");
            if (checking || backup!.RestoreReadiness?.Status == RestoreReadinessStatus.Checking)
                return new RestoreWorkflowStepState("readiness", "可恢复性检查", RestoreWorkflowStepStatus.Active, "Worker 正在隔离目录读取归档、清点文件并执行可用的完整性检查。");
            if (!string.IsNullOrWhiteSpace(error))
                return new RestoreWorkflowStepState("readiness", "可恢复性检查", RestoreWorkflowStepStatus.Failed, error);

            var result = backup.RestoreReadiness;
            if (result == null || result.Status == RestoreReadinessStatus.Unknown)
                return new RestoreWorkflowStepState("readiness", "可恢复性检查", RestoreWorkflowStepStatus.Pending, "尚未检查该版本；验证不会覆盖当前存档。");

            var detail = string.IsNullOrWhiteSpace(result.Summary) ? result.StatusDisplay : result.Summary;
            detail += $" 文件 {result.ActualFileCount}/{result.ExpectedFileCount}，大小 {FormatBytes(result.ActualTotalSize)}/{FormatBytes(result.ExpectedTotalSize)}。";
            return result.Status switch
            {
                RestoreReadinessStatus.Ready => new RestoreWorkflowStepState("readiness", "可恢复性检查", RestoreWorkflowStepStatus.Complete, detail),
                RestoreReadinessStatus.Warning => new RestoreWorkflowStepState("readiness", "可恢复性检查", RestoreWorkflowStepStatus.Warning, detail + " 执行前仍会再次预览目标版本。"),
                RestoreReadinessStatus.Corrupted or RestoreReadinessStatus.Unsupported or RestoreReadinessStatus.Failed
                    => new RestoreWorkflowStepState("readiness", "可恢复性检查", RestoreWorkflowStepStatus.Failed, detail + " 请重新验证或处理归档后再恢复。"),
                _ => new RestoreWorkflowStepState("readiness", "可恢复性检查", RestoreWorkflowStepStatus.Warning, detail)
            };
        }

        private static RestoreWorkflowStepState BuildTargetStep(
            bool selected,
            bool confirmed,
            bool executing,
            TaskStatusDto? task,
            bool targetFailure)
        {
            if (!selected)
                return new RestoreWorkflowStepState("target", "目标核对", RestoreWorkflowStepStatus.Pending, "选择版本后确认游戏、启动器和 MOD 管理器均已关闭。");
            if (targetFailure)
                return new RestoreWorkflowStepState("target", "目标核对", RestoreWorkflowStepStatus.Failed, task!.DetailMessage);
            if (task?.State == TaskState.Succeeded)
                return new RestoreWorkflowStepState("target", "目标核对", RestoreWorkflowStepStatus.Complete, "Worker 已完成游戏关闭检查，恢复任务进入了安全执行链路。");
            if (task?.State == TaskState.Cancelled)
                return new RestoreWorkflowStepState("target", "目标核对", RestoreWorkflowStepStatus.Warning, "任务已取消；目标是否已完成核对请查看任务详情。");
            if (confirmed || executing)
                return new RestoreWorkflowStepState("target", "目标核对", RestoreWorkflowStepStatus.Active, "已确认当前状态可创建快照；Worker 执行前仍会再次确认游戏进程已关闭。");
            return new RestoreWorkflowStepState("target", "目标核对", RestoreWorkflowStepStatus.Pending, "开始恢复前会要求确认关闭游戏、启动器和 MOD 管理器。");
        }

        private static RestoreWorkflowStepState BuildExecutionStep(
            bool selected,
            bool executing,
            TaskStatusDto? task,
            RestoreWorkflowStepState readiness,
            RestoreWorkflowStepState target,
            string error)
        {
            if (!selected)
                return new RestoreWorkflowStepState("execution", "执行结果", RestoreWorkflowStepStatus.Pending, "完成前面的阶段后才会进入恢复执行。");
            if (readiness.IsFailed || target.IsFailed)
                return new RestoreWorkflowStepState("execution", "执行结果", RestoreWorkflowStepStatus.Pending, "前置阶段未通过，尚未进入写入当前存档的阶段。 ");
            if (!string.IsNullOrWhiteSpace(error))
                return new RestoreWorkflowStepState("execution", "执行结果", RestoreWorkflowStepStatus.Failed, error);
            if (executing || task?.State == TaskState.Queued || task?.State == TaskState.Running || task?.State == TaskState.WaitingForUser)
                return new RestoreWorkflowStepState("execution", "执行结果", RestoreWorkflowStepStatus.Active, "当前阶段：保护备份。Worker 会先创建并锁定当前状态的 PreRestore；随后才预览、写入并执行恢复后校验。");
            if (task?.State == TaskState.Succeeded)
                return new RestoreWorkflowStepState("execution", "执行结果", RestoreWorkflowStepStatus.Complete, string.IsNullOrWhiteSpace(task.DetailMessage) ? "安全恢复完成。" : task.DetailMessage);
            if (task?.State == TaskState.Cancelled)
                return new RestoreWorkflowStepState("execution", "执行结果", RestoreWorkflowStepStatus.Cancelled, string.IsNullOrWhiteSpace(task.DetailMessage) ? "恢复任务已取消；请查看任务详情确认当前状态。" : task.DetailMessage);
            if (task?.State == TaskState.Failed)
                return new RestoreWorkflowStepState("execution", "执行结果", RestoreWorkflowStepStatus.Failed, BuildExecutionFailureDetail(task));
            return new RestoreWorkflowStepState("execution", "执行结果", RestoreWorkflowStepStatus.Pending, "尚未开始写入当前存档。");
        }

        private static string BuildExecutionFailureDetail(TaskStatusDto task)
        {
            var detail = string.IsNullOrWhiteSpace(task.DetailMessage)
                ? "恢复任务失败；请查看任务详情中的回滚状态。"
                : task.DetailMessage;
            return string.Equals(task.ErrorCode, "RESTORE_PRERESTORE_FAILED", StringComparison.OrdinalIgnoreCase)
                ? $"保护备份阶段失败，危险恢复已中止。{detail}"
                : detail;
        }

        private static bool IsTargetFailure(string? errorCode)
            => string.Equals(errorCode, "RESTORE_GAME_RUNNING", StringComparison.OrdinalIgnoreCase)
               || string.Equals(errorCode, "RESTORE_GAME_PROCESS_RUNNING", StringComparison.OrdinalIgnoreCase);

        private static RestoreWorkflowStepState? First(IReadOnlyList<RestoreWorkflowStepState> steps, RestoreWorkflowStepStatus status)
        {
            foreach (var step in steps)
                if (step.Status == status) return step;
            return null;
        }

        private static RestoreWorkflowStepState? FirstPending(IReadOnlyList<RestoreWorkflowStepState> steps)
        {
            foreach (var step in steps)
                if (step.Status == RestoreWorkflowStepStatus.Pending) return step;
            return null;
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024L * 1024) return $"{bytes / 1024d:0.##} KiB";
            if (bytes < 1024L * 1024 * 1024) return $"{bytes / 1024d / 1024d:0.##} MiB";
            return $"{bytes / 1024d / 1024d / 1024d:0.##} GiB";
        }
    }
}
