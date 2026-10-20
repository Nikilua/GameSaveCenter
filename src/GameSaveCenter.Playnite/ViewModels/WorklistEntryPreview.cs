using System;
using System.IO;
using System.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.Settings;

namespace GameSaveCenter.Playnite.ViewModels
{
    /// <summary>Ephemeral detail shown before a stable reference is added to a user worklist.</summary>
    public sealed class WorklistEntryPreview
    {
        public WorklistEntryPreview(WorklistObjectKind kind, string stableObjectId, string objectTitle, string summary)
        {
            Kind = kind;
            StableObjectId = stableObjectId ?? string.Empty;
            ObjectTitle = objectTitle ?? string.Empty;
            Summary = summary ?? string.Empty;
        }

        public WorklistObjectKind Kind { get; }
        public string ObjectKindDisplay => Kind switch
        {
            WorklistObjectKind.BackupVersion => "存档版本",
            WorklistObjectKind.MediaItem => "媒体项",
            WorklistObjectKind.BackgroundTask => "后台任务",
            WorklistObjectKind.ValidationFinding => "校验问题",
            _ => "对象"
        };
        public string StableObjectId { get; }
        public string ObjectTitle { get; }
        public string Summary { get; }

        public WorklistItemRecord ToRecord()
            => new WorklistItemRecord { Kind = Kind, StableObjectId = StableObjectId };
    }

    public static class WorklistEntryPreviewFactory
    {
        public static WorklistEntryPreview? FromBackupVersion(BackupVersionDto? backup, string? gameName)
        {
            if (backup == null) return null;
            var gameId = (backup.PlayniteId ?? string.Empty).Trim();
            var backupId = (backup.BackupId ?? string.Empty).Trim();
            if (gameId.Length == 0 || backupId.Length == 0) return null;

            var stableId = $"v1|{gameId.Length}:{gameId}|{backupId.Length}:{backupId}";
            if (!IsBoundedStableId(stableId)) return null;
            var title = Join(" · ", Limit(gameName), Limit(backup.BackupTypeDisplay), Limit(backupId));
            var summary = Join("\n",
                $"创建时间：{Limit(backup.CreatedFullDisplay)}",
                $"文件：{Math.Max(0, backup.FileCount)} 个 · {Limit(backup.SizeDisplay)}",
                $"来源设备：{Limit(backup.SourceDisplay)}");
            return new WorklistEntryPreview(WorklistObjectKind.BackupVersion, stableId, title, summary);
        }

        public static WorklistEntryPreview? FromMediaItem(MediaItemDto? media, string? gameName)
        {
            if (media == null) return null;
            var stableId = (media.MediaId ?? string.Empty).Trim();
            if (!IsBoundedStableId(stableId)) return null;

            var fileName = Path.GetFileName(media.FileName ?? string.Empty);
            var title = Join(" · ", Limit(gameName), Limit(media.KindDisplay), Limit(fileName));
            var summary = Join("\n",
                $"来源：{Limit(media.SourceDisplay)}",
                $"拍摄时间：{Limit(media.CapturedFullDisplay)}",
                $"大小：{Limit(media.SizeDisplay)}");
            return new WorklistEntryPreview(WorklistObjectKind.MediaItem, stableId, title, summary);
        }

        public static WorklistEntryPreview? FromBackgroundTask(TaskStatusDto? task)
        {
            if (task == null) return null;
            var stableId = (task.TaskId ?? string.Empty).Trim();
            if (!IsBoundedStableId(stableId)) return null;

            var title = Join(" · ", Limit(task.GameName), Limit(task.TaskTypeDisplay), Limit(task.StateDisplay));
            var summary = Join("\n",
                $"创建时间：{Limit(task.CreatedFullDisplay)}",
                $"任务阶段：{Limit(task.StageDisplay)}",
                $"任务编号：{Limit(stableId)}");
            return new WorklistEntryPreview(WorklistObjectKind.BackgroundTask, stableId, title, summary);
        }

        public static WorklistEntryPreview? FromValidationFinding(ValidationFindingDto? finding)
        {
            if (finding == null) return null;
            var stableId = (finding.FindingId ?? string.Empty).Trim();
            if (!IsBoundedStableId(stableId)) return null;

            var title = Join(" · ", Limit(finding.GameName), Limit(finding.SeverityDisplay), Limit(finding.Title));
            var summary = Join("\n",
                $"问题代码：{Limit(finding.Code)}",
                $"证据时间：{Limit(finding.EvidenceTimeFullDisplay)}",
                $"记录编号：{Limit(stableId)}");
            return new WorklistEntryPreview(WorklistObjectKind.ValidationFinding, stableId, title, summary);
        }

        private static bool IsBoundedStableId(string value)
            => !string.IsNullOrWhiteSpace(value)
                && value.Length <= WorklistDefinition.MaximumStableObjectIdLength;

        private static string Join(string separator, params string[] values)
            => string.Join(separator, values.Where(value => !string.IsNullOrWhiteSpace(value)));

        private static string Limit(string? value, int maximumLength = 160)
        {
            var normalized = (value ?? string.Empty).Trim();
            return normalized.Length <= maximumLength
                ? normalized
                : normalized.Substring(0, maximumLength - 1) + "…";
        }
    }
}
