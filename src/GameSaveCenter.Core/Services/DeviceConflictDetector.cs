using System;
using GameSaveCenter.Core.Models;

namespace GameSaveCenter.Core.Services
{
    /// <summary>Detects likely divergent save histories between devices.</summary>
    public sealed class DeviceConflictDetector
    {
        public DeviceConflict Detect(BackupSnapshot? local, BackupSnapshot? remote)
        {
            if (local == null || remote == null)
            {
                return new DeviceConflict { HasConflict = false, Reason = "OnlyOneSideAvailable", Confidence = 1 };
            }

            if (string.Equals(local.SourceDevice, remote.SourceDevice, StringComparison.OrdinalIgnoreCase))
            {
                return new DeviceConflict { HasConflict = false, Reason = "SameDevice", Confidence = 0.9 };
            }

            if (string.Equals(local.BackupId, remote.BackupId, StringComparison.OrdinalIgnoreCase))
            {
                return new DeviceConflict { HasConflict = false, Reason = "SameBackupId", Confidence = 1 };
            }

            if ((!string.IsNullOrWhiteSpace(local.ParentBackupId)
                && string.Equals(local.ParentBackupId, remote.BackupId, StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrWhiteSpace(remote.ParentBackupId)
                && string.Equals(remote.ParentBackupId, local.BackupId, StringComparison.OrdinalIgnoreCase)))
            {
                return new DeviceConflict { HasConflict = false, Reason = "LinearFromKnownBase", Confidence = 0.9 };
            }

            if (!string.IsNullOrWhiteSpace(local.ContentFingerprint)
                && string.Equals(local.ContentFingerprint, remote.ContentFingerprint, StringComparison.OrdinalIgnoreCase))
            {
                return new DeviceConflict { HasConflict = false, Reason = "EquivalentContent", Confidence = 1 };
            }

            if (!string.IsNullOrWhiteSpace(local.ParentBackupId)
                && string.Equals(local.ParentBackupId, remote.ParentBackupId, StringComparison.OrdinalIgnoreCase))
            {
                return new DeviceConflict { HasConflict = true, Reason = "DivergedFromCommonBase", Confidence = 0.99 };
            }

            var sameSummary = local.TotalBytes == remote.TotalBytes && local.FileCount == remote.FileCount;
            if (sameSummary)
            {
                return new DeviceConflict { HasConflict = true, Reason = "UnknownDivergence", Confidence = 0.8 };
            }

            var timeDifference = (local.CreatedUtc - remote.CreatedUtc).Duration();
            if (timeDifference <= TimeSpan.FromMinutes(10))
            {
                return new DeviceConflict
                {
                    HasConflict = true,
                    Reason = "DifferentDevicesChangedWithinTenMinutes",
                    Confidence = 0.95
                };
            }

            // A newer timestamp alone is not enough to choose a winner. Never populate a
            // preferred version for a conflict; the UI must require an explicit decision.
            return new DeviceConflict
            {
                HasConflict = true,
                Reason = "DivergentDeviceSummaries",
                Confidence = 0.65
            };
        }
    }
}
