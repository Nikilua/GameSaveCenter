using System;
using System.Collections.Generic;
using System.Linq;
using GameSaveCenter.Contracts;
using GameSaveCenter.Core.Services;
using Newtonsoft.Json;

namespace GameSaveCenter.Playnite.Settings
{
    /// <summary>Credential-free, read-only restore planning evidence. This type has no execution or path fields.</summary>
    public sealed class RestorePlanDraftDefinition
    {
        public const int MaximumSavedDrafts = 50;
        public const string FixedProtectionRequirements = "执行前关闭游戏及启动器、MOD 工具；实际恢复时重新检查状态，并先创建和锁定 PreRestore 快照，再经明确确认后写入。";

        public string DraftId { get; set; } = Guid.NewGuid().ToString("N");
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public string PlayniteId { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public string TargetBackupId { get; set; } = string.Empty;
        public DateTime TargetCreatedUtc { get; set; }
        public string BaselineBackupId { get; set; } = string.Empty;
        public int MappingFileCount { get; set; }
        public long MappingTotalBytes { get; set; }
        public int? MappingUnknownSizeCount { get; set; }
        public string MappingPreviewId { get; set; } = string.Empty;
        public DateTime MappingPreviewedUtc { get; set; }
        public int MappingFingerprintVersion { get; set; }
        public string MappingFingerprint { get; set; } = string.Empty;
        public string MappingFingerprintKeyId { get; set; } = string.Empty;
        public string ReadinessStatus { get; set; } = "Unknown";
        public DateTime? ReadinessCheckedUtc { get; set; }
        public string ReadinessBackupId { get; set; } = string.Empty;

        [JsonIgnore]
        public bool IsReadOnly => true;

        [JsonIgnore]
        public string MappingSummary => !MappingUnknownSizeCount.HasValue
            ? $"映射预览：{MappingFileCount} 个文件 · 已知大小小计 {ByteSizeFormatter.Format(MappingTotalBytes)}；文件大小完整性未记录，路径不保存。"
            : MappingUnknownSizeCount.Value > 0
                ? $"映射预览：{MappingFileCount} 个文件 · 已知大小小计 {ByteSizeFormatter.Format(MappingTotalBytes)}；{MappingUnknownSizeCount.Value} 个文件大小未知，路径不保存。"
                : $"映射预览：{MappingFileCount} 个文件 · {ByteSizeFormatter.Format(MappingTotalBytes)}；路径不保存。";

        [JsonIgnore]
        public string ComparisonSelectorDisplay
            => $"{(string.IsNullOrWhiteSpace(GameName) ? "未命名游戏" : GameName)} · {TargetBackupId} · {CreatedUtc.ToLocalTime():MM-dd HH:mm}";

        [JsonIgnore]
        public string CreationDisplay => $"草稿创建于 {CreatedUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss}";

        [JsonIgnore]
        public string TargetDisplay => $"目标版本 {TargetBackupId} · {TargetCreatedUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss}";

        [JsonIgnore]
        public string BaselineDisplay => string.IsNullOrWhiteSpace(BaselineBackupId)
            ? "基线：完整版本（无差分父版本）"
            : $"基线版本：{BaselineBackupId}";

        [JsonIgnore]
        public string ReadinessReferenceDisplay => ReadinessCheckedUtc.HasValue && !string.IsNullOrWhiteSpace(ReadinessBackupId)
            ? $"可恢复性结果：{ReadinessStatus} · {ReadinessBackupId} · {ReadinessCheckedUtc.Value.ToLocalTime():yyyy-MM-dd HH:mm:ss}"
            : "可恢复性结果：未检查（本草稿不代表已验证）";

        [JsonProperty]
        public string ProtectionRequirements => FixedProtectionRequirements;

        public static RestorePlanDraftDefinition Create(
            GameStatusDto game,
            BackupVersionDto target,
            RestoreMappingPreviewDto mapping)
        {
            if (game == null) throw new ArgumentNullException(nameof(game));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            if (!mapping.Success
                || !string.Equals(game.PlayniteId, mapping.PlayniteId, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(target.BackupId, mapping.BackupId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("当前映射预览与所选游戏或版本不匹配，请重新预览。");

            var readiness = target.RestoreReadiness;
            var matchingReadiness = readiness != null
                && string.Equals(readiness.BackupVersionId, target.BackupId, StringComparison.OrdinalIgnoreCase)
                && readiness.CheckedUtc.HasValue;
            return new RestorePlanDraftDefinition
            {
                DraftId = Guid.NewGuid().ToString("N"),
                CreatedUtc = DateTime.UtcNow,
                PlayniteId = game.PlayniteId,
                GameName = game.Name,
                TargetBackupId = target.BackupId,
                TargetCreatedUtc = target.CreatedUtc,
                BaselineBackupId = target.ParentBackupId,
                MappingFileCount = mapping.FileCount,
                MappingTotalBytes = mapping.TotalBytes,
                MappingUnknownSizeCount = mapping.UnknownSizeCount,
                MappingPreviewId = mapping.PreviewId,
                MappingPreviewedUtc = mapping.PreviewedUtc,
                MappingFingerprintVersion = mapping.MappingFingerprintVersion,
                MappingFingerprint = mapping.MappingFingerprint,
                MappingFingerprintKeyId = mapping.MappingFingerprintKeyId,
                ReadinessStatus = matchingReadiness ? readiness!.Status.ToString() : "Unknown",
                ReadinessCheckedUtc = matchingReadiness ? readiness!.CheckedUtc : null,
                ReadinessBackupId = matchingReadiness ? readiness!.BackupVersionId : string.Empty
            };
        }

        public static List<RestorePlanDraftDefinition> NormalizeMany(IEnumerable<RestorePlanDraftDefinition>? source)
        {
            var result = new List<RestorePlanDraftDefinition>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in source ?? Enumerable.Empty<RestorePlanDraftDefinition>())
            {
                if (result.Count >= MaximumSavedDrafts) break;
                if (item == null) continue;
                var playniteId = NormalizeText(item.PlayniteId, 128);
                var backupId = NormalizeText(item.TargetBackupId, 180);
                if (playniteId.Length == 0 || backupId.Length == 0) continue;

                var id = NormalizeText(item.DraftId, 64);
                if (!Guid.TryParseExact(id, "N", out _)) id = Guid.NewGuid().ToString("N");
                if (!seen.Add(id)) continue;
                var fileCount = Math.Max(0, item.MappingFileCount);
                var totalBytes = Math.Max(0, item.MappingTotalBytes);
                var unknownSizeCount = item.MappingUnknownSizeCount.HasValue
                    ? Math.Min(fileCount, Math.Max(0, item.MappingUnknownSizeCount.Value))
                    : (int?)null;
                var fingerprint = NormalizeFingerprint(item.MappingFingerprint);
                var fingerprintVersion = fingerprint.Length == 64 && item.MappingFingerprintVersion == 1 ? 1 : 0;
                var fingerprintKeyId = fingerprintVersion == 1
                    && Guid.TryParseExact(item.MappingFingerprintKeyId, "N", out var parsedKeyId)
                    ? parsedKeyId.ToString("N")
                    : string.Empty;
                if (fingerprintVersion == 0) fingerprint = string.Empty;
                var readiness = Enum.TryParse(item.ReadinessStatus, ignoreCase: true, out RestoreReadinessStatus parsed)
                    ? parsed.ToString()
                    : "Unknown";

                result.Add(new RestorePlanDraftDefinition
                {
                    DraftId = id,
                    CreatedUtc = NormalizeUtc(item.CreatedUtc),
                    PlayniteId = playniteId,
                    GameName = NormalizeText(item.GameName, 180),
                    TargetBackupId = backupId,
                    TargetCreatedUtc = NormalizeUtc(item.TargetCreatedUtc),
                    BaselineBackupId = NormalizeText(item.BaselineBackupId, 180),
                    MappingFileCount = fileCount,
                    MappingTotalBytes = totalBytes,
                    MappingUnknownSizeCount = unknownSizeCount,
                    MappingPreviewId = NormalizeText(item.MappingPreviewId, 64),
                    MappingPreviewedUtc = NormalizeUtc(item.MappingPreviewedUtc),
                    MappingFingerprintVersion = fingerprintVersion,
                    MappingFingerprint = fingerprint,
                    MappingFingerprintKeyId = fingerprintKeyId,
                    ReadinessStatus = readiness,
                    ReadinessCheckedUtc = item.ReadinessCheckedUtc.HasValue ? NormalizeUtc(item.ReadinessCheckedUtc.Value) : null,
                    ReadinessBackupId = NormalizeText(item.ReadinessBackupId, 180)
                });
            }
            return result;
        }

        private static DateTime NormalizeUtc(DateTime value)
            => value == default ? new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) : value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();

        private static string NormalizeText(string? value, int maximumLength)
        {
            var text = (value ?? string.Empty).Trim();
            if (text.Length > maximumLength) text = text.Substring(0, maximumLength);
            return new string(text.Where(character => !char.IsControl(character)).ToArray());
        }

        private static string NormalizeFingerprint(string? value)
        {
            var text = value ?? string.Empty;
            if (text.Length != 64 || text.Any(character => !Uri.IsHexDigit(character))) return string.Empty;
            return text.ToUpperInvariant();
        }
    }
}
