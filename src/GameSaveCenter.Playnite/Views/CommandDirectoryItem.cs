using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace GameSaveCenter.Playnite.Views
{
    internal enum CommandDirectoryScope
    {
        Global,
        Workspace,
        CurrentGame,
        SelectedTask
    }

    /// <summary>
    /// A stable, searchable description of an existing safe navigation command.
    /// The wrapper captures its real command and argument and rechecks availability
    /// immediately before forwarding execution.
    /// </summary>
    internal sealed class CommandDirectoryItem : ICommand, INotifyPropertyChanged
    {
        private readonly ICommand command;
        private readonly object? commandParameter;
        private readonly Func<bool>? additionalCanExecute;
        private readonly Func<string?>? unavailableReason;
        private readonly Func<string?>? contextPreview;
        private readonly Func<string?>? contextUnavailableReason;
        private readonly Func<string?>? targetDisplay;
        private EventHandler? canExecuteChanged;
        private bool isPinned;
        private bool isRecent;
        private bool canAddPin = true;

        public CommandDirectoryItem(
            string id,
            string name,
            IEnumerable<string> aliases,
            string category,
            CommandDirectoryScope scope,
            ICommand command,
            object? commandParameter = null,
            Func<bool>? additionalCanExecute = null,
            Func<string?>? unavailableReason = null,
            Func<string?>? contextPreview = null,
            Func<string?>? contextUnavailableReason = null,
            Func<string?>? targetDisplay = null,
            string shortcutDisplay = "")
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("命令 ID 不能为空。", nameof(id));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("命令名称不能为空。", nameof(name));
            if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("命令分类不能为空。", nameof(category));

            Id = id.Trim();
            Name = name.Trim();
            Aliases = Array.AsReadOnly((aliases ?? throw new ArgumentNullException(nameof(aliases)))
                .Where(alias => !string.IsNullOrWhiteSpace(alias))
                .Select(alias => alias.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray());
            Category = category.Trim();
            Scope = scope;
            this.command = command ?? throw new ArgumentNullException(nameof(command));
            this.commandParameter = commandParameter;
            this.additionalCanExecute = additionalCanExecute;
            this.unavailableReason = unavailableReason;
            this.contextPreview = contextPreview;
            this.contextUnavailableReason = contextUnavailableReason;
            this.targetDisplay = targetDisplay;
            ShortcutDisplay = (shortcutDisplay ?? string.Empty).Trim();
        }

        public string Id { get; }
        public string Name { get; }
        public IReadOnlyList<string> Aliases { get; }
        public string Category { get; }
        public CommandDirectoryScope Scope { get; }
        public string ShortcutDisplay { get; }
        public string TargetDisplay => targetDisplay?.Invoke() ?? ScopeDisplay;
        public bool IsAvailable => CanExecute(null);
        public bool IsPinned => isPinned;
        public bool IsRecent => isRecent;
        public string RecentDisplay => IsRecent ? "最近" : string.Empty;
        public string PinActionDisplay => IsPinned ? "取消固定" : "固定";
        public bool PinActionEnabled => IsPinned || canAddPin;
        public string PinAutomationName => $"{PinActionDisplay}命令 {Name}";
        public string PinActionHelpText => !PinActionEnabled
            ? "最多固定 5 个命令；取消固定一个命令后即可继续。"
            : IsPinned ? "从固定命令中移除，不影响命令本身。"
            : "将此命令固定在命令搜索结果的前面。";
        public string ScopeDisplay => Scope switch
        {
            CommandDirectoryScope.Global => "全局",
            CommandDirectoryScope.Workspace => "工作区",
            CommandDirectoryScope.CurrentGame => "当前游戏",
            CommandDirectoryScope.SelectedTask => "所选任务",
            _ => "当前范围",
        };

        public string AvailabilityDisplay => IsAvailable ? "可用" : "暂不可用";

        public string ContextPreview => contextPreview?.Invoke() ?? "该命令不依赖当前游戏或任务。";

        public string FullDescription
        {
            get
            {
                var shortcut = ShortcutDisplay.Length == 0 ? string.Empty : $"\n快捷键：{ShortcutDisplay}";
                var preview = contextPreview == null ? string.Empty : "\n" + ContextPreview;
                var reason = UnavailableReason;
                return $"{Name}\n{Category} · {ScopeDisplay}\n{TargetDisplay}{shortcut}{preview}"
                    + (string.IsNullOrWhiteSpace(reason) ? string.Empty : "\n不可用原因：" + reason);
            }
        }

        public string AutomationDisplayName => IsAvailable
            ? BuildAutomationDisplayName("可用")
            : BuildAutomationDisplayName($"暂不可用：{UnavailableReason}");

        public string UnavailableReason
        {
            get
            {
                var contextReason = contextUnavailableReason?.Invoke();
                if (!string.IsNullOrWhiteSpace(contextReason)) return contextReason!.Trim();
                if (IsAvailable) return string.Empty;
                var reason = unavailableReason?.Invoke();
                return string.IsNullOrWhiteSpace(reason) ? "当前条件下不可用。" : reason!.Trim();
            }
        }

        public event EventHandler? CanExecuteChanged
        {
            add => canExecuteChanged += value;
            remove => canExecuteChanged -= value;
        }

        internal event EventHandler? Executed;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool CanExecute(object? parameter)
            => (contextUnavailableReason == null || string.IsNullOrWhiteSpace(contextUnavailableReason()))
               && command.CanExecute(commandParameter)
               && (additionalCanExecute == null || additionalCanExecute());

        public void Execute(object? parameter)
        {
            if (CanExecute(null))
            {
                command.Execute(commandParameter);
                Executed?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool TryExecute()
        {
            if (!CanExecute(null)) return false;
            command.Execute(commandParameter);
            Executed?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public void RaiseCanExecuteChanged()
        {
            canExecuteChanged?.Invoke(this, EventArgs.Empty);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAvailable)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AvailabilityDisplay)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContextPreview)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TargetDisplay)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FullDescription)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UnavailableReason)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutomationDisplayName)));
        }

        internal void SetPreferenceState(bool pinned, bool recent, bool mayAddPin)
        {
            if (isPinned != pinned)
            {
                isPinned = pinned;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPinned)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PinActionDisplay)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PinActionEnabled)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PinAutomationName)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PinActionHelpText)));
            }

            if (isRecent != recent)
            {
                isRecent = recent;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRecent)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RecentDisplay)));
            }

            if (canAddPin != mayAddPin)
            {
                canAddPin = mayAddPin;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PinActionEnabled)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PinActionHelpText)));
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutomationDisplayName)));
        }

        private string BuildAutomationDisplayName(string availability)
        {
            var preference = IsPinned ? "，已固定" : IsRecent ? "，最近使用" : string.Empty;
            var target = TargetDisplay == ScopeDisplay ? string.Empty : "，" + TargetDisplay;
            var shortcut = ShortcutDisplay.Length == 0 ? string.Empty : "，快捷键 " + ShortcutDisplay;
            return $"{Name}，{Category}，{ScopeDisplay}{target}，{availability}{shortcut}{preference}";
        }
    }
}
