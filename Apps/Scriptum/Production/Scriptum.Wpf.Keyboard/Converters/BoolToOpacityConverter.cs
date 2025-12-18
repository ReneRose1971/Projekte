using System.Globalization;
using System.Windows.Data;

namespace Scriptum.Wpf.Keyboard.Converters;

/// <summary>
/// Konvertiert einen bool-Wert in einen Opacity-Wert.
/// </summary>
public class BoolToOpacityConverter : IValueConverter
{
    public double TrueOpacity { get; set; } = 1.0;
    public double FalseOpacity { get; set; } = 0.35;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b ? (object)(b ? TrueOpacity : FalseOpacity) : FalseOpacity;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
