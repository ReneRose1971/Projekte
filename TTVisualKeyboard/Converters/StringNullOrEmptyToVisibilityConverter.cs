using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TTVisualKeyboard.Converters
{
    public class StringNullOrEmptyToVisibilityConverter : IValueConverter
    {
        public Visibility EmptyVisibility { get; set; } = Visibility.Collapsed;
        public Visibility NonEmptyVisibility { get; set; } = Visibility.Visible;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;
            return string.IsNullOrWhiteSpace(s) ? EmptyVisibility : NonEmptyVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}