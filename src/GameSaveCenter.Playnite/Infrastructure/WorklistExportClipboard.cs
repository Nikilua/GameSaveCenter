using System;
using System.Threading.Tasks;
using System.Windows;
using GameSaveCenter.Playnite.Settings;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>Copies only the already-reviewed, redacted worklist preview.</summary>
    internal static class WorklistExportClipboard
    {
        internal static Action<string>? ClipboardSetterForVerification { get; set; }
        internal static Func<int, Task>? RetryDelayForVerification { get; set; }

        internal static async Task<bool> CopyAsync(string text, FrameworkElement feedbackTarget)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                ClipboardFeedback.Show(feedbackTarget, "没有可复制的预览内容。", true);
                return false;
            }

            var copied = await ClipboardRetry.TrySetTextAsync(
                text,
                ClipboardSetterForVerification ?? Clipboard.SetText,
                RetryDelayForVerification).ConfigureAwait(true);
            ClipboardFeedback.Show(
                feedbackTarget,
                copied ? "待办清单摘要已复制。" : "复制失败：剪贴板暂时被其他程序占用，请稍后重试。",
                !copied);
            return copied;
        }
    }
}
