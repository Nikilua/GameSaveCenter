using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;

namespace GameSaveCenter.Playnite.Views
{
    internal static class CommandDirectoryCatalog
    {
        private sealed class WorkspaceCommandDefinition
        {
            public WorkspaceCommandDefinition(
                WorkspaceKind workspace,
                string id,
                string name,
                string[] aliases,
                CommandDirectoryScope scope,
                bool requiresGame = false)
            {
                Workspace = workspace;
                Id = id;
                Name = name;
                Aliases = aliases;
                Scope = scope;
                RequiresGame = requiresGame;
            }

            public WorkspaceKind Workspace { get; }
            public string Id { get; }
            public string Name { get; }
            public string[] Aliases { get; }
            public CommandDirectoryScope Scope { get; }
            public bool RequiresGame { get; }
        }

        private static readonly WorkspaceCommandDefinition[] Workspaces =
        {
            new WorkspaceCommandDefinition(WorkspaceKind.Overview, "workspace.overview", "打开首页",
                new[] { "首页", "概览", "Home", "Overview", "Dashboard" }, CommandDirectoryScope.Workspace),
            new WorkspaceCommandDefinition(WorkspaceKind.Saves, "workspace.saves", "打开存档中心",
                new[] { "存档", "备份历史", "Saves", "Save center", "Save history" }, CommandDirectoryScope.CurrentGame, requiresGame: true),
            new WorkspaceCommandDefinition(WorkspaceKind.Trainers, "workspace.trainers", "打开修改器中心",
                new[] { "修改器", "游戏工具", "Trainers", "Game tools" }, CommandDirectoryScope.CurrentGame, requiresGame: true),
            new WorkspaceCommandDefinition(WorkspaceKind.Media, "workspace.media", "打开媒体中心",
                new[] { "媒体", "截图", "录像", "Media", "Screenshots", "Videos" }, CommandDirectoryScope.CurrentGame, requiresGame: true),
            new WorkspaceCommandDefinition(WorkspaceKind.Tasks, "workspace.tasks", "查看任务",
                new[] { "任务", "任务列表", "Tasks", "Task center", "View tasks" }, CommandDirectoryScope.Workspace),
            new WorkspaceCommandDefinition(WorkspaceKind.Maintenance, "workspace.maintenance", "打开维护中心",
                new[] { "维护", "诊断", "设备状态", "Maintenance", "Diagnostics" }, CommandDirectoryScope.Workspace)
        };

        public static IReadOnlyList<CommandDirectoryItem> Create(
            DashboardViewModel dashboard,
            ICommand navigateWorkspaceCommand,
            ICommand openSettingsCommand,
            ICommand openGamePickerCommand,
            ICommand focusWorkspaceSearchCommand)
        {
            if (dashboard == null) throw new ArgumentNullException(nameof(dashboard));
            if (navigateWorkspaceCommand == null) throw new ArgumentNullException(nameof(navigateWorkspaceCommand));
            if (openSettingsCommand == null) throw new ArgumentNullException(nameof(openSettingsCommand));
            if (openGamePickerCommand == null) throw new ArgumentNullException(nameof(openGamePickerCommand));
            if (focusWorkspaceSearchCommand == null) throw new ArgumentNullException(nameof(focusWorkspaceSearchCommand));

            var items = new List<CommandDirectoryItem>();
            foreach (var definition in Workspaces)
            {
                var requiresGame = definition.RequiresGame;
                items.Add(new CommandDirectoryItem(
                    definition.Id,
                    definition.Name,
                    definition.Aliases,
                    "页面导航",
                    definition.Scope,
                    navigateWorkspaceCommand,
                    definition.Workspace,
                    additionalCanExecute: () => !requiresGame || dashboard.SelectedGame != null,
                    unavailableReason: () => requiresGame && dashboard.SelectedGame == null
                        ? "请先选择当前游戏，再打开此工作区。"
                        : "当前页面暂时无法打开。"));
            }

            items.Add(new CommandDirectoryItem(
                "settings.open",
                "打开插件设置",
                new[] { "设置", "偏好设置", "Settings", "Preferences", "Plugin settings" },
                "设置",
                CommandDirectoryScope.Global,
                openSettingsCommand,
                unavailableReason: () => "插件设置入口当前不可用。"));

            items.Add(new CommandDirectoryItem(
                "game-picker.open",
                "选择当前游戏",
                new[] { "切换游戏", "选择游戏", "Game picker", "Select game", "Switch game" },
                "上下文",
                CommandDirectoryScope.Workspace,
                openGamePickerCommand,
                unavailableReason: () => "请先进入提供游戏选择器的工作区。"));

            items.Add(new CommandDirectoryItem(
                "workspace.focus-search",
                "定位到当前页面搜索",
                new[] { "页面搜索", "搜索框", "Ctrl+F", "Focus search", "Search current page" },
                "键盘导航",
                CommandDirectoryScope.Workspace,
                focusWorkspaceSearchCommand,
                unavailableReason: () => "当前页面没有可用的搜索框。"));

            items.Add(new CommandDirectoryItem(
                "task.open-associated-game",
                "打开任务关联游戏",
                new[] { "关联游戏", "任务游戏", "Open task game", "Open related game" },
                "任务导航",
                CommandDirectoryScope.SelectedTask,
                dashboard.OpenSelectedTaskGameCommand,
                additionalCanExecute: () => CanResolveSelectedTaskGame(dashboard),
                unavailableReason: () => GetSelectedTaskGameUnavailableReason(dashboard)));

            if (items.Select(item => item.Id).Distinct(StringComparer.Ordinal).Count() != items.Count)
                throw new InvalidOperationException("命令目录中存在重复的稳定 ID。");

            return items.AsReadOnly();
        }

        private static bool CanResolveSelectedTaskGame(DashboardViewModel dashboard)
        {
            if (dashboard.CurrentWorkspace != WorkspaceKind.Tasks || dashboard.IsBusy)
                return false;

            var task = dashboard.SelectedTask;
            if (task == null || string.IsNullOrWhiteSpace(task.GameId))
                return false;

            return dashboard.Games.Any(game => string.Equals(
                game.PlayniteId,
                task.GameId,
                StringComparison.OrdinalIgnoreCase));
        }

        private static string GetSelectedTaskGameUnavailableReason(DashboardViewModel dashboard)
        {
            if (dashboard.IsBusy) return "当前操作正在处理，请稍后再试。";
            if (dashboard.CurrentWorkspace != WorkspaceKind.Tasks) return "请先打开任务中心。";

            var task = dashboard.SelectedTask;
            if (task == null) return "请先选择一个任务。";
            if (string.IsNullOrWhiteSpace(task.GameId)) return "当前任务没有可定位的关联游戏。";
            return "任务关联游戏不在当前游戏列表中；请刷新游戏库后重试。";
        }
    }
}
