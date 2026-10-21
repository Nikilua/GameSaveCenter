using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using GameSaveCenter.Playnite.Settings;

namespace GameSaveCenter.Playnite.ViewModels
{
    /// <summary>Short-lived UI state for an explicitly reviewed, read-only worklist export.</summary>
    internal sealed class WorklistExportPreviewSession : INotifyPropertyChanged
    {
        private bool isOpen;
        private string text = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        internal bool IsOpen
        {
            get => isOpen;
            private set
            {
                if (isOpen == value) return;
                isOpen = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsOpen)));
            }
        }

        internal string Text
        {
            get => text;
            private set
            {
                if (string.Equals(text, value, StringComparison.Ordinal)) return;
                text = value ?? string.Empty;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            }
        }

        internal bool Open(WorklistDefinition? list, IEnumerable<WorklistBrowserEntry>? entries, DateTime generatedUtc)
        {
            if (list == null) return false;
            Text = WorklistExportPreviewFormatter.Create(list, entries, generatedUtc);
            IsOpen = true;
            return true;
        }

        internal void Close()
        {
            IsOpen = false;
            Text = string.Empty;
        }
    }

    /// <summary>Creates a fresh preview from the selected list and current transient entry states.</summary>
    internal sealed class CreateWorklistExportPreviewCommand : ICommand
    {
        private readonly WorklistExportPreviewSession session;
        private readonly Func<WorklistDefinition?> listProvider;
        private readonly Func<IEnumerable<WorklistBrowserEntry>> entriesProvider;
        private readonly Func<DateTime> clock;

        internal CreateWorklistExportPreviewCommand(
            WorklistExportPreviewSession session,
            Func<WorklistDefinition?> listProvider,
            Func<IEnumerable<WorklistBrowserEntry>> entriesProvider,
            Func<DateTime>? clock = null)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            this.listProvider = listProvider ?? throw new ArgumentNullException(nameof(listProvider));
            this.entriesProvider = entriesProvider ?? throw new ArgumentNullException(nameof(entriesProvider));
            this.clock = clock ?? (() => DateTime.UtcNow);
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => listProvider() != null;

        public void Execute(object? parameter)
        {
            var list = listProvider();
            if (list == null) return;
            session.Open(list, entriesProvider().ToArray(), clock());
        }

        internal void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
