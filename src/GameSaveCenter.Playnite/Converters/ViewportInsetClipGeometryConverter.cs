using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace GameSaveCenter.Playnite.Converters;

/// <summary>Clips row content at the visible DataGrid viewport edge without changing layout or scroll extent.</summary>
public sealed class ViewportInsetClipGeometryConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2
            || values[0] is not double viewportWidth
            || double.IsNaN(viewportWidth)
            || double.IsInfinity(viewportWidth)
            || viewportWidth <= 0
            || !double.TryParse(parameter?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var trailingInset))
            return DependencyProperty.UnsetValue;

        // A DataGrid row may be wider than the visible viewport while horizontal
        // scrolling is active. Translate the visible right edge back into the row's
        // content coordinates so the cell content and selection frame share one edge.
        var horizontalOffset = 0d;
        double rowHeight;
        if (values.Length >= 3)
        {
            if (values[1] is not double offset
                || double.IsNaN(offset)
                || double.IsInfinity(offset)
                || values[2] is not double measuredRowHeight)
                return DependencyProperty.UnsetValue;
            horizontalOffset = Math.Max(0, offset);
            rowHeight = measuredRowHeight;
        }
        else if (values[1] is double legacyRowHeight)
        {
            rowHeight = legacyRowHeight;
        }
        else
        {
            return DependencyProperty.UnsetValue;
        }

        if (double.IsNaN(rowHeight) || double.IsInfinity(rowHeight) || rowHeight <= 0)
            return DependencyProperty.UnsetValue;

        var geometry = new RectangleGeometry(new Rect(
            0,
            0,
            Math.Max(0, horizontalOffset + viewportWidth - trailingInset),
            rowHeight));
        if (geometry.CanFreeze)
            geometry.Freeze();
        return geometry;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => Array.Empty<object>();
}
