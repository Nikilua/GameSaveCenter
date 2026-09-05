using System;

namespace GameSaveCenter.Contracts
{
    /// <summary>Durable plan and latest result for the non-destructive backup health inspection.</summary>
    public sealed class HealthInspectionStateDto
    {
        public bool Enabled { get; set; } = true;
        public int IntervalMinutes { get; set; } = 1440;
        public int StaleAfterDays { get; set; } = 30;
        public int MaxDurationSeconds { get; set; } = 300;
        public DateTime? NextDueUtc { get; set; }
        public DateTime? LastStartedUtc { get; set; }
        public DateTime? LastCompletedUtc { get; set; }
        public DateTime? LastSuccessfulUtc { get; set; }
        public string CursorPlayniteId { get; set; } = string.Empty;
        public string CursorBackupId { get; set; } = string.Empty;
        public string LastPlayniteId { get; set; } = string.Empty;
        public string LastBackupId { get; set; } = string.Empty;
        public string LastStatus { get; set; } = "NeverRun";
        public string LastSummary { get; set; } = "尚未运行恢复可用性巡检。";
        public int DeferredCount { get; set; }
        public int FailureCount { get; set; }

        public bool IsRunning => LastStartedUtc.HasValue
            && (!LastCompletedUtc.HasValue || LastStartedUtc.Value > LastCompletedUtc.Value);

        public string LastStatusDisplay => LastStatus switch
        {
            "Ready" => "最近验证通过",
            "Warning" => "最近验证有警告",
            "Corrupted" => "发现疑似损坏",
            "Failed" => "最近验证失败",
            "Unsupported" => "格式暂不支持",
            "Deferred" => "已推迟",
            "Cancelled" => "已取消",
            "NoBackups" => "暂无备份",
            "Running" => "巡检中",
            _ => "尚未运行"
        };

        public string LastSuccessfulLocalDisplay => LastSuccessfulUtc.HasValue
            ? LastSuccessfulUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
            : "尚未成功验证";

        public string NextDueLocalDisplay => NextDueUtc.HasValue
            ? NextDueUtc.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
            : "待安排";
    }
}
