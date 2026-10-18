using System;
using System.Collections.Generic;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.Settings
{
    /// <summary>Compares saved, path-free restore-plan evidence with a freshly loaded snapshot.</summary>
    public static class RestorePlanDraftRevalidation
    {
        public static string CompareSnapshot(
            RestorePlanDraftDefinition draft,
            GameStatusDto game,
            BackupVersionDto backup,
            string currentFileIdentityFingerprint,
            string currentFingerprintKeyId,
            string currentReadinessFingerprint)
        {
            if (draft == null) throw new ArgumentNullException(nameof(draft));
            if (game == null) throw new ArgumentNullException(nameof(game));
            if (backup == null) throw new ArgumentNullException(nameof(backup));

            var reasons = new List<string>();
            if (!string.Equals(draft.PlayniteId, game.PlayniteId, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(draft.TargetBackupId, backup.BackupId, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrWhiteSpace(backup.PlayniteId)
                    && !string.Equals(draft.PlayniteId, backup.PlayniteId, StringComparison.OrdinalIgnoreCase))
                || !string.Equals(draft.GameName, game.Name, StringComparison.Ordinal)
                || !string.Equals(draft.TargetLudusaviName, backup.LudusaviName, StringComparison.Ordinal)
                || !SameUtc(draft.TargetCreatedUtc, backup.CreatedUtc)
                || !string.Equals(draft.BaselineBackupId, backup.ParentBackupId, StringComparison.OrdinalIgnoreCase))
                reasons.Add("游戏或目标版本信息已变化，需要重新预览并保存草稿。");

            if (draft.TargetFileCount != backup.FileCount || draft.TargetTotalBytes != backup.TotalBytes)
                reasons.Add("目标文件数量或大小已变化，需要重新验证草稿。");

            if (!HasFingerprint(draft.TargetFileIdentityFingerprint)
                || draft.TargetFileIdentityFingerprintVersion != 1
                || !Guid.TryParseExact(draft.TargetFileIdentityFingerprintKeyId, "N", out _)
                || !string.Equals(draft.TargetFileIdentityFingerprintKeyId, currentFingerprintKeyId, StringComparison.OrdinalIgnoreCase)
                || !HasFingerprint(currentFileIdentityFingerprint))
                reasons.Add("目标文件身份无法确认，需要重新验证草稿。");
            else if (!string.Equals(draft.TargetFileIdentityFingerprint, currentFileIdentityFingerprint, StringComparison.OrdinalIgnoreCase))
                reasons.Add("目标文件身份已变化，需要重新验证草稿。");

            var readinessReason = CompareReadiness(draft, backup.RestoreReadiness, currentReadinessFingerprint, currentFingerprintKeyId);
            if (readinessReason.Length > 0) reasons.Add(readinessReason);
            return string.Join("；", reasons);
        }

        public static string CompareMapping(
            RestorePlanDraftDefinition draft,
            RestoreMappingPreviewDto preview)
        {
            if (draft == null) throw new ArgumentNullException(nameof(draft));
            if (preview == null) throw new ArgumentNullException(nameof(preview));

            if (!preview.Success
                || !string.Equals(draft.PlayniteId, preview.PlayniteId, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(draft.TargetBackupId, preview.BackupId, StringComparison.OrdinalIgnoreCase))
                return "映射预览对象或结果已变化，需要重新保存草稿。";

            if (!HasFingerprint(draft.MappingFingerprint)
                || draft.MappingFingerprintVersion != 1
                || !Guid.TryParseExact(draft.MappingFingerprintKeyId, "N", out _)
                || !HasFingerprint(preview.MappingFingerprint)
                || preview.MappingFingerprintVersion != draft.MappingFingerprintVersion
                || !string.Equals(draft.MappingFingerprintKeyId, preview.MappingFingerprintKeyId, StringComparison.OrdinalIgnoreCase))
                return "映射或文件身份无法确认，需要重新验证草稿。";

            if (!string.Equals(draft.MappingFingerprint, preview.MappingFingerprint, StringComparison.OrdinalIgnoreCase)
                || draft.MappingFileCount != preview.FileCount
                || draft.MappingTotalBytes != preview.TotalBytes
                || draft.MappingUnknownSizeCount != preview.UnknownSizeCount)
                return "映射或文件身份已变化，需要重新验证草稿。";

            return string.Empty;
        }

        private static string CompareReadiness(
            RestorePlanDraftDefinition draft,
            RestoreReadinessDto? current,
            string currentReadinessFingerprint,
            string currentFingerprintKeyId)
        {
            var savedHasResult = draft.ReadinessCheckedUtc.HasValue
                && !string.IsNullOrWhiteSpace(draft.ReadinessBackupId);
            var currentHasResult = current?.CheckedUtc.HasValue == true
                && !string.IsNullOrWhiteSpace(current.BackupVersionId);

            if (!savedHasResult && !currentHasResult) return string.Empty;
            if (!savedHasResult || !currentHasResult
                || !string.Equals(draft.ReadinessBackupId, current!.BackupVersionId, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(draft.ReadinessBackupId, draft.TargetBackupId, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(draft.ReadinessStatus, current.Status.ToString(), StringComparison.Ordinal)
                || !SameUtc(draft.ReadinessCheckedUtc!.Value, current.CheckedUtc!.Value)
                || draft.ReadinessFingerprintVersion != 1
                || !HasFingerprint(draft.ReadinessFingerprint)
                || !string.Equals(draft.ReadinessFingerprintKeyId, currentFingerprintKeyId, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(draft.ReadinessFingerprint, currentReadinessFingerprint, StringComparison.OrdinalIgnoreCase))
                return "可恢复性检查结果已变化，需要重新验证草稿。";

            return string.Empty;
        }

        private static bool SameUtc(DateTime left, DateTime right)
            => left.ToUniversalTime() == right.ToUniversalTime();

        private static bool HasFingerprint(string? value)
        {
            if (value?.Length != 64) return false;
            foreach (var character in value)
                if (!Uri.IsHexDigit(character)) return false;
            return true;
        }
    }
}
