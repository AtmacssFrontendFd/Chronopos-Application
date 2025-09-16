using System;
using System.Globalization;
using System.Windows.Data;

namespace ChronoPos.Desktop.Converters;

/// <summary>
/// Converter that returns half of the input value
/// Used for converting diameter to radius for circular shapes
/// </summary>
public class HalfValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            return doubleValue / 2.0;
        }
        
        if (value is int intValue)
        {
            return intValue / 2.0;
        }
        
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double doubleValue)
        {
            return doubleValue * 2.0;
        }
        
        return 0;
    }
}