using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ChronoPos.Desktop.Converters;

/// <summary>
/// Converter that converts a boolean value to a GridLength.
/// True = 600px width, False = 0px width
/// </summary>
public class BoolToGridLengthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? new GridLength(600, GridUnitType.Pixel) : new GridLength(0, GridUnitType.Pixel);
        }
        
        return new GridLength(0, GridUnitType.Pixel);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is GridLength gridLength)
        {
            return gridLength.Value > 0;
        }
        
        return false;
    }
}