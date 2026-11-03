using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace GameSaveCenter.Playnite.Converters;

/// <summary>Clips selected row content inside its visible cells viewport without changing layout or scroll extent.</summary>
public sealed class ViewportInsetClipGeometryConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2
            || values[0] is not double viewportWidth
            || values[1] is not double rowHeight
            || double.IsNaN(viewportWidth)
            || double.IsInfinity(viewportWidth)
            || double.IsNaN(rowHeight)
            || double.IsInfinity(rowHeight)
            || viewportWidth <= 0
            || rowHeight <= 0
            || !double.TryParse(parameter?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var trailingInset))
            return DependencyProperty.UnsetValue;

        var geometry = new RectangleGeometry(new Rect(0, 0, Math.Max(0, viewportWidth - trailingInset), rowHeight));
        if (geometry.CanFreeze)
            geometry.Freeze();
        return geometry;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => Array.Empty<object>();
}
