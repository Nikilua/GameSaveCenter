using System;
using System.Collections.Generic;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Core.Services
{
    /// <summary>
    /// Calculates a compact, explainable backup-health state from already indexed evidence.
    /// It deliberately performs no I/O, archive access, network access or persistence.
    /// </summary>
    public sealed class GameHealthAssessmentService
    {
        private static readonly TimeSpan RecentPlayWindow = TimeSpan.FromDays(30);
        private static readonly TimeSpan AttentionBackupAge = TimeSpan.FromDays(14);
        private static readonly TimeSpan RiskBackupAge = TimeSpan.FromDays(30);

        public GameHealthAssessment Assess(GameHealthInput input, DateTime nowUtc)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var reasons = new List<string>();
            var recentlyPlayed = input.LastPlayedUtc.HasValue
                && nowUtc - input.LastPlayedUtc.Value.ToUniversalTime() <= RecentPlayWindow;
            var hasBackup = input.BackupVersionCount > 0 || input.LastBackupUtc.HasValue;

            if (!input.LudusaviMatched)
            {
                reasons.Add("尚未识别到该游戏的存档规则");
                return new GameHealthAssessment(GameHealthState.Unknown, "暂无法判断存档健康度。", reasons);
            }

            if (!hasBackup && !recentlyPlayed)
            {
                reasons.Add("近期没有游玩记录，也没有本地备份证据");
                return new GameHealthAssessment(GameHealthState.Unknown, "暂无足够的近期活动证据。", reasons);
            }

            if (!hasBackup && recentlyPlayed)
            {
                reasons.Add("最近游玩过，但尚未发现本地备份");
                return new GameHealthAssessment(GameHealthState.Risk, "最近的存档还没有健康恢复点。", reasons);
            }

            if (input.RecentBackupFailureCount >= 3)
            {
                reasons.Add($"最近 30 天有 {input.RecentBackupFailureCount} 次备份失败");
                return new GameHealthAssessment(GameHealthState.Risk, "备份多次失败，需要尽快处理。", reasons);
            }

            if (input.OpenFindingErrorCount > 0)
            {
                reasons.Add(string.IsNullOrWhiteSpace(input.LatestFindingTitle)
                    ? "存在未解决的备份错误"
                    : input.LatestFindingTitle);
                return new GameHealthAssessment(GameHealthState.Risk, "最新备份存在未解决的异常。", reasons);
            }

            if (input.LatestRestoreReadinessStatus == RestoreReadinessStatus.Corrupted
                || input.LatestRestoreReadinessStatus == RestoreReadinessStatus.Failed)
            {
                reasons.Add(input.LatestRestoreReadinessStatus == RestoreReadinessStatus.Corrupted
                    ? "最新恢复点疑似损坏"
                    : "最新恢复点检查失败");
                return new GameHealthAssessment(GameHealthState.Risk, "没有确认可用的最新恢复点。", reasons);
            }

            var age = input.LastBackupUtc.HasValue
                ? nowUtc - input.LastBackupUtc.Value.ToUniversalTime()
                : TimeSpan.MaxValue;
            if (recentlyPlayed && age > RiskBackupAge)
            {
                reasons.Add($"最近游玩过，但最新备份已 {FormatAge(age)}");
                return new GameHealthAssessment(GameHealthState.Risk, "最近游玩内容可能尚未得到保护。", reasons);
            }

            if (input.RecentBackupFailureCount > 0)
                reasons.Add($"最近 30 天有 {input.RecentBackupFailureCount} 次备份失败");
            if (input.LastBackupTaskState == TaskState.Failed)
                reasons.Add("最近一次备份任务失败");
            if (input.OpenFindingWarningCount > 0)
                reasons.Add(string.IsNullOrWhiteSpace(input.LatestFindingTitle)
                    ? "存在未解决的备份警告"
                    : input.LatestFindingTitle);
            if (input.LatestRestoreReadinessStatus == RestoreReadinessStatus.Warning
                || input.LatestRestoreReadinessStatus == RestoreReadinessStatus.Unsupported)
                reasons.Add(input.LatestRestoreReadinessStatus == RestoreReadinessStatus.Warning
                    ? "最新恢复点有可见警告"
                    : "最新恢复点尚未被当前读取器验证");
            if (input.LatestRestoreReadinessStatus == null)
                reasons.Add("最新恢复点尚未验证");
            if (input.CloudEnabled && (input.CloudState == "Failed" || input.CloudState == "RetryScheduled"))
                reasons.Add("云端复制尚未成功");
            if (recentlyPlayed && age > AttentionBackupAge)
                reasons.Add($"最近一次备份距今已 {FormatAge(age)}");

            if (reasons.Count > 0)
                return new GameHealthAssessment(GameHealthState.Attention, "本地备份仍在，但有需要留意的项目。", reasons);

            return new GameHealthAssessment(GameHealthState.Healthy, "本地存在近期且未发现异常的恢复点。", new[] { "本地备份正常" });
        }

        private static string FormatAge(TimeSpan age)
        {
            if (age.TotalDays >= 1) return $"{Math.Max(1, (int)age.TotalDays)} 天";
            return $"{Math.Max(1, (int)age.TotalHours)} 小时";
        }
    }

    public sealed class GameHealthInput
    {
        public bool LudusaviMatched { get; set; }
        public DateTime? LastPlayedUtc { get; set; }
        public DateTime? LastBackupUtc { get; set; }
        public int BackupVersionCount { get; set; }
        public TaskState? LastBackupTaskState { get; set; }
        public int RecentBackupFailureCount { get; set; }
        public RestoreReadinessStatus? LatestRestoreReadinessStatus { get; set; }
        public int OpenFindingWarningCount { get; set; }
        public int OpenFindingErrorCount { get; set; }
        public string LatestFindingTitle { get; set; } = string.Empty;
        public string CloudState { get; set; } = "Disabled";
        public bool CloudEnabled { get; set; }
    }

    public sealed class GameHealthAssessment
    {
        public GameHealthAssessment(GameHealthState state, string summary, IEnumerable<string> reasons)
        {
            State = state;
            Summary = summary ?? string.Empty;
            Reasons = new List<string>(reasons ?? Array.Empty<string>());
        }

        public GameHealthState State { get; }
        public string Summary { get; }
        public IReadOnlyList<string> Reasons { get; }
    }
}
