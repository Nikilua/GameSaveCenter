using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.Settings
{
    /// <summary>Creates an allow-listed, redacted text summary. The result is not an import format.</summary>
    public static class RestorePlanDraftExportFormatter
    {
        private static readonly Regex LocalPath = new Regex(
            @"(?<![A-Za-z0-9])(?:[A-Za-z]:[\\/]|\\\\[^\\/\s]+[\\/][^\\/\s]+[\\/])|%USERPROFILE%(?:[\\/][^\s]*)?|%HOMEDRIVE%%HOMEPATH%(?:[\\/][^\s]*)?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string Create(RestorePlanDraftDefinition draft)
        {
            if (draft == null) throw new ArgumentNullException(nameof(draft));

            var readiness = draft.ReadinessCheckedUtc.HasValue
                && !string.IsNullOrWhiteSpace(draft.ReadinessBackupId)
                ? $"{SafeText(draft.ReadinessStatus, 80)}（{FormatUtc(draft.ReadinessCheckedUtc)}）"
                : "未记录";
            var revalidation = draft.RequiresRevalidation
                ? $"需重验：{SafeText(string.IsNullOrWhiteSpace(draft.RevalidationReason) ? "原因未记录" : draft.RevalidationReason, 180)}"
                : "当前草稿未标记失效";
            var unknownSizes = draft.MappingUnknownSizeCount.HasValue
                ? Math.Max(0, draft.MappingUnknownSizeCount.Value).ToString(CultureInfo.InvariantCulture)
                : "未记录";

            var output = new StringBuilder();
            output.AppendLine("GameSaveCenter 恢复方案摘要");
            output.AppendLine("只读分享文本 · 不可导入或执行");
            output.AppendLine();
            output.AppendLine($"游戏：{SafeText(string.IsNullOrWhiteSpace(draft.GameName) ? "未命名游戏" : draft.GameName, 180)}");
            output.AppendLine($"草稿创建时间：{FormatUtc(draft.CreatedUtc)}");
            output.AppendLine($"目标版本时间：{FormatUtc(draft.TargetCreatedUtc)}");
            output.AppendLine($"目标文件：{Math.Max(0, draft.TargetFileCount).ToString(CultureInfo.InvariantCulture)} 个 · {Math.Max(0, draft.TargetTotalBytes).ToString(CultureInfo.InvariantCulture)} bytes");
            output.AppendLine($"差分基线：{(string.IsNullOrWhiteSpace(draft.BaselineBackupId) ? "完整版本（无父版本）" : "存在父版本")}");
            output.AppendLine($"映射预览时间：{FormatUtc(draft.MappingPreviewedUtc)}");
            output.AppendLine($"映射摘要：{Math.Max(0, draft.MappingFileCount).ToString(CultureInfo.InvariantCulture)} 个文件 · {Math.Max(0, draft.MappingTotalBytes).ToString(CultureInfo.InvariantCulture)} bytes · 大小未知 {unknownSizes} 个");
            output.AppendLine($"可恢复性检查：{readiness}");
            output.AppendLine($"草稿状态：{revalidation}");
            output.AppendLine($"保护要求：{SafeText(draft.ProtectionRequirements, 240)}");
            output.AppendLine();
            output.AppendLine("隐私与用途：不包含存档数据、路径、认证参数、内部 ID、指纹或自动执行脚本。此摘要反映已保存草稿，不代表刚刚完成验证；实际恢复仍需重新检查并明确确认。文本导入功能不存在，导入此文本不能触发恢复。");

            return MaintenanceReportRedactor.Redact(output.ToString());
        }

        private static string FormatUtc(DateTime value)
        {
            if (value == default) return "未记录";
            var utc = value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                : value.ToUniversalTime();
            return utc.ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture);
        }

        private static string FormatUtc(DateTime? value)
            => value.HasValue ? FormatUtc(value.Value) : "未记录";

        private static string SafeText(string? value, int maxLength)
        {
            var singleLine = new string((value ?? string.Empty)
                .Select(character => char.IsControl(character) ? ' ' : character)
                .ToArray()).Trim();
            if (LocalPath.IsMatch(singleLine)) singleLine = "[包含本地路径，已隐藏]";
            var redacted = MaintenanceReportRedactor.Redact(singleLine);
            return redacted.Length <= maxLength ? redacted : redacted.Substring(0, maxLength);
        }
    }
}
