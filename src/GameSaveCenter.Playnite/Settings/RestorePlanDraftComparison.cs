using System;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.Settings
{
    public enum RestorePlanMappingComparisonState
    {
        Unknown,
        Same,
        Different
    }

    /// <summary>Read-only comparison of two saved restore planning summaries.</summary>
    public sealed class RestorePlanDraftComparison
    {
        public RestorePlanDraftDefinition From { get; }
        public RestorePlanDraftDefinition To { get; }
        public RestorePlanMappingComparisonState MappingState { get; }

        public string DirectionDisplay
            => $"{FriendlyName(From)} · {From.TargetBackupId} → {FriendlyName(To)} · {To.TargetBackupId}";

        public string SourceDisplay
        {
            get
            {
                var sameGame = string.Equals(From.PlayniteId, To.PlayniteId, StringComparison.OrdinalIgnoreCase);
                var sameTarget = sameGame && string.Equals(From.TargetBackupId, To.TargetBackupId, StringComparison.OrdinalIgnoreCase);
                var sameBaseline = string.Equals(From.BaselineBackupId, To.BaselineBackupId, StringComparison.OrdinalIgnoreCase);
                var game = sameGame ? "同一游戏" : "不同游戏";
                var target = sameTarget ? "同一目标版本" : "目标版本不同";
                var baseline = sameBaseline ? "基线相同" : "基线不同";
                return $"来源对照：{game} · {target} · {baseline}；{From.BaselineDisplay} → {To.BaselineDisplay}";
            }
        }

        public string MappingDisplay => MappingState switch
        {
            RestorePlanMappingComparisonState.Same => "映射摘要一致（相同签名密钥下的路径与大小条目摘要相同）。",
            RestorePlanMappingComparisonState.Different => "映射摘要不同（路径或大小条目有变化；未保存具体路径）。",
            _ => "映射是否相同未知（旧草稿缺少签名，或签名密钥已变化）。"
        };

        public string EstimatedImpactDisplay
        {
            get
            {
                var fileDelta = (long)To.MappingFileCount - From.MappingFileCount;
                var sizesComplete = From.MappingUnknownSizeCount == 0 && To.MappingUnknownSizeCount == 0;
                var sizeLabel = sizesComplete ? "总大小" : "已知大小小计";
                var byteDelta = (decimal)To.MappingTotalBytes - From.MappingTotalBytes;
                var size = $"{sizeLabel}：{ByteSizeFormatter.Format(From.MappingTotalBytes)} → {ByteSizeFormatter.Format(To.MappingTotalBytes)}（{FormatSignedBytes(byteDelta)}）";
                return $"预计影响：文件数 {From.MappingFileCount:N0} → {To.MappingFileCount:N0}（{FormatSignedCount(fileDelta)}）；{size}";
            }
        }

        public string ReadinessDisplay
            => $"检查引用：{From.ReadinessReferenceDisplay} → {To.ReadinessReferenceDisplay}";

        public string UnknownsDisplay
        {
            get
            {
                var affected = MappingState == RestorePlanMappingComparisonState.Same
                    ? "受影响条目数为 0（摘要相同）"
                    : "受影响路径数未知；草稿不保存逐条路径";
                return $"未知项：{affected}；文件大小未知数：A {FormatUnknownCount(From.MappingUnknownSizeCount)} / B {FormatUnknownCount(To.MappingUnknownSizeCount)}。";
            }
        }

        public string SnapshotDisplay
            => $"只比较草稿保存时的映射预览：{From.MappingPreviewedUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss} → {To.MappingPreviewedUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss}；不代表当前映射仍相同，也不授权执行。";

        private RestorePlanDraftComparison(
            RestorePlanDraftDefinition from,
            RestorePlanDraftDefinition to,
            RestorePlanMappingComparisonState mappingState)
        {
            From = from;
            To = to;
            MappingState = mappingState;
        }

        public static RestorePlanDraftComparison Create(RestorePlanDraftDefinition from, RestorePlanDraftDefinition to)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (to == null) throw new ArgumentNullException(nameof(to));
            return new RestorePlanDraftComparison(from, to, CompareMapping(from, to));
        }

        private static RestorePlanMappingComparisonState CompareMapping(
            RestorePlanDraftDefinition from,
            RestorePlanDraftDefinition to)
        {
            if (!HasFingerprint(from) || !HasFingerprint(to)
                || from.MappingFingerprintVersion != to.MappingFingerprintVersion
                || !string.Equals(from.MappingFingerprintKeyId, to.MappingFingerprintKeyId, StringComparison.OrdinalIgnoreCase))
                return RestorePlanMappingComparisonState.Unknown;

            return string.Equals(from.MappingFingerprint, to.MappingFingerprint, StringComparison.OrdinalIgnoreCase)
                ? RestorePlanMappingComparisonState.Same
                : RestorePlanMappingComparisonState.Different;
        }

        private static bool HasFingerprint(RestorePlanDraftDefinition draft)
        {
            if (draft.MappingFingerprintVersion != 1
                || !Guid.TryParseExact(draft.MappingFingerprintKeyId, "N", out _)
                || draft.MappingFingerprint?.Length != 64)
                return false;

            foreach (var character in draft.MappingFingerprint)
                if (!Uri.IsHexDigit(character)) return false;
            return true;
        }

        private static string FriendlyName(RestorePlanDraftDefinition draft)
            => string.IsNullOrWhiteSpace(draft.GameName) ? "未命名游戏" : draft.GameName;

        private static string FormatSignedCount(long value)
            => value > 0 ? "+" + value.ToString("N0") : value < 0 ? "−" + Math.Abs(value).ToString("N0") : "0";

        private static string FormatSignedBytes(decimal value)
        {
            if (value == 0) return "0 B";
            var magnitude = ByteSizeFormatter.Format((long)Math.Abs(value));
            return value > 0 ? "+" + magnitude : "−" + magnitude;
        }

        private static string FormatUnknownCount(int? value)
            => value.HasValue ? value.Value.ToString("N0") : "未记录";
    }
}
