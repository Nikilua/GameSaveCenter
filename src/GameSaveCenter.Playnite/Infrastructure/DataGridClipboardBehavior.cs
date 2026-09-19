using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.Infrastructure
{
    /// <summary>
    /// Provides an explicit, stable clipboard contract for read-only production tables.
    /// Ctrl+C copies selected rows as TSV; Ctrl+Shift+C copies the current cell.
    /// The behavior deliberately does not fall back to DataGrid's reflection-based copy.
    /// </summary>
    public static class DataGridClipboardBehavior
    {
        private static readonly DependencyProperty IsAttachedProperty = DependencyProperty.RegisterAttached(
            "IsAttached",
            typeof(bool),
            typeof(DataGridClipboardBehavior),
            new PropertyMetadata(false));

        public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
            "Enabled",
            typeof(bool),
            typeof(DataGridClipboardBehavior),
            new PropertyMetadata(false, OnEnabledChanged));

        public static readonly DependencyProperty ProfileProperty = DependencyProperty.RegisterAttached(
            "Profile",
            typeof(string),
            typeof(DataGridClipboardBehavior),
            new PropertyMetadata(string.Empty));

        public static void SetEnabled(DependencyObject element, bool value)
            => element.SetValue(EnabledProperty, value);

        public static bool GetEnabled(DependencyObject element)
            => (bool)element.GetValue(EnabledProperty);

        public static void SetProfile(DependencyObject element, string value)
            => element.SetValue(ProfileProperty, value ?? string.Empty);

        public static string GetProfile(DependencyObject element)
            => (string)element.GetValue(ProfileProperty);

        private static void OnEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (!(sender is DataGrid grid)) return;

            if ((bool)args.NewValue)
            {
                if (grid.GetValue(IsAttachedProperty) is bool attached && attached) return;
                grid.SetValue(IsAttachedProperty, true);
                grid.AddHandler(Keyboard.PreviewKeyDownEvent, new KeyEventHandler(OnPreviewKeyDown), true);
                grid.ClipboardCopyMode = DataGridClipboardCopyMode.None;
            }
            else
            {
                if (!(grid.GetValue(IsAttachedProperty) is bool attached) || !attached) return;
                grid.RemoveHandler(Keyboard.PreviewKeyDownEvent, new KeyEventHandler(OnPreviewKeyDown));
                grid.ClearValue(IsAttachedProperty);
            }
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs args)
        {
            if (!(sender is DataGrid grid) || args.Key != Key.C) return;

            var modifiers = Keyboard.Modifiers;
            if (modifiers != ModifierKeys.Control && modifiers != (ModifierKeys.Control | ModifierKeys.Shift)) return;

            var copyCell = modifiers == (ModifierKeys.Control | ModifierKeys.Shift);
            var text = BuildCopyText(grid, copyCell);
            if (string.IsNullOrEmpty(text)) return;

            if (!TrySetClipboardText(text)) return;
            args.Handled = true;
        }

        private static bool TrySetClipboardText(string text)
        {
            try
            {
                ClipboardSetterForVerification?.Invoke(text);
                if (ClipboardSetterForVerification == null)
                    Clipboard.SetText(text);
                return true;
            }
            catch (Exception exception) when (exception is System.Runtime.InteropServices.COMException || exception is InvalidOperationException)
            {
                return false;
            }
        }

        private static string BuildCopyText(DataGrid grid, bool copyCell)
        {
            var profile = GetProfile(grid);
            return copyCell
                ? DataGridClipboardFormatter.FormatCurrentCell(profile, grid.CurrentCell)
                : DataGridClipboardFormatter.FormatSelectedRows(profile, grid);
        }

        /// <summary>Test-only seam; production leaves this null and uses the OS clipboard.</summary>
        internal static Action<string>? ClipboardSetterForVerification { get; set; }

        internal static string BuildCopyTextForVerification(DataGrid grid, bool copyCell)
            => BuildCopyText(grid, copyCell);
    }

    /// <summary>Allowlisted TSV formatter used by the production clipboard behavior.</summary>
    internal static class DataGridClipboardFormatter
    {
        private const string SaveHistory = "SaveHistory";
        private const string SaveCandidate = "SaveCandidate";
        private const string Task = "Task";
        private const string MediaInbox = "MediaInbox";
        private const string Finding = "Finding";

        internal static string FormatSelectedRows(string profile, DataGrid grid)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));

            var selected = grid.SelectedItems.Cast<object>().ToArray();
            if (selected.Length == 0 && grid.SelectedItem != null)
                selected = new[] { grid.SelectedItem };
            if (selected.Length == 0) return string.Empty;

            var selectedKeys = new HashSet<string>(selected.Select(item => GetStableKey(profile, item)), StringComparer.Ordinal);
            var rows = new List<string>();
            foreach (var item in grid.Items.Cast<object>())
            {
                var key = GetStableKey(profile, item);
                if (!selectedKeys.Remove(key)) continue;
                rows.Add(FormatRow(profile, item));
            }

            return string.Join("\r\n", rows.Where(row => row != null));
        }

        internal static string FormatCurrentCell(string profile, DataGridCellInfo cell)
        {
            if (cell.Item == null || cell.Column == null) return string.Empty;
            return FormatCell(profile, cell.Column.Header as string ?? Convert.ToString(cell.Column.Header, CultureInfo.InvariantCulture) ?? string.Empty, cell.Item);
        }

        internal static string FormatRowForVerification(string profile, object item)
            => FormatRow(profile, item);

        internal static string FormatCellForVerification(string profile, string header, object item)
            => FormatCell(profile, header, item);

        private static string FormatRow(string profile, object item)
        {
            var values = profile switch
            {
                SaveHistory when item is BackupVersionDto backup => new[]
                {
                    backup.CreatedLocal.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    backup.BackupTypeDisplay,
                    backup.FileCount.ToString(CultureInfo.InvariantCulture),
                    backup.SizeDisplay,
                    backup.SourceDevice,
                    backup.Comment,
                    backup.LockStateDisplay
                },
                SaveCandidate when item is SavePathCandidateDto candidate => new[]
                {
                    FormatPercent(candidate.Score),
                    candidate.StatusDisplay,
                    candidate.Path,
                    candidate.ReasonsDisplay
                },
                Task when item is TaskStatusDto task => new[]
                {
                    task.CreatedLocal.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    task.TaskTypeDisplay,
                    task.StageDisplay,
                    task.GameName,
                    task.StateDisplay,
                    task.ProgressDisplay,
                    task.DetailMessage
                },
                MediaInbox when item is MediaItemDto media => new[]
                {
                    media.CapturedLocal.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    media.KindDisplay,
                    media.SourceDisplay,
                    string.IsNullOrWhiteSpace(media.OriginalPath) ? media.ArchivePath : media.OriginalPath,
                    media.ClassificationReason
                },
                Finding when item is ValidationFindingDto finding => new[]
                {
                    finding.SeverityDisplay,
                    finding.GameName,
                    finding.Title,
                    finding.Detail,
                    finding.SuggestedAction,
                    finding.Code
                },
                _ => Array.Empty<string>()
            };

            return string.Join("\t", values.Select(SanitizeAndEscape));
        }

        private static string FormatCell(string profile, string header, object item)
        {
            var value = profile switch
            {
                SaveHistory when item is BackupVersionDto backup => header switch
                {
                    "时间" => backup.CreatedLocal.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    "类型" => backup.BackupTypeDisplay,
                    "文件数" => backup.FileCount.ToString(CultureInfo.InvariantCulture),
                    "大小" => backup.SizeDisplay,
                    "设备" => backup.SourceDevice,
                    "备注" => backup.Comment,
                    "状态" => backup.LockStateDisplay,
                    _ => string.Empty
                },
                SaveCandidate when item is SavePathCandidateDto candidate => header switch
                {
                    "可信度" => FormatPercent(candidate.Score),
                    "状态" => candidate.StatusDisplay,
                    "路径" => candidate.Path,
                    "依据" => candidate.ReasonsDisplay,
                    _ => string.Empty
                },
                Task when item is TaskStatusDto task => header switch
                {
                    "本地时间" => task.CreatedLocal.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    "任务" => task.TaskTypeDisplay,
                    "阶段" => task.StageDisplay,
                    "游戏" => task.GameName,
                    "状态" => task.StateDisplay,
                    "进度" => task.ProgressDisplay,
                    "详情" => task.DetailMessage,
                    _ => string.Empty
                },
                MediaInbox when item is MediaItemDto media => header switch
                {
                    "拍摄时间" => media.CapturedLocal.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                    "类型" => media.KindDisplay,
                    "来源" => media.SourceDisplay,
                    "文件" => string.IsNullOrWhiteSpace(media.OriginalPath) ? media.ArchivePath : media.OriginalPath,
                    "原因" => media.ClassificationReason,
                    _ => string.Empty
                },
                Finding when item is ValidationFindingDto finding => header switch
                {
                    "等级" => finding.SeverityDisplay,
                    "游戏" => finding.GameName,
                    "问题" => finding.Title,
                    "详情" => finding.Detail,
                    _ => string.Empty
                },
                _ => string.Empty
            };

            return SanitizeAndEscape(value);
        }

        private static string GetStableKey(string profile, object item)
        {
            var identity = profile switch
            {
                SaveHistory when item is BackupVersionDto backup => backup.BackupId,
                SaveCandidate when item is SavePathCandidateDto candidate => (candidate.PlayniteId ?? string.Empty) + "\u001f" + (candidate.Path ?? string.Empty),
                Task when item is TaskStatusDto task => task.TaskId,
                MediaInbox when item is MediaItemDto media => media.MediaId,
                Finding when item is ValidationFindingDto finding => (finding.PlayniteId ?? string.Empty) + "\u001f" + (finding.Code ?? string.Empty) + "\u001f" + (finding.Title ?? string.Empty),
                _ => string.Empty
            };

            return string.IsNullOrWhiteSpace(identity)
                ? profile + "\u001f" + RuntimeHelpers.GetHashCode(item).ToString(CultureInfo.InvariantCulture)
                : profile + "\u001f" + identity;
        }

        private static string FormatPercent(double value)
            => double.IsNaN(value) || double.IsInfinity(value)
                ? "—"
                : (value * 100d).ToString("0.##", CultureInfo.InvariantCulture) + "%";

        private static string SanitizeAndEscape(string value)
        {
            var sanitized = ClipboardValueSanitizer.Sanitize(value ?? string.Empty);
            return sanitized
                .Replace("\t", "\\t")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
        }
    }

    internal static class ClipboardValueSanitizer
    {
        private static readonly Regex KeyValueSecret = new Regex(
            @"(?ix)\b(password|passwd|pwd|token|secret|client_secret|access_token|refresh_token|api[_-]?key|authorization|credential|private_key)\b\s*([=:])\s*(""[^""]*""|'[^']*'|Bearer\s+[^\s,;&]+|[^\s,;&]+)",
            RegexOptions.Compiled);
        private static readonly Regex BearerSecret = new Regex(@"(?i)\bBearer\s+[^\s,;]+", RegexOptions.Compiled);
        private static readonly Regex UriCredential = new Regex(@"(?i)(https?://)[^\s/@]+:[^\s/@]+@", RegexOptions.Compiled);

        internal static string Sanitize(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            var sanitized = UriCredential.Replace(value, "$1[已隐藏]@");
            sanitized = KeyValueSecret.Replace(sanitized, match => match.Groups[1].Value + match.Groups[2].Value + "[已隐藏]");
            return BearerSecret.Replace(sanitized, "Bearer [已隐藏]");
        }
    }
}
