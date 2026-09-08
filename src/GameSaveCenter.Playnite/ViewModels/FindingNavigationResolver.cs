using System;
using System.Collections.Generic;
using System.Linq;
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

    /// <summary>
    /// Resolves the object identity carried by a finding without falling back to the
    /// dashboard's current selection. A finding can outlive the game snapshot that
    /// produced it, so callers must distinguish an exact target from a name-only task
    /// filter and from an unavailable target.
    /// </summary>
    public sealed class FindingGameTarget
    {
        private FindingGameTarget(
            string playniteId,
            string gameName,
            bool isAvailable,
            bool isExact,
            string message)
        {
            PlayniteId = playniteId;
            GameName = gameName;
            IsAvailable = isAvailable;
            IsExact = isExact;
            Message = message;
        }

        public string PlayniteId { get; }
        public string GameName { get; }
        public bool IsAvailable { get; }
        public bool IsExact { get; }
        public string Message { get; }

        public static FindingGameTarget Exact(GameStatusDto game)
            => new FindingGameTarget(
                game.PlayniteId ?? string.Empty,
                game.Name ?? string.Empty,
                isAvailable: true,
                isExact: true,
                message: string.Empty);

        public static FindingGameTarget NameOnly(string gameName, string message)
            => new FindingGameTarget(
                string.Empty,
                gameName ?? string.Empty,
                isAvailable: true,
                isExact: false,
                message: message ?? string.Empty);

        public static FindingGameTarget Missing(string message)
            => new FindingGameTarget(
                string.Empty,
                string.Empty,
                isAvailable: false,
                isExact: false,
                message: message ?? string.Empty);
    }

    public static class FindingNavigationTargetResolver
    {
        public static FindingGameTarget ResolveExactGame(
            ValidationFindingDto? finding,
            IEnumerable<GameStatusDto>? games)
        {
            var playniteId = finding?.PlayniteId?.Trim() ?? string.Empty;
            if (playniteId.Length == 0)
                return FindingGameTarget.Missing("该诊断没有稳定的游戏标识，无法安全打开对应游戏。请先刷新诊断。");

            var game = (games ?? Enumerable.Empty<GameStatusDto>())
                .FirstOrDefault(candidate => string.Equals(candidate.PlayniteId, playniteId, StringComparison.OrdinalIgnoreCase));
            return game != null
                ? FindingGameTarget.Exact(game)
                : FindingGameTarget.Missing($"当前快照中找不到诊断对应的游戏（{playniteId}），未切换到其他游戏。请先刷新面板。");
        }

        public static FindingGameTarget ResolveTaskGame(
            ValidationFindingDto? finding,
            IEnumerable<GameStatusDto>? games)
        {
            var playniteId = finding?.PlayniteId?.Trim() ?? string.Empty;
            var game = (games ?? Enumerable.Empty<GameStatusDto>())
                .FirstOrDefault(candidate => playniteId.Length > 0
                    && string.Equals(candidate.PlayniteId, playniteId, StringComparison.OrdinalIgnoreCase));
            if (game != null)
                return FindingGameTarget.Exact(game);

            var gameName = finding?.GameName?.Trim() ?? string.Empty;
            if (gameName.Length > 0)
                return FindingGameTarget.NameOnly(
                    gameName,
                    playniteId.Length == 0
                        ? $"诊断未提供游戏 ID，任务页将按“{gameName}”筛选失败任务。"
                        : $"当前快照中暂时找不到该游戏，任务页将按诊断中的名称“{gameName}”筛选失败任务。未修改当前游戏选择。");

            return string.IsNullOrWhiteSpace(playniteId)
                ? FindingGameTarget.Missing(string.Empty)
                : FindingGameTarget.Missing($"当前快照中找不到诊断对应的游戏（{playniteId}），无法定位失败任务。请先刷新面板。");
        }
    }
}
