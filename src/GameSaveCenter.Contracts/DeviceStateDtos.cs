using System;
using System.Collections.Generic;

namespace GameSaveCenter.Contracts
{
    /// <summary>A content-free summary of the newest local backup for one game.</summary>
    public sealed class DeviceBackupSummaryDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public string BackupId { get; set; } = string.Empty;
        public string ParentBackupId { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; }
        public long TotalBytes { get; set; }
        public int FileCount { get; set; }
        public string ContentFingerprint { get; set; } = string.Empty;
        /// <summary>Small validation summary for this exact backup; absent in older sidecar schemas.</summary>
        public string RestoreReadinessStatus { get; set; } = string.Empty;
        public DateTime? RestoreReadinessCheckedUtc { get; set; }
    }

    /// <summary>Small JSON sidecar shared between devices. It deliberately contains no save files or credentials.</summary>
    public sealed class DeviceStateSidecarDto
    {
        public int SchemaVersion { get; set; } = 4;
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public DateTime GeneratedUtc { get; set; } = DateTime.UtcNow;
        public List<DeviceBackupSummaryDto> Backups { get; set; } = new List<DeviceBackupSummaryDto>();
    }

    /// <summary>Read-only comparison result. A conflict is never resolved automatically.</summary>
    public sealed class DeviceConflictStatusDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public string RemoteDevice { get; set; } = string.Empty;
        public string RemoteDeviceId { get; set; } = string.Empty;
        public string LocalDevice { get; set; } = string.Empty;
        public string LocalBackupId { get; set; } = string.Empty;
        public string RemoteBackupId { get; set; } = string.Empty;
        public string LocalParentBackupId { get; set; } = string.Empty;
        public string RemoteParentBackupId { get; set; } = string.Empty;
        public long LocalTotalBytes { get; set; }
        public long RemoteTotalBytes { get; set; }
        public int LocalFileCount { get; set; }
        public int RemoteFileCount { get; set; }
        public bool LocalContentFingerprintAvailable { get; set; }
        public bool RemoteContentFingerprintAvailable { get; set; }
        public bool? ContentFingerprintsMatch { get; set; }
        public string LocalReadinessStatus { get; set; } = string.Empty;
        public string RemoteReadinessStatus { get; set; } = string.Empty;
        public DateTime? LocalReadinessCheckedUtc { get; set; }
        public DateTime? RemoteReadinessCheckedUtc { get; set; }
        public DateTime? LocalSummaryGeneratedUtc { get; set; }
        public DateTime? RemoteSummaryGeneratedUtc { get; set; }
        public bool HasConflict { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string SuggestedBackupId { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public DateTime LocalCreatedUtc { get; set; }
        public DateTime RemoteCreatedUtc { get; set; }
        public string Decision { get; set; } = string.Empty;
        public string DecisionComment { get; set; } = string.Empty;
        public DateTime? DecidedUtc { get; set; }
        public string StateDisplay => HasConflict ? "需要人工决定" : "一致或仅单端存在";
        public string DecisionDisplay => Decision switch
        {
            "KeepBoth" => "保留两者",
            "PreferLocal" => "记录为优先本机",
            "PreferRemote" => "记录为优先远端",
            "Defer" => "稍后处理",
            _ => "尚未记录"
        };
        public string ReasonDisplay
        {
            get
            {
                if (Reason == "UnknownDivergence")
                {
                    if (LocalContentFingerprintAvailable && RemoteContentFingerprintAvailable && ContentFingerprintsMatch == false)
                        return "两侧摘要字段相同，但内容指纹不同；父版本关系未知。";
                    if (LocalContentFingerprintAvailable && RemoteContentFingerprintAvailable && ContentFingerprintsMatch == true)
                        return "内容指纹相同，但检测器仍报告冲突；需人工核对输入一致性。";
                    return "两侧摘要字段相同，但内容指纹证据缺失；冲突原因未知。";
                }

                return Reason switch
                {
                    "SameDevice" => "两个摘要来自同一设备",
                    "SameBackupId" => "两侧引用同一备份版本",
                    "LinearFromKnownBase" => "一侧版本明确基于另一侧版本继续创建",
                    "DivergedFromCommonBase" => "两个版本从同一父版本分叉",
                    "DifferentDevicesChangedWithinTenMinutes" => "两端摘要不同且创建时间相差十分钟内；时间接近只是线索",
                    "DivergentDeviceSummaries" => "两台设备的最新备份摘要不同",
                    "EquivalentContent" => "Manifest 内容指纹相同",
                    "EquivalentSummary" => "摘要相同（旧版证据未提供内容指纹）",
                    "UnknownDivergence" => "冲突原因未知",
                    "OnlyOneSideAvailable" => "仅一台设备存在该备份摘要",
                    _ => string.IsNullOrWhiteSpace(Reason) ? "检测原因未提供" : "检测器返回了未识别的原因；需要人工查看两侧证据"
                };
            }
        }
    }

    /// <summary>Records a human decision only; it never downloads, restores, deletes or overwrites a backup.</summary>
    public sealed class DeviceConflictDecisionDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public string RemoteDevice { get; set; } = string.Empty;
        public string LocalBackupId { get; set; } = string.Empty;
        public string RemoteBackupId { get; set; } = string.Empty;
        public string Decision { get; set; } = "Defer";
        public string Comment { get; set; } = string.Empty;
        public DateTime DecidedUtc { get; set; }
    }

    public sealed class DeviceStateSyncResultDto
    {
        public string LocalDevice { get; set; } = string.Empty;
        public DateTime GeneratedUtc { get; set; }
        public bool Uploaded { get; set; }
        public int RemoteSidecarsRead { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
        public List<DeviceConflictStatusDto> Comparisons { get; set; } = new List<DeviceConflictStatusDto>();
    }

    /// <summary>Downloads one remote device's complete Ludusavi vault into an isolated local staging directory.</summary>
    public sealed class RemoteBackupStageRequestDto
    {
        public string PlayniteId { get; set; } = string.Empty;
        public string RemoteDevice { get; set; } = string.Empty;
        public string RemoteDeviceId { get; set; } = string.Empty;
        public string BackupId { get; set; } = string.Empty;
    }

    /// <summary>Opaque handle for a verified, isolated remote backup vault.</summary>
    public sealed class RemoteBackupStageResultDto
    {
        public string StagingId { get; set; } = string.Empty;
        public string PlayniteId { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public string RemoteDevice { get; set; } = string.Empty;
        public string RemoteDeviceId { get; set; } = string.Empty;
        public string BackupId { get; set; } = string.Empty;
        public DateTime StagedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }
        public bool Verified { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
        public string StagedRelativeDisplay => TimeDisplayFormatter.Relative(StagedUtc, DateTime.UtcNow);
        public string StagedFullDisplay => TimeDisplayFormatter.Full(StagedUtc);
        public string StagedRawUtcDisplay => TimeDisplayFormatter.RawUtc(StagedUtc);
        public string ExpiresRelativeDisplay => TimeDisplayFormatter.Relative(ExpiresUtc, DateTime.UtcNow);
        public string ExpiresFullDisplay => TimeDisplayFormatter.Full(ExpiresUtc);
        public string ExpiresRawUtcDisplay => TimeDisplayFormatter.RawUtc(ExpiresUtc);
    }

    /// <summary>Explicit restore request for a previously verified remote staging vault.</summary>
    public sealed class RemoteRestoreRequestDto : IIpcRequestWithId
    {
        public string RequestId { get; set; } = string.Empty;
        public string StagingId { get; set; } = string.Empty;
        public bool ConfirmedCurrentSnapshot { get; set; }
        public bool ConfirmedGameClosed { get; set; }
        public string UserComment { get; set; } = string.Empty;
    }
}
