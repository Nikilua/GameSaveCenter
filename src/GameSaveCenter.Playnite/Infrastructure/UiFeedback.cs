using System;
using System.Threading.Tasks;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.Infrastructure
{
    public enum UiNotificationKind
    {
        Information,
        Success,
        Warning,
        Error
    }

    public sealed class UiNotificationEventArgs : EventArgs
    {
        public UiNotificationEventArgs(string title, string message, UiNotificationKind kind)
        {
            Title = title ?? string.Empty;
            Message = message ?? string.Empty;
            Kind = kind;
        }

        public string Title { get; }
        public string Message { get; }
        public UiNotificationKind Kind { get; }
        public bool Handled { get; set; }
    }

    public sealed class UiConfirmationEventArgs : EventArgs
    {
        public UiConfirmationEventArgs(string title, string message, string confirmText, string cancelText, bool isDangerous)
        {
            Title = title ?? string.Empty;
            Message = message ?? string.Empty;
            ConfirmText = string.IsNullOrWhiteSpace(confirmText) ? "确认" : confirmText;
            CancelText = string.IsNullOrWhiteSpace(cancelText) ? "取消" : cancelText;
            IsDangerous = isDangerous;
            Completion = new TaskCompletionSource<bool>();
        }

        public string Title { get; }
        public string Message { get; }
        public string ConfirmText { get; }
        public string CancelText { get; }
        public bool IsDangerous { get; }
        public bool Handled { get; set; }
        public TaskCompletionSource<bool> Completion { get; }
    }

    public sealed class UiChoiceEventArgs : EventArgs
    {
        public UiChoiceEventArgs(string title, string message, string primaryText, string laterText, string neverText)
        {
            Title = title ?? string.Empty;
            Message = message ?? string.Empty;
            PrimaryText = string.IsNullOrWhiteSpace(primaryText) ? "启用" : primaryText;
            LaterText = string.IsNullOrWhiteSpace(laterText) ? "以后再说" : laterText;
            NeverText = string.IsNullOrWhiteSpace(neverText) ? "不再提醒" : neverText;
            Completion = new TaskCompletionSource<ProtectionPromptChoice?>();
        }

        public string Title { get; }
        public string Message { get; }
        public string PrimaryText { get; }
        public string LaterText { get; }
        public string NeverText { get; }
        public bool Handled { get; set; }
        public TaskCompletionSource<ProtectionPromptChoice?> Completion { get; }
    }
}
