using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GameSaveCenter.Playnite.Converters;

/// <summary>Constrains selection chrome to a scrolling viewport while preserving its insets.</summary>
public sealed class ViewportInsetWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not double viewportWidth || double.IsNaN(viewportWidth) || double.IsInfinity(viewportWidth) || viewportWidth <= 0)
            return DependencyProperty.UnsetValue;

        if (!double.TryParse(parameter?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var inset))
            return DependencyProperty.UnsetValue;

        return Math.Max(0d, viewportWidth - inset);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
