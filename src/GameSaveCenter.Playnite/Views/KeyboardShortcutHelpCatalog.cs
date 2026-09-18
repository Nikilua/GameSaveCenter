using System;
using System.Collections.Generic;
using System.Windows.Input;
using GameSaveCenter.Playnite.ViewModels;

namespace GameSaveCenter.Playnite.Views
{
    internal sealed class KeyboardShortcutHelpItem
    {
        public KeyboardShortcutHelpItem(string gesture, string description, ICommand command)
        {
            Gesture = gesture ?? string.Empty;
            Description = description ?? string.Empty;
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        public string Gesture { get; }
        public string Description { get; }
        public ICommand Command { get; }
    }

    internal static class KeyboardShortcutHelpCatalog
    {
        public static IReadOnlyList<KeyboardShortcutHelpItem> Create(
            WorkspaceKind workspace,
            ICommand searchCommand)
        {
            var item = new KeyboardShortcutHelpItem(
                "Ctrl+F",
                "搜索" + WorkspaceName(workspace) + "；弹层打开时由弹层继续处理输入",
                searchCommand);

            return item.Command.CanExecute(null)
                ? new[] { item }
                : Array.Empty<KeyboardShortcutHelpItem>();
        }

        private static string WorkspaceName(WorkspaceKind workspace)
            => workspace switch
            {
                WorkspaceKind.Saves => "存档中心",
                WorkspaceKind.Trainers => "修改器中心",
                WorkspaceKind.Media => "媒体中心",
                WorkspaceKind.Tasks => "任务中心",
                WorkspaceKind.Maintenance => "维护中心",
                _ => "当前页面",
            };
    }
}
