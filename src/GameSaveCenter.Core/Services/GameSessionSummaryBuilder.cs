using System;
using System.Collections.Generic;
using System.Linq;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Core.Services;

/// <summary>Builds one exit summary from the same terminal task states shown in Task Center.</summary>
public static class GameSessionSummaryBuilder
{
    public static GameSessionSummaryDto Build(string gameName, IEnumerable<TaskStatusDto> tasks)
    {
        var items = (tasks ?? Enumerable.Empty<TaskStatusDto>()).ToList();
        var backup = items.FirstOrDefault(x => string.Equals(x.TaskType, "Backup", StringComparison.OrdinalIgnoreCase));
        var media = items.FirstOrDefault(x => string.Equals(x.TaskType, "MediaSync", StringComparison.OrdinalIgnoreCase));
        var cloudFailed = backup?.State == TaskState.Failed
            && (backup.ErrorCode?.StartsWith("RCLONE_", StringComparison.OrdinalIgnoreCase) ?? false);
        var localBackupSucceeded = backup?.State == TaskState.Succeeded || cloudFailed;
        var failure = items.Any(x => x.State == TaskState.Failed && !(ReferenceEquals(x, backup) && cloudFailed));
        var cancelled = items.Any(x => x.State == TaskState.Cancelled);
        var lines = new List<string>();
        if (backup != null)
        {
            lines.Add(localBackupSucceeded ? "✓ 本地备份完成" : backup.State == TaskState.Failed ? "⚠ 本地备份失败" : "… 本地备份处理中");
            if (cloudFailed) lines.Add("⚠ 云端同步失败，可稍后在任务中心重试");
        }
        if (media != null)
            lines.Add(media.State == TaskState.Succeeded ? "✓ 媒体同步完成" : media.State == TaskState.Failed ? "⚠ 媒体同步失败" : "… 媒体同步处理中");
        if (cancelled) lines.Add("⚠ 部分任务已取消");
        if (lines.Count == 0) lines.Add("本次退出没有产生后台备份任务");
        return new GameSessionSummaryDto
        {
            GameName = gameName ?? string.Empty,
            IsWarning = cloudFailed || cancelled || failure,
            IsFailure = failure,
            Message = string.Join(Environment.NewLine, lines)
        };
    }
}
