using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using GameSaveCenter.Contracts;

namespace GameSaveCenter.Playnite.Infrastructure
{
    public sealed class BackupArrivalVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            => values.Length == 2 && values[0] is BackupVersionDto backup
                && values[1] is IReadOnlyCollection<BackupVersionDto> arrivals && arrivals.Contains(backup)
                ? Visibility.Visible
                : Visibility.Collapsed;

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
