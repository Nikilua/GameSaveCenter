using System;
using System.Linq;
using GameSaveCenter.Playnite.ViewModels;

namespace GameSaveCenter.Playnite.Views
{
    /// <summary>
    /// Captures stable selections when the command directory opens. Context-bound
    /// commands stay tied to these IDs and are disabled if the selection or source
    /// object changes while the directory is visible.
    /// </summary>
    internal sealed class CommandDirectoryContextSnapshot
    {
        private CommandDirectoryContextSnapshot(
            WorkspaceKind workspace,
            string? gameId,
            string? gameName,
            string? taskId,
            string? taskType,
            string? taskGameId,
            string? taskGameName)
        {
            Workspace = workspace;
            GameId = gameId;
            GameName = gameName;
            TaskId = taskId;
            TaskType = taskType;
            TaskGameId = taskGameId;
            TaskGameName = taskGameName;
        }

        private WorkspaceKind Workspace { get; }
        private string? GameId { get; }
        private string? GameName { get; }
        private string? TaskId { get; }
        private string? TaskType { get; }
        private string? TaskGameId { get; }
        private string? TaskGameName { get; }

        public static CommandDirectoryContextSnapshot Capture(DashboardViewModel dashboard)
        {
            if (dashboard == null) throw new ArgumentNullException(nameof(dashboard));

            var game = dashboard.SelectedGame;
            var task = dashboard.SelectedTask;
            return new CommandDirectoryContextSnapshot(
                dashboard.CurrentWorkspace,
                Normalize(game?.PlayniteId),
                Normalize(game?.Name),
                Normalize(task?.TaskId),
                Normalize(task?.TaskType),
                Normalize(task?.GameId),
                Normalize(task?.GameName));
        }

        public static string GetPreview(
            CommandDirectoryContextSnapshot? snapshot,
            string commandId,
            CommandDirectoryScope scope,
            object? commandParameter)
        {
            if (scope == CommandDirectoryScope.Global)
                return "插件全局设置，不依赖当前游戏或任务。";

            if (snapshot == null)
                return "打开命令搜索后显示本次操作的准确目标。";

            if (scope == CommandDirectoryScope.CurrentGame)
            {
                var workspace = commandParameter is WorkspaceKind kind
                    ? GetWorkspaceDisplayName(kind)
                    : "当前工作区";
                var gameName = snapshot.GameName;
                var game = string.IsNullOrWhiteSpace(gameName) ? "未选择游戏" : gameName!;
                return $"目标工作区：{workspace} · 游戏：{game}";
            }

            if (scope == CommandDirectoryScope.SelectedTask)
            {
                var taskType = snapshot.TaskType;
                var task = string.IsNullOrWhiteSpace(taskType) ? "所选任务" : taskType!;
                var taskGameName = snapshot.TaskGameName;
                var game = string.IsNullOrWhiteSpace(taskGameName) ? "未关联游戏" : taskGameName!;
                return $"任务：{task} · 关联游戏：{game}";
            }

            if (string.Equals(commandId, "game-picker.open", StringComparison.Ordinal))
                return $"作用对象：{GetWorkspaceDisplayName(snapshot.Workspace)} 的游戏选择器";

            if (string.Equals(commandId, "workspace.focus-search", StringComparison.Ordinal))
                return $"作用对象：{GetWorkspaceDisplayName(snapshot.Workspace)} 的搜索框";

            if (commandParameter is WorkspaceKind targetWorkspace)
                return $"目标工作区：{GetWorkspaceDisplayName(targetWorkspace)}";

            return $"当前工作区：{GetWorkspaceDisplayName(snapshot.Workspace)}";
        }

        public static string? GetUnavailableReason(
            CommandDirectoryContextSnapshot? snapshot,
            DashboardViewModel dashboard,
            string commandId,
            CommandDirectoryScope scope)
        {
            if (dashboard == null) throw new ArgumentNullException(nameof(dashboard));

            if (scope == CommandDirectoryScope.CurrentGame)
            {
                if (snapshot == null)
                    return "请关闭并重新打开命令搜索，以确认目标游戏。";
                var gameId = snapshot.GameId;
                if (string.IsNullOrWhiteSpace(gameId))
                    return "打开命令搜索时没有选择当前游戏；请先选择游戏，再重新打开。";
                if (!ContainsGame(dashboard, gameId!))
                    return "目标游戏已不在当前游戏列表中，请重新选择游戏。";
                if (!string.Equals(dashboard.SelectedGame?.PlayniteId, gameId, StringComparison.OrdinalIgnoreCase))
                    return "当前游戏选择已变化，请关闭命令搜索后重新选择。";
                return null;
            }

            if (scope == CommandDirectoryScope.SelectedTask)
            {
                if (snapshot == null)
                    return "请关闭并重新打开命令搜索，以确认目标任务。";
                if (snapshot.Workspace != WorkspaceKind.Tasks)
                    return "请先打开任务中心并选择任务，再重新打开命令搜索。";
                if (dashboard.CurrentWorkspace != snapshot.Workspace)
                    return "当前工作区已变化，请打开任务中心后重新选择任务。";
                var taskId = snapshot.TaskId;
                if (string.IsNullOrWhiteSpace(taskId))
                    return "打开命令搜索时没有选择任务；请先选择任务，再重新打开。";
                if (!ContainsTask(dashboard, taskId!))
                    return "目标任务已不在当前任务列表中，请重新选择任务。";

                var task = dashboard.SelectedTask;
                if (task == null || !string.Equals(task.TaskId, taskId, StringComparison.OrdinalIgnoreCase))
                    return "所选任务已变化，请关闭命令搜索后重新选择。";
                var taskGameId = snapshot.TaskGameId;
                if (!string.Equals(task.GameId, taskGameId, StringComparison.OrdinalIgnoreCase))
                    return "任务关联游戏已变化，请关闭命令搜索后重新选择任务。";
                if (string.IsNullOrWhiteSpace(taskGameId))
                    return "当前任务没有可定位的关联游戏，请重新选择任务。";
                if (!ContainsGame(dashboard, taskGameId!))
                    return "任务关联游戏已不在当前游戏列表中，请重新选择任务。";
                return null;
            }

            if (string.Equals(commandId, "game-picker.open", StringComparison.Ordinal)
                || string.Equals(commandId, "workspace.focus-search", StringComparison.Ordinal))
            {
                if (snapshot == null)
                    return "请关闭并重新打开命令搜索，以确认当前工作区。";
                if (dashboard.CurrentWorkspace != snapshot.Workspace)
                    return "当前工作区已变化，请关闭命令搜索后重试。";
            }

            return null;
        }

        private static bool ContainsGame(DashboardViewModel dashboard, string gameId)
            => dashboard.Games != null && dashboard.Games.Any(game => string.Equals(
                game.PlayniteId,
                gameId,
                StringComparison.OrdinalIgnoreCase));

        private static bool ContainsTask(DashboardViewModel dashboard, string taskId)
            => dashboard.Tasks != null && dashboard.Tasks.Any(task => string.Equals(
                task.TaskId,
                taskId,
                StringComparison.OrdinalIgnoreCase));

        private static string? Normalize(string? value)
        {
            if (value == null) return null;
            var trimmed = value.Trim();
            return trimmed.Length == 0 ? null : trimmed;
        }

        private static string GetWorkspaceDisplayName(WorkspaceKind workspace)
            => workspace switch
            {
                WorkspaceKind.Overview => "首页",
                WorkspaceKind.Saves => "存档中心",
                WorkspaceKind.Trainers => "修改器中心",
                WorkspaceKind.Media => "媒体中心",
                WorkspaceKind.Tasks => "任务中心",
                WorkspaceKind.Notifications => "通知收件箱",
                WorkspaceKind.Maintenance => "维护中心",
                _ => "当前工作区",
            };
    }
}
