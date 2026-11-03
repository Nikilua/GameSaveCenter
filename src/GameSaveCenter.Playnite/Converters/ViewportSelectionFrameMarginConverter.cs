using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace GameSaveCenter.Playnite.Converters;

/// <summary>Offsets selection chrome with horizontally scrolled rows so it stays on the visible viewport.</summary>
public sealed class ViewportSelectionFrameMarginConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        var parts = (parameter?.ToString() ?? string.Empty).Split(',');
        if (parts.Length != 4
            || !parts.All(part => double.TryParse(part, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            || !double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var left)
            || !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var top)
            || !double.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var right)
            || !double.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var bottom))
            return DependencyProperty.UnsetValue;

        if (values.Length == 0
            || values[0] is not double horizontalOffset
            || double.IsNaN(horizontalOffset)
            || double.IsInfinity(horizontalOffset))
            return new Thickness(left, top, right, bottom);

        return new Thickness(Math.Max(0, horizontalOffset) + left, top, right, bottom);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => Array.Empty<object>();
}
