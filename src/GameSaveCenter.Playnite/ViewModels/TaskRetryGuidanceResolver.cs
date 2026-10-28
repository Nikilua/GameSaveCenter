using System;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.ViewModels
{
    /// <summary>Display-only retry guidance; it never changes command eligibility or Worker behavior.</summary>
    internal static class TaskRetryGuidanceResolver
    {
        public static string Resolve(TaskStatusDto? task, bool canRetry)
        {
            if (task == null || (task.State != TaskState.Failed && task.State != TaskState.Cancelled))
                return string.Empty;

            if (!canRetry)
                return "此任务类型或状态当前不支持安全重试。请先查看完整详情和对应的恢复入口；上次结果不明时不要重复提交。";

            var errorCode = task.ErrorCode ?? string.Empty;
            var isCloudTask = string.Equals(task.TaskType, "CloudUpload", StringComparison.OrdinalIgnoreCase)
                || errorCode.StartsWith("RCLONE_", StringComparison.OrdinalIgnoreCase);
            if (isCloudTask)
            {
                var explanation = CloudFailureExplanation.Resolve(errorCode);
                if (explanation.IsRecognized)
                    return $"{explanation.CategoryDisplay}。{explanation.NextStepDisplay}";

                return "云端错误或提交结果尚未分类。请先核对远端是否已收到本次提交；结果不明时不要盲目重试。";
            }

            return "重试会按所选任务类型和对象重新提交。请先确认失败原因与上次执行结果；结果不明时不要盲目重试。";
        }
    }
}
