using System;
using System.Globalization;
using System.Windows.Data;

namespace ChronoPos.Desktop.Converters;

/// <summary>
/// Converter that checks if the value equals the parameter
/// Returns true if they match, false otherwise
/// </summary>
public class EqualsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null && parameter == null)
            return true;
            
        if (value == null || parameter == null)
            return false;
            
        return value.ToString().Equals(parameter.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}