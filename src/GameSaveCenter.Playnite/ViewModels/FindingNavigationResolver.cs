using System;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.ViewModels
{
    public enum FindingNavigationKind
    {
        None,
        Save,
        FailedTasks,
        CloudQueue
    }

    public sealed class FindingNavigation
    {
        private FindingNavigation(FindingNavigationKind kind, string text, string toolTip)
        {
            Kind = kind;
            Text = text;
            ToolTip = toolTip;
        }

        public FindingNavigationKind Kind { get; }
        public string Text { get; }
        public string ToolTip { get; }
        public bool IsAvailable => Kind != FindingNavigationKind.None;

        public static FindingNavigation None { get; } = new FindingNavigation(
            FindingNavigationKind.None,
            string.Empty,
            string.Empty);

        public static FindingNavigation ForSave { get; } = new FindingNavigation(
            FindingNavigationKind.Save,
            "进入存档路径确认",
            "打开该诊断对应游戏的存档工作区，确认候选路径和当前规则。不会自动修改路径。");

        public static FindingNavigation ForFailedTasks { get; } = new FindingNavigation(
            FindingNavigationKind.FailedTasks,
            "查看失败任务",
            "打开任务中心并筛选失败任务，查看错误详情或按现有重试流程处理。");

        public static FindingNavigation ForCloudQueue { get; } = new FindingNavigation(
            FindingNavigationKind.CloudQueue,
            "打开云队列",
            "打开云端队列查看上传、远端校验和认证状态。不会自动重试或覆盖本地内容。");
    }

    public static class FindingNavigationResolver
    {
        public static FindingNavigation Resolve(ValidationFindingDto? finding)
        {
            if (finding == null)
                return FindingNavigation.None;

            var code = finding.Code ?? string.Empty;
            if (ContainsAny(code, "CLOUD", "RCLONE", "REMOTE"))
                return FindingNavigation.ForCloudQueue;

            if (ContainsAny(code, "TASK", "WORKER", "HEALTH")
                || ContainsAny(finding.Title, "任务", "Worker")
                || ContainsAny(finding.SuggestedAction, "任务中心", "任务详情", "重试"))
                return FindingNavigation.ForFailedTasks;

            if (!string.IsNullOrWhiteSpace(finding.PlayniteId))
                return FindingNavigation.ForSave;

            return FindingNavigation.None;
        }

        private static bool ContainsAny(string? value, params string[] candidates)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            foreach (var candidate in candidates)
            {
                if (value!.IndexOf(candidate, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }
    }
}
