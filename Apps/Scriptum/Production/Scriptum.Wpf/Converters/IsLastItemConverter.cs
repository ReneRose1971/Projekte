using System;
using System.Globalization;
using System.Windows.Data;

namespace Scriptum.Wpf.Converters;

public sealed class IsLastItemConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int index && parameter is int count)
        {
            return index == count - 1;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
