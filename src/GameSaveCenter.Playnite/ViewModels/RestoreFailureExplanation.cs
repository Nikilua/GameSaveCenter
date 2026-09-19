using System;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.ViewModels
{
    internal enum RestoreFailureKind
    {
        None,
        GameRunning,
        OperationLock,
        DiskSpace,
        TargetPermission,
        Unknown
    }

    internal sealed class RestoreFailureExplanation
    {
        private RestoreFailureExplanation(RestoreFailureKind kind, string resolution)
        {
            Kind = kind;
            Resolution = resolution ?? string.Empty;
        }

        public RestoreFailureKind Kind { get; }
        public string Resolution { get; }

        public static RestoreFailureExplanation ForTask(TaskStatusDto? task)
        {
            if (task == null)
                return new RestoreFailureExplanation(RestoreFailureKind.None, string.Empty);

            return ForText(task.ErrorCode, task.DetailMessage ?? task.ErrorMessage);
        }

        public static RestoreFailureExplanation ForReadiness(RestoreReadinessDto? readiness)
        {
            if (readiness == null || readiness.Status != RestoreReadinessStatus.Failed)
                return new RestoreFailureExplanation(RestoreFailureKind.None, string.Empty);

            return ForText(null, readiness.Summary);
        }

        public static RestoreFailureExplanation ForText(string? errorCode, string? detail)
        {
            var code = errorCode ?? string.Empty;
            var text = detail ?? string.Empty;
            if (ContainsAny(code, text, "RESTORE_GAME_RUNNING", "RESTORE_GAME_PROCESS_RUNNING", "游戏正在运行", "进程仍在运行"))
            {
                return new RestoreFailureExplanation(
                    RestoreFailureKind.GameRunning,
                    "请退出游戏、启动器及相关 MOD 管理器后重试，并确认进程已结束；不要强制结束未知进程，也不要关闭安全检查。 ");
            }

            if (ContainsAny(code, text, "GAME_OPERATION_BUSY", "操作正在执行", "已有备份", "恢复任务正在执行", "锁定"))
            {
                return new RestoreFailureExplanation(
                    RestoreFailureKind.OperationLock,
                    "请等待任务中心中的备份、恢复或媒体操作完成后再重试；不要并发重试或绕过操作锁。 ");
            }

            if (ContainsAny(code, text, "磁盘空间不足", "空间不足", "DISK_SPACE", "INSUFFICIENT_SPACE", "可用空间"))
            {
                return new RestoreFailureExplanation(
                    RestoreFailureKind.DiskSpace,
                    "请清理或迁移恢复校验隔离区、目标存档所在磁盘的可恢复空间，再重新执行可恢复性检查；不要关闭空间检查。 ");
            }

            if (ContainsAny(code, text, "Unauthorized", "Access denied", "permission", "权限", "拒绝访问", "无权"))
            {
                return new RestoreFailureExplanation(
                    RestoreFailureKind.TargetPermission,
                    "请确认 Playnite/Worker 运行账户对目标存档目录具有读写权限，并修复受控文件夹或 ACL 后重试；不要通过关闭安全机制规避权限问题。 ");
            }

            if (!string.IsNullOrWhiteSpace(code) || !string.IsNullOrWhiteSpace(text))
            {
                return new RestoreFailureExplanation(
                    RestoreFailureKind.Unknown,
                    "请保留错误码、任务详情和回滚状态，检查保护备份、目标路径及安全校验后再决定下一步；没有明确依据时不要关闭安全机制。 ");
            }

            return new RestoreFailureExplanation(RestoreFailureKind.None, string.Empty);
        }

        private static bool ContainsAny(string code, string text, params string[] needles)
        {
            foreach (var needle in needles)
            {
                if (code.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0
                    || text.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }
    }
}
