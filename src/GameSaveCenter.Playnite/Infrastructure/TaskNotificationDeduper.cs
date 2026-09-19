using System;
using System.Text;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.Infrastructure;

/// <summary>
/// Claims terminal task outcomes for notification delivery without turning progress updates
/// into screen noise. A later failure with different evidence remains visible.
/// </summary>
internal sealed class TaskNotificationDeduper
{
    private readonly BoundedTaskIdSet claimedOutcomes;

    internal TaskNotificationDeduper(int capacity = BoundedTaskIdSet.DefaultCapacity)
    {
        claimedOutcomes = new BoundedTaskIdSet(capacity);
    }

    internal bool TryClaim(TaskStatusDto task)
    {
        if (!TaskNotificationFingerprint.IsTerminal(task)
            || string.IsNullOrWhiteSpace(task.TaskId)) return false;
        return claimedOutcomes.TryAdd(TaskNotificationFingerprint.Build(task));
    }
}

internal static class TaskNotificationFingerprint
{
    internal static bool IsTerminal(TaskStatusDto task)
        => task != null
            && (task.State == TaskState.Succeeded
                || task.State == TaskState.Failed
                || task.State == TaskState.Cancelled);

    internal static string Build(TaskStatusDto task)
    {
        if (task == null) return string.Empty;
        var builder = new StringBuilder();
        builder.Append(task.TaskId ?? string.Empty).Append('|').Append((int)task.State);
        if (task.State == TaskState.Failed)
        {
            // Error code is stable when available; the message is retained for unknown or
            // newly classified failures so an important new failure is not muted.
            builder.Append('|').Append(Normalize(task.ErrorCode));
            builder.Append('|').Append(Normalize(task.ErrorMessage));
            if (string.IsNullOrWhiteSpace(task.ErrorCode) && string.IsNullOrWhiteSpace(task.ErrorMessage))
                builder.Append('|').Append(Normalize(task.DetailMessage));
        }
        return builder.ToString();
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var trimmed = value.Trim();
        return trimmed.Length <= 240 ? trimmed : trimmed.Substring(0, 240);
    }
}
