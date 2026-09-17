using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>Identifies the syntax of an editor that receives pasted text.</summary>
    public enum PasteNormalizationKind
    {
        None,
        Path,
        Port,
        ExcludePattern,
        RemoteTarget
    }

    public enum PasteNormalizationDisposition
    {
        Unchanged,
        Normalized,
        Rejected
    }

    /// <summary>Describes a paste decision without changing the clipboard.</summary>
    public sealed class PasteNormalizationResult
    {
        internal PasteNormalizationResult(
            PasteNormalizationDisposition disposition,
            string rawValue,
            string value,
            string message)
        {
            Disposition = disposition;
            RawValue = rawValue;
            Value = value;
            Message = message;
        }

        public PasteNormalizationDisposition Disposition { get; }
        public string RawValue { get; }
        public string Value { get; }
        public string Message { get; }
        public bool Accepted => Disposition != PasteNormalizationDisposition.Rejected;
        public bool Changed => Disposition == PasteNormalizationDisposition.Normalized;
    }

    /// <summary>
    /// Normalizes only the unambiguous wrapper around pasted editor input. It rejects
    /// multiple lines instead of silently turning several paths into one invalid value.
    /// </summary>
    public static class PasteNormalization
    {
        public static readonly DependencyProperty KindProperty = DependencyProperty.RegisterAttached(
            "Kind",
            typeof(PasteNormalizationKind),
            typeof(PasteNormalization),
            new PropertyMetadata(PasteNormalizationKind.None, OnKindChanged));

        private static readonly DependencyProperty BaseToolTipProperty = DependencyProperty.RegisterAttached(
            "BaseToolTip",
            typeof(object),
            typeof(PasteNormalization),
            new PropertyMetadata(null));

        private static readonly DependencyProperty BaseHelpTextProperty = DependencyProperty.RegisterAttached(
            "BaseHelpText",
            typeof(string),
            typeof(PasteNormalization),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty LastRawTextProperty = DependencyProperty.RegisterAttached(
            "LastRawText",
            typeof(string),
            typeof(PasteNormalization),
            new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty LastMessageProperty = DependencyProperty.RegisterAttached(
            "LastMessage",
            typeof(string),
            typeof(PasteNormalization),
            new PropertyMetadata(string.Empty));

        public static PasteNormalizationKind GetKind(DependencyObject element)
            => (PasteNormalizationKind)element.GetValue(KindProperty);

        public static void SetKind(DependencyObject element, PasteNormalizationKind value)
            => element.SetValue(KindProperty, value);

        public static string GetLastRawText(DependencyObject element)
            => (string)element.GetValue(LastRawTextProperty);

        public static void SetLastRawText(DependencyObject element, string value)
            => element.SetValue(LastRawTextProperty, value ?? string.Empty);

        public static string GetLastMessage(DependencyObject element)
            => (string)element.GetValue(LastMessageProperty);

        public static void SetLastMessage(DependencyObject element, string value)
            => element.SetValue(LastMessageProperty, value ?? string.Empty);

        public static PasteNormalizationResult Normalize(string rawValue, PasteNormalizationKind kind)
        {
            rawValue = rawValue ?? string.Empty;
            if (kind == PasteNormalizationKind.None)
                return new PasteNormalizationResult(PasteNormalizationDisposition.Unchanged, rawValue, rawValue, string.Empty);

            var value = rawValue.Trim();
            var removedOuterQuotes = false;
            if (HasMatchingOuterQuotes(value))
            {
                value = value.Substring(1, value.Length - 2).Trim();
                removedOuterQuotes = true;
            }

            // Trim removes a single path copied with a trailing shell newline. A
            // remaining line break means the clipboard contains multiple values and
            // must not be collapsed into an opaque invalid path or pattern.
            if (ContainsLineBreak(value))
            {
                var label = GetLabel(kind);
                return new PasteNormalizationResult(
                    PasteNormalizationDisposition.Rejected,
                    rawValue,
                    rawValue,
                    $"已阻止{label}的多行粘贴；请一次粘贴一个值。原字段未改变，剪贴板内容未改写。\n可按 Ctrl+Z 恢复粘贴前的字段值。");
            }

            if (string.Equals(rawValue, value, StringComparison.Ordinal))
                return new PasteNormalizationResult(PasteNormalizationDisposition.Unchanged, rawValue, value, string.Empty);

            var changes = removedOuterQuotes ? "去除外层引号" : "去除外层空白";
            if (removedOuterQuotes && !string.Equals(rawValue.Trim(), value, StringComparison.Ordinal))
                changes = "去除外层空白和引号";

            return new PasteNormalizationResult(
                PasteNormalizationDisposition.Normalized,
                rawValue,
                value,
                $"已标准化粘贴内容：{changes}。当前字段显示标准化后的值；可按 Ctrl+Z 恢复粘贴前的字段值。");
        }

        private static void OnKindChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            if (!(target is TextBox textBox)) return;

            var oldKind = (PasteNormalizationKind)args.OldValue;
            var newKind = (PasteNormalizationKind)args.NewValue;
            if (oldKind == PasteNormalizationKind.None && newKind != PasteNormalizationKind.None)
            {
                textBox.SetValue(BaseToolTipProperty, textBox.ToolTip);
                textBox.SetValue(BaseHelpTextProperty, AutomationProperties.GetHelpText(textBox) ?? string.Empty);
                DataObject.AddPastingHandler(textBox, OnPasting);
            }
            else if (oldKind != PasteNormalizationKind.None && newKind == PasteNormalizationKind.None)
            {
                DataObject.RemovePastingHandler(textBox, OnPasting);
                RestoreBasePresentation(textBox);
            }
        }

        private static void OnPasting(object sender, DataObjectPastingEventArgs args)
        {
            if (!(sender is TextBox textBox)) return;
            var kind = GetKind(textBox);
            if (kind == PasteNormalizationKind.None) return;

            var rawValue = ReadText(args.DataObject);
            if (rawValue == null) return;

            var result = Normalize(rawValue, kind);
            SetLastRawText(textBox, result.RawValue);
            SetLastMessage(textBox, result.Message);
            ApplyPresentation(textBox, result.Message);
            if (!result.Accepted)
            {
                args.CancelCommand();
                return;
            }

            if (!result.Changed) return;

            // Let TextBox create its normal undo unit. The clipboard is never changed,
            // and Ctrl+Z therefore restores the value that existed before this paste.
            args.CancelCommand();
            textBox.SelectedText = result.Value;
            textBox.CaretIndex = textBox.SelectionStart + result.Value.Length;
        }

        private static string? ReadText(IDataObject data)
        {
            if (data.GetDataPresent(DataFormats.UnicodeText))
                return data.GetData(DataFormats.UnicodeText) as string;
            if (data.GetDataPresent(DataFormats.Text))
                return data.GetData(DataFormats.Text) as string;
            return null;
        }

        private static void ApplyPresentation(TextBox textBox, string message)
        {
            var baseToolTip = textBox.GetValue(BaseToolTipProperty);
            var baseToolTipText = baseToolTip as string ?? string.Empty;
            textBox.ToolTip = string.IsNullOrEmpty(message)
                ? baseToolTip
                : string.IsNullOrEmpty(baseToolTipText)
                    ? message
                    : baseToolTipText + Environment.NewLine + Environment.NewLine + message;

            var baseHelpText = (string)textBox.GetValue(BaseHelpTextProperty) ?? string.Empty;
            AutomationProperties.SetHelpText(
                textBox,
                string.IsNullOrEmpty(message)
                    ? baseHelpText
                    : string.IsNullOrEmpty(baseHelpText) ? message : baseHelpText + " " + message);
        }

        private static void RestoreBasePresentation(TextBox textBox)
        {
            textBox.ToolTip = textBox.GetValue(BaseToolTipProperty);
            AutomationProperties.SetHelpText(textBox, (string)textBox.GetValue(BaseHelpTextProperty) ?? string.Empty);
        }

        private static bool HasMatchingOuterQuotes(string value)
        {
            if (value.Length < 2) return false;
            var first = value[0];
            var last = value[value.Length - 1];
            return (first == '"' && last == '"') || (first == '\'' && last == '\'');
        }

        private static bool ContainsLineBreak(string value)
            => value.IndexOf('\r') >= 0
                || value.IndexOf('\n') >= 0
                || value.IndexOf('\u2028') >= 0
                || value.IndexOf('\u2029') >= 0;

        private static string GetLabel(PasteNormalizationKind kind)
        {
            switch (kind)
            {
                case PasteNormalizationKind.Path: return "目录或路径";
                case PasteNormalizationKind.Port: return "端口";
                case PasteNormalizationKind.ExcludePattern: return "排除模式";
                case PasteNormalizationKind.RemoteTarget: return "云端目标";
                default: return "字段";
            }
        }
    }
}
