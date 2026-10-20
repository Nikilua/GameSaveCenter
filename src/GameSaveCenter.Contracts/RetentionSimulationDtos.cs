using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSaveCenter.Contracts
{
    /// <summary>Durable state of one retention quarantine entry.</summary>
    public enum RetentionQuarantineState
    {
        Planned,
        Moved,
        IndexRemoved,
        Deleted,
        RecoveryRequired
    }

    /// <summary>Durable identity and recovery paths for one quarantined archive.</summary>
    public sealed class RetentionQuarantineEntryDto
    {
        public string EntryId { get; set; } = string.Empty;
        public string BatchId { get; set; } = string.Empty;
        public string PlayniteId { get; set; } = string.Empty;
        public string BackupId { get; set; } = string.Empty;
        public string OriginalPath { get; set; } = string.Empty;
        public string QuarantinePath { get; set; } = string.Empty;
        public long FileBytes { get; set; }
        public RetentionQuarantineState State { get; set; }
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
        public string LastError { get; set; } = string.Empty;
    }

    /// <summary>Current durable quarantine occupancy shown by maintenance surfaces.</summary>
    public sealed class RetentionQuarantineStatusDto
    {
        public int PendingCount { get; set; }
        public long OccupancyBytes { get; set; }
        public int RecoveryRequiredCount { get; set; }
    }

    /// <summary>Read-only page request for the maintenance quarantine ledger.</summary>
    public sealed class RetentionQuarantinePageRequestDto
    {
        public int Offset { get; set; }
        public int Limit { get; set; } = 100;
    }

    /// <summary>Bounded maintenance quarantine ledger page and its durable total.</summary>
    public sealed class RetentionQuarantinePageDto
    {
        public int Offset { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public bool HasMore { get; set; }
        public List<RetentionQuarantineEntryDto> Items { get; set; } = new List<RetentionQuarantineEntryDto>();

        public int LoadedCount => Offset + Items.Count;
        public string LoadedDisplay => HasMore
            ? $"已加载 {LoadedCount}/{TotalCount} 项"
            : $"已加载全部 {LoadedCount} 项";
    }

    /// <summary>Explicit confirmation for reconciling one durable quarantine ledger entry.</summary>
    public sealed class RetentionQuarantineRecoveryRequestDto
    {
        public string EntryId { get; set; } = string.Empty;
        public bool Confirmed { get; set; }
    }

    /// <summary>Result of a targeted, safety-checked quarantine reconciliation.</summary>
    public sealed class RetentionQuarantineRecoverySummaryDto
    {
        public int RestoredCount { get; set; }
        public int DeletedCount { get; set; }
        public int RecoveryRequiredCount { get; set; }
    }

    /// <summary>Read-only global retention simulation returned by the Worker.</summary>
    public sealed class RetentionSimulationPreviewDto
    {
        /// <summary>Opaque Worker-owned handle for this exact preview snapshot.</summary>
        public string PreviewId { get; set; } = string.Empty;
        public DateTime GeneratedUtc { get; set; } = DateTime.UtcNow;
        public int ExistingVersionCount { get; set; }
        public int KeepVersionCount { get; set; }
        public int DeleteCandidateCount { get; set; }
        public int UserLockedCount { get; set; }
        public int HealthProtectedCount { get; set; }
        public int PreRestoreCount { get; set; }
        public long EstimatedReleaseBytes { get; set; }
        public int PendingQuarantineCount { get; set; }
        public long PendingQuarantineBytes { get; set; }
        public int RecoveryRequiredCount { get; set; }
        public long QuarantineOccupancyBytes { get; set; }
        public string Summary { get; set; } = string.Empty;
        public List<RetentionSimulationItemDto> Items { get; set; } = new List<RetentionSimulationItemDto>();

        public string EstimatedReleaseDisplay => FormatBytes(EstimatedReleaseBytes);

        private static string FormatBytes(long bytes) => ByteSizeFormatter.Format(bytes);
    }

    /// <summary>Session-scoped candidate retention inputs sent for a read-only comparison.</summary>
    public sealed class RetentionScenarioComparisonRequestDto
    {
        public int KeepAllForHours { get; set; }
        public int KeepDailyDays { get; set; }
        public int KeepWeeklyWeeks { get; set; }
        public int KeepMonthlyMonths { get; set; }
        public int? GrowthMinimumMiBPer30Days { get; set; }
        public int? GrowthMaximumMiBPer30Days { get; set; }
    }

    /// <summary>Side-by-side logical counts produced by the existing retention planner.</summary>
    public sealed class RetentionScenarioPlanSummaryDto
    {
        public int ExistingVersionCount { get; set; }
        public int KeepVersionCount { get; set; }
        public int ProtectedVersionCount { get; set; }
        public int UserLockedCount { get; set; }
        public int PreRestoreCount { get; set; }
        public int HealthProtectedCount { get; set; }
        public int DeleteCandidateCount { get; set; }
        public long CandidateLogicalBytes { get; set; }

        public string CandidateLogicalBytesDisplay => ByteSizeFormatter.Format(CandidateLogicalBytes);
        public string DeleteCandidateDisplay => $"{DeleteCandidateCount} 项 · {CandidateLogicalBytesDisplay}";
        public string UserLockedDisplay => $"用户锁定：{UserLockedCount}";
        public string PreRestoreDisplay => $"PreRestore：{PreRestoreCount}";
        public string HealthProtectedDisplay => $"健康恢复点保护：{HealthProtectedCount}";
    }

    /// <summary>Per-game counts from the same retention snapshot and cutoff as the global comparison.</summary>
    public sealed class RetentionScenarioGameSummaryDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public int ExistingVersionCount { get; set; }
        public int CurrentKeepVersionCount { get; set; }
        public int CurrentProtectedVersionCount { get; set; }
        public int CurrentDeleteCandidateCount { get; set; }
        public long CurrentDeleteCandidateLogicalBytes { get; set; }
        public int CandidateKeepVersionCount { get; set; }
        public int CandidateProtectedVersionCount { get; set; }
        public int CandidateDeleteCandidateCount { get; set; }
        public long CandidateDeleteCandidateLogicalBytes { get; set; }
        public int ChangedVersionCount { get; set; }
        public List<RetentionScenarioVersionDetailDto> VersionDetails { get; set; } = new List<RetentionScenarioVersionDetailDto>();

        public bool HasScenarioDifference => ChangedVersionCount > 0;
        public string PolicyImpactDisplay => $"保留 {CurrentKeepVersionCount} → {CandidateKeepVersionCount} · 淘汰候选 {CurrentDeleteCandidateCount} → {CandidateDeleteCandidateCount} · 变化 {ChangedVersionCount} 项";
        public string ProtectedSummaryDisplay => $"保护项：当前 {CurrentProtectedVersionCount} · 候选 {CandidateProtectedVersionCount}";
    }

    /// <summary>Read-only per-version explanation; archive availability is informational and never authorizes cleanup.</summary>
    public sealed class RetentionScenarioVersionDetailDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public string BackupId { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; }
        public long TotalBytes { get; set; }
        public bool IsProtected { get; set; }
        public bool IsCurrentDeleteCandidate { get; set; }
        public bool IsCandidateDeleteCandidate { get; set; }
        public string CurrentDispositionDisplay { get; set; } = string.Empty;
        public string CandidateDispositionDisplay { get; set; } = string.Empty;
        public string ProtectionReasonDisplay { get; set; } = string.Empty;
        public string ArchiveAvailabilityDisplay { get; set; } = string.Empty;

        public string CreatedDisplay => CreatedUtc == default(DateTime)
            ? string.Empty
            : $"{CreatedUtc.ToLocalTime():yyyy-MM-dd HH:mm} · {BackupId}";
    }

    /// <summary>
    /// Read-only comparison of the effective per-game policies and one uniform candidate
    /// policy against the same indexed backup snapshot and planner cutoff.
    /// This DTO has no cleanup preview handle and cannot authorize Apply.
    /// </summary>
    public sealed class RetentionScenarioComparisonDto
    {
        public DateTime GeneratedUtc { get; set; }
        public int BaselineVersionCount { get; set; }
        public bool IsExpired { get; set; }
        public bool IsCancelled { get; set; }
        public string CandidatePolicyDisplay { get; set; } = string.Empty;
        public int? GrowthMinimumMiBPer30Days { get; set; }
        public int? GrowthMaximumMiBPer30Days { get; set; }
        public RetentionScenarioPlanSummaryDto Current { get; set; } = new RetentionScenarioPlanSummaryDto();
        public RetentionScenarioPlanSummaryDto Candidate { get; set; } = new RetentionScenarioPlanSummaryDto();
        public List<RetentionScenarioGameSummaryDto> Games { get; set; } = new List<RetentionScenarioGameSummaryDto>();
        public string Summary { get; set; } = "输入情景参数后生成只读对比。";
        public string Assumptions { get; set; } = "候选策略统一应用于所有游戏；当前策略按游戏生效。增长范围只记录为假设，不参与本次保留数量计算。";

        public int ChangedGameCount => Games.Count(x => x.ChangedVersionCount > 0);
        public int ChangedVersionCount => Games.Sum(x => x.ChangedVersionCount);
        public int ProtectedVersionDetailCount => Games.Sum(x => x.VersionDetails.Count(version => version.IsProtected));
        public string DetailSummaryDisplay => $"逐游戏对账 {Games.Count} 个游戏；策略结论变化 {ChangedGameCount} 个游戏 / {ChangedVersionCount} 个版本；另列 {ProtectedVersionDetailCount} 个保护项。选择游戏查看版本和保护原因。详情只读，不执行清理。";

        public string GeneratedDisplay => GeneratedUtc == default(DateTime)
            ? "尚未生成共同截止时点"
            : $"共同截止时点：{GeneratedUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss}";

        public string BaselineDisplay => $"共同基线：{BaselineVersionCount} 个已索引版本";
        public string GrowthAssumptionDisplay => GrowthMinimumMiBPer30Days.HasValue && GrowthMaximumMiBPer30Days.HasValue
            ? $"用户假设增长：{GrowthMinimumMiBPer30Days.Value}–{GrowthMaximumMiBPer30Days.Value} MiB/30 天（不参与候选数计算）"
            : "用户增长假设尚未填写";

        public static string FormatPolicy(int keepAllForHours, int dailyDays, int weeklyWeeks, int monthlyMonths)
            => $"全量 {keepAllForHours} 小时 · 每日 {dailyDays} 天 · 每周 {weeklyWeeks} 周 · 每月 {monthlyMonths} 个月";
    }

    /// <summary>One candidate row in the retention simulation. Deletion is never implied by preview.</summary>
    public sealed class RetentionSimulationItemDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public string BackupId { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; }
        public long TotalBytes { get; set; }
        public string ArchivePath { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
        public bool IsPreRestore { get; set; }
        public bool IsHealthProtected { get; set; }

        public string CreatedDisplay => CreatedUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
        public string CreatedRelativeDisplay => TimeDisplayFormatter.Relative(CreatedUtc, DateTime.UtcNow);
        public string CreatedFullDisplay => TimeDisplayFormatter.Full(CreatedUtc);
        public string CreatedRawUtcDisplay => TimeDisplayFormatter.RawUtc(CreatedUtc);
        public string TotalBytesDisplay => FormatBytes(TotalBytes);

        private static string FormatBytes(long bytes) => ByteSizeFormatter.Format(bytes);
    }

    /// <summary>Request to apply the simulated cleanup after an explicit second confirmation.</summary>
    public sealed class RetentionSimulationApplyRequestDto
    {
        public bool Confirmed { get; set; }
        /// <summary>Worker-generated preview handle. The apply operation never trusts client-supplied candidate counts.</summary>
        public string PreviewId { get; set; } = string.Empty;
        /// <summary>UTC timestamp of the read-only preview the user confirmed.</summary>
        public DateTime PreviewGeneratedUtc { get; set; }
        /// <summary>Legacy display value retained for wire compatibility; the Worker does not use it for authorization.</summary>
        public int ExpectedCandidateCount { get; set; }
        /// <summary>Legacy display value retained for wire compatibility; the Worker does not use it for authorization.</summary>
        public long ExpectedReleaseBytes { get; set; }
    }

    /// <summary>Result of applying a user-confirmed retention cleanup.</summary>
    public sealed class RetentionSimulationResultDto
    {
        public int DeletedCount { get; set; }
        public int SkippedProtectedCount { get; set; }
        public int SkippedMissingCount { get; set; }
        public int SkippedUnsupportedCount { get; set; }
        public int SkippedBusyCount { get; set; }
        public int SkippedChangedCount { get; set; }
        public int FailedCount { get; set; }
        public int PendingQuarantineCount { get; set; }
        public long PendingQuarantineBytes { get; set; }
        public int RecoveryRequiredCount { get; set; }
        public int MovedCount { get; set; }
        public long MovedBytes { get; set; }
        public long QuarantineOccupancyBytes { get; set; }
        public long FreedBytes { get; set; }
        public string Summary { get; set; } = string.Empty;
    }
}
