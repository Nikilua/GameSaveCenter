using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GameSaveCenter.Contracts;
using GameSaveCenter.Playnite.ViewModels;

namespace GameSaveCenter.Playnite.Settings
{
    /// <summary>Formats a bounded, redacted manual worklist record. It is not an import format.</summary>
    internal static class WorklistExportPreviewFormatter
    {
        private static readonly Regex LocalPath = new Regex(
            @"(?<![A-Za-z0-9])(?:[A-Za-z]:[\\/]|\\\\[^\\/\s]+[\\/][^\\/\s]+[\\/])|%USERPROFILE%(?:[\\/][^\s]*)?|%HOMEDRIVE%%HOMEPATH%(?:[\\/][^\s]*)?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal static string Create(
            WorklistDefinition? list,
            IEnumerable<WorklistBrowserEntry>? entries,
            DateTime generatedUtc)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));

            var normalizedList = WorklistDefinition.Normalize(list)
                ?? throw new InvalidOperationException("待办清单无法规范化。");
            var allowedIdentities = new HashSet<string>(
                normalizedList.Items.Select(item => Identity(item.Kind, item.StableObjectId)),
                StringComparer.OrdinalIgnoreCase);
            var unresolved = (entries ?? Enumerable.Empty<WorklistBrowserEntry>())
                .Where(entry => entry != null && !entry.IsManuallyComplete
                    && allowedIdentities.Contains(Identity(entry.Kind, entry.StableObjectId)))
                .OrderBy(entry => entry.Position)
                .GroupBy(entry => Identity(entry.Kind, entry.StableObjectId), StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .Take(WorklistDefinition.MaximumItemsPerWorklist)
                .ToArray();

            var output = new StringBuilder();
            output.AppendLine("GameSaveCenter 人工待办清单记录");
            output.AppendLine("只读脱敏文本 · 人工整理记录 · 不可导入或执行");
            output.AppendLine();
            output.AppendLine($"清单：{SafeText(list.Name, WorklistDefinition.MaximumNameLength)}");
            output.AppendLine($"导出时间：{FormatUtc(generatedUtc)}");
            output.AppendLine($"条目：{unresolved.Length.ToString(CultureInfo.InvariantCulture)} 项未手动完成（包括失效、待刷新或无法检查的引用）");
            output.AppendLine();

            if (unresolved.Length == 0)
            {
                output.AppendLine("当前没有未手动完成的条目。");
            }
            else
            {
                for (var index = 0; index < unresolved.Length; index++)
                {
                    var entry = unresolved[index];
                    var hasCurrentSummary = !string.IsNullOrWhiteSpace(entry.CurrentSummary);
                    output.AppendLine($"{(index + 1).ToString(CultureInfo.InvariantCulture)}. {SafeText(entry.KindDisplay, 40)}");
                    output.AppendLine($"   摘要：{SafeText(hasCurrentSummary ? entry.CurrentSummary : entry.CurrentSummaryDisplay, 180)}");
                    if (hasCurrentSummary && !string.IsNullOrWhiteSpace(entry.ParentSummary))
                        output.AppendLine($"   所属游戏：{SafeText(entry.ParentSummary, 120)}");
                    output.AppendLine($"   引用状态：{SafeText(entry.StateDisplay, 60)}");
                    if (!string.IsNullOrWhiteSpace(entry.Note))
                        output.AppendLine($"   人工备注：{SafeText(entry.Note, WorklistDefinition.MaximumNoteLength)}");
                    output.AppendLine();
                }
            }

            output.AppendLine("用途说明：此文本只记录人工整理的待办事项，不代表备份、恢复、媒体处理或校验成功。文本不可导入或执行；它不包含稳定对象编号、完整本机路径、真实文件内容、认证信息或自动化命令。");
            return output.ToString();
        }

        private static string FormatUtc(DateTime value)
        {
            var utc = value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                : value.ToUniversalTime();
            return utc.ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture);
        }

        private static string SafeText(string? value, int maxLength)
        {
            var singleLine = new string((value ?? string.Empty)
                .Select(character => char.IsControl(character) ? ' ' : character)
                .ToArray()).Trim();
            if (LocalPath.IsMatch(singleLine)) return "[包含本地路径，已隐藏]";

            var redacted = MaintenanceReportRedactor.Redact(ClipboardTextSanitizer.Sanitize(singleLine));
            if (LocalPath.IsMatch(redacted)) return "[包含本地路径，已隐藏]";
            return redacted.Length <= maxLength ? redacted : redacted.Substring(0, maxLength);
        }

        private static string Identity(WorklistObjectKind kind, string stableObjectId)
            => ((int)kind).ToString(CultureInfo.InvariantCulture) + ":" + (stableObjectId ?? string.Empty).Trim();
    }
}
