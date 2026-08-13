using GameSaveCenter.Contracts;

namespace GameSaveCenter.Core.Services;

/// <summary>Pure notification-level decisions shared by the plugin and tests.</summary>
public static class NotificationLevelPolicy
{
    public static bool ShouldEmitSessionSummary(NotificationLevel level, GameSessionSummaryDto summary)
    {
        if (summary == null) return false;
        return level switch
        {
            NotificationLevel.ImportantOnly => summary.IsWarning || summary.IsFailure,
            NotificationLevel.Summary => true,
            NotificationLevel.Verbose => true,
            _ => true
        };
    }

    public static bool ShouldEmitTask(NotificationLevel level, TaskStatusDto task)
    {
        if (task == null) return false;
        return level switch
        {
            NotificationLevel.ImportantOnly => task.State == TaskState.Failed || task.State == TaskState.Cancelled,
            NotificationLevel.Summary => true,
            NotificationLevel.Verbose => true,
            _ => true
        };
    }
}
