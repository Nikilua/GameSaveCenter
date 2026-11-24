using System;
using System.Linq;
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
        public UiNotificationEventArgs(string title, string message, UiNotificationKind kind, string? detailMessage = null, bool isCopyFeedback = false)
        {
            Title = title ?? string.Empty;
            Message = message ?? string.Empty;
            Kind = kind;
            DetailMessage = string.IsNullOrWhiteSpace(detailMessage) ? Message : detailMessage ?? string.Empty;
            IsCopyFeedback = isCopyFeedback;
        }

        public string Title { get; }
        public string Message { get; }
        public string DetailMessage { get; }
        public UiNotificationKind Kind { get; }
        public bool IsCopyFeedback { get; }
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
        internal RestoreConfirmationDetails? RestoreDetails { get; set; }
        public bool Handled { get; set; }
        public TaskCompletionSource<bool> Completion { get; }
    }

    public sealed class RestoreConfirmationDetails
    {
        private readonly RestoreConfirmationPreviewDto preview;

        internal RestoreConfirmationDetails(string gameName, BackupVersionDto backup, RestoreConfirmationPreviewDto? preview)
        {
            this.preview = preview ?? new RestoreConfirmationPreviewDto();
            Current = $"{gameName}\n执行前会确认游戏已关闭，并创建和锁定当前存档的 PreRestore 快照。";
            Target = $"{backup.CreatedFullDisplay}\n{backup.BackupId}\n{backup.SourceDisplay} · {backup.LockStateDisplay}";
        }

        public string Current { get; }
        public string Target { get; }
        public string Directories => preview.TargetDirectories.Count == 0 ? "目标目录未知" : string.Join("\n", preview.TargetDirectories);
        public string Changes => $"新增 {FormatCount(preview.AddedCount)} · 覆盖 {FormatCount(preview.ModifiedCount)} · 删除 {FormatCount(preview.RemovedCount)}";
        public string FileListTitle => preview.FileCount > preview.Files.Count
            ? $"预览文件（列出 {preview.Files.Count}/{preview.FileCount} 项）" : $"预览文件（{preview.Files.Count} 项）";
        public bool HasFiles => preview.Files.Count > 0;
        public string FileListText => string.Join("\n", preview.Files.Select(file => $"{file.Change} · {file.Path}"));

        private static string FormatCount(int? count) => count?.ToString() ?? "未知";
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
