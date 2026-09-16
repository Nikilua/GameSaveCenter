using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GameSaveCenter.Playnite.Infrastructure;

/// <summary>
/// Measures realized numeric DataGrid cells against an unconstrained WPF text run.
/// Numeric values are not allowed to silently become ellipsized: a caller can decide
/// whether a different column or an intentional tooltip is appropriate, but the audit
/// must first know that the complete value fits the realized cell.
/// </summary>
public static class NumericCellReadability
{
    // Keep a small allowance for WPF's device-independent rounding without allowing
    // an entire glyph to hide behind the DataGridCell chrome.
    public const double ToleranceDip = 0.5;

    public sealed class Measurement
    {
        public int RowIndex { get; set; }
        public string ColumnHeader { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public double TextWidth { get; set; }
        public double AvailableWidth { get; set; }
        public double TextHeight { get; set; }
        public double CellHeight { get; set; }
        public TextWrapping Wrapping { get; set; }
        public TextTrimming Trimming { get; set; }

        public bool HorizontalFit => TextWidth <= AvailableWidth + ToleranceDip;

        public bool VerticalFit => TextHeight > 0
            && CellHeight > 0
            && TextHeight <= CellHeight + ToleranceDip;

        public bool IsReadable => HorizontalFit && VerticalFit;
    }

    public static IReadOnlyList<Measurement> Measure(DataGrid grid, string columnHeader = "数值")
    {
        if (grid == null) throw new ArgumentNullException(nameof(grid));
        if (string.IsNullOrWhiteSpace(columnHeader)) throw new ArgumentException("数值列标题不能为空。", nameof(columnHeader));

        return FindVisualChildren<DataGridCell>(grid)
            .Where(cell => cell.Visibility == Visibility.Visible
                && cell.ActualWidth > 0
                && cell.ActualHeight > 0
                && string.Equals(cell.Column?.Header?.ToString(), columnHeader, StringComparison.Ordinal))
            .Select(cell => CreateMeasurement(grid, cell, columnHeader))
            .Where(measurement => measurement != null)
            .Cast<Measurement>()
            .OrderBy(measurement => measurement.RowIndex)
            .ToArray();
    }

    public static double MeasureUnconstrainedTextWidth(TextBlock text)
    {
        if (text == null) throw new ArgumentNullException(nameof(text));
        if (string.IsNullOrEmpty(text.Text))
            return 0;

        var typeface = new Typeface(text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch);
        var formatted = new FormattedText(
            text.Text,
            CultureInfo.CurrentUICulture,
            text.FlowDirection,
            typeface,
            text.FontSize,
            text.Foreground ?? Brushes.Black,
            VisualTreeHelper.GetDpi(text).PixelsPerDip);
        return formatted.WidthIncludingTrailingWhitespace;
    }

    private static Measurement? CreateMeasurement(DataGrid grid, DataGridCell cell, string columnHeader)
    {
        var text = FindVisualChildren<TextBlock>(cell)
            .FirstOrDefault(candidate => candidate.Visibility == Visibility.Visible
                && !string.IsNullOrEmpty(candidate.Text));
        if (text == null)
            return null;

        var row = FindVisualParent<DataGridRow>(cell);
        var rowIndex = row == null ? -1 : grid.ItemContainerGenerator.IndexFromContainer(row);
        return new Measurement
        {
            RowIndex = rowIndex,
            ColumnHeader = columnHeader,
            Text = text.Text,
            TextWidth = MeasureUnconstrainedTextWidth(text),
            AvailableWidth = Math.Max(
                0,
                cell.ActualWidth - cell.Padding.Left - cell.Padding.Right),
            TextHeight = text.DesiredSize.Height,
            CellHeight = cell.ActualHeight,
            Wrapping = text.TextWrapping,
            Trimming = text.TextTrimming
        };
    }

    private static T? FindVisualParent<T>(DependencyObject element)
        where T : DependencyObject
    {
        for (var current = VisualTreeHelper.GetParent(element); current != null; current = VisualTreeHelper.GetParent(current))
        {
            if (current is T match)
                return match;
        }

        return null;
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
                yield return match;

            foreach (var nested in FindVisualChildren<T>(child))
                yield return nested;
        }
    }
}
