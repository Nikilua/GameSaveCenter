using System;
using System.Globalization;
using System.Windows.Data;

namespace GameSaveCenter.Playnite.Controls
{
    /// <summary>
    /// Adds a small, text-based status cue to table badges without changing the
    /// underlying display value or any business state. Unknown values pass through.
    /// </summary>
    public sealed class StatusGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value?.ToString()?.Trim() ?? string.Empty;
            if (text.Length == 0 || StartsWithStatusGlyph(text)) return text;

            if (ContainsAny(text, "成功", "完成", "通过", "可用", "正常", "健康", "已配置"))
                return "✓ " + text;
            if (ContainsAny(text, "失败", "错误", "严重", "拒绝", "不可用"))
                return "× " + text;
            if (ContainsAny(text, "警告", "需关注", "注意", "待", "等待", "未配置", "未找到", "跳过", "异常"))
                return "⚠ " + text;

            return text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;

        private static bool StartsWithStatusGlyph(string text)
            => text.StartsWith("✓", StringComparison.Ordinal)
                || text.StartsWith("×", StringComparison.Ordinal)
                || text.StartsWith("⚠", StringComparison.Ordinal)
                || text.StartsWith("ℹ", StringComparison.Ordinal)
                || text.StartsWith("…", StringComparison.Ordinal);

        private static bool ContainsAny(string text, params string[] terms)
        {
            foreach (var term in terms)
            {
                if (text.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }
    }
}
