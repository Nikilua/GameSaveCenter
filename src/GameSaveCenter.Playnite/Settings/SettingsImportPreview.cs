using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameSaveCenter.Playnite.Settings
{
    /// <summary>
    /// A non-mutating inspection of a portable settings package. The detached settings
    /// instance is kept internal so callers can only apply the inspected package through
    /// GameSaveCenterSettings.ApplyPortableJson.
    /// </summary>
    public sealed class SettingsImportPreview
    {
        internal SettingsImportPreview(
            int schemaVersion,
            DateTime exportedUtc,
            bool isCompatible,
            string compatibilitySummary,
            IEnumerable<SettingsImportField> fieldsToOverwrite,
            IEnumerable<string> unknownFields,
            IEnumerable<string> validationErrors,
            GameSaveCenterSettings? importedSettings)
        {
            SchemaVersion = schemaVersion;
            ExportedUtc = exportedUtc;
            IsCompatible = isCompatible;
            CompatibilitySummary = compatibilitySummary;
            FieldsToOverwrite = new List<SettingsImportField>(fieldsToOverwrite ?? Enumerable.Empty<SettingsImportField>());
            UnknownFields = new List<string>(unknownFields ?? Enumerable.Empty<string>());
            ValidationErrors = new List<string>(validationErrors ?? Enumerable.Empty<string>());
            ImportedSettings = importedSettings;
        }

        public int SchemaVersion { get; }
        public DateTime ExportedUtc { get; }
        public bool IsCompatible { get; }
        public string CompatibilitySummary { get; }
        public IReadOnlyList<SettingsImportField> FieldsToOverwrite { get; }
        public IReadOnlyList<string> UnknownFields { get; }
        public IReadOnlyList<string> ValidationErrors { get; }

        internal GameSaveCenterSettings? ImportedSettings { get; }

        public string BuildConfirmationMessage()
        {
            var lines = new List<string>
            {
                $"架构版本：v{SchemaVersion}",
                $"兼容性：{CompatibilitySummary}"
            };

            if (ExportedUtc != DateTime.MinValue)
                lines.Add("导出时间（UTC）：" + ExportedUtc.ToString("u"));

            if (FieldsToOverwrite.Count == 0)
            {
                lines.Add("将覆盖字段：无（当前配置值不变）。");
            }
            else
            {
                lines.Add("将覆盖字段（设备身份保留当前安装值）：");
                lines.Add("  " + string.Join("、", FieldsToOverwrite.Select(field => field.DisplayName)));
            }

            if (UnknownFields.Count > 0)
            {
                lines.Add("未知字段：" + string.Join("、", UnknownFields));
                lines.Add("未知字段将被忽略，不会破坏当前配置。");
            }

            lines.Add("安全说明：可分享导出不包含凭据；导入不会替换当前设备身份。");

            if (ValidationErrors.Count > 0)
            {
                lines.Add("不能应用：" + string.Join("；", ValidationErrors));
                lines.Add("当前文件不会写入配置。");
            }
            else if (IsCompatible)
            {
                lines.Add("选择“是”后才会写入当前草稿；选择“否”保持原配置。");
            }

            return string.Join(Environment.NewLine, lines);
        }
    }

    public sealed class SettingsImportField
    {
        internal SettingsImportField(string propertyName, string displayName)
        {
            PropertyName = propertyName;
            DisplayName = displayName;
        }

        public string PropertyName { get; }
        public string DisplayName { get; }
    }
}
