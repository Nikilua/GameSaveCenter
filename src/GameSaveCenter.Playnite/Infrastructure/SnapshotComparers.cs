using System;
using System.Collections.Generic;
using System.Linq;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Content comparers used by dashboard snapshot application. When a snapshot returns a
    /// fresh DTO list whose visible content is identical, these comparers let the collection
    /// skip the Reset entirely (0 CollectionChanged) instead of rebuilding WPF containers.
    /// </summary>
    public static class SnapshotComparers
    {
        public static readonly Func<GameStatusDto, GameStatusDto, bool> Game = (a, b) =>
            string.Equals(a.PlayniteId, b.PlayniteId, StringComparison.Ordinal)
            && string.Equals(a.Name, b.Name, StringComparison.Ordinal)
            && a.Platform == b.Platform
            && a.IsInstalled == b.IsInstalled
            && a.LastPlayedUtc == b.LastPlayedUtc
            && a.IsRunning == b.IsRunning
            && a.LudusaviMatched == b.LudusaviMatched
            && string.Equals(a.LudusaviName, b.LudusaviName, StringComparison.Ordinal)
            && a.LastBackupUtc == b.LastBackupUtc
            && a.BackupVersionCount == b.BackupVersionCount
            && a.LastMediaSyncUtc == b.LastMediaSyncUtc
            && a.MediaCount == b.MediaCount
            && string.Equals(a.CloudState, b.CloudState, StringComparison.Ordinal)
            && string.Equals(a.HealthState, b.HealthState, StringComparison.Ordinal)
            && string.Equals(a.HealthSummary, b.HealthSummary, StringComparison.Ordinal)
            && SequenceEquals(a.HealthReasons, b.HealthReasons, string.Equals)
            && a.LatestRestoreReadinessStatus == b.LatestRestoreReadinessStatus
            && PolicyEquals(a.Policy, b.Policy);

        public static readonly Func<TaskStatusDto, TaskStatusDto, bool> Task = (a, b) =>
            string.Equals(a.TaskId, b.TaskId, StringComparison.Ordinal)
            && string.Equals(a.SessionId, b.SessionId, StringComparison.Ordinal)
            && string.Equals(a.TaskType, b.TaskType, StringComparison.Ordinal)
            && string.Equals(a.GameId, b.GameId, StringComparison.Ordinal)
            && string.Equals(a.GameName, b.GameName, StringComparison.Ordinal)
            && a.State == b.State
            && a.ProgressPercent == b.ProgressPercent
            && string.Equals(a.Message, b.Message, StringComparison.Ordinal)
            && a.CreatedUtc == b.CreatedUtc
            && a.StartedUtc == b.StartedUtc
            && a.FinishedUtc == b.FinishedUtc
            && string.Equals(a.ErrorCode, b.ErrorCode, StringComparison.Ordinal)
            && string.Equals(a.ErrorMessage, b.ErrorMessage, StringComparison.Ordinal);

        public static readonly Func<ValidationFindingDto, ValidationFindingDto, bool> Finding = (a, b) =>
            string.Equals(a.PlayniteId, b.PlayniteId, StringComparison.Ordinal)
            && string.Equals(a.GameName, b.GameName, StringComparison.Ordinal)
            && a.Severity == b.Severity
            && string.Equals(a.Code, b.Code, StringComparison.Ordinal)
            && string.Equals(a.Title, b.Title, StringComparison.Ordinal)
            && string.Equals(a.Detail, b.Detail, StringComparison.Ordinal)
            && string.Equals(a.SuggestedAction, b.SuggestedAction, StringComparison.Ordinal);

        public static readonly Func<AuditLogEntryDto, AuditLogEntryDto, bool> Audit = (a, b) =>
            string.Equals(a.Category, b.Category, StringComparison.Ordinal)
            && string.Equals(a.Message, b.Message, StringComparison.Ordinal)
            && string.Equals(a.DetailJson, b.DetailJson, StringComparison.Ordinal)
            && a.CreatedUtc == b.CreatedUtc;

        public static readonly Func<BackupVersionDto, BackupVersionDto, bool> Backup = (a, b) =>
            string.Equals(a.BackupId, b.BackupId, StringComparison.Ordinal)
            && string.Equals(a.PlayniteId, b.PlayniteId, StringComparison.Ordinal)
            && string.Equals(a.LudusaviName, b.LudusaviName, StringComparison.Ordinal)
            && a.CreatedUtc == b.CreatedUtc
            && a.TotalBytes == b.TotalBytes
            && a.FileCount == b.FileCount
            && a.IsLocked == b.IsLocked
            && string.Equals(a.Comment, b.Comment, StringComparison.Ordinal)
            && string.Equals(a.SourceDevice, b.SourceDevice, StringComparison.Ordinal)
            && string.Equals(a.OperatingSystem, b.OperatingSystem, StringComparison.Ordinal)
            && a.IsPreRestore == b.IsPreRestore
            && string.Equals(a.ParentBackupId, b.ParentBackupId, StringComparison.Ordinal)
            && string.Equals(a.ArchivePath, b.ArchivePath, StringComparison.Ordinal)
            && string.Equals(a.RestoreReadiness?.Summary, b.RestoreReadiness?.Summary, StringComparison.Ordinal)
            && a.RestoreReadiness?.Status == b.RestoreReadiness?.Status
            && a.RestoreReadiness?.CheckedUtc == b.RestoreReadiness?.CheckedUtc;

        public static readonly Func<SavePathCandidateDto, SavePathCandidateDto, bool> SaveCandidate = (a, b) =>
            string.Equals(a.PlayniteId, b.PlayniteId, StringComparison.Ordinal)
            && string.Equals(a.Path, b.Path, StringComparison.Ordinal)
            && Math.Abs(a.Score - b.Score) < 0.0001
            && string.Equals(a.Status, b.Status, StringComparison.Ordinal)
            && SequenceEquals(a.Reasons, b.Reasons, string.Equals);

        public static readonly Func<MediaItemDto, MediaItemDto, bool> Media = (a, b) =>
            string.Equals(a.MediaId, b.MediaId, StringComparison.Ordinal)
            && string.Equals(a.PlayniteId, b.PlayniteId, StringComparison.Ordinal)
            && a.Kind == b.Kind
            && a.Source == b.Source
            && string.Equals(a.ArchivePath, b.ArchivePath, StringComparison.Ordinal)
            && string.Equals(a.OriginalPath, b.OriginalPath, StringComparison.Ordinal)
            && a.CapturedUtc == b.CapturedUtc
            && a.SizeBytes == b.SizeBytes
            && string.Equals(a.Sha256, b.Sha256, StringComparison.Ordinal)
            && a.IsFavorite == b.IsFavorite
            && string.Equals(a.Comment, b.Comment, StringComparison.Ordinal)
            && string.Equals(a.CloudState, b.CloudState, StringComparison.Ordinal)
            && string.Equals(a.ClassificationState, b.ClassificationState, StringComparison.Ordinal)
            && string.Equals(a.ClassificationReason, b.ClassificationReason, StringComparison.Ordinal);

        public static readonly Func<MediaSourceRuleDto, MediaSourceRuleDto, bool> MediaSource = (a, b) =>
            string.Equals(a.SourceId, b.SourceId, StringComparison.Ordinal)
            && string.Equals(a.PlayniteId, b.PlayniteId, StringComparison.Ordinal)
            && a.SourceKind == b.SourceKind
            && string.Equals(a.RootPath, b.RootPath, StringComparison.Ordinal)
            && string.Equals(a.IncludePattern, b.IncludePattern, StringComparison.Ordinal)
            && a.Enabled == b.Enabled
            && a.SharedDirectory == b.SharedDirectory;

        public static readonly Func<GameToolDto, GameToolDto, bool> GameTool = (a, b) =>
            string.Equals(a.ToolId, b.ToolId, StringComparison.Ordinal)
            && string.Equals(a.PlayniteId, b.PlayniteId, StringComparison.Ordinal)
            && a.ToolType == b.ToolType
            && a.SourceType == b.SourceType
            && string.Equals(a.DisplayName, b.DisplayName, StringComparison.Ordinal)
            && a.Enabled == b.Enabled
            && a.AutoStart == b.AutoStart
            && a.LaunchTiming == b.LaunchTiming
            && a.LaunchDelaySeconds == b.LaunchDelaySeconds
            && a.CloseOnGameExit == b.CloseOnGameExit
            && a.RequiresAdmin == b.RequiresAdmin
            && a.IfAlreadyRunning == b.IfAlreadyRunning
            && a.RiskCategory == b.RiskCategory
            && a.AllowUnknownToolWithAntiCheat == b.AllowUnknownToolWithAntiCheat
            && string.Equals(a.ActiveVersionId, b.ActiveVersionId, StringComparison.Ordinal)
            && a.CreatedUtc == b.CreatedUtc
            && a.UpdatedUtc == b.UpdatedUtc
            && SequenceEquals(a.Versions, b.Versions, GameToolVersionEquals);

        public static readonly Func<ProcessMappingDto, ProcessMappingDto, bool> ProcessMapping = (a, b) =>
            string.Equals(a.ExecutableName, b.ExecutableName, StringComparison.Ordinal)
            && string.Equals(a.PlayniteId, b.PlayniteId, StringComparison.Ordinal)
            && string.Equals(a.GameName, b.GameName, StringComparison.Ordinal)
            && a.Enabled == b.Enabled
            && a.CreatedUtc == b.CreatedUtc;

        public static readonly Func<DeviceConflictStatusDto, DeviceConflictStatusDto, bool> DeviceComparison = (a, b) =>
            string.Equals(a.PlayniteId, b.PlayniteId, StringComparison.Ordinal)
            && string.Equals(a.GameName, b.GameName, StringComparison.Ordinal)
            && string.Equals(a.RemoteDevice, b.RemoteDevice, StringComparison.Ordinal)
            && string.Equals(a.LocalBackupId, b.LocalBackupId, StringComparison.Ordinal)
            && string.Equals(a.RemoteBackupId, b.RemoteBackupId, StringComparison.Ordinal)
            && a.HasConflict == b.HasConflict
            && string.Equals(a.Reason, b.Reason, StringComparison.Ordinal)
            && string.Equals(a.SuggestedBackupId, b.SuggestedBackupId, StringComparison.Ordinal)
            && Math.Abs(a.Confidence - b.Confidence) < 0.0001
            && a.LocalCreatedUtc == b.LocalCreatedUtc
            && a.RemoteCreatedUtc == b.RemoteCreatedUtc
            && string.Equals(a.Decision, b.Decision, StringComparison.Ordinal)
            && string.Equals(a.DecisionComment, b.DecisionComment, StringComparison.Ordinal)
            && a.DecidedUtc == b.DecidedUtc;

        private static bool PolicyEquals(BackupPolicyDto a, BackupPolicyDto b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            return a.Enabled == b.Enabled
                && a.BackupOnGameStop == b.BackupOnGameStop
                && a.BackupDuringPlay == b.BackupDuringPlay
                && a.DuringPlayIntervalMinutes == b.DuringPlayIntervalMinutes
                && a.UploadAfterBackup == b.UploadAfterBackup
                && a.SyncMediaDuringPlay == b.SyncMediaDuringPlay
                && a.SyncMediaOnGameStop == b.SyncMediaOnGameStop
                && a.AllowAutomaticRestore == b.AllowAutomaticRestore
                && a.AnomalyProtectionLevel == b.AnomalyProtectionLevel
                && a.KeepRecentAllHours == b.KeepRecentAllHours
                && a.KeepDailyDays == b.KeepDailyDays
                && a.KeepWeeklyWeeks == b.KeepWeeklyWeeks
                && a.KeepMonthlyMonths == b.KeepMonthlyMonths;
        }

        private static bool GameToolVersionEquals(GameToolVersionDto a, GameToolVersionDto b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            return string.Equals(a.VersionId, b.VersionId, StringComparison.Ordinal)
                && string.Equals(a.ToolId, b.ToolId, StringComparison.Ordinal)
                && string.Equals(a.VersionName, b.VersionName, StringComparison.Ordinal)
                && string.Equals(a.EntryPath, b.EntryPath, StringComparison.Ordinal)
                && string.Equals(a.WorkingDirectory, b.WorkingDirectory, StringComparison.Ordinal)
                && string.Equals(a.Arguments, b.Arguments, StringComparison.Ordinal)
                && string.Equals(a.SourceUrl, b.SourceUrl, StringComparison.Ordinal)
                && string.Equals(a.FileSha256, b.FileSha256, StringComparison.Ordinal)
                && a.DownloadUtc == b.DownloadUtc
                && a.CreatedUtc == b.CreatedUtc
                && a.IsAvailable == b.IsAvailable;
        }

        private static bool SequenceEquals<T>(IEnumerable<T> a, IEnumerable<T> b, Func<T, T, bool> equals)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            using var left = a.GetEnumerator();
            using var right = b.GetEnumerator();
            while (true)
            {
                var leftHas = left.MoveNext();
                var rightHas = right.MoveNext();
                if (leftHas != rightHas) return false;
                if (!leftHas) return true;
                if (!equals(left.Current, right.Current)) return false;
            }
        }
    }
}
