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
        private EventHandler? canExecuteChanged;

        public CommandDirectoryItem(
            string id,
            string name,
            IEnumerable<string> aliases,
            string category,
            CommandDirectoryScope scope,
            ICommand command,
            object? commandParameter = null,
            Func<bool>? additionalCanExecute = null,
            Func<string?>? unavailableReason = null)
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
        }

        public string Id { get; }
        public string Name { get; }
        public IReadOnlyList<string> Aliases { get; }
        public string Category { get; }
        public CommandDirectoryScope Scope { get; }
        public bool IsAvailable => CanExecute(null);
        public string ScopeDisplay => Scope switch
        {
            CommandDirectoryScope.Global => "全局",
            CommandDirectoryScope.Workspace => "工作区",
            CommandDirectoryScope.CurrentGame => "当前游戏",
            CommandDirectoryScope.SelectedTask => "所选任务",
            _ => "当前范围",
        };

        public string AvailabilityDisplay => IsAvailable ? "可用" : "暂不可用";

        public string AutomationDisplayName => IsAvailable
            ? $"{Name}，{Category}，{ScopeDisplay}，可用"
            : $"{Name}，{Category}，{ScopeDisplay}，暂不可用：{UnavailableReason}";

        public string UnavailableReason
        {
            get
            {
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

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool CanExecute(object? parameter)
            => command.CanExecute(commandParameter)
               && (additionalCanExecute == null || additionalCanExecute());

        public void Execute(object? parameter)
        {
            if (CanExecute(null))
                command.Execute(commandParameter);
        }

        public bool TryExecute()
        {
            if (!CanExecute(null)) return false;
            command.Execute(commandParameter);
            return true;
        }

        public void RaiseCanExecuteChanged()
        {
            canExecuteChanged?.Invoke(this, EventArgs.Empty);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAvailable)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AvailabilityDisplay)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UnavailableReason)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutomationDisplayName)));
        }
    }
}
