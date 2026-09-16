using System;
using System.Globalization;
using System.Windows.Data;

namespace GameSaveCenter.Playnite.Converters
{
    /// <summary>
    /// Keeps the drive/root and the file name visible in compact path previews.
    /// The source path is never changed; details and copy actions still use it verbatim.
    /// </summary>
    public sealed class PathDisplayConverter : IValueConverter
    {
        internal const int PreviewCharacterLimit = 72;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return FormatPreview(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();

        internal static string FormatPreview(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;

            // Windows paths cannot contain CR/LF. Removing them from the compact
            // preview keeps malformed fixture/input text from looking like a second
            // path while leaving the original value untouched for selection/copy.
            var displayPath = path!.Replace("\r", string.Empty).Replace("\n", string.Empty);
            if (displayPath.Length <= PreviewCharacterLimit) return displayPath;

            var separator = Math.Max(displayPath.LastIndexOf('\\'), displayPath.LastIndexOf('/'));
            var fileName = separator >= 0 && separator + 1 < displayPath.Length
                ? displayPath.Substring(separator + 1)
                : displayPath;
            var prefixLength = Math.Min(PreviewCharacterLimit / 2, displayPath.Length);
            var suffixLength = PreviewCharacterLimit - prefixLength - 1;

            // Prefer the filename tail, but do not let an unusually long filename
            // remove the drive/root cue from the preview.
            var prefix = displayPath.Substring(0, prefixLength);
            var suffix = fileName.Length > suffixLength
                ? fileName.Substring(fileName.Length - suffixLength)
                : fileName;
            if (suffix.Length < suffixLength)
            {
                var remaining = suffixLength - suffix.Length;
                var pathTailStart = Math.Max(prefixLength, displayPath.Length - suffix.Length - remaining);
                var pathTail = displayPath.Substring(pathTailStart, Math.Min(remaining, displayPath.Length - pathTailStart));
                suffix = pathTail + suffix;
            }

            return prefix + "…" + suffix;
        }
    }
}
