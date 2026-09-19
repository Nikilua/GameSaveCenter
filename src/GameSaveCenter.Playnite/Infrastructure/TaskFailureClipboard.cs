using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>Builds the stable, already-redacted payload behind the task copy action.</summary>
    internal static class TaskFailureClipboardFormatter
    {
        internal static string Format(TaskStatusDto task)
        {
            if (task == null) return string.Empty;
            var raw = task.RestoreReport != null
                ? task.RestoreReport.ToRedactedText()
                : $"任务摘要：{task.FailureSummary}\r\n"
                    + $"游戏：{task.GameName} · {task.TaskType}\r\n"
                    + $"失败原因：{task.ErrorMessage}\r\n"
                    + $"错误码：{task.ErrorCode}\r\n"
                    + $"技术详情：{task.DetailMessage}\r\n"
                    + $"任务 ID：{task.TaskId}";
            return ClipboardValueSanitizer.Sanitize(raw);
        }
    }

    /// <summary>Retries transient Windows clipboard ownership failures without changing the payload.</summary>
    internal static class ClipboardRetry
    {
        internal static async Task<bool> TrySetTextAsync(
            string text,
            Action<string> setter,
            Func<int, Task>? delayAsync = null)
        {
            if (setter == null) throw new ArgumentNullException(nameof(setter));

            for (var attempt = 0; attempt < 4; attempt++)
            {
                try
                {
                    setter(text ?? string.Empty);
                    return true;
                }
                catch (Exception exception) when (IsTransientClipboardFailure(exception) && attempt < 3)
                {
                    if (delayAsync != null)
                        await delayAsync(attempt).ConfigureAwait(true);
                    else
                        await Task.Delay(150 + attempt * 100).ConfigureAwait(true);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return false;
        }

        private static bool IsTransientClipboardFailure(Exception exception)
            => exception is COMException || exception is InvalidOperationException;
    }
}
