using System;
using System.Globalization;
using System.Windows.Data;

namespace GameSaveCenter.Playnite.Converters;

/// <summary>Maps a disclosure's checked state to the chevron's resting angle.</summary>
public sealed class ExpandedStateToChevronAngleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool expanded && expanded ? 0d : -90d;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
